using DarkUI.Docking;
using DarkUI.Forms;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor.ToolWindows
{
    public partial class ItemBrowser : DarkToolWindow
    {
        private readonly Editor _editor;

        public ItemBrowser()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        public void InitializeRendering(RenderingDevice device)
        {
            panelItem.InitializeRendering(device, _editor.Configuration.RenderingItem_Antialias);
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
            // Update available items combo box
            if (obj is Editor.LoadedWadsChangedEvent)
            {
                comboItems.Items.Clear();
                foreach (var moveable in _editor.Level.Settings.WadGetAllMoveables().Values)
                    comboItems.Items.Add(moveable);
                foreach (var staticMesh in _editor.Level.Settings.WadGetAllStatics().Values)
                    comboItems.Items.Add(staticMesh);
                if (comboItems.Items.Count > 0 && comboItems.SelectedIndex == -1)
                    comboItems.SelectedIndex = 0;
            }

            // Update selection of items combo box
            if (obj is Editor.ChosenItemChangedEvent)
            {
                var e = (Editor.ChosenItemChangedEvent)obj;
                if (!e.Current.HasValue)
                    comboItems.SelectedItem = panelItem.CurrentObject = null;
                else if (e.Current.Value.IsStatic)
                    comboItems.SelectedItem = panelItem.CurrentObject = _editor.Level.Settings.WadTryGetStatic(e.Current.Value.StaticId);
                else
                {
                    comboItems.SelectedItem = panelItem.CurrentObject = _editor.Level.Settings.WadTryGetMoveable(e.Current.Value.MoveableId);
                    if (e.Current.Value.MoveableId == WadMoveableId.Lara) // Show Lara's skin
                    {
                        WadMoveable moveable = _editor.Level.Settings.WadTryGetMoveable(WadMoveableId.LaraSkin);
                        if (moveable != null)
                            panelItem.CurrentObject = moveable;
                    }
                    panelItem.Invalidate();
                }
            }

            // Update item color control
            if (obj is Editor.SelectedObjectChangedEvent)
            {
                StaticInstance itemInstance = ((Editor.SelectedObjectChangedEvent)obj).Current as StaticInstance;
                panelStaticMeshColor.BackColor = itemInstance == null ? Color.Black : (itemInstance.Color * 0.5f).ToWinFormsColor();
            }
        }

        private ItemType? GetCurrentItemWithMessage()
        {
            ItemType? result = _editor.ChosenItem;
            if (result == null)
                DarkMessageBox.Show(this, "Select an item first", "Error", MessageBoxIcon.Error);
            return result;
        }

        public void FindItem()
        {
            ItemType? currentItem = GetCurrentItemWithMessage();
            if (currentItem == null)
                return;

            // Search for matching objects after the previous one
            ObjectInstance previousFind = _editor.SelectedObject;
            ObjectInstance instance = _editor.Level.Rooms
                .Where(room => room != null)
                .SelectMany(room => room.Objects)
                .FindFirstAfterWithWrapAround(
                obj => previousFind == obj,
                obj => obj is ItemInstance && ((ItemInstance)obj).ItemType == currentItem.Value);

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
            if (comboItems.Items.Count == 0)
                return;

            if (comboItems.SelectedIndex - 1 < 0)
                comboItems.SelectedIndex = comboItems.Items.Count - 1;
            else
                comboItems.SelectedIndex = comboItems.SelectedIndex - 1;
        }

        private void butItemsNext_Click(object sender, EventArgs e)
        {
            if (comboItems.Items.Count == 0)
                return;

            if (comboItems.SelectedIndex + 1 >= comboItems.Items.Count)
                comboItems.SelectedIndex = 0;
            else
                comboItems.SelectedIndex = comboItems.SelectedIndex + 1;
        }

        private void butAddItem_Click(object sender, EventArgs e)
        {
            var currentItem = GetCurrentItemWithMessage();
            if (currentItem == null)
                return;

            if (!currentItem.Value.IsStatic && _editor.SelectedRoom.Flipped && _editor.SelectedRoom.AlternateRoom == null)
            {
                DarkMessageBox.Show(this, "You can't add moveables to a flipped room", "Error", MessageBoxIcon.Information);
                return;
            }

            _editor.Action = new EditorActionPlace(false, (r, l) => ItemInstance.FromItemType(currentItem.Value));
        }

        private void panelStaticMeshColor_Click(object sender, EventArgs e)
        {
            var instance = _editor.SelectedObject as StaticInstance;
            if (instance == null)
                return;

            colorDialog.Color = (instance.Color * 0.5f).ToWinFormsColor();
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelStaticMeshColor.BackColor = colorDialog.Color;
            instance.Color = colorDialog.Color.ToFloatColor() * 2.0f;
            _editor.ObjectChange(instance, ObjectChangeType.Change);
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
            if (comboItems.SelectedItem == null)
                _editor.ChosenItem = null;
            if (comboItems.SelectedItem is WadMoveable)
                _editor.ChosenItem = new ItemType(((WadMoveable)comboItems.SelectedItem).Id, _editor?.Level?.Settings);
            else if (comboItems.SelectedItem is WadStatic)
                _editor.ChosenItem = new ItemType(((WadStatic)comboItems.SelectedItem).Id, _editor?.Level?.Settings);
        }

        private void comboItems_Format(object sender, ListControlConvertEventArgs e)
        {
            WadGameVersion? gameVersion = _editor?.Level?.Settings?.WadGameVersion;
            IWadObject listItem = e.ListItem as IWadObject;
            if (gameVersion != null && listItem != null)
                e.Value = listItem.ToString(gameVersion.Value);
        }
    }
}
