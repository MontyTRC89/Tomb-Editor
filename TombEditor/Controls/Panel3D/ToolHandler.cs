using System;
using System.Drawing;
using System.Linq;
using TombLib.LevelData;
using TombLib;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        private class ToolHandler
        {
            private class ReferenceCell
            {
                public readonly int[,] Heights = new int[9, 4];
                public bool Processed = false;
            }

            private readonly Panel3D _parent;
            private ReferenceCell[,] _actionGrid;
            private Point _referencePosition;
            private Point _newPosition;

            private Point GetQuantizedPosition(int x, int y) => new Point((int)(x * _parent._editor.Configuration.Rendering3D_DragMouseSensitivity), (int)(y * _parent._editor.Configuration.Rendering3D_DragMouseSensitivity));

            // Terrain map resolution must be ALWAYS POWER OF 2 PLUS 1 - this is the requirement of diamond square algorithm.
            public float[,] RandomHeightMap;

            public bool Engaged { get; private set; }
            public bool Dragged { get; private set; }
            public bool PositionDiffers(int x, int y) => _newPosition != GetQuantizedPosition(x, y);

            public PickingResultBlock ReferencePicking { get; private set; }
            public Room ReferenceRoom { get; private set; }
            public Block ReferenceBlock => ReferenceRoom.GetBlockTry(ReferencePicking.Pos.X, ReferencePicking.Pos.Y);
            public bool ReferenceIsDiagonalStep => ReferencePicking.BelongsToFloor ? ReferenceBlock.Floor.DiagonalSplit != DiagonalSplit.None : ReferenceBlock.Ceiling.DiagonalSplit != DiagonalSplit.None;
            public bool ReferenceIsOppositeDiagonalStep
            {
                get
                {
                    if (ReferenceIsDiagonalStep)
                    {
                        if (ReferencePicking.BelongsToFloor)
                        {
                            switch (ReferenceBlock.Floor.DiagonalSplit)
                            {
                                case DiagonalSplit.XnZp:
                                    if ((ReferencePicking.Face is BlockFace.Floor_Triangle2 or
                                        BlockFace.Wall_NegativeZ_QA or
                                        BlockFace.Wall_PositiveX_QA) ||
                                        ReferencePicking.Face.IsSpecificFloorSubdivision(Direction.NegativeZ) ||
                                        ReferencePicking.Face.IsSpecificFloorSubdivision(Direction.PositiveX))
                                        return true;
                                    break;
                                case DiagonalSplit.XpZn:
                                    if ((ReferencePicking.Face is BlockFace.Floor or
                                        BlockFace.Wall_NegativeX_QA or
                                        BlockFace.Wall_PositiveZ_QA) ||
                                        ReferencePicking.Face.IsSpecificFloorSubdivision(Direction.NegativeX) ||
                                        ReferencePicking.Face.IsSpecificFloorSubdivision(Direction.PositiveZ))
                                        return true;
                                    break;
                                case DiagonalSplit.XpZp:
                                    if ((ReferencePicking.Face is BlockFace.Floor_Triangle2 or
                                        BlockFace.Wall_NegativeX_QA or
                                        BlockFace.Wall_NegativeZ_QA) ||
                                        ReferencePicking.Face.IsSpecificFloorSubdivision(Direction.NegativeX) ||
                                        ReferencePicking.Face.IsSpecificFloorSubdivision(Direction.NegativeZ))
                                        return true;
                                    break;
                                case DiagonalSplit.XnZn:
                                    if ((ReferencePicking.Face is BlockFace.Floor or
                                        BlockFace.Wall_PositiveX_QA or
                                        BlockFace.Wall_PositiveZ_QA) ||
                                        ReferencePicking.Face.IsSpecificFloorSubdivision(Direction.PositiveX) ||
                                        ReferencePicking.Face.IsSpecificFloorSubdivision(Direction.PositiveZ))
                                        return true;
                                    break;
                            }
                        }
                        else
                        {
                            switch (ReferenceBlock.Ceiling.DiagonalSplit)
                            {
                                case DiagonalSplit.XnZp:
                                    if ((ReferencePicking.Face is BlockFace.Ceiling_Triangle2 or
                                        BlockFace.Wall_NegativeZ_WS or
                                        BlockFace.Wall_PositiveX_WS) ||
                                        ReferencePicking.Face.IsSpecificCeilingSubdivision(Direction.NegativeZ) ||
                                        ReferencePicking.Face.IsSpecificCeilingSubdivision(Direction.PositiveX))
                                        return true;
                                    break;
                                case DiagonalSplit.XpZn:
                                    if ((ReferencePicking.Face is BlockFace.Ceiling or
                                        BlockFace.Wall_NegativeX_WS or
                                        BlockFace.Wall_PositiveZ_WS) ||
                                        ReferencePicking.Face.IsSpecificCeilingSubdivision(Direction.NegativeX) ||
                                        ReferencePicking.Face.IsSpecificCeilingSubdivision(Direction.PositiveZ))
                                        return true;
                                    break;
                                case DiagonalSplit.XpZp:
                                    if ((ReferencePicking.Face is BlockFace.Ceiling_Triangle2 or
                                        BlockFace.Wall_NegativeX_WS or
                                        BlockFace.Wall_NegativeZ_WS) ||
                                        ReferencePicking.Face.IsSpecificCeilingSubdivision(Direction.NegativeX) ||
                                        ReferencePicking.Face.IsSpecificCeilingSubdivision(Direction.NegativeZ))
                                        return true;
                                    break;
                                case DiagonalSplit.XnZn:
                                    if ((ReferencePicking.Face is BlockFace.Ceiling or
                                        BlockFace.Wall_PositiveX_WS or
                                        BlockFace.Wall_PositiveZ_WS) ||
                                        ReferencePicking.Face.IsSpecificCeilingSubdivision(Direction.PositiveX) ||
                                        ReferencePicking.Face.IsSpecificCeilingSubdivision(Direction.PositiveZ))
                                        return true;
                                    break;
                            }
                        }
                    }
                    return false;
                }
            }

            public ToolHandler(Panel3D parent)
            {
                _parent = parent;
            }

            private void PrepareActionGrid()
            {
                _actionGrid = new ReferenceCell[ReferenceRoom.NumXSectors, ReferenceRoom.NumZSectors];
                for (int x = 0; x < _actionGrid.GetLength(0); x++)
                    for (int z = 0; z < _actionGrid.GetLength(1); z++)
                    {
                        _actionGrid[x, z] = new ReferenceCell();
                        for (BlockEdge edge = 0; edge < BlockEdge.Count; edge++)
                            if (ReferencePicking.BelongsToFloor)
                            {
                                _actionGrid[x, z].Heights[0, (int)edge] = ReferenceRoom.Blocks[x, z].Floor.GetHeight(edge);

                                for (int i = 0; i < ReferenceRoom.Blocks[x, z].ExtraFloorSubdivisions.Count; i++)
                                    _actionGrid[x, z].Heights[i + 1, (int)edge] = ReferenceRoom.Blocks[x, z].GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), edge);
                            }
                            else
                            {
                                _actionGrid[x, z].Heights[0, (int)edge] = ReferenceRoom.Blocks[x, z].Ceiling.GetHeight(edge);

                                for (int i = 0; i < ReferenceRoom.Blocks[x, z].ExtraCeilingSubdivisions.Count; i++)
                                    _actionGrid[x, z].Heights[i + 1, (int)edge] = ReferenceRoom.Blocks[x, z].GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), edge);
                            }
                    }
            }

            private void GenerateNewTerrain()
            {
                // Algorithm used here is naive Diamond-Square, which should be enough for low-res TR geometry.

                int s = RandomHeightMap.GetLength(0) - 1;

                if ((s & s - 1) != 0)
                    throw new Exception("Wrong heightmap size defined for Diamond-Square algorithm. Must be power of 2.");

                float range = 1.0f;
                float rough = 0.9f;
                float average = 0.0f;

                Random rndValue = new Random();
                Array.Clear(RandomHeightMap, 0, RandomHeightMap.Length);

                // While the side length is greater than 1
                for (int sideLength = s; sideLength > 1; sideLength /= 2)
                {
                    int halfSide = sideLength / 2;

                    // Run Diamond Step
                    for (int x = 0; x < s; x += sideLength)
                        for (int y = 0; y < s; y += sideLength)
                        {
                            // Get the average of the corners
                            average = RandomHeightMap[x, y];
                            average += RandomHeightMap[x + sideLength, y];
                            average += RandomHeightMap[x, y + sideLength];
                            average += RandomHeightMap[x + sideLength, y + sideLength];
                            average /= 4.0f;

                            // Offset by a random value
                            average += ((float)rndValue.NextDouble() - 0.5f) * (2.0f * range);
                            RandomHeightMap[x + halfSide, y + halfSide] = average;
                        }

                    // Run Square Step
                    for (int x = 0; x < s; x += halfSide)
                        for (int y = (x + halfSide) % sideLength; y < s; y += sideLength)
                        {
                            // Get the average of the corners
                            average = RandomHeightMap[(x - halfSide + s) % s, y];
                            average += RandomHeightMap[(x + halfSide) % s, y];
                            average += RandomHeightMap[x, (y + halfSide) % s];
                            average += RandomHeightMap[x, (y - halfSide + s) % s];
                            average /= 4.0f;

                            // Offset by a random value
                            average += ((float)rndValue.NextDouble() - 0.5f) * (2.0f * range);

                            // Set the height value to be the calculated average
                            RandomHeightMap[x, y] = average + range;

                            // Set the height on the opposite edge if this is an edge piece
                            if (x == 0)
                                RandomHeightMap[s, y] = average;
                            if (y == 0)
                                RandomHeightMap[x, s] = average;
                        }

                    // Lower the random value range
                    range -= range * 0.5f * rough;
                }

                // Hacky postprocess first point to be in sync during scaling operations
                RandomHeightMap[0, 0] = (RandomHeightMap[0, 1] + RandomHeightMap[1, 0]) / 2.0f;
            }

            private void RelocatePicking()
            {
                // We need to relocate picked diagonal faces, because behaviour is undefined
                // for these cases if diagonal step was raised above limit and swapped.
                // Also, we relocate middle face pickings for walls to nearest floor or ceiling face.

                if (ReferencePicking.Face is BlockFace.Wall_Diagonal_QA || ReferencePicking.Face.IsSpecificFloorSubdivision(Direction.Diagonal))
                {
                    switch (ReferenceBlock.Floor.DiagonalSplit)
                    {
                        case DiagonalSplit.XnZp:
                        case DiagonalSplit.XpZp:
                            ReferencePicking.Face = BlockFace.Floor;
                            break;
                        case DiagonalSplit.XpZn:
                        case DiagonalSplit.XnZn:
                            ReferencePicking.Face = BlockFace.Floor_Triangle2;
                            break;
                    }
                }
                else if (ReferencePicking.Face is BlockFace.Wall_Diagonal_WS || ReferencePicking.Face.IsSpecificCeilingSubdivision(Direction.Diagonal))
                {
                    switch (ReferenceBlock.Ceiling.DiagonalSplit)
                    {
                        case DiagonalSplit.XnZp:
                        case DiagonalSplit.XpZp:
                            ReferencePicking.Face = BlockFace.Ceiling;
                            break;
                        case DiagonalSplit.XpZn:
                        case DiagonalSplit.XnZn:
                            ReferencePicking.Face = BlockFace.Ceiling_Triangle2;
                            break;
                    }
                }
                else if (ReferencePicking.Face is
                    BlockFace.Wall_NegativeX_Middle or
                    BlockFace.Wall_NegativeZ_Middle or
                    BlockFace.Wall_PositiveX_Middle or
                    BlockFace.Wall_PositiveZ_Middle or
                    BlockFace.Wall_Diagonal_Middle)
                {
                    Direction direction;
                    switch (ReferencePicking.Face)
                    {
                        case BlockFace.Wall_NegativeX_Middle:
                            direction = Direction.NegativeX;
                            break;
                        case BlockFace.Wall_PositiveX_Middle:
                            direction = Direction.PositiveX;
                            break;
                        case BlockFace.Wall_NegativeZ_Middle:
                            direction = Direction.NegativeZ;
                            break;
                        case BlockFace.Wall_PositiveZ_Middle:
                            direction = Direction.PositiveZ;
                            break;
                        default:
                            direction = Direction.Diagonal;
                            break;
                    }

                    var face = EditorActions.GetFaces(ReferenceRoom, ReferencePicking.Pos, direction, BlockFaceType.Wall).First(item => item.Key == ReferencePicking.Face);

                    if (face.Value[0] - ReferencePicking.VerticalCoord > ReferencePicking.VerticalCoord - face.Value[1])
                        switch (ReferenceBlock.Floor.DiagonalSplit)
                        {
                            default:
                            case DiagonalSplit.XnZp:
                            case DiagonalSplit.XpZp:
                                ReferencePicking.Face = BlockFace.Floor;
                                break;
                            case DiagonalSplit.XpZn:
                            case DiagonalSplit.XnZn:
                                ReferencePicking.Face = BlockFace.Floor_Triangle2;
                                break;
                        }
                    else
                        switch (ReferenceBlock.Ceiling.DiagonalSplit)
                        {
                            default:
                            case DiagonalSplit.XnZp:
                            case DiagonalSplit.XpZp:
                                ReferencePicking.Face = BlockFace.Ceiling;
                                break;
                            case DiagonalSplit.XpZn:
                            case DiagonalSplit.XnZn:
                                ReferencePicking.Face = BlockFace.Ceiling_Triangle2;
                                break;
                        }
                }
            }

            public void Engage(int refX, int refY, PickingResultBlock refPicking, bool relocatePicking = true, Room refRoom = null)
            {
                if (!Engaged)
                {
                    Engaged = true;
                    _referencePosition = new Point((int)(refX * _parent._editor.Configuration.Rendering3D_DragMouseSensitivity), (int)(refY * _parent._editor.Configuration.Rendering3D_DragMouseSensitivity));
                    _newPosition = _referencePosition;
                    ReferencePicking = refPicking;
                    ReferenceRoom = refRoom ?? _parent._editor.SelectedRoom;

                    // Relocate picking may be not needed for texture operations (e.g. wall 4x4 painting)
                    if (relocatePicking)
                        RelocatePicking();

                    // Initialize data structures
                    PrepareActionGrid();

                    int randomHeightMapSize = 1;
                    while (randomHeightMapSize < Math.Max(ReferenceRoom.NumXSectors, ReferenceRoom.NumZSectors))
                        randomHeightMapSize *= 2; // Find random height map that is a power of two plus.
                    ++randomHeightMapSize;
                    RandomHeightMap = new float[randomHeightMapSize, randomHeightMapSize];

                    if (_parent._editor.Tool.Tool == EditorToolType.Terrain)
                        GenerateNewTerrain();
                }
            }

            public void Disengage()
            {
                if (Engaged)
                {
                    Engaged = false;
                    Dragged = false;
                    _parent._editor.HighlightedSectors = SectorSelection.None;
                    _parent._renderingCachedRooms.Remove(ReferenceRoom); // To update highlight state
                }
            }

            public bool Process(int x, int y)
            {
                if ((_parent._editor.SelectedSectors.Valid && _parent._editor.SelectedSectors.Area.Contains(new VectorInt2(x, y)) || _parent._editor.SelectedSectors.Empty) && !_actionGrid[x, y].Processed)
                {
                    _actionGrid[x, y].Processed = true;
                    return true;
                }
                else
                    return false;
            }

            public Point? UpdateDragState(int newX, int newY, bool relative, bool highlightSelection = true)
            {
                var newPosition = GetQuantizedPosition(newX, newY);

                if (newPosition != _newPosition)
                {
                    Point delta;
                    if (relative)
                        delta = new Point(Math.Sign(_newPosition.X - newPosition.X), Math.Sign(_newPosition.Y - newPosition.Y));
                    else
                        delta = new Point(_referencePosition.X - newPosition.X, _referencePosition.Y - newPosition.Y);
                    _newPosition = newPosition;
                    Dragged = true;
                    if (highlightSelection)
                        _parent._editor.HighlightedSectors = _parent._editor.SelectedSectors;
                    _parent._renderingCachedRooms.Remove(ReferenceRoom); // To update highlight state
                    return delta;
                }
                else
                    return null;
            }

            public void DiscardEditedGeometry(bool autoUpdate = false)
            {
                for (int x = 0; x < ReferenceRoom.NumXSectors; x++)
                    for (int z = 0; z < ReferenceRoom.NumZSectors; z++)
                    {
                        for (BlockEdge edge = 0; edge < BlockEdge.Count; edge++)
                        {
                            if (ReferencePicking.BelongsToFloor)
                            {
                                ReferenceRoom.Blocks[x, z].Floor.SetHeight(edge, _actionGrid[x, z].Heights[0, (int)edge]);

                                for (int i = 0; i < ReferenceRoom.Blocks[x, z].ExtraFloorSubdivisions.Count; i++)
                                    ReferenceRoom.Blocks[x, z].SetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), edge, _actionGrid[x, z].Heights[i + 1, (int)edge]);
                            }
                            else
                            {
                                ReferenceRoom.Blocks[x, z].Ceiling.SetHeight(edge, _actionGrid[x, z].Heights[0, (int)edge]);

                                for (int i = 0; i < ReferenceRoom.Blocks[x, z].ExtraCeilingSubdivisions.Count; i++)
                                    ReferenceRoom.Blocks[x, z].SetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), edge, _actionGrid[x, z].Heights[i + 1, (int)edge]);
                            }
                        }
                    }

                if (autoUpdate)
                    EditorActions.SmartBuildGeometry(ReferenceRoom, ReferenceRoom.LocalArea);
            }
        }
    }
}
