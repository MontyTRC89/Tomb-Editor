using System.Collections.Generic;
using TombIDE.ScriptingStudio.UI;
using TombLib.Scripting.UI.Editors;

namespace TombIDE.ScriptingStudio.CommandSurface;

internal sealed class XmlDocumentCommandSurfaceProvider : IStudioDocumentCommandSurfaceProvider
{
	public static XmlDocumentCommandSurfaceProvider Instance { get; } = new();

	private XmlDocumentCommandSurfaceProvider()
	{ }

	public IReadOnlyList<StudioToolStripItem> GetContextMenuItems(IEditorControl editor, DocumentMode documentMode)
		=> GetItems("ContextMenus", documentMode);

	public IReadOnlyList<StudioToolStripItem> GetMenuStripItems(IEditorControl editor, DocumentMode documentMode)
		=> GetItems("MenuStrips", documentMode);

	public IReadOnlyList<StudioToolStripItem> GetToolStripItems(IEditorControl editor, DocumentMode documentMode)
		=> GetItems("ToolStrips", documentMode);

	private static IReadOnlyList<StudioToolStripItem> GetItems(string surfaceFolder, DocumentMode documentMode)
	{
		if (documentMode == DocumentMode.None)
			return [];

		return ToolStripXmlReader.GetItemsFromXml($"UI.DocumentModePresets.{surfaceFolder}.{documentMode}.xml");
	}
}