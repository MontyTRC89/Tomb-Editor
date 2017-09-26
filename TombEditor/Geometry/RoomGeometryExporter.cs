using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;

namespace TombEditor.Geometry
{
    public class RoomGeometryExporter
    {
        public static void ExportToObj(Room room, string fileName)
        {
            var roomVertices = room.GetRoomVertices();

            using (var writer = new StreamWriter(new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write)))
            {
                writer.WriteLine("# Exported by Tomb Editor");
                writer.WriteLine("o Room");

                // Export positions
                foreach (var vertex in roomVertices)
                {
                    writer.WriteLine("v " + vertex.Position.X + " " + vertex.Position.Y + " " + vertex.Position.Z + " 1.0");
                }

                // TODO: to remove when materials will be exported
                Random r = new Random(new Random().Next());

                // Export UVs
                foreach (var vertex in roomVertices)
                {
                    // TODO: to remove when materials will be exported
                    float randX = r.NextFloat(0, 1);
                    float randY = r.NextFloat(0, 1);

                    writer.WriteLine("vt " + /*vertex.UV.X*/ randX + " " + randY /* vertex.UV.Y*/);
                }

                writer.WriteLine("s 1");

                // Export faces
                for (int i = 0; i < roomVertices.Count; ++i)
                    writer.WriteLine("f " + i + " " + (i + 1) + " " + (i + 2));

                writer.Flush();
            }
        }
    }
}
