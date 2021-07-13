using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib;
using System.Linq;

namespace TombEditor.Forms
{
    public partial class FormSink : DarkForm
    {
        public bool IsNew { get; set; }
        private readonly SinkInstance _sink;
        private readonly Editor _editor = Editor.Instance;

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
            nudStrength.Value = MathC.Clamp(_sink.Strength + 1, 1, 32);

            if (_editor.Level.Settings.GameVersion == TRVersion.Game.TombEngine)
            {
                tbLuaId.Text = _sink.LuaName;
            }
            else
            {
                labelLuaId.Visible = false;
                tbLuaId.Visible = false;
            }
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            if (_editor.Level.Settings.GameVersion == TRVersion.Game.TombEngine)
            {
                if (!_sink.TrySetLuaName(tbLuaId.Text))
                {
                    DarkMessageBox.Show(this, "The value of Lua Name is already taken by another object", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            _sink.Strength = (short)(nudStrength.Value - 1);

            if (_editor.Level.Settings.GameVersion == TRVersion.Game.TombEngine)
            {
                _sink.LuaName = tbLuaId.Text;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
