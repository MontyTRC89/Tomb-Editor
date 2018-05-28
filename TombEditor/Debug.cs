using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using TombLib.Graphics;

namespace TombEditor
{
    public struct DebugString
    {
        public Vector2 Position;
        public string Content;
    }

    public class Debug
    {
        public double Fps { get; set; }
        public int NumRooms { get; set; }
        public int NumVerticesRooms { get; set; }
        public int NumTrianglesRooms { get; set; }
        public int NumVerticesObjects { get; set; }
        public int NumTrianglesObjects { get; set; }
        public int NumMoveables { get; set; }
        public int NumStaticMeshes { get; set; }
        public List<DebugString> Strings { get; } = new List<DebugString>();

        public void AddString(string content, Vector3 position)
        {
            DebugString str = new DebugString();
            str.Content = content;
            str.Position = new Vector2(position.X, position.Y);
            Strings.Add(str);
        }

        public void Draw(DeviceManager deviceManager, string selectedItem, Vector4 textColor)
        {
            using (SpriteBatch batch = new SpriteBatch(deviceManager.___LegacyDevice))
            {
                batch.Begin(SpriteSortMode.FrontToBack, deviceManager.___LegacyDevice.BlendStates.AlphaBlend);

                batch.DrawStringOnOldSharpDx(deviceManager.___LegacyFont, "FPS: " + Math.Round(Fps, 2) + ", Rooms vertices: " + NumVerticesRooms + ", Objects vertices: " + NumVerticesObjects, new Vector2(0, 0), new SharpDX.Color(textColor.ToSharpDX()));
                batch.DrawStringOnOldSharpDx(deviceManager.___LegacyFont, "Rooms: " + NumRooms + ", Moveables: " + NumMoveables + ", Static Meshes: " + NumStaticMeshes, new Vector2(0, 18), new SharpDX.Color(textColor.ToSharpDX()));
                if (!string.IsNullOrEmpty(selectedItem))
                    batch.DrawStringOnOldSharpDx(deviceManager.___LegacyFont, "Selected Object: " + selectedItem, new Vector2(0, 36), new SharpDX.Color(textColor.ToSharpDX()));

                for (int i = 0; i < Strings.Count; i++)
                {
                    batch.DrawStringOnOldSharpDx(deviceManager.___LegacyFont, Strings[i].Content, Strings[i].Position, new SharpDX.Color(textColor.ToSharpDX()));
                }

                batch.End();
            }

            deviceManager.___LegacyDevice.SetBlendState(deviceManager.___LegacyDevice.BlendStates.Opaque);
        }
    }

    // Remove this class and use 'DrawString' once we have updated the SharpDX library so this becomes unnecessary
    public static class SpriteBatchExtensions
    {
        // There seems to be a bug in the old version of SharpDx we are currently using
        // that makes 'DrawString' crash the application when unknown characters are encountered
        // even though 'SpriteFont' is setup to use a 'DefaultCharacter' as a replacement.
        public static void DrawStringOnOldSharpDx(this SpriteBatch spriteBatch, SpriteFont spriteFont, string text, Vector2 position, SharpDX.Color color)
        {
            var text2 = new StringBuilder(text);
            if (spriteFont.DefaultCharacter.HasValue)
                for (int i = 0; i < text2.Length; ++i)
                    if (!spriteFont.Characters.Contains(text2[i]))
                        text2[i] = spriteFont.DefaultCharacter.Value;
            spriteBatch.DrawString(spriteFont, text2.ToString(), position.ToSharpDX(), color);
        }
    }
}
