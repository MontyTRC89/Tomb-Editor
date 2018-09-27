namespace TombLib.Controls
{
    partial class LuaTextBox
    {
        /// <summary> 
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione componenti

        /// <summary> 
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare 
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.textEditor = new FastColoredTextBoxNS.FastColoredTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.textEditor)).BeginInit();
            this.SuspendLayout();
            // 
            // textEditor
            // 
            this.textEditor.AutoCompleteBracketsList = new char[] {
        '[',
        ']'};
            this.textEditor.AutoIndent = false;
            this.textEditor.AutoIndentChars = false;
            this.textEditor.AutoIndentExistingLines = false;
            this.textEditor.AutoScrollMinSize = new System.Drawing.Size(43, 18);
            this.textEditor.BackBrush = null;
            this.textEditor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.textEditor.BookmarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.textEditor.CaretColor = System.Drawing.Color.Gainsboro;
            this.textEditor.ChangedLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(96)))));
            this.textEditor.CharHeight = 18;
            this.textEditor.CharWidth = 9;
            this.textEditor.CommentPrefix = ";";
            this.textEditor.CurrentLineColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.textEditor.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textEditor.DelayedEventsInterval = 200;
            this.textEditor.DelayedTextChangedInterval = 500;
            this.textEditor.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.textEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textEditor.Font = new System.Drawing.Font("Consolas", 12F);
            this.textEditor.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.textEditor.IndentBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(48)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.textEditor.IsReplaceMode = false;
            this.textEditor.LeftPadding = 5;
            this.textEditor.LineNumberColor = System.Drawing.Color.FromArgb(((int)(((byte)(160)))), ((int)(((byte)(160)))), ((int)(((byte)(160)))));
            this.textEditor.Location = new System.Drawing.Point(0, 0);
            this.textEditor.Name = "textEditor";
            this.textEditor.Paddings = new System.Windows.Forms.Padding(0);
            this.textEditor.PreferredLineWidth = 80;
            this.textEditor.ReservedCountOfLineNumberChars = 2;
            this.textEditor.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(30)))), ((int)(((byte)(144)))), ((int)(((byte)(255)))));
            this.textEditor.ServiceColors = null;
            this.textEditor.ServiceLinesColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.textEditor.Size = new System.Drawing.Size(150, 150);
            this.textEditor.TabIndex = 4;
            this.textEditor.Zoom = 100;
            this.textEditor.TextChanged += new System.EventHandler<FastColoredTextBoxNS.TextChangedEventArgs>(this.textEditor_TextChanged);
            // 
            // LuaTextBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textEditor);
            this.Name = "LuaTextBox";
            ((System.ComponentModel.ISupportInitialize)(this.textEditor)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private FastColoredTextBoxNS.FastColoredTextBox textEditor;
    }
}
