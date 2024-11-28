namespace DocumentVersionControlSystem.Logging;
using Serilog;

public class Logger
{
    Logger()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("log.txt")
            .CreateLogger();
    }

    public void LogInformation(string message)
    {
        Log.Information(message);
    }

    public void LogError(string message)
    {
        Log.Error(message);
    }
}
