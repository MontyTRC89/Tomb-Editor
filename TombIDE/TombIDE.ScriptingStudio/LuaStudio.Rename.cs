#nullable enable

using DarkUI.Forms;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Interop;
using TombIDE.ScriptingStudio.TextEditing;
using TombIDE.Shared;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.Scripting.Lua;
using TombLib.Scripting.UI.Editing;

namespace TombIDE.ScriptingStudio;

public sealed partial class LuaStudio
{
	private readonly TextWorkspaceEditApplier _workspaceEditApplier;

	private async Task RenameSymbolAsync()
	{
		if (CurrentEditor is not LuaEditor editor)
		{
			ShowRenameInfo(Strings.Default.LuaRenameNoDocument, MessageBoxIcon.Information);
			return;
		}

		if (!_workspaceCommandService.SupportsRename)
		{
			ShowRenameInfo(Strings.Default.LuaRenameUnsupported, MessageBoxIcon.Information);
			return;
		}

		if (!TryGetRenameTarget(editor, out int renameOffset, out string currentName))
		{
			ShowRenameInfo(Strings.Default.LuaRenameNoSymbol, MessageBoxIcon.Information);
			return;
		}

		if (!TryPromptRenameSymbol(currentName, out string? newName)
			|| string.IsNullOrWhiteSpace(newName)
			|| string.Equals(currentName, newName, StringComparison.Ordinal))
		{
			return;
		}

		TextLocation location = editor.Document.GetLocation(renameOffset);

		try
		{
			TextWorkspaceCommandResult result = await _workspaceCommandService
				.RenameSymbolAsync(
					editor,
					Math.Max(0, location.Line - 1),
					Math.Max(0, location.Column - 1),
					newName)
				.ConfigureAwait(true);

			if (result.Status is TextWorkspaceCommandStatus.Cancelled)
				return;

			if (result.Transaction is not TextWorkspaceEditTransaction transaction || !transaction.HasChanges)
			{
				ShowRenameInfo(Strings.Default.LuaRenameNoChanges, MessageBoxIcon.Information);
				return;
			}

			PushWorkspaceEditTransaction(transaction);
			HandleWorkspaceDocumentsChanged(transaction.DocumentChanges.Select(documentChange => documentChange.FilePath));
		}
		catch (Exception ex)
		{
			DarkMessageBox.Show(this, ex.Message, Strings.Default.RenameSymbol, MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}

	private void ShowRenameInfo(string message, MessageBoxIcon icon)
		=> DarkMessageBox.Show(this, message, Strings.Default.RenameSymbol, MessageBoxButtons.OK, icon);

	private static bool TryGetRenameTarget(LuaEditor editor, out int renameOffset, out string currentName)
	{
		renameOffset = 0;
		currentName = string.Empty;

		if (editor.SelectionLength > 0)
		{
			renameOffset = editor.SelectionStart;
			currentName = editor.SelectedText?.Trim() ?? string.Empty;
			return !string.IsNullOrWhiteSpace(currentName);
		}

		if (!TryGetIdentifierStartOffset(editor.Document, editor.CaretOffset, out renameOffset))
			return false;

		currentName = editor.GetWordFromOffset(renameOffset)?.Trim() ?? string.Empty;
		return !string.IsNullOrWhiteSpace(currentName);
	}

	private bool TryPromptRenameSymbol(string currentName, out string? newName)
	{
		var viewModel = new InputBoxWindowViewModel(Strings.Default.RenameSymbol, Strings.Default.LuaRenamePromptLabel, currentName);
		var window = new InputBoxWindow { DataContext = viewModel };
		PropertyChangedEventHandler? propertyChangedHandler = null;

		propertyChangedHandler = (_, e) =>
		{
			if (e.PropertyName == nameof(InputBoxWindowViewModel.DialogResult) && viewModel.DialogResult.HasValue)
				window.DialogResult = viewModel.DialogResult;
		};

		viewModel.PropertyChanged += propertyChangedHandler;

		try
		{
			if (FindForm() is Form ownerForm)
				new WindowInteropHelper(window).Owner = ownerForm.Handle;

			bool? dialogResult = window.ShowDialog();
			newName = dialogResult == true ? viewModel.Value.Trim() : null;
			return dialogResult == true;
		}
		finally
		{
			viewModel.PropertyChanged -= propertyChangedHandler;
		}
	}

	private bool TryGetOpenLuaEditor(string filePath, [NotNullWhen(true)] out LuaEditor? editor)
	{
		foreach (TabPage tabPage in EditorTabControl.FindTabPagesOfFile(filePath))
		{
			if (EditorTabControl.GetEditorOfTab(tabPage) is LuaEditor luaEditor)
			{
				editor = luaEditor;
				return true;
			}
		}

		editor = null;
		return false;
	}

	private static bool TryGetIdentifierStartOffset(TextDocument document, int offset, out int identifierStartOffset)
	{
		identifierStartOffset = 0;

		if (document.TextLength == 0)
			return false;

		int probeOffset = Math.Clamp(offset, 0, document.TextLength);

		if (probeOffset >= document.TextLength)
			probeOffset = document.TextLength - 1;

		if (probeOffset > 0
			&& !IsLuaIdentifierCharacter(document.GetCharAt(probeOffset))
			&& IsLuaIdentifierCharacter(document.GetCharAt(probeOffset - 1)))
		{
			probeOffset--;
		}

		if (!IsLuaIdentifierCharacter(document.GetCharAt(probeOffset)))
			return false;

		identifierStartOffset = probeOffset;

		while (identifierStartOffset > 0 && IsLuaIdentifierCharacter(document.GetCharAt(identifierStartOffset - 1)))
			identifierStartOffset--;

		return true;
	}

	private static bool IsLuaIdentifierCharacter(char character)
		=> char.IsLetterOrDigit(character) || character == '_';
}