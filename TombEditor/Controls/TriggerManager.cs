using DarkUI.Config;
using DarkUI.Forms;
using System;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using TombLib.Controls;
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
                nodeEditor.Nodes = _event.Nodes;
            }
        }
        private VolumeEvent _event = null;

        private Editor _editor;
        private bool _lockUI = false;

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.ConfigurationChangedEvent)
            {
                ReloadFunctions();
                FindAndSelectFunction();
                UpdateNodeEditorOptions();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= EditorEventRaised;

                if (components != null)
                    components.Dispose();
            }

            base.Dispose(disposing);
        }

        public void Initialize(Editor editor)
        {
            _editor = editor;
            _editor.EditorEventRaised += EditorEventRaised;

            ReloadFunctions();

            nodeEditor.Initialize();
            UpdateNodeEditorOptions();

            nodeEditor.ViewPositionChanged += NodeEditorViewPostionChanged;
            nodeEditor.SelectionChanged += NodeEditor_SelectionChanged;
        }

        private void NodeEditor_SelectionChanged(object sender, EventArgs e)
        {
            UpdateNodeEditorControls();
        }

        private void NodeEditorViewPostionChanged(object sender, EventArgs e)
        {
            if (_event != null)
                _event.NodePosition = nodeEditor.ViewPosition;
        }

        private void UpdateNodeEditorOptions()
        {
            nodeEditor.GridSize = _editor.Configuration.NodeEditor_Size;
            nodeEditor.GridStep = _editor.Configuration.NodeEditor_GridStep;
            nodeEditor.LinksAsRopes = _editor.Configuration.NodeEditor_LinksAsRopes;
        }

        private void SelectTriggerMode()
        {
            tabbedContainer.SelectedIndex = rbLevelScript.Checked ? 0 : 1;
            butSearch.Visible = butUnassign.Visible = rbLevelScript.Checked;
            lblNotify.Visible = false;

            if (rbLevelScript.Checked)
                FindAndSelectFunction();

            if (!_lockUI)
                _event.Mode = rbLevelScript.Checked ? VolumeEventMode.LevelScript : VolumeEventMode.NodeEditor;
        }

        public void UpdateUI()
        {
            tbArgument.Enabled   =
            nudCallCount.Enabled =
            nudCallCount2.Enabled =
            lstFunctions.Enabled = 
            nodeEditor.Enabled =
            rbLevelScript.Enabled = 
            rbNodeEditor.Enabled = _event != null;

            FindAndSelectFunction();

            if (_event == null)
                return;

            _lockUI = true;
            {
                rbLevelScript.Checked = _event.Mode == VolumeEventMode.LevelScript;
                rbNodeEditor.Checked = _event.Mode == VolumeEventMode.NodeEditor;
                tbArgument.Text = _event.Argument;
                nudCallCount.Value = _event.CallCounter;

                if (_event.NodePosition.X == float.MaxValue)
                    nodeEditor.ViewPosition = new Vector2(nodeEditor.GridSize / 2.0f);
                else
                    nodeEditor.ViewPosition = _event.NodePosition;

                UpdateNodeEditorControls();
            }
            _lockUI = false;
        }

        private void UpdateNodeEditorControls()
        {
            butChangeNodeColor.Enabled =
            butRenameNode.Enabled =
            butDeleteNode.Enabled = nodeEditor.SelectedNodes.Count > 0;
        }

        private void ReloadFunctions()
        {
            lblListNotify.ForeColor = Colors.DisabledText;
            lstFunctions.Items.Clear();

            if (string.IsNullOrEmpty(_editor.Level.Settings.TenLuaScriptFile?.Trim() ?? string.Empty))
            {
                lblListNotify.Tag = 1;
                lblListNotify.Text = "Level script file is not specified." + "\n" +
                                     "Click here to load level script file.";
            }
            else
            {
                string path = _editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.TenLuaScriptFile);

                if (!File.Exists(path))
                {
                    lblListNotify.Tag = 1;
                    lblListNotify.Text = "Level script file '" + Path.GetFileName(path) + "' not found." + "\n" +
                                         "Click here to choose replacement.";
                }
                else
                {
                    var functions = ScriptingUtils.GetAllFunctionsNames(path);
                    functions.ForEach(f => lstFunctions.Items.Add(new DarkUI.Controls.DarkListItem(f)));

                    if (lstFunctions.Items.Count == 0)
                    {
                        lblListNotify.Tag = 0;
                        lblListNotify.Text = "Level script file does not have any level functions." + "\n" +
                                             "They must have 'LevelFuncs.FuncName = function() ... end' format.";
                    }
                }
            }

            panelFunctionControls.Visible =
            lstFunctions.Visible =
            butSearch.Enabled =
            butUnassign.Enabled = lstFunctions.Items.Count > 0;
        }

        private void FindAndSelectFunction()
        {
            if (lstFunctions.Items.Count == 0)
                return;

            if (_event == null || string.IsNullOrEmpty(_event.Function))
            {
                lblNotify.Visible = false;
                lstFunctions.ClearSelection();
                return;
            }

            for (int i = 0; i < lstFunctions.Items.Count; i++)
                if (lstFunctions.Items[i].Text == _event.Function)
                {
                    lstFunctions.ClearSelection();
                    lstFunctions.SelectItem(i);
                    return;
                }

            _lockUI = true;
            lstFunctions.ClearSelection();
            lblNotify.Text = "Not found: '" + _event.Function + "'";
            lblNotify.Visible = true;
            _lockUI = false;
        }

        private void rbLevelScript_CheckedChanged(object sender, EventArgs e) => SelectTriggerMode();
        private void rbNodeEditor_CheckedChanged(object sender, EventArgs e) => SelectTriggerMode();

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
            if (_event == null || _lockUI)
                return;

            _event.Function = lstFunctions.SelectedItem?.Text ?? string.Empty;
            lblNotify.Visible = false;
        }

        private void nudCallCount_ValueChanged(object sender, EventArgs e)
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

        private void lblListNotify_Click(object sender, EventArgs e)
        {
            if (lblListNotify.Tag == null || (int)lblListNotify.Tag == 0)
                return;

            string result = LevelFileDialog.BrowseFile(this, _editor.Level.Settings, _editor.Level.Settings.TenLuaScriptFile,
               "Select the LUA script file for this level", new[] { new FileFormat("LUA script file", "lua") },
               VariableType.LevelDirectory, false);

            if (result != null)
            {
                _editor.Level.Settings.TenLuaScriptFile = result;
                _editor.ConfigurationChange();
            }
        }

        private void lblListNotify_EnabledChanged(object sender, EventArgs e)
        {
            lblListNotify.Visible = lblListNotify.Enabled;
        }

        private void butAddConditionNode_Click(object sender, EventArgs e)
        {
            nodeEditor.AddConditionNode(Control.ModifierKeys != Keys.None);
        }

        private void butAddActionNode_Click(object sender, EventArgs e)
        {
            nodeEditor.AddActionNode(Control.ModifierKeys != Keys.None);
        }

        private void butClearNodes_Click(object sender, EventArgs e)
        {
            if (DarkMessageBox.Show(FindForm(), "Do you really want to delete all nodes?", "Delete all nodes",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            nodeEditor.ClearNodes();
        }

        private void butDeleteNode_Click(object sender, EventArgs e)
        {
            nodeEditor.DeleteNodes();
        }

        private void butRenameNode_Click(object sender, EventArgs e)
        {
            if (nodeEditor.SelectedNode == null)
                return;

            using (var form = new FormInputBox("Edit node name", "Enter new name for this node:", nodeEditor.SelectedNode.Name))
            {
                if (form.ShowDialog(FindForm()) == DialogResult.Cancel)
                    return;

                nodeEditor.SelectedNode.Name = form.Result;
                nodeEditor.Refresh();
            }
        }

        private void butChangeNodeColor_Click(object sender, EventArgs e)
        {
            if (nodeEditor.SelectedNode == null)
                return;

            using (var colorDialog = new RealtimeColorDialog())
            {
                var oldColor = nodeEditor.SelectedNode.Color.ToWinFormsColor();
                colorDialog.Color = oldColor;
                colorDialog.FullOpen = true;
                if (colorDialog.ShowDialog(this) != DialogResult.OK)
                    return;

                if (oldColor != colorDialog.Color)
                {
                    nodeEditor.SelectedNode.Color = colorDialog.Color.ToFloat3Color();
                    nodeEditor.Refresh();
                }
            }
        }
    }
}
