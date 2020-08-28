using System;
using System.Windows.Forms;
using DarkUI.Forms;

namespace TombEditor.Forms
{
    public partial class FormRoomProperties : DarkForm
    {
        private readonly Editor _editor;

        public FormRoomProperties(Editor editor)
        {
            InitializeComponent();
            _editor = editor;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show(this, "Are you sure you want to apply selected properties to selected rooms rooms?\n" +
                                          "This action can't be undone.",
                                    "Apply room properties", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                var curr = _editor.SelectedRoom;
                foreach (var r in _editor.SelectedRooms)
                {
                    // Apply selected attribs

                    if (cbAmbient.Checked)       r.AmbientLight = curr.AmbientLight;
                    if (cbCold.Checked)          r.FlagCold = curr.FlagCold;
                    if (cbDamage.Checked)        r.FlagDamage = curr.FlagDamage;
                    if (cbLensflare.Checked)     r.FlagNoLensflare = curr.FlagCold;
                    if (cbLightEffect.Checked)   r.LightEffect = curr.LightEffect;
                    if (cbLightStrength.Checked) r.LightEffectStrength = curr.LightEffectStrength;
                    if (cbPathfinding.Checked)   r.FlagExcludeFromPathFinding = curr.FlagExcludeFromPathFinding;
                    if (cbPortalShade.Checked)   r.LightInterpolationMode = curr.LightInterpolationMode;
                    if (cbReverb.Checked)        r.Reverberation = curr.Reverberation;
                    if (cbRoomType.Checked)      r.Type = curr.Type;
                    if (cbRoomType.Checked)      r.TypeStrength = curr.TypeStrength;
                    if (cbSky.Checked)           r.FlagHorizon = curr.FlagHorizon;
                    if (cbTags.Checked)          r.Tags = curr.Tags;
                    if (cbVisible.Checked)       r.Hidden = curr.Hidden;
                    if (cbWind.Checked)          r.FlagOutside = curr.FlagOutside;

                    // Updating operations

                    if (cbAmbient.Checked)
                    {
                        r.BuildGeometry();
                        r.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                    }

                    _editor.RoomPropertiesChange(r);
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
