#nullable enable

namespace TombLib.Scripting.UI.Completion;

/// <summary>
/// Centralizes shared popup precedence rules across completion, hover, and signature help.
/// </summary>
public static class TextPopupInteractionRules
{
	/// <summary>
	/// Determines whether hover content may be shown while other transient popups are active.
	/// </summary>
	public static bool CanShowHover(bool isCompletionWindowOpen, bool isSignatureHelpOpen)
		=> !isCompletionWindowOpen && !isSignatureHelpOpen;
}