namespace TombLib.Scripting.ClassicScript.Objects
{
	public class PluginConstant
	{
		public PluginConstant(string flagName, string description, short decimalValue)
		{
			FlagName = flagName;
			Description = description;
			DecimalValue = decimalValue;

			string hexValue = decimalValue.ToString("X");

			int zerosToAdd = 4 - hexValue.Length;
			hexValue = $"${new string('0', zerosToAdd)}{hexValue}";

			HexValue = hexValue;
		}

		public string FlagName { get; }
		public string Description { get; }
		public short DecimalValue { get; }
		public string HexValue { get; }
	}
}
