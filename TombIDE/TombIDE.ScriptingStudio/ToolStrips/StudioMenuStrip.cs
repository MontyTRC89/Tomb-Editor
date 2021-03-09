using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ScriptingStudio.ToolStrips
{
	public class StudioMenuStrip : DarkMenuStrip
	{
		#region Properties

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

		#region Construction

		public StudioMenuStrip()
		{ }
		public StudioMenuStrip(StudioMode studioMode, DocumentMode editMode)
		{
			StudioMode = studioMode;
			DocumentMode = editMode;
		}

		#endregion Construction

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

		private void UpdateItems<T>() where T : Enum
		{
			string enumName = typeof(T).Name;
			Enum modeEnum = GetModeEnum(enumName); // Either StudioMode or DocumentMode
			string enumValueName = GetEnumValueName(modeEnum);

			ClearRelatedItems(modeEnum);

			if (enumValueName.Equals("None", StringComparison.OrdinalIgnoreCase))
				return;

			IEnumerable<StudioToolStripItem> studioItems = GetStudioItems(enumName, enumValueName);
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
					Tag = modeEnum.GetType()
				};

				menuItem.DropDownItems.AddRange(GetSubMenuItems(studioItem)?.ToArray());

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

		private IEnumerable<ToolStripItem> GetSubMenuItems(StudioToolStripItem root)
		{
			foreach (StudioToolStripItem item in root.DropDownItems)
				if (item is StudioSeparator)
					yield return new ToolStripSeparator();
				else
				{
					string text = StudioItemParser.GetItemText(item);
					Image icon = StudioItemParser.FindImageInResources(item.Icon);
					Keys keys = StudioItemParser.FindPredefinedKeys(item.Keys);

					var menuItem = new ToolStripMenuItem(text, icon, OnItemClicked, keys)
					{
						ShortcutKeyDisplayString = item.KeysDisplay,
						CheckOnClick = item.CheckOnClick,
						Tag = StudioItemParser.GetCommand(item.Command)
					};

					menuItem.DropDownItems.AddRange(GetSubMenuItems(item)?.ToArray());

					yield return menuItem;
				}
		}

		#endregion Other methods

		private Enum GetModeEnum(string enumName)
			=> GetType().GetProperty(enumName).GetValue(this) as Enum;

		private string GetEnumValueName(Enum @enum)
			=> @enum.ToString().Split('.').Last();

		private IEnumerable<StudioToolStripItem> GetStudioItems(string enumTypeName, string enumValueName)
			=> ToolStripXmlReader.GetItemsFromXml($"UI.{enumTypeName}Presets.MenuStrips.{enumValueName}.xml");
	}
}
