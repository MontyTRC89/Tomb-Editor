#nullable enable

using TombLib.Scripting.Signatures;

namespace TombLib.Scripting.UI.Presentation;

public readonly record struct TextEditorStatusPresentation(
    bool HasEditor,
    string RowLabelText,
    string ColumnLabelText,
    string SelectionLabelText,
    string ZoomLabelText,
    bool CanResetZoom,
    string ResetZoomToolTipText,
    bool ShowSyntaxPreview,
    TextSignatureHelpInfo? SyntaxPreview)
{
    public static TextEditorStatusPresentation Empty(bool showSyntaxPreview)
        => new(
            HasEditor: false,
            RowLabelText: string.Empty,
            ColumnLabelText: string.Empty,
            SelectionLabelText: string.Empty,
            ZoomLabelText: string.Empty,
            CanResetZoom: false,
            ResetZoomToolTipText: string.Empty,
            ShowSyntaxPreview: showSyntaxPreview,
            SyntaxPreview: null);
}