using System;
using System.Collections.Generic;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public interface IScaleable
    {
        float Scale { get; set; }
    };

    public interface IRotateableY
    {
        float RotationY { get; set; }
    };

    public interface IRotateableYX : IRotateableY
    {
        float RotationX { get; set; }
    };

    public interface IRotateableYXRoll : IRotateableYX
    {
        float Roll { get; set; }
    };

    public abstract class ObjectInstance : ICloneable
    {
        public delegate void RemovedFromRoomDelegate(ObjectInstance instance);
        public event RemovedFromRoomDelegate DeletedEvent;
        public Room Room { get; private set; } = null;

        public virtual ObjectInstance Clone()
        {
            ObjectInstance result = (ObjectInstance)MemberwiseClone();
            result.Room = null;
            return result;
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        public virtual void AddToRoom(Level level, Room room)
        {
            if (room == null)
                throw new NullReferenceException("room was null");
            if (Room != null)
                throw new ArgumentException("An object can only be part of 1 room. The object '" + this + "' already belongs to room '" +
                    Room + "', it can not be added again to room '" + room + "'");
            Room = room;
        }

        public virtual void RemoveFromRoom(Level level, Room room)
        {
            if (room == null)
                throw new NullReferenceException("room was null");
            if (Room != room)
                throw new ArgumentException("An object can't be removed from a room it not part of. The object '" + this + "' belongs to room '" +
                    (Room == null ? "<none>" : Room.ToString()) + "', it can not be removed from room '" + room + "'");
            Room = null;
        }

        /// <summary>
        /// This methode should be invoked after the object was added to a room and the object will go out of scope.
        /// </summary>
        public virtual void Delete()
        {
            if (Room != null)
                throw new NullReferenceException("An object can't be deleted if it's still part of a room.");
            DeletedEvent?.Invoke(this);
        }

        public virtual void Transform(RectTransformation transformation, VectorInt2 oldRoomSize)
        {
            IRotateableY rotateableObject = this as IRotateableY;
            if (rotateableObject != null)
            {
                float newRotation = rotateableObject.RotationY;
                if (transformation.MirrorX)
                    newRotation = -newRotation;
                newRotation -= transformation.QuadrantRotation * 90;
                rotateableObject.RotationY = newRotation;
            }
        }

        public virtual void TransformRoomReferences(Func<Room, Room> transformRoom)
        {}

        public virtual bool CopyToFlipRooms => true;

        public virtual void CopyDependentLevelSettings(LevelSettings destinationLevelSettings, LevelSettings sourceLevelSettings, bool unifyData)
        { }
    }

    public abstract class SectorBasedObjectInstance : ObjectInstance
    {
        private RectangleInt2 _area;
        public RectangleInt2 Area
        {
            get { return _area; }
            set
            {
                if (Room != null)
                    throw new InvalidOperationException("Sector based objects may not change in size while they are assigned to a room.");
                _area = value;
            }
        }

        public SectorBasedObjectInstance(RectangleInt2 area)
        {
            Area = area;
        }

        public SectorBasedObjectInstance Clone(RectangleInt2 area)
        {
            var result = (SectorBasedObjectInstance)Clone();
            result.Area = area;
            return result;
        }

        public override void Transform(RectTransformation transformation, VectorInt2 oldRoomSize)
        {
            base.Transform(transformation, oldRoomSize);
            _area = transformation.TransformRect(_area, oldRoomSize);
        }
    }

    public abstract class PositionBasedObjectInstance : ObjectInstance
    {
        public Vector3 Position { get; set; }

        public Vector3 SectorPosition => Position / 1024;

        public void Move(int deltaX, int deltaY, int deltaZ)
        {
            Position = Position + new Vector3(deltaX, deltaY, deltaZ);
        }

        public Matrix4x4 RotationMatrix
        {
            get
            {
                return Matrix4x4.CreateFromYawPitchRoll(
                    (this as IRotateableY)?.GetRotationYRadians() ?? 0.0f,
                    -(this as IRotateableYX)?.GetRotationXRadians() ?? 0.0f,
                    (this as IRotateableYXRoll)?.GetRotationRollRadians() ?? 0.0f);
            }
        }

        public Matrix4x4 RotationPositionMatrix
        {
            get
            {
                return RotationMatrix *
                    Matrix4x4.CreateTranslation((Room?.WorldPos ?? new Vector3()) + Position);
            }
        }

        public Matrix4x4 ObjectMatrix
        {
            get
            {
                return RotationMatrix *
                    Matrix4x4.CreateScale((this as IScaleable)?.Scale ?? 1.0f) *
                    Matrix4x4.CreateTranslation((Room?.WorldPos ?? new Vector3()) + Position);
            }
        }

        public Matrix4x4 LocalObjectMatrix
        {
            get
            {
                return RotationMatrix *
                    Matrix4x4.CreateScale((this as IScaleable)?.Scale ?? 1.0f) *
                    Matrix4x4.CreateTranslation(Position);
            }
        }

        public override void Transform(RectTransformation transformation, VectorInt2 oldRoomSize)
        {
            base.Transform(transformation, oldRoomSize);
            Position = transformation.TransformVec3(Position, oldRoomSize.X * 1024.0f, oldRoomSize.Y * 1024.0f);
        }
    }

    public abstract class PositionAndScriptBasedObjectInstance : PositionBasedObjectInstance, IHasScriptID
    {
        private ScriptIdTable<IHasScriptID> _scriptTable = null;
        private uint? _scriptId = null;
        public uint? ScriptId
        {
            get
            {
                return _scriptId;
            }
            set
            {
                _scriptTable?.Update(this, _scriptId, value);
                _scriptId = value;
            }
        }

        public void AllocateNewScriptId()
        {
            _scriptId = _scriptTable?.UpdateWithNewId(this, _scriptId);
        }

        public bool TrySetScriptId(uint? newScriptId)
        {
            if (_scriptTable != null && !_scriptTable.TryUpdate(this, _scriptId, newScriptId))
                return false;
            _scriptId = newScriptId;
            return true;
        }

        public override ObjectInstance Clone()
        {
            ObjectInstance result = base.Clone();
            ((PositionAndScriptBasedObjectInstance)result)._scriptId = null;
            return result;
        }

        public override void AddToRoom(Level level, Room room)
        {
            base.AddToRoom(level, room);
            try
            {
                level.GlobalScriptingIdsTable.Update(this, null, ScriptId);
            }
            catch (Exception)
            {
                base.RemoveFromRoom(level, room);
                throw;
            }
            _scriptTable = level.GlobalScriptingIdsTable;
        }

        public override void RemoveFromRoom(Level level, Room room)
        {
            base.RemoveFromRoom(level, room);
            try
            {
                level.GlobalScriptingIdsTable.Update(this, ScriptId, null);
            }
            catch (Exception)
            {
                base.AddToRoom(level, room);
                throw;
            }
            _scriptTable = null;
        }
    }

    public static class RotatableExtensions
    {
        public static float GetRotationYRadians(this IRotateableY obj)
        {
            return obj.RotationY * (float)(Math.PI / 180.0);
        }
        public static void SetRotationYRadians(this IRotateableY obj, float value)
        {
            obj.RotationY = value * (float)(180.0 / Math.PI);
        }

        public static float GetRotationXRadians(this IRotateableYX obj)
        {
            return obj.RotationX * (float)(Math.PI / 180.0);
        }
        public static void SetRotationXRadians(this IRotateableYX obj, float value)
        {
            obj.RotationX = value * (float)(180.0 / Math.PI);
        }

        public static float GetRotationRollRadians(this IRotateableYXRoll obj)
        {
            return obj.Roll * (float)(Math.PI / 180.0);
        }
        public static void SetRotationRollRadians(this IRotateableYXRoll obj, float value)
        {
            obj.Roll = value * (float)(180.0 / Math.PI);
        }

        public static Vector3 GetDirection(this IRotateableYX obj)
        {
            float RadiansX = GetRotationXRadians(obj);
            float RadiansY = GetRotationYRadians(obj);
            return new Vector3(
                (float)(Math.Cos(RadiansX) * Math.Sin(RadiansY)),
                (float)Math.Sin(RadiansX),
                (float)(Math.Cos(RadiansX) * Math.Cos(RadiansY)));
        }

        public static void SetArbitaryRotationsYX(this IRotateableYX obj, float rotationY, float rotationX)
        {
            rotationX -= (float)(360 * Math.Floor(rotationX / 360));
            if (rotationX > 270)
                rotationX -= 360;
            else if (rotationX > 90)
            {
                rotationX = 180 - rotationX;
                rotationY += 180;
            }

            obj.RotationX = rotationX;
            obj.RotationY = rotationY;
        }
    }
}
