using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace mcp_server
{
    /// <summary>
    /// Very small file logger provider that appends to a single file. Thread-safe and disposable.
    /// </summary>
    public sealed class SimpleFileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        private readonly StreamWriter _writer;
        private readonly object _lock = new object();
        private bool _disposed;

        public SimpleFileLoggerProvider(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            _writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = true
            };
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SimpleFileLoggerProvider));
            return new SimpleFileLogger(categoryName, _writer, _lock);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (_lock)
            {
                _writer.Dispose();
            }
        }

        private sealed class SimpleFileLogger : ILogger
        {
            private readonly string _category;
            private readonly StreamWriter _writer;
            private readonly object _lock;

            public SimpleFileLogger(string category, StreamWriter writer, object @lock)
            {
                _category = category;
                _writer = writer;
                _lock = @lock;
            }

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true; // Filtering is handled by LoggerFactory

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel)) return;
                if (formatter == null) throw new ArgumentNullException(nameof(formatter));

                var timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
                var message = formatter(state, exception);

                var line = $"{timestamp} [{logLevel}] {_category}: {message}";
                lock (_lock)
                {
                    _writer.WriteLine(line);
                    if (exception != null)
                    {
                        _writer.WriteLine(exception);
                    }
                }
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();
            private NullScope() { }
            public void Dispose() { }
        }
    }
}
