using System;
using System.Collections.Generic;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense
{
	internal sealed class LuaIntellisenseDocumentManager
	{
		private sealed class DocumentState
		{
			public string FilePath { get; init; }
			public string Uri { get; init; }
			public string Content { get; set; }
			public int Version { get; set; }
			public bool IsOpen { get; set; }
		}

		private readonly object _syncRoot = new();
		private readonly Dictionary<string, DocumentState> _documents = new(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, IReadOnlyList<TextEditorDiagnostic>> _diagnosticsByFilePath = new(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, int> _diagnosticsVersionByFilePath = new(StringComparer.OrdinalIgnoreCase);

		public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath)
		{
			lock (_syncRoot)
				return _diagnosticsByFilePath.TryGetValue(filePath, out IReadOnlyList<TextEditorDiagnostic> diagnostics)
					? diagnostics
					: Array.Empty<TextEditorDiagnostic>();
		}

		public LuaDocumentSynchronizationRequest Synchronize(string filePath, string content)
		{
			string safeContent = content ?? string.Empty;

			lock (_syncRoot)
			{
				if (!_documents.TryGetValue(filePath, out DocumentState state))
				{
					state = new DocumentState
					{
						FilePath = filePath,
						Uri = LuaLanguageServerPathHelper.CreateFileUri(filePath),
						Content = safeContent,
						Version = 1,
						IsOpen = true
					};

					_documents[filePath] = state;
					return new LuaDocumentSynchronizationRequest(LuaDocumentSynchronizationKind.Open, CreateSnapshot(state));
				}

				if (!state.IsOpen)
				{
					state.Content = safeContent;
					state.Version++;
					state.IsOpen = true;
					return new LuaDocumentSynchronizationRequest(LuaDocumentSynchronizationKind.Open, CreateSnapshot(state));
				}

				if (!string.Equals(state.Content, safeContent, StringComparison.Ordinal))
				{
					state.Content = safeContent;
					state.Version++;
					return new LuaDocumentSynchronizationRequest(LuaDocumentSynchronizationKind.Change, CreateSnapshot(state));
				}

				return null;
			}
		}

		public bool TryClose(string filePath, out LuaDocumentSnapshot document)
		{
			lock (_syncRoot)
			{
				if (!_documents.TryGetValue(filePath, out DocumentState state) || !state.IsOpen)
				{
					document = null;
					return false;
				}

				state.IsOpen = false;
				document = CreateSnapshot(state);
				return true;
			}
		}

		public void PrepareForRestart()
		{
			lock (_syncRoot)
			{
				foreach (DocumentState state in _documents.Values)
				{
					if (state.IsOpen)
						state.IsOpen = false;
				}
			}
		}

		public LuaDocumentSnapshot GetDocumentSnapshot(string filePath)
		{
			lock (_syncRoot)
				return _documents.TryGetValue(filePath, out DocumentState state)
					? CreateSnapshot(state)
					: null;
		}

		public bool TryStoreDiagnostics(LuaPublishedDiagnostics publishedDiagnostics)
		{
			lock (_syncRoot)
			{
				if (publishedDiagnostics.Version > 0
					&& _diagnosticsVersionByFilePath.TryGetValue(publishedDiagnostics.FilePath, out int currentVersion)
					&& publishedDiagnostics.Version < currentVersion)
				{
					return false;
				}

				if (publishedDiagnostics.Version > 0)
					_diagnosticsVersionByFilePath[publishedDiagnostics.FilePath] = publishedDiagnostics.Version;

				_diagnosticsByFilePath[publishedDiagnostics.FilePath] = publishedDiagnostics.Diagnostics;
				return true;
			}
		}

		private static LuaDocumentSnapshot CreateSnapshot(DocumentState state)
			=> new LuaDocumentSnapshot(state.FilePath, state.Uri, state.Content, state.Version);
	}

	internal enum LuaDocumentSynchronizationKind
	{
		Open,
		Change
	}

	internal sealed class LuaDocumentSynchronizationRequest
	{
		public LuaDocumentSynchronizationRequest(LuaDocumentSynchronizationKind kind, LuaDocumentSnapshot document)
		{
			Kind = kind;
			Document = document ?? throw new ArgumentNullException(nameof(document));
		}

		public LuaDocumentSynchronizationKind Kind { get; }
		public LuaDocumentSnapshot Document { get; }
	}

	internal sealed class LuaDocumentSnapshot
	{
		public LuaDocumentSnapshot(string filePath, string uri, string content, int version)
		{
			FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
			Uri = uri ?? throw new ArgumentNullException(nameof(uri));
			Content = content ?? string.Empty;
			Version = version;
		}

		public string FilePath { get; }
		public string Uri { get; }
		public string Content { get; }
		public int Version { get; }
	}
}