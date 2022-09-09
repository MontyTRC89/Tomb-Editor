using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Controls;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormVolume : DarkForm
    {
        private VolumeInstance _instance;
        private readonly Editor _editor;

        private bool _lockUI = false;
        private bool _genericMode = false;

        private List<VolumeEventSet> _backupEventSetList;
        private Dictionary<VolumeInstance, int> _backupVolumes;

        private List<TriggerNode> _clipboard;

        private readonly PopUpInfo _popup = new PopUpInfo();

        public FormVolume(VolumeInstance instance)
        {
            InitializeComponent();

            _genericMode = instance == null;

            _instance = _genericMode ? new BoxVolumeInstance() : instance;
            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);

            // Backup event set list
            BackupEventSets();

            // Populate function lists
            var nodeFuncs = ScriptingUtils.GetAllNodeFunctions(ScriptingUtils.NodeScriptPath);
            var scriptFuncs = ScriptingUtils.GetAllFunctionNames(_editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.TenLuaScriptFile));
            tmEnter.Initialize(_editor, nodeFuncs, scriptFuncs);
            tmInside.Initialize(_editor, nodeFuncs, scriptFuncs);
            tmLeave.Initialize(_editor, nodeFuncs, scriptFuncs);

            // Determine editing mode
            SetupUI();

            // Populate and select event set list
            PopulateEventSetList();
            FindAndSelectEventSet();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;

            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        public void ChangeVolume(VolumeInstance instance)
        {
            if (instance == null && _genericMode)
                return;

            _genericMode = instance == null;
            _instance = _genericMode ? new BoxVolumeInstance() { EventSet = _instance?.EventSet ?? null } : instance;

            FindAndSelectEventSet();
            SetupUI();
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.MessageEvent)
            {
                var msg = (Editor.MessageEvent)obj;
                PopUpInfo.Show(_popup, msg.ForceInMainWindow ? null : FindForm(), tcEvents, msg.Message, msg.Type);
            }

            if (obj is Editor.LevelChangedEvent)
                Close();

            if (obj is Editor.SelectedObjectChangedEvent)
                ChangeVolume(_editor.SelectedObject as VolumeInstance);

            if (obj is Editor.EventSetsChangedEvent)
            {
                PopulateEventSetList();
                FindAndSelectEventSet();
            }
        }

        private void SetupUI()
        {
            if (_genericMode)
            {
                butSearch.Location = butUnassignEventSet.Location;
                butUnassignEventSet.Visible = false;
                Text = "Edit volumes";
            }
            else
            {
                butSearch.Location = new Point(butUnassignEventSet.Location.X - butSearch.Width - 6, butSearch.Location.Y);
                butUnassignEventSet.Visible = true;
                Text = "Edit volume: " + _instance.ToShortString();
            }
        }

        private void BackupEventSets()
        {
            _backupVolumes = new Dictionary<VolumeInstance, int>();
            foreach (var vol in _editor.Level.GetAllObjects().OfType<VolumeInstance>())
                _backupVolumes.Add(vol, _editor.Level.Settings.EventSets.IndexOf(vol.EventSet));

            _backupEventSetList = new List<VolumeEventSet>();
            foreach (var evt in _editor.Level.Settings.EventSets)
                _backupEventSetList.Add(evt.Clone());
        }

        private void RestoreEventSets()
        {
            _editor.Level.Settings.EventSets = _backupEventSetList;

            var volumes = _editor.Level.GetAllObjects().OfType<VolumeInstance>().ToList();

            foreach (var vol in volumes)
            {
                if (!_backupVolumes.ContainsKey(vol))
                    continue;

                int index = -1;
                var entry = _backupVolumes.TryGetValue(vol, out index);
                if (index >= 0)
                    vol.EventSet = _backupEventSetList[index];
            }
        }

        private void PopulateEventSetList()
        {
            lstEvents.Items.Clear();

            foreach (var evtSet in _editor.Level.Settings.EventSets)
                lstEvents.Items.Add(new DarkUI.Controls.DarkListItem(evtSet.Name) { Tag = evtSet });
        }

        private void FindAndSelectEventSet()
        {
            if (_instance.EventSet == null)
            {
                if (_genericMode && lstEvents.Items.Count > 0)
                    lstEvents.SelectItem(0);
                else
                    lstEvents.ClearSelection();
                return;
            }

            for (int i = 0; i < lstEvents.Items.Count; i++)
                if (lstEvents.Items[i].Tag == _instance.EventSet)
                {
                    lstEvents.ClearSelection();
                    lstEvents.SelectItem(i);
                    return;
                }

            lstEvents.ClearSelection();
        }

        private void LoadEventSetIntoUI(VolumeEventSet newEventSet)
        {
            _instance.EventSet = newEventSet;

            UpdateUI();

            if (_instance.EventSet == null)
                return;

            if (_instance.EventSet.OnEnter  == tmEnter.Event  &&
                _instance.EventSet.OnInside == tmInside.Event &&
                _instance.EventSet.OnLeave  == tmLeave.Event)
                return;

            _lockUI = true;

            cbActivatorLara.Checked = (_instance.EventSet.Activators & VolumeActivators.Player) != 0;
            cbActivatorNPC.Checked = (_instance.EventSet.Activators & VolumeActivators.NPCs) != 0;
            cbActivatorOtherMoveables.Checked = (_instance.EventSet.Activators & VolumeActivators.OtherMoveables) != 0;
            cbActivatorStatics.Checked = (_instance.EventSet.Activators & VolumeActivators.Statics) != 0;
            cbActivatorFlyBy.Checked = (_instance.EventSet.Activators & VolumeActivators.Flybys) != 0;

            // A hack to prevent respawn for non-visible event tabs
            tmEnter.Event = tmInside.Event = tmLeave.Event = null;
            tcEvents.SelectedIndex = _instance.EventSet.LastUsedEventIndex;
            tcEvents.Invalidate();

            tmEnter.Event = _instance.EventSet.OnEnter;
            tmInside.Event = _instance.EventSet.OnInside;
            tmLeave.Event = _instance.EventSet.OnLeave;

            tbName.Text = _instance.EventSet.Name;

            _lockUI = false;
        }

        private void ModifyActivators()
        {
            if (_instance.EventSet == null || _lockUI)
                return;

            _instance.EventSet.Activators = 0 |
                                            (cbActivatorLara.Checked ? VolumeActivators.Player : 0) |
                                            (cbActivatorNPC.Checked ? VolumeActivators.NPCs : 0) |
                                            (cbActivatorOtherMoveables.Checked ? VolumeActivators.OtherMoveables : 0) |
                                            (cbActivatorStatics.Checked ? VolumeActivators.Statics : 0) |
                                            (cbActivatorFlyBy.Checked ? VolumeActivators.Flybys : 0);
        }

        private void UpdateUI()
        {
            tbName.Enabled =
            grpActivators.Enabled =
            tcEvents.Enabled =
            butUnassignEventSet.Enabled = _instance.EventSet != null;

            butCloneEventSet.Enabled =
            butDeleteEventSet.Enabled = lstEvents.SelectedItem != null;

            butSearch.Enabled = lstEvents.Items.Count > 0;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
            _editor.EventSetsChange();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
            RestoreEventSets();
            _editor.EventSetsChange();
        }

        private void lstEvents_SelectedIndicesChanged(object sender, EventArgs e)
        {
            UpdateUI();

            if (lstEvents.SelectedItem == null)
                return;

            var newEventSet = lstEvents.SelectedItem.Tag as VolumeEventSet;

            LoadEventSetIntoUI(newEventSet);
        }

        private void butNewEventSet_Click(object sender, EventArgs e)
        {
            var newSet = new VolumeEventSet()
            {
                Name = "New event set " + lstEvents.Items.Count,
                LastUsedEventIndex = _editor.Configuration.NodeEditor_DefaultEventToEdit
            };

            newSet.OnEnter.Mode  =
            newSet.OnInside.Mode =
            newSet.OnLeave.Mode  = (VolumeEventMode)_editor.Configuration.NodeEditor_DefaultEventMode;

            _editor.Level.Settings.EventSets.Add(newSet);
            _instance.EventSet = newSet;

            PopulateEventSetList();
            FindAndSelectEventSet();

            tbName.Focus();
        }

        private void butCloneEventSet_Click(object sender, EventArgs e)
        {
            if (_instance.EventSet == null)
                return;

            var clonedSet = _instance.EventSet.Clone();
            clonedSet.Name = _instance.EventSet.Name + " (copy)";
            _editor.Level.Settings.EventSets.Add(clonedSet);
            _instance.EventSet = clonedSet;

            PopulateEventSetList();
            FindAndSelectEventSet();
        }

        private void butDeleteEventSet_Click(object sender, EventArgs e)
        {
            EditorActions.DeleteEventSet(_instance.EventSet);
            _instance.EventSet = null;

            PopulateEventSetList();
        }

        private void butUnassignEventSet_Click(object sender, EventArgs e)
        {
            _instance.EventSet = null;
            lstEvents.ClearSelection();
        }

        private void cbActivators_CheckedChanged(object sender, EventArgs e)
        {
            ModifyActivators();
        }

        private void tbName_TextChanged(object sender, EventArgs e)
        {
            if (_instance.EventSet == null || _lockUI)
                return;

            _instance.EventSet.Name = lstEvents.SelectedItem.Text = tbName.Text;
        }

        private void butSearch_Click(object sender, EventArgs e)
        {
            var searchPopUp = new PopUpSearch(lstEvents) { ShowAboveControl = true };
            searchPopUp.Show(this);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Don't process reserved camera keys
            if (WinFormsUtils.DirectionalCameraKeys.Contains(keyData))
                return base.ProcessCmdKey(ref msg, keyData);

            // Don't process one-key and shift hotkeys if we're focused on control which allows text input
            if (WinFormsUtils.CurrentControlSupportsInput(this, keyData))
                return base.ProcessCmdKey(ref msg, keyData);

            // HACK: Because WinForms has no proper way of translating keyboard events to nested
            // controls, we need to introduce this helper function to translate pressed key info
            // to currently active trigger manager.

            TriggerManager manager = null;

            switch (tcEvents.SelectedIndex)
            {
                case 0:
                    manager = tmEnter;
                    break;
                case 1:
                    manager = tmInside;
                    break;
                case 2:
                    manager = tmLeave;
                    break;
            }

            switch (keyData)
            {
                case (Keys.Control | Keys.C):
                    var copiedNodes = manager.CopyNodes(false);
                    if (copiedNodes.Count > 0)
                    {
                        _clipboard = copiedNodes;
                        _editor.SendMessage("Selected nodes are copied to clipboard.", PopupType.Info);
                    }
                    break;

                case (Keys.Control | Keys.X):
                    _clipboard = manager.CopyNodes(true);
                    break;

                case (Keys.Control | Keys.V):
                    manager.PasteNodes(_clipboard);
                    break;

                default:
                    manager.ProcessKey(keyData);
                    break;

            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void tcEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            _instance.EventSet.LastUsedEventIndex = tcEvents.SelectedIndex;
        }
    }
}
