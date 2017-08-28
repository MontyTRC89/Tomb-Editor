using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;

namespace TombEditor
{
    public struct DebugString
    {
        public Vector2 Position;
        public string Content;
    }

    public class Debug
    {
        public static float Fps { get; set; }
        public static long NumRooms { get; set; }
        public static long NumVerticesRooms { get; set; }
        public static long NumTrianglesRooms { get; set; }
        public static long NumVerticesObjects { get; set; }
        public static long NumTrianglesObjects { get; set; }
        public static long NumMoveables { get; set; }
        public static long NumStaticMeshes { get; set; }
        public static List<DebugString> Strings { get; set; }
        
        public static void Reset()
        {
            NumRooms = 0;
            NumVerticesRooms = 0;
            NumTrianglesRooms = 0;
            NumVerticesObjects = 0;
            NumTrianglesObjects = 0;
            NumMoveables = 0;
            NumStaticMeshes = 0;
            //SelectedItem = "";
            Strings = new List<DebugString>();
        }

        public static void AddString(string content, Vector3 position)
        {
            DebugString str = new DebugString();
            str.Content = content;
            str.Position = new Vector2(position.X, position.Y);
            Strings.Add(str);
        }

        public static void Draw(DeviceManager deviceManager, string selectedItem)
        {
            SpriteBatch batch = new SpriteBatch(deviceManager.Device);
            batch.Begin(SpriteSortMode.FrontToBack, deviceManager.Device.BlendStates.Additive);

            batch.DrawString(deviceManager.Font, "FPS: " + Math.Round(Fps, 2) + ", Rooms vertices: " + NumVerticesRooms + ", Objects vertices: " + NumVerticesObjects, new Vector2(0, 0), Color.White);
            batch.DrawString(deviceManager.Font, "Rooms: " + NumRooms + ", Moveables: " + NumMoveables + ", Static Meshes: " + NumStaticMeshes, new Vector2(0, 18), Color.White);
            if (!string.IsNullOrEmpty(selectedItem))
                batch.DrawString(deviceManager.Font, "Selected Object: " + selectedItem, new Vector2(0, 36), Color.White);

            for (int i = 0; i < Strings.Count; i++)
            {
                batch.DrawString(deviceManager.Font, Strings[i].Content, Strings[i].Position, Color.White);
            }

            batch.End();

            deviceManager.Device.SetBlendState(deviceManager.Device.BlendStates.Opaque);
        }
    }
}
