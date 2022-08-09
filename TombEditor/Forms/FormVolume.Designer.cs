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
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.cbActivatorLara = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorNPC = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorOtherMoveables = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorStatics = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorFlyBy = new DarkUI.Controls.DarkCheckBox();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.lstEvents = new DarkUI.Controls.DarkListView();
            this.darkPanel1 = new DarkUI.Controls.DarkPanel();
            this.butSearch = new DarkUI.Controls.DarkButton();
            this.butUnassignEventSet = new DarkUI.Controls.DarkButton();
            this.butDeleteEventSet = new DarkUI.Controls.DarkButton();
            this.butCloneEventSet = new DarkUI.Controls.DarkButton();
            this.butNewEventSet = new DarkUI.Controls.DarkButton();
            this.tcEvents = new System.Windows.Forms.CustomTabControl();
            this.tabPage_OnEnter = new System.Windows.Forms.TabPage();
            this.tmEnter = new TombEditor.Controls.TriggerManager();
            this.tabPage_OnInside = new System.Windows.Forms.TabPage();
            this.tmInside = new TombEditor.Controls.TriggerManager();
            this.tabPage_OnLeave = new System.Windows.Forms.TabPage();
            this.tmLeave = new TombEditor.Controls.TriggerManager();
            this.grpActivators = new DarkUI.Controls.DarkGroupBox();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.tbName = new DarkUI.Controls.DarkTextBox();
            this.darkSectionPanel1.SuspendLayout();
            this.darkPanel1.SuspendLayout();
            this.tcEvents.SuspendLayout();
            this.tabPage_OnEnter.SuspendLayout();
            this.tabPage_OnInside.SuspendLayout();
            this.tabPage_OnLeave.SuspendLayout();
            this.grpActivators.SuspendLayout();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(579, 391);
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
            this.butOk.Location = new System.Drawing.Point(493, 391);
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
            this.darkSectionPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.darkSectionPanel1.Controls.Add(this.lstEvents);
            this.darkSectionPanel1.Controls.Add(this.darkPanel1);
            this.darkSectionPanel1.Location = new System.Drawing.Point(6, 6);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = "Event sets";
            this.darkSectionPanel1.Size = new System.Drawing.Size(204, 378);
            this.darkSectionPanel1.TabIndex = 22;
            // 
            // lstEvents
            // 
            this.lstEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstEvents.Location = new System.Drawing.Point(5, 55);
            this.lstEvents.Name = "lstEvents";
            this.lstEvents.Size = new System.Drawing.Size(194, 318);
            this.lstEvents.TabIndex = 0;
            this.lstEvents.SelectedIndicesChanged += new System.EventHandler(this.lstEvents_SelectedIndicesChanged);
            // 
            // darkPanel1
            // 
            this.darkPanel1.Controls.Add(this.butSearch);
            this.darkPanel1.Controls.Add(this.butUnassignEventSet);
            this.darkPanel1.Controls.Add(this.butDeleteEventSet);
            this.darkPanel1.Controls.Add(this.butCloneEventSet);
            this.darkPanel1.Controls.Add(this.butNewEventSet);
            this.darkPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.darkPanel1.Location = new System.Drawing.Point(1, 25);
            this.darkPanel1.Name = "darkPanel1";
            this.darkPanel1.Size = new System.Drawing.Size(202, 30);
            this.darkPanel1.TabIndex = 1;
            // 
            // butSearch
            // 
            this.butSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butSearch.Checked = false;
            this.butSearch.Image = global::TombEditor.Properties.Resources.general_search_16;
            this.butSearch.Location = new System.Drawing.Point(146, 3);
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
            this.butUnassignEventSet.Location = new System.Drawing.Point(175, 3);
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
            // tcEvents
            // 
            this.tcEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tcEvents.Controls.Add(this.tabPage_OnEnter);
            this.tcEvents.Controls.Add(this.tabPage_OnInside);
            this.tcEvents.Controls.Add(this.tabPage_OnLeave);
            this.tcEvents.DisplayStyle = System.Windows.Forms.TabStyle.Dark;
            this.tcEvents.DisplayStyleProvider.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
            this.tcEvents.DisplayStyleProvider.BorderColorHot = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
            this.tcEvents.DisplayStyleProvider.BorderColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
            this.tcEvents.DisplayStyleProvider.CloserColor = System.Drawing.Color.White;
            this.tcEvents.DisplayStyleProvider.CloserColorActive = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(196)))), ((int)(((byte)(232)))));
            this.tcEvents.DisplayStyleProvider.FocusTrack = false;
            this.tcEvents.DisplayStyleProvider.HotTrack = false;
            this.tcEvents.DisplayStyleProvider.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.tcEvents.DisplayStyleProvider.Opacity = 1F;
            this.tcEvents.DisplayStyleProvider.Overlap = 0;
            this.tcEvents.DisplayStyleProvider.Padding = new System.Drawing.Point(6, 3);
            this.tcEvents.DisplayStyleProvider.Radius = 10;
            this.tcEvents.DisplayStyleProvider.ShowTabCloser = false;
            this.tcEvents.DisplayStyleProvider.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(153)))), ((int)(((byte)(153)))), ((int)(((byte)(153)))));
            this.tcEvents.DisplayStyleProvider.TextColorDisabled = System.Drawing.Color.FromArgb(((int)(((byte)(96)))), ((int)(((byte)(96)))), ((int)(((byte)(96)))));
            this.tcEvents.DisplayStyleProvider.TextColorSelected = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(196)))), ((int)(((byte)(232)))));
            this.tcEvents.Location = new System.Drawing.Point(212, 33);
            this.tcEvents.Name = "tcEvents";
            this.tcEvents.SelectedIndex = 0;
            this.tcEvents.Size = new System.Drawing.Size(450, 318);
            this.tcEvents.TabIndex = 1;
            // 
            // tabPage_OnEnter
            // 
            this.tabPage_OnEnter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabPage_OnEnter.Controls.Add(this.tmEnter);
            this.tabPage_OnEnter.Location = new System.Drawing.Point(4, 23);
            this.tabPage_OnEnter.Name = "tabPage_OnEnter";
            this.tabPage_OnEnter.Size = new System.Drawing.Size(442, 291);
            this.tabPage_OnEnter.TabIndex = 0;
            this.tabPage_OnEnter.Text = "When entering";
            // 
            // tmEnter
            // 
            this.tmEnter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tmEnter.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tmEnter.Location = new System.Drawing.Point(0, 0);
            this.tmEnter.Name = "tmEnter";
            this.tmEnter.Padding = new System.Windows.Forms.Padding(2, 2, 2, 1);
            this.tmEnter.Size = new System.Drawing.Size(442, 291);
            this.tmEnter.TabIndex = 0;
            // 
            // tabPage_OnInside
            // 
            this.tabPage_OnInside.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabPage_OnInside.Controls.Add(this.tmInside);
            this.tabPage_OnInside.Location = new System.Drawing.Point(4, 23);
            this.tabPage_OnInside.Name = "tabPage_OnInside";
            this.tabPage_OnInside.Size = new System.Drawing.Size(431, 291);
            this.tabPage_OnInside.TabIndex = 1;
            this.tabPage_OnInside.Text = "When inside";
            // 
            // tmInside
            // 
            this.tmInside.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tmInside.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tmInside.Location = new System.Drawing.Point(0, 0);
            this.tmInside.Name = "tmInside";
            this.tmInside.Padding = new System.Windows.Forms.Padding(2, 2, 2, 1);
            this.tmInside.Size = new System.Drawing.Size(431, 291);
            this.tmInside.TabIndex = 1;
            // 
            // tabPage_OnLeave
            // 
            this.tabPage_OnLeave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabPage_OnLeave.Controls.Add(this.tmLeave);
            this.tabPage_OnLeave.Location = new System.Drawing.Point(4, 23);
            this.tabPage_OnLeave.Name = "tabPage_OnLeave";
            this.tabPage_OnLeave.Size = new System.Drawing.Size(431, 291);
            this.tabPage_OnLeave.TabIndex = 2;
            this.tabPage_OnLeave.Text = "When leaving";
            // 
            // tmLeave
            // 
            this.tmLeave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tmLeave.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tmLeave.Location = new System.Drawing.Point(0, 0);
            this.tmLeave.Name = "tmLeave";
            this.tmLeave.Padding = new System.Windows.Forms.Padding(2, 2, 2, 1);
            this.tmLeave.Size = new System.Drawing.Size(431, 291);
            this.tmLeave.TabIndex = 1;
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
            this.grpActivators.Location = new System.Drawing.Point(215, 353);
            this.grpActivators.Name = "grpActivators";
            this.grpActivators.Size = new System.Drawing.Size(444, 31);
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
            this.darkLabel1.Location = new System.Drawing.Point(212, 8);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(87, 13);
            this.darkLabel1.TabIndex = 27;
            this.darkLabel1.Text = "Event set name:";
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbName.Location = new System.Drawing.Point(303, 6);
            this.tbName.Name = "tbName";
            this.tbName.SelectOnClick = true;
            this.tbName.Size = new System.Drawing.Size(356, 22);
            this.tbName.TabIndex = 28;
            this.tbName.TextChanged += new System.EventHandler(this.tbName_TextChanged);
            // 
            // FormVolume
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(665, 421);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.grpActivators);
            this.Controls.Add(this.tcEvents);
            this.Controls.Add(this.darkSectionPanel1);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOk);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(670, 460);
            this.Name = "FormVolume";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit trigger volume";
            this.darkSectionPanel1.ResumeLayout(false);
            this.darkPanel1.ResumeLayout(false);
            this.tcEvents.ResumeLayout(false);
            this.tabPage_OnEnter.ResumeLayout(false);
            this.tabPage_OnInside.ResumeLayout(false);
            this.tabPage_OnLeave.ResumeLayout(false);
            this.grpActivators.ResumeLayout(false);
            this.grpActivators.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private DarkListView lstEvents;
        private CustomTabControl tcEvents;
        private TabPage tabPage_OnEnter;
        private TabPage tabPage_OnInside;
        private TabPage tabPage_OnLeave;
        private Controls.TriggerManager tmEnter;
        private Controls.TriggerManager tmInside;
        private Controls.TriggerManager tmLeave;
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
    }
}