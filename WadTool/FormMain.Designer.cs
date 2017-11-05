namespace WadTool
{
    partial class FormMain
    {
        /// <summary>
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.darkMenuStrip1 = new DarkUI.Controls.DarkMenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newEmptyWad2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newEmptyWad2WithSystemSoundsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.openDestinationWad2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSourceWADToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveWad2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveWad2AsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNewStaticMeshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNewSpriteSequenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertWADToWad2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.soundManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spriteEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importModelAsStaticMeshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction1ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction5ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction6ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction7ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction8ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction9ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutWadToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkToolStrip1 = new DarkUI.Controls.DarkToolStrip();
            this.butNewWad2 = new System.Windows.Forms.ToolStripButton();
            this.butNewWad2ForLevel = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.butOpenDestWad2 = new System.Windows.Forms.ToolStripButton();
            this.butOpenSourceWad = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.butSave = new System.Windows.Forms.ToolStripButton();
            this.butSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.butSoundEditor = new System.Windows.Forms.ToolStripButton();
            this.butSpriteEditor = new System.Windows.Forms.ToolStripButton();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.treeDestWad = new DarkUI.Controls.DarkTreeView();
            this.treeSourceWad = new DarkUI.Controls.DarkTreeView();
            this.panel3D = new WadTool.Controls.PanelRendering();
            this.openFileDialogWad = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialogWad2 = new System.Windows.Forms.SaveFileDialog();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.treeSounds = new DarkUI.Controls.DarkTreeView();
            this.butRenameSound = new DarkUI.Controls.DarkButton();
            this.butPlaySound = new DarkUI.Controls.DarkButton();
            this.butDeleteObject = new DarkUI.Controls.DarkButton();
            this.butAddObjectToDifferentSlot = new DarkUI.Controls.DarkButton();
            this.butAddObject = new DarkUI.Controls.DarkButton();
            this.labelType = new DarkUI.Controls.DarkLabel();
            this.butChangeSlot = new DarkUI.Controls.DarkButton();
            this.scrollbarAnimations = new DarkUI.Controls.DarkScrollBar();
            this.groupSelectedMoveable = new DarkUI.Controls.DarkGroupBox();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.darkButton1 = new DarkUI.Controls.DarkButton();
            this.treeAnimations = new DarkUI.Controls.DarkTreeView();
            this.darkButton2 = new DarkUI.Controls.DarkButton();
            this.butEditItem = new DarkUI.Controls.DarkButton();
            this.darkMenuStrip1.SuspendLayout();
            this.darkToolStrip1.SuspendLayout();
            this.groupSelectedMoveable.SuspendLayout();
            this.SuspendLayout();
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 722);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(1008, 24);
            this.darkStatusStrip1.TabIndex = 1;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // darkMenuStrip1
            // 
            this.darkMenuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkMenuStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.debugToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.darkMenuStrip1.Location = new System.Drawing.Point(0, 0);
            this.darkMenuStrip1.Name = "darkMenuStrip1";
            this.darkMenuStrip1.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this.darkMenuStrip1.Size = new System.Drawing.Size(1008, 24);
            this.darkMenuStrip1.TabIndex = 2;
            this.darkMenuStrip1.Text = "darkMenuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newEmptyWad2ToolStripMenuItem,
            this.newEmptyWad2WithSystemSoundsToolStripMenuItem,
            this.toolStripMenuItem3,
            this.openDestinationWad2ToolStripMenuItem,
            this.openSourceWADToolStripMenuItem,
            this.toolStripMenuItem1,
            this.saveWad2ToolStripMenuItem,
            this.saveWad2AsToolStripMenuItem,
            this.toolStripMenuItem2,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // newEmptyWad2ToolStripMenuItem
            // 
            this.newEmptyWad2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.newEmptyWad2ToolStripMenuItem.Image = global::WadTool.Properties.Resources.create_new_16;
            this.newEmptyWad2ToolStripMenuItem.Name = "newEmptyWad2ToolStripMenuItem";
            this.newEmptyWad2ToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            this.newEmptyWad2ToolStripMenuItem.Text = "New empty Wad2";
            this.newEmptyWad2ToolStripMenuItem.Click += new System.EventHandler(this.newEmptyWad2ToolStripMenuItem_Click);
            // 
            // newEmptyWad2WithSystemSoundsToolStripMenuItem
            // 
            this.newEmptyWad2WithSystemSoundsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.newEmptyWad2WithSystemSoundsToolStripMenuItem.Image = global::WadTool.Properties.Resources.create_archive_16;
            this.newEmptyWad2WithSystemSoundsToolStripMenuItem.Name = "newEmptyWad2WithSystemSoundsToolStripMenuItem";
            this.newEmptyWad2WithSystemSoundsToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            this.newEmptyWad2WithSystemSoundsToolStripMenuItem.Text = "New empty Wad2 for level";
            this.newEmptyWad2WithSystemSoundsToolStripMenuItem.Click += new System.EventHandler(this.newEmptyWad2WithSystemSoundsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(247, 6);
            // 
            // openDestinationWad2ToolStripMenuItem
            // 
            this.openDestinationWad2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.openDestinationWad2ToolStripMenuItem.Image = global::WadTool.Properties.Resources.opened_folder_16;
            this.openDestinationWad2ToolStripMenuItem.Name = "openDestinationWad2ToolStripMenuItem";
            this.openDestinationWad2ToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            this.openDestinationWad2ToolStripMenuItem.Text = "Open destination Wad2";
            this.openDestinationWad2ToolStripMenuItem.Click += new System.EventHandler(this.openDestinationWad2ToolStripMenuItem_Click);
            // 
            // openSourceWADToolStripMenuItem
            // 
            this.openSourceWADToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.openSourceWADToolStripMenuItem.Image = global::WadTool.Properties.Resources.import_16;
            this.openSourceWADToolStripMenuItem.Name = "openSourceWADToolStripMenuItem";
            this.openSourceWADToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            this.openSourceWADToolStripMenuItem.Text = "Open source WAD - Wad2 - Level";
            this.openSourceWADToolStripMenuItem.Click += new System.EventHandler(this.openSourceWADToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(247, 6);
            // 
            // saveWad2ToolStripMenuItem
            // 
            this.saveWad2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.saveWad2ToolStripMenuItem.Image = global::WadTool.Properties.Resources.save_16;
            this.saveWad2ToolStripMenuItem.Name = "saveWad2ToolStripMenuItem";
            this.saveWad2ToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            this.saveWad2ToolStripMenuItem.Text = "Save Wad2";
            // 
            // saveWad2AsToolStripMenuItem
            // 
            this.saveWad2AsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.saveWad2AsToolStripMenuItem.Image = global::WadTool.Properties.Resources.save_as_16;
            this.saveWad2AsToolStripMenuItem.Name = "saveWad2AsToolStripMenuItem";
            this.saveWad2AsToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            this.saveWad2AsToolStripMenuItem.Text = "Save Wad2 as...";
            this.saveWad2AsToolStripMenuItem.Click += new System.EventHandler(this.saveWad2AsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(247, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.exitToolStripMenuItem.Image = global::WadTool.Properties.Resources.door_opened_16;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(250, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addNewStaticMeshToolStripMenuItem,
            this.addNewSpriteSequenceToolStripMenuItem});
            this.editToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // addNewStaticMeshToolStripMenuItem
            // 
            this.addNewStaticMeshToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addNewStaticMeshToolStripMenuItem.Name = "addNewStaticMeshToolStripMenuItem";
            this.addNewStaticMeshToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.addNewStaticMeshToolStripMenuItem.Text = "Add new static mesh";
            // 
            // addNewSpriteSequenceToolStripMenuItem
            // 
            this.addNewSpriteSequenceToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.addNewSpriteSequenceToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.addNewSpriteSequenceToolStripMenuItem.Name = "addNewSpriteSequenceToolStripMenuItem";
            this.addNewSpriteSequenceToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.addNewSpriteSequenceToolStripMenuItem.Text = "Add new sprite sequence";
            this.addNewSpriteSequenceToolStripMenuItem.Click += new System.EventHandler(this.addNewSpriteSequenceToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.convertWADToWad2ToolStripMenuItem,
            this.soundManagerToolStripMenuItem,
            this.spriteEditorToolStripMenuItem,
            this.importModelAsStaticMeshToolStripMenuItem});
            this.toolsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // convertWADToWad2ToolStripMenuItem
            // 
            this.convertWADToWad2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.convertWADToWad2ToolStripMenuItem.Image = global::WadTool.Properties.Resources.software_installer_16;
            this.convertWADToWad2ToolStripMenuItem.Name = "convertWADToWad2ToolStripMenuItem";
            this.convertWADToWad2ToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.convertWADToWad2ToolStripMenuItem.Text = "Convert source WAD to Wad2";
            this.convertWADToWad2ToolStripMenuItem.Click += new System.EventHandler(this.convertWADToWad2ToolStripMenuItem_Click);
            // 
            // soundManagerToolStripMenuItem
            // 
            this.soundManagerToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.soundManagerToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.soundManagerToolStripMenuItem.Image = global::WadTool.Properties.Resources.volume_up_16;
            this.soundManagerToolStripMenuItem.Name = "soundManagerToolStripMenuItem";
            this.soundManagerToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.soundManagerToolStripMenuItem.Text = "Sound editor";
            this.soundManagerToolStripMenuItem.Click += new System.EventHandler(this.soundManagerToolStripMenuItem_Click);
            // 
            // spriteEditorToolStripMenuItem
            // 
            this.spriteEditorToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.spriteEditorToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.spriteEditorToolStripMenuItem.Image = global::WadTool.Properties.Resources.small_icons_16;
            this.spriteEditorToolStripMenuItem.Name = "spriteEditorToolStripMenuItem";
            this.spriteEditorToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.spriteEditorToolStripMenuItem.Text = "Sprite editor";
            this.spriteEditorToolStripMenuItem.Click += new System.EventHandler(this.spriteEditorToolStripMenuItem_Click);
            // 
            // importModelAsStaticMeshToolStripMenuItem
            // 
            this.importModelAsStaticMeshToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.importModelAsStaticMeshToolStripMenuItem.Name = "importModelAsStaticMeshToolStripMenuItem";
            this.importModelAsStaticMeshToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.importModelAsStaticMeshToolStripMenuItem.Text = "Import model as static mesh";
            this.importModelAsStaticMeshToolStripMenuItem.Click += new System.EventHandler(this.importModelAsStaticMeshToolStripMenuItem_Click);
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugAction0ToolStripMenuItem,
            this.debugAction1ToolStripMenuItem,
            this.debugAction1ToolStripMenuItem1,
            this.debugAction4ToolStripMenuItem,
            this.debugAction5ToolStripMenuItem,
            this.debugAction6ToolStripMenuItem,
            this.debugAction7ToolStripMenuItem,
            this.debugAction8ToolStripMenuItem,
            this.debugAction9ToolStripMenuItem});
            this.debugToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            this.debugToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.debugToolStripMenuItem.Text = "Debug";
            // 
            // debugAction0ToolStripMenuItem
            // 
            this.debugAction0ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction0ToolStripMenuItem.Name = "debugAction0ToolStripMenuItem";
            this.debugAction0ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.debugAction0ToolStripMenuItem.Text = "Debug action 0";
            this.debugAction0ToolStripMenuItem.Click += new System.EventHandler(this.debugAction0ToolStripMenuItem_Click);
            // 
            // debugAction1ToolStripMenuItem
            // 
            this.debugAction1ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction1ToolStripMenuItem.Name = "debugAction1ToolStripMenuItem";
            this.debugAction1ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.debugAction1ToolStripMenuItem.Text = "Debug action 1";
            this.debugAction1ToolStripMenuItem.Click += new System.EventHandler(this.debugAction1ToolStripMenuItem_Click);
            // 
            // debugAction1ToolStripMenuItem1
            // 
            this.debugAction1ToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.debugAction1ToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction1ToolStripMenuItem1.Name = "debugAction1ToolStripMenuItem1";
            this.debugAction1ToolStripMenuItem1.Size = new System.Drawing.Size(154, 22);
            this.debugAction1ToolStripMenuItem1.Text = "Debug action 1";
            this.debugAction1ToolStripMenuItem1.Click += new System.EventHandler(this.debugAction1ToolStripMenuItem1_Click);
            // 
            // debugAction4ToolStripMenuItem
            // 
            this.debugAction4ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.debugAction4ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction4ToolStripMenuItem.Name = "debugAction4ToolStripMenuItem";
            this.debugAction4ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.debugAction4ToolStripMenuItem.Text = "Debug action 4";
            this.debugAction4ToolStripMenuItem.Click += new System.EventHandler(this.debugAction4ToolStripMenuItem_Click);
            // 
            // debugAction5ToolStripMenuItem
            // 
            this.debugAction5ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.debugAction5ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction5ToolStripMenuItem.Name = "debugAction5ToolStripMenuItem";
            this.debugAction5ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.debugAction5ToolStripMenuItem.Text = "Debug action 5";
            this.debugAction5ToolStripMenuItem.Click += new System.EventHandler(this.debugAction5ToolStripMenuItem_Click);
            // 
            // debugAction6ToolStripMenuItem
            // 
            this.debugAction6ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.debugAction6ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction6ToolStripMenuItem.Name = "debugAction6ToolStripMenuItem";
            this.debugAction6ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.debugAction6ToolStripMenuItem.Text = "Debug action 6";
            this.debugAction6ToolStripMenuItem.Click += new System.EventHandler(this.debugAction6ToolStripMenuItem_Click);
            // 
            // debugAction7ToolStripMenuItem
            // 
            this.debugAction7ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.debugAction7ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction7ToolStripMenuItem.Name = "debugAction7ToolStripMenuItem";
            this.debugAction7ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.debugAction7ToolStripMenuItem.Text = "Debug action 7";
            this.debugAction7ToolStripMenuItem.Click += new System.EventHandler(this.debugAction7ToolStripMenuItem_Click);
            // 
            // debugAction8ToolStripMenuItem
            // 
            this.debugAction8ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.debugAction8ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction8ToolStripMenuItem.Name = "debugAction8ToolStripMenuItem";
            this.debugAction8ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.debugAction8ToolStripMenuItem.Text = "Debug action 8";
            this.debugAction8ToolStripMenuItem.Click += new System.EventHandler(this.debugAction8ToolStripMenuItem_Click);
            // 
            // debugAction9ToolStripMenuItem
            // 
            this.debugAction9ToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.debugAction9ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction9ToolStripMenuItem.Name = "debugAction9ToolStripMenuItem";
            this.debugAction9ToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.debugAction9ToolStripMenuItem.Text = "Debug action 9";
            this.debugAction9ToolStripMenuItem.Click += new System.EventHandler(this.debugAction9ToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutWadToolToolStripMenuItem});
            this.helpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutWadToolToolStripMenuItem
            // 
            this.aboutWadToolToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.aboutWadToolToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.aboutWadToolToolStripMenuItem.Name = "aboutWadToolToolStripMenuItem";
            this.aboutWadToolToolStripMenuItem.Size = new System.Drawing.Size(169, 22);
            this.aboutWadToolToolStripMenuItem.Text = "About Wad Tool...";
            this.aboutWadToolToolStripMenuItem.Click += new System.EventHandler(this.aboutWadToolToolStripMenuItem_Click);
            // 
            // darkToolStrip1
            // 
            this.darkToolStrip1.AutoSize = false;
            this.darkToolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkToolStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkToolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.butNewWad2,
            this.butNewWad2ForLevel,
            this.toolStripSeparator3,
            this.butOpenDestWad2,
            this.butOpenSourceWad,
            this.toolStripSeparator2,
            this.butSave,
            this.butSaveAs,
            this.toolStripSeparator1,
            this.butSoundEditor,
            this.butSpriteEditor});
            this.darkToolStrip1.Location = new System.Drawing.Point(0, 24);
            this.darkToolStrip1.Name = "darkToolStrip1";
            this.darkToolStrip1.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.darkToolStrip1.Size = new System.Drawing.Size(1008, 28);
            this.darkToolStrip1.TabIndex = 3;
            this.darkToolStrip1.Text = "darkToolStrip1";
            // 
            // butNewWad2
            // 
            this.butNewWad2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butNewWad2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butNewWad2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butNewWad2.Image = global::WadTool.Properties.Resources.create_new_16;
            this.butNewWad2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butNewWad2.Name = "butNewWad2";
            this.butNewWad2.Size = new System.Drawing.Size(23, 25);
            this.butNewWad2.Text = "New empty Wad2";
            this.butNewWad2.Click += new System.EventHandler(this.butNewWad2_Click);
            // 
            // butNewWad2ForLevel
            // 
            this.butNewWad2ForLevel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butNewWad2ForLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butNewWad2ForLevel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butNewWad2ForLevel.Image = global::WadTool.Properties.Resources.create_archive_16;
            this.butNewWad2ForLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butNewWad2ForLevel.Name = "butNewWad2ForLevel";
            this.butNewWad2ForLevel.Size = new System.Drawing.Size(23, 25);
            this.butNewWad2ForLevel.Text = "New Wad2 for level";
            this.butNewWad2ForLevel.Click += new System.EventHandler(this.butNewWad2ForLevel_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator3.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 28);
            // 
            // butOpenDestWad2
            // 
            this.butOpenDestWad2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butOpenDestWad2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butOpenDestWad2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butOpenDestWad2.Image = global::WadTool.Properties.Resources.opened_folder_16;
            this.butOpenDestWad2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butOpenDestWad2.Name = "butOpenDestWad2";
            this.butOpenDestWad2.Size = new System.Drawing.Size(23, 25);
            this.butOpenDestWad2.Text = "Open destination Wad2";
            this.butOpenDestWad2.Click += new System.EventHandler(this.butOpenDestWad2_Click);
            // 
            // butOpenSourceWad
            // 
            this.butOpenSourceWad.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butOpenSourceWad.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butOpenSourceWad.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butOpenSourceWad.Image = global::WadTool.Properties.Resources.import_16;
            this.butOpenSourceWad.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butOpenSourceWad.Name = "butOpenSourceWad";
            this.butOpenSourceWad.Size = new System.Drawing.Size(23, 25);
            this.butOpenSourceWad.Text = "Open source WAD/Wad2";
            this.butOpenSourceWad.Click += new System.EventHandler(this.butOpenSourceWad_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
            // 
            // butSave
            // 
            this.butSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butSave.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butSave.Image = global::WadTool.Properties.Resources.save_16;
            this.butSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butSave.Name = "butSave";
            this.butSave.Size = new System.Drawing.Size(23, 25);
            this.butSave.Text = "Save Wad2";
            this.butSave.Click += new System.EventHandler(this.butSave_Click);
            // 
            // butSaveAs
            // 
            this.butSaveAs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butSaveAs.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butSaveAs.Image = global::WadTool.Properties.Resources.save_as_16;
            this.butSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butSaveAs.Name = "butSaveAs";
            this.butSaveAs.Size = new System.Drawing.Size(23, 25);
            this.butSaveAs.Text = "Save Wad2 as...";
            this.butSaveAs.Click += new System.EventHandler(this.butSaveAs_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 28);
            // 
            // butSoundEditor
            // 
            this.butSoundEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butSoundEditor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butSoundEditor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butSoundEditor.Image = global::WadTool.Properties.Resources.volume_up_16;
            this.butSoundEditor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butSoundEditor.Name = "butSoundEditor";
            this.butSoundEditor.Size = new System.Drawing.Size(23, 25);
            this.butSoundEditor.Text = "Sound editor";
            this.butSoundEditor.Click += new System.EventHandler(this.butSoundEditor_Click);
            // 
            // butSpriteEditor
            // 
            this.butSpriteEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.butSpriteEditor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.butSpriteEditor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.butSpriteEditor.Image = global::WadTool.Properties.Resources.small_icons_16;
            this.butSpriteEditor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butSpriteEditor.Name = "butSpriteEditor";
            this.butSpriteEditor.Size = new System.Drawing.Size(23, 25);
            this.butSpriteEditor.Text = "Sprite editor";
            this.butSpriteEditor.Click += new System.EventHandler(this.butSpriteEditor_Click);
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(12, 59);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(79, 13);
            this.darkLabel1.TabIndex = 5;
            this.darkLabel1.Text = "Current Wad2";
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(713, 59);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(149, 13);
            this.darkLabel2.TabIndex = 6;
            this.darkLabel2.Text = "Source WAD - Wad2 - Level";
            // 
            // treeDestWad
            // 
            this.treeDestWad.Location = new System.Drawing.Point(15, 107);
            this.treeDestWad.MaxDragChange = 20;
            this.treeDestWad.Name = "treeDestWad";
            this.treeDestWad.Size = new System.Drawing.Size(280, 361);
            this.treeDestWad.TabIndex = 7;
            this.treeDestWad.Text = "darkTreeView1";
            this.treeDestWad.DoubleClick += new System.EventHandler(this.treeDestWad_DoubleClick);
            this.treeDestWad.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeDestWad_MouseClick);
            // 
            // treeSourceWad
            // 
            this.treeSourceWad.Location = new System.Drawing.Point(716, 107);
            this.treeSourceWad.MaxDragChange = 20;
            this.treeSourceWad.Name = "treeSourceWad";
            this.treeSourceWad.Size = new System.Drawing.Size(279, 361);
            this.treeSourceWad.TabIndex = 8;
            this.treeSourceWad.Text = "darkTreeView1";
            this.treeSourceWad.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeSourceWad_MouseClick);
            // 
            // panel3D
            // 
            this.panel3D.Animation = 0;
            this.panel3D.Camera = null;
            this.panel3D.CurrentObject = null;
            this.panel3D.CurrentWad = null;
            this.panel3D.KeyFrame = 0;
            this.panel3D.Location = new System.Drawing.Point(301, 59);
            this.panel3D.Name = "panel3D";
            this.panel3D.Size = new System.Drawing.Size(409, 409);
            this.panel3D.TabIndex = 9;
            // 
            // saveFileDialogWad2
            // 
            this.saveFileDialogWad2.Filter = "Tomb Editor Wad2 (*.wad2)|*.wad2";
            this.saveFileDialogWad2.Title = "Save Wad2";
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(6, 28);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(46, 13);
            this.darkLabel3.TabIndex = 13;
            this.darkLabel3.Text = "Sounds";
            // 
            // treeSounds
            // 
            this.treeSounds.Location = new System.Drawing.Point(6, 44);
            this.treeSounds.MaxDragChange = 20;
            this.treeSounds.Name = "treeSounds";
            this.treeSounds.Size = new System.Drawing.Size(192, 137);
            this.treeSounds.TabIndex = 16;
            this.treeSounds.Text = "darkTreeView1";
            // 
            // butRenameSound
            // 
            this.butRenameSound.Image = global::WadTool.Properties.Resources.edit_16;
            this.butRenameSound.Location = new System.Drawing.Point(91, 187);
            this.butRenameSound.Name = "butRenameSound";
            this.butRenameSound.Size = new System.Drawing.Size(107, 23);
            this.butRenameSound.TabIndex = 17;
            this.butRenameSound.Text = "Rename";
            this.butRenameSound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // butPlaySound
            // 
            this.butPlaySound.Image = global::WadTool.Properties.Resources.play_16;
            this.butPlaySound.Location = new System.Drawing.Point(9, 187);
            this.butPlaySound.Name = "butPlaySound";
            this.butPlaySound.Size = new System.Drawing.Size(76, 23);
            this.butPlaySound.TabIndex = 15;
            this.butPlaySound.Text = "Play";
            this.butPlaySound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlaySound.Click += new System.EventHandler(this.butPlaySound_Click);
            // 
            // butDeleteObject
            // 
            this.butDeleteObject.Image = global::WadTool.Properties.Resources.trash_161;
            this.butDeleteObject.Location = new System.Drawing.Point(192, 474);
            this.butDeleteObject.Name = "butDeleteObject";
            this.butDeleteObject.Size = new System.Drawing.Size(103, 23);
            this.butDeleteObject.TabIndex = 12;
            this.butDeleteObject.Text = "Delete object";
            this.butDeleteObject.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteObject.Click += new System.EventHandler(this.butDeleteObject_Click);
            // 
            // butAddObjectToDifferentSlot
            // 
            this.butAddObjectToDifferentSlot.Image = global::WadTool.Properties.Resources.angle_left_16;
            this.butAddObjectToDifferentSlot.Location = new System.Drawing.Point(823, 474);
            this.butAddObjectToDifferentSlot.Name = "butAddObjectToDifferentSlot";
            this.butAddObjectToDifferentSlot.Size = new System.Drawing.Size(172, 23);
            this.butAddObjectToDifferentSlot.TabIndex = 11;
            this.butAddObjectToDifferentSlot.Text = "Add object to different slot";
            this.butAddObjectToDifferentSlot.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddObjectToDifferentSlot.Click += new System.EventHandler(this.butAddObjectToDifferentSlot_Click);
            // 
            // butAddObject
            // 
            this.butAddObject.Image = global::WadTool.Properties.Resources.angle_left_16;
            this.butAddObject.Location = new System.Drawing.Point(716, 474);
            this.butAddObject.Name = "butAddObject";
            this.butAddObject.Size = new System.Drawing.Size(101, 23);
            this.butAddObject.TabIndex = 10;
            this.butAddObject.Text = "Add object";
            this.butAddObject.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddObject.Click += new System.EventHandler(this.butAddObject_Click);
            // 
            // labelType
            // 
            this.labelType.AutoSize = true;
            this.labelType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.labelType.Location = new System.Drawing.Point(713, 80);
            this.labelType.Name = "labelType";
            this.labelType.Size = new System.Drawing.Size(19, 13);
            this.labelType.TabIndex = 18;
            this.labelType.Text = "---";
            // 
            // butChangeSlot
            // 
            this.butChangeSlot.Image = global::WadTool.Properties.Resources.copy_16;
            this.butChangeSlot.Location = new System.Drawing.Point(93, 474);
            this.butChangeSlot.Name = "butChangeSlot";
            this.butChangeSlot.Size = new System.Drawing.Size(93, 23);
            this.butChangeSlot.TabIndex = 19;
            this.butChangeSlot.Text = "Change slot";
            this.butChangeSlot.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butChangeSlot.Click += new System.EventHandler(this.butChangeSlot_Click);
            // 
            // scrollbarAnimations
            // 
            this.scrollbarAnimations.Location = new System.Drawing.Point(301, 474);
            this.scrollbarAnimations.Name = "scrollbarAnimations";
            this.scrollbarAnimations.ScrollOrientation = DarkUI.Controls.DarkScrollOrientation.Horizontal;
            this.scrollbarAnimations.Size = new System.Drawing.Size(409, 23);
            this.scrollbarAnimations.TabIndex = 20;
            this.scrollbarAnimations.Text = "darkScrollBar1";
            this.scrollbarAnimations.ValueChanged += new System.EventHandler<DarkUI.Controls.ScrollValueEventArgs>(this.scrollbarAnimations_ValueChanged);
            // 
            // groupSelectedMoveable
            // 
            this.groupSelectedMoveable.Controls.Add(this.darkLabel4);
            this.groupSelectedMoveable.Controls.Add(this.darkButton1);
            this.groupSelectedMoveable.Controls.Add(this.treeAnimations);
            this.groupSelectedMoveable.Controls.Add(this.darkButton2);
            this.groupSelectedMoveable.Controls.Add(this.darkLabel3);
            this.groupSelectedMoveable.Controls.Add(this.butPlaySound);
            this.groupSelectedMoveable.Controls.Add(this.treeSounds);
            this.groupSelectedMoveable.Controls.Add(this.butRenameSound);
            this.groupSelectedMoveable.Location = new System.Drawing.Point(15, 503);
            this.groupSelectedMoveable.Name = "groupSelectedMoveable";
            this.groupSelectedMoveable.Size = new System.Drawing.Size(560, 216);
            this.groupSelectedMoveable.TabIndex = 21;
            this.groupSelectedMoveable.TabStop = false;
            this.groupSelectedMoveable.Text = "Selected moveable:";
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(212, 28);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(67, 13);
            this.darkLabel4.TabIndex = 18;
            this.darkLabel4.Text = "Animations";
            // 
            // darkButton1
            // 
            this.darkButton1.Image = global::WadTool.Properties.Resources.play_16;
            this.darkButton1.Location = new System.Drawing.Point(215, 187);
            this.darkButton1.Name = "darkButton1";
            this.darkButton1.Size = new System.Drawing.Size(76, 23);
            this.darkButton1.TabIndex = 19;
            this.darkButton1.Text = "Play";
            this.darkButton1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // treeAnimations
            // 
            this.treeAnimations.Location = new System.Drawing.Point(215, 44);
            this.treeAnimations.MaxDragChange = 20;
            this.treeAnimations.Name = "treeAnimations";
            this.treeAnimations.Size = new System.Drawing.Size(189, 137);
            this.treeAnimations.TabIndex = 20;
            this.treeAnimations.Text = "darkTreeView1";
            this.treeAnimations.Click += new System.EventHandler(this.lstAnimations_Click);
            // 
            // darkButton2
            // 
            this.darkButton2.Image = global::WadTool.Properties.Resources.edit_16;
            this.darkButton2.Location = new System.Drawing.Point(297, 187);
            this.darkButton2.Name = "darkButton2";
            this.darkButton2.Size = new System.Drawing.Size(107, 23);
            this.darkButton2.TabIndex = 21;
            this.darkButton2.Text = "Rename";
            this.darkButton2.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // butEditItem
            // 
            this.butEditItem.Image = global::WadTool.Properties.Resources.edit_16;
            this.butEditItem.Location = new System.Drawing.Point(15, 474);
            this.butEditItem.Name = "butEditItem";
            this.butEditItem.Size = new System.Drawing.Size(72, 23);
            this.butEditItem.TabIndex = 22;
            this.butEditItem.Text = "Edit";
            this.butEditItem.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 746);
            this.Controls.Add(this.butEditItem);
            this.Controls.Add(this.groupSelectedMoveable);
            this.Controls.Add(this.scrollbarAnimations);
            this.Controls.Add(this.butChangeSlot);
            this.Controls.Add(this.labelType);
            this.Controls.Add(this.butDeleteObject);
            this.Controls.Add(this.butAddObjectToDifferentSlot);
            this.Controls.Add(this.butAddObject);
            this.Controls.Add(this.panel3D);
            this.Controls.Add(this.treeSourceWad);
            this.Controls.Add(this.treeDestWad);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.darkToolStrip1);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.darkMenuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MainMenuStrip = this.darkMenuStrip1;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Wad Tool";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.darkMenuStrip1.ResumeLayout(false);
            this.darkMenuStrip1.PerformLayout();
            this.darkToolStrip1.ResumeLayout(false);
            this.darkToolStrip1.PerformLayout();
            this.groupSelectedMoveable.ResumeLayout(false);
            this.groupSelectedMoveable.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private DarkUI.Controls.DarkMenuStrip darkMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSourceWADToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openDestinationWad2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveWad2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private DarkUI.Controls.DarkToolStrip darkToolStrip1;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTreeView treeDestWad;
        private DarkUI.Controls.DarkTreeView treeSourceWad;
        private WadTool.Controls.PanelRendering panel3D;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertWADToWad2ToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialogWad;
        private System.Windows.Forms.ToolStripButton butOpenDestWad2;
        private System.Windows.Forms.ToolStripButton butOpenSourceWad;
        private System.Windows.Forms.ToolStripButton butSave;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton butSoundEditor;
        private System.Windows.Forms.ToolStripMenuItem saveWad2AsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem soundManagerToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton butSaveAs;
        private DarkUI.Controls.DarkButton butAddObject;
        private DarkUI.Controls.DarkButton butAddObjectToDifferentSlot;
        private DarkUI.Controls.DarkButton butDeleteObject;
        private System.Windows.Forms.SaveFileDialog saveFileDialogWad2;
        private System.Windows.Forms.ToolStripMenuItem importModelAsStaticMeshToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton butSpriteEditor;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewStaticMeshToolStripMenuItem;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkButton butPlaySound;
        private DarkUI.Controls.DarkTreeView treeSounds;
        private DarkUI.Controls.DarkButton butRenameSound;
        private System.Windows.Forms.ToolStripMenuItem debugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction0ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction1ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newEmptyWad2ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newEmptyWad2WithSystemSoundsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem spriteEditorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewSpriteSequenceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction1ToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem debugAction4ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction5ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction6ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutWadToolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction7ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction8ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugAction9ToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton butNewWad2;
        private System.Windows.Forms.ToolStripButton butNewWad2ForLevel;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private DarkUI.Controls.DarkLabel labelType;
        private DarkUI.Controls.DarkButton butChangeSlot;
        private DarkUI.Controls.DarkScrollBar scrollbarAnimations;
        private DarkUI.Controls.DarkGroupBox groupSelectedMoveable;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkButton darkButton1;
        private DarkUI.Controls.DarkTreeView treeAnimations;
        private DarkUI.Controls.DarkButton darkButton2;
        private DarkUI.Controls.DarkButton butEditItem;
    }
}

