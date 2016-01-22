namespace CM3D2.Toolkit.Logging
{
    /// <summary>
    ///     Simple Logging Abstraction Layer
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        ///     Debug-Level Log Message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Parameters</param>
        void Debug(string message, params object[] args);

        /// <summary>
        ///     Error-Level Log Message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Parameters</param>
        void Error(string message, params object[] args);

        /// <summary>
        ///     Info-Level Log Message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Parameters</param>
        void Info(string message, params object[] args);

        /// <summary>
        ///     Trace-Level Log Message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Parameters</param>
        void Trace(string message, params object[] args);

        /// <summary>
        ///     Warn-Level Log Message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Parameters</param>
        void Warn(string message, params object[] args);

        /// <summary>
        ///     Fatal-Level Log Message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="args">Parameters</param>
        void Fatal(string message, params object[] args);

        /// <summary>
        ///     Logger Name
        /// </summary>
        string Name { get; }
    }
}