using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.SectorStructs;

public struct FaceLayerInfo
{
	public SectorFace Face { get; set; }
	public FaceLayer Layer { get; set; }

	public FaceLayerInfo(SectorFace face, FaceLayer layer)
	{
		Face = face;
		Layer = layer;
	}

	public override int GetHashCode() => Face.GetHashCode() ^ Layer.GetHashCode();
}
