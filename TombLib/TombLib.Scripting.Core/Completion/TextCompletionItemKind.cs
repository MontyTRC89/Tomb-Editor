namespace TombLib.Scripting.Completion;

/// <summary>
/// Identifies the semantic category of a completion item for icon and styling purposes.
/// </summary>
public enum TextCompletionItemKind
{
	/// <summary>
	/// A generic completion item with no specialized category.
	/// </summary>
	Generic,

	/// <summary>
	/// A property-like member.
	/// </summary>
	Property,

	/// <summary>
	/// An array-like value or container.
	/// </summary>
	Array,

	/// <summary>
	/// A section header or named block.
	/// </summary>
	Section,

	/// <summary>
	/// A directive or pragma-style keyword.
	/// </summary>
	Directive,

	/// <summary>
	/// A constant value.
	/// </summary>
	Constant,

	/// <summary>
	/// A language keyword.
	/// </summary>
	Keyword,

	/// <summary>
	/// A legacy command form (e.g. original TR4 commands).
	/// </summary>
	OldCommand,

	/// <summary>
	/// A new command form (e.g. TRNG commands).
	/// </summary>
	NewCommand,

	/// <summary>
	/// A callable method or function.
	/// </summary>
	Method,

	/// <summary>
	/// A variable.
	/// </summary>
	Variable,

	/// <summary>
	/// A field-like member.
	/// </summary>
	Field,

	/// <summary>
	/// A type or class.
	/// </summary>
	Class,

	/// <summary>
	/// A parameter.
	/// </summary>
	Parameter,

	/// <summary>
	/// A namespace or module scope.
	/// </summary>
	Namespace,

	/// <summary>
	/// A file system file.
	/// </summary>
	File,

	/// <summary>
	/// A file system folder.
	/// </summary>
	Folder
}
