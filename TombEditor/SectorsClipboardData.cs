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

                            for (BlockVertical vertical = 0; vertical < BlockVertical.Count; ++vertical)
                                for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                                    writer.Write((short)b.GetHeight(vertical, edge));

                            writer.Write((int)b.Type);
                            writer.Write(b.ForceFloorSolid);
                            writer.Write(b.Floor.SplitDirectionToggled);
                            writer.Write(b.Floor.SplitDirectionIsXEqualsZ);
                            writer.Write((int)b.Floor.DiagonalSplit);
                            writer.Write(b.Ceiling.SplitDirectionToggled);
                            writer.Write(b.Ceiling.SplitDirectionIsXEqualsZ);
                            writer.Write((int)b.Ceiling.DiagonalSplit);
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

                            for (BlockVertical vertical = 0; vertical < BlockVertical.Count; ++vertical)
                                for (BlockEdge edge = 0; edge < BlockEdge.Count; ++edge)
                                    b.SetHeight(vertical, edge, reader.ReadInt16());

                            b.Type = (BlockType)reader.ReadInt32();
                            b.ForceFloorSolid = reader.ReadBoolean();
                            b.Floor.SplitDirectionToggled = reader.ReadBoolean();
                            b.Floor.SplitDirectionIsXEqualsZ = reader.ReadBoolean();
                            b.Floor.DiagonalSplit = (DiagonalSplit)reader.ReadInt32();
                            b.Ceiling.SplitDirectionToggled = reader.ReadBoolean();
                            b.Ceiling.SplitDirectionIsXEqualsZ = reader.ReadBoolean();
                            b.Ceiling.DiagonalSplit = (DiagonalSplit)reader.ReadInt32();
                            b.Flags = (BlockFlags)reader.ReadInt32();

                            sectors[x, z] = b;
                        }
                }
            }

            return sectors;
        }
    }
}
