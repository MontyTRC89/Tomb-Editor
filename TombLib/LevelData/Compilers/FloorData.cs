using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.NG;

namespace TombLib.LevelData.Compilers
{
    public sealed partial class LevelCompilerClassicTR
    {
        // Floordata sequence class is used OPTIONALLY, if agressive floordata
        // packing is enabled in level settings. In this case, similar floordata
        // sequences will be hashed and compared on compiling, which allows several
        // sectors to reference same floordata entry, which in turn DRASTICALLY
        // reduces floordata size (up to 4-5 times).

        private class FloordataSequence
        {
            public List<ushort> FDList { get; private set; } = new List<ushort>();
            private int _hash;

            public void Add(ushort entry)
            {
                FDList.Add(entry);
                RecalculateHash();
            }

            public void AddRange(List<ushort> entry)
            {
                FDList.AddRange(entry);
                RecalculateHash();
            }

            private void RecalculateHash()
            {
                string hash = "";
                FDList.ForEach(entry => hash += entry.ToString() + " ");
                _hash = hash.GetHashCode();
            }

            public override int GetHashCode() => _hash;
            public override bool Equals(object obj) => (obj != null) && (obj is FloordataSequence) && (((FloordataSequence)obj)._hash == _hash);
        }

        private bool IsWallSurroundedByWalls(int x, int z, Room room)
        {
            if (x > 0 && !room.Blocks[x - 1, z].IsAnyWall)
                return false;
            if (z > 0 && !room.Blocks[x, z - 1].IsAnyWall)
                return false;
            if (x < room.NumXSectors - 1 && !room.Blocks[x + 1, z].IsAnyWall)
                return false;
            if (z < room.NumZSectors - 1 && !room.Blocks[x, z + 1].IsAnyWall)
                return false;
            return true;
        }

        private void BuildFloorData()
        {
            ReportProgress(53, "Building floordata");

            // Floordata sequence dictionary is used OPTIONALLY, if agressive floordata packing is on!
            var floorDataDictionary = new Dictionary<FloordataSequence, ushort>();

            // Prepare LUA triggers eventually
            if (_level.Settings.GameVersion == GameVersion.TR5Main)
                PrepareLuaTriggers();

            // Initialize the floordata list and add the dummy entry for walls and sectors without particular things

            if (_level.Settings.AgressiveFloordataPacking)
            {
                if (!floorDataDictionary.ContainsValue(0))
                {
                    var dummy = new FloordataSequence();
                    dummy.Add(0x0000);
                    floorDataDictionary.Add(dummy, 0);
                }
            }
            else
                _floorData.Add(0x0000);

            for (var i = 0; i < _level.Rooms.Length; i++)
            {
                var room = _level.Rooms[i];
                if (room == null)
                    continue;
                var tempRoom = _tempRooms[room];

                // Get all portals
                var ceilingPortals = new List<PortalInstance>();
                for (var z = 0; z < room.NumZSectors; z++)
                    for (var x = 0; x < room.NumXSectors; x++)
                    {
                        var ceilingPortal = room.Blocks[x, z].CeilingPortal;
                        if (ceilingPortal != null && !ceilingPortals.Contains(ceilingPortal))
                            ceilingPortals.Add(ceilingPortal);
                    }

                var tempFloorData = new List<ushort>();
                for (var x = 0; x < room.NumXSectors; x++)
                {
                    for (var z = 0; z < room.NumZSectors; z++)
                    {
                        Block block = room.Blocks[x, z];

                        // If a sector is a wall and this room is a water room,
                        // It must be checked before if on the neighbour sector if there's a ceiling portal
                        // because eventually a vertical portal will be added
                        Room isWallWithCeilingPortal = null;
                        foreach (var portal in ceilingPortals)
                        {
                            // Check if x, z is inside the portal
                            if (!(x >= portal.Area.X0 - 1 &&
                                  z >= portal.Area.Y0 - 1 &&
                                  x <= portal.Area.X0 + portal.Area.Width + 1 &&
                                  z <= portal.Area.Y0 + portal.Area.Height + 1))
                                continue;

                            // Check if this is a wall
                            if (!block.IsAnyWall)
                                continue;

                            // Check if ceiling is traversable or not (now I check only for walls inside rooms)
                            if (x != 0 && z != 0 && x != room.NumXSectors - 1 && z != room.NumZSectors - 1)
                            {
                                var connectionInfo = room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z));
                                if (connectionInfo.TraversableType == Room.RoomConnectionType.NoPortal)
                                {
                                    // Last chance: is above block climbable?
                                    if (block.CeilingPortal != null)
                                    {
                                        Room adjoiningRoom = block.CeilingPortal.AdjoiningRoom;
                                        VectorInt2 adjoiningPos = new VectorInt2(x, z) + (room.SectorPos - adjoiningRoom.SectorPos);
                                        if (adjoiningRoom.Blocks[adjoiningPos.X, adjoiningPos.Y].IsAnyWall)
                                            continue;
                                    }
                                    else
                                        continue;
                                }
                            }

                            // Check if current wall is surrounded by walls
                            if (IsWallSurroundedByWalls(x, z, room))
                                continue;

                            // Get new coordinates
                            Room adjoining = portal.AdjoiningRoom;
                            int x2 = room.Position.X + x - adjoining.Position.X;
                            int z2 = room.Position.Z + z - adjoining.Position.Z;

                            // Check if we are outside the boundaries of adjoining room
                            if (x2 < 0 || z2 < 0 || x2 > adjoining.NumXSectors - 1 || z2 > adjoining.NumZSectors - 1)
                                continue;

                            var adjoiningBlock = adjoining.Blocks[x2, z2];

                            // Now check for a ladder
                            if (block.Type == BlockType.Wall)
                            {
                                // Simplest case, just check for ceiling rooms
                                if (!adjoiningBlock.IsAnyWall)
                                {
                                    isWallWithCeilingPortal = portal.AdjoiningRoom;
                                    break;
                                }
                            }
                            else
                            {
                                // For border walls, we must consider also possible wall portals on ceiling room
                                if (adjoiningBlock.Type == BlockType.BorderWall && adjoiningBlock.WallPortal != null)
                                {
                                    isWallWithCeilingPortal = adjoiningBlock.WallPortal.AdjoiningRoom;
                                    break;
                                }
                                else if (adjoiningBlock.Type == BlockType.Floor)
                                {
                                    isWallWithCeilingPortal = portal.AdjoiningRoom;
                                    break;
                                }
                            }
                        }

                        // Build sector info
                        var sector = GetSector(tempRoom, x, z);
                        sector.Floor = -127;
                        sector.Ceiling = -127;
                        sector.FloorDataIndex = 0;
                        sector.RoomBelow = 255;
                        sector.RoomAbove = 255;

                        var newEntry = new FloordataSequence();

                        if ((block.Type == BlockType.Wall && block.Floor.DiagonalSplit == DiagonalSplit.None) || block.Type == BlockType.BorderWall)
                        { // Sector is a complete wall
                            if (block.WallPortal != null)
                            { // Sector is a wall portal
                                if (block.WallPortal.Opacity != PortalOpacity.SolidFaces)
                                { // Only if the portal is not a Toggle Opacity 1
                                    newEntry.Add(0x8001);
                                    newEntry.Add((ushort)_roomsRemappingDictionary[block.WallPortal.AdjoiningRoom]);
                                }
                            }
                            else if (isWallWithCeilingPortal != null)
                            { // Sector has a ceiling portal on it or near it

                                // Convert sector type to floor with maxed out floor height, as tom2pc/winroomedit does it.
                                // Otherwise, even if tomb4 will work correctly, meta2tr or other custom tools may fail here.
                                sector.Floor = (sbyte)(-room.Position.Y - block.Ceiling.Min);
                                sector.Ceiling = (sbyte)(-room.Position.Y - block.Ceiling.Min);

                                newEntry.Add(0x8001);
                                newEntry.Add((ushort)_roomsRemappingDictionary[isWallWithCeilingPortal]);
                            }
                        }
                        else
                        { // Sector is not a complete wall
                            Room.RoomConnectionType floorPortalType = room.GetFloorRoomConnectionInfo(new VectorInt2(x, z)).TraversableType;
                            Room.RoomConnectionType ceilingPortalType = room.GetCeilingRoomConnectionInfo(new VectorInt2(x, z)).TraversableType;
                            var floorShape = new RoomSectorShape(block.Floor, floorPortalType, block.IsAnyWall);
                            var ceilingShape = new RoomSectorShape(block.Ceiling, ceilingPortalType, block.IsAnyWall);

                            // Floor
                            int floorHeight = -room.Position.Y - GetBalancedRealHeight(floorShape, ceilingShape.Max, false);
                            if (floorHeight < -127 || floorHeight > 127)
                                throw new ApplicationException("Floor height in room '" + room + "' at " + new VectorInt2(x, z) + " is out of range.");
                            sector.Floor = (sbyte)floorHeight;
                            if (floorPortalType != Room.RoomConnectionType.NoPortal)
                            {
                                var portal = block.FloorPortal;
                                int roomIndex = _roomsRemappingDictionary[portal.AdjoiningRoom];
                                if (roomIndex >= 254)
                                    throw new ApplicationException("Passable floor and ceiling portals are unfortunately only possible in the first 255 rooms. Portal " + portal + " can't be added.");
                                sector.RoomBelow = (byte)roomIndex;
                            }

                            // Ceiling
                            int ceilingHeight = -room.Position.Y - GetBalancedRealHeight(ceilingShape, floorShape.Min, true);
                            if (ceilingHeight < -127 || ceilingHeight > 127)
                                throw new ApplicationException("Ceiling height in room '" + room + "' at " + new VectorInt2(x, z) + " is out of range.");
                            sector.Ceiling = (sbyte)ceilingHeight;
                            if (ceilingPortalType != Room.RoomConnectionType.NoPortal)
                            {
                                var portal = block.CeilingPortal;
                                int roomIndex = _roomsRemappingDictionary[portal.AdjoiningRoom];
                                if (roomIndex >= 254)
                                    throw new ApplicationException("Passable floor and ceiling portals are unfortunately only possible in the first 255 rooms. Portal " + portal + " can't be added.");
                                sector.RoomAbove = (byte)roomIndex;
                            }

                            // Calculate the floordata now
                            tempFloorData.Clear();
                            BuildFloorDataForSector(room, block, new VectorInt2(x, z), floorShape, ceilingShape, tempFloorData);
                            if (tempFloorData.Count != 0)
                                newEntry.AddRange(tempFloorData);
                        }

                        // Try to find similar floordata sequence and use it (ONLY if agressive FD packing is enabled)
                        if(_level.Settings.AgressiveFloordataPacking)
                        {
                            ushort index = 0;
                            if (newEntry.FDList.Count != 0 && !floorDataDictionary.TryGetValue(newEntry, out index))
                            {
                                index = (ushort)floorDataDictionary.Keys.Sum(list => list.FDList.Count);
                                floorDataDictionary.Add(newEntry, index);
                            }
                            sector.FloorDataIndex = checked(index);
                        }
                        else if (newEntry.FDList.Count != 0)
                        {
                            sector.FloorDataIndex = checked((ushort)_floorData.Count);
                            _floorData.AddRange(newEntry.FDList);
                        }

                        // Update the sector
                        SaveSector(tempRoom, x, z, sector);
                    }
                }
            }

            // Build final floordata block
            if (_level.Settings.AgressiveFloordataPacking)
                floorDataDictionary.ToList().ForEach(entry => _floorData.AddRange(entry.Key.FDList));

            ReportProgress(58, "    Floordata size: " + _floorData.Count * 2 + " bytes");
        }

        private void BuildFloorDataForSector(Room room, Block block, VectorInt2 pos, RoomSectorShape floorShape, RoomSectorShape ceilingShape, List<ushort> outFloorData)
        {
            int lastFloorDataFunction = -1;

            // Floor collision
            BuildFloorDataCollision(floorShape, ceilingShape.Max, false, outFloorData, ref lastFloorDataFunction, room, pos);

            // Ceiling collision
            BuildFloorDataCollision(ceilingShape, floorShape.Min, true, outFloorData, ref lastFloorDataFunction, room, pos);

            // If sector is Climbable
            if (_level.Settings.GameVersion >= GameVersion.TR2 &&
                (block.Flags & BlockFlags.ClimbAny) != BlockFlags.None)
            {
                ushort climb = 0x06;
                if ((block.Flags & BlockFlags.ClimbPositiveZ) != BlockFlags.None)
                    climb |= 0x0100;
                if ((block.Flags & BlockFlags.ClimbPositiveX) != BlockFlags.None)
                    climb |= 0x0200;
                if ((block.Flags & BlockFlags.ClimbNegativeZ) != BlockFlags.None)
                    climb |= 0x0400;
                if ((block.Flags & BlockFlags.ClimbNegativeX) != BlockFlags.None)
                    climb |= 0x0800;

                lastFloorDataFunction = outFloorData.Count;
                outFloorData.Add(climb);
            }

            // If sector is Death
            if (block.HasFlag(BlockFlags.DeathFire))
            {
                lastFloorDataFunction = outFloorData.Count;
                outFloorData.Add(0x05);
            }

            // If sector is Monkey
            if (_level.Settings.GameVersion >= GameVersion.TR3 &&
                (block.Flags & BlockFlags.Monkey) != BlockFlags.None)
            {
                lastFloorDataFunction = outFloorData.Count;
                outFloorData.Add(0x13);
            }

            // If sector is Beetle
            if (_level.Settings.GameVersion >= GameVersion.TR3 &&
                (block.Flags & BlockFlags.Beetle) != BlockFlags.None)
            {
                lastFloorDataFunction = outFloorData.Count;
                outFloorData.Add(0x15);
            }

            // If sector is Trigger triggerer
            if (_level.Settings.GameVersion >= GameVersion.TR3 &&
                (block.Flags & BlockFlags.TriggerTriggerer) != BlockFlags.None)
            {
                lastFloorDataFunction = outFloorData.Count;
                outFloorData.Add(0x14);
            }

            // Triggers
            var triggers = block.Triggers.Where(t => NgParameterInfo.TriggerIsValid(_level.Settings, t));
            var firstTrigger = triggers.FirstOrDefault();
            if (firstTrigger != null)
            {

                // First, we search if a special trigger exists.
                TriggerInstance found = triggers.FirstOrDefault(t => t.TriggerType == TriggerType.ConditionNg ||
                                                                     t.TriggerType == TriggerType.Switch ||
                                                                     t.TriggerType == TriggerType.Key ||
                                                                     t.TriggerType == TriggerType.Pickup) ?? firstTrigger;
                var sortedTriggers = new List<TriggerInstance>() { found };
                sortedTriggers.AddRange(triggers.Where(trigger => trigger != found));

                {
                    lastFloorDataFunction = outFloorData.Count;

                    // Trigger type and setup are coming from the found trigger. Other triggers are needed only for action.
                    ushort trigger1 = 0x04;
                    switch (found.TriggerType)
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
                        case TriggerType.ConditionNg:   // @FIXME: check if these really use same subfunction?
                            trigger1 |= 0x0c << 8;
                            break;
                        case TriggerType.Skeleton:
                            trigger1 |= 0x0d << 8;
                            break;
                        case TriggerType.TightRope:
                            trigger1 |= 0x0e << 8;
                            break;
                        case TriggerType.Crawl:
                            trigger1 |= 0x0f << 8;
                            break;
                        case TriggerType.Climb:
                            trigger1 |= 0x10 << 8;
                            break;
                        default:
                            throw new Exception("Unknown trigger type found '" + found + "'");
                    }

                    // Do some warnings in case user switches targets and some incompatible triggers are left behind

                    if(_level.Settings.GameVersion != GameVersion.TRNG && found.TriggerType == TriggerType.ConditionNg)
                        _progressReporter.ReportWarn("Level uses 'Condition' trigger type, which is not supported in this game engine.");

                    if(_level.Settings.GameVersion == GameVersion.TRNG && found.TriggerType == TriggerType.Monkey)
                        _progressReporter.ReportWarn("Level uses 'Monkey' trigger type, which was replaced with 'Condition' in this game engine.");

                    if ((_level.Settings.GameVersion != GameVersion.TR5 && _level.Settings.GameVersion != GameVersion.TR5Main) &&
                        (found.TriggerType > TriggerType.ConditionNg && found.TriggerType < TriggerType.Monkey))
                        _progressReporter.ReportWarn("Level uses trigger type '" + found.TriggerType + "', which is not supported in this game engine.");


                    ushort triggerSetup;
                    if (_level.Settings.GameVersion == GameVersion.TRNG)
                    {
                        // NG flipeffects store timer and extra in additional ushort
                        if (found.TargetType == TriggerTargetType.FlipEffect && ((TriggerParameterUshort)found.Timer).Key > 46)
                            triggerSetup = 0;
                        // NG condition trigger uses timer in low byte and extra stored as bits in the high byte
                        else if (found.TriggerType == TriggerType.ConditionNg)
                            triggerSetup = GetTriggerRealTimer(found, 0xffff);
                        // all other triggers work as usual
                        else
                            triggerSetup = GetTriggerRealTimer(found, 0xff);
                    }
                    else
                        triggerSetup = GetTriggerParameter(found.Timer, found, 0xff);
                    
                    triggerSetup |= (ushort)(found.OneShot ? 0x100 : 0);

                    // Omit writing bitmask for ConditionNg, because it uses these bits for keeping EXTRA param.
                    if (found.TriggerType != TriggerType.ConditionNg)
                        triggerSetup |= (ushort)((found.CodeBits & 0x1f) << 9);

                    outFloorData.Add(trigger1);
                    outFloorData.Add(triggerSetup);
                }

                foreach (var trigger in sortedTriggers)
                {
                    ushort trigger2;

                    switch (trigger.TargetType)
                    {
                        case TriggerTargetType.Object:
                            // Trigger for object
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (0 << 10));
                            outFloorData.Add(trigger2);
                            break;
                        case TriggerTargetType.Camera:
                            // Trigger for camera
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (1 << 10));
                            outFloorData.Add(trigger2);

                            // Additional short
                            ushort trigger3 = 0;
                            trigger3 |= GetTriggerParameter(trigger.Timer, trigger, 0xff);
                            trigger3 |= (ushort)(trigger.OneShot ? 0x100 : 0);
                            outFloorData.Add(trigger3);
                            break;
                        case TriggerTargetType.Sink:
                            // Trigger for sink
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (2 << 10));
                            outFloorData.Add(trigger2);
                            break;
                        case TriggerTargetType.FlipMap:
                            // Trigger for flip map
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (3 << 10));
                            outFloorData.Add(trigger2);
                            break;
                        case TriggerTargetType.FlipOn:
                            // Trigger for flip map on
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (4 << 10));
                            outFloorData.Add(trigger2);
                            break;
                        case TriggerTargetType.FlipOff:
                            // Trigger for flip map off
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (5 << 10));
                            outFloorData.Add(trigger2);
                            break;
                        case TriggerTargetType.Target:
                            // Trigger for look at item
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (6 << 10));
                            outFloorData.Add(trigger2);
                            break;
                        case TriggerTargetType.FinishLevel:
                            // Trigger for finish level
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (7 << 10));
                            outFloorData.Add(trigger2);
                            break;
                        case TriggerTargetType.PlayAudio:
                            // Trigger for play soundtrack
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (8 << 10));
                            outFloorData.Add(trigger2);
                            break;
                        case TriggerTargetType.FlipEffect:
                            // Trigger for flip effect
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (9 << 10));
                            outFloorData.Add(trigger2);

                            // TRNG stores flipeffect timer as an extra ushort
                            if (_level.Settings.GameVersion == GameVersion.TRNG)
                            {
                                trigger3 = GetTriggerRealTimer(trigger, 0xffff);
                                outFloorData.Add(trigger3);
                            }

                            break;
                        case TriggerTargetType.Secret:
                            // Trigger for secret found
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (10 << 10));
                            outFloorData.Add(trigger2);
                            break;
                        case TriggerTargetType.ActionNg:
                            // Trigger for action
                            if (_level.Settings.GameVersion == GameVersion.TRNG)
                            {
                                trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (11 << 10));
                                outFloorData.Add(trigger2);

                                trigger2 = GetTriggerRealTimer(trigger, 0xffff);
                                outFloorData.Add(trigger2);
                            }
                            else
                                _progressReporter.ReportWarn("Level uses action trigger '" + trigger + "' which is not supported in this game engine.");
                            break;
                        case TriggerTargetType.FlyByCamera:
                            // Trigger for fly by
                            if (!(trigger.Target is FlybyCameraInstance))
                                throw new Exception("A Flyby trigger must point to a flyby camera! ('" + trigger + "')");
                            var flyByCamera = (FlybyCameraInstance)trigger.Target;
                            trigger2 = (ushort)(flyByCamera.Sequence & 0x3ff | (12 << 10));
                            outFloorData.Add(trigger2);

                            trigger2 = (ushort)(trigger.OneShot ? 0x0100 : 0x00);
                            outFloorData.Add(trigger2);
                            break;
                        case TriggerTargetType.ParameterNg:
                            ushort targetTypeBits = trigger.Target is ObjectInstance ? (ushort)(0 << 10) : (ushort)(13 << 10);
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | targetTypeBits);
                            outFloorData.Add(trigger2);
                            break;
                        case TriggerTargetType.FmvNg:
                            trigger2 = (ushort)(GetTriggerParameter(trigger.Target, trigger, 0x3ff) | (14 << 10));
                            outFloorData.Add(trigger2);
                            break;
                        case TriggerTargetType.LuaScript:
                            trigger2 = (ushort)(0 | (11 << 10));
                            outFloorData.Add(trigger2);
                            // Using trigger parameter as function index would be too limiting (max 512 functions) so we add 
                            // an extra short that allows 32768 functions
                            trigger2 = (ushort)(_luaTriggers.IndexOf(trigger));
                            outFloorData.Add(trigger2);
                            break;
                        default:
                            throw new Exception("Unknown trigger type found '" + trigger + "'");
                    }
                }

                outFloorData[outFloorData.Count - 1] |= 0x8000; // End of the action list
            }

            // Set end of floor data function
            if (lastFloorDataFunction != -1)
                outFloorData[lastFloorDataFunction] |= 0x8000;
        }

        private ushort GetTriggerRealTimer(TriggerInstance trigger, ushort upperBound)
        {
            return NgParameterInfo.EncodeNGRealTimer(trigger.TargetType, trigger.TriggerType,
                (trigger.Target as TriggerParameterUshort)?.Key ?? ushort.MaxValue, ushort.MaxValue,
                upperBoundInner => GetTriggerParameter(trigger.Timer, trigger, upperBoundInner),
                upperBoundInner => GetTriggerParameter(trigger.Extra, trigger, upperBoundInner));
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
                    throw new ArgumentException("Triggering an object of type " + parameter.GetType().Name + " is not supported ('" + triggerDiagnostic + "').");
            }
            else if (parameter is TriggerParameterUshort)
                index = ((TriggerParameterUshort)parameter).Key;
            else
                throw new Exception("Trigger has unrecognized parameter! ('" + triggerDiagnostic + "')");

            if (index > upperBound)
                throw new ArgumentException("Trigger parameter is too big ('" + triggerDiagnostic + "').");
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

            public RoomSectorShape(BlockSurface surface, Room.RoomConnectionType portalType, bool wall)
            {
                HeightXnZn = surface.XnZn;
                HeightXpZn = surface.XpZn;
                HeightXnZp = surface.XnZp;
                HeightXpZp = surface.XpZp;
                SplitDirectionIsXEqualsZ = surface.SplitDirectionIsXEqualsZWithDiagonalSplit;

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

        // Portal type {no, first, second}, {Floor, Ceiling}, {Bisecting, NotBisecting}
        private static readonly byte[,,] FunctionTriangleLookUp = new byte[3, 2, 2] {
                        {{0x07, 0x08}, {0x09, 0x0a}},
                        {{0x0b, 0x0d}, {0x0f, 0x11}},
                        {{0x0c, 0x0e}, {0x10, 0x12}}};

        private ushort TriangleCollisionGetSigned(int Value, Room reportRoom, VectorInt2 reportPos)
        {
            if (Value < -16 || Value > 15)
            {
                _progressReporter.ReportWarn("Triangle collision value outside range in room '" + reportRoom + "' at " + reportPos + ". Triangle is too steep, collision is inaccurate.");
                Value = Math.Max(Math.Min(Value, 15), -16);
            }
            ushort Result = (ushort)Value;
            Result &= 0x1f;
            return Result;
        }

        private ushort TriangleCollisionGetUnsigned(int Value, Room reportRoom, VectorInt2 reportPos)
        {
            if (Value < 0 || Value > 15)
            {
                _progressReporter.ReportWarn("Triangle collision value outside range in room '" + reportRoom + "' at " + reportPos + ". Triangle is too steep, collision is inaccurate.");
                Value = Math.Max(Math.Min(Value, 0), 15);
            }
            return (ushort)Value;
        }

        private void BuildFloorDataCollision(RoomSectorShape shape, int oppositeExtreme, bool isCeiling, List<ushort> outFloorData, ref int lastFloorDataFunction, Room reportRoom, VectorInt2 reportPos)
        {
            if (shape.IsSplit && _level.Settings.GameVersion >= GameVersion.TR3)
            { // Build a triangulated slope
                int bisectingIndex = shape.SplitDirectionIsXEqualsZ ? 1 : 0;
                int portalIndex;
                if (shape.SplitPortalFirst && !shape.SplitPortalSecond)
                    portalIndex = 1;
                else if (!shape.SplitPortalFirst && shape.SplitPortalSecond)
                    portalIndex = 2;
                else
                    portalIndex = 0;

                ushort data0 = FunctionTriangleLookUp[portalIndex, isCeiling ? 1 : 0, bisectingIndex];
                ushort data1 = 0;

                int heightXnZn = shape.HeightXnZn;
                int heightXnZp = shape.HeightXnZp;
                int heightXpZn = shape.HeightXpZn;
                int heightXpZp = shape.HeightXpZp;
                if (shape.SplitDirectionIsXEqualsZ)
                    heightXpZn -= shape.DiagonalStep;
                else
                    heightXpZp -= shape.DiagonalStep;
                if (!isCeiling)
                {
                    heightXnZn = -heightXnZn;
                    heightXnZp = -heightXnZp;
                    heightXpZn = -heightXpZn;
                    heightXpZp = -heightXpZp;
                }

                // https://s18.postimg.org/i66wjol2h/Calculating_t1x.gif
                int lowestY = Math.Max(Math.Max(heightXnZn, heightXpZn), Math.Max(heightXnZp, heightXpZp));

                int t00;
                int t01;
                BuildRoomSectorShape_t00_t01(shape, oppositeExtreme, isCeiling, out t00, out t01);

                // Equalize height for maximum step size
                {
                    int average = (t00 + t01) / 2;
                    t00 -= average;
                    t01 -= average;
                }

                int t10;
                int t11;
                int t12;
                int t13;
                if (isCeiling)
                {
                    t00 = -t00;
                    t01 = -t01;
                    t10 = lowestY - heightXpZp;
                    t11 = lowestY - heightXnZp;
                    t12 = lowestY - heightXnZn;
                    t13 = lowestY - heightXpZn;
                }
                else
                {
                    t10 = lowestY - heightXpZn;
                    t11 = lowestY - heightXnZn;
                    t12 = lowestY - heightXnZp;
                    t13 = lowestY - heightXpZp;
                }

                data0 |= (ushort)(TriangleCollisionGetSigned(t00, reportRoom, reportPos) << 5);
                data0 |= (ushort)(TriangleCollisionGetSigned(t01, reportRoom, reportPos) << 10);
                data1 |= TriangleCollisionGetUnsigned(t10, reportRoom, reportPos);
                data1 |= (ushort)(TriangleCollisionGetUnsigned(t11, reportRoom, reportPos) << 4);
                data1 |= (ushort)(TriangleCollisionGetUnsigned(t12, reportRoom, reportPos) << 8);
                data1 |= (ushort)(TriangleCollisionGetUnsigned(t13, reportRoom, reportPos) << 12);

                lastFloorDataFunction = outFloorData.Count;
                outFloorData.Add(data0);
                outFloorData.Add(data1);
            }
            else if (!shape.IsFlat)
            { // Build a quad slope
                int heightDiffX = shape.HeightXnZp - shape.HeightXnZn;
                int heightDiffY = shape.HeightXpZn - shape.HeightXnZn;
                if (isCeiling)
                    heightDiffX = -heightDiffX;

                if (Math.Abs(heightDiffX) > 127 || Math.Abs(heightDiffY) > 127)
                {
                    _progressReporter.ReportWarn("Quad slope collision value outside range in room '" + reportRoom + "' at " + reportPos + ". The quad is too steep, the collision is inaccurate.");
                    heightDiffX = Math.Min(Math.Max(heightDiffX, -127), 127);
                    heightDiffY = Math.Min(Math.Max(heightDiffY, -127), 127);
                }

                ushort result = 0;
                result |= (ushort)((ushort)heightDiffY & 0xff);
                result |= (ushort)(((ushort)heightDiffX & 0xff) << 8);

                lastFloorDataFunction = outFloorData.Count;
                outFloorData.Add((ushort)(isCeiling ? 0x03 : 0x02));
                outFloorData.Add(result);
            }
        }

        private void BuildRoomSectorShape_t00_t01(RoomSectorShape shape, int oppositeExtreme, bool isCeiling, out int out_t00, out int out_t01)
        {
            int heightXnZn = shape.HeightXnZn;
            int heightXnZp = shape.HeightXnZp;
            int heightXpZn = shape.HeightXpZn;
            int heightXpZp = shape.HeightXpZp;
            if (shape.SplitDirectionIsXEqualsZ)
                heightXpZn -= shape.DiagonalStep;
            else
                heightXpZp -= shape.DiagonalStep;
            if (!isCeiling)
            {
                heightXnZn = -heightXnZn;
                heightXnZp = -heightXnZp;
                heightXpZn = -heightXpZn;
                heightXpZp = -heightXpZp;
            }

            // https://s18.postimg.org/u82adt75l/Calculating_t0x.gif
            int highestY = Math.Min(Math.Min(heightXnZn, heightXpZn), Math.Min(heightXnZp, heightXpZp));

            // Extend triangle as an flat quad that covers the sector. Heighest point of this.
            int highestY_Extended00;
            int highestY_Extended01;
            if (shape.SplitDirectionIsXEqualsZ)
            {
                int extended00 = heightXnZn + heightXpZp - heightXpZn;
                int extended01 = heightXnZn + heightXpZp - heightXnZp;
                highestY_Extended00 = Math.Min
                    (Math.Min(heightXnZn, heightXpZp),
                    Math.Min(heightXpZn, extended00));
                highestY_Extended01 = Math.Min
                    (Math.Min(heightXnZn, heightXpZp),
                    Math.Min(heightXnZp, extended01));
            }
            else
            {
                int extended00 = heightXnZp + heightXpZn - heightXpZp;
                int extended01 = heightXnZp + heightXpZn - heightXnZn;
                highestY_Extended00 = Math.Min
                    (Math.Min(heightXnZp, heightXpZn),
                    Math.Min(heightXpZp, extended00));
                highestY_Extended01 = Math.Min
                    (Math.Min(heightXnZp, heightXpZn),
                    Math.Min(heightXnZn, extended01));
            }
            out_t00 = highestY_Extended00 - highestY + (isCeiling ? shape.DiagonalStep : -shape.DiagonalStep);
            out_t01 = highestY_Extended01 - highestY;

            // Handle walls
            if (shape.SplitWallFirst || shape.SplitWallSecond)
            {
                // Extend approximately a little over half the sector height from both the ceiling and the floor.
                int proposal = Math.Max(Math.Abs(shape.Min - oppositeExtreme), Math.Abs(shape.Max - oppositeExtreme)) / 2 + 1;
                if (shape.SplitWallFirst)
                    out_t01 = out_t00 - proposal;
                else if (shape.SplitWallSecond)
                    out_t00 = out_t01 - proposal;
            }
        }

        private int GetBalancedRealHeight(RoomSectorShape shape, int oppositeExtreme, bool isCeiling)
        {
            int heightXnZn = shape.HeightXnZn;
            int heightXnZp = shape.HeightXnZp;
            int heightXpZn = shape.HeightXpZn;
            int heightXpZp = shape.HeightXpZp;
            if (shape.SplitDirectionIsXEqualsZ)
                heightXpZn -= shape.DiagonalStep;
            else
                heightXpZp -= shape.DiagonalStep;
            int result = isCeiling ?
                Math.Min(Math.Min(heightXnZn, heightXnZp), Math.Min(heightXpZn, heightXpZp)) :
                Math.Max(Math.Max(heightXnZn, heightXnZp), Math.Max(heightXpZn, heightXpZp));

            // Equalize height for maximum step size
            if (shape.IsSplit)
            {
                int t00;
                int t01;
                BuildRoomSectorShape_t00_t01(shape, oppositeExtreme, isCeiling, out t00, out t01);
                int average = (t00 + t01) / 2;
                result += isCeiling ? average : -average;
            }
            return result;
        }
    }
}
