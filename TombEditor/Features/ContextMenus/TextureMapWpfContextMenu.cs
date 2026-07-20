#nullable enable
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Forms;
using TombLib.Utils;
using static TombEditor.Features.ContextMenus.WpfContextMenuHelper;

namespace TombEditor.Features.ContextMenus;

internal static class TextureMapWpfContextMenu
{
    public static ContextMenu Show(Editor editor, IWin32Window owner,
                                   Vector2 position, System.Drawing.Point screenPoint)
    {
        var menu = new ContextMenu();

        if (editor.SelectedTexture != TextureArea.None)
            menu.Items.Add(Item("Set as default texture", null,
                () =>
                {
                    editor.Level.Settings.DefaultTexture = editor.SelectedTexture;
                    (owner as System.Windows.Forms.Control)?.Invalidate();
                }));

        if (editor.Level.Settings.DefaultTexture != TextureArea.None)
            menu.Items.Add(Item("Clear default texture", null,
                () =>
                {
                    editor.Level.Settings.DefaultTexture = TextureArea.None;
                    (owner as System.Windows.Forms.Control)?.Invalidate();
                }));

        // No items at all means right-click is a no-op. Match the old
        // DarkContextMenu behaviour (which did nothing in that case).
        if (menu.Items.Count == 0)
            return menu;

        Open(menu, screenPoint);
        return menu;
    }
}
