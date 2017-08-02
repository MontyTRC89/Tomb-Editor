using System;
using System.IO;
using System.Linq;
using NLog;
using TombLib.IO;

namespace TombEditor.Geometry.IO
{
    public class Prj2Writer
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static bool SaveToPrj2(string filename, Level level)
        {
            const byte filler8 = 0;
            const short filler16 = 0;
            const int filler32 = 0;

            try
            {
                var ms = new MemoryStream();
                byte[] projectData;
                using (var writer = new BinaryWriterEx(ms))
                {
                    // Write version
                    var version = new byte[] { 0x50, 0x52, 0x4A, 0x32 };
                    writer.Write(version);

                    const int versionCode = 2;
                    writer.Write(versionCode);

                    string textureFileName = Utils.GetRelativePath(filename, level.TextureFile);
                    string wadFileName = Utils.GetRelativePath(filename, level.WadFile);

                    // Write texture map
                    var textureFile = System.Text.Encoding.UTF8.GetBytes(textureFileName);
                    int numBytes = textureFile.Length;
                    writer.Write(numBytes);
                    writer.Write(textureFile);

                    var wadFile = System.Text.Encoding.UTF8.GetBytes(wadFileName);
                    numBytes = wadFile.Length;
                    writer.Write(numBytes);
                    writer.Write(wadFile);

                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);

                    // Write textures
                    int numTextures = level.TextureSamples.Count;
                    writer.Write(numTextures);
                    foreach (var txt in level.TextureSamples.Values)
                    {
                        writer.Write(txt.Id);
                        writer.Write(txt.X);
                        writer.Write(txt.Y);
                        writer.Write(txt.Width);
                        writer.Write(txt.Height);
                        writer.Write(txt.Page);
                        writer.Write(filler32);
                        writer.Write(txt.Transparent);
                        writer.Write(txt.DoubleSided);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                    }

                    // Write portals
                    int numPortals = level.Portals.Count;
                    writer.Write(numPortals);
                    foreach (var p in level.Portals.Values)
                    {
                        writer.Write(p.Id);
                        writer.Write((int) level.Portals.First(kv => ReferenceEquals(kv.Value, p.Other)).Key);
                        writer.Write((short)level.Rooms.ReferenceIndexOf(p.Room));
                        writer.Write((short)level.Rooms.ReferenceIndexOf(p.AdjoiningRoom));
                        writer.Write((byte)p.Direction);
                        writer.Write(p.X);
                        writer.Write(p.Z);
                        writer.Write(p.NumXBlocks);
                        writer.Write(p.NumZBlocks);
                        writer.Write(filler8);
                        writer.Write(p.MemberOfFlippedRoom);
                        writer.Write(p.Flipped);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                    }

                    // Write objects: moveables, static meshes, cameras, sinks, sound sources
                    int numObjects = level.Objects.Count;
                    writer.Write(numObjects);
                    foreach (var o in level.Objects.Values)
                    {
                        writer.Write(o.Id);
                        writer.Write((byte)o.Type);
                        writer.Write(o.Position.X);
                        writer.Write(o.Position.Y);
                        writer.Write(o.Position.Z);
                        writer.Write((short)level.Rooms.ReferenceIndexOf(o.Room));
                        writer.Write(o.Ocb);
                        writer.Write(o.Rotation);
                        writer.Write(o.Invisible);
                        writer.Write(o.ClearBody);
                        writer.Write(o.Bits[0]);
                        writer.Write(o.Bits[1]);
                        writer.Write(o.Bits[2]);
                        writer.Write(o.Bits[3]);
                        writer.Write(o.Bits[4]);

                        switch (o.Type)
                        {
                            case ObjectInstanceType.StaticMesh:
                                var sm = (StaticMeshInstance)o;
                                writer.Write(sm.Model.ObjectID);
                                writer.Write(sm.Color.R);
                                writer.Write(sm.Color.G);
                                writer.Write(sm.Color.B);

                                writer.Write(filler8);
                                break;
                            case ObjectInstanceType.Moveable:
                                var m = (MoveableInstance)o;
                                writer.Write(m.Model?.ObjectID ?? 0);

                                writer.Write(filler32);
                                break;
                            case ObjectInstanceType.Camera:
                                var c = (CameraInstance)o;
                                writer.Write(c.Fixed);

                                writer.Write(filler8);
                                writer.Write(filler8);
                                writer.Write(filler8);
                                writer.Write(filler32);
                                break;
                            case ObjectInstanceType.Sink:
                            {
                                var s = (SinkInstance)o;
                                writer.Write(s.Strength);

                                writer.Write(filler8);
                                writer.Write(filler8);
                                writer.Write(filler32);
                            }
                                break;
                            case ObjectInstanceType.Sound:
                            {
                                var s = (SoundInstance)o;
                                writer.Write(s.SoundId);

                                writer.Write(filler8);
                                writer.Write(filler8);
                                writer.Write(filler32);
                            }
                                break;
                        }

                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                    }

                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);

                    // Write triggers
                    int numTriggers = level.Triggers.Count;
                    writer.Write(numTriggers);
                    foreach (var o in level.Triggers.Values)
                    {
                        writer.Write(o.Id);
                        writer.Write(o.X);
                        writer.Write(o.Z);
                        writer.Write(o.NumXBlocks);
                        writer.Write(o.NumZBlocks);
                        writer.Write((byte)o.TriggerType);
                        writer.Write((byte)o.TargetType);
                        writer.Write(o.Target);
                        writer.Write(o.Timer);
                        writer.Write(o.OneShot);
                        writer.Write(o.Bits[0]);
                        writer.Write(o.Bits[1]);
                        writer.Write(o.Bits[2]);
                        writer.Write(o.Bits[3]);
                        writer.Write(o.Bits[4]);
                        writer.Write((short)level.Rooms.ReferenceIndexOf(o.Room));

                        writer.Write(filler16);
                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                    }

                    // Write rooms
                    const int numRooms = 255;
                    writer.Write(numRooms);
                    for (int i = 0; i < numRooms; i++)
                    {
                        var roomMagicWord = System.Text.Encoding.ASCII.GetBytes("ROOM");
                        writer.Write(roomMagicWord);

                        var r = level.Rooms[i];

                        if (r == null)
                        {
                            writer.Write(false);
                            continue;
                        }
                        else
                        {
                            writer.Write(true);
                        }

                        if (r.Name == null)
                            r.Name = "Room " + i.ToString();

                        writer.Write(System.Text.Encoding.UTF8.GetBytes(r.Name.PadRight(100, ' ')));
                        writer.Write(r.Position.X);
                        writer.Write(r.Position.Y);
                        writer.Write(r.Position.Z);
                        writer.Write(r.Ceiling);
                        writer.Write(r.NumXSectors);
                        writer.Write(r.NumZSectors);

                        for (int z = 0; z < r.NumZSectors; z++)
                        {
                            for (int x = 0; x < r.NumXSectors; x++)
                            {
                                var b = r.Blocks[x, z];

                                writer.Write((byte)b.Type);
                                writer.Write((short)b.Flags);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.QAFaces[n]);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.EDFaces[n]);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.WSFaces[n]);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.RFFaces[n]);
                                writer.Write(b.SplitFoorType);
                                writer.Write(b.SplitFloor);
                                writer.Write(b.SplitCeilingType);
                                writer.Write(b.SplitCeiling);
                                writer.Write(b.RealSplitFloor);
                                writer.Write(b.RealSplitCeiling);
                                for (int n = 0; n < 4; n++)
                                    writer.Write(b.Climb[n]);
                                writer.Write((byte)b.FloorOpacity);
                                writer.Write((byte)b.CeilingOpacity);
                                writer.Write((byte)b.WallOpacity);
                                writer.Write((int) b.FloorPortal.Id);
                                writer.Write((int) b.CeilingPortal.Id);
                                writer.Write(b.WallPortal);
                                writer.Write(b.IsFloorSolid);
                                writer.Write(b.IsCeilingSolid);
                                writer.Write(b.NoCollisionFloor);
                                writer.Write(b.NoCollisionCeiling);

                                foreach (var f in b.Faces)
                                {
                                    writer.Write(f.Defined);
                                    writer.Write(f.Flipped);
                                    writer.Write(f.Texture);
                                    writer.Write(f.Rotation);
                                    writer.Write(f.Transparent);
                                    writer.Write(f.DoubleSided);
                                    writer.Write(f.Invisible);
                                    writer.Write(f.NoCollision);
                                    writer.Write((byte)f.TextureTriangle);
                                    for (int n = 0; n < 4; n++)
                                        writer.Write(f.RectangleUV[n]);
                                    for (int n = 0; n < 3; n++)
                                        writer.Write(f.TriangleUV[n]);
                                    for (int n = 0; n < 3; n++)
                                        writer.Write(f.TriangleUV2[n]);
                                    writer.Write(filler32);
                                    writer.Write(filler32);
                                    writer.Write(filler32);
                                    writer.Write(filler32);
                                }

                                writer.Write((byte)b.FloorDiagonalSplit);
                                writer.Write((byte)b.FloorDiagonalSplitType);
                                writer.Write((byte)b.CeilingDiagonalSplit);
                                writer.Write((byte)b.CeilingDiagonalSplitType);

                                writer.Write(filler32);
                                writer.Write(filler32);
                                writer.Write(filler32);
                                writer.Write(filler32);
                            }
                        }

                        int numLights = r.Lights.Count;
                        writer.Write(numLights);
                        foreach (var l in r.Lights)
                        {
                            writer.Write((byte)l.Type);
                            writer.Write(l.Position.X);
                            writer.Write(l.Position.Y);
                            writer.Write(l.Position.Z);
                            writer.Write(l.Intensity);
                            writer.Write(l.Color.R);
                            writer.Write(l.Color.G);
                            writer.Write(l.Color.B);
                            writer.Write(l.In);
                            writer.Write(l.Out);
                            writer.Write(l.Len);
                            writer.Write(l.Cutoff);
                            writer.Write(l.DirectionX);
                            writer.Write(l.DirectionY);

                            byte b = (byte)l.Face;
                            writer.Write(b);

                            writer.Write(filler8);
                            writer.Write(filler8);
                            writer.Write(filler8);
                        }

                        writer.Write((byte)r.AmbientLight.R);
                        writer.Write((byte)r.AmbientLight.G);
                        writer.Write((byte)r.AmbientLight.B);
                        writer.Write(r.Flipped);
                        writer.Write((short)level.Rooms.ReferenceIndexOf(r.AlternateRoom));
                        writer.Write(r.AlternateGroup);
                        writer.Write(r.WaterLevel);
                        writer.Write(r.MistLevel);
                        writer.Write(r.ReflectionLevel);
                        writer.Write(r.FlagCold);
                        writer.Write(r.FlagDamage);
                        writer.Write(r.FlagHorizon);
                        writer.Write(r.FlagMist);
                        writer.Write(r.FlagOutside);
                        writer.Write(r.FlagRain);
                        writer.Write(r.FlagReflection);
                        writer.Write(r.FlagSnow);
                        writer.Write(r.FlagWater);
                        writer.Write(r.FlagQuickSand);
                        writer.Write(r.Flipped);
                        writer.Write((short)level.Rooms.ReferenceIndexOf(r.BaseRoom));
                        writer.Write(r.ExcludeFromPathFinding);

                        writer.Write(filler32);
                        writer.Write(filler32);
                        writer.Write(filler32);
                    }

                    // Write animated textures
                    int numAnimatedTextures = level.AnimatedTextures.Count;
                    writer.Write(numAnimatedTextures);
                    foreach (var textureSet in level.AnimatedTextures)
                    {
                        writer.Write((byte)textureSet.Effect);

                        int numTexturesInSet = textureSet.Textures.Count;
                        writer.Write(numTexturesInSet);

                        foreach (var texture in textureSet.Textures)
                        {
                            writer.Write(texture.Page);
                            writer.Write(texture.X);
                            writer.Write(texture.Y);
                        }
                    }

                    int numTextureSounds = level.TextureSounds.Count;
                    writer.Write(numTextureSounds);
                    foreach (var sound in level.TextureSounds)
                    {
                        writer.Write(sound.X);
                        writer.Write(sound.Y);
                        writer.Write(sound.Page);
                        writer.Write((byte)sound.Sound);
                    }

                    int numPalette = Editor.Instance.Palette.Count;
                    writer.Write(numPalette);
                    for (int i = 0; i < numPalette; i++)
                    {
                        writer.Write(Editor.Instance.Palette[i].R);
                        writer.Write(Editor.Instance.Palette[i].G);
                        writer.Write(Editor.Instance.Palette[i].B);
                    }

                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);
                    writer.Write(filler32);

                    writer.Flush();

                    projectData = ms.ToArray();
                }

                using (var writer = new BinaryWriterEx(File.OpenWrite(filename)))
                {
                    writer.Write(projectData);
                    writer.Flush();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }

            level.FileName = filename;

            return true;
        }
    }
}
