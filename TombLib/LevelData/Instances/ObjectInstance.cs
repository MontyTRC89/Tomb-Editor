﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TombLib.Utils;

namespace TombLib.LevelData
{
    public enum RotationAxis
    {
        Y,
        X,
        Roll,
        None
    }

    public interface ISpatial
    {
        VectorInt2 SectorPosition { get; set; }
    }

    public interface IScaleable
    {
        float DefaultScale { get; }
        float Scale { get; set; }
    }

    public interface ISizeable
    {
        Vector3 DefaultSize { get; }
        Vector3 Size { get; set; }
    }

    public interface IRotateableY
    {
        float RotationY { get; set; }
    }

    public interface IRotateableYX : IRotateableY
    {
        float RotationX { get; set; }
    }

    public interface IRotateableYXRoll : IRotateableYX
    {
        float Roll { get; set; }
    }

    public interface IReplaceable
    {
        string PrimaryAttribDesc { get; }
        string SecondaryAttribDesc { get; }

        bool ReplaceableEquals(IReplaceable other, bool withProperties = false);
        bool Replace(IReplaceable other, bool withProperties);
    }

    public abstract class ObjectInstance : ICloneable, ITriggerParameter
    {
        public delegate void RemovedFromRoomDelegate(ObjectInstance instance);
        public event RemovedFromRoomDelegate DeletedEvent;
        public Room Room { get; private set; }

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
        { }

        public virtual bool CopyToAlternateRooms => true;

        public virtual void CopyDependentLevelSettings(Room.CopyDependentLevelSettingsArgs args)
        { }

        bool IEquatable<ITriggerParameter>.Equals(ITriggerParameter other) => this == other;

        public virtual string ToShortString()
        {
            var shortName = GetType().GetMethod("ShortName");

            if (shortName != null)
                return shortName.Invoke(this, null).ToString();
            else
                return ToString();
        }
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

        public virtual RectangleInt2 GetValidArea(RectangleInt2 newLocalRoomArea)
        {
            return newLocalRoomArea.Inflate(-1); // Not on room walls
        }

        public override void Transform(RectTransformation transformation, VectorInt2 oldRoomSize)
        {
            base.Transform(transformation, oldRoomSize);
            _area = transformation.TransformRect(_area, oldRoomSize);
        }

        public static List<RectangleInt2> SplitIntoRectangles(RectangleInt2 area, Func<VectorInt2, bool> sectorToBeCovered)
        {
            // Check were the map must be covered
            bool[,] toBeCoveredMap = new bool[area.Width + 1, area.Height + 1];
            for (int x = 0; x <= area.Width; ++x)
                for (int z = 0; z <= area.Height; ++z)
                    toBeCoveredMap[x, z] = sectorToBeCovered(new VectorInt2(x, z) + area.Start);

            // Add rectanges greedily to cover space
            List<RectangleInt2> result = new List<RectangleInt2>();
            for (int currentX = 0; currentX <= area.Width; ++currentX)
                for (int currentZ = 0; currentZ <= area.Height; ++currentZ)
                    if (toBeCoveredMap[currentX, currentZ])
                    { // Needs to be covered with a rectangles.
                        // Create a rectangle and cover as much space as possible
                        int endX = currentX;
                        while (endX < area.Width)
                        {
                            if (!toBeCoveredMap[endX + 1, currentZ])
                                break;
                            ++endX;
                        }
                        int endZ = currentZ;
                        while (endZ < area.Height)
                        {
                            for (int x = currentX; x <= endX; ++x)
                                if (!toBeCoveredMap[x, endZ + 1])
                                    goto ExitEndZSearch;
                            ++endZ;
                        }
                        ExitEndZSearch:

                        // Update map and create actual rectangle
                        for (int x = currentX; x <= endX; ++x)
                            for (int z = currentZ; z <= endZ; ++z)
                                toBeCoveredMap[x, z] = false;
                        result.Add(new RectangleInt2(currentX, currentZ, endX, endZ) + area.Start);
                    }
            return result;
        }

        public IEnumerable<SectorBasedObjectInstance> SplitIntoRectangles(Func<VectorInt2, bool> sectorToBeCovered, VectorInt2 offset)
        {
            return SplitIntoRectangles(Area, sectorToBeCovered).Select(area => Clone(area + offset));
        }
    }

    public abstract class PositionBasedObjectInstance : ObjectInstance, ISpatial
    {
        public Vector3 Position { get; set; }

        public VectorInt2 SectorPosition
        {
            get { return new VectorInt2((int)(Position.X / 1024.0f), (int)(Position.Z / 1024.0f)); }
            set { Position = new Vector3(value.X * 1024.0f, Position.Y, value.Y * 1024.0f);} }

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
                    Matrix4x4.CreateScale(((this as ISizeable)?.Size ?? new Vector3(1.0f)) * ((this as IScaleable)?.Scale ?? 1.0f)) *
                    Matrix4x4.CreateTranslation((Room?.WorldPos ?? new Vector3()) + Position);
            }
        }

        public Matrix4x4 LocalObjectMatrix
        {
            get
            {
                return RotationMatrix *
                    Matrix4x4.CreateScale(((this as ISizeable)?.Size ?? new Vector3(1.0f)) * ((this as IScaleable)?.Scale ?? 1.0f)) *
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
        private ScriptIdTable<IHasScriptID> _scriptTable;
        private uint? _scriptId;
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
