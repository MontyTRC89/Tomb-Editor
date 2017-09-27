using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace TombEditor
{
    public struct DebugString
    {
        public Vector2 Position;
        public string Content;
    }

    public class Debug
    {
        public static double Fps { get; set; }
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
            Strings = new List<DebugString>();
        }

        public static void AddString(string content, Vector3 position)
        {
            DebugString str = new DebugString();
            str.Content = content;
            str.Position = new Vector2(position.X, position.Y);
            Strings.Add(str);
        }

        public static void Draw(DeviceManager deviceManager, string selectedItem, Vector4 textColor)
        {
            SpriteBatch batch = new SpriteBatch(deviceManager.Device);
            batch.Begin(SpriteSortMode.FrontToBack, deviceManager.Device.BlendStates.Additive);

            batch.DrawStringOnOldSharpDx(deviceManager.Font, "FPS: " + Math.Round(Fps, 2) + ", Rooms vertices: " + NumVerticesRooms + ", Objects vertices: " + NumVerticesObjects, new Vector2(0, 0), Color.White);
            batch.DrawStringOnOldSharpDx(deviceManager.Font, "Rooms: " + NumRooms + ", Moveables: " + NumMoveables + ", Static Meshes: " + NumStaticMeshes, new Vector2(0, 18), Color.White);
            if (!string.IsNullOrEmpty(selectedItem))
                batch.DrawStringOnOldSharpDx(deviceManager.Font, "Selected Object: " + selectedItem, new Vector2(0, 36), Color.White);

            for (int i = 0; i < Strings.Count; i++)
            {
                batch.DrawStringOnOldSharpDx(deviceManager.Font, Strings[i].Content, Strings[i].Position, new Color(textColor));
            }

            batch.End();

            deviceManager.Device.SetBlendState(deviceManager.Device.BlendStates.Opaque);
        }
    }

    // Remove this class and use 'DrawString' once we have updated the SharpDX library so this becomes unnecessary
    public static class SpriteBatchExtensions
    {
        // There seems to be a bug in the old version of SharpDx we are currently using
        // that makes 'DrawString' crash the application when unknown characters are encountered
        // even though 'SpriteFont' is setup to use a 'DefaultCharacter' as a replacement.
        public static void DrawStringOnOldSharpDx(this SpriteBatch spriteBatch, SpriteFont spriteFont, string text, Vector2 position, Color color)
        {
            var text2 = new StringBuilder(text);
            if (spriteFont.DefaultCharacter.HasValue)
                for (int i = 0; i < text2.Length; ++i)
                    if (!spriteFont.Characters.Contains(text2[i]))
                        text2[i] = spriteFont.DefaultCharacter.Value;
            spriteBatch.DrawString(spriteFont, text2, position, color);
        }
    }
}
