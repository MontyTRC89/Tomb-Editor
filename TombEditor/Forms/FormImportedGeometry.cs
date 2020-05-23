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
            comboLightingModel.SelectedIndex = (int)_instance.LightingModel;
            cbSharpEdges.Checked = _instance.SharpEdges;
            cbHide.Checked = _instance.Hidden;

            // Set window property handlers
            Configuration.LoadWindowProperties(this, Editor.Instance.Configuration);
            FormClosing += new FormClosingEventHandler((s, e) => Configuration.SaveWindowProperties(this, Editor.Instance.Configuration));
        }

        private void UpdateCurrentModelDisplay()
        {
            ImportedGeometry currentModelObj = NewLevelSettings.ImportedGeometryFromID(_currentModel);
            if (currentModelObj == null)
                importedGeometryLabel.Text = "None";
            else
                importedGeometryLabel.Text = currentModelObj.Info.Name + "   (" + currentModelObj.Info.Path + ")";
        }

        private void Assign()
        {
            if (importedGeometryManager.SelectedImportedGeometry != null)
            {
                _currentModel = importedGeometryManager.SelectedImportedGeometry.UniqueID;
                UpdateCurrentModelDisplay();
            }
        }

        private void butAssign_Click(object sender, EventArgs e) => Assign();
        private void importedGeometryManager_MouseDoubleClick(object sender, MouseEventArgs e) => Assign();

        private void butOk_Click(object sender, EventArgs e)
        {
            _instance.Model = OldLevelSettings.ImportedGeometryFromID(_currentModel) ?? NewLevelSettings.ImportedGeometryFromID(_currentModel);
            _instance.LightingModel = (ImportedGeometryLightingModel)comboLightingModel.SelectedIndex;
            _instance.SharpEdges = cbSharpEdges.Checked;
            _instance.Hidden = cbHide.Checked;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if (_currentModel != null && NewLevelSettings != null)
                importedGeometryManager.SelectedImportedGeometry = NewLevelSettings.ImportedGeometryFromID(_currentModel);
            UpdateCurrentModelDisplay();
        }
    }
}
