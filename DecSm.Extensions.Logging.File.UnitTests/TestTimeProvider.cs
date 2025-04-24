namespace DecSm.Extensions.Logging.File.UnitTests;

public sealed class TestTimeProvider : TimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = new(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Local;

    public override DateTimeOffset GetUtcNow() =>
        UtcNow;
}
