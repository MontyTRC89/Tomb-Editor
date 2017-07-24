using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;

namespace TombEditor
{
    public partial class FormSink : DarkForm
    {
        public bool IsNew { get; set; }

        private Editor _editor;

        public FormSink()
        {
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FormSink_Load(object sender, EventArgs e)
        {
            _editor = Editor.Instance;

            SinkInstance sink = (SinkInstance)_editor.Level.Objects[_editor.PickingResult.Element];

            comboStrength.SelectedIndex = sink.Strength;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            SinkInstance sink = (SinkInstance)_editor.Level.Objects[_editor.PickingResult.Element];
            sink.Strength = (short)comboStrength.SelectedIndex;
            _editor.Level.Objects[_editor.PickingResult.Element] = sink;

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
