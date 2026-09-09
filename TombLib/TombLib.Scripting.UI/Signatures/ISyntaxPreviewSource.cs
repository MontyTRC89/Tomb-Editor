using Nickelony.LanguageServer.Abstractions.Signatures;

namespace TombLib.Scripting.UI.Signatures;

/// <summary>
/// Provides the syntax preview shown in the editor status presentation.
/// </summary>
public interface ISyntaxPreviewSource
{
	/// <summary>
	/// Gets the current syntax preview, or <c>null</c> when none is available.
	/// </summary>
	TextSignatureHelpInfo? GetSyntaxPreview();
}
