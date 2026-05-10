#nullable enable

using System;
using TombLib.Scripting.Bases;

namespace TombIDE.ScriptingStudio.Services;

internal sealed class LuaWorkspaceEditSelectionState
{
	private LuaWorkspaceEditSelectionState(string filePath, TextEditorBase editor, int selectionStart, int selectionEnd, int caretOffset)
	{
		FilePath = filePath;
		Editor = editor;
		SelectionStart = selectionStart;
		SelectionEnd = selectionEnd;
		CaretOffset = caretOffset;
	}

	public string FilePath { get; }

	public TextEditorBase Editor { get; }

	public int SelectionStart { get; }

	public int SelectionEnd { get; }

	public int CaretOffset { get; }

	public static LuaWorkspaceEditSelectionState Capture(TextEditorBase editor)
	{
		ArgumentNullException.ThrowIfNull(editor);

		int selectionStart = editor.SelectionStart;
		int selectionEnd = selectionStart + editor.SelectionLength;

		return new LuaWorkspaceEditSelectionState(editor.FilePath, editor, selectionStart, selectionEnd, editor.CaretOffset);
	}
}