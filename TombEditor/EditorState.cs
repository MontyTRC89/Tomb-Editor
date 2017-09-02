using SharpDX;
using System;
using System.Diagnostics.CodeAnalysis;
using TombEditor.Geometry;

namespace TombEditor
{
    public enum EditorMode
    {
        Geometry,
        Map2D,
        FaceEdit,
        Lighting
    }

    public enum EditorActionType
    {
        None,
        PlaceItem,
        PlaceLight,
        PlaceCamera,
        PlaceFlyByCamera,
        PlaceSoundSource,
        PlaceSink,
        PlaceNoCollision,
        Paste,
        Stamp
    }

    public struct EditorAction
    {
        public EditorActionType Action { get; set; }
        public LightType LightType { get; set; }
        public ItemType ItemType { get; set; }
        public bool RelocateCameraActive { get; set; }

        public static readonly EditorAction None = new EditorAction();

        public static bool operator ==(EditorAction first, EditorAction second)
        {
            return (first.Action == second.Action) && (first.LightType == second.LightType) &&
                   (first.ItemType == second.ItemType) && (first.RelocateCameraActive == second.RelocateCameraActive);
        }

        public static bool operator !=(EditorAction first, EditorAction second)
        {
            return (first.Action != second.Action) || (first.LightType != second.LightType) ||
                   (first.ItemType != second.ItemType) || (first.RelocateCameraActive != second.RelocateCameraActive);
        }

        public override bool Equals(object obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);
            return this == (EditorAction)obj;
        }

        public override int GetHashCode()
        {
            return Action.GetHashCode() ^ (LightType.GetHashCode() * unchecked((int)3062904283)) ^
                   (ItemType.GetHashCode() * 1334740973) ^ (RelocateCameraActive ? 0 : 0x3ce0dc8f);
        }
    };

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum EditorArrowType
    {
        EntireFace,
        EdgeN,
        EdgeE,
        EdgeS,
        EdgeW,
        CornerNW,
        CornerNE,
        CornerSE,
        CornerSW,
        DiagonalFloorCorner,
        DiagonalCeilingCorner
    }

    public struct SectorSelection
    {
        public DrawingPoint Start { get; set; }
        public DrawingPoint End { get; set; }
        public EditorArrowType Arrow { get; set; }

        public static readonly SectorSelection None =
            new SectorSelection {Start = new DrawingPoint(-1, 1), End = new DrawingPoint(-1, 1)};

        public static bool operator ==(SectorSelection first, SectorSelection second)
        {
            return (first.Start == second.Start) && (first.End == second.End) && (first.Arrow == second.Arrow);
        }

        public static bool operator !=(SectorSelection first, SectorSelection second)
        {
            return (first.Start != second.Start) || (first.End != second.End) || (first.Arrow == second.Arrow);
        }

        public override bool Equals(object obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);
            return this == (SectorSelection)obj;
        }

        public override int GetHashCode()
        {
            return Start.GetHashCode() ^ (End.GetHashCode() * unchecked((int)3062904283)) ^
                   (Arrow.GetHashCode() * 1334740973);
        }

        // The rectangle is (-1, -1, -1, 1) when nothing is selected.
        // The "Right" and "Bottom" point of the rectangle is inclusive.
        public Rectangle Area
        {
            get
            {
                return new Rectangle(
                    Math.Min(Start.X, End.X), Math.Min(Start.Y, End.Y),
                    Math.Max(Start.X, End.X), Math.Max(Start.Y, End.Y));
            }
            set
            {
                Start = new DrawingPoint(value.X, value.Y);
                End = new DrawingPoint(value.Right, value.Bottom);
            }
        }

        public bool Valid => (Start.X != -1) && (Start.Y != -1) && (End.X != -1) && (End.Y != -1);

        public SectorSelection ChangeArrows(EditorArrowType arrow)
        {
            SectorSelection result = this;
            result.Arrow = arrow;
            return result;
        }
    }

    public struct TextureSelection
    {
        public int Index { get; set; }
        public int Triangle { get; set; }
        public bool Invisible { get; set; }
        public bool DoubleSided { get; set; }
        public bool Transparent { get; set; }

        public static readonly TextureSelection None = new TextureSelection {Index = -1};

        public static bool operator ==(TextureSelection first, TextureSelection second)
        {
            return (first.Index == second.Index) && (first.Triangle == second.Triangle) &&
                   (first.Invisible == second.Invisible) && (first.DoubleSided == second.DoubleSided) &&
                   (first.Transparent == second.Transparent);
        }

        public static bool operator !=(TextureSelection first, TextureSelection second)
        {
            return (first.Index != second.Index) || (first.Triangle != second.Triangle) ||
                   (first.Invisible != second.Invisible) || (first.DoubleSided != second.DoubleSided) ||
                   (first.Transparent != second.Transparent);
        }

        public override bool Equals(object obj)
        {
            System.Diagnostics.Debug.Assert(obj != null);
            return this == (TextureSelection)obj;
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ (Triangle.GetHashCode() * unchecked((int)3062904283)) ^
                   (Invisible ? 0x5d5edef6 : 0) ^ (DoubleSided ? 0x07b4bc1e : 0) ^ (Transparent ? 0x1d5ff7db : 0);
        }
    }
}
