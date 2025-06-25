using System;
using JetBrains.Annotations;

namespace GbaMonoGame;

public static class Logger
{
    public static event EventHandler<LogEventArgs> Log;

    private static void OnLog(LogEventArgs e)
    {
        Log?.Invoke(null, e);
    }

    [StringFormatMethod("message")]
    public static void NotImplemented(string message, params object[] args)
    {
        if (Engine.Config?.Debug.DebugModeEnabled == true)
            OnLog(new LogEventArgs(message, args, LogType.NotImplemented));
    }

    [StringFormatMethod("message")]
    public static void Debug(string message, params object[] args)
    {
        if (Engine.Config?.Debug.DebugModeEnabled == true)
            OnLog(new LogEventArgs(message, args, LogType.Debug));
    }

    [StringFormatMethod("message")]
    public static void Info(string message, params object[] args)
    {
        if (Engine.Config?.Debug.DebugModeEnabled == true)
            OnLog(new LogEventArgs(message, args, LogType.Info));
    }

    [StringFormatMethod("message")]
    public static void Error(string message, params object[] args)
    {
        if (Engine.Config?.Debug.DebugModeEnabled == true)
            OnLog(new LogEventArgs(message, args, LogType.Error));
    }

    [Flags]
    public enum LogType
    {
        NotImplemented = 1 << 0,
        Debug = 1 << 1,
        Info = 1 << 2,
        Error = 1 << 3,
    }

    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(string message, object[] args, LogType type)
        {
            Message = message;
            Args = args;
            Type = type;
        }

        public string Message { get; }
        public object[] Args { get; }
        public LogType Type { get; }
    }
}