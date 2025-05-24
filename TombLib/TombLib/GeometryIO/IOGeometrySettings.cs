﻿namespace TombLib.GeometryIO
{
    public record IOGeometrySettingsPreset(string Name, IOGeometrySettings Settings)
    {
        public override string ToString() => Name;
	}

    public record IOGeometrySettings
    {
        // Internal settings
        public bool Export { get; set; } = false;
        public bool ExportRoom { get; set; } = false;
        public bool ProcessGeometry { get; set; } = true;
        public bool ProcessUntexturedGeometry { get; set; } = false;
        public bool ProcessAnimations { get; set; } = false;

        public bool SwapXY { get; set; } = false;
        public bool SwapXZ { get; set; } = false;
        public bool SwapYZ { get; set; } = false;
        public bool FlipX { get; set; } = false;
        public bool FlipY { get; set; } = false;
        public bool FlipZ { get; set; } = true;
        public bool FlipUV_V { get; set; } = true;
        public float Scale { get; set; } = 1.0f;
        public bool MappedUV { get; set; } = true;
        public bool WrapUV { get; set; } = true;
        public bool PremultiplyUV { get; set; } = true;
        public bool InvertFaces { get; set; } = false;
        public bool UseVertexColor { get; set; } = true;
        public bool SortByName { get; set; } = true;
        public bool PackTextures { get; set; } = true;
        public bool PadPackedTextures { get; set; } = true;
    }
}
