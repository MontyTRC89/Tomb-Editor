using System.Windows;

namespace DarkUI.WPF
{
	public static class Defaults
	{
		#region Constants

		public const double ScaleFactor = 1.0;

		public const bool SnapsToDevicePixels = true;
		public const bool UseLayoutRounding = true;

		#endregion Constants

		public static readonly double FontSize = 11 * ScaleFactor;

		#region General thicknesses

		public static readonly double BorderThicknessWidth = 1 * ScaleFactor;
		public static readonly double TinyThicknessWidth = 2 * ScaleFactor;
		public static readonly double MediumThicknessWidth = 4 * ScaleFactor;
		public static readonly double LargeThicknessWidth = 8 * ScaleFactor;
		public static readonly double JumboThicknessWidth = 16 * ScaleFactor;

		public static readonly Thickness BorderThickness = new(BorderThicknessWidth);
		public static readonly Thickness TinyThickness = new(TinyThicknessWidth);
		public static readonly Thickness MediumThickness = new(MediumThicknessWidth);
		public static readonly Thickness LargeThickness = new(LargeThicknessWidth);
		public static readonly Thickness JumboThickness = new(JumboThicknessWidth);

		public static readonly Thickness InvertedBorderThickness = new(-BorderThicknessWidth);

		#endregion General thicknesses

		#region InputControl // Button, CheckBox, ColorPickerButton, ComboBox, NumericUpDown, ProgressBar, RadioButton, SearchableComboBox, TextBox, ToggleButton

		public static readonly double InputControl_Width = 75 * ScaleFactor;
		public static readonly double InputControl_Height = 24 * ScaleFactor;
		public static readonly double InputControl_DefaultPaddingWidth = 3 * ScaleFactor;
		public static readonly double InputControl_LargePaddingWidth = 6 * ScaleFactor;

		public static readonly Thickness InputControl_DefaultPadding = new(InputControl_DefaultPaddingWidth);

		#endregion InputControl // Button, CheckBox, ColorPickerButton, ComboBox, NumericUpDown, ProgressBar, RadioButton, SearchableComboBox, TextBox, ToggleButton

		#region CheckBox

		public static readonly double CheckBox_BoxWidth = 13 * ScaleFactor;
		public static readonly double CheckBox_BoxHeight = 13 * ScaleFactor;
		public static readonly double CheckBox_CheckMarkStrokeWidth = 2 * ScaleFactor;

		public static readonly Thickness CheckBox_CheckMarkMargin = new(0.5 * ScaleFactor);
		public static readonly Thickness CheckBox_IndeterminateMarkMargin = new(2 * ScaleFactor);

		#endregion CheckBox

		#region ComboBox

		public static readonly double ComboBox_ArrowBox_Width = 17 * ScaleFactor;
		public static readonly double ComboBox_Popup_MaxHeight = 300 * ScaleFactor;
		public static readonly double ComboBox_Popup_NoItemsHeight = 40 * ScaleFactor;

		public static readonly Thickness ComboBox_ArrowBox_Padding = new(3 * ScaleFactor);

		#endregion ComboBox

		#region DataGrid

		public static readonly double DataGrid_ColumnHeader_GripperWidth = 8 * ScaleFactor;
		public static readonly double DataGrid_ColumnHeader_SortingArrowWidth = 10 * ScaleFactor;

		#endregion DataGrid

		#region DropShadowEffect

		public static readonly double DropShadow_Depth = 3 * ScaleFactor;
		public static readonly double DropShadow_Opacity = 0.3;

		#endregion DropShadowEffect

		#region GroupBox

		public static readonly double GroupBox_BorderSpacingWidth = 6 * ScaleFactor;
		public static readonly GridLength GroupBox_BorderSpacing = new(GroupBox_BorderSpacingWidth);

		#endregion GroupBox

		#region Image

		public static readonly double Image_MaxIconWidth = 16 * ScaleFactor;
		public static readonly double Image_MaxIconHeight = 16 * ScaleFactor;

		#endregion Image

		#region Menu / MenuItem

		public static readonly double MenuItem_Height = 24 * ScaleFactor;
		public static readonly double MenuItem_CheckBoxWidth = 20 * ScaleFactor;
		public static readonly double MenuItem_CheckBoxHeight = 20 * ScaleFactor;

		public static readonly double SubMenuPopup_HorizontalOffset = -4 * ScaleFactor;
		public static readonly double SubMenuPopup_ScrollButton_MinWidth = 10 * ScaleFactor;
		public static readonly double SubMenuPopup_ScrollButton_MinHeight = 10 * ScaleFactor;

		public static readonly GridLength SubMenuItem_ArrowArea_Width = new(12 * ScaleFactor);
		public static readonly GridLength SubMenuItem_IconArea_Width = new(24 * ScaleFactor);

		public static readonly Thickness SubMenuItem_Arrow_Margin = new(4 * ScaleFactor);

		#endregion Menu / MenuItem

		#region NumericUpDown

		public static readonly double NumericUpDown_SpinnerWidth = 17 * ScaleFactor;
		public static readonly Thickness NumericUpDown_RepeatButtonPadding = new(2 * ScaleFactor);

		#endregion NumericUpDown

		#region Path

		public static readonly double Path_MaxIconWidth = 12 * ScaleFactor;
		public static readonly double Path_MaxIconHeight = 12 * ScaleFactor;

		#endregion Path

		#region RadioButton

		public static readonly double RadioButton_BoxWidth = 13 * ScaleFactor;
		public static readonly double RadioButton_BoxHeight = 13 * ScaleFactor;

		public static readonly Thickness RadioButton_CheckMarkMargin = new(2 * ScaleFactor);

		#endregion RadioButton

		public static readonly double ShadowMarginThicknessWidth = 6 * ScaleFactor;
		public static readonly Thickness Thickness_ShadowMargin = new(0, 0, ShadowMarginThicknessWidth, ShadowMarginThicknessWidth);

		public static readonly GridLength TreeView_Indentation = new(19 * ScaleFactor);
		public static readonly Thickness TreeView_ExpanderIconNormalMargin = new(1.5 * ScaleFactor);
		public static readonly Thickness TreeView_ExpanderIconCheckedMargin = new(2.75 * ScaleFactor);

		public static readonly double ScrollBar_Width = 17 * ScaleFactor;
		public static readonly double ScrollBar_Height = 17 * ScaleFactor;

		public static readonly double TitleBar_Height = 30 * ScaleFactor;
		public static readonly double TitleBar_ChromeHeight = TitleBar_Height - 5; // 5 is a system constant (I think)
		public static readonly double TitleBar_PathIconStrokeWidth = 1 * ScaleFactor;
		public static readonly double TitleBar_ButtonWidth = 40 * ScaleFactor;
		public static readonly double TitleBar_FontSize = 12 * ScaleFactor;
		public static readonly double TitleBar_IconWidth = 64 * ScaleFactor;
		public static readonly double TitleBar_IconVerticalOffset = -12 * ScaleFactor;

		public static readonly double TabControl_SelectedTabMarginOffset = -2 * ScaleFactor;

		public static readonly double ToolBar_GripperSize = 6 * ScaleFactor;
		public static readonly double ToolBar_GripperMarginWidth = 3 * ScaleFactor;
		public static readonly Thickness ToolBar_GripperMargin = new(ToolBar_GripperMarginWidth);
		public static readonly Rect ToolBar_GripperPatternViewbox = new(0, 0, 4, 4);
		public static readonly Rect ToolBar_GripperPatternViewport = new(0, 0, 4, 4);
		public static readonly double ToolBar_OverflowButtonSize = 14 * ScaleFactor;
		public static readonly double ToolBar_MaxOverflowPopupWidth = 200 * ScaleFactor;
		public static readonly double ToolBar_DropDownButtonArrowWidth = 6 * ScaleFactor;
	}
}
