using System;
using System.Numerics;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Forms.TombEngine
{
    public partial class FormStatic : DarkForm
    {
        private readonly StaticInstance _staticMesh;
        private Vector3 oldColor;

        public FormStatic(StaticInstance staticMesh)
        {
            _staticMesh = staticMesh;
            InitializeComponent();

            // Set window property handlers
            Configuration.ConfigureWindow(this, Editor.Instance.Configuration);
        }

        private void FormObject_Load(object sender, EventArgs e)
        {
            tbOCB.Text = _staticMesh.Ocb.ToString();
            panelColor.BackColor = (_staticMesh.Color * 0.5f).ToWinFormsColor();
            oldColor = _staticMesh.Color;

            tbLuaName.Text = _staticMesh.LuaName;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (!_staticMesh.TrySetLuaName(tbLuaName.Text, this))
                return;

            Editor.Instance.UndoManager.PushObjectPropertyChanged(_staticMesh);

            short result = 0;
            short.TryParse(tbOCB.Text, out result);
            _staticMesh.Ocb = result;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            _staticMesh.Color = oldColor; 
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void tbOCB_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void panelColor_Click(object sender, EventArgs e)
        {
            EditorActions.EditColor(this, _staticMesh, (Vector3 newColor) => { 
                panelColor.BackColor = newColor.ToWinFormsColor(); 
            });
        }
    }
}
