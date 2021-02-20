namespace TombIDE.ScriptingStudio.UI.StudioModePresets
{
	internal static class MenuStripPresets
	{
		public static readonly UIElement[] ClassicScript = new UIElement[]
		{
			// File:
			UIElement.NewFile,
			UIElement.Save,
			UIElement.SaveAs,
			UIElement.SaveAll,
			UIElement.Build,

			// Edit:
			UIElement.Undo,
			UIElement.Redo,
			UIElement.Cut,
			UIElement.Copy,
			UIElement.Paste,
			UIElement.Find,
			UIElement.SelectAll,

			// Options:
			UIElement.UseNewInclude,
			UIElement.ShowLogsAfterBuild,
			UIElement.ReindentOnSave,
			UIElement.Settings,

			// View:
			UIElement.ContentExplorer,
			UIElement.FileExplorer,
			UIElement.ReferenceBrowser,
			UIElement.CompilerLogs,
			UIElement.SearchResults,
			UIElement.ToolStrip,
			UIElement.StatusStrip,

			// Help:
			UIElement.About
		};

		public static readonly UIElement[] Lua = new UIElement[]
		{
			// File:
			UIElement.NewFile,
			UIElement.Save,
			UIElement.SaveAs,
			UIElement.SaveAll,

			// Edit:
			UIElement.Undo,
			UIElement.Redo,
			UIElement.Cut,
			UIElement.Copy,
			UIElement.Paste,
			UIElement.Find,
			UIElement.SelectAll,

			// Options:
			UIElement.ReindentOnSave,
			UIElement.Settings,

			// View:
			UIElement.ContentExplorer,
			UIElement.FileExplorer,
			UIElement.SearchResults,
			UIElement.ToolStrip,
			UIElement.StatusStrip,

			// Help:
			UIElement.About
		};
	}
}
