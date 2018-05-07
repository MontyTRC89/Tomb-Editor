using DarkUI.Forms;
using System.Windows.Forms;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormSoundInfoEditor : DarkForm
    {
        public FormSoundInfoEditor(bool @fixed)
        {
            InitializeComponent();
            Text = (@fixed ? "Fixed " : "Additional ") + Text;
        }

        private void btOk_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        public WadSoundInfo SoundInfo
        {
            get { return soundInfoEditor1.SoundInfo; }
            set { soundInfoEditor1.SoundInfo = value; }
        }
    }
}
