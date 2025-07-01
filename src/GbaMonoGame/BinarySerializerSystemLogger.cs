using BinarySerializer;

namespace GbaMonoGame;

public class BinarySerializerSystemLogger : ISystemLogger
{
    public void Log(LogLevel logLevel, object log, params object[] args)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                Logger.Debug(log?.ToString(), args);
                break;

            case LogLevel.Info:
                Logger.Info(log?.ToString(), args);
                break;
            case LogLevel.Warning:
            case LogLevel.Error:
                Logger.Error(log?.ToString(), args);
                break;
        }
    }
}