using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormVolume : DarkForm
    {
        private readonly VolumeInstance _instance;
        private readonly Editor _editor;

        private bool _lockUI = false;
        private bool _genericMode = false;

        private List<VolumeEventSet> _backupEventSetList;
        private List<int> _backupEventSetIndices;

        public FormVolume(VolumeInstance instance)
        {
            InitializeComponent();

            _genericMode = instance == null;

            _instance = _genericMode ? new BoxVolumeInstance() : instance;
            _editor = Editor.Instance;

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);

            // Backup event set list
            BackupEventSets();

            // Populate function lists
            tmEnter.Initialize(_editor);
            tmInside.Initialize(_editor);
            tmLeave.Initialize(_editor);

            // Determine editing mode
            SetupUI();

            // Populate and select event set list
            PopulateEventSetList();
            FindAndSelectEventSet();
        }

        private void SetupUI()
        {
            if (!_genericMode)
                return;

            butSearch.Location = butUnassignEventSet.Location;
            butUnassignEventSet.Visible = false;
            Text = "Event set editor";
        }

        private void BackupEventSets()
        {
            _backupEventSetIndices = new List<int>();
            foreach (var vol in _editor.Level.GetAllObjects().OfType<VolumeInstance>())
                _backupEventSetIndices.Add(_editor.Level.Settings.EventSets.IndexOf(vol.EventSet));

            _backupEventSetList = new List<VolumeEventSet>();
            foreach (var evt in _editor.Level.Settings.EventSets)
                _backupEventSetList.Add(evt.Clone());
        }

        private void RestoreEventSets()
        {
            _editor.Level.Settings.EventSets = _backupEventSetList;

            var volumes = _editor.Level.GetAllObjects().OfType<VolumeInstance>().ToList();
            for (int i = 0; i < volumes.Count; i++)
            {
                if (_backupEventSetIndices[i] >= 0)
                    volumes[i].EventSet = _editor.Level.Settings.EventSets[_backupEventSetIndices[i]];
                else
                    volumes[i].EventSet = null; // Paranoia
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

        private void LoadEventSetIntoUI()
        {
            if (_instance.EventSet == null)
                return;

            UpdateUI();

            _lockUI = true;

            cbActivatorLara.Checked = (_instance.EventSet.Activators & VolumeActivators.Player) != 0;
            cbActivatorNPC.Checked = (_instance.EventSet.Activators & VolumeActivators.NPCs) != 0;
            cbActivatorOtherMoveables.Checked = (_instance.EventSet.Activators & VolumeActivators.OtherMoveables) != 0;
            cbActivatorStatics.Checked = (_instance.EventSet.Activators & VolumeActivators.Statics) != 0;
            cbActivatorFlyBy.Checked = (_instance.EventSet.Activators & VolumeActivators.Flybys) != 0;

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
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            RestoreEventSets();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void lstEvents_SelectedIndicesChanged(object sender, EventArgs e)
        {
            UpdateUI();

            if (lstEvents.SelectedItem == null)
                return;

            _instance.EventSet = lstEvents.SelectedItem.Tag as VolumeEventSet;
            LoadEventSetIntoUI();
        }

        private void butNewEventSet_Click(object sender, EventArgs e)
        {
            var newSet = new VolumeEventSet() { Name = "New event set " + lstEvents.Items.Count };
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
    }
}
