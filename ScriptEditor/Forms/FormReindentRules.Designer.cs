namespace ScriptEditor
{
	partial class FormReindentRules : DarkUI.Forms.DarkForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormReindentRules));
			this.button_Cancel = new DarkUI.Controls.DarkButton();
			this.button_ResetDefault = new DarkUI.Controls.DarkButton();
			this.button_Save = new DarkUI.Controls.DarkButton();
			this.checkBox_PostCommaSpace = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_PostEqualSpace = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_PreCommaSpace = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_PreEqualSpace = new DarkUI.Controls.DarkCheckBox();
			this.checkBox_ReduceSpaces = new DarkUI.Controls.DarkCheckBox();
			this.groupBox_AddSpaces = new DarkUI.Controls.DarkGroupBox();
			this.label_Comma = new DarkUI.Controls.DarkLabel();
			this.label_Equal = new DarkUI.Controls.DarkLabel();
			this.groupBox_Buttons = new DarkUI.Controls.DarkGroupBox();
			this.groupBox_Main = new DarkUI.Controls.DarkGroupBox();
			this.GroupBox_Preview = new DarkUI.Controls.DarkGroupBox();
			this.textBox_Preview = new ScriptEditor.ScriptTextBox();
			this.groupBox_AddSpaces.SuspendLayout();
			this.groupBox_Buttons.SuspendLayout();
			this.groupBox_Main.SuspendLayout();
			this.GroupBox_Preview.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.textBox_Preview)).BeginInit();
			this.SuspendLayout();
			// 
			// button_Cancel
			// 
			this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_Cancel.Location = new System.Drawing.Point(489, 9);
			this.button_Cancel.Name = "button_Cancel";
			this.button_Cancel.Size = new System.Drawing.Size(75, 25);
			this.button_Cancel.TabIndex = 5;
			this.button_Cancel.Text = "Cancel";
			// 
			// button_ResetDefault
			// 
			this.button_ResetDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.button_ResetDefault.Location = new System.Drawing.Point(6, 9);
			this.button_ResetDefault.Name = "button_ResetDefault";
			this.button_ResetDefault.Size = new System.Drawing.Size(128, 25);
			this.button_ResetDefault.TabIndex = 6;
			this.button_ResetDefault.Text = "Reset rules to default";
			this.button_ResetDefault.Click += new System.EventHandler(this.button_Default_Click);
			// 
			// button_Save
			// 
			this.button_Save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button_Save.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button_Save.Location = new System.Drawing.Point(408, 9);
			this.button_Save.Name = "button_Save";
			this.button_Save.Size = new System.Drawing.Size(75, 25);
			this.button_Save.TabIndex = 4;
			this.button_Save.Text = "Save";
			this.button_Save.Click += new System.EventHandler(this.button_Save_Click);
			// 
			// checkBox_PostCommaSpace
			// 
			this.checkBox_PostCommaSpace.AutoSize = true;
			this.checkBox_PostCommaSpace.Location = new System.Drawing.Point(124, 78);
			this.checkBox_PostCommaSpace.Margin = new System.Windows.Forms.Padding(0, 32, 40, 26);
			this.checkBox_PostCommaSpace.Name = "checkBox_PostCommaSpace";
			this.checkBox_PostCommaSpace.Size = new System.Drawing.Size(15, 14);
			this.checkBox_PostCommaSpace.TabIndex = 3;
			this.checkBox_PostCommaSpace.CheckedChanged += new System.EventHandler(this.checkBox_PostCommaSpace_CheckedChanged);
			// 
			// checkBox_PostEqualSpace
			// 
			this.checkBox_PostEqualSpace.AutoSize = true;
			this.checkBox_PostEqualSpace.Location = new System.Drawing.Point(124, 32);
			this.checkBox_PostEqualSpace.Margin = new System.Windows.Forms.Padding(0, 16, 40, 0);
			this.checkBox_PostEqualSpace.Name = "checkBox_PostEqualSpace";
			this.checkBox_PostEqualSpace.Size = new System.Drawing.Size(15, 14);
			this.checkBox_PostEqualSpace.TabIndex = 1;
			this.checkBox_PostEqualSpace.CheckedChanged += new System.EventHandler(this.checkBox_PostEqualSpace_CheckedChanged);
			// 
			// checkBox_PreCommaSpace
			// 
			this.checkBox_PreCommaSpace.AutoSize = true;
			this.checkBox_PreCommaSpace.Location = new System.Drawing.Point(43, 78);
			this.checkBox_PreCommaSpace.Margin = new System.Windows.Forms.Padding(40, 32, 0, 26);
			this.checkBox_PreCommaSpace.Name = "checkBox_PreCommaSpace";
			this.checkBox_PreCommaSpace.Size = new System.Drawing.Size(15, 14);
			this.checkBox_PreCommaSpace.TabIndex = 2;
			this.checkBox_PreCommaSpace.CheckedChanged += new System.EventHandler(this.checkBox_PreCommaSpace_CheckedChanged);
			// 
			// checkBox_PreEqualSpace
			// 
			this.checkBox_PreEqualSpace.AutoSize = true;
			this.checkBox_PreEqualSpace.Location = new System.Drawing.Point(43, 32);
			this.checkBox_PreEqualSpace.Margin = new System.Windows.Forms.Padding(40, 16, 0, 0);
			this.checkBox_PreEqualSpace.Name = "checkBox_PreEqualSpace";
			this.checkBox_PreEqualSpace.Size = new System.Drawing.Size(15, 14);
			this.checkBox_PreEqualSpace.TabIndex = 0;
			this.checkBox_PreEqualSpace.CheckedChanged += new System.EventHandler(this.checkBox_PreEqualSpace_CheckedChanged);
			// 
			// checkBox_ReduceSpaces
			// 
			this.checkBox_ReduceSpaces.AutoSize = true;
			this.checkBox_ReduceSpaces.Location = new System.Drawing.Point(16, 137);
			this.checkBox_ReduceSpaces.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
			this.checkBox_ReduceSpaces.Name = "checkBox_ReduceSpaces";
			this.checkBox_ReduceSpaces.Size = new System.Drawing.Size(169, 17);
			this.checkBox_ReduceSpaces.TabIndex = 3;
			this.checkBox_ReduceSpaces.Text = "Reduce the amount of spaces";
			this.checkBox_ReduceSpaces.CheckedChanged += new System.EventHandler(this.checkBox_ReduceSpaces_CheckedChanged);
			// 
			// groupBox_AddSpaces
			// 
			this.groupBox_AddSpaces.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox_AddSpaces.Controls.Add(this.label_Comma);
			this.groupBox_AddSpaces.Controls.Add(this.label_Equal);
			this.groupBox_AddSpaces.Controls.Add(this.checkBox_PostCommaSpace);
			this.groupBox_AddSpaces.Controls.Add(this.checkBox_PreCommaSpace);
			this.groupBox_AddSpaces.Controls.Add(this.checkBox_PostEqualSpace);
			this.groupBox_AddSpaces.Controls.Add(this.checkBox_PreEqualSpace);
			this.groupBox_AddSpaces.Location = new System.Drawing.Point(9, 9);
			this.groupBox_AddSpaces.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
			this.groupBox_AddSpaces.Name = "groupBox_AddSpaces";
			this.groupBox_AddSpaces.Size = new System.Drawing.Size(182, 121);
			this.groupBox_AddSpaces.TabIndex = 6;
			this.groupBox_AddSpaces.TabStop = false;
			this.groupBox_AddSpaces.Text = "Insert spaces";
			// 
			// label_Comma
			// 
			this.label_Comma.AutoSize = true;
			this.label_Comma.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Comma.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Comma.Location = new System.Drawing.Point(80, 76);
			this.label_Comma.Margin = new System.Windows.Forms.Padding(22, 0, 22, 0);
			this.label_Comma.Name = "label_Comma";
			this.label_Comma.Size = new System.Drawing.Size(22, 24);
			this.label_Comma.TabIndex = 5;
			this.label_Comma.Text = ",";
			// 
			// label_Equal
			// 
			this.label_Equal.AutoSize = true;
			this.label_Equal.Font = new System.Drawing.Font("Consolas", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
			this.label_Equal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Equal.Location = new System.Drawing.Point(80, 30);
			this.label_Equal.Margin = new System.Windows.Forms.Padding(22, 0, 22, 0);
			this.label_Equal.Name = "label_Equal";
			this.label_Equal.Size = new System.Drawing.Size(22, 24);
			this.label_Equal.TabIndex = 4;
			this.label_Equal.Text = "=";
			// 
			// groupBox_Buttons
			// 
			this.groupBox_Buttons.Controls.Add(this.button_ResetDefault);
			this.groupBox_Buttons.Controls.Add(this.button_Cancel);
			this.groupBox_Buttons.Controls.Add(this.button_Save);
			this.groupBox_Buttons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.groupBox_Buttons.Location = new System.Drawing.Point(0, 160);
			this.groupBox_Buttons.Name = "groupBox_Buttons";
			this.groupBox_Buttons.Size = new System.Drawing.Size(570, 40);
			this.groupBox_Buttons.TabIndex = 7;
			this.groupBox_Buttons.TabStop = false;
			// 
			// groupBox_Main
			// 
			this.groupBox_Main.Controls.Add(this.GroupBox_Preview);
			this.groupBox_Main.Controls.Add(this.groupBox_AddSpaces);
			this.groupBox_Main.Controls.Add(this.checkBox_ReduceSpaces);
			this.groupBox_Main.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox_Main.Location = new System.Drawing.Point(0, 0);
			this.groupBox_Main.Name = "groupBox_Main";
			this.groupBox_Main.Size = new System.Drawing.Size(570, 160);
			this.groupBox_Main.TabIndex = 8;
			this.groupBox_Main.TabStop = false;
			// 
			// GroupBox_Preview
			// 
			this.GroupBox_Preview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GroupBox_Preview.Controls.Add(this.textBox_Preview);
			this.GroupBox_Preview.Location = new System.Drawing.Point(194, 9);
			this.GroupBox_Preview.Margin = new System.Windows.Forms.Padding(3, 0, 0, 3);
			this.GroupBox_Preview.Name = "GroupBox_Preview";
			this.GroupBox_Preview.Size = new System.Drawing.Size(367, 145);
			this.GroupBox_Preview.TabIndex = 7;
			this.GroupBox_Preview.TabStop = false;
			this.GroupBox_Preview.Text = "Preview";
			// 
			// textBox_Preview
			// 
			this.textBox_Preview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textBox_Preview.AutoCompleteBracketsList = new char[0];
			this.textBox_Preview.AutoIndent = false;
			this.textBox_Preview.AutoIndentChars = false;
			this.textBox_Preview.AutoIndentCharsPatterns = "\r\n";
			this.textBox_Preview.AutoIndentExistingLines = false;
			this.textBox_Preview.AutoScrollMinSize = new System.Drawing.Size(305, 108);
			this.textBox_Preview.BackBrush = null;
			this.textBox_Preview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
			this.textBox_Preview.BookmarkColor = System.Drawing.Color.Transparent;
			this.textBox_Preview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.textBox_Preview.CaretColor = System.Drawing.Color.Transparent;
			this.textBox_Preview.CaretVisible = false;
			this.textBox_Preview.ChangedLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(96)))));
			this.textBox_Preview.CharHeight = 18;
			this.textBox_Preview.CharWidth = 9;
			this.textBox_Preview.CommentPrefix = ";";
			this.textBox_Preview.CurrentLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
			this.textBox_Preview.Cursor = System.Windows.Forms.Cursors.IBeam;
			this.textBox_Preview.DisabledColor = System.Drawing.Color.Transparent;
			this.textBox_Preview.Enabled = false;
			this.textBox_Preview.FoldingIndicatorColor = System.Drawing.Color.Transparent;
			this.textBox_Preview.Font = new System.Drawing.Font("Consolas", 12F);
			this.textBox_Preview.ForeColor = System.Drawing.SystemColors.ControlLight;
			this.textBox_Preview.IndentBackColor = System.Drawing.Color.Transparent;
			this.textBox_Preview.IsReplaceMode = false;
			this.textBox_Preview.LeftPadding = 6;
			this.textBox_Preview.LineNumberColor = System.Drawing.Color.Transparent;
			this.textBox_Preview.Location = new System.Drawing.Point(6, 19);
			this.textBox_Preview.Margin = new System.Windows.Forms.Padding(0);
			this.textBox_Preview.Name = "textBox_Preview";
			this.textBox_Preview.Paddings = new System.Windows.Forms.Padding(0);
			this.textBox_Preview.ReadOnly = true;
			this.textBox_Preview.ReservedCountOfLineNumberChars = 2;
			this.textBox_Preview.SelectionColor = System.Drawing.Color.Transparent;
			this.textBox_Preview.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textBox_Preview.ServiceColors")));
			this.textBox_Preview.ServiceLinesColor = System.Drawing.Color.Transparent;
			this.textBox_Preview.ShowLineNumbers = false;
			this.textBox_Preview.ShowScrollBars = false;
			this.textBox_Preview.Size = new System.Drawing.Size(355, 120);
			this.textBox_Preview.TabIndex = 0;
			this.textBox_Preview.Text = "[Level]\r\nName=Coastal Ruins\r\nRain=ENABLED\r\nLayer1=128,128,128,-8\r\nMirror=69,$7400" +
    "   ; Crossbow room\r\nLevel=DATA\\COASTAL,105";
			this.textBox_Preview.TextAreaBorderColor = System.Drawing.Color.Transparent;
			this.textBox_Preview.Zoom = 100;
			// 
			// FormReindentRules
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(570, 200);
			this.Controls.Add(this.groupBox_Main);
			this.Controls.Add(this.groupBox_Buttons);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FormReindentRules";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Reindent rules";
			this.groupBox_AddSpaces.ResumeLayout(false);
			this.groupBox_AddSpaces.PerformLayout();
			this.groupBox_Buttons.ResumeLayout(false);
			this.groupBox_Main.ResumeLayout(false);
			this.groupBox_Main.PerformLayout();
			this.GroupBox_Preview.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.textBox_Preview)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private DarkUI.Controls.DarkButton button_Cancel;
		private DarkUI.Controls.DarkButton button_ResetDefault;
		private DarkUI.Controls.DarkButton button_Save;
		private DarkUI.Controls.DarkCheckBox checkBox_PostCommaSpace;
		private DarkUI.Controls.DarkCheckBox checkBox_PostEqualSpace;
		private DarkUI.Controls.DarkCheckBox checkBox_PreCommaSpace;
		private DarkUI.Controls.DarkCheckBox checkBox_PreEqualSpace;
		private DarkUI.Controls.DarkCheckBox checkBox_ReduceSpaces;
		private DarkUI.Controls.DarkGroupBox groupBox_AddSpaces;
		private DarkUI.Controls.DarkGroupBox groupBox_Buttons;
		private DarkUI.Controls.DarkGroupBox groupBox_Main;
		private DarkUI.Controls.DarkGroupBox GroupBox_Preview;
		private DarkUI.Controls.DarkLabel label_Comma;
		private DarkUI.Controls.DarkLabel label_Equal;
		private ScriptTextBox textBox_Preview;
	}
}