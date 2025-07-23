namespace DecSm.Extensions.Logging.File;

internal sealed class FileLogger(string name, IFileLogWriter logWriter) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull =>
        null;

    public bool IsEnabled(LogLevel logLevel) =>
        true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var logMessage = formatter(state, exception);

        if (logMessage is null or "")
            return;

        var now = logWriter
            .TimeProvider
            .GetLocalNow()
            .ToString("yyyy-MM-dd HH:mm:ss.fff zzz");

        var logLevelCode = logLevel switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Information => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            LogLevel.Critical => "CRT",
            _ => "???",
        };

        var log = $"[{now} {logLevelCode} {name}] {logMessage}{Environment.NewLine}";

        logWriter.Log(log, logLevel);
    }
}
