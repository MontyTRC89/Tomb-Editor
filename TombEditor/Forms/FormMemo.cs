using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormMemo : DarkForm
    {
        private readonly MemoInstance _memo;
        private Editor _editor;

        public FormMemo(MemoInstance memo)
        {
            InitializeComponent();

            _editor = Editor.Instance;
            _memo = memo;
            tbText.Text = _memo.Text;

            // Set window property handlers
            Configuration.LoadWindowProperties(this, Editor.Instance.Configuration);
            FormClosing += new FormClosingEventHandler((s, e) => Configuration.SaveWindowProperties(this, Editor.Instance.Configuration));
        }

        private void SaveChanges()
        {
            _memo.Text = tbText.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormMemo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control)
                switch (e.KeyCode)
                {
                    case Keys.A:
                        if (tbText.Focused)
                            tbText.SelectAll();
                        break;

                    case Keys.Enter:
                        SaveChanges();
                        break;
                }
        }

        private void FormMemo_Shown(object sender, EventArgs e) => tbText.Focus();
        private void butOK_Click(object sender, EventArgs e) => SaveChanges();
    }
}
