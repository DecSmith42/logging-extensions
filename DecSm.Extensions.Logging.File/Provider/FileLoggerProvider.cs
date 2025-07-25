﻿namespace DecSm.Extensions.Logging.File.Provider;

internal abstract class FileLoggerProvider
{
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
    private readonly IDisposable? _onChangeToken;
    private FileLoggerConfiguration _currentConfig;

    protected FileLoggerProvider(IOptionsMonitor<FileLoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    protected abstract IFileLogWriter LogWriter { get; }

    public ILogger CreateLogger(string categoryName)
    {
        LogWriter.Start();

        #if NET8_0_OR_GREATER
        return _loggers.GetOrAdd(categoryName, name => new(name, LogWriter));
        #else
        return _loggers.GetOrAdd(categoryName, name => new(name, LogWriter))!;
        #endif
    }

    protected FileLoggerConfiguration GetCurrentConfig() =>
        _currentConfig;

    public virtual void Dispose()
    {
        LogWriter.Dispose();
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }
}
