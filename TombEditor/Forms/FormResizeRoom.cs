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
        private Editor _editor;

        private bool _alreadyUpdatingGui = false;
        private readonly Room _roomToResize;
        public RectangleInt2 NewArea => new RectangleInt2(
            -(int)numericXn.Value,
            -(int)numericZn.Value,
            _roomToResize.NumXSectors + (int)numericXp.Value - 1,
            _roomToResize.NumZSectors + (int)numericZp.Value - 1);
        public bool UseFloor => optUseFloor.Checked;

        public FormResizeRoom(Editor editor, Room roomToResize, RectangleInt2 newArea)
        {
            InitializeComponent();
            gridControl.Parent = this;
            gridControl.Room = roomToResize;
            _editor = editor;
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
                numericXn.Minimum = (3 - _roomToResize.NumXSectors) - numericXp.Value;
                numericZn.Minimum = (3 - _roomToResize.NumZSectors) - numericZp.Value;
                numericXp.Minimum = (3 - _roomToResize.NumXSectors) - numericXn.Value;
                numericZp.Minimum = (3 - _roomToResize.NumZSectors) - numericZn.Value;

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

            protected override void PaintSectorTile(PaintEventArgs e, RectangleF sectorArea, int x, int z)
            {
                Room room = Parent._roomToResize;
                RectangleInt2 newArea = Parent.NewArea;
                VectorInt2 old = new VectorInt2(x, z) + newArea.Start;

                // Draw new border wall
                if ((x == 0) || (z == 0) || (x == newArea.Width) || (z == newArea.Height))
                {
                    // Draw border wall using old room if possible
                    if ((newArea.X0 == 0 && x == 0 && room.LocalArea.Inflate(0, -1).Contains(old) && 0 < z && z < newArea.Height) ||
                        (newArea.Y0 == 0 && z == 0 && room.LocalArea.Inflate(-1, 0).Contains(old) && 0 < x && x < newArea.Width) ||
                        (newArea.X1 == room.NumXSectors - 1 && x == newArea.Width && room.LocalArea.Inflate(0, -1).Contains(old) && 0 < z && z < newArea.Height) ||
                        (newArea.Y1 == room.NumZSectors - 1 && z == newArea.Height && room.LocalArea.Inflate(-1, 0).Contains(old) && 0 < x && x < newArea.Width))
                        base.PaintSectorTile(e, sectorArea, old.X, old.Y);
                    else
                        using(var b = new SolidBrush(Parent._editor.Configuration.Editor_ColorScheme.ColorBorderWall.ToWinFormsColor()))
                            e.Graphics.FillRectangle(b, sectorArea);
                    return;
                }

                // Draw inner parts of the old room
                if (old.X > 0 && old.Y > 0 && old.X < (room.NumXSectors - 1) && old.Y < (room.NumZSectors - 1))
                {
                    base.PaintSectorTile(e, sectorArea, old.X, old.Y);
                    return;
                }

                // Draw new floor
                using (var b = new SolidBrush(Parent.UseFloor ?
                                                Parent._editor.Configuration.Editor_ColorScheme.ColorFloor.ToWinFormsColor() :
                                                Parent._editor.Configuration.Editor_ColorScheme.ColorWall.ToWinFormsColor()))
                    e.Graphics.FillRectangle(b, sectorArea);
            }
        }
    }
}
