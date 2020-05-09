using TombLib.Forms;
using TombLib.Utils;

namespace SoundTool
{
    public partial class FormOptions : FormOptionsBase
    {
        public FormOptions(Configuration config) : base(config)
        {
            InitializeComponent();
            InitializeDialog();
            this.SetActualSize();
            this.LockWidth();
        }
    }
}
