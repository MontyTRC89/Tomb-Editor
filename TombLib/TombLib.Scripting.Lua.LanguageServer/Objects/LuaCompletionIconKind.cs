namespace TombLib.Scripting.Lua.Objects;

/// <summary>
/// Defines the icon categories used by Lua completion items.
/// </summary>
public enum LuaCompletionIconKind
{
	/// <summary>
	/// A generic icon for uncategorized completion items.
	/// </summary>
	Misc,

	/// <summary>
	/// An icon for variables.
	/// </summary>
	Variable,

	/// <summary>
	/// An icon for fields.
	/// </summary>
	Field,

	/// <summary>
	/// An icon for methods or functions.
	/// </summary>
	Method,

	/// <summary>
	/// An icon for properties.
	/// </summary>
	Property,

	/// <summary>
	/// An icon for classes or types.
	/// </summary>
	Class,

	/// <summary>
	/// An icon for keywords.
	/// </summary>
	Keyword,

	/// <summary>
	/// An icon for constants.
	/// </summary>
	Constant,

	/// <summary>
	/// An icon for parameters.
	/// </summary>
	Parameter,

	/// <summary>
	/// An icon for namespaces.
	/// </summary>
	Namespace,

	/// <summary>
	/// An icon for files.
	/// </summary>
	File,

	/// <summary>
	/// An icon for folders.
	/// </summary>
	Folder
}