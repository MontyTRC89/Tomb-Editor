using ICSharpCode.AvalonEdit.Document;
using ScriptLib.Core.Structs;

namespace ScriptLib.Core.Services;

/// <summary>
/// Interface for services that provide syntax error checking functionality.
/// </summary>
public interface IErrorCheckingService
{
	/// <summary>
	/// Checks the syntax of the provided text and returns any detected errors.
	/// </summary>
	/// <param name="textSource">The text source to check for syntax errors.</param>
	/// <returns>A read-only list of document errors found in the text source.</returns>
	IReadOnlyList<DocumentError> CheckSyntax(ITextSource textSource);
}
