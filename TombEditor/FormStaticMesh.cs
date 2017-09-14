using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor
{
    public partial class FormStaticMesh : DarkForm
    {
        private StaticInstance _staticMesh;
        
        public FormStaticMesh(StaticInstance staticMesh)
        {
            _staticMesh = staticMesh;
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
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
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            ushort ocb = 0;

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

            _staticMesh.Ocb = ocb;

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
