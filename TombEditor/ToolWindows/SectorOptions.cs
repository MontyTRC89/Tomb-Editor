using DarkUI.Docking;
using NLog;
using System;
using System.Windows.Forms;
using TombLib.Rendering;

namespace TombEditor.ToolWindows
{
    public partial class SectorOptions : DarkToolWindow
    {
        private readonly Editor _editor;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public SectorOptions()
        {
            InitializeComponent();
            CommandHandler.AssignCommandsToControls(Editor.Instance, this, toolTip);

            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
            panel2DGrid.Room = _editor.SelectedRoom;
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
            if (obj is Editor.SelectedRoomChangedEvent)
                panel2DGrid.Room = ((Editor.SelectedRoomChangedEvent)obj).Current;

            // Update tooltip texts
            if (obj is Editor.ConfigurationChangedEvent)
            {
                if (((Editor.ConfigurationChangedEvent)obj).UpdateKeyboardShortcuts)
                    CommandHandler.AssignCommandsToControls(_editor, this, toolTip, true);
            }
        }

        private void but_MouseEnter(object sender, EventArgs e)
        {
            SetSectorColoringInfoPriority(sender as Control);
        }

        private void SetSectorColoringInfoPriority(Control button)
        {
            if (!_editor.Configuration.Editor_AutoSwitchSectorColoringInfo)
                return;

            if (button == butBox)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Box);
            else if (button == butDeath)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Death);
            else if (button == butMonkey)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Monkey);
            else if (button == butFlagBeetle)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Beetle);
            else if (button == butFlagTriggerTriggerer)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.TriggerTriggerer);
            else if (button == butNotWalkableBox)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.NotWalkableFloor);
            else if (button == butPortal)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Portal);
            else if (button == butClimbNegativeX ||
                     button == butClimbNegativeZ ||
                     button == butClimbPositiveX ||
                     button == butClimbPositiveZ)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Climb);
            else if (button == butWall)
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Wall);
            else
                _editor.SectorColoringManager.SetPriority(SectorColoringType.Trigger);
        }
    }
}
