using DarkUI.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace TombIDE.ScriptingStudio.Controls
{
	internal class SearchTextBox : DarkTextBox
	{
		public string SearchText
		{
			get => label.Text;
			set => label.Text = value;
		}

		public Color SearchTextColor
		{
			get => label.ForeColor;
			set => label.ForeColor = value;
		}

		private DarkLabel label = new DarkLabel();

		public SearchTextBox()
		{
			label.Dock = DockStyle.Fill;
			label.TextAlign = ContentAlignment.MiddleLeft;

			label.Click += Label_Click;

			Controls.Add(label);
		}

		private void Label_Click(object sender, EventArgs e)
		{
			label.Visible = false;
			Focus();
		}

		protected override void OnEnter(EventArgs e)
		{
			label.Visible = false;

			base.OnEnter(e);
		}

		protected override void OnLeave(EventArgs e)
		{
			base.OnLeave(e);

			if (string.IsNullOrWhiteSpace(Text))
			{
				Text = string.Empty;
				label.Visible = true;
			}
		}
	}
}
