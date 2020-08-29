using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using TombLib.Utils;

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
                var undoList = new List<UndoRedoInstance>();

                var curr = _editor.SelectedRoom;
                foreach (var r in _editor.SelectedRooms)
                {
                    undoList.Add(new RoomPropertyUndoInstance(_editor.UndoManager, r));

                    // Apply selected attribs

                    if (cbAmbient.Checked)       r.Properties.AmbientLight = curr.Properties.AmbientLight;
                    if (cbCold.Checked)          r.Properties.FlagCold = curr.Properties.FlagCold;
                    if (cbDamage.Checked)        r.Properties.FlagDamage = curr.Properties.FlagDamage;
                    if (cbLensflare.Checked)     r.Properties.FlagNoLensflare = curr.Properties.FlagCold;
                    if (cbLightEffect.Checked)   r.Properties.LightEffect = curr.Properties.LightEffect;
                    if (cbLightStrength.Checked) r.Properties.LightEffectStrength = curr.Properties.LightEffectStrength;
                    if (cbPathfinding.Checked)   r.Properties.FlagExcludeFromPathFinding = curr.Properties.FlagExcludeFromPathFinding;
                    if (cbPortalShade.Checked)   r.Properties.LightInterpolationMode = curr.Properties.LightInterpolationMode;
                    if (cbReverb.Checked)        r.Properties.Reverberation = curr.Properties.Reverberation;
                    if (cbRoomType.Checked)      r.Properties.Type = curr.Properties.Type;
                    if (cbRoomType.Checked)      r.Properties.TypeStrength = curr.Properties.TypeStrength;
                    if (cbSky.Checked)           r.Properties.FlagHorizon = curr.Properties.FlagHorizon;
                    if (cbTags.Checked)          r.Properties.Tags = curr.Properties.Tags;
                    if (cbVisible.Checked)       r.Properties.Hidden = curr.Properties.Hidden;
                    if (cbWind.Checked)          r.Properties.FlagOutside = curr.Properties.FlagOutside;

                    // Updating operations

                    if (cbAmbient.Checked)
                    {
                        r.BuildGeometry();
                        r.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);
                    }

                    _editor.RoomPropertiesChange(r);
                }

                _editor.UndoManager.Push(undoList);
                DialogResult = DialogResult.OK;
            }

            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
