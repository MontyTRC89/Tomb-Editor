using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormTriggerVolume : DarkForm
    {
        private readonly VolumeInstance _instance;
        private readonly Editor _editor;

        public FormTriggerVolume(VolumeInstance instance)
        {
            InitializeComponent();

            _instance = instance;
            _editor = Editor.Instance;

            cbActivatorLara.Checked = (_instance.Activators & VolumeActivators.Player) != 0;
            cbActivatorNPC.Checked = (_instance.Activators & VolumeActivators.NPCs) != 0;
            cbActivatorOtherMoveables.Checked = (_instance.Activators & VolumeActivators.OtherMoveables) != 0;
            cbActivatorStatics.Checked = (_instance.Activators & VolumeActivators.Statics) != 0;
            cbActivatorFlyBy.Checked = (_instance.Activators & VolumeActivators.Flybys) != 0;

            ReloadFunctions();

            comboboxOnEnter.Text = _instance.Scripts.OnEnter;
            comboboxOnInside.Text = _instance.Scripts.OnInside;
            comboboxOnLeave.Text = _instance.Scripts.OnLeave;
        }

        private void ReloadFunctions()
        {
            if (_editor.Level.Settings.TenLuaScriptFile != null)
            {
                string path = _editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.TenLuaScriptFile, null);
                var functions = ScriptingUtils.GetAllFunctionsNames(path);
                if (functions!=null)
                {
                    comboboxOnEnter.Items.Clear();
                    comboboxOnEnter.Items.AddRange(functions.ToArray());

                    comboboxOnInside.Items.Clear();
                    comboboxOnInside.Items.AddRange(functions.ToArray());

                    comboboxOnLeave.Items.Clear();
                    comboboxOnLeave.Items.AddRange(functions.ToArray());
                }
            }
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            _instance.Activators = 0 | 
                                   (cbActivatorLara.Checked ? VolumeActivators.Player : 0) |
                                   (cbActivatorNPC.Checked ? VolumeActivators.NPCs : 0) |
                                   (cbActivatorOtherMoveables.Checked ? VolumeActivators.OtherMoveables : 0) |
                                   (cbActivatorStatics.Checked ? VolumeActivators.Statics : 0) |
                                   (cbActivatorFlyBy.Checked ? VolumeActivators.Flybys : 0);

            _instance.Scripts.OnEnter = comboboxOnEnter.Text;
            _instance.Scripts.OnInside = comboboxOnInside.Text;
            _instance.Scripts.OnLeave = comboboxOnLeave.Text;

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
