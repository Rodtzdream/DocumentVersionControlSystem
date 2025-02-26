namespace DocumentVersionControlSystem.Logging;

using DocumentVersionControlSystem.Infrastructure;
using Serilog;

public class Logger : IDisposable
{
    private static readonly ILogger _logger;

    static Logger()
    {
        _logger = new LoggerConfiguration()
            .WriteTo.File(AppPaths.AppFolderPath + "/log.txt")
            .CreateLogger();
    }

    public void LogInformation(string message)
    {
        _logger.Information(message);
    }

    public void LogWarning(string message)
    {
        _logger.Warning(message);
    }

    public void LogError(string message)
    {
        _logger.Error(message);
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
    }
}
