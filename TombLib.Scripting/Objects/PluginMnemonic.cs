namespace TombLib.Scripting.Objects
{
	public class PluginMnemonic
	{
		public PluginMnemonic(string flagName, string description, short decimalValue)
		{
			FlagName = flagName;
			Description = description;
			DecimalValue = decimalValue;
		}

		public string FlagName { get; }
		public string Description { get; }
		public string HexValue { get; private set; }

		private short _decimalValue;

		public short DecimalValue
		{
			get { return _decimalValue; }
			private set
			{
				_decimalValue = value;

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

				HexValue = hexValue;
			}
		}
	}
}
