﻿using DarkUI.Docking;
using System;
using System.Numerics;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.ToolWindows
{
    public partial class Lighting : DarkToolWindow
    {
        private readonly Editor _editor;

        public Lighting()
        {
            InitializeComponent();
            CommandHandler.AssignCommandsToControls(Editor.Instance, this, toolTip);
            cbLightQuality.SelectedIndex = 0; // Reset index to default

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

            // Disable version-specific controls
            if (obj is Editor.InitEvent ||
                obj is Editor.GameVersionChangedEvent ||
                obj is Editor.LevelChangedEvent)
                butAddFogBulb.Enabled = _editor.Level.Settings.GameVersion > TRVersion.Game.TR3;

            // Update light UI
            if (obj is Editor.ObjectChangedEvent ||
                obj is Editor.SelectedObjectChangedEvent)
            {
                var light = _editor.SelectedObject as LightInstance;

                // Get light type
                bool HasInOutRange = false;
                bool HasInOutAngle = false;
                bool HasDirection = false;
                bool CanCastShadows = false;
                bool CanIlluminateGeometry = false;
                cbLightQuality.Enabled = false;
                if (light != null)
                    switch (light.Type)
                    {
                        case LightType.Point:
                            HasInOutRange = true;
                            CanCastShadows = true;
                            CanIlluminateGeometry = true;
                            break;

                        case LightType.Shadow:
                            HasInOutRange = true;
                            CanCastShadows = true;
                            CanIlluminateGeometry = true;
                            break;

                        case LightType.Effect:
                            HasInOutRange = true;
                            break;

                        case LightType.FogBulb:
                            HasInOutRange = true;
                            break;

                        case LightType.Spot:
                            HasInOutRange = true;
                            HasInOutAngle = true;
                            HasDirection = true;
                            CanCastShadows = true;
                            CanIlluminateGeometry = true;
                            break;

                        case LightType.Sun:
                            HasDirection = true;
                            CanCastShadows = true;
                            CanIlluminateGeometry = true;
                            break;
                    }

                // Update enable state
                // We set it before the values to make sure that irrelevant values are
                // not recognized as being changed to 0 in the "ValueChanged" functions.
                panelLightColor.Enabled = light != null;
                cbLightEnabled.Enabled = light != null;
                cbLightIsObstructedByRoomGeometry.Enabled = CanCastShadows;
                cbLightIsDynamicallyUsed.Enabled = CanIlluminateGeometry;
                cbLightIsStaticallyUsed.Enabled = CanIlluminateGeometry;
                cbLightIsUsedForImportedGeometry.Enabled = CanIlluminateGeometry;
                numIntensity.Enabled = light != null;
                numInnerRange.Enabled = HasInOutRange;
                numOuterRange.Enabled = HasInOutRange;
                numInnerAngle.Enabled = HasInOutAngle;
                numOuterAngle.Enabled = HasInOutAngle;
                numDirectionY.Enabled = HasDirection;
                numDirectionX.Enabled = HasDirection;

                // Update value state
                panelLightColor.BackColor = light != null ? new Vector4(light.Color * 0.5f, 1.0f).ToWinFormsColor() : BackColor;
                numIntensity.Value = (decimal)(light?.Intensity ?? 0);
                numInnerRange.Value = HasInOutRange ? (decimal)light.InnerRange : 0;
                numOuterRange.Value = HasInOutRange ? (decimal)light.OuterRange : 0;
                numInnerAngle.Value = HasInOutAngle ? (decimal)light.InnerAngle : 0;
                numOuterAngle.Value = HasInOutAngle ? (decimal)light.OuterAngle : 0;
                numDirectionY.Value = HasDirection ? (decimal)light.RotationY : 0;
                numDirectionX.Value = HasDirection ? (decimal)light.RotationX : 0;
                cbLightEnabled.Checked = light?.Enabled ?? false;
                cbLightIsObstructedByRoomGeometry.Checked = light?.IsObstructedByRoomGeometry ?? false;
                cbLightIsDynamicallyUsed.Checked = light?.IsDynamicallyUsed ?? false;
                cbLightIsStaticallyUsed.Checked = light?.IsStaticallyUsed ?? false;
                cbLightIsUsedForImportedGeometry.Checked = light?.IsUsedForImportedGeometry ?? false;
                cbLightQuality.Enabled = light != null;
                cbLightQuality.SelectedIndex = (int)(light?.Quality ?? 0);
            }

            // Update tooltip texts
            if (obj is Editor.ConfigurationChangedEvent)
            {
                if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
                    CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);
            }
        }

        private void cbLightEnabled_CheckedChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<bool>((light, value) => light.Enabled == value, (light, value) => light.Enabled = value,
                light => cbLightEnabled.Checked);
        }

        private void cbLightIsObstructedByRoomGeometry_CheckedChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<bool>((light, value) => light.IsObstructedByRoomGeometry == value, (light, value) => light.IsObstructedByRoomGeometry = value,
                light => cbLightIsObstructedByRoomGeometry.Checked);
        }

        private void cbLightIsStaticallyUsed_CheckedChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<bool>((light, value) => light.IsStaticallyUsed == value, (light, value) => light.IsStaticallyUsed = value,
                light => cbLightIsStaticallyUsed.Checked);
        }

        private void cbLightIsDynamicallyUsed_CheckedChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<bool>((light, value) => light.IsDynamicallyUsed == value, (light, value) => light.IsDynamicallyUsed = value,
                light => cbLightIsDynamicallyUsed.Checked);
        }

        private void cbLightIsUsedForImportedGeometry_CheckedChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<bool>((light, value) => light.IsUsedForImportedGeometry == value, (light, value) => light.IsUsedForImportedGeometry = value,
                light => cbLightIsUsedForImportedGeometry.Checked);
        }

        private static bool Compare(float firstValue, float secondValue, NumericUpDown control)
        {
            // Check that this setting even matters for the light...
            if (!control.Enabled)
                return true;

            // Check if the value differs enough to warrant changing it
            // We don't want to polute the editor with useless event's because NumericUpDown
            // decided to round.
            for (int i = 0; i < control.DecimalPlaces; ++i)
            {
                firstValue *= 10.0f;
                secondValue *= 10.0f;
            }

            double firstValueDbl = Math.Round(firstValue);
            double secondValueDbl = Math.Round(secondValue);
            return firstValueDbl == secondValueDbl;
        }

        private void numIntensity_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.Intensity, value, numIntensity),
                (light, value) => light.Intensity = value, light => (float)numIntensity.Value);
        }

        private void numInnerRange_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.InnerRange, value, numInnerRange),
                (light, value) => light.InnerRange = value, light => (float)numInnerRange.Value);
        }

        private void numOuterRange_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.OuterRange, value, numOuterRange),
                (light, value) => light.OuterRange = value, light => (float)numOuterRange.Value);
        }

        private void numInnerAngle_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.InnerAngle, value, numInnerAngle),
                 (light, value) => light.InnerAngle = value, light => (float)numInnerAngle.Value);
        }

        private void numOuterAngle_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.OuterAngle, value, numOuterAngle),
                 (light, value) => light.OuterAngle = value, light => (float)numOuterAngle.Value);
        }

        private void numDirectionY_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.RotationY, value, numDirectionY),
                 (light, value) => light.RotationY = value, light => (float)numDirectionY.Value);
        }

        private void numDirectionX_ValueChanged(object sender, EventArgs e)
        {
            EditorActions.UpdateLight<float>((light, value) => Compare(light.RotationX, value, numDirectionX),
                 (light, value) => light.RotationX = value, light => (float)numDirectionX.Value);
        }

        private void panelLightColor_Click(object sender, EventArgs e)
        {
            EditorActions.EditLightColor(this);
        }

        private void cbLightQualityChanged(object sender, EventArgs e)
        {
            int index = cbLightQuality.SelectedIndex;
            LightQuality newQuality = (LightQuality)index;
            EditorActions.UpdateLightQuality(newQuality);
        }
    }
}
