using System;
using System.Windows.Input;

namespace TombLib.Scripting.UI.Completion;

/// <summary>
/// Provides the shared heuristics used to decide when completion should trigger.
/// </summary>
public static class EditorCompletionTriggerHelper
{
	/// <summary>
	/// Determines whether the input represents a Ctrl+Space completion request.
	/// </summary>
	/// <param name="inputText">The input text to check.</param>
	/// <param name="modifiers">The pressed modifier keys.</param>
	/// <returns><c>true</c> when the input is a space with the Ctrl modifier; otherwise, <c>false</c>.</returns>
	public static bool IsCtrlSpaceInput(string? inputText, ModifierKeys modifiers)
		=> inputText == " " && modifiers.HasFlag(ModifierKeys.Control);

	/// <summary>
	/// Determines whether the line contains exactly one character.
	/// </summary>
	/// <param name="currentLineText">The current line text.</param>
	/// <returns><c>true</c> when the line is a single character; otherwise, <c>false</c>.</returns>
	public static bool IsSingleCharacterLine(string? currentLineText)
		=> currentLineText?.Length == 1;

	/// <summary>
	/// Determines whether the line is a single character that matches the given predicate.
	/// </summary>
	/// <param name="currentLineText">The current line text.</param>
	/// <param name="characterPredicate">The predicate the single character must satisfy.</param>
	/// <returns><c>true</c> when the line is a single matching character; otherwise, <c>false</c>.</returns>
	public static bool IsSingleCharacterLine(string? currentLineText, Func<char, bool> characterPredicate)
		=> currentLineText?.Length == 1 && characterPredicate(currentLineText[0]);
}
