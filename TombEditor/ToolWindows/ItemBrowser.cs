using DarkUI.Docking;
using System;
using System.Drawing;
using System.Windows.Forms;
using TombLib.Controls;
using TombLib.Forms;
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
            CommandHandler.AssignCommandsToControls(Editor.Instance, this, toolTip);

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

            // Update tooltip texts
            if(obj is Editor.ConfigurationChangedEvent)
            {
                if(((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
                    CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);
            }
        }

        private void butSearch_Click(object sender, EventArgs e)
        {
            var searchPopUp = new PopUpSearch(comboItems);
            searchPopUp.Show(this);
        }

        private void panelStaticMeshColor_Click(object sender, EventArgs e)
        {
            var instance = _editor.SelectedObject as StaticInstance;
            if (instance == null)
                return;

            using (var colorDialog = new RealtimeColorDialog(c =>
            {
                panelStaticMeshColor.BackColor = c;
                instance.Color = c.ToFloatColor() * 2.0f;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }))
            {
                colorDialog.Color = (instance.Color * 0.5f).ToWinFormsColor();
                var oldLightColor = colorDialog.Color;

                if (colorDialog.ShowDialog(this) != DialogResult.OK)
                    colorDialog.Color = oldLightColor;

                panelStaticMeshColor.BackColor = colorDialog.Color;
                instance.Color = colorDialog.Color.ToFloatColor() * 2.0f;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
        }

        private void comboItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboItems.SelectedItem == null)
                _editor.ChosenItem = null;
            if (comboItems.SelectedItem is WadMoveable)
            {
                lblStaticMeshColor.Visible = false;
                panelStaticMeshColor.Visible = false;
                _editor.ChosenItem = new ItemType(((WadMoveable)comboItems.SelectedItem).Id, _editor?.Level?.Settings);
            }
            else if (comboItems.SelectedItem is WadStatic)
            {
                lblStaticMeshColor.Visible = true;
                panelStaticMeshColor.Visible = true;
                _editor.ChosenItem = new ItemType(((WadStatic)comboItems.SelectedItem).Id, _editor?.Level?.Settings);
            }
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
