using System.Numerics;
using TombLib.LevelData.SectorStructs;

namespace TombLib.LevelData.SectorGeometry;

/// <summary>
/// Represents a face of a sector, either a triangle or a quad (2 triangles).
/// </summary>
public readonly struct SectorFaceData
{
	/// <summary>
	/// The exact hard-coded face type.
	/// </summary>
	public readonly FaceLayerInfo Face;

	public readonly Vector3 P0;
	public readonly Vector3 P1;
	public readonly Vector3 P2;
	public readonly Vector3? P3;

	public readonly Vector2 UV0;
	public readonly Vector2 UV1;
	public readonly Vector2 UV2;
	public readonly Vector2? UV3;

	public readonly bool? IsXEqualYDiagonal;

	public readonly bool IsQuad;
	public readonly bool IsTriangle;

	/// <summary>
	/// Constructor for a triangle face.
	/// </summary>
	public SectorFaceData(FaceLayerInfo face, Vector3 p0, Vector3 p1, Vector3 p2, Vector2 uv0, Vector2 uv1, Vector2 uv2, bool isXEqualYDiagonal)
	{
		Face = face;

		P0 = p0;
		P1 = p1;
		P2 = p2;
		P3 = null;

		UV0 = uv0;
		UV1 = uv1;
		UV2 = uv2;
		UV3 = null;

		IsXEqualYDiagonal = isXEqualYDiagonal;

		IsQuad = false;
		IsTriangle = true;
	}

	/// <summary>
	/// Constructor for a quad face.
	/// </summary>
	public SectorFaceData(FaceLayerInfo face, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
	{
		Face = face;

		P0 = p0;
		P1 = p1;
		P2 = p2;
		P3 = p3;

		UV0 = uv0;
		UV1 = uv1;
		UV2 = uv2;
		UV3 = uv3;

		IsXEqualYDiagonal = null;

		IsQuad = true;
		IsTriangle = false;
	}

	public static SectorFaceData? CreateVerticalFloorFaceData(FaceLayerInfo sectorFace, (int X, int Z) wallStartPoint, (int X, int Z) wallEndPoint, WallSplitData faceStartSplit, WallSplitData faceEndSplit)
	{
		if (faceStartSplit.StartY > faceEndSplit.StartY && faceStartSplit.EndY > faceEndSplit.EndY) // Is quad
		{
			return new SectorFaceData(sectorFace,
				p0: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceStartSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				p1: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceStartSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				p2: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceEndSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				p3: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceEndSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				uv0: new Vector2(0, 0), uv1: new Vector2(1, 0), uv2: new Vector2(1, 1), uv3: new Vector2(0, 1));
		}
		else if (faceStartSplit.StartY == faceEndSplit.StartY && faceStartSplit.EndY > faceEndSplit.EndY) // Is triangle (type 1)
		{
			return new SectorFaceData(sectorFace,
				p0: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceEndSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				p1: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceStartSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				p2: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceEndSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				uv0: new Vector2(1, 1), uv1: new Vector2(0, 0), uv2: new Vector2(1, 0), isXEqualYDiagonal: false);
		}
		else if (faceStartSplit.StartY > faceEndSplit.StartY && faceStartSplit.EndY == faceEndSplit.EndY) // Is triangle (type 2)
		{
			return new SectorFaceData(sectorFace,
				p0: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceStartSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				p1: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceEndSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				p2: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceEndSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				uv0: new Vector2(0, 1), uv1: new Vector2(0, 0), uv2: new Vector2(1, 0), isXEqualYDiagonal: true);
		}
		else
		{
			return null; // Can't render - failed to meet any of the conditions
		}
	}

	public static SectorFaceData? CreateVerticalCeilingFaceData(FaceLayerInfo sectorFace, (int X, int Z) wallStartPoint, (int X, int Z) wallEndPoint, WallSplitData faceStartSplit, WallSplitData faceEndSplit)
	{
		if (faceStartSplit.StartY < faceEndSplit.StartY && faceStartSplit.EndY < faceEndSplit.EndY) // Is quad
		{
			return new SectorFaceData(sectorFace,
				p0: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceEndSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				p1: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceEndSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				p2: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceStartSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				p3: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceStartSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				uv0: new Vector2(0, 0), uv1: new Vector2(1, 0), uv2: new Vector2(1, 1), uv3: new Vector2(0, 1));
		}
		else if (faceStartSplit.StartY < faceEndSplit.StartY && faceStartSplit.EndY == faceEndSplit.EndY) // Is triangle (type 1)
		{
			return new SectorFaceData(sectorFace,
				p0: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceEndSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				p1: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceEndSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				p2: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceStartSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				uv0: new Vector2(0, 1), uv1: new Vector2(0, 0), uv2: new Vector2(1, 0), isXEqualYDiagonal: true);
		}
		else if (faceStartSplit.StartY == faceEndSplit.StartY && faceStartSplit.EndY < faceEndSplit.EndY) // Is triangle (type 2)
		{
			return new SectorFaceData(sectorFace,
				p0: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceEndSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				p1: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceEndSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				p2: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceStartSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				uv0: new Vector2(1, 1), uv1: new Vector2(0, 0), uv2: new Vector2(1, 0), isXEqualYDiagonal: false);
		}
		else
		{
			return null; // Can't render - failed to meet any of the conditions
		}
	}

	public static SectorFaceData? CreateVerticalMiddleFaceData(FaceLayerInfo sectorFace, (int X, int Z) wallStartPoint, (int X, int Z) wallEndPoint, WallSplitData faceStartSplit, WallSplitData faceEndSplit)
	{
		if (faceStartSplit.StartY != faceEndSplit.StartY && faceStartSplit.EndY != faceEndSplit.EndY) // Is quad
		{
			return new SectorFaceData(sectorFace,
				p0: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceEndSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				p1: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceEndSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				p2: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceStartSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				p3: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceStartSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				uv0: new Vector2(0, 0), uv1: new Vector2(1, 0), uv2: new Vector2(1, 1), uv3: new Vector2(0, 1));
		}
		else if (faceStartSplit.StartY != faceEndSplit.StartY && faceStartSplit.EndY == faceEndSplit.EndY) // Is triangle (type 1)
		{
			return new SectorFaceData(sectorFace,
				p0: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceEndSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				p1: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceEndSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				p2: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceStartSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				uv0: new Vector2(0, 1), uv1: new Vector2(0, 0), uv2: new Vector2(1, 0), isXEqualYDiagonal: true);
		}
		else if (faceStartSplit.StartY == faceEndSplit.StartY && faceStartSplit.EndY != faceEndSplit.EndY) // Is triangle (type 2)
		{
			return new SectorFaceData(sectorFace,
				p0: new Vector3(wallStartPoint.X * Level.SectorSizeUnit, faceEndSplit.StartY, wallStartPoint.Z * Level.SectorSizeUnit),
				p1: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceEndSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				p2: new Vector3(wallEndPoint.X * Level.SectorSizeUnit, faceStartSplit.EndY, wallEndPoint.Z * Level.SectorSizeUnit),
				uv0: new Vector2(1, 1), uv1: new Vector2(0, 0), uv2: new Vector2(1, 0), isXEqualYDiagonal: false);
		}
		else
		{
			return null; // Can't render - failed to meet any of the conditions
		}
	}
}
