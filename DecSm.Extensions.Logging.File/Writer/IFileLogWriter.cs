namespace DecSm.Extensions.Logging.File.Writer;

internal interface IFileLogWriter : IDisposable
{
    internal TimeProvider TimeProvider { get; }

    void Start();

    void Log(string log);
}
