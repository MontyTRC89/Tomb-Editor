using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombEditor.Controls.ContextMenus
{
    class SolidGeometryContextMenu : DarkContextMenu
    {
        private ToolStripMenuItem _itemPaste;

        private Editor _editor;

        public SolidGeometryContextMenu(PanelRendering3D panel3D)
        {
            _editor = Editor.Instance;

            _itemPaste = new ToolStripMenuItem("Paste", global::TombEditor.Properties.Resources.general_clipboard_16, (o, e) =>
            {
                EditorActions.PasteObject(panel3D.LastSelectedBlock);
            });
            Items.Add(_itemPaste);
        }
    }
}
