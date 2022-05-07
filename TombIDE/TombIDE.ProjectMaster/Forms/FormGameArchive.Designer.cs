namespace TombIDE.ProjectMaster.Forms
{
	partial class FormGameArchive
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormGameArchive));
			this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.button_Generate = new DarkUI.Controls.DarkButton();
			this.label_Info = new DarkUI.Controls.DarkLabel();
			this.panel_TextBox = new System.Windows.Forms.Panel();
			this.richTextBox = new System.Windows.Forms.RichTextBox();
			this.label_Enter = new DarkUI.Controls.DarkLabel();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.tableLayoutPanel.SuspendLayout();
			this.panel_TextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel
			// 
			this.tableLayoutPanel.ColumnCount = 1;
			this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.Controls.Add(this.button_Generate, 0, 3);
			this.tableLayoutPanel.Controls.Add(this.label_Info, 0, 0);
			this.tableLayoutPanel.Controls.Add(this.panel_TextBox, 0, 2);
			this.tableLayoutPanel.Controls.Add(this.label_Enter, 0, 1);
			this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel.Name = "tableLayoutPanel";
			this.tableLayoutPanel.RowCount = 4;
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
			this.tableLayoutPanel.Size = new System.Drawing.Size(784, 561);
			this.tableLayoutPanel.TabIndex = 0;
			// 
			// button_Generate
			// 
			this.button_Generate.Checked = false;
			this.button_Generate.Dock = System.Windows.Forms.DockStyle.Right;
			this.button_Generate.Location = new System.Drawing.Point(644, 511);
			this.button_Generate.Margin = new System.Windows.Forms.Padding(0, 20, 20, 20);
			this.button_Generate.Name = "button_Generate";
			this.button_Generate.Size = new System.Drawing.Size(120, 30);
			this.button_Generate.TabIndex = 5;
			this.button_Generate.Text = "Generate archive...";
			this.button_Generate.Click += new System.EventHandler(this.button_Generate_Click);
			// 
			// label_Info
			// 
			this.label_Info.AutoSize = true;
			this.label_Info.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_Info.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Info.Location = new System.Drawing.Point(20, 20);
			this.label_Info.Margin = new System.Windows.Forms.Padding(20, 20, 20, 0);
			this.label_Info.Name = "label_Info";
			this.label_Info.Size = new System.Drawing.Size(744, 100);
			this.label_Info.TabIndex = 3;
			this.label_Info.Text = resources.GetString("label_Info.Text");
			// 
			// panel_TextBox
			// 
			this.panel_TextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.panel_TextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel_TextBox.Controls.Add(this.richTextBox);
			this.panel_TextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel_TextBox.Location = new System.Drawing.Point(20, 150);
			this.panel_TextBox.Margin = new System.Windows.Forms.Padding(20, 0, 20, 0);
			this.panel_TextBox.Name = "panel_TextBox";
			this.panel_TextBox.Padding = new System.Windows.Forms.Padding(3);
			this.panel_TextBox.Size = new System.Drawing.Size(744, 341);
			this.panel_TextBox.TabIndex = 1;
			// 
			// richTextBox
			// 
			this.richTextBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
			this.richTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.richTextBox.ForeColor = System.Drawing.Color.Gainsboro;
			this.richTextBox.Location = new System.Drawing.Point(3, 3);
			this.richTextBox.Margin = new System.Windows.Forms.Padding(20, 0, 20, 0);
			this.richTextBox.Name = "richTextBox";
			this.richTextBox.Size = new System.Drawing.Size(736, 333);
			this.richTextBox.TabIndex = 0;
			this.richTextBox.Text = "";
			// 
			// label_Enter
			// 
			this.label_Enter.AutoSize = true;
			this.label_Enter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.label_Enter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
			this.label_Enter.Location = new System.Drawing.Point(20, 120);
			this.label_Enter.Margin = new System.Windows.Forms.Padding(20, 0, 20, 0);
			this.label_Enter.Name = "label_Enter";
			this.label_Enter.Size = new System.Drawing.Size(744, 30);
			this.label_Enter.TabIndex = 2;
			this.label_Enter.Text = "Enter readme.txt text: (Optional)";
			this.label_Enter.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// timer
			// 
			this.timer.Interval = 300;
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// FormGameArchive
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 561);
			this.Controls.Add(this.tableLayoutPanel);
			this.MinimumSize = new System.Drawing.Size(740, 480);
			this.Name = "FormGameArchive";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Create an archive";
			this.tableLayoutPanel.ResumeLayout(false);
			this.tableLayoutPanel.PerformLayout();
			this.panel_TextBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
		private DarkUI.Controls.DarkLabel label_Info;
		private System.Windows.Forms.Panel panel_TextBox;
		private System.Windows.Forms.RichTextBox richTextBox;
		private DarkUI.Controls.DarkLabel label_Enter;
		private DarkUI.Controls.DarkButton button_Generate;
		private System.Windows.Forms.Timer timer;
	}
}