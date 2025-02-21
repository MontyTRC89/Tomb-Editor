using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormPortal : DarkForm
    {
        private readonly PortalInstance _instance;

        public FormPortal(PortalInstance instance)
        {
            InitializeComponent();

            foreach (PortalEffectType effect in Enum.GetValues(typeof(PortalEffectType)))
                comboPortalEffect.Items.Add(effect.ToString().SplitCamelcase());

            _instance = instance;

            int effectID = (int)_instance.Effect;
            comboPortalEffect.SelectedIndex = effectID < comboPortalEffect.Items.Count ? effectID : -1;
            cbReflectMoveables.Checked = _instance.Properties.ReflectMoveables;
            cbReflectStatics.Checked = _instance.Properties.ReflectStatics;
            cbReflectSprites.Checked = _instance.Properties.ReflectSprites;
            cbReflectLights.Checked = _instance.Properties.ReflectLights;
        }

        private void UpdateUI()
        {
            cbReflectMoveables.Enabled =
            cbReflectStatics.Enabled =
            cbReflectSprites.Enabled =
            cbReflectLights.Enabled = (PortalEffectType)comboPortalEffect.SelectedIndex == PortalEffectType.ClassicMirror;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            _instance.Effect = (PortalEffectType)comboPortalEffect.SelectedIndex;
            _instance.Properties.ReflectMoveables = cbReflectMoveables.Checked;
            _instance.Properties.ReflectStatics = cbReflectStatics.Checked;
            _instance.Properties.ReflectSprites = cbReflectSprites.Checked;
            _instance.Properties.ReflectLights = cbReflectLights.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void comboPortalEffect_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }
    }
}
