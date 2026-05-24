using System.Windows.Forms;
using TombIDE.ScriptingStudio.ToolStrips;
using TombIDE.ScriptingStudio.UI;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public class StudioContributionSurfaceTests
{
	[TestMethod]
	public void RebuildStudioModeItems_MenuStrip_UsesContributionsWhenStudioModeIsNone()
	{
		var menuStrip = new StudioMenuStrip
		{
			StudioMode = StudioMode.None,
			StudioModeContributionItems =
			new[]
			{
				new StudioToolStripItem
				{
					LangKey = "TestRoot",
					Position = "0",
					DropDownItems =
					new List<StudioToolStripItem>
					{
						new StudioToolStripItem
						{
							LangKey = "TestCommand",
							Command = nameof(UICommand.About)
						}
					}
				}
			}
		};

		menuStrip.RebuildStudioModeItems();

		Assert.AreEqual(1, menuStrip.Items.Count);
		Assert.IsTrue(menuStrip.Items[0] is ToolStripMenuItem);

		var rootItem = menuStrip.Items[0] as ToolStripMenuItem ?? throw new AssertFailedException();
		Assert.AreEqual(1, rootItem.DropDownItems.Count);
		var rootArgs = rootItem.DropDownItems[0].Tag as UIElementArgs ?? throw new AssertFailedException();
		Assert.AreEqual(UICommand.About, rootArgs.Command);
	}

	[TestMethod]
	public void RebuildStudioModeItems_ToolStrip_UsesContributionsWhenStudioModeIsNone()
	{
		var toolStrip = new StudioToolStrip
		{
			StudioMode = StudioMode.None,
			StudioModeContributionItems =
			new[]
			{
				new StudioToolStripItem
				{
					LangKey = "TestCommand",
					Command = nameof(UICommand.About)
				}
			}
		};

		toolStrip.RebuildStudioModeItems();

		Assert.AreEqual(1, toolStrip.Items.Count);
		var toolStripArgs = toolStrip.Items[0].Tag as UIElementArgs ?? throw new AssertFailedException();
		Assert.AreEqual(UICommand.About, toolStripArgs.Command);
	}
}