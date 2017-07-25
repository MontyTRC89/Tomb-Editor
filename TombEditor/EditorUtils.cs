using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TombEditor.Geometry;

namespace TombEditor
{
    public class EditorUtils
    {
        /*public static void AddPortal(PortalDirection direction, int x, int z, int xb, int zb, int r, int a, ref Level level)
        {
            int xMin = x;
            int xMax = z;
            int zMin = xb;
            int zMax = zb;

            Room room = level.Rooms[r];

            int numXblocks = room.NumXSectors;
            int numZblocks = room.NumZSectors;

            if (xMin < 0) xMin = 0;
            if (xMax > numXblocks - 1) xMax = numXblocks - 1;
            if (zMin < 0) zMin = 0;
            if (zMax > numZblocks - 1) zMax = numZblocks - 1;

            // MURO OVEST
            if (xMin == 0 && xMax == 0 && zMin != 0 && zMax != numZblocks - 1)
            {
                // controllo che si stia sovrapponendo a un portale
                for (int z = zMin; z <= zMax; z++)
                {
                    if (room.Blocks[xMin, z].Type == BlockType.WallPortal)
                    {
                        MessageBox.Show("Can't overlap portals", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                int xPortalWorld = Utils.GetWorldX(room, xMin);
                int zPortalWorld = Utils.GetWorldZ(room, zMin);

                // ora cerco una possibile vicina
                short found = -1;
                for (int i = 0; i < 2048; i++)
                {
                    found = -1;
                    Room otherRoom = _editor.Level.Rooms[i];
                    if (i == _editor.RoomIndex || otherRoom == null) continue;
                    if (otherRoom.Position.X + otherRoom.NumXSectors - 1 == room.Position.X + 1 && room.Position.Z + zMin >= otherRoom.Position.Z + 1 &&
                        room.Position.Z + zMax <= otherRoom.Position.Z + otherRoom.NumZSectors - 1)
                    {
                        for (int z = zMin; z <= zMax; z++)
                        {
                            int facingZ = z + (int)(room.Position.Z - otherRoom.Position.Z);
                            if (facingZ < 1 || facingZ > otherRoom.NumZSectors - 1)
                            {
                                found = -1;
                                break;
                            }

                            if (otherRoom.Blocks[otherRoom.NumXSectors - 1, facingZ].Type == BlockType.WallPortal)
                            {
                                found = -1;
                                break;
                            }
                            else
                            {
                                found = (short)i;
                            }
                        }
                    }

                    if (found != -1) break;
                }

                if (found != -1)
                {
                    Portal currentRoomPortal = new Portal(0, _editor.RoomIndex);
                    Portal otherRoomPortal = new Portal(0, found);
                    Room otherRoom = _editor.Level.Rooms[found];

                    int xPortalOther = (int)(xPortalWorld - otherRoom.Position.X);
                    int zPortalOther = (int)(zPortalWorld - otherRoom.Position.Z);

                    currentRoomPortal.ID = _editor.Level.GetNewPortalID();
                    currentRoomPortal.NumXBlocks = 1;
                    currentRoomPortal.NumZBlocks = (byte)(zMax - zMin + 1);
                    currentRoomPortal.Opacity = PortalOpacity.None;
                    currentRoomPortal.Direction = PortalDirection.West;
                    currentRoomPortal.X = 0;
                    currentRoomPortal.Z = (byte)zMin;
                    currentRoomPortal.AdjoiningRoom = found;

                    _editor.Level.Portals.Add(currentRoomPortal.ID, currentRoomPortal);

                    otherRoomPortal.ID = _editor.Level.GetNewPortalID();
                    otherRoomPortal.NumXBlocks = 1;
                    otherRoomPortal.NumZBlocks = (byte)(zMax - zMin + 1);
                    otherRoomPortal.Opacity = PortalOpacity.None;
                    otherRoomPortal.Direction = PortalDirection.East;
                    otherRoomPortal.X = (byte)(otherRoom.NumXSectors - 1);
                    otherRoomPortal.Z = (byte)zPortalOther;
                    otherRoomPortal.AdjoiningRoom = _editor.RoomIndex;

                    _editor.Level.Portals.Add(otherRoomPortal.ID, otherRoomPortal);

                    currentRoomPortal.OtherID = otherRoomPortal.ID;
                    otherRoomPortal.OtherID = currentRoomPortal.ID;

                    for (int z = zMin; z <= zMax; z++)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].Type = BlockType.WallPortal;
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[0, z].WallPortal = currentRoomPortal.ID;

                        _editor.Level.Rooms[found].Blocks[otherRoom.NumXSectors - 1, z + (int)(room.Position.Z - otherRoom.Position.Z)].Type = BlockType.WallPortal;
                        _editor.Level.Rooms[found].Blocks[otherRoom.NumXSectors - 1, z + (int)(room.Position.Z - otherRoom.Position.Z)].WallPortal = otherRoomPortal.ID;
                    }

                    _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                    _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
                    _editor.Level.Rooms[found].BuildGeometry();
                    _editor.Level.Rooms[found].UpdateBuffers();

                    _editor.DrawPanel3D();
                    _editor.DrawPanelGrid();

                    // labelStatistics.Text = _editor.UpdateStatistics();

                    return;
                }
                else
                {
                    MessageBox.Show("Not a valid portal position", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // MURO EST
            if (xMin == numXblocks - 1 && xMax == xMin && zMin != 0 && zMax != numZblocks - 1)
            {
                // controllo che si stia sovrapponendo a un portale
                for (int z = zMin; z <= zMax; z++)
                {
                    if (room.Blocks[xMin, z].Type == BlockType.WallPortal)
                    {
                        MessageBox.Show("Can't overlap portals", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                int xPortalWorld = Utils.GetWorldX(room, xMin);
                int zPortalWorld = Utils.GetWorldZ(room, zMin);

                // ora cerco una possibile vicina
                short found = -1;
                for (int i = 0; i < 2048; i++)
                {
                    found = -1;
                    Room otherRoom = _editor.Level.Rooms[i];
                    if (i == _editor.RoomIndex || otherRoom == null) continue;
                    if (room.Position.X + room.NumXSectors - 1 == otherRoom.Position.X + 1 && room.Position.Z + zMin >= otherRoom.Position.Z + 1 &&
                        room.Position.Z + zMax <= otherRoom.Position.Z + otherRoom.NumZSectors - 1)
                    {
                        for (int z = zMin; z <= zMax; z++)
                        {
                            int facingZ = z + (int)(room.Position.Z - otherRoom.Position.Z);
                            if (facingZ < 1 || facingZ > otherRoom.NumZSectors - 1)
                            {
                                found = -1;
                                break;
                            }

                            if (otherRoom.Blocks[0, facingZ].Type == BlockType.WallPortal)
                            {
                                found = -1;
                                break;
                            }
                            else
                            {
                                found = (short)i;
                            }
                        }
                    }

                    if (found != -1) break;
                }

                if (found != -1)
                {
                    Portal currentRoomPortal = new Portal(0, _editor.RoomIndex);
                    Portal otherRoomPortal = new Portal(0, found);
                    Room otherRoom = _editor.Level.Rooms[found];

                    int xPortalOther = (int)(xPortalWorld - otherRoom.Position.X);
                    int zPortalOther = (int)(zPortalWorld - otherRoom.Position.Z);

                    currentRoomPortal.ID = _editor.Level.GetNewPortalID();
                    currentRoomPortal.NumXBlocks = 1;
                    currentRoomPortal.NumZBlocks = (byte)(zMax - zMin + 1);
                    currentRoomPortal.Opacity = PortalOpacity.None;
                    currentRoomPortal.Direction = PortalDirection.East;
                    currentRoomPortal.X = (byte)(numXblocks - 1);
                    currentRoomPortal.Z = (byte)zMin;
                    currentRoomPortal.AdjoiningRoom = found;

                    _editor.Level.Portals.Add(currentRoomPortal.ID, currentRoomPortal);

                    otherRoomPortal.ID = _editor.Level.GetNewPortalID();
                    otherRoomPortal.NumXBlocks = 1;
                    otherRoomPortal.NumZBlocks = (byte)(zMax - zMin + 1);
                    otherRoomPortal.Opacity = PortalOpacity.None;
                    otherRoomPortal.Direction = PortalDirection.West;
                    otherRoomPortal.X = 0;
                    otherRoomPortal.Z = (byte)zPortalOther;
                    otherRoomPortal.AdjoiningRoom = _editor.RoomIndex;

                    _editor.Level.Portals.Add(otherRoomPortal.ID, otherRoomPortal);

                    currentRoomPortal.OtherID = otherRoomPortal.ID;
                    otherRoomPortal.OtherID = currentRoomPortal.ID;

                    for (int z = zMin; z <= zMax; z++)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[numXblocks - 1, z].Type = BlockType.WallPortal;
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[numXblocks - 1, z].WallPortal = currentRoomPortal.ID;

                        _editor.Level.Rooms[found].Blocks[0, z + (int)(room.Position.Z - otherRoom.Position.Z)].Type = BlockType.WallPortal;
                        _editor.Level.Rooms[found].Blocks[0, z + (int)(room.Position.Z - otherRoom.Position.Z)].WallPortal = otherRoomPortal.ID;
                    }

                    _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                    _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
                    _editor.Level.Rooms[found].BuildGeometry();
                    _editor.Level.Rooms[found].UpdateBuffers();

                    _editor.DrawPanel3D();
                    _editor.DrawPanelGrid();

                    return;
                }
                else
                {
                    MessageBox.Show("Not a valid portal position", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // MURO NORD
            if (zMin == numZblocks - 1 && zMax == zMin && xMin != 0 && xMax != numXblocks - 1)
            {
                // controllo che si stia sovrapponendo a un portale
                for (int x = xMin; x <= xMax; x++)
                {
                    if (room.Blocks[x, zMin].Type == BlockType.WallPortal)
                    {
                        MessageBox.Show("Can't overlap portals", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                int xPortalWorld = Utils.GetWorldX(room, xMin);
                int zPortalWorld = Utils.GetWorldZ(room, zMin);

                // ora cerco una possibile vicina
                short found = -1;
                for (int i = 0; i < 2048; i++)
                {
                    found = -1;
                    Room otherRoom = _editor.Level.Rooms[i];
                    if (i == _editor.RoomIndex || otherRoom == null) continue;
                    if (room.Position.Z + room.NumZSectors - 1 == otherRoom.Position.Z + 1 && room.Position.X + xMin >= otherRoom.Position.X + 1 &&
                        room.Position.X + xMax <= otherRoom.Position.X + otherRoom.NumXSectors - 1)
                    {
                        for (int x = xMin; x <= xMax; x++)
                        {
                            int facingX = x + (int)(room.Position.X - otherRoom.Position.X);
                            if (facingX < 1 || facingX > otherRoom.NumXSectors - 1)
                            {
                                found = -1;
                                break;
                            }

                            if (otherRoom.Blocks[facingX, 0].Type == BlockType.WallPortal)
                            {
                                found = -1;
                                break;
                            }
                            else
                            {
                                found = (short)i;
                            }
                        }
                    }

                    if (found != -1) break;
                }

                if (found != -1)
                {
                    Portal currentRoomPortal = new Portal(0, _editor.RoomIndex);
                    Portal otherRoomPortal = new Portal(0, found);
                    Room otherRoom = _editor.Level.Rooms[found];

                    byte xPortalOther = (byte)(xPortalWorld - otherRoom.Position.X);
                    byte zPortalOther = (byte)(zPortalWorld - otherRoom.Position.Z);

                    currentRoomPortal.ID = _editor.Level.GetNewPortalID();
                    currentRoomPortal.NumXBlocks = (byte)(xMax - xMin + 1);
                    currentRoomPortal.NumZBlocks = 1;
                    currentRoomPortal.Opacity = PortalOpacity.None;
                    currentRoomPortal.Direction = PortalDirection.North;
                    currentRoomPortal.X = (byte)xMin;
                    currentRoomPortal.Z = (byte)(numZblocks - 1);
                    currentRoomPortal.AdjoiningRoom = found;

                    _editor.Level.Portals.Add(currentRoomPortal.ID, currentRoomPortal);

                    otherRoomPortal.ID = _editor.Level.GetNewPortalID();
                    otherRoomPortal.NumXBlocks = (byte)(xMax - xMin + 1);
                    otherRoomPortal.NumZBlocks = 1;
                    otherRoomPortal.Opacity = PortalOpacity.None;
                    otherRoomPortal.Direction = PortalDirection.South;
                    otherRoomPortal.X = xPortalOther;
                    otherRoomPortal.Z = 0;
                    otherRoomPortal.AdjoiningRoom = _editor.RoomIndex;

                    _editor.Level.Portals.Add(otherRoomPortal.ID, otherRoomPortal);

                    currentRoomPortal.OtherID = otherRoomPortal.ID;
                    otherRoomPortal.OtherID = currentRoomPortal.ID;

                    for (int x = xMin; x <= xMax; x++)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, numZblocks - 1].Type = BlockType.WallPortal;
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, numZblocks - 1].WallPortal = currentRoomPortal.ID;

                        _editor.Level.Rooms[found].Blocks[x + xPortalOther - xMin, 0].Type = BlockType.WallPortal;
                        _editor.Level.Rooms[found].Blocks[x + xPortalOther - xMin, 0].WallPortal = otherRoomPortal.ID;
                    }

                    _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                    _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
                    _editor.Level.Rooms[found].BuildGeometry();
                    _editor.Level.Rooms[found].UpdateBuffers();

                    _editor.DrawPanel3D();
                    _editor.DrawPanelGrid();

                    // labelStatistics.Text = _editor.UpdateStatistics();

                    return;
                }
                else
                {
                    MessageBox.Show("Not a valid portal position", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // MURO SUD
            if (zMin == 0 && zMax == zMin && xMin != 0 && xMax != numXblocks - 1)
            {
                // controllo che si stia sovrapponendo a un portale
                for (int x = xMin; x <= xMax; x++)
                {
                    if (room.Blocks[x, zMin].Type == BlockType.WallPortal)
                    {
                        MessageBox.Show("Can't overlap portals", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                int xPortalWorld = Utils.GetWorldX(room, xMin);
                int zPortalWorld = Utils.GetWorldZ(room, zMin);

                // ora cerco una possibile vicina
                short found = -1;
                for (int i = 0; i < 2048; i++)
                {
                    found = -1;
                    Room otherRoom = _editor.Level.Rooms[i];
                    if (i == _editor.RoomIndex || otherRoom == null) continue;
                    if (otherRoom.Position.Z + otherRoom.NumZSectors - 1 == room.Position.Z + 1 && room.Position.X + xMin >= otherRoom.Position.X + 1 &&
                        room.Position.X + xMax <= otherRoom.Position.X + otherRoom.NumXSectors - 1)
                    {
                        for (int x = xMin; x <= xMax; x++)
                        {
                            int facingX = x + (int)(room.Position.X - otherRoom.Position.X);
                            if (facingX < 1 || facingX > otherRoom.NumXSectors - 1)
                            {
                                found = -1;
                                break;
                            }

                            if (otherRoom.Blocks[facingX, 0].Type == BlockType.WallPortal)
                            {
                                found = -1;
                                break;
                            }
                            else
                            {
                                found = (short)i;
                            }
                        }
                    }

                    if (found != -1) break;
                }

                if (found != -1)
                {
                    Portal currentRoomPortal = new Portal(0, _editor.RoomIndex);
                    Portal otherRoomPortal = new Portal(0, found);
                    Room otherRoom = _editor.Level.Rooms[found];

                    int xPortalOther = (int)(xPortalWorld - otherRoom.Position.X);
                    int zPortalOther = (int)(zPortalWorld - otherRoom.Position.Z);

                    currentRoomPortal.ID = _editor.Level.GetNewPortalID();
                    currentRoomPortal.NumXBlocks = (byte)(xMax - xMin + 1);
                    currentRoomPortal.NumZBlocks = 1;
                    currentRoomPortal.Opacity = PortalOpacity.None;
                    currentRoomPortal.Direction = PortalDirection.South;
                    currentRoomPortal.X = (byte)xMin;
                    currentRoomPortal.Z = 0;
                    currentRoomPortal.AdjoiningRoom = found;

                    _editor.Level.Portals.Add(currentRoomPortal.ID, currentRoomPortal);

                    otherRoomPortal.ID = _editor.Level.GetNewPortalID();
                    otherRoomPortal.NumXBlocks = (byte)(xMax - xMin + 1);
                    otherRoomPortal.NumZBlocks = 1;
                    otherRoomPortal.Opacity = PortalOpacity.None;
                    otherRoomPortal.Direction = PortalDirection.North;
                    otherRoomPortal.X = (byte)xPortalOther;
                    otherRoomPortal.Z = (byte)(otherRoom.NumZSectors - 1);
                    otherRoomPortal.AdjoiningRoom = _editor.RoomIndex;

                    _editor.Level.Portals.Add(otherRoomPortal.ID, otherRoomPortal);

                    currentRoomPortal.OtherID = otherRoomPortal.ID;
                    otherRoomPortal.OtherID = currentRoomPortal.ID;

                    for (int x = xMin; x <= xMax; x++)
                    {
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].Type = BlockType.WallPortal;
                        _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, 0].WallPortal = currentRoomPortal.ID;

                        _editor.Level.Rooms[found].Blocks[x + xPortalOther - xMin, otherRoom.NumZSectors - 1].Type = BlockType.WallPortal;
                        _editor.Level.Rooms[found].Blocks[x + xPortalOther - xMin, otherRoom.NumZSectors - 1].WallPortal = otherRoomPortal.ID;
                    }

                    _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                    _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
                    _editor.Level.Rooms[found].BuildGeometry();
                    _editor.Level.Rooms[found].UpdateBuffers();

                    _editor.DrawPanel3D();
                    _editor.DrawPanelGrid();

                    // labelStatistics.Text = _editor.UpdateStatistics();

                    return;
                }
                else
                {
                    MessageBox.Show("Not a valid portal position", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // PAVIMENTO - SOFFITTO
            if (xMin > 0 && xMax < numXblocks - 1 && zMin > 0 && zMax < numZblocks - 1)
            {
                int lowest = room.GetLowestCorner(); ;

                // per prima cosa verifico la validità del pavimento del futuro portale
                for (int x = xMin; x <= xMax; x++)
                {
                    for (int z = zMin; z <= zMax; z++)
                    {
                        int h1 = room.Blocks[x, z].QAFaces[0];
                        int h2 = room.Blocks[x, z].QAFaces[1];
                        int h3 = room.Blocks[x, z].QAFaces[2];
                        int h4 = room.Blocks[x, z].QAFaces[3];

                        // verifico che sia un settore il cui pavimento non ha portali
                        if (room.Blocks[x, z].Type != BlockType.Floor && room.Blocks[x, z].Type != BlockType.CeilingPortal)
                        {
                            MessageBox.Show("Not a valid portal position", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // verifico che il minimo della stanza coincida con il minimo del settore
                        int min = Math.Min(Math.Min(Math.Min(h1, h2), h3), h4);
                        if (min < lowest)
                        {
                            MessageBox.Show("Not a valid portal position", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                // ora cerco una possibile stanza sotto non distante più di 256 pixel
                short found = -1;

                for (int i = 0; i < 2048; i++)
                {
                    found = -1;
                    Room otherRoom = _editor.Level.Rooms[i];
                    if (i == _editor.RoomIndex || otherRoom == null) continue;
                    int distance = (int)room.Position.Y + room.GetLowestCorner() - ((int)otherRoom.Position.Y + otherRoom.GetHighestCorner());
                    if (distance < 0 || distance > 2) continue;

                    int lowXmin = xMin + (int)(room.Position.X - otherRoom.Position.X);
                    int lowXmax = xMax + (int)(room.Position.X - otherRoom.Position.X);
                    int lowZmin = zMin + (int)(room.Position.Z - otherRoom.Position.Z);
                    int lowZmax = zMax + (int)(room.Position.Z - otherRoom.Position.Z);

                    // se le coordinate riportate relativamente alla stanza sottostante sono fuori range allora ignoro questa stanza
                    if (lowXmin < 1 || lowXmin > otherRoom.NumXSectors - 2 || lowXmax < 1 || lowXmax > otherRoom.NumXSectors - 2 ||
                        lowZmin < 1 || lowZmin > otherRoom.NumZSectors - 2 || lowZmax < 1 || lowZmax > otherRoom.NumZSectors - 2)
                        continue;

                    bool validRoom = true;
                    int highest = otherRoom.GetHighestCorner();

                    for (int x = lowXmin; x <= lowXmax; x++)
                    {
                        for (int z = lowZmin; z <= lowZmax; z++)
                        {
                            // faccio gli stessi controlli del pavimento stavolta per il soffitto della stanza di sotto
                            int h1 = otherRoom.Ceiling + otherRoom.Blocks[x, z].WSFaces[0];
                            int h2 = otherRoom.Ceiling + otherRoom.Blocks[x, z].WSFaces[1];
                            int h3 = otherRoom.Ceiling + otherRoom.Blocks[x, z].WSFaces[2];
                            int h4 = otherRoom.Ceiling + otherRoom.Blocks[x, z].WSFaces[3];

                            // verifico che sia un settore il cui soffitto non ha portali
                            if (otherRoom.Blocks[x, z].Type != BlockType.Floor && otherRoom.Blocks[x, z].Type != BlockType.FloorPortal)
                            {
                                validRoom = false;
                                break;
                            }

                            // verifico che il massimo della stanza coincida con il massimo del settore
                            int max = Math.Max(Math.Max(Math.Max(h1, h2), h3), h4);
                            if (max > highest)
                            {
                                validRoom = false;
                                break;
                            }
                        }

                        if (!validRoom) break;
                    }

                    if (!validRoom)
                        continue;
                    else
                    {
                        found = (short)i;
                        break;
                    }
                }

                if (found > -1)
                {
                    Room otherRoom = _editor.Level.Rooms[found];
                    int highest = _editor.Level.Rooms[found].GetHighestCorner();

                    for (int x = xMin; x <= xMax; x++)
                    {
                        for (int z = zMin; z <= zMax; z++)
                        {
                            int lowX = x + (int)(room.Position.X - otherRoom.Position.X);
                            int lowZ = z + (int)(room.Position.Z - otherRoom.Position.Z);

                            int h1 = room.Blocks[x, z].QAFaces[0];
                            int h2 = room.Blocks[x, z].QAFaces[1];
                            int h3 = room.Blocks[x, z].QAFaces[2];
                            int h4 = room.Blocks[x, z].QAFaces[3];

                            int lh1 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[0];
                            int lh2 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[1];
                            int lh3 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[2];
                            int lh4 = otherRoom.Ceiling + otherRoom.Blocks[lowX, lowZ].WSFaces[3];

                            bool defined = false;

                            if (Room.IsQuad(x, z, h1, h2, h3, h4, true) && h1 == lowest)
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Type = BlockType.FloorPortal;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].IsFloorSolid = false;
                                defined = true;
                            }
                            else
                            {
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].Type = BlockType.FloorPortal;
                                _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].IsFloorSolid = true;
                                defined = false;
                            }

                            if (Room.IsQuad(x, z, lh1, lh2, lh3, lh4, true) && lh1 == highest && defined)
                            {
                                _editor.Level.Rooms[found].Blocks[lowX, lowZ].Type = BlockType.CeilingPortal;
                                _editor.Level.Rooms[found].Blocks[lowX, lowZ].IsCeilingSolid = false;
                            }
                            else
                            {
                                _editor.Level.Rooms[found].Blocks[lowX, lowZ].IsCeilingSolid = true;
                            }
                        }
                    }

                    Portal currentRoomPortal = new Portal(0, _editor.RoomIndex);
                    Portal otherRoomPortal = new Portal(0, found);

                    currentRoomPortal.ID = _editor.Level.GetNewPortalID();
                    currentRoomPortal.NumXBlocks = (byte)(xMax - xMin + 1);
                    currentRoomPortal.NumZBlocks = (byte)(zMax - zMin + 1);
                    currentRoomPortal.Opacity = PortalOpacity.None;
                    currentRoomPortal.Direction = PortalDirection.Floor;
                    currentRoomPortal.X = (byte)xMin;
                    currentRoomPortal.Z = (byte)zMin;
                    currentRoomPortal.AdjoiningRoom = found;

                    _editor.Level.Portals.Add(currentRoomPortal.ID, currentRoomPortal);

                    int lowXmin = xMin + (int)(room.Position.X - otherRoom.Position.X);
                    int lowXmax = xMax + (int)(room.Position.X - otherRoom.Position.X);
                    int lowZmin = zMin + (int)(room.Position.Z - otherRoom.Position.Z);
                    int lowZmax = zMax + (int)(room.Position.Z - otherRoom.Position.Z);

                    otherRoomPortal.ID = _editor.Level.GetNewPortalID();
                    otherRoomPortal.NumXBlocks = (byte)(lowXmax - lowXmin + 1);
                    otherRoomPortal.NumZBlocks = (byte)(lowZmax - lowZmin + 1);
                    otherRoomPortal.Opacity = PortalOpacity.None;
                    otherRoomPortal.Direction = PortalDirection.Ceiling;
                    otherRoomPortal.X = (byte)lowXmin;
                    otherRoomPortal.Z = (byte)lowZmin;
                    otherRoomPortal.AdjoiningRoom = _editor.RoomIndex;

                    _editor.Level.Portals.Add(otherRoomPortal.ID, otherRoomPortal);

                    currentRoomPortal.OtherID = otherRoomPortal.ID;
                    otherRoomPortal.OtherID = currentRoomPortal.ID;

                    // labelStatistics.Text = _editor.UpdateStatistics();

                    for (int x = xMin; x <= xMax; x++)
                    {
                        for (int z = zMin; z <= zMax; z++)
                        {
                            _editor.Level.Rooms[_editor.RoomIndex].Blocks[x, z].FloorPortal = currentRoomPortal.ID;
                        }
                    }

                    for (int x = otherRoomPortal.X; x <= otherRoomPortal.X + otherRoomPortal.NumXBlocks - 1; x++)
                    {
                        for (int z = otherRoomPortal.Z; z <= otherRoomPortal.Z + otherRoomPortal.NumZBlocks - 1; z++)
                        {
                            _editor.Level.Rooms[found].Blocks[x, z].CeilingPortal = otherRoomPortal.ID;
                        }
                    }

                    _editor.Level.Rooms[_editor.RoomIndex].BuildGeometry();
                    _editor.Level.Rooms[_editor.RoomIndex].UpdateBuffers();
                    _editor.Level.Rooms[found].BuildGeometry();
                    _editor.Level.Rooms[found].UpdateBuffers();

                    _editor.DrawPanel3D();
                    _editor.DrawPanelGrid();

                    return;
                }
            }
        }*/
    }
}
