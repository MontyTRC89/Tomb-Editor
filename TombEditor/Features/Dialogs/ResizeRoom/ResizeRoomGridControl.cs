using System.Drawing;
using System.Windows.Forms;
using TombEditor.Controls;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Utils;

namespace TombEditor.Features.Dialogs.ResizeRoom;

// Standalone Panel2DGrid subclass extracted from FormResizeRoom for the WPF migration.
// State is supplied via public properties (set by the hosting window code-behind) so the
// control no longer depends on a Form-typed Parent back-reference.
public class ResizeRoomGridControl : Panel2DGrid
{
    public Room Room { get; set; }
    public RectangleInt2 NewArea { get; set; }
    public bool UseFloor { get; set; }
    public ColorScheme ColorScheme { get; set; }

    protected override VectorInt2 RoomSize => NewArea.Size + VectorInt2.One;
    protected override bool DrawSelection => false;
    protected override VectorInt2 GetGridDimensions() => RoomSize;

    protected override void OnMouseDown(MouseEventArgs e) { }
    protected override void OnMouseMove(MouseEventArgs e) { }
    protected override void OnMouseUp(MouseEventArgs e) { }
    protected override void OnMouseWheel(MouseEventArgs e) { }
    protected override void OnKeyDown(KeyEventArgs e) { }
    protected override void OnKeyUp(KeyEventArgs e) { }
    protected override void OnKeyPress(KeyPressEventArgs e) { }

    protected override void PaintSectorTile(PaintEventArgs e, RectangleF sectorArea, int x, int z)
    {
        if (Room is null)
            return;

        RectangleInt2 newArea = NewArea;
        VectorInt2 old = new VectorInt2(x, z) + newArea.Start;

        // Draw new border wall
        if ((x == 0) || (z == 0) || (x == newArea.Width) || (z == newArea.Height))
        {
            if ((newArea.X0 == 0 && x == 0 && Room.LocalArea.Inflate(0, -1).Contains(old) && 0 < z && z < newArea.Height) ||
                (newArea.Y0 == 0 && z == 0 && Room.LocalArea.Inflate(-1, 0).Contains(old) && 0 < x && x < newArea.Width) ||
                (newArea.X1 == Room.NumXSectors - 1 && x == newArea.Width && Room.LocalArea.Inflate(0, -1).Contains(old) && 0 < z && z < newArea.Height) ||
                (newArea.Y1 == Room.NumZSectors - 1 && z == newArea.Height && Room.LocalArea.Inflate(-1, 0).Contains(old) && 0 < x && x < newArea.Width))
            {
                base.PaintSectorTile(e, sectorArea, old.X, old.Y);
            }
            else
            {
                using var b = new SolidBrush(ColorScheme.ColorBorderWall.ToWinFormsColor());
                e.Graphics.FillRectangle(b, sectorArea);
            }

            return;
        }

        // Draw inner parts of the old room
        if (old.X > 0 && old.Y > 0 && old.X < (Room.NumXSectors - 1) && old.Y < (Room.NumZSectors - 1))
        {
            base.PaintSectorTile(e, sectorArea, old.X, old.Y);
            return;
        }

        // Draw new floor / wall fill
        using var fill = new SolidBrush(UseFloor
            ? ColorScheme.ColorFloor.ToWinFormsColor()
            : ColorScheme.ColorWall.ToWinFormsColor());

        e.Graphics.FillRectangle(fill, sectorArea);
    }
}
