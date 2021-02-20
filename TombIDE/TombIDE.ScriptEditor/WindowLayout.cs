using DarkUI.Docking;
using System.Collections.Generic;
using System.Drawing;

namespace TombIDE.ScriptEditor
{
	internal class WindowLayout
	{
		public static readonly DockPanelState DefaultLayout = new DockPanelState
		{
			Regions = new List<DockRegionState>
			{
				new DockRegionState
				{
					Area = DarkDockArea.Document,
					Size = new Size(0, 0),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "EditorTabControlDocument" },
							VisibleContent = "EditorTabControlDocument"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Left,
					Size = new Size(200, 300),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "ContentExplorer" },
							VisibleContent = "ContentExplorer"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Right,
					Size = new Size(200, 300),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "FileExplorer" },
							VisibleContent = "FileExplorer"
						},
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Bottom,
					Size = new Size(300, 200),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string>
							{
								"ReferenceBrowser",
								"CompilerLogs",
								"SearchResults"
							},

							VisibleContent = "ReferenceBrowser"
						},
					}
				}
			}
		};

		public static readonly DockPanelState TestLayout = new DockPanelState
		{
			Regions = new List<DockRegionState>
			{
				new DockRegionState
				{
					Area = DarkDockArea.Document,
					Size = new Size(0, 0),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "EditorTabControlDocument" },
							VisibleContent = "EditorTabControlDocument"
						}
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Left,
					Size = new Size(200, 300),
					Groups = new List<DockGroupState>()
				},
				new DockRegionState
				{
					Area = DarkDockArea.Right,
					Size = new Size(200, 300),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string> { "FileExplorer" },
							VisibleContent = "FileExplorer"
						},
					}
				},
				new DockRegionState
				{
					Area = DarkDockArea.Bottom,
					Size = new Size(300, 200),
					Groups = new List<DockGroupState>
					{
						new DockGroupState
						{
							Contents = new List<string>
							{
								"ReferenceBrowser",
								"CompilerLogs",
								"SearchResults"
							},

							VisibleContent = "ReferenceBrowser"
						},
					}
				}
			}
		};
	}
}
