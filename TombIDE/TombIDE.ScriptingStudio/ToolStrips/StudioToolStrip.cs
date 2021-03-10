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
	public class StudioToolStrip : DarkToolStrip
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
			IEnumerable<ToolStripItem> toolStripItems = GetToolStripItemsFromStudioItems(studioItems, modeEnum);

			Items.AddRange(toolStripItems.ToArray());
		}

		private void ClearRelatedItems(Enum modeEnum)
			=> SharedMethods.DisposeItems(Items.GetTargetItems(modeEnum));

		private IEnumerable<ToolStripItem> GetToolStripItemsFromStudioItems(IEnumerable<StudioToolStripItem> studioItems, Enum modeEnum)
		{
			foreach (StudioToolStripItem studioItem in studioItems)
				if (studioItem is StudioSeparator)
					yield return new ToolStripSeparator() { Tag = new UIElementArgs(modeEnum.GetType()) };
				else
				{
					string text = StudioItemParser.GetItemText(studioItem);
					Image icon = StudioItemParser.FindImageInResources(studioItem.Icon);
					Keys keys = StudioItemParser.FindPredefinedKeys(studioItem.Keys);

					if (studioItem is StudioToolStripButton)
						yield return new ToolStripDropDownButton(text, icon, OnItemClicked)
						{
							DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
							TextImageRelation = TextImageRelation.ImageBeforeText,
							ShowDropDownArrow = false,
							Tag = new UIElementArgs(modeEnum.GetType(), StudioItemParser.GetCommand(studioItem.Command))
						};
					else
						yield return new ToolStripButton(text, icon, OnItemClicked)
						{
							DisplayStyle = ToolStripItemDisplayStyle.Image,
							CheckOnClick = studioItem.CheckOnClick,
							Tag = new UIElementArgs(modeEnum.GetType(), StudioItemParser.GetCommand(studioItem.Command))
						};
				}
		}

		#endregion Other methods

		private Enum GetModeEnum(string enumName)
			=> GetType().GetProperty(enumName).GetValue(this) as Enum;

		private string GetEnumValueName(Enum @enum)
			=> @enum.ToString().Split('.').Last();

		private IEnumerable<StudioToolStripItem> GetStudioItems(string enumTypeName, string enumValueName)
			=> ToolStripXmlReader.GetItemsFromXml($"UI.{enumTypeName}Presets.ToolStrips.{enumValueName}.xml");
	}
}
