using System;

namespace TombLib.LevelData.SectorStructs;

/// <summary>
/// A unique, one of a kind identifier for a sector face, which includes the position of the sector that the face belongs to.
/// </summary>
public readonly struct SectorFaceIdentity : IEquatable<SectorFaceIdentity>, IComparable, IComparable<SectorFaceIdentity>
{
	public VectorInt2 Position { get; }
	public FaceLayerInfo Face { get; }

	public SectorFaceIdentity(int x, int z, FaceLayerInfo face)
	{
		Position = new VectorInt2(x, z);
		Face = face;
	}

	public override int GetHashCode() => HashCode.Combine(Position, Face.Face, Face.Layer);

	public bool Equals(SectorFaceIdentity other) => Position == other.Position && Face == other.Face;
	public override bool Equals(object other) => other is SectorFaceIdentity identity && Equals(identity);

	int IComparable.CompareTo(object other) => CompareTo((SectorFaceIdentity)other);
	public int CompareTo(SectorFaceIdentity other)
	{
		if (Position.X != other.Position.X)
			return Position.X > other.Position.X ? 1 : -1;

		if (Position.Y != other.Position.Y)
			return Position.Y > other.Position.Y ? 1 : -1;

		if (Face.Face != other.Face.Face)
			return Face.Face > other.Face.Face ? 1 : -1;

		if (Face.Layer != other.Face.Layer)
			return Face.Layer > other.Face.Layer ? 1 : -1;

		return 0;
	}

	public static bool operator ==(SectorFaceIdentity left, SectorFaceIdentity right) => left.Equals(right);
	public static bool operator !=(SectorFaceIdentity left, SectorFaceIdentity right) => !left.Equals(right);
	public static bool operator <(SectorFaceIdentity left, SectorFaceIdentity right) => left.CompareTo(right) < 0;
	public static bool operator <=(SectorFaceIdentity left, SectorFaceIdentity right) => left.CompareTo(right) <= 0;
	public static bool operator >(SectorFaceIdentity left, SectorFaceIdentity right) => left.CompareTo(right) > 0;
	public static bool operator >=(SectorFaceIdentity left, SectorFaceIdentity right) => left.CompareTo(right) >= 0;
}
