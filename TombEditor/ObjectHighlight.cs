using TombLib.LevelData;

namespace TombEditor
{
    public abstract class HighlightedObjects
    {
        public abstract bool Contains(ObjectInstance obj);

        public static HighlightedObjects Create(ObjectInstance obj)
        {
            var objectGroup = obj as ObjectGroup;
            if (objectGroup != null)
                return new MultiHighlightedObjects(objectGroup);

            return new SingleHighlightedObject(obj);
        }

        private class SingleHighlightedObject : HighlightedObjects
        {
            private readonly ObjectInstance _selectedObject;

            public SingleHighlightedObject(ObjectInstance selectedObject)
            {
                _selectedObject = selectedObject;
            }

            public override bool Contains(ObjectInstance obj)
            {
                return obj == _selectedObject;
            }
        }

        private class MultiHighlightedObjects : HighlightedObjects
        {
            private readonly ObjectGroup _selectedGroup;

            public MultiHighlightedObjects(ObjectGroup selectedGroup)
            {
                _selectedGroup = selectedGroup;
            }

            public override bool Contains(ObjectInstance obj)
            {
                var positionBased = obj as PositionBasedObjectInstance;
                return positionBased != null && _selectedGroup.Contains(positionBased);
            }
        }
    }
}
