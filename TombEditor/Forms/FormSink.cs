using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormSink : DarkForm
    {
        public bool IsNew { get; set; }
        private SinkInstance _sink;

        public FormSink(SinkInstance sink)
        {
            _sink = sink;
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormSink_Load(object sender, EventArgs e)
        {
            comboStrength.SelectedIndex = _sink.Strength;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _sink.Strength = (short)comboStrength.SelectedIndex;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
