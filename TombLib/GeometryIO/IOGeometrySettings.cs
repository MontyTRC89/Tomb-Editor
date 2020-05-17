namespace TombLib.GeometryIO
{
    public struct IOGeometrySettingsPreset
    {
        public string Name;
        public IOGeometrySettings Settings;
    }

    public class IOGeometrySettings
    {
        public bool Export { get; set; } = false;

        public bool ProcessGeometry { get; set; } = true;
        public bool ProcessAnimations { get; set; } = false;

        public bool SwapAnimTranslationXY { get; set; } = false;
        public bool SwapAnimTranslationXZ { get; set; } = false;
        public bool SwapAnimTranslationYZ { get; set; } = false;

        public bool SwapXY { get; set; } = false;
        public bool SwapXZ { get; set; } = false;
        public bool SwapYZ { get; set; } = false;
        public bool FlipX { get; set; } = false;
        public bool FlipY { get; set; } = false;
        public bool FlipZ { get; set; } = true;
        public bool FlipUV_V { get; set; } = true;
        public float Scale { get; set; } = 1.0f;
        public bool WrapUV { get; set; } = true;
        public bool PremultiplyUV { get; set; } = true;
        public bool InvertFaces { get; set; } = false;
        public bool UseVertexColor { get; set; } = true;
        public bool SortByName { get; set; } = true;
    }
}
