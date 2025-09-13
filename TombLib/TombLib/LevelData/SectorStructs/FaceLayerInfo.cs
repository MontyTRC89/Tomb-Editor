using System;
using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.SectorStructs;

public readonly struct FaceLayerInfo : IEquatable<FaceLayerInfo>
{
	public readonly SectorFace Face { get; }
	public readonly FaceLayer Layer { get; }

	public FaceLayerInfo(SectorFace face, FaceLayer layer)
	{
		Face = face;
		Layer = layer;
	}

	public override int GetHashCode() => HashCode.Combine(Face, Layer);

	public bool Equals(FaceLayerInfo other) => Face == other.Face && Layer == other.Layer;
	public override bool Equals(object obj) => obj is FaceLayerInfo other && Equals(other);

	public static bool operator ==(FaceLayerInfo first, FaceLayerInfo second) => first.Equals(second);
	public static bool operator !=(FaceLayerInfo first, FaceLayerInfo second) => !first.Equals(second);
}
