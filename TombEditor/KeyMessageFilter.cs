using System.Collections.Generic;
using System.Windows.Forms;

namespace TombEditor
{
	public class KeyMessageFilter : IMessageFilter
	{
		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;
		private bool _keyPressed = false;

		public Dictionary<Keys, bool> KeyTable { get; private set; } = new Dictionary<Keys, bool>();

		public bool IsKeyPressed()
		{
			return _keyPressed;
		}

		public bool IsKeyPressed(Keys k)
		{
            bool pressed;
			if (KeyTable.TryGetValue(k, out pressed))
				return pressed;

			return false;
		}

		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg == WM_KEYDOWN)
			{
				KeyTable[(Keys)m.WParam] = true;

				_keyPressed = true;
			}

			if (m.Msg == WM_KEYUP)
			{
				KeyTable[(Keys)m.WParam] = false;

				_keyPressed = false;
			}

			return false;
		}
	}
}
