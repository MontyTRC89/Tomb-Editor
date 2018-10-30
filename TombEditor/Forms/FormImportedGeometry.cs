using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormImportedGeometry : DarkForm
    {
        public LevelSettings OldLevelSettings { get; }
        public LevelSettings NewLevelSettings { get; }

        private readonly ImportedGeometryInstance _instance;
        private ImportedGeometry.UniqueIDType _currentModel; // Refer to the current geometry by ID to identify it on old and new level settings.

        public FormImportedGeometry(ImportedGeometryInstance instance, LevelSettings levelSettings)
        {
            InitializeComponent();
            _instance = instance;
            _currentModel = instance.Model?.UniqueID;
            OldLevelSettings = levelSettings;
            NewLevelSettings = levelSettings.Clone();
            importedGeometryManager.LevelSettings = NewLevelSettings;
            importedGeometryManager.SelectedImportedGeometry = NewLevelSettings.ImportedGeometryFromID(_currentModel);
            tbMeshFilter.Text = instance.MeshFilter;
            UpdateCurrentModelDisplay();

            // Set position and size
            Size = Editor.Instance.Configuration.Window_FormImportedGeometry_Size;
            Location = Editor.Instance.Configuration.Window_FormImportedGeometry_Position;
            WindowState = Editor.Instance.Configuration.Window_FormImportedGeometry_Maximized ? FormWindowState.Maximized : FormWindowState.Normal;

            if (Location.X == -1000 && Location.Y == -1000)
                StartPosition = FormStartPosition.CenterParent;
            else
                StartPosition = FormStartPosition.Manual;
        }

        private void UpdateCurrentModelDisplay()
        {
            ImportedGeometry currentModelObj = NewLevelSettings.ImportedGeometryFromID(_currentModel);
            if (currentModelObj == null)
                importedGeometryLabel.Text = "None ☹";
            else
                importedGeometryLabel.Text = currentModelObj.Info.Name + "   (" + currentModelObj.Info.Path + ")";
        }

        private void butAssign_Click(object sender, EventArgs e)
        {
            if(importedGeometryManager.SelectedImportedGeometry != null)
            {
                _currentModel = importedGeometryManager.SelectedImportedGeometry.UniqueID;
                UpdateCurrentModelDisplay();
            }
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            _instance.Model = OldLevelSettings.ImportedGeometryFromID(_currentModel) ?? NewLevelSettings.ImportedGeometryFromID(_currentModel);
            _instance.MeshFilter = tbMeshFilter.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Set position and size
            Editor.Instance.Configuration.Window_FormImportedGeometry_Size = Size;
            Editor.Instance.Configuration.Window_FormImportedGeometry_Position = Location;
            Editor.Instance.Configuration.Window_FormImportedGeometry_Maximized = WindowState == FormWindowState.Maximized;

            base.OnClosing(e);
        }
    }
}
