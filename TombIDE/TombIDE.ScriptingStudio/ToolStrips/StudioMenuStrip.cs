using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.UI;
using TombIDE.ScriptingStudio.Shortcuts;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ScriptingStudio.ToolStrips
{
	public class StudioMenuStrip : DarkMenuStrip
	{
		#region Properties

		public StudioShortcutBindingService ShortcutBindingService { get; set; }

		public IReadOnlyList<StudioToolStripItem> DocumentModeContributionItems { get; set; }

		public IReadOnlyList<StudioToolStripItem> StudioModeContributionItems { get; set; }

		private StudioMode _studioMode;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public StudioMode StudioMode
		{
			get => _studioMode;
			set
			{
				if (value != _studioMode)
				{
					_studioMode = value;
					UpdateItems<StudioMode>();

					OnStudioModeChanged(EventArgs.Empty);
				}
			}
		}

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
					UpdateItems<DocumentMode>();

					OnDocumentModeChanged(EventArgs.Empty);
				}
			}
		}

		#endregion Properties

		#region Events

		public new event EventHandler ItemClicked;
		private void OnItemClicked(object sender, EventArgs e)
			=> ItemClicked?.Invoke(sender, e);

		public event EventHandler StudioModeChanged;
		private void OnStudioModeChanged(EventArgs e)
			=> StudioModeChanged?.Invoke(this, e);

		public event EventHandler DocumentModeChanged;
		private void OnDocumentModeChanged(EventArgs e)
			=> DocumentModeChanged?.Invoke(this, e);

		#endregion Events

		#region Other methods

		public void RebuildStudioModeItems()
		{
			UpdateItems<StudioMode>();
			OnStudioModeChanged(EventArgs.Empty);
		}

		public void RebuildDocumentModeItems()
		{
			UpdateItems<DocumentMode>();
			OnDocumentModeChanged(EventArgs.Empty);
		}

		private void UpdateItems<T>() where T : Enum
		{
			string enumName = typeof(T).Name;
			Enum modeEnum = GetModeEnum(enumName); // Either StudioMode or DocumentMode
			string enumValueName = GetEnumValueName(modeEnum);
			StudioToolStripItem[] studioItems = GetStudioItems(enumName, enumValueName).ToArray();

			ClearRelatedItems(modeEnum);

			if (enumValueName.Equals("None", StringComparison.OrdinalIgnoreCase) && studioItems.Length == 0)
				return;

			IEnumerable<ToolStripMenuItem> menuItems = GetMenuItemsFromStudioItems(studioItems, modeEnum);

			AddMenuItems(menuItems);
		}

		private void ClearRelatedItems(Enum modeEnum)
		{
			IEnumerable<ToolStripItem> targetItems = Items.GetTargetItems(modeEnum);

			foreach (ToolStripMenuItem menuItem in targetItems)
				SharedMethods.DisposeItems(menuItem.GetAllItems());

			SharedMethods.DisposeItems(targetItems);
		}

		private IEnumerable<ToolStripMenuItem> GetMenuItemsFromStudioItems(IEnumerable<StudioToolStripItem> studioItems, Enum modeEnum)
		{
			foreach (StudioToolStripItem studioItem in studioItems)
			{
				var menuItem = new ToolStripMenuItem(StudioItemParser.GetItemText(studioItem))
				{
					Name = studioItem.Position,
					Tag = new UIElementArgs(modeEnum.GetType())
				};

				menuItem.DropDownItems.AddRange(GetSubMenuItems(studioItem, modeEnum.GetType())?.ToArray());

				yield return menuItem;
			}
		}

		private void AddMenuItems(IEnumerable<ToolStripMenuItem> menuItems)
		{
			foreach (ToolStripMenuItem menuItem in menuItems)
			{
				bool hasPositionDefined = int.TryParse(menuItem.Name, out int position);

				if (hasPositionDefined && position < Items.Count)
					Items.Insert(position, menuItem);
				else
					Items.Add(menuItem);
			}
		}

		private IEnumerable<ToolStripItem> GetSubMenuItems(StudioToolStripItem root, Type uiModeEnumType)
		{
			foreach (StudioToolStripItem item in root.DropDownItems)
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

					var menuItem = new ToolStripMenuItem(text, icon, OnItemClicked, keys)
					{
						ShortcutKeyDisplayString = GetShortcutDisplayText(command, item.KeysDisplay),
						CheckOnClick = item.CheckOnClick,
						Tag = new UIElementArgs(uiModeEnumType, command)
					};

					menuItem.DropDownItems.AddRange(GetSubMenuItems(item, uiModeEnumType)?.ToArray());

					yield return menuItem;
				}
		}

		#endregion Other methods

		private Enum GetModeEnum(string enumName)
			=> GetType().GetProperty(enumName).GetValue(this) as Enum;

		private string GetEnumValueName(Enum @enum)
			=> @enum.ToString().Split('.').Last();

		private string GetShortcutDisplayText(UICommand command, string fallbackDisplayText)
			=> ShortcutBindingService?.GetShortcutDisplayText(command, fallbackDisplayText) ?? fallbackDisplayText;

		private bool TryGetShortcut(UICommand command, out Keys keys)
		{
			if (ShortcutBindingService is not null)
				return ShortcutBindingService.TryGetPrimaryShortcut(command, out keys);

			keys = Keys.None;
			return false;
		}

		private IEnumerable<StudioToolStripItem> GetStudioItems(string enumTypeName, string enumValueName)
		{
			if (enumTypeName == nameof(StudioMode))
				return StudioModeContributionItems ?? [];

			if (enumTypeName == nameof(DocumentMode) && DocumentModeContributionItems?.Count > 0)
				return DocumentModeContributionItems;

			return ToolStripXmlReader.GetItemsFromXml($"UI.{enumTypeName}Presets.MenuStrips.{enumValueName}.xml");
		}
	}
}
