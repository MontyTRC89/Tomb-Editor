namespace TombLib.Scripting.Navigation;

/// <summary>
/// Identifies the language-specific kind of a definition lookup carried across provider boundaries.
/// Language packages derive from this type to carry a strongly typed discriminator. Providers that
/// do not recognize a discriminator must return no result.
/// </summary>
public abstract record TextDefinitionDiscriminator;
