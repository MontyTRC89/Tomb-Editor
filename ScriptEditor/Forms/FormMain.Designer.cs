namespace ScriptEditor
{
    partial class FormMain : DarkUI.Forms.DarkForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
			this.button_About = new System.Windows.Forms.ToolStripButton();
			this.button_AddProject = new System.Windows.Forms.ToolStripButton();
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
			this.button_ToggleBookmark = new System.Windows.Forms.ToolStripButton();
			this.button_Uncomment = new System.Windows.Forms.ToolStripButton();
			this.button_Undo = new System.Windows.Forms.ToolStripButton();
			this.comboBox_Projects = new DarkUI.Controls.DarkComboBox();
			this.contextMenu_TextBox = new DarkUI.Controls.DarkContextMenu();
			this.contextMenuItem_Cut = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuItem_Paste = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_15 = new System.Windows.Forms.ToolStripSeparator();
			this.contextMenuItem_Comment = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuItem_Uncomment = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_16 = new System.Windows.Forms.ToolStripSeparator();
			this.contextMenuItem_ToggleBookmark = new System.Windows.Forms.ToolStripMenuItem();
			this.groupBox_Editor = new DarkUI.Controls.DarkGroupBox();
			this.tabControl_Editor = new System.Windows.Forms.CustomTabControl();
			this.groupBox_InfoBox = new DarkUI.Controls.DarkGroupBox();
			this.tabControl_Info = new System.Windows.Forms.CustomTabControl();
			this.tabPage_RefBrowser = new System.Windows.Forms.TabPage();
			this.referenceBrowser = new ScriptEditor.ReferenceBrowser();
			this.tabPage_CompilerLogs = new System.Windows.Forms.TabPage();
			this.groupBox_ObjBrowser = new DarkUI.Controls.DarkGroupBox();
			this.treeView_ObjBrowser = new DarkUI.Controls.DarkTreeView();
			this.textBox_ObjBrowserSearch = new DarkUI.Controls.DarkTextBox();
			this.groupBox_ProjExplorer = new DarkUI.Controls.DarkGroupBox();
			this.treeView_Files = new DarkUI.Controls.DarkTreeView();
			this.label_ColNumber = new System.Windows.Forms.ToolStripStatusLabel();
			this.label_LineNumber = new System.Windows.Forms.ToolStripStatusLabel();
			this.label_SelectedChars = new System.Windows.Forms.ToolStripStatusLabel();
			this.label_Zoom = new DarkUI.Controls.DarkLabel();
			this.menu_Edit = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Undo = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Redo = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_03 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Cut = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Copy = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Paste = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_04 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Find = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Replace = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_05 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_SelectAll = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_File = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_AddProject = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_01 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Save = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_Build = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_02 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Exit = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_Help = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_About = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_Tools = new System.Windows.Forms.ToolStripMenuItem();
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
			this.separator_08 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_Settings = new System.Windows.Forms.ToolStripMenuItem();
			this.menu_View = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_ObjBrowser = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_ProjExplorer = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_InfoBox = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_17 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_ToolStrip = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_StatusStrip = new System.Windows.Forms.ToolStripMenuItem();
			this.separator_18 = new System.Windows.Forms.ToolStripSeparator();
			this.menuItem_LineNumbers = new System.Windows.Forms.ToolStripMenuItem();
			this.menuItem_ToolTips = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip = new DarkUI.Controls.DarkMenuStrip();
			this.scriptFolderWatcher = new System.IO.FileSystemWatcher();
			this.separator_09 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_10 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_11 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_12 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_13 = new System.Windows.Forms.ToolStripSeparator();
			this.separator_14 = new System.Windows.Forms.ToolStripSeparator();
			this.splitter_Bottom = new System.Windows.Forms.Splitter();
			this.splitter_Left = new System.Windows.Forms.Splitter();
			this.splitter_Right = new System.Windows.Forms.Splitter();
			this.statusStrip = new DarkUI.Controls.DarkStatusStrip();
			this.toolStrip = new DarkUI.Controls.DarkToolStrip();
			this.contextMenu_TextBox.SuspendLayout();
			this.groupBox_Editor.SuspendLayout();
			this.groupBox_InfoBox.SuspendLayout();
			this.tabControl_Info.SuspendLayout();
			this.tabPage_RefBrowser.SuspendLayout();
			this.groupBox_ObjBrowser.SuspendLayout();
			this.groupBox_ProjExplorer.SuspendLayout();
			this.menuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.scriptFolderWatcher)).BeginInit();
			this.statusStrip.SuspendLayout();
			this.toolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_About
			// 
			this.button_About.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_About.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_About.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_About.Image = ((System.Drawing.Image)(resources.GetObject("button_About.Image")));
			this.button_About.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_About.Name = "button_About";
			this.button_About.Size = new System.Drawing.Size(23, 25);
			this.button_About.Text = "About (F1)";
			this.button_About.Click += new System.EventHandler(this.ToolStrip_About_Click);
			// 
			// button_AddProject
			// 
			this.button_AddProject.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_AddProject.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_AddProject.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_AddProject.Image = global::ScriptEditor.Properties.Resources.general_Open_16;
			this.button_AddProject.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_AddProject.Name = "button_AddProject";
			this.button_AddProject.Size = new System.Drawing.Size(23, 25);
			this.button_AddProject.Text = "Add New Project... (Ctrl+N)";
			this.button_AddProject.Click += new System.EventHandler(this.ToolStrip_AddProject_Click);
			// 
			// button_Build
			// 
			this.button_Build.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Build.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Build.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Build.Image = global::ScriptEditor.Properties.Resources.play_16;
			this.button_Build.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Build.Name = "button_Build";
			this.button_Build.Size = new System.Drawing.Size(23, 25);
			this.button_Build.Text = "Build (F9)";
			this.button_Build.Click += new System.EventHandler(this.ToolStrip_Build_Click);
			// 
			// button_ClearBookmarks
			// 
			this.button_ClearBookmarks.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_ClearBookmarks.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_ClearBookmarks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_ClearBookmarks.Image = global::ScriptEditor.Properties.Resources.clearBookmark_16;
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
			this.button_Comment.Image = global::ScriptEditor.Properties.Resources.comment_16;
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
			this.button_Copy.Image = ((System.Drawing.Image)(resources.GetObject("button_Copy.Image")));
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
			this.button_Cut.Image = global::ScriptEditor.Properties.Resources.cut_16;
			this.button_Cut.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Cut.Name = "button_Cut";
			this.button_Cut.Size = new System.Drawing.Size(23, 25);
			this.button_Cut.Text = "Cut (Ctrl+X)";
			this.button_Cut.Click += new System.EventHandler(this.ToolStrip_Cut_Click);
			// 
			// button_EditLanguages
			// 
			this.button_EditLanguages.Location = new System.Drawing.Point(102, 49);
			this.button_EditLanguages.Name = "button_EditLanguages";
			this.button_EditLanguages.Size = new System.Drawing.Size(95, 23);
			this.button_EditLanguages.TabIndex = 7;
			this.button_EditLanguages.Text = "Edit Languages";
			this.button_EditLanguages.Click += new System.EventHandler(this.Explorer_EditLanguages_Click);
			// 
			// button_EditScript
			// 
			this.button_EditScript.Location = new System.Drawing.Point(3, 49);
			this.button_EditScript.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.button_EditScript.Name = "button_EditScript";
			this.button_EditScript.Size = new System.Drawing.Size(95, 23);
			this.button_EditScript.TabIndex = 6;
			this.button_EditScript.Text = "Edit Script File";
			this.button_EditScript.Click += new System.EventHandler(this.Explorer_EditScript_Click);
			// 
			// button_NextBookmark
			// 
			this.button_NextBookmark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_NextBookmark.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_NextBookmark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_NextBookmark.Image = global::ScriptEditor.Properties.Resources.nextBookmark_16;
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
			this.button_OpenInExplorer.Location = new System.Drawing.Point(3, 296);
			this.button_OpenInExplorer.Name = "button_OpenInExplorer";
			this.button_OpenInExplorer.Size = new System.Drawing.Size(194, 23);
			this.button_OpenInExplorer.TabIndex = 9;
			this.button_OpenInExplorer.Text = "Open in File Explorer";
			this.button_OpenInExplorer.Click += new System.EventHandler(this.Explorer_OpenInExplorer_Click);
			// 
			// button_Paste
			// 
			this.button_Paste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Paste.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Paste.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Paste.Image = ((System.Drawing.Image)(resources.GetObject("button_Paste.Image")));
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
			this.button_PrevBookmark.Image = global::ScriptEditor.Properties.Resources.prevBookmark_16;
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
			this.button_Redo.Image = global::ScriptEditor.Properties.Resources.general_redo_16;
			this.button_Redo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Redo.Name = "button_Redo";
			this.button_Redo.Size = new System.Drawing.Size(23, 25);
			this.button_Redo.Text = "Redo (Ctrl+Y)";
			this.button_Redo.Click += new System.EventHandler(this.ToolStrip_Redo_Click);
			// 
			// button_ResetZoom
			// 
			this.button_ResetZoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_ResetZoom.Location = new System.Drawing.Point(794, 543);
			this.button_ResetZoom.Name = "button_ResetZoom";
			this.button_ResetZoom.Size = new System.Drawing.Size(75, 20);
			this.button_ResetZoom.TabIndex = 9;
			this.button_ResetZoom.Text = "Reset zoom";
			this.button_ResetZoom.Visible = false;
			this.button_ResetZoom.Click += new System.EventHandler(this.StatusStrip_ResetZoom_Click);
			// 
			// button_Save
			// 
			this.button_Save.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_Save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_Save.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_Save.Image = global::ScriptEditor.Properties.Resources.general_Save_16;
			this.button_Save.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Save.Name = "button_Save";
			this.button_Save.Size = new System.Drawing.Size(23, 25);
			this.button_Save.Text = "Save (Ctrl+S)";
			this.button_Save.Click += new System.EventHandler(this.ToolStrip_Save_Click);
			// 
			// button_ToggleBookmark
			// 
			this.button_ToggleBookmark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.button_ToggleBookmark.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.button_ToggleBookmark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.button_ToggleBookmark.Image = global::ScriptEditor.Properties.Resources.toggleBookmark_16;
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
			this.button_Uncomment.Image = global::ScriptEditor.Properties.Resources.uncomment_16;
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
			this.button_Undo.Image = global::ScriptEditor.Properties.Resources.general_undo_16;
			this.button_Undo.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.button_Undo.Name = "button_Undo";
			this.button_Undo.Size = new System.Drawing.Size(23, 25);
			this.button_Undo.Text = "Undo (Ctrl+Z)";
			this.button_Undo.Click += new System.EventHandler(this.ToolStrip_Undo_Click);
			// 
			// comboBox_Projects
			// 
			this.comboBox_Projects.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox_Projects.FormattingEnabled = true;
			this.comboBox_Projects.Location = new System.Drawing.Point(3, 22);
			this.comboBox_Projects.Margin = new System.Windows.Forms.Padding(3, 9, 3, 3);
			this.comboBox_Projects.Name = "comboBox_Projects";
			this.comboBox_Projects.Size = new System.Drawing.Size(194, 21);
			this.comboBox_Projects.TabIndex = 0;
			this.comboBox_Projects.SelectedIndexChanged += new System.EventHandler(this.Explorer_Projects_SelectedIndexChanged);
			// 
			// contextMenu_TextBox
			// 
			this.contextMenu_TextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenu_TextBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenu_TextBox.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contextMenuItem_Cut,
            this.contextMenuItem_Copy,
            this.contextMenuItem_Paste,
            this.separator_15,
            this.contextMenuItem_Comment,
            this.contextMenuItem_Uncomment,
            this.separator_16,
            this.contextMenuItem_ToggleBookmark});
			this.contextMenu_TextBox.Name = "editorContextMenu";
			this.contextMenu_TextBox.Size = new System.Drawing.Size(180, 150);
			// 
			// contextMenuItem_Cut
			// 
			this.contextMenuItem_Cut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Cut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Cut.Image = global::ScriptEditor.Properties.Resources.cut_16;
			this.contextMenuItem_Cut.Name = "contextMenuItem_Cut";
			this.contextMenuItem_Cut.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Cut.Text = "Cut";
			this.contextMenuItem_Cut.Click += new System.EventHandler(this.ContextMenu_Cut_Click);
			// 
			// contextMenuItem_Copy
			// 
			this.contextMenuItem_Copy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Copy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Copy.Image = global::ScriptEditor.Properties.Resources.general_copy_16;
			this.contextMenuItem_Copy.Name = "contextMenuItem_Copy";
			this.contextMenuItem_Copy.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Copy.Text = "Copy";
			this.contextMenuItem_Copy.Click += new System.EventHandler(this.ContextMenu_Copy_Click);
			// 
			// contextMenuItem_Paste
			// 
			this.contextMenuItem_Paste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Paste.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Paste.Image = global::ScriptEditor.Properties.Resources.general_clipboard_16;
			this.contextMenuItem_Paste.Name = "contextMenuItem_Paste";
			this.contextMenuItem_Paste.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Paste.Text = "Paste";
			this.contextMenuItem_Paste.Click += new System.EventHandler(this.ContextMenu_Paste_Click);
			// 
			// separator_15
			// 
			this.separator_15.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_15.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_15.Name = "separator_15";
			this.separator_15.Size = new System.Drawing.Size(176, 6);
			// 
			// contextMenuItem_Comment
			// 
			this.contextMenuItem_Comment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Comment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Comment.Image = global::ScriptEditor.Properties.Resources.comment_16;
			this.contextMenuItem_Comment.Name = "contextMenuItem_Comment";
			this.contextMenuItem_Comment.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Comment.Text = "Comment out Lines";
			this.contextMenuItem_Comment.Click += new System.EventHandler(this.ContextMenu_Comment_Click);
			// 
			// contextMenuItem_Uncomment
			// 
			this.contextMenuItem_Uncomment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_Uncomment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_Uncomment.Image = global::ScriptEditor.Properties.Resources.uncomment_16;
			this.contextMenuItem_Uncomment.Name = "contextMenuItem_Uncomment";
			this.contextMenuItem_Uncomment.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_Uncomment.Text = "Uncomment Lines";
			this.contextMenuItem_Uncomment.Click += new System.EventHandler(this.ContextMenu_Uncomment_Click);
			// 
			// separator_16
			// 
			this.separator_16.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_16.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_16.Name = "separator_16";
			this.separator_16.Size = new System.Drawing.Size(176, 6);
			// 
			// contextMenuItem_ToggleBookmark
			// 
			this.contextMenuItem_ToggleBookmark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.contextMenuItem_ToggleBookmark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.contextMenuItem_ToggleBookmark.Image = global::ScriptEditor.Properties.Resources.toggleBookmark_16;
			this.contextMenuItem_ToggleBookmark.Name = "contextMenuItem_ToggleBookmark";
			this.contextMenuItem_ToggleBookmark.Size = new System.Drawing.Size(179, 22);
			this.contextMenuItem_ToggleBookmark.Text = "Toggle Bookmark";
			this.contextMenuItem_ToggleBookmark.Click += new System.EventHandler(this.ContextMenu_ToggleBookmark_Click);
			// 
			// groupBox_Editor
			// 
			this.groupBox_Editor.Controls.Add(this.tabControl_Editor);
			this.groupBox_Editor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox_Editor.Location = new System.Drawing.Point(205, 52);
			this.groupBox_Editor.Margin = new System.Windows.Forms.Padding(0);
			this.groupBox_Editor.Name = "groupBox_Editor";
			this.groupBox_Editor.Padding = new System.Windows.Forms.Padding(0);
			this.groupBox_Editor.Size = new System.Drawing.Size(539, 322);
			this.groupBox_Editor.TabIndex = 17;
			this.groupBox_Editor.TabStop = false;
			// 
			// tabControl_Editor
			// 
			this.tabControl_Editor.AllowDrop = true;
			this.tabControl_Editor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			// 
			// 
			// 
			this.tabControl_Editor.DisplayStyleProvider.BorderColor = System.Drawing.SystemColors.ControlDark;
			this.tabControl_Editor.DisplayStyleProvider.BorderColorHot = System.Drawing.SystemColors.ControlDark;
			this.tabControl_Editor.DisplayStyleProvider.BorderColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(157)))), ((int)(((byte)(185)))));
			this.tabControl_Editor.DisplayStyleProvider.CloserColor = System.Drawing.Color.White;
			this.tabControl_Editor.DisplayStyleProvider.FocusTrack = true;
			this.tabControl_Editor.DisplayStyleProvider.HotTrack = true;
			this.tabControl_Editor.DisplayStyleProvider.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.tabControl_Editor.DisplayStyleProvider.Opacity = 1F;
			this.tabControl_Editor.DisplayStyleProvider.Overlap = 0;
			this.tabControl_Editor.DisplayStyleProvider.Padding = new System.Drawing.Point(10, 3);
			this.tabControl_Editor.DisplayStyleProvider.Radius = 2;
			this.tabControl_Editor.DisplayStyleProvider.ShowTabCloser = true;
			this.tabControl_Editor.DisplayStyleProvider.TextColor = System.Drawing.Color.White;
			this.tabControl_Editor.DisplayStyleProvider.TextColorDisabled = System.Drawing.Color.White;
			this.tabControl_Editor.DisplayStyleProvider.TextColorSelected = System.Drawing.Color.White;
			this.tabControl_Editor.HotTrack = true;
			this.tabControl_Editor.Location = new System.Drawing.Point(0, 0);
			this.tabControl_Editor.Margin = new System.Windows.Forms.Padding(0);
			this.tabControl_Editor.Name = "tabControl_Editor";
			this.tabControl_Editor.SelectedIndex = 0;
			this.tabControl_Editor.Size = new System.Drawing.Size(534, 322);
			this.tabControl_Editor.TabIndex = 0;
			this.tabControl_Editor.TabClosing += new System.EventHandler<System.Windows.Forms.TabControlCancelEventArgs>(this.Editor_TabControl_TabClosing);
			this.tabControl_Editor.SelectedIndexChanged += new System.EventHandler(this.Editor_TabControl_SelectedIndexChanged);
			this.tabControl_Editor.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.Editor_TabControl_Selecting);
			this.tabControl_Editor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Editor_TabControl_MouseClick);
			// 
			// groupBox_InfoBox
			// 
			this.groupBox_InfoBox.Controls.Add(this.tabControl_Info);
			this.groupBox_InfoBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.groupBox_InfoBox.Location = new System.Drawing.Point(0, 379);
			this.groupBox_InfoBox.Margin = new System.Windows.Forms.Padding(0);
			this.groupBox_InfoBox.Name = "groupBox_InfoBox";
			this.groupBox_InfoBox.Padding = new System.Windows.Forms.Padding(0);
			this.groupBox_InfoBox.Size = new System.Drawing.Size(944, 160);
			this.groupBox_InfoBox.TabIndex = 11;
			this.groupBox_InfoBox.TabStop = false;
			// 
			// tabControl_Info
			// 
			this.tabControl_Info.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabControl_Info.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl_Info.Controls.Add(this.tabPage_RefBrowser);
			this.tabControl_Info.Controls.Add(this.tabPage_CompilerLogs);
			// 
			// 
			// 
			this.tabControl_Info.DisplayStyleProvider.BorderColor = System.Drawing.SystemColors.ControlDark;
			this.tabControl_Info.DisplayStyleProvider.BorderColorHot = System.Drawing.SystemColors.ControlDark;
			this.tabControl_Info.DisplayStyleProvider.BorderColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(127)))), ((int)(((byte)(157)))), ((int)(((byte)(185)))));
			this.tabControl_Info.DisplayStyleProvider.CloserColor = System.Drawing.Color.White;
			this.tabControl_Info.DisplayStyleProvider.CloserColorActive = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.tabControl_Info.DisplayStyleProvider.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.tabControl_Info.DisplayStyleProvider.FocusTrack = true;
			this.tabControl_Info.DisplayStyleProvider.HotTrack = true;
			this.tabControl_Info.DisplayStyleProvider.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.tabControl_Info.DisplayStyleProvider.Opacity = 1F;
			this.tabControl_Info.DisplayStyleProvider.Overlap = 0;
			this.tabControl_Info.DisplayStyleProvider.Padding = new System.Drawing.Point(6, 3);
			this.tabControl_Info.DisplayStyleProvider.Radius = 2;
			this.tabControl_Info.DisplayStyleProvider.ShowTabCloser = false;
			this.tabControl_Info.DisplayStyleProvider.TextColor = System.Drawing.Color.White;
			this.tabControl_Info.DisplayStyleProvider.TextColorSelected = System.Drawing.Color.White;
			this.tabControl_Info.HotTrack = true;
			this.tabControl_Info.Location = new System.Drawing.Point(0, 0);
			this.tabControl_Info.Margin = new System.Windows.Forms.Padding(0);
			this.tabControl_Info.Name = "tabControl_Info";
			this.tabControl_Info.SelectedIndex = 0;
			this.tabControl_Info.Size = new System.Drawing.Size(944, 160);
			this.tabControl_Info.TabIndex = 0;
			// 
			// tabPage_RefBrowser
			// 
			this.tabPage_RefBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_RefBrowser.Controls.Add(this.referenceBrowser);
			this.tabPage_RefBrowser.Location = new System.Drawing.Point(4, 4);
			this.tabPage_RefBrowser.Margin = new System.Windows.Forms.Padding(0);
			this.tabPage_RefBrowser.Name = "tabPage_RefBrowser";
			this.tabPage_RefBrowser.Size = new System.Drawing.Size(936, 133);
			this.tabPage_RefBrowser.TabIndex = 0;
			this.tabPage_RefBrowser.Text = "Reference Browser";
			// 
			// referenceBrowser
			// 
			this.referenceBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.referenceBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.referenceBrowser.Location = new System.Drawing.Point(0, 0);
			this.referenceBrowser.Margin = new System.Windows.Forms.Padding(0);
			this.referenceBrowser.Name = "referenceBrowser";
			this.referenceBrowser.Size = new System.Drawing.Size(936, 133);
			this.referenceBrowser.TabIndex = 0;
			// 
			// tabPage_CompilerLogs
			// 
			this.tabPage_CompilerLogs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.tabPage_CompilerLogs.Location = new System.Drawing.Point(4, 4);
			this.tabPage_CompilerLogs.Name = "tabPage_CompilerLogs";
			this.tabPage_CompilerLogs.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage_CompilerLogs.Size = new System.Drawing.Size(936, 133);
			this.tabPage_CompilerLogs.TabIndex = 1;
			this.tabPage_CompilerLogs.Text = "Compiler Logs";
			// 
			// groupBox_ObjBrowser
			// 
			this.groupBox_ObjBrowser.Controls.Add(this.treeView_ObjBrowser);
			this.groupBox_ObjBrowser.Controls.Add(this.textBox_ObjBrowserSearch);
			this.groupBox_ObjBrowser.Dock = System.Windows.Forms.DockStyle.Left;
			this.groupBox_ObjBrowser.Location = new System.Drawing.Point(0, 52);
			this.groupBox_ObjBrowser.Margin = new System.Windows.Forms.Padding(0);
			this.groupBox_ObjBrowser.Name = "groupBox_ObjBrowser";
			this.groupBox_ObjBrowser.Padding = new System.Windows.Forms.Padding(0);
			this.groupBox_ObjBrowser.Size = new System.Drawing.Size(200, 322);
			this.groupBox_ObjBrowser.TabIndex = 7;
			this.groupBox_ObjBrowser.TabStop = false;
			this.groupBox_ObjBrowser.Text = "Object Browser";
			// 
			// treeView_ObjBrowser
			// 
			this.treeView_ObjBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeView_ObjBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.treeView_ObjBrowser.Location = new System.Drawing.Point(3, 43);
			this.treeView_ObjBrowser.MaxDragChange = 20;
			this.treeView_ObjBrowser.Name = "treeView_ObjBrowser";
			this.treeView_ObjBrowser.Size = new System.Drawing.Size(194, 276);
			this.treeView_ObjBrowser.TabIndex = 6;
			this.treeView_ObjBrowser.Click += new System.EventHandler(this.ObjBrowser_TreeView_Click);
			// 
			// textBox_ObjBrowserSearch
			// 
			this.textBox_ObjBrowserSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_ObjBrowserSearch.Location = new System.Drawing.Point(3, 19);
			this.textBox_ObjBrowserSearch.Margin = new System.Windows.Forms.Padding(3, 6, 3, 0);
			this.textBox_ObjBrowserSearch.Name = "textBox_ObjBrowserSearch";
			this.textBox_ObjBrowserSearch.Size = new System.Drawing.Size(194, 20);
			this.textBox_ObjBrowserSearch.TabIndex = 7;
			this.textBox_ObjBrowserSearch.Text = "Search...";
			this.textBox_ObjBrowserSearch.TextChanged += new System.EventHandler(this.ObjBrowser_Search_TextChanged);
			this.textBox_ObjBrowserSearch.GotFocus += new System.EventHandler(this.ObjBrowser_Search_GotFocus);
			this.textBox_ObjBrowserSearch.LostFocus += new System.EventHandler(this.ObjBrowser_Search_LostFocus);
			// 
			// groupBox_ProjExplorer
			// 
			this.groupBox_ProjExplorer.Controls.Add(this.button_OpenInExplorer);
			this.groupBox_ProjExplorer.Controls.Add(this.button_EditLanguages);
			this.groupBox_ProjExplorer.Controls.Add(this.button_EditScript);
			this.groupBox_ProjExplorer.Controls.Add(this.treeView_Files);
			this.groupBox_ProjExplorer.Controls.Add(this.comboBox_Projects);
			this.groupBox_ProjExplorer.Dock = System.Windows.Forms.DockStyle.Right;
			this.groupBox_ProjExplorer.Location = new System.Drawing.Point(744, 52);
			this.groupBox_ProjExplorer.Margin = new System.Windows.Forms.Padding(0);
			this.groupBox_ProjExplorer.Name = "groupBox_ProjExplorer";
			this.groupBox_ProjExplorer.Padding = new System.Windows.Forms.Padding(0);
			this.groupBox_ProjExplorer.Size = new System.Drawing.Size(200, 322);
			this.groupBox_ProjExplorer.TabIndex = 16;
			this.groupBox_ProjExplorer.TabStop = false;
			this.groupBox_ProjExplorer.Text = "Project Explorer";
			// 
			// treeView_Files
			// 
			this.treeView_Files.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.treeView_Files.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.treeView_Files.Location = new System.Drawing.Point(3, 78);
			this.treeView_Files.MaxDragChange = 20;
			this.treeView_Files.Name = "treeView_Files";
			this.treeView_Files.ShowIcons = true;
			this.treeView_Files.Size = new System.Drawing.Size(194, 212);
			this.treeView_Files.TabIndex = 1;
			this.treeView_Files.DoubleClick += new System.EventHandler(this.Explorer_TreeView_DoubleClick);
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
			this.label_Zoom.AutoSize = true;
			this.label_Zoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.label_Zoom.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Zoom.Location = new System.Drawing.Point(874, 547);
			this.label_Zoom.Name = "label_Zoom";
			this.label_Zoom.Size = new System.Drawing.Size(66, 13);
			this.label_Zoom.TabIndex = 8;
			this.label_Zoom.Text = "Zoom: 100%";
			// 
			// menu_Edit
			// 
			this.menu_Edit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menu_Edit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_Undo,
            this.menuItem_Redo,
            this.separator_03,
            this.menuItem_Cut,
            this.menuItem_Copy,
            this.menuItem_Paste,
            this.separator_04,
            this.menuItem_Find,
            this.menuItem_Replace,
            this.separator_05,
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
			this.menuItem_Undo.Image = global::ScriptEditor.Properties.Resources.general_undo_16;
			this.menuItem_Undo.Name = "menuItem_Undo";
			this.menuItem_Undo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
			this.menuItem_Undo.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Undo.Text = "&Undo";
			this.menuItem_Undo.Click += new System.EventHandler(this.Edit_Undo_Click);
			// 
			// menuItem_Redo
			// 
			this.menuItem_Redo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Redo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Redo.Image = global::ScriptEditor.Properties.Resources.general_redo_16;
			this.menuItem_Redo.Name = "menuItem_Redo";
			this.menuItem_Redo.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
			this.menuItem_Redo.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Redo.Text = "&Redo";
			this.menuItem_Redo.Click += new System.EventHandler(this.Edit_Redo_Click);
			// 
			// separator_03
			// 
			this.separator_03.Name = "separator_03";
			this.separator_03.Size = new System.Drawing.Size(164, 6);
			// 
			// menuItem_Cut
			// 
			this.menuItem_Cut.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Cut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Cut.Image = global::ScriptEditor.Properties.Resources.cut_16;
			this.menuItem_Cut.Name = "menuItem_Cut";
			this.menuItem_Cut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.menuItem_Cut.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Cut.Text = "Cu&t";
			this.menuItem_Cut.Click += new System.EventHandler(this.Edit_Cut_Click);
			// 
			// menuItem_Copy
			// 
			this.menuItem_Copy.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Copy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Copy.Image = global::ScriptEditor.Properties.Resources.general_copy_16;
			this.menuItem_Copy.Name = "menuItem_Copy";
			this.menuItem_Copy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.menuItem_Copy.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Copy.Text = "&Copy";
			this.menuItem_Copy.Click += new System.EventHandler(this.Edit_Copy_Click);
			// 
			// menuItem_Paste
			// 
			this.menuItem_Paste.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Paste.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Paste.Image = global::ScriptEditor.Properties.Resources.general_clipboard_16;
			this.menuItem_Paste.Name = "menuItem_Paste";
			this.menuItem_Paste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.menuItem_Paste.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Paste.Text = "&Paste";
			this.menuItem_Paste.Click += new System.EventHandler(this.Edit_Paste_Click);
			// 
			// separator_04
			// 
			this.separator_04.Name = "separator_04";
			this.separator_04.Size = new System.Drawing.Size(164, 6);
			// 
			// menuItem_Find
			// 
			this.menuItem_Find.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Find.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Find.Image = global::ScriptEditor.Properties.Resources.general_search_16;
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
			this.menuItem_Replace.Image = global::ScriptEditor.Properties.Resources.general_copy_16;
			this.menuItem_Replace.Name = "menuItem_Replace";
			this.menuItem_Replace.ShortcutKeyDisplayString = "Ctrl+H";
			this.menuItem_Replace.Size = new System.Drawing.Size(167, 22);
			this.menuItem_Replace.Text = "&Replace...";
			this.menuItem_Replace.Click += new System.EventHandler(this.Edit_Replace_Click);
			// 
			// separator_05
			// 
			this.separator_05.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_05.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_05.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_05.Name = "separator_05";
			this.separator_05.Size = new System.Drawing.Size(164, 6);
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
            this.menuItem_AddProject,
            this.separator_01,
            this.menuItem_Save,
            this.menuItem_Build,
            this.separator_02,
            this.menuItem_Exit});
			this.menu_File.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menu_File.Name = "menu_File";
			this.menu_File.Size = new System.Drawing.Size(37, 20);
			this.menu_File.Text = "&File";
			// 
			// menuItem_AddProject
			// 
			this.menuItem_AddProject.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_AddProject.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_AddProject.Image = global::ScriptEditor.Properties.Resources.general_Open_16;
			this.menuItem_AddProject.Name = "menuItem_AddProject";
			this.menuItem_AddProject.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
			this.menuItem_AddProject.Size = new System.Drawing.Size(190, 22);
			this.menuItem_AddProject.Text = "&New Project...";
			this.menuItem_AddProject.Click += new System.EventHandler(this.File_AddProject_Click);
			// 
			// separator_01
			// 
			this.separator_01.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_01.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_01.Name = "separator_01";
			this.separator_01.Size = new System.Drawing.Size(187, 6);
			// 
			// menuItem_Save
			// 
			this.menuItem_Save.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Save.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Save.Image = global::ScriptEditor.Properties.Resources.general_Save_16;
			this.menuItem_Save.Name = "menuItem_Save";
			this.menuItem_Save.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.menuItem_Save.Size = new System.Drawing.Size(190, 22);
			this.menuItem_Save.Text = "&Save Script";
			this.menuItem_Save.Click += new System.EventHandler(this.File_Save_Click);
			// 
			// menuItem_Build
			// 
			this.menuItem_Build.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Build.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Build.Image = global::ScriptEditor.Properties.Resources.play_16;
			this.menuItem_Build.Name = "menuItem_Build";
			this.menuItem_Build.ShortcutKeys = System.Windows.Forms.Keys.F9;
			this.menuItem_Build.Size = new System.Drawing.Size(190, 22);
			this.menuItem_Build.Text = "&Build Script";
			this.menuItem_Build.Click += new System.EventHandler(this.File_Build_Click);
			// 
			// separator_02
			// 
			this.separator_02.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_02.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_02.Name = "separator_02";
			this.separator_02.Size = new System.Drawing.Size(187, 6);
			// 
			// menuItem_Exit
			// 
			this.menuItem_Exit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Exit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Exit.Name = "menuItem_Exit";
			this.menuItem_Exit.ShortcutKeyDisplayString = "Alt+F4";
			this.menuItem_Exit.Size = new System.Drawing.Size(190, 22);
			this.menuItem_Exit.Text = "E&xit";
			this.menuItem_Exit.Click += new System.EventHandler(this.File_Exit_Click);
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
			this.menuItem_About.Image = ((System.Drawing.Image)(resources.GetObject("menuItem_About.Image")));
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
            this.separator_06,
            this.menuItem_Comment,
            this.menuItem_Uncomment,
            this.separator_07,
            this.menuItem_ToggleBookmark,
            this.menuItem_PrevBookmark,
            this.menuItem_NextBookmark,
            this.menuItem_ClearBookmarks,
            this.separator_08,
            this.menuItem_Settings});
			this.menu_Tools.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menu_Tools.Name = "menu_Tools";
			this.menu_Tools.Size = new System.Drawing.Size(47, 20);
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
			// separator_06
			// 
			this.separator_06.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_06.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_06.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_06.Name = "separator_06";
			this.separator_06.Size = new System.Drawing.Size(297, 6);
			// 
			// menuItem_Comment
			// 
			this.menuItem_Comment.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Comment.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Comment.Image = global::ScriptEditor.Properties.Resources.comment_16;
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
			this.menuItem_Uncomment.Image = global::ScriptEditor.Properties.Resources.uncomment_16;
			this.menuItem_Uncomment.Name = "menuItem_Uncomment";
			this.menuItem_Uncomment.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.U)));
			this.menuItem_Uncomment.Size = new System.Drawing.Size(300, 22);
			this.menuItem_Uncomment.Text = "&Uncomment Selected Lines";
			this.menuItem_Uncomment.Click += new System.EventHandler(this.Tools_Uncomment_Click);
			// 
			// separator_07
			// 
			this.separator_07.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_07.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_07.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_07.Name = "separator_07";
			this.separator_07.Size = new System.Drawing.Size(297, 6);
			// 
			// menuItem_ToggleBookmark
			// 
			this.menuItem_ToggleBookmark.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ToggleBookmark.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_ToggleBookmark.Image = global::ScriptEditor.Properties.Resources.toggleBookmark_16;
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
			this.menuItem_PrevBookmark.Image = global::ScriptEditor.Properties.Resources.prevBookmark_16;
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
			this.menuItem_NextBookmark.Image = global::ScriptEditor.Properties.Resources.nextBookmark_16;
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
			this.menuItem_ClearBookmarks.Image = global::ScriptEditor.Properties.Resources.clearBookmark_16;
			this.menuItem_ClearBookmarks.Name = "menuItem_ClearBookmarks";
			this.menuItem_ClearBookmarks.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.B)));
			this.menuItem_ClearBookmarks.Size = new System.Drawing.Size(300, 22);
			this.menuItem_ClearBookmarks.Text = "Clear &all Bookmarks";
			this.menuItem_ClearBookmarks.Click += new System.EventHandler(this.Tools_ClearBookmarks_Click);
			// 
			// separator_08
			// 
			this.separator_08.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_08.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_08.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_08.Name = "separator_08";
			this.separator_08.Size = new System.Drawing.Size(297, 6);
			// 
			// menuItem_Settings
			// 
			this.menuItem_Settings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_Settings.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_Settings.Image = global::ScriptEditor.Properties.Resources.general_settings_16;
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
            this.menuItem_ProjExplorer,
            this.menuItem_InfoBox,
            this.separator_17,
            this.menuItem_ToolStrip,
            this.menuItem_StatusStrip,
            this.separator_18,
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
			this.menuItem_ObjBrowser.Size = new System.Drawing.Size(159, 22);
			this.menuItem_ObjBrowser.Text = "&Object Browser";
			this.menuItem_ObjBrowser.Click += new System.EventHandler(this.View_ObjBrowser_Click);
			// 
			// menuItem_ProjExplorer
			// 
			this.menuItem_ProjExplorer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ProjExplorer.Checked = true;
			this.menuItem_ProjExplorer.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_ProjExplorer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_ProjExplorer.Name = "menuItem_ProjExplorer";
			this.menuItem_ProjExplorer.Size = new System.Drawing.Size(159, 22);
			this.menuItem_ProjExplorer.Text = "&Project Explorer";
			this.menuItem_ProjExplorer.Click += new System.EventHandler(this.View_ProjExplorer_Click);
			// 
			// menuItem_InfoBox
			// 
			this.menuItem_InfoBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_InfoBox.Checked = true;
			this.menuItem_InfoBox.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_InfoBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_InfoBox.Name = "menuItem_InfoBox";
			this.menuItem_InfoBox.Size = new System.Drawing.Size(159, 22);
			this.menuItem_InfoBox.Text = "&Information Box";
			this.menuItem_InfoBox.Click += new System.EventHandler(this.View_InfoBox_Click);
			// 
			// separator_17
			// 
			this.separator_17.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_17.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_17.Name = "separator_17";
			this.separator_17.Size = new System.Drawing.Size(156, 6);
			// 
			// menuItem_ToolStrip
			// 
			this.menuItem_ToolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ToolStrip.Checked = true;
			this.menuItem_ToolStrip.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_ToolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_ToolStrip.Name = "menuItem_ToolStrip";
			this.menuItem_ToolStrip.Size = new System.Drawing.Size(159, 22);
			this.menuItem_ToolStrip.Text = "Tool Strip";
			this.menuItem_ToolStrip.Click += new System.EventHandler(this.View_ToolStrip_Click);
			// 
			// menuItem_StatusStrip
			// 
			this.menuItem_StatusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_StatusStrip.Checked = true;
			this.menuItem_StatusStrip.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_StatusStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_StatusStrip.Name = "menuItem_StatusStrip";
			this.menuItem_StatusStrip.Size = new System.Drawing.Size(159, 22);
			this.menuItem_StatusStrip.Text = "Status Strip";
			this.menuItem_StatusStrip.Click += new System.EventHandler(this.View_StatusStrip_Click);
			// 
			// separator_18
			// 
			this.separator_18.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_18.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_18.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this.separator_18.Name = "separator_18";
			this.separator_18.Size = new System.Drawing.Size(156, 6);
			// 
			// menuItem_LineNumbers
			// 
			this.menuItem_LineNumbers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_LineNumbers.Checked = true;
			this.menuItem_LineNumbers.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_LineNumbers.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_LineNumbers.Name = "menuItem_LineNumbers";
			this.menuItem_LineNumbers.Size = new System.Drawing.Size(159, 22);
			this.menuItem_LineNumbers.Text = "Line Numbers";
			this.menuItem_LineNumbers.Click += new System.EventHandler(this.View_LineNumbers_Click);
			// 
			// menuItem_ToolTips
			// 
			this.menuItem_ToolTips.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.menuItem_ToolTips.Checked = true;
			this.menuItem_ToolTips.CheckState = System.Windows.Forms.CheckState.Checked;
			this.menuItem_ToolTips.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.menuItem_ToolTips.Name = "menuItem_ToolTips";
			this.menuItem_ToolTips.Size = new System.Drawing.Size(159, 22);
			this.menuItem_ToolTips.Text = "Tool Tips";
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
			this.menuStrip.Size = new System.Drawing.Size(944, 24);
			this.menuStrip.TabIndex = 0;
			// 
			// scriptFolderWatcher
			// 
			this.scriptFolderWatcher.EnableRaisingEvents = true;
			this.scriptFolderWatcher.IncludeSubdirectories = true;
			this.scriptFolderWatcher.NotifyFilter = ((System.IO.NotifyFilters)((((System.IO.NotifyFilters.FileName | System.IO.NotifyFilters.DirectoryName) 
            | System.IO.NotifyFilters.LastWrite) 
            | System.IO.NotifyFilters.LastAccess)));
			this.scriptFolderWatcher.SynchronizingObject = this;
			this.scriptFolderWatcher.Changed += new System.IO.FileSystemEventHandler(this.scriptFolderWatcher_Changed);
			this.scriptFolderWatcher.Created += new System.IO.FileSystemEventHandler(this.scriptFolderWatcher_Changed);
			this.scriptFolderWatcher.Deleted += new System.IO.FileSystemEventHandler(this.scriptFolderWatcher_Changed);
			this.scriptFolderWatcher.Renamed += new System.IO.RenamedEventHandler(this.scriptFolderWatcher_Renamed);
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
			// separator_12
			// 
			this.separator_12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.separator_12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.separator_12.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
			this.separator_12.Name = "separator_12";
			this.separator_12.Size = new System.Drawing.Size(6, 28);
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
			// splitter_Bottom
			// 
			this.splitter_Bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter_Bottom.Location = new System.Drawing.Point(0, 374);
			this.splitter_Bottom.Margin = new System.Windows.Forms.Padding(0);
			this.splitter_Bottom.MinSize = 160;
			this.splitter_Bottom.Name = "splitter_Bottom";
			this.splitter_Bottom.Size = new System.Drawing.Size(944, 5);
			this.splitter_Bottom.TabIndex = 13;
			this.splitter_Bottom.TabStop = false;
			// 
			// splitter_Left
			// 
			this.splitter_Left.Location = new System.Drawing.Point(200, 52);
			this.splitter_Left.Margin = new System.Windows.Forms.Padding(0);
			this.splitter_Left.MinSize = 200;
			this.splitter_Left.Name = "splitter_Left";
			this.splitter_Left.Size = new System.Drawing.Size(5, 322);
			this.splitter_Left.TabIndex = 12;
			this.splitter_Left.TabStop = false;
			// 
			// splitter_Right
			// 
			this.splitter_Right.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter_Right.Location = new System.Drawing.Point(739, 52);
			this.splitter_Right.Margin = new System.Windows.Forms.Padding(0);
			this.splitter_Right.MinSize = 200;
			this.splitter_Right.Name = "splitter_Right";
			this.splitter_Right.Size = new System.Drawing.Size(5, 322);
			this.splitter_Right.TabIndex = 18;
			this.splitter_Right.TabStop = false;
			this.splitter_Right.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.Explorer_Splitter_Moved);
			// 
			// statusStrip
			// 
			this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.statusStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.label_LineNumber,
            this.label_ColNumber,
            this.label_SelectedChars});
			this.statusStrip.Location = new System.Drawing.Point(0, 539);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
			this.statusStrip.Size = new System.Drawing.Size(944, 28);
			this.statusStrip.TabIndex = 2;
			this.statusStrip.Text = "statusStrip";
			// 
			// toolStrip
			// 
			this.toolStrip.AutoSize = false;
			this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.toolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.button_AddProject,
            this.separator_09,
            this.button_Save,
            this.button_Build,
            this.separator_10,
            this.button_Undo,
            this.button_Redo,
            this.separator_11,
            this.button_Cut,
            this.button_Copy,
            this.button_Paste,
            this.separator_12,
            this.button_Comment,
            this.button_Uncomment,
            this.separator_13,
            this.button_ToggleBookmark,
            this.button_PrevBookmark,
            this.button_NextBookmark,
            this.button_ClearBookmarks,
            this.separator_14,
            this.button_About});
			this.toolStrip.Location = new System.Drawing.Point(0, 24);
			this.toolStrip.Name = "toolStrip";
			this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
			this.toolStrip.Size = new System.Drawing.Size(944, 28);
			this.toolStrip.TabIndex = 1;
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(944, 567);
			this.Controls.Add(this.splitter_Right);
			this.Controls.Add(this.groupBox_Editor);
			this.Controls.Add(this.groupBox_ProjExplorer);
			this.Controls.Add(this.splitter_Left);
			this.Controls.Add(this.button_ResetZoom);
			this.Controls.Add(this.label_Zoom);
			this.Controls.Add(this.groupBox_ObjBrowser);
			this.Controls.Add(this.toolStrip);
			this.Controls.Add(this.menuStrip);
			this.Controls.Add(this.splitter_Bottom);
			this.Controls.Add(this.groupBox_InfoBox);
			this.Controls.Add(this.statusStrip);
			this.MainMenuStrip = this.menuStrip;
			this.MinimumSize = new System.Drawing.Size(800, 450);
			this.Name = "FormMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Script Editor";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.contextMenu_TextBox.ResumeLayout(false);
			this.groupBox_Editor.ResumeLayout(false);
			this.groupBox_InfoBox.ResumeLayout(false);
			this.tabControl_Info.ResumeLayout(false);
			this.tabPage_RefBrowser.ResumeLayout(false);
			this.groupBox_ObjBrowser.ResumeLayout(false);
			this.groupBox_ObjBrowser.PerformLayout();
			this.groupBox_ProjExplorer.ResumeLayout(false);
			this.menuStrip.ResumeLayout(false);
			this.menuStrip.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.scriptFolderWatcher)).EndInit();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.toolStrip.ResumeLayout(false);
			this.toolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

		#endregion

		private DarkUI.Controls.DarkMenuStrip menuStrip;
		private System.Windows.Forms.ToolStripMenuItem menu_File;
		private System.Windows.Forms.ToolStripMenuItem menu_Edit;
		private System.Windows.Forms.ToolStripMenuItem menu_Tools;
		private System.Windows.Forms.ToolStripMenuItem menu_View;
		private System.Windows.Forms.ToolStripMenuItem menu_Help;
		private System.Windows.Forms.ToolStripMenuItem menuItem_AddProject;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Save;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Build;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Exit;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Undo;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Redo;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Cut;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Copy;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Paste;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Find;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Replace;
		private System.Windows.Forms.ToolStripMenuItem menuItem_SelectAll;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Reindent;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Trim;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Comment;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Uncomment;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ToggleBookmark;
		private System.Windows.Forms.ToolStripMenuItem menuItem_PrevBookmark;
		private System.Windows.Forms.ToolStripMenuItem menuItem_NextBookmark;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ClearBookmarks;
		private System.Windows.Forms.ToolStripMenuItem menuItem_Settings;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ToolStrip;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ObjBrowser;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ProjExplorer;
		private System.Windows.Forms.ToolStripMenuItem menuItem_InfoBox;
		private System.Windows.Forms.ToolStripMenuItem menuItem_StatusStrip;
		private System.Windows.Forms.ToolStripMenuItem menuItem_LineNumbers;
		private System.Windows.Forms.ToolStripMenuItem menuItem_ToolTips;
		private System.Windows.Forms.ToolStripMenuItem menuItem_About;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private System.Windows.Forms.ToolStripButton button_AddProject;
		private System.Windows.Forms.ToolStripButton button_Save;
		private System.Windows.Forms.ToolStripButton button_Build;
		private System.Windows.Forms.ToolStripButton button_Undo;
		private System.Windows.Forms.ToolStripButton button_Redo;
		private System.Windows.Forms.ToolStripButton button_Cut;
		private System.Windows.Forms.ToolStripButton button_Copy;
		private System.Windows.Forms.ToolStripButton button_Paste;
		private System.Windows.Forms.ToolStripButton button_Comment;
		private System.Windows.Forms.ToolStripButton button_Uncomment;
		private System.Windows.Forms.ToolStripButton button_ToggleBookmark;
		private System.Windows.Forms.ToolStripButton button_PrevBookmark;
		private System.Windows.Forms.ToolStripButton button_NextBookmark;
		private System.Windows.Forms.ToolStripButton button_ClearBookmarks;
		private System.Windows.Forms.ToolStripButton button_About;
		private DarkUI.Controls.DarkContextMenu contextMenu_TextBox;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_Cut;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_Copy;
        private System.Windows.Forms.ToolStripMenuItem contextMenuItem_Paste;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_Comment;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_Uncomment;
		private System.Windows.Forms.ToolStripMenuItem contextMenuItem_ToggleBookmark;
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
		private DarkUI.Controls.DarkGroupBox groupBox_Editor;
		private DarkUI.Controls.DarkGroupBox groupBox_ObjBrowser;
		private DarkUI.Controls.DarkGroupBox groupBox_ProjExplorer;
		private DarkUI.Controls.DarkGroupBox groupBox_InfoBox;
		private System.Windows.Forms.Splitter splitter_Left;
		private System.Windows.Forms.Splitter splitter_Right;
		private System.Windows.Forms.Splitter splitter_Bottom;
		private DarkUI.Controls.DarkStatusStrip statusStrip;
		private System.Windows.Forms.ToolStripStatusLabel label_LineNumber;
		private System.Windows.Forms.ToolStripStatusLabel label_ColNumber;
		private System.Windows.Forms.ToolStripStatusLabel label_SelectedChars;
		private DarkUI.Controls.DarkLabel label_Zoom;
		private DarkUI.Controls.DarkButton button_ResetZoom;
		private DarkUI.Controls.DarkTextBox textBox_ObjBrowserSearch;
		private DarkUI.Controls.DarkTreeView treeView_ObjBrowser;
		private DarkUI.Controls.DarkComboBox comboBox_Projects;
		private DarkUI.Controls.DarkTreeView treeView_Files;
		private DarkUI.Controls.DarkButton button_EditScript;
		private DarkUI.Controls.DarkButton button_EditLanguages;
		private DarkUI.Controls.DarkButton button_OpenInExplorer;
		private System.Windows.Forms.CustomTabControl tabControl_Editor;
		private System.Windows.Forms.CustomTabControl tabControl_Info;
		private System.Windows.Forms.TabPage tabPage_CompilerLogs;
		private System.Windows.Forms.TabPage tabPage_RefBrowser;
		private ReferenceBrowser referenceBrowser;
		private System.IO.FileSystemWatcher scriptFolderWatcher;
	}
}

