namespace DecSm.Extensions.Logging.File.Configuration;

[PublicAPI]
public sealed class FileLoggerConfiguration
{
    public const string DefaultLogDirectory = "Logs";

    public const string? DefaultLogName = null;

    public const long DefaultFileSizeLimitBytes = 100L * 1024 * 1024;

    public const FileRolloverInterval DefaultRollingInterval = FileRolloverInterval.Day;

    public const long DefaultMaxTotalSizeBytes = 10L * 1024 * 1024 * 1024;

    public string LogDirectory { get; set; } = DefaultLogDirectory;

    public string? LogName { get; set; } = DefaultLogName;

    public long FileSizeLimitBytes { get; set; } = DefaultFileSizeLimitBytes;

    public FileRolloverInterval RolloverInterval { get; set; } = DefaultRollingInterval;

    public long MaxTotalSizeBytes { get; set; } = DefaultMaxTotalSizeBytes;
}
