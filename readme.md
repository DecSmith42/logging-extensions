# DecSm.Extensions.Logging.File

[![Build](https://github.com/DecSmith42/logging-extensions/actions/workflows/Build.yml/badge.svg)](https://github.com/DecSmith42/logging-extensions/actions/workflows/Build.yml)
[![Validate](https://github.com/DecSmith42/logging-extensions/actions/workflows/Validate.yml/badge.svg)](https://github.com/DecSmith42/logging-extensions/actions/workflows/Validate.yml)

A simple file logger provider for `Microsoft.Extensions.Logging`.

## Installation

To install the package, run the following command in the Package Manager Console:

```powershell
Install-Package DecSm.Extensions.Logging.File
```

## Usage

Add the file logger to your `ILoggingBuilder` in your `Program.cs` or `Startup.cs`:

### Basic Usage

```csharp
builder.Logging.AddFile();
```

This will register the file logger with the default options. A new log file will be created in the `Logs` directory of your application's root.

### Buffered vs. Direct Logging

The `AddFile` method has an optional `buffered` parameter that defaults to `true`.

- **Buffered (Default):** Logs are written to an in-memory channel and processed by a background service. This is the recommended approach for most applications as it minimizes the performance impact on your application.
- **Direct:** Logs are written directly to the file system. This is useful for console applications or scenarios where you want to ensure logs are written immediately.

To use direct logging, set the `buffered` parameter to `false`:

```csharp
builder.Logging.AddFile(buffered: false);
```

### Advanced Configuration

You can configure the file logger by passing an action to the `AddFile` method:

```csharp
builder.Logging.AddFile(options =>
{
    options.LogDirectory = "MyLogs";
    options.LogName = "my-app";
    options.FileSizeLimitBytes = 10 * 1024 * 1024; // 10 MB
    options.RolloverInterval = FileRolloverInterval.Hour;
    options.MaxTotalSizeBytes = 1024 * 1024 * 1024; // 1 GB
});
```

### Per-Level Log Files

You can also specify a different log file for each log level:

```csharp
builder.Logging.AddFile(options =>
{
    options.PerLevelLogName = new()
    {
        [LogLevel.Information] = "info",
        [LogLevel.Warning] = "warn",
        [LogLevel.Error] = "error",
    };
});
```

This will create separate log files for `Information`, `Warning`, and `Error` level logs. All other log levels will be logged to the default log file.

## Configuration Options

| Option | Description | Default |
| --- | --- | --- |
| `LogDirectory` | The directory to store log files in. | `Logs` |
| `LogName` | The base name for log files. If not specified, the application name will be used. | `null` |
| `PerLevelLogName` | A dictionary to specify a different log file for each log level. | `[]` |
| `FileSizeLimitBytes` | The maximum size of a single log file in bytes. | `104857600` (100 MB) |
| `RolloverInterval` | The interval at which to create a new log file. | `FileRolloverInterval.Day` |
| `MaxTotalSizeBytes` | The maximum total size of all log files in bytes. | `10737418240` (10 GB) |

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.
