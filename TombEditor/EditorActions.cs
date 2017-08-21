using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using TombEditor.Geometry;

namespace TombEditor
{
    public static class EditorActions
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private static Editor _editor = Editor.Instance;

        public static void SmartBuildGeometry(Room room, Rectangle area)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            room.SmartBuildGeometry(area);
            watch.Stop();
            logger.Debug("Edit geometry time: " + watch.ElapsedMilliseconds + "  ms");

            _editor.RoomGeometryChange(room);
        }

        public static void EditSectorGeometry(Room room, Rectangle area, EditorArrowType arrow, int verticalSubdivision, short increment, bool smooth)
        {
            if (smooth)
            {
                // Adjust editing area to exclude the side on which the arrow starts
                // This is a superset of the behaviour of the old editor to smooth edit a single edge or side.
                switch (arrow)
                {
                    case EditorArrowType.EdgeE:
                        area = new Rectangle(area.X + 1, area.Y, area.Right, area.Bottom);
                        break;
                    case EditorArrowType.EdgeN:
                        area = new Rectangle(area.X, area.Y + 1, area.Right, area.Bottom);
                        break;
                    case EditorArrowType.EdgeW:
                        area = new Rectangle(area.X, area.Y, area.Right - 1, area.Bottom);
                        break;
                    case EditorArrowType.EdgeS:
                        area = new Rectangle(area.X, area.Y, area.Right, area.Bottom - 1);
                        break;
                    case EditorArrowType.CornerNE:
                        area = new Rectangle(area.X + 1, area.Y + 1, area.Right, area.Bottom);
                        break;
                    case EditorArrowType.CornerNW:
                        area = new Rectangle(area.X, area.Y + 1, area.Right - 1, area.Bottom);
                        break;
                    case EditorArrowType.CornerSW:
                        area = new Rectangle(area.X, area.Y, area.Right - 1, area.Bottom - 1);
                        break;
                    case EditorArrowType.CornerSE:
                        area = new Rectangle(area.X + 1, area.Y, area.Right, area.Bottom - 1);
                        break;
                }
                arrow = EditorArrowType.EntireFace;

                // Smoothly change sectors on the corners
                room.GetBlockTry(area.X - 1, area.Bottom + 1)?.ChangeEdge(verticalSubdivision, Block.FaceXpZn, increment);
                room.GetBlockTry(area.Right + 1, area.Bottom + 1)?.ChangeEdge(verticalSubdivision, Block.FaceXnZn, increment);
                room.GetBlockTry(area.Right + 1, area.Y - 1)?.ChangeEdge(verticalSubdivision, Block.FaceXnZp, increment);
                room.GetBlockTry(area.X - 1, area.Y - 1)?.ChangeEdge(verticalSubdivision, Block.FaceXpZp, increment);

                // Smoothly change sectors on the sides
                for (int x = area.X; x <= area.Right; x++)
                {
                    room.GetBlockTry(x, area.Y - 1)?.ChangeEdge(verticalSubdivision, Block.FaceXnZp, increment);
                    room.GetBlockTry(x, area.Y - 1)?.ChangeEdge(verticalSubdivision, Block.FaceXpZp, increment);

                    room.GetBlockTry(x, area.Bottom + 1)?.ChangeEdge(verticalSubdivision, Block.FaceXnZn, increment);
                    room.GetBlockTry(x, area.Bottom + 1)?.ChangeEdge(verticalSubdivision, Block.FaceXpZn,  increment);
                }
                
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    room.GetBlockTry(area.X - 1, z)?.ChangeEdge(verticalSubdivision, Block.FaceXpZp, increment);
                    room.GetBlockTry(area.X - 1, z)?.ChangeEdge(verticalSubdivision, Block.FaceXpZn, increment);

                    room.GetBlockTry(area.Right + 1, z)?.ChangeEdge(verticalSubdivision, Block.FaceXnZp, increment);
                    room.GetBlockTry(area.Right + 1, z)?.ChangeEdge(verticalSubdivision, Block.FaceXnZn, increment);
                }
            }

            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    Block block = room.Blocks[x, z];

                    switch (arrow)
                    {
                        case EditorArrowType.EntireFace:
                            if (verticalSubdivision == 0)
                            {
                                if (block.FloorDiagonalSplit == DiagonalSplit.NW && block.QAFaces[2] == block.QAFaces[0] && increment < 0)
                                    continue;
                                if (block.FloorDiagonalSplit == DiagonalSplit.NE && block.QAFaces[3] == block.QAFaces[1] && increment < 0)
                                    continue;
                                if (block.FloorDiagonalSplit == DiagonalSplit.SE && block.QAFaces[0] == block.QAFaces[2] && increment < 0)
                                    continue;
                                if (block.FloorDiagonalSplit == DiagonalSplit.SW && block.QAFaces[1] == block.QAFaces[3] && increment < 0)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.NW)
                                    room.Blocks[x, z].QAFaces[0] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.NE)
                                    room.Blocks[x, z].QAFaces[1] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SE)
                                    room.Blocks[x, z].QAFaces[2] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SW)
                                    room.Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.CeilingDiagonalSplit == DiagonalSplit.NW && block.WSFaces[2] == block.WSFaces[0] && increment > 0)
                                    continue;
                                if (block.CeilingDiagonalSplit == DiagonalSplit.NE && block.WSFaces[3] == block.WSFaces[1] && increment > 0)
                                    continue;
                                if (block.CeilingDiagonalSplit == DiagonalSplit.SE && block.WSFaces[0] == block.WSFaces[2] && increment > 0)
                                    continue;
                                if (block.CeilingDiagonalSplit == DiagonalSplit.SW && block.WSFaces[1] == block.WSFaces[3] && increment > 0)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.NW)
                                    room.Blocks[x, z].WSFaces[0] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.NE)
                                    room.Blocks[x, z].WSFaces[1] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SE)
                                    room.Blocks[x, z].WSFaces[2] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SW)
                                    room.Blocks[x, z].WSFaces[3] += increment;
                            }
                            else if (verticalSubdivision == 2)
                            {
                                room.Blocks[x, z].EDFaces[0] += increment;
                                room.Blocks[x, z].EDFaces[1] += increment;
                                room.Blocks[x, z].EDFaces[2] += increment;
                                room.Blocks[x, z].EDFaces[3] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 3)
                            {
                                room.Blocks[x, z].RFFaces[0] += increment;
                                room.Blocks[x, z].RFFaces[1] += increment;
                                room.Blocks[x, z].RFFaces[2] += increment;
                                room.Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case EditorArrowType.EdgeN:
                            if (verticalSubdivision == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.NW)
                                    room.Blocks[x, z].QAFaces[0] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.NE)
                                    room.Blocks[x, z].QAFaces[1] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.NW)
                                    room.Blocks[x, z].WSFaces[0] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.NE)
                                    room.Blocks[x, z].WSFaces[1] += increment;
                            }
                            else if (verticalSubdivision == 2)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                room.Blocks[x, z].EDFaces[0] += increment;
                                room.Blocks[x, z].EDFaces[1] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 3)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                room.Blocks[x, z].RFFaces[0] += increment;
                                room.Blocks[x, z].RFFaces[1] += increment;
                            }
                            break;

                        case EditorArrowType.EdgeE:
                            if (verticalSubdivision == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.NE)
                                    room.Blocks[x, z].QAFaces[1] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SE)
                                    room.Blocks[x, z].QAFaces[2] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.NE)
                                    room.Blocks[x, z].WSFaces[1] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SE)
                                    room.Blocks[x, z].WSFaces[2] += increment;
                            }
                            else if (verticalSubdivision == 2)
                            {
                                room.Blocks[x, z].EDFaces[1] += increment;
                                room.Blocks[x, z].EDFaces[2] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 3)
                            {
                                room.Blocks[x, z].RFFaces[1] += increment;
                                room.Blocks[x, z].RFFaces[2] += increment;
                            }
                            break;

                        case EditorArrowType.EdgeS:
                            if (verticalSubdivision == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.SE)
                                    room.Blocks[x, z].QAFaces[2] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SW)
                                    room.Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.SE)
                                    room.Blocks[x, z].WSFaces[2] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SW)
                                    room.Blocks[x, z].WSFaces[3] += increment;
                            }
                            else if (verticalSubdivision == 2)
                            {
                                room.Blocks[x, z].EDFaces[2] += increment;
                                room.Blocks[x, z].EDFaces[3] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 3)
                            {
                                room.Blocks[x, z].RFFaces[2] += increment;
                                room.Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case EditorArrowType.EdgeW:
                            if (verticalSubdivision == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.NW)
                                    room.Blocks[x, z].QAFaces[0] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.SW)
                                    room.Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.NW)
                                    room.Blocks[x, z].WSFaces[0] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SW)
                                    room.Blocks[x, z].WSFaces[3] += increment;
                            }
                            else if (verticalSubdivision == 2)
                            {
                                room.Blocks[x, z].EDFaces[0] += increment;
                                room.Blocks[x, z].EDFaces[3] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 3)
                            {
                                room.Blocks[x, z].RFFaces[0] += increment;
                                room.Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case EditorArrowType.CornerNW:
                            if (verticalSubdivision == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.SE && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.SE)
                                {
                                    if (block.QAFaces[0] == block.QAFaces[1] && increment < 0)
                                        continue;
                                }

                                room.Blocks[x, z].QAFaces[0] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SE && block.CeilingDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit == DiagonalSplit.SE)
                                {
                                    if (block.WSFaces[0] == block.WSFaces[1] && increment > 0)
                                        continue;
                                }

                                room.Blocks[x, z].WSFaces[0] += increment;
                            }
                            else if (verticalSubdivision == 2)
                            {
                                room.Blocks[x, z].EDFaces[0] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 3)
                            {
                                room.Blocks[x, z].RFFaces[0] += increment;
                            }
                            break;

                        case EditorArrowType.CornerNE:
                            if (verticalSubdivision == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.SW && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.SW)
                                {
                                    if (block.QAFaces[1] == block.QAFaces[2] && increment < 0)
                                        continue;
                                }

                                room.Blocks[x, z].QAFaces[1] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.CeilingDiagonalSplit != DiagonalSplit.SW && block.CeilingDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit == DiagonalSplit.SW)
                                {
                                    if (block.WSFaces[1] == block.WSFaces[2] && increment > 0)
                                        continue;
                                }

                                room.Blocks[x, z].WSFaces[1] += increment;
                            }
                            else if (verticalSubdivision == 2)
                            {
                                room.Blocks[x, z].EDFaces[1] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 3)
                            {
                                room.Blocks[x, z].RFFaces[1] += increment;
                            }
                            break;

                        case EditorArrowType.CornerSE:
                            if (verticalSubdivision == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.NW && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.NW)
                                {
                                    if (block.QAFaces[2] == block.QAFaces[3] && increment < 0)
                                        continue;
                                }

                                room.Blocks[x, z].QAFaces[2] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.CeilingDiagonalSplit != DiagonalSplit.NW && block.CeilingDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit == DiagonalSplit.NW)
                                {
                                    if (block.WSFaces[2] == block.WSFaces[3] && increment > 0)
                                        continue;
                                }

                                room.Blocks[x, z].WSFaces[2] += increment;
                            }
                            else if (verticalSubdivision == 2)
                            {
                                room.Blocks[x, z].EDFaces[2] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 3)
                            {
                                room.Blocks[x, z].RFFaces[2] += increment;
                            }
                            break;

                        case EditorArrowType.CornerSW:
                            if (verticalSubdivision == 0)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.NE && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.NE)
                                {
                                    if (block.QAFaces[3] == block.QAFaces[0] && increment < 0)
                                        continue;
                                }

                                room.Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.CeilingDiagonalSplit != DiagonalSplit.NE && block.CeilingDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit == DiagonalSplit.NE)
                                {
                                    if (block.WSFaces[3] == block.WSFaces[0] && increment > 0)
                                        continue;
                                }

                                room.Blocks[x, z].WSFaces[3] += increment;
                            }
                            else if (verticalSubdivision == 2)
                            {
                                room.Blocks[x, z].EDFaces[3] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 3)
                            {
                                room.Blocks[x, z].RFFaces[3] += increment;
                            }
                            break;

                        case EditorArrowType.DiagonalFloorCorner:
                            if (block.FloorDiagonalSplit == DiagonalSplit.NW)
                            {
                                if (block.QAFaces[0] == block.QAFaces[1] && increment > 0)
                                    continue;
                                room.Blocks[x, z].QAFaces[0] += increment;
                            }

                            if (block.FloorDiagonalSplit == DiagonalSplit.NE)
                            {
                                if (block.QAFaces[1] == block.QAFaces[2] && increment > 0)
                                    continue;
                                room.Blocks[x, z].QAFaces[1] += increment;
                            }

                            if (block.FloorDiagonalSplit == DiagonalSplit.SE)
                            {
                                if (block.QAFaces[2] == block.QAFaces[3] && increment > 0)
                                    continue;
                                room.Blocks[x, z].QAFaces[2] += increment;
                            }

                            if (block.FloorDiagonalSplit == DiagonalSplit.SW)
                            {
                                if (block.QAFaces[3] == block.QAFaces[0] && increment > 0)
                                    continue;
                                room.Blocks[x, z].QAFaces[3] += increment;
                            }

                            break;

                        case EditorArrowType.DiagonalCeilingCorner:
                            if (block.CeilingDiagonalSplit == DiagonalSplit.NW)
                            {
                                if (block.WSFaces[0] == block.WSFaces[1] && increment < 0)
                                    continue;
                                room.Blocks[x, z].WSFaces[0] += increment;
                            }

                            if (block.CeilingDiagonalSplit == DiagonalSplit.NE)
                            {
                                if (block.WSFaces[1] == block.WSFaces[2] && increment < 0)
                                    continue;
                                room.Blocks[x, z].WSFaces[1] += increment;
                            }

                            if (block.CeilingDiagonalSplit == DiagonalSplit.SE)
                            {
                                if (block.WSFaces[2] == block.WSFaces[3] && increment < 0)
                                    continue;
                                room.Blocks[x, z].WSFaces[2] += increment;
                            }

                            if (block.CeilingDiagonalSplit == DiagonalSplit.SW)
                            {
                                if (block.WSFaces[3] == block.WSFaces[0] && increment < 0)
                                    continue;
                                room.Blocks[x, z].WSFaces[3] += increment;
                            }

                            break;
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void FlipFloorSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                    room.Blocks[x, z].SplitFoorType = 
                        (byte)(room.Blocks[x, z].SplitFoorType == 0 ? 1 : 0);

            SmartBuildGeometry(room, area);
        }

        public static void FlipCeilingSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                    room.Blocks[x, z].SplitCeilingType =
                        (byte)(room.Blocks[x, z].SplitCeilingType == 0 ? 1 : 0);

            SmartBuildGeometry(room, area);
        }

        public static void AddTrigger(Room room, Rectangle area, IWin32Window parent)
        { 
            var trigger = new TriggerInstance(area);
            using (var formTrigger = new FormTrigger(_editor.Level, trigger))
            {
                if (formTrigger.ShowDialog(parent) != DialogResult.OK)
                    return;
                room.AddObject(_editor.Level, trigger);
            }
            _editor.RoomSectorPropertiesChange(room);
        }

        public static Vector3 GetMovementPrecision(Keys modifierKeys)
        {
            if (modifierKeys.HasFlag(Keys.Shift | Keys.Control))
                return new Vector3(0.0f);
            if (modifierKeys.HasFlag(Keys.Shift))
                return new Vector3(64.0f);
            return new Vector3(512.0f, 128.0f, 512.0f);
        }

        public static void MoveObject(Room room, PositionBasedObjectInstance instance, Vector3 pos, Keys modifierKeys)
        {
            MoveObject(room, instance, pos, GetMovementPrecision(modifierKeys), modifierKeys.HasFlag(Keys.Alt));
        }

        public static void MoveObject(Room room, PositionBasedObjectInstance instance, Vector3 pos, Vector3 precision = new Vector3(), bool canGoOutsideRoom = false)
        {
            if (instance == null)
                return;

            // Limit movement precision
            for (int i = 0; i < 3; ++i)
                if (precision[i] != 0)
                    pos[i] = ((float)Math.Round(pos[i] / precision[i])) * precision[i];

            // Limit movement area ...
            if (!canGoOutsideRoom)
            {
                float x = (float)Math.Floor(pos.X / 1024.0f);
                float z = (float)Math.Floor(pos.Z / 1024.0f);

                if ((x < 0.0f) || (x > (room.NumXSectors - 1)) ||
                    (z < 0.0f) || (z > (room.NumZSectors - 1)))
                    return;

                if (room.Blocks[(int)x, (int)z].IsAnyWall)
                    return;

                var lowest = room.GetLowestFloorCorner((int)x, (int)z);
                var highest = room.GetHighestCeilingCorner((int)x, (int)z);

                // Don't go outside room boundaries
                if ((pos.X < 1024.0f) || (pos.X > (room.NumXSectors - 1) * 1024.0f) ||
                    (pos.Z < 1024.0f) || (pos.Z > (room.NumZSectors - 1) * 1024.0f) ||
                    (pos.Y < lowest * 256.0f) || (pos.Y > highest * 256.0f))
                    return;
            }

            // Update position
            instance.Position = pos;

            // Update state
            if (instance is Light)
            {
                room.CalculateLightingForThisRoom();
                room.UpdateBuffers();
            }
            _editor.ObjectChange(instance);
        }

        public enum RotationAxis
        {
            Y,
            X,
            Roll
        };

        public static void RotateObject(Room room, ObjectInstance instance, RotationAxis axis, float angleInDegrees)
        {
            switch (axis)
            {
                case RotationAxis.Y:
                    IRotateableY rotateableY = instance as IRotateableY;
                    if (rotateableY == null)
                        return;
                    rotateableY.RotationY += angleInDegrees;
                    break;
                case RotationAxis.X:
                    IRotateableYX rotateableX = instance as IRotateableYX;
                    if (rotateableX == null)
                        return;
                    rotateableX.RotationY += angleInDegrees;
                    break;
                case RotationAxis.Roll:
                    IRotateableY rotateableRoll = instance as IRotateableY;
                    if (rotateableRoll == null)
                        return;
                    rotateableRoll.RotationY += angleInDegrees;
                    break;
            }
            if (instance is Light)
                room.UpdateCompletely();
            _editor.ObjectChange(instance);
        }

        public static void EditObject(Room room, ObjectInstance instance, IWin32Window owner)
        {
            if (instance is MoveableInstance)
            {
                using (var formMoveable = new FormMoveable((MoveableInstance)instance))
                    formMoveable.ShowDialog(owner);
                _editor.ObjectChange(instance);
            }
            else if (instance is FlybyCameraInstance)
            {
                using (var formFlyby = new FormFlybyCamera((FlybyCameraInstance)instance))
                    formFlyby.ShowDialog(owner);
                _editor.ObjectChange(instance);
            }
            else if (instance is SinkInstance)
            {
                using (var formSink = new FormSink((SinkInstance)instance))
                    formSink.ShowDialog(owner);
                _editor.ObjectChange(instance);
            }
            else if (instance is SoundSourceInstance)
            {
                using (var formSoundSource = new FormSound((SoundSourceInstance)instance, _editor.Level.Wad))
                    formSoundSource.ShowDialog(owner);
                _editor.ObjectChange(instance);
            }
            else if (instance is TriggerInstance)
            {
                using (var formTrigger = new FormTrigger(_editor.Level, (TriggerInstance)instance))
                    formTrigger.ShowDialog(owner);
                _editor.ObjectChange(instance);
            }
        }

        public static void DeleteObjectWithWarning(Room room, ObjectInstance instance, IWin32Window owner)
        {
            if (room.Flipped && (instance is Portal))
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You can't delete portals of a flipped room", "Error");
                return;
            }

            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete " + instance.ToString() + "?",
                    "Confirm delete", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            DeleteObject(room, instance);
        }

        public static void DeleteObject(Room room, ObjectInstance instance)
        {
            room.RemoveObject(_editor.Level, instance);

            // Additional updates
            if (instance is SectorBasedObjectInstance)
                _editor.RoomSectorPropertiesChange(room);
            if (instance is Light)
                room.UpdateCompletely();

            // Avoid having the removed object still selected
            if (_editor.SelectedObject == instance)
                _editor.SelectedObject = null;
            _editor.ObjectChange(null);
        }
        
        public static void PlaceTexture(Room room, DrawingPoint pos, BlockFaces faceType)
        {
            BlockFace face = room.GetBlock(pos).Faces[(int)faceType];

            ApplyTexture(room, pos, faceType);

            face.Flipped = false;

            room.BuildGeometry(new Rectangle(pos.X, pos.Y, pos.X, pos.Y));
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.RoomTextureChange(room);
        }

        public static void RotateTexture(Room room, DrawingPoint pos, BlockFaces faceType)
        {
            BlockFace face = room.GetBlock(pos).Faces[(int)faceType];
            if (_editor.SelectedTexture.Invisible || face.Invisible)
                return;

            if (face.Shape == BlockFaceShape.Triangle)
            {
                Vector2 temp3 = face.TriangleUV[2];
                face.TriangleUV[2] = face.TriangleUV[1];
                face.TriangleUV[1] = face.TriangleUV[0];
                face.TriangleUV[0] = temp3;

                if (faceType == BlockFaces.FloorTriangle2)
                {
                    Vector2 temp4 = face.TriangleUV2[2];
                    face.TriangleUV2[2] = face.TriangleUV2[1];
                    face.TriangleUV2[1] = face.TriangleUV2[0];
                    face.TriangleUV2[0] = temp4;
                }

                face.Rotation += 1;
                if (face.Rotation == 3)
                    face.Rotation = 0;
            }
            else
            {
                Vector2 temp2 = face
                    .RectangleUV[3];
                face.RectangleUV[3] = face.RectangleUV[2];
                face.RectangleUV[2] = face.RectangleUV[1];
                face.RectangleUV[1] = face.RectangleUV[0];
                face.RectangleUV[0] = temp2;

                face.Rotation += 1;
                if (face.Rotation == 4)
                    face.Rotation = 0;
            }

            room.BuildGeometry();
            room.UpdateBuffers();
            _editor.RoomTextureChange(room);
        }

        public static void FlipTexture(Room room, DrawingPoint pos, BlockFaces faceType)
        {
            BlockFace face = room.GetBlock(pos).Faces[(int)faceType];

            if (_editor.SelectedTexture.Invisible || face.Invisible || face.Texture == -1)
                return;

            if (face.Shape == BlockFaceShape.Triangle)
            {
                Vector2[] UV = new Vector2[4];

                // Calculate the new UV
                LevelTexture texture = _editor.Level.TextureSamples[face.Texture];

                UV[0] = new Vector2(texture.X / 256.0f, texture.Y / 256.0f);
                UV[1] = new Vector2((texture.X + texture.Width) / 256.0f, texture.Y / 256.0f);
                UV[2] = new Vector2((texture.X + texture.Width) / 256.0f, (texture.Y + texture.Height) / 256.0f);
                UV[3] = new Vector2(texture.X / 256.0f, (texture.Y + texture.Height) / 256.0f);

                if (_editor.SelectedTexture.Triangle == TextureTileType.TriangleNW)
                {
                    face.TriangleUV[0] = UV[1];
                    face.TriangleUV[1] = UV[0];
                    face.TriangleUV[2] = UV[2];
                    if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                    {
                        face.TriangleUV2[0] = UV[1];
                        face.TriangleUV2[1] = UV[0];
                        face.TriangleUV2[2] = UV[2];
                    }
                }

                if (_editor.SelectedTexture.Triangle == TextureTileType.TriangleNE)
                {
                    face.TriangleUV[0] = UV[0];
                    face.TriangleUV[1] = UV[3];
                    face.TriangleUV[2] = UV[1];

                    if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                    {
                        face.TriangleUV2[0] = UV[0];
                        face.TriangleUV2[1] = UV[3];
                        face.TriangleUV2[2] = UV[1];
                    }
                }

                if (_editor.SelectedTexture.Triangle == TextureTileType.TriangleSE)
                {
                    face.TriangleUV[0] = UV[3];
                    face.TriangleUV[1] = UV[2];
                    face.TriangleUV[2] = UV[0];

                    if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                    {
                        face.TriangleUV2[0] = UV[3];
                        face.TriangleUV2[1] = UV[2];
                        face.TriangleUV2[2] = UV[0];
                    }
                }

                if (_editor.SelectedTexture.Triangle == TextureTileType.TriangleSW)
                {
                    face.TriangleUV[0] = UV[2];
                    face.TriangleUV[1] = UV[1];
                    face.TriangleUV[2] = UV[3];

                    if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                    {
                        face.TriangleUV2[0] = UV[2];
                        face.TriangleUV2[1] = UV[1];
                        face.TriangleUV2[2] = UV[3];
                    }
                }

                for (int k = 0; k < face.Rotation; k++)
                {
                    Vector2 temp3 = face
                        .TriangleUV[2];
                    face.TriangleUV[2] = face.TriangleUV[1];
                    face.TriangleUV[1] = face.TriangleUV[0];
                    face.TriangleUV[0] = temp3;

                    if (faceType == BlockFaces.FloorTriangle2)
                    {
                        Vector2 temp4 = face.TriangleUV2[2];
                        face.TriangleUV2[2] = face.TriangleUV2[1];
                        face.TriangleUV2[1] = face.TriangleUV2[0];
                        face.TriangleUV2[0] = temp4;
                    }
                }
            }
            else
            {
                Vector2 temp2 = face.RectangleUV[1];
                face.RectangleUV[1] = face.RectangleUV[0];
                face.RectangleUV[0] = temp2;

                temp2 = face.RectangleUV[3];
                face.RectangleUV[3] = face.RectangleUV[2];
                face.RectangleUV[2] = temp2;
            }

            face.Flipped = !face.Flipped;

            room.BuildGeometry();
            room.UpdateBuffers();
            _editor.RoomTextureChange(room);
        }

        public static void PlaceNoCollision(Room room, DrawingPoint pos, BlockFaces faceType)
        {
            if (faceType == BlockFaces.Floor || faceType == BlockFaces.FloorTriangle2)
                room.GetBlock(pos).NoCollisionFloor = !room.GetBlock(pos).NoCollisionFloor;

            if (faceType == BlockFaces.Ceiling || faceType == BlockFaces.CeilingTriangle2)
                room.GetBlock(pos).NoCollisionCeiling = !room.GetBlock(pos).NoCollisionCeiling;

            room.UpdateCompletely();
            _editor.RoomSectorPropertiesChange(room);
        }

        private static void ApplyTextureNoUpdated(Room room, DrawingPoint pos, BlockFaces faceType)
        {
            if (_editor == null || (_editor.SelectedTexture.Index == -1 && !_editor.SelectedTexture.Invisible))
                return;

            BlockFace face = room.GetBlock(pos).Faces[(int)faceType];

            if (_editor.SelectedTexture.Invisible)
            {
                room.GetBlock(pos).Faces[(int)faceType].Invisible = true;
                room.GetBlock(pos).Faces[(int)faceType].Transparent = false;
                room.GetBlock(pos).Faces[(int)faceType].DoubleSided = false;

                int tid = room.GetBlock(pos).Faces[(int)faceType].Texture;

                room.GetBlock(pos).Faces[(int)faceType].Texture = -1;
            }
            else
            {
                // if face was invisible, then reset flag
                if (face.Invisible)
                    room.GetBlock(pos).Faces[(int)faceType].Invisible = false;

                // set trasparency of this face
                if (_editor.SelectedTexture.Transparent)
                    room.GetBlock(pos).Faces[(int)faceType].Transparent = true;
                else
                    room.GetBlock(pos).Faces[(int)faceType].Transparent = false;

                // set double sided flag of this face
                if (_editor.SelectedTexture.DoubleSided)
                    room.GetBlock(pos).Faces[(int)faceType].DoubleSided = true;
                else
                    room.GetBlock(pos).Faces[(int)faceType].DoubleSided = false;

                Vector2[] UV = new Vector2[4];

                LevelTexture texture = _editor.Level.TextureSamples[_editor.SelectedTexture.Index];

                int yBlock = (int)(texture.Page / 8);
                int xBlock = (int)(texture.Page % 8);

                UV[0] = new Vector2((xBlock * 256.0f + texture.X + 0.5f) / 2048.0f, (yBlock * 256.0f + texture.Y + 0.5f) / 2048.0f);
                UV[1] = new Vector2((xBlock * 256.0f + texture.X + texture.Width - 0.5f) / 2048.0f, (yBlock * 256.0f + texture.Y + 0.5f) / 2048.0f);
                UV[2] = new Vector2((xBlock * 256.0f + texture.X + texture.Width - 0.5f) / 2048.0f, (yBlock * 256.0f + texture.Y + texture.Height - 0.5f) / 2048.0f);
                UV[3] = new Vector2((xBlock * 256.0f + texture.X + 0.5f) / 2048.0f, (yBlock * 256.0f + texture.Y + texture.Height - 0.5f) / 2048.0f);

                room.GetBlock(pos).Faces[(int)faceType].RectangleUV[0] = UV[0];
                room.GetBlock(pos).Faces[(int)faceType].RectangleUV[1] = UV[1];
                room.GetBlock(pos).Faces[(int)faceType].RectangleUV[2] = UV[2];
                room.GetBlock(pos).Faces[(int)faceType].RectangleUV[3] = UV[3];

                /*
                *  1----2    Split 0: 231 413  
                *  | \  |    Split 1: 124 342
                *  |  \ |
                *  4----3
                */

                if (face.Shape == BlockFaceShape.Triangle)
                {
                    if (_editor.SelectedTexture.Triangle == TextureTileType.TriangleNW)
                    {
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[0] = UV[0];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[1] = UV[1];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[2] = UV[3];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[0] = UV[0];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[1] = UV[1];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[2] = UV[3];
                        }
                    }

                    if (_editor.SelectedTexture.Triangle == TextureTileType.TriangleNE)
                    {
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[0] = UV[1];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[1] = UV[2];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[2] = UV[0];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[0] = UV[1];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[1] = UV[2];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[2] = UV[0];
                        }
                    }

                    if (_editor.SelectedTexture.Triangle == TextureTileType.TriangleSE)
                    {
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[0] = UV[2];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[1] = UV[3];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[2] = UV[1];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[0] = UV[2];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[1] = UV[3];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[2] = UV[1];
                        }
                    }

                    if (_editor.SelectedTexture.Triangle == TextureTileType.TriangleSW)
                    {
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[0] = UV[3];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[1] = UV[0];
                        room.GetBlock(pos).Faces[(int)faceType].TriangleUV[2] = UV[2];

                        if (faceType == BlockFaces.FloorTriangle2 || faceType == BlockFaces.CeilingTriangle2)
                        {
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[0] = UV[3];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[1] = UV[0];
                            room.GetBlock(pos).Faces[(int)faceType].TriangleUV2[2] = UV[2];
                        }
                    }
                }

                if (face.Shape == BlockFaceShape.Triangle)
                {
                    room.GetBlock(pos).Faces[(int)faceType].TextureTriangle = _editor.SelectedTexture.Triangle;
                }

                room.GetBlock(pos).Faces[(int)faceType].Texture = (short)_editor.SelectedTexture.Index;
                room.GetBlock(pos).Faces[(int)faceType].Rotation = 0;
            }
        }

        public static void ApplyTexture(Room room, DrawingPoint pos, BlockFaces faceType)
        {
            ApplyTextureNoUpdated(room, pos, faceType);
            _editor.RoomTextureChange(room);
        }

        public static void TexturizeAllFloor(Room room)
        {
            for (int x = 0; x < room.NumXSectors - 1; x++)
                for (int z = 0; z < room.NumZSectors - 1; z++)
                {
                    ApplyTextureNoUpdated(room, new DrawingPoint(x, z), BlockFaces.Floor);
                    ApplyTextureNoUpdated(room, new DrawingPoint(x, z), BlockFaces.FloorTriangle2);
                }

            room.UpdateCompletely();
            _editor.RoomTextureChange(room);
        }

        public static void TexturizeAllCeiling(Room room)
        {
            for (int x = 0; x < room.NumXSectors - 1; x++)
                for (int z = 0; z < room.NumZSectors - 1; z++)
                {
                    ApplyTextureNoUpdated(room, new DrawingPoint(x, z), BlockFaces.Ceiling);
                    ApplyTextureNoUpdated(room, new DrawingPoint(x, z), BlockFaces.CeilingTriangle2);
                }

            room.UpdateCompletely();
            _editor.RoomTextureChange(room);
        }

        public static void TexturizeAllWalls(Room room)
        {
            for (int x = 0; x < room.NumXSectors; x++)
                for (int z = 0; z < room.NumZSectors; z++)
                    for (int k = 10; k <= 13; k++)
                        if (room.Blocks[x, z].Faces[k].Defined)
                            ApplyTextureNoUpdated(room, new DrawingPoint(x, z), (BlockFaces)k);

            room.UpdateCompletely();
            _editor.RoomTextureChange(room);
        }

        public static void PlaceLight(Room room, DrawingPoint pos, LightType lightType)
        {
            Block block = room.GetBlock(pos);
            int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;
            Vector3 position = new Vector3(pos.X * 1024 + 512, y * 256 + 128.0f, pos.Y * 1024 + 512);

            var instance = new Light(lightType) { Position = position };
            room.AddObject(_editor.Level, instance);
            _editor.ObjectChange(instance);
        }
        
        private static void AddObject(Room room, DrawingPoint pos, PositionBasedObjectInstance instance)
        {
            Block block = room.GetBlock(pos);
            int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

            instance.Position = new Vector3(pos.X * 1024 + 512, y * 256, pos.Y * 1024 + 512);
            room.AddObject(_editor.Level, instance);
            _editor.ObjectChange(instance);
        }

        public static void PlaceItem(Room room, DrawingPoint pos, ItemType itemType)
        {
            AddObject(room, pos, ItemInstance.FromItemType(itemType));
        }

        public static void PlaceCamera(Room room, DrawingPoint pos)
        {
            AddObject(room, pos, new CameraInstance());
        }

        public static void PlaceFlyByCamera(Room room, DrawingPoint pos)
        {
            AddObject(room, pos, new FlybyCameraInstance());
        }

        public static void PlaceSoundSource(Room room, DrawingPoint pos)
        {
            AddObject(room, pos, new SoundSourceInstance());
        }

        public static void PlaceSink(Room room, DrawingPoint pos)
        {
            AddObject(room, pos, new SinkInstance());
        }

        public static void DeleteRoom(Room room)
        {
            // Check if is the last room
            int roomCount = _editor.Level.Rooms.Count(r => r != null);
            if (roomCount <= 1)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You must have at least one room in your level", "Error");
                return;
            }

            // Check if room has portals
            int portalCount = room.Portals.Count();
            if (portalCount != 0)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You can't delete a room with portals to other rooms.", "Error");
                return;
            }

            // Ask for confirmation
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                    "Do you really want to delete this room? All objects inside room will be deleted and " +
                    "triggers pointing to them will be removed.",
                    "Delete room", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
            {
                return;
            }

            // Do it finally
            _editor.Level.DeleteRoom(room);

            // Update selection
            if (_editor.SelectedRoom == room)
                _editor.SelectRoomAndCenterCamera(_editor.Level.Rooms.FirstOrDefault(r => r != null));
            _editor.RoomListChange();
        }

        public static void CropRoom(Room room, Rectangle newArea)
        {
            if (room.Flipped)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You can't crop a flipped room yet :(", "Error");
                return;
            }
            newArea = newArea.Inflate(1);
            if ((newArea.Width + 1) > Room.MaxRoomDimensions ||
                (newArea.Height + 1) > Room.MaxRoomDimensions)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("The selected area exceeds the maximum room size.", "Error");
                return;
            }
            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Warning: if you crop this room, all portals and triggers outside the new area will be deleted." +
                " Do you want to continue?", "Crop room", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;
            
            // Resize room
            room.Resize(_editor.Level, newArea.Width + 1, newArea.Height + 1,
                (short)room.GetLowestCorner(), (short)room.GetHighestCorner(), new DrawingPoint(newArea.X, newArea.Y));

            // Fix selection if necessary
            if ((_editor.SelectedRoom == room) && _editor.SelectedSectors.Valid)
                _editor.SelectedSectors = new SectorSelection
                {
                    Area = _editor.SelectedSectors.Area.Intersect(newArea).OffsetNeg(new DrawingPoint(newArea.X, newArea.Y)),
                    Arrow = _editor.SelectedSectors.Arrow
                };
            _editor.RoomPropertiesChange(room);
            _editor.RoomSectorPropertiesChange(room);
        }
        
        public static void SetDiagonalFloorSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    // Now try to guess the floor split
                    short maxHeight = -32767;
                    byte theCorner = 0;

                    if (room.Blocks[x, z].QAFaces[0] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[0];
                        theCorner = 0;
                    }

                    if (room.Blocks[x, z].QAFaces[1] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[1];
                        theCorner = 1;
                    }

                    if (room.Blocks[x, z].QAFaces[2] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[2];
                        theCorner = 2;
                    }

                    if (room.Blocks[x, z].QAFaces[3] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[3];
                        theCorner = 3;
                    }

                    if (theCorner == 0)
                    {
                        room.Blocks[x, z].QAFaces[1] = maxHeight;
                        room.Blocks[x, z].QAFaces[3] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.SE;
                    }

                    if (theCorner == 1)
                    {
                        room.Blocks[x, z].QAFaces[0] = maxHeight;
                        room.Blocks[x, z].QAFaces[2] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.SW;
                    }

                    if (theCorner == 2)
                    {
                        room.Blocks[x, z].QAFaces[1] = maxHeight;
                        room.Blocks[x, z].QAFaces[3] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.NW;
                    }

                    if (theCorner == 3)
                    {
                        room.Blocks[x, z].QAFaces[0] = maxHeight;
                        room.Blocks[x, z].QAFaces[2] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.NE;
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void SetDiagonalCeilingSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    // Now try to guess the floor split
                    short minHeight = 32767;
                    byte theCorner = 0;

                    if (room.Blocks[x, z].WSFaces[0] < minHeight)
                    {
                        minHeight = room.Blocks[x, z].WSFaces[0];
                        theCorner = 0;
                    }

                    if (room.Blocks[x, z].WSFaces[1] < minHeight)
                    {
                        minHeight = room.Blocks[x, z].WSFaces[1];
                        theCorner = 1;
                    }

                    if (room.Blocks[x, z].WSFaces[2] < minHeight)
                    {
                        minHeight = room.Blocks[x, z].WSFaces[2];
                        theCorner = 2;
                    }

                    if (room.Blocks[x, z].WSFaces[3] < minHeight)
                    {
                        minHeight = room.Blocks[x, z].WSFaces[3];
                        theCorner = 3;
                    }

                    if (theCorner == 0)
                    {
                        room.Blocks[x, z].WSFaces[1] = minHeight;
                        room.Blocks[x, z].WSFaces[3] = minHeight;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.SE;
                    }

                    if (theCorner == 1)
                    {
                        room.Blocks[x, z].WSFaces[0] = minHeight;
                        room.Blocks[x, z].WSFaces[2] = minHeight;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.SW;
                    }

                    if (theCorner == 2)
                    {
                        room.Blocks[x, z].WSFaces[1] = minHeight;
                        room.Blocks[x, z].WSFaces[3] = minHeight;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.NW;
                    }

                    if (theCorner == 3)
                    {
                        room.Blocks[x, z].WSFaces[0] = minHeight;
                        room.Blocks[x, z].WSFaces[2] = minHeight;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.NE;
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void SetDiagonalWallSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    // Now try to guess the floor split
                    short maxHeight = -32767;
                    byte theCorner = 0;

                    if (room.Blocks[x, z].QAFaces[0] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[0];
                        theCorner = 0;
                    }

                    if (room.Blocks[x, z].QAFaces[1] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[1];
                        theCorner = 1;
                    }

                    if (room.Blocks[x, z].QAFaces[2] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[2];
                        theCorner = 2;
                    }

                    if (room.Blocks[x, z].QAFaces[3] > maxHeight)
                    {
                        maxHeight = room.Blocks[x, z].QAFaces[3];
                        theCorner = 3;
                    }

                    if (theCorner == 0)
                    {
                        room.Blocks[x, z].QAFaces[1] = maxHeight;
                        room.Blocks[x, z].QAFaces[3] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.SE;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.SE;
                    }

                    if (theCorner == 1)
                    {
                        room.Blocks[x, z].QAFaces[0] = maxHeight;
                        room.Blocks[x, z].QAFaces[2] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.SW;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.SW;
                    }

                    if (theCorner == 2)
                    {
                        room.Blocks[x, z].QAFaces[1] = maxHeight;
                        room.Blocks[x, z].QAFaces[3] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.NW;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.NW;
                    }

                    if (theCorner == 3)
                    {
                        room.Blocks[x, z].QAFaces[0] = maxHeight;
                        room.Blocks[x, z].QAFaces[2] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.NE;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.NE;
                    }

                    room.Blocks[x, z].Type = BlockType.Wall;
                }

            SmartBuildGeometry(room, area);
        }
        
        public static void SetWall(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    room.Blocks[x, z].Type = BlockType.Wall;
                }

            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetFloor(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;
                    room.Blocks[x, z].SplitFloor = false;
                    room.Blocks[x, z].Type = BlockType.Floor;
                    room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.None;
                }

            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetCeiling(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;
                    room.Blocks[x, z].SplitCeiling = false;
                    room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.None;
                }

            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void ToggleBlockFlag(Room room, Rectangle area, BlockFlags flag)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                    room.Blocks[x, z].Flags ^= flag;
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void ToggleClimb(Room room, Rectangle area, int direction)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                    room.Blocks[x, z].Climb[direction] = !room.Blocks[x, z].Climb[direction];
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void AddPortal(Room room, Rectangle area)
        {
            // Check if fliproom
            if (room.Flipped)
                throw new NotSupportedException("Unfortunately we don't support adding portals to flipped rooms currently. :(");

            // Check for possible candidates ...
            VerticalSpace? verticalSpaceLocal = room.GetHeightInAreaMaxSpace(new Rectangle(area.X, area.Y, area.Right + 1, area.Bottom + 1));

            List<Tuple<PortalDirection, Room>> candidates = new List<Tuple<PortalDirection, Room>>();
            if (verticalSpaceLocal != null)
            {
                VerticalSpace verticalSpace = verticalSpaceLocal.Value + room.Position.Y;
                bool couldBeFloorCeilingPortal = false;
                if (new Rectangle(1, 1, room.NumXSectors - 2, room.NumZSectors - 2).Contains(area))
                    for (int z = area.Top; z <= area.Bottom; ++z)
                        for (int x = area.Left; x <= area.Right; ++x)
                            if (room.Blocks[x, z].IsFloor)
                                couldBeFloorCeilingPortal = true;
                
                foreach (Room neighborRoom in _editor.Level.Rooms.Where(possibleNeighborRoom => possibleNeighborRoom != null))
                {
                    if (neighborRoom == room)
                        continue;
                    Rectangle neighborArea = area.Offset(room.SectorPos).OffsetNeg(neighborRoom.SectorPos);
                    if (!new Rectangle(0, 0, neighborRoom.NumXSectors - 1, neighborRoom.NumZSectors - 1).Contains(neighborArea))
                        continue;

                    // Check if they vertically touch
                    VerticalSpace? neighborVerticalSpaceLocal = neighborRoom.GetHeightInAreaMaxSpace(
                        new Rectangle(neighborArea.X, neighborArea.Y, neighborArea.Right + 1, neighborArea.Bottom + 1));
                    if (neighborVerticalSpaceLocal == null)
                        continue;
                    VerticalSpace neighborVerticalSpace = neighborVerticalSpaceLocal.Value + neighborRoom.Position.Y;
                    if (!((verticalSpace.FloorY <= neighborVerticalSpace.CeilingY) && (verticalSpace.CeilingY >= neighborVerticalSpace.FloorY)))
                        continue;

                    // Decide on a direction
                    if (couldBeFloorCeilingPortal &&
                        new Rectangle(1, 1, neighborRoom.NumXSectors - 2, neighborRoom.NumZSectors - 2).Contains(neighborArea))
                    {
                        if (Math.Abs(neighborVerticalSpace.CeilingY - verticalSpace.FloorY) <
                            Math.Abs(neighborVerticalSpace.FloorY - verticalSpace.CeilingY))
                        { // Consider floor portal
                            candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.Floor, neighborRoom));
                        }
                        else
                        { // Consider ceiling portal
                            candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.Ceiling, neighborRoom));
                        }
                    }
                    if ((area.Width == 0) && (area.X == 0))
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.West, neighborRoom));
                    if ((area.Width == 0) && (area.X == (room.NumXSectors - 1)))
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.East, neighborRoom));
                    if ((area.Height == 0) && (area.Y == 0))
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.North, neighborRoom));
                    if ((area.Height == 0) && (area.Y == (room.NumZSectors - 1)))
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.South, neighborRoom));
                }
            }

            if (candidates.Count > 1)
                throw new NotSupportedException("There is more than 1 valid way to connect to a room here! An option dialog is not yet implemented, sorry. :(");
            if (candidates.Count < 1)
                throw new ApplicationException("No room candidate found to connect to.");

            PortalDirection destinationDirection = candidates[0].Item1;
            Room destination = candidates[0].Item2;
            if (destination.Flipped)
                throw new NotSupportedException("Unfortunately we don't support adding portals to flipped rooms currently. :(");
            
            // Create portals
            Portal portal = new Portal(area, destinationDirection, destination);
            Portal oppositePortal = room.AddBidirectionalPortalsToLevel(_editor.Level, portal);

            // Update
            room.UpdateCompletely();
            destination.UpdateCompletely();

            _editor.ObjectChange(portal);
            _editor.ObjectChange(oppositePortal);

            _editor.RoomSectorPropertiesChange(room);
            _editor.RoomSectorPropertiesChange(destination);
        }

        public static void SetPortalOpacity(Room room, Portal portal, PortalOpacity opacity)
        {
            for (int x = portal.Area.X; x <= portal.Area.Right; x++)
                for (int z = portal.Area.Y; z <= portal.Area.Bottom; z++)
                {
                    switch (portal.Direction)
                    {
                        case PortalDirection.North:
                            room.Blocks[x, z].WallOpacity = opacity;
                            break;

                        case PortalDirection.South:
                            room.Blocks[x, z].WallOpacity = opacity;
                            break;

                        case PortalDirection.East:
                            room.Blocks[x, z].WallOpacity = opacity;
                            break;

                        case PortalDirection.West:
                            room.Blocks[x, z].WallOpacity = opacity;
                            break;

                        case PortalDirection.Floor:
                            if (room.Blocks[x, z].FloorPortal != null)
                                room.Blocks[x, z].FloorOpacity = opacity;
                            else
                                room.Blocks[x, z].FloorOpacity = PortalOpacity.None;
                            break;

                        case PortalDirection.Ceiling:
                            if (room.Blocks[x, z].CeilingPortal != null)
                                room.Blocks[x, z].CeilingOpacity = opacity;
                            else
                                room.Blocks[x, z].CeilingOpacity = PortalOpacity.None;
                            break;
                    }
                }
            room.UpdateCompletely();
            _editor.ObjectChange(portal);
        }

        public static void CopyRoom(Room room, Rectangle area)
        {
            int found = _editor.Level.GetFreeRoomIndex();
            var newRoom = new Room(_editor.Level, area.Width + 3, area.Height + 3, "Room " + found);

            for (int x = 1; x < area.Width - 1; x++)
                for (int z = 1; z < area.Height - 1; z++)
                    newRoom.Blocks[x, z] = room.Blocks[x + area.X - 1, z + area.Y - 1].Clone();
            
            newRoom.UpdateCompletely();

            _editor.Level.Rooms[found] = newRoom;
            _editor.RoomListChange();
        }

        public static void SplitRoom(Room room, Rectangle area)
        {
            if (room.Flipped)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You can't split a flipped room", "Error");
                return;
            }

            int found = _editor.Level.GetFreeRoomIndex();
            var newRoom = new Room(_editor.Level, area.Width + 3, area.Height + 3, "Room " + found);

            for (int x = 0; x < area.Width + 1; x++)
                for (int z = 0; z < area.Height + 1; z++)
                {
                    newRoom.Blocks[x, z] = room.Blocks[x + area.X, z + area.Y].Clone();
                    room.Blocks[x + area.X, z + area.Y].Type = BlockType.Wall;
                }

            // Build the geometry of the new room
            room.UpdateCompletely();
            newRoom.UpdateCompletely();

            // Update the UI
            _editor.Level.Rooms[found] = newRoom;
            _editor.RoomListChange();
        }

        public static void AlternateRoomEnable(Room room, short AlternateGroup)
        {
            // Search the first free room
            int freeRoomIndex = _editor.Level.GetFreeRoomIndex();

            // Duplicate portals
            var duplicatedPortals = new Dictionary<Portal, Portal>();

            foreach (var p in room.Portals)
                duplicatedPortals.Add(p, (Portal)p.Clone());
            
            string name = "(Flipped of " + room.ToString() + ") Room " + freeRoomIndex;
            var newRoom = new Room(_editor.Level, room.NumXSectors, room.NumZSectors, name);

            for (int x = 0; x < room.NumXSectors; x++)
                for (int z = 0; z < room.NumZSectors; z++)
                {
                    newRoom.Blocks[x, z] = room.Blocks[x, z].Clone();
                    newRoom.Blocks[x, z].FloorPortal = (room.Blocks[x, z].FloorPortal != null
                        ? duplicatedPortals[room.Blocks[x, z].FloorPortal] : null);
                    newRoom.Blocks[x, z].CeilingPortal = (room.Blocks[x, z].CeilingPortal != null
                        ? duplicatedPortals[room.Blocks[x, z].CeilingPortal] : null);
                    newRoom.Blocks[x, z].WallPortal = (room.Blocks[x, z].WallPortal != null
                        ? duplicatedPortals[room.Blocks[x, z].WallPortal] : null);
                }

            foreach (var instance in room.Objects)
                if (instance.CopyToFlipRooms)
                    newRoom.Objects.Add((PositionBasedObjectInstance)instance.Clone());

            // Build the geometry of the new room
            newRoom.UpdateCompletely();

            _editor.Level.Rooms[freeRoomIndex] = newRoom;
            _editor.RoomListChange();
            
            room.AlternateGroup = AlternateGroup;
            room.AlternateRoom = newRoom;
            _editor.RoomPropertiesChange(room);
            
            newRoom.AlternateGroup = AlternateGroup;
            newRoom.AlternateBaseRoom = room;
            _editor.RoomPropertiesChange(newRoom);
        }

        public static void AlternateRoomDisable(Room room)
        {
            // Check if room has portals
            if (room.Portals.Count() > 0)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You can't delete a room with portals to other rooms.", "Error");
                return;
            }

            // Ask for confirmation
            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Do you really want to delete the flip room?",
                "Delete flipped room", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
            {
                return;
            }

            _editor.Level.DeleteRoom(room.AlternateRoom);
            _editor.RoomListChange();
            
            room.AlternateRoom = null;
            room.AlternateGroup = -1;
            _editor.RoomPropertiesChange(room);

        }

        public static void SmoothRandomFloor(Room room, Rectangle area, float strengthDirection)
        {
            float[,] changes = new float[area.Width + 2, area.Height + 2];
            Random rng = new Random();
            for (int x = 1; x <= area.Width; x++)
                for (int z = 1; z <= area.Height; z++)
                    changes[x, z] = rng.NextFloat(0, 1) * strengthDirection;

            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (int i = 0; i < 4; ++i)
                        room.Blocks[area.X + x, area.Y + z].QAFaces[i] += 
                            (short)Math.Round(changes[x + Block.FaceX[i], z + Block.FaceZ[i]]);

            SmartBuildGeometry(room, area);
        }

        public static void SmoothRandomCeiling(Room room, Rectangle area, float strengthDirection)
        {
            float[,] changes = new float[area.Width + 2, area.Height + 2];
            Random rng = new Random();
            for (int x = 1; x <= area.Width; x++)
                for (int z = 1; z <= area.Height; z++)
                    changes[x, z] = rng.NextFloat(0, 1) * strengthDirection;

            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (int i = 0; i < 4; ++i)
                        room.Blocks[area.X + x, area.Y + z].WSFaces[i] +=
                            (short)Math.Round(changes[x + Block.FaceX[i], z + Block.FaceZ[i]]);

            SmartBuildGeometry(room, area);
        }

        public static void SharpRandomFloor(Room room, Rectangle area, float strengthDirection)
        {
            Random rng = new Random();
            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (int i = 0; i < 4; ++i)
                        room.Blocks[area.X + x, area.Y + z].QAFaces[i] +=
                            (short)Math.Round(rng.NextFloat(0, 1) * strengthDirection);

            SmartBuildGeometry(room, area);
        }

        public static void SharpRandomCeiling(Room room, Rectangle area, float strengthDirection)
        {
            Random rng = new Random();
            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (int i = 0; i < 4; ++i)
                        room.Blocks[area.X + x, area.Y + z].WSFaces[i] +=
                            (short)Math.Round(rng.NextFloat(0, 1) * strengthDirection);

            SmartBuildGeometry(room, area);
        }

        public static void FlattenFloor(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    Block b = room.Blocks[x, z];

                    short mean = (short)((b.QAFaces[0] + b.QAFaces[1] + b.QAFaces[2] + b.QAFaces[3]) / 4);

                    for (int i = 0; i < 4; i++)
                    {
                        room.Blocks[x, z].QAFaces[i] = mean;
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void FlattenCeiling(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    Block b = room.Blocks[x, z];

                    short mean = (short)((b.WSFaces[0] + b.WSFaces[1] + b.WSFaces[2] + b.WSFaces[3]) / 4);

                    for (int i = 0; i < 4; i++)
                    {
                        room.Blocks[x, z].WSFaces[i] = mean;
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void GridWalls3(Room room, Rectangle Area)
        {
            for (int x = Area.X; x <= Area.Right; x++)
                for (int z = Area.Y; z <= Area.Bottom; z++)
                {
                    Block block = room.Blocks[x, z];
                    if (block.IsAnyWall)
                    {
                        VerticalSpace?[] verticalAreas = new VerticalSpace?[4];
                        for (int i = 0; i < 4; ++i)
                            verticalAreas[i] = room.GetHeightAtPointMinSpace(x + Block.FaceX[i], z + Block.FaceZ[i]);
                        if (verticalAreas.Any((verticalArea) => verticalArea.HasValue)) // We can only do it if there is information available
                            for (int i = 0; i < 4; ++i)
                            {
                                // Use the closest available vertical area information and divide it equally
                                VerticalSpace verticalArea = verticalAreas[i] ?? verticalAreas[(i + 1) % 4] ?? verticalAreas[(i + 3) % 4] ?? verticalAreas[(i + 2) % 4].Value;
                                block.EDFaces[i] = (short)Math.Round(verticalArea.FloorY);
                                block.QAFaces[i] = (short)Math.Round((verticalArea.FloorY * 2.0f + verticalArea.CeilingY * 1.0f) / 3.0f);
                                block.WSFaces[i] = (short)Math.Round((verticalArea.FloorY * 1.0f + verticalArea.CeilingY * 2.0f) / 3.0f);
                                block.RFFaces[i] = (short)Math.Round(verticalArea.CeilingY);
                            }
                    }
                }

            SmartBuildGeometry(room, Area);
        }

        public static void GridWalls5(Room room, Rectangle Area)
        {
            for (int x = Area.X; x <= Area.Right; x++)
                for (int z = Area.Y; z <= Area.Bottom; z++)
                {
                    Block block = room.Blocks[x, z];
                    if (block.IsAnyWall)
                    {
                        VerticalSpace?[] verticalAreas = new VerticalSpace?[4];
                        for (int i = 0; i < 4; ++i)
                            verticalAreas[i] = room.GetHeightAtPointMinSpace(x + Block.FaceX[i], z + Block.FaceZ[i]);
                        if (verticalAreas.Any(verticalArea => verticalArea.HasValue)) // We can only do it if there is information available
                            for (int i = 0; i < 4; ++i)
                            {
                                // Use the closest available vertical area information and divide it equally
                                VerticalSpace verticalArea = verticalAreas[i] ?? verticalAreas[(i + 1) % 4] ?? verticalAreas[(i + 3) % 4] ?? verticalAreas[(i + 2) % 4].Value;
                                block.EDFaces[i] = (short)Math.Round((verticalArea.FloorY * 4.0f + verticalArea.CeilingY * 1.0f) / 5.0f);
                                block.QAFaces[i] = (short)Math.Round((verticalArea.FloorY * 3.0f + verticalArea.CeilingY * 2.0f) / 5.0f);
                                block.WSFaces[i] = (short)Math.Round((verticalArea.FloorY * 2.0f + verticalArea.CeilingY * 3.0f) / 5.0f);
                                block.RFFaces[i] = (short)Math.Round((verticalArea.FloorY * 1.0f + verticalArea.CeilingY * 4.0f) / 5.0f);
                            }
                    }
                }

            SmartBuildGeometry(room, Area);
        }

        public static void CreateRoomAboveOrBelow(Room room, Func<Room, float> GetYOffset, short newRoomHeight)
        {
            // Create room
            var newRoom = new Room(_editor.Level, room.NumXSectors, room.NumZSectors, "room next to " + room.Name, newRoomHeight);
            newRoom.Position = room.Position + new Vector3(0, GetYOffset(newRoom), 0);
            _editor.Level.AssignRoomToFree(newRoom);
            _editor.RoomListChange();

            // Build the geometry of the new room
            newRoom.UpdateCompletely();
            
            // Update the UI
            if (_editor.SelectedRoom == room)
                _editor.SelectedRoom = newRoom; //Don't center
        }
    }
}
