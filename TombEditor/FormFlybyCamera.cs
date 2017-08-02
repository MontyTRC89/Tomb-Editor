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

        private FlybyCameraInstance _flyByCamera;

        public FormFlybyCamera(FlybyCameraInstance flyByCamera)
        {
            _flyByCamera = flyByCamera;
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormFlybyCamera_Load(object sender, EventArgs e)
        {
            cbBit0.Checked = _flyByCamera.Flags[0];
            cbBit1.Checked = _flyByCamera.Flags[1];
            cbBit2.Checked = _flyByCamera.Flags[2];
            cbBit3.Checked = _flyByCamera.Flags[3];
            cbBit4.Checked = _flyByCamera.Flags[4];
            cbBit5.Checked = _flyByCamera.Flags[5];
            cbBit6.Checked = _flyByCamera.Flags[6];
            cbBit7.Checked = _flyByCamera.Flags[7];
            cbBit8.Checked = _flyByCamera.Flags[8];
            cbBit9.Checked = _flyByCamera.Flags[9];
            cbBit10.Checked = _flyByCamera.Flags[10];
            cbBit11.Checked = _flyByCamera.Flags[11];
            cbBit12.Checked = _flyByCamera.Flags[12];
            cbBit13.Checked = _flyByCamera.Flags[13];
            cbBit14.Checked = _flyByCamera.Flags[14];
            cbBit15.Checked = _flyByCamera.Flags[15];

            tbTimer.Text = _flyByCamera.Timer.ToString();
            tbSequence.Text = _flyByCamera.Sequence.ToString();
            tbSpeed.Text = _flyByCamera.Speed.ToString();
            tbNumber.Text = _flyByCamera.Number.ToString();
            tbFOV.Text = _flyByCamera.Fov.ToString();
            tbRoll.Text = _flyByCamera.Roll.ToString();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _flyByCamera.Flags[0] = cbBit0.Checked;
            _flyByCamera.Flags[1] = cbBit1.Checked;
            _flyByCamera.Flags[2] = cbBit2.Checked;
            _flyByCamera.Flags[3] = cbBit3.Checked;
            _flyByCamera.Flags[4] = cbBit4.Checked;
            _flyByCamera.Flags[5] = cbBit5.Checked;
            _flyByCamera.Flags[6] = cbBit6.Checked;
            _flyByCamera.Flags[7] = cbBit7.Checked;
            _flyByCamera.Flags[8] = cbBit8.Checked;
            _flyByCamera.Flags[9] = cbBit9.Checked;
            _flyByCamera.Flags[10] = cbBit10.Checked;
            _flyByCamera.Flags[11] = cbBit11.Checked;
            _flyByCamera.Flags[12] = cbBit12.Checked;
            _flyByCamera.Flags[13] = cbBit13.Checked;
            _flyByCamera.Flags[14] = cbBit14.Checked;
            _flyByCamera.Flags[15] = cbBit15.Checked;

            _flyByCamera.Timer = Int16.Parse(tbTimer.Text);
            _flyByCamera.Speed = Int16.Parse(tbSpeed.Text);
            _flyByCamera.Roll = Int16.Parse(tbRoll.Text);
            _flyByCamera.Sequence = Int16.Parse(tbSequence.Text);
            _flyByCamera.Number = Int16.Parse(tbNumber.Text);
            _flyByCamera.Fov = Int16.Parse(tbFOV.Text);
            
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
