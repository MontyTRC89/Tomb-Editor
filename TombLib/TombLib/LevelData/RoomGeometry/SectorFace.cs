using System.Numerics;

namespace TombLib.LevelData;

public readonly struct SectorFace
{
	public readonly BlockFace BlockFace;

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
	/// Creates a triangle face data.
	/// </summary>
	public SectorFace(BlockFace blockFace, Vector3 p0, Vector3 p1, Vector3 p2, Vector2 uv0, Vector2 uv1, Vector2 uv2, bool isXEqualYDiagonal)
	{
		BlockFace = blockFace;

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
	/// Creates a quad face data.
	/// </summary>
	public SectorFace(BlockFace blockFace, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
	{
		BlockFace = blockFace;

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
}
