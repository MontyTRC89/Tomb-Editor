using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace TombLib.LevelData
{
    /// <summary>
    /// Represents a group of objects multi-selected by ctrl-clicking.
    /// </summary>
    public class ObjectGroup : PositionBasedObjectInstance, IRotateableY, IEnumerable<PositionBasedObjectInstance>
    {
        private readonly HashSet<PositionBasedObjectInstance> _objects = new HashSet<PositionBasedObjectInstance>();

        public ObjectGroup(PositionBasedObjectInstance initialObject)
        {
            Room = initialObject.Room;
            Position = initialObject.Position;

            _objects.Add(initialObject);
        }

        public ObjectGroup(IReadOnlyList<PositionBasedObjectInstance> objects)
        {
            var initialObject = objects.First();

            Room = initialObject.Room;
            Position = initialObject.Position;

            foreach (var obj in objects)
            {
                _objects.Add(obj);
            }
        }

        public void Add(PositionBasedObjectInstance objectInstance) => _objects.Add(objectInstance);
        public void Remove(PositionBasedObjectInstance objectInstance) => _objects.Remove(objectInstance);
        public bool Contains(PositionBasedObjectInstance obInstance) => _objects.Contains(obInstance);
        public bool Any() => _objects.Any();

        public void AddOrRemove(PositionBasedObjectInstance objectInstance)
        {
            if (Contains(objectInstance))
            {
                Remove(objectInstance);
            }
            else
            {
                Add(objectInstance);
            }
        }

        protected override void SetPosition(Vector3 position)
        {
            var difference = position - Position;
            base.SetPosition(position);

            foreach (var i in _objects)
                i.Position = i.Position + difference;
        }

        private float _rotationY;

        public float RotationY
        {
            get
            {
                return _rotationY;
            }
            set
            {
                var difference = value - _rotationY;

                _rotationY = value;

                foreach (var i in _objects.OfType<IRotateableY>())
                    i.RotationY += difference;
            }
        }

        public void RotateAsGroup(float targetRotationDeg)
        {
            var rotationDifferenceRad = (targetRotationDeg - RotationY) * Math.PI / 180.0f;

            RotationY = targetRotationDeg;

            var sin = (float)Math.Sin(-rotationDifferenceRad);
            var cos = (float)Math.Cos(-rotationDifferenceRad);

            foreach (var i in _objects)
            {
                var distance = i.Position - Position;

                var x = distance.X * cos - distance.Z * sin + Position.X;
                var z = distance.X * sin + distance.Z * cos + Position.Z;

                i.Position =new Vector3(x, i.Position.Y, z);
            }
        }

        public ObjectGroup SetRoom(Room room)
        {
            Room = room;
            return this;
        }

        public string ShortName() => $"Group of {_objects.Count} objects";

        public IEnumerator<PositionBasedObjectInstance> GetEnumerator() => _objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _objects.GetEnumerator();
    }
}