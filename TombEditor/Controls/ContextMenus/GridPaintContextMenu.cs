using System.Windows.Forms;

namespace TombEditor.Controls.ContextMenus
{
    class GridPaintContextMenu : BaseContextMenu
    {
        public GridPaintContextMenu(Editor editor, IWin32Window owner)
            : base(editor, owner)
        {
            Items.Add(new ToolStripMenuItem("Grid Paint (2x2)", Properties.Resources.toolbox_GridPaint2x2_16, (o, e) =>
            {
                SwitchGridPaintTool(PaintGridSize.Grid2x2);
            }));

            Items.Add(new ToolStripMenuItem("Grid Paint (3x3)", Properties.Resources.toolbox_GridPaint3x3_16, (o, e) =>
            {
                SwitchGridPaintTool(PaintGridSize.Grid3x3);
            }));

            Items.Add(new ToolStripMenuItem("Grid Paint (4x4)", Properties.Resources.toolbox_GridPaint4x4_16, (o, e) =>
            {
                SwitchGridPaintTool(PaintGridSize.Grid4x4);
            }));
        }

        private void SwitchGridPaintTool(PaintGridSize size)
        {
            EditorTool currentTool = new EditorTool() { Tool = EditorToolType.GridPaint, TextureUVFixer = _editor.Tool.TextureUVFixer, GridSize = size };
            _editor.Tool = currentTool;
        }
    }
}
