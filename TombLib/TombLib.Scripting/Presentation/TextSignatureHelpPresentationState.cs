using Nickelony.LanguageServer.Abstractions.Signatures;

namespace TombLib.Scripting.Presentation;

/// <summary>
/// Describes the current state of the signature help presentation.
/// </summary>
/// <param name="SignatureHelp">The signature help information, when available.</param>
/// <param name="IsVisible">Whether the signature help popup is visible.</param>
/// <param name="IsRequestInFlight">Whether a signature help request is currently in flight.</param>
/// <param name="IsRefreshPending">Whether a signature help refresh is pending.</param>
public readonly record struct TextSignatureHelpPresentationState(
	TextSignatureHelpInfo? SignatureHelp,
	bool IsVisible,
	bool IsRequestInFlight,
	bool IsRefreshPending)
{
	/// <summary>
	/// Gets the empty signature help presentation state.
	/// </summary>
	public static TextSignatureHelpPresentationState Empty { get; } = new(null, false, false, false);

	/// <summary>
	/// Gets a value indicating whether signature help is visible or any request or refresh is pending.
	/// </summary>
	public bool IsActiveOrPending => IsVisible || IsRequestInFlight || IsRefreshPending;
}
