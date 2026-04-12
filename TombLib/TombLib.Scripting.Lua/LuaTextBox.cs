using DarkUI.Config;
using DarkUI.Forms;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace TombLib.Scripting.Lua
{
	public partial class LuaTextBox : UserControl
	{
		private DarkTranslucentForm? _overlayForm;
		private LuaEditor? _textEditor;

		public LuaEditor TextEditor => _textEditor ?? throw new InvalidOperationException("Lua editor is not initialized.");

		public LuaTextBox()
		{
			InitializeComponent();
			InitializeTextEditor();
			BackColor = Colors.GreyBackground;
		}

		private void InitializeTextEditor()
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
			{
				_textEditor = new LuaEditor(new Version(0, 0));
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
			if (_textEditor is null)
				return;

			_textEditor.TextArea.PerformTextInput(text);
			_textEditor.Focus();
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);

			if (_textEditor is not null)
				_textEditor.Focus();
		}

		private void textEditor_DragEnter(object? sender, System.Windows.DragEventArgs e)
		{
			if (_overlayForm is null)
				return;

			_overlayForm.Show();
			_overlayForm.Location = PointToScreen(new System.Drawing.Point(0));
			_overlayForm.Size = ClientSize;
		}

		private void overlayForm_DragEnter(object? sender, DragEventArgs e) =>
			OnDragEnter(e);

		private void overlayForm_DragDrop(object? sender, DragEventArgs e)
		{
			if (_overlayForm is null)
				return;

			_overlayForm.Hide();
			OnDragDrop(e);
		}

		private void overlayForm_DragLeave(object? sender, EventArgs e)
		{
			if (_overlayForm is null)
				return;

			_overlayForm.Hide();
			OnDragLeave(e);
		}
	}
}
