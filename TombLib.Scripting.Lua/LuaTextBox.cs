using DarkUI.Config;
using DarkUI.Forms;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace TombLib.Scripting.Lua
{
	public partial class LuaTextBox : UserControl
	{
		private DarkTranslucentForm _overlayForm;

		public LuaEditor TextEditor { get; private set; }

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
				TextEditor = new LuaEditor();
				TextEditor.AllowDrop = true;
				TextEditor.WordWrap = true;
				TextEditor.DragEnter += textEditor_DragEnter;
				ehTextEditor.Child = TextEditor;

				_overlayForm = new DarkTranslucentForm(Colors.GreyBackground, 0.01); // 0 won't show form!
				_overlayForm.AllowDrop = true;
				_overlayForm.DragEnter += overlayForm_DragEnter;
				_overlayForm.DragDrop += overlayForm_DragDrop;
				_overlayForm.DragLeave += overlayForm_DragLeave;
			}
		}

		public void Paste(string text)
		{
			TextEditor.TextArea.PerformTextInput(text);
			TextEditor.Focus();
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			TextEditor.Focus();
		}

		private void textEditor_DragEnter(object sender, System.Windows.DragEventArgs e)
		{
			_overlayForm.Show();
			_overlayForm.Location = PointToScreen(new System.Drawing.Point(0));
			_overlayForm.Size = ClientSize;
		}

		private void overlayForm_DragEnter(object sender, DragEventArgs e) =>
			OnDragEnter(e);

		private void overlayForm_DragDrop(object sender, DragEventArgs e)
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
