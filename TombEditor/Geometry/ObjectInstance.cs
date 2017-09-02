using SharpDX;
using System;

namespace TombEditor.Geometry
{
    public abstract class ObjectInstance : ICloneable
    {
        public Room Room { get; private set; }

        public abstract ObjectInstance Clone();

        object ICloneable.Clone()
        {
            return Clone();
        }

        public virtual void AddToRoom(Level level, Room room)
        {
            Room = room;
        }

        public virtual void RemoveFromRoom(Level level, Room room)
        {
        }

        public virtual bool CopyToFlipRooms => true;
    }

    public abstract class SectorBasedObjectInstance : ObjectInstance
    {
        public Rectangle Area { get; }

        protected SectorBasedObjectInstance(Rectangle area)
        {
            Area = area;
        }

        public abstract SectorBasedObjectInstance Clone(Rectangle newArea);
    }

    public abstract class PositionBasedObjectInstance : ObjectInstance
    {
        public Vector3 Position { get; set; }

        public Vector3 SectorPosition => Position / 1024;

        public void Move(int deltaX, int deltaY, int deltaZ)
        {
            Position = Position + new Vector3(deltaX, deltaY, deltaZ);
        }
    }

    public interface IHasScriptId
    {
        ushort? ScriptId { get; set; }
    };

    public interface IRotateableY
    {
        float RotationY { get; set; }
    };

    public interface IRotateableYx : IRotateableY
    {
        float RotationX { get; set; }
    };

    public interface IRotateableYxRoll : IRotateableYx
    {
        float Roll { get; set; }
    };

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

        public static float GetRotationXRadians(this IRotateableYx obj)
        {
            return obj.RotationX * (float)(Math.PI / 180.0);
        }

        public static void SetRotationXRadians(this IRotateableYx obj, float value)
        {
            obj.RotationX = value * (float)(180.0 / Math.PI);
        }

        public static float GetRotationRollRadians(this IRotateableYxRoll obj)
        {
            return obj.Roll * (float)(Math.PI / 180.0);
        }

        public static void SetRotationRollRadians(this IRotateableYxRoll obj, float value)
        {
            obj.Roll = value * (float)(180.0 / Math.PI);
        }

        public static Vector3 GetDirection(this IRotateableYx obj)
        {
            float radiansX = GetRotationXRadians(obj);
            float radiansY = GetRotationYRadians(obj);
            return new Vector3(
                (float)(Math.Cos(radiansX) * Math.Sin(radiansY)),
                (float)Math.Sin(radiansX),
                (float)(Math.Cos(radiansX) * Math.Cos(radiansY)));
        }

        public static void SetArbitaryRotationsYx(this IRotateableYx obj, float rotationY, float rotationX)
        {
            rotationX -= (float)(360 * Math.Floor(rotationX / 360));
            if (rotationX > 270)
                rotationX = rotationX - 360;
            else if (rotationX > 180)
            {
                rotationX = rotationX - 360;
                rotationY += 180;
            }

            obj.RotationX = rotationX;
            obj.RotationY = rotationY;
        }
    }
}
