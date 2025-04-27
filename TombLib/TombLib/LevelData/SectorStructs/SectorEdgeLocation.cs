using TombLib.LevelData.SectorEnums;

namespace TombLib.LevelData.SectorStructs;

/// <summary>
/// A sector edge with its sector coordinates.
/// </summary>
public record struct SectorEdgeLocation(int SectorX, int SectorZ, SectorEdge Edge);
