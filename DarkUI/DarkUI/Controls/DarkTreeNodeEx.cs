using System;
using System.Drawing;

namespace DarkUI.Controls
{
	public class DarkTreeNodeEx : DarkTreeNode
	{
		public event EventHandler SubTextChanged;

		private string _subText;
		public string SubText
		{
			get { return _subText; }
			set
			{
				if (_subText == value)
					return;

				_subText = value;

				OnSubTextChanged();
			}
		}

		public Rectangle SubTextArea { get; set; }

		public DarkTreeNodeEx(string text, string subText) : base(text)
		{
			SubText = subText;
		}

		private void OnSubTextChanged()
			=> SubTextChanged?.Invoke(this, null);
	}
}
