using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TombLib.IO;
using TombLib.LevelData;
using TombLib.LevelData.IO;

namespace TombEditor
{
    [Serializable]
    public class ObjectClipboardData
    {
        private byte[] _data;
        private string _levelPath;

        public ObjectClipboardData(Editor editor)
        {
            // Write data
            _levelPath = editor.Level.Settings.LevelFilePath ?? "";
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriterEx(stream);
                Prj2Writer.SaveToPrj2OnlyObjects(stream, editor.Level, new ObjectInstance[] { editor.SelectedObject });
                _data = stream.GetBuffer();
            }
        }

        public Prj2Loader.LoadedObjects CreateObjects()
        {
            using (var stream = new MemoryStream(_data, false))
            {
                var loadedObjects = Prj2Loader.LoadFromPrj2OnlyObjects(_levelPath, stream,
                    new Prj2Loader.Settings { IgnoreTextures = true, IgnoreWads = true });
                return loadedObjects;
            }
        }

        public PositionBasedObjectInstance MergeGetSingleObject(Editor editor)
        {
            Prj2Loader.LoadedObjects loadedObjects = CreateObjects();
            PositionBasedObjectInstance obj = (PositionBasedObjectInstance)(loadedObjects.Objects[0]);
            LevelSettings newLevelSettings = editor.Level.Settings.Clone();
            obj.CopyDependentLevelSettings(newLevelSettings, loadedObjects.Settings, true);
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