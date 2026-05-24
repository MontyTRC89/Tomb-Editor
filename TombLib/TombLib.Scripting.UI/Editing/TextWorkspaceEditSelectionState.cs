#nullable enable

using System;
using System.Runtime.Versioning;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Editing;

/// <summary>
/// Captures the selection and caret state for a text editor before applying workspace edits.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class TextWorkspaceEditSelectionState
{
	private TextWorkspaceEditSelectionState(string filePath, TextEditorBase editor, int selectionStart, int selectionEnd, int caretOffset)
	{
		FilePath = filePath;
		Editor = editor;
		SelectionStart = selectionStart;
		SelectionEnd = selectionEnd;
		CaretOffset = caretOffset;
	}

	/// <summary>
	/// Gets the file path associated with the captured editor.
	/// </summary>
	public string FilePath { get; }

	/// <summary>
	/// Gets the editor whose selection state was captured.
	/// </summary>
	public TextEditorBase Editor { get; }

	/// <summary>
	/// Gets the selection start offset.
	/// </summary>
	public int SelectionStart { get; }

	/// <summary>
	/// Gets the selection end offset.
	/// </summary>
	public int SelectionEnd { get; }

	/// <summary>
	/// Gets the caret offset.
	/// </summary>
	public int CaretOffset { get; }

	/// <summary>
	/// Captures the current selection state from an editor.
	/// </summary>
	/// <param name="editor">The editor whose state should be captured.</param>
	/// <returns>The captured selection state.</returns>
	public static TextWorkspaceEditSelectionState Capture(TextEditorBase editor)
	{
		ArgumentNullException.ThrowIfNull(editor);

		int selectionStart = editor.SelectionStart;
		int selectionEnd = selectionStart + editor.SelectionLength;

		return new TextWorkspaceEditSelectionState(editor.FilePath, editor, selectionStart, selectionEnd, editor.CaretOffset);
	}
}
