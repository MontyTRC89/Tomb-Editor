using DarkUI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ScriptingStudio.Properties;
using TombIDE.ScriptingStudio.UI;
using TombIDE.Shared;
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
				SharedMethods.DisposeItems(GetStudioItems()?.ToArray());

				_studioMode = value;
				UpdateStudioItems();
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
				SharedMethods.DisposeItems(GetToolsItems()?.ToArray());

				_documentMode = value;
				UpdateToolsItems();
			}
		}

		#endregion Properties

		#region Items

		private ToolStripItem[] StudioItems => new ToolStripItem[]
		{
			new ToolStripButton(Strings.Default.NewFile, Resources.New_16, OnItemClicked)
			{ Tag = UICommand.NewFile, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripSeparator(),

			new ToolStripButton(Strings.Default.Save, Resources.Save_16, OnItemClicked)
			{ Tag = UICommand.Save, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripButton(Strings.Default.SaveAll, Resources.SaveAll_16, OnItemClicked)
			{ Tag = UICommand.SaveAll, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripSeparator(),

			new ToolStripButton(Strings.Default.Undo, Resources.Undo_16, OnItemClicked)
			{ Tag = UICommand.Undo, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripButton(Strings.Default.Redo, Resources.Redo_16, OnItemClicked)
			{ Tag = UICommand.Redo, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripSeparator(),

			new ToolStripButton(Strings.Default.Cut, Resources.Cut_16, OnItemClicked)
			{ Tag = UICommand.Cut, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripButton(Strings.Default.Copy, Resources.Copy_16, OnItemClicked)
			{ Tag = UICommand.Copy, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripButton(Strings.Default.Paste, Resources.Clipboard_16, OnItemClicked)
			{ Tag = UICommand.Paste, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripSeparator(),

			new ToolStripDropDownButton(Strings.Default.Build, Resources.Play_16, OnItemClicked)
			{
				Tag = UICommand.Build,
				DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
				TextImageRelation = TextImageRelation.ImageBeforeText,
				ShowDropDownArrow = false
			}
		};

		private ToolStripItem[] ToolsItems => new ToolStripItem[]
		{
			new ToolStripSeparator(),

			new ToolStripButton(Strings.Default.CommentOut, Resources.Comment_16, OnItemClicked)
			{ Tag = UICommand.CommentOut, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripButton(Strings.Default.Uncomment, Resources.Uncomment_16, OnItemClicked)
			{ Tag = UICommand.Uncomment, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripSeparator(),

			new ToolStripButton(Strings.Default.ToggleBookmark, Resources.Bookmark_16, OnItemClicked)
			{ Tag = UICommand.ToggleBookmark, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripButton(Strings.Default.PrevBookmark, Resources.PrevBookmark_16, OnItemClicked)
			{ Tag = UICommand.PrevBookmark, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripButton(Strings.Default.NextBookmark, Resources.NextBookmark_16, OnItemClicked)
			{ Tag = UICommand.NextBookmark, DisplayStyle = ToolStripItemDisplayStyle.Image },

			new ToolStripButton(Strings.Default.ClearBookmarks, Resources.ClearBookmarks_16, OnItemClicked)
			{ Tag = UICommand.ClearBookmarks, DisplayStyle = ToolStripItemDisplayStyle.Image }
		};

		#endregion Items

		#region Construction

		public StudioToolStrip()
		{ }
		public StudioToolStrip(StudioMode studioMode, DocumentMode editMode)
		{
			StudioMode = studioMode;
			DocumentMode = editMode;
		}

		#endregion Construction

		#region Events

		public new event EventHandler ItemClicked;
		private void OnItemClicked(object sender, EventArgs e)
			=> ItemClicked?.Invoke(sender, e);

		#endregion Events

		#region Other methods

		private void UpdateStudioItems()
		{
			//Items.AddRange(ItemValidator.GetValidToolStripItems(StudioMode, StudioItems).ToArray());
			//Items.ReduceSeparators();
		}

		private void UpdateToolsItems()
		{
			//Items.AddRange(ItemValidator.GetValidToolStripItems(DocumentMode, ToolsItems).ToArray());
			//Items.ReduceSeparators();
		}

		private IEnumerable<ToolStripItem> GetStudioItems()
		{
			return null;

			//var elements = typeof(UI.StudioModePresets.ToolStripPresets).GetField(StudioMode.ToString()).GetValue(null) as UICommand[];
			//return ItemValidator.GetItems(this.GetAllItems().ToArray(), elements, false);
		}

		private IEnumerable<ToolStripItem> GetToolsItems()
		{
			return null;

			//var elements = typeof(UI.DocumentModePresets.ToolStripPresets).GetField(DocumentMode.ToString()).GetValue(null) as UICommand[];
			//return ItemValidator.GetItems(this.GetAllItems().ToArray(), elements, false);
		}

		#endregion Other methods
	}
}
