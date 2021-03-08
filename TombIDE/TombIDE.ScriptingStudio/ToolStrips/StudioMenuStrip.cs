using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Properties;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
using TombIDE.Shared.Local;

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
				_studioMode = value;
				GetStudioModeItems();
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
				_documentMode = value;
				GetDocumentModeItems();
			}
		}

		#endregion Properties

		#region Construction

		public StudioMenuStrip()
			=> AddDefaultItems();

		public StudioMenuStrip(StudioMode studioMode, DocumentMode editMode)
		{
			AddDefaultItems();

			StudioMode = studioMode;
			DocumentMode = editMode;
		}

		private void AddDefaultItems()
		{
			FieldInfo[] nonPublicFields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			FieldInfo[] menuFields = Array.FindAll(nonPublicFields, x => x.Name.EndsWith("Menu"));

			foreach (FieldInfo menuField in menuFields)
				Items.Add(menuField.GetValue(this) as ToolStripMenuItem);
		}

		#endregion Construction

		#region Events

		public new event EventHandler ItemClicked;
		private void OnItemClicked(object sender, EventArgs e)
			=> ItemClicked?.Invoke(sender, e);

		#endregion Events

		#region Other methods

		private IEnumerable<ToolStripItem> GetStudioModeItems()
		{
			List<StudioToolStripItem> items = ToolStripXmlReader.GetItemsFromXml($"UI.StudioModePresets.MenuStrips.{StudioMode}.xml");

			foreach (StudioToolStripItem item in items)
			{
				string text = item.OverrideText ??
					typeof(Localization).GetProperty(item.LangKey ?? string.Empty)?.GetValue(Strings.Default)?.ToString() ??
					item.LangKey;

				var menuItem = new ToolStripMenuItem(text) { Name = item.Position };
				menuItem.DropDownItems.AddRange(GetItems(item)?.ToArray());

				yield return menuItem;
			}
		}

		private IEnumerable<ToolStripItem> GetDocumentModeItems()
		{
			//List<StudioToolStripItem> items = ToolStripXmlReader.GetItemsFromXml($"UI.DocumentModePresets.MenuStrips.{DocumentMode}.xml");

			//foreach (StudioToolStripItem item in items)
			//{
			//	GetItems(item);
			//}
		}

		private IEnumerable<ToolStripItem> GetItems(StudioToolStripItem root)
		{
			foreach (StudioToolStripItem item in root.DropDownItems)
			{
				if (item is StudioSeparator)
				{
					yield return new ToolStripSeparator();
					continue;
				}

				string text = item.OverrideText ??
					typeof(Localization).GetProperty(item.LangKey ?? string.Empty)?.GetValue(Strings.Default)?.ToString() ??
					item.LangKey;

				var icon = typeof(Resources).GetProperty(item.Icon ?? string.Empty, BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as Image;

				object keysValue = typeof(UIKeys).GetField(item.Keys ?? string.Empty, BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
				Keys keys = keysValue != null ? (Keys)keysValue : Keys.None;

				Enum.TryParse(item.Command, true, out UICommand command);

				var menuItem = new ToolStripMenuItem(text, icon, OnItemClicked, keys)
				{
					ShortcutKeyDisplayString = item.KeysDisplay,
					CheckOnClick = item.CheckOnClick.GetValueOrDefault(),
					Tag = command
				};

				menuItem.DropDownItems.AddRange(GetItems(item)?.ToArray());

				yield return menuItem;
			}
		}

		#endregion Other methods
	}
}
