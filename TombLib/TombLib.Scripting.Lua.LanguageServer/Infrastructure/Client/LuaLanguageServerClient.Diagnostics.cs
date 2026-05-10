using System.Collections.Concurrent;
using System.Threading.Channels;

namespace TombLib.Scripting.Lua.LanguageServer;

public sealed partial class LuaLanguageServerClient
{
	private readonly record struct QueuedDiagnostics(long TransportGeneration, LuaPublishDiagnosticsParams Parameters);

	private readonly ConcurrentDictionary<string, QueuedDiagnostics> _pendingDiagnostics = new(StringComparer.OrdinalIgnoreCase);

	// Diagnostics arrive on the LSP read loop and are stored as the latest payload per file URI.
	// A bounded single-slot channel acts only as a wake signal for the pump, so bursty notifications
	// for the same file collapse to one queued wake-up instead of building an unbounded backlog.
	private readonly Channel<bool> _diagnosticsSignal = Channel.CreateBounded<bool>(
		new BoundedChannelOptions(1)
		{
			SingleReader = true,
			SingleWriter = true,
			AllowSynchronousContinuations = false,
			FullMode = BoundedChannelFullMode.DropWrite
		});

	private Task? _diagnosticsPumpTask;
	private long _diagnosticsFallbackSequence;

	/// <summary>
	/// Occurs when the server publishes diagnostics for a tracked document.
	/// </summary>
	public event Action<LuaPublishDiagnosticsParams>? DiagnosticsPublished;

	private void RaiseDiagnosticsPublished(long transportGeneration, LuaPublishDiagnosticsParams parameters)
	{
		// Stash only the newest diagnostics payload per file and wake the pump if it is idle.
		// Parameters were already cloned by the dispatcher in HandleMessageAsync.
		_pendingDiagnostics[GetDiagnosticsQueueKey(parameters)] = new QueuedDiagnostics(transportGeneration, parameters);
		_diagnosticsSignal.Writer.TryWrite(true);
	}

	private async Task PumpDiagnosticsAsync()
	{
		ChannelReader<bool> reader = _diagnosticsSignal.Reader;

		try
		{
			while (await reader.WaitToReadAsync(_lifetimeCts.Token).ConfigureAwait(false))
			{
				while (reader.TryRead(out _))
				{ }

				while (!_pendingDiagnostics.IsEmpty)
				{
					KeyValuePair<string, QueuedDiagnostics>[] pendingDiagnostics = [.. _pendingDiagnostics];

					for (int i = 0; i < pendingDiagnostics.Length; i++)
					{
						if (!_pendingDiagnostics.TryRemove(pendingDiagnostics[i].Key, out QueuedDiagnostics queuedDiagnostics)
							|| queuedDiagnostics.TransportGeneration != Volatile.Read(ref _activeTransportGeneration))
							continue;

						try
						{
							DiagnosticsPublished?.Invoke(queuedDiagnostics.Parameters);
						}
						catch (Exception exception)
						{
							Log.Warn(exception, "Lua diagnostics handler threw; the diagnostics pump is being kept alive.");
						}
					}
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Expected on dispose.
		}
	}

	private string GetDiagnosticsQueueKey(LuaPublishDiagnosticsParams parameters)
	{
		if (!string.IsNullOrWhiteSpace(parameters.Uri))
			return parameters.Uri;

		return "diagnostics:" + Interlocked.Increment(ref _diagnosticsFallbackSequence);
	}
}
