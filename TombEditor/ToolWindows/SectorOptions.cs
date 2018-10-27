using DarkUI.Docking;
using NLog;
using System;
using System.Windows.Forms;
using TombLib.Rendering;
using TombLib.Utils;

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
            _editor.EditorEventRaised += EditorEventRaised;EditorEventRaised(new Editor.InitEvent());
            EditorEventRaised(new Editor.InitEvent());

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

            // Update color scheme on buttons
            if (obj is Editor.ConfigurationChangedEvent ||
                obj is Editor.InitEvent)
            {
                butFloor.BackColor = _editor.Configuration.UI_ColorScheme.ColorFloor.ToWinFormsColor();
                butCeiling.BackColor = _editor.Configuration.UI_ColorScheme.ColorFloor.ToWinFormsColor();
                butBox.BackColor = _editor.Configuration.UI_ColorScheme.ColorBox.ToWinFormsColor();
                butNotWalkableBox.BackColor = _editor.Configuration.UI_ColorScheme.ColorNotWalkable.ToWinFormsColor();
                butMonkey.BackColor = _editor.Configuration.UI_ColorScheme.ColorMonkey.ToWinFormsColor();
                butDeath.BackColor = _editor.Configuration.UI_ColorScheme.ColorDeath.ToWinFormsColor();
                butPortal.BackColor = _editor.Configuration.UI_ColorScheme.ColorPortal.ToWinFormsColor();
                butWall.BackColor = _editor.Configuration.UI_ColorScheme.ColorWall.ToWinFormsColor();
            }
        }

        private void but_MouseEnter(object sender, EventArgs e)
        {
            SetSectorColoringInfoPriority(sender as Control);
        }

        private void SetSectorColoringInfoPriority(Control button)
        {
            if (!_editor.Configuration.UI_AutoSwitchSectorColoringInfo)
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
