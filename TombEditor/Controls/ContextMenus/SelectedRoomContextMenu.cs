using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib;
using TombLib.LevelData;

namespace TombEditor.Controls.ContextMenus
{
    class SelectedRoomContextMenu : BaseContextMenu
    {
        public SelectedRoomContextMenu(Editor editor)
            : base(editor)
        {
            Items.Add(new ToolStripMenuItem("Export rooms", global::TombEditor.Properties.Resources.general_Save_As_16, (o, e) =>
            {
                EditorActions.ExportRooms(this, _editor.SelectedRooms);
            }));
            Items.Add(new ToolStripSeparator());

            Items.Add(new ToolStripMenuItem("Delete rooms", global::TombEditor.Properties.Resources.general_trash_16, (o, e) =>
            {
                EditorActions.DeleteRooms(_editor.SelectedRooms, this);
            }));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
