using DarkUI.Forms;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib;
using TombLib.Forms;
using TombLib.GeometryIO;
using TombLib.GeometryIO.Exporters;
using TombLib.LevelData;
using TombLib.LevelData.Compilers;
using TombLib.LevelData.IO;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor
{
    public static class EditorActions
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static Editor _editor = Editor.Instance;

        public static bool ContinueOnFileDrop(IWin32Window owner, string description)
        {
            if (!_editor.HasUnsavedChanges)
                return true;

            switch (DarkMessageBox.Show(owner,
                "Your unsaved changes will be lost. Do you want to save?",
                description,
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2))
            {
                case DialogResult.No:
                    return true;
                case DialogResult.Yes:
                    return SaveLevel(owner, false);
                default:
                    return false;
            }
        }

        public static void SmartBuildGeometry(Room room, RectangleInt2 area)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            room.SmartBuildGeometry(area);
            watch.Stop();
            logger.Debug("Edit geometry time: " + watch.ElapsedMilliseconds + "  ms");
            _editor.RoomGeometryChange(room);
        }

        private enum SmoothGeometryEditingType
        {
            None,
            Floor,
            Wall,
            Any
        }

        public static void EditSectorGeometry(Room room, RectangleInt2 area, EditorArrowType arrow, int verticalSubdivision, short increment, bool smooth, bool oppositeDiagonalCorner = false, bool autoSwitchDiagonals = false, bool autoUpdateThroughPortal = true)
        {
            if (smooth)
            {
                // Scan selection and decide if the selected zone is wall-only, floor-only, or both.
                // It's needed to force smoothing function to edit either only wall sections or floor sections,
                // in case user wants to smoothly edit only wall splits or actual floor height.

                SmoothGeometryEditingType smoothEditingType = SmoothGeometryEditingType.None;

                for (int x = area.X0; x <= area.X1; x++)
                {
                    for (int z = area.Y0; z <= area.Y1; z++)
                    {
                        if (smoothEditingType != SmoothGeometryEditingType.Wall && room.Blocks[x, z].Type == BlockType.Floor)
                            smoothEditingType = SmoothGeometryEditingType.Floor;
                        else if (smoothEditingType != SmoothGeometryEditingType.Floor && room.Blocks[x, z].Type != BlockType.Floor)
                            smoothEditingType = SmoothGeometryEditingType.Wall;
                        else
                        {
                            smoothEditingType = SmoothGeometryEditingType.Any;
                            break;
                        }
                    }
                    if (smoothEditingType == SmoothGeometryEditingType.Any)
                        break;
                }

                // Adjust editing area to exclude the side on which the arrow starts
                // This is a superset of the behaviour of the old editor to smooth edit a single edge or side.
                switch (arrow)
                {
                    case EditorArrowType.EdgeE:
                        area = new RectangleInt2(area.X0 + 1, area.Y0, area.X1, area.Y1);
                        break;
                    case EditorArrowType.EdgeN:
                        area = new RectangleInt2(area.X0, area.Y0 + 1, area.X1, area.Y1);
                        break;
                    case EditorArrowType.EdgeW:
                        area = new RectangleInt2(area.X0, area.Y0, area.X1 - 1, area.Y1);
                        break;
                    case EditorArrowType.EdgeS:
                        area = new RectangleInt2(area.X0, area.Y0, area.X1, area.Y1 - 1);
                        break;
                    case EditorArrowType.CornerNE:
                        area = new RectangleInt2(area.X0 + 1, area.Y0 + 1, area.X1, area.Y1);
                        break;
                    case EditorArrowType.CornerNW:
                        area = new RectangleInt2(area.X0, area.Y0 + 1, area.X1 - 1, area.Y1);
                        break;
                    case EditorArrowType.CornerSW:
                        area = new RectangleInt2(area.X0, area.Y0, area.X1 - 1, area.Y1 - 1);
                        break;
                    case EditorArrowType.CornerSE:
                        area = new RectangleInt2(area.X0 + 1, area.Y0, area.X1, area.Y1 - 1);
                        break;
                }
                arrow = EditorArrowType.EntireFace;

                Action<Block, int> smoothEdit = (Block block, int edge) =>
                {
                    if (block == null)
                        return;

                    if (((verticalSubdivision == 0 || verticalSubdivision == 2) && block.FloorDiagonalSplit == DiagonalSplit.None) ||
                       ((verticalSubdivision == 1 || verticalSubdivision == 3) && block.CeilingDiagonalSplit == DiagonalSplit.None))
                    {
                        if (smoothEditingType == SmoothGeometryEditingType.Any ||
                           (!block.IsAnyWall && smoothEditingType == SmoothGeometryEditingType.Floor) ||
                           (!block.IsAnyWall && smoothEditingType == SmoothGeometryEditingType.Wall))
                        {
                            block.ChangeEdge(verticalSubdivision, edge, increment);
                            block.FixHeights(verticalSubdivision);
                        }
                    }
                };


                // Smoothly change sectors on the corners
                smoothEdit(room.GetBlockTry(area.X0 - 1, area.Y1 + 1), Block.FaceXpZn);
                smoothEdit(room.GetBlockTry(area.X1 + 1, area.Y1 + 1), Block.FaceXnZn);
                smoothEdit(room.GetBlockTry(area.X1 + 1, area.Y0 - 1), Block.FaceXnZp);
                smoothEdit(room.GetBlockTry(area.X0 - 1, area.Y0 - 1), Block.FaceXpZp);

                // Smoothly change sectors on the sides
                for (int x = area.X0; x <= area.X1; x++)
                {
                    smoothEdit(room.GetBlockTry(x, area.Y0 - 1), Block.FaceXnZp);
                    smoothEdit(room.GetBlockTry(x, area.Y0 - 1), Block.FaceXpZp);

                    smoothEdit(room.GetBlockTry(x, area.Y1 + 1), Block.FaceXnZn);
                    smoothEdit(room.GetBlockTry(x, area.Y1 + 1), Block.FaceXpZn);
                }

                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    smoothEdit(room.GetBlockTry(area.X0 - 1, z), Block.FaceXpZp);
                    smoothEdit(room.GetBlockTry(area.X0 - 1, z), Block.FaceXpZn);

                    smoothEdit(room.GetBlockTry(area.X1 + 1, z), Block.FaceXnZp);
                    smoothEdit(room.GetBlockTry(area.X1 + 1, z), Block.FaceXnZn);
                }
            }

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Block block = room.Blocks[x, z];
                    Room.RoomBlockPair lookupBlock = room.GetBlockTryThroughPortal(x, z);

                    EditBlock:
                    {
                        if (arrow == EditorArrowType.EntireFace)
                        {
                            if (verticalSubdivision < 2)
                                block.RaiseStepWise(verticalSubdivision, oppositeDiagonalCorner, increment, autoSwitchDiagonals);
                            else
                                block.Raise(verticalSubdivision, false, increment);
                        }
                        else
                        {
                            var floor = (verticalSubdivision % 2 == 0);
                            var currentFaces = block.GetVerticalSubdivision(verticalSubdivision);
                            var currentSplit = floor ? block.FloorDiagonalSplit : block.CeilingDiagonalSplit;
                            var incrementInvalid = floor ? (increment < 0) : (increment > 0);
                            int[] corners = new int[2] { 0, 0 };
                            DiagonalSplit[] splits = new DiagonalSplit[2] { DiagonalSplit.None, DiagonalSplit.None };

                            switch (arrow)
                            {
                                case EditorArrowType.EdgeN:
                                case EditorArrowType.CornerNW:
                                    corners[0] = 0;
                                    corners[1] = 1;
                                    splits[0] = DiagonalSplit.XpZn;
                                    splits[1] = (arrow == EditorArrowType.CornerNW) ? DiagonalSplit.XnZp : DiagonalSplit.XnZn;
                                    break;
                                case EditorArrowType.EdgeE:
                                case EditorArrowType.CornerNE:
                                    corners[0] = 1;
                                    corners[1] = 2;
                                    splits[0] = DiagonalSplit.XnZn;
                                    splits[1] = (arrow == EditorArrowType.CornerNE) ? DiagonalSplit.XpZp : DiagonalSplit.XnZp;
                                    break;
                                case EditorArrowType.EdgeS:
                                case EditorArrowType.CornerSE:
                                    corners[0] = 2;
                                    corners[1] = 3;
                                    splits[0] = DiagonalSplit.XnZp;
                                    splits[1] = (arrow == EditorArrowType.CornerSE) ? DiagonalSplit.XpZn : DiagonalSplit.XpZp;
                                    break;
                                case EditorArrowType.EdgeW:
                                case EditorArrowType.CornerSW:
                                    corners[0] = 3;
                                    corners[1] = 0;
                                    splits[0] = DiagonalSplit.XpZp;
                                    splits[1] = (arrow == EditorArrowType.CornerSW) ? DiagonalSplit.XnZn : DiagonalSplit.XpZn;
                                    break;
                            }

                            if (arrow <= EditorArrowType.EdgeW)
                            {
                                if (block.Type != BlockType.Wall && currentSplit != DiagonalSplit.None)
                                    continue;
                                for (int i = 0; i < 2; i++)
                                    if (currentSplit != splits[i])
                                        currentFaces[corners[i]] += increment;
                            }
                            else
                            {
                                if (block.Type != BlockType.Wall && currentSplit != DiagonalSplit.None)
                                {
                                    if (currentSplit == splits[1])
                                    {
                                        if (currentFaces[corners[0]] == currentFaces[corners[1]] && incrementInvalid)
                                            continue;
                                    }
                                    else if (autoSwitchDiagonals && currentSplit == splits[0] && currentFaces[corners[0]] == currentFaces[corners[1]] && !incrementInvalid)
                                        block.Transform(new RectTransformation { QuadrantRotation = 2 }, floor);
                                    else
                                        continue;
                                }
                                currentFaces[corners[0]] += increment;
                            }
                        }
                        block.FixHeights(verticalSubdivision);
                    }

                    if (autoUpdateThroughPortal && lookupBlock.Block != block)
                    {
                        block = lookupBlock.Block;
                        goto EditBlock;
                    }

                    // FIXME: VERY SLOW CODE! Since we need to update geometry in adjoining block through portal, and each block may contain portal to different room,
                    // we need to find a way to quickly update geometry in all possible adjoining rooms in area. Until then, this function is used on per-sector basis.

                    if (lookupBlock.Room != room)
                        SmartBuildGeometry(lookupBlock.Room, new RectangleInt2(lookupBlock.Pos, lookupBlock.Pos));
                }

            SmartBuildGeometry(room, area);
        }

        public static void ResetObjectRotation(RotationAxis axis = RotationAxis.None)
        {
            if (_editor.SelectedObject is IRotateableYX)
            {
                if (axis == RotationAxis.X || axis == RotationAxis.None) (_editor.SelectedObject as IRotateableYX).RotationX = 0;
            }

            if (_editor.SelectedObject is IRotateableY)
            {
                if (axis == RotationAxis.Y || axis == RotationAxis.None) (_editor.SelectedObject as IRotateableY).RotationY = 0;
            }

            if (_editor.SelectedObject is IRotateableYXRoll)
            {
                if (axis == RotationAxis.Roll || axis == RotationAxis.None) (_editor.SelectedObject as IRotateableYXRoll).Roll = 0;
            }

            _editor.ObjectChange(_editor.SelectedObject, ObjectChangeType.Change);
        }

        public static void SmoothSector(Room room, int x, int z, int v)
        {
            var floor = (v % 2 == 0);
            var currBlock = room.GetBlockTryThroughPortal(x, z);

            if (currBlock.Room != room || (floor && currBlock.Block.FloorDiagonalSplit != DiagonalSplit.None) || (!floor && currBlock.Block.CeilingDiagonalSplit != DiagonalSplit.None))
                return;

            Room.RoomBlockPair[] lookupBlocks = new Room.RoomBlockPair[8]
            {
                room.GetBlockTryThroughPortal(x - 1, z + 1),
                room.GetBlockTryThroughPortal(x, z + 1),
                room.GetBlockTryThroughPortal(x + 1, z + 1),
                room.GetBlockTryThroughPortal(x + 1, z),
                room.GetBlockTryThroughPortal(x + 1, z - 1),
                room.GetBlockTryThroughPortal(x, z - 1),
                room.GetBlockTryThroughPortal(x - 1, z - 1),
                room.GetBlockTryThroughPortal(x - 1, z)
            };

            int[] adj = new int[8];
            for (int i = 0; i < 8; i++)
                adj[i] = currBlock.Room.Position.Y - lookupBlocks[i].Room.Position.Y;

            short[] newFaces = new short[4];

            int validBlockCnt = ((lookupBlocks[7].Room != null ? 1 : 0) + (lookupBlocks[0].Room != null ? 1 : 0) + (lookupBlocks[1].Room != null ? 1 : 0));
            newFaces[0] = (short)(((lookupBlocks[7].Block?.GetVerticalSubdivision(v)[1] ?? 0) + adj[7] +
                                   (lookupBlocks[0].Block?.GetVerticalSubdivision(v)[2] ?? 0) + adj[0] +
                                   (lookupBlocks[1].Block?.GetVerticalSubdivision(v)[3] ?? 0) + adj[1]) / (validBlockCnt));

            validBlockCnt = ((lookupBlocks[1].Room != null ? 1 : 0) + (lookupBlocks[2].Room != null ? 1 : 0) + (lookupBlocks[3].Room != null ? 1 : 0));
            newFaces[1] = (short)(((lookupBlocks[1].Block?.GetVerticalSubdivision(v)[2] ?? 0) + adj[2] +
                                   (lookupBlocks[2].Block?.GetVerticalSubdivision(v)[3] ?? 0) + adj[3] +
                                   (lookupBlocks[3].Block?.GetVerticalSubdivision(v)[0] ?? 0) + adj[0]) / (validBlockCnt));

            validBlockCnt = ((lookupBlocks[3].Room != null ? 1 : 0) + (lookupBlocks[4].Room != null ? 1 : 0) + (lookupBlocks[5].Room != null ? 1 : 0));
            newFaces[2] = (short)(((lookupBlocks[3].Block?.GetVerticalSubdivision(v)[3] ?? 0) + adj[3] +
                                   (lookupBlocks[4].Block?.GetVerticalSubdivision(v)[0] ?? 0) + adj[0] +
                                   (lookupBlocks[5].Block?.GetVerticalSubdivision(v)[1] ?? 0) + adj[1]) / (validBlockCnt));

            validBlockCnt = ((lookupBlocks[5].Room != null ? 1 : 0) + (lookupBlocks[6].Room != null ? 1 : 0) + (lookupBlocks[7].Room != null ? 1 : 0));
            newFaces[3] = (short)(((lookupBlocks[5].Block?.GetVerticalSubdivision(v)[0] ?? 0) + adj[0] +
                                   (lookupBlocks[6].Block?.GetVerticalSubdivision(v)[1] ?? 0) + adj[1] +
                                   (lookupBlocks[7].Block?.GetVerticalSubdivision(v)[2] ?? 0) + adj[2]) / (validBlockCnt));

            currBlock.Block.GetVerticalSubdivision(v)[0] += (short)Math.Sign(newFaces[0] - currBlock.Block.GetVerticalSubdivision(v)[0]);
            currBlock.Block.GetVerticalSubdivision(v)[1] += (short)Math.Sign(newFaces[1] - currBlock.Block.GetVerticalSubdivision(v)[1]);
            currBlock.Block.GetVerticalSubdivision(v)[2] += (short)Math.Sign(newFaces[2] - currBlock.Block.GetVerticalSubdivision(v)[2]);
            currBlock.Block.GetVerticalSubdivision(v)[3] += (short)Math.Sign(newFaces[3] - currBlock.Block.GetVerticalSubdivision(v)[3]);

            SmartBuildGeometry(room, new RectangleInt2(x, z, x, z));
        }

        public static void ShapeGroup(Room room, RectangleInt2 area, EditorArrowType arrow, EditorToolType type, int verticalSubdivision, double heightScale, bool precise, bool stepped)
        {
            if (precise)
                heightScale /= 4;

            bool linearShape = (type <= EditorToolType.HalfPipe);
            bool uniformShape = (type >= EditorToolType.HalfPipe);
            bool step90 = (arrow <= EditorArrowType.EdgeW);
            bool turn90 = (arrow == EditorArrowType.EdgeW || arrow == EditorArrowType.EdgeE);
            bool reverseX = (arrow == EditorArrowType.EdgeW || arrow == EditorArrowType.CornerSW || arrow == EditorArrowType.CornerNW) ^ uniformShape;
            bool reverseZ = (arrow == EditorArrowType.EdgeS || arrow == EditorArrowType.CornerSW || arrow == EditorArrowType.CornerSE) ^ uniformShape;
            bool uniformAlign = (arrow != EditorArrowType.EntireFace && type > EditorToolType.HalfPipe && step90);

            double sizeX = area.Width + (stepped ? 0 : 1);
            double sizeZ = area.Height + (stepped ? 0 : 1);
            double grainBias = (uniformShape ? (!step90 ? 0 : 1) : 0);
            double grainX = (1 + grainBias) / sizeX / (uniformAlign && turn90 ? 2 : 1);
            double grainZ = (1 + grainBias) / sizeZ / (uniformAlign && !turn90 ? 2 : 1);

            for (int w = area.X0, x = 0; w < area.X0 + sizeX + 1; w++, x++)
                for (int h = area.Y0, z = 0; h != area.Y0 + sizeZ + 1; h++, z++)
                {
                    double currentHeight;
                    double currX = (linearShape && !turn90 && step90) ? 0 : grainX * (reverseX ? sizeX - x : x) - (uniformAlign && turn90 ? 0 : grainBias);
                    double currZ = (linearShape && turn90 && step90) ? 0 : grainZ * (reverseZ ? sizeZ - z : z) - (uniformAlign && !turn90 ? 0 : grainBias);

                    switch (type)
                    {
                        case EditorToolType.Ramp:
                            currentHeight = currX + currZ;
                            break;
                        case EditorToolType.Pyramid:
                            currentHeight = 1 - Math.Max(Math.Abs(currX), Math.Abs(currZ));
                            break;
                        default:
                            currentHeight = Math.Sqrt(1 - Math.Pow(currX, 2) - Math.Pow(currZ, 2));
                            currentHeight = double.IsNaN(currentHeight) ? 0 : currentHeight;
                            if (type == EditorToolType.QuarterPipe)
                                currentHeight = 1 - currentHeight;
                            break;
                    }
                    currentHeight = Math.Round(currentHeight * heightScale);

                    if (stepped)
                    {
                        room.Blocks[w, h].Raise(verticalSubdivision, false, (short)currentHeight);
                        room.Blocks[w, h].FixHeights();
                    }
                    else
                        room.ModifyPoint(w, h, verticalSubdivision, (short)currentHeight, area);
                }
            SmartBuildGeometry(room, area);
        }

        public static void ApplyHeightmap(Room room, RectangleInt2 area, EditorArrowType arrow, int verticalSubdivision, float[,] heightmap, float heightScale, bool precise, bool raw)
        {
            if (precise)
                heightScale /= 4;

            bool allFace = (arrow == EditorArrowType.EntireFace);
            bool step90 = (arrow <= EditorArrowType.EdgeW);
            bool turn90 = (arrow == EditorArrowType.EdgeW || arrow == EditorArrowType.EdgeE);
            bool reverseX = (arrow == EditorArrowType.EdgeW || arrow == EditorArrowType.CornerSW || arrow == EditorArrowType.CornerNW);
            bool reverseZ = (arrow == EditorArrowType.EdgeS || arrow == EditorArrowType.CornerSW || arrow == EditorArrowType.CornerSE);

            float smoothGrainX = (float)(allFace || (step90 && !turn90) ? Math.PI : (Math.PI * 0.5f)) / (area.Width + 1);
            float smoothGrainZ = (float)(allFace || (step90 && turn90) ? Math.PI : (Math.PI * 0.5f)) / (area.Height + 1);

            for (int w = area.X0, x = 0; w < area.X1 + 2; w++, x++)
                for (int h = area.Y0, z = 0; h != area.Y1 + 2; h++, z++)
                {
                    var smoothFactor = raw ? 1 : (Math.Sin(smoothGrainX * (reverseX ? area.Width + 1 - x : x)) * Math.Sin(smoothGrainZ * (reverseZ ? area.Height + 1 - z : z)));

                    int currX = x * (heightmap.GetLength(0) / (area.Width + 2));
                    int currZ = z * (heightmap.GetLength(1) / (area.Height + 2));
                    room.ModifyPoint(w, h, verticalSubdivision, (short)(Math.Round(heightmap[currX, currZ] * smoothFactor * heightScale)), area);
                }
            SmartBuildGeometry(room, area);
        }

        public static void FlipFloorSplit(Room room, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    if (!room.Blocks[x, z].FloorIsQuad && room.Blocks[x, z].FloorDiagonalSplit == DiagonalSplit.None)
                        room.Blocks[x, z].FloorSplitDirectionToggled = !room.Blocks[x, z].FloorSplitDirectionToggled;

            SmartBuildGeometry(room, area);
        }

        public static void FlipCeilingSplit(Room room, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    if (!room.Blocks[x, z].CeilingIsQuad && room.Blocks[x, z].CeilingDiagonalSplit == DiagonalSplit.None)
                        room.Blocks[x, z].CeilingSplitDirectionToggled = !room.Blocks[x, z].CeilingSplitDirectionToggled;

            SmartBuildGeometry(room, area);
        }

        public static void AddTrigger(Room room, RectangleInt2 area, IWin32Window owner)
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
            using (var formTrigger = new FormTrigger(_editor.Level, trigger, obj => _editor.ShowObject(obj),
                                                     r => _editor.SelectRoomAndResetCamera(r)))
            {
                if (formTrigger.ShowDialog(owner) != DialogResult.OK)
                    return;
            }
            room.AddObject(_editor.Level, trigger);
            _editor.ObjectChange(trigger, ObjectChangeType.Add);
            _editor.RoomSectorPropertiesChange(room);

            if (_editor.Configuration.Editor_AutoSwitchHighlight)
                _editor.HighlightManager.SetPriority(HighlightType.Trigger);
        }

        public static Vector3 GetMovementPrecision(Keys modifierKeys)
        {
            if (modifierKeys.HasFlag(Keys.Control))
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

            _editor.ObjectChange(_editor.SelectedObject, ObjectChangeType.Change);
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
            if (precision.X > 0.0f)
                pos.X = ((float)Math.Round(pos.X / precision.X)) * precision.X;
            if (precision.Y > 0.0f)
                pos.Y = ((float)Math.Round(pos.Y / precision.Y)) * precision.Y;
            if (precision.Z > 0.0f)
                pos.Z = ((float)Math.Round(pos.Z / precision.Z)) * precision.Z;

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
            _editor.ObjectChange(instance, ObjectChangeType.Change);
        }

        public static void MoveObjectRelative(PositionBasedObjectInstance instance, Vector3 pos, Vector3 precision = new Vector3(), bool canGoOutsideRoom = false)
        {
            MoveObject(instance, instance.Position + pos, precision, canGoOutsideRoom);
        }

        public enum RotationAxis
        {
            Y,
            X,
            Roll,
            None
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
            _editor.ObjectChange(instance, ObjectChangeType.Change);
        }

        public static void EditObject(ObjectInstance instance, IWin32Window owner)
        {
            if (instance is MoveableInstance)
            {
                using (var formMoveable = new FormMoveable((MoveableInstance)instance))
                    formMoveable.ShowDialog(owner);
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is StaticInstance)
            {
                using (var formStaticMesh = new FormStaticMesh((StaticInstance)instance))
                    formStaticMesh.ShowDialog(owner);
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is FlybyCameraInstance)
            {
                using (var formFlyby = new FormFlybyCamera((FlybyCameraInstance)instance))
                    formFlyby.ShowDialog(owner);
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is CameraInstance)
            {
                using (var formCamera = new FormCamera((CameraInstance)instance))
                    formCamera.ShowDialog(owner);
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is SinkInstance)
            {
                using (var formSink = new FormSink((SinkInstance)instance))
                    formSink.ShowDialog(owner);
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is SoundSourceInstance)
            {
                using (var formSoundSource = new FormSound((SoundSourceInstance)instance, _editor.Level.Wad))
                    formSoundSource.ShowDialog(owner);
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is TriggerInstance)
            {
                using (var formTrigger = new FormTrigger(_editor.Level, (TriggerInstance)instance, obj => _editor.ShowObject(obj),
                                                         r => _editor.SelectRoomAndResetCamera(r)))
                    formTrigger.ShowDialog(owner);
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is ImportedGeometryInstance)
            {
                using (var formImportedGeometry = new FormImportedGeometry((ImportedGeometryInstance)instance, _editor.Level.Settings))
                    if (formImportedGeometry.ShowDialog(owner) != DialogResult.Cancel)
                        _editor.UpdateLevelSettings(formImportedGeometry.NewLevelSettings);
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
        }

        public static void PasteObject(VectorInt2 pos)
        {
            ObjectClipboardData data = Clipboard.GetDataObject().GetData(typeof(ObjectClipboardData)) as ObjectClipboardData;
            if (data == null)
                MessageBox.Show("Clipboard contains no object data.");
            else
                PlaceObject(_editor.SelectedRoom, pos, data.MergeGetSingleObject(_editor));
        }

        public static void DeleteObjectWithWarning(ObjectInstance instance, IWin32Window owner)
        {
            if (DarkMessageBox.Show(owner, "Do you really want to delete " + instance.ToString() + "?",
                    "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            DeleteObject(instance);
        }

        public static void DeleteObject(ObjectInstance instance)
        {
            Room room = instance.Room;
            Room adjoiningRoom = (instance as PortalInstance)?.AdjoiningRoom;
            room.RemoveObject(_editor.Level, instance);

            // Additional updates
            if (instance is SectorBasedObjectInstance)
                _editor.RoomSectorPropertiesChange(room);
            if (instance is LightInstance)
                room.UpdateCompletely();
            if (instance is PortalInstance)
            {
                room?.UpdateCompletely();
                adjoiningRoom?.UpdateCompletely();
                room?.AlternateVersion?.UpdateCompletely();
                adjoiningRoom?.AlternateVersion?.UpdateCompletely();
            }

            // Avoid having the removed object still selected
            _editor.ObjectChange(instance, ObjectChangeType.Remove, room);
        }

        public static void RotateTexture(Room room, VectorInt2 pos, BlockFace face)
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

        public static void MirrorTexture(Room room, VectorInt2 pos, BlockFace face)
        {
            Block blocks = room.GetBlock(pos);
            TextureArea textureArea = blocks.GetFaceTexture(face);
            if (room.GetFaceVertexRange(pos.X, pos.Y, face).Count == 3)
            {
                Swap.Do(ref textureArea.TexCoord0, ref textureArea.TexCoord2);
                textureArea.TexCoord3 = textureArea.TexCoord2;
            }
            else
            {
                Swap.Do(ref textureArea.TexCoord0, ref textureArea.TexCoord1);
                Swap.Do(ref textureArea.TexCoord2, ref textureArea.TexCoord3);
            }
            blocks.SetFaceTexture(face, textureArea);

            // Update state
            room.BuildGeometry();
            room.CalculateLightingForThisRoom();
            room.UpdateBuffers();
            _editor.RoomTextureChange(room);
        }

        public static void PickTexture(Room room, VectorInt2 pos, BlockFace face)
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

        public static void MirrorSelectedTexture()
        {
            TextureArea textureArea = _editor.SelectedTexture;
            Swap.Do(ref textureArea.TexCoord0, ref textureArea.TexCoord3);
            Swap.Do(ref textureArea.TexCoord1, ref textureArea.TexCoord2);
            _editor.SelectedTexture = textureArea;
        }

        private static bool ApplyTextureAutomaticallyNoUpdated(Room room, VectorInt2 pos, BlockFace face, TextureArea texture)
        {
            Block block = room.GetBlock(pos);

            TextureArea processedTexture = texture;

            if (_editor.Tool.TextureUVFixer && texture.TextureIsRectangle)
            {
                switch (face)
                {
                    case BlockFace.Floor:
                        if (block.FloorIsQuad)
                            break;
                        else
                        {
                            if (block.FloorSplitDirectionIsXEqualsZ)
                            {
                                Swap.Do(ref processedTexture.TexCoord0, ref processedTexture.TexCoord2);
                                processedTexture.TexCoord1 = processedTexture.TexCoord3;
                                processedTexture.TexCoord3 = processedTexture.TexCoord2;
                            }
                            else
                            {
                                processedTexture.TexCoord0 = processedTexture.TexCoord1;
                                processedTexture.TexCoord1 = processedTexture.TexCoord2;
                                processedTexture.TexCoord2 = processedTexture.TexCoord3;
                            }
                        }
                        break;

                    case BlockFace.FloorTriangle2:
                        if (block.FloorIsQuad)
                            break;
                        else
                        {
                            if (block.FloorSplitDirectionIsXEqualsZ)
                                processedTexture.TexCoord3 = processedTexture.TexCoord0;
                            else
                            {
                                processedTexture.TexCoord2 = processedTexture.TexCoord1;
                                processedTexture.TexCoord1 = processedTexture.TexCoord0;
                                processedTexture.TexCoord0 = processedTexture.TexCoord3;
                                processedTexture.TexCoord3 = processedTexture.TexCoord2;
                            }
                        }
                        break;


                    case BlockFace.Ceiling:
                        if (block.CeilingIsQuad)
                            break;
                        else
                        {
                            if (block.CeilingSplitDirectionIsXEqualsZ)
                            {
                                Swap.Do(ref processedTexture.TexCoord0, ref processedTexture.TexCoord2);
                                processedTexture.TexCoord1 = processedTexture.TexCoord3;
                                processedTexture.TexCoord3 = processedTexture.TexCoord2;
                            }
                            else
                            {
                                processedTexture.TexCoord0 = processedTexture.TexCoord1;
                                processedTexture.TexCoord1 = processedTexture.TexCoord2;
                                processedTexture.TexCoord2 = processedTexture.TexCoord3;
                            }
                        }
                        break;


                    case BlockFace.CeilingTriangle2:
                        if (block.CeilingIsQuad)
                            break;
                        else
                        {
                            if (block.CeilingSplitDirectionIsXEqualsZ)
                            {
                                processedTexture.TexCoord3 = processedTexture.TexCoord0;
                            }
                            else
                            {
                                processedTexture.TexCoord2 = processedTexture.TexCoord1;
                                processedTexture.TexCoord1 = processedTexture.TexCoord0;
                                processedTexture.TexCoord0 = processedTexture.TexCoord3;
                                processedTexture.TexCoord3 = processedTexture.TexCoord2;
                            }
                        }
                        break;

                    default:
                        {
                            var indices = room.GetFaceIndices(pos.X, pos.Y, face);
                            var vertices = room.GetRoomVertices();

                            if (indices.Count < 3)
                                break;
                            else
                            {
                                float maxUp;
                                float minDown;
                                float delta0;
                                float delta1;
                                float delta2;
                                float delta3;
                                float difference;

                                if (indices.Count == 4)
                                {
                                    maxUp = Math.Max(vertices[indices[0]].Position.Y, vertices[indices[1]].Position.Y);
                                    minDown = Math.Min(vertices[indices[3]].Position.Y, vertices[indices[2]].Position.Y);

                                    difference = maxUp - minDown;

                                    delta0 = (minDown - vertices[indices[3]].Position.Y) / difference;
                                    delta1 = (maxUp - vertices[indices[0]].Position.Y) / difference;
                                    delta2 = (maxUp - vertices[indices[1]].Position.Y) / difference;
                                    delta3 = (minDown - vertices[indices[2]].Position.Y) / difference;

                                    if (texture.TexCoord0.X == texture.TexCoord1.X && texture.TexCoord3.X == texture.TexCoord2.X)
                                    {
                                        processedTexture.TexCoord0.Y += (texture.TexCoord0.Y - texture.TexCoord1.Y) * delta0;
                                        processedTexture.TexCoord1.Y += (texture.TexCoord0.Y - texture.TexCoord1.Y) * delta1;
                                        processedTexture.TexCoord2.Y += (texture.TexCoord3.Y - texture.TexCoord2.Y) * delta2;
                                        processedTexture.TexCoord3.Y += (texture.TexCoord3.Y - texture.TexCoord2.Y) * delta3;
                                    }
                                    else
                                    {
                                        processedTexture.TexCoord0.X += (texture.TexCoord0.X - texture.TexCoord1.X) * delta0;
                                        processedTexture.TexCoord1.X += (texture.TexCoord0.X - texture.TexCoord1.X) * delta1;
                                        processedTexture.TexCoord2.X += (texture.TexCoord3.X - texture.TexCoord2.X) * delta2;
                                        processedTexture.TexCoord3.X += (texture.TexCoord3.X - texture.TexCoord2.X) * delta3;
                                    }
                                }
                                else
                                {
                                    maxUp = Math.Max(Math.Max(vertices[indices[0]].Position.Y, vertices[indices[1]].Position.Y), vertices[indices[2]].Position.Y);
                                    minDown = Math.Min(Math.Min(vertices[indices[0]].Position.Y, vertices[indices[1]].Position.Y), vertices[indices[2]].Position.Y);
                                    difference = maxUp - minDown;

                                    if (vertices[indices[0]].Position.X == vertices[indices[2]].Position.X && vertices[indices[0]].Position.Z == vertices[indices[2]].Position.Z)
                                    {
                                        delta0 = (minDown - vertices[indices[2]].Position.Y) / difference;
                                        delta1 = (maxUp - vertices[indices[0]].Position.Y) / difference;
                                        delta2 = (maxUp - vertices[indices[1]].Position.Y) / difference;
                                        delta3 = (minDown - vertices[indices[1]].Position.Y) / difference;

                                        if (texture.TexCoord0.X == texture.TexCoord1.X && texture.TexCoord3.X == texture.TexCoord2.X)
                                        {
                                            processedTexture.TexCoord0.Y += (texture.TexCoord0.Y - texture.TexCoord1.Y) * delta0;
                                            processedTexture.TexCoord1.Y += (texture.TexCoord0.Y - texture.TexCoord1.Y) * delta1;
                                            processedTexture.TexCoord2.Y += (texture.TexCoord3.Y - texture.TexCoord2.Y) * delta2;
                                            processedTexture.TexCoord3.Y += (texture.TexCoord3.Y - texture.TexCoord2.Y) * delta3;
                                        }
                                        else
                                        {
                                            processedTexture.TexCoord0.X += (texture.TexCoord0.X - texture.TexCoord1.X) * delta0;
                                            processedTexture.TexCoord1.X += (texture.TexCoord0.X - texture.TexCoord1.X) * delta1;
                                            processedTexture.TexCoord2.X += (texture.TexCoord3.X - texture.TexCoord2.X) * delta2;
                                            processedTexture.TexCoord3.X += (texture.TexCoord3.X - texture.TexCoord2.X) * delta3;
                                        }

                                        processedTexture.TexCoord3 = processedTexture.TexCoord0;
                                        processedTexture.TexCoord0 = processedTexture.TexCoord1;
                                        processedTexture.TexCoord1 = processedTexture.TexCoord2;
                                        processedTexture.TexCoord2 = processedTexture.TexCoord3;

                                    }
                                    else
                                    {

                                        delta0 = (minDown - vertices[indices[0]].Position.Y) / difference;
                                        delta1 = (maxUp - vertices[indices[0]].Position.Y) / difference;
                                        delta2 = (maxUp - vertices[indices[1]].Position.Y) / difference;
                                        delta3 = (minDown - vertices[indices[2]].Position.Y) / difference;

                                        if (texture.TexCoord0.X == texture.TexCoord1.X && texture.TexCoord3.X == texture.TexCoord2.X)
                                        {
                                            processedTexture.TexCoord0.Y += (texture.TexCoord0.Y - texture.TexCoord1.Y) * delta0;
                                            processedTexture.TexCoord1.Y += (texture.TexCoord0.Y - texture.TexCoord1.Y) * delta1;
                                            processedTexture.TexCoord2.Y += (texture.TexCoord3.Y - texture.TexCoord2.Y) * delta2;
                                            processedTexture.TexCoord3.Y += (texture.TexCoord3.Y - texture.TexCoord2.Y) * delta3;
                                        }
                                        else
                                        {
                                            processedTexture.TexCoord0.X += (texture.TexCoord0.X - texture.TexCoord1.X) * delta0;
                                            processedTexture.TexCoord1.X += (texture.TexCoord0.X - texture.TexCoord1.X) * delta1;
                                            processedTexture.TexCoord2.X += (texture.TexCoord3.X - texture.TexCoord2.X) * delta2;
                                            processedTexture.TexCoord3.X += (texture.TexCoord3.X - texture.TexCoord2.X) * delta3;
                                        }

                                        processedTexture.TexCoord0 = processedTexture.TexCoord3;

                                        Vector2 tempTexCoord = processedTexture.TexCoord2;
                                        processedTexture.TexCoord2 = processedTexture.TexCoord3;
                                        processedTexture.TexCoord3 = processedTexture.TexCoord0;
                                        processedTexture.TexCoord0 = processedTexture.TexCoord1;
                                        processedTexture.TexCoord1 = tempTexCoord;
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            return block.SetFaceTexture(face, processedTexture);
        }

        public static bool ApplyTextureAutomatically(Room room, VectorInt2 pos, BlockFace face, TextureArea texture)
        {
            var textureApplied = ApplyTextureAutomaticallyNoUpdated(room, pos, face, texture);
            if (textureApplied)
            {
                room.BuildGeometry(new RectangleInt2(pos, pos));
                room.CalculateLightingForThisRoom();
                room.UpdateBuffers();
                _editor.RoomTextureChange(room);
            }
            return textureApplied;
        }

        public static Dictionary<BlockFace, float[]> GetFaces(Room room, VectorInt2 pos, Direction direction, BlockFaceType section)
        {
            bool sectionIsWall = room.GetBlockTry(pos.X, pos.Y).IsAnyWall;

            Dictionary<BlockFace, float[]> segments = new Dictionary<BlockFace, float[]>();

            switch (direction)
            {
                case Direction.PositiveZ:
                    if (section == BlockFaceType.Ceiling || sectionIsWall)
                    {
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.PositiveZ_RF))
                            segments.Add(BlockFace.PositiveZ_RF, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.PositiveZ_RF), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.PositiveZ_RF) });
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.PositiveZ_WS))
                            segments.Add(BlockFace.PositiveZ_WS, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.PositiveZ_WS), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.PositiveZ_WS) });
                    }
                    if (section == BlockFaceType.Floor || sectionIsWall)
                    {
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.PositiveZ_QA))
                            segments.Add(BlockFace.PositiveZ_QA, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.PositiveZ_QA), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.PositiveZ_QA) });
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.PositiveZ_ED))
                            segments.Add(BlockFace.PositiveZ_ED, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.PositiveZ_ED), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.PositiveZ_ED) });
                    }
                    if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.PositiveZ_Middle))
                        segments.Add(BlockFace.PositiveZ_Middle, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.PositiveZ_Middle), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.PositiveZ_Middle) });
                    break;

                case Direction.NegativeZ:
                    if (section == BlockFaceType.Ceiling || sectionIsWall)
                    {
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.NegativeZ_RF))
                            segments.Add(BlockFace.NegativeZ_RF, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.NegativeZ_RF), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.NegativeZ_RF) });
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.NegativeZ_WS))
                            segments.Add(BlockFace.NegativeZ_WS, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.NegativeZ_WS), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.NegativeZ_WS) });
                    }
                    if (section == BlockFaceType.Floor || sectionIsWall)
                    {
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.NegativeZ_QA))
                            segments.Add(BlockFace.NegativeZ_QA, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.NegativeZ_QA), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.NegativeZ_QA) });
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.NegativeZ_ED))
                            segments.Add(BlockFace.NegativeZ_ED, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.NegativeZ_ED), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.NegativeZ_ED) });
                    }
                    if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.NegativeZ_Middle))
                        segments.Add(BlockFace.NegativeZ_Middle, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.NegativeZ_Middle), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.NegativeZ_Middle) });
                    break;

                case Direction.PositiveX:
                    if (section == BlockFaceType.Ceiling || sectionIsWall)
                    {
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.PositiveX_RF))
                            segments.Add(BlockFace.PositiveX_RF, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.PositiveX_RF), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.PositiveX_RF) });
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.PositiveX_WS))
                            segments.Add(BlockFace.PositiveX_WS, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.PositiveX_WS), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.PositiveX_WS) });
                    }
                    if (section == BlockFaceType.Floor || sectionIsWall)
                    {
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.PositiveX_QA))
                            segments.Add(BlockFace.PositiveX_QA, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.PositiveX_QA), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.PositiveX_QA) });
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.PositiveX_ED))
                            segments.Add(BlockFace.PositiveX_ED, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.PositiveX_ED), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.PositiveX_ED) });
                    }
                    if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.PositiveX_Middle))
                        segments.Add(BlockFace.PositiveX_Middle, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.PositiveX_Middle), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.PositiveX_Middle) });
                    break;

                case Direction.NegativeX:
                    if (section == BlockFaceType.Ceiling || sectionIsWall)
                    {
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.NegativeX_RF))
                            segments.Add(BlockFace.NegativeX_RF, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.NegativeX_RF), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.NegativeX_RF) });
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.NegativeX_WS))
                            segments.Add(BlockFace.NegativeX_WS, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.NegativeX_WS), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.NegativeX_WS) });
                    }
                    if (section == BlockFaceType.Floor || sectionIsWall)
                    {
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.NegativeX_QA))
                            segments.Add(BlockFace.NegativeX_QA, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.NegativeX_QA), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.NegativeX_QA) });
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.NegativeX_ED))
                            segments.Add(BlockFace.NegativeX_ED, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.NegativeX_ED), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.NegativeX_ED) });
                    }
                    if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.NegativeX_Middle))
                        segments.Add(BlockFace.NegativeX_Middle, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.NegativeX_Middle), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.NegativeX_Middle) });
                    break;

                case Direction.Diagonal:
                    if (section == BlockFaceType.Ceiling || sectionIsWall)
                    {
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.DiagonalRF))
                            segments.Add(BlockFace.DiagonalRF, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.DiagonalRF), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.DiagonalRF) });
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.DiagonalWS))
                            segments.Add(BlockFace.DiagonalWS, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.DiagonalWS), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.DiagonalWS) });
                    }
                    if (section == BlockFaceType.Floor || sectionIsWall)
                    {
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.DiagonalQA))
                            segments.Add(BlockFace.DiagonalQA, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.DiagonalQA), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.DiagonalQA) });
                        if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.DiagonalED))
                            segments.Add(BlockFace.DiagonalED, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.DiagonalED), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.DiagonalED) });
                    }
                    if (room.IsFaceDefined(pos.X, pos.Y, BlockFace.DiagonalMiddle))
                        segments.Add(BlockFace.DiagonalMiddle, new float[2] { room.GetFaceHighestPoint(pos.X, pos.Y, BlockFace.DiagonalMiddle), room.GetFaceLowestPoint(pos.X, pos.Y, BlockFace.DiagonalMiddle) });
                    break;
            }

            return segments;
        }

        private static float[] GetAreaExtremums(Room room, RectangleInt2 area, Direction direction, BlockFaceType type)
        {
            float maxHeight = float.MinValue;
            float minHeight = float.MaxValue;

            for (int x = area.X0, iterX = 0; x <= area.X1; x++, iterX++)
                for (int z = area.Y0, iterZ = 0; z <= area.Y1; z++, iterZ++)
                {
                    var segments = GetFaces(room, new VectorInt2(x, z), direction, type);

                    foreach (var segment in segments)
                    {
                        minHeight = Math.Min(minHeight, segment.Value[1]);
                        maxHeight = Math.Max(maxHeight, segment.Value[0]);
                    }
                }

            return new float[2] { minHeight, maxHeight };
        }

        public static void TexturizeWallSection(Room room, VectorInt2 pos, Direction direction, BlockFaceType section, TextureArea texture, int subdivisions = 0, int iteration = 0, float[] overrideHeights = null)
        {
            if (subdivisions < 0 || iteration < 0)
                subdivisions = 0;

            if (overrideHeights?.Count() != 2)
                overrideHeights = null;

            var segments = GetFaces(room, pos, direction, section);

            var processedTexture = texture;
            float minSectionHeight = float.MaxValue;
            float maxSectionHeight = float.MinValue;

            if (overrideHeights == null)
            {
                foreach (var segment in segments)
                {
                    minSectionHeight = Math.Min(minSectionHeight, segment.Value[1]);
                    maxSectionHeight = Math.Max(maxSectionHeight, segment.Value[0]);
                }
            }
            else
            {
                minSectionHeight = overrideHeights[0];
                maxSectionHeight = overrideHeights[1];
            }

            float sectionHeight = (maxSectionHeight - minSectionHeight);
            bool inverted = false;

            foreach (var segment in segments)
            {
                float currentHighestPoint = Math.Abs(segment.Value[0] - maxSectionHeight) / sectionHeight;
                float currentLowestPoint = Math.Abs(maxSectionHeight - segment.Value[1]) / sectionHeight;

                if (texture.TexCoord0.X == texture.TexCoord1.X && texture.TexCoord3.X == texture.TexCoord2.X)
                {
                    float textureHeight = texture.TexCoord0.Y - texture.TexCoord1.Y;

                    processedTexture.TexCoord0.Y = texture.TexCoord1.Y + textureHeight * currentLowestPoint;
                    processedTexture.TexCoord3.Y = texture.TexCoord2.Y + textureHeight * currentLowestPoint;
                    processedTexture.TexCoord1.Y = texture.TexCoord1.Y + textureHeight * currentHighestPoint;
                    processedTexture.TexCoord2.Y = texture.TexCoord2.Y + textureHeight * currentHighestPoint;

                    if (subdivisions > 0)
                    {
                        float stride = (texture.TexCoord2.X - texture.TexCoord1.X) / (subdivisions + 1);

                        if (inverted == false & (direction == Direction.NegativeX || direction == Direction.PositiveZ))
                        {
                            inverted = true;
                            iteration = subdivisions - iteration;
                        }

                        processedTexture.TexCoord0.X = texture.TexCoord0.X + stride * iteration;
                        processedTexture.TexCoord1.X = texture.TexCoord1.X + stride * iteration;
                        processedTexture.TexCoord3.X = texture.TexCoord3.X - stride * (subdivisions - (iteration));
                        processedTexture.TexCoord2.X = texture.TexCoord2.X - stride * (subdivisions - (iteration));
                    }

                }
                else
                {
                    float textureWidth = texture.TexCoord3.X - texture.TexCoord2.X;

                    processedTexture.TexCoord3.X = texture.TexCoord2.X + textureWidth * currentLowestPoint;
                    processedTexture.TexCoord0.X = texture.TexCoord1.X + textureWidth * currentLowestPoint;
                    processedTexture.TexCoord2.X = texture.TexCoord2.X + textureWidth * currentHighestPoint;
                    processedTexture.TexCoord1.X = texture.TexCoord1.X + textureWidth * currentHighestPoint;

                    if (subdivisions > 0)
                    {
                        float stride = (texture.TexCoord0.Y - texture.TexCoord3.Y) / (subdivisions + 1);

                        if (inverted == false & (direction == Direction.PositiveX || direction == Direction.NegativeZ))
                        {
                            inverted = true;
                            iteration = subdivisions - iteration;
                        }

                        processedTexture.TexCoord2.Y = texture.TexCoord2.Y + stride * iteration;
                        processedTexture.TexCoord3.Y = texture.TexCoord3.Y + stride * iteration;
                        processedTexture.TexCoord1.Y = texture.TexCoord1.Y - stride * (subdivisions - (iteration));
                        processedTexture.TexCoord0.Y = texture.TexCoord0.Y - stride * (subdivisions - (iteration));
                    }
                }

                ApplyTextureAutomaticallyNoUpdated(room, pos, segment.Key, processedTexture);
            }
        }

        public static void TexturizeGroup(Room room, SectorSelection selection, TextureArea texture, BlockFace pickedFace, bool subdivideWalls = false, bool unifyHeight = false)
        {
            RectangleInt2 area = (selection.Valid ? selection.Area : _editor.SelectedRoom.LocalArea);

            if (pickedFace < BlockFace.Floor)
            {
                int xSubs = (subdivideWalls == true ? area.X1 - area.X0 : 0);
                int zSubs = (subdivideWalls == true ? area.Y1 - area.Y0 : 0);

                for (int x = area.X0, iterX = 0; x <= area.X1; x++, iterX++)
                    for (int z = area.Y0, iterZ = 0; z <= area.Y1; z++, iterZ++)
                        switch (pickedFace)
                        {
                            case BlockFace.NegativeX_QA:
                            case BlockFace.NegativeX_ED:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.NegativeX, BlockFaceType.Floor, texture, zSubs, iterZ, (unifyHeight ? GetAreaExtremums(room, area, Direction.NegativeX, BlockFaceType.Floor) : null));
                                break;

                            case BlockFace.NegativeX_Middle:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.NegativeX, BlockFaceType.Wall, texture, zSubs, iterZ, (unifyHeight ? GetAreaExtremums(room, area, Direction.NegativeX, BlockFaceType.Wall) : null));
                                break;

                            case BlockFace.NegativeX_RF:
                            case BlockFace.NegativeX_WS:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.NegativeX, BlockFaceType.Ceiling, texture, zSubs, iterZ, (unifyHeight ? GetAreaExtremums(room, area, Direction.NegativeX, BlockFaceType.Ceiling) : null));
                                break;

                            case BlockFace.PositiveX_QA:
                            case BlockFace.PositiveX_ED:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.PositiveX, BlockFaceType.Floor, texture, zSubs, iterZ, (unifyHeight ? GetAreaExtremums(room, area, Direction.PositiveX, BlockFaceType.Floor) : null));
                                break;

                            case BlockFace.PositiveX_Middle:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.PositiveX, BlockFaceType.Wall, texture, zSubs, iterZ, (unifyHeight ? GetAreaExtremums(room, area, Direction.PositiveX, BlockFaceType.Wall) : null));
                                break;

                            case BlockFace.PositiveX_RF:
                            case BlockFace.PositiveX_WS:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.PositiveX, BlockFaceType.Ceiling, texture, zSubs, iterZ, (unifyHeight ? GetAreaExtremums(room, area, Direction.PositiveX, BlockFaceType.Ceiling) : null));
                                break;

                            case BlockFace.NegativeZ_QA:
                            case BlockFace.NegativeZ_ED:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.NegativeZ, BlockFaceType.Floor, texture, xSubs, iterX, (unifyHeight ? GetAreaExtremums(room, area, Direction.NegativeZ, BlockFaceType.Floor) : null));
                                break;

                            case BlockFace.NegativeZ_Middle:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.NegativeZ, BlockFaceType.Wall, texture, xSubs, iterX, (unifyHeight ? GetAreaExtremums(room, area, Direction.NegativeZ, BlockFaceType.Wall) : null));
                                break;

                            case BlockFace.NegativeZ_RF:
                            case BlockFace.NegativeZ_WS:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.NegativeZ, BlockFaceType.Ceiling, texture, xSubs, iterX, (unifyHeight ? GetAreaExtremums(room, area, Direction.NegativeZ, BlockFaceType.Ceiling) : null));
                                break;

                            case BlockFace.PositiveZ_QA:
                            case BlockFace.PositiveZ_ED:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.PositiveZ, BlockFaceType.Floor, texture, xSubs, iterX, (unifyHeight ? GetAreaExtremums(room, area, Direction.PositiveZ, BlockFaceType.Floor) : null));
                                break;

                            case BlockFace.PositiveZ_Middle:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.PositiveZ, BlockFaceType.Wall, texture, xSubs, iterX, (unifyHeight ? GetAreaExtremums(room, area, Direction.PositiveZ, BlockFaceType.Wall) : null));
                                break;

                            case BlockFace.PositiveZ_RF:
                            case BlockFace.PositiveZ_WS:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.PositiveZ, BlockFaceType.Ceiling, texture, xSubs, iterX, (unifyHeight ? GetAreaExtremums(room, area, Direction.PositiveZ, BlockFaceType.Ceiling) : null));
                                break;

                            case BlockFace.DiagonalQA:
                            case BlockFace.DiagonalED:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.Diagonal, BlockFaceType.Floor, texture);
                                break;

                            case BlockFace.DiagonalMiddle:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.Diagonal, BlockFaceType.Wall, texture);
                                break;

                            case BlockFace.DiagonalRF:
                            case BlockFace.DiagonalWS:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.Diagonal, BlockFaceType.Ceiling, texture);
                                break;
                        }
            }
            else
            {
                Vector2 verticalUVStride = (texture.TexCoord3 - texture.TexCoord2) / (float)(area.Y1 - area.Y0 + 1);
                Vector2 horizontalUVStride = (texture.TexCoord2 - texture.TexCoord1) / (float)(area.X1 - area.X0 + 1);

                for (int x = area.X0, x1 = 0; x <= area.X1; x++, x1++)
                {
                    Vector2 currentX = horizontalUVStride * x1;

                    for (int z = area.Y0, z1 = 0; z <= area.Y1; z++, z1++)
                    {
                        TextureArea currentTexture = texture;
                        Vector2 currentZ = verticalUVStride * z1;

                        currentTexture.TexCoord0 -= currentZ - currentX;
                        currentTexture.TexCoord1 = currentTexture.TexCoord0 - verticalUVStride;
                        currentTexture.TexCoord3 = currentTexture.TexCoord0 + horizontalUVStride;
                        currentTexture.TexCoord2 = currentTexture.TexCoord0 + horizontalUVStride - verticalUVStride;

                        switch (pickedFace)
                        {
                            case BlockFace.Floor:
                            case BlockFace.FloorTriangle2:
                                ApplyTextureAutomaticallyNoUpdated(room, new VectorInt2(x, z), BlockFace.Floor, currentTexture);
                                ApplyTextureAutomaticallyNoUpdated(room, new VectorInt2(x, z), BlockFace.FloorTriangle2, currentTexture);
                                break;

                            case BlockFace.Ceiling:
                            case BlockFace.CeilingTriangle2:
                                ApplyTextureAutomaticallyNoUpdated(room, new VectorInt2(x, z), BlockFace.Ceiling, currentTexture);
                                ApplyTextureAutomaticallyNoUpdated(room, new VectorInt2(x, z), BlockFace.CeilingTriangle2, currentTexture);
                                break;
                        }
                    }
                }
            }

            room.UpdateCompletely();
            _editor.RoomTextureChange(room);
        }

        public static void TexturizeAll(Room room, SectorSelection selection, TextureArea texture, BlockFaceType type)
        {
            RectangleInt2 area = (selection.Valid ? selection.Area : _editor.SelectedRoom.LocalArea);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    switch (type)
                    {
                        case BlockFaceType.Floor:
                            ApplyTextureAutomaticallyNoUpdated(room, new VectorInt2(x, z), BlockFace.Floor, texture);
                            ApplyTextureAutomaticallyNoUpdated(room, new VectorInt2(x, z), BlockFace.FloorTriangle2, texture);
                            break;

                        case BlockFaceType.Ceiling:
                            ApplyTextureAutomaticallyNoUpdated(room, new VectorInt2(x, z), BlockFace.Ceiling, texture);
                            ApplyTextureAutomaticallyNoUpdated(room, new VectorInt2(x, z), BlockFace.CeilingTriangle2, texture);
                            break;

                        case BlockFaceType.Wall:
                            for (BlockFace face = BlockFace.PositiveZ_QA; face <= BlockFace.DiagonalRF; face++)
                                if (room.IsFaceDefined(x, z, face))
                                    ApplyTextureAutomaticallyNoUpdated(room, new VectorInt2(x, z), face, texture);
                            break;
                    }

                }

            room.UpdateCompletely();
            _editor.RoomTextureChange(room);
        }

        public static void PlaceObject(Room room, VectorInt2 pos, PositionBasedObjectInstance instance)
        {
            Block block = room.GetBlock(pos);
            int y = (block.QA[0] + block.QA[1] + block.QA[2] + block.QA[3]) / 4;

            instance.Position = new Vector3(pos.X * 1024 + 512, y * 256, pos.Y * 1024 + 512);
            room.AddObject(_editor.Level, instance);
            if (instance is LightInstance)
                room.UpdateCompletely(); // Rebuild lighting!
            _editor.ObjectChange(instance, ObjectChangeType.Add);
            _editor.SelectedObject = instance;
        }

        public static void DeleteRooms(IEnumerable<Room> rooms_, IWin32Window owner)
        {
            rooms_ = rooms_.SelectMany(room => room.Versions).Distinct();
            HashSet<Room> rooms = new HashSet<Room>(rooms_);

            // Check if is the last room
            int remainingRoomCount = _editor.Level.Rooms.Count(r => (r != null) && !rooms.Contains(r) && !rooms.Contains(r.AlternateVersion));
            if (remainingRoomCount <= 0)
            {
                DarkMessageBox.Show(owner, "You must have at least one room in your level.", "Error", MessageBoxIcon.Error);
                return;
            }

            // Ask for confirmation
            if (DarkMessageBox.Show(owner,
                    "Do you really want to delete rooms? All objects (including portals) inside rooms will be deleted and " +
                    "triggers pointing to them will be removed.",
                    "Delete rooms", MessageBoxButtons.YesNo, MessageBoxIcon.Error) != DialogResult.Yes)
            {
                return;
            }

            // Do it finally
            List<Room> adjoiningRooms = rooms.SelectMany(room => room.Portals)
                .Select(portal => portal.AdjoiningRoom)
                .Distinct()
                .Except(rooms)
                .ToList();
            foreach (Room room in rooms)
                _editor.Level.DeleteAlternateRoom(room);

            // Update selection
            foreach (Room adjoiningRoom in adjoiningRooms)
            {
                adjoiningRoom?.UpdateCompletely();
                adjoiningRoom?.AlternateVersion?.UpdateCompletely();
            }
            if (rooms.Contains(_editor.SelectedRoom))
                _editor.SelectRoomAndResetCamera(_editor.Level.Rooms.FirstOrDefault(r => r != null));
            _editor.RoomListChange();
        }

        public static void CropRoom(Room room, RectangleInt2 newArea, IWin32Window owner)
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
            if (room.AlternateVersion != null)
            {
                room.AlternateVersion.Resize(_editor.Level, newArea, (short)room.GetLowestCorner(), (short)room.GetHighestCorner());
                room.AlternateVersion.UpdateCompletely();
            }
            room.Resize(_editor.Level, newArea, (short)room.GetLowestCorner(), (short)room.GetHighestCorner());
            room.UpdateCompletely();

            // Fix selection if necessary
            if ((_editor.SelectedRoom == room) && _editor.SelectedSectors.Valid)
            {
                var selection = _editor.SelectedSectors;
                selection.Area = selection.Area.Intersect(newArea) - newArea.Start;
                _editor.SelectedSectors = selection;
            }
            _editor.RoomPropertiesChange(room);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetDiagonalFloorSplit(Room room, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;

                    if (room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                    {
                        room.Blocks[x, z].Transform(new RectTransformation { QuadrantRotation = 1 }, false);
                    }
                    else
                    {
                        // Now try to guess the floor split
                        short maxHeight = -32767;
                        byte theCorner = 0;

                        if (room.Blocks[x, z].QA[0] > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].QA[0];
                            theCorner = 0;
                        }

                        if (room.Blocks[x, z].QA[1] > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].QA[1];
                            theCorner = 1;
                        }

                        if (room.Blocks[x, z].QA[2] > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].QA[2];
                            theCorner = 2;
                        }

                        if (room.Blocks[x, z].QA[3] > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].QA[3];
                            theCorner = 3;
                        }

                        if (theCorner == 0)
                        {
                            room.Blocks[x, z].QA[1] = maxHeight;
                            room.Blocks[x, z].QA[3] = maxHeight;
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XnZp;
                            if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                                room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XnZp;
                        }

                        if (theCorner == 1)
                        {
                            room.Blocks[x, z].QA[0] = maxHeight;
                            room.Blocks[x, z].QA[2] = maxHeight;
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XpZp;
                            if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                                room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XpZp;
                        }

                        if (theCorner == 2)
                        {
                            room.Blocks[x, z].QA[1] = maxHeight;
                            room.Blocks[x, z].QA[3] = maxHeight;
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XpZn;
                            if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                                room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XpZn;
                        }

                        if (theCorner == 3)
                        {
                            room.Blocks[x, z].QA[0] = maxHeight;
                            room.Blocks[x, z].QA[2] = maxHeight;
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XnZn;
                            if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                                room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XnZn;
                        }

                        room.Blocks[x, z].FloorSplitDirectionToggled = false;
                        room.Blocks[x, z].FixHeights();
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void SetDiagonalCeilingSplit(Room room, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;


                    if (room.Blocks[x, z].CeilingDiagonalSplit != DiagonalSplit.None)
                    {
                        room.Blocks[x, z].Transform(new RectTransformation { QuadrantRotation = 1 }, true);
                    }
                    else
                    {
                        // Now try to guess the floor split
                        short minHeight = 32767;
                        byte theCorner = 0;

                        if (room.Blocks[x, z].WS[0] < minHeight)
                        {
                            minHeight = room.Blocks[x, z].WS[0];
                            theCorner = 0;
                        }

                        if (room.Blocks[x, z].WS[1] < minHeight)
                        {
                            minHeight = room.Blocks[x, z].WS[1];
                            theCorner = 1;
                        }

                        if (room.Blocks[x, z].WS[2] < minHeight)
                        {
                            minHeight = room.Blocks[x, z].WS[2];
                            theCorner = 2;
                        }

                        if (room.Blocks[x, z].WS[3] < minHeight)
                        {
                            minHeight = room.Blocks[x, z].WS[3];
                            theCorner = 3;
                        }

                        if (theCorner == 0)
                        {
                            room.Blocks[x, z].WS[1] = minHeight;
                            room.Blocks[x, z].WS[3] = minHeight;
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XnZp;
                            if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                                room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XnZp;
                        }

                        if (theCorner == 1)
                        {
                            room.Blocks[x, z].WS[0] = minHeight;
                            room.Blocks[x, z].WS[2] = minHeight;
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XpZp;
                            if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                                room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XpZp;
                        }

                        if (theCorner == 2)
                        {
                            room.Blocks[x, z].WS[1] = minHeight;
                            room.Blocks[x, z].WS[3] = minHeight;
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XpZn;
                            if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                                room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XpZn;
                        }

                        if (theCorner == 3)
                        {
                            room.Blocks[x, z].WS[0] = minHeight;
                            room.Blocks[x, z].WS[2] = minHeight;
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XnZn;
                            if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                                room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XnZn;
                        }

                        room.Blocks[x, z].CeilingSplitDirectionToggled = false;
                        room.Blocks[x, z].FixHeights();
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void SetDiagonalWall(Room room, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;

                    if (room.Blocks[x, z].Type == BlockType.Wall && room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None)
                        room.Blocks[x, z].Transform(new RectTransformation { QuadrantRotation = 1 }, false);
                    else
                    {
                        // Now try to guess the floor split
                        short maxHeight = -32767;
                        byte theCorner = 0;

                        if (room.Blocks[x, z].QA[0] > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].QA[0];
                            theCorner = 0;
                        }

                        if (room.Blocks[x, z].QA[1] > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].QA[1];
                            theCorner = 1;
                        }

                        if (room.Blocks[x, z].QA[2] > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].QA[2];
                            theCorner = 2;
                        }

                        if (room.Blocks[x, z].QA[3] > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].QA[3];
                            theCorner = 3;
                        }

                        if (theCorner == 0)
                        {
                            room.Blocks[x, z].QA[1] = maxHeight;
                            room.Blocks[x, z].QA[3] = maxHeight;
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XnZp;
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XnZp;
                        }
                        else if (theCorner == 1)
                        {
                            room.Blocks[x, z].QA[0] = maxHeight;
                            room.Blocks[x, z].QA[2] = maxHeight;
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XpZp;
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XpZp;
                        }
                        else if (theCorner == 2)
                        {
                            room.Blocks[x, z].QA[1] = maxHeight;
                            room.Blocks[x, z].QA[3] = maxHeight;
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XpZn;
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XpZn;
                        }
                        else
                        {
                            room.Blocks[x, z].QA[0] = maxHeight;
                            room.Blocks[x, z].QA[2] = maxHeight;
                            room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.XnZn;
                            room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.XnZn;
                        }

                        room.Blocks[x, z].Type = BlockType.Wall;
                    }
                }

            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void RotateSectors(Room room, RectangleInt2 area, bool floor)
        {
            bool wallsRotated = false;

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;
                    room.Blocks[x, z].Transform(new RectTransformation { QuadrantRotation = 1 }, floor);

                    if (room.Blocks[x, z].FloorDiagonalSplit != DiagonalSplit.None && room.Blocks[x, z].IsAnyWall)
                        wallsRotated = true;
                }

            SmartBuildGeometry(room, area);
            _editor.RoomGeometryChange(room);

            if (wallsRotated)
                _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetWall(Room room, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;
                    room.Blocks[x, z].Type = BlockType.Wall;
                    room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.None;
                }

            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetFloor(Room room, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;

                    room.Blocks[x, z].Type = BlockType.Floor;
                    room.Blocks[x, z].FloorDiagonalSplit = DiagonalSplit.None;
                }

            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetCeiling(Room room, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;

                    room.Blocks[x, z].CeilingDiagonalSplit = DiagonalSplit.None;
                }

            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void ToggleBlockFlag(Room room, RectangleInt2 area, BlockFlags flag)
        {
            List<Room> roomsToUpdate = new List<Room>();
            roomsToUpdate.Add(room);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Room.RoomBlockPair currentBlock = room.ProbeLowestBlock(x, z, _editor.Configuration.Editor_ProbeAttributesThroughPortals);
                    currentBlock.Block.Flags ^= flag;

                    if (!roomsToUpdate.Contains(currentBlock.Room))
                        roomsToUpdate.Add(currentBlock.Room);
                }

            foreach (var currentRoom in roomsToUpdate)
                _editor.RoomSectorPropertiesChange(currentRoom);
        }

        public static void ToggleForceFloorSolid(Room room, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    room.Blocks[x, z].ForceFloorSolid = !room.Blocks[x, z].ForceFloorSolid;
            SmartBuildGeometry(room, area);
            _editor.RoomGeometryChange(room);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void AddPortal(Room room, RectangleInt2 area, IWin32Window owner)
        {
            // Check for possible candidates ...
            VerticalSpace? verticalSpaceLocal = room.GetHeightInAreaMaxSpace(new RectangleInt2(area.X0, area.Y0, area.X1 + 1, area.Y1 + 1));

            List<Tuple<PortalDirection, Room>> candidates = new List<Tuple<PortalDirection, Room>>();
            if (verticalSpaceLocal != null)
            {
                VerticalSpace verticalSpace = verticalSpaceLocal.Value + room.Position.Y;
                bool couldBeFloorCeilingPortal = false;
                if (new RectangleInt2(1, 1, room.NumXSectors - 2, room.NumZSectors - 2).Contains(area))
                    for (int z = area.Y0; z <= area.Y1; ++z)
                        for (int x = area.X0; x <= area.X1; ++x)
                            if (!room.Blocks[x, z].IsAnyWall)
                                couldBeFloorCeilingPortal = true;

                foreach (Room neighborRoom in _editor.Level.Rooms.Where(possibleNeighborRoom => possibleNeighborRoom != null))
                {
                    if ((neighborRoom == room) || (neighborRoom == room.AlternateVersion))
                        continue;
                    RectangleInt2 neighborArea = area + (room.SectorPos - neighborRoom.SectorPos);
                    if (!new RectangleInt2(0, 0, neighborRoom.NumXSectors - 1, neighborRoom.NumZSectors - 1).Contains(neighborArea))
                        continue;

                    // Check if they vertically touch
                    VerticalSpace? neighborVerticalSpaceLocal = neighborRoom.GetHeightInAreaMaxSpace(
                        new RectangleInt2(neighborArea.Start, neighborArea.End + new VectorInt2(1, 1)));
                    if (neighborVerticalSpaceLocal == null)
                        continue;
                    VerticalSpace neighborVerticalSpace = neighborVerticalSpaceLocal.Value + neighborRoom.Position.Y;
                    if (!((verticalSpace.FloorY <= neighborVerticalSpace.CeilingY) && (verticalSpace.CeilingY >= neighborVerticalSpace.FloorY)))
                        continue;

                    // Decide on a direction
                    if (couldBeFloorCeilingPortal &&
                        new RectangleInt2(1, 1, neighborRoom.NumXSectors - 2, neighborRoom.NumZSectors - 2).Contains(neighborArea))
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
                    if ((area.Width == 0) && (area.X0 == 0))
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallNegativeX, neighborRoom));
                    if ((area.Width == 0) && (area.X0 == (room.NumXSectors - 1)))
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallPositiveX, neighborRoom));
                    if ((area.Height == 0) && (area.Y0 == 0))
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallNegativeZ, neighborRoom));
                    if ((area.Height == 0) && (area.Y0 == (room.NumZSectors - 1)))
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

            // Create portals
            var portals = room.AddObject(_editor.Level, new PortalInstance(area, destinationDirection, destination)).Cast<PortalInstance>();

            // Update
            foreach (Room portalRoom in portals.Select(portal => portal.Room).Distinct())
                portalRoom.UpdateCompletely();
            foreach (PortalInstance portal in portals)
                _editor.ObjectChange(portal, ObjectChangeType.Add);

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
            newRoom.AlternateGroup = AlternateGroup;
            newRoom.AlternateBaseRoom = room;

            _editor.RoomPropertiesChange(room);
            _editor.RoomPropertiesChange(newRoom);
        }

        public static void AlternateRoomDisableWithWarning(Room room, IWin32Window owner)
        {
            room = room.AlternateBaseRoom ?? room;

            // Ask for confirmation
            if (DarkMessageBox.Show(owner, "Do you really want to delete the flip room?",
                "Delete flipped room", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            // Change room selection if necessary
            if (_editor.SelectedRoom == room.AlternateRoom)
                _editor.SelectedRoom = room;

            // Delete alternate room
            _editor.Level.DeleteAlternateRoom(room.AlternateRoom);
            room.AlternateRoom = null;
            room.AlternateGroup = -1;

            _editor.RoomListChange();
            _editor.RoomPropertiesChange(room);
        }

        public static void SmoothRandomFloor(Room room, RectangleInt2 area, float strengthDirection)
        {
            float[,] changes = new float[area.Width + 2, area.Height + 2];
            Random rng = new Random();
            for (int x = 1; x <= area.Width; x++)
                for (int z = 1; z <= area.Height; z++)
                    changes[x, z] = ((float)rng.NextDouble()) * strengthDirection;

            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (int i = 0; i < 4; ++i)
                        room.Blocks[area.X0 + x, area.Y0 + z].QA[i] +=
                            (short)Math.Round(changes[x + Block.FaceX[i], z + Block.FaceZ[i]]);

            SmartBuildGeometry(room, area);
        }

        public static void SmoothRandomCeiling(Room room, RectangleInt2 area, float strengthDirection)
        {
            float[,] changes = new float[area.Width + 2, area.Height + 2];
            Random rng = new Random();
            for (int x = 1; x <= area.Width; x++)
                for (int z = 1; z <= area.Height; z++)
                    changes[x, z] = ((float)rng.NextDouble()) * strengthDirection;

            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (int i = 0; i < 4; ++i)
                        room.Blocks[area.X0 + x, area.Y0 + z].WS[i] +=
                            (short)Math.Round(changes[x + Block.FaceX[i], z + Block.FaceZ[i]]);

            SmartBuildGeometry(room, area);
        }

        public static void SharpRandomFloor(Room room, RectangleInt2 area, float strengthDirection)
        {
            Random rng = new Random();
            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (int i = 0; i < 4; ++i)
                        room.Blocks[area.X0 + x, area.Y0 + z].QA[i] +=
                            (short)Math.Round(((float)rng.NextDouble()) * strengthDirection);

            SmartBuildGeometry(room, area);
        }

        public static void SharpRandomCeiling(Room room, RectangleInt2 area, float strengthDirection)
        {
            Random rng = new Random();
            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (int i = 0; i < 4; ++i)
                        room.Blocks[area.X0 + x, area.Y0 + z].WS[i] +=
                            (short)Math.Round(((float)rng.NextDouble()) * strengthDirection);

            SmartBuildGeometry(room, area);
        }

        public static void FlattenFloor(Room room, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Block b = room.Blocks[x, z];

                    short mean = (short)((b.QA[0] + b.QA[1] + b.QA[2] + b.QA[3]) / 4);

                    for (int i = 0; i < 4; i++)
                    {
                        room.Blocks[x, z].QA[i] = mean;
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void FlattenCeiling(Room room, RectangleInt2 area)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Block b = room.Blocks[x, z];

                    short mean = (short)((b.WS[0] + b.WS[1] + b.WS[2] + b.WS[3]) / 4);

                    for (int i = 0; i < 4; i++)
                    {
                        room.Blocks[x, z].WS[i] = mean;
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void GridWalls(Room room, RectangleInt2 area, bool fiveDivisions = false)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Block block = room.Blocks[x, z];
                    if (block.IsAnyWall)
                    {
                        int cornerToSkip = -1;

                        switch (block.FloorDiagonalSplit)
                        {
                            case DiagonalSplit.XnZn:
                                cornerToSkip = 1;
                                break;
                            case DiagonalSplit.XnZp:
                                cornerToSkip = 2;
                                break;
                            case DiagonalSplit.XpZn:
                                cornerToSkip = 0;
                                break;
                            case DiagonalSplit.XpZp:
                                cornerToSkip = 3;
                                break;
                        }

                        VerticalSpace?[] verticalAreas = new VerticalSpace?[4];
                        for (int i = 0; i < 4; ++i)
                            verticalAreas[i] = room.GetHeightAtPointMinSpace(x + Block.FaceX[i], z + Block.FaceZ[i]);
                        if (verticalAreas.Any(verticalArea => verticalArea.HasValue)) // We can only do it if there is information available
                            for (int i = 0; i < 4; ++i)
                            {
                                // Skip opposite diagonal step corner
                                if (i == cornerToSkip)
                                    continue;

                                // Use the closest available vertical area information and divide it equally
                                VerticalSpace verticalArea = verticalAreas[i] ?? verticalAreas[(i + 1) % 4] ?? verticalAreas[(i + 3) % 4] ?? verticalAreas[(i + 2) % 4].Value;
                                block.ED[i] = (short)Math.Round(fiveDivisions ? ((verticalArea.FloorY * 4.0f + verticalArea.CeilingY * 1.0f) / 5.0f) : verticalArea.FloorY);
                                block.QA[i] = (short)Math.Round(fiveDivisions ? ((verticalArea.FloorY * 3.0f + verticalArea.CeilingY * 2.0f) / 5.0f) : ((verticalArea.FloorY * 2.0f + verticalArea.CeilingY * 1.0f) / 3.0f));
                                block.WS[i] = (short)Math.Round(fiveDivisions ? ((verticalArea.FloorY * 2.0f + verticalArea.CeilingY * 3.0f) / 5.0f) : ((verticalArea.FloorY * 1.0f + verticalArea.CeilingY * 2.0f) / 3.0f));
                                block.RF[i] = (short)Math.Round(fiveDivisions ? ((verticalArea.FloorY * 1.0f + verticalArea.CeilingY * 4.0f) / 5.0f) : verticalArea.CeilingY);
                            }
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void CreateRoomAboveOrBelow(Room room, Func<Room, int> GetYOffset, short newRoomHeight)
        {
            // Create room
            var newRoom = new Room(room.NumXSectors, room.NumZSectors, "", newRoomHeight);
            newRoom.Position = room.Position + new VectorInt3(0, GetYOffset(newRoom), 0);
            newRoom.Name = "Room " + (newRoom.Position.Y > room.Position.Y ? "above " : "below ") + room.Name;
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

            RectangleInt2 area = _editor.SelectedSectors.Area.Inflate(1);
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
                selection.Area = selection.Area + new VectorInt2((int)(oldRoomPos.X - room.Position.X), (int)(oldRoomPos.Z - room.Position.Z));
                _editor.SelectedSectors = selection;
            }
        }

        public static void SelectConnectedRooms()
        {
            _editor.SelectRooms(_editor.Level.GetConnectedRooms(_editor.SelectedRooms).ToList());
        }

        public static void DuplicateRooms(IWin32Window owner)
        {
            var newRoom = _editor.SelectedRoom.Clone(_editor.Level);
            newRoom.Name = _editor.SelectedRoom.Name + " (copy)";
            newRoom.UpdateCompletely();
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

        public static void ApplyCurrentAmbientLightToAllRooms()
        {
            foreach (var room in _editor.Level.Rooms)
                if (room != null && room != _editor.SelectedRoom)
                {
                    room.AmbientLight = new Vector4(_editor.SelectedRoom.AmbientLight.X,
                                                    _editor.SelectedRoom.AmbientLight.Y,
                                                    _editor.SelectedRoom.AmbientLight.Z,
                                                    _editor.SelectedRoom.AmbientLight.W);
                    room.CalculateLightingForThisRoom();
                    room.UpdateBuffers();
                }
            Editor.Instance.RaiseEvent(new Editor.ModeChangedEvent());
        }

        public static bool BuildLevel(bool autoCloseWhenDone, IWin32Window owner)
        {
            Level level = _editor.Level;
            string fileName = level.Settings.MakeAbsolute(level.Settings.GameLevelFilePath);

            using (var form = new FormOperationDialog("Build *.tr4 level", autoCloseWhenDone,
                (progressReporter) =>
                {
                    var watch = new Stopwatch();
                    watch.Start();
                    LevelCompilerTr4 compiler = new LevelCompilerTr4(level, fileName, progressReporter);
                    LevelCompilerTr4.CompilerStatistics statistics = compiler.CompileLevel();
                    watch.Stop();
                    progressReporter.ReportProgress(100, "Elapsed time: " + watch.Elapsed.TotalMilliseconds + "ms");

                    // Raise an event for statistics update
                    Editor.Instance.RaiseEvent(new Editor.LevelCompilationCompletedEvent { InfoString = statistics.ToString() });

                    // Force garbage collector to compact memory
                    GC.Collect();
                }))
            {
                form.ShowDialog(owner);
                return form.DialogResult != DialogResult.Cancel;
            }
        }

        public static void BuildLevelAndPlay(IWin32Window owner)
        {
            if ((_editor?.Level?.Wad?.Moveables[0] != null) &&
                 _editor.Level.Rooms
                .Where(room => room != null)
                .SelectMany(room => room.Objects)
                .Any((obj) => (obj is ItemInstance) && ((ItemInstance)obj).ItemType == new ItemType(false, 0, _editor.Level.Wad.Version)))
            {
                if (BuildLevel(true, owner))
                    TombLauncher.Launch(_editor.Level.Settings, owner);
            }
            else
            {
                DarkMessageBox.Show(owner, "No Lara found. Place Lara to play level.", "No Lara", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

        }

        public static void LoadTextures(IWin32Window owner)
        {
            var settings = _editor.Level.Settings;
            string path = GraphicalDialogHandler.BrowseTextureFile(settings, settings.TextureFilePath, owner);
            if (settings.TextureFilePath == path)
                return;

            settings.TextureFilePath = path;
            _editor.LoadedTexturesChange();
        }

        public static void LoadWad(IWin32Window owner)
        {
            var settings = _editor.Level.Settings;
            string path = GraphicalDialogHandler.BrowseObjectFile(settings, settings.WadFilePath, owner);
            if (path == settings.WadFilePath)
                return;

            settings.WadFilePath = path;
            _editor.Level.ReloadWad(new GraphicalDialogHandler(owner));
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

        public static bool EnsureNoOutsidePortalsInSelecton(IWin32Window owner)
        {
            return Room.RemoveOutsidePortals(_editor.Level, _editor.SelectedRooms, (list) =>
            {
                StringBuilder portalsToRemoveList = list.Aggregate(new StringBuilder(), (str, room) => str.Append(room).Append("\n"), str => str.Remove(str.Length - 1, 1));
                return DarkMessageBox.Show(owner, "The rooms can't have portals to the outside. Do you want to continue by removing all portals to the outside? " +
                    " Portals to remove: " + portalsToRemoveList.ToString(),
                    "Outside portals", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes;
            });
        }

        public static bool TransformRooms(RectTransformation transformation, IWin32Window owner)
        {
            if (!EnsureNoOutsidePortalsInSelecton(owner))
                return false;
            var newRooms = _editor.Level.TransformRooms(_editor.SelectedRooms, transformation);
            foreach (Room room in newRooms)
                room.UpdateCompletely();
            _editor.SelectRoomsAndResetCamera(newRooms);

            _editor.RoomListChange();
            foreach (Room room in newRooms)
            {
                _editor.RoomGeometryChange(room);
                _editor.RoomSectorPropertiesChange(room);
            }
            return true;
        }

        public static void TryCopyObject(ObjectInstance instance, IWin32Window owner)
        {
            if (!(instance is PositionBasedObjectInstance))
            {
                DarkMessageBox.Show(owner, "You have to select an position based object before you can copy it.", "No object selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Clipboard.SetDataObject(new ObjectClipboardData(_editor));
        }

        public static void TryStampObject(ObjectInstance instance, IWin32Window owner)
        {
            if (!(instance is PositionBasedObjectInstance))
            {
                DarkMessageBox.Show(owner, "You have to select a position based object before you can stamp it.", "No object selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            _editor.Action = new EditorActionPlace(false, (level, room) => (PositionBasedObjectInstance)instance.Clone());
        }

        public static bool DragDropFileSupported(DragEventArgs e, bool allow3DImport = false)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return false;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                if (Wad2.WadFormatExtensions.Matches(file) ||
                    LevelTexture.FileExtensions.Matches(file) ||
                    (allow3DImport && ImportedGeometry.FileExtensions.Matches(file)) ||
                    file.EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase) ||
                    file.EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase))
                    return true;
            return false;
        }

        public static void MoveLara(IWin32Window owner, VectorInt2 p)
        {
            // Search for first Lara and remove her
            MoveableInstance lara;
            foreach (Room room in _editor.Level.Rooms.Where(room => room != null))
                foreach (var instance in room.Objects)
                {
                    lara = instance as MoveableInstance;
                    if ((lara != null) && (lara.WadObjectId == 0))
                    {
                        room.RemoveObject(_editor.Level, instance);
                        _editor.ObjectChange(lara, ObjectChangeType.Remove, room);
                        goto FoundLara;
                    }
                }
            lara = new MoveableInstance { WadObjectId = 0 }; // Lara
            FoundLara:

            // Add lara to current sector
            EditorActions.PlaceObject(_editor.SelectedRoom, p, lara);
        }

        public static int DragDropCommonFiles(DragEventArgs e, IWin32Window owner)
        {
            int unsupportedFileCount = 0;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (Wad2.WadFormatExtensions.Matches(file))
                    {
                        _editor.Level.Settings.WadFilePath = _editor.Level.Settings.MakeRelative(file, VariableType.LevelDirectory);
                        _editor.Level.ReloadWad(new GraphicalDialogHandler(owner));
                        _editor.LoadedWadsChange(_editor.Level.Wad);
                    }
                    else if (LevelTexture.FileExtensions.Matches(file))
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
            using (FormAnimatedTextures form = new FormAnimatedTextures(_editor))
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
            _editor.ObjectChange(portal, ObjectChangeType.Change);
        }

        public static bool SaveLevel(IWin32Window owner, bool askForPath)
        {
            string filePath = _editor.Level.Settings.LevelFilePath;

            // Show save dialog if necessary
            if (askForPath || string.IsNullOrEmpty(filePath))
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Tomb Editor Project (*.prj2)|*.prj2";
                    if (!string.IsNullOrEmpty(filePath))
                    {
                        saveFileDialog.InitialDirectory = Path.GetDirectoryName(filePath);
                        saveFileDialog.FileName = Path.GetFileName(filePath);
                    }
                    if (saveFileDialog.ShowDialog(owner) != DialogResult.OK)
                        return false;
                    filePath = saveFileDialog.FileName;
                }

            // Save level
            try
            {
                Prj2Writer.SaveToPrj2(filePath, _editor.Level);
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Unable to save to \"" + filePath + "\".");
                DarkMessageBox.Show(owner, "There was an error while saving project file. Exception: " + exc.Message, "Error", MessageBoxIcon.Error);
                return false;
            }

            // Update state
            _editor.HasUnsavedChanges = false;
            if (_editor.Level.Settings.LevelFilePath != filePath)
            {
                _editor.Level.Settings.LevelFilePath = filePath;
                _editor.LevelFileNameChange();
            }
            return true;
        }

        public static void ExportCurrentRoom(IWin32Window owner)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = "Export current room";
                saveFileDialog.Filter = BaseGeometryExporter.FileExtensions.GetFilter();
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = "obj";
                saveFileDialog.FileName = _editor.SelectedRoom.Name;

                if (saveFileDialog.ShowDialog(owner) == DialogResult.OK)
                {
                    using (var settingsDialog = new GeometryIOSettingsDialog(new IOGeometrySettings()))
                    {
                        settingsDialog.AddPreset(IOSettingsPresets.SettingsPresets);
                        string resultingExtension = Path.GetExtension(saveFileDialog.FileName).ToLowerInvariant();

                        if (resultingExtension.Equals(".mqo"))
                            settingsDialog.SelectPreset("Metasequoia MQO");

                        if (settingsDialog.ShowDialog(owner) == DialogResult.OK)
                        {
                            BaseGeometryExporter.GetTextureDelegate getTextureCallback = (txt) =>
                            {
                                if (txt is LevelTexture)
                                    return _editor.Level.Settings.MakeAbsolute(((LevelTexture)txt).Path);
                                else
                                    return "";
                            };

                            BaseGeometryExporter exporter;
                            switch (resultingExtension)
                            {
                                default:
                                case ".mqo":
                                    exporter = new RoomExporterMetasequoia(settingsDialog.Settings, getTextureCallback);
                                    break;
                                /*case ".obj":
                                    exporter = new RoomExporterObj(settingsDialog.Settings, getTextureCallback);
                                    break;
                                case ".ply":
                                    exporter = new RoomExporterPly(settingsDialog.Settings, getTextureCallback);
                                    break;
                                case ".dae":
                                    exporter = new RoomExporterCollada(settingsDialog.Settings, getTextureCallback);
                                    break;*/
                            }

                            // Prepare data for export
                            var model = new IOModel();
                            var mesh = new IOMesh();
                            var room = _editor.SelectedRoom;
                            var deltaPos = new Vector3(room.GetLocalCenter().X, 0, room.GetLocalCenter().Z);

                            var texture = _editor.Level.Settings.Textures[0];

                            // Create various materials
                            var materialOpaque = new IOMaterial("TeMat_0_0_0_0", texture, false, false, 0);
                            var materialOpaqueDoubleSided = new IOMaterial("TeMat_0_0_1_0", texture, false, true, 0);
                            var materialAdditiveBlending = new IOMaterial("TeMat_0_1_0_0", texture, true, false, 0);
                            var materialAdditiveBlendingDoubleSided = new IOMaterial("TeMat_0_1_1_0", texture, true, true, 0);

                            model.Materials.Add(materialOpaque);
                            model.Materials.Add(materialOpaqueDoubleSided);
                            model.Materials.Add(materialAdditiveBlending);
                            model.Materials.Add(materialAdditiveBlendingDoubleSided);

                            // Add submeshes
                            mesh.Submeshes.Add(materialOpaque, new IOSubmesh(materialOpaque));
                            mesh.Submeshes.Add(materialOpaqueDoubleSided, new IOSubmesh(materialOpaqueDoubleSided));
                            mesh.Submeshes.Add(materialAdditiveBlending, new IOSubmesh(materialAdditiveBlending));
                            mesh.Submeshes.Add(materialAdditiveBlendingDoubleSided, new IOSubmesh(materialAdditiveBlendingDoubleSided));

                            var vertices = room.GetRoomVertices();
                            var lastIndex = 0;
                            for (var z = 0; z < room.NumZSectors; z++)
                            {
                                for (var x = 0; x < room.NumXSectors; x++)
                                {
                                    for (var f = 0; f < 29; f++)
                                    {
                                        var textureArea = room.Blocks[x, z].GetFaceTexture((BlockFace)f);
                                        if (room.IsFaceDefined(x, z, (BlockFace)f) && !room.Blocks[x, z].GetFaceTexture((BlockFace)f).TextureIsInvisble &&
                                            !textureArea.TextureIsUnavailable)
                                        {
                                            var indices = room.GetFaceIndices(x, z, (BlockFace)f);
                                            var poly = new IOPolygon(indices.Count == 3 ? IOPolygonShape.Triangle : IOPolygonShape.Quad);
                                            for (var i = 0; i < indices.Count; i++)
                                            {
                                                poly.Indices.Add(lastIndex);
                                                lastIndex++;
                                            }

                                            // Get the right submesh
                                            var submesh = mesh.Submeshes[materialOpaque];
                                            if (textureArea.BlendMode == BlendMode.Additive)
                                            {
                                                if (textureArea.DoubleSided)
                                                    submesh = mesh.Submeshes[materialAdditiveBlendingDoubleSided];
                                                else
                                                    submesh = mesh.Submeshes[materialAdditiveBlending];
                                            }
                                            else
                                            {
                                                if (textureArea.DoubleSided)
                                                    submesh = mesh.Submeshes[materialOpaqueDoubleSided];
                                            }
                                                
                                            submesh.Polygons.Add(poly);

                                            foreach (var index in indices)
                                            {
                                                mesh.Positions.Add(vertices[index].Position - deltaPos);
                                                mesh.UV.Add(vertices[index].UV);
                                                mesh.Colors.Add(vertices[index].Color);
                                            }
                                        }
                                    }
                                }
                            }

                            //mesh.Texture = _editor.Level.Settings.Textures[0];
                            model.Meshes.Add(mesh);

                            if (exporter.ExportToFile(model, saveFileDialog.FileName))
                            {
                                DarkMessageBox.Show(owner, "Room exported correctly", "Information", MessageBoxButtons.OK,
                                                MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
        }

        public static void OpenLevel(IWin32Window owner, string fileName = null)
        {
            if (!ContinueOnFileDrop(owner, "Open level"))
                return;

            string _fileName = fileName;

            if (_fileName == null)
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
                newLevel = Prj2Loader.LoadFromPrj2(_fileName, new ProgressReporterSimple());
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
            if (!ContinueOnFileDrop(owner, "Open level"))
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

        public static void MoveRooms(VectorInt3 positionDelta, IEnumerable<Room> rooms)
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

        public static void SwitchMode(EditorMode mode)
        {
            _editor.Mode = mode;

            if (_editor.Configuration.Editor_DiscardSelectionOnModeSwitch)
                _editor.SelectedSectors = SectorSelection.None;
        }

        public static void SwitchTool(EditorTool tool)
        {
            _editor.Tool = tool;
        }
    }
}
