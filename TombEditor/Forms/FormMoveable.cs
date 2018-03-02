using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor
{
    public partial class FormMoveable : DarkForm
    {
        private MoveableInstance _movable;

        public FormMoveable(MoveableInstance moveable)
        {
            _movable = moveable;
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormObject_Load(object sender, EventArgs e)
        {
            cbBit1.Checked = (_movable.CodeBits & (1 << 0)) != 0;
            cbBit2.Checked = (_movable.CodeBits & (1 << 1)) != 0;
            cbBit3.Checked = (_movable.CodeBits & (1 << 2)) != 0;
            cbBit4.Checked = (_movable.CodeBits & (1 << 3)) != 0;
            cbBit5.Checked = (_movable.CodeBits & (1 << 4)) != 0;

            cbInvisible.Checked = _movable.Invisible;
            cbClearBody.Checked = _movable.ClearBody;

            tbOCB.Text = _movable.Ocb.ToString();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            short ocb;
            if (!short.TryParse(tbOCB.Text, out ocb))
            {
                DarkMessageBox.Show(this, "The value of OCB field is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte CodeBits = 0;
            CodeBits |= (byte)(cbBit1.Checked ? (1 << 0) : 0);
            CodeBits |= (byte)(cbBit2.Checked ? (1 << 1) : 0);
            CodeBits |= (byte)(cbBit3.Checked ? (1 << 2) : 0);
            CodeBits |= (byte)(cbBit4.Checked ? (1 << 3) : 0);
            CodeBits |= (byte)(cbBit5.Checked ? (1 << 4) : 0);
            _movable.CodeBits = CodeBits;

            _movable.Invisible = cbInvisible.Checked;
            _movable.ClearBody = cbClearBody.Checked;

            _movable.Ocb = ocb;

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
