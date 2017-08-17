using SharpDX;
using TombEditor.Geometry;

namespace TombEditor
{
    public class Clipboard
    {
        private static ObjectPtr? _objectPtr;
        private static Room _originalRoom;
        
        public static void Copy(Room originalRoom, ObjectPtr objectPtr)
        {
            _objectPtr = objectPtr;
            _originalRoom = originalRoom;
        }

        private static ObjectInstance CopyObject(int Index, Level level, Room room, Vector3 position)
        {
            PositionBasedObjectInstance result = (PositionBasedObjectInstance)level.Objects[_objectPtr.Value.Id].Clone();
            result.Position = position;
            result.Room = room;
            level.Objects.Add(result.Id, result);
            return result;
        }

        public static object Paste(Level level, Room room, DrawingPoint pos)
        {
            Block block = room.GetBlock(pos);
            int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;
            Vector3 position = new Vector3(pos.X * 1024 + 512, y * 256, pos.Y * 1024 + 512);
            
            if (_objectPtr.HasValue)
                switch (_objectPtr.Value.Type)
                {
                    case ObjectInstanceType.Light:
                        var light = _originalRoom.Lights[_objectPtr.Value.Id].Clone();
                        light.Position = position;

                        room.Lights.Add(light);

                        room.BuildGeometry();
                        room.CalculateLightingForThisRoom();
                        room.UpdateBuffers();
                        return light;

                    case ObjectInstanceType.Camera:
                        var camera = CopyObject(_objectPtr.Value.Id, level, room, position);
                        room.Cameras.Add(camera.Id);
                        return camera;

                    case ObjectInstanceType.FlyByCamera:
                        var flyByCamera = CopyObject(_objectPtr.Value.Id, level, room, position);
                        room.FlyByCameras.Add(flyByCamera.Id);
                        return flyByCamera;

                    case ObjectInstanceType.Sink:
                        var sink = CopyObject(_objectPtr.Value.Id, level, room, position);
                        room.Sinks.Add(sink.Id);
                        return sink;

                    case ObjectInstanceType.SoundSource:
                        var soundSource = CopyObject(_objectPtr.Value.Id, level, room, position);
                        room.SoundSources.Add(soundSource.Id);
                        return soundSource;

                    case ObjectInstanceType.Moveable:
                        var movable = CopyObject(_objectPtr.Value.Id, level, room, position);
                        room.Moveables.Add(movable.Id);
                        return movable;

                    case ObjectInstanceType.Static:
                        var staticObj = CopyObject(_objectPtr.Value.Id, level, room, position);
                        room.Statics.Add(staticObj.Id);
                        return staticObj;
                }

            return null;
        }
    }
}
