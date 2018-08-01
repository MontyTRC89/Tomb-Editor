using System;

namespace TombLib.LevelData
{
    public class FlybyCameraInstance : PositionAndScriptBasedObjectInstance, IRotateableYXRoll
    {
        public ushort Sequence { get; set; }
        public ushort Number { get; set; }
        public short Timer { get; set; }
        public ushort Flags { get; set; }

        private float _speed { get; set; } = 1;
        private float _fov { get; set; } = 45;
        private float _roll { get; set; }
        private float _rotationX { get; set; }
        private float _rotationY { get; set; }

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

        /// <summary> Degrees in the range [0, 360) </summary>
        public float Roll
        {
            get { return _roll; }
            set { _roll = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }

        /// <summary> Degrees in the range [-90, 90] </summary>
        public float RotationX
        {
            get { return _rotationX; }
            set { _rotationX = Math.Max(-90, Math.Min(90, value)); }
        }

        /// <summary> Degrees in the range [0, 360) </summary>
        public float RotationY
        {
            get { return _rotationY; }
            set { _rotationY = (float)(value - Math.Floor(value / 360.0) * 360.0); }
        }

        public override bool CopyToAlternateRooms => false;

        public override string ToString()
        {
            return "Flyby " +
                ", Sequence = " + Sequence +
                ", Number = " + Number +
                ", Room = " + (Room?.ToString() ?? "NULL") +
                ", X = " + SectorPosition.X +
                ", Y = " + SectorPosition.Y +
                ", Z = " + SectorPosition.Z;
        }

        public override void CopyDependentLevelSettings(Room.CopyDependentLevelSettingsArgs args)
        {
            base.CopyDependentLevelSettings(args);
            Sequence = args.ReassociateFlyBySequence(Sequence);
        }
    }
}
