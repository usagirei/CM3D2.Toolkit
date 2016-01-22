// --------------------------------------------------
// CM3D2.Toolkit - EventLogger.cs
// --------------------------------------------------

using CM3D2.Toolkit.Logging;

namespace CM3D2.Utils.Common.Logging
{
    /// <summary>
    ///     Event Based Message Logger
    /// </summary>
    public class EventLogger : ILogger
    {
        /// <summary>
        ///     Direct Log Delegate
        /// </summary>
        /// <param name="name">Logger Name</param>
        /// <param name="message">Message</param>
        public delegate void LogDelegate(string name, string message);

        /// <summary>
        ///     Leveled Log Delegate
        /// </summary>
        /// <param name="level">Log Level</param>
        /// <param name="name">Logger Name</param>
        /// <param name="message">Message</param>
        public delegate void LogLevelDelegate(LogLevel level, string name, string message);

        //<inheritdoc/>
        public string Name { get; set; }

        /// <summary>
        ///     Creates a new Instance of the <see cref="EventLogger" /> named <paramref name="name"/>
        /// </summary>
        /// <param name="name">Logger Name</param>
        public EventLogger(string name)
        {
            Name = name;
        }

        /// <inheritdoc />
        public void Fatal(string message, params object[] args)
        {
            var formatted = string.Format(message, args);
            OnFatal?.Invoke(Name, formatted);
            OnLog?.Invoke(LogLevel.Fatal, Name, formatted);
        }

        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
            var formatted = string.Format(message, args);
            OnDebug?.Invoke(Name, formatted);
            OnLog?.Invoke(LogLevel.Debug, Name, formatted);
        }

        /// <inheritdoc />
        public void Error(string message, params object[] args)
        {
            var formatted = string.Format(message, args);
            OnError?.Invoke(Name, formatted);
            OnLog?.Invoke(LogLevel.Error, Name, formatted);
        }

        /// <inheritdoc />
        public void Info(string message, params object[] args)
        {
            var formatted = string.Format(message, args);
            OnInfo?.Invoke(Name, formatted);
            OnLog?.Invoke(LogLevel.Info, Name, formatted);
        }

        /// <inheritdoc />
        public void Trace(string message, params object[] args)
        {
            var formatted = string.Format(message, args);
            OnTrace?.Invoke(Name, formatted);
            OnLog?.Invoke(LogLevel.Trace, Name, formatted);
        }

        /// <inheritdoc />
        public void Warn(string message, params object[] args)
        {
            var formatted = string.Format(message, args);
            OnWarn?.Invoke(Name, formatted);
            OnLog?.Invoke(LogLevel.Warn, Name, formatted);
        }

        /// <summary>
        ///     Debug Level Log Event
        /// </summary>
        public static event LogDelegate OnDebug;

        /// <summary>
        ///     Error Level Log Event
        /// </summary>
        public static event LogDelegate OnError;

        /// <summary>
        ///     Fatal Level Log Event
        /// </summary>
        public static event LogDelegate OnFatal;

        /// <summary>
        ///     Information Level Log Event
        /// </summary>
        public static event LogDelegate OnInfo;

        /// <summary>
        ///     Log Event with <see cref="LogLevel" /> parameter
        /// </summary>
        public static event LogLevelDelegate OnLog;

        /// <summary>
        ///     Trace Level Log Event
        /// </summary>
        public static event LogDelegate OnTrace;

        /// <summary>
        ///     Warn Level Log Event
        /// </summary>
        public static event LogDelegate OnWarn;
    }
}
