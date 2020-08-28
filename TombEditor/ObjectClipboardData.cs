using System;
using System.IO;
using TombLib.IO;
using TombLib.LevelData;
using TombLib.LevelData.IO;

namespace TombEditor
{
    [Serializable]
    public class ObjectClipboardData
    {
        private readonly byte[] _data;
        private readonly string _levelPath;

        public ObjectClipboardData(Editor editor)
        {
            // Write data
            _levelPath = editor.Level.Settings.LevelFilePath ?? "";
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriterEx(stream);
                Prj2Writer.SaveToPrj2OnlyObjects(stream, editor.Level, new[] { editor.SelectedObject });
                _data = stream.GetBuffer();
            }
        }

        public Prj2Loader.LoadedObjects CreateObjects(Level level)
        {
            using (var stream = new MemoryStream(_data, false))
            {
                var loadedObjects = Prj2Loader.LoadFromPrj2OnlyObjects(_levelPath, level, stream,
                    new Prj2Loader.Settings { IgnoreTextures = true, IgnoreWads = true });
                return loadedObjects;
            }
        }

        public ObjectInstance MergeGetSingleObject(Editor editor)
        {
            Prj2Loader.LoadedObjects loadedObjects = CreateObjects(editor.Level);

            if (loadedObjects.Objects.Count == 0)
                return null;

            ObjectInstance obj = (ObjectInstance)loadedObjects.Objects[0];
            LevelSettings newLevelSettings = editor.Level.Settings.Clone();
            obj.CopyDependentLevelSettings(new Room.CopyDependentLevelSettingsArgs(null, newLevelSettings, loadedObjects.Settings, true));
            editor.UpdateLevelSettings(newLevelSettings);

            // A little workaround to detect script id collisions already
            if (obj is IHasScriptID)
            {
                Room testRoom = editor.SelectedRoom;
                try
                {
                    testRoom.AddObject(editor.Level, obj);
                    testRoom.RemoveObject(editor.Level, obj);
                }
                catch (ScriptIdCollisionException)
                {
                    ((IHasScriptID)obj).ScriptId = null;
                }
            }
            return obj;
        }
    }
}