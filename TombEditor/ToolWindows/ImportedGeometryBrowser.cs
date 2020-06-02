using DarkUI.Docking;
using System;
using System.Linq;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Wad;

namespace TombEditor.ToolWindows
{
    public partial class ImportedGeometryBrowser : DarkToolWindow
    {
        private readonly Editor _editor;

        public ImportedGeometryBrowser()
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
            if (obj is Editor.LoadedImportedGeometriesChangedEvent)
            {
                var allGeometry  = _editor.Level.Settings.ImportedGeometries.Where(geo => geo.LoadException == null && geo.DirectXModel != null);

                comboItems.Items.Clear();
                foreach (var geo in allGeometry)
                    comboItems.Items.Add(geo);

                if (comboItems.Items.Count > 0)
                {
                    comboItems.SelectedIndex = 0;
                    if (comboItems.SelectedItem is ImportedGeometry)
                    {
                        var currentObject = (ImportedGeometry)panelItem.CurrentObject;
                        if (allGeometry.Contains(currentObject))
                            panelItem.CurrentObject = currentObject;
                    }
                }
            }

            if (obj is Editor.ChosenImportedGeometryChangedEvent)
            {
                var e = (Editor.ChosenImportedGeometryChangedEvent)obj;
                if (e.Model != null)
                    comboItems.SelectedItem = panelItem.CurrentObject = e.Model;
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
            if (comboItems.SelectedItem is ImportedGeometry)
                _editor.ChosenImportedGeometryChange((ImportedGeometry)comboItems.SelectedItem);
        }

        private void comboItems_Format(object sender, ListControlConvertEventArgs e)
        {
            IWadObject listItem = e.ListItem as IWadObject;
            if (listItem != null)
                e.Value = listItem.ToString(_editor.Level.Settings.GameVersion);
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
