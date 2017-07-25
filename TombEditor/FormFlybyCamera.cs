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
    public partial class FormFlybyCamera : DarkForm
    {
        public bool IsNew { get; set; }

        private Editor _editor;

        public FormFlybyCamera()
        {
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormFlybyCamera_Load(object sender, EventArgs e)
        {
            _editor = Editor.Instance;

            FlybyCameraInstance flyby = (FlybyCameraInstance)_editor.Level.Objects[_editor.PickingResult.Element];

            cbBit0.Checked = flyby.Flags[0];
            cbBit1.Checked = flyby.Flags[1];
            cbBit2.Checked = flyby.Flags[2];
            cbBit3.Checked = flyby.Flags[3];
            cbBit4.Checked = flyby.Flags[4];
            cbBit5.Checked = flyby.Flags[5];
            cbBit6.Checked = flyby.Flags[6];
            cbBit7.Checked = flyby.Flags[7];
            cbBit8.Checked = flyby.Flags[8];
            cbBit9.Checked = flyby.Flags[9];
            cbBit10.Checked = flyby.Flags[10];
            cbBit11.Checked = flyby.Flags[11];
            cbBit12.Checked = flyby.Flags[12];
            cbBit13.Checked = flyby.Flags[13];
            cbBit14.Checked = flyby.Flags[14];
            cbBit15.Checked = flyby.Flags[15];

            tbTimer.Text = flyby.Timer.ToString();
            tbSequence.Text = flyby.Sequence.ToString();
            tbSpeed.Text = flyby.Speed.ToString();
            tbNumber.Text = flyby.Number.ToString();
            tbFOV.Text = flyby.FOV.ToString();
            tbRoll.Text = flyby.Roll.ToString();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            FlybyCameraInstance flyby = (FlybyCameraInstance)_editor.Level.Objects[_editor.PickingResult.Element];

            flyby.Flags[0] = cbBit0.Checked;
            flyby.Flags[1] = cbBit1.Checked;
            flyby.Flags[2] = cbBit2.Checked;
            flyby.Flags[3] = cbBit3.Checked;
            flyby.Flags[4] = cbBit4.Checked;
            flyby.Flags[5] = cbBit5.Checked;
            flyby.Flags[6] = cbBit6.Checked;
            flyby.Flags[7] = cbBit7.Checked;
            flyby.Flags[8] = cbBit8.Checked;
            flyby.Flags[9] = cbBit9.Checked;
            flyby.Flags[10] = cbBit10.Checked;
            flyby.Flags[11] = cbBit11.Checked;
            flyby.Flags[12] = cbBit12.Checked;
            flyby.Flags[13] = cbBit13.Checked;
            flyby.Flags[14] = cbBit14.Checked;
            flyby.Flags[15] = cbBit15.Checked;

            flyby.Timer = Int16.Parse(tbTimer.Text);
            flyby.Speed = Int16.Parse(tbSpeed.Text);
            flyby.Roll = Int16.Parse(tbRoll.Text);
            flyby.Sequence = Int16.Parse(tbSequence.Text);
            flyby.Number = Int16.Parse(tbNumber.Text);
            flyby.FOV = Int16.Parse(tbFOV.Text);

            _editor.Level.Objects[_editor.PickingResult.Element] = flyby;

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
