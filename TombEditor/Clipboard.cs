using SharpDX;
using TombEditor.Geometry;

namespace TombEditor
{
    public class Clipboard
    {
        private static PositionBasedObjectInstance _instance;
        
        public static void Copy(PositionBasedObjectInstance instance)
        {
            _instance = instance;
        }

        public static bool HasObjectToPaste { get { return _instance != null; } }

        public static PositionBasedObjectInstance Paste(Level level, Room room, DrawingPoint pos)
        {
            if (_instance == null)
                return null;

            Block block = room.GetBlock(pos);
            int y = (block.QAFaces[0] + block.QAFaces[1] + block.QAFaces[2] + block.QAFaces[3]) / 4;
            Vector3 position = new Vector3(pos.X * 1024 + 512, y * 256, pos.Y * 1024 + 512);
            
            var newObject = (PositionBasedObjectInstance)_instance.Clone();
            newObject.Position = position;
            room.AddObject(level, newObject);
            return newObject;
        }
    }
}
