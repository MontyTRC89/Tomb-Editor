using DarkUI.Controls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using TombIDE.ScriptEditor.Properties;
using TombIDE.ScriptEditor.UI;
using TombIDE.Shared;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ScriptEditor.ToolStrips
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
				UpdateItems<StudioMode>();
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
				UpdateItems<DocumentMode>();
			}
		}

		#endregion Properties

		#region Menus

		// Field naming pattern: (required for proper Reflection fetching)
		// {MENU_NAME}Menu

		private readonly ToolStripMenuItem FileMenu = new ToolStripMenuItem(Strings.Default.File) { Tag = typeof(StudioMode) };
		private readonly ToolStripMenuItem EditMenu = new ToolStripMenuItem(Strings.Default.Edit) { Tag = typeof(StudioMode) };
		private readonly ToolStripMenuItem ToolsMenu = new ToolStripMenuItem(Strings.Default.Tools) { Tag = typeof(DocumentMode) };
		private readonly ToolStripMenuItem OptionsMenu = new ToolStripMenuItem(Strings.Default.Options) { Tag = typeof(StudioMode) };
		private readonly ToolStripMenuItem ViewMenu = new ToolStripMenuItem(Strings.Default.View) { Tag = typeof(StudioMode) };
		private readonly ToolStripMenuItem HelpMenu = new ToolStripMenuItem(Strings.Default.Help) { Tag = typeof(StudioMode) };

		#endregion Menus

		#region Items

		// Field naming pattern: (required for proper Reflection fetching)
		// {MENU_NAME}Items

		// Always make sure to check the following classes after adding new items into the menus:
		// UI.StudioModePresets.MenuStripPresets
		// UI.DocumentModePresets.MenuStripPresets

		private ToolStripItem[] FileItems => new ToolStripItem[]
		{
			new ToolStripMenuItem(Strings.Default.NewFile, Resources.New_16, OnItemClicked, UIKeys.NewFile)
			{ Tag = UIElement.NewFile },

			new ToolStripSeparator(),

			new ToolStripMenuItem(Strings.Default.Save, Resources.Save_16, OnItemClicked, UIKeys.Save)
			{ Tag = UIElement.Save },

			new ToolStripMenuItem(Strings.Default.SaveAs, null, OnItemClicked)
			{ Tag = UIElement.SaveAs },

			new ToolStripMenuItem(Strings.Default.SaveAll, Resources.SaveAll_16, OnItemClicked, UIKeys.SaveAll)
			{ Tag = UIElement.SaveAll },

			new ToolStripSeparator(),

			new ToolStripMenuItem(Strings.Default.Build, Resources.Play_16, OnItemClicked, UIKeys.Build)
			{ Tag = UIElement.Build },
		};

		private ToolStripItem[] EditItems => new ToolStripItem[]
		{
			new ToolStripMenuItem(Strings.Default.Undo, Resources.Undo_16, OnItemClicked)
			{ Tag = UIElement.Undo, ShortcutKeyDisplayString = "Ctrl+Z" }, // Shortcut is handled by the editor itself

			new ToolStripMenuItem(Strings.Default.Redo, Resources.Redo_16, OnItemClicked)
			{ Tag = UIElement.Redo, ShortcutKeyDisplayString = "Ctrl+Y" }, // Shortcut is handled by the editor itself

			new ToolStripSeparator(),

			new ToolStripMenuItem(Strings.Default.Cut, Resources.Cut_16, OnItemClicked)
			{ Tag = UIElement.Cut, ShortcutKeyDisplayString = "Ctrl+X" }, // Shortcut is handled by the editor itself

			new ToolStripMenuItem(Strings.Default.Copy, Resources.Copy_16, OnItemClicked, UIKeys.Copy)
			{ Tag = UIElement.Copy, ShortcutKeyDisplayString = "Ctrl+C" },

			new ToolStripMenuItem(Strings.Default.Paste, Resources.CopyComments_16, OnItemClicked, UIKeys.Paste)
			{ Tag = UIElement.Paste, ShortcutKeyDisplayString = "Ctrl+V" },

			new ToolStripSeparator(),

			new ToolStripMenuItem(Strings.Default.FindReplace, Resources.Search_16, OnItemClicked)
			{ Tag = UIElement.Find, ShortcutKeyDisplayString = "Ctrl+F / Ctrl+H" }, // Shortcut is handled by the editor itself

			new ToolStripSeparator(),

			new ToolStripMenuItem(Strings.Default.SelectAll, null, OnItemClicked)
			{ Tag = UIElement.SelectAll, ShortcutKeyDisplayString = "Ctrl+A" }, // Shortcut is handled by the editor itself
		};

		private ToolStripItem[] ToolsItems => new ToolStripItem[]
		{
			new ToolStripMenuItem(Strings.Default.Reindent, null, OnItemClicked, UIKeys.Reindent)
			{ Tag = UIElement.Reindent },

			new ToolStripMenuItem(Strings.Default.TrimWhitespace, null, OnItemClicked, UIKeys.TrimWhitespace)
			{ Tag = UIElement.TrimWhiteSpace },

			new ToolStripSeparator(),

			new ToolStripMenuItem(Strings.Default.CommentOut, Resources.Comment_16, OnItemClicked, UIKeys.CommentOut)
			{ Tag = UIElement.CommentOut },

			new ToolStripMenuItem(Strings.Default.Uncomment, Resources.Uncomment_16, OnItemClicked, UIKeys.Uncomment)
			{ Tag = UIElement.Uncomment },

			new ToolStripSeparator(),

			new ToolStripMenuItem(Strings.Default.ToggleBookmark, Resources.Bookmark_16, OnItemClicked, UIKeys.ToggleBookmark)
			{ Tag = UIElement.ToggleBookmark },

			new ToolStripMenuItem(Strings.Default.PrevBookmark, Resources.PrevBookmark_16, OnItemClicked, UIKeys.PrevBookmark)
			{ Tag = UIElement.PrevBookmark, ShortcutKeyDisplayString = "Ctrl+Comma" },

			new ToolStripMenuItem(Strings.Default.NextBookmark, Resources.NextBookmark_16, OnItemClicked, UIKeys.NextBookmark)
			{ Tag = UIElement.NextBookmark, ShortcutKeyDisplayString = "Ctrl+Period" },

			new ToolStripMenuItem(Strings.Default.ClearBookmarks, Resources.ClearBookmarks_16, OnItemClicked, UIKeys.ClearBookmarks)
			{ Tag = UIElement.ClearBookmarks },
		};

		private ToolStripItem[] OptionsItems => new ToolStripItem[]
		{
			new ToolStripMenuItem(Strings.Default.UseNewInclude, null, OnItemClicked)
			{ Tag = UIElement.UseNewInclude, CheckOnClick = true },

			new ToolStripMenuItem(Strings.Default.ShowLogsAfterBuild, null, OnItemClicked)
			{ Tag = UIElement.ShowLogsAfterBuild, CheckOnClick = true },

			new ToolStripMenuItem(Strings.Default.ReindentOnSave, null, OnItemClicked)
			{ Tag = UIElement.ReindentOnSave, CheckOnClick = true },

			new ToolStripSeparator(),

			new ToolStripMenuItem(Strings.Default.Settings, Resources.Settings_16, OnItemClicked)
			{ Tag = UIElement.Settings },
		};

		private ToolStripItem[] ViewItems => new ToolStripItem[]
		{
			new ToolStripMenuItem(Strings.Default.ContentExplorer, null, OnItemClicked)
			{ Tag = UIElement.ContentExplorer, CheckOnClick = true },

			new ToolStripMenuItem(Strings.Default.FileExplorer, null, OnItemClicked)
			{ Tag = UIElement.FileExplorer, CheckOnClick = true },

			new ToolStripMenuItem(Strings.Default.ReferenceBrowser, null, OnItemClicked)
			{ Tag = UIElement.ReferenceBrowser, CheckOnClick = true },

			new ToolStripMenuItem(Strings.Default.CompilerLogs, null, OnItemClicked)
			{ Tag = UIElement.CompilerLogs, CheckOnClick = true },

			new ToolStripMenuItem(Strings.Default.SearchResults, null, OnItemClicked)
			{ Tag = UIElement.SearchResults, CheckOnClick = true },

			new ToolStripSeparator(),

			new ToolStripMenuItem(Strings.Default.ToolStrip, null, OnItemClicked)
			{ Tag = UIElement.ToolStrip, CheckOnClick = true },

			new ToolStripMenuItem(Strings.Default.StatusStrip, null, OnItemClicked)
			{ Tag = UIElement.StatusStrip, CheckOnClick = true },
		};

		private ToolStripItem[] HelpItems => new ToolStripItem[]
		{
			new ToolStripMenuItem(Strings.Default.About, Resources.About_16, OnItemClicked)
			{ Tag = UIElement.About }
		};

		#endregion Items

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

		private void UpdateItems<T>() where T : Enum
		{
			FieldInfo[] nonPublicFields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
			PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

			FieldInfo[] menuFields = Array.FindAll(nonPublicFields, x => x.Name.EndsWith("Menu"));
			PropertyInfo[] itemFields = Array.FindAll(properties, x => x.Name.EndsWith("Items"));

			var modeEnum = GetType().GetProperty(typeof(T).Name).GetValue(this) as Enum; // Either StudioMode or DocumentMode

			foreach (FieldInfo menuField in menuFields)
			{
				var menuFieldValue = menuField.GetValue(this) as ToolStripMenuItem;

				if (menuFieldValue.Tag is Type type && type == modeEnum.GetType())
				{
					SharedMethods.DisposeItems(menuFieldValue.GetAllItems().ToArray());

					string menuName = menuField.Name.Replace("Menu", string.Empty); // Example: "FileMenu" returns "File"
					PropertyInfo itemField = Array.Find(itemFields, x => x.Name == $"{menuName}Items"); // Example: x.Name == "FileItems"

					if (itemField == null)
						continue;

					var itemFieldValue = itemField.GetValue(this) as ToolStripItem[];

					menuFieldValue.DropDownItems.AddRange(ItemValidator.GetValidMenuItems(modeEnum, itemFieldValue).ToArray());
					menuFieldValue.DropDownItems.ReduceSeparators();

					menuFieldValue.Enabled = menuFieldValue.DropDownItems.Count > 0;
				}
			}
		}

		#endregion Other methods
	}
}
