using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor.Controls.ContextMenus
{
    class PositionBasedObjectContextMenu : BaseContextMenu
    {
        private ToolStripMenuItem _itemProperties;
        private ToolStripMenuItem _itemCopy;
        private ToolStripMenuItem _itemClone;
        private ToolStripMenuItem _itemDelete;
        private ToolStripMenuItem _itemId;

        public PositionBasedObjectContextMenu(PanelRendering3D panel3D)
            : base()
        {
            _itemProperties = new ToolStripMenuItem("Edit object", global::TombEditor.Properties.Resources.general_edit_16, (o, e) =>
            {
              EditorActions.EditObject(_editor.SelectedObject, panel3D);
            });

            _itemCopy = new ToolStripMenuItem("Copy", global::TombEditor.Properties.Resources.general_copy_link_16, (o, e) =>
            {
                EditorActions.TryCopyObject(_editor.SelectedObject, panel3D);
            });

            _itemClone = new ToolStripMenuItem("Clone", global::TombEditor.Properties.Resources.actions_rubber_stamp_16, (o, e) =>
            {
                EditorActions.TryStampObject(_editor.SelectedObject, panel3D);
            });

            _itemDelete = new ToolStripMenuItem("Delete", global::TombEditor.Properties.Resources.toolbox_Eraser_16, (o, e) =>
            {
                EditorActions.DeleteObjectWithWarning(_editor.SelectedObject, panel3D);
            });

            _itemId = new ToolStripMenuItem("");
            _itemId.Enabled = false;
        }

        public override void OpenMenu(Control control, Point p)
        {
            Items.Clear();

            if (!(_editor.SelectedObject is LightInstance))
            {
                if (_editor.SelectedObject is IHasScriptID)
                {
                    _itemId.Text = "ID = " + (_editor.SelectedObject as IHasScriptID).ScriptId;
                    Items.Add(_itemId);
                    Items.Add(new ToolStripSeparator());
                }
                Items.Add(_itemProperties);
                Items.Add(new ToolStripSeparator());
            }

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
