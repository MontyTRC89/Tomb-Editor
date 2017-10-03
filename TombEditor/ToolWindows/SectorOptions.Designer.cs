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
            this.butCeiling = new System.Windows.Forms.Button();
            this.butClimbPositiveZ = new DarkUI.Controls.DarkButton();
            this.butClimbPositiveX = new DarkUI.Controls.DarkButton();
            this.butClimbNegativeZ = new DarkUI.Controls.DarkButton();
            this.butClimbNegativeX = new DarkUI.Controls.DarkButton();
            this.butNotWalkableBox = new System.Windows.Forms.Button();
            this.butPortal = new System.Windows.Forms.Button();
            this.butFlagTriggerTriggerer = new DarkUI.Controls.DarkButton();
            this.butDeath = new System.Windows.Forms.Button();
            this.butForceSolidFloor = new DarkUI.Controls.DarkButton();
            this.butMonkey = new System.Windows.Forms.Button();
            this.butFlagBeetle = new DarkUI.Controls.DarkButton();
            this.butBox = new System.Windows.Forms.Button();
            this.butFloor = new System.Windows.Forms.Button();
            this.butWall = new System.Windows.Forms.Button();
            this.panel2DGrid_sub = new System.Windows.Forms.Panel();
            this.butDiagonalWall = new DarkUI.Controls.DarkButton();
            this.butDiagonalCeiling = new DarkUI.Controls.DarkButton();
            this.butDiagonalFloor = new DarkUI.Controls.DarkButton();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.panelRight.SuspendLayout();
            this.panel2DGrid_sub.SuspendLayout();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2DGrid
            // 
            this.panel2DGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2DGrid.Location = new System.Drawing.Point(2, 0);
            this.panel2DGrid.Name = "panel2DGrid";
            this.panel2DGrid.Size = new System.Drawing.Size(295, 295);
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
            this.butCeiling.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(190)))), ((int)(((byte)(190)))));
            this.butCeiling.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.butCeiling.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(95)))), ((int)(((byte)(95)))));
            this.butCeiling.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(142)))), ((int)(((byte)(142)))));
            this.butCeiling.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butCeiling.ForeColor = System.Drawing.Color.White;
            this.butCeiling.Location = new System.Drawing.Point(32, 0);
            this.butCeiling.Name = "butCeiling";
            this.butCeiling.Size = new System.Drawing.Size(24, 24);
            this.butCeiling.TabIndex = 105;
            this.butCeiling.Text = "C";
            this.toolTip.SetToolTip(this.butCeiling, "Set sector ceiling");
            this.butCeiling.UseVisualStyleBackColor = false;
            this.butCeiling.Click += new System.EventHandler(this.butCeiling_Click);
            // 
            // butClimbPositiveZ
            // 
            this.butClimbPositiveZ.Image = global::TombEditor.Properties.Resources.climb_north;
            this.butClimbPositiveZ.Location = new System.Drawing.Point(3, 140);
            this.butClimbPositiveZ.Name = "butClimbPositiveZ";
            this.butClimbPositiveZ.Padding = new System.Windows.Forms.Padding(5);
            this.butClimbPositiveZ.Size = new System.Drawing.Size(24, 24);
            this.butClimbPositiveZ.TabIndex = 99;
            this.toolTip.SetToolTip(this.butClimbPositiveZ, "Climb on North side");
            this.butClimbPositiveZ.Click += new System.EventHandler(this.butClimbPositiveZ_Click);
            // 
            // butClimbPositiveX
            // 
            this.butClimbPositiveX.Image = global::TombEditor.Properties.Resources.climb_east;
            this.butClimbPositiveX.Location = new System.Drawing.Point(32, 168);
            this.butClimbPositiveX.Name = "butClimbPositiveX";
            this.butClimbPositiveX.Padding = new System.Windows.Forms.Padding(5);
            this.butClimbPositiveX.Size = new System.Drawing.Size(24, 24);
            this.butClimbPositiveX.TabIndex = 98;
            this.toolTip.SetToolTip(this.butClimbPositiveX, "Climb on East side");
            this.butClimbPositiveX.Click += new System.EventHandler(this.butClimbPositiveX_Click);
            // 
            // butClimbNegativeZ
            // 
            this.butClimbNegativeZ.Image = global::TombEditor.Properties.Resources.climb_south;
            this.butClimbNegativeZ.Location = new System.Drawing.Point(32, 140);
            this.butClimbNegativeZ.Name = "butClimbNegativeZ";
            this.butClimbNegativeZ.Padding = new System.Windows.Forms.Padding(5);
            this.butClimbNegativeZ.Size = new System.Drawing.Size(24, 24);
            this.butClimbNegativeZ.TabIndex = 97;
            this.toolTip.SetToolTip(this.butClimbNegativeZ, "Climb on South side");
            this.butClimbNegativeZ.Click += new System.EventHandler(this.butClimbNegativeZ_Click);
            // 
            // butClimbNegativeX
            // 
            this.butClimbNegativeX.Image = global::TombEditor.Properties.Resources.climb_west;
            this.butClimbNegativeX.Location = new System.Drawing.Point(3, 168);
            this.butClimbNegativeX.Name = "butClimbNegativeX";
            this.butClimbNegativeX.Padding = new System.Windows.Forms.Padding(5);
            this.butClimbNegativeX.Size = new System.Drawing.Size(24, 24);
            this.butClimbNegativeX.TabIndex = 96;
            this.toolTip.SetToolTip(this.butClimbNegativeX, "Climb on West side");
            this.butClimbNegativeX.Click += new System.EventHandler(this.butClimbNegativeX_Click);
            // 
            // butNotWalkableBox
            // 
            this.butNotWalkableBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(150)))));
            this.butNotWalkableBox.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.butNotWalkableBox.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(75)))));
            this.butNotWalkableBox.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(112)))));
            this.butNotWalkableBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butNotWalkableBox.ForeColor = System.Drawing.Color.White;
            this.butNotWalkableBox.Location = new System.Drawing.Point(32, 28);
            this.butNotWalkableBox.Name = "butNotWalkableBox";
            this.butNotWalkableBox.Size = new System.Drawing.Size(24, 24);
            this.butNotWalkableBox.TabIndex = 104;
            this.butNotWalkableBox.Text = "N";
            this.toolTip.SetToolTip(this.butNotWalkableBox, "Not walkable");
            this.butNotWalkableBox.UseVisualStyleBackColor = false;
            this.butNotWalkableBox.Click += new System.EventHandler(this.butNotWalkableBox_Click);
            // 
            // butPortal
            // 
            this.butPortal.BackColor = System.Drawing.Color.Black;
            this.butPortal.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.butPortal.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Black;
            this.butPortal.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Black;
            this.butPortal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butPortal.ForeColor = System.Drawing.Color.White;
            this.butPortal.Location = new System.Drawing.Point(3, 84);
            this.butPortal.Name = "butPortal";
            this.butPortal.Size = new System.Drawing.Size(24, 24);
            this.butPortal.TabIndex = 95;
            this.butPortal.Text = "P";
            this.toolTip.SetToolTip(this.butPortal, "Portal");
            this.butPortal.UseVisualStyleBackColor = false;
            this.butPortal.Click += new System.EventHandler(this.butPortal_Click);
            // 
            // butFlagTriggerTriggerer
            // 
            this.butFlagTriggerTriggerer.Location = new System.Drawing.Point(32, 112);
            this.butFlagTriggerTriggerer.Name = "butFlagTriggerTriggerer";
            this.butFlagTriggerTriggerer.Padding = new System.Windows.Forms.Padding(5);
            this.butFlagTriggerTriggerer.Size = new System.Drawing.Size(24, 24);
            this.butFlagTriggerTriggerer.TabIndex = 102;
            this.butFlagTriggerTriggerer.Text = "T";
            this.toolTip.SetToolTip(this.butFlagTriggerTriggerer, "Delay trigger until Trigger Triggerer is used");
            this.butFlagTriggerTriggerer.Click += new System.EventHandler(this.butFlagTriggerTriggerer_Click);
            // 
            // butDeath
            // 
            this.butDeath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(240)))), ((int)(((byte)(0)))));
            this.butDeath.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.butDeath.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(0)))));
            this.butDeath.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(180)))), ((int)(((byte)(0)))));
            this.butDeath.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butDeath.ForeColor = System.Drawing.Color.White;
            this.butDeath.Location = new System.Drawing.Point(32, 56);
            this.butDeath.Name = "butDeath";
            this.butDeath.Size = new System.Drawing.Size(24, 24);
            this.butDeath.TabIndex = 94;
            this.butDeath.Text = "D";
            this.toolTip.SetToolTip(this.butDeath, "Death");
            this.butDeath.UseVisualStyleBackColor = false;
            this.butDeath.Click += new System.EventHandler(this.butDeath_Click);
            // 
            // butForceSolidFloor
            // 
            this.butForceSolidFloor.Location = new System.Drawing.Point(3, 196);
            this.butForceSolidFloor.Name = "butForceSolidFloor";
            this.butForceSolidFloor.Padding = new System.Windows.Forms.Padding(5);
            this.butForceSolidFloor.Size = new System.Drawing.Size(24, 24);
            this.butForceSolidFloor.TabIndex = 101;
            this.butForceSolidFloor.Text = "Ff";
            this.toolTip.SetToolTip(this.butForceSolidFloor, "Force solid floor");
            this.butForceSolidFloor.Click += new System.EventHandler(this.butForceSolidFloor_Click);
            // 
            // butMonkey
            // 
            this.butMonkey.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.butMonkey.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.butMonkey.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.butMonkey.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(196)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.butMonkey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butMonkey.ForeColor = System.Drawing.Color.White;
            this.butMonkey.Location = new System.Drawing.Point(3, 56);
            this.butMonkey.Name = "butMonkey";
            this.butMonkey.Size = new System.Drawing.Size(24, 24);
            this.butMonkey.TabIndex = 93;
            this.butMonkey.Text = "M";
            this.toolTip.SetToolTip(this.butMonkey, "Monkeyswing");
            this.butMonkey.UseVisualStyleBackColor = false;
            this.butMonkey.Click += new System.EventHandler(this.butMonkey_Click);
            // 
            // butFlagBeetle
            // 
            this.butFlagBeetle.Location = new System.Drawing.Point(3, 112);
            this.butFlagBeetle.Name = "butFlagBeetle";
            this.butFlagBeetle.Padding = new System.Windows.Forms.Padding(5);
            this.butFlagBeetle.Size = new System.Drawing.Size(24, 24);
            this.butFlagBeetle.TabIndex = 100;
            this.butFlagBeetle.Text = "B";
            this.toolTip.SetToolTip(this.butFlagBeetle, "Beetle checkpoint");
            this.butFlagBeetle.Click += new System.EventHandler(this.butFlagBeetle_Click);
            // 
            // butBox
            // 
            this.butBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.butBox.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.butBox.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.butBox.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(75)))), ((int)(((byte)(75)))));
            this.butBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butBox.ForeColor = System.Drawing.Color.White;
            this.butBox.Location = new System.Drawing.Point(3, 28);
            this.butBox.Name = "butBox";
            this.butBox.Size = new System.Drawing.Size(24, 24);
            this.butBox.TabIndex = 92;
            this.butBox.Text = "B";
            this.butBox.UseVisualStyleBackColor = false;
            this.butBox.Click += new System.EventHandler(this.butBox_Click);
            // 
            // butFloor
            // 
            this.butFloor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(190)))), ((int)(((byte)(190)))));
            this.butFloor.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.butFloor.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(95)))), ((int)(((byte)(95)))));
            this.butFloor.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(142)))), ((int)(((byte)(142)))));
            this.butFloor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butFloor.ForeColor = System.Drawing.Color.White;
            this.butFloor.Location = new System.Drawing.Point(3, 0);
            this.butFloor.Name = "butFloor";
            this.butFloor.Size = new System.Drawing.Size(24, 24);
            this.butFloor.TabIndex = 90;
            this.butFloor.Text = "F";
            this.toolTip.SetToolTip(this.butFloor, "Set sector floor");
            this.butFloor.UseVisualStyleBackColor = false;
            this.butFloor.Click += new System.EventHandler(this.butFloor_Click);
            // 
            // butWall
            // 
            this.butWall.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(160)))), ((int)(((byte)(0)))));
            this.butWall.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
            this.butWall.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(80)))), ((int)(((byte)(0)))));
            this.butWall.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(0)))));
            this.butWall.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.butWall.ForeColor = System.Drawing.Color.White;
            this.butWall.Location = new System.Drawing.Point(32, 84);
            this.butWall.Name = "butWall";
            this.butWall.Size = new System.Drawing.Size(24, 24);
            this.butWall.TabIndex = 91;
            this.butWall.Text = "W";
            this.toolTip.SetToolTip(this.butWall, "Wall");
            this.butWall.UseVisualStyleBackColor = false;
            this.butWall.Click += new System.EventHandler(this.butWall_Click);
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
            // butDiagonalWall
            // 
            this.butDiagonalWall.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butDiagonalWall.Image = global::TombEditor.Properties.Resources.diagonal_wall_SE;
            this.butDiagonalWall.Location = new System.Drawing.Point(192, 0);
            this.butDiagonalWall.Name = "butDiagonalWall";
            this.butDiagonalWall.Padding = new System.Windows.Forms.Padding(5);
            this.butDiagonalWall.Size = new System.Drawing.Size(88, 24);
            this.butDiagonalWall.TabIndex = 108;
            this.butDiagonalWall.Text = "Diag. Wall";
            this.butDiagonalWall.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDiagonalWall.Click += new System.EventHandler(this.butDiagonalWall_Click);
            // 
            // butDiagonalCeiling
            // 
            this.butDiagonalCeiling.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butDiagonalCeiling.Image = global::TombEditor.Properties.Resources.diagonal_floor_SE;
            this.butDiagonalCeiling.Location = new System.Drawing.Point(97, 0);
            this.butDiagonalCeiling.Name = "butDiagonalCeiling";
            this.butDiagonalCeiling.Padding = new System.Windows.Forms.Padding(5);
            this.butDiagonalCeiling.Size = new System.Drawing.Size(89, 24);
            this.butDiagonalCeiling.TabIndex = 107;
            this.butDiagonalCeiling.Text = "Diag. CE";
            this.butDiagonalCeiling.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDiagonalCeiling.Click += new System.EventHandler(this.butDiagonalCeiling_Click);
            // 
            // butDiagonalFloor
            // 
            this.butDiagonalFloor.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butDiagonalFloor.Image = global::TombEditor.Properties.Resources.diagonal_floor_SE;
            this.butDiagonalFloor.Location = new System.Drawing.Point(2, 0);
            this.butDiagonalFloor.Name = "butDiagonalFloor";
            this.butDiagonalFloor.Padding = new System.Windows.Forms.Padding(5);
            this.butDiagonalFloor.Size = new System.Drawing.Size(89, 24);
            this.butDiagonalFloor.TabIndex = 106;
            this.butDiagonalFloor.Text = "Diag. FL";
            this.butDiagonalFloor.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.butDiagonalFloor.Click += new System.EventHandler(this.butDiagonalFloor_Click);
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.butDiagonalFloor);
            this.panelBottom.Controls.Add(this.butDiagonalWall);
            this.panelBottom.Controls.Add(this.butDiagonalCeiling);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 254);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(3);
            this.panelBottom.Size = new System.Drawing.Size(284, 26);
            this.panelBottom.TabIndex = 110;
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
            this.ResumeLayout(false);

        }

        #endregion
        private Controls.Panel2DGrid panel2DGrid;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Button butCeiling;
        private DarkUI.Controls.DarkButton butClimbPositiveZ;
        private DarkUI.Controls.DarkButton butClimbPositiveX;
        private DarkUI.Controls.DarkButton butClimbNegativeZ;
        private DarkUI.Controls.DarkButton butClimbNegativeX;
        private System.Windows.Forms.Button butNotWalkableBox;
        private System.Windows.Forms.Button butPortal;
        private DarkUI.Controls.DarkButton butFlagTriggerTriggerer;
        private System.Windows.Forms.Button butDeath;
        private DarkUI.Controls.DarkButton butForceSolidFloor;
        private System.Windows.Forms.Button butMonkey;
        private DarkUI.Controls.DarkButton butFlagBeetle;
        private System.Windows.Forms.Button butBox;
        private System.Windows.Forms.Button butFloor;
        private System.Windows.Forms.Button butWall;
        private System.Windows.Forms.Panel panel2DGrid_sub;
        private DarkUI.Controls.DarkButton butDiagonalWall;
        private DarkUI.Controls.DarkButton butDiagonalCeiling;
        private DarkUI.Controls.DarkButton butDiagonalFloor;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
