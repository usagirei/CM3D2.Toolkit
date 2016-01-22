using System;

using CM3D2.Toolkit.Logging;

namespace CM3D2.Utils.Common.Logging
{
    /// <summary>
    ///    Debug Trace Output Based Logger
    /// </summary>
    public class TraceLogger : ILogger
    {
        private const int DEBUG = 1 << (int)LogLevel.Debug;
        private const int ERROR = 1 << (int)LogLevel.Error;
        private const int FATAL = 1 << (int)LogLevel.Fatal;
        private const int INFO = 1 << (int)LogLevel.Info;
        private const int TRACE = 1 << (int)LogLevel.Trace;
        private const int WARN = 1 << (int)LogLevel.Warn;

        private int _flags = ERROR | FATAL | INFO | WARN | TRACE | DEBUG;

        /// <summary>
        ///     Debug Messages Enabled
        /// </summary>
        public bool EnableDebug
        {
            get { return (_flags & DEBUG) == DEBUG; }
            set
            {
                _flags = value
                             ? _flags | DEBUG
                             : _flags & ~DEBUG;
            }
        }

        /// <summary>
        ///     Error Messages Enabled
        /// </summary>
        public bool EnableError
        {
            get { return (_flags & ERROR) == ERROR; }
            set
            {
                _flags = value
                             ? _flags | ERROR
                             : _flags & ~ERROR;
            }
        }

        /// <summary>
        ///     Fatal Messages Enabled
        /// </summary>
        public bool EnableFatal
        {
            get { return (_flags & FATAL) == FATAL; }
            set
            {
                _flags = value
                             ? _flags | FATAL
                             : _flags & ~FATAL;
            }
        }

        /// <summary>
        ///     Info Messages Enabled
        /// </summary>
        public bool EnableInfo
        {
            get { return (_flags & INFO) == INFO; }
            set
            {
                _flags = value
                             ? _flags | INFO
                             : _flags & ~INFO;
            }
        }

        /// <summary>
        ///     Trace Messages Enabled
        /// </summary>
        public bool EnableTrace
        {
            get { return (_flags & TRACE) == TRACE; }
            set
            {
                _flags = value
                             ? _flags | TRACE
                             : _flags & ~TRACE;
            }
        }

        /// <summary>
        ///     Warn Messages Enabled
        /// </summary>
        public bool EnableWarn
        {
            get { return (_flags & WARN) == WARN; }
            set
            {
                _flags = value
                             ? _flags | WARN
                             : _flags & ~WARN;
            }
        }

        /// <summary>
        /// Creates a new Instance of the <see cref="TraceLogger"/> named <paramref name="name"/>
        /// </summary>
        /// <param name="name"></param>
        public TraceLogger(string name)
        {
            Name = name;
        }

        private static string GetTimeStamp() => DateTime.Now.ToString("yy/mm/dd-hh:MM:ss");

        /// <inheritdoc />
        public void Debug(string message, params object[] args)
        {
            if (!EnableDebug)
                return;
            var msg = string.Format(message, args);
            System.Diagnostics.Debug.WriteLine($"[{Name}] [DEBUG] {GetTimeStamp()} - {msg}");
        }

        /// <inheritdoc />
        public void Error(string message, params object[] args)
        {
            if (!EnableError)
                return;
            var msg = string.Format(message, args);
            System.Diagnostics.Debug.WriteLine($"[{Name}] [ERROR] {GetTimeStamp()} - {msg}");
        }

        /// <inheritdoc />
        public void Info(string message, params object[] args)
        {
            var msg = string.Format(message, args);
            System.Diagnostics.Debug.WriteLine($"[{Name}] [INFO ] {GetTimeStamp()} - {msg}");
        }

        /// <inheritdoc />
        public void Trace(string message, params object[] args)
        {
            if (!EnableTrace)
                return;
            var msg = string.Format(message, args);
            System.Diagnostics.Debug.WriteLine($"[{Name}] [TRACE] {GetTimeStamp()} - {msg}");
        }

        /// <inheritdoc />
        public void Warn(string message, params object[] args)
        {
            if (!EnableWarn)
                return;
            var msg = string.Format(message, args);
            System.Diagnostics.Debug.WriteLine($"[{Name}] [WARN ] {GetTimeStamp()} - {msg}");
        }

        /// <inheritdoc />
        public void Fatal(string message, params object[] args)
        {
            if (!EnableFatal)
                return;
            var msg = string.Format(message, args);
            System.Diagnostics.Debug.WriteLine($"[{Name}] [FATAL] {GetTimeStamp()} - {msg}");
        }

        /// <inheritdoc />
        public string Name { get; set; }
    }
}