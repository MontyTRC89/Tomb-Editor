using System.Windows.Forms;
using DarkUI.Controls;
namespace TombEditor.Forms.TombEngine
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
            this.butCancel = new DarkUI.Controls.DarkButton();
            this.butOk = new DarkUI.Controls.DarkButton();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.cbActivatorLara = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorNPC = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorOtherMoveables = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorStatics = new DarkUI.Controls.DarkCheckBox();
            this.cbActivatorFlyBy = new DarkUI.Controls.DarkCheckBox();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.darkLabel4 = new DarkUI.Controls.DarkLabel();
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.darkSectionPanel1 = new DarkUI.Controls.DarkSectionPanel();
            this.listEvents = new DarkUI.Controls.DarkListView();
            this.darkPanel1 = new DarkUI.Controls.DarkPanel();
            this.darkButton1 = new DarkUI.Controls.DarkButton();
            this.butNewRoom = new DarkUI.Controls.DarkButton();
            this.butEditRoomName = new DarkUI.Controls.DarkButton();
            this.tcEvents = new System.Windows.Forms.CustomTabControl();
            this.tabPage_OnEnter = new System.Windows.Forms.TabPage();
            this.tabPage_OnInside = new System.Windows.Forms.TabPage();
            this.tabPage_OnLeave = new System.Windows.Forms.TabPage();
            this.darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.tmEnter = new TombEditor.Controls.TriggerManager();
            this.tmInside = new TombEditor.Controls.TriggerManager();
            this.tmLeave = new TombEditor.Controls.TriggerManager();
            this.comboboxOnLeave = new TombLib.Controls.DarkSearchableComboBox();
            this.comboboxOnInside = new TombLib.Controls.DarkSearchableComboBox();
            this.comboboxOnEnter = new TombLib.Controls.DarkSearchableComboBox();
            this.darkSectionPanel1.SuspendLayout();
            this.darkPanel1.SuspendLayout();
            this.tcEvents.SuspendLayout();
            this.tabPage_OnEnter.SuspendLayout();
            this.tabPage_OnInside.SuspendLayout();
            this.tabPage_OnLeave.SuspendLayout();
            this.darkGroupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // butCancel
            // 
            this.butCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.butCancel.Checked = false;
            this.butCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.butCancel.Location = new System.Drawing.Point(816, 519);
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
            this.butOk.Location = new System.Drawing.Point(730, 519);
            this.butOk.Name = "butOk";
            this.butOk.Size = new System.Drawing.Size(80, 23);
            this.butOk.TabIndex = 1;
            this.butOk.Text = "OK";
            this.butOk.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butOk.Click += new System.EventHandler(this.butOk_Click);
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(12, 9);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(62, 13);
            this.darkLabel2.TabIndex = 9;
            this.darkLabel2.Text = "Activators:";
            // 
            // cbActivatorLara
            // 
            this.cbActivatorLara.AutoSize = true;
            this.cbActivatorLara.Location = new System.Drawing.Point(69, 8);
            this.cbActivatorLara.Name = "cbActivatorLara";
            this.cbActivatorLara.Size = new System.Drawing.Size(47, 17);
            this.cbActivatorLara.TabIndex = 10;
            this.cbActivatorLara.Text = "Lara";
            // 
            // cbActivatorNPC
            // 
            this.cbActivatorNPC.AutoSize = true;
            this.cbActivatorNPC.Location = new System.Drawing.Point(121, 8);
            this.cbActivatorNPC.Name = "cbActivatorNPC";
            this.cbActivatorNPC.Size = new System.Drawing.Size(47, 17);
            this.cbActivatorNPC.TabIndex = 11;
            this.cbActivatorNPC.Text = "NPC";
            // 
            // cbActivatorOtherMoveables
            // 
            this.cbActivatorOtherMoveables.AutoSize = true;
            this.cbActivatorOtherMoveables.Location = new System.Drawing.Point(172, 8);
            this.cbActivatorOtherMoveables.Name = "cbActivatorOtherMoveables";
            this.cbActivatorOtherMoveables.Size = new System.Drawing.Size(96, 17);
            this.cbActivatorOtherMoveables.TabIndex = 12;
            this.cbActivatorOtherMoveables.Text = "Other objects";
            // 
            // cbActivatorStatics
            // 
            this.cbActivatorStatics.AutoSize = true;
            this.cbActivatorStatics.Location = new System.Drawing.Point(269, 8);
            this.cbActivatorStatics.Name = "cbActivatorStatics";
            this.cbActivatorStatics.Size = new System.Drawing.Size(59, 17);
            this.cbActivatorStatics.TabIndex = 13;
            this.cbActivatorStatics.Text = "Statics";
            // 
            // cbActivatorFlyBy
            // 
            this.cbActivatorFlyBy.AutoSize = true;
            this.cbActivatorFlyBy.Location = new System.Drawing.Point(334, 8);
            this.cbActivatorFlyBy.Name = "cbActivatorFlyBy";
            this.cbActivatorFlyBy.Size = new System.Drawing.Size(91, 17);
            this.cbActivatorFlyBy.TabIndex = 14;
            this.cbActivatorFlyBy.Text = "Flyby camera";
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(176, 9);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(49, 13);
            this.darkLabel3.TabIndex = 15;
            this.darkLabel3.Text = "Actions:";
            // 
            // darkLabel4
            // 
            this.darkLabel4.AutoSize = true;
            this.darkLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel4.Location = new System.Drawing.Point(179, 38);
            this.darkLabel4.Name = "darkLabel4";
            this.darkLabel4.Size = new System.Drawing.Size(53, 13);
            this.darkLabel4.TabIndex = 16;
            this.darkLabel4.Text = "OnEnter:";
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(179, 67);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(57, 13);
            this.darkLabel1.TabIndex = 18;
            this.darkLabel1.Text = "OnInside:";
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(179, 96);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(54, 13);
            this.darkLabel5.TabIndex = 20;
            this.darkLabel5.Text = "OnLeave:";
            // 
            // darkSectionPanel1
            // 
            this.darkSectionPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.darkSectionPanel1.Controls.Add(this.listEvents);
            this.darkSectionPanel1.Controls.Add(this.darkPanel1);
            this.darkSectionPanel1.Location = new System.Drawing.Point(238, 127);
            this.darkSectionPanel1.Name = "darkSectionPanel1";
            this.darkSectionPanel1.SectionHeader = "Events";
            this.darkSectionPanel1.Size = new System.Drawing.Size(200, 385);
            this.darkSectionPanel1.TabIndex = 22;
            // 
            // listEvents
            // 
            this.listEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listEvents.Location = new System.Drawing.Point(5, 29);
            this.listEvents.Name = "listEvents";
            this.listEvents.Size = new System.Drawing.Size(190, 323);
            this.listEvents.TabIndex = 0;
            // 
            // darkPanel1
            // 
            this.darkPanel1.Controls.Add(this.darkButton1);
            this.darkPanel1.Controls.Add(this.butNewRoom);
            this.darkPanel1.Controls.Add(this.butEditRoomName);
            this.darkPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.darkPanel1.Location = new System.Drawing.Point(1, 354);
            this.darkPanel1.Name = "darkPanel1";
            this.darkPanel1.Size = new System.Drawing.Size(198, 30);
            this.darkPanel1.TabIndex = 1;
            // 
            // darkButton1
            // 
            this.darkButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.darkButton1.Checked = false;
            this.darkButton1.Image = global::TombEditor.Properties.Resources.general_trash_16;
            this.darkButton1.Location = new System.Drawing.Point(62, 3);
            this.darkButton1.Name = "darkButton1";
            this.darkButton1.Size = new System.Drawing.Size(23, 23);
            this.darkButton1.TabIndex = 20;
            this.darkButton1.Tag = "AddNewRoom";
            // 
            // butNewRoom
            // 
            this.butNewRoom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butNewRoom.Checked = false;
            this.butNewRoom.Image = global::TombEditor.Properties.Resources.general_copy_16;
            this.butNewRoom.Location = new System.Drawing.Point(33, 3);
            this.butNewRoom.Name = "butNewRoom";
            this.butNewRoom.Size = new System.Drawing.Size(23, 23);
            this.butNewRoom.TabIndex = 19;
            this.butNewRoom.Tag = "AddNewRoom";
            // 
            // butEditRoomName
            // 
            this.butEditRoomName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.butEditRoomName.Checked = false;
            this.butEditRoomName.Image = global::TombEditor.Properties.Resources.general_plus_math_16;
            this.butEditRoomName.Location = new System.Drawing.Point(4, 3);
            this.butEditRoomName.Name = "butEditRoomName";
            this.butEditRoomName.Size = new System.Drawing.Size(23, 23);
            this.butEditRoomName.TabIndex = 18;
            this.butEditRoomName.Tag = "EditRoomName";
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
            this.tcEvents.Location = new System.Drawing.Point(444, 125);
            this.tcEvents.Name = "tcEvents";
            this.tcEvents.SelectedIndex = 0;
            this.tcEvents.Size = new System.Drawing.Size(455, 354);
            this.tcEvents.TabIndex = 1;
            // 
            // tabPage_OnEnter
            // 
            this.tabPage_OnEnter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabPage_OnEnter.Controls.Add(this.tmEnter);
            this.tabPage_OnEnter.Location = new System.Drawing.Point(4, 23);
            this.tabPage_OnEnter.Name = "tabPage_OnEnter";
            this.tabPage_OnEnter.Size = new System.Drawing.Size(447, 327);
            this.tabPage_OnEnter.TabIndex = 0;
            this.tabPage_OnEnter.Text = "When entering";
            // 
            // tabPage_OnInside
            // 
            this.tabPage_OnInside.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabPage_OnInside.Controls.Add(this.tmInside);
            this.tabPage_OnInside.Location = new System.Drawing.Point(4, 23);
            this.tabPage_OnInside.Name = "tabPage_OnInside";
            this.tabPage_OnInside.Size = new System.Drawing.Size(447, 327);
            this.tabPage_OnInside.TabIndex = 1;
            this.tabPage_OnInside.Text = "When inside";
            // 
            // tabPage_OnLeave
            // 
            this.tabPage_OnLeave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.tabPage_OnLeave.Controls.Add(this.tmLeave);
            this.tabPage_OnLeave.Location = new System.Drawing.Point(4, 23);
            this.tabPage_OnLeave.Name = "tabPage_OnLeave";
            this.tabPage_OnLeave.Size = new System.Drawing.Size(447, 327);
            this.tabPage_OnLeave.TabIndex = 2;
            this.tabPage_OnLeave.Text = "When leaving";
            // 
            // darkGroupBox1
            // 
            this.darkGroupBox1.Controls.Add(this.darkLabel6);
            this.darkGroupBox1.Controls.Add(this.cbActivatorLara);
            this.darkGroupBox1.Controls.Add(this.cbActivatorNPC);
            this.darkGroupBox1.Controls.Add(this.cbActivatorOtherMoveables);
            this.darkGroupBox1.Controls.Add(this.cbActivatorStatics);
            this.darkGroupBox1.Controls.Add(this.cbActivatorFlyBy);
            this.darkGroupBox1.Location = new System.Drawing.Point(447, 481);
            this.darkGroupBox1.Name = "darkGroupBox1";
            this.darkGroupBox1.Size = new System.Drawing.Size(449, 31);
            this.darkGroupBox1.TabIndex = 24;
            this.darkGroupBox1.TabStop = false;
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
            // tmEnter
            // 
            this.tmEnter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tmEnter.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tmEnter.Location = new System.Drawing.Point(0, 0);
            this.tmEnter.Name = "tmEnter";
            this.tmEnter.Padding = new System.Windows.Forms.Padding(2, 2, 2, 1);
            this.tmEnter.Size = new System.Drawing.Size(447, 327);
            this.tmEnter.TabIndex = 0;
            // 
            // tmInside
            // 
            this.tmInside.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tmInside.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tmInside.Location = new System.Drawing.Point(0, 0);
            this.tmInside.Name = "tmInside";
            this.tmInside.Padding = new System.Windows.Forms.Padding(2, 2, 2, 1);
            this.tmInside.Size = new System.Drawing.Size(447, 327);
            this.tmInside.TabIndex = 1;
            // 
            // tmLeave
            // 
            this.tmLeave.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tmLeave.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tmLeave.Location = new System.Drawing.Point(0, 0);
            this.tmLeave.Name = "tmLeave";
            this.tmLeave.Padding = new System.Windows.Forms.Padding(2, 2, 2, 1);
            this.tmLeave.Size = new System.Drawing.Size(447, 327);
            this.tmLeave.TabIndex = 1;
            // 
            // comboboxOnLeave
            // 
            this.comboboxOnLeave.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboboxOnLeave.Location = new System.Drawing.Point(238, 90);
            this.comboboxOnLeave.Name = "comboboxOnLeave";
            this.comboboxOnLeave.Size = new System.Drawing.Size(658, 23);
            this.comboboxOnLeave.TabIndex = 21;
            // 
            // comboboxOnInside
            // 
            this.comboboxOnInside.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboboxOnInside.Location = new System.Drawing.Point(238, 61);
            this.comboboxOnInside.Name = "comboboxOnInside";
            this.comboboxOnInside.Size = new System.Drawing.Size(658, 23);
            this.comboboxOnInside.TabIndex = 19;
            // 
            // comboboxOnEnter
            // 
            this.comboboxOnEnter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboboxOnEnter.Location = new System.Drawing.Point(238, 32);
            this.comboboxOnEnter.Name = "comboboxOnEnter";
            this.comboboxOnEnter.Size = new System.Drawing.Size(658, 23);
            this.comboboxOnEnter.TabIndex = 17;
            // 
            // FormVolume
            // 
            this.AcceptButton = this.butOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.butCancel;
            this.ClientSize = new System.Drawing.Size(904, 550);
            this.Controls.Add(this.darkGroupBox1);
            this.Controls.Add(this.tcEvents);
            this.Controls.Add(this.darkSectionPanel1);
            this.Controls.Add(this.comboboxOnLeave);
            this.Controls.Add(this.darkLabel5);
            this.Controls.Add(this.comboboxOnInside);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.comboboxOnEnter);
            this.Controls.Add(this.darkLabel4);
            this.Controls.Add(this.darkLabel3);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.butCancel);
            this.Controls.Add(this.butOk);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormVolume";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit trigger volume";
            this.darkSectionPanel1.ResumeLayout(false);
            this.darkPanel1.ResumeLayout(false);
            this.tcEvents.ResumeLayout(false);
            this.tabPage_OnEnter.ResumeLayout(false);
            this.tabPage_OnInside.ResumeLayout(false);
            this.tabPage_OnLeave.ResumeLayout(false);
            this.darkGroupBox1.ResumeLayout(false);
            this.darkGroupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkButton butCancel;
        private DarkUI.Controls.DarkButton butOk;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkCheckBox cbActivatorLara;
        private DarkUI.Controls.DarkCheckBox cbActivatorNPC;
        private DarkUI.Controls.DarkCheckBox cbActivatorOtherMoveables;
        private DarkUI.Controls.DarkCheckBox cbActivatorStatics;
        private DarkUI.Controls.DarkCheckBox cbActivatorFlyBy;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkLabel darkLabel4;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private TombLib.Controls.DarkSearchableComboBox comboboxOnEnter;
        private TombLib.Controls.DarkSearchableComboBox comboboxOnInside;
        private TombLib.Controls.DarkSearchableComboBox comboboxOnLeave;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkSectionPanel darkSectionPanel1;
        private DarkListView listEvents;
        private CustomTabControl tcEvents;
        private TabPage tabPage_OnEnter;
        private TabPage tabPage_OnInside;
        private TabPage tabPage_OnLeave;
        private Controls.TriggerManager tmEnter;
        private Controls.TriggerManager tmInside;
        private Controls.TriggerManager tmLeave;
        private DarkPanel darkPanel1;
        private DarkButton darkButton1;
        private DarkButton butNewRoom;
        private DarkButton butEditRoomName;
        private DarkGroupBox darkGroupBox1;
        private DarkLabel darkLabel6;
    }
}