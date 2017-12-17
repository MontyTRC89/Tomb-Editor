using SharpDX;
using TombEditor.Geometry;

namespace TombEditor
{
    public class ClipboardC
    {
        private static PositionBasedObjectInstance _instance;

        public static void Copy(PositionBasedObjectInstance instance)
        {
            _instance = instance;
        }

        public static bool HasObjectToPaste { get { return _instance != null; } }

        public static PositionBasedObjectInstance Retrieve()
        {
            return (PositionBasedObjectInstance)_instance?.Clone();
        }
    }
}
