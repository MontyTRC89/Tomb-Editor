#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using TombLib.Scripting.Lua.Objects;
using TombLib.Scripting.Objects;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Tracks the local document state mirrored to LuaLS, including versions, diagnostics, and semantic-token caches.
/// </summary>
internal sealed class LuaIntellisenseDocumentManager
{
	private sealed class DocumentState
	{
		public required string FilePath { get; init; }
		public required string Uri { get; init; }
		public string Content { get; set; } = string.Empty;
		public int Version { get; set; }
		public bool IsOpen { get; set; }

		// Number of editor tabs that currently consider this document open. The provider only sends
		// `textDocument/didClose` to LuaLS when this count returns to zero, so closing one tab while
		// another tab still shows the same file does not strip diagnostics or semantic tokens from
		// the surviving editor.
		public int OpenReferenceCount { get; set; }

		public IReadOnlyList<TextEditorDiagnostic> Diagnostics { get; set; } = [];
		public int DiagnosticsVersion { get; set; }

		public IReadOnlyList<LuaSemanticToken> SemanticTokens { get; set; } = [];
		public int SemanticTokensVersion { get; set; }

		// Last raw `data` payload returned by `textDocument/semanticTokens/full(/delta)` for this
		// file, kept around so an incoming delta `edits` payload can be applied without forcing the
		// server to resend the entire token stream.
		public int[]? SemanticTokensData { get; set; }
		public string? SemanticTokensResultId { get; set; }
	}

	private readonly object _syncRoot = new();
	private readonly Dictionary<string, DocumentState> _documents = new(StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Gets the cached diagnostics for the specified normalized file path.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <returns>The cached diagnostics, or an empty list when none are stored.</returns>
	public IReadOnlyList<TextEditorDiagnostic> GetDiagnostics(string filePath)
	{
		lock (_syncRoot)
			return _documents.TryGetValue(filePath, out DocumentState? state) ? state.Diagnostics : [];
	}

	/// <summary>
	/// Gets the cached semantic tokens for the specified normalized file path.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <returns>The cached semantic tokens, or an empty list when none are stored.</returns>
	public IReadOnlyList<LuaSemanticToken> GetSemanticTokens(string filePath)
	{
		lock (_syncRoot)
			return _documents.TryGetValue(filePath, out DocumentState? state) ? state.SemanticTokens : [];
	}

	/// <summary>
	/// Synchronizes the tracked state for a document and returns the LSP action required to mirror it to LuaLS.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <param name="content">The latest document content.</param>
	/// <param name="acquireOpenReference">Whether an additional open-editor reference should be recorded.</param>
	/// <returns>A synchronization request when LuaLS must be updated; otherwise, <see langword="null"/>.</returns>
	public LuaDocumentSynchronizationRequest? Synchronize(string filePath, string? content, bool acquireOpenReference = false)
	{
		string safeContent = content ?? string.Empty;

		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(filePath, out DocumentState? state))
			{
				state = new DocumentState
				{
					FilePath = filePath,
					Uri = LuaLanguageServerPathHelper.CreateFileUri(filePath),
					Content = safeContent,
					Version = 1,
					IsOpen = true,
					OpenReferenceCount = acquireOpenReference ? 1 : 0
				};

				_documents[filePath] = state;
				return new LuaDocumentSynchronizationRequest(LuaDocumentSynchronizationKind.Open, CreateSnapshot(state));
			}

			if (acquireOpenReference)
				state.OpenReferenceCount++;

			if (!state.IsOpen)
			{
				state.Content = safeContent;
				state.Version++;
				state.IsOpen = true;
				return new LuaDocumentSynchronizationRequest(LuaDocumentSynchronizationKind.Open, CreateSnapshot(state));
			}

			if (!string.Equals(state.Content, safeContent, StringComparison.Ordinal))
			{
				string previousContent = state.Content;
				state.Content = safeContent;
				state.Version++;

				LuaDocumentLineOffsets previousOffsets = LuaDocumentLineOffsets.Build(previousContent);
				LuaDocumentChangeRange changeRange = LuaIncrementalEditCalculator.Compute(previousContent, safeContent, previousOffsets);
				return new LuaDocumentSynchronizationRequest(LuaDocumentSynchronizationKind.Change, CreateSnapshot(state), changeRange);
			}

			return null;
		}
	}

	/// <summary>
	/// Releases one open reference for <paramref name="filePath"/>. The document is only fully
	/// removed (and a `didClose` snapshot returned) once the reference count drops to zero,
	/// so multiple editor tabs sharing the same file do not invalidate each other on close.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <param name="document">When this method returns, contains the closing snapshot if the document was removed.</param>
	/// <returns><see langword="true"/> when the caller should send <c>didClose</c>; otherwise, <see langword="false"/>.</returns>
	public bool TryClose(string filePath, [NotNullWhen(true)] out LuaDocumentSnapshot? document)
	{
		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(filePath, out DocumentState? state) || !state.IsOpen)
			{
				document = null;
				return false;
			}

			if (state.OpenReferenceCount > 0)
				state.OpenReferenceCount--;

			if (state.OpenReferenceCount > 0)
			{
				document = null;
				return false;
			}

			document = CreateSnapshot(state);
			_documents.Remove(filePath);
			return true;
		}
	}

	/// <summary>
	/// Marks all open documents for replay after a language-server restart and returns their snapshots.
	/// </summary>
	/// <returns>The snapshots that should be reopened on the next successful start.</returns>
	public IReadOnlyList<LuaDocumentSnapshot> PrepareForRestart()
	{
		lock (_syncRoot)
		{
			var documentsToReopen = new List<LuaDocumentSnapshot>();

			foreach (DocumentState state in _documents.Values)
			{
				if (!state.IsOpen)
					continue;

				documentsToReopen.Add(CreateSnapshot(state));
				state.IsOpen = false;
			}

			return documentsToReopen;
		}
	}

	/// <summary>
	/// Gets the current snapshot for a tracked document.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <returns>The current snapshot, or <see langword="null"/> when the document is not tracked.</returns>
	public LuaDocumentSnapshot? GetDocumentSnapshot(string filePath)
	{
		lock (_syncRoot)
		{
			return _documents.TryGetValue(filePath, out DocumentState? state)
				? CreateSnapshot(state)
				: null;
		}
	}

	/// <summary>
	/// Gets snapshots for all documents that are currently considered open.
	/// </summary>
	/// <returns>The open-document snapshots.</returns>
	public IReadOnlyList<LuaDocumentSnapshot> GetOpenDocuments()
	{
		lock (_syncRoot)
		{
			var documents = new List<LuaDocumentSnapshot>();

			foreach (DocumentState state in _documents.Values)
			{
				if (state.IsOpen)
					documents.Add(CreateSnapshot(state));
			}

			return documents;
		}
	}

	/// <summary>
	/// Stores a diagnostics payload when it is not stale for the tracked document version.
	/// </summary>
	/// <param name="publishedDiagnostics">The diagnostics payload to cache.</param>
	/// <returns><see langword="true"/> when the payload was stored; otherwise, <see langword="false"/>.</returns>
	public bool TryStoreDiagnostics(LuaPublishedDiagnostics publishedDiagnostics)
	{
		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(publishedDiagnostics.FilePath, out DocumentState? state))
				return false;

			if (IsStaleVersion(state.DiagnosticsVersion, publishedDiagnostics.Version))
				return false;

			if (publishedDiagnostics.Version > 0)
				state.DiagnosticsVersion = publishedDiagnostics.Version;

			state.Diagnostics = publishedDiagnostics.Diagnostics;
			return true;
		}
	}

	/// <summary>
	/// Stores semantic tokens when they are not stale for the tracked document version.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <param name="version">The document version associated with the tokens.</param>
	/// <param name="semanticTokens">The semantic tokens to cache.</param>
	/// <returns><see langword="true"/> when the token set was stored; otherwise, <see langword="false"/>.</returns>
	public bool TryStoreSemanticTokens(string filePath, int version, IReadOnlyList<LuaSemanticToken> semanticTokens)
	{
		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(filePath, out DocumentState? state))
				return false;

			if (IsStaleVersion(state.SemanticTokensVersion, version))
				return false;

			if (version > 0)
				state.SemanticTokensVersion = version;

			state.SemanticTokens = semanticTokens ?? [];
			return true;
		}
	}

	/// <summary>
	/// Returns the cached semantic-tokens delta state for <paramref name="filePath"/>, if any.
	/// Used by the provider to send `semanticTokens/full/delta` requests with the previous result id.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <returns>The previous result id and cached integer stream, if available.</returns>
	public (string? PreviousResultId, int[]? PreviousData) GetSemanticTokensDeltaState(string filePath)
	{
		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(filePath, out DocumentState? state))
				return (null, null);

			return (state.SemanticTokensResultId, state.SemanticTokensData);
		}
	}

	/// <summary>
	/// Stores the raw `data` payload returned by `semanticTokens/full(/delta)` along with the
	/// associated `resultId`, so subsequent requests can ask LuaLS for incremental edits.
	/// </summary>
	/// <param name="filePath">The normalized file path.</param>
	/// <param name="resultId">The server-provided semantic-token result id.</param>
	/// <param name="data">The cached integer token stream.</param>
	public void StoreSemanticTokensDeltaState(string filePath, string? resultId, int[]? data)
	{
		lock (_syncRoot)
		{
			if (!_documents.TryGetValue(filePath, out DocumentState? state))
				return;

			state.SemanticTokensResultId = resultId;
			state.SemanticTokensData = data;
		}
	}

	private static bool IsStaleVersion(int currentVersion, int incomingVersion)
		=> incomingVersion > 0 && currentVersion > 0 && incomingVersion < currentVersion;

	private static LuaDocumentSnapshot CreateSnapshot(DocumentState state)
		=> new(state.FilePath, state.Uri, state.Content, state.Version);
}
