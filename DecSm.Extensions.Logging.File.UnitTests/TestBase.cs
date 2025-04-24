namespace DecSm.Extensions.Logging.File.UnitTests;

public abstract class TestBase
{
    protected IDisposable? DisposableApp;
    protected MockFileSystem FileSystem = null!;
    protected TestTimeProvider TimeProvider = null!;

    protected string GetLogPath(string? timestamp = null) =>
        FileSystem.Path.Combine(FileSystem.Directory.GetCurrentDirectory(),
            "Logs",
            timestamp is not null
                ? $"{AppDomain.CurrentDomain.FriendlyName}_{timestamp}.log"
                : $"{AppDomain.CurrentDomain.FriendlyName}.log");

    protected ILogger CreateBuilderWithLogger<T>(Action<FileLoggerConfiguration>? configure = null)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);

        if (configure is not null)
            builder.Logging.AddFile(configure);
        else
            builder.Logging.AddFile();

        var app = builder.Build();

        DisposableApp = app;

        return app.Services.GetRequiredService<ILogger<T>>();
    }

    protected void StopApp()
    {
        Thread.Sleep(100);
        DisposableApp?.Dispose();
    }
}
