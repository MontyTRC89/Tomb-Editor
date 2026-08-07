namespace TombLib.Scripting.Completion;

/// <summary>
/// Identifies what kind of editor action triggered a completion request.
/// </summary>
public enum TextCompletionTrigger
{
	/// <summary>
	/// Completion was raised automatically from general typing heuristics.
	/// </summary>
	Automatic,

	/// <summary>
	/// Completion was raised for an empty line or line-start scenario.
	/// </summary>
	EmptyLine,

	/// <summary>
	/// Completion was raised from a context-sensitive trigger such as a <c>`= `</c> after a <c>Customize=</c> command,
	/// or a <c>`, `</c> after an argument.
	/// </summary>
	Contextual,

	/// <summary>
	/// Completion was raised while extending or replacing an identifier.
	/// </summary>
	Word
}
