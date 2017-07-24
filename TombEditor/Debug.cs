using SharpDX;
using SharpDX.Toolkit.Graphics;
using System;
using System.Collections.Generic;
using NLog;
using NLog.Config;
using NLog.Targets;
using TombEditor.Controls;

namespace TombEditor
{
    public class Debug
    {
        public static long NumRooms { get; set; }
        public static long NumVertices { get; set; }
        public static long NumTriangles { get; set; }
        public static long NumMoveables { get; set; }
        public static long NumStaticMeshes { get; set; }
        public static string SelectedItem { get; set; }
        public static List<DebugString> Strings { get; set; }

        private static Editor _editor;

        public static void Initialize()
        {
            _editor = Editor.Instance;
        }

        public static void Reset()
        {
            NumRooms = 0;
            NumVertices = 0;
            NumTriangles = 0;
            NumMoveables = 0;
            NumStaticMeshes = 0;
            //SelectedItem = "";
            Strings = new List<Controls.DebugString>();
        }

        public static void AddString(string content, Vector3 position)
        {
            DebugString str = new Controls.DebugString();
            str.Content = content;
            str.Position = new Vector2(position.X, position.Y);
            Strings.Add(str);
        }

        public static void Draw()
        {
            SpriteBatch batch = new SpriteBatch(_editor.GraphicsDevice);
            batch.Begin(SpriteSortMode.FrontToBack, _editor.GraphicsDevice.BlendStates.Additive);

            batch.DrawString(_editor.Font,
                "FPS: " + Math.Round(_editor.FPS, 2) + ", Vertices: " + NumVertices + ", Triangles: " + NumTriangles,
                new Vector2(0, 0), Color.White);
            batch.DrawString(_editor.Font,
                "Rooms: " + NumRooms + ", Moveables: " + NumMoveables + ", Static Meshes: " + NumStaticMeshes,
                new Vector2(0, 18), Color.White);
            if (SelectedItem != "")
                batch.DrawString(_editor.Font, "Selected Object: " + SelectedItem, new Vector2(0, 36), Color.White);

            for (int i = 0; i < Strings.Count; i++)
            {
                batch.DrawString(_editor.Font, Strings[i].Content, Strings[i].Position, Color.White);
            }

            batch.End();

            _editor.GraphicsDevice.SetBlendState(_editor.GraphicsDevice.BlendStates.Opaque);
        }

        public static void InitLogging()
        {
            var config = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            consoleTarget.Layout =
                @"[${date:format=HH\:mm\:ss.fff} ${pad:padding=5:inner=${level:uppercase=true}}] ${logger} | ${message}";

            var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule1);

            LogManager.Configuration = config;
        }
    }
}
