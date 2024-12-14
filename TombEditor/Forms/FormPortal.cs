using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormPortal : DarkForm
    {
        private readonly PortalInstance _instance;

        public FormPortal(PortalInstance instance)
        {
            InitializeComponent();

            foreach (PortalEffectType effect in Enum.GetValues(typeof(PortalEffectType)))
                comboPortalEffect.Items.Add(effect);

            _instance = instance;
            comboPortalEffect.SelectedItem = _instance.Effect;
            cbReflectMoveables.Checked = _instance.Properties.ReflectMoveables;
            cbReflectStatics.Checked = _instance.Properties.ReflectStatics;
        }

        private void UpdateUI()
        {
            cbReflectMoveables.Enabled =
            cbReflectStatics.Enabled = (PortalEffectType)comboPortalEffect.SelectedItem == PortalEffectType.Mirror;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            _instance.Effect = (PortalEffectType)comboPortalEffect.SelectedItem;
            _instance.Properties.ReflectMoveables = cbReflectMoveables.Checked;
            _instance.Properties.ReflectStatics = cbReflectStatics.Checked;

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
