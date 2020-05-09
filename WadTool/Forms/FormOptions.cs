using TombLib.Forms;
using TombLib.Utils;

namespace WadTool
{
    public partial class FormOptions : FormOptionsBase
    {
        private readonly WadToolClass _tool;

        public FormOptions(WadToolClass tool)
        {
            InitializeComponent();
            InitializeDialog();
            this.SetActualSize(630, 458);
            this.LockWidth();

            _tool = tool;
            _tool.EditorEventRaised += EditorEventRaised;

            ReadConfigIntoControls(this);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            //if (obj is WadToolClass.ConfigurationChangedEvent)
            //    ReadConfigIntoControls(this);
        }

        protected override void WriteConfigFromControls()
        {
            base.WriteConfigFromControls();
            _tool.Configuration?.SaveTry();
        }
    }
}
