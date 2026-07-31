using Microsoft.Extensions.Logging;

namespace Nickelony.LanguageServer.Testing;

/// <summary>
/// Captures <see cref="ILogger"/> output for test assertions.
/// </summary>
/// <remarks>
/// Implements <see cref="ILoggerFactory"/> so typed loggers can be obtained via
/// <see cref="LoggerFactoryExtensions.CreateLogger{T}(ILoggerFactory)"/>, and <see cref="ILogger"/>
/// so it can be passed directly to APIs that accept a non-generic logger.
/// Each entry is rendered as <c>Level|Message|ExceptionMessage</c> to match the previous NLog layout.
/// </remarks>
internal sealed class TestLoggerScope : IDisposable, ILogger, ILoggerFactory
{
	private readonly LogLevel _minLevel;
	private readonly object _logsSync = new();

	public TestLoggerScope(LogLevel minLevel)
	{
		_minLevel = minLevel;
		Logs = [];
	}

	public IList<string> Logs { get; }

	public ILogger CreateLogger(string categoryName) => this;

	public void AddProvider(ILoggerProvider provider)
		=> throw new NotSupportedException("TestLoggerScope does not accept external providers.");

	public void Dispose() { }

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		if (!IsEnabled(logLevel))
			return;

		lock (_logsSync)
			Logs.Add($"{FormatLevel(logLevel)}|{formatter(state, exception)}|{exception?.Message}");
	}

	public bool IsEnabled(LogLevel logLevel) => logLevel >= _minLevel && logLevel != LogLevel.None;

	public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

	private static string FormatLevel(LogLevel logLevel) => logLevel switch
	{
		LogLevel.Trace => "Trace",
		LogLevel.Debug => "Debug",
		LogLevel.Information => "Info",
		LogLevel.Warning => "Warn",
		LogLevel.Error => "Error",
		LogLevel.Critical => "Fatal",
		_ => logLevel.ToString()
	};
}
