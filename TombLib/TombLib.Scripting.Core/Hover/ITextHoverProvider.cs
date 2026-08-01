using Nickelony.LanguageServer.Abstractions.Hover;

namespace TombLib.Scripting.Hover;

/// <summary>
/// Resolves hover content from the current document context.
/// </summary>
public interface ITextHoverProvider
{
	/// <summary>
	/// Attempts to resolve hover information for the supplied request.
	/// </summary>
	/// <param name="request">The current document and hover-position request.</param>
	/// <returns>The resolved hover information, or <see langword="null"/> when none is available.</returns>
	TextHoverInfo? GetHoverInfo(TextHoverRequest request);
}
