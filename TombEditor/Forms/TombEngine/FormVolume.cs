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
                }
        }

        private void LoadEventSetIntoUI()
        {
            bool eventAvailable = _instance.EventSet != null;
            grpActivators.Enabled = tcEvents.Enabled = eventAvailable;

            if (!eventAvailable)
                return;

            cbActivatorLara.Checked = (_instance.EventSet.Activators & VolumeActivators.Player) != 0;
            cbActivatorNPC.Checked = (_instance.EventSet.Activators & VolumeActivators.NPCs) != 0;
            cbActivatorOtherMoveables.Checked = (_instance.EventSet.Activators & VolumeActivators.OtherMoveables) != 0;
            cbActivatorStatics.Checked = (_instance.EventSet.Activators & VolumeActivators.Statics) != 0;
            cbActivatorFlyBy.Checked = (_instance.EventSet.Activators & VolumeActivators.Flybys) != 0;

            tmEnter.Event = _instance.EventSet.OnEnter;
            tmInside.Event = _instance.EventSet.OnInside;
            tmLeave.Event = _instance.EventSet.OnLeave;
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            _instance.EventSet.Activators = 0 | 
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

        private void lstEvents_SelectedIndicesChanged(object sender, EventArgs e)
        {
            _instance.EventSet = lstEvents.SelectedItem.Tag as VolumeEventSet;
            LoadEventSetIntoUI();
        }
    }
}
