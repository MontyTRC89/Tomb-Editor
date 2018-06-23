using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using TombLib.LevelData;

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
                    Block[,] sectors = new Block[selection.Area.Width, selection.Area.Height];
                    for (int x = 0; x < Width; x++)
                        for (int z = 0; z < Height; z++)
                        {
                            var currX = selection.Area.X0 + x;
                            var currZ = selection.Area.Y0 + z;
                            var b = editor.SelectedRoom.Blocks[currX, currZ];

                            for (int i = 0; i < 4; i++)
                                writer.Write(b.QA[i]);
                            for (int i = 0; i < 4; i++)
                                writer.Write(b.WS[i]);
                            for (int i = 0; i < 4; i++)
                                writer.Write(b.ED[i]);
                            for (int i = 0; i < 4; i++)
                                writer.Write(b.RF[i]);

                            writer.Write((int)b.Type);
                            writer.Write(b.ForceFloorSolid);
                            writer.Write(b.FloorSplitDirectionToggled);
                            writer.Write(b.FloorSplitDirectionIsXEqualsZ);
                            writer.Write((int)b.FloorDiagonalSplit);
                            writer.Write(b.CeilingSplitDirectionToggled);
                            writer.Write(b.CeilingSplitDirectionIsXEqualsZ);
                            writer.Write((int)b.CeilingDiagonalSplit);
                            writer.Write((int)b.Flags);
                        }
                }

                _data = ms.ToArray();
            }
        }

        public Block[,] GetSectors()
        {
            if (_data == null)
                return null;

            var sectors = new Block[Width, Height];

            using (var ms = new MemoryStream(_data))
            {
                using (var reader = new BinaryReader(ms))
                {
                    for (int x = 0; x < Width; x++)
                        for (int z = 0; z < Height; z++)
                        {
                            var b = sectors[x, z] = new Block(0, 12);

                            for (int i = 0; i < 4; i++)
                                b.QA[i] = reader.ReadInt16();
                            for (int i = 0; i < 4; i++)
                                b.WS[i] = reader.ReadInt16();
                            for (int i = 0; i < 4; i++)
                                b.ED[i] = reader.ReadInt16();
                            for (int i = 0; i < 4; i++)
                                b.RF[i] = reader.ReadInt16();

                            b.Type = (BlockType)reader.ReadInt32();
                            b.ForceFloorSolid = reader.ReadBoolean();
                            b.FloorSplitDirectionToggled = reader.ReadBoolean();
                            b.FloorSplitDirectionIsXEqualsZ = reader.ReadBoolean();
                            b.FloorDiagonalSplit = (DiagonalSplit)reader.ReadInt32();
                            b.CeilingSplitDirectionToggled = reader.ReadBoolean();
                            b.CeilingSplitDirectionIsXEqualsZ = reader.ReadBoolean();
                            b.CeilingDiagonalSplit = (DiagonalSplit)reader.ReadInt32();
                            b.Flags = (BlockFlags)reader.ReadInt32();

                            sectors[x, z] = b;
                        }
                }
            }

            return sectors;
        }
    }
}
