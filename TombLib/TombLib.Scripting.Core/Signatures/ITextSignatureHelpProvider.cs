namespace TombLib.Scripting.Signatures;

/// <summary>
/// Resolves signature-help content from the current document context.
/// </summary>
public interface ITextSignatureHelpProvider
{
	/// <summary>
	/// Attempts to resolve signature-help information for the supplied request.
	/// </summary>
	/// <param name="request">The current document and caret-position request.</param>
	/// <returns>The resolved signature-help information, or <see langword="null"/> when none is available.</returns>
	TextSignatureHelpInfo? GetSignatureHelp(TextSignatureHelpRequest request);
}
