namespace TombLib.Scripting.Forms
{
	partial class FormFindReplace
	{
		private System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		private void InitializeComponent()
		{
			this.button_Find = new DarkUI.Controls.DarkButton();
			this.button_FindAll = new DarkUI.Controls.DarkButton();
			this.button_FindNext = new DarkUI.Controls.DarkButton();
			this.button_FindPrev = new DarkUI.Controls.DarkButton();
			this.button_Replace = new DarkUI.Controls.DarkButton();
			this.button_ReplaceAll = new DarkUI.Controls.DarkButton();
			this.button_ReplaceNext = new DarkUI.Controls.DarkButton();
			this.button_ReplacePrev = new DarkUI.Controls.DarkButton();
			this.checkBox_CaseSensitive = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_WholeWords = new DarkUI.Controls.DarkCheckBox();
			this.groupBox_Direction = new DarkUI.Controls.DarkGroupBox();
			this.radioButton_Down = new DarkUI.Controls.DarkRadioButton();
			this.radioButton_Up = new DarkUI.Controls.DarkRadioButton();
			this.groupBox_Mode = new DarkUI.Controls.DarkGroupBox();
			this.radioButton_Regex = new DarkUI.Controls.DarkRadioButton();
			this.radioButton_Normal = new DarkUI.Controls.DarkRadioButton();
			this.groupBox_Options = new DarkUI.Controls.DarkGroupBox();
			this.groupBox_WhereToLook = new DarkUI.Controls.DarkGroupBox();
			this.radioButton_AllTabs = new DarkUI.Controls.DarkRadioButton();
			this.radioButton_Current = new DarkUI.Controls.DarkRadioButton();
			this.label_Find = new DarkUI.Controls.DarkLabel();
			this.label_Replace = new DarkUI.Controls.DarkLabel();
			this.label_Status = new System.Windows.Forms.ToolStripStatusLabel();
			this.panel_All = new System.Windows.Forms.Panel();
			this.panel_Main = new System.Windows.Forms.Panel();
			this.statusStrip = new DarkUI.Controls.DarkStatusStrip();
			this.textBox_Replace = new DarkUI.Controls.DarkTextBox();
			this.textBox_Find = new DarkUI.Controls.DarkTextBox();
			this.groupBox_Direction.SuspendLayout();
			this.groupBox_Mode.SuspendLayout();
			this.groupBox_Options.SuspendLayout();
			this.groupBox_WhereToLook.SuspendLayout();
			this.panel_All.SuspendLayout();
			this.panel_Main.SuspendLayout();
			this.statusStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// button_Find
			// 
			this.button_Find.Checked = false;
			this.button_Find.Location = new System.Drawing.Point(352, 8);
			this.button_Find.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
			this.button_Find.Name = "button_Find";
			this.button_Find.Size = new System.Drawing.Size(74, 22);
			this.button_Find.TabIndex = 3;
			this.button_Find.Text = "Find";
			this.button_Find.Click += new System.EventHandler(this.button_Find_Click);
			// 
			// button_FindAll
			// 
			this.button_FindAll.Checked = false;
			this.button_FindAll.Location = new System.Drawing.Point(6, 6);
			this.button_FindAll.Margin = new System.Windows.Forms.Padding(6, 6, 3, 6);
			this.button_FindAll.Name = "button_FindAll";
			this.button_FindAll.Size = new System.Drawing.Size(137, 31);
			this.button_FindAll.TabIndex = 1;
			this.button_FindAll.Text = "Find All";
			this.button_FindAll.Click += new System.EventHandler(this.button_FindAll_Click);
			// 
			// button_FindNext
			// 
			this.button_FindNext.Checked = false;
			this.button_FindNext.Location = new System.Drawing.Point(432, 8);
			this.button_FindNext.Margin = new System.Windows.Forms.Padding(3, 0, 0, 6);
			this.button_FindNext.Name = "button_FindNext";
			this.button_FindNext.Size = new System.Drawing.Size(22, 22);
			this.button_FindNext.TabIndex = 4;
			this.button_FindNext.Text = ">";
			this.button_FindNext.Click += new System.EventHandler(this.button_FindNext_Click);
			// 
			// button_FindPrev
			// 
			this.button_FindPrev.Checked = false;
			this.button_FindPrev.Location = new System.Drawing.Point(324, 8);
			this.button_FindPrev.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
			this.button_FindPrev.Name = "button_FindPrev";
			this.button_FindPrev.Size = new System.Drawing.Size(22, 22);
			this.button_FindPrev.TabIndex = 2;
			this.button_FindPrev.Text = "<";
			this.button_FindPrev.Click += new System.EventHandler(this.button_FindPrev_Click);
			// 
			// button_Replace
			// 
			this.button_Replace.Checked = false;
			this.button_Replace.Location = new System.Drawing.Point(352, 39);
			this.button_Replace.Name = "button_Replace";
			this.button_Replace.Size = new System.Drawing.Size(74, 22);
			this.button_Replace.TabIndex = 8;
			this.button_Replace.Text = "Replace";
			this.button_Replace.Click += new System.EventHandler(this.button_Replace_Click);
			// 
			// button_ReplaceAll
			// 
			this.button_ReplaceAll.Checked = false;
			this.button_ReplaceAll.Location = new System.Drawing.Point(150, 6);
			this.button_ReplaceAll.Margin = new System.Windows.Forms.Padding(3, 6, 6, 6);
			this.button_ReplaceAll.Name = "button_ReplaceAll";
			this.button_ReplaceAll.Size = new System.Drawing.Size(137, 31);
			this.button_ReplaceAll.TabIndex = 2;
			this.button_ReplaceAll.Text = "Replace All";
			this.button_ReplaceAll.Click += new System.EventHandler(this.button_ReplaceAll_Click);
			// 
			// button_ReplaceNext
			// 
			this.button_ReplaceNext.Checked = false;
			this.button_ReplaceNext.Location = new System.Drawing.Point(432, 39);
			this.button_ReplaceNext.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
			this.button_ReplaceNext.Name = "button_ReplaceNext";
			this.button_ReplaceNext.Size = new System.Drawing.Size(22, 22);
			this.button_ReplaceNext.TabIndex = 9;
			this.button_ReplaceNext.Text = ">";
			this.button_ReplaceNext.Click += new System.EventHandler(this.button_ReplaceNext_Click);
			// 
			// button_ReplacePrev
			// 
			this.button_ReplacePrev.Checked = false;
			this.button_ReplacePrev.Location = new System.Drawing.Point(324, 39);
			this.button_ReplacePrev.Name = "button_ReplacePrev";
			this.button_ReplacePrev.Size = new System.Drawing.Size(22, 22);
			this.button_ReplacePrev.TabIndex = 7;
			this.button_ReplacePrev.Text = "<";
			this.button_ReplacePrev.Click += new System.EventHandler(this.button_ReplacePrev_Click);
			// 
			// checkBox_CaseSensitive
			// 
			this.checkBox_CaseSensitive.AutoSize = true;
			this.checkBox_CaseSensitive.Location = new System.Drawing.Point(6, 19);
			this.checkBox_CaseSensitive.Name = "checkBox_CaseSensitive";
			this.checkBox_CaseSensitive.Size = new System.Drawing.Size(96, 17);
			this.checkBox_CaseSensitive.TabIndex = 0;
			this.checkBox_CaseSensitive.Text = "Case Sensitive";
			// 
			// checkBox_WholeWords
			// 
			this.checkBox_WholeWords.AutoSize = true;
			this.checkBox_WholeWords.Location = new System.Drawing.Point(6, 42);
			this.checkBox_WholeWords.Name = "checkBox_WholeWords";
			this.checkBox_WholeWords.Size = new System.Drawing.Size(124, 17);
			this.checkBox_WholeWords.TabIndex = 1;
			this.checkBox_WholeWords.Text = "Match Whole Words";
			// 
			// groupBox_Direction
			// 
			this.groupBox_Direction.Controls.Add(this.radioButton_Down);
			this.groupBox_Direction.Controls.Add(this.radioButton_Up);
			this.groupBox_Direction.Location = new System.Drawing.Point(8, 70);
			this.groupBox_Direction.Margin = new System.Windows.Forms.Padding(0, 6, 3, 3);
			this.groupBox_Direction.Name = "groupBox_Direction";
			this.groupBox_Direction.Size = new System.Drawing.Size(144, 39);
			this.groupBox_Direction.TabIndex = 11;
			this.groupBox_Direction.TabStop = false;
			this.groupBox_Direction.Text = "Search Direction";
			// 
			// radioButton_Down
			// 
			this.radioButton_Down.AutoSize = true;
			this.radioButton_Down.Checked = true;
			this.radioButton_Down.Location = new System.Drawing.Point(51, 16);
			this.radioButton_Down.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.radioButton_Down.Name = "radioButton_Down";
			this.radioButton_Down.Size = new System.Drawing.Size(53, 17);
			this.radioButton_Down.TabIndex = 1;
			this.radioButton_Down.TabStop = true;
			this.radioButton_Down.Text = "Down";
			// 
			// radioButton_Up
			// 
			this.radioButton_Up.AutoSize = true;
			this.radioButton_Up.Location = new System.Drawing.Point(6, 16);
			this.radioButton_Up.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
			this.radioButton_Up.Name = "radioButton_Up";
			this.radioButton_Up.Size = new System.Drawing.Size(39, 17);
			this.radioButton_Up.TabIndex = 0;
			this.radioButton_Up.TabStop = true;
			this.radioButton_Up.Text = "Up";
			// 
			// groupBox_Mode
			// 
			this.groupBox_Mode.Controls.Add(this.radioButton_Regex);
			this.groupBox_Mode.Controls.Add(this.radioButton_Normal);
			this.groupBox_Mode.Location = new System.Drawing.Point(159, 115);
			this.groupBox_Mode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 6);
			this.groupBox_Mode.Name = "groupBox_Mode";
			this.groupBox_Mode.Size = new System.Drawing.Size(144, 65);
			this.groupBox_Mode.TabIndex = 13;
			this.groupBox_Mode.TabStop = false;
			this.groupBox_Mode.Text = "Mode";
			// 
			// radioButton_Regex
			// 
			this.radioButton_Regex.AutoSize = true;
			this.radioButton_Regex.Location = new System.Drawing.Point(6, 42);
			this.radioButton_Regex.Name = "radioButton_Regex";
			this.radioButton_Regex.Size = new System.Drawing.Size(121, 17);
			this.radioButton_Regex.TabIndex = 1;
			this.radioButton_Regex.TabStop = true;
			this.radioButton_Regex.Text = "Regular Expressions";
			// 
			// radioButton_Normal
			// 
			this.radioButton_Normal.AutoSize = true;
			this.radioButton_Normal.Checked = true;
			this.radioButton_Normal.Location = new System.Drawing.Point(6, 19);
			this.radioButton_Normal.Name = "radioButton_Normal";
			this.radioButton_Normal.Size = new System.Drawing.Size(58, 17);
			this.radioButton_Normal.TabIndex = 0;
			this.radioButton_Normal.TabStop = true;
			this.radioButton_Normal.Text = "Normal";
			// 
			// groupBox_Options
			// 
			this.groupBox_Options.Controls.Add(this.checkBox_WholeWords);
			this.groupBox_Options.Controls.Add(this.checkBox_CaseSensitive);
			this.groupBox_Options.Location = new System.Drawing.Point(8, 115);
			this.groupBox_Options.Margin = new System.Windows.Forms.Padding(0, 3, 3, 6);
			this.groupBox_Options.Name = "groupBox_Options";
			this.groupBox_Options.Size = new System.Drawing.Size(144, 65);
			this.groupBox_Options.TabIndex = 12;
			this.groupBox_Options.TabStop = false;
			this.groupBox_Options.Text = "Options";
			// 
			// groupBox_WhereToLook
			// 
			this.groupBox_WhereToLook.Controls.Add(this.radioButton_AllTabs);
			this.groupBox_WhereToLook.Controls.Add(this.radioButton_Current);
			this.groupBox_WhereToLook.Location = new System.Drawing.Point(310, 115);
			this.groupBox_WhereToLook.Margin = new System.Windows.Forms.Padding(3, 3, 0, 6);
			this.groupBox_WhereToLook.Name = "groupBox_WhereToLook";
			this.groupBox_WhereToLook.Size = new System.Drawing.Size(144, 65);
			this.groupBox_WhereToLook.TabIndex = 14;
			this.groupBox_WhereToLook.TabStop = false;
			this.groupBox_WhereToLook.Text = "Where To Look";
			// 
			// radioButton_AllTabs
			// 
			this.radioButton_AllTabs.AutoSize = true;
			this.radioButton_AllTabs.Location = new System.Drawing.Point(6, 42);
			this.radioButton_AllTabs.Name = "radioButton_AllTabs";
			this.radioButton_AllTabs.Size = new System.Drawing.Size(104, 17);
			this.radioButton_AllTabs.TabIndex = 2;
			this.radioButton_AllTabs.Text = "All Opened Tabs";
			// 
			// radioButton_Current
			// 
			this.radioButton_Current.AutoSize = true;
			this.radioButton_Current.Checked = true;
			this.radioButton_Current.Location = new System.Drawing.Point(6, 19);
			this.radioButton_Current.Name = "radioButton_Current";
			this.radioButton_Current.Size = new System.Drawing.Size(111, 17);
			this.radioButton_Current.TabIndex = 0;
			this.radioButton_Current.TabStop = true;
			this.radioButton_Current.Text = "Current Document";
			// 
			// label_Find
			// 
			this.label_Find.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Find.Location = new System.Drawing.Point(8, 8);
			this.label_Find.Margin = new System.Windows.Forms.Padding(0, 0, 0, 6);
			this.label_Find.Name = "label_Find";
			this.label_Find.Size = new System.Drawing.Size(50, 22);
			this.label_Find.TabIndex = 0;
			this.label_Find.Text = "Find:";
			this.label_Find.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label_Replace
			// 
			this.label_Replace.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Replace.Location = new System.Drawing.Point(8, 39);
			this.label_Replace.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
			this.label_Replace.Name = "label_Replace";
			this.label_Replace.Size = new System.Drawing.Size(50, 22);
			this.label_Replace.TabIndex = 5;
			this.label_Replace.Text = "Replace:";
			this.label_Replace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label_Status
			// 
			this.label_Status.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.label_Status.Name = "label_Status";
			this.label_Status.Size = new System.Drawing.Size(39, 15);
			this.label_Status.Text = "Status";
			// 
			// panel_All
			// 
			this.panel_All.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_All.Controls.Add(this.button_FindAll);
			this.panel_All.Controls.Add(this.button_ReplaceAll);
			this.panel_All.Location = new System.Drawing.Point(159, 67);
			this.panel_All.Margin = new System.Windows.Forms.Padding(4, 3, 0, 0);
			this.panel_All.Name = "panel_All";
			this.panel_All.Size = new System.Drawing.Size(295, 45);
			this.panel_All.TabIndex = 10;
			// 
			// panel_Main
			// 
			this.panel_Main.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_Main.Controls.Add(this.panel_All);
			this.panel_Main.Controls.Add(this.groupBox_WhereToLook);
			this.panel_Main.Controls.Add(this.groupBox_Direction);
			this.panel_Main.Controls.Add(this.statusStrip);
			this.panel_Main.Controls.Add(this.button_Replace);
			this.panel_Main.Controls.Add(this.button_ReplaceNext);
			this.panel_Main.Controls.Add(this.button_ReplacePrev);
			this.panel_Main.Controls.Add(this.groupBox_Mode);
			this.panel_Main.Controls.Add(this.groupBox_Options);
			this.panel_Main.Controls.Add(this.textBox_Replace);
			this.panel_Main.Controls.Add(this.label_Find);
			this.panel_Main.Controls.Add(this.label_Replace);
			this.panel_Main.Controls.Add(this.textBox_Find);
			this.panel_Main.Controls.Add(this.button_Find);
			this.panel_Main.Controls.Add(this.button_FindNext);
			this.panel_Main.Controls.Add(this.button_FindPrev);
			this.panel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_Main.Location = new System.Drawing.Point(0, 0);
			this.panel_Main.Name = "panel_Main";
			this.panel_Main.Size = new System.Drawing.Size(464, 216);
			this.panel_Main.TabIndex = 13;
			// 
			// statusStrip
			// 
			this.statusStrip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
			this.statusStrip.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.label_Status});
			this.statusStrip.Location = new System.Drawing.Point(0, 186);
			this.statusStrip.Name = "statusStrip";
			this.statusStrip.Padding = new System.Windows.Forms.Padding(0, 5, 0, 3);
			this.statusStrip.Size = new System.Drawing.Size(462, 28);
			this.statusStrip.TabIndex = 15;
			// 
			// textBox_Replace
			// 
			this.textBox_Replace.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_Replace.Location = new System.Drawing.Point(58, 39);
			this.textBox_Replace.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
			this.textBox_Replace.Name = "textBox_Replace";
			this.textBox_Replace.Size = new System.Drawing.Size(260, 22);
			this.textBox_Replace.TabIndex = 6;
			// 
			// textBox_Find
			// 
			this.textBox_Find.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.textBox_Find.Location = new System.Drawing.Point(58, 8);
			this.textBox_Find.Margin = new System.Windows.Forms.Padding(0, 0, 3, 6);
			this.textBox_Find.Name = "textBox_Find";
			this.textBox_Find.Size = new System.Drawing.Size(260, 22);
			this.textBox_Find.TabIndex = 1;
			// 
			// FormFindReplace
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 216);
			this.Controls.Add(this.panel_Main);
			this.FlatBorder = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "FormFindReplace";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Find & Replace";
			this.groupBox_Direction.ResumeLayout(false);
			this.groupBox_Direction.PerformLayout();
			this.groupBox_Mode.ResumeLayout(false);
			this.groupBox_Mode.PerformLayout();
			this.groupBox_Options.ResumeLayout(false);
			this.groupBox_Options.PerformLayout();
			this.groupBox_WhereToLook.ResumeLayout(false);
			this.groupBox_WhereToLook.PerformLayout();
			this.panel_All.ResumeLayout(false);
			this.panel_Main.ResumeLayout(false);
			this.panel_Main.PerformLayout();
			this.statusStrip.ResumeLayout(false);
			this.statusStrip.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Find;
		private DarkUI.Controls.DarkButton button_FindAll;
		private DarkUI.Controls.DarkButton button_FindNext;
		private DarkUI.Controls.DarkButton button_FindPrev;
		private DarkUI.Controls.DarkButton button_Replace;
		private DarkUI.Controls.DarkButton button_ReplaceAll;
		private DarkUI.Controls.DarkButton button_ReplaceNext;
		private DarkUI.Controls.DarkButton button_ReplacePrev;
		private DarkUI.Controls.DarkCheckBox checkBox_CaseSensitive;
		private DarkUI.Controls.DarkCheckBox checkBox_WholeWords;
		private DarkUI.Controls.DarkGroupBox groupBox_Direction;
		private DarkUI.Controls.DarkGroupBox groupBox_Mode;
		private DarkUI.Controls.DarkGroupBox groupBox_Options;
		private DarkUI.Controls.DarkGroupBox groupBox_WhereToLook;
		private DarkUI.Controls.DarkLabel label_Find;
		private DarkUI.Controls.DarkLabel label_Replace;
		private DarkUI.Controls.DarkRadioButton radioButton_AllTabs;
		private DarkUI.Controls.DarkRadioButton radioButton_Current;
		private DarkUI.Controls.DarkRadioButton radioButton_Down;
		private DarkUI.Controls.DarkRadioButton radioButton_Normal;
		private DarkUI.Controls.DarkRadioButton radioButton_Regex;
		private DarkUI.Controls.DarkRadioButton radioButton_Up;
		private DarkUI.Controls.DarkStatusStrip statusStrip;
		private DarkUI.Controls.DarkTextBox textBox_Find;
		private DarkUI.Controls.DarkTextBox textBox_Replace;
		private System.Windows.Forms.Panel panel_All;
		private System.Windows.Forms.Panel panel_Main;
		private System.Windows.Forms.ToolStripStatusLabel label_Status;
	}
}