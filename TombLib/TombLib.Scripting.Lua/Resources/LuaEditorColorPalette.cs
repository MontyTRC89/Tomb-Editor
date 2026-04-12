using System.Windows.Media;
using TombLib.Scripting.Resources;
using static TombLib.WPF.BrushHelpers;

namespace TombLib.Scripting.Lua.Resources
{
	internal static class LuaEditorColorPalette
	{
		public static readonly Brush MutedTextBrush = CreateFrozenBrush("#8C8C8C");
		public static readonly Brush MiscBrush = CreateFrozenBrush("#C8C8C8");
		public static readonly Brush MethodBrush = CreateFrozenBrush("#DCDCAA");
		public static readonly Brush VariableBrush = CreateFrozenBrush("#9CDCFE");
		public static readonly Brush PropertyBrush = CreateFrozenBrush("#4FC1FF");
		public static readonly Brush TypeBrush = CreateFrozenBrush("#4EC9B0");
		public static readonly Brush KeywordBrush = CreateFrozenBrush("#C586C0");
		public static readonly Brush ConstantBrush = CreateFrozenBrush("#B5CEA8");
		public static readonly Brush FileBrush = CreateFrozenBrush("#D7BA7D");

		public static readonly SolidColorBrush SignatureParamDocForeground = CreateFrozenBrush(Color.FromRgb(180, 180, 180));
		public static readonly SolidColorBrush SignatureActiveParamForeground = CreateFrozenBrush(Color.FromRgb(86, 180, 235));
		public static readonly SolidColorBrush SignatureForeground = TextEditorColorPalette.ToolTipForeground;
	}
}