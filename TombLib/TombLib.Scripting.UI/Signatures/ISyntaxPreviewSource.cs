#nullable enable

using Nickelony.LanguageServer.Core.Signatures;

namespace TombLib.Scripting.UI.Signatures;

public interface ISyntaxPreviewSource
{
	TextSignatureHelpInfo? GetSyntaxPreview();
}
