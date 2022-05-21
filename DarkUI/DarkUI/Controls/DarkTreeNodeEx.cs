using System;
using System.Drawing;

namespace DarkUI.Controls
{
	public class DarkTreeNodeEx : DarkTreeNode
	{
		public event EventHandler SubTextChanged;
		public event EventHandler ExtraIconChanged;

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

		private Image _extraIcon;
		public Image ExtraIcon
		{
			get { return _extraIcon; }
			set
			{
				if (_extraIcon == value)
					return;

				_extraIcon = value;

				OnExtraIconChanged();
			}
		}

		public Rectangle ExtraIconArea { get; set; }

		public DarkTreeNodeEx(string text, string subText) : base(text)
		{
			SubText = subText;
		}

		private void OnSubTextChanged()
			=> SubTextChanged?.Invoke(this, null);

		private void OnExtraIconChanged()
			=> ExtraIconChanged?.Invoke(this, null);
	}
}
