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

                nodeEditor.ViewPositionChanged -= NodeEditor_ViewPostionChanged;
                nodeEditor.SelectionChanged -= NodeEditor_SelectionChanged;
                nodeEditor.LocatedItemFound -= NodeEditor_LocatedItemFound;

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

            nodeEditor.Initialize(editor.Level);
            UpdateNodeEditorOptions();

            nodeEditor.ViewPositionChanged += NodeEditor_ViewPostionChanged;
            nodeEditor.SelectionChanged += NodeEditor_SelectionChanged;
            nodeEditor.LocatedItemFound += NodeEditor_LocatedItemFound;
        }

        private void NodeEditor_LocatedItemFound(object sender, EventArgs e)
        {
            if (sender is PositionBasedObjectInstance)
                _editor.ShowObject(sender as PositionBasedObjectInstance);
        }

        private void NodeEditor_SelectionChanged(object sender, EventArgs e)
        {
            UpdateNodeEditorControls();
        }

        private void NodeEditor_ViewPostionChanged(object sender, EventArgs e)
        {
            if (_event != null)
                _event.NodePosition = nodeEditor.ViewPosition;
        }

        private void UpdateNodeEditorOptions()
        {
            nodeEditor.GridSize = _editor.Configuration.NodeEditor_Size;
            nodeEditor.GridStep = _editor.Configuration.NodeEditor_GridStep;
            nodeEditor.DefaultNodeWidth = _editor.Configuration.NodeEditor_DefaultNodeWidth;
            nodeEditor.LinksAsRopes = _editor.Configuration.NodeEditor_LinksAsRopes;
        }

        private void SelectTriggerMode()
        {
            tabbedContainer.SelectedIndex = rbLevelScript.Checked ? 1 : 0;

            UpdateNodeEditorControls();

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
            butClearNodes.Enabled = nodeEditor.LinearizedNodes().Count > 0;
            butLinkSelectedNodes.Enabled = nodeEditor.SelectedNodes.Count > 1;

            butUnassign.Visible = rbLevelScript.Checked;
        }

        private void ReloadFunctions()
        {
            lblListNotify.ForeColor = Colors.DisabledText;
            lstFunctions.Items.Clear();

            nodeEditor.NodeFunctions.Clear();
            nodeEditor.NodeFunctions.AddRange(
                ScriptingUtils.GetAllNodeFunctions(_editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.TenNodeScriptFile)));

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
                    var functions = ScriptingUtils.GetAllFunctionNames(path);
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
            if (rbLevelScript.Checked)
            {
                var searchPopUp = new PopUpSearch(lstFunctions) { ShowAboveControl = true };
                searchPopUp.Show(this);
            }
            else if (rbNodeEditor.Checked)
            {
                using (var form = new FormInputBox("Find node", "Enter name of the node to find:", nodeEditor.SelectedNode?.Name ?? string.Empty))
                {
                    if (form.ShowDialog(FindForm()) == DialogResult.Cancel)
                        return;

                    nodeEditor.FindNodeByName(form.Result);
                }
            }
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
            nodeEditor.AddConditionNode(Control.ModifierKeys != Keys.Control, Control.ModifierKeys == Keys.Alt);
        }

        private void butAddActionNode_Click(object sender, EventArgs e)
        {
            nodeEditor.AddActionNode(Control.ModifierKeys != Keys.Control, Control.ModifierKeys == Keys.Alt);
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

            using (var colorDialog = new RealtimeColorDialog(-1, -1, c =>
            {
                nodeEditor.SelectedNode.Color = c.ToFloat3Color();
                nodeEditor.Refresh();
            }))
            {
                var oldColor = nodeEditor.SelectedNode.Color.ToWinFormsColor();
                colorDialog.Color = oldColor;
                colorDialog.FullOpen = true;
                if (colorDialog.ShowDialog(this) != DialogResult.OK)
                {
                    nodeEditor.SelectedNode.Color = oldColor.ToFloat3Color();
                    nodeEditor.Invalidate();
                    return;
                }

                if (oldColor != colorDialog.Color)
                {
                    nodeEditor.SelectedNode.Color = colorDialog.Color.ToFloat3Color();
                    nodeEditor.Invalidate();
                }
            }
        }

        private void butLinkSelectedNodes_Click(object sender, EventArgs e)
        {
            nodeEditor.LinkSelectedNodes();
        }

        public void ProcessKey(Keys keyCode)
        {
            if (_event == null)
                return;

            if (rbLevelScript.Checked)
            {
                switch (keyCode)
                {
                    case Keys.Delete:
                    case Keys.Back:
                        lstFunctions.ClearSelection();
                        break;
                }
            }
            else
            {
                switch (keyCode)
                {
                    case Keys.Delete:
                    case Keys.Back:
                        nodeEditor.DeleteNodes();
                        break;

                    case (Keys.Shift | Keys.Delete):
                    case (Keys.Shift | Keys.Back):
                        nodeEditor.ClearNodes();
                        break;

                    case Keys.N:
                    case Keys.A:
                        nodeEditor.AddActionNode(true, false);
                        break;

                    case Keys.C:
                        nodeEditor.AddConditionNode(true, false);
                        break;

                    case (Keys.Shift | Keys.N):
                    case (Keys.Shift | Keys.A):
                    case (Keys.Alt | Keys.N):
                    case (Keys.Alt | Keys.A):
                        nodeEditor.AddActionNode(true, true);
                        break;

                    case (Keys.Shift | Keys.C):
                    case (Keys.Alt | Keys.C):
                        nodeEditor.AddConditionNode(true, true);
                        break;

                    case Keys.L:
                        nodeEditor.LinkSelectedNodes();
                        break;

                    case Keys.Escape:
                        nodeEditor.ClearSelection();
                        break;
                }
            }
        }

        private void butExport_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(ScriptingUtils.ParseNodes(nodeEditor.Nodes));
        }
    }
}
