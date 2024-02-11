using System.Windows;

namespace TombIDE.ScriptingStudio.WPFStyles
{
	public static class Defaults
	{
		public const bool SnapsToDevicePixels = true;
		public const bool UseLayoutRounding = true;

		public static readonly double SmallThicknessWidth = 4;
		public static readonly Thickness SmallThickness = new(SmallThicknessWidth);

		public static readonly double ScrollBar_Width = 16;
		public static readonly double ScrollBar_Height = 16;
	}
}
