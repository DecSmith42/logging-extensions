namespace DecSm.Extensions.Logging.File.Writer;

internal sealed class DirectFileLogWriter(
    IFileSystem fileSystem,
    TimeProvider timeProvider,
    Func<FileLoggerConfiguration> getCurrentConfig
) : IFileLogWriter
{
    public TimeProvider TimeProvider => timeProvider;

    public void Start()
    {
        // No-op
    }

    public void Log(string log, LogLevel logLevel)
    {
        #if NET8_0_OR_GREATER
        var config = getCurrentConfig();
        #else
        var config = getCurrentConfig()!;
        #endif

        var logLengthBytes = Encoding.UTF8.GetByteCount(log);

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

                var logName = config.PerLevelLogName.TryGetValue(logLevel, out var name)
                    ? name ?? AppDomain.CurrentDomain.FriendlyName
                    : config.LogName ?? AppDomain.CurrentDomain.FriendlyName;

                var logFilePath = fileSystem.Path.Combine(logsDirectory, $"{logName}.log");

                var fileInfo = fileSystem.FileInfo.New(logFilePath);
                var newFileCreated = !fileInfo.Exists;

                if (!newFileCreated && fileInfo.Length + logLengthBytes >= config.FileSizeLimitBytes)
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

                FileLogWriterUtil.WriteToFile(fileSystem, logFilePath, [log]);

                // If we have rolled over the file or are writing for the first time, we want to ensure the
                // file has the correct timestamps
                if (newFileCreated)
                {
                    fileInfo.Refresh();

                    fileInfo.CreationTimeUtc = fileInfo.LastWriteTimeUtc = fileInfo.LastAccessTimeUtc = timeProvider
                        .GetUtcNow()
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

    public void Dispose()
    {
        // No-op
    }
}
