namespace DecSm.Extensions.Logging.File.Writer;

internal static class FileLogWriterUtil
{
    public static void RollOnFileSize(
        IFileSystem fileSystem,
        TimeProvider timeProvider,
        string logsDirectory,
        string logName,
        string logFilePath)
    {
        string newLogFilePath;

        for (var i = 0;; i++)
        {
            var suffix = i == 0
                ? string.Empty
                : $"_{i}";

            newLogFilePath = fileSystem.Path.Combine(logsDirectory, $"{logName}_{timeProvider.GetLocalNow():yyMMdd-HHmmss}{suffix}.log");

            if (!fileSystem.File.Exists(newLogFilePath))
                break;
        }

        fileSystem.File.Move(logFilePath, newLogFilePath);
    }

    public static bool RollOnTimeInterval(
        IFileSystem fileSystem,
        TimeProvider timeProvider,
        FileRolloverInterval rolloverInterval,
        IFileInfo fileInfo,
        string logsDirectory,
        string logName,
        string logFilePath)
    {
        var now = timeProvider.GetLocalNow();
        var fileCreatedAt = fileInfo.CreationTime;

        var rollingTimeSpan = rolloverInterval switch
        {
            FileRolloverInterval.Year => TimeSpan.FromDays(365),
            FileRolloverInterval.Month => TimeSpan.FromDays(30),
            FileRolloverInterval.Day => TimeSpan.FromDays(1),
            FileRolloverInterval.Hour => TimeSpan.FromHours(1),
            FileRolloverInterval.Minute => TimeSpan.FromMinutes(1),
            FileRolloverInterval.Infinite => TimeSpan.MaxValue,
            _ => throw new ArgumentOutOfRangeException(nameof(rolloverInterval), rolloverInterval, "Invalid rolling interval"),
        };

        if (now - fileCreatedAt < rollingTimeSpan)
            return false;

        string newLogFilePath;

        for (var i = 0;; i++)
        {
            var suffix = i == 0
                ? string.Empty
                : $"_{i}";

            newLogFilePath = fileSystem.Path.Combine(logsDirectory, $"{logName}_{timeProvider.GetLocalNow():yyMMdd-HHmmss}{suffix}.log");

            if (!fileSystem.File.Exists(newLogFilePath))
                break;
        }

        fileSystem.File.Move(logFilePath, newLogFilePath);

        return true;
    }

    public static void PurgeOnTotalSize(IFileSystem fileSystem, long maxTotalSizeBytes, string logsDirectory, string logName)
    {
        var allLogs = fileSystem.Directory.GetFiles(logsDirectory, $"{logName}_*.log");

        var totalSize = allLogs.Sum(file => fileSystem.FileInfo.New(file)
            .Length);

        if (totalSize < maxTotalSizeBytes)
            return;

        var oldestLog = allLogs
            .OrderBy(file => fileSystem.FileInfo.New(file)
                .CreationTime)
            .First();

        fileSystem.File.Delete(oldestLog);
    }

    public static void WriteToFile(IFileSystem fileSystem, string filePath, IEnumerable<string> logs)
    {
        using var writer = fileSystem.File.AppendText(filePath);

        foreach (var log in logs)
            writer.Write(log);

        writer.Flush();
    }
}
