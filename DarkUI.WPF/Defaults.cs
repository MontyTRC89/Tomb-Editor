using System.Windows;

namespace DarkUI.WPF
{
	public static class Defaults
	{
		public const double FontSize = 11;

		public const double InputControlWidth = 75;
		public const double InputControlHeight = 24;

		public const double MaxIconWidth = 12;
		public const double MaxIconHeight = 12;

		public const double CheckBoxWidth = 13;
		public const double CheckBoxHeight = 13;
		public const double CheckMarkStrokeWidth = 2;

		public const double RadioButtonBoxWidth = 13;
		public const double RadioButtonBoxHeight = 13;

		public const double ComboBoxArrowBoxWidth = 17;

		public const double DataGridColumnHeaderGripperWidth = 8;
		public const double DataGridColumnHeaderSortingArrowWidth = 10;

		public const double ThicknessWidth_Border = 1;
		public const double ThicknessWidth_InputControl_DefaultPadding = 3;
		public const double ThicknessWidth_InputControl_LargePadding = 6;
		public const double ThicknessWidth_CheckMarkMargin = 2;

		public const double ThicknessWidth_Tiny = 2;
		public const double ThicknessWidth_Default = 4;
		public const double ThicknessWidth_Large = 8;
		public const double ThicknessWidth_Jumbo = 16;

		public const bool SnapsToDevicePixels = true;
		public const bool UseLayoutRounding = true;

		public static readonly Thickness Thickness_Border = new(ThicknessWidth_Border);
		public static readonly Thickness Padding_InputControl = new(ThicknessWidth_InputControl_DefaultPadding);
		public static readonly Thickness Margin_CheckMark = new(ThicknessWidth_CheckMarkMargin);

		public static readonly Thickness Margin_Tiny = new(ThicknessWidth_Tiny);
		public static readonly Thickness Margin_Default = new(ThicknessWidth_Default);
		public static readonly Thickness Margin_Large = new(ThicknessWidth_Large);
		public static readonly Thickness Margin_Jumbo = new(ThicknessWidth_Jumbo);

		public static readonly Thickness Padding_Tiny = new(ThicknessWidth_Tiny);
		public static readonly Thickness Padding_Default = new(ThicknessWidth_Default);
		public static readonly Thickness Padding_Large = new(ThicknessWidth_Large);
		public static readonly Thickness Padding_Jumbo = new(ThicknessWidth_Jumbo);

		public static readonly Thickness Padding_ListViewItem = new(ThicknessWidth_Default, ThicknessWidth_Tiny, ThicknessWidth_Default, ThicknessWidth_Tiny);

		/// <summary>
		/// Keeps the size of a button at 23x23 with a 16x16 icon.
		/// </summary>
		public static readonly Thickness Padding_ButtonWithIconOnly = new(ThicknessWidth_InputControl_DefaultPadding);
	}
}
