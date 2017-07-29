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

        public FormMoveable()
        {
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormObject_Load(object sender, EventArgs e)
        {
            MoveableInstance instance = (MoveableInstance)_editor.Level.Objects[_editor.PickingResult._element];

            cbBit1.Checked = instance.Bits[0];
            cbBit2.Checked = instance.Bits[1];
            cbBit3.Checked = instance.Bits[2];
            cbBit4.Checked = instance.Bits[3];
            cbBit5.Checked = instance.Bits[4];

            cbInvisible.Checked = instance.Invisible;
            cbClearBody.Checked = instance.ClearBody;

            tbOCB.Text = instance.Ocb.ToString();
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (!Utils.IsIntegerNumber(tbOCB.Text))
            {
                MessageBox.Show("The value of OCB field is not valid", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MoveableInstance instance = (MoveableInstance)_editor.Level.Objects[_editor.PickingResult._element];

            instance.Bits[0] = cbBit1.Checked;
            instance.Bits[1] = cbBit2.Checked;
            instance.Bits[2] = cbBit3.Checked;
            instance.Bits[3] = cbBit4.Checked;
            instance.Bits[4] = cbBit5.Checked;

            instance.Invisible = cbInvisible.Checked;
            instance.ClearBody = cbClearBody.Checked;

            instance.Ocb = Int16.Parse(tbOCB.Text);

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
