using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormStaticMesh : DarkForm
    {
        private readonly StaticInstance _staticMesh;

        public FormStaticMesh(StaticInstance staticMesh)
        {
            _staticMesh = staticMesh;
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormObject_Load(object sender, EventArgs e)
        {
            cbBurnLaraOnCollision.Checked = (_staticMesh.Ocb & (ushort)StaticMeshFlags.BurnLaraOnCollision) != 0;
            cbDamageLaraOnContact.Checked = (_staticMesh.Ocb & (ushort)StaticMeshFlags.DamageLaraOnCollision) != 0;
            cbDisableCollision.Checked = (_staticMesh.Ocb & (ushort)StaticMeshFlags.DisableCollision) != 0;
            cbExplodeKillingOnCollision.Checked = (_staticMesh.Ocb & (ushort)StaticMeshFlags.ExplodeKillingOnCollision) != 0;
            cbGlassTrasparency.Checked = (_staticMesh.Ocb & (ushort)StaticMeshFlags.GlassTrasparency) != 0;
            cbHardShatter.Checked = (_staticMesh.Ocb & (ushort)StaticMeshFlags.HardShatter) != 0;
            cbHeavyTriggerOnCollision.Checked = (_staticMesh.Ocb & (ushort)StaticMeshFlags.EnableHeavyTriggerOnCollision) != 0;
            cbHugeCollision.Checked = (_staticMesh.Ocb & (ushort)StaticMeshFlags.HugeCollision) != 0;
            cbIceTrasparency.Checked = (_staticMesh.Ocb & (ushort)StaticMeshFlags.IceTrasparency) != 0;
            cbPoisonLaraOnCollision.Checked = (_staticMesh.Ocb & (ushort)StaticMeshFlags.PoisonLaraOnCollision) != 0;
            cbScalable.Checked = (_staticMesh.Ocb & (ushort)StaticMeshFlags.Scalable) != 0;

            if (cbScalable.Checked)
            {
                cbBurnLaraOnCollision.Checked = false;
                cbDamageLaraOnContact.Checked = false;
                cbDisableCollision.Checked = false;
                cbExplodeKillingOnCollision.Checked = false;
                cbGlassTrasparency.Checked = false;
                cbHardShatter.Checked = false;
                cbHeavyTriggerOnCollision.Checked = false;
                cbHugeCollision.Checked = false;
                cbIceTrasparency.Checked = false;
                cbPoisonLaraOnCollision.Checked = false;

                tbScalable.Visible = true;

                tbScalable.Text = ((_staticMesh.Ocb - 4096) / 4).ToString();
            }
            else
            {
                tbScalable.Visible = false;
            }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            ushort ocb = 0;

            if (!cbScalable.Checked)
            {
                if (cbBurnLaraOnCollision.Checked) ocb += (ushort)StaticMeshFlags.BurnLaraOnCollision;
                if (cbDamageLaraOnContact.Checked) ocb += (ushort)StaticMeshFlags.DamageLaraOnCollision;
                if (cbDisableCollision.Checked) ocb += (ushort)StaticMeshFlags.DisableCollision;
                if (cbExplodeKillingOnCollision.Checked) ocb += (ushort)StaticMeshFlags.ExplodeKillingOnCollision;
                if (cbGlassTrasparency.Checked) ocb += (ushort)StaticMeshFlags.GlassTrasparency;
                if (cbHardShatter.Checked) ocb += (ushort)StaticMeshFlags.HardShatter;
                if (cbHeavyTriggerOnCollision.Checked) ocb += (ushort)StaticMeshFlags.EnableHeavyTriggerOnCollision;
                if (cbHugeCollision.Checked) ocb += (ushort)StaticMeshFlags.HugeCollision;
                if (cbIceTrasparency.Checked) ocb += (ushort)StaticMeshFlags.IceTrasparency;
                if (cbPoisonLaraOnCollision.Checked) ocb += (ushort)StaticMeshFlags.PoisonLaraOnCollision;
            }
            else
            {
                int percentage = 0;
                Int32.TryParse(tbScalable.Text, out percentage);
                ocb = (ushort)((ushort)StaticMeshFlags.Scalable + 4 * percentage);
            }

            _staticMesh.Ocb = ocb;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void cbScalable_CheckedChanged(object sender, EventArgs e)
        {
            if (cbScalable.Checked)
            {
                cbBurnLaraOnCollision.Checked = false;
                cbDamageLaraOnContact.Checked = false;
                cbDisableCollision.Checked = false;
                cbExplodeKillingOnCollision.Checked = false;
                cbGlassTrasparency.Checked = false;
                cbHardShatter.Checked = false;
                cbHeavyTriggerOnCollision.Checked = false;
                cbHugeCollision.Checked = false;
                cbIceTrasparency.Checked = false;
                cbPoisonLaraOnCollision.Checked = false;

                tbScalable.Visible = true;
            }
            else
            {
                tbScalable.Visible = false;
            }
        }
    }
}
