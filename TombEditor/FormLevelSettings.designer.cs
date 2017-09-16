namespace TombEditor
{
    partial class FormLevelSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLevelSettings));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel10 = new System.Windows.Forms.Panel();
            this.soundsIgnoreMissingSounds = new DarkUI.Controls.DarkCheckBox();
            this.soundDataGridViewControls = new TombEditor.Controls.DarkDataGridViewControls();
            this.soundDataGridView = new TombEditor.Controls.DarkDataGridView();
            this.soundDataGridViewColumnPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.soundDataGridViewColumnSearch = new TombEditor.Controls.DarkDataGridViewButtonColumn();
            this.darkLabel10 = new DarkUI.Controls.DarkLabel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.gameLevelFilePathBut = new DarkUI.Controls.DarkButton();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.gameLevelFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.gameExecutableFilePathBut = new DarkUI.Controls.DarkButton();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.gameExecutableFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.textureFilePathBut = new DarkUI.Controls.DarkButton();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.textureFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.panel5 = new System.Windows.Forms.Panel();
            this.wadFilePathBut = new DarkUI.Controls.DarkButton();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.wadFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.panel6 = new System.Windows.Forms.Panel();
            this.levelFilePathBut = new DarkUI.Controls.DarkButton();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.levelFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.panel7 = new System.Windows.Forms.Panel();
            this.gameDirectoryBut = new DarkUI.Controls.DarkButton();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.gameDirectoryTxt = new DarkUI.Controls.DarkTextBox();
            this.panel8 = new System.Windows.Forms.Panel();
            this.fontTextureFilePathPicPreview = new System.Windows.Forms.PictureBox();
            this.fontTextureFilePathBut = new DarkUI.Controls.DarkButton();
            this.fontTextureFilePathOptCustom = new DarkUI.Controls.DarkRadioButton();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.fontTextureFilePathOptAuto = new DarkUI.Controls.DarkRadioButton();
            this.fontTextureFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.panel9 = new System.Windows.Forms.Panel();
            this.skyTextureFilePathPicPreview = new System.Windows.Forms.PictureBox();
            this.skyTextureFilePathBut = new DarkUI.Controls.DarkButton();
            this.skyTextureFilePathOptCustom = new DarkUI.Controls.DarkRadioButton();
            this.darkLabel9 = new DarkUI.Controls.DarkLabel();
            this.skyTextureFilePathOptAuto = new DarkUI.Controls.DarkRadioButton();
            this.skyTextureFilePathTxt = new DarkUI.Controls.DarkTextBox();
            this.panel11 = new System.Windows.Forms.Panel();
            this.butApply = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.pathVariablesDataGridView = new TombEditor.Controls.DarkDataGridView();
            this.pathVariablesDataGridViewNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pathVariablesDataGridViewContextMenu = new DarkUI.Controls.DarkContextMenu();
            this.pathVariablesDataGridViewContextMenuCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.pathVariablesDataGridViewValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.pathToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.gameExecutableSuppressAskingForOptionsCheckBox = new DarkUI.Controls.DarkCheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel10.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.soundDataGridView)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel7.SuspendLayout();
            this.panel8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fontTextureFilePathPicPreview)).BeginInit();
            this.panel9.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.skyTextureFilePathPicPreview)).BeginInit();
            this.panel11.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pathVariablesDataGridView)).BeginInit();
            this.pathVariablesDataGridViewContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel10, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.panel2, 0, 8);
            this.tableLayoutPanel1.Controls.Add(this.panel3, 0, 9);
            this.tableLayoutPanel1.Controls.Add(this.panel4, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel5, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel6, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel7, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.panel8, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.panel9, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.panel11, 0, 11);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 10);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 12;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 74F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 74F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(592, 773);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel10
            // 
            this.panel10.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel10.Controls.Add(this.soundsIgnoreMissingSounds);
            this.panel10.Controls.Add(this.soundDataGridViewControls);
            this.panel10.Controls.Add(this.soundDataGridView);
            this.panel10.Controls.Add(this.darkLabel10);
            this.panel10.Location = new System.Drawing.Point(3, 292);
            this.panel10.Name = "panel10";
            this.panel10.Size = new System.Drawing.Size(586, 143);
            this.panel10.TabIndex = 6;
            // 
            // soundsIgnoreMissingSounds
            // 
            this.soundsIgnoreMissingSounds.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.soundsIgnoreMissingSounds.Location = new System.Drawing.Point(453, 0);
            this.soundsIgnoreMissingSounds.Name = "soundsIgnoreMissingSounds";
            this.soundsIgnoreMissingSounds.Size = new System.Drawing.Size(130, 16);
            this.soundsIgnoreMissingSounds.TabIndex = 4;
            this.soundsIgnoreMissingSounds.Text = "Ignore missing sounds";
            this.soundsIgnoreMissingSounds.CheckedChanged += new System.EventHandler(this.soundsIgnoreMissingSounds_CheckedChanged);
            // 
            // soundDataGridViewControls
            // 
            this.soundDataGridViewControls.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundDataGridViewControls.Enabled = false;
            this.soundDataGridViewControls.Location = new System.Drawing.Point(491, 40);
            this.soundDataGridViewControls.MinimumSize = new System.Drawing.Size(92, 100);
            this.soundDataGridViewControls.Name = "soundDataGridViewControls";
            this.soundDataGridViewControls.NewName = "New";
            this.soundDataGridViewControls.Size = new System.Drawing.Size(92, 100);
            this.soundDataGridViewControls.TabIndex = 3;
            // 
            // soundDataGridView
            // 
            this.soundDataGridView.AllowUserToAddRows = false;
            this.soundDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.soundDataGridView.ColumnHeadersHeight = 17;
            this.soundDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.soundDataGridViewColumnPath,
            this.soundDataGridViewColumnSearch});
            this.soundDataGridView.Location = new System.Drawing.Point(19, 40);
            this.soundDataGridView.Name = "soundDataGridView";
            this.soundDataGridView.RowHeadersWidth = 41;
            this.soundDataGridView.Size = new System.Drawing.Size(466, 98);
            this.soundDataGridView.TabIndex = 2;
            this.soundDataGridView.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.soundDataGridView_CellContentClick);
            this.soundDataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.soundDataGridView_CellFormatting);
            // 
            // soundDataGridViewColumnPath
            // 
            this.soundDataGridViewColumnPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.soundDataGridViewColumnPath.DataPropertyName = "Path";
            this.soundDataGridViewColumnPath.HeaderText = "Path";
            this.soundDataGridViewColumnPath.Name = "soundDataGridViewColumnPath";
            this.soundDataGridViewColumnPath.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // soundDataGridViewColumnSearch
            // 
            this.soundDataGridViewColumnSearch.HeaderText = "";
            this.soundDataGridViewColumnSearch.Name = "soundDataGridViewColumnSearch";
            this.soundDataGridViewColumnSearch.Text = "Search";
            // 
            // darkLabel10
            // 
            this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel10.Location = new System.Drawing.Point(3, 0);
            this.darkLabel10.Name = "darkLabel10";
            this.darkLabel10.Size = new System.Drawing.Size(380, 46);
            this.darkLabel10.TabIndex = 1;
            this.darkLabel10.Text = resources.GetString("darkLabel10.Text");
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.Controls.Add(this.gameLevelFilePathBut);
            this.panel2.Controls.Add(this.darkLabel2);
            this.panel2.Controls.Add(this.gameLevelFilePathTxt);
            this.panel2.Location = new System.Drawing.Point(3, 498);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(586, 41);
            this.panel2.TabIndex = 2;
            // 
            // gameLevelFilePathBut
            // 
            this.gameLevelFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gameLevelFilePathBut.Location = new System.Drawing.Point(491, 15);
            this.gameLevelFilePathBut.Name = "gameLevelFilePathBut";
            this.gameLevelFilePathBut.Padding = new System.Windows.Forms.Padding(5);
            this.gameLevelFilePathBut.Size = new System.Drawing.Size(92, 20);
            this.gameLevelFilePathBut.TabIndex = 3;
            this.gameLevelFilePathBut.Text = "Search";
            this.gameLevelFilePathBut.Click += new System.EventHandler(this.gameLevelFilePathBut_Click);
            // 
            // darkLabel2
            // 
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(2, 0);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(381, 12);
            this.darkLabel2.TabIndex = 1;
            this.darkLabel2.Text = "Target folder for the built *.tr4 file:";
            // 
            // gameLevelFilePathTxt
            // 
            this.gameLevelFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameLevelFilePathTxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.gameLevelFilePathTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gameLevelFilePathTxt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.gameLevelFilePathTxt.Location = new System.Drawing.Point(19, 15);
            this.gameLevelFilePathTxt.Name = "gameLevelFilePathTxt";
            this.gameLevelFilePathTxt.Size = new System.Drawing.Size(466, 20);
            this.gameLevelFilePathTxt.TabIndex = 2;
            this.gameLevelFilePathTxt.TextChanged += new System.EventHandler(this.gameLevelFilePathTxt_TextChanged);
            // 
            // panel3
            // 
            this.panel3.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel3.Controls.Add(this.gameExecutableSuppressAskingForOptionsCheckBox);
            this.panel3.Controls.Add(this.gameExecutableFilePathBut);
            this.panel3.Controls.Add(this.darkLabel3);
            this.panel3.Controls.Add(this.gameExecutableFilePathTxt);
            this.panel3.Location = new System.Drawing.Point(3, 545);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(586, 41);
            this.panel3.TabIndex = 3;
            // 
            // gameExecutableFilePathBut
            // 
            this.gameExecutableFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gameExecutableFilePathBut.Location = new System.Drawing.Point(491, 15);
            this.gameExecutableFilePathBut.Name = "gameExecutableFilePathBut";
            this.gameExecutableFilePathBut.Padding = new System.Windows.Forms.Padding(5);
            this.gameExecutableFilePathBut.Size = new System.Drawing.Size(92, 20);
            this.gameExecutableFilePathBut.TabIndex = 3;
            this.gameExecutableFilePathBut.Text = "Search";
            this.gameExecutableFilePathBut.Click += new System.EventHandler(this.gameExecutableFilePathBut_Click);
            // 
            // darkLabel3
            // 
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(2, 0);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(381, 12);
            this.darkLabel3.TabIndex = 1;
            this.darkLabel3.Text = "Target executable that is started with the \'Build and Play\' button";
            // 
            // gameExecutableFilePathTxt
            // 
            this.gameExecutableFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameExecutableFilePathTxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.gameExecutableFilePathTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gameExecutableFilePathTxt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.gameExecutableFilePathTxt.Location = new System.Drawing.Point(19, 15);
            this.gameExecutableFilePathTxt.Name = "gameExecutableFilePathTxt";
            this.gameExecutableFilePathTxt.Size = new System.Drawing.Size(466, 20);
            this.gameExecutableFilePathTxt.TabIndex = 2;
            this.gameExecutableFilePathTxt.TextChanged += new System.EventHandler(this.gameExecutableFilePathTxt_TextChanged);
            // 
            // panel4
            // 
            this.panel4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel4.Controls.Add(this.textureFilePathBut);
            this.panel4.Controls.Add(this.darkLabel4);
            this.panel4.Controls.Add(this.textureFilePathTxt);
            this.panel4.Location = new System.Drawing.Point(3, 50);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(586, 41);
            this.panel4.TabIndex = 2;
            // 
            // textureFilePathBut
            // 
            this.textureFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.textureFilePathBut.Location = new System.Drawing.Point(491, 15);
            this.textureFilePathBut.Name = "textureFilePathBut";
            this.textureFilePathBut.Padding = new System.Windows.Forms.Padding(5);
            this.textureFilePathBut.Size = new System.Drawing.Size(92, 20);
            this.textureFilePathBut.TabIndex = 3;
            this.textureFilePathBut.Text = "Search";
            this.textureFilePathBut.Click += new System.EventHandler(this.textureFilePathBut_Click);
            // 
            // darkLabel4
            // 
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(2, 0);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(381, 12);
            this.darkLabel4.TabIndex = 1;
            this.darkLabel4.Text = "Texture path:";
            // 
            // textureFilePathTxt
            // 
            this.textureFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textureFilePathTxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.textureFilePathTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textureFilePathTxt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.textureFilePathTxt.Location = new System.Drawing.Point(19, 15);
            this.textureFilePathTxt.Name = "textureFilePathTxt";
            this.textureFilePathTxt.Size = new System.Drawing.Size(466, 20);
            this.textureFilePathTxt.TabIndex = 2;
            this.textureFilePathTxt.TextChanged += new System.EventHandler(this.textureFilePathTxt_TextChanged);
            // 
            // panel5
            // 
            this.panel5.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel5.Controls.Add(this.wadFilePathBut);
            this.panel5.Controls.Add(this.darkLabel5);
            this.panel5.Controls.Add(this.wadFilePathTxt);
            this.panel5.Location = new System.Drawing.Point(3, 97);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(586, 41);
            this.panel5.TabIndex = 2;
            // 
            // wadFilePathBut
            // 
            this.wadFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.wadFilePathBut.Location = new System.Drawing.Point(491, 15);
            this.wadFilePathBut.Name = "wadFilePathBut";
            this.wadFilePathBut.Padding = new System.Windows.Forms.Padding(5);
            this.wadFilePathBut.Size = new System.Drawing.Size(92, 20);
            this.wadFilePathBut.TabIndex = 3;
            this.wadFilePathBut.Text = "Search";
            this.wadFilePathBut.Click += new System.EventHandler(this.wadFilePathBut_Click);
            // 
            // darkLabel5
            // 
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(2, 0);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(381, 12);
            this.darkLabel5.TabIndex = 1;
            this.darkLabel5.Text = "Object file (*.wad) path:";
            // 
            // wadFilePathTxt
            // 
            this.wadFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.wadFilePathTxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.wadFilePathTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.wadFilePathTxt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.wadFilePathTxt.Location = new System.Drawing.Point(19, 15);
            this.wadFilePathTxt.Name = "wadFilePathTxt";
            this.wadFilePathTxt.Size = new System.Drawing.Size(466, 20);
            this.wadFilePathTxt.TabIndex = 2;
            this.wadFilePathTxt.TextChanged += new System.EventHandler(this.wadFilePathTxt_TextChanged);
            // 
            // panel6
            // 
            this.panel6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel6.Controls.Add(this.levelFilePathBut);
            this.panel6.Controls.Add(this.darkLabel6);
            this.panel6.Controls.Add(this.levelFilePathTxt);
            this.panel6.Location = new System.Drawing.Point(3, 3);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(586, 41);
            this.panel6.TabIndex = 2;
            // 
            // levelFilePathBut
            // 
            this.levelFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.levelFilePathBut.Location = new System.Drawing.Point(491, 15);
            this.levelFilePathBut.Name = "levelFilePathBut";
            this.levelFilePathBut.Padding = new System.Windows.Forms.Padding(5);
            this.levelFilePathBut.Size = new System.Drawing.Size(92, 20);
            this.levelFilePathBut.TabIndex = 3;
            this.levelFilePathBut.Text = "Search";
            this.levelFilePathBut.Click += new System.EventHandler(this.levelFilePathBut_Click);
            // 
            // darkLabel6
            // 
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(2, 0);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(384, 12);
            this.darkLabel6.TabIndex = 1;
            this.darkLabel6.Text = "Full file path for the currently open level:";
            // 
            // levelFilePathTxt
            // 
            this.levelFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.levelFilePathTxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.levelFilePathTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.levelFilePathTxt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.levelFilePathTxt.Location = new System.Drawing.Point(19, 15);
            this.levelFilePathTxt.Name = "levelFilePathTxt";
            this.levelFilePathTxt.Size = new System.Drawing.Size(466, 20);
            this.levelFilePathTxt.TabIndex = 2;
            this.levelFilePathTxt.TextChanged += new System.EventHandler(this.levelFilePathTxt_TextChanged);
            // 
            // panel7
            // 
            this.panel7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel7.Controls.Add(this.gameDirectoryBut);
            this.panel7.Controls.Add(this.darkLabel7);
            this.panel7.Controls.Add(this.gameDirectoryTxt);
            this.panel7.Location = new System.Drawing.Point(3, 451);
            this.panel7.Name = "panel7";
            this.panel7.Size = new System.Drawing.Size(586, 41);
            this.panel7.TabIndex = 2;
            // 
            // gameDirectoryBut
            // 
            this.gameDirectoryBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.gameDirectoryBut.Location = new System.Drawing.Point(491, 15);
            this.gameDirectoryBut.Name = "gameDirectoryBut";
            this.gameDirectoryBut.Padding = new System.Windows.Forms.Padding(5);
            this.gameDirectoryBut.Size = new System.Drawing.Size(92, 20);
            this.gameDirectoryBut.TabIndex = 3;
            this.gameDirectoryBut.Text = "Search";
            this.gameDirectoryBut.Click += new System.EventHandler(this.GameDirectoryBut_Click);
            // 
            // darkLabel7
            // 
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(2, 0);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(381, 12);
            this.darkLabel7.TabIndex = 1;
            this.darkLabel7.Text = "Folder in which all runtime game components reside:";
            // 
            // gameDirectoryTxt
            // 
            this.gameDirectoryTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gameDirectoryTxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.gameDirectoryTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.gameDirectoryTxt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.gameDirectoryTxt.Location = new System.Drawing.Point(19, 15);
            this.gameDirectoryTxt.Name = "gameDirectoryTxt";
            this.gameDirectoryTxt.Size = new System.Drawing.Size(466, 20);
            this.gameDirectoryTxt.TabIndex = 2;
            this.gameDirectoryTxt.TextChanged += new System.EventHandler(this.gameDirectoryTxt_TextChanged);
            // 
            // panel8
            // 
            this.panel8.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel8.Controls.Add(this.fontTextureFilePathPicPreview);
            this.panel8.Controls.Add(this.fontTextureFilePathBut);
            this.panel8.Controls.Add(this.fontTextureFilePathOptCustom);
            this.panel8.Controls.Add(this.darkLabel8);
            this.panel8.Controls.Add(this.fontTextureFilePathOptAuto);
            this.panel8.Controls.Add(this.fontTextureFilePathTxt);
            this.panel8.Location = new System.Drawing.Point(3, 144);
            this.panel8.Name = "panel8";
            this.panel8.Size = new System.Drawing.Size(586, 68);
            this.panel8.TabIndex = 2;
            // 
            // fontTextureFilePathPicPreview
            // 
            this.fontTextureFilePathPicPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fontTextureFilePathPicPreview.BackColor = System.Drawing.Color.Gray;
            this.fontTextureFilePathPicPreview.BackgroundImage = global::TombEditor.Properties.Resources.TransparentBackground;
            this.fontTextureFilePathPicPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fontTextureFilePathPicPreview.Location = new System.Drawing.Point(453, 2);
            this.fontTextureFilePathPicPreview.Name = "fontTextureFilePathPicPreview";
            this.fontTextureFilePathPicPreview.Size = new System.Drawing.Size(32, 32);
            this.fontTextureFilePathPicPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.fontTextureFilePathPicPreview.TabIndex = 6;
            this.fontTextureFilePathPicPreview.TabStop = false;
            // 
            // fontTextureFilePathBut
            // 
            this.fontTextureFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.fontTextureFilePathBut.Location = new System.Drawing.Point(491, 40);
            this.fontTextureFilePathBut.Name = "fontTextureFilePathBut";
            this.fontTextureFilePathBut.Padding = new System.Windows.Forms.Padding(5);
            this.fontTextureFilePathBut.Size = new System.Drawing.Size(92, 20);
            this.fontTextureFilePathBut.TabIndex = 3;
            this.fontTextureFilePathBut.Text = "Search";
            this.fontTextureFilePathBut.Click += new System.EventHandler(this.fontTextureFilePathBut_Click);
            // 
            // fontTextureFilePathOptCustom
            // 
            this.fontTextureFilePathOptCustom.Location = new System.Drawing.Point(19, 42);
            this.fontTextureFilePathOptCustom.Name = "fontTextureFilePathOptCustom";
            this.fontTextureFilePathOptCustom.Size = new System.Drawing.Size(162, 17);
            this.fontTextureFilePathOptCustom.TabIndex = 5;
            this.fontTextureFilePathOptCustom.TabStop = true;
            this.fontTextureFilePathOptCustom.Text = "Custom file (has to be 256²)";
            // 
            // darkLabel8
            // 
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(2, 0);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(381, 12);
            this.darkLabel8.TabIndex = 1;
            this.darkLabel8.Text = "Font texture (\'Font.pc\' in the official editor):";
            // 
            // fontTextureFilePathOptAuto
            // 
            this.fontTextureFilePathOptAuto.Checked = true;
            this.fontTextureFilePathOptAuto.Location = new System.Drawing.Point(19, 19);
            this.fontTextureFilePathOptAuto.Name = "fontTextureFilePathOptAuto";
            this.fontTextureFilePathOptAuto.Size = new System.Drawing.Size(450, 17);
            this.fontTextureFilePathOptAuto.TabIndex = 4;
            this.fontTextureFilePathOptAuto.TabStop = true;
            this.fontTextureFilePathOptAuto.Text = "Use default \'Font.pc\' file";
            this.fontTextureFilePathOptAuto.CheckedChanged += new System.EventHandler(this.fontTextureFilePathOptAuto_CheckedChanged);
            // 
            // fontTextureFilePathTxt
            // 
            this.fontTextureFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fontTextureFilePathTxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.fontTextureFilePathTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.fontTextureFilePathTxt.Enabled = false;
            this.fontTextureFilePathTxt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.fontTextureFilePathTxt.Location = new System.Drawing.Point(187, 40);
            this.fontTextureFilePathTxt.Name = "fontTextureFilePathTxt";
            this.fontTextureFilePathTxt.Size = new System.Drawing.Size(298, 20);
            this.fontTextureFilePathTxt.TabIndex = 2;
            this.fontTextureFilePathTxt.TextChanged += new System.EventHandler(this.fontTextureFilePathTxt_TextChanged);
            // 
            // panel9
            // 
            this.panel9.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel9.Controls.Add(this.skyTextureFilePathPicPreview);
            this.panel9.Controls.Add(this.skyTextureFilePathBut);
            this.panel9.Controls.Add(this.skyTextureFilePathOptCustom);
            this.panel9.Controls.Add(this.darkLabel9);
            this.panel9.Controls.Add(this.skyTextureFilePathOptAuto);
            this.panel9.Controls.Add(this.skyTextureFilePathTxt);
            this.panel9.Location = new System.Drawing.Point(3, 218);
            this.panel9.Name = "panel9";
            this.panel9.Size = new System.Drawing.Size(586, 68);
            this.panel9.TabIndex = 2;
            // 
            // skyTextureFilePathPicPreview
            // 
            this.skyTextureFilePathPicPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.skyTextureFilePathPicPreview.BackColor = System.Drawing.Color.Gray;
            this.skyTextureFilePathPicPreview.BackgroundImage = global::TombEditor.Properties.Resources.TransparentBackground;
            this.skyTextureFilePathPicPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.skyTextureFilePathPicPreview.Location = new System.Drawing.Point(453, 2);
            this.skyTextureFilePathPicPreview.Name = "skyTextureFilePathPicPreview";
            this.skyTextureFilePathPicPreview.Size = new System.Drawing.Size(32, 32);
            this.skyTextureFilePathPicPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.skyTextureFilePathPicPreview.TabIndex = 6;
            this.skyTextureFilePathPicPreview.TabStop = false;
            // 
            // skyTextureFilePathBut
            // 
            this.skyTextureFilePathBut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.skyTextureFilePathBut.Location = new System.Drawing.Point(491, 40);
            this.skyTextureFilePathBut.Name = "skyTextureFilePathBut";
            this.skyTextureFilePathBut.Padding = new System.Windows.Forms.Padding(5);
            this.skyTextureFilePathBut.Size = new System.Drawing.Size(92, 20);
            this.skyTextureFilePathBut.TabIndex = 3;
            this.skyTextureFilePathBut.Text = "Search";
            this.skyTextureFilePathBut.Click += new System.EventHandler(this.skyTextureFilePathBut_Click);
            // 
            // skyTextureFilePathOptCustom
            // 
            this.skyTextureFilePathOptCustom.Location = new System.Drawing.Point(19, 42);
            this.skyTextureFilePathOptCustom.Name = "skyTextureFilePathOptCustom";
            this.skyTextureFilePathOptCustom.Size = new System.Drawing.Size(162, 17);
            this.skyTextureFilePathOptCustom.TabIndex = 5;
            this.skyTextureFilePathOptCustom.TabStop = true;
            this.skyTextureFilePathOptCustom.Text = "Custom file (has to be 256²)";
            // 
            // darkLabel9
            // 
            this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel9.Location = new System.Drawing.Point(2, 0);
            this.darkLabel9.Name = "darkLabel9";
            this.darkLabel9.Size = new System.Drawing.Size(381, 12);
            this.darkLabel9.TabIndex = 1;
            this.darkLabel9.Text = "Sky texture (\'pcsky.raw\' in the official editor):     ";
            // 
            // skyTextureFilePathOptAuto
            // 
            this.skyTextureFilePathOptAuto.Checked = true;
            this.skyTextureFilePathOptAuto.Location = new System.Drawing.Point(19, 19);
            this.skyTextureFilePathOptAuto.Name = "skyTextureFilePathOptAuto";
            this.skyTextureFilePathOptAuto.Size = new System.Drawing.Size(450, 17);
            this.skyTextureFilePathOptAuto.TabIndex = 4;
            this.skyTextureFilePathOptAuto.TabStop = true;
            this.skyTextureFilePathOptAuto.Text = "Use default \'pcsky.raw\' file";
            this.skyTextureFilePathOptAuto.CheckedChanged += new System.EventHandler(this.skyTextureFilePathOptAuto_CheckedChanged);
            // 
            // skyTextureFilePathTxt
            // 
            this.skyTextureFilePathTxt.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.skyTextureFilePathTxt.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.skyTextureFilePathTxt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.skyTextureFilePathTxt.Enabled = false;
            this.skyTextureFilePathTxt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.skyTextureFilePathTxt.Location = new System.Drawing.Point(187, 40);
            this.skyTextureFilePathTxt.Name = "skyTextureFilePathTxt";
            this.skyTextureFilePathTxt.Size = new System.Drawing.Size(298, 20);
            this.skyTextureFilePathTxt.TabIndex = 2;
            this.skyTextureFilePathTxt.TextChanged += new System.EventHandler(this.skyTextureFilePathTxt_TextChanged);
            // 
            // panel11
            // 
            this.panel11.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel11.Controls.Add(this.butApply);
            this.panel11.Controls.Add(this.butOk);
            this.panel11.Controls.Add(this.butCancel);
            this.panel11.Location = new System.Drawing.Point(3, 741);
            this.panel11.Name = "panel11";
            this.panel11.Size = new System.Drawing.Size(586, 29);
            this.panel11.TabIndex = 5;
            // 
            // butApply
            // 
            this.butApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butApply.Location = new System.Drawing.Point(181, 2);
            this.butApply.Name = "butApply";
            this.butApply.Padding = new System.Windows.Forms.Padding(5);
            this.butApply.Size = new System.Drawing.Size(130, 24);
            this.butApply.TabIndex = 3;
            this.butApply.Text = "Apply";
            this.butApply.Click += new System.EventHandler(this.butApply_Click);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Location = new System.Drawing.Point(317, 2);
            this.butOk.Name = "butOk";
            this.butOk.Padding = new System.Windows.Forms.Padding(5);
            this.butOk.Size = new System.Drawing.Size(130, 24);
            this.butOk.TabIndex = 3;
            this.butOk.Text = "Ok";
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Location = new System.Drawing.Point(453, 2);
            this.butCancel.Name = "butCancel";
            this.butCancel.Padding = new System.Windows.Forms.Padding(5);
            this.butCancel.Size = new System.Drawing.Size(130, 24);
            this.butCancel.TabIndex = 3;
            this.butCancel.Text = "Cancel";
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.darkLabel1);
            this.panel1.Controls.Add(this.pathVariablesDataGridView);
            this.panel1.Location = new System.Drawing.Point(3, 592);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(586, 143);
            this.panel1.TabIndex = 5;
            // 
            // darkLabel1
            // 
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(2, 0);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(381, 12);
            this.darkLabel1.TabIndex = 1;
            this.darkLabel1.Text = "Available variable place holders that can be used inside paths: ";
            // 
            // pathVariablesDataGridView
            // 
            this.pathVariablesDataGridView.AllowUserToAddRows = false;
            this.pathVariablesDataGridView.AllowUserToDeleteRows = false;
            this.pathVariablesDataGridView.AllowUserToDragDropRows = false;
            this.pathVariablesDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pathVariablesDataGridView.ColumnHeadersHeight = 17;
            this.pathVariablesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.pathVariablesDataGridViewNameColumn,
            this.pathVariablesDataGridViewValueColumn});
            this.pathVariablesDataGridView.Location = new System.Drawing.Point(19, 15);
            this.pathVariablesDataGridView.Name = "pathVariablesDataGridView";
            this.pathVariablesDataGridView.RowHeadersWidth = 41;
            this.pathVariablesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.pathVariablesDataGridView.Size = new System.Drawing.Size(466, 125);
            this.pathVariablesDataGridView.TabIndex = 2;
            this.pathVariablesDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.pathVariablesDataGridView_CellMouseDown);
            // 
            // pathVariablesDataGridViewNameColumn
            // 
            this.pathVariablesDataGridViewNameColumn.ContextMenuStrip = this.pathVariablesDataGridViewContextMenu;
            this.pathVariablesDataGridViewNameColumn.HeaderText = "Placeholder";
            this.pathVariablesDataGridViewNameColumn.MinimumWidth = 50;
            this.pathVariablesDataGridViewNameColumn.Name = "pathVariablesDataGridViewNameColumn";
            this.pathVariablesDataGridViewNameColumn.ReadOnly = true;
            this.pathVariablesDataGridViewNameColumn.Width = 120;
            // 
            // pathVariablesDataGridViewContextMenu
            // 
            this.pathVariablesDataGridViewContextMenu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.pathVariablesDataGridViewContextMenu.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pathVariablesDataGridViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pathVariablesDataGridViewContextMenuCopy});
            this.pathVariablesDataGridViewContextMenu.Name = "variablesListContextMenu";
            this.pathVariablesDataGridViewContextMenu.Size = new System.Drawing.Size(103, 26);
            // 
            // pathVariablesDataGridViewContextMenuCopy
            // 
            this.pathVariablesDataGridViewContextMenuCopy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.pathVariablesDataGridViewContextMenuCopy.Name = "pathVariablesDataGridViewContextMenuCopy";
            this.pathVariablesDataGridViewContextMenuCopy.Size = new System.Drawing.Size(102, 22);
            this.pathVariablesDataGridViewContextMenuCopy.Text = "Copy";
            this.pathVariablesDataGridViewContextMenuCopy.Click += new System.EventHandler(this.pathVariablesDataGridViewContextMenuCopy_Click);
            // 
            // pathVariablesDataGridViewValueColumn
            // 
            this.pathVariablesDataGridViewValueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.pathVariablesDataGridViewValueColumn.ContextMenuStrip = this.pathVariablesDataGridViewContextMenu;
            this.pathVariablesDataGridViewValueColumn.HeaderText = "Value";
            this.pathVariablesDataGridViewValueColumn.Name = "pathVariablesDataGridViewValueColumn";
            this.pathVariablesDataGridViewValueColumn.ReadOnly = true;
            // 
            // pathToolTip
            // 
            this.pathToolTip.AutoPopDelay = 32000;
            this.pathToolTip.InitialDelay = 300;
            this.pathToolTip.ReshowDelay = 100;
            this.pathToolTip.ShowAlways = true;
            // 
            // gameExecutableSuppressAskingForOptionsCheckBox
            // 
            this.gameExecutableSuppressAskingForOptionsCheckBox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.gameExecutableSuppressAskingForOptionsCheckBox.Location = new System.Drawing.Point(382, -2);
            this.gameExecutableSuppressAskingForOptionsCheckBox.Name = "gameExecutableSuppressAskingForOptionsCheckBox";
            this.gameExecutableSuppressAskingForOptionsCheckBox.Size = new System.Drawing.Size(201, 16);
            this.gameExecutableSuppressAskingForOptionsCheckBox.TabIndex = 4;
            this.gameExecutableSuppressAskingForOptionsCheckBox.Text = "Suppress asking for settings dialong";
            this.gameExecutableSuppressAskingForOptionsCheckBox.CheckedChanged += new System.EventHandler(this.gameExecutableSuppressAskingForOptionsCheckBox_CheckedChanged);
            // 
            // FormLevelSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(592, 773);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimizeBox = false;
            this.Name = "FormLevelSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Level Settings";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel10.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.soundDataGridView)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel7.ResumeLayout(false);
            this.panel7.PerformLayout();
            this.panel8.ResumeLayout(false);
            this.panel8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fontTextureFilePathPicPreview)).EndInit();
            this.panel9.ResumeLayout(false);
            this.panel9.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.skyTextureFilePathPicPreview)).EndInit();
            this.panel11.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pathVariablesDataGridView)).EndInit();
            this.pathVariablesDataGridViewContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel2;
        private DarkUI.Controls.DarkButton gameLevelFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTextBox gameLevelFilePathTxt;
        private System.Windows.Forms.Panel panel3;
        private DarkUI.Controls.DarkButton gameExecutableFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkTextBox gameExecutableFilePathTxt;
        private System.Windows.Forms.Panel panel4;
        private DarkUI.Controls.DarkButton textureFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkTextBox textureFilePathTxt;
        private System.Windows.Forms.Panel panel5;
        private DarkUI.Controls.DarkButton fontTextureFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkTextBox wadFilePathTxt;
        private System.Windows.Forms.Panel panel6;
        private DarkUI.Controls.DarkButton levelFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkTextBox levelFilePathTxt;
        private System.Windows.Forms.Panel panel7;
        private DarkUI.Controls.DarkButton gameDirectoryBut;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkTextBox gameDirectoryTxt;
        private System.Windows.Forms.Panel panel8;
        private DarkUI.Controls.DarkRadioButton fontTextureFilePathOptCustom;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkRadioButton fontTextureFilePathOptAuto;
        private DarkUI.Controls.DarkTextBox fontTextureFilePathTxt;
        private System.Windows.Forms.Panel panel9;
        private DarkUI.Controls.DarkButton skyTextureFilePathBut;
        private DarkUI.Controls.DarkRadioButton skyTextureFilePathOptCustom;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkRadioButton skyTextureFilePathOptAuto;
        private DarkUI.Controls.DarkTextBox skyTextureFilePathTxt;
        private DarkUI.Controls.DarkButton wadFilePathBut;
        private System.Windows.Forms.PictureBox fontTextureFilePathPicPreview;
        private System.Windows.Forms.PictureBox skyTextureFilePathPicPreview;
        private System.Windows.Forms.Panel panel10;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private System.Windows.Forms.Panel panel11;
        private DarkUI.Controls.DarkButton butApply;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkButton butCancel;
        private System.Windows.Forms.Panel panel1;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkContextMenu pathVariablesDataGridViewContextMenu;
        private System.Windows.Forms.ToolStripMenuItem pathVariablesDataGridViewContextMenuCopy;
        private System.Windows.Forms.ToolTip pathToolTip;
        private Controls.DarkDataGridViewControls soundDataGridViewControls;
        private Controls.DarkDataGridView soundDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn soundDataGridViewColumnPath;
        private Controls.DarkDataGridViewButtonColumn soundDataGridViewColumnSearch;
        private Controls.DarkDataGridView pathVariablesDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathVariablesDataGridViewNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathVariablesDataGridViewValueColumn;
        private DarkUI.Controls.DarkCheckBox soundsIgnoreMissingSounds;
        private DarkUI.Controls.DarkCheckBox gameExecutableSuppressAskingForOptionsCheckBox;
    }
}