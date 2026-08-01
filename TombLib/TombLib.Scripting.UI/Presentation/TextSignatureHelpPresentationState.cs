#nullable enable

using Nickelony.LanguageServer.Abstractions.Signatures;

namespace TombLib.Scripting.UI.Presentation;

public readonly record struct TextSignatureHelpPresentationState(
	TextSignatureHelpInfo? SignatureHelp,
	bool IsVisible,
	bool IsRequestInFlight,
	bool IsRefreshPending)
{
	public static TextSignatureHelpPresentationState Empty { get; } = new(null, false, false, false);

	public bool IsActiveOrPending => IsVisible || IsRequestInFlight || IsRefreshPending;
}
