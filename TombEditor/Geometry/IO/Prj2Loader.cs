using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NLog;
using SharpDX;
using SharpDX.Toolkit.Graphics;
using Color = System.Drawing.Color;

namespace TombEditor.Geometry.IO
{
    public class Prj2Loader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static Level LoadFromPrj2(string filename, GraphicsDevice device, FormMain form)
        {
            var level = new Level();

            try
            {
                using (var reader = Level.CreatePrjReader(filename))
                {
                    if (reader == null)
                        return null;

                    // Read version code (in the future it can be used to have multiple PRJ versions)
                    int versionCode = reader.ReadInt32();

                    // Read texture file
                    int stringLength = reader.ReadInt32();
                    level.TextureFile = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(stringLength));

                    // Check if texture file exists
                    if (level.TextureFile == "" || !File.Exists(level.TextureFile))
                    {
                        logger.Error("Can't find texture map!");

                        if (DarkUI.Forms.DarkMessageBox.ShowWarning("The texture map '" + level.TextureFile + " could not be found. Do you want to browse it or cancel opening project?",
                                "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                        {
                            logger.Error("PRJ2 loading canceled");
                            reader.Close();
                            return null;
                        }

                        // Ask for texture file
                        string textureFile = form.BrowseTextureMap();
                        if (textureFile == "")
                        {
                            logger.Error("PRJ2 loading canceled");
                            reader.Close();
                            return null;
                        }

                        level.TextureFile = textureFile;
                    }

                    //Read WAD file
                    stringLength = reader.ReadInt32();
                    level.WadFile = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(stringLength));

                    // Check if WAD file exists
                    if (level.WadFile == "" || !File.Exists(level.WadFile))
                    {
                        logger.Error("Can't find WAD!");

                        if (DarkUI.Forms.DarkMessageBox.ShowWarning("The WAD '" + level.WadFile + " could not be found. Do you want to browse it or cancel opening project?",
                                "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                        {
                            logger.Error("PRJ2 loading canceled");
                            reader.Close();
                            return null;
                        }

                        // Ask for texture file
                        string wadFile = form.BrowseWAD();
                        if (wadFile == "")
                        {
                            logger.Error("PRJ2 loading canceled");
                            reader.Close();
                            return null;
                        }

                        level.WadFile = wadFile;
                    }

                    // Read fillers
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();

                    // Read textures
                    int numTextures = reader.ReadInt32();
                    for (int i = 0; i < numTextures; i++)
                    {
                        var texture = new LevelTexture
                        {
                            Id = reader.ReadInt32(),
                            X = reader.ReadInt16(),
                            Y = reader.ReadInt16(),
                            Width = reader.ReadInt16(),
                            Height = reader.ReadInt16(),
                            Page = reader.ReadInt16()
                        };

                        /*texture.UsageCount =*/
                        reader.ReadInt32();
                        texture.Transparent = reader.ReadBoolean();
                        texture.DoubleSided = reader.ReadBoolean();

                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();

                        level.TextureSamples.Add(texture.Id, texture);
                    }

                    // Read portals
                    int numPortals = reader.ReadInt32();
                    for (int i = 0; i < numPortals; i++)
                    {
                        var portal = new Portal(0, null)
                        {
                            Id = reader.ReadInt32(),
                            OtherId = reader.ReadInt32(),
                            Room = level.GetOrCreateRoom(reader.ReadInt16()),
                            AdjoiningRoom = level.GetOrCreateRoom(reader.ReadInt16()),
                            Direction = (PortalDirection)reader.ReadByte(),
                            X = reader.ReadByte(),
                            Z = reader.ReadByte(),
                            NumXBlocks = reader.ReadByte(),
                            NumZBlocks = reader.ReadByte()
                        };

                        reader.ReadByte();
                        portal.MemberOfFlippedRoom = reader.ReadBoolean();
                        portal.Flipped = reader.ReadBoolean();
                        portal.OtherIdFlipped = reader.ReadInt32();

                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();

                        level.Portals.Add(portal.Id, portal);
                    }

                    // Read objects
                    int numObjects = reader.ReadInt32();
                    for (int i = 0; i < numObjects; i++)
                    {
                        int objectId = reader.ReadInt32();
                        var objectType = (ObjectInstanceType)reader.ReadByte();

                        ObjectInstance o;

                        switch (objectType)
                        {
                            case ObjectInstanceType.Moveable:
                                o = new MoveableInstance(objectId, null);
                                break;
                            case ObjectInstanceType.StaticMesh:
                                o = new StaticMeshInstance(objectId, null);
                                break;
                            case ObjectInstanceType.Camera:
                                o = new CameraInstance(objectId, null);
                                break;
                            case ObjectInstanceType.Sink:
                                o = new SinkInstance(objectId, null);
                                break;
                            case ObjectInstanceType.SoundSource:
                                o = new SoundSourceInstance(objectId, null);
                                break;
                            case ObjectInstanceType.FlyByCamera:
                                o = new FlybyCameraInstance(objectId, null);
                                break;
                            default:
                                return null;
                        }

                        o.Type = objectType;
                        o.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        o.Room = level.GetOrCreateRoom(reader.ReadInt16());
                        o.Ocb = reader.ReadInt16();
                        o.Rotation = reader.ReadInt16();
                        o.Invisible = reader.ReadBoolean();
                        o.ClearBody = reader.ReadBoolean();
                        o.Bits[0] = reader.ReadBoolean();
                        o.Bits[1] = reader.ReadBoolean();
                        o.Bits[2] = reader.ReadBoolean();
                        o.Bits[3] = reader.ReadBoolean();
                        o.Bits[4] = reader.ReadBoolean();

                        if (o.Type == ObjectInstanceType.StaticMesh)
                        {
                            ((StaticMeshInstance)o).ObjectId = reader.ReadInt32();
                            ((StaticMeshInstance)o).Color = Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(),
                                reader.ReadByte());
                            reader.ReadBytes(1);
                        }

                        if (o.Type == ObjectInstanceType.Moveable)
                        {
                            ((MoveableInstance)o).ObjectId = reader.ReadInt32();
                            reader.ReadBytes(4);
                        }

                        if (o.Type == ObjectInstanceType.Camera)
                        {
                            ((CameraInstance)o).Fixed = reader.ReadBoolean();
                            reader.ReadBytes(7);
                        }

                        if (o.Type == ObjectInstanceType.Sink)
                        {
                            ((SinkInstance)o).Strength = reader.ReadInt16();
                            reader.ReadBytes(6);
                        }

                        if (o.Type == ObjectInstanceType.SoundSource)
                        {
                            ((SoundSourceInstance)o).SoundId = reader.ReadInt16();
                            reader.ReadBytes(6);
                        }

                        /*if (o.Type == ObjectInstanceType.Fkl)
                        {
                            ((SoundInstance)o).SoundID = reader.ReadInt16();
                            reader.ReadBytes(6);
                        }*/

                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();

                        level.Objects.Add(o.Id, o);
                    }

                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();

                    // Read triggers
                    int numTriggers = reader.ReadInt32();
                    for (int i = 0; i < numTriggers; i++)
                    {
                        var o = new TriggerInstance(0, null)
                        {
                            Id = reader.ReadInt32(),
                            X = reader.ReadByte(),
                            Z = reader.ReadByte(),
                            NumXBlocks = reader.ReadByte(),
                            NumZBlocks = reader.ReadByte(),
                            TriggerType = (TriggerType)reader.ReadByte(),
                            TargetType = (TriggerTargetType)reader.ReadByte(),
                            Target = reader.ReadInt32(),
                            Timer = reader.ReadInt16(),
                            OneShot = reader.ReadBoolean(),
                            Bits =
                            {
                                [0] = reader.ReadBoolean(),
                                [1] = reader.ReadBoolean(),
                                [2] = reader.ReadBoolean(),
                                [3] = reader.ReadBoolean(),
                                [4] = reader.ReadBoolean()
                            },
                            Room = level.GetOrCreateRoom(reader.ReadInt16())
                        };


                        reader.ReadInt16();
                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();

                        level.Triggers.Add(o.Id, o);
                    }

                    // Read rooms
                    int numRooms = reader.ReadInt32();
                    for (int i = 0; i < numRooms; i++)
                    {
                        string roomMagicWord = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4));

                        bool defined = reader.ReadBoolean();
                        if (!defined)
                        {
                            if (level.Rooms[i] != null)
                                logger.Warn($"Room {i} is used, but is marked as undefined");
                            level.Rooms[i] = null;
                            continue;
                        }

                        var room = level.GetOrCreateRoom(i);
                        room.Name = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(100)).Trim();
                        room.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        room.Ceiling = reader.ReadInt16();
                        room.NumXSectors = reader.ReadByte();
                        room.NumZSectors = reader.ReadByte();
                        room.Blocks = new Block[room.NumXSectors, room.NumZSectors];

                        for (int z = 0; z < room.NumZSectors; z++)
                        {
                            for (int x = 0; x < room.NumXSectors; x++)
                            {
                                var b = new Block(BlockType.Floor, BlockFlags.None)
                                {
                                    Type = (BlockType)reader.ReadByte(),
                                    Flags = (BlockFlags)reader.ReadInt16(),
                                    QAFaces =
                                    {
                                        [0] = reader.ReadInt16(),
                                        [1] = reader.ReadInt16(),
                                        [2] = reader.ReadInt16(),
                                        [3] = reader.ReadInt16()
                                    },
                                    EDFaces =
                                    {
                                        [0] = reader.ReadInt16(),
                                        [1] = reader.ReadInt16(),
                                        [2] = reader.ReadInt16(),
                                        [3] = reader.ReadInt16()
                                    },
                                    WSFaces =
                                    {
                                        [0] = reader.ReadInt16(),
                                        [1] = reader.ReadInt16(),
                                        [2] = reader.ReadInt16(),
                                        [3] = reader.ReadInt16()
                                    },
                                    RFFaces =
                                    {
                                        [0] = reader.ReadInt16(),
                                        [1] = reader.ReadInt16(),
                                        [2] = reader.ReadInt16(),
                                        [3] = reader.ReadInt16()
                                    },
                                    SplitFoorType = reader.ReadByte(),
                                    SplitFloor = reader.ReadBoolean(),
                                    SplitCeilingType = reader.ReadByte(),
                                    SplitCeiling = reader.ReadBoolean(),
                                    RealSplitFloor = reader.ReadByte(),
                                    RealSplitCeiling = reader.ReadByte(),
                                    Climb =
                                    {
                                        [0] = reader.ReadBoolean(),
                                        [1] = reader.ReadBoolean(),
                                        [2] = reader.ReadBoolean(),
                                        [3] = reader.ReadBoolean()
                                    },
                                    FloorOpacity = (PortalOpacity)reader.ReadByte(),
                                    CeilingOpacity = (PortalOpacity)reader.ReadByte(),
                                    WallOpacity = (PortalOpacity)reader.ReadByte(),
                                    FloorPortal = reader.ReadInt32(),
                                    CeilingPortal = reader.ReadInt32(),
                                    WallPortal = reader.ReadInt32(),
                                    IsFloorSolid = reader.ReadBoolean(),
                                    IsCeilingSolid = reader.ReadBoolean(),
                                    NoCollisionFloor = reader.ReadBoolean(),
                                    NoCollisionCeiling = reader.ReadBoolean()
                                };

                                foreach (var f in b.Faces)
                                {
                                    f.Defined = reader.ReadBoolean();
                                    f.Flipped = reader.ReadBoolean();
                                    f.Texture = reader.ReadInt16();
                                    f.Rotation = reader.ReadByte();
                                    f.Transparent = reader.ReadBoolean();
                                    f.DoubleSided = reader.ReadBoolean();
                                    f.Invisible = reader.ReadBoolean();
                                    f.NoCollision = reader.ReadBoolean();
                                    f.TextureTriangle = (TextureTileType)reader.ReadByte();

                                    for (int n = 0; n < 4; n++)
                                        f.RectangleUV[n] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                                    for (int n = 0; n < 3; n++)
                                        f.TriangleUV[n] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                                    for (int n = 0; n < 3; n++)
                                        f.TriangleUV2[n] = new Vector2(reader.ReadSingle(), reader.ReadSingle());

                                    reader.ReadInt32();
                                    reader.ReadInt32();
                                    reader.ReadInt32();
                                    reader.ReadInt32();
                                }

                                b.FloorDiagonalSplit = (DiagonalSplit)reader.ReadByte();
                                b.FloorDiagonalSplitType = (DiagonalSplitType)reader.ReadByte();
                                b.CeilingDiagonalSplit = (DiagonalSplit)reader.ReadByte();
                                b.CeilingDiagonalSplitType = (DiagonalSplitType)reader.ReadByte();

                                reader.ReadInt32();
                                reader.ReadInt32();
                                reader.ReadInt32();
                                reader.ReadInt32();

                                room.Blocks[x, z] = b;
                            }
                        }

                        int numLights = reader.ReadInt32();
                        for (int j = 0; j < numLights; j++)
                        {
                            var l = new Light
                            {
                                Type = (LightType)reader.ReadByte(),
                                Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(),
                                    reader.ReadSingle()),
                                Intensity = reader.ReadSingle(),
                                Color = Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(), reader.ReadByte()),
                                In = reader.ReadSingle(),
                                Out = reader.ReadSingle(),
                                Len = reader.ReadSingle(),
                                Cutoff = reader.ReadSingle(),
                                DirectionX = reader.ReadSingle(),
                                DirectionY = reader.ReadSingle(),
                                Face = (BlockFaces)reader.ReadByte()
                            };


                            reader.ReadByte();
                            reader.ReadInt16();

                            room.Lights.Add(l);
                        }

                        room.AmbientLight =
                            Color.FromArgb(255, reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
                        room.Flipped = reader.ReadBoolean();
                        room.AlternateRoom = level.GetOrCreateRoom(reader.ReadInt16());
                        room.AlternateGroup = reader.ReadInt16();
                        room.WaterLevel = reader.ReadInt16();
                        room.MistLevel = reader.ReadInt16();
                        room.ReflectionLevel = reader.ReadInt16();
                        room.FlagCold = reader.ReadBoolean();
                        room.FlagDamage = reader.ReadBoolean();
                        room.FlagHorizon = reader.ReadBoolean();
                        room.FlagMist = reader.ReadBoolean();
                        room.FlagOutside = reader.ReadBoolean();
                        room.FlagRain = reader.ReadBoolean();
                        room.FlagReflection = reader.ReadBoolean();
                        room.FlagSnow = reader.ReadBoolean();
                        room.FlagWater = reader.ReadBoolean();
                        room.FlagQuickSand = reader.ReadBoolean();
                        room.Flipped = reader.ReadBoolean();
                        room.BaseRoom = level.GetOrCreateRoom(reader.ReadInt16());
                        room.ExcludeFromPathFinding = reader.ReadBoolean();

                        reader.ReadInt32();
                        reader.ReadInt32();
                        reader.ReadInt32();

                        System.Diagnostics.Debug.Assert(ReferenceEquals(room, level.Rooms[i]));
                    }

                    int numAnimatedSets = reader.ReadInt32();
                    for (int i = 0; i < numAnimatedSets; i++)
                    {
                        var effect = (AnimatexTextureSetEffect)reader.ReadByte();
                        var aSet = new AnimatedTextureSet { Effect = effect };
                        int numAnimatedTextures = reader.ReadInt32();
                        for (int j = 0; j < numAnimatedTextures; j++)
                        {
                            short texturePage = reader.ReadInt16();
                            short textureX = reader.ReadInt16();
                            short textureY = reader.ReadInt16();

                            aSet.Textures.Add(new AnimatedTexture(textureX, textureY, texturePage));
                        }

                        level.AnimatedTextures.Add(aSet);
                    }

                    int numTextureSounds = reader.ReadInt32();
                    for (int i = 0; i < numTextureSounds; i++)
                    {
                        var txtSound = new TextureSound(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16())
                        {
                            Sound = (TextureSounds)reader.ReadByte()
                        };

                        level.TextureSounds.Add(txtSound);
                    }
                    
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                    reader.ReadInt32();
                }

                // Check that there are uninitialized rooms
                foreach (Room room in level.Rooms)
                    if (room != null)
                        if ((room.NumXSectors <= 0) && (room.NumZSectors <= 0))
                            throw new Exception("Room " + level.Rooms.ReferenceIndexOf(room) + " has a sector size of zero. This is invalid. Probably the room was referenced but never initialized.");

            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }

            // Now it's time to load texturs
            level.LoadTextureMap(level.TextureFile, device);

            // Now it's time to load WAD
            level.LoadWad(level.WadFile, device);

            // Now fill the structures loaded from PRJ2 
            for (int i = 0; i < level.Triggers.Count; i++)
            {
                var trigger = level.Triggers[i];

                for (int x = trigger.X; x < trigger.X + trigger.NumXBlocks; x++)
                {
                    for (int z = trigger.Z; z < trigger.Z + trigger.NumZBlocks; z++)
                    {
                        trigger.Room.Blocks[x, z].Triggers.Add(trigger.Id);
                    }
                }
            }

            foreach (var obj in level.Objects.Values)
            {
                var objectType = obj.Type;
                int objectId = obj.Id;

                switch (objectType)
                {
                    case ObjectInstanceType.Moveable:
                        obj.Room.Moveables.Add(objectId);
                        ((MoveableInstance)obj).Model = level.Wad.Moveables[(uint)((MoveableInstance)obj).ObjectId];
                        break;
                    case ObjectInstanceType.StaticMesh:
                        obj.Room.StaticMeshes.Add(objectId);
                        ((StaticMeshInstance)obj).Model = level.Wad.StaticMeshes[(uint)((StaticMeshInstance)obj).ObjectId];
                        break;
                    case ObjectInstanceType.Camera:
                        obj.Room.Cameras.Add(objectId);
                        break;
                    case ObjectInstanceType.Sink:
                        obj.Room.Sinks.Add(objectId);
                        break;
                    case ObjectInstanceType.SoundSource:
                        obj.Room.SoundSources.Add(objectId);
                        break;
                    case ObjectInstanceType.FlyByCamera:
                        obj.Room.FlyByCameras.Add(objectId);
                        break;
                }
            }

            // Now build the real geometry and update DirectX buffers
            foreach (var room in level.Rooms.Where(room => room != null))
            {
                room.InitializeVerticesGrid();
                room.BuildGeometry();
                room.CalculateLightingForThisRoom();
                room.UpdateBuffers();
            }

            foreach (var portal in level.Portals)
            {
                portal.Value.Room.Portals.Add(portal.Key);
            }

            level.FileName = filename;

            return level;
        }
    }
}
