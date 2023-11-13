using System.Windows.Forms;
using DarkUI.Controls;

namespace TombEditor.Forms
{
    partial class FormVolume
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
            this.components = new System.ComponentModel.Container();
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.cbActivatorLara = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorNPC = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorOtherMoveables = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorStatics = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorFlyBy = new DarkUI.Controls.DarkCheckBox();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.dgvEvents = new DarkUI.Controls.DarkDataGridView();
            this.darkPanel1 = new DarkUI.Controls.DarkPanel();
            this.butSearch = new DarkUI.Controls.DarkButton();
            this.butUnassignEventSet = new DarkUI.Controls.DarkButton();
            this.butDeleteEventSet = new DarkUI.Controls.DarkButton();
            this.butCloneEventSet = new DarkUI.Controls.DarkButton();
            this.butNewEventSet = new DarkUI.Controls.DarkButton();
            this.triggerManager = new TombEditor.Controls.TriggerManager();
            this.grpActivators = new DarkUI.Controls.DarkGroupBox();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.tbName = new DarkUI.Controls.DarkTextBox();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.cbEvents = new DarkUI.Controls.DarkComboBox();
            this.darkSectionPanel2 = new DarkUI.Controls.DarkSectionPanel();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.darkSectionPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvEvents)).BeginInit();
            this.darkPanel1.SuspendLayout();
            this.grpActivators.SuspendLayout();
            this.darkSectionPanel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(648, 391);
            this.butCancel.Name = "butCancel";
            this.butCancel.Size = new System.Drawing.Size(80, 23);
            this.butCancel.TabIndex = 2;
            this.butCancel.Text = "Cancel";
            this.butCancel.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butCancel.Click += new System.EventHandler(this.butCancel_Click);
            // 
            // butOk
            // 
            this.butOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butOk.Checked = false;
            this.butOk.Location = new System.Drawing.Point(562, 391);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 1;
            this.butOk.Text = "OK";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // cbActivatorLara
            // 
            this.cbActivatorLara.AutoSize = true;
            this.cbActivatorLara.Location = new System.Drawing.Point(69, 8);
            this.cbActivatorLara.Name = "cbActivatorLara";
            this.cbActivatorLara.Size = new System.Drawing.Size(47, 17);
            this.cbActivatorLara.TabIndex = 10;
            this.cbActivatorLara.Text = "Lara";
            this.toolTip.SetToolTip(this.cbActivatorLara, "Can be activated by Lara");
            this.cbActivatorLara.CheckedChanged += new System.EventHandler(this.cbActivators_CheckedChanged);
            // 
            // cbActivatorNPC
            // 
            this.cbActivatorNPC.AutoSize = true;
            this.cbActivatorNPC.Location = new System.Drawing.Point(121, 8);
            this.cbActivatorNPC.Name = "cbActivatorNPC";
            this.cbActivatorNPC.Size = new System.Drawing.Size(47, 17);
            this.cbActivatorNPC.TabIndex = 11;
            this.cbActivatorNPC.Text = "NPC";
            this.toolTip.SetToolTip(this.cbActivatorNPC, "Can be activated by creatures");
            this.cbActivatorNPC.CheckedChanged += new System.EventHandler(this.cbActivators_CheckedChanged);
            // 
            // cbActivatorOtherMoveables
            // 
            this.cbActivatorOtherMoveables.AutoSize = true;
            this.cbActivatorOtherMoveables.Location = new System.Drawing.Point(173, 8);
            this.cbActivatorOtherMoveables.Name = "cbActivatorOtherMoveables";
            this.cbActivatorOtherMoveables.Size = new System.Drawing.Size(96, 17);
            this.cbActivatorOtherMoveables.TabIndex = 12;
            this.cbActivatorOtherMoveables.Text = "Other objects";
            this.toolTip.SetToolTip(this.cbActivatorOtherMoveables, "Can be activated by other moveables, such as rolling balls");
            this.cbActivatorOtherMoveables.CheckedChanged += new System.EventHandler(this.cbActivators_CheckedChanged);
            // 
            // cbActivatorStatics
            // 
            this.cbActivatorStatics.AutoSize = true;
            this.cbActivatorStatics.Location = new System.Drawing.Point(272, 8);
            this.cbActivatorStatics.Name = "cbActivatorStatics";
            this.cbActivatorStatics.Size = new System.Drawing.Size(59, 17);
            this.cbActivatorStatics.TabIndex = 13;
            this.cbActivatorStatics.Text = "Statics";
            this.toolTip.SetToolTip(this.cbActivatorStatics, "Can be activated by shattering statics");
            this.cbActivatorStatics.CheckedChanged += new System.EventHandler(this.cbActivators_CheckedChanged);
            // 
            // cbActivatorFlyBy
            // 
            this.cbActivatorFlyBy.Location = new System.Drawing.Point(337, 8);
            this.cbActivatorFlyBy.Name = "cbActivatorFlyBy";
            this.cbActivatorFlyBy.Size = new System.Drawing.Size(93, 17);
            this.cbActivatorFlyBy.TabIndex = 14;
            this.cbActivatorFlyBy.Text = "Flyby cameras";
            this.toolTip.SetToolTip(this.cbActivatorFlyBy, "Can be activated by flyby cameras");
            this.cbActivatorFlyBy.CheckedChanged += new System.EventHandler(this.cbActivators_CheckedChanged);
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkSectionPanel1.Controls.Add(this.dgvEvents);
            this.darkSectionPanel1.Controls.Add(this.darkPanel1);
            this.darkSectionPanel1.Location = new System.Drawing.Point(3, 3);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = null;
            this.darkSectionPanel1.Size = new System.Drawing.Size(207, 377);
            this.darkSectionPanel1.TabIndex = 22;
            // 
            // dgvEvents
            // 
            this.dgvEvents.AllowUserToAddRows = false;
            this.dgvEvents.AllowUserToDeleteRows = false;
            this.dgvEvents.AllowUserToPasteCells = false;
            this.dgvEvents.AllowUserToResizeColumns = false;
            this.dgvEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvEvents.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvEvents.ColumnHeadersHeight = 4;
            this.dgvEvents.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dgvEvents.ForegroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.dgvEvents.Location = new System.Drawing.Point(4, 32);
            this.dgvEvents.MultiSelect = false;
            this.dgvEvents.Name = "dgvEvents";
            this.dgvEvents.ReadOnly = true;
            this.dgvEvents.RowHeadersWidth = 41;
            this.dgvEvents.Size = new System.Drawing.Size(199, 341);
            this.dgvEvents.TabIndex = 0;
            this.dgvEvents.ColumnHeaderMouseClick += dgvEvents_ColumnHeaderMouseClick;
            this.dgvEvents.SelectionChanged += dgvEvents_SelectedIndicesChanged;
            this.dgvEvents.DragDrop += dgvEvents_DragDrop;
            this.dgvEvents.UseAlternativeDragDropMethod = true;
            // 
            // darkPanel1
            // 
            this.darkPanel1.Controls.Add(this.butSearch);
            this.darkPanel1.Controls.Add(this.butUnassignEventSet);
            this.darkPanel1.Controls.Add(this.butDeleteEventSet);
            this.darkPanel1.Controls.Add(this.butCloneEventSet);
            this.darkPanel1.Controls.Add(this.butNewEventSet);
            this.darkPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.darkPanel1.Location = new System.Drawing.Point(1, 1);
            this.darkPanel1.Name = "darkPanel1";
            this.darkPanel1.Size = new System.Drawing.Size(205, 30);
            this.darkPanel1.TabIndex = 1;
            // 
            // butSearch
            // 
            this.butSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSearch.Checked = false;
            this.butSearch.Image = global::TombEditor.Properties.Resources.general_search_16;
            this.butSearch.Location = new System.Drawing.Point(149, 3);
            this.butSearch.Name = "butSearch";
            this.butSearch.Size = new System.Drawing.Size(23, 23);
            this.butSearch.TabIndex = 26;
            this.toolTip.SetToolTip(this.butSearch, "Search for event set");
            this.butSearch.Click += new System.EventHandler(this.butSearch_Click);
            // 
            // butUnassignEventSet
            // 
            this.butUnassignEventSet.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butUnassignEventSet.Checked = false;
            this.butUnassignEventSet.Image = global::TombEditor.Properties.Resources.actions_delete_16;
            this.butUnassignEventSet.Location = new System.Drawing.Point(178, 3);
            this.butUnassignEventSet.Name = "butUnassignEventSet";
            this.butUnassignEventSet.Size = new System.Drawing.Size(23, 23);
            this.butUnassignEventSet.TabIndex = 25;
            this.toolTip.SetToolTip(this.butUnassignEventSet, "Unassign event set from volume");
            this.butUnassignEventSet.Click += new System.EventHandler(this.butUnassignEventSet_Click);
            // 
            // butDeleteEventSet
            // 
            this.butDeleteEventSet.Checked = false;
            this.butDeleteEventSet.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.butDeleteEventSet.Location = new System.Drawing.Point(62, 3);
            this.butDeleteEventSet.Name = "butDeleteEventSet";
            this.butDeleteEventSet.Size = new System.Drawing.Size(23, 23);
            this.butDeleteEventSet.TabIndex = 20;
            this.butDeleteEventSet.Tag = "AddNewRoom";
            this.toolTip.SetToolTip(this.butDeleteEventSet, "Delete selected event set");
            this.butDeleteEventSet.Click += new System.EventHandler(this.butDeleteEventSet_Click);
            // 
            // butCloneEventSet
            // 
            this.butCloneEventSet.Checked = false;
            this.butCloneEventSet.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCloneEventSet.Image = global::TombEditor.Properties.Resources.general_copy_16;
            this.butCloneEventSet.Location = new System.Drawing.Point(33, 3);
            this.butCloneEventSet.Name = "butCloneEventSet";
            this.butCloneEventSet.Size = new System.Drawing.Size(23, 23);
            this.butCloneEventSet.TabIndex = 19;
            this.butCloneEventSet.Tag = "AddNewRoom";
            this.toolTip.SetToolTip(this.butCloneEventSet, "Copy selected event set");
            this.butCloneEventSet.Click += new System.EventHandler(this.butCloneEventSet_Click);
            // 
            // butNewEventSet
            // 
            this.butNewEventSet.Checked = false;
            this.butNewEventSet.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butNewEventSet.Location = new System.Drawing.Point(4, 3);
            this.butNewEventSet.Name = "butNewEventSet";
            this.butNewEventSet.Size = new System.Drawing.Size(23, 23);
            this.butNewEventSet.TabIndex = 18;
            this.butNewEventSet.Tag = "EditRoomName";
            this.toolTip.SetToolTip(this.butNewEventSet, "Add new event set");
            this.butNewEventSet.Click += new System.EventHandler(this.butNewEventSet_Click);
            // 
            // triggerManager
            // 
            this.triggerManager.Dock = System.Windows.Forms.DockStyle.Fill;
            this.triggerManager.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.triggerManager.Location = new System.Drawing.Point(1, 1);
            this.triggerManager.Name = "triggerManager";
            this.triggerManager.Padding = new System.Windows.Forms.Padding(2, 2, 2, 1);
            this.triggerManager.Size = new System.Drawing.Size(509, 310);
            this.triggerManager.TabIndex = 0;
            // 
            // grpActivators
            // 
            this.grpActivators.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpActivators.Controls.Add(this.darkLabel6);
            this.grpActivators.Controls.Add(this.cbActivatorLara);
            this.grpActivators.Controls.Add(this.cbActivatorNPC);
            this.grpActivators.Controls.Add(this.cbActivatorOtherMoveables);
            this.grpActivators.Controls.Add(this.cbActivatorStatics);
            this.grpActivators.Controls.Add(this.cbActivatorFlyBy);
            this.grpActivators.Location = new System.Drawing.Point(1, 349);
            this.grpActivators.Name = "grpActivators";
            this.grpActivators.Size = new System.Drawing.Size(511, 31);
            this.grpActivators.TabIndex = 24;
            this.grpActivators.TabStop = false;
            // 
            // darkLabel6
            // 
            this.darkLabel6.AutoSize = true;
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(6, 9);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(60, 13);
            this.darkLabel6.TabIndex = 26;
            this.darkLabel6.Text = "Activators:";
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(1, 10);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(87, 13);
            this.darkLabel1.TabIndex = 27;
            this.darkLabel1.Text = "Event set name:";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(89, 7);
            this.tbName.Name = "tbName";
            this.tbName.SelectOnClick = true;
            this.tbName.Size = new System.Drawing.Size(213, 22);
            this.tbName.TabIndex = 28;
            this.tbName.Validated += new System.EventHandler(this.tbName_Validated);
            // 
            // darkLabel2
            // 
            this.darkLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(310, 11);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(38, 13);
            this.darkLabel2.TabIndex = 29;
            this.darkLabel2.Text = "Event:";
            // 
            // cbEvents
            // 
            this.cbEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cbEvents.FormattingEnabled = true;
            this.cbEvents.Location = new System.Drawing.Point(349, 7);
            this.cbEvents.Name = "cbEvents";
            this.cbEvents.Size = new System.Drawing.Size(162, 23);
            this.cbEvents.TabIndex = 30;
            this.cbEvents.SelectedIndexChanged += new System.EventHandler(this.cbEvents_SelectedIndexChanged);
            // 
            // darkSectionPanel2
            // 
            this.darkSectionPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.darkSectionPanel2.Controls.Add(this.triggerManager);
            this.darkSectionPanel2.Location = new System.Drawing.Point(1, 35);
            this.darkSectionPanel2.Name = "darkSectionPanel2";
            this.darkSectionPanel2.SectionHeader = null;
            this.darkSectionPanel2.Size = new System.Drawing.Size(511, 312);
            this.darkSectionPanel2.TabIndex = 31;
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(1, 1);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.darkSectionPanel1);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.darkLabel1);
            this.splitContainer.Panel2.Controls.Add(this.darkSectionPanel2);
            this.splitContainer.Panel2.Controls.Add(this.grpActivators);
            this.splitContainer.Panel2.Controls.Add(this.cbEvents);
            this.splitContainer.Panel2.Controls.Add(this.tbName);
            this.splitContainer.Panel2.Controls.Add(this.darkLabel2);
            this.splitContainer.Size = new System.Drawing.Size(732, 387);
            this.splitContainer.SplitterDistance = 211;
            this.splitContainer.TabIndex = 32;
            this.splitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer_SplitterMoved);
            // 
            // FormVolume
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 421);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOk);
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(750, 460);
            this.Name = "FormVolume";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit trigger volume";
            this.darkSectionPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvEvents)).EndInit();
            this.darkPanel1.ResumeLayout(false);
            this.grpActivators.ResumeLayout(false);
            this.grpActivators.PerformLayout();
            this.darkSectionPanel2.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkCheckBox cbActivatorLara;
        private DarkUI.Controls.DarkCheckBox cbActivatorNPC;
        private DarkUI.Controls.DarkCheckBox cbActivatorOtherMoveables;
        private DarkUI.Controls.DarkCheckBox cbActivatorStatics;
        private DarkUI.Controls.DarkCheckBox cbActivatorFlyBy;
        private DarkSectionPanel darkSectionPanel1;
        private DarkDataGridView dgvEvents;
        private TabPage tabPage_OnInside;
        private TabPage tabPage_OnLeave;
        private Controls.TriggerManager triggerManager;
        private DarkPanel darkPanel1;
        private DarkButton butDeleteEventSet;
        private DarkButton butCloneEventSet;
        private DarkButton butNewEventSet;
        private DarkGroupBox grpActivators;
        private DarkLabel darkLabel6;
        private DarkButton butUnassignEventSet;
        private ToolTip toolTip;
        private DarkLabel darkLabel1;
        private DarkTextBox tbName;
        private DarkButton butSearch;
        private DarkLabel darkLabel2;
        private DarkComboBox cbEvents;
        private DarkSectionPanel darkSectionPanel2;
        private SplitContainer splitContainer;
    }
}