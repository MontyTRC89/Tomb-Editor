using DarkUI.Docking;
using System;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor.ToolWindows
{
    public partial class ObjectList : DarkToolWindow
    {
        private Editor _editor;

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
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update the trigger control
            if ((obj is Editor.SelectedRoomChangedEvent) || (obj is Editor.ObjectChangedEvent))
            {
                lstObjects.BeginUpdate();
                lstObjects.Items.Clear();
                lstObjects.Items.AddRange(_editor.SelectedRoom.AnyObjects.ToArray());
                lstObjects.EndUpdate();
            }

            // Update the object control selection
            if ((obj is Editor.SelectedRoomChangedEvent) || (obj is Editor.SelectedObjectChangedEvent))
            {
                lstObjects.SelectedItem = (_editor.SelectedObject?.Room == _editor.SelectedRoom) ? _editor.SelectedObject : null;
            }
        }

        private void lstObjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((_editor.SelectedRoom == null) || (lstObjects.SelectedItem == null))
                return;
            _editor.SelectedObject = (ObjectInstance)(lstObjects.SelectedItem);
        }

        private void lstObjects_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = lstObjects.IndexFromPoint(e.Location);
            if (index != ListBox.NoMatches)
            {
                var instance = lstObjects.Items[index] as ObjectInstance;
                if (instance != null)
                    EditorActions.EditObject(instance, this);
            }
        }

        private void butDeleteObject_Click(object sender, EventArgs e)
        {
            var instance = lstObjects.SelectedItem as ObjectInstance;
            if (instance != null)
                EditorActions.DeleteObject(instance);
        }

        private void butEditObject_Click(object sender, EventArgs e)
        {
            var instance = lstObjects.SelectedItem as ObjectInstance;
            if (instance != null)
                EditorActions.EditObject(instance, this);
        }
    }
}
