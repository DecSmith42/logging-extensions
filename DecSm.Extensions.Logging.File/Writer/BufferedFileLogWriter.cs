namespace DecSm.Extensions.Logging.File.Writer;

internal sealed class BufferedFileLogWriter(
    IFileSystem fileSystem,
    TimeProvider timeProvider,
    Func<FileLoggerConfiguration> getCurrentConfig
) : IFileLogWriter
{
    private readonly Channel<LogEvent> _logEntryChannel = Channel.CreateUnbounded<LogEvent>(new()
    {
        SingleReader = true,
        SingleWriter = false,
    });

    private readonly CancellationTokenSource _writerCancelTokenSource = new();
    private Thread? _writerThread;

    public TimeProvider TimeProvider => timeProvider;

    public void Start()
    {
        if (_writerThread is not null)
            return;

        _writerThread = new(() => RunBackgroundThread(_logEntryChannel.Reader,
            fileSystem,
            TimeProvider,
            getCurrentConfig,
            _writerCancelTokenSource.Token));

        _writerThread.Start();
    }

    public void Log(string log, LogLevel logLevel)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                _logEntryChannel.Writer.TryWrite(new(log, logLevel));

                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                if (attempt >= 5)
                    throw;

                attempt++;
            }
        }
    }

    public void Dispose()
    {
        _writerCancelTokenSource.Cancel();
        _writerThread?.Join();
        _writerThread = null;
    }

    private static void RunBackgroundThread(
        ChannelReader<LogEvent> reader,
        IFileSystem fileSystem,
        TimeProvider timeProvider,
        Func<FileLoggerConfiguration> getCurrentConfig,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var logsByLevel = new Dictionary<LogLevel, List<string>>();
            var logsLengthBytesByLevel = new Dictionary<LogLevel, int>();

            #if NET8_0_OR_GREATER
            var config = getCurrentConfig();
            #else
            var config = getCurrentConfig()!;
            #endif

            var readCount = 0;
            const int maxReadCount = 10;

            while (reader.TryRead(out var item) && readCount < maxReadCount)
            {
                logsByLevel.TryAdd(item.LogLevel, []);

                logsByLevel[item.LogLevel]
                    .Add(item.Message);

                logsLengthBytesByLevel.TryAdd(item.LogLevel, 0);
                logsLengthBytesByLevel[item.LogLevel] += Encoding.UTF8.GetByteCount(item.Message);

                readCount++;
            }

            if (logsByLevel.Count == 0)
                continue;

            foreach (var logLevel in logsByLevel.Keys)
            {
                var logs = logsByLevel[logLevel];
                var logsLengthBytes = logsLengthBytesByLevel[logLevel];

                var logsDirectoryName = config.LogDirectory;

                var logsDirectory = fileSystem.Path.IsPathRooted(logsDirectoryName)
                    ? logsDirectoryName
                    : fileSystem.Path.Combine(fileSystem.Directory.GetCurrentDirectory(), logsDirectoryName);

                if (!fileSystem.Directory.Exists(logsDirectory))
                    fileSystem.Directory.CreateDirectory(logsDirectory);

                var logName = config.PerLevelLogName.TryGetValue(logLevel, out var name)
                    ? name ?? AppDomain.CurrentDomain.FriendlyName
                    : config.LogName ?? AppDomain.CurrentDomain.FriendlyName;

                var logFilePath = fileSystem.Path.Combine(logsDirectory, $"{logName}.log");

                var attempt = 0;

                while (true)
                {
                    try
                    {
                        var fileInfo = fileSystem.FileInfo.New(logFilePath);
                        var newFileCreated = !fileInfo.Exists;

                        if (!newFileCreated && fileInfo.Length + logsLengthBytes >= config.FileSizeLimitBytes)
                        {
                            FileLogWriterUtil.RollOnFileSize(fileSystem, timeProvider, logsDirectory, logName, logFilePath);
                            newFileCreated = true;
                        }

                        if (!newFileCreated && config.RolloverInterval is not FileRolloverInterval.Infinite)
                            newFileCreated = FileLogWriterUtil.RollOnTimeInterval(fileSystem,
                                timeProvider,
                                config.RolloverInterval,
                                fileInfo,
                                logsDirectory,
                                logName,
                                logFilePath);

                        if (newFileCreated)
                            FileLogWriterUtil.PurgeOnTotalSize(fileSystem, config.MaxTotalSizeBytes, logsDirectory, logName);

                        FileLogWriterUtil.WriteToFile(fileSystem, logFilePath, logs);

                        // If we have rolled over the file or are writing for the first time, we want to ensure the
                        // file has the correct timestamps
                        if (newFileCreated)
                        {
                            fileInfo.Refresh();

                            fileInfo.CreationTimeUtc = fileInfo.LastWriteTimeUtc = fileInfo.LastAccessTimeUtc = timeProvider.GetUtcNow()
                                .DateTime;
                        }

                        break;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            Console.WriteLine(ex);
                            Debug.WriteLine(ex);
                        }
                        catch
                        {
                            // Can't do anything more here, better to just continue
                        }

                        if (attempt >= 5)
                            break;

                        attempt++;
                    }
                }
            }
        }
    }

    private sealed record LogEvent(string Message, LogLevel LogLevel);
}
