using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Forms.TombEngine
{
    public partial class FormVolume : DarkForm
    {
        private readonly VolumeInstance _instance;
        private readonly Editor _editor;

        public FormVolume(VolumeInstance instance)
        {
            InitializeComponent();

            _instance = instance;
            _editor = Editor.Instance;

            cbActivatorLara.Checked = (_instance.Script.Activators & VolumeActivators.Player) != 0;
            cbActivatorNPC.Checked = (_instance.Script.Activators & VolumeActivators.NPCs) != 0;
            cbActivatorOtherMoveables.Checked = (_instance.Script.Activators & VolumeActivators.OtherMoveables) != 0;
            cbActivatorStatics.Checked = (_instance.Script.Activators & VolumeActivators.Statics) != 0;
            cbActivatorFlyBy.Checked = (_instance.Script.Activators & VolumeActivators.Flybys) != 0;

            tmEnter.Initialize(_editor);
            tmInside.Initialize(_editor);
            tmLeave.Initialize(_editor);

            // Set window property handlers
            Configuration.ConfigureWindow(this, Editor.Instance.Configuration);
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            _instance.Script.Activators = 0 | 
                                            (cbActivatorLara.Checked ? VolumeActivators.Player : 0) |
                                            (cbActivatorNPC.Checked ? VolumeActivators.NPCs : 0) |
                                            (cbActivatorOtherMoveables.Checked ? VolumeActivators.OtherMoveables : 0) |
                                            (cbActivatorStatics.Checked ? VolumeActivators.Statics : 0) |
                                            (cbActivatorFlyBy.Checked ? VolumeActivators.Flybys : 0);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
