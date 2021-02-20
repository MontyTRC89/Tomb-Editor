namespace TombIDE.ScriptingStudio.UI.StudioModePresets
{
	internal static class ToolStripPresets
	{
		public static readonly UIElement[] ClassicScript = new UIElement[]
		{
			UIElement.NewFile,

			UIElement.Save,
			UIElement.SaveAll,

			UIElement.Undo,
			UIElement.Redo,

			UIElement.Cut,
			UIElement.Copy,
			UIElement.Paste,

			UIElement.Build
		};

		public static readonly UIElement[] Lua = new UIElement[]
		{
			UIElement.NewFile,

			UIElement.Save,
			UIElement.SaveAll,

			UIElement.Undo,
			UIElement.Redo,

			UIElement.Cut,
			UIElement.Copy,
			UIElement.Paste
		};
	}
}
