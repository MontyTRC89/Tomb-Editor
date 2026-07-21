// These enums historically lived in TombLib.Forms next to the WinForms PopUpInfo form. They are
// kept in that namespace (so the many existing call sites keep compiling) but defined in the core
// TombLib assembly, so the WPF popup in TombLib.WPF can use them without referencing WinForms.
namespace TombLib.Forms
{
    public enum PopupAlignment
    {
        Center,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum PopupType
    {
        None,
        Info,
        Warning,
        Error
    }
}
