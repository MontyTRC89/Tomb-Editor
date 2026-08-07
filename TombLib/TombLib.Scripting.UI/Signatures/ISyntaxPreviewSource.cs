using Nickelony.LanguageServer.Abstractions.Signatures;

namespace TombLib.Scripting.UI.Signatures;

public interface ISyntaxPreviewSource
{
	TextSignatureHelpInfo? GetSyntaxPreview();
}
