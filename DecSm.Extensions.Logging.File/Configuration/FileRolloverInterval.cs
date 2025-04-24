namespace DecSm.Extensions.Logging.File.Configuration;

[PublicAPI]
public enum FileRolloverInterval
{
    Infinite,
    Year,
    Month,
    Day,
    Hour,
    Minute,
}
