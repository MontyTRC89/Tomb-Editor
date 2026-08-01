#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using Nickelony.LanguageServer.Abstractions.Editing;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editing;

namespace TombIDE.ScriptingStudio.TextEditing;

internal sealed class TextWorkspaceCommandService(TextWorkspaceEditApplier workspaceEditApplier, ITextEditProvider? editProvider = null)
{
	private readonly ITextEditProvider? _editProvider = editProvider;
	private readonly TextWorkspaceEditApplier _workspaceEditApplier = workspaceEditApplier ?? throw new ArgumentNullException(nameof(workspaceEditApplier));

	public bool SupportsRename => _editProvider?.SupportsRename == true;

	public async Task<TextWorkspaceCommandResult> FormatDocumentAsync(
		TextEditorBase editor,
		ITextFormattingProvider formattingProvider,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(formattingProvider);

		try
		{
			TextWorkspaceEdit? workspaceEdit = await formattingProvider
				.FormatDocumentAsync(CreateFormatRequest(editor), cancellationToken)
				.ConfigureAwait(true);

			if (workspaceEdit is null || !workspaceEdit.HasEdits)
				return TextWorkspaceCommandResult.NoChanges;

			TextWorkspaceEditSelectionState selectionState = TextWorkspaceEditSelectionState.Capture(editor);
			TextWorkspaceEditTransaction transaction = _workspaceEditApplier.Apply(workspaceEdit, selectionState);

			return transaction.HasChanges
				? TextWorkspaceCommandResult.Applied(transaction)
				: TextWorkspaceCommandResult.NoChanges;
		}
		catch (OperationCanceledException)
		{
			return TextWorkspaceCommandResult.Cancelled;
		}
	}

	public async Task<TextWorkspaceCommandResult> RenameSymbolAsync(
		TextEditorBase editor,
		int line,
		int column,
		string newName,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(editor);

		if (_editProvider is null)
			return TextWorkspaceCommandResult.NoChanges;

		try
		{
			TextWorkspaceEdit? workspaceEdit = await _editProvider
				.RenameSymbolAsync(new TextRenameRequest(editor.FilePath, editor.Text, line, column, newName), cancellationToken)
				.ConfigureAwait(true);

			if (workspaceEdit is null || !workspaceEdit.HasEdits)
				return TextWorkspaceCommandResult.NoChanges;

			TextWorkspaceEditTransaction transaction = _workspaceEditApplier.Apply(workspaceEdit);

			return transaction.HasChanges
				? TextWorkspaceCommandResult.Applied(transaction)
				: TextWorkspaceCommandResult.NoChanges;
		}
		catch (OperationCanceledException)
		{
			return TextWorkspaceCommandResult.Cancelled;
		}
	}

	private static TextFormatRequest CreateFormatRequest(TextEditorBase editor)
	{
		int tabSize = editor.Options.IndentationSize > 0
			? editor.Options.IndentationSize
			: 4;

		return new TextFormatRequest(
			editor.FilePath,
			editor.Text,
			new TextFormattingOptions(tabSize, editor.Options.ConvertTabsToSpaces));
	}
}
