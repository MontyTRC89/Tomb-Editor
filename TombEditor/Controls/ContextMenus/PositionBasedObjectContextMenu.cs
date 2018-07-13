using System.Windows.Forms;
using TombLib.LevelData;

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

            Items.Add(new ToolStripMenuItem("Edit object", Properties.Resources.general_edit_16, (o, e) =>
            {
                EditorActions.EditObject(targetObject, this);
            }) { Enabled = !(targetObject is LightInstance) });

            Items.Add(new ToolStripMenuItem("Copy", Properties.Resources.general_copy_link_16, (o, e) =>
            {
                EditorActions.TryCopyObject(targetObject, this);
            }));

            Items.Add(new ToolStripMenuItem("Clone", Properties.Resources.actions_rubber_stamp_16, (o, e) =>
            {
                EditorActions.TryStampObject(targetObject, this);
            }));

            Items.Add(new ToolStripMenuItem("Delete", Properties.Resources.toolbox_Eraser_16, (o, e) =>
            {
                EditorActions.DeleteObjectWithWarning(targetObject, this);
            }));

            Items.Add(new ToolStripMenuItem("Bookmark object", null, (o, e) =>
            {
                EditorActions.BookmarkObject(targetObject);
            }));

            if (targetObject is ImportedGeometryInstance)
            {
                Items.Add(new ToolStripMenuItem("Reload imported geometry", Properties.Resources.general_Open_16, (o, e) =>
                {
                    _editor.Level.Settings.ImportedGeometryUpdate(
                        ((ImportedGeometryInstance)targetObject).Model,
                        ((ImportedGeometryInstance)targetObject).Model.Info);
                }));
            }

            if (targetObject is IRotateableY || targetObject is IRotateableYX || targetObject is IRotateableYXRoll)
            {
                Items.Add(new ToolStripSeparator());
                Items.Add(new ToolStripMenuItem("Reset rotation (all axes)", Properties.Resources.actions_center_direction_16, (o, e) =>
                {
                    EditorActions.ResetObjectRotation();
                }));

                if (targetObject is IRotateableYX)
                {
                    Items.Add(new ToolStripMenuItem("Reset rotation (X axis)", null, (o, e) =>
                    {
                        EditorActions.ResetObjectRotation(EditorActions.RotationAxis.X);
                    }));
                }

                if (targetObject is IRotateableY)
                {
                    Items.Add(new ToolStripMenuItem("Reset rotation (Y axis)", null, (o, e) =>
                    {
                        EditorActions.ResetObjectRotation(EditorActions.RotationAxis.Y);
                    }));
                }

                if (targetObject is IRotateableYXRoll)
                {
                    Items.Add(new ToolStripMenuItem("Reset rotation (Roll axis)", null, (o, e) =>
                    {
                        EditorActions.ResetObjectRotation(EditorActions.RotationAxis.Roll);
                    }));
                }
            }

            // Get all triggers pointing to target object
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
