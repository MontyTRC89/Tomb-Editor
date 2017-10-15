using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NLog;
using TombEditor.Compilers;
using TombEditor.Geometry;
using TombEditor.Geometry.IO;
using TombLib.Utils;
using DarkUI.Forms;
using TombLib.IO;
using System.IO;

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
                    room.GetBlockTry(x, area.Bottom + 1)?.ChangeEdge(verticalSubdivision, Block.FaceXpZn, increment);
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
                                if (block.FloorDiagonalSplit == DiagonalSplit.XpZn && block.QAFaces[2] == block.QAFaces[0] && increment < 0)
                                    continue;
                                if (block.FloorDiagonalSplit == DiagonalSplit.XnZn && block.QAFaces[3] == block.QAFaces[1] && increment < 0)
                                    continue;
                                if (block.FloorDiagonalSplit == DiagonalSplit.XnZp && block.QAFaces[0] == block.QAFaces[2] && increment < 0)
                                    continue;
                                if (block.FloorDiagonalSplit == DiagonalSplit.XpZp && block.QAFaces[1] == block.QAFaces[3] && increment < 0)
                                    continue;

                                if (block.FloorDiagonalSplit != DiagonalSplit.XpZn)
                                    room.Blocks[x, z].QAFaces[0] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.XnZn)
                                    room.Blocks[x, z].QAFaces[1] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.XnZp)
                                    room.Blocks[x, z].QAFaces[2] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.XpZp)
                                    room.Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.CeilingDiagonalSplit == DiagonalSplit.XpZn && block.WSFaces[2] == block.WSFaces[0] && increment > 0)
                                    continue;
                                if (block.CeilingDiagonalSplit == DiagonalSplit.XnZn && block.WSFaces[3] == block.WSFaces[1] && increment > 0)
                                    continue;
                                if (block.CeilingDiagonalSplit == DiagonalSplit.XnZp && block.WSFaces[0] == block.WSFaces[2] && increment > 0)
                                    continue;
                                if (block.CeilingDiagonalSplit == DiagonalSplit.XpZp && block.WSFaces[1] == block.WSFaces[3] && increment > 0)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.XpZn)
                                    room.Blocks[x, z].WSFaces[0] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.XnZn)
                                    room.Blocks[x, z].WSFaces[1] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.XnZp)
                                    room.Blocks[x, z].WSFaces[2] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.XpZp)
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

                                if (block.FloorDiagonalSplit != DiagonalSplit.XpZn)
                                    room.Blocks[x, z].QAFaces[0] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.XnZn)
                                    room.Blocks[x, z].QAFaces[1] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.XpZn)
                                    room.Blocks[x, z].WSFaces[0] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.XnZn)
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

                                if (block.FloorDiagonalSplit != DiagonalSplit.XnZn)
                                    room.Blocks[x, z].QAFaces[1] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.XnZp)
                                    room.Blocks[x, z].QAFaces[2] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.XnZn)
                                    room.Blocks[x, z].WSFaces[1] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.XnZp)
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

                                if (block.FloorDiagonalSplit != DiagonalSplit.XnZp)
                                    room.Blocks[x, z].QAFaces[2] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.XpZp)
                                    room.Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.XnZp)
                                    room.Blocks[x, z].WSFaces[2] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.XpZp)
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

                                if (block.FloorDiagonalSplit != DiagonalSplit.XpZn)
                                    room.Blocks[x, z].QAFaces[0] += increment;
                                if (block.FloorDiagonalSplit != DiagonalSplit.XpZp)
                                    room.Blocks[x, z].QAFaces[3] += increment;

                                if (block.FloorPortal != null)
                                    continue;
                            }
                            else if (verticalSubdivision == 1)
                            {
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit != DiagonalSplit.XpZn)
                                    room.Blocks[x, z].WSFaces[0] += increment;
                                if (block.CeilingDiagonalSplit != DiagonalSplit.XpZp)
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
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.XnZp && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.XnZp)
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
                                if (block.CeilingDiagonalSplit != DiagonalSplit.XnZp && block.CeilingDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit == DiagonalSplit.XnZp)
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
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.XpZp && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.XpZp)
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
                                if (block.CeilingDiagonalSplit != DiagonalSplit.XpZp && block.CeilingDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit == DiagonalSplit.XpZp)
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
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.XpZn && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.XpZn)
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
                                if (block.CeilingDiagonalSplit != DiagonalSplit.XpZn && block.CeilingDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit == DiagonalSplit.XpZn)
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
                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit != DiagonalSplit.XnZn && block.FloorDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.Type != BlockType.Wall && block.FloorDiagonalSplit == DiagonalSplit.XnZn)
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
                                if (block.CeilingDiagonalSplit != DiagonalSplit.XnZn && block.CeilingDiagonalSplit != DiagonalSplit.None)
                                    continue;

                                if (block.CeilingDiagonalSplit == DiagonalSplit.XnZn)
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
                            if (block.FloorDiagonalSplit == DiagonalSplit.XpZn)
                            {
                                if (block.QAFaces[0] == block.QAFaces[1] && increment > 0)
                                    continue;
                                room.Blocks[x, z].QAFaces[0] += increment;
                            }

                            if (block.FloorDiagonalSplit == DiagonalSplit.XnZn)
                            {
                                if (block.QAFaces[1] == block.QAFaces[2] && increment > 0)
                                    continue;
                                room.Blocks[x, z].QAFaces[1] += increment;
                            }

                            if (block.FloorDiagonalSplit == DiagonalSplit.XnZp)
                            {
                                if (block.QAFaces[2] == block.QAFaces[3] && increment > 0)
                                    continue;
                                room.Blocks[x, z].QAFaces[2] += increment;
                            }

                            if (block.FloorDiagonalSplit == DiagonalSplit.XpZp)
                            {
                                if (block.QAFaces[3] == block.QAFaces[0] && increment > 0)
                                    continue;
                                room.Blocks[x, z].QAFaces[3] += increment;
                            }

                            break;

                        case EditorArrowType.DiagonalCeilingCorner:
                            if (block.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                            {
                                if (block.WSFaces[0] == block.WSFaces[1] && increment < 0)
                                    continue;
                                room.Blocks[x, z].WSFaces[0] += increment;
                            }

                            if (block.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                            {
                                if (block.WSFaces[1] == block.WSFaces[2] && increment < 0)
                                    continue;
                                room.Blocks[x, z].WSFaces[1] += increment;
                            }

                            if (block.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                            {
                                if (block.WSFaces[2] == block.WSFaces[3] && increment < 0)
                                    continue;
                                room.Blocks[x, z].WSFaces[2] += increment;
                            }

                            if (block.CeilingDiagonalSplit == DiagonalSplit.XpZp)
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
                    room.Blocks[x, z].FloorSplitDirectionToggled = !room.Blocks[x, z].FloorSplitDirectionToggled;

            SmartBuildGeometry(room, area);
        }

        public static void FlipCeilingSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                    room.Blocks[x, z].CeilingSplitDirectionToggled = !room.Blocks[x, z].CeilingSplitDirectionToggled;

            SmartBuildGeometry(room, area);
        }

        public static void AddTrigger(Room room, Rectangle area, IWin32Window owner)
        {
            var trigger = new TriggerInstance(area);

            // Initialize trigger with selected object if the selected object makes sense in the trigger context.
            if (_editor.SelectedObject is MoveableInstance)
            {
                trigger.TargetType = TriggerTargetType.Object;
                trigger.TargetObj = _editor.SelectedObject;
            }
            else if (_editor.SelectedObject is FlybyCameraInstance)
            {
                trigger.TargetType = TriggerTargetType.FlyByCamera;
                trigger.TargetObj = _editor.SelectedObject;
            }
            else if (_editor.SelectedObject is CameraInstance)
            {
                trigger.TargetType = TriggerTargetType.Camera;
                trigger.TargetObj = _editor.SelectedObject;
            }
            else if (_editor.SelectedObject is SinkInstance)
            {
                trigger.TargetType = TriggerTargetType.Sink;
                trigger.TargetObj = _editor.SelectedObject;
            }

            // Display form
            using (var formTrigger = new FormTrigger(_editor.Level, trigger, obj => _editor.ShowObject(obj)))
            {
                if (formTrigger.ShowDialog(owner) != DialogResult.OK)
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

        public static void ScaleObject(IScaleable instance, float newScale, double quantization)
        {
            if (quantization != 0.0f)
            {
                double logScale = Math.Log(newScale);
                double logQuantization = Math.Log(quantization);
                logScale = Math.Round(logScale / logQuantization) * logQuantization;
                newScale = (float)Math.Exp(logScale);
            }
            // Set some limits to scale
            // TODO: object risks to be too small and to be not pickable. We should add some size check
            if (newScale < (1 / 32.0f))
                newScale = (1 / 32.0f);
            if (newScale > 128.0f)
                newScale = 128.0f;
            instance.Scale = newScale;

            _editor.ObjectChange(_editor.SelectedObject);
        }

        public static void MoveObject(PositionBasedObjectInstance instance, Vector3 pos, Keys modifierKeys)
        {
            MoveObject(instance, pos, GetMovementPrecision(modifierKeys), modifierKeys.HasFlag(Keys.Alt));
        }

        public static void MoveObject(PositionBasedObjectInstance instance, Vector3 pos, Vector3 precision = new Vector3(), bool canGoOutsideRoom = false)
        {
            if (instance == null)
                return;

            // Limit movement precision
            for (int i = 0; i < 3; ++i)
                if (precision[i] != 0)
                    pos[i] = ((float)Math.Round(pos[i] / precision[i])) * precision[i];

            // Limit movement area
            if (!canGoOutsideRoom)
            {
                float x = (float)Math.Floor(pos.X / 1024.0f);
                float z = (float)Math.Floor(pos.Z / 1024.0f);

                if ((x < 0.0f) || (x > (instance.Room.NumXSectors - 1)) ||
                    (z < 0.0f) || (z > (instance.Room.NumZSectors - 1)))
                    return;

                Block block = instance.Room.Blocks[(int)x, (int)z];
                if (block.IsAnyWall)
                    return;
            }

            // Update position
            instance.Position = pos;

            // Update state
            if (instance is LightInstance)
            {
                instance.Room.CalculateLightingForThisRoom();
                instance.Room.UpdateBuffers();
            }
            _editor.ObjectChange(instance);
        }

        public static void MoveObjectRelative(PositionBasedObjectInstance instance, Vector3 pos, Vector3 precision = new Vector3(), bool canGoOutsideRoom = false)
        {
            MoveObject(instance, instance.Position + pos, precision, canGoOutsideRoom);
        }

        public enum RotationAxis
        {
            Y,
            X,
            Roll
        };

        public static void RotateObject(ObjectInstance instance, RotationAxis axis, float angleInDegrees, float quantization = 0.0f, bool delta = true)
        {
            if (quantization != 0.0f)
                angleInDegrees = (float)(Math.Round(angleInDegrees / quantization) * quantization);

            switch (axis)
            {
                case RotationAxis.Y:
                    var rotateableY = instance as IRotateableY;
                    if (rotateableY == null)
                        return;
                    rotateableY.RotationY = angleInDegrees + (delta ? rotateableY.RotationY : 0);
                    break;
                case RotationAxis.X:
                    var rotateableX = instance as IRotateableYX;
                    if (rotateableX == null)
                        return;
                    rotateableX.RotationX = angleInDegrees + (delta ? rotateableX.RotationX : 0);
                    break;
                case RotationAxis.Roll:
                    var rotateableRoll = instance as IRotateableYXRoll;
                    if (rotateableRoll == null)
                        return;
                    rotateableRoll.Roll = angleInDegrees + (delta ? rotateableRoll.Roll : 0);
                    break;
            }
            if (instance is LightInstance)
                instance.Room.UpdateCompletely();
            _editor.ObjectChange(instance);
        }

        public static void EditObject(ObjectInstance instance, IWin32Window owner)
        {
            if (instance is MoveableInstance)
            {
                using (var formMoveable = new FormMoveable((MoveableInstance)instance))
                    formMoveable.ShowDialog(owner);
                _editor.ObjectChange(instance);
            }
            else if (instance is StaticInstance)
            {
                using (var formStaticMesh = new FormStaticMesh((StaticInstance)instance))
                    formStaticMesh.ShowDialog(owner);
                _editor.ObjectChange(instance);
            }
            else if (instance is FlybyCameraInstance)
            {
                using (var formFlyby = new FormFlybyCamera((FlybyCameraInstance)instance))
                    formFlyby.ShowDialog(owner);
                _editor.ObjectChange(instance);
            }
            else if (instance is CameraInstance)
            {
                using (var formCamera = new FormCamera((CameraInstance)instance))
                    formCamera.ShowDialog(owner);
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
                using (var formTrigger = new FormTrigger(_editor.Level, (TriggerInstance)instance, obj => _editor.ShowObject(obj)))
                    formTrigger.ShowDialog(owner);
                _editor.ObjectChange(instance);
            }
            else if (instance is ImportedGeometryInstance)
            {
                using (var formImportedGeometry = new FormImportedGeometry((ImportedGeometryInstance)instance, _editor.Level.Settings))
                    if (formImportedGeometry.ShowDialog(owner) != DialogResult.Cancel)
                        _editor.UpdateLevelSettings(formImportedGeometry.NewLevelSettings);
                _editor.ObjectChange(instance);
            }
        }

        public static void DeleteObjectWithWarning(ObjectInstance instance, IWin32Window owner)
        {
            if (instance.Room.Flipped && (instance is PortalInstance))
            {
                DarkMessageBox.Show(owner, "You can't delete portals of a flipped room currently. :(", "Error", MessageBoxIcon.Error);
                return;
            }

            if (DarkMessageBox.Show(owner, "Do you really want to delete " + instance.ToString() + "?",
                    "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            DeleteObject(instance);
        }

        public static void DeleteObject(ObjectInstance instance)
        {
            Room room = instance.Room;
            room.RemoveObject(_editor.Level, instance);

            // Additional updates
            if (instance is SectorBasedObjectInstance)
                _editor.RoomSectorPropertiesChange(room);
            if (instance is LightInstance)
                room.UpdateCompletely();

            // Remove triggers pointing to that object
            foreach (var r in _editor.Level.Rooms)
            {
                if (r == null)
                    continue;

                List<TriggerInstance> triggersToDelete = new List<TriggerInstance>();

                foreach (var trigger in r.Triggers)
                {
                    if (trigger.TargetObj != null && trigger.TargetObj == instance)
                        triggersToDelete.Add(trigger);
                }

                foreach (var trigger in triggersToDelete)
                {
                    r.RemoveObject(_editor.Level, trigger);
                }
            }

            // Avoid having the removed object still selected
            if (_editor.SelectedObject == instance)
                _editor.SelectedObject = null;
            _editor.ObjectChange(null);
        }

        public static void RotateTexture(Room room, DrawingPoint pos, BlockFace face)
        {
            Block blocks = room.GetBlock(pos);
            TextureArea textureArea = blocks.GetFaceTexture(face);
            if (room.GetFaceVertexRange(pos.X, pos.Y, face).Count == 3)
            {
                Vector2 tempTexCoord = textureArea.TexCoord2;
                textureArea.TexCoord2 = textureArea.TexCoord1;
                textureArea.TexCoord1 = textureArea.TexCoord0;
                textureArea.TexCoord0 = tempTexCoord;
                textureArea.TexCoord3 = textureArea.TexCoord2;
            }
            else
            {
                Vector2 tempTexCoord = textureArea.TexCoord3;
                textureArea.TexCoord3 = textureArea.TexCoord2;
                textureArea.TexCoord2 = textureArea.TexCoord1;
                textureArea.TexCoord1 = textureArea.TexCoord0;
                textureArea.TexCoord0 = tempTexCoord;
            }
            blocks.SetFaceTexture(face, textureArea);

            // Update state
            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.RoomTextureChange(room);
        }

        public static void MirrorTexture(Room room, DrawingPoint pos, BlockFace face)
        {
            Block blocks = room.GetBlock(pos);
            TextureArea textureArea = blocks.GetFaceTexture(face);
            if (room.GetFaceVertexRange(pos.X, pos.Y, face).Count == 3)
            {
                Utils.Swap(ref textureArea.TexCoord0, ref textureArea.TexCoord2);
                textureArea.TexCoord3 = textureArea.TexCoord2;
            }
            else
            {
                Utils.Swap(ref textureArea.TexCoord0, ref textureArea.TexCoord1);
                Utils.Swap(ref textureArea.TexCoord2, ref textureArea.TexCoord3);
            }
            blocks.SetFaceTexture(face, textureArea);

            // Update state
            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.RoomTextureChange(room);
        }

        public static void PickTexture(Room room, DrawingPoint pos, BlockFace face)
        {
            _editor.SelectTextureAndCenterView(room.GetBlock(pos).GetFaceTexture(face));
        }

        public static void RotateSelectedTexture()
        {
            TextureArea textureArea = _editor.SelectedTexture;
            Vector2 texCoordTemp = textureArea.TexCoord3;
            textureArea.TexCoord3 = textureArea.TexCoord2;
            textureArea.TexCoord2 = textureArea.TexCoord1;
            textureArea.TexCoord1 = textureArea.TexCoord0;
            textureArea.TexCoord0 = texCoordTemp;
            _editor.SelectedTexture = textureArea;
        }

        private static void ApplyTextureAutomaticallyNoUpdated(Room room, DrawingPoint pos, BlockFace face, TextureArea texture)
        {
            Block block = room.GetBlock(pos);
            block.SetFaceTexture(face, texture);
        }

        public static void ApplyTextureAutomatically(Room room, DrawingPoint pos, BlockFace face, TextureArea texture)
        {
            ApplyTextureAutomaticallyNoUpdated(room, pos, face, texture);

            room.BuildGeometry(new Rectangle(pos.X, pos.Y, pos.X, pos.Y));
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.RoomTextureChange(room);
        }

        public static void TexturizeAllFloor(Room room, TextureArea texture)
        {
            for (int x = 0; x < room.NumXSectors - 1; x++)
                for (int z = 0; z < room.NumZSectors - 1; z++)
                {
                    ApplyTextureAutomaticallyNoUpdated(room, new DrawingPoint(x, z), BlockFace.Floor, texture);
                    ApplyTextureAutomaticallyNoUpdated(room, new DrawingPoint(x, z), BlockFace.FloorTriangle2, texture);
                }

            room.UpdateCompletely();
            _editor.RoomTextureChange(room);
        }

        public static void TexturizeAllCeiling(Room room, TextureArea texture)
        {
            for (int x = 0; x < room.NumXSectors - 1; x++)
                for (int z = 0; z < room.NumZSectors - 1; z++)
                {
                    ApplyTextureAutomaticallyNoUpdated(room, new DrawingPoint(x, z), BlockFace.Ceiling, texture);
                    ApplyTextureAutomaticallyNoUpdated(room, new DrawingPoint(x, z), BlockFace.CeilingTriangle2, texture);
                }

            room.UpdateCompletely();
            _editor.RoomTextureChange(room);
        }

        public static void TexturizeAllWalls(Room room, TextureArea texture)
        {
            for (int x = 0; x < room.NumXSectors; x++)
                for (int z = 0; z < room.NumZSectors; z++)
                    for (BlockFace face = BlockFace.PositiveZ_QA; face <= BlockFace.DiagonalRF; face++)
                        if (room.IsFaceDefined(x, z, face))
                            ApplyTextureAutomaticallyNoUpdated(room, new DrawingPoint(x, z), face, texture);

            room.UpdateCompletely();
            _editor.RoomTextureChange(room);
        }

        public static void PlaceObject(Room room, DrawingPoint pos, PositionBasedObjectInstance instance)
        {
            Block block = room.GetBlock(pos);
            int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

            instance.Position = new Vector3(pos.X * 1024 + 512, y * 256, pos.Y * 1024 + 512);
            room.AddObject(_editor.Level, instance);
            if (instance is LightInstance)
                room.UpdateCompletely(); // Rebuild lighting!
            _editor.ObjectChange(instance);
        }

        public static void DeleteRoom(Room room, IWin32Window owner)
        {
            // Check if is the last room
            int roomCount = _editor.Level.Rooms.Count(r => r != null);
            if (roomCount <= 1)
            {
                DarkMessageBox.Show(owner, "You must have at least one room in your level", "Error", MessageBoxIcon.Error);
                return;
            }

            // Check if room has portals
            int portalCount = room.Portals.Count();
            if (portalCount != 0)
            {
                DarkMessageBox.Show(owner, "You can't delete a room with portals to other rooms.", "Error", MessageBoxIcon.Error);
                return;
            }

            // Ask for confirmation
            if (DarkMessageBox.Show(owner,
                    "Do you really want to delete this room? All objects inside room will be deleted and " +
                    "triggers pointing to them will be removed.",
                    "Delete room", MessageBoxButtons.YesNo, MessageBoxIcon.Error) != DialogResult.Yes)
            {
                return;
            }

            // Do it finally
            _editor.Level.DeleteRoom(room);

            // Update selection
            if (_editor.SelectedRoom == room)
                _editor.SelectRoomAndResetCamera(_editor.Level.Rooms.FirstOrDefault(r => r != null));
            _editor.RoomListChange();
        }

        public static void CropRoom(Room room, Rectangle newArea, IWin32Window owner)
        {
            newArea = newArea.Inflate(1);
            if ((newArea.Width + 1) > Room.MaxRoomDimensions || (newArea.Height + 1) > Room.MaxRoomDimensions)
            {
                DarkMessageBox.Show(owner, "The selected area exceeds the maximum room size.", "Error", MessageBoxIcon.Error);
                return;
            }
            if (DarkMessageBox.Show(owner, "Warning: if you crop this room, all portals and triggers outside the new area will be deleted." +
                " Do you want to continue?", "Crop room", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            // Resize room
            if (room.AlternateRoom != null)
                room.Resize(_editor.Level, newArea, (short)room.GetLowestCorner(), (short)room.GetHighestCorner());
            else if (room.AlternateBaseRoom != null)
                room.Resize(_editor.Level, newArea, (short)room.GetLowestCorner(), (short)room.GetHighestCorner());
            room.Resize(_editor.Level, newArea, (short)room.GetLowestCorner(), (short)room.GetHighestCorner());

            // Fix selection if necessary
            if ((_editor.SelectedRoom == room) && _editor.SelectedSectors.Valid)
            {
                var selection = _editor.SelectedSectors;
                selection.Area = selection.Area.Intersect(newArea).OffsetNeg(new DrawingPoint(newArea.X, newArea.Y));
                _editor.SelectedSectors = selection;
            }
            _editor.RoomPropertiesChange(room);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetDiagonalFloorSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall ||
                        room.Blocks[x, z].Portals.ToList().Exists(item => item.Direction == PortalDirection.Floor))
                        continue;

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
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XnZp;
                        if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XnZp;
                    }

                    if (theCorner == 1)
                    {
                        room.Blocks[x, z].QAFaces[0] = maxHeight;
                        room.Blocks[x, z].QAFaces[2] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XpZp;
                        if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XpZp;
                    }

                    if (theCorner == 2)
                    {
                        room.Blocks[x, z].QAFaces[1] = maxHeight;
                        room.Blocks[x, z].QAFaces[3] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XpZn;
                        if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XpZn;
                    }

                    if (theCorner == 3)
                    {
                        room.Blocks[x, z].QAFaces[0] = maxHeight;
                        room.Blocks[x, z].QAFaces[2] = maxHeight;
                        room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XnZn;
                        if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XnZn;
                    }

                    room.Blocks[x, z].EDFaces[0] = 0;
                    room.Blocks[x, z].EDFaces[1] = 0;
                    room.Blocks[x, z].EDFaces[2] = 0;
                    room.Blocks[x, z].EDFaces[3] = 0;
                }

            SmartBuildGeometry(room, area);
        }

        public static void SetDiagonalCeilingSplit(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall ||
                        room.Blocks[x, z].Portals.ToList().Exists(item => item.Direction == PortalDirection.Ceiling))
                        continue;

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
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XnZp;
                        if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XnZp;
                    }

                    if (theCorner == 1)
                    {
                        room.Blocks[x, z].WSFaces[0] = minHeight;
                        room.Blocks[x, z].WSFaces[2] = minHeight;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XpZp;
                        if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XpZp;
                    }

                    if (theCorner == 2)
                    {
                        room.Blocks[x, z].WSFaces[1] = minHeight;
                        room.Blocks[x, z].WSFaces[3] = minHeight;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XpZn;
                        if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XpZn;
                    }

                    if (theCorner == 3)
                    {
                        room.Blocks[x, z].WSFaces[0] = minHeight;
                        room.Blocks[x, z].WSFaces[2] = minHeight;
                        room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XnZn;
                        if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XnZn;
                    }

                    room.Blocks[x, z].RFFaces[0] = 0;
                    room.Blocks[x, z].RFFaces[1] = 0;
                    room.Blocks[x, z].RFFaces[2] = 0;
                    room.Blocks[x, z].RFFaces[3] = 0;
                }

            SmartBuildGeometry(room, area);
        }

        public static void SetDiagonalWall(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall ||
                        room.Blocks[x, z].Portals.Any())
                        continue;

                    if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                        RotateSector(room.Blocks[x, z], true);
                    else
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
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XnZp;
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XnZp;
                        }
                        else if (theCorner == 1)
                        {
                            room.Blocks[x, z].QAFaces[0] = maxHeight;
                            room.Blocks[x, z].QAFaces[2] = maxHeight;
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XpZp;
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XpZp;
                        }
                        else if (theCorner == 2)
                        {
                            room.Blocks[x, z].QAFaces[1] = maxHeight;
                            room.Blocks[x, z].QAFaces[3] = maxHeight;
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XpZn;
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XpZn;
                        }
                        else
                        {
                            room.Blocks[x, z].QAFaces[0] = maxHeight;
                            room.Blocks[x, z].QAFaces[2] = maxHeight;
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XnZn;
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XnZn;
                        }

                        room.Blocks[x, z].Type = BlockType.Wall;
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void RotateDiagonalSplit(Block block, bool floor = true)
        {
            if (block.Type == BlockType.BorderWall)
                return;

            if ((floor || block.Type == BlockType.Wall) &&
                    block.FloorDiagonalSplit != DiagonalSplit.None)
            {
                if (block.FloorDiagonalSplit == DiagonalSplit.XnZp)
                    block.FloorDiagonalSplit = DiagonalSplit.XpZp;
                else if (block.FloorDiagonalSplit == DiagonalSplit.XpZp)
                    block.FloorDiagonalSplit = DiagonalSplit.XpZn;
                else if (block.FloorDiagonalSplit == DiagonalSplit.XpZn)
                    block.FloorDiagonalSplit = DiagonalSplit.XnZn;
                else if (block.FloorDiagonalSplit == DiagonalSplit.XnZn)
                    block.FloorDiagonalSplit = DiagonalSplit.XnZp;
            }

            if((!floor || block.Type == BlockType.Wall) &&
                 block.CeilingDiagonalSplit != DiagonalSplit.None)
            {
                if (block.CeilingDiagonalSplit == DiagonalSplit.XnZp)
                    block.CeilingDiagonalSplit = DiagonalSplit.XpZp;
                else if (block.CeilingDiagonalSplit == DiagonalSplit.XpZp)
                    block.CeilingDiagonalSplit = DiagonalSplit.XpZn;
                else if (block.CeilingDiagonalSplit == DiagonalSplit.XpZn)
                    block.CeilingDiagonalSplit = DiagonalSplit.XnZn;
                else if (block.CeilingDiagonalSplit == DiagonalSplit.XnZn)
                    block.CeilingDiagonalSplit = DiagonalSplit.XnZp;
            }
        }

        public static void RotateSector(Block block, bool floor)
        {
            RotateDiagonalSplit(block, floor);

            short[] swapFace = new short[4];

            if (floor || block.Type == BlockType.Wall)
            {
                swapFace[0] = block.QAFaces[3];
                swapFace[1] = block.QAFaces[0];
                swapFace[2] = block.QAFaces[1];
                swapFace[3] = block.QAFaces[2];

                block.QAFaces[0] = swapFace[0];
                block.QAFaces[1] = swapFace[1];
                block.QAFaces[2] = swapFace[2];
                block.QAFaces[3] = swapFace[3];

                swapFace[0] = block.EDFaces[3];
                swapFace[1] = block.EDFaces[0];
                swapFace[2] = block.EDFaces[1];
                swapFace[3] = block.EDFaces[2];

                block.EDFaces[0] = swapFace[0];
                block.EDFaces[1] = swapFace[1];
                block.EDFaces[2] = swapFace[2];
                block.EDFaces[3] = swapFace[3];
            }
            else if (!floor || block.Type == BlockType.Wall)
            {
                swapFace[0] = block.WSFaces[3];
                swapFace[1] = block.WSFaces[0];
                swapFace[2] = block.WSFaces[1];
                swapFace[3] = block.WSFaces[2];

                block.WSFaces[0] = swapFace[0];
                block.WSFaces[1] = swapFace[1];
                block.WSFaces[2] = swapFace[2];
                block.WSFaces[3] = swapFace[3];

                swapFace[0] = block.RFFaces[3];
                swapFace[1] = block.RFFaces[0];
                swapFace[2] = block.RFFaces[1];
                swapFace[3] = block.RFFaces[2];

                block.RFFaces[0] = swapFace[0];
                block.RFFaces[1] = swapFace[1];
                block.RFFaces[2] = swapFace[2];
                block.RFFaces[3] = swapFace[3];
            }
        }

        public static void RotateSectors(Room room, Rectangle area, bool floor)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall ||
                        (floor && room.Blocks[x, z].Portals.ToList().Exists(item => item.Direction == PortalDirection.Floor)) ||
                        (!floor && room.Blocks[x, z].Portals.ToList().Exists(item => item.Direction == PortalDirection.Ceiling)))
                        continue;
                    RotateSector(room.Blocks[x, z], floor);
                }

            SmartBuildGeometry(room, area);
            _editor.RoomGeometryChange(room);
        }

        public static void SetWall(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall ||
                        room.Blocks[x, z].Portals.Any())
                        continue;
                    room.Blocks[x, z].Type = BlockType.Wall;
                    room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.None;
                }

            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetFloor(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall ||
                        room.Blocks[x, z].Portals.ToList().Exists(item => item.Direction == PortalDirection.Floor))
                        continue;

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
                    if (room.Blocks[x, z].Type == BlockType.BorderWall ||
                        room.Blocks[x, z].Portals.ToList().Exists(item => item.Direction == PortalDirection.Ceiling))
                        continue;

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

        public static void ToggleForceFloorSolid(Room room, Rectangle area)
        {
            for (int x = area.X; x <= area.Right; x++)
                for (int z = area.Y; z <= area.Bottom; z++)
                    room.Blocks[x, z].ForceFloorSolid = !room.Blocks[x, z].ForceFloorSolid;
            SmartBuildGeometry(room, area);
            _editor.RoomGeometryChange(room);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void AddPortal(Room room, Rectangle area, IWin32Window owner)
        {
            // Check if it's a flip room
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
                            if (!room.Blocks[x, z].IsAnyWall)
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
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallNegativeX, neighborRoom));
                    if ((area.Width == 0) && (area.X == (room.NumXSectors - 1)))
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallPositiveX, neighborRoom));
                    if ((area.Height == 0) && (area.Y == 0))
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallNegativeZ, neighborRoom));
                    if ((area.Height == 0) && (area.Y == (room.NumZSectors - 1)))
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallPositiveZ, neighborRoom));
                }
            }

            if (candidates.Count > 1)
            {
                using (var form = new FormChooseRoom("More than one possible room found that can be connected. " +
                    "Please choose one:", candidates.Select(candidate => candidate.Item2), (selectedRoom) => _editor.SelectedRoom = selectedRoom))
                {
                    if ((form.ShowDialog(owner) != DialogResult.OK) || (form.SelectedRoom == null))
                        return;
                    candidates.RemoveAll(candidate => candidate.Item2 != form.SelectedRoom);
                }
            }
            if (candidates.Count != 1)
                throw new ApplicationException("No room candidate found to connect to.");

            PortalDirection destinationDirection = candidates[0].Item1;
            Room destination = candidates[0].Item2;
            if (destination.Flipped)
                throw new NotSupportedException("Unfortunately we don't support adding portals to flipped rooms currently. :(");

            // Create portals
            PortalInstance portal = new PortalInstance(area, destinationDirection, destination);
            PortalInstance oppositePortal = room.AddBidirectionalPortalsToLevel(_editor.Level, portal);

            // Update
            room.UpdateCompletely();
            destination.UpdateCompletely();

            _editor.ObjectChange(portal);
            _editor.ObjectChange(oppositePortal);

            _editor.RoomSectorPropertiesChange(room);
            _editor.RoomSectorPropertiesChange(destination);
        }

        public static void AlternateRoomEnable(Room room, short AlternateGroup)
        {
            // Create new room
            var newRoom = room.Clone(_editor.Level, instance => instance.CopyToFlipRooms);
            newRoom.Name = "Flipped of " + room;
            newRoom.UpdateCompletely();

            // Assign room
            _editor.Level.AssignRoomToFree(newRoom);
            _editor.RoomListChange();

            // Update room alternate groups
            room.AlternateGroup = AlternateGroup;
            room.AlternateRoom = newRoom;
            _editor.RoomPropertiesChange(room);

            newRoom.AlternateGroup = AlternateGroup;
            newRoom.AlternateBaseRoom = room;
            _editor.RoomPropertiesChange(newRoom);
        }

        public static void AlternateRoomDisableWithWarning(Room room, IWin32Window owner)
        {
            // Check if room has portals
            if (room.Portals.Count() > 0)
            {
                DarkMessageBox.Show(owner, "You can't delete a room with portals to other rooms.", "Error", MessageBoxIcon.Error);
                return;
            }

            // Ask for confirmation
            if (DarkMessageBox.Show(owner, "Do you really want to delete the flip room?",
                "Delete flipped room", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
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

        public static void SplitRoom(IWin32Window owner)
        {
            if (!CheckForRoomAndBlockSelection(owner))
                return;

            Rectangle area = _editor.SelectedSectors.Area.Inflate(1);
            var room = _editor.SelectedRoom;

            if (room.AlternateBaseRoom != null)
            {
                _editor.Level.AssignRoomToFree(room.AlternateBaseRoom.Split(_editor.Level, area));
                _editor.RoomGeometryChange(room);
                _editor.RoomSectorPropertiesChange(room);
            }

            if (room.AlternateRoom != null)
            {
                _editor.Level.AssignRoomToFree(room.AlternateRoom.Split(_editor.Level, area));
                _editor.RoomGeometryChange(room);
                _editor.RoomSectorPropertiesChange(room);
            }

            Vector3 oldRoomPos = room.Position;
            _editor.Level.AssignRoomToFree(room.Split(_editor.Level, area));
            _editor.RoomGeometryChange(room);
            _editor.RoomSectorPropertiesChange(room);
            _editor.RoomListChange();

            // Fix selection
            if ((_editor.SelectedRoom == room) && _editor.SelectedSectors.Valid)
            {
                var selection = _editor.SelectedSectors;
                selection.Area = selection.Area.Offset(new DrawingPoint((int)(oldRoomPos.X - room.Position.X), (int)(oldRoomPos.Z - room.Position.Z)));
                _editor.SelectedSectors = selection;
            }
        }

        public static void CopyRoom(IWin32Window owner)
        {
            var newRoom = _editor.SelectedRoom.Clone(_editor.Level);
            newRoom.Name = _editor.SelectedRoom.Name + " (copy)";
            _editor.Level.AssignRoomToFree(newRoom);
            _editor.RoomListChange();
            _editor.SelectedRoom = newRoom;
        }

        public static bool CheckForRoomAndBlockSelection(IWin32Window owner)
        {
            if ((_editor.SelectedRoom == null) || !_editor.SelectedSectors.Valid)
            {
                DarkMessageBox.Show(owner, "Please select a valid group of sectors", "Error", MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        public static bool BuildLevel(bool autoCloseWhenDone)
        {
            Level level = _editor.Level;
            string fileName = level.Settings.MakeAbsolute(level.Settings.GameLevelFilePath);

            using (var form = new FormOperationDialog("Build *.tr4 level", autoCloseWhenDone, (progressReporter) =>
                new LevelCompilerTr4(level, fileName, progressReporter).CompileLevel()))
            {
                form.ShowDialog();
                return form.DialogResult != DialogResult.Cancel;
            }
        }

        public static void BuildLevelAndPlay()
        {
            if (!BuildLevel(true))
                return;

            TombLauncher.Launch(_editor.Level.Settings);
        }

        public static void LoadTextures(IWin32Window owner)
        {
            var settings = _editor.Level.Settings;
            string path = ResourceLoader.BrowseTextureFile(settings, settings.TextureFilePath, owner);
            if (settings.TextureFilePath == path)
                return;

            settings.TextureFilePath = path;
            _editor.LoadedTexturesChange();
        }

        public static void LoadWad(IWin32Window owner)
        {
            var settings = _editor.Level.Settings;
            string path = ResourceLoader.BrowseObjectFile(settings, settings.WadFilePath, owner);
            if (path == settings.WadFilePath)
                return;

            settings.WadFilePath = path;
            _editor.Level.ReloadWad();
            _editor.LoadedWadsChange(_editor.Level.Wad);
        }

        public static void UnloadWad()
        {
            _editor.Level.Settings.WadFilePath = null;
            _editor.Level.ReloadWad();
            _editor.LoadedWadsChange(null);
        }

        public static void ReloadWad()
        {
            _editor.Level.ReloadWad();
            _editor.LoadedWadsChange(null);
        }

        public static void Copy(IWin32Window owner)
        {
            var instance = _editor.SelectedObject as PositionBasedObjectInstance;
            if (instance == null)
            {
                DarkMessageBox.Show(owner, "You have to select an object before you can copy it.", "No object selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Clipboard.Copy(instance);
        }

        public static void Clone(IWin32Window owner)
        {
            Copy(owner);
            _editor.Action = new EditorAction { Action = EditorActionType.Stamp };
        }

        public static bool DragDropFileSupported(DragEventArgs e, bool allow3DImport = false)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                int fileCount = files.Count();

                foreach (var file in files)
                    if (SupportedFormats.IsExtensionPresent(FileFormatType.Object, file) ||
                        SupportedFormats.IsExtensionPresent(FileFormatType.Texture, file) ||
                        (allow3DImport && SupportedFormats.IsExtensionPresent(FileFormatType.Geometry, file)) ||
                        file.EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase) ||
                        file.EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase))
                        return true;
            }

            return false;
        }

        public static int DragDropCommonFiles(DragEventArgs e, IWin32Window owner)
        {
            int unsupportedFileCount = 0;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (var file in files)
                {
                    if (SupportedFormats.IsExtensionPresent(FileFormatType.Object, file))
                    {
                        _editor.Level.Settings.WadFilePath = _editor.Level.Settings.MakeRelative(file, VariableType.LevelDirectory);
                        _editor.Level.ReloadWad();
                        _editor.LoadedWadsChange(_editor.Level.Wad);
                    }
                    else if (SupportedFormats.IsExtensionPresent(FileFormatType.Texture, file))
                    {
                        _editor.Level.Settings.TextureFilePath = _editor.Level.Settings.MakeRelative(file, VariableType.LevelDirectory);
                        _editor.LoadedTexturesChange();
                    }
                    else if (file.EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase))
                    {
                        OpenLevelPrj(owner, file);
                    }
                    else if (file.EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase))
                    {
                        OpenLevel(owner, file);
                    }
                    else
                        unsupportedFileCount++;
                }
                return unsupportedFileCount;
            }
            else
                return -1;
        }

        public static void ShowTextureSoundsDialog(IWin32Window owner)
        {
            using (var form = new FormTextureSounds(_editor, _editor.Level.Settings))
                form.ShowDialog(owner);
        }

        public static void ShowAnimationRangesDialog(IWin32Window owner)
        {
            using (FormAnimatedTextures form = new FormAnimatedTextures())
                form.ShowDialog(owner);
        }

        public static void SetPortalOpacity(PortalOpacity opacity, IWin32Window owner)
        {
            var portal = _editor.SelectedObject as PortalInstance;
            if ((_editor.SelectedRoom == null) || (portal == null))
            {
                DarkMessageBox.Show(owner, "No portal selected.", "Error", MessageBoxIcon.Error);
                return;
            }

            portal.Opacity = opacity;
            _editor.SelectedRoom.UpdateCompletely();
            _editor.ObjectChange(portal);
        }

        public static void SaveLevel(IWin32Window owner, bool askForPath)
        {
            // Show save dialog if necessary
            if (askForPath || string.IsNullOrEmpty(_editor.Level.Settings.LevelFilePath))
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Tomb Editor Project (*.prj2)|*.prj2";
                    if (!string.IsNullOrEmpty(_editor.Level.Settings.LevelFilePath))
                    {
                        saveFileDialog.InitialDirectory = Path.GetDirectoryName(_editor.Level.Settings.LevelFilePath);
                        saveFileDialog.FileName = Path.GetFileName(_editor.Level.Settings.LevelFilePath);
                    }
                    if (saveFileDialog.ShowDialog(owner) != DialogResult.OK)
                        return;

                    _editor.Level.Settings.LevelFilePath = saveFileDialog.FileName;
                    _editor.LevelFileNameChange();
                }

            // Save level
            try
            {
                Prj2Writer.SaveToPrj2(_editor.Level.Settings.LevelFilePath, _editor.Level);
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Unable to save to \"" + _editor.Level.Settings.LevelFilePath + "\".");
                DarkMessageBox.Show(owner, "There was an error while saving project file. Exception: " + exc, "Error", MessageBoxIcon.Error);
            }
        }

        public static void OpenLevel(IWin32Window owner, string fileName = null)
        {
            if (DarkMessageBox.Show(owner,
                "Your level will be lost. Do you really want to open another level file?",
                "Open level", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            string _fileName = fileName;

            if(_fileName == null)
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "Open Tomb Editor level";
                    openFileDialog.Filter = "Tomb Editor level (*.prj2)|*.prj2|All files (*.*)|*.*";
                    if (openFileDialog.ShowDialog(owner) != DialogResult.OK)
                        return;

                    _fileName = openFileDialog.FileName;
                }

            Level newLevel = null;
            try
            {
                newLevel = Prj2Loader.LoadFromPrj2(_fileName, new ProgressReporterSimple(owner));
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Unable to open \"" + _fileName + "\"");
                DarkMessageBox.Show(owner, "There was an error while opening project file. File may be in use or may be corrupted. Exception: " + exc.Message, "Error", MessageBoxIcon.Error);
            }
            _editor.Level = newLevel;
        }

        public static void OpenLevelPrj(IWin32Window owner, string fileName = null)
        {
            if (DarkMessageBox.Show(owner,
                    "Your level will be lost. Do you really want to open another level file?",
                    "Open level", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            string _fileName = fileName;

            if (_fileName == null)
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = "Open Tomb Editor level";
                    openFileDialog.Filter = "Winroomedit level (*.prj)|*.prj|All files (*.*)|*.*";
                    if (openFileDialog.ShowDialog(owner) != DialogResult.OK)
                        return;

                    _fileName = openFileDialog.FileName;
                }

            Level newLevel = null;
            try
            {
                using (var form = new FormOperationDialog("Import PRJ", false, (progressReporter) =>
                    newLevel = PrjLoader.LoadFromPrj(_fileName, progressReporter)))
                {
                    if (form.ShowDialog(owner) != DialogResult.OK || newLevel == null)
                        return;
                    _editor.Level = newLevel;
                    newLevel = null;
                }
            }
            finally
            {
                newLevel?.Dispose();
            }
        }

        public static void MoveRooms(Vector3 positionDelta, IEnumerable<Room> rooms)
        {
            HashSet<Room> roomsToMove = new HashSet<Room>(rooms);

            // Update position of all rooms
            foreach (Room room in roomsToMove)
                room.Position += positionDelta;

            // Make list of potential rooms to update
            HashSet<Room> roomsToUpdate = new HashSet<Room>();
            foreach (Room room in roomsToMove)
            {
                bool anyRoomUpdated = false;
                foreach (PortalInstance portal in room.Portals)
                    if (!roomsToMove.Contains(portal.AdjoiningRoom))
                    {
                        roomsToUpdate.Add(portal.AdjoiningRoom);
                        anyRoomUpdated = true;
                    }

                if (anyRoomUpdated)
                    roomsToUpdate.Add(room);
            }

            // Update
            foreach (Room room in roomsToUpdate)
            {
                room.UpdateCompletely();
                _editor.RoomSectorPropertiesChange(room);
            }

            foreach (Room room in roomsToMove)
                _editor.RoomGeometryChange(room);
        }
    }
}
