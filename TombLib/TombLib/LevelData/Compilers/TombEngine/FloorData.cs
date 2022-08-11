﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.NG;
using TombLib.Utils;

namespace TombLib.LevelData.Compilers.TombEngine
{
    public sealed partial class LevelCompilerTombEngine
    {
        private void BuildFloorData()
        {
            ReportProgress(53, "Building floordata");

            for (var i = 0; i < _level.Rooms.Length; i++)
            {
                var room = _level.Rooms[i];
                if (room == null)
                    continue;

                var tempRoom = _tempRooms[room];

                for (var x = 0; x < room.NumXSectors; x++)
                {
                    for (var z = 0; z < room.NumZSectors; z++)
                    {
                        Block block = room.Blocks[x, z];

                        // Build sector info
                        var sector = GetSector(tempRoom, x, z);
                        sector.TriggerIndex = -1;

                        if ((block.Type == BlockType.Wall && block.Floor.DiagonalSplit == DiagonalSplit.None) || block.Type == BlockType.BorderWall)
                        {
                            // Sector is a complete wall

                            if (block.WallPortal == null || block.WallPortal.Opacity == PortalOpacity.SolidFaces)
                            {
                                var floorPortalAssigned = false;
                                var ceilingPortalAssigned = false;

                                for (var xAround = Math.Max(0, x - 1); (!floorPortalAssigned || !ceilingPortalAssigned) && xAround <= Math.Min(room.NumXSectors - 1, x + 1); ++xAround)
                                {
                                    for (var zAround = Math.Max(0, z - 1); (!floorPortalAssigned || !ceilingPortalAssigned) && zAround <= Math.Min(room.NumZSectors - 1, z + 1); ++zAround)
                                    {
                                        if (xAround == x && zAround == z)
                                            continue;

                                        if (!floorPortalAssigned)
                                        {
                                            var floorPortal = room.Blocks[xAround, zAround].FloorPortal;
                                            if (floorPortal != null)
                                            {
                                                var adjoiningRoom = floorPortal.AdjoiningRoom;
                                                var pos = new VectorInt2(x, z);
                                                var adjoiningBlock = adjoiningRoom.GetBlockTry(pos + room.SectorPos - adjoiningRoom.SectorPos);

                                                if (adjoiningBlock != null)
                                                {
                                                    sector.FloorCollision.Portals[0] = _roomsRemappingDictionary[adjoiningRoom];
                                                    sector.FloorCollision.Portals[1] = sector.FloorCollision.Portals[0];
                                                    floorPortalAssigned = true;
                                                }
                                            }
                                        }

                                        if (!ceilingPortalAssigned)
                                        {
                                            var ceilingPortal = room.Blocks[xAround, zAround].CeilingPortal;
                                            if (ceilingPortal != null)
                                            {
                                                var adjoiningRoom = ceilingPortal.AdjoiningRoom;
                                                var pos = new VectorInt2(x, z);
                                                var adjoiningBlock = adjoiningRoom.GetBlockTry(pos + room.SectorPos - adjoiningRoom.SectorPos);

                                                if (adjoiningBlock != null)
                                                {
                                                    sector.CeilingCollision.Portals[0] = _roomsRemappingDictionary[adjoiningRoom];
                                                    sector.CeilingCollision.Portals[1] = sector.CeilingCollision.Portals[0];
                                                    ceilingPortalAssigned = true;
                                                }
                                            }
                                        }
                                    }
                                }

                                if ((x == 0 || x == room.NumXSectors - 1) && (z == 0 || z == room.NumZSectors - 1))
                                {
                                    var x2 = x == 0 ? 1 : room.NumXSectors - 2;
                                    var z2 = z == 0 ? 1 : room.NumZSectors - 2;
                                    for (var b = 0; b < 2; ++b)
                                    {
                                        var neighborBlock = b == 0 ? room.Blocks[x2, z] : room.Blocks[x, z2];

                                        if (neighborBlock.WallPortal != null && neighborBlock.WallPortal.Opacity != PortalOpacity.SolidFaces)
                                        {
                                            var adjoiningRoom = neighborBlock.WallPortal.AdjoiningRoom;
                                            var pos = new VectorInt2(x, z);
                                            var adjoiningBlock = adjoiningRoom.GetBlockTry(pos + room.SectorPos - adjoiningRoom.SectorPos);

                                            if (adjoiningBlock != null && (adjoiningBlock.Type != BlockType.BorderWall || adjoiningBlock.WallPortal != null && adjoiningBlock.WallPortal.Opacity != PortalOpacity.SolidFaces))
                                            {
                                                sector.WallPortal = _roomsRemappingDictionary[adjoiningRoom];
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                sector.WallPortal = _roomsRemappingDictionary[block.WallPortal.AdjoiningRoom];
                            }

                            sector.FloorCollision.Planes[0].Z = -room.Position.Y;
                            sector.FloorCollision.Planes[1].Z = -room.Position.Y;
                            sector.CeilingCollision.Planes[0].Z = -room.Position.Y;
                            sector.CeilingCollision.Planes[1].Z = -room.Position.Y;
                        }
                        else
                        {
                            // Sector is not a complete wall

                            Room.RoomConnectionType floorPortalType = room.GetFloorRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType;
                            Room.RoomConnectionType ceilingPortalType = room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z), true).TraversableType;
                            var floorShape = new RoomSectorShape(block, true, floorPortalType, block.IsAnyWall);
                            var ceilingShape = new RoomSectorShape(block, false, ceilingPortalType, block.IsAnyWall);

                            // Floor collision
                            BuildFloorDataCollision(floorShape, false, room, new VectorInt2(x, z));

                            // Ceiling collision
                            BuildFloorDataCollision(ceilingShape, true, room, new VectorInt2(x, z));

                            // Triggers
                            var triggers = BuildTriggers(room, block, new VectorInt2(x, z));
                            if (triggers.Count != 0)
                            {
                                sector.TriggerIndex = checked((ushort)_floorData.Count);
                                _floorData.AddRange(triggers);
                            }
                        }

                        // Update the sector
                        SaveSector(tempRoom, x, z, sector);
                    }
                }
            }

            ReportProgress(58, "    Floordata size: " + _floorData.Count * 2 + " bytes");
        }

        private List<ushort> BuildTriggers(Room room, Block block, VectorInt2 pos)
        {
            var result = new List<ushort>();

            // Collect all valid triggers
            var triggers = block.Triggers.Where(t => NgParameterInfo.TriggerIsValid(_level.Settings, t)).ToList();
           
            // Filter out singular key/switch triggers, as they are technically invalid in engine
            if (triggers.Count == 1 && (triggers[0].TriggerType == TriggerType.Key ||
                                        triggers[0].TriggerType == TriggerType.Switch))
            {
                _progressReporter.ReportWarn("Key or switch trigger in room " + room + " at sector (" + pos.X + "," + pos.Y +
                                             ") has no additional actions and will be ignored.");
            }
            else if (triggers.Count > 0)
            {
                if (triggers.Count > 1) TriggerInstance.SortTriggerList(ref triggers);
                var setupTrigger = triggers[0];

                // Trigger type and setup are coming from the found setup trigger. 
                // Other triggers are needed only for action.

                ushort trigger1 = 0;
                switch (setupTrigger.TriggerType)
                {
                    case TriggerType.Trigger:
                        trigger1 |= 0x00 << 8;
                        break;
                    case TriggerType.Pad:
                        trigger1 |= 0x01 << 8;
                        break;
                    case TriggerType.Switch:
                        trigger1 |= 0x02 << 8;
                        break;
                    case TriggerType.Key:
                        trigger1 |= 0x03 << 8;
                        break;
                    case TriggerType.Pickup:
                        trigger1 |= 0x04 << 8;
                        break;
                    case TriggerType.Heavy:
                        trigger1 |= 0x05 << 8;
                        break;
                    case TriggerType.Antipad:
                        trigger1 |= 0x06 << 8;
                        break;
                    case TriggerType.Combat:
                        trigger1 |= 0x07 << 8;
                        break;
                    case TriggerType.Dummy:
                        trigger1 |= 0x08 << 8;
                        break;
                    case TriggerType.Antitrigger:
                        trigger1 |= 0x09 << 8;
                        break;
                    case TriggerType.HeavySwitch:
                        trigger1 |= 0x0a << 8;
                        break;
                    case TriggerType.HeavyAntitrigger:
                        trigger1 |= 0x0b << 8;
                        break;
                    case TriggerType.Monkey:
                        trigger1 |= 0x0c << 8;
                        break;
                    case TriggerType.TightRope:
                        trigger1 |= 0x0d << 8;
                        break;
                    case TriggerType.Crawl:
                        trigger1 |= 0x0e << 8;
                        break;
                    case TriggerType.Climb:
                        trigger1 |= 0x0f << 8;
                        break;
                    case TriggerType.ConditionNg: // Not supported but still may be present in project
                    case TriggerType.Skeleton:
                        _progressReporter.ReportWarn("Block (" + pos.X + ", " + pos.Y + ") in room " + room.Name + " uses trigger type which is not supported in Tomb Engine.");
                        break;
                    default:
                        throw new Exception("Unknown trigger type found '" + setupTrigger + "'");
                }

                // Do some warnings in case user switches targets and some incompatible trigger targets are left behind

                if (setupTrigger.TargetType == TriggerTargetType.ActionNg ||
                    setupTrigger.TargetType == TriggerTargetType.ParameterNg ||
                    setupTrigger.TargetType == TriggerTargetType.TimerfieldNg ||
                    setupTrigger.TargetType == TriggerTargetType.FmvNg)
                    _progressReporter.ReportWarn("Block (" + pos.X + ", " + pos.Y + ") in room " + room.Name + " uses trigger target which is not supported in Tomb Engine.");

                var triggerSetup = GetTriggerParameter(setupTrigger.Timer, setupTrigger, 0xff);

                triggerSetup |= (ushort)(setupTrigger.OneShot ? 0x100 : 0);

                // Write bitmask
                triggerSetup |= (ushort)((setupTrigger.CodeBits & 0x1f) << 9);

                result.Add(trigger1);
                result.Add(triggerSetup);

                foreach (var trigger in triggers)
                {
                    ushort trigger2 = 0;
                    ushort trigger3 = 0;

                    switch (trigger.TargetType)
                    {
                        case TriggerTargetType.Object:
                            // Trigger for object
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (0 << 10));
                            result.Add(trigger2);
                            break;
                        case TriggerTargetType.Camera:
                            // Trigger for camera
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (1 << 10));
                            result.Add(trigger2);
                            // Additional short
                            trigger3 |= GetTriggerParameter(trigger.Timer, trigger, 0xff);
                            trigger3 |= (ushort)(trigger.OneShot ? 0x100 : 0);
                            result.Add(trigger3);
                            break;
                        case TriggerTargetType.Sink:
                            // Trigger for sink
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (2 << 10));
                            result.Add(trigger2);
                            break;
                        case TriggerTargetType.FlipMap:
                            // Trigger for flip map
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (3 << 10));
                            result.Add(trigger2);
                            break;
                        case TriggerTargetType.FlipOn:
                            // Trigger for flip map on
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (4 << 10));
                            result.Add(trigger2);
                            break;
                        case TriggerTargetType.FlipOff:
                            // Trigger for flip map off
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (5 << 10));
                            result.Add(trigger2);
                            break;
                        case TriggerTargetType.Target:
                            // Trigger for look at item
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (6 << 10));
                            result.Add(trigger2);
                            break;
                        case TriggerTargetType.FinishLevel:
                            // Trigger for finish level
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (7 << 10));
                            result.Add(trigger2);
                            break;
                        case TriggerTargetType.PlayAudio:
                            // Trigger for play soundtrack
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (8 << 10));
                            result.Add(trigger2);
                            break;
                        case TriggerTargetType.FlipEffect:
                            // Trigger for flip effect
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (9 << 10));
                            result.Add(trigger2);
                            break;
                        case TriggerTargetType.Secret:
                            // Trigger for secret found
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (10 << 10));
                            result.Add(trigger2);
                            break;
                        case TriggerTargetType.FlyByCamera:
                            // Trigger for fly by
                            if (!(trigger.Target is FlybyCameraInstance))
                                throw new Exception("A Flyby trigger must point to a flyby camera! ('" + trigger + "')");
                            var flyByCamera = (FlybyCameraInstance)trigger.Target;
                            trigger2 = (ushort)(flyByCamera.Sequence & 0x3ff | (12 << 10));
                            result.Add(trigger2);

                            trigger2 = (ushort)(trigger.OneShot ? 0x0100 : 0x00);
                            result.Add(trigger2);
                            break;
                        case TriggerTargetType.LuaScript:
                            // Trigger for LUA script
                            if (!(trigger.Target is TriggerParameterString))
                            {
                                throw new Exception("A LUA Script trigger must reference an event set! ('" + trigger + "')");
                            }

                            string setName = (trigger.Target as TriggerParameterString).Value;
                            if (!_level.Settings.EventSets.Any(s => s.Name == setName))
                            {
                                _progressReporter.ReportWarn("The trigger at (" + pos.X + ", " + pos.Y + ") in room " + room.Name + " refers to the missing event set '" + setName + "'.");
                                continue;
                            }

                            trigger2 = (ushort)((_level.Settings.EventSets.FindIndex(s => s.Name == setName)) & 0x3ff | (16 << 10));
                            result.Add(trigger2);
                            
                            trigger2 = (ushort)(trigger.OneShot ? 0x0100 : 0x00);
                            result.Add(trigger2);

                            break;

                        default:
                            throw new Exception("Unknown trigger target found '" + trigger + "'");
                    }
                }

                result[result.Count - 1] |= 0x8000; // End of the action list
            }

            return result;
        }

        private ushort GetTriggerParameter(ITriggerParameter parameter, TriggerInstance triggerDiagnostic, ushort upperBound)
        {
            int index;
            if (parameter == null)
                index = 0;
            else if (parameter is Room)
                index = _roomsRemappingDictionary[(Room)parameter];
            else if (parameter is ObjectInstance)
            {
                if (parameter is MoveableInstance)
                {
                    MoveableInstance @object = (MoveableInstance)parameter;
                    bool isAI = @object.WadObjectId.TypeId >= 398 && @object.WadObjectId.TypeId <= 406;
                    var table = isAI ? _aiObjectsTable : _moveablesTable;
                    if (!table.TryGetValue(@object, out index))
                    {
                        _progressReporter.ReportWarn("Trigger '" + triggerDiagnostic + "') referring to illegal moveable '" + @object + "'.");
                        index = 0;
                    }
                }
                else if (parameter is CameraInstance)
                    index = _cameraTable[(CameraInstance)parameter];
                else if (parameter is SinkInstance)
                    index = _sinkTable[(SinkInstance)parameter];
                else if (parameter is FlybyCameraInstance)
                    index = _flybyTable[(FlybyCameraInstance)parameter];
                else if (parameter is StaticInstance)
                {
                    StaticInstance @object = (StaticInstance)parameter;
                    if (@object.ScriptId == null) // Create temporary script ID if necessary
                        index = unchecked((int)_scriptingIdsTable.UpdateWithNewId(@object, null));
                    else
                        index = unchecked((int)@object.ScriptId.Value);
                }
                else
                {
                    _progressReporter.ReportWarn("Triggering an object of type " + parameter.GetType().Name + " is not supported ('" + triggerDiagnostic + "'). Game version mismatch?");
                    return 0;
                }
            }
            else if (parameter is TriggerParameterUshort)
                index = ((TriggerParameterUshort)parameter).Key & upperBound;
            else
                throw new Exception("Trigger has unrecognized parameter! ('" + triggerDiagnostic + "')");

            if (index > upperBound)
            {
                _progressReporter.ReportWarn("Trigger parameter is too big ('" + triggerDiagnostic + "'). Game version mismatch?");
                return 0;
            }
            return (ushort)index;
        }

        private struct RoomSectorShape
        {
            public readonly bool SplitDirectionIsXEqualsZ;
            public readonly bool SplitPortalFirst;
            public readonly bool SplitPortalSecond;
            public readonly bool SplitWallFirst;
            public readonly bool SplitWallSecond;
            public readonly int HeightXnZn;
            public readonly int HeightXnZp;
            public readonly int HeightXpZn;
            public readonly int HeightXpZp;
            public readonly int DiagonalStep;

            public RoomSectorShape(Block block, bool floor, Room.RoomConnectionType portalType, bool wall)
            {
                var surface = floor ? block.Floor : block.Ceiling;

                HeightXnZn = surface.XnZn;
                HeightXpZn = surface.XpZn;
                HeightXnZp = surface.XnZp;
                HeightXpZp = surface.XpZp;
                SplitDirectionIsXEqualsZ = surface.SplitDirectionIsXEqualsZWithDiagonalSplit;

                if (block.HasGhostBlock && block.GhostBlock.Valid)
                {
                    HeightXnZn += floor ? block.GhostBlock.Floor.XnZn : block.GhostBlock.Ceiling.XnZn;
                    HeightXpZn += floor ? block.GhostBlock.Floor.XpZn : block.GhostBlock.Ceiling.XpZn;
                    HeightXnZp += floor ? block.GhostBlock.Floor.XnZp : block.GhostBlock.Ceiling.XnZp;
                    HeightXpZp += floor ? block.GhostBlock.Floor.XpZp : block.GhostBlock.Ceiling.XpZp;
                }

                switch (portalType)
                {
                    case Room.RoomConnectionType.NoPortal:
                        SplitPortalFirst = false;
                        SplitPortalSecond = false;
                        break;
                    case Room.RoomConnectionType.FullPortal:
                        SplitPortalFirst = true;
                        SplitPortalSecond = true;
                        break;
                    case Room.RoomConnectionType.TriangularPortalXnZn:
                        SplitPortalFirst = true;
                        SplitPortalSecond = false;
                        SplitDirectionIsXEqualsZ = false;
                        break;
                    case Room.RoomConnectionType.TriangularPortalXnZp:
                        SplitPortalFirst = true;
                        SplitPortalSecond = false;
                        SplitDirectionIsXEqualsZ = true;
                        break;
                    case Room.RoomConnectionType.TriangularPortalXpZp:
                        SplitPortalFirst = false;
                        SplitPortalSecond = true;
                        SplitDirectionIsXEqualsZ = false;
                        break;
                    case Room.RoomConnectionType.TriangularPortalXpZn:
                        SplitPortalFirst = false;
                        SplitPortalSecond = true;
                        SplitDirectionIsXEqualsZ = true;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (surface.DiagonalSplit)
                {
                    case DiagonalSplit.None:
                        DiagonalStep = 0;
                        SplitWallFirst = wall;
                        SplitWallSecond = wall;
                        break;
                    case DiagonalSplit.XnZn:
                        DiagonalStep = surface.XpZp - surface.XnZp;
                        SplitWallFirst = wall;
                        SplitWallSecond = false;
                        break;
                    case DiagonalSplit.XnZp:
                        DiagonalStep = surface.XpZn - surface.XpZp;

                        SplitWallFirst = wall;
                        SplitWallSecond = false;
                        break;
                    case DiagonalSplit.XpZn:
                        DiagonalStep = surface.XnZp - surface.XnZn;
                        HeightXnZn += DiagonalStep;
                        HeightXpZp += DiagonalStep;
                        DiagonalStep = -DiagonalStep;

                        SplitWallFirst = false;
                        SplitWallSecond = wall;
                        break;
                    case DiagonalSplit.XpZp:
                        DiagonalStep = surface.XnZn - surface.XpZn;
                        HeightXpZn += DiagonalStep;
                        HeightXnZp += DiagonalStep;
                        DiagonalStep = -DiagonalStep;

                        SplitWallFirst = false;
                        SplitWallSecond = wall;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public int Max => Math.Max(Math.Max(HeightXnZn, HeightXnZp), Math.Max(HeightXpZn, HeightXpZp));
            public int Min => Math.Min(Math.Min(HeightXnZn, HeightXnZp), Math.Min(HeightXpZn, HeightXpZp));
            public bool IsFlat => HeightXnZp == HeightXnZn && HeightXpZn == HeightXnZn && HeightXpZp == HeightXnZn &&
                DiagonalStep == 0 && SplitPortalSecond == SplitPortalFirst && SplitWallFirst == SplitWallSecond;
            public bool IsSplit => HeightXnZn - HeightXnZp != HeightXpZn - HeightXpZp || HeightXnZn - HeightXpZn != HeightXnZp - HeightXpZp ||
                DiagonalStep != 0 || SplitPortalSecond != SplitPortalFirst || SplitWallFirst != SplitWallSecond;
        }

        private void BuildFloorDataCollision(RoomSectorShape shape, bool isCeiling, Room reportRoom, VectorInt2 reportPos)
        {
            TombEngineRoom newRoom = _tempRooms[reportRoom];
            TombEngineRoomSector newSector = newRoom.Sectors[newRoom.NumZSectors * reportPos.X + reportPos.Y];
            Block sector = reportRoom.GetBlock(reportPos);
            var newCollision = isCeiling ? newSector.CeilingCollision : newSector.FloorCollision;
            var portal = isCeiling ? sector.CeilingPortal : sector.FloorPortal;

            if (shape.IsSplit)
            {
                if (shape.SplitDirectionIsXEqualsZ)
                {
                    newCollision.SplitAngle = (float)(Math.PI / 4);

                    if (shape.SplitWallFirst)
                    {
                        if (portal != null)
                            newCollision.Portals[0] = _roomsRemappingDictionary[portal.AdjoiningRoom];

                        newCollision.Planes[0].Z = -float.MaxValue;
                    }
                    else
                    {
                        if (shape.SplitPortalFirst)
                            newCollision.Portals[0] = _roomsRemappingDictionary[portal.AdjoiningRoom];

                        newCollision.Planes[0] = GetPlane(
                                new Vector3(-Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXnZp) * Level.HeightUnit, Level.HalfBlockSizeUnit),
                                new Vector3(Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXpZp) * Level.HeightUnit, Level.HalfBlockSizeUnit),
                                new Vector3(-Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXnZn) * Level.HeightUnit, -Level.HalfBlockSizeUnit)
                            );
                    }

                    if (shape.SplitWallSecond)
                    {
                        if (portal != null)
                            newCollision.Portals[1] = _roomsRemappingDictionary[portal.AdjoiningRoom];

                        newCollision.Planes[1].Z = -float.MaxValue;
                    }
                    else
                    {
                        if (shape.SplitPortalSecond)
                            newCollision.Portals[1] = _roomsRemappingDictionary[portal.AdjoiningRoom];

                        newCollision.Planes[1] = GetPlane(
                            new Vector3(Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXpZn) * Level.HeightUnit, -Level.HalfBlockSizeUnit),
                            new Vector3(-Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXnZn - shape.DiagonalStep) * Level.HeightUnit, -Level.HalfBlockSizeUnit),
                            new Vector3(Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXpZp - shape.DiagonalStep) * Level.HeightUnit, Level.HalfBlockSizeUnit)
                        );
                    }
                }
                else
                {
                    newCollision.SplitAngle = (float)(3 * Math.PI / 4);

                    if (shape.SplitWallSecond)
                    {
                        if (portal != null)
                            newCollision.Portals[0] = _roomsRemappingDictionary[portal.AdjoiningRoom];

                        newCollision.Planes[0].Z = -float.MaxValue;
                    }
                    else
                    {
                        if (shape.SplitPortalSecond)
                            newCollision.Portals[0] = _roomsRemappingDictionary[portal.AdjoiningRoom];

                        newCollision.Planes[0] = GetPlane(
                            new Vector3(Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXpZp) * Level.HeightUnit, Level.HalfBlockSizeUnit),
                            new Vector3(Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXpZn - shape.DiagonalStep) * Level.HeightUnit, -Level.HalfBlockSizeUnit),
                            new Vector3(-Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXnZp - shape.DiagonalStep) * Level.HeightUnit, Level.HalfBlockSizeUnit)
                        );
                    }
                    if (shape.SplitWallFirst)
                    {
                        if (portal != null)
                            newCollision.Portals[1] = _roomsRemappingDictionary[portal.AdjoiningRoom];

                        newCollision.Planes[1].Z = -float.MaxValue;
                    }
                    else
                    {
                        if (shape.SplitPortalFirst)
                            newCollision.Portals[1] = _roomsRemappingDictionary[portal.AdjoiningRoom];

                        newCollision.Planes[1] = GetPlane(
                          new Vector3(-Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXnZn) * Level.HeightUnit, -Level.HalfBlockSizeUnit),
                          new Vector3(-Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXnZp) * Level.HeightUnit, Level.HalfBlockSizeUnit),
                          new Vector3(Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXpZn) * Level.HeightUnit, -Level.HalfBlockSizeUnit)
                      );
                    }
                }
            }
            else
            {
                if (shape.SplitPortalFirst && shape.SplitPortalSecond)
                {
                    newCollision.Portals[0] = _roomsRemappingDictionary[portal.AdjoiningRoom];
                    newCollision.Portals[1] = newCollision.Portals[0];
                }

                newCollision.Planes[0] = GetPlane(
                        new Vector3(-Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXnZp) * Level.HeightUnit, Level.HalfBlockSizeUnit),
                        new Vector3(Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXpZp) * Level.HeightUnit, Level.HalfBlockSizeUnit),
                        new Vector3(Level.HalfBlockSizeUnit, (-reportRoom.Position.Y - shape.HeightXpZn) * Level.HeightUnit, -Level.HalfBlockSizeUnit)
                    );
                newCollision.Planes[1] = newCollision.Planes[0];
            }
        }

        private Vector3 GetPlane(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            var plane = Plane.CreateFromVertices(p1, p2, p3);
            return new Vector3(-plane.Normal.X, -plane.Normal.Z, Vector3.Dot(plane.Normal, p1)) / plane.Normal.Y;
        }
    }
}
