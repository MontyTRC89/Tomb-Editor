using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor
{
    public enum EditorMode
    {
        Geometry, Map2D, FaceEdit, Lighting
    }

    public enum EditorToolType
    {
        None, Selection, Brush, Pencil,
        Fill, Group, Paint2x2,
        Shovel, Smooth, Flatten,
        Drag, Ramp, QuarterPipe, HalfPipe, Bowl, Pyramid, Terrain // Do not modify enum order after drag tool!
    }

    public class EditorTool
    {
        public EditorToolType Tool { get; set; }
        public bool TextureUVFixer { get; set; }

        public static bool operator == (EditorTool first, EditorTool second)
        {
            return first.Tool == second.Tool && first.TextureUVFixer == second.TextureUVFixer;
        }

        public static bool operator !=(EditorTool first, EditorTool second)
        {
            return first.Tool != second.Tool || first.TextureUVFixer != second.TextureUVFixer;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is EditorTool))
                return false;
            return this == (EditorTool)obj;
        }

        public override int GetHashCode()
        {
            return Tool.GetHashCode() ^ (TextureUVFixer.GetHashCode() * 1334740973);
        }
    }

    public struct SectorSelection
    {
        public VectorInt2 Start { get; set; }
        public VectorInt2 End { get; set; }
        public ArrowType Arrow { get; set; }

        public static readonly SectorSelection None = new SectorSelection { Start = new VectorInt2(-1, -1), End = new VectorInt2(-1, -1) };

        public static bool operator ==(SectorSelection first, SectorSelection second)
        {
            return first.Start == second.Start && first.End == second.End && first.Arrow == second.Arrow;
        }

        public static bool operator !=(SectorSelection first, SectorSelection second)
        {
            return first.Start != second.Start || first.End != second.End || first.Arrow != second.Arrow;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SectorSelection))
                return false;
            return this == (SectorSelection)obj;
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ (End.GetHashCode() * unchecked((int)3062904283)) ^ (Arrow.GetHashCode() * 1334740973);
        }

        // The rectangle is (-1, -1, -1, 1) when nothing is selected.
        // The "Right" and "Bottom" point of the rectangle is inclusive.
        public RectangleInt2 Area
        {
            get
            {
                return new RectangleInt2(VectorInt2.Min(Start, End), VectorInt2.Max(Start, End));
            }
            set
            {
                Start = value.Start;
                End = value.End;
            }
        }

        public bool Valid => Start.X != -1 && Start.Y != -1 && End.X != -1 && End.Y != -1;

        public SectorSelection ChangeArrows(ArrowType Arrow)
        {
            SectorSelection result = this;
            result.Arrow = Arrow;
            return result;
        }

        public SectorSelection? ClampToRoom(Room r, bool excludeBorderWalls = true)
        {
            int c = 0; // How many blocks to cut from compared area zone perimeter

            if(excludeBorderWalls)
            {
                if (((Start.X == 0 || Start.X == r.NumXSectors - 1) && Area.Size.X == 0) ||
                    ((Start.Y == 0 || Start.Y == r.NumZSectors - 1) && Area.Size.Y == 0))
                    return null;
                c = 1; // Cut border walls from comparison
            }

            return new SectorSelection()
            {
                Start = new VectorInt2(MathC.Clamp(Start.X, c, r.NumXSectors - (1 + c)), MathC.Clamp(Start.Y, c, r.NumZSectors - (1 + c))),
                End = new VectorInt2(MathC.Clamp(End.X, c, r.NumXSectors - (1 + c)), MathC.Clamp(End.Y, c, r.NumZSectors - (1 + c)))
            };
        }
    }

    public struct TextureSelection
    {
        public int Index { get; set; }
        public int Triangle { get; set; }
        public bool Invisible { get; set; }
        public bool DoubleSided { get; set; }
        public bool Transparent { get; set; }

        public static readonly TextureSelection None = new TextureSelection { Index = -1 };

        public static bool operator ==(TextureSelection first, TextureSelection second)
        {
            return first.Index == second.Index && first.Triangle == second.Triangle &&
                first.Invisible == second.Invisible && first.DoubleSided == second.DoubleSided && first.Transparent == second.Transparent;
        }

        public static bool operator !=(TextureSelection first, TextureSelection second)
        {
            return first.Index != second.Index || first.Triangle != second.Triangle ||
                first.Invisible != second.Invisible || first.DoubleSided != second.DoubleSided || first.Transparent != second.Transparent;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TextureSelection))
                return false;
            return this == (TextureSelection)obj;
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ (Triangle.GetHashCode() * unchecked((int)3062904283)) ^
                (Invisible ? 0x5d5edef6 : 0) ^ (DoubleSided ? 0x07b4bc1e : 0) ^ (Transparent ? 0x1d5ff7db : 0);
        }
    }
}
