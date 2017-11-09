using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Forms;

namespace TombLib.GeometryIO
{
    public partial class GeometryIOSettingsDialog : DarkForm
    {
        public IOGeometrySettings Settings { get; private set; }
        private List<IOGeometrySettingsPreset> _presets = new List<IOGeometrySettingsPreset>();

        public GeometryIOSettingsDialog(IOGeometrySettings inSettings)
        {
            InitializeComponent();
            Settings = inSettings;
            UpdateControls(Settings);
            PopulatePresetList();
            SelectNullPreset();
            StartControlListening();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
                SuspendControlListening();

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public void ReloadSettings(IOGeometrySettings settings)
        {
            UpdateControls(settings);
            UpdateSettings();
        }

        public void AddPreset(string name, IOGeometrySettings settings)
        {
            if(!_presets.Any((item) => item.Name == name))
            {
                _presets.Add(new IOGeometrySettingsPreset() { Name = name, Settings = settings });
                PopulatePresetList();
            }
        }
        
        public void AddPreset(List<IOGeometrySettingsPreset> presetList)
        {
            foreach(var preset in presetList)
                if (!_presets.Any((item => item.Name == preset.Name)))
                    _presets.Add(preset);

            PopulatePresetList();
        }

        public void AddPreset(IOGeometrySettingsPreset preset)
        {
            if (!_presets.Any((item => item.Name == preset.Name)))
            {
                _presets.Add(preset);
                PopulatePresetList();
            }
        }

        public void RemovePreset(string name = null)
        {
            if(string.IsNullOrEmpty(name))
                _presets.Clear();
            else
                _presets.RemoveAll((item) => item.Name == name);

            PopulatePresetList();
        }

        public void SelectPreset(string name)
        {
            // Use this function to remotely call specified preset.
            // Should be handy when selecting specified file format from open/save dialog.

            var index = cmbPresetList.FindString(name);
            if (index != -1)
                cmbPresetList.SelectedIndex = index;
        }

        private void SelectNullPreset()
        {
            if(cmbPresetList.SelectedIndex != -1)
                cmbPresetList.SelectedIndex = -1;
        }

        private void PopulatePresetList()
        {
            cmbPresetList.Items.Clear();
            foreach(var preset in _presets)
                cmbPresetList.Items.Add(preset.Name);

            if (cmbPresetList.Items.Count == 0)
                cmbPresetList.Enabled = false;
            else
                cmbPresetList.Enabled = true;
        }

        private void UpdateControls(IOGeometrySettings settings)
        {
            cbFlipX.Checked = settings.FlipX;
            cbFlipY.Checked = settings.FlipY;
            cbFlipZ.Checked = settings.FlipZ;
            cbSwapXY.Checked = settings.SwapXY;
            cbSwapXZ.Checked = settings.SwapXZ;
            cbSwapYZ.Checked = settings.SwapYZ;
            cbFlipUV_V.Checked = settings.FlipUV_V;
            cbPremultiplyUV.Checked = settings.PremultiplyUV;
            cbWrapUV.Checked = settings.WrapUV;
            nmScale.Value = (decimal)settings.Scale;
            cbDivide.Checked = settings.DivideByScale;
            cbInvertFaces.Checked = settings.InvertFaces;
        }

        private void UpdateSettings()
        {
            Settings.FlipX = cbFlipX.Checked;
            Settings.FlipY = cbFlipY.Checked;
            Settings.FlipZ = cbFlipZ.Checked;
            Settings.SwapXY = cbSwapXY.Checked;
            Settings.SwapXZ = cbSwapXZ.Checked;
            Settings.SwapYZ = cbSwapYZ.Checked;
            Settings.FlipUV_V = cbFlipUV_V.Checked;
            Settings.PremultiplyUV = cbPremultiplyUV.Checked;
            Settings.WrapUV = cbWrapUV.Checked;
            Settings.Scale = (float)nmScale.Value;
            Settings.DivideByScale = cbDivide.Checked;
            Settings.InvertFaces = cbInvertFaces.Checked;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            UpdateSettings();
            DialogResult = DialogResult.OK; // OKButton property does not set it...
            Close();
        }

        private void cmbPresetList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cmbPresetList.SelectedIndex != -1)
            {
                // We need to suspend control listening to prevent combobox setting
                // back to modified state.

                SuspendControlListening();
                ReloadSettings(_presets[cmbPresetList.SelectedIndex].Settings);
                StartControlListening();
            }
        }

        private void StartControlListening()
        {
            cbFlipX.CheckedChanged += ModifiedPresetEvent;
            cbFlipY.CheckedChanged += ModifiedPresetEvent;
            cbFlipZ.CheckedChanged += ModifiedPresetEvent;
            cbSwapXY.CheckedChanged += ModifiedPresetEvent;
            cbSwapXZ.CheckedChanged += ModifiedPresetEvent;
            cbSwapYZ.CheckedChanged += ModifiedPresetEvent;
            cbFlipUV_V.CheckedChanged += ModifiedPresetEvent;
            cbPremultiplyUV.CheckedChanged += ModifiedPresetEvent;
            cbWrapUV.CheckedChanged += ModifiedPresetEvent;
            cbDivide.CheckedChanged += ModifiedPresetEvent;
            nmScale.ValueChanged += ModifiedPresetEvent;
        }

        private void SuspendControlListening()
        {
            cbFlipX.CheckedChanged -= ModifiedPresetEvent;
            cbFlipY.CheckedChanged -= ModifiedPresetEvent;
            cbFlipZ.CheckedChanged -= ModifiedPresetEvent;
            cbSwapXY.CheckedChanged -= ModifiedPresetEvent;
            cbSwapXZ.CheckedChanged -= ModifiedPresetEvent;
            cbSwapYZ.CheckedChanged -= ModifiedPresetEvent;
            cbFlipUV_V.CheckedChanged -= ModifiedPresetEvent;
            cbPremultiplyUV.CheckedChanged -= ModifiedPresetEvent;
            cbWrapUV.CheckedChanged -= ModifiedPresetEvent;
            cbDivide.CheckedChanged -= ModifiedPresetEvent;
            nmScale.ValueChanged -= ModifiedPresetEvent;
        }

        private void ModifiedPresetEvent(object sender, EventArgs e)
        {
            SelectNullPreset();
        }
    }
}
