// --------------------------------------------------
// CM3D2.Toolkit - NullLogger.cs
// --------------------------------------------------

namespace CM3D2.Toolkit.Logging
{
    /// <summary>
    ///     Null Logger
    /// </summary>
    public sealed class NullLogger : ILogger
    {
        /// <summary>
        ///     Null Logger Instance
        /// </summary>
        public static NullLogger Instance = new NullLogger();

        private NullLogger() {}

        /// <inheritdoc />
        public void Debug(string message, params object[] args) {}

        /// <inheritdoc />
        public void Error(string message, params object[] args) {}

        /// <inheritdoc />
        public void Info(string message, params object[] args) {}

        /// <inheritdoc />
        public void Trace(string message, params object[] args) {}

        /// <inheritdoc />
        public void Warn(string message, params object[] args) {}

        /// <inheritdoc />
        public void Fatal(string message, params object[] args) {}

        public string Name => nameof(NullLogger);
    }
}
