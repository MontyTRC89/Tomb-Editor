#nullable enable

using DarkUI.Forms;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.Shared;
using TombLib.Scripting.Editing;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Editing;

namespace TombIDE.ScriptingStudio;

public sealed partial class LuaStudio
{
	private readonly TextWorkspaceCommandService _workspaceCommandService;

	private Task ReformatDocumentAsync()
		=> FormatDocumentAsync(_intellisenseProvider, Strings.Default.LuaReformatUnsupported, Strings.Default.Reindent);

	private Task TrimWhitespaceAsync()
		=> FormatDocumentAsync(_trimWhitespaceProvider, unsupportedMessage: null, Strings.Default.TrimWhitespace);

	private async Task FormatDocumentAsync(ITextFormattingProvider formattingProvider, string? unsupportedMessage, string commandName)
	{
		if (CurrentEditor is not LuaEditor editor)
			return;

		if (!formattingProvider.SupportsFormatting)
		{
			if (!string.IsNullOrWhiteSpace(unsupportedMessage))
			{
				DarkMessageBox.Show(this,
					unsupportedMessage,
					commandName,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}

			return;
		}

		try
		{
			TextWorkspaceCommandResult result = await _workspaceCommandService
				.FormatDocumentAsync(editor, formattingProvider)
				.ConfigureAwait(true);

			if (result.Status is TextWorkspaceCommandStatus.Cancelled)
				return;

			if (result.Transaction is not TextWorkspaceEditTransaction transaction || !transaction.HasChanges)
				return;

			PushWorkspaceEditTransaction(transaction);
			HandleWorkspaceDocumentsChanged(transaction.DocumentChanges.Select(documentChange => documentChange.FilePath));
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(this, ex.Message, commandName, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
}