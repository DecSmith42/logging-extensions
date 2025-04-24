namespace DecSm.Extensions.Logging.File.Writer;

internal sealed class BufferedFileLogWriter(
    IFileSystem fileSystem,
    TimeProvider timeProvider,
    Func<FileLoggerConfiguration> getCurrentConfig
) : IFileLogWriter
{
    private readonly Channel<string> _logEntryChannel = Channel.CreateUnbounded<string>(new()
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

    public void Log(string log)
    {
        var attempt = 0;

        while (true)
        {
            try
            {
                _logEntryChannel.Writer.TryWrite(log);

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
        ChannelReader<string> reader,
        IFileSystem fileSystem,
        TimeProvider timeProvider,
        Func<FileLoggerConfiguration> getCurrentConfig,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var config = getCurrentConfig()!;

            var logs = new List<string>();
            var logsLengthBytes = 0;

            var readCount = 0;
            const int maxReadCount = 10;

            while (reader.TryRead(out var item) && readCount < maxReadCount)
            {
                logs.Add(item);
                logsLengthBytes += Encoding.UTF8.GetByteCount(item);
                readCount++;
            }

            if (logs.Count == 0)
                continue;

            var attempt = 0;

            while (true)
            {
                try
                {
                    var logsDirectoryName = config.LogDirectory;

                    var logsDirectory = fileSystem.Path.IsPathRooted(logsDirectoryName)
                        ? logsDirectoryName
                        : fileSystem.Path.Combine(fileSystem.Directory.GetCurrentDirectory(), logsDirectoryName);

                    if (!fileSystem.Directory.Exists(logsDirectory))
                        fileSystem.Directory.CreateDirectory(logsDirectory);

                    var logName = config.LogName ?? AppDomain.CurrentDomain.FriendlyName;
                    var logFilePath = fileSystem.Path.Combine(logsDirectory, $"{logName}.log");

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
