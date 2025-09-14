using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using TombLib;
using TombLib.LevelData;
using TombLib.LevelData.SectorEnums;
using TombLib.LevelData.SectorStructs;
using TombLib.Utils;

namespace TombEditor
{
    [Serializable]
    public class SectorsClipboardData
    {
        private readonly byte[] _data;

        public int Width { get; set; }
        public int Height { get; set; }

        public SectorsClipboardData(Editor editor)
        {
            var selection = editor.SelectedSectors;

            Width = selection.Area.Width + 1;
            Height = selection.Area.Height + 1;

            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    Sector[,] sectors = new Sector[selection.Area.Width, selection.Area.Height];
                    for (int x = 0; x < Width; x++)
                        for (int z = 0; z < Height; z++)
                        {
                            var currX = selection.Area.X0 + x;
                            var currZ = selection.Area.Y0 + z;
                            var b = editor.SelectedRoom.Sectors[currX, currZ];

                            SectorVerticalPart[] verticals = b.GetVerticals().ToArray();
                            writer.Write(verticals.Length);

                            foreach (SectorVerticalPart vertical in verticals)
                            {
                                writer.Write((byte)vertical);

                                for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                                    writer.Write(b.GetHeight(vertical, edge));
                            }

                            writer.Write((byte)b.Type);
                            writer.Write(b.ForceFloorSolid);
                            writer.Write(b.Floor.SplitDirectionToggled);
                            writer.Write(b.Floor.SplitDirectionIsXEqualsZ);
                            writer.Write((byte)b.Floor.DiagonalSplit);
                            writer.Write(b.Ceiling.SplitDirectionToggled);
                            writer.Write(b.Ceiling.SplitDirectionIsXEqualsZ);
                            writer.Write((byte)b.Ceiling.DiagonalSplit);
                            writer.Write((short)b.Flags);

                            Dictionary<FaceLayerInfo, TextureArea> textures = b.GetAllFaceTextures();
                            writer.Write(textures.Count);

                            foreach (KeyValuePair<FaceLayerInfo, TextureArea> texturePair in textures)
                            {
                                writer.Write((byte)texturePair.Key.Face);
                                writer.Write((byte)texturePair.Key.Layer);

                                writer.Write(texturePair.Value.Texture.Image.FileName);
                                writer.Write(texturePair.Value.TextureIsInvisible);

                                if (string.IsNullOrEmpty(texturePair.Value.Texture.Image.FileName))
                                    continue;

                                writer.Write(texturePair.Value.TexCoord0.X);
                                writer.Write(texturePair.Value.TexCoord0.Y);
                                writer.Write(texturePair.Value.TexCoord1.X);
                                writer.Write(texturePair.Value.TexCoord1.Y);
                                writer.Write(texturePair.Value.TexCoord2.X);
                                writer.Write(texturePair.Value.TexCoord2.Y);
                                writer.Write(texturePair.Value.TexCoord3.X);
                                writer.Write(texturePair.Value.TexCoord3.Y);

                                writer.Write(texturePair.Value.ParentArea.Start.X);
                                writer.Write(texturePair.Value.ParentArea.Start.Y);
                                writer.Write(texturePair.Value.ParentArea.End.X);
                                writer.Write(texturePair.Value.ParentArea.End.Y);

                                writer.Write((ushort)texturePair.Value.BlendMode);
                                writer.Write(texturePair.Value.DoubleSided);
                            }
                        }
                }

                _data = ms.ToArray();
            }
        }

        public Sector[,] GetSectors()
        {
            if (_data == null)
                return null;

            var sectors = new Sector[Width, Height];

            using (var ms = new MemoryStream(_data))
            {
                using (var reader = new BinaryReader(ms))
                {
                    for (int x = 0; x < Width; x++)
                        for (int z = 0; z < Height; z++)
                        {
                            var b = sectors[x, z] = new Sector(0, Room.DefaultHeight);

                            int verticalsCount = reader.ReadInt32();

                            for (int i = 0; i < verticalsCount; i++)
                            {
                                var vertical = (SectorVerticalPart)reader.ReadByte();

                                for (SectorEdge edge = 0; edge < SectorEdge.Count; ++edge)
                                    b.SetHeight(vertical, edge, reader.ReadInt32());
                            }

                            b.Type = (SectorType)reader.ReadByte();
                            b.ForceFloorSolid = reader.ReadBoolean();
                            b.Floor.SplitDirectionToggled = reader.ReadBoolean();
                            b.Floor.SplitDirectionIsXEqualsZ = reader.ReadBoolean();
                            b.Floor.DiagonalSplit = (DiagonalSplit)reader.ReadByte();
                            b.Ceiling.SplitDirectionToggled = reader.ReadBoolean();
                            b.Ceiling.SplitDirectionIsXEqualsZ = reader.ReadBoolean();
                            b.Ceiling.DiagonalSplit = (DiagonalSplit)reader.ReadByte();
                            b.Flags = (SectorFlags)reader.ReadInt16();

                            int texturesCount = reader.ReadInt32();

                            for (int i = 0; i < texturesCount; i++)
                            {
                                var face = (SectorFace)reader.ReadByte();
                                var layer = (FaceLayer)reader.ReadByte();
                                var layerInfo = new FaceLayerInfo(face, layer);

                                string textureFileName = reader.ReadString();
                                bool isInvisible = reader.ReadBoolean();

                                if (string.IsNullOrEmpty(textureFileName))
                                {
                                    b.SetFaceTexture(layerInfo, isInvisible ? TextureArea.Invisible : TextureArea.None);
                                    continue;
                                }

                                var texture = new TextureArea
                                {
                                    Texture = Editor.Instance.Level.Settings.Textures.Find(t => t.Image.FileName == textureFileName),

                                    TexCoord0 = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                                    TexCoord1 = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                                    TexCoord2 = new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                                    TexCoord3 = new Vector2(reader.ReadSingle(), reader.ReadSingle()),

                                    ParentArea = new Rectangle2(
                                        reader.ReadSingle(),
                                        reader.ReadSingle(),
                                        reader.ReadSingle(),
                                        reader.ReadSingle()),

                                    BlendMode = (BlendMode)reader.ReadUInt16(),
                                    DoubleSided = reader.ReadBoolean()
                                };

                                b.SetFaceTexture(layerInfo, texture);
                            }

                            sectors[x, z] = b;
                        }
                }
            }

            return sectors;
        }
    }
}
