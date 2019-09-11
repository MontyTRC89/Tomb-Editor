using DarkUI.Controls;
using DarkUI.Docking;
using System;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor.ToolWindows
{
    public partial class ObjectList : DarkToolWindow
    {
        private readonly Editor _editor;

        public ObjectList()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update the trigger control
            if (obj is Editor.SelectedRoomChangedEvent || obj is Editor.ObjectChangedEvent)
            {
                lstObjects.Items.Clear();

                foreach (var o in _editor.SelectedRoom.Objects)
                    lstObjects.Items.Add(new DarkListItem(o.ToShortString()) { Tag = o });
            }

            // Update the object control selection
            if (obj is Editor.SelectedRoomChangedEvent || obj is Editor.SelectedObjectChangedEvent)
            {
                if (_editor.SelectedObject?.Room == _editor.SelectedRoom && _editor.SelectedObject is PositionBasedObjectInstance)
                {
                    var o = _editor.SelectedObject as PositionBasedObjectInstance;
                    var entry = lstObjects.Items.FirstOrDefault(t => t.Tag == o);
                    if (entry != null) lstObjects.SelectItem(lstObjects.Items.IndexOf(entry));
                }
                
            }
        }

        private void lstObjects_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null || lstObjects.SelectedItem == null)
                return;

            _editor.SelectedObject = (ObjectInstance)lstObjects.SelectedItem.Tag;
        }

        private void lstObjects_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstObjects.SelectedIndices.Count == 0)
                return;

            var instance = lstObjects.SelectedItem.Tag as ObjectInstance;
            if (instance != null)
                EditorActions.EditObject(instance, this);
        }

        private void lstObjects_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                butDeleteObject_Click(sender, e);
        }

        private void butDeleteObject_Click(object sender, EventArgs e)
        {
            if (lstObjects.SelectedIndices.Count == 0)
                return;

            var instance = lstObjects.SelectedItem.Tag as ObjectInstance;
            if (instance != null)
                EditorActions.DeleteObject(instance);
        }

        private void butEditObject_Click(object sender, EventArgs e)
        {
            var instance = lstObjects.SelectedItem.Tag as ObjectInstance;
            if (instance != null)
                EditorActions.EditObject(instance, this);
        }
    }
}
