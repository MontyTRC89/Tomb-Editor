using System.Collections.Generic;

namespace TombLib.LevelData;

public interface IRoom
{
	/// <summary>
	/// The level which owns the room.
	/// </summary>
	Level Level { get; }

	/// <summary>
	/// Title of the room, which is displayed in the level editor.
	/// </summary>
	string Name { get; set; }

	/// <summary>
	/// A set of properties which can be shared between rooms.
	/// </summary>
	RoomProperties Properties { get; set; }

	/// <summary>
	/// The position of the room in the level, where X and Z is measured in sectors, and Y is measured in world units.
	/// </summary>
	VectorInt3 Position { get; set; }

	/// <summary>
	/// The sectors which form the room's geometry.
	/// </summary>
	Block[,] Blocks { get; }

	/// <summary>
	/// The base room of the flipped room.
	/// </summary>
	Room AlternateBaseRoom { get; set; }

	/// <summary>
	/// The flipped room.
	/// </summary>
	Room AlternateRoom { get; set; }

	/// <summary>
	/// The assigned index of the current flip map group.
	/// </summary>
	short AlternateGroup { get; set; }

	/// <summary>
	/// All the information about the room's finalized geometry.
	/// </summary>
	RoomGeometry RoomGeometry { get; set; }

	/// <summary>
	/// Gets a list which contains the room and all the rooms which are connected to it.
	/// </summary>
	List<Room> AndAdjoiningRooms { get; }

	/// <summary>
	/// Determines whether the room is present in the level.
	/// </summary>
	bool ExistsInLevel { get; }

	/// <summary>
	/// Determines whether the room has an alternate version (flip map).
	/// </summary>
	bool Alternated { get; }

	/// <summary>
	/// Determines whether the room is an alternate version (flip map).
	/// </summary>
	bool IsAlternate { get; }

	/// <summary>
	/// The opposite version of the room (flipped / non-flipped, depending on the current room).
	/// </summary>
	Room AlternateOpposite { get; }

	/// <summary>
	/// The size of the room in sectors.
	/// </summary>
	VectorInt2 SectorSize { get; }

	/// <summary>
	/// The area of the room in sectors.
	/// </summary>
	RectangleInt2 WorldArea { get; }

	/// <summary>
	/// The positions of the room in world units.
	/// </summary>
	VectorInt3 WorldPos { get; set; }

	/// <summary>
	/// Gets all versions of the room (e.g. the room and its alternate version).
	/// </summary>
	IEnumerable<Room> Versions { get; }

	/// <summary>
	/// The position of the room in sectors.
	/// </summary>
	VectorInt2 SectorPos { get; set; }

	/// <summary>
	/// The area of the room, local to the room (starting from 0, 0).
	/// </summary>
	RectangleInt2 LocalArea { get; }

	/// <summary>
	/// The portals which are present in the room.
	/// </summary>
	IEnumerable<PortalInstance> Portals { get; }

	/// <summary>
	/// The objects which are present in the room.
	/// </summary>
	IEnumerable<ObjectInstance> AnyObjects { get; }

	/// <summary>
	/// The position-based objects which are present in the room (ones which are moved via Gizmo).
	/// </summary>
	IReadOnlyList<PositionBasedObjectInstance> Objects { get; }

	/// <summary>
	/// The triggers which are present in the room.
	/// </summary>
	IEnumerable<TriggerInstance> Triggers { get; }

	/// <summary>
	/// The ghost blocks which are present in the room.
	/// </summary>
	IEnumerable<GhostBlockInstance> GhostBlocks { get; }

	/// <summary>
	/// The objects which are present in the room and are attached to sectors (e.g. Portals, Triggers, Ghost Blocks).
	/// </summary>
	IEnumerable<SectorBasedObjectInstance> SectorObjects { get; }

	/// <summary>
	/// The X width of the room in sectors.
	/// </summary>
	int NumXSectors { get; }

	/// <summary>
	/// The Z width of the room in sectors.
	/// </summary>
	int NumZSectors { get; }
}
