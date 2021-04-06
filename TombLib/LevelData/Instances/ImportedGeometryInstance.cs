using System;
using System.Numerics;

namespace TombLib.LevelData
{
    public enum ImportedGeometryLightingModel
    {
        NoLighting,
        VertexColors,
        CalculateFromLightsInRoom,
        TintAsAmbient
    }

    public class ImportedGeometryInstance : PositionBasedObjectInstance, IReplaceable, IColorable, IScaleable, IRotateableYXRoll
    {
        public ImportedGeometry Model { get; set; }
        public float DefaultScale => 1.0f;
        public float Scale { get; set; } = 1.0f;
        public ImportedGeometryLightingModel LightingModel { get; set; } = ImportedGeometryLightingModel.CalculateFromLightsInRoom;
        public Vector3 Color { get; set; } = Vector3.One;
        public bool SharpEdges { get; set; } = false;
        public bool Hidden { get; set; } = false;


        private float _roll { get; set; }
        private float _rotationX { get; set; }
        private float _rotationY { get; set; }

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

        public override string ToString()
        {
            string result = "Imported Geometry: ";
            if (Model == null)
                result += "None";
            else
            {
                result += Model.Info.Name;
                if (Model.DirectXModel == null)
                    result += "\n(Unloaded: " + (Model.LoadException?.Message ?? "") + ")";
            }
            return result;
        }

        public override void CopyDependentLevelSettings(Room.CopyDependentLevelSettingsArgs args)
        {
            base.CopyDependentLevelSettings(args);
            if (args.UnifyData)
            {
                foreach (ImportedGeometry importedGeometry in args.DestinationLevelSettings.ImportedGeometries)
                    if (importedGeometry.Info.Equals(Model.Info))
                    {
                        Model = importedGeometry;
                        return;
                    }

                // Add imported geometry
                args.DestinationLevelSettings.ImportedGeometries.Add(Model);
            }
            else
            {
                if (args.DestinationLevelSettings.ImportedGeometries.Contains(Model))
                    return;
                args.DestinationLevelSettings.ImportedGeometries.Add(Model);
            }
        }

        public string PrimaryAttribDesc => "Model";
        public string SecondaryAttribDesc => "Scale";

        public bool ReplaceableEquals(IReplaceable other, bool withProperties = false)
        {
            var otherInstance = other as ImportedGeometryInstance;
            return (otherInstance?.Model == Model && (withProperties ? otherInstance?.Scale == Scale : true));
        }

        public bool Replace(IReplaceable other, bool withProperties)
        {
            var result = false;

            if (!ReplaceableEquals(other))
            {
                var thatObj = (ImportedGeometryInstance)other;

                if (Model != thatObj.Model)
                {
                    Model = thatObj.Model;
                    result = true;
                }
                if (withProperties && Scale != thatObj.Scale)
                {
                    Scale = thatObj.Scale;
                    result = true;
                }
            }

            return result;
        }
    }
}
