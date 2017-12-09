using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombEditor.Controls.ContextMenus
{
    class SolidGeometryContextMenu : BaseContextMenu
    {
        private ToolStripMenuItem _itemPaste;

        public SolidGeometryContextMenu(PanelRendering3D panel3D)
            : base()
        {
            _itemPaste = new ToolStripMenuItem("Paste", global::TombEditor.Properties.Resources.general_clipboard_16, (o, e) =>
            {
                EditorActions.PasteObject(panel3D.LastSelectedBlock);
            });            
        }

        public override void OpenMenu(Control c, Point p)
        {
            Items.Clear();
            Items.Add(_itemPaste);
            Show(c, p);
        }
    }
}
