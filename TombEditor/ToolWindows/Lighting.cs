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
using TombEditor.Controls;
using TombEditor.Geometry;
using SharpDX;

namespace TombEditor.ToolWindows
{
    public partial class Lighting : DarkToolWindow
    {
        private Editor _editor;

        public Lighting()
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            // For each control bind its light parameter
            numLightIntensity.LightParameter = LightParameter.Intensity;
            numLightInnerRange.LightParameter = LightParameter.InnerRange;
            numLightOuterRange.LightParameter = LightParameter.OuterRange;
            numLightInnerAngle.LightParameter = LightParameter.InnerAngle;
            numLightOuterAngle.LightParameter = LightParameter.OuterAngle;
            numLightDirectionX.LightParameter = LightParameter.DirectionX;
            numLightDirectionY.LightParameter = LightParameter.DirectionY;
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
            // Update light UI
            if ((obj is Editor.ObjectChangedEvent) ||
               (obj is Editor.SelectedObjectChangedEvent))
            {
                var light = _editor.SelectedObject as Light;

                bool IsLight = false;
                bool HasInOutRange = false;
                bool HasLenCutoffRange = false;
                bool HasDirection = false;
                bool CanCastShadows = false;
                bool CanIlluminateStaticAndDynamicGeometry = false;
                if (light != null)
                {
                    IsLight = true;
                    switch (light.Type)
                    {
                        case LightType.Light:
                            HasInOutRange = true;
                            CanCastShadows = true;
                            CanIlluminateStaticAndDynamicGeometry = true;
                            break;

                        case LightType.Shadow:
                            HasInOutRange = true;
                            CanCastShadows = true;
                            CanIlluminateStaticAndDynamicGeometry = true;
                            break;

                        case LightType.Effect:
                        case LightType.FogBulb:
                            HasInOutRange = true;
                            break;

                        case LightType.Spot:
                            HasInOutRange = true;
                            HasLenCutoffRange = true;
                            HasDirection = true;
                            CanCastShadows = true;
                            CanIlluminateStaticAndDynamicGeometry = true;
                            break;

                        case LightType.Sun:
                            HasDirection = true;
                            CanCastShadows = true;
                            CanIlluminateStaticAndDynamicGeometry = true;
                            break;
                    }

                    panelLightColor.BackColor = new Vector4(light.Color, 1.0f).ToWinFormsColor();
                    numLightIntensity.Value = light.Intensity;
                    cbLightEnabled.Checked = light.Enabled;
                    cbLightCastsShadows.Checked = light.CastsShadows;
                    cbLightIsDynamicallyUsed.Checked = light.IsDynamicallyUsed;
                    cbLightIsStaticallyUsed.Checked = light.IsStaticallyUsed;
                    numLightInnerRange.Value = light.InnerRange;
                    numLightOuterRange.Value = light.OuterRange;
                    numLightInnerAngle.Value = light.InnerAngle;
                    numLightOuterAngle.Value = light.OuterAngle;
                    numLightDirectionX.Value = light.RotationX;
                    numLightDirectionY.Value = light.RotationY;
                }
                else
                    panelLightColor.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);

                // Set enabled state
                panelLightColor.Enabled = IsLight;
                numLightIntensity.Enabled = IsLight;
                cbLightEnabled.Enabled = IsLight;
                cbLightCastsShadows.Enabled = CanCastShadows;
                cbLightIsDynamicallyUsed.Enabled = CanIlluminateStaticAndDynamicGeometry;
                cbLightIsStaticallyUsed.Enabled = CanIlluminateStaticAndDynamicGeometry;
                numLightInnerRange.Enabled = HasInOutRange;
                numLightOuterRange.Enabled = HasInOutRange;
                numLightInnerAngle.Enabled = HasLenCutoffRange;
                numLightOuterAngle.Enabled = HasLenCutoffRange;
                numLightDirectionX.Enabled = HasDirection;
                numLightDirectionY.Enabled = HasDirection;
            }
        }

        private void UpdateLight<T>(Func<Light, T> getLightValue, Action<Light, T> setLightValue, Func<T, T?> getGuiValue) where T : struct
        {
            var light = _editor.SelectedObject as Light;
            if (light == null)
                return;

            T? newValue = getGuiValue(getLightValue(light));
            if ((!newValue.HasValue) || newValue.Value.Equals(getLightValue(light)))
                return;

            setLightValue(light, newValue.Value);
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();
            _editor.ObjectChange(light);
        }

        private void panelLightColor_Click(object sender, EventArgs e)
        {
            UpdateLight((light) => light.Color, (light, value) => light.Color = value,
                (value) =>
                {
                    colorDialog.Color = new Vector4(value, 1.0f).ToWinFormsColor();
                    if (colorDialog.ShowDialog(this) != DialogResult.OK)
                        return null;
                    return colorDialog.Color.ToFloatColor3();
                });
        }

        private void cbLightEnabled_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLight((light) => light.Enabled, (light, value) => light.Enabled = value,
                (value) => cbLightEnabled.Checked);
        }

        private void cbLightCastsShadows_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLight((light) => light.CastsShadows, (light, value) => light.CastsShadows = value,
                (value) => cbLightCastsShadows.Checked);
        }

        private void cbLightIsStaticallyUsed_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLight((light) => light.IsStaticallyUsed, (light, value) => light.IsStaticallyUsed = value,
                (value) => cbLightIsStaticallyUsed.Checked);
        }

        private void cbLightIsDynamicallyUsed_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLight((light) => light.IsDynamicallyUsed, (light, value) => light.IsDynamicallyUsed = value,
                (value) => cbLightIsDynamicallyUsed.Checked);
        }

        private void butAddPointLight_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceLight, LightType = LightType.Light };
        }

        private void butAddShadow_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceLight, LightType = LightType.Shadow };
        }

        private void butAddSun_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceLight, LightType = LightType.Sun };
        }

        private void butAddSpotLight_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceLight, LightType = LightType.Spot };
        }

        private void butAddEffectLight_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceLight, LightType = LightType.Effect };
        }

        private void butAddFogBulb_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceLight, LightType = LightType.FogBulb };
        }
    }
}
