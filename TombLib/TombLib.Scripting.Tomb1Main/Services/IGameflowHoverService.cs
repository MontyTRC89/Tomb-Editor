#nullable enable

using ICSharpCode.AvalonEdit.Document;

public interface IGameflowHoverService
{
	string? GetHoverInfo(TextDocument document, int offset);
}
