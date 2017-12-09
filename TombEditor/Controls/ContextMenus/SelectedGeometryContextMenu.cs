using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TombEditor.Controls.ContextMenus
{
    class SelectedGeometryContextMenu : BaseContextMenu
    {
        private ToolStripItem _itemAddTrigger;

        public SelectedGeometryContextMenu(PanelRendering3D panel3D)
            : base()
        {
            _itemAddTrigger = new ToolStripMenuItem("Add trigger", global::TombEditor.Properties.Resources.general_plus_math_16, (o, e) =>
            {
                EditorActions.AddTrigger(_editor.SelectedRoom, _editor.SelectedSectors.Area, panel3D);
            });
        }

        public override void OpenMenu(Control c, Point p)
        {
            Items.Clear();
            Items.Add(_itemAddTrigger);
            Show(c, p);
        }
    }
}
