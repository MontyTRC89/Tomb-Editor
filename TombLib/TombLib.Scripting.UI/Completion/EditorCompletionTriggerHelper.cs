using System;
using System.Windows.Input;

namespace TombLib.Scripting.UI.Completion;

public static class EditorCompletionTriggerHelper
{
	public static bool IsCtrlSpaceInput(string? inputText, ModifierKeys modifiers)
		=> inputText == " " && modifiers.HasFlag(ModifierKeys.Control);

	public static bool IsSingleCharacterLine(string? currentLineText)
		=> currentLineText?.Length == 1;

	public static bool IsSingleCharacterLine(string? currentLineText, Func<char, bool> characterPredicate)
		=> currentLineText?.Length == 1 && characterPredicate(currentLineText[0]);
}
