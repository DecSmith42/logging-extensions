namespace DecSm.Extensions.Logging.File.UnitTests;

[TestFixture]
public sealed class BasicTests : TestBase
{
    [SetUp]
    public void SetUp()
    {
        FileSystem = new();
        BufferedFileLoggerProvider.FileSystem = FileSystem;

        TimeProvider = new();
        BufferedFileLoggerProvider.TimeProvider = TimeProvider;
    }

    [Test]
    public void Logger_Logs_Log()
    {
        // Arrange
        var logPath = GetLogPath();
        var logger = CreateBuilderWithLogger<BasicTests>();

        // Act
        logger.LogInformation("Hello, world!");
        StopApp();

        // Assert
        FileSystem.ShouldSatisfyAllConditions(fs => fs.File.Exists(logPath),
            fs => fs
                .File
                .ReadAllText(logPath)
                .ShouldBe("""
                          [2020-01-01 10:00:00.000 +10:00 INF DecSm.Extensions.Logging.File.UnitTests.BasicTests] Hello, world!

                          """));
    }

    [Test]
    public void Logger_Respects_Rooted_Config_Path()
    {
        // Arrange
        const string logsDirectory = @"C:\Logs";
        var logPath = FileSystem.Path.Combine(logsDirectory, $"{AppDomain.CurrentDomain.FriendlyName}.log");

        var logger = CreateBuilderWithLogger<BasicTests>(c => c.LogDirectory = logsDirectory);

        // Act
        logger.LogInformation("Hello, world!");
        StopApp();

        // Assert
        FileSystem.ShouldSatisfyAllConditions(fs => fs
                .File
                .Exists(logPath)
                .ShouldBeTrue(),
            fs => fs
                .File
                .ReadAllText(logPath)
                .ShouldBe("""
                          [2020-01-01 10:00:00.000 +10:00 INF DecSm.Extensions.Logging.File.UnitTests.BasicTests] Hello, world!

                          """));
    }
}
