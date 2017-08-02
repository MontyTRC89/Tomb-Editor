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
    public partial class FormMoveable : DarkForm
    {
        private Editor _editor = Editor.Instance;
        private MoveableInstance _movable;
        
        public FormMoveable(MoveableInstance movable)
        {
            _movable = movable;
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormObject_Load(object sender, EventArgs e)
        {
            cbBit1.Checked = _movable.Bits[0];
            cbBit2.Checked = _movable.Bits[1];
            cbBit3.Checked = _movable.Bits[2];
            cbBit4.Checked = _movable.Bits[3];
            cbBit5.Checked = _movable.Bits[4];

            cbInvisible.Checked = _movable.Invisible;
            cbClearBody.Checked = _movable.ClearBody;

            tbOCB.Text = _movable.Ocb.ToString();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (!Utils.IsIntegerNumber(tbOCB.Text))
            {
                MessageBox.Show("The value of OCB field is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _movable.Bits[0] = cbBit1.Checked;
            _movable.Bits[1] = cbBit2.Checked;
            _movable.Bits[2] = cbBit3.Checked;
            _movable.Bits[3] = cbBit4.Checked;
            _movable.Bits[4] = cbBit5.Checked;

            _movable.Invisible = cbInvisible.Checked;
            _movable.ClearBody = cbClearBody.Checked;

            _movable.Ocb = Int16.Parse(tbOCB.Text);

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
