namespace TombIDE.ProjectMaster.Forms;

partial class FormAdjustFrame
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
		panel_Top_StartColor = new System.Windows.Forms.Panel();
		darkLabel1 = new DarkUI.Controls.DarkLabel();
		comboBox_Top_GradientFlow = new DarkUI.Controls.DarkComboBox();
		darkGroupBox1 = new DarkUI.Controls.DarkGroupBox();
		numUpDown_Top_EndAlpha = new DarkUI.Controls.DarkNumericUpDown();
		numUpDown_Top_StartAlpha = new DarkUI.Controls.DarkNumericUpDown();
		darkLabel4 = new DarkUI.Controls.DarkLabel();
		darkLabel5 = new DarkUI.Controls.DarkLabel();
		darkLabel3 = new DarkUI.Controls.DarkLabel();
		panel_Top_EndColor = new System.Windows.Forms.Panel();
		darkLabel2 = new DarkUI.Controls.DarkLabel();
		darkGroupBox2 = new DarkUI.Controls.DarkGroupBox();
		numUpDown_Bottom_EndAlpha = new DarkUI.Controls.DarkNumericUpDown();
		numUpDown_Bottom_StartAlpha = new DarkUI.Controls.DarkNumericUpDown();
		darkLabel6 = new DarkUI.Controls.DarkLabel();
		darkLabel7 = new DarkUI.Controls.DarkLabel();
		darkLabel8 = new DarkUI.Controls.DarkLabel();
		panel_Bottom_EndColor = new System.Windows.Forms.Panel();
		darkLabel9 = new DarkUI.Controls.DarkLabel();
		panel_Bottom_StartColor = new System.Windows.Forms.Panel();
		darkLabel10 = new DarkUI.Controls.DarkLabel();
		comboBox_Bottom_GradientFlow = new DarkUI.Controls.DarkComboBox();
		darkLabel11 = new DarkUI.Controls.DarkLabel();
		panel_FontColor = new System.Windows.Forms.Panel();
		darkGroupBox3 = new DarkUI.Controls.DarkGroupBox();
		darkLabel13 = new DarkUI.Controls.DarkLabel();
		comboBox_WindowAccent = new DarkUI.Controls.DarkComboBox();
		numUpDown_DisplayTime = new DarkUI.Controls.DarkNumericUpDown();
		darkLabel12 = new DarkUI.Controls.DarkLabel();
		darkGroupBox4 = new DarkUI.Controls.DarkGroupBox();
		panel_Image = new System.Windows.Forms.Panel();
		label_LivePreview = new DarkUI.Controls.DarkLabel();
		panel_Bottom = new System.Windows.Forms.Panel();
		label_Message = new System.Windows.Forms.Label();
		panel_Top = new System.Windows.Forms.Panel();
		button_RestoreDefaults = new DarkUI.Controls.DarkButton();
		darkGroupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)numUpDown_Top_EndAlpha).BeginInit();
		((System.ComponentModel.ISupportInitialize)numUpDown_Top_StartAlpha).BeginInit();
		darkGroupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)numUpDown_Bottom_EndAlpha).BeginInit();
		((System.ComponentModel.ISupportInitialize)numUpDown_Bottom_StartAlpha).BeginInit();
		darkGroupBox3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)numUpDown_DisplayTime).BeginInit();
		darkGroupBox4.SuspendLayout();
		panel_Image.SuspendLayout();
		panel_Bottom.SuspendLayout();
		SuspendLayout();
		// 
		// panel_Top_StartColor
		// 
		panel_Top_StartColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		panel_Top_StartColor.Location = new System.Drawing.Point(12, 90);
		panel_Top_StartColor.Name = "panel_Top_StartColor";
		panel_Top_StartColor.Size = new System.Drawing.Size(75, 24);
		panel_Top_StartColor.TabIndex = 0;
		panel_Top_StartColor.Click += panel_Top_StartColor_Click;
		// 
		// darkLabel1
		// 
		darkLabel1.AutoSize = true;
		darkLabel1.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel1.Location = new System.Drawing.Point(12, 23);
		darkLabel1.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
		darkLabel1.Name = "darkLabel1";
		darkLabel1.Size = new System.Drawing.Size(81, 13);
		darkLabel1.TabIndex = 1;
		darkLabel1.Text = "Gradient flow:";
		// 
		// comboBox_Top_GradientFlow
		// 
		comboBox_Top_GradientFlow.FormattingEnabled = true;
		comboBox_Top_GradientFlow.Items.AddRange(new object[] { "Left to Right", "Top to Bottom", "Right to Left", "Bottom to Top" });
		comboBox_Top_GradientFlow.Location = new System.Drawing.Point(12, 39);
		comboBox_Top_GradientFlow.Name = "comboBox_Top_GradientFlow";
		comboBox_Top_GradientFlow.Size = new System.Drawing.Size(156, 23);
		comboBox_Top_GradientFlow.TabIndex = 2;
		comboBox_Top_GradientFlow.SelectedIndexChanged += comboBox_Top_GradientFlow_SelectedIndexChanged;
		// 
		// darkGroupBox1
		// 
		darkGroupBox1.Controls.Add(numUpDown_Top_EndAlpha);
		darkGroupBox1.Controls.Add(numUpDown_Top_StartAlpha);
		darkGroupBox1.Controls.Add(darkLabel4);
		darkGroupBox1.Controls.Add(darkLabel5);
		darkGroupBox1.Controls.Add(darkLabel3);
		darkGroupBox1.Controls.Add(panel_Top_EndColor);
		darkGroupBox1.Controls.Add(darkLabel2);
		darkGroupBox1.Controls.Add(panel_Top_StartColor);
		darkGroupBox1.Controls.Add(darkLabel1);
		darkGroupBox1.Controls.Add(comboBox_Top_GradientFlow);
		darkGroupBox1.Location = new System.Drawing.Point(12, 12);
		darkGroupBox1.Name = "darkGroupBox1";
		darkGroupBox1.Padding = new System.Windows.Forms.Padding(9, 3, 9, 3);
		darkGroupBox1.Size = new System.Drawing.Size(180, 180);
		darkGroupBox1.TabIndex = 3;
		darkGroupBox1.TabStop = false;
		darkGroupBox1.Text = "Top bar";
		// 
		// numUpDown_Top_EndAlpha
		// 
		numUpDown_Top_EndAlpha.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
		numUpDown_Top_EndAlpha.Location = new System.Drawing.Point(93, 142);
		numUpDown_Top_EndAlpha.LoopValues = false;
		numUpDown_Top_EndAlpha.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
		numUpDown_Top_EndAlpha.Name = "numUpDown_Top_EndAlpha";
		numUpDown_Top_EndAlpha.Size = new System.Drawing.Size(75, 22);
		numUpDown_Top_EndAlpha.TabIndex = 9;
		numUpDown_Top_EndAlpha.ValueChanged += numUpDown_Top_EndAlpha_ValueChanged;
		// 
		// numUpDown_Top_StartAlpha
		// 
		numUpDown_Top_StartAlpha.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
		numUpDown_Top_StartAlpha.Location = new System.Drawing.Point(12, 142);
		numUpDown_Top_StartAlpha.LoopValues = false;
		numUpDown_Top_StartAlpha.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
		numUpDown_Top_StartAlpha.Name = "numUpDown_Top_StartAlpha";
		numUpDown_Top_StartAlpha.Size = new System.Drawing.Size(75, 22);
		numUpDown_Top_StartAlpha.TabIndex = 8;
		numUpDown_Top_StartAlpha.ValueChanged += numUpDown_Top_StartAlpha_ValueChanged;
		// 
		// darkLabel4
		// 
		darkLabel4.AutoSize = true;
		darkLabel4.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel4.Location = new System.Drawing.Point(93, 126);
		darkLabel4.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
		darkLabel4.Name = "darkLabel4";
		darkLabel4.Size = new System.Drawing.Size(62, 13);
		darkLabel4.TabIndex = 7;
		darkLabel4.Text = "End alpha:";
		// 
		// darkLabel5
		// 
		darkLabel5.AutoSize = true;
		darkLabel5.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel5.Location = new System.Drawing.Point(12, 126);
		darkLabel5.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
		darkLabel5.Name = "darkLabel5";
		darkLabel5.Size = new System.Drawing.Size(66, 13);
		darkLabel5.TabIndex = 6;
		darkLabel5.Text = "Start alpha:";
		// 
		// darkLabel3
		// 
		darkLabel3.AutoSize = true;
		darkLabel3.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel3.Location = new System.Drawing.Point(93, 74);
		darkLabel3.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
		darkLabel3.Name = "darkLabel3";
		darkLabel3.Size = new System.Drawing.Size(59, 13);
		darkLabel3.TabIndex = 5;
		darkLabel3.Text = "End color:";
		// 
		// panel_Top_EndColor
		// 
		panel_Top_EndColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		panel_Top_EndColor.Location = new System.Drawing.Point(93, 90);
		panel_Top_EndColor.Name = "panel_Top_EndColor";
		panel_Top_EndColor.Size = new System.Drawing.Size(75, 24);
		panel_Top_EndColor.TabIndex = 4;
		panel_Top_EndColor.Click += panel_Top_EndColor_Click;
		// 
		// darkLabel2
		// 
		darkLabel2.AutoSize = true;
		darkLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel2.Location = new System.Drawing.Point(12, 74);
		darkLabel2.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
		darkLabel2.Name = "darkLabel2";
		darkLabel2.Size = new System.Drawing.Size(63, 13);
		darkLabel2.TabIndex = 3;
		darkLabel2.Text = "Start color:";
		// 
		// darkGroupBox2
		// 
		darkGroupBox2.Controls.Add(numUpDown_Bottom_EndAlpha);
		darkGroupBox2.Controls.Add(numUpDown_Bottom_StartAlpha);
		darkGroupBox2.Controls.Add(darkLabel6);
		darkGroupBox2.Controls.Add(darkLabel7);
		darkGroupBox2.Controls.Add(darkLabel8);
		darkGroupBox2.Controls.Add(panel_Bottom_EndColor);
		darkGroupBox2.Controls.Add(darkLabel9);
		darkGroupBox2.Controls.Add(panel_Bottom_StartColor);
		darkGroupBox2.Controls.Add(darkLabel10);
		darkGroupBox2.Controls.Add(comboBox_Bottom_GradientFlow);
		darkGroupBox2.Location = new System.Drawing.Point(12, 198);
		darkGroupBox2.Name = "darkGroupBox2";
		darkGroupBox2.Padding = new System.Windows.Forms.Padding(9, 3, 9, 3);
		darkGroupBox2.Size = new System.Drawing.Size(180, 180);
		darkGroupBox2.TabIndex = 10;
		darkGroupBox2.TabStop = false;
		darkGroupBox2.Text = "Bottom bar";
		// 
		// numUpDown_Bottom_EndAlpha
		// 
		numUpDown_Bottom_EndAlpha.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
		numUpDown_Bottom_EndAlpha.Location = new System.Drawing.Point(93, 142);
		numUpDown_Bottom_EndAlpha.LoopValues = false;
		numUpDown_Bottom_EndAlpha.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
		numUpDown_Bottom_EndAlpha.Name = "numUpDown_Bottom_EndAlpha";
		numUpDown_Bottom_EndAlpha.Size = new System.Drawing.Size(75, 22);
		numUpDown_Bottom_EndAlpha.TabIndex = 9;
		numUpDown_Bottom_EndAlpha.ValueChanged += numUpDown_Bottom_EndAlpha_ValueChanged;
		// 
		// numUpDown_Bottom_StartAlpha
		// 
		numUpDown_Bottom_StartAlpha.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
		numUpDown_Bottom_StartAlpha.Location = new System.Drawing.Point(12, 142);
		numUpDown_Bottom_StartAlpha.LoopValues = false;
		numUpDown_Bottom_StartAlpha.Maximum = new decimal(new int[] { 255, 0, 0, 0 });
		numUpDown_Bottom_StartAlpha.Name = "numUpDown_Bottom_StartAlpha";
		numUpDown_Bottom_StartAlpha.Size = new System.Drawing.Size(75, 22);
		numUpDown_Bottom_StartAlpha.TabIndex = 8;
		numUpDown_Bottom_StartAlpha.ValueChanged += numUpDown_Bottom_StartAlpha_ValueChanged;
		// 
		// darkLabel6
		// 
		darkLabel6.AutoSize = true;
		darkLabel6.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel6.Location = new System.Drawing.Point(93, 126);
		darkLabel6.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
		darkLabel6.Name = "darkLabel6";
		darkLabel6.Size = new System.Drawing.Size(62, 13);
		darkLabel6.TabIndex = 7;
		darkLabel6.Text = "End alpha:";
		// 
		// darkLabel7
		// 
		darkLabel7.AutoSize = true;
		darkLabel7.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel7.Location = new System.Drawing.Point(12, 126);
		darkLabel7.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
		darkLabel7.Name = "darkLabel7";
		darkLabel7.Size = new System.Drawing.Size(66, 13);
		darkLabel7.TabIndex = 6;
		darkLabel7.Text = "Start alpha:";
		// 
		// darkLabel8
		// 
		darkLabel8.AutoSize = true;
		darkLabel8.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel8.Location = new System.Drawing.Point(93, 74);
		darkLabel8.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
		darkLabel8.Name = "darkLabel8";
		darkLabel8.Size = new System.Drawing.Size(59, 13);
		darkLabel8.TabIndex = 5;
		darkLabel8.Text = "End color:";
		// 
		// panel_Bottom_EndColor
		// 
		panel_Bottom_EndColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		panel_Bottom_EndColor.Location = new System.Drawing.Point(93, 90);
		panel_Bottom_EndColor.Name = "panel_Bottom_EndColor";
		panel_Bottom_EndColor.Size = new System.Drawing.Size(75, 24);
		panel_Bottom_EndColor.TabIndex = 4;
		panel_Bottom_EndColor.Click += panel_Bottom_EndColor_Click;
		// 
		// darkLabel9
		// 
		darkLabel9.AutoSize = true;
		darkLabel9.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel9.Location = new System.Drawing.Point(12, 74);
		darkLabel9.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
		darkLabel9.Name = "darkLabel9";
		darkLabel9.Size = new System.Drawing.Size(63, 13);
		darkLabel9.TabIndex = 3;
		darkLabel9.Text = "Start color:";
		// 
		// panel_Bottom_StartColor
		// 
		panel_Bottom_StartColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		panel_Bottom_StartColor.Location = new System.Drawing.Point(12, 90);
		panel_Bottom_StartColor.Name = "panel_Bottom_StartColor";
		panel_Bottom_StartColor.Size = new System.Drawing.Size(75, 24);
		panel_Bottom_StartColor.TabIndex = 0;
		panel_Bottom_StartColor.Click += panel_Bottom_StartColor_Click;
		// 
		// darkLabel10
		// 
		darkLabel10.AutoSize = true;
		darkLabel10.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel10.Location = new System.Drawing.Point(12, 23);
		darkLabel10.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
		darkLabel10.Name = "darkLabel10";
		darkLabel10.Size = new System.Drawing.Size(81, 13);
		darkLabel10.TabIndex = 1;
		darkLabel10.Text = "Gradient flow:";
		// 
		// comboBox_Bottom_GradientFlow
		// 
		comboBox_Bottom_GradientFlow.FormattingEnabled = true;
		comboBox_Bottom_GradientFlow.Items.AddRange(new object[] { "Left to Right", "Top to Bottom", "Right to Left", "Bottom to Top" });
		comboBox_Bottom_GradientFlow.Location = new System.Drawing.Point(12, 39);
		comboBox_Bottom_GradientFlow.Name = "comboBox_Bottom_GradientFlow";
		comboBox_Bottom_GradientFlow.Size = new System.Drawing.Size(156, 23);
		comboBox_Bottom_GradientFlow.TabIndex = 2;
		comboBox_Bottom_GradientFlow.SelectedIndexChanged += comboBox_Bottom_GradientFlow_SelectedIndexChanged;
		// 
		// darkLabel11
		// 
		darkLabel11.AutoSize = true;
		darkLabel11.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel11.Location = new System.Drawing.Point(12, 74);
		darkLabel11.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
		darkLabel11.Name = "darkLabel11";
		darkLabel11.Size = new System.Drawing.Size(63, 13);
		darkLabel11.TabIndex = 5;
		darkLabel11.Text = "Font color:";
		// 
		// panel_FontColor
		// 
		panel_FontColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		panel_FontColor.Location = new System.Drawing.Point(12, 90);
		panel_FontColor.Name = "panel_FontColor";
		panel_FontColor.Size = new System.Drawing.Size(156, 24);
		panel_FontColor.TabIndex = 4;
		panel_FontColor.Click += panel_FontColor_Click;
		// 
		// darkGroupBox3
		// 
		darkGroupBox3.Controls.Add(darkLabel13);
		darkGroupBox3.Controls.Add(comboBox_WindowAccent);
		darkGroupBox3.Controls.Add(numUpDown_DisplayTime);
		darkGroupBox3.Controls.Add(darkLabel12);
		darkGroupBox3.Controls.Add(darkLabel11);
		darkGroupBox3.Controls.Add(panel_FontColor);
		darkGroupBox3.Location = new System.Drawing.Point(12, 384);
		darkGroupBox3.Name = "darkGroupBox3";
		darkGroupBox3.Padding = new System.Windows.Forms.Padding(9, 3, 9, 3);
		darkGroupBox3.Size = new System.Drawing.Size(180, 180);
		darkGroupBox3.TabIndex = 11;
		darkGroupBox3.TabStop = false;
		darkGroupBox3.Text = "Miscellaneous";
		// 
		// darkLabel13
		// 
		darkLabel13.AutoSize = true;
		darkLabel13.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel13.Location = new System.Drawing.Point(12, 23);
		darkLabel13.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
		darkLabel13.Name = "darkLabel13";
		darkLabel13.Size = new System.Drawing.Size(140, 13);
		darkLabel13.TabIndex = 11;
		darkLabel13.Text = "Window accent: (Win10+)";
		// 
		// comboBox_WindowAccent
		// 
		comboBox_WindowAccent.FormattingEnabled = true;
		comboBox_WindowAccent.Items.AddRange(new object[] { "None", "Acrylic Glass" });
		comboBox_WindowAccent.Location = new System.Drawing.Point(12, 39);
		comboBox_WindowAccent.Name = "comboBox_WindowAccent";
		comboBox_WindowAccent.Size = new System.Drawing.Size(156, 23);
		comboBox_WindowAccent.TabIndex = 12;
		comboBox_WindowAccent.SelectedIndexChanged += comboBox_WindowAccent_SelectedIndexChanged;
		// 
		// numUpDown_DisplayTime
		// 
		numUpDown_DisplayTime.Increment = new decimal(new int[] { 500, 0, 0, 0 });
		numUpDown_DisplayTime.IncrementAlternate = new decimal(new int[] { 10, 0, 0, 65536 });
		numUpDown_DisplayTime.Location = new System.Drawing.Point(12, 142);
		numUpDown_DisplayTime.LoopValues = false;
		numUpDown_DisplayTime.Maximum = new decimal(new int[] { 5000, 0, 0, 0 });
		numUpDown_DisplayTime.Minimum = new decimal(new int[] { 1000, 0, 0, 0 });
		numUpDown_DisplayTime.Name = "numUpDown_DisplayTime";
		numUpDown_DisplayTime.Size = new System.Drawing.Size(156, 22);
		numUpDown_DisplayTime.TabIndex = 10;
		numUpDown_DisplayTime.Value = new decimal(new int[] { 1500, 0, 0, 0 });
		numUpDown_DisplayTime.ValueChanged += numUpDown_DisplayTime_ValueChanged;
		// 
		// darkLabel12
		// 
		darkLabel12.AutoSize = true;
		darkLabel12.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		darkLabel12.Location = new System.Drawing.Point(12, 126);
		darkLabel12.Margin = new System.Windows.Forms.Padding(3, 9, 3, 0);
		darkLabel12.Name = "darkLabel12";
		darkLabel12.Size = new System.Drawing.Size(95, 13);
		darkLabel12.TabIndex = 9;
		darkLabel12.Text = "Display time (ms):";
		// 
		// darkGroupBox4
		// 
		darkGroupBox4.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
		darkGroupBox4.Controls.Add(panel_Image);
		darkGroupBox4.Controls.Add(panel_Bottom);
		darkGroupBox4.Controls.Add(panel_Top);
		darkGroupBox4.Location = new System.Drawing.Point(198, 12);
		darkGroupBox4.Name = "darkGroupBox4";
		darkGroupBox4.Padding = new System.Windows.Forms.Padding(9, 3, 9, 9);
		darkGroupBox4.Size = new System.Drawing.Size(774, 582);
		darkGroupBox4.TabIndex = 12;
		darkGroupBox4.TabStop = false;
		darkGroupBox4.Text = "Rough preview (does not represent final look)";
		// 
		// panel_Image
		// 
		panel_Image.BackColor = System.Drawing.Color.Black;
		panel_Image.Controls.Add(label_LivePreview);
		panel_Image.Dock = System.Windows.Forms.DockStyle.Fill;
		panel_Image.Location = new System.Drawing.Point(9, 44);
		panel_Image.Name = "panel_Image";
		panel_Image.Size = new System.Drawing.Size(756, 454);
		panel_Image.TabIndex = 3;
		// 
		// label_LivePreview
		// 
		label_LivePreview.Anchor = System.Windows.Forms.AnchorStyles.None;
		label_LivePreview.AutoSize = true;
		label_LivePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		label_LivePreview.Cursor = System.Windows.Forms.Cursors.Hand;
		label_LivePreview.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
		label_LivePreview.Location = new System.Drawing.Point(265, 191);
		label_LivePreview.Name = "label_LivePreview";
		label_LivePreview.Padding = new System.Windows.Forms.Padding(30);
		label_LivePreview.Size = new System.Drawing.Size(226, 73);
		label_LivePreview.TabIndex = 0;
		label_LivePreview.Text = "Click here to show Live Preview";
		label_LivePreview.Click += label_LivePreview_Click;
		// 
		// panel_Bottom
		// 
		panel_Bottom.Controls.Add(label_Message);
		panel_Bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
		panel_Bottom.Location = new System.Drawing.Point(9, 498);
		panel_Bottom.Name = "panel_Bottom";
		panel_Bottom.Size = new System.Drawing.Size(756, 75);
		panel_Bottom.TabIndex = 2;
		panel_Bottom.Paint += panel_Bottom_Paint;
		// 
		// label_Message
		// 
		label_Message.BackColor = System.Drawing.Color.Transparent;
		label_Message.Dock = System.Windows.Forms.DockStyle.Fill;
		label_Message.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
		label_Message.Location = new System.Drawing.Point(0, 0);
		label_Message.Name = "label_Message";
		label_Message.Size = new System.Drawing.Size(756, 75);
		label_Message.TabIndex = 0;
		label_Message.Text = "Press CTRL to show the SETUP dialog...";
		label_Message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		// 
		// panel_Top
		// 
		panel_Top.Dock = System.Windows.Forms.DockStyle.Top;
		panel_Top.Location = new System.Drawing.Point(9, 18);
		panel_Top.Name = "panel_Top";
		panel_Top.Size = new System.Drawing.Size(756, 26);
		panel_Top.TabIndex = 0;
		panel_Top.Paint += panel_Top_Paint;
		// 
		// button_RestoreDefaults
		// 
		button_RestoreDefaults.Checked = false;
		button_RestoreDefaults.Location = new System.Drawing.Point(12, 570);
		button_RestoreDefaults.Name = "button_RestoreDefaults";
		button_RestoreDefaults.Size = new System.Drawing.Size(180, 24);
		button_RestoreDefaults.TabIndex = 13;
		button_RestoreDefaults.Text = "Restore default frames...";
		button_RestoreDefaults.Click += button_RestoreDefaults_Click;
		// 
		// FormAdjustFrame
		// 
		AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
		AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		ClientSize = new System.Drawing.Size(984, 606);
		Controls.Add(button_RestoreDefaults);
		Controls.Add(darkGroupBox4);
		Controls.Add(darkGroupBox3);
		Controls.Add(darkGroupBox2);
		Controls.Add(darkGroupBox1);
		MinimumSize = new System.Drawing.Size(645, 645);
		Name = "FormAdjustFrame";
		ShowIcon = false;
		StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		Text = "Customize splash screen frames";
		darkGroupBox1.ResumeLayout(false);
		darkGroupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)numUpDown_Top_EndAlpha).EndInit();
		((System.ComponentModel.ISupportInitialize)numUpDown_Top_StartAlpha).EndInit();
		darkGroupBox2.ResumeLayout(false);
		darkGroupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)numUpDown_Bottom_EndAlpha).EndInit();
		((System.ComponentModel.ISupportInitialize)numUpDown_Bottom_StartAlpha).EndInit();
		darkGroupBox3.ResumeLayout(false);
		darkGroupBox3.PerformLayout();
		((System.ComponentModel.ISupportInitialize)numUpDown_DisplayTime).EndInit();
		darkGroupBox4.ResumeLayout(false);
		panel_Image.ResumeLayout(false);
		panel_Image.PerformLayout();
		panel_Bottom.ResumeLayout(false);
		ResumeLayout(false);
	}

	#endregion

	private System.Windows.Forms.Panel panel_Top_StartColor;
	private DarkUI.Controls.DarkLabel darkLabel1;
	private DarkUI.Controls.DarkComboBox comboBox_Top_GradientFlow;
	private DarkUI.Controls.DarkGroupBox darkGroupBox1;
	private DarkUI.Controls.DarkLabel darkLabel2;
	private DarkUI.Controls.DarkNumericUpDown numUpDown_Top_EndAlpha;
	private DarkUI.Controls.DarkNumericUpDown numUpDown_Top_StartAlpha;
	private DarkUI.Controls.DarkLabel darkLabel4;
	private DarkUI.Controls.DarkLabel darkLabel5;
	private DarkUI.Controls.DarkLabel darkLabel3;
	private System.Windows.Forms.Panel panel_Top_EndColor;
	private DarkUI.Controls.DarkGroupBox darkGroupBox2;
	private DarkUI.Controls.DarkNumericUpDown numUpDown_Bottom_EndAlpha;
	private DarkUI.Controls.DarkNumericUpDown numUpDown_Bottom_StartAlpha;
	private DarkUI.Controls.DarkLabel darkLabel6;
	private DarkUI.Controls.DarkLabel darkLabel7;
	private DarkUI.Controls.DarkLabel darkLabel8;
	private System.Windows.Forms.Panel panel_Bottom_EndColor;
	private DarkUI.Controls.DarkLabel darkLabel9;
	private System.Windows.Forms.Panel panel_Bottom_StartColor;
	private DarkUI.Controls.DarkLabel darkLabel10;
	private DarkUI.Controls.DarkComboBox comboBox_Bottom_GradientFlow;
	private DarkUI.Controls.DarkLabel darkLabel11;
	private System.Windows.Forms.Panel panel_FontColor;
	private DarkUI.Controls.DarkGroupBox darkGroupBox3;
	private DarkUI.Controls.DarkNumericUpDown numUpDown_DisplayTime;
	private DarkUI.Controls.DarkLabel darkLabel12;
	private DarkUI.Controls.DarkGroupBox darkGroupBox4;
	private System.Windows.Forms.Panel panel_Top;
	private System.Windows.Forms.Panel panel_Image;
	private System.Windows.Forms.Panel panel_Bottom;
	private DarkUI.Controls.DarkLabel darkLabel13;
	private DarkUI.Controls.DarkComboBox comboBox_WindowAccent;
	private System.Windows.Forms.Label label_Message;
	private DarkUI.Controls.DarkLabel label_LivePreview;
	private DarkUI.Controls.DarkButton button_RestoreDefaults;
}