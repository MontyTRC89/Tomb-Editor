namespace TombIDE.ScriptEditor
{
	partial class ScriptEditor : System.Windows.Forms.UserControl
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.button_Build = new System.Windows.Forms.ToolStripButton();
			this.button_ClearBookmarks = new System.Windows.Forms.ToolStripButton();
			this.button_Comment = new System.Windows.Forms.ToolStripButton();
			this.button_Copy = new System.Windows.Forms.ToolStripButton();
			this.button_Cut = new System.Windows.Forms.ToolStripButton();
			this.button_EditLanguages = new DarkUI.Controls.DarkButton();
			this.button_EditScript = new DarkUI.Controls.DarkButton();
			this.button_NextBookmark = new System.Windows.Forms.ToolStripButton();
			this.button_OpenInExplorer = new DarkUI.Controls.DarkButton();
			this.button_Paste = new System.Windows.Forms.ToolStripButton();
			this.button_PrevBookmark = new System.Windows.Forms.ToolStripButton();
			this.button_Redo = new System.Windows.Forms.ToolStripButton();
			this.button_ResetZoom = new DarkUI.Controls.DarkButton();
			this.button_Save = new System.Windows.Forms.ToolStripButton();
			this.button_SaveAll = new System.Windows.Forms.ToolStripButton();
			this.button_ToggleBookmark = new System.Windows.Forms.ToolStripButton();
			this.button_Uncomment = new System.Windows.Forms.ToolStripButton();
			this.button_Undo = new System.Windows.Forms.ToolStripButton();
			this.contextMenu_TextBox = new DarkUI.Controls.DarkContextMenu();
			this.contextMenuItem_Cut = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuItem_Paste = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_12 = new System.Windows.Forms.ToolStripSeparator();
			this.contextMenuItem_Comment = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuItem_Uncomment = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_13 = new System.Windows.Forms.ToolStripSeparator();
			this.contextMenuItem_ToggleBookmark = new System.Windows.Forms.ToolStripMenuItem();
			this.inputTimer = new System.Windows.Forms.Timer(this.components);
			this.label_ColNumber = new System.Windows.Forms.ToolStripStatusLabel();
			this.label_LineNumber = new System.Windows.Forms.ToolStripStatusLabel();
			this.label_SelectedChars = new System.Windows.Forms.ToolStripStatusLabel();
			this.label_Zoom = new DarkUI.Controls.DarkLabel();
			this.menu_Edit = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Undo = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Redo = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_01 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Cut = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Paste = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_02 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Find = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Replace = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_03 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_SelectAll = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_File = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Save = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_SaveAll = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_16 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Build = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_Help = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_About = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_Tools = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Reindent = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Trim = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_04 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Comment = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Uncomment = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_05 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_ToggleBookmark = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_PrevBookmark = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_NextBookmark = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_ClearBookmarks = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_06 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Settings = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_View = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_ObjBrowser = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_FileList = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_InfoBox = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_14 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_ToolStrip = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_StatusStrip = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_15 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_LineNumbers = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_ToolTips = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip = new DarkUI.Controls.DarkMenuStrip();
			this.panel_Editor = new System.Windows.Forms.Panel();
			this.tabControl_Editor = new System.Windows.Forms.CustomTabControl();
			this.referenceBrowser = new TombIDE.ScriptEditor.ReferenceBrowser();
			this.richTextBox_Logs = new System.Windows.Forms.RichTextBox();
			this.scriptFolderWatcher = new System.IO.FileSystemWatcher();
			this.sectionPanel_Files = new DarkUI.Controls.DarkSectionPanel();
			this.treeView_Files = new DarkUI.Controls.DarkTreeView();
			this.sectionPanel_InfoBox = new DarkUI.Controls.DarkSectionPanel();
			this.tabControl_Info = new System.Windows.Forms.CustomTabControl();
			this.tabPage_RefBrowser = new System.Windows.Forms.TabPage();
			this.tabPage_CompilerLogs = new System.Windows.Forms.TabPage();
			this.sectionPanel_ObjBrowser = new DarkUI.Controls.DarkSectionPanel();
			this.treeView_Objects = new DarkUI.Controls.DarkTreeView();
			this.textBox_SearchObj = new DarkUI.Controls.DarkTextBox();
			this.separator_07 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_08 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_09 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_10 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_11 = new System.Windows.Forms.ToolStripSeparator();
			this.splitter_Bottom = new System.Windows.Forms.Splitter();
			this.splitter_Left = new System.Windows.Forms.Splitter();
			this.splitter_Right = new System.Windows.Forms.Splitter();
			this.statusStrip = new DarkUI.Controls.DarkStatusStrip();
			this.toolStrip = new DarkUI.Controls.DarkToolStrip();
			this.contextMenu_TextBox.SuspendLayout();
			this.menuStrip.SuspendLayout();
			this.panel_Editor.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.scriptFolderWatcher)).BeginInit();
			this.sectionPanel_Files.SuspendLayout();
			this.sectionPanel_InfoBox.SuspendLayout();
			this.tabControl_Info.SuspendLayout();
			this.tabPage_RefBrowser.SuspendLayout();
			this.tabPage_CompilerLogs.SuspendLayout();
			this.sectionPanel_ObjBrowser.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Build
			// 
			this.button_Build.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Build.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Build.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Build.Image = global::TombIDE.ScriptEditor.Properties.Resources.actions_play_16;
			this.button_Build.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Build.Name = "button_Build";
			this.button_Build.Size = new System.Drawing.Size(23, 25);
			this.button_Build.Text = "Build Script (F9)";
			this.button_Build.Click += new System.EventHandler(this.ToolStrip_Build_Click);
			// 
			// button_ClearBookmarks
			// 
			this.button_ClearBookmarks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_ClearBookmarks.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_ClearBookmarks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_ClearBookmarks.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_clearbookmarks_16;
			this.button_ClearBookmarks.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_ClearBookmarks.Name = "button_ClearBookmarks";
			this.button_ClearBookmarks.Size = new System.Drawing.Size(23, 25);
			this.button_ClearBookmarks.Text = "Clear all Bookmarks (Ctrl+Shift+B)";
			this.button_ClearBookmarks.Click += new System.EventHandler(this.ToolStrip_ClearBookmarks_Click);
			// 
			// button_Comment
			// 
			this.button_Comment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Comment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Comment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Comment.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_comment_16;
			this.button_Comment.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Comment.Name = "button_Comment";
			this.button_Comment.Size = new System.Drawing.Size(23, 25);
			this.button_Comment.Text = "Comment out Lines (Ctrl+Shift+C)";
			this.button_Comment.Click += new System.EventHandler(this.ToolStrip_Comment_Click);
			// 
			// button_Copy
			// 
			this.button_Copy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Copy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Copy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Copy.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_copy_comments_16;
			this.button_Copy.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Copy.Name = "button_Copy";
			this.button_Copy.Size = new System.Drawing.Size(23, 25);
			this.button_Copy.Text = "Copy (Ctrl+C)";
			this.button_Copy.Click += new System.EventHandler(this.ToolStrip_Copy_Click);
			// 
			// button_Cut
			// 
			this.button_Cut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Cut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Cut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Cut.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_cut_16;
			this.button_Cut.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Cut.Name = "button_Cut";
			this.button_Cut.Size = new System.Drawing.Size(23, 25);
			this.button_Cut.Text = "Cut (Ctrl+X)";
			this.button_Cut.Click += new System.EventHandler(this.ToolStrip_Cut_Click);
			// 
			// button_EditLanguages
			// 
			this.button_EditLanguages.Location = new System.Drawing.Point(99, 28);
			this.button_EditLanguages.Margin = new System.Windows.Forms.Padding(1, 3, 3, 0);
			this.button_EditLanguages.Name = "button_EditLanguages";
			this.button_EditLanguages.Size = new System.Drawing.Size(94, 23);
			this.button_EditLanguages.TabIndex = 1;
			this.button_EditLanguages.Text = "Edit Strings";
			this.button_EditLanguages.Click += new System.EventHandler(this.FileList_EditStrings_Click);
			// 
			// button_EditScript
			// 
			this.button_EditScript.Location = new System.Drawing.Point(4, 28);
			this.button_EditScript.Margin = new System.Windows.Forms.Padding(3, 3, 0, 0);
			this.button_EditScript.Name = "button_EditScript";
			this.button_EditScript.Size = new System.Drawing.Size(94, 23);
			this.button_EditScript.TabIndex = 0;
			this.button_EditScript.Text = "Edit Script File";
			this.button_EditScript.Click += new System.EventHandler(this.FileList_EditScript_Click);
			// 
			// button_NextBookmark
			// 
			this.button_NextBookmark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_NextBookmark.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_NextBookmark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_NextBookmark.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_nextbookmark_16;
			this.button_NextBookmark.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_NextBookmark.Name = "button_NextBookmark";
			this.button_NextBookmark.Size = new System.Drawing.Size(23, 25);
			this.button_NextBookmark.Text = "Next Bookmark (Ctrl+Right)";
			this.button_NextBookmark.Click += new System.EventHandler(this.ToolStrip_NextBookmark_Click);
			// 
			// button_OpenInExplorer
			// 
			this.button_OpenInExplorer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.button_OpenInExplorer.Location = new System.Drawing.Point(4, 286);
			this.button_OpenInExplorer.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.button_OpenInExplorer.Name = "button_OpenInExplorer";
			this.button_OpenInExplorer.Size = new System.Drawing.Size(190, 23);
			this.button_OpenInExplorer.TabIndex = 3;
			this.button_OpenInExplorer.Text = "Open in File Explorer";
			this.button_OpenInExplorer.Click += new System.EventHandler(this.FileList_OpenInExplorer_Click);
			// 
			// button_Paste
			// 
			this.button_Paste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Paste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Paste.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Paste.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_clipboard_16;
			this.button_Paste.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Paste.Name = "button_Paste";
			this.button_Paste.Size = new System.Drawing.Size(23, 25);
			this.button_Paste.Text = "Paste (Ctrl+V)";
			this.button_Paste.Click += new System.EventHandler(this.ToolStrip_Paste_Click);
			// 
			// button_PrevBookmark
			// 
			this.button_PrevBookmark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_PrevBookmark.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_PrevBookmark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_PrevBookmark.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_prevbookmark_16;
			this.button_PrevBookmark.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_PrevBookmark.Name = "button_PrevBookmark";
			this.button_PrevBookmark.Size = new System.Drawing.Size(23, 25);
			this.button_PrevBookmark.Text = "Previous Bookmark (Ctrl+Left)";
			this.button_PrevBookmark.Click += new System.EventHandler(this.ToolStrip_PrevBookmark_Click);
			// 
			// button_Redo
			// 
			this.button_Redo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Redo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Redo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Redo.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_redo_16;
			this.button_Redo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Redo.Name = "button_Redo";
			this.button_Redo.Size = new System.Drawing.Size(23, 25);
			this.button_Redo.Text = "Redo (Ctrl+Y)";
			this.button_Redo.Click += new System.EventHandler(this.ToolStrip_Redo_Click);
			// 
			// button_ResetZoom
			// 
			this.button_ResetZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_ResetZoom.Location = new System.Drawing.Point(807, 577);
			this.button_ResetZoom.Name = "button_ResetZoom";
			this.button_ResetZoom.Size = new System.Drawing.Size(75, 20);
			this.button_ResetZoom.TabIndex = 1;
			this.button_ResetZoom.Text = "Reset zoom";
			this.button_ResetZoom.Visible = false;
			this.button_ResetZoom.Click += new System.EventHandler(this.StatusStrip_ResetZoom_Click);
			// 
			// button_Save
			// 
			this.button_Save.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Save.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Save.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_Save_16;
			this.button_Save.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Save.Name = "button_Save";
			this.button_Save.Size = new System.Drawing.Size(23, 25);
			this.button_Save.Text = "Save (Ctrl+S)";
			this.button_Save.Click += new System.EventHandler(this.ToolStrip_Save_Click);
			// 
			// button_SaveAll
			// 
			this.button_SaveAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_SaveAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_SaveAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_SaveAll.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_SaveAll_16;
			this.button_SaveAll.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_SaveAll.Name = "button_SaveAll";
			this.button_SaveAll.Size = new System.Drawing.Size(23, 25);
			this.button_SaveAll.Text = "Save All (Ctrl+Shift+S)";
			this.button_SaveAll.Click += new System.EventHandler(this.ToolStrip_SaveAll_Click);
			// 
			// button_ToggleBookmark
			// 
			this.button_ToggleBookmark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_ToggleBookmark.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_ToggleBookmark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_ToggleBookmark.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_bookmark_16;
			this.button_ToggleBookmark.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_ToggleBookmark.Name = "button_ToggleBookmark";
			this.button_ToggleBookmark.Size = new System.Drawing.Size(23, 25);
			this.button_ToggleBookmark.Text = "Toggle Bookmark (Ctrl+B)";
			this.button_ToggleBookmark.Click += new System.EventHandler(this.ToolStrip_ToggleBookmark_Click);
			// 
			// button_Uncomment
			// 
			this.button_Uncomment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Uncomment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Uncomment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Uncomment.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_uncomment_16;
			this.button_Uncomment.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Uncomment.Name = "button_Uncomment";
			this.button_Uncomment.Size = new System.Drawing.Size(23, 25);
			this.button_Uncomment.Text = "Uncomment Lines (Ctrl+Shift+U)";
			this.button_Uncomment.Click += new System.EventHandler(this.ToolStrip_Uncomment_Click);
			// 
			// button_Undo
			// 
			this.button_Undo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Undo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Undo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Undo.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_undo_16;
			this.button_Undo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Undo.Name = "button_Undo";
			this.button_Undo.Size = new System.Drawing.Size(23, 25);
			this.button_Undo.Text = "Undo (Ctrl+Z)";
			this.button_Undo.Click += new System.EventHandler(this.ToolStrip_Undo_Click);
			// 
			// contextMenu_TextBox
			// 
			this.contextMenu_TextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu_TextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu_TextBox.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenuItem_Cut,
            this.contextMenuItem_Copy,
            this.contextMenuItem_Paste,
            this.separator_12,
            this.contextMenuItem_Comment,
            this.contextMenuItem_Uncomment,
            this.separator_13,
            this.contextMenuItem_ToggleBookmark});
			this.contextMenu_TextBox.Name = "editorContextMenu";
			this.contextMenu_TextBox.Size = new System.Drawing.Size(180, 150);
			// 
			// contextMenuItem_Cut
			// 
			this.contextMenuItem_Cut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Cut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Cut.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_cut_16;
			this.contextMenuItem_Cut.Name = "contextMenuItem_Cut";
			this.contextMenuItem_Cut.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Cut.Text = "Cut";
			this.contextMenuItem_Cut.Click += new System.EventHandler(this.ContextMenu_Cut_Click);
			// 
			// contextMenuItem_Copy
			// 
			this.contextMenuItem_Copy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Copy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Copy.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_copy_comments_16;
			this.contextMenuItem_Copy.Name = "contextMenuItem_Copy";
			this.contextMenuItem_Copy.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Copy.Text = "Copy";
			this.contextMenuItem_Copy.Click += new System.EventHandler(this.ContextMenu_Copy_Click);
			// 
			// contextMenuItem_Paste
			// 
			this.contextMenuItem_Paste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Paste.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Paste.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_clipboard_16;
			this.contextMenuItem_Paste.Name = "contextMenuItem_Paste";
			this.contextMenuItem_Paste.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Paste.Text = "Paste";
			this.contextMenuItem_Paste.Click += new System.EventHandler(this.ContextMenu_Paste_Click);
			// 
			// separator_12
			// 
			this.separator_12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_12.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_12.Name = "separator_12";
			this.separator_12.Size = new System.Drawing.Size(176, 6);
			// 
			// contextMenuItem_Comment
			// 
			this.contextMenuItem_Comment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Comment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Comment.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_comment_16;
			this.contextMenuItem_Comment.Name = "contextMenuItem_Comment";
			this.contextMenuItem_Comment.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Comment.Text = "Comment out Lines";
			this.contextMenuItem_Comment.Click += new System.EventHandler(this.ContextMenu_Comment_Click);
			// 
			// contextMenuItem_Uncomment
			// 
			this.contextMenuItem_Uncomment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Uncomment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Uncomment.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_uncomment_16;
			this.contextMenuItem_Uncomment.Name = "contextMenuItem_Uncomment";
			this.contextMenuItem_Uncomment.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Uncomment.Text = "Uncomment Lines";
			this.contextMenuItem_Uncomment.Click += new System.EventHandler(this.ContextMenu_Uncomment_Click);
			// 
			// separator_13
			// 
			this.separator_13.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_13.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_13.Name = "separator_13";
			this.separator_13.Size = new System.Drawing.Size(176, 6);
			// 
			// contextMenuItem_ToggleBookmark
			// 
			this.contextMenuItem_ToggleBookmark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_ToggleBookmark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_ToggleBookmark.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_bookmark_16;
			this.contextMenuItem_ToggleBookmark.Name = "contextMenuItem_ToggleBookmark";
			this.contextMenuItem_ToggleBookmark.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_ToggleBookmark.Text = "Toggle Bookmark";
			this.contextMenuItem_ToggleBookmark.Click += new System.EventHandler(this.ContextMenu_ToggleBookmark_Click);
			// 
			// inputTimer
			// 
			this.inputTimer.Interval = 3000;
			this.inputTimer.Tick += new System.EventHandler(this.inputTimer_Tick);
			// 
			// label_ColNumber
			// 
			this.label_ColNumber.Name = "label_ColNumber";
			this.label_ColNumber.Size = new System.Drawing.Size(62, 15);
			this.label_ColNumber.Text = "Column: 0";
			// 
			// label_LineNumber
			// 
			this.label_LineNumber.Name = "label_LineNumber";
			this.label_LineNumber.Size = new System.Drawing.Size(41, 15);
			this.label_LineNumber.Text = "Line: 0";
			// 
			// label_SelectedChars
			// 
			this.label_SelectedChars.Name = "label_SelectedChars";
			this.label_SelectedChars.Size = new System.Drawing.Size(63, 15);
			this.label_SelectedChars.Text = "Selected: 0";
			// 
			// label_Zoom
			// 
			this.label_Zoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.label_Zoom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Zoom.Location = new System.Drawing.Point(888, 577);
			this.label_Zoom.Margin = new System.Windows.Forms.Padding(3, 3, 6, 3);
			this.label_Zoom.Name = "label_Zoom";
			this.label_Zoom.Size = new System.Drawing.Size(66, 20);
			this.label_Zoom.TabIndex = 0;
			this.label_Zoom.Text = "Zoom: 100%";
			this.label_Zoom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// menu_Edit
			// 
			this.menu_Edit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menu_Edit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_Undo,
            this.menuItem_Redo,
            this.separator_01,
            this.menuItem_Cut,
            this.menuItem_Copy,
            this.menuItem_Paste,
            this.separator_02,
            this.menuItem_Find,
            this.menuItem_Replace,
            this.separator_03,
            this.menuItem_SelectAll});
			this.menu_Edit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menu_Edit.Name = "menu_Edit";
			this.menu_Edit.Size = new System.Drawing.Size(39, 20);
			this.menu_Edit.Text = "&Edit";
			// 
			// menuItem_Undo
			// 
			this.menuItem_Undo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Undo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Undo.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_undo_16;
			this.menuItem_Undo.Name = "menuItem_Undo";
			this.menuItem_Undo.ShortcutKeyDisplayString = "Ctrl+Z";
			this.menuItem_Undo.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Undo.Text = "&Undo";
			this.menuItem_Undo.Click += new System.EventHandler(this.Edit_Undo_Click);
			// 
			// menuItem_Redo
			// 
			this.menuItem_Redo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Redo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Redo.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_redo_16;
			this.menuItem_Redo.Name = "menuItem_Redo";
			this.menuItem_Redo.ShortcutKeyDisplayString = "Ctrl+Y";
			this.menuItem_Redo.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Redo.Text = "&Redo";
			this.menuItem_Redo.Click += new System.EventHandler(this.Edit_Redo_Click);
			// 
			// separator_01
			// 
			this.separator_01.Name = "separator_01";
			this.separator_01.Size = new System.Drawing.Size(164, 6);
			// 
			// menuItem_Cut
			// 
			this.menuItem_Cut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Cut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Cut.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_cut_16;
			this.menuItem_Cut.Name = "menuItem_Cut";
			this.menuItem_Cut.ShortcutKeyDisplayString = "Ctrl+X";
			this.menuItem_Cut.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Cut.Text = "Cu&t";
			this.menuItem_Cut.Click += new System.EventHandler(this.Edit_Cut_Click);
			// 
			// menuItem_Copy
			// 
			this.menuItem_Copy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Copy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Copy.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_copy_comments_16;
			this.menuItem_Copy.Name = "menuItem_Copy";
			this.menuItem_Copy.ShortcutKeyDisplayString = "Ctrl+C";
			this.menuItem_Copy.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Copy.Text = "&Copy";
			this.menuItem_Copy.Click += new System.EventHandler(this.Edit_Copy_Click);
			// 
			// menuItem_Paste
			// 
			this.menuItem_Paste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Paste.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Paste.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_clipboard_16;
			this.menuItem_Paste.Name = "menuItem_Paste";
			this.menuItem_Paste.ShortcutKeyDisplayString = "Ctrl+V";
			this.menuItem_Paste.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Paste.Text = "&Paste";
			this.menuItem_Paste.Click += new System.EventHandler(this.Edit_Paste_Click);
			// 
			// separator_02
			// 
			this.separator_02.Name = "separator_02";
			this.separator_02.Size = new System.Drawing.Size(164, 6);
			// 
			// menuItem_Find
			// 
			this.menuItem_Find.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Find.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Find.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_search_16;
			this.menuItem_Find.Name = "menuItem_Find";
			this.menuItem_Find.ShortcutKeyDisplayString = "Ctrl+F";
			this.menuItem_Find.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Find.Text = "&Find...";
			this.menuItem_Find.Click += new System.EventHandler(this.Edit_Find_Click);
			// 
			// menuItem_Replace
			// 
			this.menuItem_Replace.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Replace.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Replace.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_copy_16;
			this.menuItem_Replace.Name = "menuItem_Replace";
			this.menuItem_Replace.ShortcutKeyDisplayString = "Ctrl+H";
			this.menuItem_Replace.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Replace.Text = "&Replace...";
			this.menuItem_Replace.Click += new System.EventHandler(this.Edit_Replace_Click);
			// 
			// separator_03
			// 
			this.separator_03.Name = "separator_03";
			this.separator_03.Size = new System.Drawing.Size(164, 6);
			// 
			// menuItem_SelectAll
			// 
			this.menuItem_SelectAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_SelectAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_SelectAll.Name = "menuItem_SelectAll";
			this.menuItem_SelectAll.ShortcutKeyDisplayString = "Ctrl+A";
			this.menuItem_SelectAll.Size = new System.Drawing.Size(167, 22);
			this.menuItem_SelectAll.Text = "Select &All";
			this.menuItem_SelectAll.Click += new System.EventHandler(this.Edit_SelectAll_Click);
			// 
			// menu_File
			// 
			this.menu_File.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menu_File.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_Save,
            this.menuItem_SaveAll,
            this.separator_16,
            this.menuItem_Build});
			this.menu_File.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menu_File.Name = "menu_File";
			this.menu_File.Size = new System.Drawing.Size(37, 20);
			this.menu_File.Text = "&File";
			// 
			// menuItem_Save
			// 
			this.menuItem_Save.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Save.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Save.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_Save_16;
			this.menuItem_Save.Name = "menuItem_Save";
			this.menuItem_Save.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.menuItem_Save.Size = new System.Drawing.Size(187, 22);
			this.menuItem_Save.Text = "&Save";
			this.menuItem_Save.Click += new System.EventHandler(this.File_Save_Click);
			// 
			// menuItem_SaveAll
			// 
			this.menuItem_SaveAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_SaveAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_SaveAll.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_SaveAll_16;
			this.menuItem_SaveAll.Name = "menuItem_SaveAll";
			this.menuItem_SaveAll.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.S)));
			this.menuItem_SaveAll.Size = new System.Drawing.Size(187, 22);
			this.menuItem_SaveAll.Text = "Save &All";
			this.menuItem_SaveAll.Click += new System.EventHandler(this.File_SaveAll_Click);
			// 
			// separator_16
			// 
			this.separator_16.Name = "separator_16";
			this.separator_16.Size = new System.Drawing.Size(184, 6);
			// 
			// menuItem_Build
			// 
			this.menuItem_Build.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Build.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Build.Image = global::TombIDE.ScriptEditor.Properties.Resources.actions_play_16;
			this.menuItem_Build.Name = "menuItem_Build";
			this.menuItem_Build.ShortcutKeys = System.Windows.Forms.Keys.F9;
			this.menuItem_Build.Size = new System.Drawing.Size(187, 22);
			this.menuItem_Build.Text = "&Build Script";
			this.menuItem_Build.Click += new System.EventHandler(this.File_Build_Click);
			// 
			// menu_Help
			// 
			this.menu_Help.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menu_Help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_About});
			this.menu_Help.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menu_Help.Name = "menu_Help";
			this.menu_Help.Size = new System.Drawing.Size(44, 20);
			this.menu_Help.Text = "&Help";
			// 
			// menuItem_About
			// 
			this.menuItem_About.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_About.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_About.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_AboutIcon_16;
			this.menuItem_About.Name = "menuItem_About";
			this.menuItem_About.ShortcutKeys = System.Windows.Forms.Keys.F1;
			this.menuItem_About.Size = new System.Drawing.Size(135, 22);
			this.menuItem_About.Text = "&About...";
			this.menuItem_About.Click += new System.EventHandler(this.Help_About_Click);
			// 
			// menu_Tools
			// 
			this.menu_Tools.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menu_Tools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_Reindent,
            this.menuItem_Trim,
            this.separator_04,
            this.menuItem_Comment,
            this.menuItem_Uncomment,
            this.separator_05,
            this.menuItem_ToggleBookmark,
            this.menuItem_PrevBookmark,
            this.menuItem_NextBookmark,
            this.menuItem_ClearBookmarks,
            this.separator_06,
            this.menuItem_Settings});
			this.menu_Tools.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menu_Tools.Name = "menu_Tools";
			this.menu_Tools.Size = new System.Drawing.Size(46, 20);
			this.menu_Tools.Text = "&Tools";
			// 
			// menuItem_Reindent
			// 
			this.menuItem_Reindent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Reindent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Reindent.Name = "menuItem_Reindent";
			this.menuItem_Reindent.ShortcutKeyDisplayString = "";
			this.menuItem_Reindent.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
			this.menuItem_Reindent.Size = new System.Drawing.Size(300, 22);
			this.menuItem_Reindent.Text = "&Reindent Script";
			this.menuItem_Reindent.Click += new System.EventHandler(this.Tools_Reindent_Click);
			// 
			// menuItem_Trim
			// 
			this.menuItem_Trim.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Trim.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Trim.Name = "menuItem_Trim";
			this.menuItem_Trim.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.R)));
			this.menuItem_Trim.Size = new System.Drawing.Size(300, 22);
			this.menuItem_Trim.Text = "&Trim Ending Whitespace";
			this.menuItem_Trim.Click += new System.EventHandler(this.Tools_Trim_Click);
			// 
			// separator_04
			// 
			this.separator_04.Name = "separator_04";
			this.separator_04.Size = new System.Drawing.Size(297, 6);
			// 
			// menuItem_Comment
			// 
			this.menuItem_Comment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Comment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Comment.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_comment_16;
			this.menuItem_Comment.Name = "menuItem_Comment";
			this.menuItem_Comment.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.C)));
			this.menuItem_Comment.Size = new System.Drawing.Size(300, 22);
			this.menuItem_Comment.Text = "&Comment out Selected Lines";
			this.menuItem_Comment.Click += new System.EventHandler(this.Tools_Comment_Click);
			// 
			// menuItem_Uncomment
			// 
			this.menuItem_Uncomment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Uncomment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Uncomment.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_uncomment_16;
			this.menuItem_Uncomment.Name = "menuItem_Uncomment";
			this.menuItem_Uncomment.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.U)));
			this.menuItem_Uncomment.Size = new System.Drawing.Size(300, 22);
			this.menuItem_Uncomment.Text = "&Uncomment Selected Lines";
			this.menuItem_Uncomment.Click += new System.EventHandler(this.Tools_Uncomment_Click);
			// 
			// separator_05
			// 
			this.separator_05.Name = "separator_05";
			this.separator_05.Size = new System.Drawing.Size(297, 6);
			// 
			// menuItem_ToggleBookmark
			// 
			this.menuItem_ToggleBookmark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ToggleBookmark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_ToggleBookmark.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_bookmark_16;
			this.menuItem_ToggleBookmark.Name = "menuItem_ToggleBookmark";
			this.menuItem_ToggleBookmark.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
			this.menuItem_ToggleBookmark.Size = new System.Drawing.Size(300, 22);
			this.menuItem_ToggleBookmark.Text = "Toggle &Bookmark";
			this.menuItem_ToggleBookmark.Click += new System.EventHandler(this.Tools_ToggleBookmark_Click);
			// 
			// menuItem_PrevBookmark
			// 
			this.menuItem_PrevBookmark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_PrevBookmark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_PrevBookmark.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_prevbookmark_16;
			this.menuItem_PrevBookmark.Name = "menuItem_PrevBookmark";
			this.menuItem_PrevBookmark.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Left)));
			this.menuItem_PrevBookmark.Size = new System.Drawing.Size(300, 22);
			this.menuItem_PrevBookmark.Text = "Go To &Previous Bookmark";
			this.menuItem_PrevBookmark.Click += new System.EventHandler(this.Tools_PrevBookmark_Click);
			// 
			// menuItem_NextBookmark
			// 
			this.menuItem_NextBookmark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_NextBookmark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_NextBookmark.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_nextbookmark_16;
			this.menuItem_NextBookmark.Name = "menuItem_NextBookmark";
			this.menuItem_NextBookmark.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right)));
			this.menuItem_NextBookmark.Size = new System.Drawing.Size(300, 22);
			this.menuItem_NextBookmark.Text = "Go To &Next Bookmark";
			this.menuItem_NextBookmark.Click += new System.EventHandler(this.Tools_NextBookmark_Click);
			// 
			// menuItem_ClearBookmarks
			// 
			this.menuItem_ClearBookmarks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ClearBookmarks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_ClearBookmarks.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_clearbookmarks_16;
			this.menuItem_ClearBookmarks.Name = "menuItem_ClearBookmarks";
			this.menuItem_ClearBookmarks.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.B)));
			this.menuItem_ClearBookmarks.Size = new System.Drawing.Size(300, 22);
			this.menuItem_ClearBookmarks.Text = "Clear &all Bookmarks";
			this.menuItem_ClearBookmarks.Click += new System.EventHandler(this.Tools_ClearBookmarks_Click);
			// 
			// separator_06
			// 
			this.separator_06.Name = "separator_06";
			this.separator_06.Size = new System.Drawing.Size(297, 6);
			// 
			// menuItem_Settings
			// 
			this.menuItem_Settings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Settings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Settings.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_settings_16;
			this.menuItem_Settings.Name = "menuItem_Settings";
			this.menuItem_Settings.Size = new System.Drawing.Size(300, 22);
			this.menuItem_Settings.Text = "&Settings...";
			this.menuItem_Settings.Click += new System.EventHandler(this.Tools_Settings_Click);
			// 
			// menu_View
			// 
			this.menu_View.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menu_View.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_ObjBrowser,
            this.menuItem_FileList,
            this.menuItem_InfoBox,
            this.separator_14,
            this.menuItem_ToolStrip,
            this.menuItem_StatusStrip,
            this.separator_15,
            this.menuItem_LineNumbers,
            this.menuItem_ToolTips});
			this.menu_View.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menu_View.Name = "menu_View";
			this.menu_View.Size = new System.Drawing.Size(44, 20);
			this.menu_View.Text = "&View";
			// 
			// menuItem_ObjBrowser
			// 
			this.menuItem_ObjBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ObjBrowser.Checked = true;
			this.menuItem_ObjBrowser.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_ObjBrowser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_ObjBrowser.Name = "menuItem_ObjBrowser";
			this.menuItem_ObjBrowser.Size = new System.Drawing.Size(160, 22);
			this.menuItem_ObjBrowser.Text = "&Object Browser";
			this.menuItem_ObjBrowser.Click += new System.EventHandler(this.View_ObjBrowser_Click);
			// 
			// menuItem_FileList
			// 
			this.menuItem_FileList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_FileList.Checked = true;
			this.menuItem_FileList.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_FileList.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_FileList.Name = "menuItem_FileList";
			this.menuItem_FileList.Size = new System.Drawing.Size(160, 22);
			this.menuItem_FileList.Text = "&File List";
			this.menuItem_FileList.Click += new System.EventHandler(this.View_FileList_Click);
			// 
			// menuItem_InfoBox
			// 
			this.menuItem_InfoBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_InfoBox.Checked = true;
			this.menuItem_InfoBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_InfoBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_InfoBox.Name = "menuItem_InfoBox";
			this.menuItem_InfoBox.Size = new System.Drawing.Size(160, 22);
			this.menuItem_InfoBox.Text = "&Information Box";
			this.menuItem_InfoBox.Click += new System.EventHandler(this.View_InfoBox_Click);
			// 
			// separator_14
			// 
			this.separator_14.Name = "separator_14";
			this.separator_14.Size = new System.Drawing.Size(157, 6);
			// 
			// menuItem_ToolStrip
			// 
			this.menuItem_ToolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ToolStrip.Checked = true;
			this.menuItem_ToolStrip.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_ToolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_ToolStrip.Name = "menuItem_ToolStrip";
			this.menuItem_ToolStrip.Size = new System.Drawing.Size(160, 22);
			this.menuItem_ToolStrip.Text = "&Tool Strip";
			this.menuItem_ToolStrip.Click += new System.EventHandler(this.View_ToolStrip_Click);
			// 
			// menuItem_StatusStrip
			// 
			this.menuItem_StatusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_StatusStrip.Checked = true;
			this.menuItem_StatusStrip.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_StatusStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_StatusStrip.Name = "menuItem_StatusStrip";
			this.menuItem_StatusStrip.Size = new System.Drawing.Size(160, 22);
			this.menuItem_StatusStrip.Text = "&Status Strip";
			this.menuItem_StatusStrip.Click += new System.EventHandler(this.View_StatusStrip_Click);
			// 
			// separator_15
			// 
			this.separator_15.Name = "separator_15";
			this.separator_15.Size = new System.Drawing.Size(157, 6);
			// 
			// menuItem_LineNumbers
			// 
			this.menuItem_LineNumbers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_LineNumbers.Checked = true;
			this.menuItem_LineNumbers.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_LineNumbers.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_LineNumbers.Name = "menuItem_LineNumbers";
			this.menuItem_LineNumbers.Size = new System.Drawing.Size(160, 22);
			this.menuItem_LineNumbers.Text = "&Line Numbers";
			this.menuItem_LineNumbers.Click += new System.EventHandler(this.View_LineNumbers_Click);
			// 
			// menuItem_ToolTips
			// 
			this.menuItem_ToolTips.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ToolTips.Checked = true;
			this.menuItem_ToolTips.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_ToolTips.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_ToolTips.Name = "menuItem_ToolTips";
			this.menuItem_ToolTips.Size = new System.Drawing.Size(160, 22);
			this.menuItem_ToolTips.Text = "T&ool Tips";
			this.menuItem_ToolTips.Click += new System.EventHandler(this.View_ToolTips_Click);
			// 
			// menuStrip
			// 
			this.menuStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_File,
            this.menu_Edit,
            this.menu_Tools,
            this.menu_View,
            this.menu_Help});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
			this.menuStrip.Size = new System.Drawing.Size(960, 24);
			this.menuStrip.TabIndex = 0;
			// 
			// panel_Editor
			// 
			this.panel_Editor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Editor.Controls.Add(this.tabControl_Editor);
			this.panel_Editor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_Editor.Location = new System.Drawing.Point(205, 52);
			this.panel_Editor.Name = "panel_Editor";
			this.panel_Editor.Size = new System.Drawing.Size(550, 315);
			this.panel_Editor.TabIndex = 2;
			// 
			// tabControl_Editor
			// 
			this.tabControl_Editor.AllowDrop = true;
			this.tabControl_Editor.DisplayStyle = System.Windows.Forms.TabStyle.Dark;
			// 
			// 
			// 
			this.tabControl_Editor.DisplayStyleProvider.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl_Editor.DisplayStyleProvider.BorderColorHot = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl_Editor.DisplayStyleProvider.BorderColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl_Editor.DisplayStyleProvider.CloserColor = System.Drawing.Color.White;
			this.tabControl_Editor.DisplayStyleProvider.CloserColorActive = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(196)))), ((int)(((byte)(232)))));
			this.tabControl_Editor.DisplayStyleProvider.FocusTrack = false;
			this.tabControl_Editor.DisplayStyleProvider.HotTrack = false;
			this.tabControl_Editor.DisplayStyleProvider.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tabControl_Editor.DisplayStyleProvider.Opacity = 1F;
			this.tabControl_Editor.DisplayStyleProvider.Overlap = 0;
			this.tabControl_Editor.DisplayStyleProvider.Padding = new System.Drawing.Point(6, 4);
			this.tabControl_Editor.DisplayStyleProvider.Radius = 10;
			this.tabControl_Editor.DisplayStyleProvider.ShowTabCloser = true;
			this.tabControl_Editor.DisplayStyleProvider.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.tabControl_Editor.DisplayStyleProvider.TextColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl_Editor.DisplayStyleProvider.TextColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(196)))), ((int)(((byte)(232)))));
			this.tabControl_Editor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl_Editor.Location = new System.Drawing.Point(0, 0);
			this.tabControl_Editor.Name = "tabControl_Editor";
			this.tabControl_Editor.SelectedIndex = 0;
			this.tabControl_Editor.Size = new System.Drawing.Size(548, 313);
			this.tabControl_Editor.TabIndex = 0;
			this.tabControl_Editor.TabClosing += new System.EventHandler<System.Windows.Forms.TabControlCancelEventArgs>(this.Editor_TabControl_TabClosing);
			this.tabControl_Editor.SelectedIndexChanged += new System.EventHandler(this.Editor_TabControl_SelectedIndexChanged);
			this.tabControl_Editor.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.Editor_TabControl_Selecting);
			this.tabControl_Editor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Editor_TabControl_MouseClick);
			// 
			// referenceBrowser
			// 
			this.referenceBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.referenceBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.referenceBrowser.Location = new System.Drawing.Point(0, 0);
			this.referenceBrowser.Name = "referenceBrowser";
			this.referenceBrowser.Size = new System.Drawing.Size(948, 143);
			this.referenceBrowser.TabIndex = 0;
			// 
			// richTextBox_Logs
			// 
			this.richTextBox_Logs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.richTextBox_Logs.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox_Logs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox_Logs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.richTextBox_Logs.ForeColor = System.Drawing.Color.Gainsboro;
			this.richTextBox_Logs.Location = new System.Drawing.Point(0, 0);
			this.richTextBox_Logs.Name = "richTextBox_Logs";
			this.richTextBox_Logs.ReadOnly = true;
			this.richTextBox_Logs.Size = new System.Drawing.Size(948, 143);
			this.richTextBox_Logs.TabIndex = 0;
			this.richTextBox_Logs.Text = "";
			// 
			// scriptFolderWatcher
			// 
			this.scriptFolderWatcher.EnableRaisingEvents = true;
			this.scriptFolderWatcher.IncludeSubdirectories = true;
			this.scriptFolderWatcher.SynchronizingObject = this;
			this.scriptFolderWatcher.Changed += new System.IO.FileSystemEventHandler(this.FolderWatcher_Changed);
			this.scriptFolderWatcher.Created += new System.IO.FileSystemEventHandler(this.FolderWatcher_Changed);
			this.scriptFolderWatcher.Deleted += new System.IO.FileSystemEventHandler(this.FolderWatcher_Changed);
			this.scriptFolderWatcher.Renamed += new System.IO.RenamedEventHandler(this.FolderWatcher_Renamed);
			// 
			// sectionPanel_Files
			// 
			this.sectionPanel_Files.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel_Files.Controls.Add(this.button_OpenInExplorer);
			this.sectionPanel_Files.Controls.Add(this.button_EditLanguages);
			this.sectionPanel_Files.Controls.Add(this.button_EditScript);
			this.sectionPanel_Files.Controls.Add(this.treeView_Files);
			this.sectionPanel_Files.Dock = System.Windows.Forms.DockStyle.Right;
			this.sectionPanel_Files.Location = new System.Drawing.Point(760, 52);
			this.sectionPanel_Files.Name = "sectionPanel_Files";
			this.sectionPanel_Files.SectionHeader = "Files";
			this.sectionPanel_Files.Size = new System.Drawing.Size(200, 315);
			this.sectionPanel_Files.TabIndex = 4;
			this.sectionPanel_Files.Text = "Project Explorer";
			// 
			// treeView_Files
			// 
			this.treeView_Files.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeView_Files.Location = new System.Drawing.Point(4, 54);
			this.treeView_Files.MaxDragChange = 20;
			this.treeView_Files.Name = "treeView_Files";
			this.treeView_Files.ShowIcons = true;
			this.treeView_Files.Size = new System.Drawing.Size(190, 229);
			this.treeView_Files.TabIndex = 2;
			this.treeView_Files.DoubleClick += new System.EventHandler(this.FileList_TreeView_DoubleClick);
			// 
			// sectionPanel_InfoBox
			// 
			this.sectionPanel_InfoBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel_InfoBox.Controls.Add(this.tabControl_Info);
			this.sectionPanel_InfoBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.sectionPanel_InfoBox.Location = new System.Drawing.Point(0, 372);
			this.sectionPanel_InfoBox.Name = "sectionPanel_InfoBox";
			this.sectionPanel_InfoBox.SectionHeader = "Information Box";
			this.sectionPanel_InfoBox.Size = new System.Drawing.Size(960, 200);
			this.sectionPanel_InfoBox.TabIndex = 5;
			// 
			// tabControl_Info
			// 
			this.tabControl_Info.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabControl_Info.Controls.Add(this.tabPage_RefBrowser);
			this.tabControl_Info.Controls.Add(this.tabPage_CompilerLogs);
			this.tabControl_Info.DisplayStyle = System.Windows.Forms.TabStyle.Dark;
			// 
			// 
			// 
			this.tabControl_Info.DisplayStyleProvider.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl_Info.DisplayStyleProvider.BorderColorHot = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl_Info.DisplayStyleProvider.BorderColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl_Info.DisplayStyleProvider.CloserColor = System.Drawing.Color.White;
			this.tabControl_Info.DisplayStyleProvider.CloserColorActive = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(196)))), ((int)(((byte)(232)))));
			this.tabControl_Info.DisplayStyleProvider.FocusTrack = false;
			this.tabControl_Info.DisplayStyleProvider.HotTrack = false;
			this.tabControl_Info.DisplayStyleProvider.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tabControl_Info.DisplayStyleProvider.Opacity = 1F;
			this.tabControl_Info.DisplayStyleProvider.Overlap = 0;
			this.tabControl_Info.DisplayStyleProvider.Padding = new System.Drawing.Point(6, 4);
			this.tabControl_Info.DisplayStyleProvider.Radius = 10;
			this.tabControl_Info.DisplayStyleProvider.ShowTabCloser = false;
			this.tabControl_Info.DisplayStyleProvider.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
			this.tabControl_Info.DisplayStyleProvider.TextColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
			this.tabControl_Info.DisplayStyleProvider.TextColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(196)))), ((int)(((byte)(232)))));
			this.tabControl_Info.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl_Info.Location = new System.Drawing.Point(1, 25);
			this.tabControl_Info.Name = "tabControl_Info";
			this.tabControl_Info.SelectedIndex = 0;
			this.tabControl_Info.Size = new System.Drawing.Size(956, 172);
			this.tabControl_Info.TabIndex = 0;
			// 
			// tabPage_RefBrowser
			// 
			this.tabPage_RefBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_RefBrowser.Controls.Add(this.referenceBrowser);
			this.tabPage_RefBrowser.Location = new System.Drawing.Point(4, 4);
			this.tabPage_RefBrowser.Name = "tabPage_RefBrowser";
			this.tabPage_RefBrowser.Size = new System.Drawing.Size(948, 143);
			this.tabPage_RefBrowser.TabIndex = 0;
			this.tabPage_RefBrowser.Text = "Reference Browser";
			// 
			// tabPage_CompilerLogs
			// 
			this.tabPage_CompilerLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_CompilerLogs.Controls.Add(this.richTextBox_Logs);
			this.tabPage_CompilerLogs.Location = new System.Drawing.Point(4, 4);
			this.tabPage_CompilerLogs.Name = "tabPage_CompilerLogs";
			this.tabPage_CompilerLogs.Size = new System.Drawing.Size(948, 143);
			this.tabPage_CompilerLogs.TabIndex = 1;
			this.tabPage_CompilerLogs.Text = "Compiler Logs";
			// 
			// sectionPanel_ObjBrowser
			// 
			this.sectionPanel_ObjBrowser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel_ObjBrowser.Controls.Add(this.treeView_Objects);
			this.sectionPanel_ObjBrowser.Controls.Add(this.textBox_SearchObj);
			this.sectionPanel_ObjBrowser.Dock = System.Windows.Forms.DockStyle.Left;
			this.sectionPanel_ObjBrowser.Location = new System.Drawing.Point(0, 52);
			this.sectionPanel_ObjBrowser.Name = "sectionPanel_ObjBrowser";
			this.sectionPanel_ObjBrowser.SectionHeader = "Object Browser";
			this.sectionPanel_ObjBrowser.Size = new System.Drawing.Size(200, 315);
			this.sectionPanel_ObjBrowser.TabIndex = 3;
			this.sectionPanel_ObjBrowser.Text = "Object Browser";
			// 
			// treeView_Objects
			// 
			this.treeView_Objects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeView_Objects.Location = new System.Drawing.Point(4, 51);
			this.treeView_Objects.MaxDragChange = 20;
			this.treeView_Objects.Name = "treeView_Objects";
			this.treeView_Objects.Size = new System.Drawing.Size(190, 258);
			this.treeView_Objects.TabIndex = 1;
			this.treeView_Objects.Click += new System.EventHandler(this.ObjBrowser_TreeView_Click);
			// 
			// textBox_SearchObj
			// 
			this.textBox_SearchObj.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_SearchObj.Location = new System.Drawing.Point(4, 28);
			this.textBox_SearchObj.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
			this.textBox_SearchObj.Name = "textBox_SearchObj";
			this.textBox_SearchObj.Size = new System.Drawing.Size(190, 20);
			this.textBox_SearchObj.TabIndex = 0;
			this.textBox_SearchObj.Text = "Search...";
			this.textBox_SearchObj.TextChanged += new System.EventHandler(this.ObjBrowser_Search_TextChanged);
			this.textBox_SearchObj.Enter += new System.EventHandler(this.ObjBrowser_Search_GotFocus);
			this.textBox_SearchObj.Leave += new System.EventHandler(this.ObjBrowser_Search_LostFocus);
			// 
			// separator_07
			// 
			this.separator_07.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_07.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_07.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_07.Name = "separator_07";
			this.separator_07.Size = new System.Drawing.Size(6, 28);
			// 
			// separator_08
			// 
			this.separator_08.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_08.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_08.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_08.Name = "separator_08";
			this.separator_08.Size = new System.Drawing.Size(6, 28);
			// 
			// separator_09
			// 
			this.separator_09.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_09.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_09.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_09.Name = "separator_09";
			this.separator_09.Size = new System.Drawing.Size(6, 28);
			// 
			// separator_10
			// 
			this.separator_10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_10.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_10.Name = "separator_10";
			this.separator_10.Size = new System.Drawing.Size(6, 28);
			// 
			// separator_11
			// 
			this.separator_11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_11.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_11.Name = "separator_11";
			this.separator_11.Size = new System.Drawing.Size(6, 28);
			// 
			// splitter_Bottom
			// 
			this.splitter_Bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter_Bottom.Location = new System.Drawing.Point(0, 367);
			this.splitter_Bottom.MinExtra = 400;
			this.splitter_Bottom.MinSize = 200;
			this.splitter_Bottom.Name = "splitter_Bottom";
			this.splitter_Bottom.Size = new System.Drawing.Size(960, 5);
			this.splitter_Bottom.TabIndex = 9;
			this.splitter_Bottom.TabStop = false;
			// 
			// splitter_Left
			// 
			this.splitter_Left.Location = new System.Drawing.Point(200, 52);
			this.splitter_Left.MinExtra = 400;
			this.splitter_Left.MinSize = 200;
			this.splitter_Left.Name = "splitter_Left";
			this.splitter_Left.Size = new System.Drawing.Size(5, 315);
			this.splitter_Left.TabIndex = 7;
			this.splitter_Left.TabStop = false;
			// 
			// splitter_Right
			// 
			this.splitter_Right.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter_Right.Location = new System.Drawing.Point(755, 52);
			this.splitter_Right.MinExtra = 400;
			this.splitter_Right.MinSize = 200;
			this.splitter_Right.Name = "splitter_Right";
			this.splitter_Right.Size = new System.Drawing.Size(5, 315);
			this.splitter_Right.TabIndex = 8;
			this.splitter_Right.TabStop = false;
			this.splitter_Right.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.FileList_Splitter_Moved);
			// 
			// statusStrip
			// 
			this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.statusStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.label_LineNumber,
            this.label_ColNumber,
            this.label_SelectedChars});
			this.statusStrip.Location = new System.Drawing.Point(0, 572);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
			this.statusStrip.Size = new System.Drawing.Size(960, 28);
			this.statusStrip.TabIndex = 6;
			this.statusStrip.Text = "statusStrip";
			// 
			// toolStrip
			// 
			this.toolStrip.AutoSize = false;
			this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.button_Save,
            this.button_SaveAll,
            this.separator_07,
            this.button_Undo,
            this.button_Redo,
            this.separator_08,
            this.button_Cut,
            this.button_Copy,
            this.button_Paste,
            this.separator_09,
            this.button_Comment,
            this.button_Uncomment,
            this.separator_10,
            this.button_ToggleBookmark,
            this.button_PrevBookmark,
            this.button_NextBookmark,
            this.button_ClearBookmarks,
            this.separator_11,
            this.button_Build});
			this.toolStrip.Location = new System.Drawing.Point(0, 24);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.toolStrip.Size = new System.Drawing.Size(960, 28);
			this.toolStrip.TabIndex = 1;
			// 
			// ScriptEditor
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.Controls.Add(this.panel_Editor);
			this.Controls.Add(this.splitter_Left);
			this.Controls.Add(this.sectionPanel_ObjBrowser);
			this.Controls.Add(this.splitter_Right);
			this.Controls.Add(this.sectionPanel_Files);
			this.Controls.Add(this.button_ResetZoom);
			this.Controls.Add(this.label_Zoom);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.menuStrip);
			this.Controls.Add(this.splitter_Bottom);
			this.Controls.Add(this.sectionPanel_InfoBox);
			this.Controls.Add(this.statusStrip);
			this.Name = "ScriptEditor";
			this.Size = new System.Drawing.Size(960, 600);
			this.contextMenu_TextBox.ResumeLayout(false);
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.panel_Editor.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.scriptFolderWatcher)).EndInit();
			this.sectionPanel_Files.ResumeLayout(false);
			this.sectionPanel_InfoBox.ResumeLayout(false);
			this.tabControl_Info.ResumeLayout(false);
			this.tabPage_RefBrowser.ResumeLayout(false);
			this.tabPage_CompilerLogs.ResumeLayout(false);
			this.sectionPanel_ObjBrowser.ResumeLayout(false);
			this.sectionPanel_ObjBrowser.PerformLayout();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private DarkUI.Controls.DarkButton button_EditLanguages;
		private DarkUI.Controls.DarkButton button_EditScript;
		private DarkUI.Controls.DarkButton button_OpenInExplorer;
		private DarkUI.Controls.DarkButton button_ResetZoom;
		private DarkUI.Controls.DarkContextMenu contextMenu_TextBox;
		private DarkUI.Controls.DarkLabel label_Zoom;
		private DarkUI.Controls.DarkMenuStrip menuStrip;
		private DarkUI.Controls.DarkSectionPanel sectionPanel_Files;
		private DarkUI.Controls.DarkSectionPanel sectionPanel_InfoBox;
		private DarkUI.Controls.DarkSectionPanel sectionPanel_ObjBrowser;
		private DarkUI.Controls.DarkStatusStrip statusStrip;
		private DarkUI.Controls.DarkTextBox textBox_SearchObj;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private DarkUI.Controls.DarkTreeView treeView_Files;
		private DarkUI.Controls.DarkTreeView treeView_Objects;
		private ReferenceBrowser referenceBrowser;
		private System.IO.FileSystemWatcher scriptFolderWatcher;
		private System.Windows.Forms.CustomTabControl tabControl_Editor;
		private System.Windows.Forms.CustomTabControl tabControl_Info;
		private System.Windows.Forms.Panel panel_Editor;
		private System.Windows.Forms.RichTextBox richTextBox_Logs;
		private System.Windows.Forms.Splitter splitter_Bottom;
		private System.Windows.Forms.Splitter splitter_Left;
		private System.Windows.Forms.Splitter splitter_Right;
		private System.Windows.Forms.TabPage tabPage_CompilerLogs;
		private System.Windows.Forms.TabPage tabPage_RefBrowser;
		private System.Windows.Forms.Timer inputTimer;
		private System.Windows.Forms.ToolStripButton button_Build;
		private System.Windows.Forms.ToolStripButton button_ClearBookmarks;
		private System.Windows.Forms.ToolStripButton button_Comment;
		private System.Windows.Forms.ToolStripButton button_Copy;
		private System.Windows.Forms.ToolStripButton button_Cut;
		private System.Windows.Forms.ToolStripButton button_NextBookmark;
		private System.Windows.Forms.ToolStripButton button_Paste;
		private System.Windows.Forms.ToolStripButton button_PrevBookmark;
		private System.Windows.Forms.ToolStripButton button_Redo;
		private System.Windows.Forms.ToolStripButton button_Save;
		private System.Windows.Forms.ToolStripButton button_SaveAll;
		private System.Windows.Forms.ToolStripButton button_ToggleBookmark;
		private System.Windows.Forms.ToolStripButton button_Uncomment;
		private System.Windows.Forms.ToolStripButton button_Undo;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_Comment;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_Copy;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_Cut;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_Paste;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_ToggleBookmark;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_Uncomment;
		private System.Windows.Forms.ToolStripMenuItem menu_Edit;
		private System.Windows.Forms.ToolStripMenuItem menu_File;
		private System.Windows.Forms.ToolStripMenuItem menu_Help;
		private System.Windows.Forms.ToolStripMenuItem menu_Tools;
		private System.Windows.Forms.ToolStripMenuItem menu_View;
		private System.Windows.Forms.ToolStripMenuItem menuItem_About;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Build;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ClearBookmarks;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Comment;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Copy;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Cut;
		private System.Windows.Forms.ToolStripMenuItem menuItem_FileList;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Find;
		private System.Windows.Forms.ToolStripMenuItem menuItem_InfoBox;
		private System.Windows.Forms.ToolStripMenuItem menuItem_LineNumbers;
		private System.Windows.Forms.ToolStripMenuItem menuItem_NextBookmark;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ObjBrowser;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Paste;
		private System.Windows.Forms.ToolStripMenuItem menuItem_PrevBookmark;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Redo;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Reindent;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Replace;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Save;
		private System.Windows.Forms.ToolStripMenuItem menuItem_SaveAll;
		private System.Windows.Forms.ToolStripMenuItem menuItem_SelectAll;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Settings;
		private System.Windows.Forms.ToolStripMenuItem menuItem_StatusStrip;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ToggleBookmark;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ToolStrip;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ToolTips;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Trim;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Uncomment;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Undo;
		private System.Windows.Forms.ToolStripSeparator separator_01;
		private System.Windows.Forms.ToolStripSeparator separator_02;
		private System.Windows.Forms.ToolStripSeparator separator_03;
		private System.Windows.Forms.ToolStripSeparator separator_04;
		private System.Windows.Forms.ToolStripSeparator separator_05;
		private System.Windows.Forms.ToolStripSeparator separator_06;
		private System.Windows.Forms.ToolStripSeparator separator_07;
		private System.Windows.Forms.ToolStripSeparator separator_08;
		private System.Windows.Forms.ToolStripSeparator separator_09;
		private System.Windows.Forms.ToolStripSeparator separator_10;
		private System.Windows.Forms.ToolStripSeparator separator_11;
		private System.Windows.Forms.ToolStripSeparator separator_12;
		private System.Windows.Forms.ToolStripSeparator separator_13;
		private System.Windows.Forms.ToolStripSeparator separator_14;
		private System.Windows.Forms.ToolStripSeparator separator_15;
		private System.Windows.Forms.ToolStripSeparator separator_16;
		private System.Windows.Forms.ToolStripStatusLabel label_ColNumber;
		private System.Windows.Forms.ToolStripStatusLabel label_LineNumber;
		private System.Windows.Forms.ToolStripStatusLabel label_SelectedChars;
	}
}

