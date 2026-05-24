using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Shortcuts;
using TombIDE.ScriptingStudio.UI;

namespace TombIDE.ScriptingStudio.Controls
{
	public class EditorContextMenu : DarkContextMenu
	{
		#region Properties

		public IReadOnlyList<StudioToolStripItem> DocumentModeContributionItems { get; set; }

		public StudioShortcutBindingService ShortcutBindingService { get; set; }

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
					UICommand command = StudioItemParser.GetCommand(item.Command);
					string text = StudioItemParser.GetItemText(item);
					Image icon = StudioItemParser.FindImageInResources(item.Icon);
					Keys keys = TryGetShortcut(command, out Keys shortcutKeys)
						? shortcutKeys
						: StudioItemParser.FindPredefinedKeys(item.Keys);

					yield return new ToolStripMenuItem(text, icon, OnItemClicked, keys)
					{
						ShortcutKeyDisplayString = GetShortcutDisplayText(command, item.KeysDisplay),
						CheckOnClick = item.CheckOnClick,
						Tag = new UIElementArgs(DocumentMode.GetType(), command)
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
			=> DocumentModeContributionItems?.Count > 0
				? DocumentModeContributionItems
				: ToolStripXmlReader.GetItemsFromXml($"UI.DocumentModePresets.ContextMenus.{DocumentMode}.xml");

		private string GetShortcutDisplayText(UICommand command, string fallbackDisplayText)
			=> ShortcutBindingService?.GetShortcutDisplayText(command, fallbackDisplayText) ?? fallbackDisplayText;

		private bool TryGetShortcut(UICommand command, out Keys keys)
		{
			if (ShortcutBindingService is not null)
				return ShortcutBindingService.TryGetPrimaryShortcut(command, out keys);

			keys = Keys.None;
			return false;
		}
	}
}
