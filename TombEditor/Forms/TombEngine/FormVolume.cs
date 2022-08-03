using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

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

            // Set window property handlers
            Configuration.ConfigureWindow(this, _editor.Configuration);

            // Populate function lists
            tmEnter.Initialize(_editor);
            tmInside.Initialize(_editor);
            tmLeave.Initialize(_editor);

            // Populate and select event set list
            PopulateEventSetList();
            FindAndSelectEventSet();
        }

        private void PopulateEventSetList()
        {
            lstEvents.Items.Clear();

            foreach (var evtSet in _editor.Level.Settings.EventSets)
                lstEvents.Items.Add(new DarkUI.Controls.DarkListItem(evtSet.Name) { Tag = evtSet });
        }

        private void FindAndSelectEventSet()
        {
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

            cbActivatorLara.Checked = (_instance.EventSet.Activators & VolumeActivators.Player) != 0;
            cbActivatorNPC.Checked = (_instance.EventSet.Activators & VolumeActivators.NPCs) != 0;
            cbActivatorOtherMoveables.Checked = (_instance.EventSet.Activators & VolumeActivators.OtherMoveables) != 0;
            cbActivatorStatics.Checked = (_instance.EventSet.Activators & VolumeActivators.Statics) != 0;
            cbActivatorFlyBy.Checked = (_instance.EventSet.Activators & VolumeActivators.Flybys) != 0;

            tmEnter.Event = _instance.EventSet.OnEnter;
            tmInside.Event = _instance.EventSet.OnInside;
            tmLeave.Event = _instance.EventSet.OnLeave;
        }

        private void ModifyActivators()
        {
            if (_instance.EventSet == null)
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
            bool eventAvailable = _instance.EventSet != null;
            grpActivators.Enabled = tcEvents.Enabled = eventAvailable;
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
        }

        private void butCloneEventSet_Click(object sender, EventArgs e)
        {
            var clonedSet = _instance.EventSet.Clone();
            _editor.Level.Settings.EventSets.Add(clonedSet);
            _instance.EventSet = clonedSet;

            PopulateEventSetList();
            FindAndSelectEventSet();
        }

        private void butDeleteEventSet_Click(object sender, EventArgs e)
        {
            _editor.Level.Settings.EventSets.Remove(_instance.EventSet);
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
    }
}
