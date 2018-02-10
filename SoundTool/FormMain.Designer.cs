namespace SoundTool
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
            this.darkMenuStrip1 = new DarkUI.Controls.DarkMenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tR1CatalogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tR2CatalogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tR3CatalogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tR4CatalogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tR5CatalogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertTXTToXMLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buildMAINSFXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutSoundToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.tbChance = new DarkUI.Controls.DarkTextBox();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.tbPitch = new DarkUI.Controls.DarkTextBox();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.tbRange = new DarkUI.Controls.DarkTextBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.tbVolume = new DarkUI.Controls.DarkTextBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.cbRandomizePitch = new DarkUI.Controls.DarkCheckBox();
            this.cbRandomizeGain = new DarkUI.Controls.DarkCheckBox();
            this.cbFlagN = new DarkUI.Controls.DarkCheckBox();
            this.tbName = new DarkUI.Controls.DarkTextBox();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.lstSamples = new DarkUI.Controls.DarkListView();
            this.lstSoundInfos = new DarkUI.Controls.DarkListView();
            this.darkStatusStrip1 = new DarkUI.Controls.DarkStatusStrip();
            this.labelStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.butPlaySound = new DarkUI.Controls.DarkButton();
            this.butAddNewWave = new DarkUI.Controls.DarkButton();
            this.butDeleteWave = new DarkUI.Controls.DarkButton();
            this.butSaveChanges = new DarkUI.Controls.DarkButton();
            this.comboLoop = new DarkUI.Controls.DarkComboBox();
            this.cbMandatorySound = new DarkUI.Controls.DarkCheckBox();
            this.cbNgLocked = new DarkUI.Controls.DarkCheckBox();
            this.darkMenuStrip1.SuspendLayout();
            this.darkStatusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // darkMenuStrip1
            // 
            this.darkMenuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkMenuStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.darkMenuStrip1.Location = new System.Drawing.Point(0, 0);
            this.darkMenuStrip1.Name = "darkMenuStrip1";
            this.darkMenuStrip1.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this.darkMenuStrip1.Size = new System.Drawing.Size(581, 24);
            this.darkMenuStrip1.TabIndex = 1;
            this.darkMenuStrip1.Text = "darkMenuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.openToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tR1CatalogToolStripMenuItem,
            this.tR2CatalogToolStripMenuItem,
            this.tR3CatalogToolStripMenuItem,
            this.tR4CatalogToolStripMenuItem,
            this.tR5CatalogToolStripMenuItem});
            this.openToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.openToolStripMenuItem.Image = global::SoundTool.Properties.Resources.general_Open_16;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openToolStripMenuItem.Text = "Open";
            // 
            // tR1CatalogToolStripMenuItem
            // 
            this.tR1CatalogToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tR1CatalogToolStripMenuItem.Enabled = false;
            this.tR1CatalogToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.tR1CatalogToolStripMenuItem.Name = "tR1CatalogToolStripMenuItem";
            this.tR1CatalogToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.tR1CatalogToolStripMenuItem.Text = "TR1 Catalog";
            this.tR1CatalogToolStripMenuItem.Click += new System.EventHandler(this.tR1CatalogToolStripMenuItem_Click);
            // 
            // tR2CatalogToolStripMenuItem
            // 
            this.tR2CatalogToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tR2CatalogToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tR2CatalogToolStripMenuItem.Name = "tR2CatalogToolStripMenuItem";
            this.tR2CatalogToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.tR2CatalogToolStripMenuItem.Text = "TR2 Catalog";
            this.tR2CatalogToolStripMenuItem.Click += new System.EventHandler(this.tR2CatalogToolStripMenuItem_Click);
            // 
            // tR3CatalogToolStripMenuItem
            // 
            this.tR3CatalogToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tR3CatalogToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tR3CatalogToolStripMenuItem.Name = "tR3CatalogToolStripMenuItem";
            this.tR3CatalogToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.tR3CatalogToolStripMenuItem.Text = "TR3 Catalog";
            this.tR3CatalogToolStripMenuItem.Click += new System.EventHandler(this.tR3CatalogToolStripMenuItem_Click);
            // 
            // tR4CatalogToolStripMenuItem
            // 
            this.tR4CatalogToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tR4CatalogToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tR4CatalogToolStripMenuItem.Name = "tR4CatalogToolStripMenuItem";
            this.tR4CatalogToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.tR4CatalogToolStripMenuItem.Text = "TR4 Catalog";
            this.tR4CatalogToolStripMenuItem.Click += new System.EventHandler(this.tR4CatalogToolStripMenuItem_Click);
            // 
            // tR5CatalogToolStripMenuItem
            // 
            this.tR5CatalogToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tR5CatalogToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.tR5CatalogToolStripMenuItem.Name = "tR5CatalogToolStripMenuItem";
            this.tR5CatalogToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.tR5CatalogToolStripMenuItem.Text = "TR5 Catalog";
            this.tR5CatalogToolStripMenuItem.Click += new System.EventHandler(this.tR5CatalogToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.saveToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.saveToolStripMenuItem.Image = global::SoundTool.Properties.Resources.general_Save_16;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripMenuItem1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripMenuItem1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.exitToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.convertTXTToXMLToolStripMenuItem,
            this.buildMAINSFXToolStripMenuItem});
            this.toolsToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // convertTXTToXMLToolStripMenuItem
            // 
            this.convertTXTToXMLToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.convertTXTToXMLToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.convertTXTToXMLToolStripMenuItem.Name = "convertTXTToXMLToolStripMenuItem";
            this.convertTXTToXMLToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.convertTXTToXMLToolStripMenuItem.Text = "Convert TXT to XML";
            this.convertTXTToXMLToolStripMenuItem.Click += new System.EventHandler(this.convertTXTToXMLToolStripMenuItem_Click);
            // 
            // buildMAINSFXToolStripMenuItem
            // 
            this.buildMAINSFXToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.buildMAINSFXToolStripMenuItem.Enabled = false;
            this.buildMAINSFXToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.buildMAINSFXToolStripMenuItem.Name = "buildMAINSFXToolStripMenuItem";
            this.buildMAINSFXToolStripMenuItem.Size = new System.Drawing.Size(181, 22);
            this.buildMAINSFXToolStripMenuItem.Text = "Build MAIN.SFX";
            this.buildMAINSFXToolStripMenuItem.Click += new System.EventHandler(this.buildMAINSFXToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutSoundToolToolStripMenuItem});
            this.helpToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutSoundToolToolStripMenuItem
            // 
            this.aboutSoundToolToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.aboutSoundToolToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.aboutSoundToolToolStripMenuItem.Name = "aboutSoundToolToolStripMenuItem";
            this.aboutSoundToolToolStripMenuItem.Size = new System.Drawing.Size(170, 22);
            this.aboutSoundToolToolStripMenuItem.Text = "About Sound Tool";
            // 
            // darkLabel8
            // 
            this.darkLabel8.AutoSize = true;
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(273, 387);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(76, 13);
            this.darkLabel8.TabIndex = 66;
            this.darkLabel8.Text = "WAV samples:";
            // 
            // darkLabel6
            // 
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(273, 238);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(34, 13);
            this.darkLabel6.TabIndex = 61;
            this.darkLabel6.Text = "Loop:";
            // 
            // tbChance
            // 
            this.tbChance.Location = new System.Drawing.Point(326, 197);
            this.tbChance.Name = "tbChance";
            this.tbChance.Size = new System.Drawing.Size(79, 20);
            this.tbChance.TabIndex = 60;
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(273, 199);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(47, 13);
            this.darkLabel5.TabIndex = 59;
            this.darkLabel5.Text = "Chance:";
            // 
            // tbPitch
            // 
            this.tbPitch.Location = new System.Drawing.Point(326, 171);
            this.tbPitch.Name = "tbPitch";
            this.tbPitch.Size = new System.Drawing.Size(79, 20);
            this.tbPitch.TabIndex = 58;
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(273, 173);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(34, 13);
            this.darkLabel4.TabIndex = 57;
            this.darkLabel4.Text = "Pitch:";
            // 
            // tbRange
            // 
            this.tbRange.Location = new System.Drawing.Point(326, 145);
            this.tbRange.Name = "tbRange";
            this.tbRange.Size = new System.Drawing.Size(79, 20);
            this.tbRange.TabIndex = 56;
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(273, 147);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(42, 13);
            this.darkLabel3.TabIndex = 55;
            this.darkLabel3.Text = "Range:";
            // 
            // tbVolume
            // 
            this.tbVolume.Location = new System.Drawing.Point(326, 119);
            this.tbVolume.Name = "tbVolume";
            this.tbVolume.Size = new System.Drawing.Size(79, 20);
            this.tbVolume.TabIndex = 54;
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(273, 121);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(45, 13);
            this.darkLabel2.TabIndex = 53;
            this.darkLabel2.Text = "Volume:";
            // 
            // cbRandomizePitch
            // 
            this.cbRandomizePitch.AutoSize = true;
            this.cbRandomizePitch.Location = new System.Drawing.Point(276, 320);
            this.cbRandomizePitch.Name = "cbRandomizePitch";
            this.cbRandomizePitch.Size = new System.Drawing.Size(105, 17);
            this.cbRandomizePitch.TabIndex = 52;
            this.cbRandomizePitch.Text = "Randomize pitch";
            // 
            // cbRandomizeGain
            // 
            this.cbRandomizeGain.AutoSize = true;
            this.cbRandomizeGain.Location = new System.Drawing.Point(276, 297);
            this.cbRandomizeGain.Name = "cbRandomizeGain";
            this.cbRandomizeGain.Size = new System.Drawing.Size(102, 17);
            this.cbRandomizeGain.TabIndex = 51;
            this.cbRandomizeGain.Text = "Randomize gain";
            // 
            // cbFlagN
            // 
            this.cbFlagN.AutoSize = true;
            this.cbFlagN.Location = new System.Drawing.Point(276, 274);
            this.cbFlagN.Name = "cbFlagN";
            this.cbFlagN.Size = new System.Drawing.Size(103, 17);
            this.cbFlagN.TabIndex = 50;
            this.cbFlagN.Text = "Unknown N flag";
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(326, 81);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(239, 20);
            this.tbName.TabIndex = 49;
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(273, 83);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(38, 13);
            this.darkLabel1.TabIndex = 48;
            this.darkLabel1.Text = "Name:";
            // 
            // lstSamples
            // 
            this.lstSamples.Location = new System.Drawing.Point(276, 403);
            this.lstSamples.Name = "lstSamples";
            this.lstSamples.Size = new System.Drawing.Size(204, 110);
            this.lstSamples.TabIndex = 47;
            this.lstSamples.Text = "darkListView1";
            // 
            // lstSoundInfos
            // 
            this.lstSoundInfos.Location = new System.Drawing.Point(12, 30);
            this.lstSoundInfos.Name = "lstSoundInfos";
            this.lstSoundInfos.Size = new System.Drawing.Size(241, 483);
            this.lstSoundInfos.TabIndex = 73;
            this.lstSoundInfos.Text = "darkListView1";
            this.lstSoundInfos.Click += new System.EventHandler(this.lstSoundInfos_Click);
            // 
            // darkStatusStrip1
            // 
            this.darkStatusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkStatusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkStatusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.labelStatus});
            this.darkStatusStrip1.Location = new System.Drawing.Point(0, 522);
            this.darkStatusStrip1.Name = "darkStatusStrip1";
            this.darkStatusStrip1.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
            this.darkStatusStrip1.Size = new System.Drawing.Size(581, 32);
            this.darkStatusStrip1.TabIndex = 74;
            this.darkStatusStrip1.Text = "darkStatusStrip1";
            // 
            // labelStatus
            // 
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(0, 0);
            // 
            // butPlaySound
            // 
            this.butPlaySound.Image = global::SoundTool.Properties.Resources.actions_play_16;
            this.butPlaySound.Location = new System.Drawing.Point(486, 461);
            this.butPlaySound.Name = "butPlaySound";
            this.butPlaySound.Size = new System.Drawing.Size(79, 23);
            this.butPlaySound.TabIndex = 69;
            this.butPlaySound.Text = "Play";
            this.butPlaySound.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butPlaySound.Click += new System.EventHandler(this.butPlaySound_Click);
            // 
            // butAddNewWave
            // 
            this.butAddNewWave.Image = global::SoundTool.Properties.Resources.general_plus_math_16;
            this.butAddNewWave.Location = new System.Drawing.Point(486, 403);
            this.butAddNewWave.Name = "butAddNewWave";
            this.butAddNewWave.Size = new System.Drawing.Size(79, 23);
            this.butAddNewWave.TabIndex = 68;
            this.butAddNewWave.Text = "Add new";
            this.butAddNewWave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butAddNewWave.Click += new System.EventHandler(this.butAddNewWave_Click);
            // 
            // butDeleteWave
            // 
            this.butDeleteWave.Image = global::SoundTool.Properties.Resources.general_trash_16;
            this.butDeleteWave.Location = new System.Drawing.Point(486, 432);
            this.butDeleteWave.Name = "butDeleteWave";
            this.butDeleteWave.Size = new System.Drawing.Size(79, 23);
            this.butDeleteWave.TabIndex = 67;
            this.butDeleteWave.Text = "Delete";
            this.butDeleteWave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDeleteWave.Click += new System.EventHandler(this.butDeleteWave_Click);
            // 
            // butSaveChanges
            // 
            this.butSaveChanges.Image = global::SoundTool.Properties.Resources.general_Save_16;
            this.butSaveChanges.Location = new System.Drawing.Point(460, 30);
            this.butSaveChanges.Name = "butSaveChanges";
            this.butSaveChanges.Size = new System.Drawing.Size(105, 23);
            this.butSaveChanges.TabIndex = 65;
            this.butSaveChanges.Text = "Save changes";
            this.butSaveChanges.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butSaveChanges.Visible = false;
            this.butSaveChanges.Click += new System.EventHandler(this.butSaveChanges_Click);
            // 
            // comboLoop
            // 
            this.comboLoop.FormattingEnabled = true;
            this.comboLoop.Items.AddRange(new object[] {
            "None",
            "W (0x01)",
            "R (0x02)",
            "L (0x03)"});
            this.comboLoop.Location = new System.Drawing.Point(326, 235);
            this.comboLoop.Name = "comboLoop";
            this.comboLoop.Size = new System.Drawing.Size(79, 21);
            this.comboLoop.TabIndex = 62;
            this.comboLoop.Text = "None";
            // 
            // cbMandatorySound
            // 
            this.cbMandatorySound.AutoSize = true;
            this.cbMandatorySound.Location = new System.Drawing.Point(276, 30);
            this.cbMandatorySound.Name = "cbMandatorySound";
            this.cbMandatorySound.Size = new System.Drawing.Size(108, 17);
            this.cbMandatorySound.TabIndex = 75;
            this.cbMandatorySound.Text = "Mandatory sound";
            // 
            // cbNgLocked
            // 
            this.cbNgLocked.AutoSize = true;
            this.cbNgLocked.Location = new System.Drawing.Point(276, 53);
            this.cbNgLocked.Name = "cbNgLocked";
            this.cbNgLocked.Size = new System.Drawing.Size(92, 17);
            this.cbNgLocked.TabIndex = 76;
            this.cbNgLocked.Text = "TRNG locked";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(581, 554);
            this.Controls.Add(this.cbNgLocked);
            this.Controls.Add(this.cbMandatorySound);
            this.Controls.Add(this.darkStatusStrip1);
            this.Controls.Add(this.lstSoundInfos);
            this.Controls.Add(this.butPlaySound);
            this.Controls.Add(this.butAddNewWave);
            this.Controls.Add(this.butDeleteWave);
            this.Controls.Add(this.darkLabel8);
            this.Controls.Add(this.butSaveChanges);
            this.Controls.Add(this.comboLoop);
            this.Controls.Add(this.darkLabel6);
            this.Controls.Add(this.tbChance);
            this.Controls.Add(this.darkLabel5);
            this.Controls.Add(this.tbPitch);
            this.Controls.Add(this.darkLabel4);
            this.Controls.Add(this.tbRange);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.tbVolume);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.cbRandomizePitch);
            this.Controls.Add(this.cbRandomizeGain);
            this.Controls.Add(this.cbFlagN);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.lstSamples);
            this.Controls.Add(this.darkMenuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.darkMenuStrip1;
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sound Tool";
            this.darkMenuStrip1.ResumeLayout(false);
            this.darkMenuStrip1.PerformLayout();
            this.darkStatusStrip1.ResumeLayout(false);
            this.darkStatusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkMenuStrip darkMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutSoundToolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tR2CatalogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tR3CatalogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tR4CatalogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tR5CatalogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private DarkUI.Controls.DarkButton butPlaySound;
        private DarkUI.Controls.DarkButton butAddNewWave;
        private DarkUI.Controls.DarkButton butDeleteWave;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkButton butSaveChanges;
        private DarkUI.Controls.DarkComboBox comboLoop;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkTextBox tbChance;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkTextBox tbPitch;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkTextBox tbRange;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkTextBox tbVolume;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkCheckBox cbRandomizePitch;
        private DarkUI.Controls.DarkCheckBox cbRandomizeGain;
        private DarkUI.Controls.DarkCheckBox cbFlagN;
        private DarkUI.Controls.DarkTextBox tbName;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkListView lstSamples;
        private DarkUI.Controls.DarkListView lstSoundInfos;
        private DarkUI.Controls.DarkStatusStrip darkStatusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel labelStatus;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem convertTXTToXMLToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tR1CatalogToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem buildMAINSFXToolStripMenuItem;
        private DarkUI.Controls.DarkCheckBox cbMandatorySound;
        private DarkUI.Controls.DarkCheckBox cbNgLocked;
    }
}

