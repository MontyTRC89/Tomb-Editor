namespace TombEditor.Forms
{
    partial class FormLevelSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLevelSettings));
			pathVariablesDataGridViewContextMenu = new DarkUI.Controls.DarkContextMenu();
			pathVariablesDataGridViewContextMenuCopy = new System.Windows.Forms.ToolStripMenuItem();
			pathToolTip = new System.Windows.Forms.ToolTip(components);
			GameEnableExtraReverbPresetsCheckBox = new DarkUI.Controls.DarkCheckBox();
			cbEnableExtraBlendingModes = new DarkUI.Controls.DarkCheckBox();
			butAssignFromWads = new DarkUI.Controls.DarkButton();
			butRemoveMissing = new DarkUI.Controls.DarkButton();
			butAssignFromSoundSources = new DarkUI.Controls.DarkButton();
			butAssignSoundsFromSelectedCatalogs = new DarkUI.Controls.DarkButton();
			butAssignHardcodedSounds = new DarkUI.Controls.DarkButton();
			butAutodetectSoundsAndAssign = new DarkUI.Controls.DarkButton();
			butDeselectAllSounds = new DarkUI.Controls.DarkButton();
			butSelectAllSounds = new DarkUI.Controls.DarkButton();
			cbKeepSampleRate = new DarkUI.Controls.DarkCheckBox();
			cbRemoveObjects = new DarkUI.Controls.DarkCheckBox();
			cbRearrangeRooms = new DarkUI.Controls.DarkCheckBox();
			cbRemapAnimTextures = new DarkUI.Controls.DarkCheckBox();
			cbDither16BitTextures = new DarkUI.Controls.DarkCheckBox();
			cbOverrideAllLightQuality = new DarkUI.Controls.DarkCheckBox();
			cmbDefaultLightQuality = new DarkUI.Controls.DarkComboBox();
			cbAgressiveFloordataPacking = new DarkUI.Controls.DarkCheckBox();
			cbAgressiveTexturePacking = new DarkUI.Controls.DarkCheckBox();
			numPadding = new DarkUI.Controls.DarkNumericUpDown();
			cbUse32BitLighting = new DarkUI.Controls.DarkCheckBox();
			cbCompressTextures = new DarkUI.Controls.DarkCheckBox();
			optionsList = new DarkUI.Controls.DarkListView();
			butApply = new DarkUI.Controls.DarkButton();
			butOk = new DarkUI.Controls.DarkButton();
			butCancel = new DarkUI.Controls.DarkButton();
			colorDialog = new System.Windows.Forms.ColorDialog();
			darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
			tabbedContainer = new TombLib.Controls.DarkTabbedContainer();
			tabGame = new System.Windows.Forms.TabPage();
			panel3 = new System.Windows.Forms.Panel();
			GameEnableQuickStartFeatureCheckBox = new DarkUI.Controls.DarkCheckBox();
			gameExecutableFilePathBut = new DarkUI.Controls.DarkButton();
			darkLabel3 = new DarkUI.Controls.DarkLabel();
			gameExecutableFilePathTxt = new DarkUI.Controls.DarkTextBox();
			panelLuaPath = new System.Windows.Forms.Panel();
			butBrowseLuaPath = new DarkUI.Controls.DarkButton();
			darkLabel10 = new DarkUI.Controls.DarkLabel();
			tbLuaPath = new DarkUI.Controls.DarkTextBox();
			panelScripts = new System.Windows.Forms.Panel();
			scriptPathBut = new DarkUI.Controls.DarkButton();
			darkLabel15 = new DarkUI.Controls.DarkLabel();
			tbScriptPath = new DarkUI.Controls.DarkTextBox();
			panel1 = new System.Windows.Forms.Panel();
			gameLevelFilePathBut = new DarkUI.Controls.DarkButton();
			gameLevelFilePathTxt = new DarkUI.Controls.DarkTextBox();
			darkLabel2 = new DarkUI.Controls.DarkLabel();
			panel2 = new System.Windows.Forms.Panel();
			gameDirectoryBut = new DarkUI.Controls.DarkButton();
			darkLabel7 = new DarkUI.Controls.DarkLabel();
			gameDirectoryTxt = new DarkUI.Controls.DarkTextBox();
			panel7 = new System.Windows.Forms.Panel();
			comboGameVersion = new DarkUI.Controls.DarkComboBox();
			darkLabel14 = new DarkUI.Controls.DarkLabel();
			tabTextures = new System.Windows.Forms.TabPage();
			textureFileDataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
			textureFileDataGridView = new DarkUI.Controls.DarkDataGridView();
			textureFileDataGridViewPreviewColumn = new System.Windows.Forms.DataGridViewImageColumn();
			textureFileDataGridViewPathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			textureFileDataGridViewSearchColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
			textureFileDataGridViewShowColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
			textureFileDataGridViewMessageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			textureFileDataGridViewSizeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			textureFileDataGridViewReplaceMagentaWithTransparencyColumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
			textureFileDataGridViewConvert512PixelsToDoubleRowsColumn = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
			darkLabel4 = new DarkUI.Controls.DarkLabel();
			tabObjects = new System.Windows.Forms.TabPage();
			objectFileDataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
			objectFileDataGridView = new DarkUI.Controls.DarkDataGridView();
			objectFileDataGridViewPathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			objectFileDataGridViewSearchColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
			objectFileDataGridViewShowColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
			objectFileDataGridViewMessageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			darkLabel5 = new DarkUI.Controls.DarkLabel();
			tabImportedGeometry = new System.Windows.Forms.TabPage();
			darkLabel11 = new DarkUI.Controls.DarkLabel();
			tabStaticMeshes = new System.Windows.Forms.TabPage();
			butSelectAllButShatterStatics = new DarkUI.Controls.DarkButton();
			butDeselectAllStatics = new DarkUI.Controls.DarkButton();
			butSelectAllStatics = new DarkUI.Controls.DarkButton();
			darkLabel19 = new DarkUI.Controls.DarkLabel();
			staticMeshMergeDataGridView = new DarkUI.Controls.DarkDataGridView();
			colMeshName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			colMergeStatics = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
			colInterpretShadesAsEffect = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
			colTintAsAmbient = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
			colClearShades = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
			darkLabel18 = new DarkUI.Controls.DarkLabel();
			darkLabel17 = new DarkUI.Controls.DarkLabel();
			tabSkyAndFont = new System.Windows.Forms.TabPage();
			panelTr5Sprites = new System.Windows.Forms.Panel();
			tr5SpritesTextureFilePathPicPreview = new System.Windows.Forms.PictureBox();
			tr5SpritesTextureFilePathBut = new DarkUI.Controls.DarkButton();
			tr5SpritesFilePathOptCustom = new DarkUI.Controls.DarkRadioButton();
			lblTr5ExtraSprites = new DarkUI.Controls.DarkLabel();
			tr5SpritesFilePathOptAuto = new DarkUI.Controls.DarkRadioButton();
			tr5SpritesTextureFilePathTxt = new DarkUI.Controls.DarkTextBox();
			panelFont = new System.Windows.Forms.Panel();
			fontTextureFilePathPicPreview = new System.Windows.Forms.PictureBox();
			fontTextureFilePathBut = new DarkUI.Controls.DarkButton();
			fontTextureFilePathOptCustom = new DarkUI.Controls.DarkRadioButton();
			darkLabel8 = new DarkUI.Controls.DarkLabel();
			fontTextureFilePathOptAuto = new DarkUI.Controls.DarkRadioButton();
			fontTextureFilePathTxt = new DarkUI.Controls.DarkTextBox();
			panelSky = new System.Windows.Forms.Panel();
			skyTextureFilePathPicPreview = new System.Windows.Forms.PictureBox();
			skyTextureFilePathBut = new DarkUI.Controls.DarkButton();
			skyTextureFilePathOptCustom = new DarkUI.Controls.DarkRadioButton();
			darkLabel9 = new DarkUI.Controls.DarkLabel();
			skyTextureFilePathOptAuto = new DarkUI.Controls.DarkRadioButton();
			skyTextureFilePathTxt = new DarkUI.Controls.DarkTextBox();
			tabSoundsCatalogs = new System.Windows.Forms.TabPage();
			cbAutodetectIfNoneSelected = new DarkUI.Controls.DarkCheckBox();
			darkLabel20 = new DarkUI.Controls.DarkLabel();
			labelSoundsCatalogsStatistics = new DarkUI.Controls.DarkLabel();
			butSearchSounds = new DarkUI.Controls.DarkButton();
			tbFilterSounds = new DarkUI.Controls.DarkTextBox();
			lblCatalogsPrompt = new DarkUI.Controls.DarkLabel();
			soundsCatalogsDataGridView = new DarkUI.Controls.DarkDataGridView();
			SoundsCatalogPathColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			SoundsCatalogSearchColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
			SoundsCatalogReloadButton = new DarkUI.Controls.DarkDataGridViewButtonColumn();
			SoundsCatalogEditColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
			SoundsCatalogsAssignColumn = new DarkUI.Controls.DarkDataGridViewButtonColumn();
			SoundsCatalogsSoundCountColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			SoundsCatalogMessageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			soundsCatalogsDataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
			darkLabel50 = new DarkUI.Controls.DarkLabel();
			selectedSoundsDataGridView = new DarkUI.Controls.DarkDataGridView();
			colSoundsEnabled = new DarkUI.Controls.DarkDataGridViewCheckBoxColumn();
			colSoundID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
			tabSamples = new System.Windows.Forms.TabPage();
			soundDataGridViewControls = new TombLib.Controls.DarkDataGridViewControls();
			soundDataGridView = new DarkUI.Controls.DarkDataGridView();
			soundDataGridViewColumnPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
			soundDataGridViewColumnSearch = new DarkUI.Controls.DarkDataGridViewButtonColumn();
			lblPathsPrompt = new DarkUI.Controls.DarkLabel();
			tabMisc = new System.Windows.Forms.TabPage();
			panelTr5Weather = new System.Windows.Forms.Panel();
			comboTr5Weather = new DarkUI.Controls.DarkComboBox();
			lblTr5Weather = new DarkUI.Controls.DarkLabel();
			panelTr5LaraType = new System.Windows.Forms.Panel();
			comboLaraType = new DarkUI.Controls.DarkComboBox();
			lblLaraType = new DarkUI.Controls.DarkLabel();
			panel6 = new System.Windows.Forms.Panel();
			levelFilePathBut = new DarkUI.Controls.DarkButton();
			darkLabel6 = new DarkUI.Controls.DarkLabel();
			levelFilePathTxt = new DarkUI.Controls.DarkTextBox();
			panel12 = new System.Windows.Forms.Panel();
			cmbSampleRate = new DarkUI.Controls.DarkComboBox();
			darkLabel22 = new DarkUI.Controls.DarkLabel();
			darkLabel13 = new DarkUI.Controls.DarkLabel();
			darkLabel16 = new DarkUI.Controls.DarkLabel();
			panelRoomAmbientLight = new DarkUI.Controls.DarkPanel();
			darkLabel12 = new DarkUI.Controls.DarkLabel();
			tabPaths = new System.Windows.Forms.TabPage();
			darkLabel1 = new DarkUI.Controls.DarkLabel();
			pathVariablesDataGridView = new DarkUI.Controls.DarkDataGridView();
			pathVariablesDataGridViewNameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			pathVariablesDataGridViewValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			colSoundsId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			colSoundsName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			SelectedSoundsCatalogColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			SelectedSoundsGameColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			SelectedSoundsOriginalIdColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
			pathVariablesDataGridViewContextMenu.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)numPadding).BeginInit();
			darkSectionPanel1.SuspendLayout();
			tabbedContainer.SuspendLayout();
			tabGame.SuspendLayout();
			panel3.SuspendLayout();
			panelLuaPath.SuspendLayout();
			panelScripts.SuspendLayout();
			panel1.SuspendLayout();
			panel2.SuspendLayout();
			panel7.SuspendLayout();
			tabTextures.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)textureFileDataGridView).BeginInit();
			tabObjects.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)objectFileDataGridView).BeginInit();
			tabImportedGeometry.SuspendLayout();
			tabStaticMeshes.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)staticMeshMergeDataGridView).BeginInit();
			tabSkyAndFont.SuspendLayout();
			panelTr5Sprites.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)tr5SpritesTextureFilePathPicPreview).BeginInit();
			panelFont.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)fontTextureFilePathPicPreview).BeginInit();
			panelSky.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)skyTextureFilePathPicPreview).BeginInit();
			tabSoundsCatalogs.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)soundsCatalogsDataGridView).BeginInit();
			((System.ComponentModel.ISupportInitialize)selectedSoundsDataGridView).BeginInit();
			tabSamples.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)soundDataGridView).BeginInit();
			tabMisc.SuspendLayout();
			panelTr5Weather.SuspendLayout();
			panelTr5LaraType.SuspendLayout();
			panel6.SuspendLayout();
			panel12.SuspendLayout();
			tabPaths.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)pathVariablesDataGridView).BeginInit();
			SuspendLayout();
			// 
			// pathVariablesDataGridViewContextMenu
			// 
			pathVariablesDataGridViewContextMenu.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			pathVariablesDataGridViewContextMenu.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			pathVariablesDataGridViewContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { pathVariablesDataGridViewContextMenuCopy });
			pathVariablesDataGridViewContextMenu.Name = "variablesListContextMenu";
			pathVariablesDataGridViewContextMenu.Size = new System.Drawing.Size(103, 26);
			// 
			// pathVariablesDataGridViewContextMenuCopy
			// 
			pathVariablesDataGridViewContextMenuCopy.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			pathVariablesDataGridViewContextMenuCopy.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			pathVariablesDataGridViewContextMenuCopy.Name = "pathVariablesDataGridViewContextMenuCopy";
			pathVariablesDataGridViewContextMenuCopy.Size = new System.Drawing.Size(102, 22);
			pathVariablesDataGridViewContextMenuCopy.Text = "Copy";
			pathVariablesDataGridViewContextMenuCopy.Click += pathVariablesDataGridViewContextMenuCopy_Click;
			// 
			// pathToolTip
			// 
			pathToolTip.AutoPopDelay = 32000;
			pathToolTip.InitialDelay = 300;
			pathToolTip.ReshowDelay = 100;
			pathToolTip.ShowAlways = true;
			// 
			// GameEnableExtraReverbPresetsCheckBox
			// 
			GameEnableExtraReverbPresetsCheckBox.AutoSize = true;
			GameEnableExtraReverbPresetsCheckBox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			GameEnableExtraReverbPresetsCheckBox.Location = new System.Drawing.Point(19, 76);
			GameEnableExtraReverbPresetsCheckBox.Name = "GameEnableExtraReverbPresetsCheckBox";
			GameEnableExtraReverbPresetsCheckBox.Size = new System.Drawing.Size(156, 17);
			GameEnableExtraReverbPresetsCheckBox.TabIndex = 7;
			GameEnableExtraReverbPresetsCheckBox.Text = "Show FLEP reverb presets";
			pathToolTip.SetToolTip(GameEnableExtraReverbPresetsCheckBox, "Adds extra reverb presets to UI which are enabled by corresponding FLEP patch.\r\n");
			GameEnableExtraReverbPresetsCheckBox.CheckedChanged += GameEnableExtraReverbPresetsCheckBox_CheckedChanged;
			// 
			// cbEnableExtraBlendingModes
			// 
			cbEnableExtraBlendingModes.AutoSize = true;
			cbEnableExtraBlendingModes.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			cbEnableExtraBlendingModes.Location = new System.Drawing.Point(19, 53);
			cbEnableExtraBlendingModes.Name = "cbEnableExtraBlendingModes";
			cbEnableExtraBlendingModes.Size = new System.Drawing.Size(170, 17);
			cbEnableExtraBlendingModes.TabIndex = 6;
			cbEnableExtraBlendingModes.Text = "Show extra blending modes";
			pathToolTip.SetToolTip(cbEnableExtraBlendingModes, "Adds extra blending modes to UI which are enabled by corresponding FLEP patch.");
			cbEnableExtraBlendingModes.CheckedChanged += cbEnableExtraBlendingModes_CheckedChanged;
			// 
			// butAssignFromWads
			// 
			butAssignFromWads.Checked = false;
			butAssignFromWads.Location = new System.Drawing.Point(495, 245);
			butAssignFromWads.Name = "butAssignFromWads";
			butAssignFromWads.Size = new System.Drawing.Size(72, 22);
			butAssignFromWads.TabIndex = 115;
			butAssignFromWads.Text = "From wads";
			butAssignFromWads.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			pathToolTip.SetToolTip(butAssignFromWads, "Select sounds from wads");
			butAssignFromWads.Click += butAssignFromWads_Click;
			// 
			// butRemoveMissing
			// 
			butRemoveMissing.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butRemoveMissing.Checked = false;
			butRemoveMissing.Location = new System.Drawing.Point(684, 480);
			butRemoveMissing.Name = "butRemoveMissing";
			butRemoveMissing.Size = new System.Drawing.Size(91, 22);
			butRemoveMissing.TabIndex = 114;
			butRemoveMissing.Text = "Clear missing";
			pathToolTip.SetToolTip(butRemoveMissing, "Hide sounds which aren't present in any of the catalogs");
			butRemoveMissing.Click += butRemoveMissing_Click;
			// 
			// butAssignFromSoundSources
			// 
			butAssignFromSoundSources.Checked = false;
			butAssignFromSoundSources.Location = new System.Drawing.Point(373, 245);
			butAssignFromSoundSources.Name = "butAssignFromSoundSources";
			butAssignFromSoundSources.Size = new System.Drawing.Size(116, 22);
			butAssignFromSoundSources.TabIndex = 113;
			butAssignFromSoundSources.Text = "From sound sources";
			butAssignFromSoundSources.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			pathToolTip.SetToolTip(butAssignFromSoundSources, "Select sounds from sound sources placed in level");
			butAssignFromSoundSources.Click += ButAssignFromSoundSources_Click;
			// 
			// butAssignSoundsFromSelectedCatalogs
			// 
			butAssignSoundsFromSelectedCatalogs.Checked = false;
			butAssignSoundsFromSelectedCatalogs.Location = new System.Drawing.Point(236, 245);
			butAssignSoundsFromSelectedCatalogs.Name = "butAssignSoundsFromSelectedCatalogs";
			butAssignSoundsFromSelectedCatalogs.Size = new System.Drawing.Size(131, 22);
			butAssignSoundsFromSelectedCatalogs.TabIndex = 112;
			butAssignSoundsFromSelectedCatalogs.Text = "From selected catalogs";
			butAssignSoundsFromSelectedCatalogs.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			pathToolTip.SetToolTip(butAssignSoundsFromSelectedCatalogs, "Select sounds from selected catalogs");
			butAssignSoundsFromSelectedCatalogs.Click += ButAssignSoundsFromSelectedCatalogs_Click;
			// 
			// butAssignHardcodedSounds
			// 
			butAssignHardcodedSounds.Checked = false;
			butAssignHardcodedSounds.Location = new System.Drawing.Point(105, 245);
			butAssignHardcodedSounds.Name = "butAssignHardcodedSounds";
			butAssignHardcodedSounds.Size = new System.Drawing.Size(125, 22);
			butAssignHardcodedSounds.TabIndex = 111;
			butAssignHardcodedSounds.Text = "Hardcoded & global";
			butAssignHardcodedSounds.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			pathToolTip.SetToolTip(butAssignHardcodedSounds, "Select hardcoded sounds and all sounds from all catalogs which are marked as global");
			butAssignHardcodedSounds.Click += ButAssignHardcodedSounds_Click;
			// 
			// butAutodetectSoundsAndAssign
			// 
			butAutodetectSoundsAndAssign.BackColor = System.Drawing.Color.DarkGreen;
			butAutodetectSoundsAndAssign.BackColorUseGeneric = false;
			butAutodetectSoundsAndAssign.Checked = false;
			butAutodetectSoundsAndAssign.Image = Properties.Resources.actions_light_on_16;
			butAutodetectSoundsAndAssign.Location = new System.Drawing.Point(6, 245);
			butAutodetectSoundsAndAssign.Name = "butAutodetectSoundsAndAssign";
			butAutodetectSoundsAndAssign.Size = new System.Drawing.Size(93, 22);
			butAutodetectSoundsAndAssign.TabIndex = 110;
			butAutodetectSoundsAndAssign.Text = "Autodetect";
			butAutodetectSoundsAndAssign.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			pathToolTip.SetToolTip(butAutodetectSoundsAndAssign, "Find and select all sounds used in a level.\r\nThis includes hardcoded and global sounds, sounds used in animations, sound sources and flipeffect sounds.\r\nScripted sounds are not included.");
			butAutodetectSoundsAndAssign.Click += ButAutodetectSoundsAndAssign_Click;
			// 
			// butDeselectAllSounds
			// 
			butDeselectAllSounds.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butDeselectAllSounds.Checked = false;
			butDeselectAllSounds.Location = new System.Drawing.Point(695, 245);
			butDeselectAllSounds.Name = "butDeselectAllSounds";
			butDeselectAllSounds.Size = new System.Drawing.Size(80, 22);
			butDeselectAllSounds.TabIndex = 108;
			butDeselectAllSounds.Text = "Deselect all";
			pathToolTip.SetToolTip(butDeselectAllSounds, "Deselect all sounds");
			butDeselectAllSounds.Click += butDeselectAllSounds_Click;
			// 
			// butSelectAllSounds
			// 
			butSelectAllSounds.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butSelectAllSounds.Checked = false;
			butSelectAllSounds.Location = new System.Drawing.Point(609, 245);
			butSelectAllSounds.Name = "butSelectAllSounds";
			butSelectAllSounds.Size = new System.Drawing.Size(80, 22);
			butSelectAllSounds.TabIndex = 107;
			butSelectAllSounds.Text = "Select all";
			pathToolTip.SetToolTip(butSelectAllSounds, "Select all available sounds");
			butSelectAllSounds.Click += butSelectAllSounds_Click;
			// 
			// cbKeepSampleRate
			// 
			cbKeepSampleRate.AutoSize = true;
			cbKeepSampleRate.Location = new System.Drawing.Point(3, 281);
			cbKeepSampleRate.Name = "cbKeepSampleRate";
			cbKeepSampleRate.Size = new System.Drawing.Size(209, 17);
			cbKeepSampleRate.TabIndex = 114;
			cbKeepSampleRate.Tag = "";
			cbKeepSampleRate.Text = "Use custom sample rate for sounds:";
			pathToolTip.SetToolTip(cbKeepSampleRate, "Override original game sample rate with custom one.\r\nMay be necessary if level builder uses samples with custom sample rate.");
			cbKeepSampleRate.CheckedChanged += cbKeepSampleRate_CheckedChanged;
			// 
			// cbRemoveObjects
			// 
			cbRemoveObjects.AutoSize = true;
			cbRemoveObjects.Location = new System.Drawing.Point(3, 258);
			cbRemoveObjects.Name = "cbRemoveObjects";
			cbRemoveObjects.Size = new System.Drawing.Size(251, 17);
			cbRemoveObjects.TabIndex = 112;
			cbRemoveObjects.Tag = "";
			cbRemoveObjects.Text = "Remove unused objects from compiled level";
			pathToolTip.SetToolTip(cbRemoveObjects, "Removes moveables and statics that are in the WADs, but not placed in the level.\r\nUse with caution in the case of dynamic object creation with scripts!");
			cbRemoveObjects.CheckedChanged += cbRemoveObjects_CheckedChanged;
			// 
			// cbRearrangeRooms
			// 
			cbRearrangeRooms.AutoSize = true;
			cbRearrangeRooms.Location = new System.Drawing.Point(3, 235);
			cbRearrangeRooms.Name = "cbRearrangeRooms";
			cbRearrangeRooms.Size = new System.Drawing.Size(280, 17);
			cbRearrangeRooms.TabIndex = 111;
			cbRearrangeRooms.Tag = "";
			cbRearrangeRooms.Text = "Rearrange vertically connected rooms if necessary";
			pathToolTip.SetToolTip(cbRearrangeRooms, "Prioritize vertically connected rooms during compilation.\r\nAllows to safely make more rooms until overall amount of vertically connected ones won't hit absolute limit.");
			cbRearrangeRooms.CheckedChanged += cbRearrangeRooms_CheckedChanged;
			// 
			// cbRemapAnimTextures
			// 
			cbRemapAnimTextures.AutoSize = true;
			cbRemapAnimTextures.Location = new System.Drawing.Point(3, 212);
			cbRemapAnimTextures.Name = "cbRemapAnimTextures";
			cbRemapAnimTextures.Size = new System.Drawing.Size(396, 17);
			cbRemapAnimTextures.TabIndex = 110;
			cbRemapAnimTextures.Tag = "";
			cbRemapAnimTextures.Text = "Map animated textures to imported geometry, objects and static meshes";
			pathToolTip.SetToolTip(cbRemapAnimTextures, "Scan all objects for textures which are similar to any animated frame and apply animations to them.");
			cbRemapAnimTextures.CheckedChanged += cbRemapAnimTextures_CheckedChanged;
			// 
			// cbDither16BitTextures
			// 
			cbDither16BitTextures.AutoSize = true;
			cbDither16BitTextures.Location = new System.Drawing.Point(3, 166);
			cbDither16BitTextures.Name = "cbDither16BitTextures";
			cbDither16BitTextures.Size = new System.Drawing.Size(135, 17);
			cbDither16BitTextures.TabIndex = 109;
			cbDither16BitTextures.Tag = "";
			cbDither16BitTextures.Text = "Dither 16-bit textures";
			pathToolTip.SetToolTip(cbDither16BitTextures, "Apply dithering and premultiply alpha channel to brightness for 16-bit textures.");
			cbDither16BitTextures.CheckedChanged += cbDither16BitTextures_CheckedChanged;
			// 
			// cbOverrideAllLightQuality
			// 
			cbOverrideAllLightQuality.AutoSize = true;
			cbOverrideAllLightQuality.Location = new System.Drawing.Point(216, 42);
			cbOverrideAllLightQuality.Name = "cbOverrideAllLightQuality";
			cbOverrideAllLightQuality.Size = new System.Drawing.Size(168, 17);
			cbOverrideAllLightQuality.TabIndex = 108;
			cbOverrideAllLightQuality.Tag = "";
			cbOverrideAllLightQuality.Text = "Override individual settings";
			pathToolTip.SetToolTip(cbOverrideAllLightQuality, "Override raytracing quality for all lights with specified one");
			cbOverrideAllLightQuality.Visible = false;
			cbOverrideAllLightQuality.CheckedChanged += cbOverrideAllLightQuality_CheckedChanged;
			// 
			// cmbDefaultLightQuality
			// 
			cmbDefaultLightQuality.FormattingEnabled = true;
			cmbDefaultLightQuality.Items.AddRange(new object[] { "Low", "Medium", "High" });
			cmbDefaultLightQuality.Location = new System.Drawing.Point(129, 40);
			cmbDefaultLightQuality.Name = "cmbDefaultLightQuality";
			cmbDefaultLightQuality.Size = new System.Drawing.Size(81, 23);
			cmbDefaultLightQuality.TabIndex = 107;
			pathToolTip.SetToolTip(cmbDefaultLightQuality, "Raytracing quality for all lights with 'Default' light quality setting");
			cmbDefaultLightQuality.SelectedIndexChanged += cmbDefaultLightQuality_SelectedIndexChanged;
			// 
			// cbAgressiveFloordataPacking
			// 
			cbAgressiveFloordataPacking.AutoSize = true;
			cbAgressiveFloordataPacking.Location = new System.Drawing.Point(3, 120);
			cbAgressiveFloordataPacking.Name = "cbAgressiveFloordataPacking";
			cbAgressiveFloordataPacking.Size = new System.Drawing.Size(176, 17);
			cbAgressiveFloordataPacking.TabIndex = 105;
			cbAgressiveFloordataPacking.Tag = "";
			cbAgressiveFloordataPacking.Text = "Aggressive floordata packing";
			pathToolTip.SetToolTip(cbAgressiveFloordataPacking, "Scan and merge similar floordata sequences.\r\nRecommended mode for TR1-3 targets.");
			cbAgressiveFloordataPacking.CheckedChanged += cbAgressiveFloordataPacking_CheckedChanged;
			// 
			// cbAgressiveTexturePacking
			// 
			cbAgressiveTexturePacking.AutoSize = true;
			cbAgressiveTexturePacking.Location = new System.Drawing.Point(3, 97);
			cbAgressiveTexturePacking.Name = "cbAgressiveTexturePacking";
			cbAgressiveTexturePacking.Size = new System.Drawing.Size(337, 17);
			cbAgressiveTexturePacking.TabIndex = 104;
			cbAgressiveTexturePacking.Tag = "";
			cbAgressiveTexturePacking.Text = "Aggressive texture packing (merge object and room textures)";
			pathToolTip.SetToolTip(cbAgressiveTexturePacking, "Pack both object and room textures in same texture pages.\r\nRecommended mode for TR1-3 targets.");
			cbAgressiveTexturePacking.CheckedChanged += cbAgressiveTexturePacking_CheckedChanged;
			// 
			// numPadding
			// 
			numPadding.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
			numPadding.Location = new System.Drawing.Point(129, 69);
			numPadding.LoopValues = false;
			numPadding.Name = "numPadding";
			numPadding.Size = new System.Drawing.Size(81, 22);
			numPadding.TabIndex = 102;
			pathToolTip.SetToolTip(numPadding, "Edge pixel padding to prevent texture border bleeding.\r\nIf set to 0, seams between textures may become visible.");
			numPadding.ValueChanged += numPadding_ValueChanged;
			// 
			// cbUse32BitLighting
			// 
			cbUse32BitLighting.AutoSize = true;
			cbUse32BitLighting.Location = new System.Drawing.Point(3, 143);
			cbUse32BitLighting.Name = "cbUse32BitLighting";
			cbUse32BitLighting.Size = new System.Drawing.Size(231, 17);
			cbUse32BitLighting.TabIndex = 116;
			cbUse32BitLighting.Tag = "";
			cbUse32BitLighting.Text = "Use 32-bit lighting (requires FLEP patch)";
			pathToolTip.SetToolTip(cbUse32BitLighting, "Changes room lighting format from 16-bit to 32-bit, which fixes \"rainbow bug\".\r\nRequires specific FLEP patch to be activated, otherwise lighting will not look correct.");
			cbUse32BitLighting.CheckedChanged += cbUse32BitLighting_CheckedChanged;
			// 
			// cbCompressTextures
			// 
			cbCompressTextures.AutoSize = true;
			cbCompressTextures.Location = new System.Drawing.Point(3, 189);
			cbCompressTextures.Name = "cbCompressTextures";
			cbCompressTextures.Size = new System.Drawing.Size(120, 17);
			cbCompressTextures.TabIndex = 117;
			cbCompressTextures.Tag = "";
			cbCompressTextures.Text = "Compress textures";
			pathToolTip.SetToolTip(cbCompressTextures, "Use DirectX texture compression for all level textures.\r\nUse with caution, as it may cause artifacts on a legacy small texture fragments.");
			cbCompressTextures.CheckedChanged += cbCompressTextures_CheckedChanged;
			// 
			// optionsList
			// 
			optionsList.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			optionsList.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			optionsList.Location = new System.Drawing.Point(5, 5);
			optionsList.Name = "optionsList";
			optionsList.Size = new System.Drawing.Size(198, 533);
			optionsList.TabIndex = 6;
			// 
			// butApply
			// 
			butApply.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butApply.Checked = false;
			butApply.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			butApply.Location = new System.Drawing.Point(743, 544);
			butApply.Name = "butApply";
			butApply.Size = new System.Drawing.Size(80, 24);
			butApply.TabIndex = 3;
			butApply.Text = "Apply";
			butApply.Click += butApply_Click;
			// 
			// butOk
			// 
			butOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butOk.Checked = false;
			butOk.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			butOk.Location = new System.Drawing.Point(829, 544);
			butOk.Name = "butOk";
			butOk.Size = new System.Drawing.Size(80, 24);
			butOk.TabIndex = 3;
			butOk.Text = "OK";
			butOk.Click += butOk_Click;
			// 
			// butCancel
			// 
			butCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butCancel.Checked = false;
			butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			butCancel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			butCancel.Location = new System.Drawing.Point(915, 544);
			butCancel.Name = "butCancel";
			butCancel.Size = new System.Drawing.Size(80, 24);
			butCancel.TabIndex = 3;
			butCancel.Text = "Cancel";
			butCancel.Click += butCancel_Click;
			// 
			// colorDialog
			// 
			colorDialog.AnyColor = true;
			colorDialog.FullOpen = true;
			// 
			// darkSectionPanel1
			// 
			darkSectionPanel1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			darkSectionPanel1.Controls.Add(tabbedContainer);
			darkSectionPanel1.Location = new System.Drawing.Point(208, 5);
			darkSectionPanel1.Name = "darkSectionPanel1";
			darkSectionPanel1.SectionHeader = null;
			darkSectionPanel1.Size = new System.Drawing.Size(788, 533);
			darkSectionPanel1.TabIndex = 7;
			// 
			// tabbedContainer
			// 
			tabbedContainer.Controls.Add(tabGame);
			tabbedContainer.Controls.Add(tabTextures);
			tabbedContainer.Controls.Add(tabObjects);
			tabbedContainer.Controls.Add(tabImportedGeometry);
			tabbedContainer.Controls.Add(tabStaticMeshes);
			tabbedContainer.Controls.Add(tabSkyAndFont);
			tabbedContainer.Controls.Add(tabSoundsCatalogs);
			tabbedContainer.Controls.Add(tabSamples);
			tabbedContainer.Controls.Add(tabMisc);
			tabbedContainer.Controls.Add(tabPaths);
			tabbedContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			tabbedContainer.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			tabbedContainer.Location = new System.Drawing.Point(1, 1);
			tabbedContainer.Name = "tabbedContainer";
			tabbedContainer.SelectedIndex = 0;
			tabbedContainer.Size = new System.Drawing.Size(786, 531);
			tabbedContainer.TabIndex = 2;
			// 
			// tabGame
			// 
			tabGame.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabGame.Controls.Add(panel3);
			tabGame.Controls.Add(panelLuaPath);
			tabGame.Controls.Add(panelScripts);
			tabGame.Controls.Add(panel1);
			tabGame.Controls.Add(panel2);
			tabGame.Controls.Add(panel7);
			tabGame.Location = new System.Drawing.Point(4, 22);
			tabGame.Name = "tabGame";
			tabGame.Padding = new System.Windows.Forms.Padding(3);
			tabGame.Size = new System.Drawing.Size(778, 505);
			tabGame.TabIndex = 4;
			tabGame.Text = "Game";
			// 
			// panel3
			// 
			panel3.Controls.Add(GameEnableExtraReverbPresetsCheckBox);
			panel3.Controls.Add(cbEnableExtraBlendingModes);
			panel3.Controls.Add(GameEnableQuickStartFeatureCheckBox);
			panel3.Controls.Add(gameExecutableFilePathBut);
			panel3.Controls.Add(darkLabel3);
			panel3.Controls.Add(gameExecutableFilePathTxt);
			panel3.Dock = System.Windows.Forms.DockStyle.Fill;
			panel3.Location = new System.Drawing.Point(3, 253);
			panel3.Name = "panel3";
			panel3.Size = new System.Drawing.Size(772, 249);
			panel3.TabIndex = 3;
			// 
			// GameEnableQuickStartFeatureCheckBox
			// 
			GameEnableQuickStartFeatureCheckBox.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			GameEnableQuickStartFeatureCheckBox.Location = new System.Drawing.Point(19, 99);
			GameEnableQuickStartFeatureCheckBox.Name = "GameEnableQuickStartFeatureCheckBox";
			GameEnableQuickStartFeatureCheckBox.Size = new System.Drawing.Size(420, 16);
			GameEnableQuickStartFeatureCheckBox.TabIndex = 4;
			GameEnableQuickStartFeatureCheckBox.Text = "Enable Tomb4.exe quick start feature";
			GameEnableQuickStartFeatureCheckBox.CheckedChanged += GameEnableQuickStartFeatureCheckBox_CheckedChanged;
			// 
			// gameExecutableFilePathBut
			// 
			gameExecutableFilePathBut.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			gameExecutableFilePathBut.Checked = false;
			gameExecutableFilePathBut.Location = new System.Drawing.Point(677, 23);
			gameExecutableFilePathBut.Name = "gameExecutableFilePathBut";
			gameExecutableFilePathBut.Size = new System.Drawing.Size(92, 22);
			gameExecutableFilePathBut.TabIndex = 3;
			gameExecutableFilePathBut.Text = "Browse";
			gameExecutableFilePathBut.Click += gameExecutableFilePathBut_Click;
			// 
			// darkLabel3
			// 
			darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel3.Location = new System.Drawing.Point(0, 2);
			darkLabel3.Name = "darkLabel3";
			darkLabel3.Size = new System.Drawing.Size(439, 17);
			darkLabel3.TabIndex = 1;
			darkLabel3.Text = "Target executable that is started with the 'Build and Play' button";
			// 
			// gameExecutableFilePathTxt
			// 
			gameExecutableFilePathTxt.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			gameExecutableFilePathTxt.Location = new System.Drawing.Point(19, 23);
			gameExecutableFilePathTxt.Name = "gameExecutableFilePathTxt";
			gameExecutableFilePathTxt.Size = new System.Drawing.Size(652, 22);
			gameExecutableFilePathTxt.TabIndex = 2;
			gameExecutableFilePathTxt.TextChanged += gameExecutableFilePathTxt_TextChanged;
			// 
			// panelLuaPath
			// 
			panelLuaPath.Controls.Add(butBrowseLuaPath);
			panelLuaPath.Controls.Add(darkLabel10);
			panelLuaPath.Controls.Add(tbLuaPath);
			panelLuaPath.Dock = System.Windows.Forms.DockStyle.Top;
			panelLuaPath.Location = new System.Drawing.Point(3, 203);
			panelLuaPath.Name = "panelLuaPath";
			panelLuaPath.Size = new System.Drawing.Size(772, 50);
			panelLuaPath.TabIndex = 100;
			// 
			// butBrowseLuaPath
			// 
			butBrowseLuaPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			butBrowseLuaPath.Checked = false;
			butBrowseLuaPath.Location = new System.Drawing.Point(677, 23);
			butBrowseLuaPath.Name = "butBrowseLuaPath";
			butBrowseLuaPath.Size = new System.Drawing.Size(92, 22);
			butBrowseLuaPath.TabIndex = 3;
			butBrowseLuaPath.Text = "Browse";
			butBrowseLuaPath.Click += butBrowseLuaPath_Click;
			// 
			// darkLabel10
			// 
			darkLabel10.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel10.Location = new System.Drawing.Point(0, 3);
			darkLabel10.Name = "darkLabel10";
			darkLabel10.Size = new System.Drawing.Size(439, 17);
			darkLabel10.TabIndex = 1;
			darkLabel10.Text = "Path of Lua level script file:";
			// 
			// tbLuaPath
			// 
			tbLuaPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbLuaPath.Location = new System.Drawing.Point(19, 23);
			tbLuaPath.Name = "tbLuaPath";
			tbLuaPath.Size = new System.Drawing.Size(652, 22);
			tbLuaPath.TabIndex = 2;
			// 
			// panelScripts
			// 
			panelScripts.Controls.Add(scriptPathBut);
			panelScripts.Controls.Add(darkLabel15);
			panelScripts.Controls.Add(tbScriptPath);
			panelScripts.Dock = System.Windows.Forms.DockStyle.Top;
			panelScripts.Location = new System.Drawing.Point(3, 153);
			panelScripts.Name = "panelScripts";
			panelScripts.Size = new System.Drawing.Size(772, 50);
			panelScripts.TabIndex = 99;
			// 
			// scriptPathBut
			// 
			scriptPathBut.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			scriptPathBut.Checked = false;
			scriptPathBut.Location = new System.Drawing.Point(677, 23);
			scriptPathBut.Name = "scriptPathBut";
			scriptPathBut.Size = new System.Drawing.Size(92, 22);
			scriptPathBut.TabIndex = 3;
			scriptPathBut.Text = "Browse";
			scriptPathBut.Click += scriptPathBut_Click;
			// 
			// darkLabel15
			// 
			darkLabel15.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel15.Location = new System.Drawing.Point(0, 3);
			darkLabel15.Name = "darkLabel15";
			darkLabel15.Size = new System.Drawing.Size(439, 17);
			darkLabel15.TabIndex = 1;
			darkLabel15.Text = "Path of TXT files for script (Optional):";
			// 
			// tbScriptPath
			// 
			tbScriptPath.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tbScriptPath.Location = new System.Drawing.Point(19, 23);
			tbScriptPath.Name = "tbScriptPath";
			tbScriptPath.Size = new System.Drawing.Size(652, 22);
			tbScriptPath.TabIndex = 2;
			// 
			// panel1
			// 
			panel1.Controls.Add(gameLevelFilePathBut);
			panel1.Controls.Add(gameLevelFilePathTxt);
			panel1.Controls.Add(darkLabel2);
			panel1.Dock = System.Windows.Forms.DockStyle.Top;
			panel1.Location = new System.Drawing.Point(3, 103);
			panel1.Name = "panel1";
			panel1.Size = new System.Drawing.Size(772, 50);
			panel1.TabIndex = 3;
			// 
			// gameLevelFilePathBut
			// 
			gameLevelFilePathBut.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			gameLevelFilePathBut.Checked = false;
			gameLevelFilePathBut.Location = new System.Drawing.Point(677, 23);
			gameLevelFilePathBut.Name = "gameLevelFilePathBut";
			gameLevelFilePathBut.Size = new System.Drawing.Size(92, 22);
			gameLevelFilePathBut.TabIndex = 3;
			gameLevelFilePathBut.Text = "Browse";
			gameLevelFilePathBut.Click += gameLevelFilePathBut_Click;
			// 
			// gameLevelFilePathTxt
			// 
			gameLevelFilePathTxt.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			gameLevelFilePathTxt.Location = new System.Drawing.Point(19, 23);
			gameLevelFilePathTxt.Name = "gameLevelFilePathTxt";
			gameLevelFilePathTxt.Size = new System.Drawing.Size(652, 22);
			gameLevelFilePathTxt.TabIndex = 2;
			gameLevelFilePathTxt.TextChanged += gameLevelFilePathTxt_TextChanged;
			// 
			// darkLabel2
			// 
			darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel2.Location = new System.Drawing.Point(0, 3);
			darkLabel2.Name = "darkLabel2";
			darkLabel2.Size = new System.Drawing.Size(439, 17);
			darkLabel2.TabIndex = 1;
			darkLabel2.Text = "Target folder and filename for level file:";
			// 
			// panel2
			// 
			panel2.Controls.Add(gameDirectoryBut);
			panel2.Controls.Add(darkLabel7);
			panel2.Controls.Add(gameDirectoryTxt);
			panel2.Dock = System.Windows.Forms.DockStyle.Top;
			panel2.Location = new System.Drawing.Point(3, 53);
			panel2.Name = "panel2";
			panel2.Size = new System.Drawing.Size(772, 50);
			panel2.TabIndex = 2;
			// 
			// gameDirectoryBut
			// 
			gameDirectoryBut.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			gameDirectoryBut.Checked = false;
			gameDirectoryBut.Location = new System.Drawing.Point(677, 23);
			gameDirectoryBut.Name = "gameDirectoryBut";
			gameDirectoryBut.Size = new System.Drawing.Size(92, 22);
			gameDirectoryBut.TabIndex = 3;
			gameDirectoryBut.Text = "Browse";
			gameDirectoryBut.Click += GameDirectoryBut_Click;
			// 
			// darkLabel7
			// 
			darkLabel7.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel7.Location = new System.Drawing.Point(0, 3);
			darkLabel7.Name = "darkLabel7";
			darkLabel7.Size = new System.Drawing.Size(439, 17);
			darkLabel7.TabIndex = 1;
			darkLabel7.Text = "Folder in which all runtime game components reside:";
			// 
			// gameDirectoryTxt
			// 
			gameDirectoryTxt.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			gameDirectoryTxt.Location = new System.Drawing.Point(19, 23);
			gameDirectoryTxt.Name = "gameDirectoryTxt";
			gameDirectoryTxt.Size = new System.Drawing.Size(652, 22);
			gameDirectoryTxt.TabIndex = 2;
			gameDirectoryTxt.TextChanged += gameDirectoryTxt_TextChanged;
			// 
			// panel7
			// 
			panel7.Controls.Add(comboGameVersion);
			panel7.Controls.Add(darkLabel14);
			panel7.Dock = System.Windows.Forms.DockStyle.Top;
			panel7.Location = new System.Drawing.Point(3, 3);
			panel7.Name = "panel7";
			panel7.Size = new System.Drawing.Size(772, 50);
			panel7.TabIndex = 2;
			// 
			// comboGameVersion
			// 
			comboGameVersion.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			comboGameVersion.FormattingEnabled = true;
			comboGameVersion.Location = new System.Drawing.Point(19, 20);
			comboGameVersion.Name = "comboGameVersion";
			comboGameVersion.Size = new System.Drawing.Size(652, 23);
			comboGameVersion.TabIndex = 2;
			comboGameVersion.SelectedIndexChanged += comboGameVersion_SelectedIndexChanged;
			// 
			// darkLabel14
			// 
			darkLabel14.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel14.Location = new System.Drawing.Point(2, 0);
			darkLabel14.Name = "darkLabel14";
			darkLabel14.Size = new System.Drawing.Size(439, 17);
			darkLabel14.TabIndex = 1;
			darkLabel14.Text = "Game version to target:";
			// 
			// tabTextures
			// 
			tabTextures.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabTextures.Controls.Add(textureFileDataGridViewControls);
			tabTextures.Controls.Add(textureFileDataGridView);
			tabTextures.Controls.Add(darkLabel4);
			tabTextures.Location = new System.Drawing.Point(4, 22);
			tabTextures.Name = "tabTextures";
			tabTextures.Padding = new System.Windows.Forms.Padding(3);
			tabTextures.Size = new System.Drawing.Size(778, 505);
			tabTextures.TabIndex = 7;
			tabTextures.Text = "Texture Files";
			// 
			// textureFileDataGridViewControls
			// 
			textureFileDataGridViewControls.AllowUserDelete = false;
			textureFileDataGridViewControls.AllowUserMove = false;
			textureFileDataGridViewControls.AllowUserNew = false;
			textureFileDataGridViewControls.AlwaysInsertAtZero = false;
			textureFileDataGridViewControls.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			textureFileDataGridViewControls.Enabled = false;
			textureFileDataGridViewControls.Location = new System.Drawing.Point(751, 32);
			textureFileDataGridViewControls.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			textureFileDataGridViewControls.MinimumSize = new System.Drawing.Size(24, 100);
			textureFileDataGridViewControls.Name = "textureFileDataGridViewControls";
			textureFileDataGridViewControls.Size = new System.Drawing.Size(24, 467);
			textureFileDataGridViewControls.TabIndex = 7;
			// 
			// textureFileDataGridView
			// 
			textureFileDataGridView.AllowUserToAddRows = false;
			textureFileDataGridView.AllowUserToDragDropRows = false;
			textureFileDataGridView.AllowUserToResizeRows = true;
			textureFileDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			textureFileDataGridView.AutoGenerateColumns = false;
			textureFileDataGridView.ColumnHeadersHeight = 19;
			textureFileDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { textureFileDataGridViewPreviewColumn, textureFileDataGridViewPathColumn, textureFileDataGridViewSearchColumn, textureFileDataGridViewShowColumn, textureFileDataGridViewMessageColumn, textureFileDataGridViewSizeColumn, textureFileDataGridViewReplaceMagentaWithTransparencyColumn, textureFileDataGridViewConvert512PixelsToDoubleRowsColumn });
			textureFileDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(220, 220, 220);
			textureFileDataGridView.Location = new System.Drawing.Point(6, 32);
			textureFileDataGridView.Name = "textureFileDataGridView";
			textureFileDataGridView.RowHeadersWidth = 41;
			textureFileDataGridView.RowTemplate.Height = 40;
			textureFileDataGridView.Size = new System.Drawing.Size(739, 467);
			textureFileDataGridView.TabIndex = 6;
			textureFileDataGridView.CellContentClick += textureFileDataGridView_CellContentClick;
			textureFileDataGridView.CellFormatting += textureFileDataGridView_CellFormatting;
			// 
			// textureFileDataGridViewPreviewColumn
			// 
			textureFileDataGridViewPreviewColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			textureFileDataGridViewPreviewColumn.HeaderText = "Preview";
			textureFileDataGridViewPreviewColumn.Name = "textureFileDataGridViewPreviewColumn";
			textureFileDataGridViewPreviewColumn.ReadOnly = true;
			textureFileDataGridViewPreviewColumn.Width = 60;
			// 
			// textureFileDataGridViewPathColumn
			// 
			textureFileDataGridViewPathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			textureFileDataGridViewPathColumn.DataPropertyName = "Path";
			textureFileDataGridViewPathColumn.HeaderText = "Path";
			textureFileDataGridViewPathColumn.Name = "textureFileDataGridViewPathColumn";
			// 
			// textureFileDataGridViewSearchColumn
			// 
			textureFileDataGridViewSearchColumn.HeaderText = "";
			textureFileDataGridViewSearchColumn.Name = "textureFileDataGridViewSearchColumn";
			textureFileDataGridViewSearchColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			textureFileDataGridViewSearchColumn.Text = "Browse";
			textureFileDataGridViewSearchColumn.Width = 60;
			// 
			// textureFileDataGridViewShowColumn
			// 
			textureFileDataGridViewShowColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
			textureFileDataGridViewShowColumn.FillWeight = 45F;
			textureFileDataGridViewShowColumn.HeaderText = "Show";
			textureFileDataGridViewShowColumn.Name = "textureFileDataGridViewShowColumn";
			textureFileDataGridViewShowColumn.Text = "◀";
			textureFileDataGridViewShowColumn.Width = 45;
			// 
			// textureFileDataGridViewMessageColumn
			// 
			textureFileDataGridViewMessageColumn.DataPropertyName = "Message";
			textureFileDataGridViewMessageColumn.HeaderText = "Message";
			textureFileDataGridViewMessageColumn.Name = "textureFileDataGridViewMessageColumn";
			textureFileDataGridViewMessageColumn.ReadOnly = true;
			textureFileDataGridViewMessageColumn.Width = 150;
			// 
			// textureFileDataGridViewSizeColumn
			// 
			textureFileDataGridViewSizeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			textureFileDataGridViewSizeColumn.DataPropertyName = "Size";
			textureFileDataGridViewSizeColumn.HeaderText = "Size";
			textureFileDataGridViewSizeColumn.Name = "textureFileDataGridViewSizeColumn";
			textureFileDataGridViewSizeColumn.ReadOnly = true;
			textureFileDataGridViewSizeColumn.Width = 51;
			// 
			// textureFileDataGridViewReplaceMagentaWithTransparencyColumn
			// 
			textureFileDataGridViewReplaceMagentaWithTransparencyColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			textureFileDataGridViewReplaceMagentaWithTransparencyColumn.DataPropertyName = "ReplaceMagentaWithTransparency";
			textureFileDataGridViewReplaceMagentaWithTransparencyColumn.HeaderText = "Magenta to alpha";
			textureFileDataGridViewReplaceMagentaWithTransparencyColumn.Name = "textureFileDataGridViewReplaceMagentaWithTransparencyColumn";
			textureFileDataGridViewReplaceMagentaWithTransparencyColumn.ReadOnly = true;
			textureFileDataGridViewReplaceMagentaWithTransparencyColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			textureFileDataGridViewReplaceMagentaWithTransparencyColumn.Width = 105;
			// 
			// textureFileDataGridViewConvert512PixelsToDoubleRowsColumn
			// 
			textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.DataPropertyName = "Convert512PixelsToDoubleRows";
			textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.HeaderText = "Set width of 512 to 256";
			textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.Name = "textureFileDataGridViewConvert512PixelsToDoubleRowsColumn";
			textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.ReadOnly = true;
			textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			textureFileDataGridViewConvert512PixelsToDoubleRowsColumn.Width = 131;
			// 
			// darkLabel4
			// 
			darkLabel4.AutoSize = true;
			darkLabel4.Dock = System.Windows.Forms.DockStyle.Top;
			darkLabel4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel4.Location = new System.Drawing.Point(3, 3);
			darkLabel4.Name = "darkLabel4";
			darkLabel4.Size = new System.Drawing.Size(411, 26);
			darkLabel4.TabIndex = 5;
			darkLabel4.Text = "List of texture resources (eg *.png, *.tga, *.bmp).\r\nOnly the areas used in the textures will be compiled into the playable level file.";
			// 
			// tabObjects
			// 
			tabObjects.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabObjects.Controls.Add(objectFileDataGridViewControls);
			tabObjects.Controls.Add(objectFileDataGridView);
			tabObjects.Controls.Add(darkLabel5);
			tabObjects.Location = new System.Drawing.Point(4, 22);
			tabObjects.Name = "tabObjects";
			tabObjects.Padding = new System.Windows.Forms.Padding(3);
			tabObjects.Size = new System.Drawing.Size(778, 505);
			tabObjects.TabIndex = 0;
			tabObjects.Text = "Object Files";
			// 
			// objectFileDataGridViewControls
			// 
			objectFileDataGridViewControls.AllowUserDelete = false;
			objectFileDataGridViewControls.AllowUserMove = false;
			objectFileDataGridViewControls.AllowUserNew = false;
			objectFileDataGridViewControls.AlwaysInsertAtZero = true;
			objectFileDataGridViewControls.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			objectFileDataGridViewControls.Enabled = false;
			objectFileDataGridViewControls.Location = new System.Drawing.Point(751, 32);
			objectFileDataGridViewControls.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			objectFileDataGridViewControls.MinimumSize = new System.Drawing.Size(24, 100);
			objectFileDataGridViewControls.Name = "objectFileDataGridViewControls";
			objectFileDataGridViewControls.Size = new System.Drawing.Size(24, 467);
			objectFileDataGridViewControls.TabIndex = 5;
			// 
			// objectFileDataGridView
			// 
			objectFileDataGridView.AllowUserToAddRows = false;
			objectFileDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			objectFileDataGridView.AutoGenerateColumns = false;
			objectFileDataGridView.ColumnHeadersHeight = 19;
			objectFileDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { objectFileDataGridViewPathColumn, objectFileDataGridViewSearchColumn, objectFileDataGridViewShowColumn, objectFileDataGridViewMessageColumn });
			objectFileDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(220, 220, 220);
			objectFileDataGridView.Location = new System.Drawing.Point(6, 32);
			objectFileDataGridView.Name = "objectFileDataGridView";
			objectFileDataGridView.RowHeadersWidth = 41;
			objectFileDataGridView.Size = new System.Drawing.Size(739, 467);
			objectFileDataGridView.TabIndex = 4;
			objectFileDataGridView.CellContentClick += objectFileDataGridView_CellContentClick;
			objectFileDataGridView.CellFormatting += objectFileDataGridView_CellFormatting;
			// 
			// objectFileDataGridViewPathColumn
			// 
			objectFileDataGridViewPathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			objectFileDataGridViewPathColumn.DataPropertyName = "Path";
			objectFileDataGridViewPathColumn.FillWeight = 60F;
			objectFileDataGridViewPathColumn.HeaderText = "Path";
			objectFileDataGridViewPathColumn.Name = "objectFileDataGridViewPathColumn";
			// 
			// objectFileDataGridViewSearchColumn
			// 
			objectFileDataGridViewSearchColumn.HeaderText = "";
			objectFileDataGridViewSearchColumn.Name = "objectFileDataGridViewSearchColumn";
			objectFileDataGridViewSearchColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			objectFileDataGridViewSearchColumn.Text = "Browse";
			objectFileDataGridViewSearchColumn.Width = 80;
			// 
			// objectFileDataGridViewShowColumn
			// 
			objectFileDataGridViewShowColumn.HeaderText = "Show";
			objectFileDataGridViewShowColumn.Name = "objectFileDataGridViewShowColumn";
			objectFileDataGridViewShowColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			objectFileDataGridViewShowColumn.Text = "◀";
			objectFileDataGridViewShowColumn.Width = 45;
			// 
			// objectFileDataGridViewMessageColumn
			// 
			objectFileDataGridViewMessageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			objectFileDataGridViewMessageColumn.DataPropertyName = "Message";
			objectFileDataGridViewMessageColumn.FillWeight = 40F;
			objectFileDataGridViewMessageColumn.HeaderText = "Message";
			objectFileDataGridViewMessageColumn.Name = "objectFileDataGridViewMessageColumn";
			objectFileDataGridViewMessageColumn.ReadOnly = true;
			objectFileDataGridViewMessageColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			// 
			// darkLabel5
			// 
			darkLabel5.AutoSize = true;
			darkLabel5.Dock = System.Windows.Forms.DockStyle.Top;
			darkLabel5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel5.Location = new System.Drawing.Point(3, 3);
			darkLabel5.Name = "darkLabel5";
			darkLabel5.Size = new System.Drawing.Size(412, 26);
			darkLabel5.TabIndex = 1;
			darkLabel5.Text = "List of object resources (eg *.wad2, *.wad).\r\nObjects inside the files mentioned earlier in the list take priority over later files.";
			// 
			// tabImportedGeometry
			// 
			tabImportedGeometry.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabImportedGeometry.Controls.Add(darkLabel11);
			tabImportedGeometry.Location = new System.Drawing.Point(4, 22);
			tabImportedGeometry.Name = "tabImportedGeometry";
			tabImportedGeometry.Padding = new System.Windows.Forms.Padding(3);
			tabImportedGeometry.Size = new System.Drawing.Size(778, 505);
			tabImportedGeometry.TabIndex = 3;
			tabImportedGeometry.Text = "Imported Geometry";
			// 
			// darkLabel11
			// 
			darkLabel11.AutoSize = true;
			darkLabel11.Dock = System.Windows.Forms.DockStyle.Top;
			darkLabel11.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel11.Location = new System.Drawing.Point(3, 3);
			darkLabel11.Name = "darkLabel11";
			darkLabel11.Size = new System.Drawing.Size(277, 13);
			darkLabel11.TabIndex = 1;
			darkLabel11.Text = "All imported geometries associated with this project:";
			// 
			// tabStaticMeshes
			// 
			tabStaticMeshes.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabStaticMeshes.Controls.Add(butSelectAllButShatterStatics);
			tabStaticMeshes.Controls.Add(butDeselectAllStatics);
			tabStaticMeshes.Controls.Add(butSelectAllStatics);
			tabStaticMeshes.Controls.Add(darkLabel19);
			tabStaticMeshes.Controls.Add(staticMeshMergeDataGridView);
			tabStaticMeshes.Controls.Add(darkLabel18);
			tabStaticMeshes.Controls.Add(darkLabel17);
			tabStaticMeshes.Location = new System.Drawing.Point(4, 22);
			tabStaticMeshes.Name = "tabStaticMeshes";
			tabStaticMeshes.Padding = new System.Windows.Forms.Padding(3);
			tabStaticMeshes.Size = new System.Drawing.Size(778, 505);
			tabStaticMeshes.TabIndex = 8;
			tabStaticMeshes.Text = "Static Meshes";
			// 
			// butSelectAllButShatterStatics
			// 
			butSelectAllButShatterStatics.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butSelectAllButShatterStatics.Checked = false;
			butSelectAllButShatterStatics.Location = new System.Drawing.Point(469, 408);
			butSelectAllButShatterStatics.Name = "butSelectAllButShatterStatics";
			butSelectAllButShatterStatics.Size = new System.Drawing.Size(134, 22);
			butSelectAllButShatterStatics.TabIndex = 111;
			butSelectAllButShatterStatics.Text = "Select all but shatters";
			butSelectAllButShatterStatics.Click += butSelectAllButShatterStatics_Click;
			// 
			// butDeselectAllStatics
			// 
			butDeselectAllStatics.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butDeselectAllStatics.Checked = false;
			butDeselectAllStatics.Location = new System.Drawing.Point(695, 408);
			butDeselectAllStatics.Name = "butDeselectAllStatics";
			butDeselectAllStatics.Size = new System.Drawing.Size(80, 22);
			butDeselectAllStatics.TabIndex = 110;
			butDeselectAllStatics.Text = "Deselect all";
			butDeselectAllStatics.Click += butDeselectAllStatics_Click;
			// 
			// butSelectAllStatics
			// 
			butSelectAllStatics.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butSelectAllStatics.Checked = false;
			butSelectAllStatics.Location = new System.Drawing.Point(609, 408);
			butSelectAllStatics.Name = "butSelectAllStatics";
			butSelectAllStatics.Size = new System.Drawing.Size(80, 22);
			butSelectAllStatics.TabIndex = 109;
			butSelectAllStatics.Text = "Select all";
			butSelectAllStatics.Click += butSelectAllStatics_Click;
			// 
			// darkLabel19
			// 
			darkLabel19.Dock = System.Windows.Forms.DockStyle.Bottom;
			darkLabel19.ForeColor = System.Drawing.Color.Silver;
			darkLabel19.Location = new System.Drawing.Point(3, 438);
			darkLabel19.Name = "darkLabel19";
			darkLabel19.Size = new System.Drawing.Size(772, 32);
			darkLabel19.TabIndex = 4;
			darkLabel19.Text = "If 'Interpret shades as effect' value is set, static mesh vertex shades are interpreted as follows:\r\n0 = No Effect, 1-14 = Glow, 15-31 = Movement/Glow.";
			// 
			// staticMeshMergeDataGridView
			// 
			staticMeshMergeDataGridView.AllowUserToAddRows = false;
			staticMeshMergeDataGridView.AllowUserToDeleteRows = false;
			staticMeshMergeDataGridView.AllowUserToDragDropRows = false;
			staticMeshMergeDataGridView.AllowUserToPasteCells = false;
			staticMeshMergeDataGridView.AllowUserToResizeColumns = false;
			staticMeshMergeDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			staticMeshMergeDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			staticMeshMergeDataGridView.ColumnHeadersHeight = 19;
			staticMeshMergeDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colMeshName, colMergeStatics, colInterpretShadesAsEffect, colTintAsAmbient, colClearShades });
			staticMeshMergeDataGridView.DisableSelection = true;
			staticMeshMergeDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(220, 220, 220);
			staticMeshMergeDataGridView.Location = new System.Drawing.Point(6, 19);
			staticMeshMergeDataGridView.MultiSelect = false;
			staticMeshMergeDataGridView.Name = "staticMeshMergeDataGridView";
			staticMeshMergeDataGridView.RowHeadersWidth = 41;
			staticMeshMergeDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			staticMeshMergeDataGridView.Size = new System.Drawing.Size(769, 383);
			staticMeshMergeDataGridView.TabIndex = 2;
			// 
			// colMeshName
			// 
			colMeshName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			colMeshName.DataPropertyName = "StaticMesh";
			colMeshName.FillWeight = 40F;
			colMeshName.HeaderText = "Mesh name";
			colMeshName.Name = "colMeshName";
			colMeshName.ReadOnly = true;
			// 
			// colMergeStatics
			// 
			colMergeStatics.DataPropertyName = "Merge";
			colMergeStatics.FillWeight = 20F;
			colMergeStatics.HeaderText = "Merge";
			colMergeStatics.Name = "colMergeStatics";
			colMergeStatics.ReadOnly = true;
			colMergeStatics.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			colMergeStatics.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			colMergeStatics.ToolTipText = "Whether to merge the Mesh";
			// 
			// colInterpretShadesAsEffect
			// 
			colInterpretShadesAsEffect.DataPropertyName = "InterpretShadesAsEffect";
			colInterpretShadesAsEffect.FillWeight = 50F;
			colInterpretShadesAsEffect.HeaderText = "Interpret shades as effect";
			colInterpretShadesAsEffect.Name = "colInterpretShadesAsEffect";
			colInterpretShadesAsEffect.ReadOnly = true;
			colInterpretShadesAsEffect.ToolTipText = "See description below";
			// 
			// colTintAsAmbient
			// 
			colTintAsAmbient.DataPropertyName = "TintAsAmbient";
			colTintAsAmbient.FillWeight = 60F;
			colTintAsAmbient.HeaderText = "Use tint as ambient color";
			colTintAsAmbient.Name = "colTintAsAmbient";
			colTintAsAmbient.ReadOnly = true;
			colTintAsAmbient.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			colTintAsAmbient.ToolTipText = "Uses the Static Mesh's Tint and uses it as the base for lighting";
			// 
			// colClearShades
			// 
			colClearShades.DataPropertyName = "ClearShades";
			colClearShades.FillWeight = 50F;
			colClearShades.HeaderText = "Clear vertex shades";
			colClearShades.Name = "colClearShades";
			colClearShades.ReadOnly = true;
			colClearShades.Resizable = System.Windows.Forms.DataGridViewTriState.True;
			colClearShades.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			colClearShades.ToolTipText = "The shades of the mesh are ignored for lighting calculation";
			// 
			// darkLabel18
			// 
			darkLabel18.Dock = System.Windows.Forms.DockStyle.Bottom;
			darkLabel18.ForeColor = System.Drawing.Color.Silver;
			darkLabel18.Location = new System.Drawing.Point(3, 470);
			darkLabel18.Name = "darkLabel18";
			darkLabel18.Size = new System.Drawing.Size(772, 32);
			darkLabel18.TabIndex = 1;
			darkLabel18.Text = resources.GetString("darkLabel18.Text");
			// 
			// darkLabel17
			// 
			darkLabel17.AutoSize = true;
			darkLabel17.Dock = System.Windows.Forms.DockStyle.Top;
			darkLabel17.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel17.Location = new System.Drawing.Point(3, 3);
			darkLabel17.Name = "darkLabel17";
			darkLabel17.Size = new System.Drawing.Size(388, 13);
			darkLabel17.TabIndex = 0;
			darkLabel17.Text = "Static meshes which should be automatically merged with room geometry:";
			// 
			// tabSkyAndFont
			// 
			tabSkyAndFont.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabSkyAndFont.Controls.Add(panelTr5Sprites);
			tabSkyAndFont.Controls.Add(panelFont);
			tabSkyAndFont.Controls.Add(panelSky);
			tabSkyAndFont.Location = new System.Drawing.Point(4, 22);
			tabSkyAndFont.Name = "tabSkyAndFont";
			tabSkyAndFont.Padding = new System.Windows.Forms.Padding(3);
			tabSkyAndFont.Size = new System.Drawing.Size(778, 505);
			tabSkyAndFont.TabIndex = 1;
			tabSkyAndFont.Text = "Sky & Font";
			// 
			// panelTr5Sprites
			// 
			panelTr5Sprites.Controls.Add(tr5SpritesTextureFilePathPicPreview);
			panelTr5Sprites.Controls.Add(tr5SpritesTextureFilePathBut);
			panelTr5Sprites.Controls.Add(tr5SpritesFilePathOptCustom);
			panelTr5Sprites.Controls.Add(lblTr5ExtraSprites);
			panelTr5Sprites.Controls.Add(tr5SpritesFilePathOptAuto);
			panelTr5Sprites.Controls.Add(tr5SpritesTextureFilePathTxt);
			panelTr5Sprites.Dock = System.Windows.Forms.DockStyle.Top;
			panelTr5Sprites.Location = new System.Drawing.Point(3, 142);
			panelTr5Sprites.Name = "panelTr5Sprites";
			panelTr5Sprites.Size = new System.Drawing.Size(772, 68);
			panelTr5Sprites.TabIndex = 3;
			// 
			// tr5SpritesTextureFilePathPicPreview
			// 
			tr5SpritesTextureFilePathPicPreview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			tr5SpritesTextureFilePathPicPreview.BackColor = System.Drawing.Color.Gray;
			tr5SpritesTextureFilePathPicPreview.BackgroundImage = (System.Drawing.Image)resources.GetObject("tr5SpritesTextureFilePathPicPreview.BackgroundImage");
			tr5SpritesTextureFilePathPicPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			tr5SpritesTextureFilePathPicPreview.Location = new System.Drawing.Point(639, 2);
			tr5SpritesTextureFilePathPicPreview.Name = "tr5SpritesTextureFilePathPicPreview";
			tr5SpritesTextureFilePathPicPreview.Size = new System.Drawing.Size(32, 32);
			tr5SpritesTextureFilePathPicPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			tr5SpritesTextureFilePathPicPreview.TabIndex = 6;
			tr5SpritesTextureFilePathPicPreview.TabStop = false;
			// 
			// tr5SpritesTextureFilePathBut
			// 
			tr5SpritesTextureFilePathBut.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			tr5SpritesTextureFilePathBut.Checked = false;
			tr5SpritesTextureFilePathBut.Location = new System.Drawing.Point(677, 40);
			tr5SpritesTextureFilePathBut.Name = "tr5SpritesTextureFilePathBut";
			tr5SpritesTextureFilePathBut.Size = new System.Drawing.Size(92, 22);
			tr5SpritesTextureFilePathBut.TabIndex = 3;
			tr5SpritesTextureFilePathBut.Text = "Browse";
			tr5SpritesTextureFilePathBut.Click += tr5SpritesTextureFilePathBut_Click;
			// 
			// tr5SpritesFilePathOptCustom
			// 
			tr5SpritesFilePathOptCustom.Location = new System.Drawing.Point(19, 42);
			tr5SpritesFilePathOptCustom.Name = "tr5SpritesFilePathOptCustom";
			tr5SpritesFilePathOptCustom.Size = new System.Drawing.Size(162, 17);
			tr5SpritesFilePathOptCustom.TabIndex = 5;
			tr5SpritesFilePathOptCustom.TabStop = true;
			tr5SpritesFilePathOptCustom.Text = "Custom file (has to be 256²)";
			// 
			// lblTr5ExtraSprites
			// 
			lblTr5ExtraSprites.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblTr5ExtraSprites.Location = new System.Drawing.Point(0, 0);
			lblTr5ExtraSprites.Name = "lblTr5ExtraSprites";
			lblTr5ExtraSprites.Size = new System.Drawing.Size(381, 17);
			lblTr5ExtraSprites.TabIndex = 1;
			lblTr5ExtraSprites.Text = "TR5 extra sprites texture";
			// 
			// tr5SpritesFilePathOptAuto
			// 
			tr5SpritesFilePathOptAuto.Checked = true;
			tr5SpritesFilePathOptAuto.Location = new System.Drawing.Point(19, 19);
			tr5SpritesFilePathOptAuto.Name = "tr5SpritesFilePathOptAuto";
			tr5SpritesFilePathOptAuto.Size = new System.Drawing.Size(450, 17);
			tr5SpritesFilePathOptAuto.TabIndex = 4;
			tr5SpritesFilePathOptAuto.TabStop = true;
			tr5SpritesFilePathOptAuto.Text = "Use default 'Extra.Tr5.pc' file";
			tr5SpritesFilePathOptAuto.CheckedChanged += tr5SpritesFilePathOptAuto_CheckedChanged;
			// 
			// tr5SpritesTextureFilePathTxt
			// 
			tr5SpritesTextureFilePathTxt.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			tr5SpritesTextureFilePathTxt.Enabled = false;
			tr5SpritesTextureFilePathTxt.Location = new System.Drawing.Point(187, 40);
			tr5SpritesTextureFilePathTxt.Name = "tr5SpritesTextureFilePathTxt";
			tr5SpritesTextureFilePathTxt.Size = new System.Drawing.Size(484, 22);
			tr5SpritesTextureFilePathTxt.TabIndex = 2;
			tr5SpritesTextureFilePathTxt.TextChanged += tr5SpritesTextureFilePathTxt_TextChanged;
			// 
			// panelFont
			// 
			panelFont.Controls.Add(fontTextureFilePathPicPreview);
			panelFont.Controls.Add(fontTextureFilePathBut);
			panelFont.Controls.Add(fontTextureFilePathOptCustom);
			panelFont.Controls.Add(darkLabel8);
			panelFont.Controls.Add(fontTextureFilePathOptAuto);
			panelFont.Controls.Add(fontTextureFilePathTxt);
			panelFont.Dock = System.Windows.Forms.DockStyle.Top;
			panelFont.Location = new System.Drawing.Point(3, 74);
			panelFont.Name = "panelFont";
			panelFont.Size = new System.Drawing.Size(772, 68);
			panelFont.TabIndex = 2;
			// 
			// fontTextureFilePathPicPreview
			// 
			fontTextureFilePathPicPreview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			fontTextureFilePathPicPreview.BackColor = System.Drawing.Color.Gray;
			fontTextureFilePathPicPreview.BackgroundImage = (System.Drawing.Image)resources.GetObject("fontTextureFilePathPicPreview.BackgroundImage");
			fontTextureFilePathPicPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			fontTextureFilePathPicPreview.Location = new System.Drawing.Point(639, 2);
			fontTextureFilePathPicPreview.Name = "fontTextureFilePathPicPreview";
			fontTextureFilePathPicPreview.Size = new System.Drawing.Size(32, 32);
			fontTextureFilePathPicPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			fontTextureFilePathPicPreview.TabIndex = 6;
			fontTextureFilePathPicPreview.TabStop = false;
			// 
			// fontTextureFilePathBut
			// 
			fontTextureFilePathBut.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			fontTextureFilePathBut.Checked = false;
			fontTextureFilePathBut.Location = new System.Drawing.Point(677, 40);
			fontTextureFilePathBut.Name = "fontTextureFilePathBut";
			fontTextureFilePathBut.Size = new System.Drawing.Size(92, 22);
			fontTextureFilePathBut.TabIndex = 3;
			fontTextureFilePathBut.Text = "Browse";
			fontTextureFilePathBut.Click += fontTextureFilePathBut_Click;
			// 
			// fontTextureFilePathOptCustom
			// 
			fontTextureFilePathOptCustom.Location = new System.Drawing.Point(19, 42);
			fontTextureFilePathOptCustom.Name = "fontTextureFilePathOptCustom";
			fontTextureFilePathOptCustom.Size = new System.Drawing.Size(162, 17);
			fontTextureFilePathOptCustom.TabIndex = 5;
			fontTextureFilePathOptCustom.TabStop = true;
			fontTextureFilePathOptCustom.Text = "Custom file (has to be 256²)";
			// 
			// darkLabel8
			// 
			darkLabel8.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel8.Location = new System.Drawing.Point(0, 0);
			darkLabel8.Name = "darkLabel8";
			darkLabel8.Size = new System.Drawing.Size(381, 17);
			darkLabel8.TabIndex = 1;
			darkLabel8.Text = "Font texture ('Font.pc' in the official editor):";
			// 
			// fontTextureFilePathOptAuto
			// 
			fontTextureFilePathOptAuto.Checked = true;
			fontTextureFilePathOptAuto.Location = new System.Drawing.Point(19, 19);
			fontTextureFilePathOptAuto.Name = "fontTextureFilePathOptAuto";
			fontTextureFilePathOptAuto.Size = new System.Drawing.Size(450, 17);
			fontTextureFilePathOptAuto.TabIndex = 4;
			fontTextureFilePathOptAuto.TabStop = true;
			fontTextureFilePathOptAuto.Text = "Use default 'Font.pc' file";
			fontTextureFilePathOptAuto.CheckedChanged += fontTextureFilePathOptAuto_CheckedChanged;
			// 
			// fontTextureFilePathTxt
			// 
			fontTextureFilePathTxt.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			fontTextureFilePathTxt.Enabled = false;
			fontTextureFilePathTxt.Location = new System.Drawing.Point(187, 40);
			fontTextureFilePathTxt.Name = "fontTextureFilePathTxt";
			fontTextureFilePathTxt.Size = new System.Drawing.Size(484, 22);
			fontTextureFilePathTxt.TabIndex = 2;
			fontTextureFilePathTxt.TextChanged += fontTextureFilePathTxt_TextChanged;
			// 
			// panelSky
			// 
			panelSky.Controls.Add(skyTextureFilePathPicPreview);
			panelSky.Controls.Add(skyTextureFilePathBut);
			panelSky.Controls.Add(skyTextureFilePathOptCustom);
			panelSky.Controls.Add(darkLabel9);
			panelSky.Controls.Add(skyTextureFilePathOptAuto);
			panelSky.Controls.Add(skyTextureFilePathTxt);
			panelSky.Dock = System.Windows.Forms.DockStyle.Top;
			panelSky.Location = new System.Drawing.Point(3, 3);
			panelSky.Name = "panelSky";
			panelSky.Size = new System.Drawing.Size(772, 71);
			panelSky.TabIndex = 2;
			// 
			// skyTextureFilePathPicPreview
			// 
			skyTextureFilePathPicPreview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			skyTextureFilePathPicPreview.BackColor = System.Drawing.Color.Gray;
			skyTextureFilePathPicPreview.BackgroundImage = (System.Drawing.Image)resources.GetObject("skyTextureFilePathPicPreview.BackgroundImage");
			skyTextureFilePathPicPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			skyTextureFilePathPicPreview.Location = new System.Drawing.Point(639, 2);
			skyTextureFilePathPicPreview.Name = "skyTextureFilePathPicPreview";
			skyTextureFilePathPicPreview.Size = new System.Drawing.Size(32, 32);
			skyTextureFilePathPicPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			skyTextureFilePathPicPreview.TabIndex = 6;
			skyTextureFilePathPicPreview.TabStop = false;
			// 
			// skyTextureFilePathBut
			// 
			skyTextureFilePathBut.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			skyTextureFilePathBut.Checked = false;
			skyTextureFilePathBut.Location = new System.Drawing.Point(677, 40);
			skyTextureFilePathBut.Name = "skyTextureFilePathBut";
			skyTextureFilePathBut.Size = new System.Drawing.Size(92, 22);
			skyTextureFilePathBut.TabIndex = 3;
			skyTextureFilePathBut.Text = "Browse";
			skyTextureFilePathBut.Click += skyTextureFilePathBut_Click;
			// 
			// skyTextureFilePathOptCustom
			// 
			skyTextureFilePathOptCustom.Location = new System.Drawing.Point(19, 42);
			skyTextureFilePathOptCustom.Name = "skyTextureFilePathOptCustom";
			skyTextureFilePathOptCustom.Size = new System.Drawing.Size(162, 17);
			skyTextureFilePathOptCustom.TabIndex = 5;
			skyTextureFilePathOptCustom.TabStop = true;
			skyTextureFilePathOptCustom.Text = "Custom file (has to be 256²)";
			// 
			// darkLabel9
			// 
			darkLabel9.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel9.Location = new System.Drawing.Point(0, 0);
			darkLabel9.Name = "darkLabel9";
			darkLabel9.Size = new System.Drawing.Size(381, 17);
			darkLabel9.TabIndex = 1;
			darkLabel9.Text = "Sky texture ('pcsky.raw' in the official editor):     ";
			// 
			// skyTextureFilePathOptAuto
			// 
			skyTextureFilePathOptAuto.Checked = true;
			skyTextureFilePathOptAuto.Location = new System.Drawing.Point(19, 19);
			skyTextureFilePathOptAuto.Name = "skyTextureFilePathOptAuto";
			skyTextureFilePathOptAuto.Size = new System.Drawing.Size(450, 17);
			skyTextureFilePathOptAuto.TabIndex = 4;
			skyTextureFilePathOptAuto.TabStop = true;
			skyTextureFilePathOptAuto.Text = "Use default 'pcsky.raw' file";
			skyTextureFilePathOptAuto.CheckedChanged += skyTextureFilePathOptAuto_CheckedChanged;
			// 
			// skyTextureFilePathTxt
			// 
			skyTextureFilePathTxt.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			skyTextureFilePathTxt.Enabled = false;
			skyTextureFilePathTxt.Location = new System.Drawing.Point(187, 40);
			skyTextureFilePathTxt.Name = "skyTextureFilePathTxt";
			skyTextureFilePathTxt.Size = new System.Drawing.Size(484, 22);
			skyTextureFilePathTxt.TabIndex = 2;
			skyTextureFilePathTxt.TextChanged += skyTextureFilePathTxt_TextChanged;
			// 
			// tabSoundsCatalogs
			// 
			tabSoundsCatalogs.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabSoundsCatalogs.Controls.Add(cbAutodetectIfNoneSelected);
			tabSoundsCatalogs.Controls.Add(butAssignFromWads);
			tabSoundsCatalogs.Controls.Add(butRemoveMissing);
			tabSoundsCatalogs.Controls.Add(darkLabel20);
			tabSoundsCatalogs.Controls.Add(labelSoundsCatalogsStatistics);
			tabSoundsCatalogs.Controls.Add(butSearchSounds);
			tabSoundsCatalogs.Controls.Add(butAssignFromSoundSources);
			tabSoundsCatalogs.Controls.Add(tbFilterSounds);
			tabSoundsCatalogs.Controls.Add(butAssignSoundsFromSelectedCatalogs);
			tabSoundsCatalogs.Controls.Add(butAssignHardcodedSounds);
			tabSoundsCatalogs.Controls.Add(butAutodetectSoundsAndAssign);
			tabSoundsCatalogs.Controls.Add(butDeselectAllSounds);
			tabSoundsCatalogs.Controls.Add(butSelectAllSounds);
			tabSoundsCatalogs.Controls.Add(lblCatalogsPrompt);
			tabSoundsCatalogs.Controls.Add(soundsCatalogsDataGridView);
			tabSoundsCatalogs.Controls.Add(soundsCatalogsDataGridViewControls);
			tabSoundsCatalogs.Controls.Add(darkLabel50);
			tabSoundsCatalogs.Controls.Add(selectedSoundsDataGridView);
			tabSoundsCatalogs.Location = new System.Drawing.Point(4, 22);
			tabSoundsCatalogs.Name = "tabSoundsCatalogs";
			tabSoundsCatalogs.Padding = new System.Windows.Forms.Padding(3);
			tabSoundsCatalogs.Size = new System.Drawing.Size(778, 505);
			tabSoundsCatalogs.TabIndex = 8;
			tabSoundsCatalogs.Text = "Sound Infos";
			// 
			// cbAutodetectIfNoneSelected
			// 
			cbAutodetectIfNoneSelected.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			cbAutodetectIfNoneSelected.AutoSize = true;
			cbAutodetectIfNoneSelected.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			cbAutodetectIfNoneSelected.Location = new System.Drawing.Point(486, 222);
			cbAutodetectIfNoneSelected.Name = "cbAutodetectIfNoneSelected";
			cbAutodetectIfNoneSelected.Size = new System.Drawing.Size(290, 17);
			cbAutodetectIfNoneSelected.TabIndex = 117;
			cbAutodetectIfNoneSelected.Text = "Autodetect sounds on compilation if none selected";
			cbAutodetectIfNoneSelected.CheckedChanged += cbAutodetectIfNoneSelected_CheckedChanged;
			// 
			// darkLabel20
			// 
			darkLabel20.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			darkLabel20.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel20.Location = new System.Drawing.Point(370, 482);
			darkLabel20.Name = "darkLabel20";
			darkLabel20.Size = new System.Drawing.Size(83, 19);
			darkLabel20.TabIndex = 110;
			darkLabel20.Text = "Filter by name:";
			// 
			// labelSoundsCatalogsStatistics
			// 
			labelSoundsCatalogsStatistics.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			labelSoundsCatalogsStatistics.AutoSize = true;
			labelSoundsCatalogsStatistics.ForeColor = System.Drawing.Color.DarkGray;
			labelSoundsCatalogsStatistics.Location = new System.Drawing.Point(6, 482);
			labelSoundsCatalogsStatistics.Name = "labelSoundsCatalogsStatistics";
			labelSoundsCatalogsStatistics.Size = new System.Drawing.Size(52, 13);
			labelSoundsCatalogsStatistics.TabIndex = 105;
			labelSoundsCatalogsStatistics.Text = "               ";
			// 
			// butSearchSounds
			// 
			butSearchSounds.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			butSearchSounds.Checked = false;
			butSearchSounds.Image = Properties.Resources.general_filter_16;
			butSearchSounds.Location = new System.Drawing.Point(654, 480);
			butSearchSounds.Name = "butSearchSounds";
			butSearchSounds.Selectable = false;
			butSearchSounds.Size = new System.Drawing.Size(24, 22);
			butSearchSounds.TabIndex = 109;
			butSearchSounds.Click += butFilterSounds_Click;
			// 
			// tbFilterSounds
			// 
			tbFilterSounds.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			tbFilterSounds.Location = new System.Drawing.Point(457, 480);
			tbFilterSounds.Name = "tbFilterSounds";
			tbFilterSounds.Size = new System.Drawing.Size(198, 22);
			tbFilterSounds.TabIndex = 100;
			tbFilterSounds.KeyDown += tbFilterSounds_KeyDown;
			// 
			// lblCatalogsPrompt
			// 
			lblCatalogsPrompt.AutoSize = true;
			lblCatalogsPrompt.Dock = System.Windows.Forms.DockStyle.Top;
			lblCatalogsPrompt.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblCatalogsPrompt.Location = new System.Drawing.Point(3, 3);
			lblCatalogsPrompt.Name = "lblCatalogsPrompt";
			lblCatalogsPrompt.Size = new System.Drawing.Size(462, 26);
			lblCatalogsPrompt.TabIndex = 106;
			lblCatalogsPrompt.Text = "Sound catalogs (eg *.xml, sounds.txt, *.sfx/*.sam) from which sound infos will be loaded.\r\nIf any sound info ID is duplicated in any of catalog, first one will be used.";
			// 
			// soundsCatalogsDataGridView
			// 
			soundsCatalogsDataGridView.AllowUserToAddRows = false;
			soundsCatalogsDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			soundsCatalogsDataGridView.AutoGenerateColumns = false;
			soundsCatalogsDataGridView.ColumnHeadersHeight = 19;
			soundsCatalogsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { SoundsCatalogPathColumn, SoundsCatalogSearchColumn, SoundsCatalogReloadButton, SoundsCatalogEditColumn, SoundsCatalogsAssignColumn, SoundsCatalogsSoundCountColumn, SoundsCatalogMessageColumn });
			soundsCatalogsDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(220, 220, 220);
			soundsCatalogsDataGridView.Location = new System.Drawing.Point(6, 32);
			soundsCatalogsDataGridView.Name = "soundsCatalogsDataGridView";
			soundsCatalogsDataGridView.RowHeadersWidth = 41;
			soundsCatalogsDataGridView.Size = new System.Drawing.Size(739, 180);
			soundsCatalogsDataGridView.TabIndex = 104;
			soundsCatalogsDataGridView.CellContentClick += soundsCatalogsDataGridView_CellContentClick;
			soundsCatalogsDataGridView.CellFormatting += soundsCatalogsDataGridView_CellFormatting;
			soundsCatalogsDataGridView.CellPainting += soundsCatalogsDataGridView_CellPainting;
			soundsCatalogsDataGridView.RowsRemoved += soundsCatalogsDataGridView_RowsRemoved;
			soundsCatalogsDataGridView.Sorted += soundsCatalogsDataGridView_Sorted;
			// 
			// SoundsCatalogPathColumn
			// 
			SoundsCatalogPathColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			SoundsCatalogPathColumn.DataPropertyName = "Path";
			SoundsCatalogPathColumn.FillWeight = 60F;
			SoundsCatalogPathColumn.HeaderText = "Path";
			SoundsCatalogPathColumn.Name = "SoundsCatalogPathColumn";
			SoundsCatalogPathColumn.ReadOnly = true;
			// 
			// SoundsCatalogSearchColumn
			// 
			SoundsCatalogSearchColumn.HeaderText = "";
			SoundsCatalogSearchColumn.Name = "SoundsCatalogSearchColumn";
			SoundsCatalogSearchColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			SoundsCatalogSearchColumn.Text = "";
			SoundsCatalogSearchColumn.ToolTipText = "Browse";
			SoundsCatalogSearchColumn.Width = 24;
			// 
			// SoundsCatalogReloadButton
			// 
			SoundsCatalogReloadButton.HeaderText = "";
			SoundsCatalogReloadButton.Name = "SoundsCatalogReloadButton";
			SoundsCatalogReloadButton.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			SoundsCatalogReloadButton.Text = "";
			SoundsCatalogReloadButton.ToolTipText = "Reload this catalog from disk";
			SoundsCatalogReloadButton.Width = 24;
			// 
			// SoundsCatalogEditColumn
			// 
			SoundsCatalogEditColumn.HeaderText = "";
			SoundsCatalogEditColumn.Name = "SoundsCatalogEditColumn";
			SoundsCatalogEditColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			SoundsCatalogEditColumn.Text = "";
			SoundsCatalogEditColumn.ToolTipText = "Edit in SoundTool";
			SoundsCatalogEditColumn.Width = 24;
			// 
			// SoundsCatalogsAssignColumn
			// 
			SoundsCatalogsAssignColumn.HeaderText = "";
			SoundsCatalogsAssignColumn.Name = "SoundsCatalogsAssignColumn";
			SoundsCatalogsAssignColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			SoundsCatalogsAssignColumn.Text = "Include all";
			SoundsCatalogsAssignColumn.ToolTipText = "Include all sounds from this catalog";
			SoundsCatalogsAssignColumn.Width = 60;
			// 
			// SoundsCatalogsSoundCountColumn
			// 
			SoundsCatalogsSoundCountColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			SoundsCatalogsSoundCountColumn.DataPropertyName = "SoundsCount";
			SoundsCatalogsSoundCountColumn.FillWeight = 20F;
			SoundsCatalogsSoundCountColumn.HeaderText = "Num. sounds";
			SoundsCatalogsSoundCountColumn.Name = "SoundsCatalogsSoundCountColumn";
			SoundsCatalogsSoundCountColumn.ReadOnly = true;
			SoundsCatalogsSoundCountColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			// 
			// SoundsCatalogMessageColumn
			// 
			SoundsCatalogMessageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			SoundsCatalogMessageColumn.DataPropertyName = "Message";
			SoundsCatalogMessageColumn.FillWeight = 40F;
			SoundsCatalogMessageColumn.HeaderText = "Message";
			SoundsCatalogMessageColumn.Name = "SoundsCatalogMessageColumn";
			SoundsCatalogMessageColumn.ReadOnly = true;
			SoundsCatalogMessageColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			// 
			// soundsCatalogsDataGridViewControls
			// 
			soundsCatalogsDataGridViewControls.AllowUserDelete = false;
			soundsCatalogsDataGridViewControls.AllowUserMove = false;
			soundsCatalogsDataGridViewControls.AllowUserNew = false;
			soundsCatalogsDataGridViewControls.AlwaysInsertAtZero = false;
			soundsCatalogsDataGridViewControls.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			soundsCatalogsDataGridViewControls.Enabled = false;
			soundsCatalogsDataGridViewControls.Location = new System.Drawing.Point(751, 32);
			soundsCatalogsDataGridViewControls.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			soundsCatalogsDataGridViewControls.MinimumSize = new System.Drawing.Size(24, 100);
			soundsCatalogsDataGridViewControls.Name = "soundsCatalogsDataGridViewControls";
			soundsCatalogsDataGridViewControls.Size = new System.Drawing.Size(24, 180);
			soundsCatalogsDataGridViewControls.TabIndex = 103;
			// 
			// darkLabel50
			// 
			darkLabel50.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel50.Location = new System.Drawing.Point(3, 223);
			darkLabel50.Name = "darkLabel50";
			darkLabel50.Size = new System.Drawing.Size(196, 19);
			darkLabel50.TabIndex = 97;
			darkLabel50.Text = "Sound infos to include in level:";
			// 
			// selectedSoundsDataGridView
			// 
			selectedSoundsDataGridView.AllowUserToAddRows = false;
			selectedSoundsDataGridView.AllowUserToDeleteRows = false;
			selectedSoundsDataGridView.AllowUserToDragDropRows = false;
			selectedSoundsDataGridView.AllowUserToOrderColumns = true;
			selectedSoundsDataGridView.AllowUserToPasteCells = false;
			selectedSoundsDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			selectedSoundsDataGridView.ColumnHeadersHeight = 19;
			selectedSoundsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { colSoundsEnabled, colSoundID, dataGridViewTextBoxColumn2, dataGridViewTextBoxColumn3, dataGridViewTextBoxColumn6, dataGridViewTextBoxColumn4, dataGridViewTextBoxColumn5 });
			selectedSoundsDataGridView.DisableSelection = true;
			selectedSoundsDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(220, 220, 220);
			selectedSoundsDataGridView.Location = new System.Drawing.Point(6, 273);
			selectedSoundsDataGridView.MultiSelect = false;
			selectedSoundsDataGridView.Name = "selectedSoundsDataGridView";
			selectedSoundsDataGridView.RowHeadersWidth = 41;
			selectedSoundsDataGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			selectedSoundsDataGridView.ShowCellErrors = false;
			selectedSoundsDataGridView.ShowCellToolTips = false;
			selectedSoundsDataGridView.ShowEditingIcon = false;
			selectedSoundsDataGridView.ShowRowErrors = false;
			selectedSoundsDataGridView.Size = new System.Drawing.Size(769, 201);
			selectedSoundsDataGridView.TabIndex = 116;
			selectedSoundsDataGridView.ToggleCheckBoxOnClick = true;
			selectedSoundsDataGridView.CellValueChanged += selectedSoundsDataGridView_CellValueChanged;
			// 
			// colSoundsEnabled
			// 
			colSoundsEnabled.HeaderText = "";
			colSoundsEnabled.Name = "colSoundsEnabled";
			colSoundsEnabled.ReadOnly = true;
			colSoundsEnabled.Resizable = System.Windows.Forms.DataGridViewTriState.False;
			colSoundsEnabled.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			colSoundsEnabled.Width = 40;
			// 
			// colSoundID
			// 
			colSoundID.HeaderText = "ID";
			colSoundID.Name = "colSoundID";
			colSoundID.ReadOnly = true;
			colSoundID.Width = 40;
			// 
			// dataGridViewTextBoxColumn2
			// 
			dataGridViewTextBoxColumn2.HeaderText = "Name";
			dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
			dataGridViewTextBoxColumn2.ReadOnly = true;
			dataGridViewTextBoxColumn2.Width = 180;
			// 
			// dataGridViewTextBoxColumn3
			// 
			dataGridViewTextBoxColumn3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			dataGridViewTextBoxColumn3.HeaderText = "From catalog";
			dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
			dataGridViewTextBoxColumn3.ReadOnly = true;
			// 
			// dataGridViewTextBoxColumn6
			// 
			dataGridViewTextBoxColumn6.HeaderText = "Samples";
			dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
			dataGridViewTextBoxColumn6.ReadOnly = true;
			dataGridViewTextBoxColumn6.Width = 70;
			// 
			// dataGridViewTextBoxColumn4
			// 
			dataGridViewTextBoxColumn4.HeaderText = "Range";
			dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
			dataGridViewTextBoxColumn4.ReadOnly = true;
			dataGridViewTextBoxColumn4.ToolTipText = "Range name of TRNG extended soundmap";
			dataGridViewTextBoxColumn4.Width = 80;
			// 
			// dataGridViewTextBoxColumn5
			// 
			dataGridViewTextBoxColumn5.HeaderText = "Orig. ID";
			dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
			dataGridViewTextBoxColumn5.ReadOnly = true;
			dataGridViewTextBoxColumn5.ToolTipText = "Original sound ID derived from TRNG extended soundmap";
			dataGridViewTextBoxColumn5.Width = 80;
			// 
			// tabSamples
			// 
			tabSamples.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabSamples.Controls.Add(soundDataGridViewControls);
			tabSamples.Controls.Add(soundDataGridView);
			tabSamples.Controls.Add(lblPathsPrompt);
			tabSamples.Location = new System.Drawing.Point(4, 22);
			tabSamples.Name = "tabSamples";
			tabSamples.Padding = new System.Windows.Forms.Padding(3);
			tabSamples.Size = new System.Drawing.Size(778, 505);
			tabSamples.TabIndex = 2;
			tabSamples.Text = "Sound Sample Paths";
			// 
			// soundDataGridViewControls
			// 
			soundDataGridViewControls.AllowUserDelete = false;
			soundDataGridViewControls.AllowUserMove = false;
			soundDataGridViewControls.AllowUserNew = false;
			soundDataGridViewControls.AlwaysInsertAtZero = false;
			soundDataGridViewControls.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
			soundDataGridViewControls.Enabled = false;
			soundDataGridViewControls.Location = new System.Drawing.Point(751, 32);
			soundDataGridViewControls.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			soundDataGridViewControls.MinimumSize = new System.Drawing.Size(24, 100);
			soundDataGridViewControls.Name = "soundDataGridViewControls";
			soundDataGridViewControls.Size = new System.Drawing.Size(24, 467);
			soundDataGridViewControls.TabIndex = 3;
			// 
			// soundDataGridView
			// 
			soundDataGridView.AllowUserToAddRows = false;
			soundDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			soundDataGridView.AutoGenerateColumns = false;
			soundDataGridView.ColumnHeadersHeight = 19;
			soundDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { soundDataGridViewColumnPath, soundDataGridViewColumnSearch });
			soundDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(220, 220, 220);
			soundDataGridView.Location = new System.Drawing.Point(6, 32);
			soundDataGridView.Name = "soundDataGridView";
			soundDataGridView.RowHeadersWidth = 41;
			soundDataGridView.Size = new System.Drawing.Size(739, 467);
			soundDataGridView.TabIndex = 2;
			soundDataGridView.CellContentClick += soundDataGridView_CellContentClick;
			soundDataGridView.CellFormatting += soundDataGridView_CellFormatting;
			// 
			// soundDataGridViewColumnPath
			// 
			soundDataGridViewColumnPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			soundDataGridViewColumnPath.DataPropertyName = "Path";
			soundDataGridViewColumnPath.HeaderText = "Path";
			soundDataGridViewColumnPath.Name = "soundDataGridViewColumnPath";
			soundDataGridViewColumnPath.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
			// 
			// soundDataGridViewColumnSearch
			// 
			soundDataGridViewColumnSearch.HeaderText = "";
			soundDataGridViewColumnSearch.Name = "soundDataGridViewColumnSearch";
			soundDataGridViewColumnSearch.Text = "Browse";
			// 
			// lblPathsPrompt
			// 
			lblPathsPrompt.AutoSize = true;
			lblPathsPrompt.Dock = System.Windows.Forms.DockStyle.Top;
			lblPathsPrompt.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblPathsPrompt.Location = new System.Drawing.Point(3, 3);
			lblPathsPrompt.Name = "lblPathsPrompt";
			lblPathsPrompt.Size = new System.Drawing.Size(555, 26);
			lblPathsPrompt.TabIndex = 1;
			lblPathsPrompt.Text = "Locations from which sound samples will be loaded.\r\nEach required sample will be searched in folders in top to bottom order. If not found, sound is not played.";
			// 
			// tabMisc
			// 
			tabMisc.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabMisc.Controls.Add(panelTr5Weather);
			tabMisc.Controls.Add(panelTr5LaraType);
			tabMisc.Controls.Add(panel6);
			tabMisc.Controls.Add(panel12);
			tabMisc.ForeColor = System.Drawing.SystemColors.ControlText;
			tabMisc.Location = new System.Drawing.Point(4, 22);
			tabMisc.Name = "tabMisc";
			tabMisc.Size = new System.Drawing.Size(778, 505);
			tabMisc.TabIndex = 6;
			tabMisc.Text = "Misc";
			// 
			// panelTr5Weather
			// 
			panelTr5Weather.Controls.Add(comboTr5Weather);
			panelTr5Weather.Controls.Add(lblTr5Weather);
			panelTr5Weather.Dock = System.Windows.Forms.DockStyle.Top;
			panelTr5Weather.Location = new System.Drawing.Point(0, 415);
			panelTr5Weather.Name = "panelTr5Weather";
			panelTr5Weather.Size = new System.Drawing.Size(778, 51);
			panelTr5Weather.TabIndex = 97;
			// 
			// comboTr5Weather
			// 
			comboTr5Weather.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			comboTr5Weather.FormattingEnabled = true;
			comboTr5Weather.Location = new System.Drawing.Point(19, 23);
			comboTr5Weather.Name = "comboTr5Weather";
			comboTr5Weather.Size = new System.Drawing.Size(658, 23);
			comboTr5Weather.TabIndex = 4;
			comboTr5Weather.SelectedIndexChanged += comboTr5Weather_SelectedIndexChanged;
			// 
			// lblTr5Weather
			// 
			lblTr5Weather.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblTr5Weather.Location = new System.Drawing.Point(0, 3);
			lblTr5Weather.Name = "lblTr5Weather";
			lblTr5Weather.Size = new System.Drawing.Size(439, 17);
			lblTr5Weather.TabIndex = 3;
			lblTr5Weather.Text = "TR5 weather:";
			// 
			// panelTr5LaraType
			// 
			panelTr5LaraType.Controls.Add(comboLaraType);
			panelTr5LaraType.Controls.Add(lblLaraType);
			panelTr5LaraType.Dock = System.Windows.Forms.DockStyle.Top;
			panelTr5LaraType.Location = new System.Drawing.Point(0, 364);
			panelTr5LaraType.Name = "panelTr5LaraType";
			panelTr5LaraType.Size = new System.Drawing.Size(778, 51);
			panelTr5LaraType.TabIndex = 96;
			// 
			// comboLaraType
			// 
			comboLaraType.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			comboLaraType.FormattingEnabled = true;
			comboLaraType.Location = new System.Drawing.Point(19, 23);
			comboLaraType.Name = "comboLaraType";
			comboLaraType.Size = new System.Drawing.Size(658, 23);
			comboLaraType.TabIndex = 4;
			comboLaraType.SelectedIndexChanged += comboLaraType_SelectedIndexChanged;
			// 
			// lblLaraType
			// 
			lblLaraType.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			lblLaraType.Location = new System.Drawing.Point(0, 3);
			lblLaraType.Name = "lblLaraType";
			lblLaraType.Size = new System.Drawing.Size(439, 17);
			lblLaraType.TabIndex = 3;
			lblLaraType.Text = "TR5 Lara type:";
			// 
			// panel6
			// 
			panel6.Controls.Add(levelFilePathBut);
			panel6.Controls.Add(darkLabel6);
			panel6.Controls.Add(levelFilePathTxt);
			panel6.Dock = System.Windows.Forms.DockStyle.Top;
			panel6.Location = new System.Drawing.Point(0, 312);
			panel6.Name = "panel6";
			panel6.Size = new System.Drawing.Size(778, 52);
			panel6.TabIndex = 94;
			// 
			// levelFilePathBut
			// 
			levelFilePathBut.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			levelFilePathBut.Checked = false;
			levelFilePathBut.Location = new System.Drawing.Point(683, 20);
			levelFilePathBut.Name = "levelFilePathBut";
			levelFilePathBut.Size = new System.Drawing.Size(92, 22);
			levelFilePathBut.TabIndex = 3;
			levelFilePathBut.Text = "Browse";
			levelFilePathBut.Click += levelFilePathBut_Click;
			// 
			// darkLabel6
			// 
			darkLabel6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel6.Location = new System.Drawing.Point(0, 0);
			darkLabel6.Name = "darkLabel6";
			darkLabel6.Size = new System.Drawing.Size(384, 17);
			darkLabel6.TabIndex = 1;
			darkLabel6.Text = "Full file path for the currently open level:";
			// 
			// levelFilePathTxt
			// 
			levelFilePathTxt.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			levelFilePathTxt.Location = new System.Drawing.Point(19, 20);
			levelFilePathTxt.Name = "levelFilePathTxt";
			levelFilePathTxt.Size = new System.Drawing.Size(658, 22);
			levelFilePathTxt.TabIndex = 2;
			levelFilePathTxt.TextChanged += levelFilePathTxt_TextChanged;
			// 
			// panel12
			// 
			panel12.Controls.Add(cbCompressTextures);
			panel12.Controls.Add(cbUse32BitLighting);
			panel12.Controls.Add(cmbSampleRate);
			panel12.Controls.Add(cbKeepSampleRate);
			panel12.Controls.Add(cbRemoveObjects);
			panel12.Controls.Add(cbRearrangeRooms);
			panel12.Controls.Add(cbRemapAnimTextures);
			panel12.Controls.Add(cbDither16BitTextures);
			panel12.Controls.Add(cbOverrideAllLightQuality);
			panel12.Controls.Add(cmbDefaultLightQuality);
			panel12.Controls.Add(darkLabel22);
			panel12.Controls.Add(cbAgressiveFloordataPacking);
			panel12.Controls.Add(cbAgressiveTexturePacking);
			panel12.Controls.Add(darkLabel13);
			panel12.Controls.Add(darkLabel16);
			panel12.Controls.Add(numPadding);
			panel12.Controls.Add(panelRoomAmbientLight);
			panel12.Controls.Add(darkLabel12);
			panel12.Dock = System.Windows.Forms.DockStyle.Top;
			panel12.Location = new System.Drawing.Point(0, 0);
			panel12.Name = "panel12";
			panel12.Size = new System.Drawing.Size(778, 312);
			panel12.TabIndex = 91;
			// 
			// cmbSampleRate
			// 
			cmbSampleRate.FormattingEnabled = true;
			cmbSampleRate.Items.AddRange(new object[] { "11025", "22050", "44100", "48000" });
			cmbSampleRate.Location = new System.Drawing.Point(215, 279);
			cmbSampleRate.Name = "cmbSampleRate";
			cmbSampleRate.Size = new System.Drawing.Size(81, 23);
			cmbSampleRate.TabIndex = 115;
			cmbSampleRate.SelectedIndexChanged += cmbSampleRate_SelectedIndexChanged;
			// 
			// darkLabel22
			// 
			darkLabel22.AutoSize = true;
			darkLabel22.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			darkLabel22.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel22.Location = new System.Drawing.Point(0, 43);
			darkLabel22.Name = "darkLabel22";
			darkLabel22.Size = new System.Drawing.Size(113, 13);
			darkLabel22.TabIndex = 106;
			darkLabel22.Text = "Default light quality:";
			// 
			// darkLabel13
			// 
			darkLabel13.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel13.Location = new System.Drawing.Point(213, 71);
			darkLabel13.Name = "darkLabel13";
			darkLabel13.Size = new System.Drawing.Size(45, 17);
			darkLabel13.TabIndex = 103;
			darkLabel13.Text = "pixels";
			// 
			// darkLabel16
			// 
			darkLabel16.AutoSize = true;
			darkLabel16.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			darkLabel16.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel16.Location = new System.Drawing.Point(0, 15);
			darkLabel16.Name = "darkLabel16";
			darkLabel16.Size = new System.Drawing.Size(120, 13);
			darkLabel16.TabIndex = 90;
			darkLabel16.Text = "Default ambient light:";
			// 
			// panelRoomAmbientLight
			// 
			panelRoomAmbientLight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			panelRoomAmbientLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
			panelRoomAmbientLight.Location = new System.Drawing.Point(129, 10);
			panelRoomAmbientLight.Name = "panelRoomAmbientLight";
			panelRoomAmbientLight.Size = new System.Drawing.Size(81, 24);
			panelRoomAmbientLight.TabIndex = 89;
			panelRoomAmbientLight.Click += panelRoomAmbientLight_Click;
			// 
			// darkLabel12
			// 
			darkLabel12.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel12.Location = new System.Drawing.Point(0, 71);
			darkLabel12.Name = "darkLabel12";
			darkLabel12.Size = new System.Drawing.Size(123, 17);
			darkLabel12.TabIndex = 101;
			darkLabel12.Text = "Texture tile padding:";
			// 
			// tabPaths
			// 
			tabPaths.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
			tabPaths.Controls.Add(darkLabel1);
			tabPaths.Controls.Add(pathVariablesDataGridView);
			tabPaths.Location = new System.Drawing.Point(4, 22);
			tabPaths.Name = "tabPaths";
			tabPaths.Padding = new System.Windows.Forms.Padding(3);
			tabPaths.Size = new System.Drawing.Size(778, 505);
			tabPaths.TabIndex = 5;
			tabPaths.Text = "Path Placeholders";
			// 
			// darkLabel1
			// 
			darkLabel1.Dock = System.Windows.Forms.DockStyle.Top;
			darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
			darkLabel1.Location = new System.Drawing.Point(3, 3);
			darkLabel1.Name = "darkLabel1";
			darkLabel1.Size = new System.Drawing.Size(772, 12);
			darkLabel1.TabIndex = 1;
			darkLabel1.Text = "Available dynamic place holders that can be used inside paths: ";
			// 
			// pathVariablesDataGridView
			// 
			pathVariablesDataGridView.AllowUserToAddRows = false;
			pathVariablesDataGridView.AllowUserToDeleteRows = false;
			pathVariablesDataGridView.AllowUserToDragDropRows = false;
			pathVariablesDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
			pathVariablesDataGridView.ColumnHeadersHeight = 19;
			pathVariablesDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { pathVariablesDataGridViewNameColumn, pathVariablesDataGridViewValueColumn });
			pathVariablesDataGridView.ForegroundColor = System.Drawing.Color.FromArgb(220, 220, 220);
			pathVariablesDataGridView.Location = new System.Drawing.Point(6, 19);
			pathVariablesDataGridView.Name = "pathVariablesDataGridView";
			pathVariablesDataGridView.RowHeadersWidth = 41;
			pathVariablesDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
			pathVariablesDataGridView.Size = new System.Drawing.Size(769, 480);
			pathVariablesDataGridView.TabIndex = 2;
			pathVariablesDataGridView.CellMouseDown += pathVariablesDataGridView_CellMouseDown;
			// 
			// pathVariablesDataGridViewNameColumn
			// 
			pathVariablesDataGridViewNameColumn.ContextMenuStrip = pathVariablesDataGridViewContextMenu;
			pathVariablesDataGridViewNameColumn.HeaderText = "Placeholder";
			pathVariablesDataGridViewNameColumn.MinimumWidth = 50;
			pathVariablesDataGridViewNameColumn.Name = "pathVariablesDataGridViewNameColumn";
			pathVariablesDataGridViewNameColumn.ReadOnly = true;
			pathVariablesDataGridViewNameColumn.Width = 120;
			// 
			// pathVariablesDataGridViewValueColumn
			// 
			pathVariablesDataGridViewValueColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			pathVariablesDataGridViewValueColumn.ContextMenuStrip = pathVariablesDataGridViewContextMenu;
			pathVariablesDataGridViewValueColumn.HeaderText = "Current Value";
			pathVariablesDataGridViewValueColumn.Name = "pathVariablesDataGridViewValueColumn";
			pathVariablesDataGridViewValueColumn.ReadOnly = true;
			// 
			// colSoundsId
			// 
			colSoundsId.HeaderText = "ID";
			colSoundsId.Name = "colSoundsId";
			colSoundsId.ReadOnly = true;
			colSoundsId.Width = 40;
			// 
			// colSoundsName
			// 
			colSoundsName.HeaderText = "Name";
			colSoundsName.Name = "colSoundsName";
			colSoundsName.Width = 200;
			// 
			// SelectedSoundsCatalogColumn
			// 
			SelectedSoundsCatalogColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			SelectedSoundsCatalogColumn.HeaderText = "From catalog";
			SelectedSoundsCatalogColumn.Name = "SelectedSoundsCatalogColumn";
			SelectedSoundsCatalogColumn.ReadOnly = true;
			// 
			// SelectedSoundsGameColumn
			// 
			SelectedSoundsGameColumn.HeaderText = "Range";
			SelectedSoundsGameColumn.Name = "SelectedSoundsGameColumn";
			SelectedSoundsGameColumn.ReadOnly = true;
			SelectedSoundsGameColumn.ToolTipText = "Range name of TRNG extended soundmap";
			SelectedSoundsGameColumn.Width = 80;
			// 
			// SelectedSoundsOriginalIdColumn
			// 
			SelectedSoundsOriginalIdColumn.HeaderText = "Orig. ID";
			SelectedSoundsOriginalIdColumn.Name = "SelectedSoundsOriginalIdColumn";
			SelectedSoundsOriginalIdColumn.ReadOnly = true;
			SelectedSoundsOriginalIdColumn.ToolTipText = "Original sound ID derived from TRNG extended soundmap";
			SelectedSoundsOriginalIdColumn.Width = 80;
			// 
			// FormLevelSettings
			// 
			AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			CancelButton = butCancel;
			ClientSize = new System.Drawing.Size(1001, 573);
			Controls.Add(butApply);
			Controls.Add(butOk);
			Controls.Add(darkSectionPanel1);
			Controls.Add(butCancel);
			Controls.Add(optionsList);
			MinimizeBox = false;
			Name = "FormLevelSettings";
			ShowIcon = false;
			ShowInTaskbar = false;
			SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			Text = "Level Settings";
			pathVariablesDataGridViewContextMenu.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)numPadding).EndInit();
			darkSectionPanel1.ResumeLayout(false);
			tabbedContainer.ResumeLayout(false);
			tabGame.ResumeLayout(false);
			panel3.ResumeLayout(false);
			panel3.PerformLayout();
			panelLuaPath.ResumeLayout(false);
			panelLuaPath.PerformLayout();
			panelScripts.ResumeLayout(false);
			panelScripts.PerformLayout();
			panel1.ResumeLayout(false);
			panel1.PerformLayout();
			panel2.ResumeLayout(false);
			panel2.PerformLayout();
			panel7.ResumeLayout(false);
			tabTextures.ResumeLayout(false);
			tabTextures.PerformLayout();
			((System.ComponentModel.ISupportInitialize)textureFileDataGridView).EndInit();
			tabObjects.ResumeLayout(false);
			tabObjects.PerformLayout();
			((System.ComponentModel.ISupportInitialize)objectFileDataGridView).EndInit();
			tabImportedGeometry.ResumeLayout(false);
			tabImportedGeometry.PerformLayout();
			tabStaticMeshes.ResumeLayout(false);
			tabStaticMeshes.PerformLayout();
			((System.ComponentModel.ISupportInitialize)staticMeshMergeDataGridView).EndInit();
			tabSkyAndFont.ResumeLayout(false);
			panelTr5Sprites.ResumeLayout(false);
			panelTr5Sprites.PerformLayout();
			((System.ComponentModel.ISupportInitialize)tr5SpritesTextureFilePathPicPreview).EndInit();
			panelFont.ResumeLayout(false);
			panelFont.PerformLayout();
			((System.ComponentModel.ISupportInitialize)fontTextureFilePathPicPreview).EndInit();
			panelSky.ResumeLayout(false);
			panelSky.PerformLayout();
			((System.ComponentModel.ISupportInitialize)skyTextureFilePathPicPreview).EndInit();
			tabSoundsCatalogs.ResumeLayout(false);
			tabSoundsCatalogs.PerformLayout();
			((System.ComponentModel.ISupportInitialize)soundsCatalogsDataGridView).EndInit();
			((System.ComponentModel.ISupportInitialize)selectedSoundsDataGridView).EndInit();
			tabSamples.ResumeLayout(false);
			tabSamples.PerformLayout();
			((System.ComponentModel.ISupportInitialize)soundDataGridView).EndInit();
			tabMisc.ResumeLayout(false);
			panelTr5Weather.ResumeLayout(false);
			panelTr5LaraType.ResumeLayout(false);
			panel6.ResumeLayout(false);
			panel6.PerformLayout();
			panel12.ResumeLayout(false);
			panel12.PerformLayout();
			tabPaths.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)pathVariablesDataGridView).EndInit();
			ResumeLayout(false);
		}

		#endregion
		private System.Windows.Forms.Panel panel2;
        private DarkUI.Controls.DarkButton gameLevelFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTextBox gameLevelFilePathTxt;
        private System.Windows.Forms.Panel panel3;
        private DarkUI.Controls.DarkButton gameExecutableFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkTextBox gameExecutableFilePathTxt;
        private DarkUI.Controls.DarkButton fontTextureFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private System.Windows.Forms.Panel panel7;
        private DarkUI.Controls.DarkButton gameDirectoryBut;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkTextBox gameDirectoryTxt;
        private System.Windows.Forms.Panel panelFont;
        private DarkUI.Controls.DarkRadioButton fontTextureFilePathOptCustom;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkRadioButton fontTextureFilePathOptAuto;
        private DarkUI.Controls.DarkTextBox fontTextureFilePathTxt;
        private System.Windows.Forms.Panel panelSky;
        private DarkUI.Controls.DarkButton skyTextureFilePathBut;
        private DarkUI.Controls.DarkRadioButton skyTextureFilePathOptCustom;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkRadioButton skyTextureFilePathOptAuto;
        private DarkUI.Controls.DarkTextBox skyTextureFilePathTxt;
        private System.Windows.Forms.PictureBox fontTextureFilePathPicPreview;
        private System.Windows.Forms.PictureBox skyTextureFilePathPicPreview;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkContextMenu pathVariablesDataGridViewContextMenu;
        private System.Windows.Forms.ToolStripMenuItem pathVariablesDataGridViewContextMenuCopy;
        private System.Windows.Forms.ToolTip pathToolTip;
        private DarkUI.Controls.DarkDataGridView pathVariablesDataGridView;
        private DarkUI.Controls.DarkCheckBox GameEnableQuickStartFeatureCheckBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathVariablesDataGridViewNameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn pathVariablesDataGridViewValueColumn;
        private DarkUI.Controls.DarkLabel darkLabel11;
        private Controls.ImportedGeometryManager importedGeometryManager;
        private TombLib.Controls.DarkTabbedContainer tabbedContainer;
        private System.Windows.Forms.TabPage tabObjects;
        private System.Windows.Forms.TabPage tabSkyAndFont;
        private System.Windows.Forms.TabPage tabSamples;
        private System.Windows.Forms.TabPage tabImportedGeometry;
        private System.Windows.Forms.TabPage tabGame;
        private System.Windows.Forms.TabPage tabPaths;
        private DarkUI.Controls.DarkButton butApply;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkListView optionsList;
        private System.Windows.Forms.Panel panel1;
        private DarkUI.Controls.DarkComboBox comboGameVersion;
        private DarkUI.Controls.DarkLabel darkLabel14;
        private System.Windows.Forms.TabPage tabMisc;
        private System.Windows.Forms.Panel panel12;
        private DarkUI.Controls.DarkLabel darkLabel16;
        private DarkUI.Controls.DarkPanel panelRoomAmbientLight;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Panel panelTr5Sprites;
        private System.Windows.Forms.PictureBox tr5SpritesTextureFilePathPicPreview;
        private DarkUI.Controls.DarkButton tr5SpritesTextureFilePathBut;
        private DarkUI.Controls.DarkRadioButton tr5SpritesFilePathOptCustom;
        private DarkUI.Controls.DarkLabel lblTr5ExtraSprites;
        private DarkUI.Controls.DarkRadioButton tr5SpritesFilePathOptAuto;
        private DarkUI.Controls.DarkTextBox tr5SpritesTextureFilePathTxt;
        private TombLib.Controls.DarkDataGridViewControls objectFileDataGridViewControls;
        private DarkUI.Controls.DarkDataGridView objectFileDataGridView;
        private System.Windows.Forms.TabPage tabTextures;
        private System.Windows.Forms.Panel panelTr5Weather;
        private DarkUI.Controls.DarkComboBox comboTr5Weather;
        private DarkUI.Controls.DarkLabel lblTr5Weather;
        private System.Windows.Forms.Panel panelTr5LaraType;
        private DarkUI.Controls.DarkComboBox comboLaraType;
        private DarkUI.Controls.DarkLabel lblLaraType;
        private System.Windows.Forms.Panel panel6;
        private DarkUI.Controls.DarkButton levelFilePathBut;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkTextBox levelFilePathTxt;
        private DarkUI.Controls.DarkDataGridView textureFileDataGridView;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private TombLib.Controls.DarkDataGridViewControls textureFileDataGridViewControls;
        private System.Windows.Forms.DataGridViewTextBoxColumn objectFileDataGridViewPathColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn objectFileDataGridViewSearchColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn objectFileDataGridViewShowColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn objectFileDataGridViewMessageColumn;
        private DarkUI.Controls.DarkLabel darkLabel13;
        private DarkUI.Controls.DarkNumericUpDown numPadding;
        private DarkUI.Controls.DarkLabel darkLabel12;
        private DarkUI.Controls.DarkCheckBox cbAgressiveTexturePacking;
        private DarkUI.Controls.DarkCheckBox cbAgressiveFloordataPacking;
        private System.Windows.Forms.TabPage tabStaticMeshes;
        private System.Windows.Forms.TabPage tabSoundsCatalogs;
        private DarkUI.Controls.DarkLabel darkLabel18;
        private DarkUI.Controls.DarkDataGridView selectedSoundsDataGridView;
        private DarkUI.Controls.DarkTextBox tbFilterSounds;
        private TombLib.Controls.DarkDataGridViewControls soundsCatalogsDataGridViewControls;
        private DarkUI.Controls.DarkDataGridView soundsCatalogsDataGridView;
        private DarkUI.Controls.DarkLabel labelSoundsCatalogsStatistics;
        private DarkUI.Controls.DarkLabel darkLabel17;
        private DarkUI.Controls.DarkDataGridView staticMeshMergeDataGridView;
        private DarkUI.Controls.DarkLabel darkLabel50;
        private DarkUI.Controls.DarkLabel darkLabel19;
        private DarkUI.Controls.DarkLabel lblCatalogsPrompt;
        private TombLib.Controls.DarkDataGridViewControls soundDataGridViewControls;
        private DarkUI.Controls.DarkDataGridView soundDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn soundDataGridViewColumnPath;
        private DarkUI.Controls.DarkDataGridViewButtonColumn soundDataGridViewColumnSearch;
        private DarkUI.Controls.DarkLabel lblPathsPrompt;
        private DarkUI.Controls.DarkButton butDeselectAllSounds;
        private DarkUI.Controls.DarkButton butSelectAllSounds;
        private DarkUI.Controls.DarkButton butSearchSounds;
        private DarkUI.Controls.DarkButton butAssignSoundsFromSelectedCatalogs;
        private DarkUI.Controls.DarkButton butAssignHardcodedSounds;
        private DarkUI.Controls.DarkButton butAutodetectSoundsAndAssign;
        private DarkUI.Controls.DarkButton butAssignFromSoundSources;
        private DarkUI.Controls.DarkLabel darkLabel20;
        private DarkUI.Controls.DarkSectionPanel darkSectionPanel1;
        private DarkUI.Controls.DarkButton butRemoveMissing;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSoundsId;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSoundsName;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelectedSoundsCatalogColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelectedSoundsGameColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SelectedSoundsOriginalIdColumn;
        private DarkUI.Controls.DarkButton butAssignFromWads;
        private DarkUI.Controls.DarkButton butSelectAllButShatterStatics;
        private DarkUI.Controls.DarkButton butDeselectAllStatics;
        private DarkUI.Controls.DarkButton butSelectAllStatics;
        private System.Windows.Forms.DataGridViewTextBoxColumn SoundsCatalogPathColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn SoundsCatalogSearchColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn SoundsCatalogReloadButton;
        private DarkUI.Controls.DarkDataGridViewButtonColumn SoundsCatalogEditColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn SoundsCatalogsAssignColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SoundsCatalogsSoundCountColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn SoundsCatalogMessageColumn;
        private DarkUI.Controls.DarkCheckBox cbAutodetectIfNoneSelected;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn colSoundsEnabled;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSoundID;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMeshName;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn colMergeStatics;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn colInterpretShadesAsEffect;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn colTintAsAmbient;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn colClearShades;
        private System.Windows.Forms.DataGridViewImageColumn textureFileDataGridViewPreviewColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn textureFileDataGridViewPathColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn textureFileDataGridViewSearchColumn;
        private DarkUI.Controls.DarkDataGridViewButtonColumn textureFileDataGridViewShowColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn textureFileDataGridViewMessageColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn textureFileDataGridViewSizeColumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn textureFileDataGridViewReplaceMagentaWithTransparencyColumn;
        private DarkUI.Controls.DarkDataGridViewCheckBoxColumn textureFileDataGridViewConvert512PixelsToDoubleRowsColumn;
        private DarkUI.Controls.DarkLabel darkLabel22;
        private DarkUI.Controls.DarkCheckBox cbOverrideAllLightQuality;
        private DarkUI.Controls.DarkComboBox cmbDefaultLightQuality;
        private DarkUI.Controls.DarkCheckBox cbDither16BitTextures;
        private DarkUI.Controls.DarkCheckBox cbRemapAnimTextures;
        private DarkUI.Controls.DarkCheckBox cbRearrangeRooms;
		private DarkUI.Controls.DarkCheckBox cbRemoveObjects;
        private DarkUI.Controls.DarkCheckBox GameEnableExtraReverbPresetsCheckBox;
        private DarkUI.Controls.DarkCheckBox cbEnableExtraBlendingModes;
        private DarkUI.Controls.DarkCheckBox cbKeepSampleRate;
        private DarkUI.Controls.DarkComboBox cmbSampleRate;
        private System.Windows.Forms.Panel panelLuaPath;
        private DarkUI.Controls.DarkButton butBrowseLuaPath;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private DarkUI.Controls.DarkTextBox tbLuaPath;
        private System.Windows.Forms.Panel panelScripts;
        private DarkUI.Controls.DarkButton scriptPathBut;
        private DarkUI.Controls.DarkLabel darkLabel15;
        private DarkUI.Controls.DarkTextBox tbScriptPath;
        private DarkUI.Controls.DarkCheckBox cbUse32BitLighting;
        private DarkUI.Controls.DarkCheckBox cbCompressTextures;
    }
}