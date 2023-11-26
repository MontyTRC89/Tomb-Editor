using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormEventSetEditor : DarkForm
    {
        private enum SortMode
        {
            None,
            Ascending,
            Descending
        }

        private SortMode _nextSortMode = SortMode.Ascending;

        private VolumeInstance _instance;
        private readonly Editor _editor;

        private bool _lockUI = false;
        private bool _lockSelectionChange = false;

        private List<EventSet> _usedList;

        private List<EventSet> _backupEventSetList;
        private Dictionary<VolumeInstance, int> _backupVolumes;
        private bool[] _backupVolumeState = new bool[2];

        private List<TriggerNode> _clipboard;

        private readonly PopUpInfo _popup = new PopUpInfo();
        private readonly List<string> _scriptFuncs;

        public bool GlobalMode => _usedList == _editor.Level.Settings.GlobalEventSets;
        public bool GenericMode => GlobalMode || _instance == null;

        public EventSet SelectedSet
        {
            get 
            { 
                return _selectedSet; 
            }

            set
            {
                if (value != null && value == _selectedSet)
                    return;

                _selectedSet = value;

                if (_selectedSet == null)
                {
                    if (GenericMode && dgvEvents.Rows.Count > 0)
                        dgvEvents.Rows[0].Selected = true;
                    else
                    {
                        _lockSelectionChange = true;
                        dgvEvents.ClearSelection();
                        _lockSelectionChange = false;
                    }

                    return;
                }

                for (int i = 0; i < dgvEvents.Rows.Count; i++)
                {
                    if (dgvEvents.Rows[i].Tag == _selectedSet)
                    {
                        _lockSelectionChange = true;

                        dgvEvents.ClearSelection();
                        dgvEvents.Rows[i].Selected = true;

                        if (!GenericMode)
                            _instance.EventSet = _selectedSet;

                        LoadEventSetIntoUI(_selectedSet);

                        _lockSelectionChange = false;

                        return;
                    }
                }

                _selectedSet = null;
                dgvEvents.ClearSelection();
            }
        }
        private EventSet _selectedSet;

        public FormEventSetEditor(bool global, VolumeInstance instance = null)
        {
            InitializeComponent();
            dgvEvents.Columns.Add(new DataGridViewColumn(new DataGridViewTextBoxCell()) { HeaderText = "Event sets" });

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;

            _usedList = global ? _editor.Level.Settings.GlobalEventSets : _editor.Level.Settings.VolumeEventSets;
            _instance = instance;

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);

            // Backup event set list and volume state
            BackupState();

            // Populate function lists
            _scriptFuncs = ScriptingUtils.GetAllFunctionNames(_editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.TenLuaScriptFile));
            triggerManager.Initialize(_editor, ScriptingUtils.NodeFunctions, _scriptFuncs);

            // Determine editing mode
            SetupUI();

            // Don't select first event set if window is opened in generic mode
            _lockSelectionChange = !GenericMode;

            // Populate and select event set list
            PopulateEventSetList();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Resize splitter
            splitContainer.SplitterDistance = _editor.Configuration.Window_FormEventSetEditor_SplitterDistance;
        }

        private void dgvEvents_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            switch (_nextSortMode)
            {
                case SortMode.Ascending:
                    dgvEvents.AllowUserToDragDropRows = false;

                    dgvEvents.Sort(dgvEvents.Columns[e.ColumnIndex], ListSortDirection.Ascending);
                    dgvEvents.Columns[e.ColumnIndex].HeaderText += " ▲";
                    _nextSortMode = SortMode.Descending;
                    break;

                case SortMode.Descending:
                    dgvEvents.AllowUserToDragDropRows = false;

                    dgvEvents.Sort(dgvEvents.Columns[e.ColumnIndex], ListSortDirection.Descending);
                    dgvEvents.Columns[e.ColumnIndex].HeaderText = dgvEvents.Columns[e.ColumnIndex].HeaderText.TrimEnd('▲', ' ');
                    dgvEvents.Columns[e.ColumnIndex].HeaderText += " ▼";
                    _nextSortMode = SortMode.None;
                    break;

                default:
                    dgvEvents.AllowUserToDragDropRows = true;

                    object selectedEventCache = dgvEvents.SelectedRows.Count > 0 ? dgvEvents.SelectedRows[0].Tag : null;
                    PopulateEventSetList();
                    dgvEvents.ClearSelection();

                    if (selectedEventCache != null)
                        foreach (DataGridViewRow row in dgvEvents.Rows)
                            if (row.Tag == selectedEventCache)
                                row.Selected = true;

                    dgvEvents.Columns[e.ColumnIndex].HeaderText = dgvEvents.Columns[e.ColumnIndex].HeaderText.TrimEnd('▼', ' ');
                    _nextSortMode = SortMode.Ascending;
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= EditorEventRaised;

                if (DialogResult == DialogResult.Cancel)
                    RestoreState();

                _editor.EventSetsChange();
            }

            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        public void ChangeVolume(VolumeInstance instance)
        {
            if (GlobalMode)
                return;

            _instance = instance;

            SetupUI();
            SelectedSet = (_instance?.EventSet ?? _selectedSet);
        }

        public void UpdateVolume()
        {
            // Don't update dummy or yet not placed volumes.
            if (_instance == null || _instance.Room == null)
                return;

            _editor.ObjectChange(_instance, ObjectChangeType.Change);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            if (obj is Editor.MessageEvent)
            {
                var msg = (Editor.MessageEvent)obj;
                PopUpInfo.Show(_popup, msg.ForceInMainWindow ? null : FindForm(), triggerManager, msg.Message, msg.Type);
            }

            if (obj is Editor.LevelChangedEvent)
                Close();

            if (obj is Editor.SelectedObjectChangedEvent)
                ChangeVolume(_editor.SelectedObject as VolumeInstance);

            if (obj is Editor.EventSetsChangedEvent)
            {
                PopulateEventSetList();
                SelectedSet = (_instance?.EventSet ?? null);
            }
        }

        private void SetupUI()
        {
            PopulateEventTypeList();

            if (GenericMode)
            {
                butSearch.Location = butUnassignEventSet.Location;
                butUnassignEventSet.Visible = cbEnableVolume.Visible = cbAdjacentRooms.Visible = false;
                Text = "Edit " + (GlobalMode ? "global" : "volume") + " event sets";
            }
            else
            {
                butSearch.Location = new Point(butUnassignEventSet.Location.X - butSearch.Width - 6, butSearch.Location.Y);
                butUnassignEventSet.Visible = cbEnableVolume.Visible = cbAdjacentRooms.Visible = true;
                cbEnableVolume.Checked = _instance.Enabled;
                cbAdjacentRooms.Checked = _instance.DetectInAdjacentRooms;
                Text = "Edit volume: " + _instance.ToShortString();
            }
        }

        private void SetEventTooltip()
        {
            switch (cbEvents.SelectedItem)
            {
                case EventType.OnVolumeEnter:
                    toolTip.SetToolTip(cbEvents, "Occurs when something enters assigned volume. \nThis event performs once.");
                    break;

                case EventType.OnVolumeInside:
                    toolTip.SetToolTip(cbEvents, "Occurs when something resides inside assigned volume. \nThis event performs continuously.");
                    break;

                case EventType.OnVolumeLeave:
                    toolTip.SetToolTip(cbEvents, "Occurs when something leaves assigned volume. \nThis event performs once.");
                    break;

                case EventType.OnLevelEnd:
                    toolTip.SetToolTip(cbEvents, "Occurs when level was finished. \nThis event performs once.");
                    break;

                case EventType.OnLevelStart:
                    toolTip.SetToolTip(cbEvents, "Occurs when new level starts. \nThis event performs once.");
                    break;

                case EventType.OnLoadGame:
                    toolTip.SetToolTip(cbEvents, "Occurs when game was just loaded from savegame. \nThis event performs once.");
                    break;

                case EventType.OnSaveGame:
                    toolTip.SetToolTip(cbEvents, "Occurs when game was just saved. \nThis event performs once.");
                    break;

                case EventType.OnLoop:
                    toolTip.SetToolTip(cbEvents, "Occurs every game frame, except menus. \nThis event performs continuously.");
                    break;
            }
        }

        private void BackupState()
        {
            if (!GlobalMode)
            {
                if (!GenericMode)
                {
                    _backupVolumeState[0] = _instance.Enabled;
                    _backupVolumeState[1] = _instance.DetectInAdjacentRooms;
                }

                _backupVolumes = new Dictionary<VolumeInstance, int>();
                foreach (var vol in _editor.Level.GetAllObjects().OfType<VolumeInstance>())
                    _backupVolumes.Add(vol, _editor.Level.Settings.VolumeEventSets.IndexOf(vol.EventSet));
            }

            _backupEventSetList = new List<EventSet>();
            foreach (var evtSet in _usedList)
                _backupEventSetList.Add(evtSet.Clone());
        }

        private void RestoreState()
        {
            if (GlobalMode)
            {
                _editor.Level.Settings.GlobalEventSets = _backupEventSetList;
            }
            else
            {
                _editor.Level.Settings.VolumeEventSets = _backupEventSetList;

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

                if (!GenericMode)
                {
                    _instance.Enabled = _backupVolumeState[0];
                    _instance.DetectInAdjacentRooms = _backupVolumeState[1];
                }
            }
        }

        private void PopulateEventTypeList()
        {
            cbEvents.Items.Clear();

            foreach (var eventType in (GlobalMode ? Event.GlobalEventTypes : Event.VolumeEventTypes))
                cbEvents.Items.Add(eventType);
        }

        private void PopulateEventSetList()
        {
            _lockSelectionChange = true;

            dgvEvents.Rows.Clear();

            foreach (var evtSet in _usedList)
            {
                var row = new DataGridViewRow { Tag = evtSet };
                row.Cells.Add(new DataGridViewTextBoxCell() { Value = evtSet.Name });
                dgvEvents.Rows.Add(row);
            }

            _lockSelectionChange = false;
        }

		private void ReplaceEventSetNames(string oldName, string newName)
		{
			foreach (var set in _usedList)
				foreach (var evt in set.Events)
					foreach (var node in TriggerNode.LinearizeNodes(evt.Value.Nodes))
					{
						var func = ScriptingUtils.NodeFunctions.FirstOrDefault(f => f.Signature == node.Function && 
												             f.Arguments.Any(a => a.Type == ArgumentType.EventSets));
						if (func == null)
							continue;

						for (int i = 0; i < func.Arguments.Count; i++)
						{
							if (func.Arguments[i].Type == ArgumentType.EventSets &&
								node.Arguments.Count > i &&
								TextExtensions.Unquote(node.Arguments[i]) == oldName)
							{
								node.Arguments[i] = TextExtensions.Quote(newName);
							}
						}

					}
		}

        private void LoadEventSetIntoUI(EventSet newEventSet)
        {
            if (!GenericMode)
                _instance.EventSet = newEventSet;

            UpdateUI();

            _lockUI = true;

            if (!GlobalMode)
            {
                var evtSet = newEventSet as VolumeEventSet;

                cbActivatorLara.Checked = (evtSet.Activators & VolumeActivators.Player) != 0;
                cbActivatorNPC.Checked = (evtSet.Activators & VolumeActivators.NPCs) != 0;
                cbActivatorOtherMoveables.Checked = (evtSet.Activators & VolumeActivators.OtherMoveables) != 0;
                cbActivatorStatics.Checked = (evtSet.Activators & VolumeActivators.Statics) != 0;
                cbActivatorFlyBy.Checked = (evtSet.Activators & VolumeActivators.Flybys) != 0;
            }

            cbEvents.SelectedItem = newEventSet.LastUsedEvent;
            triggerManager.Event = newEventSet.Events[newEventSet.LastUsedEvent];

            tbName.Text = newEventSet.Name;

            _lockUI = false;
        }

        private void ModifyActivators()
        {
            if (GlobalMode || SelectedSet == null || _lockUI)
                return;

            (SelectedSet as VolumeEventSet).Activators = 0 |
                                            (cbActivatorLara.Checked ? VolumeActivators.Player : 0) |
                                            (cbActivatorNPC.Checked ? VolumeActivators.NPCs : 0) |
                                            (cbActivatorOtherMoveables.Checked ? VolumeActivators.OtherMoveables : 0) |
                                            (cbActivatorStatics.Checked ? VolumeActivators.Statics : 0) |
                                            (cbActivatorFlyBy.Checked ? VolumeActivators.Flybys : 0);
        }

        private void UpdateUI()
        {
            bool eventSetSelected = SelectedSet != null;

            tbName.Enabled =
            triggerManager.Enabled =
            cbEvents.Enabled =
            butUnassignEventSet.Enabled =
            butCloneEventSet.Enabled =
            butDeleteEventSet.Enabled = eventSetSelected;

            grpActivators.Enabled = eventSetSelected && !GlobalMode;

            butSearch.Enabled = dgvEvents.Rows.Count > 0;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void dgvEvents_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (_lockSelectionChange)
                return;

            var newEventSet = dgvEvents.SelectedRows.Count == 0 ? null : dgvEvents.SelectedRows[0].Tag as EventSet;
            SelectedSet = newEventSet;

            UpdateUI();
        }

        private void butNewEventSet_Click(object sender, EventArgs e)
        {
            var name = "New " + (GlobalMode ? "global" : "volume") + " event set " + (dgvEvents.Rows.Count + 1).ToString();

            EventSet newSet;

            if (GlobalMode)
            {
                newSet = new GlobalEventSet() { Name = name };
            }
            else
            {
                newSet = new VolumeEventSet()
                {
                    Name = name,
                    LastUsedEvent = (EventType)_editor.Configuration.NodeEditor_DefaultEventToEdit
                };
            }

            foreach (var evt in newSet.Events)
                evt.Value.Mode = (EventSetMode)_editor.Configuration.NodeEditor_DefaultEventMode;

            _usedList.Add(newSet);

            PopulateEventSetList();
            SelectedSet = newSet;

            tbName.Focus();
        }

        private void butCloneEventSet_Click(object sender, EventArgs e)
        {
            if (SelectedSet == null)
                return;

            var clonedSet = SelectedSet.Clone();
            clonedSet.Name = SelectedSet.Name + " (copy)";
            _usedList.Add(clonedSet);

            PopulateEventSetList();
            SelectedSet = clonedSet;
        }

        private void butDeleteEventSet_Click(object sender, EventArgs e)
        {
            EditorActions.DeleteEventSet(SelectedSet);

            if (!GenericMode)
                _instance.EventSet = null;

            PopulateEventSetList();
            dgvEvents.ClearSelection();

            if (dgvEvents.Rows.Count > 0)
            {
                DataGridViewRow lastRow = dgvEvents.Rows[^1];
                lastRow.Selected = true;
            }
            else
                UpdateUI();
        }

        private void butUnassignEventSet_Click(object sender, EventArgs e)
        {
            if (GenericMode)
                return;

            _instance.EventSet = null;
            dgvEvents.ClearSelection();
        }

        private void cbActivators_CheckedChanged(object sender, EventArgs e)
        {
            ModifyActivators();
        }

        private void butSearch_Click(object sender, EventArgs e)
        {
            var searchPopUp = new PopUpSearch(dgvEvents) { ShowAboveControl = true };
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

            switch (keyData)
            {
                case (Keys.Control | Keys.C):
                    var copiedNodes = triggerManager.CopyNodes(false);
                    if (copiedNodes.Count > 0)
                    {
                        _clipboard = copiedNodes;
                        _editor.SendMessage("Selected nodes are copied to clipboard.", PopupType.Info);
                    }
                    break;

                case (Keys.Control | Keys.X):
                    _clipboard = triggerManager.CopyNodes(true);
                    break;

                case (Keys.Control | Keys.V):
                    triggerManager.PasteNodes(_clipboard);
                    break;

                default:
                    triggerManager.ProcessKey(keyData);
                    break;

            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void cbEvents_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedSet == null)
                return;

            SetEventTooltip();

            if (!_lockUI)
            {
                SelectedSet.LastUsedEvent = (EventType)cbEvents.SelectedItem;
                triggerManager.Event = SelectedSet.Events[SelectedSet.LastUsedEvent];
            }
        }

        private void cbEvents_Format(object sender, ListControlConvertEventArgs e)
        {
            if (e.ListItem != null && e.ListItem is EventType)
                e.Value = e.ListItem.ToString().SplitCamelcase();
        }

        private void tbName_Validated(object sender, EventArgs e)
		{
			if (SelectedSet == null || _lockUI)
				return;

			if (SelectedSet.Name == tbName.Text)
				return;

			ReplaceEventSetNames(SelectedSet.Name, tbName.Text);
			dgvEvents.SelectedCells[0].Value = SelectedSet.Name = tbName.Text;
			_editor.EventSetsChange();
		}

        private void dgvEvents_DragDrop(object sender, DragEventArgs e)
        {
            _usedList.Clear();

            foreach (DataGridViewRow row in dgvEvents.Rows)
            {
                if (row.Tag is not VolumeEventSet evtSet)
                    continue;

                _usedList.Add(evtSet);
            }
        }

        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (Visible)
                _editor.Configuration.Window_FormEventSetEditor_SplitterDistance = splitContainer.SplitterDistance;
        }

        private void cbEnableVolume_CheckedChanged(object sender, EventArgs e)
        {
            _instance.Enabled = cbEnableVolume.Checked;
            UpdateVolume();
        }

        private void cbAdjacentRooms_CheckedChanged(object sender, EventArgs e)
        {
            _instance.DetectInAdjacentRooms = cbAdjacentRooms.Checked;
            UpdateVolume();
        }
    }
}
