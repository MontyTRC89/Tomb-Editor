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
			this.backgroundWorker_NGC = new System.ComponentModel.BackgroundWorker();
			this.button_Build = new System.Windows.Forms.ToolStripButton();
			this.button_ClearBookmarks = new System.Windows.Forms.ToolStripButton();
			this.button_Comment = new System.Windows.Forms.ToolStripButton();
			this.button_Copy = new System.Windows.Forms.ToolStripButton();
			this.button_Cut = new System.Windows.Forms.ToolStripButton();
			this.button_NewFile = new System.Windows.Forms.ToolStripButton();
			this.button_NextBookmark = new System.Windows.Forms.ToolStripButton();
			this.button_Paste = new System.Windows.Forms.ToolStripButton();
			this.button_PrevBookmark = new System.Windows.Forms.ToolStripButton();
			this.button_Redo = new System.Windows.Forms.ToolStripButton();
			this.button_ResetZoom = new DarkUI.Controls.DarkButton();
			this.button_Save = new System.Windows.Forms.ToolStripButton();
			this.button_SaveAll = new System.Windows.Forms.ToolStripButton();
			this.button_ToggleBookmark = new System.Windows.Forms.ToolStripButton();
			this.button_Uncomment = new System.Windows.Forms.ToolStripButton();
			this.button_Undo = new System.Windows.Forms.ToolStripButton();
			this.contextMenu_TextEditor = new DarkUI.Controls.DarkContextMenu();
			this.contextMenuItem_Cut = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuItem_Paste = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_18 = new System.Windows.Forms.ToolStripSeparator();
			this.contextMenuItem_Comment = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuItem_Uncomment = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_19 = new System.Windows.Forms.ToolStripSeparator();
			this.contextMenuItem_ToggleBookmark = new System.Windows.Forms.ToolStripMenuItem();
			this.fileList = new TombIDE.ScriptEditor.Controls.FileList();
			this.label_ColNumber = new System.Windows.Forms.ToolStripStatusLabel();
			this.label_LineNumber = new System.Windows.Forms.ToolStripStatusLabel();
			this.label_SelectedChars = new System.Windows.Forms.ToolStripStatusLabel();
			this.label_Zoom = new DarkUI.Controls.DarkLabel();
			this.menu_Edit = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Undo = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Redo = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_02 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Cut = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Paste = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_03 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_FindReplace = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_04 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_SelectAll = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_File = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_NewFile = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_10 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Save = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_SaveAll = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_01 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Build = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_Help = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_About = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_Options = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_NewInclude = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_ShowLogs = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_ReindentOnSave = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_08 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Settings = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_Tools = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_CheckErrors = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_05 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Reindent = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Trim = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_06 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Comment = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Uncomment = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_07 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_ToggleBookmark = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_PrevBookmark = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_NextBookmark = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_ClearBookmarks = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_View = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_ObjBrowser = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_FileList = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_InfoBox = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_09 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_ToolStrip = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_StatusStrip = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_12 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_SwapPanels = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip = new DarkUI.Controls.DarkMenuStrip();
			this.objectBrowser = new TombIDE.ScriptEditor.Controls.ObjectBrowser();
			this.panel_Editor = new System.Windows.Forms.Panel();
			this.tabControl_Editor = new System.Windows.Forms.CustomTabControl();
			this.panel_Syntax = new System.Windows.Forms.Panel();
			this.syntaxPreview = new TombIDE.ScriptEditor.Controls.SyntaxPreview();
			this.referenceBrowser = new TombIDE.ScriptEditor.Controls.ReferenceBrowser();
			this.richTextBox_Logs = new System.Windows.Forms.RichTextBox();
			this.scriptFolderWatcher = new System.IO.FileSystemWatcher();
			this.sectionPanel_InfoBox = new DarkUI.Controls.DarkSectionPanel();
			this.tabControl_Info = new System.Windows.Forms.CustomTabControl();
			this.tabPage_RefBrowser = new System.Windows.Forms.TabPage();
			this.tabPage_CompilerLogs = new System.Windows.Forms.TabPage();
			this.tabPage_SearchResults = new System.Windows.Forms.TabPage();
			this.treeView_SearchResults = new DarkUI.Controls.DarkTreeView();
			this.separator_11 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_13 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_14 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_15 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_16 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_17 = new System.Windows.Forms.ToolStripSeparator();
			this.splitter_Bottom = new System.Windows.Forms.Splitter();
			this.splitter_Left = new System.Windows.Forms.Splitter();
			this.splitter_Right = new System.Windows.Forms.Splitter();
			this.statusStrip = new DarkUI.Controls.DarkStatusStrip();
			this.textChangedDelayTimer = new System.Windows.Forms.Timer(this.components);
			this.toolStrip = new DarkUI.Controls.DarkToolStrip();
			this.contextMenu_TextEditor.SuspendLayout();
			this.menuStrip.SuspendLayout();
			this.panel_Editor.SuspendLayout();
			this.panel_Syntax.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.scriptFolderWatcher)).BeginInit();
			this.sectionPanel_InfoBox.SuspendLayout();
			this.tabControl_Info.SuspendLayout();
			this.tabPage_RefBrowser.SuspendLayout();
			this.tabPage_CompilerLogs.SuspendLayout();
			this.tabPage_SearchResults.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// backgroundWorker_NGC
			// 
			this.backgroundWorker_NGC.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker_NGC_DoWork);
			this.backgroundWorker_NGC.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker_NGC_RunWorkerCompleted);
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
			this.button_Build.Click += new System.EventHandler(this.File_Build_Click);
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
			this.button_ClearBookmarks.Click += new System.EventHandler(this.Tools_ClearBookmarks_Click);
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
			this.button_Comment.Click += new System.EventHandler(this.Tools_Comment_Click);
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
			this.button_Copy.Click += new System.EventHandler(this.Edit_Copy_Click);
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
			this.button_Cut.Click += new System.EventHandler(this.Edit_Cut_Click);
			// 
			// button_NewFile
			// 
			this.button_NewFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_NewFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_NewFile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_NewFile.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_create_new_16;
			this.button_NewFile.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_NewFile.Name = "button_NewFile";
			this.button_NewFile.Size = new System.Drawing.Size(23, 25);
			this.button_NewFile.Text = "New File...";
			this.button_NewFile.Click += new System.EventHandler(this.File_NewFile_Click);
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
			this.button_NextBookmark.Click += new System.EventHandler(this.Tools_NextBookmark_Click);
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
			this.button_Paste.Click += new System.EventHandler(this.Edit_Paste_Click);
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
			this.button_PrevBookmark.Click += new System.EventHandler(this.Tools_PrevBookmark_Click);
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
			this.button_Redo.Click += new System.EventHandler(this.Edit_Redo_Click);
			// 
			// button_ResetZoom
			// 
			this.button_ResetZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_ResetZoom.Checked = false;
			this.button_ResetZoom.Location = new System.Drawing.Point(807, 577);
			this.button_ResetZoom.Name = "button_ResetZoom";
			this.button_ResetZoom.Size = new System.Drawing.Size(75, 20);
			this.button_ResetZoom.TabIndex = 11;
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
			this.button_Save.Click += new System.EventHandler(this.File_Save_Click);
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
			this.button_SaveAll.Click += new System.EventHandler(this.File_SaveAll_Click);
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
			this.button_ToggleBookmark.Click += new System.EventHandler(this.Tools_ToggleBookmark_Click);
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
			this.button_Uncomment.Click += new System.EventHandler(this.Tools_Uncomment_Click);
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
			this.button_Undo.Click += new System.EventHandler(this.Edit_Undo_Click);
			// 
			// contextMenu_TextEditor
			// 
			this.contextMenu_TextEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu_TextEditor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu_TextEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenuItem_Cut,
            this.contextMenuItem_Copy,
            this.contextMenuItem_Paste,
            this.separator_18,
            this.contextMenuItem_Comment,
            this.contextMenuItem_Uncomment,
            this.separator_19,
            this.contextMenuItem_ToggleBookmark});
			this.contextMenu_TextEditor.Name = "editorContextMenu";
			this.contextMenu_TextEditor.Size = new System.Drawing.Size(180, 150);
			// 
			// contextMenuItem_Cut
			// 
			this.contextMenuItem_Cut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Cut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Cut.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_cut_16;
			this.contextMenuItem_Cut.Name = "contextMenuItem_Cut";
			this.contextMenuItem_Cut.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Cut.Text = "Cut";
			this.contextMenuItem_Cut.Click += new System.EventHandler(this.Edit_Cut_Click);
			// 
			// contextMenuItem_Copy
			// 
			this.contextMenuItem_Copy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Copy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Copy.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_copy_comments_16;
			this.contextMenuItem_Copy.Name = "contextMenuItem_Copy";
			this.contextMenuItem_Copy.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Copy.Text = "Copy";
			this.contextMenuItem_Copy.Click += new System.EventHandler(this.Edit_Copy_Click);
			// 
			// contextMenuItem_Paste
			// 
			this.contextMenuItem_Paste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Paste.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Paste.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_clipboard_16;
			this.contextMenuItem_Paste.Name = "contextMenuItem_Paste";
			this.contextMenuItem_Paste.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Paste.Text = "Paste";
			this.contextMenuItem_Paste.Click += new System.EventHandler(this.Edit_Paste_Click);
			// 
			// separator_18
			// 
			this.separator_18.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_18.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_18.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_18.Name = "separator_18";
			this.separator_18.Size = new System.Drawing.Size(176, 6);
			// 
			// contextMenuItem_Comment
			// 
			this.contextMenuItem_Comment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Comment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Comment.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_comment_16;
			this.contextMenuItem_Comment.Name = "contextMenuItem_Comment";
			this.contextMenuItem_Comment.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Comment.Text = "Comment out Lines";
			this.contextMenuItem_Comment.Click += new System.EventHandler(this.Tools_Comment_Click);
			// 
			// contextMenuItem_Uncomment
			// 
			this.contextMenuItem_Uncomment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Uncomment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Uncomment.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_uncomment_16;
			this.contextMenuItem_Uncomment.Name = "contextMenuItem_Uncomment";
			this.contextMenuItem_Uncomment.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Uncomment.Text = "Uncomment Lines";
			this.contextMenuItem_Uncomment.Click += new System.EventHandler(this.Tools_Uncomment_Click);
			// 
			// separator_19
			// 
			this.separator_19.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_19.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_19.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_19.Name = "separator_19";
			this.separator_19.Size = new System.Drawing.Size(176, 6);
			// 
			// contextMenuItem_ToggleBookmark
			// 
			this.contextMenuItem_ToggleBookmark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_ToggleBookmark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_ToggleBookmark.Image = global::TombIDE.ScriptEditor.Properties.Resources.script_bookmark_16;
			this.contextMenuItem_ToggleBookmark.Name = "contextMenuItem_ToggleBookmark";
			this.contextMenuItem_ToggleBookmark.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_ToggleBookmark.Text = "Toggle Bookmark";
			this.contextMenuItem_ToggleBookmark.Click += new System.EventHandler(this.Tools_ToggleBookmark_Click);
			// 
			// fileList
			// 
			this.fileList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.fileList.Dock = System.Windows.Forms.DockStyle.Right;
			this.fileList.Location = new System.Drawing.Point(760, 52);
			this.fileList.Name = "fileList";
			this.fileList.Size = new System.Drawing.Size(200, 315);
			this.fileList.TabIndex = 4;
			// 
			// label_ColNumber
			// 
			this.label_ColNumber.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.label_ColNumber.Name = "label_ColNumber";
			this.label_ColNumber.Size = new System.Drawing.Size(62, 15);
			this.label_ColNumber.Text = "Column: 0";
			// 
			// label_LineNumber
			// 
			this.label_LineNumber.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.label_LineNumber.Name = "label_LineNumber";
			this.label_LineNumber.Size = new System.Drawing.Size(41, 15);
			this.label_LineNumber.Text = "Line: 0";
			// 
			// label_SelectedChars
			// 
			this.label_SelectedChars.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
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
			this.label_Zoom.TabIndex = 10;
			this.label_Zoom.Text = "Zoom: 100%";
			this.label_Zoom.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// menu_Edit
			// 
			this.menu_Edit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menu_Edit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_Undo,
            this.menuItem_Redo,
            this.separator_02,
            this.menuItem_Cut,
            this.menuItem_Copy,
            this.menuItem_Paste,
            this.separator_03,
            this.menuItem_FindReplace,
            this.separator_04,
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
			this.menuItem_Undo.Size = new System.Drawing.Size(203, 22);
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
			this.menuItem_Redo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
			this.menuItem_Redo.Size = new System.Drawing.Size(203, 22);
			this.menuItem_Redo.Text = "&Redo";
			this.menuItem_Redo.Click += new System.EventHandler(this.Edit_Redo_Click);
			// 
			// separator_02
			// 
			this.separator_02.Name = "separator_02";
			this.separator_02.Size = new System.Drawing.Size(200, 6);
			// 
			// menuItem_Cut
			// 
			this.menuItem_Cut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Cut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Cut.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_cut_16;
			this.menuItem_Cut.Name = "menuItem_Cut";
			this.menuItem_Cut.ShortcutKeyDisplayString = "Ctrl+X";
			this.menuItem_Cut.Size = new System.Drawing.Size(203, 22);
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
			this.menuItem_Copy.Size = new System.Drawing.Size(203, 22);
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
			this.menuItem_Paste.Size = new System.Drawing.Size(203, 22);
			this.menuItem_Paste.Text = "&Paste";
			this.menuItem_Paste.Click += new System.EventHandler(this.Edit_Paste_Click);
			// 
			// separator_03
			// 
			this.separator_03.Name = "separator_03";
			this.separator_03.Size = new System.Drawing.Size(200, 6);
			// 
			// menuItem_FindReplace
			// 
			this.menuItem_FindReplace.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_FindReplace.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_FindReplace.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_search_16;
			this.menuItem_FindReplace.Name = "menuItem_FindReplace";
			this.menuItem_FindReplace.ShortcutKeyDisplayString = "Ctrl+F";
			this.menuItem_FindReplace.Size = new System.Drawing.Size(203, 22);
			this.menuItem_FindReplace.Text = "&Find && Replace...";
			this.menuItem_FindReplace.Click += new System.EventHandler(this.Edit_FindReplace_Click);
			// 
			// separator_04
			// 
			this.separator_04.Name = "separator_04";
			this.separator_04.Size = new System.Drawing.Size(200, 6);
			// 
			// menuItem_SelectAll
			// 
			this.menuItem_SelectAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_SelectAll.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_SelectAll.Name = "menuItem_SelectAll";
			this.menuItem_SelectAll.ShortcutKeyDisplayString = "Ctrl+A";
			this.menuItem_SelectAll.Size = new System.Drawing.Size(203, 22);
			this.menuItem_SelectAll.Text = "Select &All";
			this.menuItem_SelectAll.Click += new System.EventHandler(this.Edit_SelectAll_Click);
			// 
			// menu_File
			// 
			this.menu_File.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menu_File.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_NewFile,
            this.separator_10,
            this.menuItem_Save,
            this.menuItem_SaveAll,
            this.separator_01,
            this.menuItem_Build});
			this.menu_File.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menu_File.Name = "menu_File";
			this.menu_File.Size = new System.Drawing.Size(37, 20);
			this.menu_File.Text = "&File";
			// 
			// menuItem_NewFile
			// 
			this.menuItem_NewFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_NewFile.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_NewFile.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_create_new_16;
			this.menuItem_NewFile.Name = "menuItem_NewFile";
			this.menuItem_NewFile.Size = new System.Drawing.Size(187, 22);
			this.menuItem_NewFile.Text = "&New File...";
			this.menuItem_NewFile.Click += new System.EventHandler(this.File_NewFile_Click);
			// 
			// separator_10
			// 
			this.separator_10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_10.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_10.Name = "separator_10";
			this.separator_10.Size = new System.Drawing.Size(184, 6);
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
			// separator_01
			// 
			this.separator_01.Name = "separator_01";
			this.separator_01.Size = new System.Drawing.Size(184, 6);
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
			this.menuItem_About.Size = new System.Drawing.Size(116, 22);
			this.menuItem_About.Text = "&About...";
			this.menuItem_About.Click += new System.EventHandler(this.Help_About_Click);
			// 
			// menu_Options
			// 
			this.menu_Options.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menu_Options.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_NewInclude,
            this.menuItem_ShowLogs,
            this.menuItem_ReindentOnSave,
            this.separator_08,
            this.menuItem_Settings});
			this.menu_Options.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menu_Options.Name = "menu_Options";
			this.menu_Options.Size = new System.Drawing.Size(61, 20);
			this.menu_Options.Text = "&Options";
			// 
			// menuItem_NewInclude
			// 
			this.menuItem_NewInclude.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_NewInclude.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_NewInclude.Name = "menuItem_NewInclude";
			this.menuItem_NewInclude.Size = new System.Drawing.Size(242, 22);
			this.menuItem_NewInclude.Text = "&Use New #INCLUDE Method";
			this.menuItem_NewInclude.Click += new System.EventHandler(this.Options_NewInclude_Click);
			// 
			// menuItem_ShowLogs
			// 
			this.menuItem_ShowLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ShowLogs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_ShowLogs.Name = "menuItem_ShowLogs";
			this.menuItem_ShowLogs.Size = new System.Drawing.Size(242, 22);
			this.menuItem_ShowLogs.Text = "&Show Compiler Logs After Build";
			this.menuItem_ShowLogs.Click += new System.EventHandler(this.Options_ShowLogs_Click);
			// 
			// menuItem_ReindentOnSave
			// 
			this.menuItem_ReindentOnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ReindentOnSave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_ReindentOnSave.Name = "menuItem_ReindentOnSave";
			this.menuItem_ReindentOnSave.Size = new System.Drawing.Size(242, 22);
			this.menuItem_ReindentOnSave.Text = "&Reindent on Save";
			this.menuItem_ReindentOnSave.Click += new System.EventHandler(this.Options_ReindentOnSave_Click);
			// 
			// separator_08
			// 
			this.separator_08.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_08.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_08.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_08.Name = "separator_08";
			this.separator_08.Size = new System.Drawing.Size(239, 6);
			// 
			// menuItem_Settings
			// 
			this.menuItem_Settings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Settings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Settings.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_settings_16;
			this.menuItem_Settings.Name = "menuItem_Settings";
			this.menuItem_Settings.Size = new System.Drawing.Size(242, 22);
			this.menuItem_Settings.Text = "Text Editor &Settings...";
			this.menuItem_Settings.Click += new System.EventHandler(this.Options_Settings_Click);
			// 
			// menu_Tools
			// 
			this.menu_Tools.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menu_Tools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_Reindent,
            this.menuItem_Trim,
            this.separator_06,
            this.menuItem_Comment,
            this.menuItem_Uncomment,
            this.separator_07,
            this.menuItem_ToggleBookmark,
            this.menuItem_PrevBookmark,
            this.menuItem_NextBookmark,
            this.menuItem_ClearBookmarks,
            this.separator_05,
            this.menuItem_CheckErrors});
			this.menu_Tools.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menu_Tools.Name = "menu_Tools";
			this.menu_Tools.Size = new System.Drawing.Size(46, 20);
			this.menu_Tools.Text = "&Tools";
			// 
			// menuItem_CheckErrors
			// 
			this.menuItem_CheckErrors.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_CheckErrors.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_CheckErrors.Image = global::TombIDE.ScriptEditor.Properties.Resources.general_search_16;
			this.menuItem_CheckErrors.Name = "menuItem_CheckErrors";
			this.menuItem_CheckErrors.Size = new System.Drawing.Size(300, 22);
			this.menuItem_CheckErrors.Text = "Manually Check for &Errors";
			this.menuItem_CheckErrors.Click += new System.EventHandler(this.Tools_CheckErrors_Click);
			// 
			// separator_05
			// 
			this.separator_05.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_05.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_05.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_05.Name = "separator_05";
			this.separator_05.Size = new System.Drawing.Size(297, 6);
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
			// separator_06
			// 
			this.separator_06.Name = "separator_06";
			this.separator_06.Size = new System.Drawing.Size(297, 6);
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
			// separator_07
			// 
			this.separator_07.Name = "separator_07";
			this.separator_07.Size = new System.Drawing.Size(297, 6);
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
			this.menuItem_PrevBookmark.Text = "Go to &Previous Bookmark";
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
			this.menuItem_NextBookmark.Text = "Go to &Next Bookmark";
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
			// menu_View
			// 
			this.menu_View.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menu_View.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_ObjBrowser,
            this.menuItem_FileList,
            this.menuItem_InfoBox,
            this.separator_09,
            this.menuItem_ToolStrip,
            this.menuItem_StatusStrip,
            this.separator_12,
            this.menuItem_SwapPanels});
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
			this.menuItem_ObjBrowser.Size = new System.Drawing.Size(228, 22);
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
			this.menuItem_FileList.Size = new System.Drawing.Size(228, 22);
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
			this.menuItem_InfoBox.Size = new System.Drawing.Size(228, 22);
			this.menuItem_InfoBox.Text = "&Information Box";
			this.menuItem_InfoBox.Click += new System.EventHandler(this.View_InfoBox_Click);
			// 
			// separator_09
			// 
			this.separator_09.Name = "separator_09";
			this.separator_09.Size = new System.Drawing.Size(225, 6);
			// 
			// menuItem_ToolStrip
			// 
			this.menuItem_ToolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ToolStrip.Checked = true;
			this.menuItem_ToolStrip.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_ToolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_ToolStrip.Name = "menuItem_ToolStrip";
			this.menuItem_ToolStrip.Size = new System.Drawing.Size(228, 22);
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
			this.menuItem_StatusStrip.Size = new System.Drawing.Size(228, 22);
			this.menuItem_StatusStrip.Text = "&Status Strip";
			this.menuItem_StatusStrip.Click += new System.EventHandler(this.View_StatusStrip_Click);
			// 
			// separator_12
			// 
			this.separator_12.Name = "separator_12";
			this.separator_12.Size = new System.Drawing.Size(225, 6);
			// 
			// menuItem_SwapPanels
			// 
			this.menuItem_SwapPanels.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_SwapPanels.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_SwapPanels.Name = "menuItem_SwapPanels";
			this.menuItem_SwapPanels.Size = new System.Drawing.Size(228, 22);
			this.menuItem_SwapPanels.Text = "Swap Info and File List Panels";
			this.menuItem_SwapPanels.Click += new System.EventHandler(this.View_SwapPanels_Click);
			// 
			// menuStrip
			// 
			this.menuStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menu_File,
            this.menu_Edit,
            this.menu_Tools,
            this.menu_Options,
            this.menu_View,
            this.menu_Help});
			this.menuStrip.Location = new System.Drawing.Point(0, 0);
			this.menuStrip.Name = "menuStrip";
			this.menuStrip.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
			this.menuStrip.Size = new System.Drawing.Size(960, 24);
			this.menuStrip.TabIndex = 0;
			// 
			// objectBrowser
			// 
			this.objectBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.objectBrowser.Dock = System.Windows.Forms.DockStyle.Left;
			this.objectBrowser.Location = new System.Drawing.Point(0, 52);
			this.objectBrowser.Name = "objectBrowser";
			this.objectBrowser.Size = new System.Drawing.Size(200, 315);
			this.objectBrowser.TabIndex = 2;
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
			this.tabControl_Editor.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.tabControl_Editor.ItemSize = new System.Drawing.Size(0, 22);
			this.tabControl_Editor.Location = new System.Drawing.Point(0, 0);
			this.tabControl_Editor.Name = "tabControl_Editor";
			this.tabControl_Editor.SelectedIndex = 0;
			this.tabControl_Editor.Size = new System.Drawing.Size(548, 313);
			this.tabControl_Editor.TabIndex = 6;
			this.tabControl_Editor.TabClosing += new System.EventHandler<System.Windows.Forms.TabControlCancelEventArgs>(this.Editor_TabControl_TabClosing);
			this.tabControl_Editor.SelectedIndexChanged += new System.EventHandler(this.Editor_TabControl_SelectedIndexChanged);
			this.tabControl_Editor.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.Editor_TabControl_Selecting);
			this.tabControl_Editor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Editor_TabControl_MouseClick);
			// 
			// panel_Syntax
			// 
			this.panel_Syntax.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panel_Syntax.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Syntax.Controls.Add(this.syntaxPreview);
			this.panel_Syntax.Location = new System.Drawing.Point(257, 577);
			this.panel_Syntax.Name = "panel_Syntax";
			this.panel_Syntax.Size = new System.Drawing.Size(544, 20);
			this.panel_Syntax.TabIndex = 12;
			// 
			// syntaxPreview
			// 
			this.syntaxPreview.BackColor = System.Drawing.Color.Black;
			this.syntaxPreview.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.syntaxPreview.CurrentArgumentIndex = 0;
			this.syntaxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
			this.syntaxPreview.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.syntaxPreview.ForeColor = System.Drawing.Color.White;
			this.syntaxPreview.Location = new System.Drawing.Point(0, 0);
			this.syntaxPreview.Name = "syntaxPreview";
			this.syntaxPreview.ReadOnly = true;
			this.syntaxPreview.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
			this.syntaxPreview.Size = new System.Drawing.Size(542, 18);
			this.syntaxPreview.TabIndex = 0;
			this.syntaxPreview.Text = "";
			this.syntaxPreview.WordWrap = false;
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
			this.scriptFolderWatcher.SynchronizingObject = this.fileList;
			this.scriptFolderWatcher.Changed += new System.IO.FileSystemEventHandler(this.scriptFolderWatcher_Changed);
			this.scriptFolderWatcher.Created += new System.IO.FileSystemEventHandler(this.scriptFolderWatcher_Changed);
			this.scriptFolderWatcher.Deleted += new System.IO.FileSystemEventHandler(this.scriptFolderWatcher_Changed);
			this.scriptFolderWatcher.Renamed += new System.IO.RenamedEventHandler(this.scriptFolderWatcher_Renamed);
			// 
			// sectionPanel_InfoBox
			// 
			this.sectionPanel_InfoBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.sectionPanel_InfoBox.Controls.Add(this.tabControl_Info);
			this.sectionPanel_InfoBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.sectionPanel_InfoBox.Location = new System.Drawing.Point(0, 372);
			this.sectionPanel_InfoBox.Name = "sectionPanel_InfoBox";
			this.sectionPanel_InfoBox.SectionHeader = "Information Box - Double-click on a row to view details.";
			this.sectionPanel_InfoBox.Size = new System.Drawing.Size(960, 200);
			this.sectionPanel_InfoBox.TabIndex = 7;
			// 
			// tabControl_Info
			// 
			this.tabControl_Info.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabControl_Info.Controls.Add(this.tabPage_RefBrowser);
			this.tabControl_Info.Controls.Add(this.tabPage_CompilerLogs);
			this.tabControl_Info.Controls.Add(this.tabPage_SearchResults);
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
			// tabPage_SearchResults
			// 
			this.tabPage_SearchResults.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_SearchResults.Controls.Add(this.treeView_SearchResults);
			this.tabPage_SearchResults.Location = new System.Drawing.Point(4, 4);
			this.tabPage_SearchResults.Name = "tabPage_SearchResults";
			this.tabPage_SearchResults.Size = new System.Drawing.Size(948, 143);
			this.tabPage_SearchResults.TabIndex = 2;
			this.tabPage_SearchResults.Text = "Search Results";
			// 
			// treeView_SearchResults
			// 
			this.treeView_SearchResults.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView_SearchResults.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeView_SearchResults.EvenNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(52)))), ((int)(((byte)(52)))));
			this.treeView_SearchResults.FocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(110)))), ((int)(((byte)(175)))));
			this.treeView_SearchResults.Location = new System.Drawing.Point(0, 0);
			this.treeView_SearchResults.MaxDragChange = 20;
			this.treeView_SearchResults.Name = "treeView_SearchResults";
			this.treeView_SearchResults.NonFocusedNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
			this.treeView_SearchResults.OddNodeColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.treeView_SearchResults.Size = new System.Drawing.Size(948, 143);
			this.treeView_SearchResults.TabIndex = 0;
			this.treeView_SearchResults.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeView_SearchResults_MouseDoubleClick);
			// 
			// separator_11
			// 
			this.separator_11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_11.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_11.Name = "separator_11";
			this.separator_11.Size = new System.Drawing.Size(6, 28);
			// 
			// separator_13
			// 
			this.separator_13.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_13.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_13.Name = "separator_13";
			this.separator_13.Size = new System.Drawing.Size(6, 28);
			// 
			// separator_14
			// 
			this.separator_14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_14.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_14.Name = "separator_14";
			this.separator_14.Size = new System.Drawing.Size(6, 28);
			// 
			// separator_15
			// 
			this.separator_15.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_15.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_15.Name = "separator_15";
			this.separator_15.Size = new System.Drawing.Size(6, 28);
			// 
			// separator_16
			// 
			this.separator_16.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_16.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_16.Name = "separator_16";
			this.separator_16.Size = new System.Drawing.Size(6, 28);
			// 
			// separator_17
			// 
			this.separator_17.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_17.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_17.Name = "separator_17";
			this.separator_17.Size = new System.Drawing.Size(6, 28);
			// 
			// splitter_Bottom
			// 
			this.splitter_Bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter_Bottom.Location = new System.Drawing.Point(0, 367);
			this.splitter_Bottom.MinExtra = 400;
			this.splitter_Bottom.MinSize = 200;
			this.splitter_Bottom.Name = "splitter_Bottom";
			this.splitter_Bottom.Size = new System.Drawing.Size(960, 5);
			this.splitter_Bottom.TabIndex = 8;
			this.splitter_Bottom.TabStop = false;
			// 
			// splitter_Left
			// 
			this.splitter_Left.Location = new System.Drawing.Point(200, 52);
			this.splitter_Left.MinExtra = 400;
			this.splitter_Left.MinSize = 200;
			this.splitter_Left.Name = "splitter_Left";
			this.splitter_Left.Size = new System.Drawing.Size(5, 315);
			this.splitter_Left.TabIndex = 3;
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
			this.splitter_Right.TabIndex = 5;
			this.splitter_Right.TabStop = false;
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
			this.statusStrip.TabIndex = 9;
			this.statusStrip.Text = "statusStrip";
			// 
			// textChangedDelayTimer
			// 
			this.textChangedDelayTimer.Interval = 10;
			this.textChangedDelayTimer.Tick += new System.EventHandler(this.TextChangedDelayTimer_Tick);
			// 
			// toolStrip
			// 
			this.toolStrip.AutoSize = false;
			this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.button_NewFile,
            this.separator_11,
            this.button_Save,
            this.button_SaveAll,
            this.separator_13,
            this.button_Undo,
            this.button_Redo,
            this.separator_14,
            this.button_Cut,
            this.button_Copy,
            this.button_Paste,
            this.separator_15,
            this.button_Comment,
            this.button_Uncomment,
            this.separator_16,
            this.button_ToggleBookmark,
            this.button_PrevBookmark,
            this.button_NextBookmark,
            this.button_ClearBookmarks,
            this.separator_17,
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
			this.Controls.Add(this.button_ResetZoom);
			this.Controls.Add(this.panel_Editor);
			this.Controls.Add(this.splitter_Left);
			this.Controls.Add(this.objectBrowser);
			this.Controls.Add(this.splitter_Right);
			this.Controls.Add(this.fileList);
			this.Controls.Add(this.splitter_Bottom);
			this.Controls.Add(this.sectionPanel_InfoBox);
			this.Controls.Add(this.panel_Syntax);
			this.Controls.Add(this.label_Zoom);
			this.Controls.Add(this.statusStrip);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.menuStrip);
			this.Name = "ScriptEditor";
			this.Size = new System.Drawing.Size(960, 600);
			this.contextMenu_TextEditor.ResumeLayout(false);
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			this.panel_Editor.ResumeLayout(false);
			this.panel_Syntax.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.scriptFolderWatcher)).EndInit();
			this.sectionPanel_InfoBox.ResumeLayout(false);
			this.tabControl_Info.ResumeLayout(false);
			this.tabPage_RefBrowser.ResumeLayout(false);
			this.tabPage_CompilerLogs.ResumeLayout(false);
			this.tabPage_SearchResults.ResumeLayout(false);
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Controls.FileList fileList;
		private Controls.ObjectBrowser objectBrowser;
		private Controls.ReferenceBrowser referenceBrowser;
		private Controls.SyntaxPreview syntaxPreview;
		private DarkUI.Controls.DarkButton button_ResetZoom;
		private DarkUI.Controls.DarkContextMenu contextMenu_TextEditor;
		private DarkUI.Controls.DarkLabel label_Zoom;
		private DarkUI.Controls.DarkMenuStrip menuStrip;
		private DarkUI.Controls.DarkSectionPanel sectionPanel_InfoBox;
		private DarkUI.Controls.DarkStatusStrip statusStrip;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private DarkUI.Controls.DarkTreeView treeView_SearchResults;
		private System.ComponentModel.BackgroundWorker backgroundWorker_NGC;
		private System.IO.FileSystemWatcher scriptFolderWatcher;
		private System.Windows.Forms.CustomTabControl tabControl_Editor;
		private System.Windows.Forms.CustomTabControl tabControl_Info;
		private System.Windows.Forms.Panel panel_Editor;
		private System.Windows.Forms.Panel panel_Syntax;
		private System.Windows.Forms.RichTextBox richTextBox_Logs;
		private System.Windows.Forms.Splitter splitter_Bottom;
		private System.Windows.Forms.Splitter splitter_Left;
		private System.Windows.Forms.Splitter splitter_Right;
		private System.Windows.Forms.TabPage tabPage_CompilerLogs;
		private System.Windows.Forms.TabPage tabPage_RefBrowser;
		private System.Windows.Forms.TabPage tabPage_SearchResults;
		private System.Windows.Forms.Timer textChangedDelayTimer;
		private System.Windows.Forms.ToolStripButton button_Build;
		private System.Windows.Forms.ToolStripButton button_ClearBookmarks;
		private System.Windows.Forms.ToolStripButton button_Comment;
		private System.Windows.Forms.ToolStripButton button_Copy;
		private System.Windows.Forms.ToolStripButton button_Cut;
		private System.Windows.Forms.ToolStripButton button_NewFile;
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
		private System.Windows.Forms.ToolStripMenuItem menu_Options;
		private System.Windows.Forms.ToolStripMenuItem menu_Tools;
		private System.Windows.Forms.ToolStripMenuItem menu_View;
		private System.Windows.Forms.ToolStripMenuItem menuItem_About;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Build;
		private System.Windows.Forms.ToolStripMenuItem menuItem_CheckErrors;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ClearBookmarks;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Comment;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Copy;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Cut;
		private System.Windows.Forms.ToolStripMenuItem menuItem_FileList;
		private System.Windows.Forms.ToolStripMenuItem menuItem_FindReplace;
		private System.Windows.Forms.ToolStripMenuItem menuItem_InfoBox;
		private System.Windows.Forms.ToolStripMenuItem menuItem_NewFile;
		private System.Windows.Forms.ToolStripMenuItem menuItem_NewInclude;
		private System.Windows.Forms.ToolStripMenuItem menuItem_NextBookmark;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ObjBrowser;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Paste;
		private System.Windows.Forms.ToolStripMenuItem menuItem_PrevBookmark;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Redo;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Reindent;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ReindentOnSave;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Save;
		private System.Windows.Forms.ToolStripMenuItem menuItem_SaveAll;
		private System.Windows.Forms.ToolStripMenuItem menuItem_SelectAll;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Settings;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ShowLogs;
		private System.Windows.Forms.ToolStripMenuItem menuItem_StatusStrip;
		private System.Windows.Forms.ToolStripMenuItem menuItem_SwapPanels;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ToggleBookmark;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ToolStrip;
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
		private System.Windows.Forms.ToolStripSeparator separator_17;
		private System.Windows.Forms.ToolStripSeparator separator_18;
		private System.Windows.Forms.ToolStripSeparator separator_19;
		private System.Windows.Forms.ToolStripStatusLabel label_ColNumber;
		private System.Windows.Forms.ToolStripStatusLabel label_LineNumber;
		private System.Windows.Forms.ToolStripStatusLabel label_SelectedChars;
	}
}