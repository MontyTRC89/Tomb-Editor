using DarkUI.Controls;
using DarkUI.Docking;
using DarkUI.Forms;
using NLog;
using System;
using System.Windows.Forms;
using TombLib.LevelData;
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

        }

        private void butWall_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SetWall(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butBox_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.Box);
        }

        private void butDeath_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.DeathFire);
        }

        private void butMonkey_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.Monkey);
        }

        private void butPortal_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;

            try
            {
                EditorActions.AddPortal(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
            }
            catch (Exception exc)
            {
                DarkMessageBox.Show(this, "Unable to create portal: " + exc.Message, "Error", MessageBoxIcon.Error);
                logger.Error(exc, "Portal creation failed.");
            }
        }

        private void butClimbPositiveZ_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.ClimbPositiveZ);
        }

        private void butClimbPositiveX_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.ClimbPositiveX);
        }

        private void butClimbNegativeZ_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.ClimbNegativeZ);
        }

        private void butClimbNegativeX_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.ClimbNegativeX);
        }

        private void butNotWalkableBox_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.NotWalkableFloor);
        }

        private void butFloor_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SetFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butCeiling_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SetCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butDiagonalFloor_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SetDiagonalFloorSplit(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butDiagonalCeiling_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SetDiagonalCeilingSplit(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butDiagonalWall_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.SetDiagonalWall(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butFlagBeetle_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.Beetle);
        }

        private void butFlagTriggerTriggerer_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.TriggerTriggerer);
        }

        private void butForceSolidFloor_Click(object sender, EventArgs e)
        {
            if (!EditorActions.CheckForRoomAndBlockSelection(this))
                return;
            EditorActions.ToggleForceFloorSolid(_editor.SelectedRoom, _editor.SelectedSectors.Area);
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
        }
    }
}
