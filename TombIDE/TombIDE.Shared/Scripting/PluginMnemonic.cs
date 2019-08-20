namespace TombIDE.Shared.Scripting
{
	public class PluginMnemonic
	{
		public string Flag { get; set; }
		public string Description { get; set; }
		public string Hex { get; internal set; }

		private short _decimal;

		public short Decimal
		{
			get
			{
				return _decimal;
			}
			set
			{
				_decimal = value;

				string hexValue = value.ToString("X");

				switch (hexValue.Length)
				{
					case 1:
						hexValue = "$000" + hexValue;
						break;

					case 2:
						hexValue = "$00" + hexValue;
						break;

					case 3:
						hexValue = "$0" + hexValue;
						break;

					case 4:
						hexValue = "$" + hexValue;
						break;
				}

				Hex = hexValue;
			}
		}
	}
}
