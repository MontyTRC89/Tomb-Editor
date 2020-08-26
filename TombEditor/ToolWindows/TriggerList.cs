using DarkUI.Config;
using DarkUI.Controls;
using DarkUI.Docking;
using DarkUI.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor.ToolWindows
{
    public partial class TriggerList : DarkToolWindow
    {
        private readonly Editor _editor;

        public TriggerList()
        {
            InitializeComponent();
            CommandHandler.AssignCommandsToControls(Editor.Instance, this, toolTip);

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update level-specific UI
            if (obj is Editor.InitEvent ||
                obj is Editor.LevelChangedEvent ||
                obj is Editor.GameVersionChangedEvent)
            {
                if (_editor.Level.Settings.GameVersion == TRVersion.Game.TR5Main)
                    DockText = "Legacy triggers";
                else
                    DockText = "Triggers";
            }

            // Update the trigger control
            if (obj is Editor.SelectedSectorsChangedEvent ||
                obj is Editor.SelectedRoomChangedEvent ||
                obj is Editor.RoomSectorPropertiesChangedEvent)
            {
                lstTriggers.Items.Clear();
                bool noHighlight = true;

                if (_editor.Level != null && _editor.SelectedSectors.Valid)
                {
                    // Search for unique triggers inside the selected area
                    var triggers = new List<TriggerInstance>();
                    var area = _editor.SelectedSectors.Area;
                    for (int x = area.X0; x <= area.X1; x++)
                        for (int z = area.Y0; z <= area.Y1; z++)
                            foreach (var trigger in _editor.SelectedRoom.GetBlockTry(x, z)?.Triggers ?? new List<TriggerInstance>())
                                if (!triggers.Contains(trigger))
                                {
                                    noHighlight = x > area.X0 || z > area.Y0;
                                    triggers.Add(trigger);
                                }

                    if (triggers.Count == 1)
                        noHighlight = true;

                    if (triggers.Count > 0)
                    {
                        // Find setup trigger
                        var setupTrigger = TriggerInstance.GetSetupTrigger(triggers);

                        // Add triggers to listbox and highlight setup trigger
                        foreach (TriggerInstance trigger in triggers)
                            lstTriggers.Items.Add(new DarkListItem(trigger.ToShortString())
                            {
                                Tag = trigger,
                                TextColor = (!noHighlight && trigger == setupTrigger) ? 
                                                Colors.BlueHighlight.Multiply(1.2f) : Colors.LightText
                            });
                    }
                }
            }

            // Update any modified trigger from area
            if (obj is Editor.ObjectChangedEvent)
            {
                var changedObject = ((Editor.ObjectChangedEvent)obj).Object;

                if (changedObject.Room == _editor.SelectedRoom &&
                    changedObject is TriggerInstance)
                {
                    var item = lstTriggers.Items.FirstOrDefault(l => l.Tag == changedObject);
                    if (item != null)
                        item.Text = changedObject.ToShortString();
                }
            }

            // Update the trigger control selection
            if (obj is Editor.SelectedSectorsChangedEvent ||
                obj is Editor.SelectedRoomChangedEvent ||
                obj is Editor.SelectedObjectChangedEvent)
            {
                if (_editor.SelectedObject is TriggerInstance)
                {
                    var trigger = _editor.SelectedObject as TriggerInstance;
                    var entry = lstTriggers.Items.FirstOrDefault(t => t.Tag == trigger);
                    if (entry != null)
                    {
                        lstTriggers.SelectItem(lstTriggers.Items.IndexOf(entry));
                        lstTriggers.EnsureVisible();
                    }
                    else
                        lstTriggers.ClearSelection();
                }
            }

            // Update tooltip texts
            if (obj is Editor.ConfigurationChangedEvent)
            {
                if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
                    CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);
            }
        }

        private void DeleteTriggers()
        {
            if (_editor.SelectedRoom == null || lstTriggers.SelectedIndices.Count == 0)
                return;

            var triggersToRemove = new List<ObjectInstance>();
            foreach (var obj in lstTriggers.SelectedIndices)
            {
                var trigger = lstTriggers.Items[obj].Tag as ObjectInstance;
                if (trigger != null) 
                    triggersToRemove.Add(trigger);
            }

            EditorActions.DeleteObjects(triggersToRemove, FindForm());
        }


        private void lstTriggers_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                DeleteTriggers();
        }

        private void butDeleteTrigger_Click(object sender, EventArgs e)
        {
            DeleteTriggers();
        }

        private void butEditTrigger_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null || !(_editor.SelectedObject is TriggerInstance))
                return;
            EditorActions.EditObject(_editor.SelectedObject, this);
        }

        private void lstTriggers_SelectedIndicesChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null || lstTriggers.SelectedIndices.Count == 0)
                return;
            _editor.SelectedObject = (ObjectInstance)(lstTriggers.SelectedItem.Tag);
        }

        private void lstTriggers_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstTriggers.SelectedIndices.Count == 0)
                return;

            var instance = lstTriggers.SelectedItem.Tag as ObjectInstance;
            if (instance != null)
                EditorActions.EditObject(instance, this);
        }

        private void butAddTrigger_MouseEnter(object sender, EventArgs e)
        {
            if (_editor.Configuration.UI_AutoSwitchSectorColoringInfo)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Trigger);
        }
    }
}
