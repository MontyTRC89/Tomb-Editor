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
    class PositionBasedObjectContextMenu : BaseContextMenu
    {
        private ToolStripMenuItem _itemProperties;
        private ToolStripMenuItem _itemCopy;
        private ToolStripMenuItem _itemClone;
        private ToolStripMenuItem _itemDelete;

        public PositionBasedObjectContextMenu(PanelRendering3D panel3D)
            : base()
        {
            _itemProperties = new ToolStripMenuItem("Properties", null, (o, e) =>
            {
              EditorActions.EditObject(_editor.SelectedObject, panel3D);
            });

            _itemCopy = new ToolStripMenuItem("Copy", global::TombEditor.Properties.Resources.general_copy_link_16, (o, e) =>
            {
                EditorActions.Copy(panel3D);
            });

            _itemClone = new ToolStripMenuItem("Clone", global::TombEditor.Properties.Resources.actions_rubber_stamp_16, (o, e) =>
            {
                EditorActions.Clone(panel3D);
            });

            _itemDelete = new ToolStripMenuItem("Delete", global::TombEditor.Properties.Resources.toolbox_Eraser_16, (o, e) =>
            {
                EditorActions.DeleteObjectWithWarning(_editor.SelectedObject, panel3D);
            });
        }

        public override void OpenMenu(Control control, Point p)
        {
            Items.Clear();

            Items.Add(_itemProperties);
            Items.Add(new ToolStripSeparator());
            Items.Add(_itemCopy);
            Items.Add(_itemClone);
            Items.Add(_itemDelete);

            // Get all triggers pointing to selected object
            var triggers = _editor.Level.GetAllTriggersPointingToObject(_editor.SelectedObject);
            if (triggers.Count != 0)
            {
                Items.Add(new ToolStripSeparator());

                foreach (var trigger in triggers)
                {
                    var triggerItem = new ToolStripMenuItem("Trigger in room " + trigger.Room.Name,
                        null, 
                        (o, e) =>
                    {
                        _editor.SelectRoomAndResetCamera(trigger.Room);
                    });
                    Items.Add(triggerItem);
                }
            }

            Show(control, p);
        }
    }
}
