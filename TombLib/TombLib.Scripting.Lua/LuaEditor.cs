using System.Windows.Media;
using TombLib.Scripting.Bases;
using TombLib.Scripting.Lua.Objects;

namespace TombLib.Scripting.Lua
{
	public sealed class LuaEditor : TextEditorBase
	{
		public LuaEditor()
		{
			CommentPrefix = "--";
		}

		public override void UpdateSettings(ConfigurationBase configuration)
		{
			var config = configuration as LuaEditorConfiguration;

			SyntaxHighlighting = new SyntaxHighlighting(config.ColorScheme);

			Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Background));
			Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(config.ColorScheme.Foreground));

			base.UpdateSettings(configuration);
		}
	}
}
