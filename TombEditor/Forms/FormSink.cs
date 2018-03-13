using DarkUI.Forms;
using System;
using System.Windows.Forms;
using TombLib.LevelData;

namespace TombEditor
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
            this.Close();
        }

        private void FormSink_Load(object sender, EventArgs e)
        {
            comboStrength.SelectedIndex = _sink.Strength;
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _sink.Strength = (short)comboStrength.SelectedIndex;

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
