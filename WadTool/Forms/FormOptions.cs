using System.Drawing;
using TombLib.Forms;
using TombLib.Utils;

namespace WadTool
{
    public partial class FormOptions : FormOptionsBase
    {
        private readonly WadToolClass _tool;

        public FormOptions(WadToolClass tool) : base(tool.Configuration)
        {
            InitializeComponent();
            InitializeDialog();
            this.SetActualSize(631, 580);
            this.LockWidth();

            _tool = tool;
            _tool.EditorEventRaised += EditorEventRaised;

            // Add the V2 rendering-backend selector programmatically to the
            // System groupbox. Binds to Configuration.Rendering3D_Backend via
            // Tag, picked up by FormOptionsBase.ReadConfigIntoControls. Mirrors
            // TombEditor's FormOptions setup.
            AddBackendSelector();

            ReadConfigIntoControls(this);
        }

        private void AddBackendSelector()
        {
            // Drop the combo + label/hint inside the System groupbox
            // (darkGroupBox1). Grow the box AND its containing panel -- the
            // panel has fixed Size in the designer (no AutoSize), so without
            // also bumping its height the new combo would be clipped below
            // the visible area.
            int backendY = darkGroupBox1.Height + 4;
            darkGroupBox1.Height += 56;
            if (darkGroupBox1.Parent != null)
                darkGroupBox1.Parent.Height += 56;

            var cmbBackend = new DarkUI.Controls.DarkComboBox
            {
                FormattingEnabled = true,
                Location = new Point(232, backendY),
                Size     = new Size(123, 23),
                Tag      = "Rendering3D_Backend",
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
            };
            cmbBackend.Items.AddRange(new object[] { "Default", "Vulkan", "OpenGL", "DX11" });

            var lblBackend = new DarkUI.Controls.DarkLabel
            {
                AutoSize  = true,
                ForeColor = Color.FromArgb(220, 220, 220),
                Location  = new Point(3, backendY + 3),
                Text      = "Rendering backend:",
            };
            var lblBackendHint = new DarkUI.Controls.DarkLabel
            {
                AutoSize  = true,
                ForeColor = Color.FromArgb(160, 160, 160),
                Location  = new Point(3, backendY + 28),
                Text      = "Default = auto (Vulkan -> OpenGL -> DX11). Restart WadTool for a change to take effect.",
            };

            darkGroupBox1.Controls.Add(cmbBackend);
            darkGroupBox1.Controls.Add(lblBackend);
            darkGroupBox1.Controls.Add(lblBackendHint);
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
