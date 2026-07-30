#nullable enable

using TombLib.Scripting.Signatures;

namespace TombLib.Scripting.UI.Signatures;

public interface ISyntaxPreviewSource
{
	TextSignatureHelpInfo? GetSyntaxPreview();
}
