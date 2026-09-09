using ICSharpCode.AvalonEdit.Document;
using Nickelony.LanguageServer.Abstractions.Hover;
using Nickelony.LanguageServer.Abstractions.Navigation;
using System;
using TombLib.Scripting.Hover;
using TombLib.Scripting.Navigation;
using TombLib.Scripting.UI.Bases;

namespace TombLib.Scripting.UI.Navigation;

internal sealed class TextDefinitionNavigationService
{
	public bool TryGoToDefinition(TextEditorBase editor, ITextDefinitionProvider definitionProvider, ITextHoverProvider hoverProvider, int offset)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(definitionProvider);
		ArgumentNullException.ThrowIfNull(hoverProvider);

		if (offset < 0 || offset > editor.Document.TextLength)
			return false;

		TextHoverInfo? hoverInfo = hoverProvider.GetHoverInfo(new TextHoverRequest(editor.Document.Text, offset));

		if (hoverInfo is null || string.IsNullOrWhiteSpace(hoverInfo.SymbolName))
			return false;

		return TryGoToObject(editor, definitionProvider, hoverInfo.SymbolName, hoverInfo.Identifier as TextDefinitionDiscriminator);
	}

	public bool TryGoToObject(TextEditorBase editor, ITextDefinitionProvider definitionProvider, string objectName, TextDefinitionDiscriminator? identifyingObject)
	{
		ArgumentNullException.ThrowIfNull(editor);
		ArgumentNullException.ThrowIfNull(definitionProvider);

		if (string.IsNullOrWhiteSpace(objectName))
			return false;

		var request = new TextDefinitionRequest(editor.Document.Text, objectName, identifyingObject);
		TextDefinitionLocation? location = definitionProvider.GetDefinition(request);

		if (location is null || location.LineNumber < 1 || location.LineNumber > editor.Document.LineCount)
			return false;

		DocumentLine line = editor.Document.GetLineByNumber(location.LineNumber);
		editor.Focus();
		editor.ScrollToLine(location.LineNumber);
		editor.SelectLine(line);
		return true;
	}
}
