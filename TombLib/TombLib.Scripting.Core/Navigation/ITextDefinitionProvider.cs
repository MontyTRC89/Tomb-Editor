namespace TombLib.Scripting.Navigation;

/// <summary>
/// Resolves a navigable definition location for language-specific editor objects.
/// </summary>
public interface ITextDefinitionProvider
{
	/// <summary>
	/// Attempts to resolve a definition location for the supplied request.
	/// </summary>
	/// <param name="request">The current document and symbol lookup request.</param>
	/// <returns>The resolved definition location, or <see langword="null"/> when no location can be found.</returns>
	TextDefinitionLocation? GetDefinition(TextDefinitionRequest request);
}
