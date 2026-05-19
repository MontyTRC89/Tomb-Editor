using NLog;
using NLog.Config;
using NLog.Targets;

namespace TombLib.LanguageServer.Core.Tests;

internal sealed class NLogMemoryScope : IDisposable
{
	private readonly LoggingConfiguration? _previousConfiguration;

	public NLogMemoryScope(LogLevel minLevel)
	{
		_previousConfiguration = LogManager.Configuration;

		var target = new MemoryTarget("TestLogs")
		{
			Layout = "${level}|${message}|${exception:format=Message}"
		};

		var configuration = new LoggingConfiguration();
		configuration.AddTarget(target);
		configuration.AddRule(minLevel, LogLevel.Fatal, target);

		LogManager.Configuration = configuration;
		LogManager.ReconfigExistingLoggers();

		Target = target;
	}

	public MemoryTarget Target { get; }

	public IList<string> Logs => Target.Logs;

	public void Dispose()
	{
		LogManager.Configuration = _previousConfiguration;
		LogManager.ReconfigExistingLoggers();
	}
}
