using StreamJsonRpc;
using System.Diagnostics;

namespace Nickelony.LanguageServer.Core;

public sealed partial class LanguageServerClient
{
	/// <summary>
	/// Stores all process and transport objects associated with one language-server session generation.
	/// </summary>
	private sealed class LanguageServerTransportSession
	{
		private const int MaxRecentStandardErrorLines = 5;
		private const int MaxRecentStandardErrorLineLength = 200;
		private readonly object _recentStandardErrorSyncRoot = new();
		private readonly Queue<string> _recentStandardErrorLines = [];

		/// <summary>
		/// Initializes a new instance of the <see cref="LanguageServerTransportSession"/> class.
		/// </summary>
		/// <param name="generation">The monotonically increasing session generation.</param>
		/// <param name="process">The owned language-server process, when available.</param>
		/// <param name="serverOutputStream">The process output stream read by the host.</param>
		/// <param name="serverInputStream">The process input stream written by the host.</param>
		public LanguageServerTransportSession(long generation, Process? process, Stream serverOutputStream, Stream serverInputStream)
		{
			Generation = generation;
			Process = process;
			ServerOutputStream = serverOutputStream;
			ServerInputStream = serverInputStream;
		}

		/// <summary>
		/// Gets the session generation associated with this transport.
		/// </summary>
		public long Generation { get; }

		/// <summary>
		/// Gets the owned server process when the session is backed by a spawned process.
		/// </summary>
		public Process? Process { get; }

		/// <summary>
		/// Gets the stream used to read responses and notifications from the server.
		/// </summary>
		public Stream ServerOutputStream { get; }

		/// <summary>
		/// Gets the stream used to write requests and notifications to the server.
		/// </summary>
		public Stream ServerInputStream { get; }

		/// <summary>
		/// Gets or sets the message handler bound to the transport streams.
		/// </summary>
		public HeaderDelimitedMessageHandler? MessageHandler { get; set; }

		/// <summary>
		/// Gets or sets the JSON-RPC instance driving the session.
		/// </summary>
		public JsonRpc? JsonRpc { get; set; }

		/// <summary>
		/// Gets or sets the callback target exposed to the language server.
		/// </summary>
		public LanguageServerClientRpcTarget? RpcTarget { get; set; }

		/// <summary>
		/// Gets or sets the cached process-exited event handler for later detachment.
		/// </summary>
		public EventHandler? ProcessExitedHandler { get; set; }

		/// <summary>
		/// Gets or sets the JSON-RPC completion task for orderly shutdown.
		/// </summary>
		public Task? RpcCompletionTask { get; set; }

		/// <summary>
		/// Gets or sets the standard-error read loop task for orderly shutdown.
		/// </summary>
		public Task? StderrLoopTask { get; set; }

		/// <summary>
		/// Records one recent non-empty stderr line for failure diagnostics.
		/// </summary>
		/// <param name="line">The stderr line.</param>
		public void RecordStandardErrorLine(string line)
		{
			if (string.IsNullOrWhiteSpace(line))
				return;

			string trimmedLine = line.Trim();

			if (trimmedLine.Length > MaxRecentStandardErrorLineLength)
				trimmedLine = trimmedLine[..MaxRecentStandardErrorLineLength] + "...";

			lock (_recentStandardErrorSyncRoot)
			{
				_recentStandardErrorLines.Enqueue(trimmedLine);

				while (_recentStandardErrorLines.Count > MaxRecentStandardErrorLines)
					_recentStandardErrorLines.Dequeue();
			}
		}

		/// <summary>
		/// Gets a compact summary of recent stderr lines for failure logging.
		/// </summary>
		/// <returns>The recent stderr summary, or <see langword="null"/> when none was recorded.</returns>
		public string? GetRecentStandardErrorSummary()
		{
			lock (_recentStandardErrorSyncRoot)
			{
				if (_recentStandardErrorLines.Count == 0)
					return null;

				return string.Join(" | ", _recentStandardErrorLines);
			}
		}
	}
}
