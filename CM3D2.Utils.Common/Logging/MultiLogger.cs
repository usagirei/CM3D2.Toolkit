// --------------------------------------------------
// CM3D2.Toolkit - MultiLogger.cs
// --------------------------------------------------

using System.Collections.Generic;

using CM3D2.Toolkit.Logging;

namespace CM3D2.Utils.Common.Logging
{
    /// <summary>
    ///     Forwards Messages to Multiple Loggers
    /// </summary>
    public class MultiLogger : ILogger
    {
        private readonly Dictionary<string, ILogger> _loggers = new Dictionary<string, ILogger>();

        /// <summary>
        ///     Attaches a new Logger under the specified name
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="name">Name</param>
        /// <returns>True if successful</returns>
        public bool Attach(ILogger logger, string name)
        {
            if (_loggers.ContainsKey(name))
                return false;
            _loggers.Add(name, logger);
            return true;
        }

        /// <summary>
        ///     Detaches a Logger under the specified name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>True if successful</returns>
        public bool Detach(string name)
        {
            if (!_loggers.ContainsKey(name))
                return false;
            _loggers.Remove(name);
            return true;
        }

        /// <summary>
        ///     Retrieves a new Logger under the specified name
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>The Logger or null</returns>
        public ILogger Retrieve(string name)
        {
            if (!_loggers.ContainsKey(name))
                return null;
            return _loggers[name];
        }

        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
            foreach (var logger in _loggers.Values)
                logger.Debug(message, args);
        }

        /// <inheritdoc />
        public void Error(string message, params object[] args)
        {
            foreach (var logger in _loggers.Values)
                logger.Error(message, args);
        }

        /// <inheritdoc />
        public void Info(string message, params object[] args)
        {
            foreach (var logger in _loggers.Values)
                logger.Info(message, args);
        }

        /// <inheritdoc />
        public void Trace(string message, params object[] args)
        {
            foreach (var logger in _loggers.Values)
                logger.Trace(message, args);
        }

        /// <inheritdoc />
        public void Warn(string message, params object[] args)
        {
            foreach (var logger in _loggers.Values)
                logger.Warn(message, args);
        }

        /// <inheritdoc />
        public void Fatal(string message, params object[] args)
        {
            foreach (var logger in _loggers.Values)
                logger.Fatal(message, args);
        }

        public string Name => nameof(MultiLogger);
    }
}
