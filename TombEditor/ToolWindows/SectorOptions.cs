using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using DarkUI.Docking;
using TombEditor.Geometry;
using DarkUI.Forms;

namespace TombEditor.ToolWindows
{
    public partial class SectorOptions : DarkToolWindow
    {
        private Editor _editor;
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
            if (disposing && (components != null))
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
            EditorActions.SetDiagonalWallSplit(_editor.SelectedRoom, _editor.SelectedSectors.Area);
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
    }
}
