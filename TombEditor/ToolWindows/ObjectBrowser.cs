using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DarkUI.Docking;
using TombEditor.Geometry;
using TombLib.Wad;
using DarkUI.Forms;

namespace TombEditor.ToolWindows
{
    public partial class ObjectBrowser : DarkToolWindow
    {
        private Editor _editor;

        public ObjectBrowser()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        public void Initialize(DeviceManager _deviceManager)
        {
            panelItem.InitializePanel(_deviceManager);
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
            // Update available items combo box
            if (obj is Editor.LoadedWadsChangedEvent)
            {
                comboItems.Items.Clear();

                if (_editor.Level?.Wad != null)
                {
                    foreach (var movable in _editor.Level.Wad.Moveables.Values)
                        comboItems.Items.Add(movable);
                    foreach (var staticMesh in _editor.Level.Wad.Statics.Values)
                        comboItems.Items.Add(staticMesh);
                    comboItems.SelectedIndex = 0;
                }
            }

            // Update selection of items combo box
            if (obj is Editor.ChosenItemChangedEvent)
            {
                var e = (Editor.ChosenItemChangedEvent)obj;
                if (!e.Current.HasValue)
                    comboItems.SelectedIndex = -1;
                else if (e.Current.Value.IsStatic)
                    comboItems.SelectedItem = _editor.Level.Wad.Statics[e.Current.Value.Id];
                else
                    comboItems.SelectedItem = _editor.Level.Wad.Moveables[e.Current.Value.Id];
            }

            // Update item color control
            if (obj is Editor.SelectedObjectChangedEvent)
            {
                ItemInstance itemInstance = ((Editor.SelectedObjectChangedEvent)obj).Current as ItemInstance;
                panelStaticMeshColor.BackColor = itemInstance == null ? System.Drawing.Color.Black : itemInstance.Color.ToWinFormsColor();
            }
        }
        public void FindItem()
        {
            ItemType? currentItem = EditorActions.GetCurrentItemWithMessage(ParentForm);
            if (currentItem == null)
                return;

            // Search for matching objects after the previous one
            ObjectInstance previousFind = _editor.SelectedObject;
            ObjectInstance instance = _editor.Level.Rooms
                .Where(room => room != null)
                .SelectMany(room => room.Objects)
                .FindFirstAfterWithWrapAround(
                (obj) => previousFind == obj,
                (obj) => (obj is ItemInstance) && ((ItemInstance)obj).ItemType == currentItem.Value);

            // Show result
            if (instance == null)
                DarkMessageBox.Show(this, "No object of the selected item type found.", "No object found", MessageBoxIcon.Information);
            else
                _editor.ShowObject(instance);
        }

        public void ResetSearch()
        {
            _editor.SelectedObject = null;
        }


        private void butItemsBack_Click(object sender, EventArgs e)
        {
            if ((comboItems.SelectedIndex - 1) < 0)
                comboItems.SelectedIndex = comboItems.Items.Count - 1;
            else
                comboItems.SelectedIndex = comboItems.SelectedIndex - 1;
        }

        private void butItemsNext_Click(object sender, EventArgs e)
        {
            if ((comboItems.SelectedIndex + 1) >= comboItems.Items.Count)
                comboItems.SelectedIndex = 0;
            else
                comboItems.SelectedIndex = comboItems.SelectedIndex + 1;
        }

        private void butAddItem_Click(object sender, EventArgs e)
        {
            EditorActions.PlaceItem(ParentForm);
        }

        private void panelStaticMeshColor_Click(object sender, EventArgs e)
        {
            var instance = _editor.SelectedObject as ItemInstance;
            if (instance == null)
                return;

            colorDialog.Color = instance.Color.ToWinFormsColor();
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelStaticMeshColor.BackColor = colorDialog.Color;
            instance.Color = colorDialog.Color.ToFloatColor();
            _editor.ObjectChange(instance);
        }

        private void butFindItem_Click(object sender, EventArgs e)
        {
            FindItem();
        }

        private void butResetSearch_Click(object sender, EventArgs e)
        {
            ResetSearch();
        }

        private void comboItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((comboItems.SelectedItem == null) || (_editor?.Level?.Wad == null))
                _editor.ChosenItem = null;
            if (comboItems.SelectedItem is WadMoveable)
                _editor.ChosenItem = new ItemType(false, ((WadMoveable)(comboItems.SelectedItem)).ObjectID);
            else if (comboItems.SelectedItem is WadStatic)
                _editor.ChosenItem = new ItemType(true, ((WadStatic)(comboItems.SelectedItem)).ObjectID);
        }
    }
}
