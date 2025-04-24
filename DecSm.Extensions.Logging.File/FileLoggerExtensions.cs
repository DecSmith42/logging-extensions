namespace DecSm.Extensions.Logging.File;

[PublicAPI]
public static class FileLoggerExtension
{
    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, bool buffered = true)
    {
        builder.AddConfiguration();

        if (buffered)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, BufferedFileLoggerProvider>());
            LoggerProviderOptions.RegisterProviderOptions<FileLoggerConfiguration, BufferedFileLoggerProvider>(builder.Services);
        }
        else
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, DirectFileLoggerProvider>());
            LoggerProviderOptions.RegisterProviderOptions<FileLoggerConfiguration, DirectFileLoggerProvider>(builder.Services);
        }

        return builder;
    }

    public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<FileLoggerConfiguration> configure, bool buffered = true)
    {
        builder.AddFile(buffered);
        builder.Services.Configure(configure);

        return builder;
    }
}
