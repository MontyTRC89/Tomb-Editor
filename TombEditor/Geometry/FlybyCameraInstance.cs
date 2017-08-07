using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TombEditor.Geometry
{
    public class FlybyCameraInstance : PositionBasedObjectInstance
    {
        public byte Sequence { get; set; }
        public byte Number { get; set; }
        public ushort Timer { get; set; }
        public ushort Flags { get; set; }

        private float _speed { get; set; } = 1;
        private float _fov { get; set; } = 45;
        private float _roll { get; set; } = 0;
        private float _rotationX { get; set; } = 0;
        private float _rotationY { get; set; } = 0;

        /// <summary> Spped in the range [0, Infinity). </summary>
        public float Speed
        {
            get { return _speed; }
            set { _speed = Math.Max(0, value); }
        }

        /// <summary> Degrees in the range [0, 90) </summary>
        public float Fov
        {
            get { return _fov; }
            set { _fov = Math.Max(0, Math.Min(90, value)); }
        }

        /// <summary> Degrees in the range [0, Pi/2) </summary>
        public float FovRadians
        {
            get { return _fov * (float)(Math.PI / 180.0); }
            set { _fov = value * (float)(180.0 / Math.PI); }
        }

        /// <summary> Degrees in the range [0, 360) </summary>
        public float Roll
        {
            get { return _roll; }
            set { _roll = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }

        /// <summary> Radians in the range [0, 2*Pi) </summary>
        public float RollRadians
        {
            get { return Roll * (float)(Math.PI / 180.0); }
            set { Roll = value * (float)(180.0 / Math.PI); }
        }

        /// <summary> Degrees in the range [0, 360) </summary>
        public float RotationX
        {
            get { return _rotationX; }
            set { _rotationX = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }

        /// <summary> Radians in the range [0, 2*Pi) </summary>
        public float RotationXRadians
        {
            get { return RotationX * (float)(Math.PI / 180.0); }
            set { RotationX = value * (float)(180.0 / Math.PI); }
        }

        /// <summary> Degrees in the range [-90, 90] </summary>
        public float RotationY
        {
            get { return _rotationY; }
            set { _rotationY = Math.Max(-90, Math.Min(90, value)); }
        }

        /// <summary> Radians in the range [-Pi/2, Pi/2] </summary>
        public float RotationYRadians
        {
            get { return RotationY * (float)(Math.PI / 180.0); }
            set { RotationY = value * (float)(180.0 / Math.PI); }
        }

        public Vector3 GetDirection()
        {
            return new Vector3(
                (float)(Math.Cos(RotationXRadians) * Math.Sin(RotationYRadians)),
                (float)Math.Sin(RotationXRadians),
                (float)(Math.Cos(RotationXRadians) * Math.Cos(RotationYRadians)));
        }

        public FlybyCameraInstance(int id, Room room)
            : base(id, room)
        { }
        
        public override ObjectInstanceType Type
        {
            get { return ObjectInstanceType.FlyByCamera; }
        }

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }
        
        public override string ToString()
        {
            return "FlyBy " +
                ", ID = " + Id +
                ", Room = " + Room.ToString() +
                ", X = " + Position.X +
                ", Y = " + Position.Y +
                ", Z = " + Position.Z;
        }
    }
}
