#nullable enable

using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Services;
using TombIDE.Shared;
using TombLib.Scripting.Lua;
using TombLib.Scripting.Lua.Objects;

namespace TombIDE.ScriptingStudio;

public sealed partial class LuaStudio
{
	private async Task ReformatDocumentAsync()
	{
		if (CurrentEditor is not LuaEditor editor)
			return;

		if (!_intellisenseProvider.SupportsFormatting)
		{
			DarkMessageBox.Show(this,
				Strings.Default.LuaReformatUnsupported,
				Strings.Default.Reindent,
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
			return;
		}

		try
		{
			LuaFormattingOptions formattingOptions = CreateFormattingOptions(editor);
			IReadOnlyList<LuaTextEdit> textEdits = await _intellisenseProvider
				.FormatDocumentAsync(editor.FilePath, editor.Text, formattingOptions)
				.ConfigureAwait(true);

			if (textEdits.Count == 0)
				return;

			LuaWorkspaceEdit workspaceEdit = new([
				new LuaDocumentEdit(editor.FilePath, textEdits)
			]);

			LuaWorkspaceEditSelectionState selectionState = LuaWorkspaceEditSelectionState.Capture(editor);
			LuaWorkspaceEditTransaction transaction = _workspaceEditApplier.Apply(workspaceEdit, selectionState);

			if (!transaction.HasChanges)
				return;

			PushWorkspaceEditTransaction(transaction);
			HandleWorkspaceDocumentsChanged(transaction.DocumentChanges.Select(documentChange => documentChange.FilePath));
		}
		catch (OperationCanceledException)
		{
			// Ignore canceled formatting requests.
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(this, ex.Message, Strings.Default.Reindent, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private static LuaFormattingOptions CreateFormattingOptions(LuaEditor editor)
	{
		int tabSize = editor.Options.IndentationSize > 0
			? editor.Options.IndentationSize
			: 4;

		return new LuaFormattingOptions(tabSize, editor.Options.ConvertTabsToSpaces);
	}
}