using DarkUI.Docking;
using System;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Wad;
using TombLib.Wad.Catalog;

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
            if (obj is Editor.LoadedWadsChangedEvent ||
                obj is Editor.GameVersionChangedEvent)
            {
                var allMoveables = _editor.Level.Settings.WadGetAllMoveables();
                var allStatics   = _editor.Level.Settings.WadGetAllStatics();
                
                comboItems.Items.Clear();
                foreach (var moveable in allMoveables.Values)
                    comboItems.Items.Add(moveable);
                foreach (var staticMesh in allStatics.Values)
                    comboItems.Items.Add(staticMesh);

                if (comboItems.Items.Count > 0)
                {
                    comboItems.SelectedIndex = 0;

                    // Update visible conflicting item, otherwise it's not updated in 3D control.
                    if(comboItems.SelectedItem is WadMoveable)
                    {
                        var currentObject = (WadMoveableId)panelItem.CurrentObject.Id;
                        if (allMoveables.ContainsKey(currentObject))
                            panelItem.CurrentObject = allMoveables[currentObject];
                    }
                    else if (comboItems.SelectedItem is WadStatic)
                    {
                        var currentObject = (WadStaticId)panelItem.CurrentObject.Id;
                        if (allStatics.ContainsKey(currentObject))
                            panelItem.CurrentObject = allStatics[currentObject];
                    }
                }
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
                    var version = _editor.Level.Settings.GameVersion;

                    if (e.Current.Value.MoveableId == WadMoveableId.Lara) // Show Lara's skin
                    {
                        var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(version, e.Current.Value.MoveableId.TypeId));
                        var moveableSkin = _editor.Level.Settings.WadTryGetMoveable(skinId);
                        if (moveableSkin != null)
                            panelItem.CurrentObject = moveableSkin;
                    }

                    panelItem.Invalidate();
                }
            }

            // Update tooltip texts
            if (obj is Editor.ConfigurationChangedEvent)
            {
                if(((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
                    CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);
            }
        }

        private void butSearch_Click(object sender, EventArgs e)
        {
            var searchPopUp = new PopUpSearch(comboItems, _editor.Level.Settings.GameVersion);
            searchPopUp.Show(this);
        }

        private void comboItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboItems.SelectedItem == null)
                _editor.ChosenItem = null;
            else if (comboItems.SelectedItem is WadMoveable)
                _editor.ChosenItem = new ItemType(((WadMoveable)comboItems.SelectedItem).Id, _editor?.Level?.Settings);
            else if (comboItems.SelectedItem is WadStatic)
                _editor.ChosenItem = new ItemType(((WadStatic)comboItems.SelectedItem).Id, _editor?.Level?.Settings);
        }

        private void comboItems_Format(object sender, ListControlConvertEventArgs e)
        {
            TRVersion.Game? gameVersion = _editor?.Level?.Settings?.GameVersion;
            IWadObject listItem = e.ListItem as IWadObject;
            if (gameVersion != null && listItem != null)
                e.Value = listItem.ToString(gameVersion.Value);
        }

        private void butItemUp_Click(object sender, EventArgs e)
        {
            if(comboItems.Items.Count > 0 && comboItems.SelectedIndex > 0)
                comboItems.SelectedIndex--;
        }

        private void butItemDown_Click(object sender, EventArgs e)
        {
            if (comboItems.Items.Count > 0 && comboItems.SelectedIndex != comboItems.Items.Count - 1)
                comboItems.SelectedIndex++;
        }
    }
}
