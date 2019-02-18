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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.aboutToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.autosaveLabel = new DarkUI.Controls.DarkLabel();
            this.buildToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.buildToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.changeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearBookmarksToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.clearBookmarksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colNumberLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.commentContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.commentToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.commentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyContextItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutContextItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.documentMap = new FastColoredTextBoxNS.DocumentMap();
            this.textEditor = new FastColoredTextBoxNS.FastColoredTextBox();
            this.editorContextMenu = new DarkUI.Controls.DarkContextMenu();
            this.pasteContextItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_15 = new System.Windows.Forms.ToolStripSeparator();
            this.uncommentContextMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_16 = new System.Windows.Forms.ToolStripSeparator();
            this.toggleBookmarkContextItem = new System.Windows.Forms.ToolStripMenuItem();
            this.documentMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.undoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.redoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_03 = new System.Windows.Forms.ToolStripSeparator();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_04 = new System.Windows.Forms.ToolStripSeparator();
            this.findToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.replaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_05 = new System.Windows.Forms.ToolStripSeparator();
            this.selectAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_01 = new System.Windows.Forms.ToolStripSeparator();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_02 = new System.Windows.Forms.ToolStripSeparator();
            this.showStringTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_17 = new System.Windows.Forms.ToolStripSeparator();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lineNumberLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip = new DarkUI.Controls.DarkMenuStrip();
            this.toolsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.reindentScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.trimWhitespaceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_06 = new System.Windows.Forms.ToolStripSeparator();
            this.uncommentToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_07 = new System.Windows.Forms.ToolStripSeparator();
            this.toggleBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.prevBookmarkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator_08 = new System.Windows.Forms.ToolStripSeparator();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.objectBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.referenceBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextBookmarkToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.objectBrowser = new DarkUI.Controls.DarkTreeView();
            this.objectBrowserBox = new DarkUI.Controls.DarkGroupBox();
            this.searchTextBox = new DarkUI.Controls.DarkTextBox();
            this.objectBrowserSplitter = new System.Windows.Forms.Splitter();
            this.pasteToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.prevBookmarkToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.redoToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.refBrowserSplitter = new System.Windows.Forms.Splitter();
            this.referenceBrowser = new ScriptEditor.ReferenceBrowser();
            this.resetZoomButton = new DarkUI.Controls.DarkButton();
            this.saveToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.selectedCharsLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.showStringTableButton = new DarkUI.Controls.DarkButton();
            this.statusStrip = new DarkUI.Controls.DarkStatusStrip();
            this.toggleBookmarkToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStrip = new DarkUI.Controls.DarkToolStrip();
            this.toolStripSeparator_09 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator_10 = new System.Windows.Forms.ToolStripSeparator();
            this.undoToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator_11 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator_12 = new System.Windows.Forms.ToolStripSeparator();
            this.uncommentToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator_13 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator_14 = new System.Windows.Forms.ToolStripSeparator();
            this.zoomLabel = new DarkUI.Controls.DarkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.textEditor)).BeginInit();
            this.editorContextMenu.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.objectBrowserBox.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // aboutToolStripButton
            // 
            this.aboutToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.aboutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.aboutToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.aboutToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("aboutToolStripButton.Image")));
            this.aboutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.aboutToolStripButton.Name = "aboutToolStripButton";
            this.aboutToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.aboutToolStripButton.Text = "About (F1)";
            this.aboutToolStripButton.Click += new System.EventHandler(this.ToolStrip_AboutButton_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.aboutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.aboutToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("aboutToolStripMenuItem.Image")));
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutToolStripMenuItem.Text = "&About...";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.Help_About_MenuItem_Click);
            // 
            // autosaveLabel
            // 
            this.autosaveLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.autosaveLabel.AutoSize = true;
            this.autosaveLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.autosaveLabel.Location = new System.Drawing.Point(528, 547);
            this.autosaveLabel.Name = "autosaveLabel";
            this.autosaveLabel.Size = new System.Drawing.Size(265, 13);
            this.autosaveLabel.TabIndex = 14;
            this.autosaveLabel.Text = "Autosave Completed! (CURRENT TIME GOES HERE)";
            this.autosaveLabel.Visible = false;
            // 
            // buildToolStripButton
            // 
            this.buildToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.buildToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.buildToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.buildToolStripButton.Image = global::ScriptEditor.Properties.Resources.play_16;
            this.buildToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buildToolStripButton.Name = "buildToolStripButton";
            this.buildToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.buildToolStripButton.Text = "Build (F9)";
            // 
            // buildToolStripMenuItem
            // 
            this.buildToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.buildToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.buildToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.play_16;
            this.buildToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.buildToolStripMenuItem.Name = "buildToolStripMenuItem";
            this.buildToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F9;
            this.buildToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.buildToolStripMenuItem.Text = "&Build Script";
            this.buildToolStripMenuItem.Click += new System.EventHandler(this.buildToolStripMenuItem_Click);
            // 
            // changeToolStripButton
            // 
            this.changeToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.changeToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.changeToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.changeToolStripButton.Image = global::ScriptEditor.Properties.Resources.general_Open_16;
            this.changeToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.changeToolStripButton.Name = "changeToolStripButton";
            this.changeToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.changeToolStripButton.Text = "Change Paths... (Ctrl+O)";
            this.changeToolStripButton.Click += new System.EventHandler(this.ToolStrip_ChangeButton_Click);
            // 
            // changeToolStripMenuItem
            // 
            this.changeToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.changeToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.changeToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.general_Open_16;
            this.changeToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.changeToolStripMenuItem.Name = "changeToolStripMenuItem";
            this.changeToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.changeToolStripMenuItem.Size = new System.Drawing.Size(271, 22);
            this.changeToolStripMenuItem.Text = "&Change Paths && compiler...";
            this.changeToolStripMenuItem.Click += new System.EventHandler(this.File_Change_MenuItem_Click);
            // 
            // clearBookmarksToolStripButton
            // 
            this.clearBookmarksToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.clearBookmarksToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.clearBookmarksToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.clearBookmarksToolStripButton.Image = global::ScriptEditor.Properties.Resources.clearBookmark_16;
            this.clearBookmarksToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.clearBookmarksToolStripButton.Name = "clearBookmarksToolStripButton";
            this.clearBookmarksToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.clearBookmarksToolStripButton.Text = "Clear all Bookmarks (Ctrl+Shift+B)";
            this.clearBookmarksToolStripButton.Click += new System.EventHandler(this.ToolStrip_ClearAllBookmarksButton_Click);
            // 
            // clearBookmarksToolStripMenuItem
            // 
            this.clearBookmarksToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.clearBookmarksToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.clearBookmarksToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.clearBookmark_16;
            this.clearBookmarksToolStripMenuItem.Name = "clearBookmarksToolStripMenuItem";
            this.clearBookmarksToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.B)));
            this.clearBookmarksToolStripMenuItem.Size = new System.Drawing.Size(329, 22);
            this.clearBookmarksToolStripMenuItem.Text = "Clear &all Bookmarks";
            this.clearBookmarksToolStripMenuItem.Click += new System.EventHandler(this.Tools_ClearAllBookmarks_MenuItem_Click);
            // 
            // colNumberLabel
            // 
            this.colNumberLabel.Name = "colNumberLabel";
            this.colNumberLabel.Size = new System.Drawing.Size(62, 15);
            this.colNumberLabel.Text = "Column: 0";
            // 
            // commentContextMenuItem
            // 
            this.commentContextMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.commentContextMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.commentContextMenuItem.Image = global::ScriptEditor.Properties.Resources.comment_16;
            this.commentContextMenuItem.Name = "commentContextMenuItem";
            this.commentContextMenuItem.Size = new System.Drawing.Size(179, 22);
            this.commentContextMenuItem.Text = "Comment out Lines";
            this.commentContextMenuItem.Click += new System.EventHandler(this.ContextMenu_CommentItem_Click);
            // 
            // commentToolStripButton
            // 
            this.commentToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.commentToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.commentToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.commentToolStripButton.Image = global::ScriptEditor.Properties.Resources.comment_16;
            this.commentToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.commentToolStripButton.Name = "commentToolStripButton";
            this.commentToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.commentToolStripButton.Text = "Comment out Lines (Ctrl+Shift+C)";
            this.commentToolStripButton.ToolTipText = "Comment out Lines";
            this.commentToolStripButton.Click += new System.EventHandler(this.ToolStrip_CommentButton_Click);
            // 
            // commentToolStripMenuItem
            // 
            this.commentToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.commentToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.commentToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.comment_16;
            this.commentToolStripMenuItem.Name = "commentToolStripMenuItem";
            this.commentToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.C)));
            this.commentToolStripMenuItem.Size = new System.Drawing.Size(329, 22);
            this.commentToolStripMenuItem.Text = "&Comment out Selected Lines";
            this.commentToolStripMenuItem.Click += new System.EventHandler(this.Tools_CommentLines_MenuItem_Click);
            // 
            // copyContextItem
            // 
            this.copyContextItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.copyContextItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.copyContextItem.Image = global::ScriptEditor.Properties.Resources.general_copy_16;
            this.copyContextItem.Name = "copyContextItem";
            this.copyContextItem.Size = new System.Drawing.Size(179, 22);
            this.copyContextItem.Text = "Copy";
            this.copyContextItem.Click += new System.EventHandler(this.ContextMenu_CopyItem_Click);
            // 
            // copyToolStripButton
            // 
            this.copyToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.copyToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.copyToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.copyToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("copyToolStripButton.Image")));
            this.copyToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolStripButton.Name = "copyToolStripButton";
            this.copyToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.copyToolStripButton.Text = "Copy (Ctrl+C)";
            this.copyToolStripButton.Click += new System.EventHandler(this.ToolStrip_CopyButton_Click);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.copyToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.copyToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.general_copy_16;
            this.copyToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.copyToolStripMenuItem.Text = "&Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.Edit_Copy_MenuItem_Click);
            // 
            // cutContextItem
            // 
            this.cutContextItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cutContextItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cutContextItem.Image = global::ScriptEditor.Properties.Resources.cut_16;
            this.cutContextItem.Name = "cutContextItem";
            this.cutContextItem.Size = new System.Drawing.Size(179, 22);
            this.cutContextItem.Text = "Cut";
            this.cutContextItem.Click += new System.EventHandler(this.ContextMenu_CutItem_Click);
            // 
            // cutToolStripButton
            // 
            this.cutToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cutToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cutToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cutToolStripButton.Image = global::ScriptEditor.Properties.Resources.cut_16;
            this.cutToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cutToolStripButton.Name = "cutToolStripButton";
            this.cutToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.cutToolStripButton.Text = "Cut (Ctrl+X)";
            this.cutToolStripButton.Click += new System.EventHandler(this.ToolStrip_CutButton_Click);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.cutToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.cutToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.cut_16;
            this.cutToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.cutToolStripMenuItem.Text = "Cu&t";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.Edit_Cut_MenuItem_Click);
            // 
            // documentMap
            // 
            this.documentMap.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.documentMap.Dock = System.Windows.Forms.DockStyle.Right;
            this.documentMap.ForeColor = System.Drawing.Color.Gainsboro;
            this.documentMap.Location = new System.Drawing.Point(816, 52);
            this.documentMap.Name = "documentMap";
            this.documentMap.Size = new System.Drawing.Size(128, 324);
            this.documentMap.TabIndex = 1;
            this.documentMap.Target = this.textEditor;
            this.documentMap.Text = "documentMap";
            // 
            // textEditor
            // 
            this.textEditor.AutoCompleteBracketsList = new char[] {
        '[',
        ']'};
            this.textEditor.AutoIndent = false;
            this.textEditor.AutoIndentChars = false;
            this.textEditor.AutoIndentExistingLines = false;
            this.textEditor.AutoScrollMinSize = new System.Drawing.Size(43, 18);
            this.textEditor.BackBrush = null;
            this.textEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.textEditor.BookmarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.textEditor.CaretColor = System.Drawing.Color.Gainsboro;
            this.textEditor.ChangedLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(96)))));
            this.textEditor.CharHeight = 18;
            this.textEditor.CharWidth = 9;
            this.textEditor.CommentPrefix = ";";
            this.textEditor.ContextMenuStrip = this.editorContextMenu;
            this.textEditor.CurrentLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.textEditor.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textEditor.DelayedEventsInterval = 200;
            this.textEditor.DelayedTextChangedInterval = 500;
            this.textEditor.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.textEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textEditor.Font = new System.Drawing.Font("Consolas", 12F);
            this.textEditor.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.textEditor.IndentBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.textEditor.IsReplaceMode = false;
            this.textEditor.LeftPadding = 5;
            this.textEditor.LineNumberColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.textEditor.Location = new System.Drawing.Point(200, 52);
            this.textEditor.Name = "textEditor";
            this.textEditor.Paddings = new System.Windows.Forms.Padding(0);
            this.textEditor.PreferredLineWidth = 80;
            this.textEditor.ReservedCountOfLineNumberChars = 2;
            this.textEditor.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(30)))), ((int)(((byte)(144)))), ((int)(((byte)(255)))));
            this.textEditor.ServiceColors = null;
            this.textEditor.ServiceLinesColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.textEditor.Size = new System.Drawing.Size(616, 324);
            this.textEditor.TabIndex = 3;
            this.textEditor.Zoom = 100;
            this.textEditor.ToolTipNeeded += new System.EventHandler<FastColoredTextBoxNS.ToolTipNeededEventArgs>(this.textEditor_ToolTipNeeded);
            this.textEditor.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.textEditor_TextChanged);
            this.textEditor.SelectionChanged += new System.EventHandler(this.textEditor_SelectionChanged);
            this.textEditor.UndoRedoStateChanged += new System.EventHandler<System.EventArgs>(this.UndoRedoState_Changed);
            this.textEditor.ZoomChanged += new System.EventHandler(this.textEditor_ZoomChanged);
            this.textEditor.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textEditor_KeyPress);
            this.textEditor.KeyUp += new System.Windows.Forms.KeyEventHandler(this.textEditor_KeyUp);
            this.textEditor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textEditor_MouseDown);
            // 
            // editorContextMenu
            // 
            this.editorContextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.editorContextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.editorContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutContextItem,
            this.copyContextItem,
            this.pasteContextItem,
            this.toolStripSeparator_15,
            this.commentContextMenuItem,
            this.uncommentContextMenuItem,
            this.toolStripSeparator_16,
            this.toggleBookmarkContextItem});
            this.editorContextMenu.Name = "editorContextMenu";
            this.editorContextMenu.Size = new System.Drawing.Size(180, 150);
            // 
            // pasteContextItem
            // 
            this.pasteContextItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pasteContextItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pasteContextItem.Image = global::ScriptEditor.Properties.Resources.general_clipboard_16;
            this.pasteContextItem.Name = "pasteContextItem";
            this.pasteContextItem.Size = new System.Drawing.Size(179, 22);
            this.pasteContextItem.Text = "Paste";
            this.pasteContextItem.Click += new System.EventHandler(this.ContextMenu_PasteItem_Click);
            // 
            // toolStripSeparator_15
            // 
            this.toolStripSeparator_15.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_15.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_15.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator_15.Name = "toolStripSeparator_15";
            this.toolStripSeparator_15.Size = new System.Drawing.Size(176, 6);
            // 
            // uncommentContextMenuItem
            // 
            this.uncommentContextMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.uncommentContextMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.uncommentContextMenuItem.Image = global::ScriptEditor.Properties.Resources.uncomment_16;
            this.uncommentContextMenuItem.Name = "uncommentContextMenuItem";
            this.uncommentContextMenuItem.Size = new System.Drawing.Size(179, 22);
            this.uncommentContextMenuItem.Text = "Uncomment Lines";
            this.uncommentContextMenuItem.Click += new System.EventHandler(this.ContextMenu_UncommentItem_Click);
            // 
            // toolStripSeparator_16
            // 
            this.toolStripSeparator_16.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_16.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator_16.Name = "toolStripSeparator_16";
            this.toolStripSeparator_16.Size = new System.Drawing.Size(176, 6);
            // 
            // toggleBookmarkContextItem
            // 
            this.toggleBookmarkContextItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toggleBookmarkContextItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toggleBookmarkContextItem.Image = global::ScriptEditor.Properties.Resources.toggleBookmark_16;
            this.toggleBookmarkContextItem.Name = "toggleBookmarkContextItem";
            this.toggleBookmarkContextItem.Size = new System.Drawing.Size(179, 22);
            this.toggleBookmarkContextItem.Text = "Toggle Bookmark";
            this.toggleBookmarkContextItem.Click += new System.EventHandler(this.ContextMenu_ToggleBookmark_Click);
            // 
            // documentMapToolStripMenuItem
            // 
            this.documentMapToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.documentMapToolStripMenuItem.Checked = true;
            this.documentMapToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.documentMapToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.documentMapToolStripMenuItem.Name = "documentMapToolStripMenuItem";
            this.documentMapToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.documentMapToolStripMenuItem.Text = "&Document Map";
            this.documentMapToolStripMenuItem.Click += new System.EventHandler(this.View_DocumentMap_MenuItem_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.undoToolStripMenuItem,
            this.redoToolStripMenuItem,
            this.toolStripSeparator_03,
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator_04,
            this.findToolStripMenuItem,
            this.replaceToolStripMenuItem,
            this.toolStripSeparator_05,
            this.selectAllToolStripMenuItem});
            this.editToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "&Edit";
            // 
            // undoToolStripMenuItem
            // 
            this.undoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.undoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.undoToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.general_undo_16;
            this.undoToolStripMenuItem.Name = "undoToolStripMenuItem";
            this.undoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
            this.undoToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.undoToolStripMenuItem.Text = "&Undo";
            this.undoToolStripMenuItem.Click += new System.EventHandler(this.Edit_Undo_MenuItem_Click);
            // 
            // redoToolStripMenuItem
            // 
            this.redoToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.redoToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.redoToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.general_redo_16;
            this.redoToolStripMenuItem.Name = "redoToolStripMenuItem";
            this.redoToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
            this.redoToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.redoToolStripMenuItem.Text = "&Redo";
            this.redoToolStripMenuItem.Click += new System.EventHandler(this.Edit_Redo_MenuItem_Click);
            // 
            // toolStripSeparator_03
            // 
            this.toolStripSeparator_03.Name = "toolStripSeparator_03";
            this.toolStripSeparator_03.Size = new System.Drawing.Size(164, 6);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pasteToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pasteToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.general_clipboard_16;
            this.pasteToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.pasteToolStripMenuItem.Text = "&Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.Edit_Paste_MenuItem_Click);
            // 
            // toolStripSeparator_04
            // 
            this.toolStripSeparator_04.Name = "toolStripSeparator_04";
            this.toolStripSeparator_04.Size = new System.Drawing.Size(164, 6);
            // 
            // findToolStripMenuItem
            // 
            this.findToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.findToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.findToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.general_search_16;
            this.findToolStripMenuItem.Name = "findToolStripMenuItem";
            this.findToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+F";
            this.findToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.findToolStripMenuItem.Text = "&Find...";
            this.findToolStripMenuItem.Click += new System.EventHandler(this.Edit_Find_MenuItem_Click);
            // 
            // replaceToolStripMenuItem
            // 
            this.replaceToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.replaceToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.replaceToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.general_copy_16;
            this.replaceToolStripMenuItem.Name = "replaceToolStripMenuItem";
            this.replaceToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+H";
            this.replaceToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.replaceToolStripMenuItem.Text = "&Replace...";
            this.replaceToolStripMenuItem.Click += new System.EventHandler(this.Edit_Replace_MenuItem_Click);
            // 
            // toolStripSeparator_05
            // 
            this.toolStripSeparator_05.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_05.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_05.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator_05.Name = "toolStripSeparator_05";
            this.toolStripSeparator_05.Size = new System.Drawing.Size(164, 6);
            // 
            // selectAllToolStripMenuItem
            // 
            this.selectAllToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.selectAllToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
            this.selectAllToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+A";
            this.selectAllToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.selectAllToolStripMenuItem.Text = "Select &All";
            this.selectAllToolStripMenuItem.Click += new System.EventHandler(this.Edit_SelectAll_MenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.exitToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.ShortcutKeyDisplayString = "Alt+F4";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.File_Exit_MenuItem_Click);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeToolStripMenuItem,
            this.toolStripSeparator_01,
            this.saveToolStripMenuItem,
            this.buildToolStripMenuItem,
            this.toolStripSeparator_02,
            this.showStringTableToolStripMenuItem,
            this.toolStripSeparator_17,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // toolStripSeparator_01
            // 
            this.toolStripSeparator_01.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_01.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator_01.Name = "toolStripSeparator_01";
            this.toolStripSeparator_01.Size = new System.Drawing.Size(221, 6);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.saveToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.saveToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.general_Save_16;
            this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.saveToolStripMenuItem.Text = "&Save Script";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.File_Save_MenuItem_Click);
            // 
            // toolStripSeparator_02
            // 
            this.toolStripSeparator_02.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_02.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator_02.Name = "toolStripSeparator_02";
            this.toolStripSeparator_02.Size = new System.Drawing.Size(221, 6);
            // 
            // showStringTableToolStripMenuItem
            // 
            this.showStringTableToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.showStringTableToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.showStringTableToolStripMenuItem.Name = "showStringTableToolStripMenuItem";
            this.showStringTableToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.showStringTableToolStripMenuItem.Size = new System.Drawing.Size(224, 22);
            this.showStringTableToolStripMenuItem.Text = "Show string table...";
            this.showStringTableToolStripMenuItem.Click += new System.EventHandler(this.File_StringTable_MenuItem_Click);
            // 
            // toolStripSeparator_17
            // 
            this.toolStripSeparator_17.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_17.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_17.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator_17.Name = "toolStripSeparator_17";
            this.toolStripSeparator_17.Size = new System.Drawing.Size(221, 6);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // lineNumberLabel
            // 
            this.lineNumberLabel.Name = "lineNumberLabel";
            this.lineNumberLabel.Size = new System.Drawing.Size(41, 15);
            this.lineNumberLabel.Text = "Line: 0";
            // 
            // menuStrip
            // 
            this.menuStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.menuStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem1,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this.menuStrip.Size = new System.Drawing.Size(944, 24);
            this.menuStrip.TabIndex = 0;
            this.menuStrip.Text = "menuStrip";
            // 
            // toolsToolStripMenuItem1
            // 
            this.toolsToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolsToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.reindentScriptToolStripMenuItem,
            this.trimWhitespaceToolStripMenuItem,
            this.toolStripSeparator_06,
            this.commentToolStripMenuItem,
            this.uncommentToolStripMenuItem,
            this.toolStripSeparator_07,
            this.toggleBookmarkToolStripMenuItem,
            this.nextBookmarkToolStripMenuItem,
            this.prevBookmarkToolStripMenuItem,
            this.clearBookmarksToolStripMenuItem,
            this.toolStripSeparator_08,
            this.settingsToolStripMenuItem});
            this.toolsToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolsToolStripMenuItem1.Name = "toolsToolStripMenuItem1";
            this.toolsToolStripMenuItem1.Size = new System.Drawing.Size(47, 20);
            this.toolsToolStripMenuItem1.Text = "&Tools";
            // 
            // reindentScriptToolStripMenuItem
            // 
            this.reindentScriptToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.reindentScriptToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.reindentScriptToolStripMenuItem.Name = "reindentScriptToolStripMenuItem";
            this.reindentScriptToolStripMenuItem.ShortcutKeyDisplayString = "";
            this.reindentScriptToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.reindentScriptToolStripMenuItem.Size = new System.Drawing.Size(329, 22);
            this.reindentScriptToolStripMenuItem.Text = "&Reindent Script";
            this.reindentScriptToolStripMenuItem.Click += new System.EventHandler(this.Tools_ReindentScript_MenuItem_Click);
            // 
            // trimWhitespaceToolStripMenuItem
            // 
            this.trimWhitespaceToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.trimWhitespaceToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.trimWhitespaceToolStripMenuItem.Name = "trimWhitespaceToolStripMenuItem";
            this.trimWhitespaceToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.R)));
            this.trimWhitespaceToolStripMenuItem.Size = new System.Drawing.Size(329, 22);
            this.trimWhitespaceToolStripMenuItem.Text = "&Trim Ending Whitespace";
            this.trimWhitespaceToolStripMenuItem.Click += new System.EventHandler(this.Tools_TrimWhitespace_MenuItem_Click);
            // 
            // toolStripSeparator_06
            // 
            this.toolStripSeparator_06.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_06.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_06.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator_06.Name = "toolStripSeparator_06";
            this.toolStripSeparator_06.Size = new System.Drawing.Size(326, 6);
            // 
            // uncommentToolStripMenuItem
            // 
            this.uncommentToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.uncommentToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.uncommentToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.uncomment_16;
            this.uncommentToolStripMenuItem.Name = "uncommentToolStripMenuItem";
            this.uncommentToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
            | System.Windows.Forms.Keys.U)));
            this.uncommentToolStripMenuItem.Size = new System.Drawing.Size(329, 22);
            this.uncommentToolStripMenuItem.Text = "&Uncomment Selected Lines";
            this.uncommentToolStripMenuItem.Click += new System.EventHandler(this.Tools_Uncomment_MenuItem_Click);
            // 
            // toolStripSeparator_07
            // 
            this.toolStripSeparator_07.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_07.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_07.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator_07.Name = "toolStripSeparator_07";
            this.toolStripSeparator_07.Size = new System.Drawing.Size(326, 6);
            // 
            // toggleBookmarkToolStripMenuItem
            // 
            this.toggleBookmarkToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toggleBookmarkToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toggleBookmarkToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.toggleBookmark_16;
            this.toggleBookmarkToolStripMenuItem.Name = "toggleBookmarkToolStripMenuItem";
            this.toggleBookmarkToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.toggleBookmarkToolStripMenuItem.Size = new System.Drawing.Size(329, 22);
            this.toggleBookmarkToolStripMenuItem.Text = "Toggle &Bookmark";
            this.toggleBookmarkToolStripMenuItem.Click += new System.EventHandler(this.Tools_ToggleBookmark_MenuItem_Click);
            // 
            // nextBookmarkToolStripMenuItem
            // 
            this.nextBookmarkToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.nextBookmarkToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nextBookmarkToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.nextBookmark_16;
            this.nextBookmarkToolStripMenuItem.Name = "nextBookmarkToolStripMenuItem";
            this.nextBookmarkToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right)));
            this.nextBookmarkToolStripMenuItem.Size = new System.Drawing.Size(329, 22);
            this.nextBookmarkToolStripMenuItem.Text = "Go To &Next Bookmark";
            this.nextBookmarkToolStripMenuItem.Click += new System.EventHandler(this.Tools_NextBookmark_MenuItem_Click);
            // 
            // prevBookmarkToolStripMenuItem
            // 
            this.prevBookmarkToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.prevBookmarkToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.prevBookmarkToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.prevBookmark_16;
            this.prevBookmarkToolStripMenuItem.Name = "prevBookmarkToolStripMenuItem";
            this.prevBookmarkToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Left)));
            this.prevBookmarkToolStripMenuItem.Size = new System.Drawing.Size(329, 22);
            this.prevBookmarkToolStripMenuItem.Text = "Go To &Previous Bookmark";
            this.prevBookmarkToolStripMenuItem.Click += new System.EventHandler(this.Tools_PrevBookmark_MenuItem_Click);
            // 
            // toolStripSeparator_08
            // 
            this.toolStripSeparator_08.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_08.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_08.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator_08.Name = "toolStripSeparator_08";
            this.toolStripSeparator_08.Size = new System.Drawing.Size(326, 6);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.settingsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.settingsToolStripMenuItem.Image = global::ScriptEditor.Properties.Resources.general_settings_16;
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(329, 22);
            this.settingsToolStripMenuItem.Text = "&Settings...";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.Tools_Settings_MenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.objectBrowserToolStripMenuItem,
            this.referenceBrowserToolStripMenuItem,
            this.documentMapToolStripMenuItem});
            this.toolsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.toolsToolStripMenuItem.Text = "&View";
            // 
            // objectBrowserToolStripMenuItem
            // 
            this.objectBrowserToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.objectBrowserToolStripMenuItem.Checked = true;
            this.objectBrowserToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.objectBrowserToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.objectBrowserToolStripMenuItem.Name = "objectBrowserToolStripMenuItem";
            this.objectBrowserToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.objectBrowserToolStripMenuItem.Text = "&Object Browser";
            this.objectBrowserToolStripMenuItem.Click += new System.EventHandler(this.View_ObjectBrowser_MenuItem_Click);
            // 
            // referenceBrowserToolStripMenuItem
            // 
            this.referenceBrowserToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.referenceBrowserToolStripMenuItem.Checked = true;
            this.referenceBrowserToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.referenceBrowserToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.referenceBrowserToolStripMenuItem.Name = "referenceBrowserToolStripMenuItem";
            this.referenceBrowserToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.referenceBrowserToolStripMenuItem.Text = "&Reference Browser";
            this.referenceBrowserToolStripMenuItem.Click += new System.EventHandler(this.View_ReferenceBrowser_MenuItem_Click);
            // 
            // nextBookmarkToolStripButton
            // 
            this.nextBookmarkToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.nextBookmarkToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.nextBookmarkToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.nextBookmarkToolStripButton.Image = global::ScriptEditor.Properties.Resources.nextBookmark_16;
            this.nextBookmarkToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.nextBookmarkToolStripButton.Name = "nextBookmarkToolStripButton";
            this.nextBookmarkToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.nextBookmarkToolStripButton.Text = "Next Bookmark (Ctrl+Right)";
            this.nextBookmarkToolStripButton.Click += new System.EventHandler(this.ToolStrip_NextBookmarkButton_Click);
            // 
            // objectBrowser
            // 
            this.objectBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.objectBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.objectBrowser.Location = new System.Drawing.Point(0, 20);
            this.objectBrowser.MaxDragChange = 20;
            this.objectBrowser.Name = "objectBrowser";
            this.objectBrowser.Size = new System.Drawing.Size(200, 304);
            this.objectBrowser.TabIndex = 6;
            this.objectBrowser.Click += new System.EventHandler(this.objectBrowser_Click);
            // 
            // objectBrowserBox
            // 
            this.objectBrowserBox.Controls.Add(this.searchTextBox);
            this.objectBrowserBox.Controls.Add(this.objectBrowser);
            this.objectBrowserBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.objectBrowserBox.Location = new System.Drawing.Point(0, 52);
            this.objectBrowserBox.Name = "objectBrowserBox";
            this.objectBrowserBox.Size = new System.Drawing.Size(200, 324);
            this.objectBrowserBox.TabIndex = 7;
            this.objectBrowserBox.TabStop = false;
            // 
            // searchTextBox
            // 
            this.searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchTextBox.Location = new System.Drawing.Point(0, 0);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(200, 20);
            this.searchTextBox.TabIndex = 7;
            this.searchTextBox.Text = "Search...";
            this.searchTextBox.TextChanged += new System.EventHandler(this.searchTextBox_TextChanged);
            this.searchTextBox.GotFocus += new System.EventHandler(this.searchTextBox_GotFocus);
            this.searchTextBox.LostFocus += new System.EventHandler(this.searchTextBox_LostFocus);
            // 
            // objectBrowserSplitter
            // 
            this.objectBrowserSplitter.Location = new System.Drawing.Point(200, 52);
            this.objectBrowserSplitter.MinSize = 100;
            this.objectBrowserSplitter.Name = "objectBrowserSplitter";
            this.objectBrowserSplitter.Size = new System.Drawing.Size(3, 324);
            this.objectBrowserSplitter.TabIndex = 12;
            this.objectBrowserSplitter.TabStop = false;
            // 
            // pasteToolStripButton
            // 
            this.pasteToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pasteToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.pasteToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pasteToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("pasteToolStripButton.Image")));
            this.pasteToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pasteToolStripButton.Name = "pasteToolStripButton";
            this.pasteToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.pasteToolStripButton.Text = "Paste (Ctrl+V)";
            this.pasteToolStripButton.Click += new System.EventHandler(this.ToolStrip_PasteButton_Click);
            // 
            // prevBookmarkToolStripButton
            // 
            this.prevBookmarkToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.prevBookmarkToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.prevBookmarkToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.prevBookmarkToolStripButton.Image = global::ScriptEditor.Properties.Resources.prevBookmark_16;
            this.prevBookmarkToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.prevBookmarkToolStripButton.Name = "prevBookmarkToolStripButton";
            this.prevBookmarkToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.prevBookmarkToolStripButton.Text = "Previous Bookmark (Ctrl+Left)";
            this.prevBookmarkToolStripButton.Click += new System.EventHandler(this.ToolStrip_PrevBookmarkButton_Click);
            // 
            // redoToolStripButton
            // 
            this.redoToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.redoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.redoToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.redoToolStripButton.Image = global::ScriptEditor.Properties.Resources.general_redo_16;
            this.redoToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.redoToolStripButton.Name = "redoToolStripButton";
            this.redoToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.redoToolStripButton.Text = "Redo (Ctrl+Y)";
            this.redoToolStripButton.Click += new System.EventHandler(this.ToolStrip_RedoButton_Click);
            // 
            // refBrowserSplitter
            // 
            this.refBrowserSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.refBrowserSplitter.Location = new System.Drawing.Point(0, 376);
            this.refBrowserSplitter.MinSize = 70;
            this.refBrowserSplitter.Name = "refBrowserSplitter";
            this.refBrowserSplitter.Size = new System.Drawing.Size(944, 3);
            this.refBrowserSplitter.TabIndex = 13;
            this.refBrowserSplitter.TabStop = false;
            // 
            // referenceBrowser
            // 
            this.referenceBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.referenceBrowser.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.referenceBrowser.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.referenceBrowser.Location = new System.Drawing.Point(0, 379);
            this.referenceBrowser.Name = "referenceBrowser";
            this.referenceBrowser.Size = new System.Drawing.Size(944, 160);
            this.referenceBrowser.TabIndex = 11;
            // 
            // resetZoomButton
            // 
            this.resetZoomButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.resetZoomButton.Location = new System.Drawing.Point(794, 543);
            this.resetZoomButton.Name = "resetZoomButton";
            this.resetZoomButton.Size = new System.Drawing.Size(75, 20);
            this.resetZoomButton.TabIndex = 9;
            this.resetZoomButton.Text = "Reset zoom";
            this.resetZoomButton.Visible = false;
            this.resetZoomButton.Click += new System.EventHandler(this.StatusStrip_ResetZoomButton_Click);
            // 
            // saveToolStripButton
            // 
            this.saveToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.saveToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.saveToolStripButton.Image = global::ScriptEditor.Properties.Resources.general_Save_16;
            this.saveToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveToolStripButton.Name = "saveToolStripButton";
            this.saveToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.saveToolStripButton.Text = "Save (Ctrl+S)";
            this.saveToolStripButton.Click += new System.EventHandler(this.ToolStrip_SaveButton_Click);
            // 
            // selectedCharsLabel
            // 
            this.selectedCharsLabel.Name = "selectedCharsLabel";
            this.selectedCharsLabel.Size = new System.Drawing.Size(63, 15);
            this.selectedCharsLabel.Text = "Selected: 0";
            // 
            // showStringTableButton
            // 
            this.showStringTableButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.showStringTableButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.showStringTableButton.BackColorUseGeneric = false;
            this.showStringTableButton.Location = new System.Drawing.Point(816, 12);
            this.showStringTableButton.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.showStringTableButton.Name = "showStringTableButton";
            this.showStringTableButton.Size = new System.Drawing.Size(116, 31);
            this.showStringTableButton.TabIndex = 10;
            this.showStringTableButton.Text = "Show string table...";
            this.showStringTableButton.Click += new System.EventHandler(this.ToolStrip_StringTableButton_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.statusStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lineNumberLabel,
            this.colNumberLabel,
            this.selectedCharsLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 539);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.statusStrip.Size = new System.Drawing.Size(944, 28);
            this.statusStrip.TabIndex = 2;
            this.statusStrip.Text = "statusStrip";
            // 
            // toggleBookmarkToolStripButton
            // 
            this.toggleBookmarkToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toggleBookmarkToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toggleBookmarkToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toggleBookmarkToolStripButton.Image = global::ScriptEditor.Properties.Resources.toggleBookmark_16;
            this.toggleBookmarkToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toggleBookmarkToolStripButton.Name = "toggleBookmarkToolStripButton";
            this.toggleBookmarkToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.toggleBookmarkToolStripButton.Text = "Toggle Bookmark (Ctrl+B)";
            this.toggleBookmarkToolStripButton.Click += new System.EventHandler(this.ToolStrip_ToggleBookmarkButton_Click);
            // 
            // toolStrip
            // 
            this.toolStrip.AutoSize = false;
            this.toolStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeToolStripButton,
            this.toolStripSeparator_09,
            this.saveToolStripButton,
            this.buildToolStripButton,
            this.toolStripSeparator_10,
            this.undoToolStripButton,
            this.redoToolStripButton,
            this.toolStripSeparator_11,
            this.cutToolStripButton,
            this.copyToolStripButton,
            this.pasteToolStripButton,
            this.toolStripSeparator_12,
            this.commentToolStripButton,
            this.uncommentToolStripButton,
            this.toolStripSeparator_13,
            this.toggleBookmarkToolStripButton,
            this.prevBookmarkToolStripButton,
            this.nextBookmarkToolStripButton,
            this.clearBookmarksToolStripButton,
            this.toolStripSeparator_14,
            this.aboutToolStripButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.toolStrip.Size = new System.Drawing.Size(944, 28);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "toolStrip";
            // 
            // toolStripSeparator_09
            // 
            this.toolStripSeparator_09.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_09.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_09.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator_09.Name = "toolStripSeparator_09";
            this.toolStripSeparator_09.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripSeparator_10
            // 
            this.toolStripSeparator_10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_10.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator_10.Name = "toolStripSeparator_10";
            this.toolStripSeparator_10.Size = new System.Drawing.Size(6, 28);
            // 
            // undoToolStripButton
            // 
            this.undoToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.undoToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.undoToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.undoToolStripButton.Image = global::ScriptEditor.Properties.Resources.general_undo_16;
            this.undoToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.undoToolStripButton.Name = "undoToolStripButton";
            this.undoToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.undoToolStripButton.Text = "Undo (Ctrl+Z)";
            this.undoToolStripButton.Click += new System.EventHandler(this.ToolStrip_UndoButton_Click);
            // 
            // toolStripSeparator_11
            // 
            this.toolStripSeparator_11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_11.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator_11.Name = "toolStripSeparator_11";
            this.toolStripSeparator_11.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripSeparator_12
            // 
            this.toolStripSeparator_12.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_12.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator_12.Name = "toolStripSeparator_12";
            this.toolStripSeparator_12.Size = new System.Drawing.Size(6, 28);
            // 
            // uncommentToolStripButton
            // 
            this.uncommentToolStripButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.uncommentToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.uncommentToolStripButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.uncommentToolStripButton.Image = global::ScriptEditor.Properties.Resources.uncomment_16;
            this.uncommentToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.uncommentToolStripButton.Name = "uncommentToolStripButton";
            this.uncommentToolStripButton.Size = new System.Drawing.Size(23, 25);
            this.uncommentToolStripButton.Text = "Uncomment Lines (Ctrl+Shift+U)";
            this.uncommentToolStripButton.Click += new System.EventHandler(this.ToolStrip_UncommentButton_Click);
            // 
            // toolStripSeparator_13
            // 
            this.toolStripSeparator_13.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_13.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator_13.Name = "toolStripSeparator_13";
            this.toolStripSeparator_13.Size = new System.Drawing.Size(6, 28);
            // 
            // toolStripSeparator_14
            // 
            this.toolStripSeparator_14.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator_14.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator_14.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator_14.Name = "toolStripSeparator_14";
            this.toolStripSeparator_14.Size = new System.Drawing.Size(6, 28);
            // 
            // zoomLabel
            // 
            this.zoomLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.zoomLabel.AutoSize = true;
            this.zoomLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.zoomLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.zoomLabel.Location = new System.Drawing.Point(874, 547);
            this.zoomLabel.Name = "zoomLabel";
            this.zoomLabel.Size = new System.Drawing.Size(66, 13);
            this.zoomLabel.TabIndex = 8;
            this.zoomLabel.Text = "Zoom: 100%";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(944, 567);
            this.Controls.Add(this.autosaveLabel);
            this.Controls.Add(this.objectBrowserSplitter);
            this.Controls.Add(this.showStringTableButton);
            this.Controls.Add(this.resetZoomButton);
            this.Controls.Add(this.zoomLabel);
            this.Controls.Add(this.textEditor);
            this.Controls.Add(this.objectBrowserBox);
            this.Controls.Add(this.documentMap);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.refBrowserSplitter);
            this.Controls.Add(this.referenceBrowser);
            this.Controls.Add(this.statusStrip);
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(800, 450);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Script Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_Closing);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.textEditor)).EndInit();
            this.editorContextMenu.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.objectBrowserBox.ResumeLayout(false);
            this.objectBrowserBox.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private DarkUI.Controls.DarkButton resetZoomButton;
		private DarkUI.Controls.DarkButton showStringTableButton;
		private DarkUI.Controls.DarkContextMenu editorContextMenu;
		private DarkUI.Controls.DarkGroupBox objectBrowserBox;
		private DarkUI.Controls.DarkLabel autosaveLabel;
		private DarkUI.Controls.DarkLabel zoomLabel;
		private DarkUI.Controls.DarkMenuStrip menuStrip;
		private DarkUI.Controls.DarkStatusStrip statusStrip;
		private DarkUI.Controls.DarkTextBox searchTextBox;
		private DarkUI.Controls.DarkToolStrip toolStrip;
		private DarkUI.Controls.DarkTreeView objectBrowser;
		private FastColoredTextBoxNS.DocumentMap documentMap;
		private FastColoredTextBoxNS.FastColoredTextBox textEditor;
		private ReferenceBrowser referenceBrowser;
		private System.Windows.Forms.Splitter objectBrowserSplitter;
		private System.Windows.Forms.Splitter refBrowserSplitter;
		private System.Windows.Forms.ToolStripButton aboutToolStripButton;
		private System.Windows.Forms.ToolStripButton buildToolStripButton;
		private System.Windows.Forms.ToolStripButton changeToolStripButton;
		private System.Windows.Forms.ToolStripButton clearBookmarksToolStripButton;
		private System.Windows.Forms.ToolStripButton commentToolStripButton;
		private System.Windows.Forms.ToolStripButton copyToolStripButton;
		private System.Windows.Forms.ToolStripButton cutToolStripButton;
		private System.Windows.Forms.ToolStripButton nextBookmarkToolStripButton;
		private System.Windows.Forms.ToolStripButton pasteToolStripButton;
		private System.Windows.Forms.ToolStripButton prevBookmarkToolStripButton;
		private System.Windows.Forms.ToolStripButton redoToolStripButton;
		private System.Windows.Forms.ToolStripButton saveToolStripButton;
		private System.Windows.Forms.ToolStripButton toggleBookmarkToolStripButton;
		private System.Windows.Forms.ToolStripButton uncommentToolStripButton;
		private System.Windows.Forms.ToolStripButton undoToolStripButton;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem buildToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem changeToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem clearBookmarksToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem commentContextMenuItem;
		private System.Windows.Forms.ToolStripMenuItem commentToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem copyContextItem;
		private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem cutContextItem;
		private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem documentMapToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem findToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem nextBookmarkToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem objectBrowserToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem pasteContextItem;
		private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem prevBookmarkToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem redoToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem referenceBrowserToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem reindentScriptToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem replaceToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem selectAllToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showStringTableToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toggleBookmarkContextItem;
		private System.Windows.Forms.ToolStripMenuItem toggleBookmarkToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem trimWhitespaceToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem uncommentContextMenuItem;
		private System.Windows.Forms.ToolStripMenuItem uncommentToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem undoToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_01;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_02;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_03;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_04;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_05;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_06;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_07;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_08;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_09;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_10;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_11;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_12;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_13;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_14;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_15;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_16;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator_17;
		private System.Windows.Forms.ToolStripStatusLabel colNumberLabel;
		private System.Windows.Forms.ToolStripStatusLabel lineNumberLabel;
		private System.Windows.Forms.ToolStripStatusLabel selectedCharsLabel;
    }
}

