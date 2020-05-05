using System;
using System.Windows.Forms;
using System.ComponentModel;
using DarkUI.Config;
using DarkUI.Forms;
using TombLib.Scripting.TextEditors.Controls;

namespace TombLib.Scripting.Forms.Controls
{
    public partial class LuaTextBox : UserControl
    {
        private DarkTranslucentForm _overlayForm;
        private LuaTextEditor _textEditor;

        public LuaTextEditor TextEditor { get { return _textEditor; } }

        public LuaTextBox()
        {
            InitializeComponent();
            InitializeTextEditor();
            BackColor = Colors.GreyBackground;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
                {
                    _overlayForm.DragEnter -= overlayForm_DragEnter;
                    _overlayForm.DragDrop -= overlayForm_DragDrop;
                    _overlayForm.DragLeave -= overlayForm_DragLeave;
                    _overlayForm.Close();
                    _overlayForm = null;

                    ehTextEditor.Child = null;
                    _textEditor.DragEnter -= textEditor_DragEnter;
                    _textEditor = null;
                }

                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeTextEditor()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _textEditor = new LuaTextEditor();
                _textEditor.AllowDrop = true;
                _textEditor.WordWrap = true;
                _textEditor.DragEnter += textEditor_DragEnter;
                ehTextEditor.Child = _textEditor;

                _overlayForm = new DarkTranslucentForm(Colors.GreyBackground, 0.01); // 0 won't show form!
                _overlayForm.AllowDrop = true;
                _overlayForm.DragEnter += overlayForm_DragEnter;
                _overlayForm.DragDrop += overlayForm_DragDrop;
                _overlayForm.DragLeave += overlayForm_DragLeave;
            }
        }

        public void Paste(string text)
        {
            var newSelStart = _textEditor.SelectionStart + text.Length;
            _textEditor.Text = _textEditor.Text.Insert(_textEditor.SelectionStart, text);
            _textEditor.SelectionStart = newSelStart;
            _textEditor.Focus();
        }
        
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            _textEditor.Focus();
        }

        private void textEditor_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            _overlayForm.Show();
            _overlayForm.Location = PointToScreen(new System.Drawing.Point(0));
            _overlayForm.Size = ClientSize;
        }

        private void overlayForm_DragEnter(object sender, System.Windows.Forms.DragEventArgs e) => OnDragEnter(e);

        private void overlayForm_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            _overlayForm.Hide();
            OnDragDrop(e);
        }

        private void overlayForm_DragLeave(object sender, EventArgs e)
        {
            _overlayForm.Hide();
            OnDragLeave(e);
        }
    }
}
