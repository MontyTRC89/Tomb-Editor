#nullable enable

namespace TombLib.Scripting.UI.Presentation;

public sealed record class TextEditorStatusText(
	string RowFormatText,
	string LineFormatText,
	string ColumnFormatText,
	string SelectionFormatText,
	string ZoomFormatText,
	string ResetZoomToolTipText);
