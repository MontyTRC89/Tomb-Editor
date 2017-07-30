using SharpDX;
using TombEditor.Geometry;

namespace TombEditor
{
    public enum ClipboardElementType
    {
        None,
        Moveable,
        StaticMesh,
        Light,
        Sink,
        SoundSource,
        Camera,
        FlybyCamera
    }

    public enum PasteAction
    {
        None,
        Paste,
        Stamp
    }

    public class Clipboard
    {
        public static ClipboardElementType ElementType { get; set; }
        public static int ElementID { get; set; }
        public static Room OriginalRoom { get; set; }
        public static PasteAction Action { get; set; }

        private static Editor _editor;

        public static void Copy()
        {
            _editor = Editor.Instance;

            OriginalRoom = _editor.SelectedRoom;

            if (_editor.PickingResult.ElementType == PickingElementType.Moveable)
            {
                ElementType = ClipboardElementType.Moveable;
                ElementID = _editor.PickingResult.Element;
            }
            else if (_editor.PickingResult.ElementType == PickingElementType.StaticMesh)
            {
                ElementType = ClipboardElementType.StaticMesh;
                ElementID = _editor.PickingResult.Element;
            }
            else if (_editor.PickingResult.ElementType == PickingElementType.Camera)
            {
                ElementType = ClipboardElementType.Camera;
                ElementID = _editor.PickingResult.Element;
            }
            else if (_editor.PickingResult.ElementType == PickingElementType.FlyByCamera)
            {
                ElementType = ClipboardElementType.FlybyCamera;
                ElementID = _editor.PickingResult.Element;
            }
            else if (_editor.PickingResult.ElementType == PickingElementType.Sink)
            {
                ElementType = ClipboardElementType.Sink;
                ElementID = _editor.PickingResult.Element;
            }
            else if (_editor.PickingResult.ElementType == PickingElementType.SoundSource)
            {
                ElementType = ClipboardElementType.SoundSource;
                ElementID = _editor.PickingResult.Element;
            }
            else if (_editor.PickingResult.ElementType == PickingElementType.Light)
            {
                ElementType = ClipboardElementType.Light;
                ElementID = _editor.PickingResult.Element;
            }
        }

        public static bool Paste()
        {
            _editor = Editor.Instance;

            var room = _editor.SelectedRoom;

            bool hasPastedSomething = false;

            if (_editor.PickingResult.ElementType == PickingElementType.Block)
            {
                int x = _editor.PickingResult.Element >> 5;
                int z = _editor.PickingResult.Element & 31;

                Block block = room.Blocks[x, z];
                int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;

                Vector3 position = new Vector3(x * 1024 + 512, y * 256, z * 1024 + 512);

                ObjectInstance instance;

                switch (ElementType)
                {
                    case ClipboardElementType.Light:
                        Light light = OriginalRoom.Lights[ElementID].Clone();

                        light.X = x;
                        light.Y = y;
                        light.Z = z;
                        light.Position = position;

                        room.Lights.Add(light);

                        room.BuildGeometry();
                        room.CalculateLightingForThisRoom();
                        room.UpdateBuffers();

                        hasPastedSomething = true;

                        break;

                    case ClipboardElementType.Camera:
                        instance = _editor.Level.Objects[ElementID].Clone();

                        instance.X = (byte)x;
                        instance.Y = (short)y;
                        instance.Z = (byte)z;
                        instance.Position = position;
                        instance.Id = _editor.Level.GetNewObjectId();
                        instance.Room = room;

                        _editor.Level.Objects.Add(instance.Id, instance);
                        room.Cameras.Add(instance.Id);

                        hasPastedSomething = true;

                        break;

                    case ClipboardElementType.FlybyCamera:
                        instance = _editor.Level.Objects[ElementID].Clone();

                        instance.X = (byte)x;
                        instance.Y = (short)y;
                        instance.Z = (byte)z;
                        instance.Position = position;
                        instance.Id = _editor.Level.GetNewObjectId();
                        instance.Room = room;

                        _editor.Level.Objects.Add(instance.Id, instance);
                        room.FlyByCameras.Add(instance.Id);

                        hasPastedSomething = true;

                        break;

                    case ClipboardElementType.Sink:
                        instance = _editor.Level.Objects[ElementID].Clone();

                        instance.X = (byte)x;
                        instance.Y = (short)y;
                        instance.Z = (byte)z;
                        instance.Position = position;
                        instance.Id = _editor.Level.GetNewObjectId();
                        instance.Room = room;

                        _editor.Level.Objects.Add(instance.Id, instance);
                        room.Sinks.Add(instance.Id);

                        hasPastedSomething = true;

                        break;

                    case ClipboardElementType.SoundSource:
                        instance = _editor.Level.Objects[ElementID].Clone();

                        instance.X = (byte)x;
                        instance.Y = (short)y;
                        instance.Z = (byte)z;
                        instance.Position = position;
                        instance.Id = _editor.Level.GetNewObjectId();
                        instance.Room = room;

                        _editor.Level.Objects.Add(instance.Id, instance);
                        room.SoundSources.Add(instance.Id);

                        hasPastedSomething = true;

                        break;

                    case ClipboardElementType.Moveable:
                        instance = _editor.Level.Objects[ElementID].Clone();

                        instance.X = (byte)x;
                        instance.Y = (short)y;
                        instance.Z = (byte)z;
                        instance.Position = position;
                        instance.Id = _editor.Level.GetNewObjectId();
                        instance.Room = room;

                        _editor.Level.Objects.Add(instance.Id, instance);
                        room.Moveables.Add(instance.Id);

                        hasPastedSomething = true;

                        break;

                    case ClipboardElementType.StaticMesh:
                        instance = _editor.Level.Objects[ElementID].Clone();

                        instance.X = (byte)x;
                        instance.Y = (short)y;
                        instance.Z = (byte)z;
                        instance.Position = position;
                        instance.Id = _editor.Level.GetNewObjectId();
                        instance.Room = room;

                        _editor.Level.Objects.Add(instance.Id, instance);
                        room.StaticMeshes.Add(instance.Id);

                        hasPastedSomething = true;

                        break;

                }
            }

            if (Action == PasteAction.Paste)
            {
                Action = PasteAction.None;
                ElementType = ClipboardElementType.None;
                OriginalRoom = null;
                ElementID = -1;
                _editor.ResetPanel3DCursor();
            }

            return hasPastedSomething;
        }
    }
}
