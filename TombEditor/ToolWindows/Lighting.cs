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
                bool HasInOutAngle = false;
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
                            butAddPointLight.Focus();
                            break;

                        case LightType.Shadow:
                            HasInOutRange = true;
                            CanCastShadows = true;
                            CanIlluminateStaticAndDynamicGeometry = true;
                            butAddShadow.Focus();
                            break;

                        case LightType.Effect:
                            HasInOutRange = true;
                            butAddEffectLight.Focus();
                            break;

                        case LightType.FogBulb:
                            HasInOutRange = true;
                            butAddFogBulb.Focus();
                            break;

                        case LightType.Spot:
                            HasInOutRange = true;
                            HasInOutAngle = true;
                            HasDirection = true;
                            CanCastShadows = true;
                            CanIlluminateStaticAndDynamicGeometry = true;
                            butAddSpotLight.Focus();
                            break;

                        case LightType.Sun:
                            HasDirection = true;
                            CanCastShadows = true;
                            CanIlluminateStaticAndDynamicGeometry = true;
                            butAddSun.Focus();
                            break;
                    }

                    panelLightColor.BackColor = new Vector4(light.Color, 1.0f).ToWinFormsColor();
                    numIntensity.Value = (decimal)light.Intensity;

                    if (HasInOutRange)
                    {
                        numInnerRange.Value = (decimal)light.InnerRange;
                        numOuterRange.Value = (decimal)light.OuterRange;
                    }
                    else
                    {
                        numInnerRange.Value = 0;
                        numOuterRange.Value = 0;
                    }
                    if (HasInOutAngle)
                    {
                        numInnerAngle.Value = (decimal)light.InnerAngle;
                        numOuterAngle.Value = (decimal)light.OuterAngle;
                    }
                    else
                    {
                        numInnerAngle.Value = 0;
                        numOuterAngle.Value = 0;
                    }
                    if (HasDirection)
                    {
                        numDirectionY.Value = (decimal)light.RotationY;
                        numDirectionX.Value = (decimal)light.RotationX;
                    }
                    else
                    {
                        numDirectionX.Value = 0;
                        numDirectionY.Value = 0;
                    }

                    cbLightEnabled.Checked = light.Enabled;
                    cbLightCastsShadows.Checked = light.CastsShadows;
                    cbLightIsDynamicallyUsed.Checked = light.IsDynamicallyUsed;
                    cbLightIsStaticallyUsed.Checked = light.IsStaticallyUsed;
                }
                else
                {
                    panelLightColor.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
                    numIntensity.Value = 0;
                    numInnerRange.Value = 0;
                    numOuterRange.Value = 0;
                    numInnerAngle.Value = 0;
                    numOuterAngle.Value = 0;
                    numDirectionX.Value = 0;
                    numDirectionY.Value = 0;
                    cbLightEnabled.Checked = false;
                    cbLightCastsShadows.Checked = false;
                    cbLightIsDynamicallyUsed.Checked = false;
                    cbLightIsStaticallyUsed.Checked = false;
                }

                // Set enabled state
                panelLightColor.Enabled = IsLight;
                cbLightEnabled.Enabled = IsLight;
                cbLightCastsShadows.Enabled = CanCastShadows;
                cbLightIsDynamicallyUsed.Enabled = CanIlluminateStaticAndDynamicGeometry;
                cbLightIsStaticallyUsed.Enabled = CanIlluminateStaticAndDynamicGeometry;
                numIntensity.Enabled = IsLight;
                numInnerRange.Enabled = HasInOutRange;
                numOuterRange.Enabled = HasInOutRange;
                numInnerAngle.Enabled = HasInOutAngle;
                numOuterAngle.Enabled = HasInOutAngle;
                numDirectionY.Enabled = HasDirection;
                numDirectionX.Enabled = HasDirection;
            }
        }

        public void UpdateParameter(decimal value, LightParameter parameter)
        {
            Light light = _editor.SelectedObject as Light;
            if (light == null)
                return;

            switch (parameter)
            {
                default:
                case LightParameter.Intensity:
                    light.Intensity = (float)value;
                    break;

                case LightParameter.InnerRange:
                    light.InnerRange = (float)value;
                    break;

                case LightParameter.OuterRange:
                    light.OuterRange = (float)value;
                    break;

                case LightParameter.InnerAngle:
                    light.InnerAngle = (float)value;
                    break;

                case LightParameter.OuterAngle:
                    light.OuterAngle = (float)value;
                    break;

                case LightParameter.RotationY:
                    light.RotationY = (float)value;
                    break;

                case LightParameter.RotationX:
                    light.RotationX = (float)value;
                    break;
            }

            light.Room.UpdateCompletely();
            _editor.ObjectChange(light);
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
            light.Room.CalculateLightingForThisRoom();
            light.Room.UpdateBuffers();
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

        private void numIntensity_ValueChanged(object sender, EventArgs e)
        {
            UpdateParameter(numIntensity.Value, LightParameter.Intensity);
        }

        private void numInnerRange_ValueChanged(object sender, EventArgs e)
        {
            UpdateParameter(numInnerRange.Value, LightParameter.InnerRange);
        }

        private void numOuterRange_ValueChanged(object sender, EventArgs e)
        {
            UpdateParameter(numOuterRange.Value, LightParameter.OuterRange);
        }

        private void numInnerAngle_ValueChanged(object sender, EventArgs e)
        {
            UpdateParameter(numInnerAngle.Value, LightParameter.InnerAngle);
        }

        private void numOuterAngle_ValueChanged(object sender, EventArgs e)
        {
            UpdateParameter(numOuterAngle.Value, LightParameter.OuterAngle);
        }

        private void numDirectionX_ValueChanged(object sender, EventArgs e)
        {
            UpdateParameter(numDirectionX.Value, LightParameter.RotationX);
        }

        private void numDirectionY_ValueChanged(object sender, EventArgs e)
        {
            UpdateParameter(numDirectionY.Value, LightParameter.RotationY);
        }
    }
}
