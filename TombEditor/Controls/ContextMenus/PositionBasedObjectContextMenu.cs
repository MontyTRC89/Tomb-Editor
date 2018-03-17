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
                _editor.BookmarkedObject = targetObject;
            }));

            if (_editor.SelectedObject is ImportedGeometryInstance)
            {
                Items.Add(new ToolStripMenuItem("Reload imported geometry", Properties.Resources.general_Open_16, (o, e) =>
                {
                    EditorActions.ReloadImportedGeometry(_editor.SelectedObject as ImportedGeometryInstance);
                }));
            }

            if (_editor.SelectedObject is IRotateableY || _editor.SelectedObject is IRotateableYX || _editor.SelectedObject is IRotateableYXRoll)
            {
                Items.Add(new ToolStripSeparator());
                Items.Add(new ToolStripMenuItem("Reset rotation (all axes)", Properties.Resources.actions_center_direction_16, (o, e) =>
                {
                    EditorActions.ResetObjectRotation();
                }));

                if (_editor.SelectedObject is IRotateableYX)
                {
                    Items.Add(new ToolStripMenuItem("Reset rotation (X axis)", null, (o, e) =>
                    {
                        EditorActions.ResetObjectRotation(EditorActions.RotationAxis.X);
                    }));
                }

                if (_editor.SelectedObject is IRotateableY)
                {
                    Items.Add(new ToolStripMenuItem("Reset rotation (Y axis)", null, (o, e) =>
                    {
                        EditorActions.ResetObjectRotation(EditorActions.RotationAxis.Y);
                    }));
                }

                if (_editor.SelectedObject is IRotateableYXRoll)
                {
                    Items.Add(new ToolStripMenuItem("Reset rotation (Roll axis)", null, (o, e) =>
                    {
                        EditorActions.ResetObjectRotation(EditorActions.RotationAxis.Roll);
                    }));
                }
            }

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
