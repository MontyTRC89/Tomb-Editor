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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.butTest = new DarkUI.Controls.DarkButton();
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
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertWADToWad2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.soundManagerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.spriteEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importModelAsStaticMeshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction0ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction1ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkToolStrip1 = new DarkUI.Controls.DarkToolStrip();
            this.butOpenDestWad2 = new System.Windows.Forms.ToolStripButton();
            this.butOpenSourceWad = new System.Windows.Forms.ToolStripButton();
            this.butSave = new System.Windows.Forms.ToolStripButton();
            this.butSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.butSoundEditor = new System.Windows.Forms.ToolStripButton();
            this.butSpriteEditor = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
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
            this.addNewSpriteSequenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugAction1ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.darkMenuStrip1.SuspendLayout();
            this.darkToolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // butTest
            // 
            this.butTest.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butTest.Location = new System.Drawing.Point(920, 562);
            this.butTest.Name = "butTest";
            this.butTest.Padding = new System.Windows.Forms.Padding(5);
            this.butTest.Size = new System.Drawing.Size(75, 23);
            this.butTest.TabIndex = 0;
            this.butTest.Text = "Test";
            this.butTest.Click += new System.EventHandler(this.butTest_Click);
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.AutoSize = false;
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 701);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(1008, 24);
            this.darkStatusStrip1.SizingGrip = false;
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
            this.debugToolStripMenuItem});
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
            this.newEmptyWad2ToolStripMenuItem.Name = "newEmptyWad2ToolStripMenuItem";
            this.newEmptyWad2ToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.newEmptyWad2ToolStripMenuItem.Text = "New empty Wad2";
            // 
            // newEmptyWad2WithSystemSoundsToolStripMenuItem
            // 
            this.newEmptyWad2WithSystemSoundsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.newEmptyWad2WithSystemSoundsToolStripMenuItem.Name = "newEmptyWad2WithSystemSoundsToolStripMenuItem";
            this.newEmptyWad2WithSystemSoundsToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.newEmptyWad2WithSystemSoundsToolStripMenuItem.Text = "New empty Wad2 with base sounds";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(259, 6);
            // 
            // openDestinationWad2ToolStripMenuItem
            // 
            this.openDestinationWad2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.openDestinationWad2ToolStripMenuItem.Image = global::WadTool.Properties.Resources.opened_folder_16;
            this.openDestinationWad2ToolStripMenuItem.Name = "openDestinationWad2ToolStripMenuItem";
            this.openDestinationWad2ToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.openDestinationWad2ToolStripMenuItem.Text = "Open destination Wad2";
            this.openDestinationWad2ToolStripMenuItem.Click += new System.EventHandler(this.openDestinationWad2ToolStripMenuItem_Click);
            // 
            // openSourceWADToolStripMenuItem
            // 
            this.openSourceWADToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.openSourceWADToolStripMenuItem.Image = global::WadTool.Properties.Resources.import_16;
            this.openSourceWADToolStripMenuItem.Name = "openSourceWADToolStripMenuItem";
            this.openSourceWADToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.openSourceWADToolStripMenuItem.Text = "Open source WAD/Wad2";
            this.openSourceWADToolStripMenuItem.Click += new System.EventHandler(this.openSourceWADToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(259, 6);
            // 
            // saveWad2ToolStripMenuItem
            // 
            this.saveWad2ToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.saveWad2ToolStripMenuItem.Image = global::WadTool.Properties.Resources.save_16;
            this.saveWad2ToolStripMenuItem.Name = "saveWad2ToolStripMenuItem";
            this.saveWad2ToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.saveWad2ToolStripMenuItem.Text = "Save Wad2";
            // 
            // saveWad2AsToolStripMenuItem
            // 
            this.saveWad2AsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.saveWad2AsToolStripMenuItem.Image = global::WadTool.Properties.Resources.save_as_16;
            this.saveWad2AsToolStripMenuItem.Name = "saveWad2AsToolStripMenuItem";
            this.saveWad2AsToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            this.saveWad2AsToolStripMenuItem.Text = "Save Wad2 as...";
            this.saveWad2AsToolStripMenuItem.Click += new System.EventHandler(this.saveWad2AsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(259, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.exitToolStripMenuItem.Image = global::WadTool.Properties.Resources.door_opened_16;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
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
            this.convertWADToWad2ToolStripMenuItem.Image = global::WadTool.Properties.Resources.save_as_16;
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
            // 
            // spriteEditorToolStripMenuItem
            // 
            this.spriteEditorToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.spriteEditorToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.spriteEditorToolStripMenuItem.Image = global::WadTool.Properties.Resources.small_icons_161;
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
            // 
            // debugToolStripMenuItem
            // 
            this.debugToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.debugToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.debugAction0ToolStripMenuItem,
            this.debugAction1ToolStripMenuItem,
            this.debugAction1ToolStripMenuItem1});
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
            // darkToolStrip1
            // 
            this.darkToolStrip1.AutoSize = false;
            this.darkToolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkToolStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkToolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.butOpenDestWad2,
            this.butOpenSourceWad,
            this.butSave,
            this.butSaveAs,
            this.toolStripSeparator1,
            this.butSoundEditor,
            this.butSpriteEditor,
            this.toolStripSeparator2});
            this.darkToolStrip1.Location = new System.Drawing.Point(0, 24);
            this.darkToolStrip1.Name = "darkToolStrip1";
            this.darkToolStrip1.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.darkToolStrip1.Size = new System.Drawing.Size(1008, 28);
            this.darkToolStrip1.TabIndex = 3;
            this.darkToolStrip1.Text = "darkToolStrip1";
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
            this.butSpriteEditor.Image = global::WadTool.Properties.Resources.small_icons_161;
            this.butSpriteEditor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.butSpriteEditor.Name = "butSpriteEditor";
            this.butSpriteEditor.Size = new System.Drawing.Size(23, 25);
            this.butSpriteEditor.Text = "Sprite editor";
            this.butSpriteEditor.Click += new System.EventHandler(this.butSpriteEditor_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 28);
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
            this.darkLabel2.Size = new System.Drawing.Size(107, 13);
            this.darkLabel2.TabIndex = 6;
            this.darkLabel2.Text = "Source WAD/Wad2";
            // 
            // treeDestWad
            // 
            this.treeDestWad.Location = new System.Drawing.Point(15, 89);
            this.treeDestWad.MaxDragChange = 20;
            this.treeDestWad.Name = "treeDestWad";
            this.treeDestWad.Size = new System.Drawing.Size(279, 408);
            this.treeDestWad.TabIndex = 7;
            this.treeDestWad.Text = "darkTreeView1";
            this.treeDestWad.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeDestWad_MouseClick);
            // 
            // treeSourceWad
            // 
            this.treeSourceWad.Location = new System.Drawing.Point(716, 89);
            this.treeSourceWad.MaxDragChange = 20;
            this.treeSourceWad.Name = "treeSourceWad";
            this.treeSourceWad.Size = new System.Drawing.Size(279, 408);
            this.treeSourceWad.TabIndex = 8;
            this.treeSourceWad.Text = "darkTreeView1";
            this.treeSourceWad.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeSourceWad_MouseClick);
            // 
            // panel3D
            // 
            this.panel3D.Camera = null;
            this.panel3D.CurrentObject = null;
            this.panel3D.CurrentWad = null;
            this.panel3D.Location = new System.Drawing.Point(301, 89);
            this.panel3D.Name = "panel3D";
            this.panel3D.Size = new System.Drawing.Size(409, 437);
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
            this.darkLabel3.Location = new System.Drawing.Point(12, 545);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(46, 13);
            this.darkLabel3.TabIndex = 13;
            this.darkLabel3.Text = "Sounds";
            // 
            // treeSounds
            // 
            this.treeSounds.Location = new System.Drawing.Point(12, 562);
            this.treeSounds.MaxDragChange = 20;
            this.treeSounds.Name = "treeSounds";
            this.treeSounds.Size = new System.Drawing.Size(222, 136);
            this.treeSounds.TabIndex = 16;
            this.treeSounds.Text = "darkTreeView1";
            // 
            // butRenameSound
            // 
            this.butRenameSound.Image = global::WadTool.Properties.Resources.edit_16;
            this.butRenameSound.Location = new System.Drawing.Point(240, 591);
            this.butRenameSound.Name = "butRenameSound";
            this.butRenameSound.Padding = new System.Windows.Forms.Padding(5);
            this.butRenameSound.Size = new System.Drawing.Size(76, 23);
            this.butRenameSound.TabIndex = 17;
            this.butRenameSound.Text = "Rename";
            this.butRenameSound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // butPlaySound
            // 
            this.butPlaySound.Image = global::WadTool.Properties.Resources.play_16;
            this.butPlaySound.Location = new System.Drawing.Point(240, 562);
            this.butPlaySound.Name = "butPlaySound";
            this.butPlaySound.Padding = new System.Windows.Forms.Padding(5);
            this.butPlaySound.Size = new System.Drawing.Size(76, 23);
            this.butPlaySound.TabIndex = 15;
            this.butPlaySound.Text = "Play";
            this.butPlaySound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlaySound.Click += new System.EventHandler(this.butPlaySound_Click);
            // 
            // butDeleteObject
            // 
            this.butDeleteObject.Image = global::WadTool.Properties.Resources.trash_16;
            this.butDeleteObject.Location = new System.Drawing.Point(15, 503);
            this.butDeleteObject.Name = "butDeleteObject";
            this.butDeleteObject.Padding = new System.Windows.Forms.Padding(5);
            this.butDeleteObject.Size = new System.Drawing.Size(108, 23);
            this.butDeleteObject.TabIndex = 12;
            this.butDeleteObject.Text = "Delete object";
            this.butDeleteObject.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteObject.Click += new System.EventHandler(this.butDeleteObject_Click);
            // 
            // butAddObjectToDifferentSlot
            // 
            this.butAddObjectToDifferentSlot.Image = global::WadTool.Properties.Resources.angle_left_16;
            this.butAddObjectToDifferentSlot.Location = new System.Drawing.Point(823, 503);
            this.butAddObjectToDifferentSlot.Name = "butAddObjectToDifferentSlot";
            this.butAddObjectToDifferentSlot.Padding = new System.Windows.Forms.Padding(5);
            this.butAddObjectToDifferentSlot.Size = new System.Drawing.Size(172, 23);
            this.butAddObjectToDifferentSlot.TabIndex = 11;
            this.butAddObjectToDifferentSlot.Text = "Add object to different slot";
            this.butAddObjectToDifferentSlot.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddObjectToDifferentSlot.Click += new System.EventHandler(this.butAddObjectToDifferentSlot_Click);
            // 
            // butAddObject
            // 
            this.butAddObject.Image = global::WadTool.Properties.Resources.angle_left_16;
            this.butAddObject.Location = new System.Drawing.Point(716, 503);
            this.butAddObject.Name = "butAddObject";
            this.butAddObject.Padding = new System.Windows.Forms.Padding(5);
            this.butAddObject.Size = new System.Drawing.Size(101, 23);
            this.butAddObject.TabIndex = 10;
            this.butAddObject.Text = "Add object";
            this.butAddObject.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddObject.Click += new System.EventHandler(this.butAddObject_Click);
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
            // debugAction1ToolStripMenuItem1
            // 
            this.debugAction1ToolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.debugAction1ToolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.debugAction1ToolStripMenuItem1.Name = "debugAction1ToolStripMenuItem1";
            this.debugAction1ToolStripMenuItem1.Size = new System.Drawing.Size(154, 22);
            this.debugAction1ToolStripMenuItem1.Text = "Debug action 1";
            this.debugAction1ToolStripMenuItem1.Click += new System.EventHandler(this.debugAction1ToolStripMenuItem1_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 725);
            this.Controls.Add(this.butRenameSound);
            this.Controls.Add(this.treeSounds);
            this.Controls.Add(this.butPlaySound);
            this.Controls.Add(this.darkLabel3);
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
            this.Controls.Add(this.butTest);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DarkUI.Controls.DarkButton butTest;
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
    }
}

