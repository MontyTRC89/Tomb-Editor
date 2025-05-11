using ScriptLib.Core.Structs;

namespace ScriptLib.Core.Interfaces;

/// <summary>
/// Interface for components that support error checking functionality.
/// </summary>
public interface ISupportsErrorChecking
{
	/// <summary>
	/// Gets or sets the list of document errors found during validation.
	/// </summary>
	/// <value>
	/// A collection of <see cref="DocumentError"/> objects representing validation errors.
	/// </value>
	IReadOnlyList<DocumentError> Errors { get; set; }
}
