using DarkUI.Forms;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombEditor.Forms;
using TombLib;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.GeometryIO;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.LevelData.Compilers;
using TombLib.LevelData.IO;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor
{
    public static class EditorActions
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static readonly Editor _editor = Editor.Instance;

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
            var watch = new Stopwatch();
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

        public static void EditSectorGeometry(Room room, RectangleInt2 area, ArrowType arrow, BlockVertical vertical, short increment, bool smooth, bool oppositeDiagonalCorner = false, bool autoSwitchDiagonals = false, bool autoUpdateThroughPortal = true, bool disableUndo = false)
        {
            if(!disableUndo)
            {
                if(smooth)
                    _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom.AndAdjoiningRooms);
                else
                    _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);
            }

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

                VectorInt2 startCoord = area.Start;
                bool[] corners = new bool[4] { true, true, true, true };

                // Adjust editing area to exclude the side on which the arrow starts
                // This is a superset of the behaviour of the old editor to smooth edit a single edge or side.
                switch (arrow)
                {
                    case ArrowType.EdgeE:
                        area = new RectangleInt2(area.X0 + 1, area.Y0, area.X1, area.Y1);
                        break;
                    case ArrowType.EdgeN:
                        area = new RectangleInt2(area.X0, area.Y0 + 1, area.X1, area.Y1);
                        break;
                    case ArrowType.EdgeW:
                        area = new RectangleInt2(area.X0, area.Y0, area.X1 - 1, area.Y1);
                        break;
                    case ArrowType.EdgeS:
                        area = new RectangleInt2(area.X0, area.Y0, area.X1, area.Y1 - 1);
                        break;
                    case ArrowType.CornerNE:
                        area = new RectangleInt2(area.X0 + 1, area.Y0 + 1, area.X1, area.Y1);
                        break;
                    case ArrowType.CornerNW:
                        area = new RectangleInt2(area.X0, area.Y0 + 1, area.X1 - 1, area.Y1);
                        break;
                    case ArrowType.CornerSW:
                        area = new RectangleInt2(area.X0, area.Y0, area.X1 - 1, area.Y1 - 1);
                        break;
                    case ArrowType.CornerSE:
                        area = new RectangleInt2(area.X0 + 1, area.Y0, area.X1, area.Y1 - 1);
                        break;
                }

                Action<Block, BlockEdge> smoothEdit = (Block block, BlockEdge edge) =>
                {
                    if (block == null) return;

                    if (vertical.IsOnFloor()   && block.Floor.DiagonalSplit   == DiagonalSplit.None ||
                        vertical.IsOnCeiling() && block.Ceiling.DiagonalSplit == DiagonalSplit.None)
                    {
                        if (smoothEditingType == SmoothGeometryEditingType.Any ||
                           !block.IsAnyWall && smoothEditingType == SmoothGeometryEditingType.Floor ||
                            block.IsAnyWall && smoothEditingType == SmoothGeometryEditingType.Wall)
                        {
                            block.ChangeHeight(vertical, edge, increment);
                            block.FixHeights(vertical);
                        }
                    }
                };

                var cornerBlocks = new RoomBlockPair[4]
                {
                    room.GetBlockTryThroughPortal(area.X1 + 1, area.Y0 - 1),
                    room.GetBlockTryThroughPortal(area.X0 - 1, area.Y0 - 1),
                    room.GetBlockTryThroughPortal(area.X0 - 1, area.Y1 + 1),
                    room.GetBlockTryThroughPortal(area.X1 + 1, area.Y1 + 1)
                };

                // Unique case of editing single corner
                if(area.Width == -1 && area.Height == -1 && arrow > ArrowType.EdgeW)
                {
                    BlockEdge origin = BlockEdge.XnZn;
                    switch(arrow)
                    {
                        case ArrowType.CornerNE: origin = BlockEdge.XpZp; break;
                        case ArrowType.CornerNW: origin = BlockEdge.XnZp; break;
                        case ArrowType.CornerSE: origin = BlockEdge.XpZn; break;
                    }
                    var originBlock = room.GetBlockTryThroughPortal(startCoord);
                    var originHeight = originBlock.Block.GetHeight(vertical, origin) + originBlock.Room.Position.Y;
                    for (int i = 0; i < 4; i++)
                        corners[i] = originHeight == cornerBlocks[i].Block.GetHeight(vertical, (BlockEdge)i) + cornerBlocks[i].Room.Position.Y;
                }

                // Smoothly change sectors on the corners
                for (int i = 0; i < 4; i++)
                    if (corners[i]) smoothEdit(cornerBlocks[i].Block, (BlockEdge)i);

                // Smoothly change sectors on the sides
                for (int x = area.X0; x <= area.X1; x++)
                {
                    smoothEdit(room.GetBlockTryThroughPortal(x, area.Y0 - 1).Block, BlockEdge.XnZp);
                    smoothEdit(room.GetBlockTryThroughPortal(x, area.Y0 - 1).Block, BlockEdge.XpZp);
                    
                    smoothEdit(room.GetBlockTryThroughPortal(x, area.Y1 + 1).Block, BlockEdge.XnZn);
                    smoothEdit(room.GetBlockTryThroughPortal(x, area.Y1 + 1).Block, BlockEdge.XpZn);
                }

                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    smoothEdit(room.GetBlockTryThroughPortal(area.X0 - 1, z).Block, BlockEdge.XpZp);
                    smoothEdit(room.GetBlockTryThroughPortal(area.X0 - 1, z).Block, BlockEdge.XpZn);

                    smoothEdit(room.GetBlockTryThroughPortal(area.X1 + 1, z).Block, BlockEdge.XnZp);
                    smoothEdit(room.GetBlockTryThroughPortal(area.X1 + 1, z).Block, BlockEdge.XnZn);
                }

                arrow = ArrowType.EntireFace;
            }

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    var block = room.Blocks[x, z];
                    var lookupBlock = room.GetBlockTryThroughPortal(x, z);

                    EditBlock:
                    {
                        if (arrow == ArrowType.EntireFace)
                        {
                            if (vertical == BlockVertical.Floor || vertical == BlockVertical.Ceiling)
                                block.RaiseStepWise(vertical, oppositeDiagonalCorner, increment, autoSwitchDiagonals);
                            else
                                block.Raise(vertical, increment, false);
                        }
                        else
                        {
                            var currentSplit = vertical.IsOnFloor() ? block.Floor.DiagonalSplit : block.Ceiling.DiagonalSplit;
                            var incrementInvalid = vertical.IsOnFloor() ? increment < 0 : increment > 0;
                            BlockEdge[] corners = new BlockEdge[2] { BlockEdge.XnZp, BlockEdge.XnZp };
                            DiagonalSplit[] splits = new DiagonalSplit[2] { DiagonalSplit.None, DiagonalSplit.None };

                            switch (arrow)
                            {
                                case ArrowType.EdgeN:
                                case ArrowType.CornerNW:
                                    corners[0] = BlockEdge.XnZp;
                                    corners[1] = BlockEdge.XpZp;
                                    splits[0] = DiagonalSplit.XpZn;
                                    splits[1] = arrow == ArrowType.CornerNW ? DiagonalSplit.XnZp : DiagonalSplit.XnZn;
                                    break;
                                case ArrowType.EdgeE:
                                case ArrowType.CornerNE:
                                    corners[0] = BlockEdge.XpZp;
                                    corners[1] = BlockEdge.XpZn;
                                    splits[0] = DiagonalSplit.XnZn;
                                    splits[1] = arrow == ArrowType.CornerNE ? DiagonalSplit.XpZp : DiagonalSplit.XnZp;
                                    break;
                                case ArrowType.EdgeS:
                                case ArrowType.CornerSE:
                                    corners[0] = BlockEdge.XpZn;
                                    corners[1] = BlockEdge.XnZn;
                                    splits[0] = DiagonalSplit.XnZp;
                                    splits[1] = arrow == ArrowType.CornerSE ? DiagonalSplit.XpZn : DiagonalSplit.XpZp;
                                    break;
                                case ArrowType.EdgeW:
                                case ArrowType.CornerSW:
                                    corners[0] = BlockEdge.XnZn;
                                    corners[1] = BlockEdge.XnZp;
                                    splits[0] = DiagonalSplit.XpZp;
                                    splits[1] = arrow == ArrowType.CornerSW ? DiagonalSplit.XnZn : DiagonalSplit.XpZn;
                                    break;
                            }

                            if (arrow <= ArrowType.EdgeW)
                            {
                                if (block.Type != BlockType.Wall && currentSplit != DiagonalSplit.None)
                                    continue;
                                for (int i = 0; i < 2; i++)
                                    if (currentSplit != splits[i])
                                        block.ChangeHeight(vertical, corners[i], increment);
                            }
                            else
                            {
                                if (block.Type != BlockType.Wall && currentSplit != DiagonalSplit.None)
                                {
                                    if (currentSplit == splits[1])
                                    {
                                        if (block.GetHeight(vertical, corners[0]) == block.GetHeight(vertical, corners[1]) && incrementInvalid)
                                            continue;
                                    }
                                    else if (autoSwitchDiagonals && currentSplit == splits[0] && block.GetHeight(vertical, corners[0]) == block.GetHeight(vertical, corners[1]) && !incrementInvalid)
                                        block.Transform(new RectTransformation { QuadrantRotation = 2 }, vertical.IsOnFloor());
                                    else
                                        continue;
                                }
                                block.ChangeHeight(vertical, corners[0], increment);
                            }
                        }
                        block.FixHeights(vertical);
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
                if (axis == RotationAxis.X || axis == RotationAxis.None)
                    (_editor.SelectedObject as IRotateableYX).RotationX = 0;
            }

            if (_editor.SelectedObject is IRotateableY)
            {
                if (axis == RotationAxis.Y || axis == RotationAxis.None)
                    (_editor.SelectedObject as IRotateableY).RotationY = 0;
            }

            if (_editor.SelectedObject is IRotateableYXRoll)
            {
                if (axis == RotationAxis.Roll || axis == RotationAxis.None)
                    (_editor.SelectedObject as IRotateableYXRoll).Roll = 0;
            }

            _editor.ObjectChange(_editor.SelectedObject, ObjectChangeType.Change);
        }

        public static void SmoothSector(Room room, int x, int z, BlockVertical vertical, bool disableUndo = false)
        {
            var currBlock = room.GetBlockTryThroughPortal(x, z);

            if (currBlock.Room != room ||
                vertical.IsOnFloor() && currBlock.Block.Floor.DiagonalSplit != DiagonalSplit.None ||
                vertical.IsOnCeiling() && currBlock.Block.Ceiling.DiagonalSplit != DiagonalSplit.None)
                return;

            if (!disableUndo)
                _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            var lookupBlocks = new RoomBlockPair[8]
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
                adj[i] = (currBlock.Room != null ? currBlock.Room.Position.Y : 0) - (lookupBlocks[i].Room != null ? lookupBlocks[i].Room.Position.Y : 0);

            int validBlockCntXnZp = (lookupBlocks[7].Room != null ? 1 : 0) + (lookupBlocks[0].Room != null ? 1 : 0) + (lookupBlocks[1].Room != null ? 1 : 0);
            int newXnZp = ((lookupBlocks[7].Block?.GetHeight(vertical, BlockEdge.XpZp) ?? 0) + adj[7] +
                                   (lookupBlocks[0].Block?.GetHeight(vertical, BlockEdge.XpZn) ?? 0) + adj[0] +
                                   (lookupBlocks[1].Block?.GetHeight(vertical, BlockEdge.XnZn) ?? 0) + adj[1]) / validBlockCntXnZp;

            int validBlockCntXpZp = (lookupBlocks[1].Room != null ? 1 : 0) + (lookupBlocks[2].Room != null ? 1 : 0) + (lookupBlocks[3].Room != null ? 1 : 0);
            int newXpZp = ((lookupBlocks[1].Block?.GetHeight(vertical, BlockEdge.XpZn) ?? 0) + adj[2] +
                                   (lookupBlocks[2].Block?.GetHeight(vertical, BlockEdge.XnZn) ?? 0) + adj[3] +
                                   (lookupBlocks[3].Block?.GetHeight(vertical, BlockEdge.XnZp) ?? 0) + adj[0]) / validBlockCntXpZp;

            int validBlockCntXpZn = (lookupBlocks[3].Room != null ? 1 : 0) + (lookupBlocks[4].Room != null ? 1 : 0) + (lookupBlocks[5].Room != null ? 1 : 0);
            int newXpZn = ((lookupBlocks[3].Block?.GetHeight(vertical, BlockEdge.XnZn) ?? 0) + adj[3] +
                                   (lookupBlocks[4].Block?.GetHeight(vertical, BlockEdge.XnZp) ?? 0) + adj[0] +
                                   (lookupBlocks[5].Block?.GetHeight(vertical, BlockEdge.XpZp) ?? 0) + adj[1]) / validBlockCntXpZn;

            int validBlockCntXnZn = (lookupBlocks[5].Room != null ? 1 : 0) + (lookupBlocks[6].Room != null ? 1 : 0) + (lookupBlocks[7].Room != null ? 1 : 0);
            int newXnZn = ((lookupBlocks[5].Block?.GetHeight(vertical, BlockEdge.XnZp) ?? 0) + adj[0] +
                                   (lookupBlocks[6].Block?.GetHeight(vertical, BlockEdge.XpZp) ?? 0) + adj[1] +
                                   (lookupBlocks[7].Block?.GetHeight(vertical, BlockEdge.XpZn) ?? 0) + adj[2]) / validBlockCntXnZn;

            currBlock.Block.ChangeHeight(vertical, BlockEdge.XnZp, Math.Sign(newXnZp - currBlock.Block.GetHeight(vertical, BlockEdge.XnZp)));
            currBlock.Block.ChangeHeight(vertical, BlockEdge.XpZp, Math.Sign(newXpZp - currBlock.Block.GetHeight(vertical, BlockEdge.XpZp)));
            currBlock.Block.ChangeHeight(vertical, BlockEdge.XpZn, Math.Sign(newXpZn - currBlock.Block.GetHeight(vertical, BlockEdge.XpZn)));
            currBlock.Block.ChangeHeight(vertical, BlockEdge.XnZn, Math.Sign(newXnZn - currBlock.Block.GetHeight(vertical, BlockEdge.XnZn)));

            SmartBuildGeometry(room, new RectangleInt2(x, z, x, z));
        }

        public static void ShapeGroup(Room room, RectangleInt2 area, ArrowType arrow, EditorToolType type, BlockVertical vertical, double heightScale, bool precise, bool stepped)
        {
            if (precise)
                heightScale /= 4;

            bool linearShape = type <= EditorToolType.HalfPipe;
            bool uniformShape = type >= EditorToolType.HalfPipe;
            bool step90 = arrow <= ArrowType.EdgeW;
            bool turn90 = arrow == ArrowType.EdgeW || arrow == ArrowType.EdgeE;
            bool reverseX = (arrow == ArrowType.EdgeW || arrow == ArrowType.CornerSW || arrow == ArrowType.CornerNW) ^ uniformShape;
            bool reverseZ = (arrow == ArrowType.EdgeS || arrow == ArrowType.CornerSW || arrow == ArrowType.CornerSE) ^ uniformShape;
            bool uniformAlign = arrow != ArrowType.EntireFace && type > EditorToolType.HalfPipe && step90;

            double sizeX = area.Width + (stepped ? 0 : 1);
            double sizeZ = area.Height + (stepped ? 0 : 1);
            double grainBias = uniformShape ? (!step90 ? 0 : 1) : 0;
            double grainX = (1 + grainBias) / sizeX / (uniformAlign && turn90 ? 2 : 1);
            double grainZ = (1 + grainBias) / sizeZ / (uniformAlign && !turn90 ? 2 : 1);

            for (int w = area.X0, x = 0; w < area.X0 + sizeX + 1; w++, x++)
                for (int h = area.Y0, z = 0; h != area.Y0 + sizeZ + 1; h++, z++)
                {
                    double currentHeight;
                    double currX = linearShape && !turn90 && step90 ? 0 : grainX * (reverseX ? sizeX - x : x) - (uniformAlign && turn90 ? 0 : grainBias);
                    double currZ = linearShape && turn90 && step90 ? 0 : grainZ * (reverseZ ? sizeZ - z : z) - (uniformAlign && !turn90 ? 0 : grainBias);

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
                        room.Blocks[w, h].Raise(vertical, (int)currentHeight, false);
                        room.Blocks[w, h].FixHeights();
                    }
                    else
                        room.ModifyHeightThroughPortal(w, h, vertical, (int)currentHeight, area);
                }
            SmartBuildGeometry(room, area);
        }

        public static void ApplyHeightmap(Room room, RectangleInt2 area, ArrowType arrow, BlockVertical vertical, float[,] heightmap, float heightScale, bool precise, bool raw)
        {
            if (precise)
                heightScale /= 4;

            bool allFace = arrow == ArrowType.EntireFace;
            bool step90 = arrow <= ArrowType.EdgeW;
            bool turn90 = arrow == ArrowType.EdgeW || arrow == ArrowType.EdgeE;
            bool reverseX = arrow == ArrowType.EdgeW || arrow == ArrowType.CornerSW || arrow == ArrowType.CornerNW;
            bool reverseZ = arrow == ArrowType.EdgeS || arrow == ArrowType.CornerSW || arrow == ArrowType.CornerSE;

            float smoothGrainX = (float)(allFace || (step90 && !turn90) ? Math.PI : Math.PI * 0.5f) / (area.Width + 1);
            float smoothGrainZ = (float)(allFace || (step90 &&  turn90) ? Math.PI : Math.PI * 0.5f) / (area.Height + 1);

            for (int w = area.X0, x = 0; w < area.X1 + 2; w++, x++)
                for (int h = area.Y0, z = 0; h != area.Y1 + 2; h++, z++)
                {
                    var smoothFactor = raw ? 1 : Math.Sin(smoothGrainX * (reverseX ? area.Width + 1 - x : x)) * Math.Sin(smoothGrainZ * (reverseZ ? area.Height + 1 - z : z));

                    int currX = x * (heightmap.GetLength(0) / (area.Width + 2));
                    int currZ = z * (heightmap.GetLength(1) / (area.Height + 2));
                    room.ModifyHeightThroughPortal(w, h, vertical, (int)Math.Round(heightmap[currX, currZ] * smoothFactor * heightScale), area);
                }
            SmartBuildGeometry(room, area);
        }

        public static void FlipFloorSplit(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    if (!room.Blocks[x, z].Floor.IsQuad && room.Blocks[x, z].Floor.DiagonalSplit == DiagonalSplit.None)
                        room.Blocks[x, z].Floor.SplitDirectionToggled = !room.Blocks[x, z].Floor.SplitDirectionToggled;

            SmartBuildGeometry(room, area);
        }

        public static void FlipCeilingSplit(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    if (!room.Blocks[x, z].Ceiling.IsQuad && room.Blocks[x, z].Ceiling.DiagonalSplit == DiagonalSplit.None)
                        room.Blocks[x, z].Ceiling.SplitDirectionToggled = !room.Blocks[x, z].Ceiling.SplitDirectionToggled;

            SmartBuildGeometry(room, area);
        }

        public static void AddTrigger(Room room, RectangleInt2 area, IWin32Window owner)
        {
            if (Control.ModifierKeys.HasFlag(Keys.Shift) || _editor.SelectedObject == null || !(_editor.SelectedObject is PositionBasedObjectInstance))
                AddTrigger(room, area, owner, _editor.BookmarkedObject);
            else
                AddTrigger(room, area, owner, _editor.SelectedObject);
        }

        public static void AddTrigger(Room room, RectangleInt2 area, IWin32Window owner, ObjectInstance @object)
        {
            // Initialize trigger with selected object if the selected object makes sense in the trigger context.
            var trigger = new TriggerInstance(area);
            if (@object is MoveableInstance)
            {
                trigger.TargetType = TriggerTargetType.Object;
                trigger.Target = @object;
            }
            else if (@object is FlybyCameraInstance)
            {
                trigger.TargetType = TriggerTargetType.FlyByCamera;
                trigger.Target = @object;
            }
            else if (@object is CameraInstance)
            {
                trigger.TargetType = TriggerTargetType.Camera;
                trigger.Target = @object;
            }
            else if (@object is SinkInstance)
            {
                trigger.TargetType = TriggerTargetType.Sink;
                trigger.Target = @object;
            }
            else if (@object is StaticInstance && _editor.Level.Settings.GameVersion == GameVersion.TRNG)
            {
                trigger.TargetType = TriggerTargetType.FlipEffect;
                trigger.Target = new TriggerParameterUshort(160);
                trigger.Timer = @object;
            }

            // Display form
            using (var formTrigger = new FormTrigger(_editor.Level, trigger, obj => _editor.ShowObject(obj),
                                                     r => _editor.SelectRoom(r)))
            {
                if (formTrigger.ShowDialog(owner) != DialogResult.OK)
                    return;
            }
            room.AddObject(_editor.Level, trigger);
            _editor.ObjectChange(trigger, ObjectChangeType.Add);
            _editor.RoomSectorPropertiesChange(room);

            // Undo
            _editor.UndoManager.PushSectorObjectCreated(trigger);

            //if (_editor.Configuration.UI_AutoSwitchSectorColoringInfo)
            //    _editor.SectorColoringManager.SetPriority(SectorColoringType.Trigger);
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
            if (newScale < 1 / 64.0f)
                newScale = 1 / 64.0f;
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
            if (precision.X > 0.0f && pos.X != instance.Position.X)
                pos.X = (float)Math.Round(pos.X / precision.X) * precision.X;
            if (precision.Y > 0.0f && pos.Y != instance.Position.Y)
                pos.Y = (float)Math.Round(pos.Y / precision.Y) * precision.Y;
            if (precision.Z > 0.0f && pos.Z != instance.Position.Z)
                pos.Z = (float)Math.Round(pos.Z / precision.Z) * precision.Z;

            // Limit movement area
            if (!canGoOutsideRoom)
            {
                float x = (float)Math.Floor(pos.X / 1024.0f);
                float z = (float)Math.Floor(pos.Z / 1024.0f);

                if (x < 0.0f || x > instance.Room.NumXSectors - 1 ||
                    z < 0.0f || z > instance.Room.NumZSectors - 1)
                    return;

                Block block = instance.Room.Blocks[(int)x, (int)z];
                if (block.IsAnyWall)
                    return;
            }

            // Update position
            instance.Position = pos;

            // Update state
            if (instance is LightInstance)
                instance.Room.RoomGeometry?.Relight(instance.Room);
            _editor.ObjectChange(instance, ObjectChangeType.Change);
        }

        public static void MoveObjectRelative(PositionBasedObjectInstance instance, Vector3 pos, Vector3 precision = new Vector3(), bool canGoOutsideRoom = false)
        {
            _editor.UndoManager.PushObjectTransformed(instance);
            MoveObject(instance, instance.Position + pos, precision, canGoOutsideRoom);
        }

        public static void MoveObjectToOtherRoom(PositionBasedObjectInstance instance, Room newRoom)
        {
            if (instance == null)
                return;

            if(instance.Room == null || !instance.Room.ExistsInLevel || newRoom == null || !newRoom.ExistsInLevel)
            {
                _editor.SendMessage("Can't move object from or to non-existent room", PopupType.Error);
                return;
            }

            if (instance.Room == newRoom)
            {
                _editor.SendMessage("Object is already in current room", PopupType.Info);
                return;
            }

            _editor.UndoManager.PushObjectTransformed(instance);
            newRoom.MoveObjectFrom(_editor.Level, instance.Room, instance);

            // Update state
            if (instance is LightInstance)
                instance.Room.RoomGeometry?.Relight(instance.Room);
            _editor.ObjectChange(instance, ObjectChangeType.Change);
        }

        public static void RotateObject(ObjectInstance instance, RotationAxis axis, float angleInDegrees, float quantization = 0.0f, bool delta = true, bool disableUndo = false)
        {
            if(!disableUndo && instance is PositionBasedObjectInstance)
                _editor.UndoManager.PushObjectTransformed((PositionBasedObjectInstance)instance);

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
                instance.Room.BuildGeometry();
            _editor.ObjectChange(instance, ObjectChangeType.Change);
        }

        public static void EditObject(ObjectInstance instance, IWin32Window owner)
        {
            if (instance is MoveableInstance)
            {
                using (var formMoveable = new FormMoveable((MoveableInstance)instance))
                    if (formMoveable.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is StaticInstance)
            {
                // Use static editing dialog only for NG levels for now
                if (_editor.Level.Settings.GameVersion != GameVersion.TRNG)
                    return;

                using (var formStaticMesh = new FormStaticMesh((StaticInstance)instance))
                    if (formStaticMesh.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is FlybyCameraInstance)
            {
                using (var formFlyby = new FormFlybyCamera((FlybyCameraInstance)instance))
                    if (formFlyby.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is CameraInstance)
            {
                using (var formCamera = new FormCamera((CameraInstance)instance))
                    if (formCamera.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is SinkInstance)
            {
                using (var formSink = new FormSink((SinkInstance)instance))
                    if (formSink.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is SoundSourceInstance)
            {
                using (var formSoundSource = new FormSoundSource((SoundSourceInstance)instance, _editor.Level.Settings.WadGetAllSoundInfos()))
                    if (formSoundSource.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is TriggerInstance)
            {
                using (var formTrigger = new FormTrigger(_editor.Level, (TriggerInstance)instance, obj => _editor.ShowObject(obj),
                                                         r => _editor.SelectRoom(r)))
                    if (formTrigger.ShowDialog(owner) != DialogResult.OK)
                        return;
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is ImportedGeometryInstance)
            {
                using (var formImportedGeometry = new FormImportedGeometry((ImportedGeometryInstance)instance, _editor.Level.Settings))
                {
                    if (formImportedGeometry.ShowDialog(owner) != DialogResult.OK)
                        return;
                    _editor.UpdateLevelSettings(formImportedGeometry.NewLevelSettings);
                }
                _editor.ObjectChange(instance, ObjectChangeType.Change);
            }
            else if (instance is LightInstance)
                EditLightColor(owner);
        }

        public static void PasteObject(VectorInt2 pos, Room room)
        {
            ObjectClipboardData data = Clipboard.GetDataObject().GetData(typeof(ObjectClipboardData)) as ObjectClipboardData;
            if (data == null)
                _editor.SendMessage("Clipboard contains no object data.", PopupType.Error);
            else
            {
                PositionBasedObjectInstance instance = data.MergeGetSingleObject(_editor);

                // HACK: fix imported geometry reference
                if (instance is ImportedGeometryInstance)
                {
                    var imported = instance as ImportedGeometryInstance;
                    var pastedPath = _editor.Level.Settings.MakeAbsolute(imported.Model.Info.Path);
                    foreach (var model in _editor.Level.Settings.ImportedGeometries)
                    {
                        var currentPath = _editor.Level.Settings.MakeAbsolute(model.Info.Path);
                        if (currentPath == pastedPath)
                        {
                            imported.Model = model;
                            break;
                        }
                    }
                }

                PlaceObject(room, pos, instance);
            }
        }

        public static void DeleteObject(ObjectInstance instance, IWin32Window owner = null)
        {
            // No owner = silent mode!
            if (owner != null && DarkMessageBox.Show(owner, "Do you really want to delete " + instance + "?",
                                 "Confirm delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            if (instance is PositionBasedObjectInstance)
                _editor.UndoManager.PushObjectDeleted((PositionBasedObjectInstance)instance);

            DeleteObjectWithoutUpdate(instance);
        }

        public static void DeleteObjectWithoutUpdate(ObjectInstance instance)
        {
            var room = instance.Room;
            var adjoiningRoom = (instance as PortalInstance)?.AdjoiningRoom;
            var isTriggerableObject = instance is MoveableInstance || instance is StaticInstance || instance is CameraInstance ||
                                      instance is FlybyCameraInstance || instance is SinkInstance || instance is SoundSourceInstance;
            room.RemoveObject(_editor.Level, instance);

            // Delete trigger if is necessary
            if (isTriggerableObject)
            {
                var triggersToRemove = new List<TriggerInstance>();
                foreach (var r in _editor.Level.Rooms)
                    if (r != null)
                        foreach (var trigger in r.Triggers)
                        {
                            if (trigger.Target == instance)
                                triggersToRemove.Add(trigger);
                        }

                foreach (var t in triggersToRemove)
                    t.Room.RemoveObject(_editor.Level, t);
            }

            // Additional updates
            if (instance is SectorBasedObjectInstance)
                _editor.RoomSectorPropertiesChange(room);
            if (instance is LightInstance)
            {
                room.BuildGeometry();
                _editor.RoomGeometryChange(room);
            }
            if (instance is PortalInstance)
            {
                room.BuildGeometry();
                if (adjoiningRoom != null)
                {
                    adjoiningRoom.BuildGeometry();
                    _editor.RoomSectorPropertiesChange(adjoiningRoom);

                    if (adjoiningRoom.AlternateOpposite != null)
                    {
                        adjoiningRoom.AlternateOpposite.BuildGeometry();
                        _editor.RoomSectorPropertiesChange(adjoiningRoom.AlternateOpposite);
                    }
                }
                if (room.AlternateOpposite != null)
                {
                    room.AlternateOpposite.BuildGeometry();
                    _editor.RoomSectorPropertiesChange(room.AlternateOpposite);
                }
            }

            // Avoid having the removed object still selected
            _editor.ObjectChange(instance, ObjectChangeType.Remove, room);
        }

        public static void RotateTexture(Room room, VectorInt2 pos, BlockFace face, bool fullFace = false)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            Block block = room.GetBlock(pos);
            TextureArea newTexture = block.GetFaceTexture(face);
            bool isTriangle = room.GetFaceShape(pos.X, pos.Y, face) == BlockFaceShape.Triangle;
        
            if(fullFace && isTriangle && face >= BlockFace.Floor)
            {
                if(newTexture.TextureIsTriangle) newTexture = newTexture.RestoreQuad();
                BlockFace opposite = BlockFace.Floor;
                int rotation = 1;

                switch (face)
                {
                    case BlockFace.Floor:
                        opposite = BlockFace.FloorTriangle2;
                        rotation += block.Floor.SplitDirectionIsXEqualsZ ? 2 : 1;
                        break;
                    case BlockFace.FloorTriangle2:
                        opposite = BlockFace.Floor;
                        rotation += block.Floor.SplitDirectionIsXEqualsZ ? 0 : 3;
                        break;
                    case BlockFace.Ceiling:
                        opposite = BlockFace.CeilingTriangle2;
                        rotation += block.Ceiling.SplitDirectionIsXEqualsZ ? 2 : 1;
                        break;
                    case BlockFace.CeilingTriangle2:
                        opposite = BlockFace.Ceiling;
                        rotation += block.Ceiling.SplitDirectionIsXEqualsZ ? 0 : 3;
                        break;
                }

                newTexture.Rotate(rotation, false);
                if (!block.GetFaceTexture(face).TextureIsInvisible) ApplyTextureWithoutUpdate(room, pos, face, newTexture);
                if (!block.GetFaceTexture(opposite).TextureIsInvisible) ApplyTextureWithoutUpdate(room, pos, opposite, newTexture);
            }
            else
            {
                newTexture.Rotate(1, isTriangle);
                block.SetFaceTexture(face, newTexture);
            }

            // Update state
            room.BuildGeometry();
            _editor.RoomTextureChange(room);
        }

        public static void MirrorTexture(Room room, VectorInt2 pos, BlockFace face)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            Block blocks = room.GetBlock(pos);

            TextureArea newTexture = blocks.GetFaceTexture(face);
            newTexture.Mirror(room.GetFaceShape(pos.X, pos.Y, face) == BlockFaceShape.Triangle);
            blocks.SetFaceTexture(face, newTexture);

            // Update state
            room.BuildGeometry();
            _editor.RoomTextureChange(room);
        }

        public static void PickTexture(Room room, VectorInt2 pos, BlockFace face)
        {
            var area = room.GetBlock(pos).GetFaceTexture(face);

            if (area == null || area.TextureIsUnavailable)
                return;
            else if (area.TextureIsInvisible)
                _editor.SelectedTexture = TextureArea.Invisible;
            else
            {
                if (face >= BlockFace.Ceiling) area.Mirror();
                _editor.SelectTextureAndCenterView(area.RestoreQuad());
            }
        }

        private static void FixTextureCoordinates(ref TextureArea texture)
        {
            texture.TexCoord0.X = Math.Max(0.0f, texture.TexCoord0.X);
            texture.TexCoord0.Y = Math.Max(0.0f, texture.TexCoord0.Y);
            texture.TexCoord1.X = Math.Max(0.0f, texture.TexCoord1.X);
            texture.TexCoord1.Y = Math.Max(0.0f, texture.TexCoord1.Y);
            texture.TexCoord2.X = Math.Max(0.0f, texture.TexCoord2.X);
            texture.TexCoord2.Y = Math.Max(0.0f, texture.TexCoord2.Y);
            texture.TexCoord3.X = Math.Max(0.0f, texture.TexCoord3.X);
            texture.TexCoord3.Y = Math.Max(0.0f, texture.TexCoord3.Y);

            if (!texture.TextureIsInvisible && !texture.TextureIsUnavailable)
            {
                texture.TexCoord0.X = Math.Min(texture.Texture.Image.Width - 1, texture.TexCoord0.X);
                texture.TexCoord0.Y = Math.Min(texture.Texture.Image.Height - 1, texture.TexCoord0.Y);
                texture.TexCoord1.X = Math.Min(texture.Texture.Image.Width - 1, texture.TexCoord1.X);
                texture.TexCoord1.Y = Math.Min(texture.Texture.Image.Height - 1, texture.TexCoord1.Y);
                texture.TexCoord2.X = Math.Min(texture.Texture.Image.Width - 1, texture.TexCoord2.X);
                texture.TexCoord2.Y = Math.Min(texture.Texture.Image.Height - 1, texture.TexCoord2.Y);
                texture.TexCoord3.X = Math.Min(texture.Texture.Image.Width - 1, texture.TexCoord3.X);
                texture.TexCoord3.Y = Math.Min(texture.Texture.Image.Height - 1, texture.TexCoord3.Y);
            }
        }

        private static bool ApplyTextureWithoutUpdate(Room room, VectorInt2 pos, BlockFace face, TextureArea texture)
        {
            var block = room.GetBlock(pos);
            var shape = room.GetFaceShape(pos.X, pos.Y, face);

            FixTextureCoordinates(ref texture);

            if (!_editor.Tool.TextureUVFixer ||
                (shape == BlockFaceShape.Triangle && texture.TextureIsTriangle))
                return block.SetFaceTexture(face, texture);

            TextureArea processedTexture = texture;
            switch (face)
            {
                case BlockFace.Floor:
                case BlockFace.Ceiling:
                    BlockSurface surface = face == BlockFace.Floor ? block.Floor : block.Ceiling;
                    if (shape == BlockFaceShape.Quad)
                        break;
                    if (surface.DiagonalSplit != DiagonalSplit.XnZn && 
                        surface.DiagonalSplit != DiagonalSplit.XpZp && 
                        surface.SplitDirectionIsXEqualsZ)
                    {
                        if(surface.DiagonalSplit != DiagonalSplit.XnZp && surface.DiagonalSplit != DiagonalSplit.XpZn)
                        {
                            Swap.Do(ref processedTexture.TexCoord0, ref processedTexture.TexCoord2);
                            processedTexture.TexCoord1 = processedTexture.TexCoord3;
                        }
                    }
                    else
                    {
                        processedTexture.TexCoord0 = processedTexture.TexCoord1;
                        processedTexture.TexCoord1 = processedTexture.TexCoord2;
                        processedTexture.TexCoord2 = processedTexture.TexCoord3;
                    }
                    processedTexture.TexCoord3 = processedTexture.TexCoord2;
                    break;

                case BlockFace.FloorTriangle2:
                case BlockFace.CeilingTriangle2:
                    BlockSurface surface2 = face == BlockFace.FloorTriangle2 ? block.Floor : block.Ceiling;
                    if (shape == BlockFaceShape.Quad)
                        break;
                    if (surface2.DiagonalSplit == DiagonalSplit.XnZn || 
                        surface2.DiagonalSplit == DiagonalSplit.XpZp || 
                        !surface2.SplitDirectionIsXEqualsZ)
                    {
                        processedTexture.TexCoord2 = processedTexture.TexCoord1;
                        processedTexture.TexCoord1 = processedTexture.TexCoord0;
                        processedTexture.TexCoord0 = processedTexture.TexCoord3;
                    }
                    else
                    {
                        if (surface2.DiagonalSplit == DiagonalSplit.XnZp || surface2.DiagonalSplit == DiagonalSplit.XpZn)
                        {
                            Swap.Do(ref processedTexture.TexCoord0, ref processedTexture.TexCoord2);
                            processedTexture.TexCoord1 = processedTexture.TexCoord3;
                        }
                    }
                    processedTexture.TexCoord3 = processedTexture.TexCoord2;
                    break;

                default:
                    if (room.RoomGeometry != null)
                    {
                        // get current face
                        VertexRange vertexRange = room.RoomGeometry.VertexRangeLookup[new SectorInfo(pos.X, pos.Y, face)];
                        if (vertexRange.Count == 6)
                        {
                            Vector3 p0 = room.RoomGeometry.VertexPositions[vertexRange.Start + 2];
                            Vector3 p1 = room.RoomGeometry.VertexPositions[vertexRange.Start + 0];
                            Vector3 p2 = room.RoomGeometry.VertexPositions[vertexRange.Start + 1];
                            Vector3 p3 = room.RoomGeometry.VertexPositions[vertexRange.Start + 3];

                            float maxUp = Math.Max(p0.Y, p1.Y);
                            float minDown = Math.Min(p2.Y, p3.Y);

                            float difference = maxUp - minDown;

                            float delta0 = (minDown - p3.Y) / difference;
                            float delta1 = (maxUp - p0.Y) / difference;
                            float delta2 = (maxUp - p1.Y) / difference;
                            float delta3 = (minDown - p2.Y) / difference;

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
                            Vector3 p0 = room.RoomGeometry.VertexPositions[vertexRange.Start + 0];
                            Vector3 p1 = room.RoomGeometry.VertexPositions[vertexRange.Start + 1];
                            Vector3 p2 = room.RoomGeometry.VertexPositions[vertexRange.Start + 2];

                            float maxUp = Math.Max(Math.Max(p0.Y, p1.Y), p2.Y);
                            float minDown = Math.Min(Math.Min(p0.Y, p1.Y), p2.Y);
                            float difference = maxUp - minDown;

                            if (p0.X == p2.X && p0.Z == p2.Z)
                            {
                                float delta0 = (minDown - p2.Y) / difference;
                                float delta1 = (maxUp - p0.Y) / difference;
                                float delta2 = (maxUp - p1.Y) / difference;
                                float delta3 = (minDown - p1.Y) / difference;

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

                                float delta0 = (minDown - p0.Y) / difference;
                                float delta1 = (maxUp - p0.Y) / difference;
                                float delta2 = (maxUp - p1.Y) / difference;
                                float delta3 = (minDown - p2.Y) / difference;

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
                    break;
            }

            FixTextureCoordinates(ref processedTexture);

            return block.SetFaceTexture(face, processedTexture);
        }

        public static bool ApplyTexture(Room room, VectorInt2 pos, BlockFace face, TextureArea texture, bool disableUndo = false)
        {
            if(!disableUndo)
                _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            texture.ParentArea = new Rectangle2();

            if (face >= BlockFace.Ceiling) texture.Mirror();
            var textureApplied = ApplyTextureWithoutUpdate(room, pos, face, texture);
            if (textureApplied)
            {
                room.BuildGeometry();
                _editor.RoomTextureChange(room);
            }
            return textureApplied;
        }

        public static Dictionary<BlockFace, float[]> GetFaces(Room room, VectorInt2 pos, Direction direction, BlockFaceType section)
        {
            var block = room.GetBlockTry(pos.X, pos.Y);
            if (block == null)
                return null;

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

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    var segments = GetFaces(room, new VectorInt2(x, z), direction, type);
                    if (segments == null)
                        continue;

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
            processedTexture.ParentArea = texture.GetRect();
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

            float sectionHeight = maxSectionHeight - minSectionHeight;
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
                        processedTexture.TexCoord3.X = texture.TexCoord3.X - stride * (subdivisions - iteration);
                        processedTexture.TexCoord2.X = texture.TexCoord2.X - stride * (subdivisions - iteration);
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
                        processedTexture.TexCoord1.Y = texture.TexCoord1.Y - stride * (subdivisions - iteration);
                        processedTexture.TexCoord0.Y = texture.TexCoord0.Y - stride * (subdivisions - iteration);
                    }
                }

                ApplyTextureWithoutUpdate(room, pos, segment.Key, processedTexture);
            }
        }

        public static void TexturizeGroup(Room room, SectorSelection selection, SectorSelection workArea, TextureArea texture, BlockFace pickedFace, bool subdivideWalls, bool unifyHeight, bool disableUndo = false)
        {
            if(!disableUndo)
                _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);
            
            if (pickedFace >= BlockFace.Ceiling) texture.Mirror();
            RectangleInt2 area = selection != SectorSelection.None ? selection.Area : _editor.SelectedRoom.LocalArea;

            if (pickedFace < BlockFace.Floor)
            {
                int xSubs = subdivideWalls ? 0 : area.X1 - area.X0;
                int zSubs = subdivideWalls ? 0 : area.Y1 - area.Y0;

                for (int x = area.X0, iterX = 0; x <= area.X1; x++, iterX++)
                    for (int z = area.Y0, iterZ = 0; z <= area.Y1; z++, iterZ++)
                    {
                        if (room.CoordinateInvalid(x, z) || (workArea != SectorSelection.None && !workArea.Area.Contains(new VectorInt2(x, z))))
                            continue;

                        switch (pickedFace)
                        {
                            case BlockFace.NegativeX_QA:
                            case BlockFace.NegativeX_ED:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.NegativeX, BlockFaceType.Floor, texture, zSubs, iterZ, unifyHeight ? GetAreaExtremums(room, area, Direction.NegativeX, BlockFaceType.Floor) : null);
                                break;

                            case BlockFace.NegativeX_Middle:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.NegativeX, BlockFaceType.Wall, texture, zSubs, iterZ, unifyHeight ? GetAreaExtremums(room, area, Direction.NegativeX, BlockFaceType.Wall) : null);
                                break;

                            case BlockFace.NegativeX_RF:
                            case BlockFace.NegativeX_WS:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.NegativeX, BlockFaceType.Ceiling, texture, zSubs, iterZ, unifyHeight ? GetAreaExtremums(room, area, Direction.NegativeX, BlockFaceType.Ceiling) : null);
                                break;

                            case BlockFace.PositiveX_QA:
                            case BlockFace.PositiveX_ED:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.PositiveX, BlockFaceType.Floor, texture, zSubs, iterZ, unifyHeight ? GetAreaExtremums(room, area, Direction.PositiveX, BlockFaceType.Floor) : null);
                                break;

                            case BlockFace.PositiveX_Middle:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.PositiveX, BlockFaceType.Wall, texture, zSubs, iterZ, unifyHeight ? GetAreaExtremums(room, area, Direction.PositiveX, BlockFaceType.Wall) : null);
                                break;

                            case BlockFace.PositiveX_RF:
                            case BlockFace.PositiveX_WS:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.PositiveX, BlockFaceType.Ceiling, texture, zSubs, iterZ, unifyHeight ? GetAreaExtremums(room, area, Direction.PositiveX, BlockFaceType.Ceiling) : null);
                                break;

                            case BlockFace.NegativeZ_QA:
                            case BlockFace.NegativeZ_ED:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.NegativeZ, BlockFaceType.Floor, texture, xSubs, iterX, unifyHeight ? GetAreaExtremums(room, area, Direction.NegativeZ, BlockFaceType.Floor) : null);
                                break;

                            case BlockFace.NegativeZ_Middle:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.NegativeZ, BlockFaceType.Wall, texture, xSubs, iterX, unifyHeight ? GetAreaExtremums(room, area, Direction.NegativeZ, BlockFaceType.Wall) : null);
                                break;

                            case BlockFace.NegativeZ_RF:
                            case BlockFace.NegativeZ_WS:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.NegativeZ, BlockFaceType.Ceiling, texture, xSubs, iterX, unifyHeight ? GetAreaExtremums(room, area, Direction.NegativeZ, BlockFaceType.Ceiling) : null);
                                break;

                            case BlockFace.PositiveZ_QA:
                            case BlockFace.PositiveZ_ED:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.PositiveZ, BlockFaceType.Floor, texture, xSubs, iterX, unifyHeight ? GetAreaExtremums(room, area, Direction.PositiveZ, BlockFaceType.Floor) : null);
                                break;

                            case BlockFace.PositiveZ_Middle:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.PositiveZ, BlockFaceType.Wall, texture, xSubs, iterX, unifyHeight ? GetAreaExtremums(room, area, Direction.PositiveZ, BlockFaceType.Wall) : null);
                                break;

                            case BlockFace.PositiveZ_RF:
                            case BlockFace.PositiveZ_WS:
                                TexturizeWallSection(room, new VectorInt2(x, z), Direction.PositiveZ, BlockFaceType.Ceiling, texture, xSubs, iterX, unifyHeight ? GetAreaExtremums(room, area, Direction.PositiveZ, BlockFaceType.Ceiling) : null);
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
            }
            else
            {
                Vector2 verticalUVStride = (texture.TexCoord3 - texture.TexCoord2) / (area.Y1 - area.Y0 + 1);
                Vector2 horizontalUVStride = (texture.TexCoord2 - texture.TexCoord1) / (area.X1 - area.X0 + 1);

                for (int x = area.X0, x1 = 0; x <= area.X1; x++, x1++)
                {
                    Vector2 currentX = horizontalUVStride * x1;

                    for (int z = area.Y0, z1 = 0; z <= area.Y1; z++, z1++)
                    {
                        if (room.CoordinateInvalid(x, z) || (workArea != SectorSelection.None && !workArea.Area.Contains(new VectorInt2(x, z))))
                            continue;

                        TextureArea currentTexture = texture;
                        Vector2 currentZ = verticalUVStride * z1;

                        currentTexture.ParentArea = texture.GetRect();
                        currentTexture.TexCoord0 -= currentZ - currentX;
                        currentTexture.TexCoord1 = currentTexture.TexCoord0 - verticalUVStride;
                        currentTexture.TexCoord3 = currentTexture.TexCoord0 + horizontalUVStride;
                        currentTexture.TexCoord2 = currentTexture.TexCoord0 + horizontalUVStride - verticalUVStride;

                        // Round textures to 5 decimal digits
                        currentTexture.TexCoord0.X = (float)Math.Round(currentTexture.TexCoord0.X, 5);
                        currentTexture.TexCoord0.Y = (float)Math.Round(currentTexture.TexCoord0.Y, 5);
                        currentTexture.TexCoord1.X = (float)Math.Round(currentTexture.TexCoord1.X, 5);
                        currentTexture.TexCoord1.Y = (float)Math.Round(currentTexture.TexCoord1.Y, 5);
                        currentTexture.TexCoord2.X = (float)Math.Round(currentTexture.TexCoord2.X, 5);
                        currentTexture.TexCoord2.Y = (float)Math.Round(currentTexture.TexCoord2.Y, 5);
                        currentTexture.TexCoord3.X = (float)Math.Round(currentTexture.TexCoord3.X, 5);
                        currentTexture.TexCoord3.Y = (float)Math.Round(currentTexture.TexCoord3.Y, 5);
                        
                        switch (pickedFace)
                        {
                            case BlockFace.Floor:
                            case BlockFace.FloorTriangle2:
                                ApplyTextureWithoutUpdate(room, new VectorInt2(x, z), BlockFace.Floor, currentTexture);
                                ApplyTextureWithoutUpdate(room, new VectorInt2(x, z), BlockFace.FloorTriangle2, currentTexture);
                                break;

                            case BlockFace.Ceiling:
                            case BlockFace.CeilingTriangle2:
                                ApplyTextureWithoutUpdate(room, new VectorInt2(x, z), BlockFace.Ceiling, currentTexture);
                                ApplyTextureWithoutUpdate(room, new VectorInt2(x, z), BlockFace.CeilingTriangle2, currentTexture);
                                break;
                        }
                    }
                }
            }

            room.BuildGeometry();
            _editor.RoomTextureChange(room);
        }

        public static void TexturizeAll(Room room, SectorSelection selection, TextureArea texture, BlockFaceType type)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            if (type == BlockFaceType.Ceiling) texture.Mirror();
            RectangleInt2 area = selection.Valid ? selection.Area : _editor.SelectedRoom.LocalArea;

            texture.ParentArea = new Rectangle2();

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    switch (type)
                    {
                        case BlockFaceType.Floor:
                            ApplyTextureWithoutUpdate(room, new VectorInt2(x, z), BlockFace.Floor, texture);
                            ApplyTextureWithoutUpdate(room, new VectorInt2(x, z), BlockFace.FloorTriangle2, texture);
                            break;

                        case BlockFaceType.Ceiling:
                            ApplyTextureWithoutUpdate(room, new VectorInt2(x, z), BlockFace.Ceiling, texture);
                            ApplyTextureWithoutUpdate(room, new VectorInt2(x, z), BlockFace.CeilingTriangle2, texture);
                            break;

                        case BlockFaceType.Wall:
                            for (BlockFace face = BlockFace.PositiveZ_QA; face <= BlockFace.DiagonalRF; face++)
                                if (room.IsFaceDefined(x, z, face))
                                    ApplyTextureWithoutUpdate(room, new VectorInt2(x, z), face, texture);
                            break;
                    }

                }

            room.BuildGeometry();
            _editor.RoomTextureChange(room);
        }

        private static void AllocateScriptIds(PositionBasedObjectInstance instance)
        {
            if (instance is IHasScriptID &&
                (_editor.Level.Settings.GameVersion == GameVersion.TR4 ||
                 _editor.Level.Settings.GameVersion == GameVersion.TRNG))
            {
                (instance as IHasScriptID).AllocateNewScriptId();
            }

            if (instance is ItemInstance && _editor.Level.Settings.GameVersion == GameVersion.TR5Main)
            {
                (instance as ItemInstance).LuaId = _editor.Level.AllocNewLuaId();
            }
        }

        public static void PlaceObject(Room room, VectorInt2 pos, PositionBasedObjectInstance instance)
        {
            PlaceObjectWithoutUpdate(room, pos, instance);
            _editor.UndoManager.PushObjectCreated(instance);

            AllocateScriptIds(instance);
        }


        public static void PlaceObjectWithoutUpdate(Room room, VectorInt2 pos, PositionBasedObjectInstance instance)
        {
            Block block = room.GetBlock(pos);
            int y = (block.Floor.XnZp + block.Floor.XpZp + block.Floor.XpZn + block.Floor.XnZn) / 4;

            instance.Position = new Vector3(pos.X * 1024 + 512, y * 256, pos.Y * 1024 + 512);
            room.AddObject(_editor.Level, instance);
            if (instance is LightInstance)
                room.BuildGeometry(); // Rebuild lighting!
            _editor.ObjectChange(instance, ObjectChangeType.Add);
            _editor.SelectedObject = instance;
        }

        public static void DeleteRooms(IEnumerable<Room> rooms_, IWin32Window owner = null)
        {
            rooms_ = rooms_.SelectMany(room => room.Versions).Distinct();
            HashSet<Room> rooms = new HashSet<Room>(rooms_);

            // Check if is the last room
            int remainingRoomCount = _editor.Level.Rooms.Count(r => r != null && !rooms.Contains(r) && !rooms.Contains(r.AlternateOpposite));
            if (remainingRoomCount <= 0)
            {
                _editor.SendMessage("You must have at least one room in your level.", PopupType.Error);
                return;
            }

            // Ask for confirmation. No owner = silent mode!
            if (owner != null && DarkMessageBox.Show(owner,
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
                adjoiningRoom?.BuildGeometry();
                adjoiningRoom?.AlternateOpposite?.BuildGeometry();
            }

            if (rooms.Contains(_editor.SelectedRoom))
                _editor.SelectRoom(_editor.Level.Rooms.FirstOrDefault(r => r != null));

            _editor.RoomListChange();
            _editor.RoomGeometryChange(_editor.SelectedRoom);
        }

        public static void CropRoom(Room room, RectangleInt2 newArea, IWin32Window owner)
        {
            // Figure out new room area
            if (newArea.X0 <= 0 || newArea.Y0 <= 0 || newArea.X1 <= 0 || newArea.Y1 <= 0)
                newArea = new RectangleInt2(new VectorInt2(), room.SectorSize - VectorInt2.One);
            else
                newArea = newArea.Inflate(1);

            bool useFloor;
            using (FormResizeRoom form = new FormResizeRoom(_editor, room, newArea))
            {
                if (form.ShowDialog(owner) != DialogResult.OK)
                    return;
                newArea = form.NewArea;
                useFloor = form.UseFloor;
            }

            // Estimate wether triggers or portals must be removed
            // Ask the user again if portals or triggers are being removed in process.
            if (room.Portals.Any(portal => portal.Direction != PortalDirection.Floor && portal.Direction != PortalDirection.Ceiling && !newArea.Inflate(-1).Contains(portal.Area)) ||
                room.Triggers.Any(trigger => !newArea.Inflate(-1).Contains(trigger.Area)))
                if (DarkMessageBox.Show(owner, "Warning: if you crop this room, all portals and triggers outside the new area will be deleted." +
                    " Do you want to continue?", "Crop room", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

            // Resize room
            var relevantRooms = new HashSet<Room>(room.Portals.Select(p => p.AdjoiningRoom));
            if (room.Alternated)
                room.AlternateOpposite.Resize(_editor.Level, newArea, (short)room.AlternateOpposite.GetLowestCorner(), (short)room.AlternateOpposite.GetHighestCorner(), useFloor);
            room.Resize(_editor.Level, newArea, (short)room.GetLowestCorner(), (short)room.GetHighestCorner(), useFloor);
            Room.FixupNeighborPortals(_editor.Level, new[] { room }, new[] { room }, ref relevantRooms);
            Parallel.ForEach(relevantRooms, relevantRoom => relevantRoom.BuildGeometry());

            // Cleanup
            if ((_editor.SelectedRoom == room || _editor.SelectedRoom == room.AlternateOpposite) && _editor.SelectedSectors.Valid)
            {
                var selection = _editor.SelectedSectors;
                selection.Area = selection.Area.Intersect(newArea) - newArea.Start;
                _editor.SelectedSectors = selection;
            }
            foreach (Room relevantRoom in relevantRooms)
            {
                _editor.RoomPropertiesChange(relevantRoom);
                _editor.RoomSectorPropertiesChange(relevantRoom);
            }
        }

        public static void SetDiagonalFloorSplit(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;

                    if (room.Blocks[x, z].Floor.DiagonalSplit != DiagonalSplit.None)
                    {
                        if (room.Blocks[x, z].Type == BlockType.Floor)
                            room.Blocks[x, z].Transform(new RectTransformation { QuadrantRotation = -1 }, true);
                    }
                    else
                    {
                        // Now try to guess the floor split
                        short maxHeight = short.MinValue;
                        byte theCorner = 0;

                        if (room.Blocks[x, z].Floor.XnZp > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].Floor.XnZp;
                            theCorner = 0;
                        }

                        if (room.Blocks[x, z].Floor.XpZp > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].Floor.XpZp;
                            theCorner = 1;
                        }

                        if (room.Blocks[x, z].Floor.XpZn > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].Floor.XpZn;
                            theCorner = 2;
                        }

                        if (room.Blocks[x, z].Floor.XnZn > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].Floor.XnZn;
                            theCorner = 3;
                        }

                        switch(theCorner)
                        {
                            case 0:
                                room.Blocks[x, z].Floor.XpZp = maxHeight;
                                room.Blocks[x, z].Floor.XnZn = maxHeight;
                                room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.XnZp;
                                break;
                            case 1:
                                room.Blocks[x, z].Floor.XnZp = maxHeight;
                                room.Blocks[x, z].Floor.XpZn = maxHeight;
                                room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.XpZp;
                                break;
                            case 2:
                                room.Blocks[x, z].Floor.XpZp = maxHeight;
                                room.Blocks[x, z].Floor.XnZn = maxHeight;
                                room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.XpZn;
                                break;
                            case 3:
                                room.Blocks[x, z].Floor.XnZp = maxHeight;
                                room.Blocks[x, z].Floor.XpZn = maxHeight;
                                room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.XnZn;
                                break;
                        }
                        room.Blocks[x, z].Floor.SplitDirectionToggled = false;
                        room.Blocks[x, z].FixHeights();
                    }
                    room.Blocks[x, z].Type = BlockType.Floor;
                }
            SmartBuildGeometry(room, area);
        }

        public static void SetDiagonalCeilingSplit(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;

                    if (room.Blocks[x, z].Ceiling.DiagonalSplit != DiagonalSplit.None)
                    {
                        if (room.Blocks[x, z].Type == BlockType.Floor)
                            room.Blocks[x, z].Transform(new RectTransformation { QuadrantRotation = -1 }, false);
                    }
                    else
                    {
                        // Now try to guess the floor split
                        short minHeight = short.MaxValue;
                        byte theCorner = 0;

                        if (room.Blocks[x, z].Ceiling.XnZp < minHeight)
                        {
                            minHeight = room.Blocks[x, z].Ceiling.XnZp;
                            theCorner = 0;
                        }

                        if (room.Blocks[x, z].Ceiling.XpZp < minHeight)
                        {
                            minHeight = room.Blocks[x, z].Ceiling.XpZp;
                            theCorner = 1;
                        }

                        if (room.Blocks[x, z].Ceiling.XpZn < minHeight)
                        {
                            minHeight = room.Blocks[x, z].Ceiling.XpZn;
                            theCorner = 2;
                        }

                        if (room.Blocks[x, z].Ceiling.XnZn < minHeight)
                        {
                            minHeight = room.Blocks[x, z].Ceiling.XnZn;
                            theCorner = 3;
                        }

                        switch(theCorner)
                        {
                            case 0:
                                room.Blocks[x, z].Ceiling.XpZp = minHeight;
                                room.Blocks[x, z].Ceiling.XnZn = minHeight;
                                room.Blocks[x, z].Ceiling.DiagonalSplit = DiagonalSplit.XnZp;
                                break;
                            case 1:
                                room.Blocks[x, z].Ceiling.XnZp = minHeight;
                                room.Blocks[x, z].Ceiling.XpZn = minHeight;
                                room.Blocks[x, z].Ceiling.DiagonalSplit = DiagonalSplit.XpZp;
                                break;
                            case 2:
                                room.Blocks[x, z].Ceiling.XpZp = minHeight;
                                room.Blocks[x, z].Ceiling.XnZn = minHeight;
                                room.Blocks[x, z].Ceiling.DiagonalSplit = DiagonalSplit.XpZn;
                                break;
                            case 3:
                                room.Blocks[x, z].Ceiling.XnZp = minHeight;
                                room.Blocks[x, z].Ceiling.XpZn = minHeight;
                                room.Blocks[x, z].Ceiling.DiagonalSplit = DiagonalSplit.XnZn;
                                break;
                        }
                        room.Blocks[x, z].Ceiling.SplitDirectionToggled = false;
                        room.Blocks[x, z].FixHeights();
                    }
                    room.Blocks[x, z].Type = BlockType.Floor;
                }
            SmartBuildGeometry(room, area);
        }

        public static void SetDiagonalWall(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;

                    if (room.Blocks[x, z].Floor.DiagonalSplit != DiagonalSplit.None)
                    {
                        if (room.Blocks[x, z].Type == BlockType.Wall)
                            room.Blocks[x, z].Transform(new RectTransformation { QuadrantRotation = -1 }, null);
                        else
                            room.Blocks[x, z].Ceiling.DiagonalSplit = room.Blocks[x, z].Floor.DiagonalSplit;
                    }
                    else
                    {
                        // Now try to guess the floor split
                        short maxHeight = short.MinValue;
                        byte theCorner = 0;

                        if (room.Blocks[x, z].Floor.XnZp > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].Floor.XnZp;
                            theCorner = 0;
                        }

                        if (room.Blocks[x, z].Floor.XpZp > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].Floor.XpZp;
                            theCorner = 1;
                        }

                        if (room.Blocks[x, z].Floor.XpZn > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].Floor.XpZn;
                            theCorner = 2;
                        }

                        if (room.Blocks[x, z].Floor.XnZn > maxHeight)
                        {
                            maxHeight = room.Blocks[x, z].Floor.XnZn;
                            theCorner = 3;
                        }

                        switch(theCorner)
                        {
                            case 0:
                                room.Blocks[x, z].Floor.XpZp = maxHeight;
                                room.Blocks[x, z].Floor.XnZn = maxHeight;
                                room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.XnZp;
                                break;
                            case 1:
                                room.Blocks[x, z].Floor.XnZp = maxHeight;
                                room.Blocks[x, z].Floor.XpZn = maxHeight;
                                room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.XpZp;
                                break;
                            case 2:
                                room.Blocks[x, z].Floor.XpZp = maxHeight;
                                room.Blocks[x, z].Floor.XnZn = maxHeight;
                                room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.XpZn;
                                break;
                            case 3:
                                room.Blocks[x, z].Floor.XnZp = maxHeight;
                                room.Blocks[x, z].Floor.XpZn = maxHeight;
                                room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.XnZn;
                                break;
                        }
                        room.Blocks[x, z].Ceiling.DiagonalSplit = room.Blocks[x, z].Floor.DiagonalSplit;
                    }
                    room.Blocks[x, z].Type = BlockType.Wall;
                }
            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void RotateSectors(Room room, RectangleInt2 area, bool floor)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            bool wallsRotated = false;

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;
                    room.Blocks[x, z].Transform(new RectTransformation { QuadrantRotation = -1 }, room.Blocks[x, z].IsAnyWall ? null : (bool?)floor);

                    if (room.Blocks[x, z].Floor.DiagonalSplit != DiagonalSplit.None && room.Blocks[x, z].IsAnyWall)
                        wallsRotated = true;
                }

            SmartBuildGeometry(room, area);
            _editor.RoomGeometryChange(room);

            if (wallsRotated)
                _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetWall(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;
                    room.Blocks[x, z].Type = BlockType.Wall;
                    room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.None;
                }

            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetFloor(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);
            SetSurfaceWithoutUpdate(room, area, false);
            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetCeiling(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);
            SetSurfaceWithoutUpdate(room, area, true);
            SmartBuildGeometry(room, area);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void SetSurfaceWithoutUpdate(Room room, RectangleInt2 area, bool ceiling)
        {
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.BorderWall)
                        continue;

                    room.Blocks[x, z].Type = BlockType.Floor;

                    if (ceiling)
                        room.Blocks[x, z].Ceiling.DiagonalSplit = DiagonalSplit.None;
                    else
                        room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.None;
                }
        }

        public static void ToggleBlockFlag(Room room, RectangleInt2 area, BlockFlags flag)
        {
            List<Room> roomsToUpdate = new List<Room>();
            roomsToUpdate.Add(room);

            // Collect all affected rooms for undo
            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    var currentBlock = room.ProbeLowestBlock(x, z, _editor.Configuration.UI_ProbeAttributesThroughPortals);
                    if (!roomsToUpdate.Contains(currentBlock.Room))
                        roomsToUpdate.Add(currentBlock.Room);
                }

            // Do undo
            _editor.UndoManager.PushGeometryChanged(roomsToUpdate);

            if (_editor.Configuration.UI_SetAttributesAtOnce)
            {
                // Set or unset flag, based on already existing flag prevalence in selected area
                int amount = (area.Width + 1) * (area.Height + 1);
                int prevalence = 0;

                for (int x = area.X0; x <= area.X1; x++)
                    for (int z = area.Y0; z <= area.Y1; z++)
                    {
                        var currentBlock = room.ProbeLowestBlock(x, z, _editor.Configuration.UI_ProbeAttributesThroughPortals);
                        if ((currentBlock.Block.Flags & flag) != BlockFlags.None) prevalence++;
                    }

                bool toggle = (prevalence == 0 || prevalence <= (amount / 2));

                // Do actual flag editing
                for (int x = area.X0; x <= area.X1; x++)
                    for (int z = area.Y0; z <= area.Y1; z++)
                    {
                        var currentBlock = room.ProbeLowestBlock(x, z, _editor.Configuration.UI_ProbeAttributesThroughPortals);
                        if (toggle) currentBlock.Block.Flags |= flag;
                        else currentBlock.Block.Flags &= ~flag;
                    }
            }
            else
            {
                // Do actual flag editing
                for (int x = area.X0; x <= area.X1; x++)
                    for (int z = area.Y0; z <= area.Y1; z++)
                    {
                        var currentBlock = room.ProbeLowestBlock(x, z, _editor.Configuration.UI_ProbeAttributesThroughPortals);
                        currentBlock.Block.Flags ^= flag;
                    }
            }

            foreach (var currentRoom in roomsToUpdate)
                _editor.RoomSectorPropertiesChange(currentRoom);
        }

        public static void ToggleForceFloorSolid(Room room, RectangleInt2 area)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                    room.Blocks[x, z].ForceFloorSolid = !room.Blocks[x, z].ForceFloorSolid;
            SmartBuildGeometry(room, area);
            _editor.RoomGeometryChange(room);
            _editor.RoomSectorPropertiesChange(room);
        }

        public static void AddPortal(Room room, RectangleInt2 area, IWin32Window owner)
        {
            if (room.CornerSelected(area))
            {
                _editor.SendMessage("You have selected one of the four room's corners.", PopupType.Error);
                return;
            }

            // Check vertical space
            int floorLevel = int.MaxValue;
            int ceilingLevel = int.MinValue;
            for (int y = area.Y0; y <= area.Y1 + 1; ++y)
                for (int x = area.X0; x <= area.X1 + 1; ++x)
                {
                    floorLevel = room.GetHeightsAtPoint(x, y, BlockVertical.Floor).Select(v => v + room.Position.Y).Concat(new int[] { floorLevel }).Min();
                    ceilingLevel = room.GetHeightsAtPoint(x, y, BlockVertical.Ceiling).Select(v => v + room.Position.Y).Concat(new int[] { ceilingLevel }).Max();
                }

            // Check for possible candidates ...
            List<Tuple<PortalDirection, Room>> candidates = new List<Tuple<PortalDirection, Room>>();
            if (floorLevel != int.MaxValue && ceilingLevel != int.MinValue)
            {
                bool couldBeFloorCeilingPortal = false;
                if (new RectangleInt2(1, 1, room.NumXSectors - 2, room.NumZSectors - 2).Contains(area))
                    for (int z = area.Y0; z <= area.Y1; ++z)
                        for (int x = area.X0; x <= area.X1; ++x)
                            if (!room.Blocks[x, z].IsAnyWall)
                                couldBeFloorCeilingPortal = true;

                foreach (Room neighborRoom in _editor.Level.Rooms.Where(possibleNeighborRoom => possibleNeighborRoom != null))
                {
                    // Don't make a portal to the room itself
                    // Don't list alternate rooms as seperate candidates
                    if (neighborRoom == room || neighborRoom == room.AlternateOpposite || neighborRoom.AlternateBaseRoom != null)
                        continue;
                    RectangleInt2 neighborArea = area + (room.SectorPos - neighborRoom.SectorPos);
                    if (!new RectangleInt2(0, 0, neighborRoom.NumXSectors - 1, neighborRoom.NumZSectors - 1).Contains(neighborArea))
                        continue;

                    // Check if they vertically touch
                    int neighborFloorLevel = int.MaxValue;
                    int neighborCeilingLevel = int.MinValue;
                    for (int y = neighborArea.Y0; y <= neighborArea.Y1 + 1; ++y)
                        for (int x = neighborArea.X0; x <= neighborArea.X1 + 1; ++x)
                        {
                            neighborFloorLevel = neighborRoom.GetHeightsAtPoint(x, y, BlockVertical.Floor).Select(v => v + neighborRoom.Position.Y).Concat(new int[] { neighborFloorLevel }).Min();
                            neighborCeilingLevel = neighborRoom.GetHeightsAtPoint(x, y, BlockVertical.Ceiling).Select(v => v + neighborRoom.Position.Y).Concat(new int[] { neighborCeilingLevel }).Max();
                            if (neighborRoom.AlternateOpposite != null)
                            {
                                neighborFloorLevel = neighborRoom.AlternateOpposite.GetHeightsAtPoint(x, y, BlockVertical.Floor).Select(v => v + neighborRoom.AlternateOpposite.Position.Y).Concat(new int[] { neighborFloorLevel }).Min();
                                neighborCeilingLevel = neighborRoom.AlternateOpposite.GetHeightsAtPoint(x, y, BlockVertical.Ceiling).Select(v => v + neighborRoom.AlternateOpposite.Position.Y).Concat(new int[] { neighborCeilingLevel }).Max();
                            }
                        }
                    if (neighborFloorLevel == int.MaxValue || neighborCeilingLevel == int.MinValue)
                        continue;
                    if (!(floorLevel <= neighborCeilingLevel && ceilingLevel >= neighborFloorLevel))
                        continue;

                    // Decide on a direction
                    if (couldBeFloorCeilingPortal &&
                        new RectangleInt2(1, 1, neighborRoom.NumXSectors - 2, neighborRoom.NumZSectors - 2).Contains(neighborArea))
                    {
                        if (Math.Abs(neighborCeilingLevel - floorLevel) <
                            Math.Abs(neighborFloorLevel - ceilingLevel))
                        { // Consider floor portal
                            candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.Floor, neighborRoom));
                        }
                        else
                        { // Consider ceiling portal
                            candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.Ceiling, neighborRoom));
                        }
                    }
                    if (area.Width == 0 && area.X0 == 0)
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallNegativeX, neighborRoom));
                    if (area.Width == 0 && area.X0 == room.NumXSectors - 1)
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallPositiveX, neighborRoom));
                    if (area.Height == 0 && area.Y0 == 0)
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallNegativeZ, neighborRoom));
                    if (area.Height == 0 && area.Y0 == room.NumZSectors - 1)
                        candidates.Add(new Tuple<PortalDirection, Room>(PortalDirection.WallPositiveZ, neighborRoom));
                }
            }

            if (candidates.Count > 1)
            {
                using (var form = new FormChooseRoom("More than one possible room found that can be connected. " +
                    "Please choose one:", candidates.Select(candidate => candidate.Item2), selectedRoom => _editor.SelectedRoom = selectedRoom))
                {
                    if (form.ShowDialog(owner) != DialogResult.OK || form.SelectedRoom == null)
                        return;
                    candidates.RemoveAll(candidate => candidate.Item2 != form.SelectedRoom);
                }
            }
            if (candidates.Count != 1)
            {
                _editor.SendMessage("There are no possible room candidates for a portal.", PopupType.Error);
                return;
            }

            PortalDirection destinationDirection = candidates[0].Item1;
            Room destination = candidates[0].Item2;

            // Create portals
            var mainPortal = new PortalInstance(area, destinationDirection, destination);
            var portals = room.AddObject(_editor.Level, mainPortal).Cast<PortalInstance>();

            // Update
            foreach (Room portalRoom in portals.Select(portal => portal.Room).Distinct())
                portalRoom.BuildGeometry();
            foreach (PortalInstance portal in portals)
                _editor.ObjectChange(portal, ObjectChangeType.Add);

            // Reset selection
            _editor.Action = null;
            _editor.SelectedSectors = SectorSelection.None;
            _editor.SelectedObject = null;
            _editor.SelectedRooms = new[] { _editor.SelectedRoom };

            _editor.RoomSectorPropertiesChange(room);
            _editor.RoomSectorPropertiesChange(destination);

            // Undo
            _editor.UndoManager.PushSectorObjectCreated(mainPortal);
        }

        public static void AlternateRoomEnable(Room room, short AlternateGroup)
        {
            // Create new room
            var newRoom = room.Clone(_editor.Level, instance => instance.CopyToAlternateRooms);
            newRoom.Name = "Flipped of " + room;
            newRoom.BuildGeometry();

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

        public static void SmoothRandom(Room room, RectangleInt2 area, float strengthDirection, BlockVertical vertical)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            float[,] changes = new float[area.Width + 2, area.Height + 2];
            Random rng = new Random();
            for (int x = 1; x <= area.Width; x++)
                for (int z = 1; z <= area.Height; z++)
                    changes[x, z] = (float)rng.NextDouble() * strengthDirection;

            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                        room.Blocks[area.X0 + x, area.Y0 + z].ChangeHeight(vertical, edge,
                            (int)Math.Round(changes[x + edge.DirectionX(), z + edge.DirectionZ()]));

            SmartBuildGeometry(room, area);
        }

        public static void SharpRandom(Room room, RectangleInt2 area, float strengthDirection, BlockVertical vertical)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            Random rng = new Random();
            for (int x = 0; x <= area.Width; x++)
                for (int z = 0; z <= area.Height; z++)
                    for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                        room.Blocks[area.X0 + x, area.Y0 + z].ChangeHeight(vertical, edge,
                            (int)Math.Round((float)rng.NextDouble() * strengthDirection));

            SmartBuildGeometry(room, area);
        }

        public static void AverageSectors(Room room, RectangleInt2 area, BlockVertical vertical)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Block b = room.Blocks[x, z];
                    int sum = 0;
                    for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                        sum += b.GetHeight(vertical, edge);
                    for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                        b.SetHeight(vertical, edge, sum / 4);
                }
            SmartBuildGeometry(room, area);
        }

        public static void GridWalls(Room room, RectangleInt2 area, bool fiveDivisions = false)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Block block = room.Blocks[x, z];
                    if (block.IsAnyWall)
                    {
                        // Figure out corner heights
                        int?[] floorHeights = new int?[(int)BlockEdge.Count];
                        int?[] ceilingHeights = new int?[(int)BlockEdge.Count];
                        for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                        {
                            int testX = x + edge.DirectionX(), testZ = z + edge.DirectionZ();
                            floorHeights[(int)edge] = room.GetHeightsAtPoint(testX, testZ, BlockVertical.Floor).Cast<int?>().Max();
                            ceilingHeights[(int)edge] = room.GetHeightsAtPoint(testX, testZ, BlockVertical.Ceiling).Cast<int?>().Min();
                        }

                        if (!floorHeights.Any(floorHeight => floorHeight.HasValue) || !ceilingHeights.Any(floorHeight => floorHeight.HasValue))
                            continue; // We can only do it if there is information available

                        for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                        {
                            // Skip opposite diagonal step corner
                            switch (block.Floor.DiagonalSplit)
                            {
                                case DiagonalSplit.XnZn:
                                    if (edge == BlockEdge.XpZp)
                                        continue;
                                    break;
                                case DiagonalSplit.XnZp:
                                    if (edge == BlockEdge.XpZn)
                                        continue;
                                    break;
                                case DiagonalSplit.XpZn:
                                    if (edge == BlockEdge.XnZp)
                                        continue;
                                    break;
                                case DiagonalSplit.XpZp:
                                    if (edge == BlockEdge.XnZn)
                                        continue;
                                    break;
                            }

                            // Use the closest available vertical area information and divide it equally
                            int floor = floorHeights[(int)edge] ?? floorHeights[((int)edge + 1) % 4] ?? floorHeights[((int)edge + 3) % 4] ?? floorHeights[((int)edge + 2) % 4].Value;
                            int ceiling = ceilingHeights[(int)edge] ?? ceilingHeights[((int)edge + 1) % 4] ?? ceilingHeights[((int)edge + 3) % 4] ?? ceilingHeights[((int)edge + 2) % 4].Value;

                            block.SetHeight(BlockVertical.Ed, edge, (short)Math.Round(fiveDivisions ? (floor * 4.0f + ceiling * 1.0f) / 5.0f : floor));
                            block.Floor.SetHeight(edge, (short)Math.Round(fiveDivisions ? (floor * 3.0f + ceiling * 2.0f) / 5.0f : (floor * 2.0f + ceiling * 1.0f) / 3.0f));
                            block.Ceiling.SetHeight(edge, (short)Math.Round(fiveDivisions ? (floor * 2.0f + ceiling * 3.0f) / 5.0f : (floor * 1.0f + ceiling * 2.0f) / 3.0f));
                            block.SetHeight(BlockVertical.Rf, edge, (short)Math.Round(fiveDivisions ? (floor * 1.0f + ceiling * 4.0f) / 5.0f : ceiling));
                        }
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static void GridWallsSquares(Room room, RectangleInt2 area, bool fiveDivisions = false)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            int minFloor = int.MaxValue;
            int maxCeiling = int.MinValue;

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Block block = room.Blocks[x, z];
                    if (block.IsAnyWall)
                    {
                        // Figure out corner heights
                        int?[] floorHeights = new int?[(int)BlockEdge.Count];
                        int?[] ceilingHeights = new int?[(int)BlockEdge.Count];
                        for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                        {
                            int testX = x + edge.DirectionX(), testZ = z + edge.DirectionZ();
                            floorHeights[(int)edge] = room.GetHeightsAtPoint(testX, testZ, BlockVertical.Floor).Cast<int?>().Max();
                            ceilingHeights[(int)edge] = room.GetHeightsAtPoint(testX, testZ, BlockVertical.Ceiling).Cast<int?>().Min();
                        }

                        if (!floorHeights.Any(floorHeight => floorHeight.HasValue) || !ceilingHeights.Any(floorHeight => floorHeight.HasValue))
                            continue; // We can only do it if there is information available

                        for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                        {
                            // Skip opposite diagonal step corner
                            switch (block.Floor.DiagonalSplit)
                            {
                                case DiagonalSplit.XnZn:
                                    if (edge == BlockEdge.XpZp)
                                        continue;
                                    break;
                                case DiagonalSplit.XnZp:
                                    if (edge == BlockEdge.XpZn)
                                        continue;
                                    break;
                                case DiagonalSplit.XpZn:
                                    if (edge == BlockEdge.XnZp)
                                        continue;
                                    break;
                                case DiagonalSplit.XpZp:
                                    if (edge == BlockEdge.XnZn)
                                        continue;
                                    break;
                            }

                            // Use the closest available vertical area information and divide it equally
                            int floor = floorHeights[(int)edge] ?? floorHeights[((int)edge + 1) % 4] ?? floorHeights[((int)edge + 3) % 4] ?? floorHeights[((int)edge + 2) % 4].Value;
                            int ceiling = ceilingHeights[(int)edge] ?? ceilingHeights[((int)edge + 1) % 4] ?? ceilingHeights[((int)edge + 3) % 4] ?? ceilingHeights[((int)edge + 2) % 4].Value;

                            if (floor <= minFloor) minFloor = floor;
                            if (ceiling >= maxCeiling) maxCeiling = ceiling;
                        }
                    }
                }

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    Block block = room.Blocks[x, z];
                    if (block.IsAnyWall)
                    {
                        for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                        {
                            // Skip opposite diagonal step corner
                            switch (block.Floor.DiagonalSplit)
                            {
                                case DiagonalSplit.XnZn:
                                    if (edge == BlockEdge.XpZp)
                                        continue;
                                    break;
                                case DiagonalSplit.XnZp:
                                    if (edge == BlockEdge.XpZn)
                                        continue;
                                    break;
                                case DiagonalSplit.XpZn:
                                    if (edge == BlockEdge.XnZp)
                                        continue;
                                    break;
                                case DiagonalSplit.XpZp:
                                    if (edge == BlockEdge.XnZn)
                                        continue;
                                    break;
                            }
                            block.SetHeight(BlockVertical.Ed, edge, (short)Math.Round(fiveDivisions ? (minFloor * 4.0f + maxCeiling * 1.0f) / 5.0f : minFloor));
                            block.Floor.SetHeight(edge, (short)Math.Round(fiveDivisions ? (minFloor * 3.0f + maxCeiling * 2.0f) / 5.0f : (minFloor * 2.0f + maxCeiling * 1.0f) / 3.0f));
                            block.Ceiling.SetHeight(edge, (short)Math.Round(fiveDivisions ? (minFloor * 2.0f + maxCeiling * 3.0f) / 5.0f : (minFloor * 1.0f + maxCeiling * 2.0f) / 3.0f));
                            block.SetHeight(BlockVertical.Rf, edge, (short)Math.Round(fiveDivisions ? (minFloor * 1.0f + maxCeiling * 4.0f) / 5.0f : maxCeiling));
                        }
                    }
                }

            SmartBuildGeometry(room, area);
        }

        public static Room CreateAdjoiningRoom(Room room, SectorSelection selection, PortalDirection direction, short roomDepth = 12, bool switchRoom = true, bool clearAdjoiningArea = false)
        {
            if (!selection.Empty && !selection.Valid)
            {
                _editor.SendMessage("Selection is invalid. Can't create new room.", PopupType.Error);
                return null;
            }

            var clampedSelection = (selection.Empty ? new SectorSelection { Area = room.LocalArea } : selection).ClampToRoom(room, PortalInstance.GetDirection(direction));
            if (!clampedSelection.HasValue)
            {
                _editor.SendMessage("Can't create adjoining room. \nSelection is inside border walls.", PopupType.Error);
                return null;
            }

            if(room.IsAnyPortal(direction, clampedSelection.Value.Area))
            {
                _editor.SendMessage("Can't create adjoining room. \nSelection contains existing portals.", PopupType.Error);
                return null;
            }

            int roomSizeX, roomSizeY, roomSizeZ;
            VectorInt3 roomPos;
            RectangleInt2 portalArea;
            var dirString = "";

            int selectionRefPoint = 0;
            if (direction == PortalDirection.WallPositiveZ)
                selectionRefPoint = room.NumZSectors - 1;
            else if (direction == PortalDirection.WallPositiveX)
                selectionRefPoint = room.NumXSectors - 1;

            switch (direction)
            {
                case PortalDirection.WallNegativeX:
                case PortalDirection.WallPositiveX:
                    if (!(clampedSelection.Value.Area.Start.X == selectionRefPoint || clampedSelection.Value.Area.End.X == selectionRefPoint))
                    {
                        _editor.SendMessage("Can't create horizontally adjoining room. \nSelect room border to attach it.", PopupType.Error);
                        return null; // Don't create room aside if selection doesn't touch border wall
                    }

                    // Inflate small selection to define min/max height later based on neighbor sector heights
                    if (clampedSelection.Value.Area.Size.X == 0)
                    {
                        clampedSelection = new SectorSelection() { Area = clampedSelection.Value.Area.Inflate(1, 0) };
                        clampedSelection = clampedSelection.Value.ClampToRoom(room);
                    }

                    roomSizeX = roomDepth + 2;
                    roomSizeY = room.GetHighestCorner(clampedSelection.Value.Area) - room.GetLowestCorner(clampedSelection.Value.Area);
                    roomSizeZ = clampedSelection.Value.Area.Size.Y + 3;
                    roomPos   = room.Position + new VectorInt3(direction == PortalDirection.WallNegativeX ? -roomDepth : room.NumXSectors - 2,
                                                               room.GetLowestCorner(clampedSelection.Value.Area),
                                                               clampedSelection.Value.Area.Start.Y - 1);
                    // Position portal and assign dir name
                    if (direction == PortalDirection.WallNegativeX)
                    {
                        portalArea = new RectangleInt2(roomSizeX - 1, 1, roomSizeX - 1, roomSizeZ - 2);
                        dirString = "left";
                    }
                    else
                    {
                        portalArea = new RectangleInt2(0, 1, 0, roomSizeZ - 2);
                        dirString = "right";
                    }
                    break;

                case PortalDirection.WallNegativeZ:
                case PortalDirection.WallPositiveZ:
                    if (!(clampedSelection.Value.Area.Start.Y == selectionRefPoint || clampedSelection.Value.Area.End.Y == selectionRefPoint))
                    {
                        _editor.SendMessage("Can't create horizontally adjoining room. \nSelect room border to attach it.", PopupType.Error);
                        return null; // Don't create room aside if selection doesn't touch border wall
                    }

                    // Inflate small selection to define min/max height later based on neighbor sector heights
                    if (clampedSelection.Value.Area.Size.Y == 0)
                    {
                        clampedSelection = new SectorSelection() { Area = clampedSelection.Value.Area.Inflate(0, 1) };
                        clampedSelection = clampedSelection.Value.ClampToRoom(room);
                    }

                    roomSizeX = clampedSelection.Value.Area.Size.X + 3;
                    roomSizeY = room.GetHighestCorner(clampedSelection.Value.Area) - room.GetLowestCorner(clampedSelection.Value.Area);
                    roomSizeZ = roomDepth + 2;
                    roomPos   = room.Position + new VectorInt3(clampedSelection.Value.Area.Start.X - 1,
                                                               room.GetLowestCorner(clampedSelection.Value.Area),
                                                               direction == PortalDirection.WallNegativeZ ? -roomDepth : room.NumZSectors - 2);
                    // Position portal and assign dir name
                    if (direction == PortalDirection.WallNegativeZ)
                    {
                        portalArea = new RectangleInt2(1, roomSizeZ - 1, roomSizeX - 2, roomSizeZ - 1);
                        dirString = "back";
                    }
                    else
                    {
                        portalArea = new RectangleInt2(1, 0, roomSizeX - 2, 0);
                        dirString = "in front";
                    }
                    break;

                case PortalDirection.Floor:
                case PortalDirection.Ceiling:
                default:
                    roomSizeX = clampedSelection.Value.Area.Size.X + 3;
                    roomSizeY = roomDepth;
                    roomSizeZ = clampedSelection.Value.Area.Size.Y + 3;
                    roomPos = room.Position + new VectorInt3(clampedSelection.Value.Area.Start.X - 1,
                                                             direction == PortalDirection.Floor ? room.GetLowestCorner(selection.Area) - roomDepth : room.GetHighestCorner(selection.Area),
                                                             clampedSelection.Value.Area.Start.Y - 1);
                    portalArea = new RectangleInt2(1, 1, roomSizeX - 2, roomSizeZ - 2);
                    dirString = direction == PortalDirection.Floor ? "below" : "above";

                    // Reset parent floor or ceiling to adjoin new portal
                    if(clearAdjoiningArea)
                        FlattenRoomArea(room, clampedSelection.Value.Area, null, direction == PortalDirection.Ceiling, false, false);
                    break;
            }

            // Create room and attach portal
            var newRoom = new Room(_editor.Level, roomSizeX, roomSizeZ, _editor.Level.Settings.DefaultAmbientLight,
                                   "", (short)roomSizeY);
            int roomNumber = _editor.Level.AssignRoomToFree(newRoom);
            newRoom.Position = roomPos;
            newRoom.AddObject(_editor.Level, new PortalInstance(portalArea, PortalInstance.GetOppositeDirection(direction), room));
            newRoom.Name = "Room " + roomNumber + " (digged " + dirString + ")";
            _editor.RoomListChange();

            // Build the geometry of the new room
            Parallel.Invoke(() => newRoom.BuildGeometry(), () => room.BuildGeometry());

            if (switchRoom && (_editor.SelectedRoom == room || _editor.SelectedRoom == room.AlternateOpposite))
                _editor.SelectedRoom = newRoom; //Don't center
            else
            {
                _editor.RoomSectorPropertiesChange(room);
                _editor.RoomSectorPropertiesChange(newRoom);
            }

            // Undo
            _editor.UndoManager.PushAdjoiningRoomCreated(newRoom);

            return newRoom;
        }

        public static void FlattenRoomArea(Room room, RectangleInt2 area, int? height, bool ceiling, bool includeWalls, bool rebuild, bool disableUndo = false)
        {
            if (!disableUndo)
                _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            if (!height.HasValue)
                height = ceiling ? room.GetHighestCorner(area) : room.GetLowestCorner(area);

            for (int x = area.X0; x <= area.X1; x++)
                for (int z = area.Y0; z <= area.Y1; z++)
                {
                    if (room.Blocks[x, z].Type == BlockType.Floor || (includeWalls && room.Blocks[x, z].Type != BlockType.Floor))
                    {

                        if (ceiling)
                            room.Blocks[x, z].Ceiling.SetHeight(height.Value);
                        else
                            room.Blocks[x, z].Floor.SetHeight(height.Value);
                    }
                }

            if(rebuild)
                SmartBuildGeometry(room, area);
        }

        public static void MergeRoomsHorizontally(IEnumerable<Room> rooms, IWin32Window owner)
        {
            if (rooms.Count() < 2)
            {
                _editor.SendMessage("Select at least 2 rooms to merge them.");
                return;
            }

            bool alternated = rooms.Any(room => room.Alternated);
            if (alternated)
                rooms = rooms.Select(room => room.AlternateBaseRoom ?? room);
            rooms = rooms.Distinct();

            // First we need to make sure that no rooms overlap
            // If rooms overlap, it's uncertain which room takes priority.
            Func<Room, Dictionary<VectorInt2, Room>, bool> buildSectorMap = (room, sectorMap) =>
            {
                bool roomOverlaps = false;
                for (int x = 1; x < (room.NumXSectors - 1); ++x)
                    for (int z = 1; z < (room.NumZSectors - 1); ++z)
                        // TODO: maybe we could improve this, for now keep it disabled and test 
                        if (!room.Blocks[x, z].IsAnyWall  || true)
                        {
                            VectorInt2 worldPos = room.SectorPos + new VectorInt2(x, z);
                            Room existingRoom;
                            if (sectorMap.TryGetValue(worldPos, out existingRoom))
                                roomOverlaps = true;
                            else
                                sectorMap.Add(worldPos, room);
                        }
                return roomOverlaps;
            };

            bool baseRoomOverlaps = false;
            bool alternateRoomOverlaps = false;
            Dictionary<VectorInt2, Room> baseNewSectorMap = new Dictionary<VectorInt2, Room>();
            Dictionary<VectorInt2, Room> alternateNewSectorMap = new Dictionary<VectorInt2, Room>();
            foreach (Room room in rooms)
            {
                baseRoomOverlaps = buildSectorMap(room, baseNewSectorMap) || baseRoomOverlaps;
                if (alternated)
                    alternateRoomOverlaps = buildSectorMap(room.AlternateOpposite ?? room, alternateNewSectorMap) || alternateRoomOverlaps;
            }

            // If there are overlaps, ask the user about it
            if (baseRoomOverlaps || alternateRoomOverlaps)
                if (DarkMessageBox.Show(owner, "The selected rooms overlap horizontally. To resolve the conflict manually, put down walls on the overlapped sectors. " +
                    "You can continue anyway, the rooms are then taken in this priority: " + string.Join(", ", rooms.Select(room => room.Name)),
                    "Rooms are overlapping", MessageBoxButtons.OKCancel, MessageBoxIcon.Information) != DialogResult.OK)
                    return;

            // Figure out position and size of the merged room
            VectorInt2 minSectorPos = new VectorInt2(int.MaxValue, int.MaxValue);
            VectorInt2 maxSectorPos = new VectorInt2(int.MinValue, int.MinValue);
            foreach (VectorInt2 position in baseNewSectorMap.Keys.Concat(alternateNewSectorMap.Keys))
            {
                minSectorPos = VectorInt2.Min(minSectorPos, position);
                maxSectorPos = VectorInt2.Max(maxSectorPos, position);
            }
            VectorInt2 size = maxSectorPos - minSectorPos + new VectorInt2(2, 2);
            if (size.X > Room.MaxRecommendedRoomDimensions || size.Y > Room.MaxRecommendedRoomDimensions)
                if (DarkMessageBox.Show(owner, "After merging all rooms, the new room will have size " + size.X + " by " + size.Y + ". That bigger than " +
                    Room.MaxRecommendedRoomDimensions + " by " + Room.MaxRecommendedRoomDimensions + " which is maximum size for the engine. You can continue anyway," +
                    " but in game there will be issues with rendering. Are you sure?", "Room too big.", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                    return;

            // Create new room and start merging
            var relevantRooms = new HashSet<Room>(rooms.SelectMany(room => room.Portals).Select(portal => portal.AdjoiningRoom));
            Room newRoom = alternated ? rooms.First(room => room.Alternated) : rooms.First();
            var resizeParameter = RectangleInt2.FromLTRB(minSectorPos - newRoom.SectorPos - VectorInt2.One, size);
            if (alternated)
                newRoom.AlternateOpposite.Resize(_editor.Level, resizeParameter,
                    checked((short)rooms.Min(room => (room.AlternateOpposite ?? room).GetLowestCorner())),
                    checked((short)rooms.Max(room => (room.AlternateOpposite ?? room).GetHighestCorner())));
            newRoom.Resize(_editor.Level, resizeParameter,
                checked((short)rooms.Min(room => room.GetLowestCorner())),
                checked((short)rooms.Max(room => room.GetHighestCorner())));
            IEnumerable<Room> mergeRooms = rooms.Where(room => room != newRoom);

            Action<Room, IEnumerable<Room>, Dictionary<VectorInt2, Room>, bool> performMergeAction =
                (newRoomToHandle, roomsToHandle, newSectorMap, removeObjectsInNotAlternatedRooms) =>
                {
                    foreach (KeyValuePair<VectorInt2, Room> sector in newSectorMap)
                    {
                        if (sector.Value == newRoomToHandle)
                            continue;
                        Block oldBlock = newRoomToHandle.GetBlock(sector.Key - newRoomToHandle.SectorPos);
                        Block newBlock = sector.Value.GetBlock(sector.Key - sector.Value.SectorPos).Clone();

                        // Preserve outer wall textures
                        for (BlockFace face = 0; face < BlockFace.Count; ++face)
                        {
                            var direction = face.GetDirection();
                            if (direction == Direction.NegativeX || direction == Direction.PositiveX || direction == Direction.NegativeZ || direction == Direction.PositiveZ)
                                newBlock.SetFaceTexture(face, oldBlock.GetFaceTexture(face));
                        }

                        // Transform positions
                        for (BlockVertical vertical = 0; vertical < BlockVertical.Count; ++vertical)
                            for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                                newBlock.ChangeHeight(vertical, edge, sector.Value.Position.Y - newRoomToHandle.Position.Y);

                        newRoomToHandle.SetBlock(sector.Key - newRoomToHandle.SectorPos, newBlock);
                    }
                    foreach (KeyValuePair<VectorInt2, Room> sector in newSectorMap)
                    { // Copy all adjacent sectors of walls
                        if (sector.Value == newRoomToHandle)
                            continue;

                        // Copy adjacemt blocks
                        Block thisBlockNegativeX = newRoomToHandle.GetBlock(sector.Key - newRoomToHandle.SectorPos + new VectorInt2(-1, 0));
                        Block otherBlockNegativeX = sector.Value.GetBlock(sector.Key - sector.Value.SectorPos + new VectorInt2(-1, 0));
                        Block thisBlockPositiveX = newRoomToHandle.GetBlock(sector.Key - newRoomToHandle.SectorPos + new VectorInt2(1, 0));
                        Block otherBlockPositiveX = sector.Value.GetBlock(sector.Key - sector.Value.SectorPos + new VectorInt2(1, 0));
                        Block thisBlockNegativeZ = newRoomToHandle.GetBlock(sector.Key - newRoomToHandle.SectorPos + new VectorInt2(0, -1));
                        Block otherBlockNegativeZ = sector.Value.GetBlock(sector.Key - sector.Value.SectorPos + new VectorInt2(0, -1));
                        Block thisBlockPositiveZ = newRoomToHandle.GetBlock(sector.Key - newRoomToHandle.SectorPos + new VectorInt2(0, 1));
                        Block otherBlockPositiveZ = sector.Value.GetBlock(sector.Key - sector.Value.SectorPos + new VectorInt2(0, 1));

                        // Copy adjacent outer wall textures
                        // Unfortunately they are always on the adjacent block, so they need extra handling
                        for (BlockFace face = 0; face < BlockFace.Count; ++face)
                        {
                            var direction = face.GetDirection();
                            switch (direction)
                            {
                                case Direction.NegativeX:
                                    thisBlockPositiveX.SetFaceTexture(face, otherBlockPositiveX.GetFaceTexture(face));
                                    break;
                                case Direction.PositiveX:
                                    thisBlockNegativeX.SetFaceTexture(face, otherBlockNegativeX.GetFaceTexture(face));
                                    break;
                                case Direction.NegativeZ:
                                    thisBlockPositiveZ.SetFaceTexture(face, otherBlockPositiveZ.GetFaceTexture(face));
                                    break;
                                case Direction.PositiveZ:
                                    thisBlockNegativeZ.SetFaceTexture(face, otherBlockNegativeZ.GetFaceTexture(face));
                                    break;
                            }
                        }

                        // Copy vertical subdivisions along edge
                        int heightDifference = sector.Value.Position.Y - newRoomToHandle.Position.Y;
                        for (BlockVertical vertical = 0; vertical < BlockVertical.Count; ++vertical)
                        {
                            if (thisBlockNegativeX.IsAnyWall)
                            {
                                thisBlockNegativeX.SetHeight(vertical, BlockEdge.XpZn, otherBlockNegativeX.GetHeight(vertical, BlockEdge.XpZn) + heightDifference);
                                thisBlockNegativeX.SetHeight(vertical, BlockEdge.XpZp, otherBlockNegativeX.GetHeight(vertical, BlockEdge.XpZp) + heightDifference);
                            }
                            if (thisBlockPositiveX.IsAnyWall)
                            {
                                thisBlockPositiveX.SetHeight(vertical, BlockEdge.XnZn, otherBlockPositiveX.GetHeight(vertical, BlockEdge.XnZn) + heightDifference);
                                thisBlockPositiveX.SetHeight(vertical, BlockEdge.XnZp, otherBlockPositiveX.GetHeight(vertical, BlockEdge.XnZp) + heightDifference);
                            }
                            if (thisBlockNegativeZ.IsAnyWall)
                            {
                                thisBlockNegativeZ.SetHeight(vertical, BlockEdge.XnZp, otherBlockNegativeZ.GetHeight(vertical, BlockEdge.XnZp) + heightDifference);
                                thisBlockNegativeZ.SetHeight(vertical, BlockEdge.XpZp, otherBlockNegativeZ.GetHeight(vertical, BlockEdge.XpZp) + heightDifference);
                            }
                            if (thisBlockPositiveZ.IsAnyWall)
                            {
                                thisBlockPositiveZ.SetHeight(vertical, BlockEdge.XnZn, otherBlockPositiveZ.GetHeight(vertical, BlockEdge.XnZn) + heightDifference);
                                thisBlockPositiveZ.SetHeight(vertical, BlockEdge.XpZn, otherBlockPositiveZ.GetHeight(vertical, BlockEdge.XpZn) + heightDifference);
                            }
                        }
                    }
                    foreach (Room room in roomsToHandle)
                        foreach (PositionBasedObjectInstance @object in room.Objects.ToList())
                        {
                            PositionBasedObjectInstance objectToPlace = @object;
                            // Remove the objects or clone it, if we need it for the alternated room. (If it's not 'CopyToAlternateRooms', we can remove it after using it)
                            if (removeObjectsInNotAlternatedRooms || room.Alternated || !@object.CopyToAlternateRooms)
                                room.RemoveObjectAndSingularPortalAndKeepAlive(_editor.Level, @object);
                            else
                                objectToPlace = (PositionBasedObjectInstance)(@object.Clone());
                            objectToPlace.Position += room.WorldPos - newRoomToHandle.WorldPos;
                            newRoomToHandle.AddObjectAndSingularPortal(_editor.Level, objectToPlace);
                        }
                    foreach (Room room in new[] { newRoomToHandle }.Concat(roomsToHandle)) // We also need to handle the base room.
                        foreach (SectorBasedObjectInstance sectorObject in room.SectorObjects.ToList())
                        {
                            // Skip internal portals between the merged rooms
                            if (@sectorObject is PortalInstance)
                            {
                                var adjoiningRoom = ((PortalInstance)@sectorObject).AdjoiningRoom;
                                if (rooms.Contains(adjoiningRoom))
                                    continue;
                                if (adjoiningRoom.AlternateOpposite != null)
                                    if (rooms.Contains(adjoiningRoom.AlternateOpposite))
                                        continue;
                            }

                            // Remove the objects or clone it, if we need it for the alternated room. (If it's not 'CopyToAlternateRooms', we can remove it after using it)
                            SectorBasedObjectInstance sectorObjectToPlace = sectorObject;
                            if (removeObjectsInNotAlternatedRooms || room.Alternated || !sectorObjectToPlace.CopyToAlternateRooms)
                                room.RemoveObjectAndSingularPortalAndKeepAlive(_editor.Level, @sectorObject);
                            else
                                sectorObjectToPlace = (SectorBasedObjectInstance)(sectorObject.Clone());

                            // Split sector based objects if there are overlaps now
                            var portalInstance = sectorObjectToPlace as PortalInstance;
                            Func<VectorInt2, bool> sectorToBeCoveredTest;
                            if (portalInstance == null ||
                                portalInstance.Direction == PortalDirection.Ceiling ||
                                portalInstance.Direction == PortalDirection.Floor)
                            {
                                sectorToBeCoveredTest = pos =>
                                {
                                    Room sectorFromRoom = newSectorMap.TryGetOrDefault(pos + room.SectorPos);
                                    if (sectorFromRoom == room) // Must be from that room.
                                        return true;
                                    if (sectorFromRoom != null)
                                        return false;

                                    // Or it must be the only room with a wall here.
                                    foreach (Room roomToHandle2 in roomsToHandle)
                                        if (roomToHandle2 != room)
                                            if (roomToHandle2.WorldArea.Contains(pos))
                                                return false;
                                    return true;
                                };
                            }
                            else
                            {
                                sectorToBeCoveredTest = pos =>
                                {
                                    if ((pos.X + room.SectorPos.X) != newRoom.SectorPos.X && // It must be at the border
                                        (pos.Y + room.SectorPos.Y) != newRoom.SectorPos.Y &&
                                        (pos.X + room.SectorPos.X) != newRoom.SectorPos.X + newRoom.SectorSize.X - 1 &&
                                        (pos.Y + room.SectorPos.Y) != newRoom.SectorPos.Y + newRoom.SectorSize.Y - 1)
                                        return false;
                                    if (newSectorMap.ContainsKey(pos + room.SectorPos)) // There must be a wall at the wall portal
                                        return false;
                                    VectorInt2 inFrontPos = PortalInstance.GetOppositePortalArea(portalInstance.Direction,
                                        new RectangleInt2(pos, pos)).Start;
                                    return newSectorMap.TryGetOrDefault(inFrontPos + room.SectorPos) == room; // There must be the sector from the corresponding room in front of the portal.
                                };
                            }

                            // Add split portals
                            IEnumerable<SectorBasedObjectInstance> newSectorBasedObjects = sectorObjectToPlace.SplitIntoRectangles(sectorToBeCoveredTest, room.SectorPos - newRoom.SectorPos);
                            foreach (SectorBasedObjectInstance sectorBasedObjectInstance in newSectorBasedObjects)
                                newRoomToHandle.AddObjectAndSingularPortal(_editor.Level, sectorBasedObjectInstance);
                        }
                };
            performMergeAction(newRoom, mergeRooms, baseNewSectorMap, !alternated);
            if (alternated)
                performMergeAction(newRoom.AlternateOpposite, mergeRooms?.Select(room => room.AlternateOpposite ?? room), alternateNewSectorMap, true);
            Room.FixupNeighborPortals(_editor.Level, new[] { newRoom }, new[] { newRoom }.Concat(mergeRooms), ref relevantRooms);
            Parallel.ForEach(relevantRooms, relevantRoom => relevantRoom.BuildGeometry());

            // Add room and update the editor
            foreach (Room room in mergeRooms)
                _editor.Level.DeleteRoom(room);
            if (_editor.SelectedRooms.Intersect(rooms).Any())
                _editor.SelectedRoom = newRoom;
            foreach (Room relevantRoom in relevantRooms)
            {
                _editor.RoomPropertiesChange(relevantRoom);
                _editor.RoomSectorPropertiesChange(relevantRoom);
            }
            _editor.RoomListChange();
        }

        public static void SplitRoom(IWin32Window owner)
        {
            if (!CheckForRoomAndBlockSelection(owner))
                return;

            var room = _editor.SelectedRoom;
            room = room.AlternateBaseRoom ?? room;
            RectangleInt2 area = _editor.SelectedSectors.Area.Inflate(1).Intersect(room.LocalArea);

            // Split alternate room
            var relevantRooms = new HashSet<Room>(room.Portals.Select(p => p.AdjoiningRoom));
            Room splitRoom = room.Split(_editor.Level, area);
            _editor.Level.AssignRoomToFree(splitRoom);
            if (room.Alternated)
                _editor.Level.AssignRoomToFree(room.AlternateRoom.Split(_editor.Level, area, splitRoom));

            relevantRooms.Add(room);
            relevantRooms.Add(splitRoom);
            Room.FixupNeighborPortals(_editor.Level, new[] { room, splitRoom }, new[] { room, splitRoom }, ref relevantRooms);
            Parallel.ForEach(relevantRooms, relevantRoom => relevantRoom.BuildGeometry());

            // Cleanup
            if ((_editor.SelectedRoom == room || _editor.SelectedRoom == room.AlternateOpposite) && _editor.SelectedSectors.Valid)
                _editor.SelectedSectors = SectorSelection.None;
            foreach (Room relevantRoom in relevantRooms)
            {
                _editor.RoomPropertiesChange(relevantRoom);
                _editor.RoomSectorPropertiesChange(relevantRoom);
            }
            _editor.RoomListChange();
        }

        public static void SelectConnectedRooms()
        {
            _editor.SelectRooms(_editor.Level.GetConnectedRooms(_editor.SelectedRooms).ToList());
        }

        public static void DuplicateRoom(IWin32Window owner)
        {
            // Prevent "Copy of Copy of Copy" situation
            int foundPostfixPos = _editor.SelectedRoom.Name.IndexOf(" (copy");
            string buffer = string.Empty;
            if (foundPostfixPos != -1)
            {
                int potentialCopyNumber = 0;
                buffer = _editor.SelectedRoom.Name.Substring(foundPostfixPos, _editor.SelectedRoom.Name.Length - foundPostfixPos);
                int enclosingBracketPos = buffer.IndexOf(")");

                if (enclosingBracketPos != -1)
                {
                    var numString = buffer.Substring(6, enclosingBracketPos - 6);
                    bool numberFound = Int32.TryParse(numString, out potentialCopyNumber);
                    if (!numberFound)
                        potentialCopyNumber = 2;
                    else
                        potentialCopyNumber++;
                    buffer = " " + potentialCopyNumber++.ToString();
                }
                else
                    buffer = string.Empty;
            }
            var cutName = _editor.SelectedRoom.Name.Substring(0, foundPostfixPos == -1 ? _editor.SelectedRoom.Name.Length : foundPostfixPos);

            var newRoom = _editor.SelectedRoom.Clone(_editor.Level);
            newRoom.Name = cutName + " (copy" + buffer + ")";
            newRoom.BuildGeometry();
            _editor.Level.AssignRoomToFree(newRoom);
            _editor.RoomListChange();
            _editor.UndoManager.PushRoomCreated(newRoom);
            _editor.SelectedRoom = newRoom;
        }

        public static bool CheckForRoomAndBlockSelection(IWin32Window owner)
        {
            if (_editor.SelectedRoom == null || !_editor.SelectedSectors.Valid)
            {
                _editor.SendMessage("Please select a valid group of sectors.", PopupType.Error);
                return false;
            }
            return true;
        }

        public static bool CheckForLockedRooms(IWin32Window owner, IEnumerable<Room> rooms)
        {
            if (rooms.All(room => !room.Locked))
                return false;
            string lockedRoomList = "Locked rooms: " + string.Join(" ,", rooms.Where(room => room.Locked).Select(s => s.Name));

            if (_editor.Configuration.UI_OnlyShowSmallMessageWhenRoomIsLocked)
            {
                _editor.SendMessage("Can't move rooms because some rooms are locked.\n" + lockedRoomList, PopupType.Info);
                return true;
            }

            // Inform user and offer an option to unlock the room position
            if (DarkMessageBox.Show(owner, "Can't move rooms because some rooms are locked. Unlock and continue?\n" + lockedRoomList, "Locked rooms", MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.No)
                return true;

            // Unlock rooms
            foreach (Room room in rooms)
                if (room.Locked)
                {
                    room.Locked = false;
                    _editor.RoomPropertiesChange(room);
                }
            return true;
        }

        public static void ApplyCurrentAmbientLightToAllRooms()
        {
            foreach (var room in _editor.Level.Rooms.Where(room => room != null))
                room.AmbientLight = _editor.SelectedRoom.AmbientLight;
            Parallel.ForEach(_editor.Level.Rooms.Where(room => room != null), room => room.RoomGeometry?.Relight(room));
            foreach (var room in _editor.Level.Rooms.Where(room => room != null))
                Editor.Instance.RaiseEvent(new Editor.RoomPropertiesChangedEvent { Room = room });
        }

        public static void UpdateLight<T>(Func<LightInstance, T, bool> compareEquals, Action<LightInstance, T> setLightValue, Func<LightInstance, T?> getGuiValue) where T : struct
        {
            var light = _editor.SelectedObject as LightInstance;
            if (light == null)
                return;

            T? newValue = getGuiValue(light);
            if (!newValue.HasValue || compareEquals(light, newValue.Value))
                return;

            setLightValue(light, newValue.Value);
            light.Room.RoomGeometry?.Relight(light.Room);
            _editor.ObjectChange(light, ObjectChangeType.Change);
        }

        public static void EditLightColor(IWin32Window owner)
        {
            UpdateLight<Vector3>((light, value) => light.Color == value, (light, value) => light.Color = value,
                light =>
                {
                    using (var colorDialog = new RealtimeColorDialog(0, 0, c =>
                    {
                        UpdateLight<Vector3>((l, v) => l.Color == v, (l, v) => l.Color = v,
                        l => { return c.ToFloat3Color() * 2.0f; });
                    }, _editor.Configuration.UI_ColorScheme))
                    {
                        colorDialog.Color = new Vector4(light.Color * 0.5f, 1.0f).ToWinFormsColor();

                        var oldLightColor = colorDialog.Color;
                        if (colorDialog.ShowDialog(owner) != DialogResult.OK)
                            colorDialog.Color = oldLightColor;

                        return colorDialog.Color.ToFloat3Color() * 2.0f;
                    }
                });
        }

        public static bool BuildLevel(bool autoCloseWhenDone, IWin32Window owner)
        {
            Level level = _editor.Level;

            if (!level.Settings.Wads.All(wad => wad.Wad != null))
            {
                _editor.SendMessage("Wads are missing. Can't compile level without wads.", PopupType.Error);
                return false;
            }

            if (!level.Settings.Textures.All(texture => texture.IsAvailable))
            {
                _editor.SendMessage("Textures are missing. Can't compile level without textures.", PopupType.Error);
                return false;
            }

            string fileName = level.Settings.MakeAbsolute(level.Settings.GameLevelFilePath);

            if(!Directory.Exists(PathC.GetDirectoryNameTry(fileName)))
            {
                _editor.SendMessage("Specified folder for level file does not exist.\nPlease specify different folder in level settings.", PopupType.Error);
                return false;
            }

            if(string.IsNullOrEmpty(Path.GetExtension(fileName)))
            {
                _editor.SendMessage("Specified level file name is incorrect.\nPlease add an extension.", PopupType.Error);
                return false;
            }

            using (var form = new FormOperationDialog("Build level", autoCloseWhenDone, false,
                progressReporter =>
                {
                    using (var compiler = new LevelCompilerClassicTR(level, fileName, progressReporter))
                    {
                        var watch = new Stopwatch();
                        watch.Start();
                        var statistics = compiler.CompileLevel();
                        watch.Stop();
                        progressReporter.ReportProgress(100, "Elapsed time: " + watch.Elapsed.TotalMilliseconds + "ms");
                        // Raise an event for statistics update
                        Editor.Instance.RaiseEvent(new Editor.LevelCompilationCompletedEvent { InfoString = statistics.ToString() });
                    }

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
            if (IsLaraInLevel())
            {
                if (BuildLevel(true, owner))
                    TombLauncher.Launch(_editor.Level.Settings, owner);
            }
            else
                _editor.SendMessage("No Lara found. Place Lara to play level.", PopupType.Error);
        }

        public static bool IsLaraInLevel()
        {
            return _editor?.Level?.Settings?.WadTryGetMoveable(WadMoveableId.Lara) != null &&
                   _editor.Level.Rooms.Where(room => room != null)
                                      .SelectMany(room => room.Objects)
                                      .Any(obj => obj is ItemInstance && ((ItemInstance)obj).ItemType == new ItemType(WadMoveableId.Lara));
        }

        public static IEnumerable<LevelTexture> AddTexture(IWin32Window owner, IEnumerable<string> predefinedPaths = null)
        {
            List<string> paths = (predefinedPaths ?? LevelFileDialog.BrowseFiles(owner, _editor.Level.Settings,
                PathC.GetDirectoryNameTry(_editor.Level.Settings.LevelFilePath),
                "Load texture files", LevelTexture.FileExtensions, VariableType.LevelDirectory)).ToList();
            if (paths.Count == 0) // Fast track to avoid unnecessary updates
                return new LevelTexture[0];

            // Load textures concurrently
            LevelTexture[] results = new LevelTexture[paths.Count];
            Parallel.For(0, paths.Count, i => results[i] = new LevelTexture(_editor.Level.Settings, paths[i]));

            // Open GUI for texture that couldn't be loaded
            for (int i = 0; i < results.Length; ++i)
                while (results[i]?.LoadException != null)
                    switch (DarkMessageBox.Show(owner, "An error occurred while loading texture file '" + paths[i] + "'." +
                        "\nError message: " + results[i].LoadException.GetType(), "Unable to load texture file.",
                        paths.Count == 1 ? MessageBoxButtons.RetryCancel : MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error,
                        paths.Count == 1 ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1))
                    {
                        case DialogResult.Retry:
                            results[i].Reload(_editor.Level.Settings);
                            break;
                        case DialogResult.Ignore:
                            results[i] = null;
                            break;
                        default:
                            return new LevelTexture[0];
                    }

            // Update level
            _editor.Level.Settings.Textures.AddRange(results.Where(result => result != null));
            _editor.LoadedTexturesChange(results.FirstOrDefault(result => result != null));
            return results.Where(result => result != null);
        }

        public static void ClearAllTexturesInLevel(Level level)
        {
            var emptyTexture = new TextureArea() { Texture = null };
            foreach (var room in level.Rooms)
            {
                if (room != null)
                {
                    var selection = new SectorSelection();
                    selection.Area = new RectangleInt2(VectorInt2.Zero, new VectorInt2(room.NumXSectors - 1, room.NumZSectors - 1));

                    EditorActions.TexturizeAll(room, selection, emptyTexture, BlockFaceType.Floor);
                    EditorActions.TexturizeAll(room, selection, emptyTexture, BlockFaceType.Ceiling);
                    EditorActions.TexturizeAll(room, selection, emptyTexture, BlockFaceType.Wall);
                }
            }
        }

        public static void UpdateTextureFilepath(IWin32Window owner, LevelTexture toReplace)
        {
            var settings = _editor.Level.Settings;
            string path = LevelFileDialog.BrowseFile(owner, settings, toReplace.Path, "Load a texture", LevelTexture.FileExtensions, VariableType.LevelDirectory, false);
            if (String.IsNullOrEmpty(path) || path == toReplace?.Path)
                return;

            toReplace.SetPath(_editor.Level.Settings, path);
            _editor.LoadedTexturesChange(toReplace);
        }

        public static void UnloadTextures(IWin32Window owner)
        {
            if (DarkMessageBox.Show(owner, "Are you sure to unload ALL " + _editor.Level.Settings.Textures.Count +
                " texture files loaded?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            foreach (LevelTexture texture in _editor.Level.Settings.Textures)
                texture.SetPath(_editor.Level.Settings, null);
            _editor.LoadedTexturesChange();
        }

        public static void RemoveTextures(IWin32Window owner)
        {
            if (DarkMessageBox.Show(owner, "Are you sure to DELETE ALL " + _editor.Level.Settings.Textures.Count +
                " texture files loaded? Everything will be untextured.", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _editor.Level.RemoveTextures(texture => true);
            //ClearAllTexturesInLevel(_editor.Level);
            _editor.Level.Settings.Textures.Clear();
            _editor.LoadedTexturesChange();
        }

        public static void RemoveTexture(IWin32Window owner, LevelTexture textureToDelete)
        {
            if (DarkMessageBox.Show(owner, "Are you sure to DELETE the texture " + textureToDelete +
                "? Everything using the texture will be untextured.", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            if (_editor.SelectedTexture.Texture == textureToDelete)
                _editor.SelectedTexture = TextureArea.None;
            _editor.Level.Settings.Textures.Remove(textureToDelete);
            _editor.Level.RemoveTextures(texture => texture == textureToDelete);
            _editor.LoadedTexturesChange(_editor.Level.Settings.Textures.FirstOrDefault());
        }

        public static IEnumerable<ReferencedWad> AddWad(IWin32Window owner, IEnumerable<string> predefinedPaths = null)
        {
            List<string> paths = (predefinedPaths ?? LevelFileDialog.BrowseFiles(owner, _editor.Level.Settings,
                PathC.GetDirectoryNameTry(_editor.Level.Settings.LevelFilePath),
                "Load object files (*.wad)", Wad2.WadFormatExtensions, VariableType.LevelDirectory)).ToList();
            if (paths.Count == 0) // Fast track to avoid unnecessary updates
                return new ReferencedWad[0];

            // Load objects (*.wad files) concurrently
            ReferencedWad[] results = new ReferencedWad[paths.Count];
            GraphicalDialogHandler synchronizedDialogHandler = new GraphicalDialogHandler(owner); // Have only one to synchronize the messages.
            using (var loadingTask = Task.Run(() =>
                Parallel.For(0, paths.Count, i => results[i] = new ReferencedWad(_editor.Level.Settings, paths[i], synchronizedDialogHandler))))
                while (!loadingTask.IsCompleted)
                {
                    Thread.Sleep(1);
                    Application.DoEvents(); // Keep dialog handler responsive, otherwise wad loading can deadlock waiting on GUI thread, while GUI thread is waiting for Parallel.For.
                }

            // Open GUI for objects (*.wad files) that couldn't be loaded
            for (int i = 0; i < results.Length; ++i)
                while (results[i]?.LoadException != null)
                    switch (DarkMessageBox.Show(owner, "An error occurred while loading object file '" + paths[i] + "'." +
                        "\nError message: " + results[i].LoadException.GetType(), "Unable to load object file.",
                        paths.Count == 1 ? MessageBoxButtons.RetryCancel : MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error,
                        paths.Count == 1 ? MessageBoxDefaultButton.Button2 : MessageBoxDefaultButton.Button1))
                    {
                        case DialogResult.Retry:
                            results[i].Reload(_editor.Level.Settings);
                            break;
                        case DialogResult.Ignore:
                            results[i] = null;
                            break;
                        default:
                            return new ReferencedWad[0];
                    }


            // Update level
            _editor.Level.Settings.Wads.InsertRange(0, results.Where(result => result != null));
            _editor.LoadedWadsChange();
            return results.Where(result => result != null);
        }

        public static void UpdateWadFilepath(IWin32Window owner, ReferencedWad toReplace)
        {
            string path = LevelFileDialog.BrowseFile(owner, _editor.Level.Settings, toReplace.Path,
                "Load an object file (*.wad)", Wad2.WadFormatExtensions, VariableType.LevelDirectory, false);
            if (path == toReplace?.Path)
                return;
            toReplace.SetPath(_editor.Level.Settings, path);
            _editor.LoadedWadsChange();
        }

        public static void RemoveWads(IWin32Window owner)
        {
            if (DarkMessageBox.Show(owner, "Are you sure to delete ALL " + _editor.Level.Settings.Wads.Count +
                " wad files loaded?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;
            _editor.Level.Settings.Wads.Clear();
            _editor.LoadedWadsChange();
        }

        public static void ReloadWads(IWin32Window owner)
        {
            var dialogHandler = new GraphicalDialogHandler(owner);
            foreach (var wad in _editor.Level.Settings.Wads)
                wad.Reload(_editor.Level.Settings, dialogHandler);
            _editor.LoadedWadsChange();
        }

        public static bool EnsureNoOutsidePortalsInSelecton(IWin32Window owner)
        {
            return Room.RemoveOutsidePortals(_editor.Level, _editor.SelectedRooms, list =>
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
                room.BuildGeometry();
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
                _editor.SendMessage("No object selected. \nYou have to select position-based object before you can cut or copy it.", PopupType.Info);
                return;
            }

            if (_editor.SelectedObject == null && instance == null)
                return;

            if (_editor.SelectedObject == null && instance != null)
            {
                _editor.SelectedObject = instance;
                EditorActions.BookmarkObject(instance);
            }

            Clipboard.SetDataObject(new ObjectClipboardData(_editor));
        }

        public static void TryCopySectors(SectorSelection selection, IWin32Window owner)
        {
            Clipboard.SetDataObject(new SectorsClipboardData(_editor));
        }

        public static void TryStampObject(ObjectInstance instance, IWin32Window owner)
        {
            if (!(instance is PositionBasedObjectInstance))
            {
                _editor.SendMessage("No object selected. \nYou have to select position-based object before you can copy it.", PopupType.Info);
                return;
            }

            if (_editor.SelectedObject == null && instance == null)
                return;

            if (_editor.SelectedObject == null && instance != null)
            {
                _editor.SelectedObject = instance;
                EditorActions.BookmarkObject(instance);
            }

            _editor.Action = new EditorActionPlace(true, (level, room) => (PositionBasedObjectInstance)instance.Clone());
        }

        public static void TryPasteSectors(SectorsClipboardData data, IWin32Window owner)
        {
            _editor.UndoManager.PushGeometryChanged(_editor.SelectedRoom);

            int x0 = _editor.SelectedSectors.Area.X0;
            int z0 = _editor.SelectedSectors.Area.Y0;
            int x1 = Math.Min(_editor.SelectedRoom.NumXSectors - 1, x0 + data.Width);
            int z1 = Math.Min(_editor.SelectedRoom.NumZSectors - 1, z0 + data.Height);

            var sectors = data.GetSectors();
            var portals = new List<PortalInstance>();

            for (int x = x0; x < x1; x++)
                for (int z = z0; z < z1; z++)
                {
                    var currentSector = sectors[x - x0, z - z0];
                    if (currentSector.Type == BlockType.BorderWall && _editor.SelectedRoom.Blocks[x, z].Type != BlockType.BorderWall)
                        continue;

                    if (_editor.SelectedSectors.Empty ||
                        _editor.SelectedSectors.Single ||
                        _editor.SelectedSectors.Area.Contains(new VectorInt2(x, z)))
                    {
                        portals.AddRange(_editor.SelectedRoom.Blocks[x, z].Portals);
                        _editor.SelectedRoom.Blocks[x, z].ReplaceGeometry(_editor.Level, currentSector);
                    }
                }

            // Redraw rooms in portals
            portals.Select(p => p.AdjoiningRoom).ToList().ForEach(room => { room.BuildGeometry(); _editor.RoomGeometryChange(room); });

            _editor.SelectedRoom.BuildGeometry();
            _editor.RoomSectorPropertiesChange(_editor.SelectedRoom);
        }

        public static bool DragDropFileSupported(DragEventArgs e, bool allow3DImport = false)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return false;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
                if (Wad2.WadFormatExtensions.Matches(file) ||
                    LevelTexture.FileExtensions.Matches(file) ||
                    allow3DImport && ImportedGeometry.FileExtensions.Matches(file) ||
                    file.EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase) ||
                    file.EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase))
                    return true;
            return false;
        }

        public static void MoveLara(IWin32Window owner, Room targetRoom, VectorInt2 p)
        {
            // Search for first Lara and remove her
            MoveableInstance lara;
            foreach (Room room in _editor.Level.Rooms.Where(room => room != null))
                foreach (var instance in room.Objects)
                {
                    lara = instance as MoveableInstance;
                    if (lara != null && lara.WadObjectId == WadMoveableId.Lara)
                    {
                        _editor.UndoManager.PushObjectTransformed(lara);

                        room.RemoveObject(_editor.Level, instance);
                        _editor.ObjectChange(lara, ObjectChangeType.Remove, room);

                        // Move lara to current sector
                        PlaceObjectWithoutUpdate(targetRoom, p, lara);
                        return;
                    }
                }

            // Add lara to current sector
            PlaceObject(targetRoom, p, new MoveableInstance { WadObjectId = WadMoveableId.Lara });
        }

        public static int DragDropCommonFiles(DragEventArgs e, IWin32Window owner)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return -1;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            // Is there a prj file to open?
            string prjFile = files.FirstOrDefault(file => file.EndsWith(".prj", StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(prjFile))
            {
                OpenLevelPrj(owner, prjFile);
                return files.Length - 1;
            }

            // Is there a prj2 file to open?
            string prj2File = files.FirstOrDefault(file => file.EndsWith(".prj2", StringComparison.InvariantCultureIgnoreCase));
            if (!string.IsNullOrEmpty(prjFile))
            {
                OpenLevel(owner, prj2File);
                return files.Length - 1;
            }

            // Are there any more specific files to open?
            // (Process the ones of the same type concurrently)
            IEnumerable<string> wadFiles = files.Where(file => Wad2.WadFormatExtensions.Matches(file));
            IEnumerable<string> textureFiles = files.Where(file => LevelTexture.FileExtensions.Matches(file));
            AddWad(owner, wadFiles.Select(file => _editor.Level.Settings.MakeRelative(file, VariableType.LevelDirectory)));
            AddTexture(owner, textureFiles.Select(file => _editor.Level.Settings.MakeRelative(file, VariableType.LevelDirectory)));
            return files.Length - (wadFiles.Count() + textureFiles.Count()); // Unsupported file count
        }

        public static void SetPortalOpacity(PortalOpacity opacity, IWin32Window owner)
        {
            var portal = _editor.SelectedObject as PortalInstance;
            if (portal == null)
            {
                _editor.SendMessage("No portal selected.", PopupType.Error);
                return;
            }

            portal.Opacity = opacity;
            portal.Room.BuildGeometry();
            _editor.RoomGeometryChange(portal.Room);
            _editor.ObjectChange(portal, ObjectChangeType.Change);
        }

        public static bool SaveLevel(IWin32Window owner, bool askForPath)
        {
            string fileName = _editor.Level.Settings.LevelFilePath;

            // Show save dialog if necessary
            if (askForPath || string.IsNullOrEmpty(fileName))
                fileName = LevelFileDialog.BrowseFile(owner, null, fileName, "Save level", LevelSettings.FileFormatsLevel, null, true);
            if (string.IsNullOrEmpty(fileName))
                return false;

            // Save level
            try
            {
                Prj2Writer.SaveToPrj2(fileName, _editor.Level);
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Unable to save to \"" + fileName + "\".");
                _editor.SendMessage("There was an error while saving project file. Exception: " + exc.Message, PopupType.Error);
                return false;
            }

            // Update state
            if (_editor.Level.Settings.LevelFilePath != fileName)
            {
                AddProjectToRecent(fileName);
                _editor.Level.Settings.LevelFilePath = fileName;
                _editor.LevelFileNameChange();
            }
            _editor.HasUnsavedChanges = false;

            return true;
        }

        public static ItemType? GetCurrentItemWithMessage()
        {
            ItemType? result = _editor.ChosenItem;
            if (result == null)
                _editor.SendMessage("Select an item first.", PopupType.Error);
            return result;
        }

        public static void FindItem()
        {
            ItemType? currentItem = GetCurrentItemWithMessage();
            if (currentItem == null)
                return;

            // Search for matching objects after the previous one
            ObjectInstance previousFind = _editor.SelectedObject;
            ObjectInstance instance = _editor.Level.Rooms
                .Where(room => room != null)
                .SelectMany(room => room.Objects)
                .FindFirstAfterWithWrapAround(
                obj => previousFind == obj,
                obj => obj is ItemInstance && ((ItemInstance)obj).ItemType == currentItem.Value);

            // Show result
            if (instance == null)
                _editor.SendMessage("No object of the selected item type found.", PopupType.Info);
            else
                _editor.ShowObject(instance);
        }

        public static void ExportCurrentRoom(IWin32Window owner)
        {
            ExportRooms(new[] { _editor.SelectedRoom }, owner);
        }

        private static Vector2 GetNormalizedUV(Vector2 uv)
        {
            return new Vector2((int)Math.Round(uv.X), (int)Math.Round(uv.Y) % 256);
        }

        public static void ExportRooms(IEnumerable<Room> rooms, IWin32Window owner)
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
                            BaseGeometryExporter.GetTextureDelegate getTextureCallback = txt =>
                            {
                                if (txt is LevelTexture)
                                    return _editor.Level.Settings.MakeAbsolute(((LevelTexture)txt).Path);
                                else
                                    return "";
                            };

                            BaseGeometryExporter exporter = BaseGeometryExporter.CreateForFile(saveFileDialog.FileName, settingsDialog.Settings, getTextureCallback);

                            // Prepare data for export
                            var model = new IOModel();
                            var texture = _editor.Level.Settings.Textures[0];

                            // Collect all used textures
                            var usedTextures = new List<Texture>();
                            foreach (var room in rooms)
                            {
                                for (int x = 0; x < room.NumXSectors; x++)
                                {
                                    for (int z = 0; z < room.NumZSectors; z++)
                                    {
                                        var block = room.GetBlock(new VectorInt2(x, z));

                                        for (int faceType = 0; faceType < (int)BlockFace.Count; faceType++)
                                        {
                                            var faceTexture = block.GetFaceTexture((BlockFace)faceType);
                                            if (faceTexture.TextureIsInvisible || faceTexture.TextureIsUnavailable || faceTexture.Texture == null)
                                                continue;
                                            if (!usedTextures.Contains(faceTexture.Texture))
                                                usedTextures.Add(faceTexture.Texture);
                                        }
                                    }
                                }
                            }

                            // Now fragment textures in pages
                            for (int j = 0; j < usedTextures.Count; j++)
                            {
                                var t = usedTextures[j];
                                string baseName = Path.GetFileNameWithoutExtension(t.Image.FileName);
                                int pageSize = t.Image.Width;
                                int numPages = (int)Math.Ceiling((float)t.Image.Height / pageSize);

                                for (int i = 0; i < numPages; i++)
                                {
                                    string fileName = baseName + "_" + i + ".png";
                                    int startX = 0;
                                    int width = pageSize;
                                    int startY = i * pageSize;
                                    int height = (i * pageSize + pageSize > t.Image.Height ? t.Image.Height - i * pageSize : pageSize);

                                    ImageC newImage = ImageC.CreateNew(pageSize, pageSize);
                                    newImage.CopyFrom(0, 0, t.Image, startX, startY, width, height);
                                    newImage.Save(fileName);

                                    // Build materials for this texture pahe
                                    var matOpaque = new IOMaterial(Material.Material_Opaque + "_" + j + "_" + i,
                                                                   texture,
                                                                   fileName,
                                                                   i,
                                                                   false,
                                                                   false,
                                                                   0);

                                    var matOpaqueDoubleSided = new IOMaterial(Material.Material_OpaqueDoubleSided + "_" + j + "_" + i, 
                                                                              texture,
                                                                              fileName,
                                                                              i,
                                                                              false,
                                                                              true,
                                                                              0);

                                    var matAdditiveBlending = new IOMaterial(Material.Material_AdditiveBlending + "_" + j + "_" + i, 
                                                                             texture,
                                                                             fileName,
                                                                             i, 
                                                                             true, 
                                                                             false, 
                                                                             0);

                                    var matAdditiveBlendingDoubleSided = new IOMaterial(Material.Material_AdditiveBlendingDoubleSided + "_" + j + "_" + i, 
                                                                                        texture, 
                                                                                        fileName,
                                                                                        i,
                                                                                        true, 
                                                                                        true, 
                                                                                        0);

                                    model.Materials.Add(matOpaque);
                                    model.Materials.Add(matOpaqueDoubleSided);
                                    model.Materials.Add(matAdditiveBlending);
                                    model.Materials.Add(matAdditiveBlendingDoubleSided);
                                }
                            }
                            
                            bool normalizePosition = rooms.Count() == 1;
                            foreach (var room in rooms)
                            {
                                Vector3 deltaPos = Vector3.Zero;
                                if (normalizePosition)
                                    deltaPos = room.WorldPos;

                                var mesh = new IOMesh("TeRoom_" + _editor.Level.Rooms.ReferenceIndexOf(room));
                                mesh.Position = room.WorldPos;

                                // Add submeshes
                                foreach (var material in model.Materials)
                                    mesh.Submeshes.Add(material, new IOSubmesh(material));
                        
                                if (room.RoomGeometry == null)
                                    continue;

                                int lastIndex = 0;
                                for (int x = 0; x < room.NumXSectors; x++)
                                {
                                    for (int z = 0; z < room.NumZSectors; z++)
                                    {
                                        var block = room.GetBlock(new VectorInt2(x, z));

                                        for (int faceType = 0; faceType < (int)BlockFace.Count; faceType++)
                                        {
                                            var faceTexture = block.GetFaceTexture((BlockFace)faceType);
                                            
                                            if (faceTexture.TextureIsInvisible || faceTexture.TextureIsUnavailable || faceTexture.Texture == null)
                                                continue;
                                            var range = room.RoomGeometry.VertexRangeLookup.TryGetOrDefault(new SectorInfo(x, z, (BlockFace)faceType));
                                            var shape = room.GetFaceShape(x, z, (BlockFace)faceType);

                                            if (shape == BlockFaceShape.Quad)
                                            {
                                                int i = range.Start;

                                                var textureArea1 = room.RoomGeometry.TriangleTextureAreas[i / 3];
                                                var textureArea2 = room.RoomGeometry.TriangleTextureAreas[(i + 3) / 3];

                                                if (textureArea1.TextureIsUnavailable || textureArea1.TextureIsInvisible || textureArea1.Texture == null)
                                                    continue;
                                                if (textureArea2.TextureIsUnavailable || textureArea2.TextureIsInvisible || textureArea2.Texture == null)
                                                    continue;

                                                var poly = new IOPolygon(IOPolygonShape.Quad);
                                                poly.Indices.Add(lastIndex + 0);
                                                poly.Indices.Add(lastIndex + 1);
                                                poly.Indices.Add(lastIndex + 2);
                                                poly.Indices.Add(lastIndex + 3);

                                                mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 2] - deltaPos + room.WorldPos);
                                                mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 0] - deltaPos + room.WorldPos);
                                                mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 1] - deltaPos + room.WorldPos);
                                                mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 3] - deltaPos + room.WorldPos);

                                                var uvFactor = new Vector2(0.5f / (float)textureArea1.Texture.Image.Width, 0.5f / (float)textureArea1.Texture.Image.Height);
                                                                      
                                                mesh.UV.Add(GetNormalizedUV(textureArea1.TexCoord2 - uvFactor));
                                                mesh.UV.Add(GetNormalizedUV(textureArea1.TexCoord0 - uvFactor));
                                                mesh.UV.Add(GetNormalizedUV(textureArea1.TexCoord1 - uvFactor));
                                                mesh.UV.Add(GetNormalizedUV(textureArea2.TexCoord0 - uvFactor));

                                                mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 2], 1.0f));
                                                mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 0], 1.0f));
                                                mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 1], 1.0f));
                                                mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 3], 1.0f));

                                                // Get the right submesh
                                                int tile = (int)((textureArea1.TexCoord2 - uvFactor).Y / 256);
                                                var mat = model.GetMaterial(textureArea1.Texture,
                                                                            tile,
                                                                            textureArea1.BlendMode == BlendMode.Additive,
                                                                            textureArea1.DoubleSided,
                                                                            0);
                                                var submesh = mesh.Submeshes[mat];
                                                submesh.Polygons.Add(poly);

                                                lastIndex += 4;
                                            }
                                            else
                                            {
                                                int i = range.Start;

                                                var textureArea = room.RoomGeometry.TriangleTextureAreas[i / 3];
                                                if (textureArea.TextureIsUnavailable || textureArea.TextureIsInvisible || textureArea.Texture == null)
                                                    continue;

                                                var poly = new IOPolygon(IOPolygonShape.Triangle);
                                                poly.Indices.Add(lastIndex);
                                                poly.Indices.Add(lastIndex + 1);
                                                poly.Indices.Add(lastIndex + 2);

                                                i = range.Start;

                                                mesh.Positions.Add(room.RoomGeometry.VertexPositions[i] - deltaPos + room.WorldPos);
                                                mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 1] - deltaPos + room.WorldPos);
                                                mesh.Positions.Add(room.RoomGeometry.VertexPositions[i + 2] - deltaPos + room.WorldPos);

                                                var uvFactor = new Vector2(0.5f / (float)textureArea.Texture.Image.Width, 0.5f / (float)textureArea.Texture.Image.Height);

                                                mesh.UV.Add(GetNormalizedUV(textureArea.TexCoord0 - uvFactor));
                                                mesh.UV.Add(GetNormalizedUV(textureArea.TexCoord1 - uvFactor));
                                                mesh.UV.Add(GetNormalizedUV(textureArea.TexCoord2 - uvFactor));
                                                
                                                mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i], 1.0f));
                                                mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 1], 1.0f));
                                                mesh.Colors.Add(new Vector4(room.RoomGeometry.VertexColors[i + 2], 1.0f));

                                                // Get the right submesh
                                                int tile = (int)((textureArea.TexCoord0 - uvFactor).Y / 256);
                                                var mat = model.GetMaterial(textureArea.Texture,
                                                                            tile,
                                                                            textureArea.BlendMode == BlendMode.Additive,
                                                                            textureArea.DoubleSided,
                                                                            0);
                                                var submesh = mesh.Submeshes[mat];
                                                submesh.Polygons.Add(poly);

                                                lastIndex += 3;
                                            }
                                        }
                                    }
                                }

                                model.Meshes.Add(mesh);
                            }

                            string dbFile = Path.GetDirectoryName(saveFileDialog.FileName) + "\\" + Path.GetFileNameWithoutExtension(saveFileDialog.FileName) + ".xml";

                            if (exporter.ExportToFile(model, saveFileDialog.FileName) /*&& RoomsImportExportXmlDatabase.WriteToFile(dbFile, db)*/)
                            {
                                _editor.SendMessage("Room exported correctly.", PopupType.Info);
                            }
                        }
                    }
                }
            }
        }

        public const string RoomIdentifier = "TeRoom_";

        public static void ImportRooms(IWin32Window owner)
        {
            string importedGeometryPath;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Import rooms";
                openFileDialog.Filter = BaseGeometryImporter.FileExtensions.GetFilter();
                openFileDialog.FileName = _editor.SelectedRoom.Name;
                if (openFileDialog.ShowDialog(owner) != DialogResult.OK)
                    return;
                importedGeometryPath = openFileDialog.FileName;
            }

            // Add imported geometries
            try
            {
                string importedGeometryDirectory = Path.GetDirectoryName(importedGeometryPath);

                var info = ImportedGeometryInfo.Default;
                info.Path = importedGeometryPath;
                info.Name = Path.GetFileNameWithoutExtension(importedGeometryPath);

                ImportedGeometry newObject = new ImportedGeometry();
                _editor.Level.Settings.ImportedGeometryUpdate(newObject, info);
                _editor.Level.Settings.ImportedGeometries.Add(newObject);

                // Translate the vertices to room's origin
                foreach (var mesh in newObject.DirectXModel.Meshes)
                {
                    // Find the room
                    int roomIndex = int.Parse(mesh.Name.Split('_')[1]);
                    var room = _editor.Level.Rooms[roomIndex];

                    // Translate vertices
                    for (int j = 0; j < mesh.Vertices.Count; j++)
                    {
                        var vertex = mesh.Vertices[j];
                        vertex.Position.X -= room.WorldPos.X;
                        vertex.Position.Y -= room.WorldPos.Y;
                        vertex.Position.Z -= room.WorldPos.Z;
                        mesh.Vertices[j] = vertex;
                    }
                }

                // Rebuild DirectX buffer
                newObject.DirectXModel.UpdateBuffers();

                // Load the XML db
                /*string dbName = Path.GetDirectoryName(importedGeometryPath) + "\\" + Path.GetFileNameWithoutExtension(importedGeometryPath) + ".xml";
                var db = RoomsImportExportXmlDatabase.LoadFromFile(dbName);
                if (db == null)
                    throw new FileNotFoundException("There must be also an XML file with the same name of the 3D file");
        */

                // Create a dictionary of the rooms by name
                /* var roomDictionary = new Dictionary<string, IOMesh>();
                 foreach (var msh in newObject.DirectXModel)

                 // Translate rooms
                 for (int i=0;i<db.Rooms.Count;i++)
                 {
                     string roomMeshName = db.Rooms.ElementAt(i).Key;
                     foreach (var mesh in model.Meshes)
                         for (int i = 0; i < mesh.Positions.Count; i++)
                         {
                             var pos = mesh.Positions[i];
                             pos -= mesh.Origin;
                             mesh.Positions[i] = pos;
                         }
                 }
                 */

                // Figure out the relevant rooms
                Dictionary<int, int> roomIndices = new Dictionary<int, int>();
                int meshIndex = 0;
                foreach (ImportedGeometryMesh mesh in newObject.DirectXModel.Meshes)
                {
                    int currentIndex = 0;
                    do
                    {
                        int roomIndexStart = mesh.Name.IndexOf(RoomIdentifier, currentIndex, StringComparison.InvariantCultureIgnoreCase);
                        if (roomIndexStart < 0)
                            break;

                        int roomIndexEnd = roomIndexStart + RoomIdentifier.Length;
                        while (roomIndexEnd < mesh.Name.Length && !char.IsWhiteSpace(mesh.Name[roomIndexEnd]))
                            ++roomIndexEnd;

                        string roomIndexStr = mesh.Name.Substring(roomIndexStart + RoomIdentifier.Length, roomIndexEnd - (roomIndexStart + RoomIdentifier.Length));
                        ushort roomIndex;
                        if (ushort.TryParse(roomIndexStr, out roomIndex))
                        {
                            roomIndices.Add(meshIndex, roomIndex);
                            meshIndex++;
                        }

                        currentIndex = roomIndexEnd;
                    } while (currentIndex < mesh.Name.Length);
                }
                //roomIndices = roomIndices.Distinct().ToList();

                // Add rooms
                foreach (var pair in roomIndices)
                {
                    Room room = _editor.Level.Rooms[pair.Value];
                    var newImported = new ImportedGeometryInstance();
                    newImported.Position = Vector3.Zero;
                    newImported.Model = newObject;
                    newImported.MeshFilter = newObject.DirectXModel.Meshes[pair.Key].Name;
                    room.AddObject(_editor.Level, newImported);
                }
            }
            catch (Exception exc)
            {
                logger.Error(exc.Message);
                _editor.SendMessage("Unable to import rooms from geometry.", PopupType.Error);
            }
        }

        public static void OpenLevel(IWin32Window owner, string fileName = null)
        {
            if (!ContinueOnFileDrop(owner, "Open level"))
                return;

            if (string.IsNullOrEmpty(fileName))
                fileName = LevelFileDialog.BrowseFile(owner, null, fileName, "Open Tomb Editor level", LevelSettings.FileFormatsLevel, null, false);
            if (string.IsNullOrEmpty(fileName))
                return;

            Level newLevel = null;
            try
            {
                using (var form = new FormOperationDialog("Open level", true, true, progressReporter =>
                    newLevel = Prj2Loader.LoadFromPrj2(fileName, progressReporter)))
                {
                    if (form.ShowDialog(owner) != DialogResult.OK || newLevel == null)
                        return;

                    _editor.Level = newLevel;
                    newLevel = null;
                    AddProjectToRecent(fileName);
                    _editor.HasUnsavedChanges = false;
                }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Unable to open \"" + fileName + "\"");

                if(exc is FileNotFoundException)
                {
                    RemoveProjectFromRecent(fileName);
                    _editor.SendMessage("Project file not found!", PopupType.Warning);
                    _editor.LevelFileNameChange();  // Updates recent files on the main form
                }
                else
                    _editor.SendMessage("There was an error while opening project file. File may be in use or may be corrupted. \nException: " + exc.Message, PopupType.Error);
            }
        }

        private static void AddProjectToRecent(string fileName)
        {
            if (Properties.Settings.Default.RecentProjects == null)
                Properties.Settings.Default.RecentProjects = new List<string>();

            Properties.Settings.Default.RecentProjects.RemoveAll(s => s == fileName);
            Properties.Settings.Default.RecentProjects.Insert(0, fileName);

            if (Properties.Settings.Default.RecentProjects.Count > 10)
                Properties.Settings.Default.RecentProjects.RemoveRange(10, Properties.Settings.Default.RecentProjects.Count - 10);

            Properties.Settings.Default.Save();
        }

        private static void RemoveProjectFromRecent(string fileName)
        {
            Properties.Settings.Default.RecentProjects.RemoveAll(s => s == fileName);
            Properties.Settings.Default.Save();
        }

        public static void OpenLevelPrj(IWin32Window owner, string fileName = null)
        {
            if (!ContinueOnFileDrop(owner, "Open level"))
                return;

            if (string.IsNullOrEmpty(fileName))
                fileName = LevelFileDialog.BrowseFile(owner, null, fileName, "Open Tomb Editor level", LevelSettings.FileFormatsLevelPrj, null, false);
            if (string.IsNullOrEmpty(fileName))
                return;

            Level newLevel = null;
            using (var form = new FormOperationDialog("Import PRJ", false, false, progressReporter =>
                newLevel = PrjLoader.LoadFromPrj(fileName, progressReporter, _editor.Configuration.Editor_RespectFlybyPatchOnPrjImport, _editor.Configuration.Editor_UseHalfPixelCorrection)))
            {
                if (form.ShowDialog(owner) != DialogResult.OK || newLevel == null)
                    return;
                _editor.Level = newLevel;
                newLevel = null;
            }
        }

        public static void MoveRooms(VectorInt3 positionDelta, IEnumerable<Room> rooms, bool disableUndo = false)
        {
            if (!disableUndo)
                _editor.UndoManager.PushRoomsMoved(rooms.ToList(), positionDelta);

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
                room.BuildGeometry();
                _editor.RoomSectorPropertiesChange(room);
            }

            foreach (Room room in roomsToMove)
                _editor.RoomGeometryChange(room);
        }

        public static void SplitSectorObjectOnSelection(SectorBasedObjectInstance @object)
        {
            if (@object == null || !_editor.SelectedSectors.Valid)
            {
                _editor.SendMessage("Please select a sector object as well as some sectors.", PopupType.Info);
                return;
            }
            RectangleInt2 area = _editor.SelectedSectors.Area;
            Room room = @object.Room;
            if (!@object.Area.Intersects(area))
            {
                _editor.SendMessage("Object and sectors don't intersect. No operation performed", PopupType.Info);
                return;
            }
            bool wasSelected = _editor.SelectedObject == @object;

            // Figure out where trigger was placed
            int[,] triggerIndices = null;
            if (@object is TriggerInstance)
            {
                triggerIndices = new int[@object.Area.Width + 1, @object.Area.Height + 1];
                for (int x = 0; x <= @object.Area.Width; ++x)
                    for (int z = 0; z <= @object.Area.Height; ++z)
                        triggerIndices[x, z] = room.Blocks[x + @object.Area.X0, z + @object.Area.Y0].Triggers.IndexOf((TriggerInstance)@object);
            }

            // Handle both base room and alternated room
            var newObjects = new List<ObjectInstance>();
            SectorBasedObjectInstance newObject = null;
            Action<Room, SectorBasedObjectInstance> handleRoomVersion = (roomVersion, objectVersion) =>
            {
                // Remove object
                roomVersion.RemoveObjectAndSingularPortal(_editor.Level, objectVersion);
                _editor.ObjectChange(objectVersion, ObjectChangeType.Remove, roomVersion);

                // Readd in smaller pieces
                foreach (SectorBasedObjectInstance splitInstance in objectVersion.SplitIntoRectangles(pos => !area.Contains(pos), new VectorInt2()))
                    newObjects.Add(roomVersion.AddObjectAndSingularPortal(_editor.Level, splitInstance));
                newObject = objectVersion.Clone(objectVersion.Area.Intersect(area));
                newObjects.Add(roomVersion.AddObjectAndSingularPortal(_editor.Level, newObject));
            };
            if (@object is PortalInstance && room.Alternated)
                handleRoomVersion(room.AlternateOpposite, ((PortalInstance)@object).FindAlternatePortal(room.AlternateOpposite));
            handleRoomVersion(room, @object);

            // Handle special objects
            if (@object is PortalInstance)
            {
                var relevantRooms = new HashSet<Room> { room, ((PortalInstance)@object).AdjoiningRoom };
                Room.FixupNeighborPortals(_editor.Level, new[] { room }, new[] { room }, ref relevantRooms);
                Parallel.ForEach(relevantRooms, relevantRoom => relevantRoom.BuildGeometry());
                foreach (Room relevantRoom in relevantRooms)
                    _editor.RoomPropertiesChange(relevantRoom);
            }
            else if (@object is TriggerInstance)
            {
                // Make sure that the trigger is in the same place in the trigger list.
                // We don't want to change the order.
                for (int x = 0; x <= @object.Area.Width; ++x)
                    for (int z = 0; z <= @object.Area.Height; ++z)
                    {
                        var triggersOnSector = room.Blocks[x + @object.Area.X0, z + @object.Area.Y0].Triggers;
                        TriggerInstance last = triggersOnSector[triggersOnSector.Count - 1];
                        triggersOnSector.RemoveAt(triggersOnSector.Count - 1);
                        triggersOnSector.Insert(triggerIndices[x, z], last);
                    }
            }

            // Update
            _editor.RoomSectorPropertiesChange(room);
            if (room.AlternateOpposite != null)
                _editor.RoomSectorPropertiesChange(room.AlternateOpposite);
            _editor.ObjectChange(newObjects, ObjectChangeType.Add);
            if (wasSelected)
                _editor.SelectedObject = newObject;
        }

        public static void BookmarkObject(ObjectInstance objectToBookmark)
        {
            _editor.BookmarkedObject = objectToBookmark;
            _editor.SendMessage("Object bookmarked: " + _editor.BookmarkedObject, PopupType.Info);
        }

        public static void SwitchToolOrdered(int toolIndex)
        {
            if (_editor.Mode == EditorMode.Map2D || toolIndex > (int)EditorToolType.Terrain ||
                _editor.Mode != EditorMode.Geometry && toolIndex > 6)
                return;

            EditorTool currentTool = new EditorTool() { Tool = _editor.Tool.Tool, TextureUVFixer = _editor.Tool.TextureUVFixer, GridSize = _editor.Tool.GridSize };

            switch (toolIndex)
            {
                case 0:
                    currentTool.Tool = EditorToolType.Selection;
                    break;
                case 1:
                    currentTool.Tool = EditorToolType.Brush;
                    break;
                case 2:
                    if (_editor.Mode == EditorMode.Geometry)
                        currentTool.Tool = EditorToolType.Shovel;
                    else
                        currentTool.Tool = EditorToolType.Pencil;
                    break;
                case 3:
                    if (_editor.Mode == EditorMode.Geometry)
                        currentTool.Tool = EditorToolType.Pencil;
                    else
                        currentTool.Tool = EditorToolType.Fill;
                    break;
                case 4:
                    if (_editor.Mode == EditorMode.Geometry)
                        currentTool.Tool = EditorToolType.Flatten;
                    else
                        currentTool.Tool = EditorToolType.GridPaint;
                    break;
                case 5:
                    if (_editor.Mode == EditorMode.Geometry)
                        currentTool.Tool = EditorToolType.Smooth;
                    else
                        currentTool.Tool = EditorToolType.Group;
                    break;
                case 6:
                    if(_editor.Mode == EditorMode.Geometry)
                        currentTool.Tool = EditorToolType.Drag;
                    else
                        currentTool.TextureUVFixer = !currentTool.TextureUVFixer;
                    break;
                default:
                    currentTool.Tool = (EditorToolType)(toolIndex + 4);
                    break;
            }
            _editor.Tool = currentTool;
        }

        public static void SelectWaterRooms()
        {
            IEnumerable<Room> allRooms = _editor.Level.Rooms;
            IEnumerable<Room> waterRooms = allRooms.Where((r, b) => { return r != null && r.Type == RoomType.Water; });
            TrySelectRooms(waterRooms);
        }

        public static void SelectSkyRooms()
        {
            IEnumerable<Room> allRooms = _editor.Level.Rooms;
            IEnumerable<Room> skyRooms = allRooms.Where((r, b) => { return r != null && r.FlagHorizon; });
            TrySelectRooms(skyRooms);
        }

        public static void SelectQuicksandRooms()
        {
            IEnumerable<Room> allRooms = _editor.Level.Rooms;
            IEnumerable<Room> quicksandRooms = allRooms.Where((r, b) => { return r != null && r.Type == RoomType.Quicksand; });
            TrySelectRooms(quicksandRooms);
        }
        public static void SelectOutsideRooms()
        {
            IEnumerable<Room> allRooms = _editor.Level.Rooms;
            IEnumerable<Room> outsideRooms = allRooms.Where((r, b) => { return r != null && r.FlagOutside; });
            TrySelectRooms(outsideRooms);
        }

        public static void SelectRoomsByTags(IWin32Window owner, Editor editor)
        {
            using (var formTags = new FormSelectRoomByTags())
            {
                if (formTags.ShowDialog(owner) != DialogResult.OK)
                    return;

                string[] tags = formTags.tbTagSearch.Text.Split(' ');
                if (tags.Count() < 1)
                    return;

                bool findAllTags = formTags.findAllTags;
                IEnumerable<Room> allRooms = editor.Level.Rooms;
                IEnumerable<Room> matchingRooms = allRooms.Where((r, b) => {
                    if (findAllTags)
                        return r != null && r.Tags.Intersect(tags).Count() == tags.Count();
                    else
                        return r != null && r.Tags.Intersect(tags).Any();
                });
                TrySelectRooms(matchingRooms);
            }
        }

        private static void TrySelectRooms(IEnumerable<Room> rooms)
        {
            if(rooms.Count() > 0)
                _editor.SelectRooms(rooms);
        }

        public static void SetAmbientLightForSelectedRooms(IWin32Window owner)
        {
            IEnumerable<Room> SelectedRooms = _editor.SelectedRooms;
            using (var colorDialog = new RealtimeColorDialog(c =>
            {
                foreach(Room room  in SelectedRooms)
                {
                    room.AmbientLight = c.ToFloat3Color() * 2.0f;
                    room.BuildGeometry();
                    _editor.RoomPropertiesChange(room);
                }
                
            }, _editor.Configuration.UI_ColorScheme))
            {

                if (colorDialog.ShowDialog(owner) == DialogResult.OK)
                    foreach (Room room in SelectedRooms)
                        room.AmbientLight = colorDialog.Color.ToFloat3Color() * 2.0f;

            }
            foreach(Room room in SelectedRooms)
            {
                room.BuildGeometry();
                _editor.RoomPropertiesChange(room);
            }
            
        }
    }
}
