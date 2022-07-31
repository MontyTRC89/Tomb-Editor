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

            ReloadFunctions();

            comboboxOnEnter.SelectedItem  = _instance.Script.OnEnter;
            comboboxOnInside.SelectedItem = _instance.Script.OnInside;
            comboboxOnLeave.SelectedItem  = _instance.Script.OnLeave;

            // Set window property handlers
            Configuration.ConfigureWindow(this, Editor.Instance.Configuration);
        }

        private void ReloadFunctions()
        {
            if (string.IsNullOrEmpty(_editor.Level.Settings.TenLuaScriptFile?.Trim() ?? string.Empty))
                return;

            string path = _editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.TenLuaScriptFile);
            var functions = ScriptingUtils.GetAllFunctionsNames(path);
            functions.Insert(0, string.Empty);

            if (functions != null)
            {
                comboboxOnEnter.Items.Clear();
                comboboxOnEnter.Items.AddRange(functions.ToArray());

                comboboxOnInside.Items.Clear();
                comboboxOnInside.Items.AddRange(functions.ToArray());

                comboboxOnLeave.Items.Clear();
                comboboxOnLeave.Items.AddRange(functions.ToArray());
            }
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            _instance.Script.Activators = 0 | 
                                            (cbActivatorLara.Checked ? VolumeActivators.Player : 0) |
                                            (cbActivatorNPC.Checked ? VolumeActivators.NPCs : 0) |
                                            (cbActivatorOtherMoveables.Checked ? VolumeActivators.OtherMoveables : 0) |
                                            (cbActivatorStatics.Checked ? VolumeActivators.Statics : 0) |
                                            (cbActivatorFlyBy.Checked ? VolumeActivators.Flybys : 0);

            _instance.Script.OnEnter.Function  = comboboxOnEnter.SelectedItem?.ToString()  ?? string.Empty;
            _instance.Script.OnInside.Function = comboboxOnInside.SelectedItem?.ToString() ?? string.Empty;
            _instance.Script.OnLeave.Function = comboboxOnLeave.SelectedItem?.ToString()  ?? string.Empty;

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
