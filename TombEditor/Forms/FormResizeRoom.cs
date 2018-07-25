using DarkUI.Forms;
using System.Windows.Forms;
using TombEditor.Controls;
using TombLib;
using TombLib.LevelData;
using System.Drawing;
using TombLib.Rendering;
using TombLib.Utils;

namespace TombEditor.Forms
{
    public partial class FormResizeRoom : DarkForm
    {
        private bool _alreadyUpdatingGui = false;
        private readonly Room _roomToResize;
        public RectangleInt2 NewArea => new RectangleInt2(
            -(int)numericXn.Value,
            -(int)numericZn.Value,
            _roomToResize.NumXSectors + (int)numericXp.Value - 1,
            _roomToResize.NumZSectors + (int)numericZp.Value - 1);
        public bool UseFloor => optUseFloor.Checked;

        public FormResizeRoom(Room roomToResize, RectangleInt2 newArea)
        {
            InitializeComponent();
            gridControl.Parent = this;
            gridControl.Room = roomToResize;
            _roomToResize = roomToResize;

            try
            {
                _alreadyUpdatingGui = true;
                numericXn.Value = -newArea.X0;
                numericZn.Value = -newArea.Y0;
                numericXp.Value = newArea.X1 + 1 - _roomToResize.NumXSectors;
                numericZp.Value = newArea.Y1 + 1 - _roomToResize.NumZSectors;
            }
            finally
            {
                _alreadyUpdatingGui = false;
            }
            UpdateGui();
        }

        private void UpdateGui()
        {
            if (_alreadyUpdatingGui)
                return;
            try
            {
                _alreadyUpdatingGui = true;
                numericXn.Minimum = (1 - _roomToResize.NumXSectors) - numericXp.Value;
                numericZn.Minimum = (1 - _roomToResize.NumZSectors) - numericZp.Value;
                numericXp.Minimum = (1 - _roomToResize.NumXSectors) - numericXn.Value;
                numericZp.Minimum = (1 - _roomToResize.NumZSectors) - numericZn.Value;

                int maxDimensions = cbAllowOversizedRooms.Checked ? 255 : Room.MaxRecommendedRoomDimensions;
                numericXn.Maximum = (maxDimensions - _roomToResize.NumXSectors) - numericXp.Value;
                numericXp.Maximum = (maxDimensions - _roomToResize.NumXSectors) - numericXn.Value;
                numericZn.Maximum = (maxDimensions - _roomToResize.NumZSectors) - numericZp.Value;
                numericZp.Maximum = (maxDimensions - _roomToResize.NumZSectors) - numericZn.Value;

                gridControl.Invalidate();
            }
            finally
            {
                _alreadyUpdatingGui = false;
            }
        }

        private void butOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void butCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cbAllowOversizedRooms_CheckedChanged(object sender, System.EventArgs e)
        {
            if (cbAllowOversizedRooms.Checked)
                if (DarkMessageBox.Show(this, "You are about to make a room bigger than 32 by 32. This will probably cause issues with rendering in game. Are you sure?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    cbAllowOversizedRooms.Checked = false;
            UpdateGui();
        }

        private void numericZp_ValueChanged(object sender, System.EventArgs e)
        {
            UpdateGui();
        }

        private void numericZn_ValueChanged(object sender, System.EventArgs e)
        {
            UpdateGui();
        }

        private void numericXn_ValueChanged(object sender, System.EventArgs e)
        {
            UpdateGui();
        }

        private void numericXp_ValueChanged(object sender, System.EventArgs e)
        {
            UpdateGui();
        }

        private void optUseWalls_CheckedChanged(object sender, System.EventArgs e)
        {
            gridControl.Invalidate();
        }

        private void optUseFloor_CheckedChanged(object sender, System.EventArgs e)
        {
            gridControl.Invalidate();
        }

        public class GridControl : Panel2DGrid
        {
            public new FormResizeRoom Parent;

            protected override void OnMouseDown(MouseEventArgs e) { }
            protected override void OnMouseMove(MouseEventArgs e) { }
            protected override void OnMouseUp(MouseEventArgs e) { }
            protected override void OnMouseWheel(MouseEventArgs e) { }
            protected override void OnKeyDown(KeyEventArgs e) { }
            protected override void OnKeyUp(KeyEventArgs e) { }
            protected override void OnKeyPress(KeyPressEventArgs e) { }

            protected override VectorInt2 RoomSize => Parent.NewArea.Size + new VectorInt2(1, 1);
            protected override bool DrawSelection => false;
            protected override VectorInt2 GetGridDimensions() => RoomSize;

            private static readonly Brush _borderWallBrush = new SolidBrush(SectorColoringInfo.ColorBorderWall.ToWinFormsColor());
            private static readonly Brush _wallBrush = new SolidBrush(SectorColoringInfo.ColorWall.ToWinFormsColor());
            private static readonly Brush _floorBrush = new SolidBrush(SectorColoringInfo.ColorFloor.ToWinFormsColor());

            protected override void PaintSectorTile(PaintEventArgs e, RectangleF sectorArea, int x, int z)
            {
                // Draw new border wall
                RectangleInt2 newArea = Parent.NewArea;
                if ((x == 0) || (z == 0) || (x == newArea.Width) || (z == newArea.Height))
                {
                    e.Graphics.FillRectangle(_borderWallBrush, sectorArea);
                    return;
                }

                // Draw parts of the old room
                x += newArea.X0;
                z += newArea.Y0;
                if (x > 0 && z > 0 && x < (Parent._roomToResize.NumXSectors - 1) && z < (Parent._roomToResize.NumZSectors - 1))
                {
                    base.PaintSectorTile(e, sectorArea, x, z);
                    return;
                }

                // Draw new floor
                e.Graphics.FillRectangle(Parent.UseFloor ? _floorBrush : _wallBrush, sectorArea);
            }
        }
    }
}
