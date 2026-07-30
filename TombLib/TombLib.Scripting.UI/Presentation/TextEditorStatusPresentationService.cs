#nullable enable

using System;
using TombLib.Scripting.UI.Bases;
using TombLib.Scripting.UI.Editors;
using TombLib.Scripting.UI.Signatures;

namespace TombLib.Scripting.UI.Presentation;

public sealed class TextEditorStatusPresentationService
{
	public TextEditorStatusPresentation Create(
		IEditorControl? editorControl,
		bool showSyntaxPreviewWhenEmpty,
		TextEditorStatusText text)
	{
		ArgumentNullException.ThrowIfNull(text);

		bool showSyntaxPreview = editorControl is ISyntaxPreviewSource || showSyntaxPreviewWhenEmpty;

		if (editorControl is null)
			return TextEditorStatusPresentation.Empty(showSyntaxPreview);

		return new TextEditorStatusPresentation(
			HasEditor: true,
			RowLabelText: BuildRowLabel(editorControl, text),
			ColumnLabelText: string.Format(text.ColumnFormatText, editorControl.CurrentColumn),
			SelectionLabelText: string.Format(text.SelectionFormatText, editorControl.SelectionLength),
			ZoomLabelText: string.Format(text.ZoomFormatText, editorControl.Zoom),
			CanResetZoom: editorControl.Zoom != 100,
			ResetZoomToolTipText: editorControl.Zoom != 100 ? text.ResetZoomToolTipText : string.Empty,
			ShowSyntaxPreview: showSyntaxPreview,
			SyntaxPreview: editorControl is ISyntaxPreviewSource syntaxPreviewSource
				? syntaxPreviewSource.GetSyntaxPreview()
				: null);
	}

	private static string BuildRowLabel(IEditorControl editorControl, TextEditorStatusText text)
		=> editorControl switch
		{
			_ when editorControl.EditorType == EditorType.Strings => string.Format(text.RowFormatText, editorControl.CurrentRow),
			TextEditorBase => string.Format(text.LineFormatText, editorControl.CurrentRow),
			_ => string.Empty
		};
}
