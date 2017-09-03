using System;

namespace TombEditor.Geometry
{
    public class FlybyCameraInstance : PositionBasedObjectInstance, IRotateableYxRoll, IHasScriptId
    {
        public ushort? ScriptId { get; set; }
        public byte Sequence { get; set; }
        public byte Number { get; set; }
        public ushort Timer { get; set; }
        public ushort Flags { get; set; }

        private float _speed = 1;
        private float _fov = 45;
        private float _roll;
        private float _rotationX;
        private float _rotationY;

        /// <summary> Spped in the range [0, Infinity). </summary>
        public float Speed
        {
            get => _speed;
            set => _speed = Math.Max(0, value);
        }

        /// <summary> Degrees in the range [0, 90) </summary>
        public float Fov
        {
            get => _fov;
            set => _fov = Math.Max(0, Math.Min(90, value));
        }

        /// <summary> Degrees in the range [0, 360) </summary>
        public float Roll
        {
            get => _roll;
            set => _roll = (float)(value - Math.Floor(value / 360.0) * 360.0);
        }

        /// <summary> Degrees in the range [-90, 90] </summary>
        public float RotationX
        {
            get => _rotationX;
            set => _rotationX = Math.Max(-90, Math.Min(90, value));
        }

        /// <summary> Degrees in the range [0, 360) </summary>
        public float RotationY
        {
            get => _rotationY;
            set => _rotationY = (float)(value - Math.Floor(value / 360.0) * 360.0);
        }

        public override bool CopyToFlipRooms => false;

        public override ObjectInstance Clone()
        {
            return (ObjectInstance)MemberwiseClone();
        }

        public override string ToString()
        {
            return
                $"FlyBy , Sequence = {Sequence}, Number = {Number}, Room = {Room?.ToString() ?? "NULL"}, X = {SectorPosition.X}, Y = {SectorPosition.Y}, Z = {SectorPosition.Z}";
        }
    }
}
