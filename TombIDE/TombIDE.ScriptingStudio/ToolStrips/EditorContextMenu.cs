using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Controls
{
	public class EditorContextMenu : DarkContextMenu
	{
		#region Properties

		private DocumentMode _documentMode;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DocumentMode DocumentMode
		{
			get => _documentMode;
			set
			{
				if (value != _documentMode)
				{
					_documentMode = value;
					UpdateItems();

					OnDocumentModeChanged(EventArgs.Empty);
				}
			}
		}

		#endregion Properties

		#region Events

		public new event EventHandler ItemClicked;
		private void OnItemClicked(object sender, EventArgs e)
			=> ItemClicked?.Invoke(sender, e);

		public event EventHandler DocumentModeChanged;
		private void OnDocumentModeChanged(EventArgs e)
			=> DocumentModeChanged?.Invoke(this, e);

		#endregion Events

		#region Other methods

		private void UpdateItems()
		{
			Items.Clear();

			if (DocumentMode == DocumentMode.None)
				return;

			IEnumerable<StudioToolStripItem> studioItems = GetStudioItems();
			IEnumerable<ToolStripItem> toolStripItems = GetToolStripItemsFromStudioItems(studioItems);

			AddItems(toolStripItems);
		}

		private IEnumerable<ToolStripItem> GetToolStripItemsFromStudioItems(IEnumerable<StudioToolStripItem> studioItems)
		{
			foreach (StudioToolStripItem item in studioItems)
				if (item is StudioSeparator)
					yield return new ToolStripSeparator();
				else
				{
					string text = StudioItemParser.GetItemText(item);
					Image icon = StudioItemParser.FindImageInResources(item.Icon);
					Keys keys = StudioItemParser.FindPredefinedKeys(item.Keys);

					yield return new ToolStripMenuItem(text, icon, OnItemClicked, keys)
					{
						ShortcutKeyDisplayString = item.KeysDisplay,
						CheckOnClick = item.CheckOnClick,
						Tag = new UIElementArgs(DocumentMode.GetType(), StudioItemParser.GetCommand(item.Command))
					};
				}
		}

		private void AddItems(IEnumerable<ToolStripItem> items)
		{
			foreach (ToolStripItem item in items)
			{
				bool hasPositionDefined = int.TryParse(item.Name, out int position);

				if (hasPositionDefined && position < Items.Count)
					Items.Insert(position, item);
				else
					Items.Add(item);
			}
		}

		#endregion Other methods

		private IEnumerable<StudioToolStripItem> GetStudioItems()
			=> ToolStripXmlReader.GetItemsFromXml($"UI.DocumentModePresets.ContextMenus.{DocumentMode}.xml");
	}
}
