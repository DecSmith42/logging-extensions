namespace DecSm.Extensions.Logging.File.Provider;

[PublicAPI]
[ProviderAlias("File")]
internal sealed class DirectFileLoggerProvider(IOptionsMonitor<FileLoggerConfiguration> config)
    : FileLoggerProvider(config), ILoggerProvider
{
    private DirectFileLogWriter? _logWriter;

    protected override IFileLogWriter LogWriter => _logWriter ??= new(FileSystem, TimeProvider, GetCurrentConfig);

    internal static IFileSystem FileSystem { get; set; } = new FileSystem();

    internal static TimeProvider TimeProvider { get; set; } = TimeProvider.System;
}
