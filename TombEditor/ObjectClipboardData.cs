using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                var objectInstances = new List<ObjectInstance>();

                if (editor.SelectedObject is ObjectGroup)
                {
                    var og = (ObjectGroup)editor.SelectedObject;
                    objectInstances.AddRange(og);
                }
                else
                    objectInstances.Add(editor.SelectedObject);

                Prj2Writer.SaveToPrj2OnlyObjects(stream, editor.Level, objectInstances);
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
            var newLevelSettings = editor.Level.Settings.Clone();
            var loadedObjects = CreateObjects(editor.Level);
            
            if (loadedObjects.Objects.Count == 0)
                return null;
                
            var unpackedObjects = loadedObjects.Objects.Select(obj =>
            {
                obj.CopyDependentLevelSettings(
                    new Room.CopyDependentLevelSettingsArgs(null, newLevelSettings, loadedObjects.Settings, true));

                // A little workaround to detect script id collisions already

                var testRoom = editor.SelectedRoom;

                if (obj is IHasScriptID)
                {
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

                if (obj is IHasLuaName)
                {
                    testRoom.AddObject(editor.Level, obj);
                    var luaObj = obj as IHasLuaName;
                    if (!luaObj.TrySetLuaName(luaObj.LuaName, null))
                        luaObj.AllocateNewLuaName();
                    testRoom.RemoveObject(editor.Level, obj);
                }

                return obj;
            })
            .ToList();
                
			editor.UpdateLevelSettings(newLevelSettings);

            if (unpackedObjects.Count == 0)
                return null;
            else if (unpackedObjects.Count == 1)
                return unpackedObjects.FirstOrDefault();
            else
            {
                var unpackedChildren = unpackedObjects.OfType<PositionBasedObjectInstance>().ToList();
                return new ObjectGroup(unpackedChildren);
            }
        }
    }
}