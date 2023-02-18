﻿using DarkUI.Controls;
using DarkUI.Docking;
using System;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor.WPF.ToolWindows
{
    public partial class ObjectList : UserControl
    {
        private readonly Editor _editor;
        private bool _lockList = false;

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
            if (obj is Editor.SelectedRoomChangedEvent || 
                obj is Editor.ObjectChangedEvent ||
                obj is Editor.GameVersionChangedEvent)
            {
                // Disable events
                _lockList = true;

                 // Preserve selection
                 var currentObject = lstObjects.SelectedItems.Count > 0 ? lstObjects.SelectedItem.Tag : null;
                lstObjects.Items.Clear();

                foreach (var o in _editor.SelectedRoom.Objects)
                    lstObjects.Items.Add(new DarkListItem(o.ToShortString()) { Tag = o });

                foreach (var o in _editor.SelectedRoom.GhostBlocks)
                    lstObjects.Items.Add(new DarkListItem(o.ToShortString()) { Tag = o });

                // Restore selection
                for (int i = 0; i < lstObjects.Items.Count; i++)
                    if (lstObjects.Items[i].Tag == currentObject)
                    {
                        lstObjects.SelectItem(i);
                        break;
                    }

                // Reenable events
                _lockList = false;
            }

            // Update the object control selection
            if (obj is Editor.SelectedRoomChangedEvent || obj is Editor.SelectedObjectChangedEvent)
            {
                // Disable events
                _lockList = true;

                lstObjects.ClearSelection();

                if (_editor.SelectedObject?.Room == _editor.SelectedRoom)
                {
                    if (_editor.SelectedObject is PositionBasedObjectInstance)
                    {
                        var o = _editor.SelectedObject as PositionBasedObjectInstance;
                        var entry = lstObjects.Items.FirstOrDefault(t => t.Tag == o);
                        if (entry != null)
                        {
                            lstObjects.SelectItem(lstObjects.Items.IndexOf(entry));
                            lstObjects.EnsureVisible();
                        }
                    }
                    else if (_editor.SelectedObject is GhostBlockInstance)
                    {
                        var o = _editor.SelectedObject as GhostBlockInstance;
                        var entry = lstObjects.Items.FirstOrDefault(t => t.Tag == o);
                        if (entry != null)
                        {
                            lstObjects.SelectItem(lstObjects.Items.IndexOf(entry));
                            lstObjects.EnsureVisible();
                        }
                    }
                }

                // Reenable events
                _lockList = false;
            }
        }

        private void lstObjects_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (_lockList || lstObjects.SelectedItem == null ||
                _editor.SelectedRoom == null || _editor.SelectedObject is ObjectGroup)
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

        private void butDeleteObject_Click(object sender, EventArgs e)
        {
            if (lstObjects.SelectedIndices.Count == 0)
                return;

            var instances = lstObjects.SelectedItems.Select(o => o.Tag as ObjectInstance).ToList();
            EditorActions.DeleteObjects(instances, this);
        }

        private void butEditObject_Click(object sender, EventArgs e)
        {
            if (lstObjects.SelectedItem == null)
                return;

            var instance = lstObjects.SelectedItem.Tag as ObjectInstance;
            if (instance != null)
                EditorActions.EditObject(instance, this);
        }
    }
}
