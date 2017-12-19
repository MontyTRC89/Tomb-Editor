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
        public PositionBasedObjectContextMenu(Editor editor, PositionBasedObjectInstance targetObject)
            : base(editor)
        {
            if (targetObject is IHasScriptID)
            {
                Items.Add(new ToolStripMenuItem("ScriptID = " + ((targetObject as IHasScriptID).ScriptId?.ToString() ?? "<None>")) { Enabled = false });
                Items.Add(new ToolStripSeparator());
            }

            Items.Add(new ToolStripMenuItem("Edit object", global::TombEditor.Properties.Resources.general_edit_16, (o, e) =>
            {
                EditorActions.EditObject(targetObject, this);
            }) { Enabled = !(targetObject is LightInstance) });

            Items.Add(new ToolStripMenuItem("Copy", global::TombEditor.Properties.Resources.general_copy_link_16, (o, e) =>
            {
                EditorActions.TryCopyObject(targetObject, this);
            }));

            Items.Add(new ToolStripMenuItem("Clone", global::TombEditor.Properties.Resources.actions_rubber_stamp_16, (o, e) =>
            {
                EditorActions.TryStampObject(targetObject, this);
            }));

            Items.Add(new ToolStripMenuItem("Delete", global::TombEditor.Properties.Resources.toolbox_Eraser_16, (o, e) =>
            {
                EditorActions.DeleteObjectWithWarning(targetObject, this);
            }));

            // Get all triggers pointing to selected object
            var triggers = _editor.Level.GetAllTriggersPointingToObject(targetObject);
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
        }
    }
}
