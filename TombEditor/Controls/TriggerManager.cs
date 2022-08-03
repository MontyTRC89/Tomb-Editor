using System;
using System.ComponentModel;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.Controls
{
    public partial class TriggerManager : UserControl
    {
        public TriggerManager()
        {
            InitializeComponent();

            // Link options list
            //tabbedContainer.LinkedControl = optionsList;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VolumeEvent Event
        {
            get
            {
                return _event;
            }
            set
            {
                _event = value;
                UpdateUI();
            }
        }
        private VolumeEvent _event = null;

        private Editor _editor;
        private bool _lockUI = false;

        public void Initialize(Editor editor)
        {
            _editor = editor;
            ReloadFunctions();
        }

        private void SelectTriggerMode()
        {
            tabbedContainer.SelectedIndex = rbLevelScript.Checked ? 0 : 1;

            if (!_lockUI)
                _event.Mode = rbLevelScript.Checked ? VolumeEventMode.LevelScript : VolumeEventMode.Constructor;
        }


        public void UpdateUI()
        {
            tbArgument.Enabled   =
            nudCallCount.Enabled = 
            lstFunctions.Enabled = _event != null;

            FindAndSelectFunction();
            ConstructVisualTrigger();

            if (_event == null)
                return;

            _lockUI = true;

            rbLevelScript.Checked = _event.Mode == VolumeEventMode.LevelScript;
            tbArgument.Text = _event.Argument;
            nudCallCount.Value = _event.CallCounter;

            _lockUI = false;
        }

        private void ReloadFunctions()
        {
            if (string.IsNullOrEmpty(_editor.Level.Settings.TenLuaScriptFile?.Trim() ?? string.Empty))
                return;

            string path = _editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.TenLuaScriptFile);
            var functions = ScriptingUtils.GetAllFunctionsNames(path);

            lstFunctions.Items.Clear();
            functions.ForEach(f => lstFunctions.Items.Add(new DarkUI.Controls.DarkListItem(f)));
        }

        private void FindAndSelectFunction()
        {
            if (lstFunctions.Items.Count == 0)
                return;

            if (_event == null || string.IsNullOrEmpty(_event.Function))
                return;

            for (int i = 0; i < lstFunctions.Items.Count; i++)
                if (lstFunctions.Items[i].Text == _event.Function)
                {
                    lblNotify.Visible = false;
                    lstFunctions.ClearSelection();
                    lstFunctions.SelectItem(i);
                    return;
                }

            lblNotify.Text = "Referenced function '" + _event.Function + "' was not found!";
            lblNotify.Visible = true;
        }

        private void ConstructVisualTrigger()
        {
            // TODO
        }

        private void rbLevelScript_CheckedChanged(object sender, EventArgs e) => SelectTriggerMode();
        private void rbConstructor_CheckedChanged(object sender, EventArgs e) => SelectTriggerMode();

        private void butSearch_Click(object sender, EventArgs e)
        {
            var searchPopUp = new PopUpSearch(lstFunctions) { ShowAboveControl = true };
            searchPopUp.Show(this);
        }

        private void butUnassign_Click(object sender, EventArgs e)
        {
            lstFunctions.ClearSelection();
        }

        private void lstFunctions_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (_event == null)
                return;

            _event.Function = lstFunctions.SelectedItem?.Text ?? string.Empty;
            lblNotify.Visible = false;
        }

        private void nudCallCount_Validated(object sender, EventArgs e)
        {
            if (_event == null || _lockUI)
                return;

            _event.CallCounter = (int)nudCallCount.Value;
        }

        private void tbArgument_Validated(object sender, EventArgs e)
        {
            if (_event == null || _lockUI)
                return;

            _event.Argument = tbArgument.Text;
        }
    }
}
