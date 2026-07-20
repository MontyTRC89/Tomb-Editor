#nullable enable
using System.Windows.Controls;
using System.Windows.Forms;
using static TombEditor.Features.ContextMenus.WpfContextMenuHelper;

namespace TombEditor.Features.ContextMenus;

internal static class GridPaintWpfContextMenu
{
    public static ContextMenu Show(Editor editor, IWin32Window owner,
                                   System.Drawing.Point screenPoint)
    {
        var menu = new ContextMenu();

        menu.Items.Add(Item("Grid Paint (2x2)", "Toolbox/GridPaint2x2",
            () => SwitchGridPaintTool(editor, PaintGridSize.Grid2x2)));
        menu.Items.Add(Item("Grid Paint (3x3)", "Toolbox/GridPaint3x3",
            () => SwitchGridPaintTool(editor, PaintGridSize.Grid3x3)));
        menu.Items.Add(Item("Grid Paint (4x4)", "Toolbox/GridPaint4x4",
            () => SwitchGridPaintTool(editor, PaintGridSize.Grid4x4)));

        Open(menu, screenPoint);
        return menu;
    }

    private static void SwitchGridPaintTool(Editor editor, PaintGridSize size)
    {
        editor.Tool = new EditorTool
        {
            Tool            = EditorToolType.GridPaint,
            TextureUVFixer  = editor.Tool.TextureUVFixer,
            GridSize        = size,
        };
    }
}
