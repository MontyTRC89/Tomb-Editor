using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormStaticMesh : DarkForm
    {
        private readonly StaticInstance _staticMesh;
        private ushort newOCB;
        private bool locked;

        public FormStaticMesh(StaticInstance staticMesh)
        {
            _staticMesh = staticMesh;
            newOCB = _staticMesh.Ocb;
            InitializeComponent();
        }

        private void FormObject_Load(object sender, EventArgs e)
        {
            locked = true;
            tbOCB.Text = _staticMesh.Ocb.ToString();
            DecodeOCB();
            locked = false;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (ParseOCB())
            {
                _staticMesh.Ocb = newOCB;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void anyCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (!locked && ActiveControl != tbOCB) EncodeOCB();
        }

        private void numScalable_ValueChanged(object sender, EventArgs e)
        {
            if (!locked) EncodeOCB();
        }

        private void tbOCB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
            else
                locked = true;
        }

        private void tbOCB_TextChanged(object sender, EventArgs e)
        {
            // Preserve cursor position
            var position = tbOCB.SelectionStart;

            // Reset OCB to another control values in case wrong input was done (e.g. pasting of non-numerical string)
            // In other case, try to re-decode OCB so controls stay in sync with textbox.

            if (!ParseOCB())
                EncodeOCB();
            else
            {
                DecodeOCB();
                tbOCB.SelectionStart = position;
            }

            locked = false;
        }

        private void cbScalable_CheckedChanged(object sender, EventArgs e)
        {
            var otherEnabled = !cbScalable.Checked;

            cbBurnLaraOnCollision.Enabled = otherEnabled;
            cbDamageLaraOnContact.Enabled = otherEnabled;
            cbDisableCollision.Enabled = otherEnabled;
            cbExplodeKillingOnCollision.Enabled = otherEnabled;
            cbGlassTrasparency.Enabled = otherEnabled;
            cbHardShatter.Enabled = otherEnabled;
            cbHeavyTriggerOnCollision.Enabled = otherEnabled;
            cbHugeCollision.Enabled = otherEnabled;
            cbIceTrasparency.Enabled = otherEnabled;
            cbPoisonLaraOnCollision.Enabled = otherEnabled;
            // cbSpecificShatter.Enabled = otherEnabled; // No need to disable it as it's above 4096
            numScalable.Visible = !otherEnabled;

            anyCheckbox_CheckedChanged(sender, e);
        }

        private void EncodeOCB()
        {
            ushort backupOCB = (ushort)(newOCB & ~(ushort)StaticMeshFlags.All);
            ushort ocb = 0;

            // CUST_SHATTER_SPECIFIC is above 4096, so let's set it in any case, although behaviour is undefined
            if (cbSpecificShatter.Checked) ocb += (ushort)StaticMeshFlags.SpecificShatter;

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
                ocb += (ushort)((ushort)StaticMeshFlags.Scalable + 4 * (int)numScalable.Value);

            ocb = (ushort)(ocb | backupOCB);

            locked = true;
            tbOCB.Text = ocb.ToString();
            locked = false;
        }

        private void DecodeOCB()
        {
            cbBurnLaraOnCollision.Checked = (newOCB & (ushort)StaticMeshFlags.BurnLaraOnCollision) != 0;
            cbDamageLaraOnContact.Checked = (newOCB & (ushort)StaticMeshFlags.DamageLaraOnCollision) != 0;
            cbDisableCollision.Checked = (newOCB & (ushort)StaticMeshFlags.DisableCollision) != 0;
            cbExplodeKillingOnCollision.Checked = (newOCB & (ushort)StaticMeshFlags.ExplodeKillingOnCollision) != 0;
            cbGlassTrasparency.Checked = (newOCB & (ushort)StaticMeshFlags.GlassTrasparency) != 0;
            cbHardShatter.Checked = (newOCB & (ushort)StaticMeshFlags.HardShatter) != 0;
            cbHeavyTriggerOnCollision.Checked = (newOCB & (ushort)StaticMeshFlags.EnableHeavyTriggerOnCollision) != 0;
            cbHugeCollision.Checked = (newOCB & (ushort)StaticMeshFlags.HugeCollision) != 0;
            cbIceTrasparency.Checked = (newOCB & (ushort)StaticMeshFlags.IceTrasparency) != 0;
            cbPoisonLaraOnCollision.Checked = (newOCB & (ushort)StaticMeshFlags.PoisonLaraOnCollision) != 0;
            cbSpecificShatter.Checked = (newOCB & (ushort)StaticMeshFlags.SpecificShatter) != 0;
            cbScalable.Checked = (newOCB & (ushort)StaticMeshFlags.Scalable) != 0;

            if (cbScalable.Checked)
            {
                numScalable.Visible = true;
                numScalable.Value = (decimal)((newOCB & 4095) / 4.0f);
            }
            else
            {
                numScalable.Visible = false;
            }
        }

        private bool ParseOCB()
        {
            ushort ocb;
            if (!ushort.TryParse(tbOCB.Text, out ocb))
            {
                DarkMessageBox.Show(this, "The value of OCB field is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            newOCB = ocb;
            return true;
        }
    }
}
