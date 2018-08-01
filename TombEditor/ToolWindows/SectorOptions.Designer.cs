namespace TombEditor.ToolWindows
{
    partial class SectorOptions
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel2DGrid = new TombEditor.Controls.Panel2DGrid();
            this.panelRight = new System.Windows.Forms.Panel();
            this.butCeiling = new DarkUI.Controls.DarkButton();
            this.butClimbPositiveZ = new DarkUI.Controls.DarkButton();
            this.butClimbPositiveX = new DarkUI.Controls.DarkButton();
            this.butClimbNegativeZ = new DarkUI.Controls.DarkButton();
            this.butClimbNegativeX = new DarkUI.Controls.DarkButton();
            this.butNotWalkableBox = new DarkUI.Controls.DarkButton();
            this.butPortal = new DarkUI.Controls.DarkButton();
            this.butFlagTriggerTriggerer = new DarkUI.Controls.DarkButton();
            this.butDeath = new DarkUI.Controls.DarkButton();
            this.butForceSolidFloor = new DarkUI.Controls.DarkButton();
            this.butMonkey = new DarkUI.Controls.DarkButton();
            this.butFlagBeetle = new DarkUI.Controls.DarkButton();
            this.butBox = new DarkUI.Controls.DarkButton();
            this.butFloor = new DarkUI.Controls.DarkButton();
            this.butWall = new DarkUI.Controls.DarkButton();
            this.panel2DGrid_sub = new System.Windows.Forms.Panel();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.butDiagonalWall = new DarkUI.Controls.DarkButton();
            this.butDiagonalFloor = new DarkUI.Controls.DarkButton();
            this.butDiagonalCeiling = new DarkUI.Controls.DarkButton();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panelRight.SuspendLayout();
            this.panel2DGrid_sub.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2DGrid
            // 
            this.panel2DGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2DGrid.Location = new System.Drawing.Point(2, 0);
            this.panel2DGrid.Name = "panel2DGrid";
            this.panel2DGrid.Room = null;
            this.panel2DGrid.Size = new System.Drawing.Size(224, 225);
            this.panel2DGrid.TabIndex = 103;
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.butCeiling);
            this.panelRight.Controls.Add(this.butClimbPositiveZ);
            this.panelRight.Controls.Add(this.butClimbPositiveX);
            this.panelRight.Controls.Add(this.butClimbNegativeZ);
            this.panelRight.Controls.Add(this.butClimbNegativeX);
            this.panelRight.Controls.Add(this.butNotWalkableBox);
            this.panelRight.Controls.Add(this.butPortal);
            this.panelRight.Controls.Add(this.butFlagTriggerTriggerer);
            this.panelRight.Controls.Add(this.butDeath);
            this.panelRight.Controls.Add(this.butForceSolidFloor);
            this.panelRight.Controls.Add(this.butMonkey);
            this.panelRight.Controls.Add(this.butFlagBeetle);
            this.panelRight.Controls.Add(this.butBox);
            this.panelRight.Controls.Add(this.butFloor);
            this.panelRight.Controls.Add(this.butWall);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelRight.Location = new System.Drawing.Point(226, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Padding = new System.Windows.Forms.Padding(2);
            this.panelRight.Size = new System.Drawing.Size(58, 225);
            this.panelRight.TabIndex = 109;
            // 
            // butCeiling
            // 
            this.butCeiling.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butCeiling.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.butCeiling.BackColorUseGeneric = false;
            this.butCeiling.ForeColor = System.Drawing.Color.White;
            this.butCeiling.Image = global::TombEditor.Properties.Resources.sectortype_Roof_16;
            this.butCeiling.Location = new System.Drawing.Point(32, 2);
            this.butCeiling.Name = "butCeiling";
            this.butCeiling.Size = new System.Drawing.Size(24, 24);
            this.butCeiling.TabIndex = 1;
            this.butCeiling.Tag = "SetCeiling";
            this.butCeiling.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butClimbPositiveZ
            // 
            this.butClimbPositiveZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butClimbPositiveZ.Image = global::TombEditor.Properties.Resources.sectortype_ClimbNorth_16;
            this.butClimbPositiveZ.Location = new System.Drawing.Point(3, 142);
            this.butClimbPositiveZ.Name = "butClimbPositiveZ";
            this.butClimbPositiveZ.Size = new System.Drawing.Size(24, 24);
            this.butClimbPositiveZ.TabIndex = 10;
            this.butClimbPositiveZ.Tag = "SetClimbPositiveZ";
            this.butClimbPositiveZ.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butClimbPositiveX
            // 
            this.butClimbPositiveX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butClimbPositiveX.Image = global::TombEditor.Properties.Resources.sectortype_ClimbEast_16;
            this.butClimbPositiveX.Location = new System.Drawing.Point(32, 170);
            this.butClimbPositiveX.Name = "butClimbPositiveX";
            this.butClimbPositiveX.Size = new System.Drawing.Size(24, 24);
            this.butClimbPositiveX.TabIndex = 13;
            this.butClimbPositiveX.Tag = "SetClimbPositiveX";
            this.butClimbPositiveX.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butClimbNegativeZ
            // 
            this.butClimbNegativeZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butClimbNegativeZ.Image = global::TombEditor.Properties.Resources.sectortype_ClimbSouth_1_16;
            this.butClimbNegativeZ.Location = new System.Drawing.Point(32, 142);
            this.butClimbNegativeZ.Name = "butClimbNegativeZ";
            this.butClimbNegativeZ.Size = new System.Drawing.Size(24, 24);
            this.butClimbNegativeZ.TabIndex = 11;
            this.butClimbNegativeZ.Tag = "SetClimbNegativeZ";
            this.butClimbNegativeZ.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butClimbNegativeX
            // 
            this.butClimbNegativeX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butClimbNegativeX.Image = global::TombEditor.Properties.Resources.sectortype_ClimbWest_16;
            this.butClimbNegativeX.Location = new System.Drawing.Point(3, 170);
            this.butClimbNegativeX.Name = "butClimbNegativeX";
            this.butClimbNegativeX.Size = new System.Drawing.Size(24, 24);
            this.butClimbNegativeX.TabIndex = 12;
            this.butClimbNegativeX.Tag = "SetClimbNegativeX";
            this.butClimbNegativeX.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butNotWalkableBox
            // 
            this.butNotWalkableBox.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butNotWalkableBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(150)))));
            this.butNotWalkableBox.BackColorUseGeneric = false;
            this.butNotWalkableBox.ForeColor = System.Drawing.Color.White;
            this.butNotWalkableBox.Image = global::TombEditor.Properties.Resources.sectortype_NotWalkable_16;
            this.butNotWalkableBox.Location = new System.Drawing.Point(32, 30);
            this.butNotWalkableBox.Name = "butNotWalkableBox";
            this.butNotWalkableBox.Size = new System.Drawing.Size(24, 24);
            this.butNotWalkableBox.TabIndex = 3;
            this.butNotWalkableBox.Tag = "SetNotWalkable";
            this.butNotWalkableBox.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butPortal
            // 
            this.butPortal.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butPortal.BackColor = System.Drawing.Color.Black;
            this.butPortal.BackColorUseGeneric = false;
            this.butPortal.ForeColor = System.Drawing.Color.White;
            this.butPortal.Image = global::TombEditor.Properties.Resources.sectortype_Portal__16;
            this.butPortal.Location = new System.Drawing.Point(3, 86);
            this.butPortal.Name = "butPortal";
            this.butPortal.Size = new System.Drawing.Size(24, 24);
            this.butPortal.TabIndex = 6;
            this.butPortal.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butFlagTriggerTriggerer
            // 
            this.butFlagTriggerTriggerer.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butFlagTriggerTriggerer.Image = global::TombEditor.Properties.Resources.sectortype_TriggerTriggerer_16;
            this.butFlagTriggerTriggerer.Location = new System.Drawing.Point(32, 114);
            this.butFlagTriggerTriggerer.Name = "butFlagTriggerTriggerer";
            this.butFlagTriggerTriggerer.Size = new System.Drawing.Size(24, 24);
            this.butFlagTriggerTriggerer.TabIndex = 9;
            this.butFlagTriggerTriggerer.Tag = "SetTriggerTriggerer";
            this.butFlagTriggerTriggerer.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butDeath
            // 
            this.butDeath.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butDeath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(200)))), ((int)(((byte)(0)))));
            this.butDeath.BackColorUseGeneric = false;
            this.butDeath.ForeColor = System.Drawing.Color.White;
            this.butDeath.Image = global::TombEditor.Properties.Resources.sectortype_Death_16;
            this.butDeath.Location = new System.Drawing.Point(32, 58);
            this.butDeath.Name = "butDeath";
            this.butDeath.Size = new System.Drawing.Size(24, 24);
            this.butDeath.TabIndex = 5;
            this.butDeath.Tag = "SetDeath";
            this.butDeath.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butForceSolidFloor
            // 
            this.butForceSolidFloor.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butForceSolidFloor.Image = global::TombEditor.Properties.Resources.sectortype_ForceSolidFloor_16_copy;
            this.butForceSolidFloor.Location = new System.Drawing.Point(3, 198);
            this.butForceSolidFloor.Name = "butForceSolidFloor";
            this.butForceSolidFloor.Size = new System.Drawing.Size(53, 25);
            this.butForceSolidFloor.TabIndex = 14;
            this.butForceSolidFloor.Tag = "ToggleForceFloorSolid";
            this.butForceSolidFloor.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butMonkey
            // 
            this.butMonkey.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butMonkey.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.butMonkey.BackColorUseGeneric = false;
            this.butMonkey.ForeColor = System.Drawing.Color.White;
            this.butMonkey.Image = global::TombEditor.Properties.Resources.sectortype_Monkey_16;
            this.butMonkey.Location = new System.Drawing.Point(3, 58);
            this.butMonkey.Name = "butMonkey";
            this.butMonkey.Size = new System.Drawing.Size(24, 24);
            this.butMonkey.TabIndex = 4;
            this.butMonkey.Tag = "SetMonkeyswing";
            this.butMonkey.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butFlagBeetle
            // 
            this.butFlagBeetle.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butFlagBeetle.Image = global::TombEditor.Properties.Resources.sectortype_Beetle_16;
            this.butFlagBeetle.Location = new System.Drawing.Point(3, 114);
            this.butFlagBeetle.Name = "butFlagBeetle";
            this.butFlagBeetle.Size = new System.Drawing.Size(24, 24);
            this.butFlagBeetle.TabIndex = 8;
            this.butFlagBeetle.Tag = "SetBeetleCheckpoint";
            this.butFlagBeetle.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butBox
            // 
            this.butBox.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.butBox.BackColorUseGeneric = false;
            this.butBox.ForeColor = System.Drawing.Color.White;
            this.butBox.Image = global::TombEditor.Properties.Resources.sectortype_Box_16;
            this.butBox.Location = new System.Drawing.Point(3, 30);
            this.butBox.Name = "butBox";
            this.butBox.Size = new System.Drawing.Size(24, 24);
            this.butBox.TabIndex = 2;
            this.butBox.Tag = "SetBox";
            this.butBox.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butFloor
            // 
            this.butFloor.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butFloor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.butFloor.BackColorUseGeneric = false;
            this.butFloor.ForeColor = System.Drawing.Color.White;
            this.butFloor.Image = global::TombEditor.Properties.Resources.sectortype_Floor_1_16;
            this.butFloor.Location = new System.Drawing.Point(3, 2);
            this.butFloor.Name = "butFloor";
            this.butFloor.Size = new System.Drawing.Size(24, 24);
            this.butFloor.TabIndex = 0;
            this.butFloor.Tag = "SetFloor";
            this.butFloor.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // butWall
            // 
            this.butWall.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.butWall.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(0)))));
            this.butWall.BackColorUseGeneric = false;
            this.butWall.ForeColor = System.Drawing.Color.White;
            this.butWall.Image = global::TombEditor.Properties.Resources.sectortype_Wall_1_16;
            this.butWall.Location = new System.Drawing.Point(32, 86);
            this.butWall.Name = "butWall";
            this.butWall.Size = new System.Drawing.Size(24, 24);
            this.butWall.TabIndex = 7;
            this.butWall.Tag = "SetWall";
            this.butWall.MouseEnter += new System.EventHandler(this.but_MouseEnter);
            // 
            // panel2DGrid_sub
            // 
            this.panel2DGrid_sub.Controls.Add(this.panel2DGrid);
            this.panel2DGrid_sub.Controls.Add(this.panelRight);
            this.panel2DGrid_sub.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2DGrid_sub.Location = new System.Drawing.Point(0, 25);
            this.panel2DGrid_sub.Name = "panel2DGrid_sub";
            this.panel2DGrid_sub.Padding = new System.Windows.Forms.Padding(2, 0, 0, 4);
            this.panel2DGrid_sub.Size = new System.Drawing.Size(284, 229);
            this.panel2DGrid_sub.TabIndex = 111;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.tableLayoutPanel1);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 254);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(0);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(284, 26);
            this.panelBottom.TabIndex = 110;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.Controls.Add(this.butDiagonalWall, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.butDiagonalFloor, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.butDiagonalCeiling, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(284, 26);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // butDiagonalWall
            // 
            this.butDiagonalWall.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butDiagonalWall.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butDiagonalWall.Image = global::TombEditor.Properties.Resources.sectortype_DiagonalWall2_16;
            this.butDiagonalWall.Location = new System.Drawing.Point(190, 0);
            this.butDiagonalWall.Margin = new System.Windows.Forms.Padding(2, 0, 2, 2);
            this.butDiagonalWall.Name = "butDiagonalWall";
            this.butDiagonalWall.Size = new System.Drawing.Size(92, 24);
            this.butDiagonalWall.TabIndex = 17;
            this.butDiagonalWall.Tag = "SetDiagonalWall";
            this.butDiagonalWall.Text = "Diag wall";
            this.butDiagonalWall.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // butDiagonalFloor
            // 
            this.butDiagonalFloor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butDiagonalFloor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butDiagonalFloor.Image = global::TombEditor.Properties.Resources.sectortype_StepFloor_1_16;
            this.butDiagonalFloor.Location = new System.Drawing.Point(2, 0);
            this.butDiagonalFloor.Margin = new System.Windows.Forms.Padding(2, 0, 2, 2);
            this.butDiagonalFloor.Name = "butDiagonalFloor";
            this.butDiagonalFloor.Size = new System.Drawing.Size(90, 24);
            this.butDiagonalFloor.TabIndex = 15;
            this.butDiagonalFloor.Tag = "SetDiagonalFloorStep";
            this.butDiagonalFloor.Text = "Floor step";
            this.butDiagonalFloor.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // butDiagonalCeiling
            // 
            this.butDiagonalCeiling.Dock = System.Windows.Forms.DockStyle.Fill;
            this.butDiagonalCeiling.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butDiagonalCeiling.Image = global::TombEditor.Properties.Resources.sectortype_StepCeiling_16;
            this.butDiagonalCeiling.Location = new System.Drawing.Point(96, 0);
            this.butDiagonalCeiling.Margin = new System.Windows.Forms.Padding(2, 0, 2, 2);
            this.butDiagonalCeiling.Name = "butDiagonalCeiling";
            this.butDiagonalCeiling.Size = new System.Drawing.Size(90, 24);
            this.butDiagonalCeiling.TabIndex = 16;
            this.butDiagonalCeiling.Tag = "SetDiagonalCeilingStep";
            this.butDiagonalCeiling.Text = "Ceiling step";
            this.butDiagonalCeiling.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // SectorOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.panel2DGrid_sub);
            this.Controls.Add(this.panelBottom);
            this.DockText = "Sector Options";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(284, 280);
            this.Name = "SectorOptions";
            this.SerializationKey = "SectorOptions";
            this.Size = new System.Drawing.Size(284, 280);
            this.panelRight.ResumeLayout(false);
            this.panel2DGrid_sub.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private Controls.Panel2DGrid panel2DGrid;
        private System.Windows.Forms.Panel panelRight;
        private DarkUI.Controls.DarkButton butCeiling;
        private DarkUI.Controls.DarkButton butClimbPositiveZ;
        private DarkUI.Controls.DarkButton butClimbPositiveX;
        private DarkUI.Controls.DarkButton butClimbNegativeZ;
        private DarkUI.Controls.DarkButton butClimbNegativeX;
        private DarkUI.Controls.DarkButton butNotWalkableBox;
        private DarkUI.Controls.DarkButton butPortal;
        private DarkUI.Controls.DarkButton butFlagTriggerTriggerer;
        private DarkUI.Controls.DarkButton butDeath;
        private DarkUI.Controls.DarkButton butForceSolidFloor;
        private DarkUI.Controls.DarkButton butMonkey;
        private DarkUI.Controls.DarkButton butFlagBeetle;
        private DarkUI.Controls.DarkButton butBox;
        private DarkUI.Controls.DarkButton butFloor;
        private DarkUI.Controls.DarkButton butWall;
        private System.Windows.Forms.Panel panel2DGrid_sub;
        private DarkUI.Controls.DarkButton butDiagonalWall;
        private DarkUI.Controls.DarkButton butDiagonalCeiling;
        private DarkUI.Controls.DarkButton butDiagonalFloor;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.ToolTip toolTip;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}
