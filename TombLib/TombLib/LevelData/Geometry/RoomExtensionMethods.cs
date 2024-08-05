using System;
using System.Collections.Generic;

namespace TombLib.LevelData.Geometry;

public static class RoomExtensionMethods
{
	public static SectorWall GetPositiveZWallData(this Room room, int x, int z, bool normalize)
	{
		Block block = room.Blocks[x, z];
		Block neighborBlock = room.Blocks[x, z + 1];

		int startMinY = neighborBlock.Floor.XpZn,
			startMaxY = neighborBlock.Ceiling.XpZn,

			endMinY = neighborBlock.Floor.XnZn,
			endMaxY = neighborBlock.Ceiling.XnZn,

			qaStartY = block.Floor.XpZp,
			qaEndY = block.Floor.XnZp,

			wsStartY = block.Ceiling.XpZp,
			wsEndY = block.Ceiling.XnZp;

		List<WallSplit>
			extraFloorSubdivisions = new(),
			extraCeilingSubdivisions = new();

		for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
		{
			extraFloorSubdivisions.Add(new WallSplit(
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp)));
		}

		for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
		{
			extraCeilingSubdivisions.Add(new WallSplit(
				block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp),
				block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp)));
		}

		if (block.WallPortal is not null)
		{
			// Get the adjoining room of the portal
			Room adjoiningRoom = block.WallPortal.AdjoiningRoom;

			if (room.Alternated &&
				room.AlternateBaseRoom is not null &&
				adjoiningRoom.Alternated &&
				adjoiningRoom.AlternateRoom is not null)
			{
				adjoiningRoom = adjoiningRoom.AlternateRoom;
			}

			// Get the near block in current room
			Block nearBlock = room.Blocks[x, 1];

			int qaNearStart = nearBlock.Floor.XpZn;
			int qaNearEnd = nearBlock.Floor.XnZn;

			if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
				qaNearStart = qaNearEnd;
			else if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
				qaNearEnd = qaNearStart;

			int wsNearStart = nearBlock.Ceiling.XpZn;
			int wsNearEnd = nearBlock.Ceiling.XnZn;

			if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
				wsNearStart = wsNearEnd;
			else if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
				wsNearEnd = wsNearStart;

			// Now get the facing block on the adjoining room and calculate the correct heights
			int facingX = x + (room.Position.X - adjoiningRoom.Position.X);

			Block adjoiningBlock = adjoiningRoom.GetBlockTry(facingX, adjoiningRoom.NumZSectors - 2) ?? Block.Empty;

			int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XpZp;
			int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XnZp;

			if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
				qAportal = qBportal;
			else if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
				qBportal = qAportal;

			qaStartY = room.Position.Y + qaNearStart;
			qaEndY = room.Position.Y + qaNearEnd;
			qaStartY = Math.Max(qaStartY, qAportal) - room.Position.Y;
			qaEndY = Math.Max(qaEndY, qBportal) - room.Position.Y;

			int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZp;
			int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZp;

			if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
				wAportal = wBportal;
			else if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
				wBportal = wAportal;

			wsStartY = room.Position.Y + wsNearStart;
			wsEndY = room.Position.Y + wsNearEnd;
			wsStartY = Math.Min(wsStartY, wAportal) - room.Position.Y;
			wsEndY = Math.Min(wsEndY, wBportal) - room.Position.Y;

			WallSplit newSubdivision;

			for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp));

				if (i >= extraFloorSubdivisions.Count)
					extraFloorSubdivisions.Add(newSubdivision);
				else
					extraFloorSubdivisions[i] = newSubdivision;
			}

			for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp));

				if (i >= extraCeilingSubdivisions.Count)
					extraCeilingSubdivisions.Add(newSubdivision);
				else
					extraCeilingSubdivisions[i] = newSubdivision;
			}
		}

		if (block.Floor.DiagonalSplit is DiagonalSplit.XpZn)
		{
			qaStartY = block.Floor.XnZp;
			qaEndY = block.Floor.XnZp;
		}
		else if (block.Floor.DiagonalSplit is DiagonalSplit.XnZn)
		{
			qaStartY = block.Floor.XpZp;
			qaEndY = block.Floor.XpZp;
		}

		if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
		{
			startMinY = neighborBlock.Floor.XpZn;
			endMinY = neighborBlock.Floor.XpZn;
		}
		else if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
		{
			startMinY = neighborBlock.Floor.XnZn;
			endMinY = neighborBlock.Floor.XnZn;
		}

		if (block.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
		{
			wsStartY = block.Ceiling.XnZp;
			wsEndY = block.Ceiling.XnZp;
		}
		else if (block.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
		{
			wsStartY = block.Ceiling.XpZp;
			wsEndY = block.Ceiling.XpZp;
		}

		if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
		{
			startMaxY = neighborBlock.Ceiling.XpZn;
			endMaxY = neighborBlock.Ceiling.XpZn;
		}
		else if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
		{
			startMaxY = neighborBlock.Ceiling.XnZn;
			endMaxY = neighborBlock.Ceiling.XnZn;
		}

		var wall = new SectorWall
		(
			direction: Direction.PositiveZ,

			start: new WallEnd
			(
				x: x + 1,
				z: z + 1,
				minY: startMinY,
				maxY: startMaxY
			),

			end: new WallEnd
			(
				x: x,
				z: z + 1,
				minY: endMinY,
				maxY: endMaxY
			),

			qa: new WallSplit
			(
				startY: qaStartY,
				endY: qaEndY
			),

			ws: new WallSplit
			(
				startY: wsStartY,
				endY: wsEndY
			),

			extraFloorSubdivisions: extraFloorSubdivisions,
			extraCeilingSubdivisions: extraCeilingSubdivisions
		);

		return normalize
			? wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall)
			: wall;
	}

	public static SectorWall GetNegativeZWallData(this Room room, int x, int z, bool normalize)
	{
		Block block = room.Blocks[x, z];
		Block neighborBlock = room.Blocks[x, z - 1];

		int startMinY = neighborBlock.Floor.XnZp,
			startMaxY = neighborBlock.Ceiling.XnZp,

			endMinY = neighborBlock.Floor.XpZp,
			endMaxY = neighborBlock.Ceiling.XpZp,

			qaStartY = block.Floor.XnZn,
			qaEndY = block.Floor.XpZn,

			wsStartY = block.Ceiling.XnZn,
			wsEndY = block.Ceiling.XpZn;

		List<WallSplit>
			extraFloorSubdivisions = new(),
			extraCeilingSubdivisions = new();

		for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
		{
			extraFloorSubdivisions.Add(new WallSplit(
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn)));
		}

		for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
		{
			extraCeilingSubdivisions.Add(new WallSplit(
				block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn),
				block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn)));
		}

		if (block.WallPortal is not null)
		{
			// Get the adjoining room of the portal
			Room adjoiningRoom = block.WallPortal.AdjoiningRoom;

			if (room.Alternated &&
				room.AlternateBaseRoom is not null &&
				adjoiningRoom.Alternated &&
				adjoiningRoom.AlternateRoom is not null)
			{
				adjoiningRoom = adjoiningRoom.AlternateRoom;
			}

			// Get the near block in current room
			Block nearBlock = room.Blocks[x, room.NumZSectors - 2];

			int qaNearStart = nearBlock.Floor.XnZp;
			int qaNearEnd = nearBlock.Floor.XpZp;

			if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
				qaNearStart = qaNearEnd;
			else if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
				qaNearEnd = qaNearStart;

			int wsNearStart = nearBlock.Ceiling.XnZp;
			int wsNearEnd = nearBlock.Ceiling.XpZp;

			if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
				wsNearStart = wsNearEnd;
			else if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
				wsNearEnd = wsNearStart;

			// Now get the facing block on the adjoining room and calculate the correct heights
			int facingX = x + (room.Position.X - adjoiningRoom.Position.X);

			Block adjoiningBlock = adjoiningRoom.GetBlockTry(facingX, 1) ?? Block.Empty;

			int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XnZn;
			int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XpZn;

			if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
				qAportal = qBportal;
			else if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
				qBportal = qAportal;

			qaStartY = room.Position.Y + qaNearStart;
			qaEndY = room.Position.Y + qaNearEnd;
			qaStartY = Math.Max(qaStartY, qAportal) - room.Position.Y;
			qaEndY = Math.Max(qaEndY, qBportal) - room.Position.Y;

			int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZn;
			int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZn;

			if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
				wAportal = wBportal;
			else if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
				wBportal = wAportal;

			wsStartY = room.Position.Y + wsNearStart;
			wsEndY = room.Position.Y + wsNearEnd;
			wsStartY = Math.Min(wsStartY, wAportal) - room.Position.Y;
			wsEndY = Math.Min(wsEndY, wBportal) - room.Position.Y;

			WallSplit newSubdivision;

			for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn));

				if (i >= extraFloorSubdivisions.Count)
					extraFloorSubdivisions.Add(newSubdivision);
				else
					extraFloorSubdivisions[i] = newSubdivision;
			}

			for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn));

				if (i >= extraCeilingSubdivisions.Count)
					extraCeilingSubdivisions.Add(newSubdivision);
				else
					extraCeilingSubdivisions[i] = newSubdivision;
			}
		}

		if (block.Floor.DiagonalSplit is DiagonalSplit.XpZp)
		{
			qaStartY = block.Floor.XnZn;
			qaEndY = block.Floor.XnZn;
		}
		else if (block.Floor.DiagonalSplit is DiagonalSplit.XnZp)
		{
			qaStartY = block.Floor.XpZn;
			qaEndY = block.Floor.XpZn;
		}

		if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
		{
			startMinY = neighborBlock.Floor.XnZp;
			endMinY = neighborBlock.Floor.XnZp;
		}
		else if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
		{
			startMinY = neighborBlock.Floor.XpZp;
			endMinY = neighborBlock.Floor.XpZp;
		}

		if (block.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
		{
			wsStartY = block.Ceiling.XnZn;
			wsEndY = block.Ceiling.XnZn;
		}
		else if (block.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
		{
			wsStartY = block.Ceiling.XpZn;
			wsEndY = block.Ceiling.XpZn;
		}

		if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
		{
			startMaxY = neighborBlock.Ceiling.XnZp;
			endMaxY = neighborBlock.Ceiling.XnZp;
		}
		else if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
		{
			startMaxY = neighborBlock.Ceiling.XpZp;
			endMaxY = neighborBlock.Ceiling.XpZp;
		}

		var wall = new SectorWall
		(
			direction: Direction.NegativeZ,

			start: new WallEnd
			(
				x: x,
				z: z,
				minY: startMinY,
				maxY: startMaxY
			),

			end: new WallEnd
			(
				x: x + 1,
				z: z,
				minY: endMinY,
				maxY: endMaxY
			),

			qa: new WallSplit
			(
				startY: qaStartY,
				endY: qaEndY
			),

			ws: new WallSplit
			(
				startY: wsStartY,
				endY: wsEndY
			),

			extraFloorSubdivisions: extraFloorSubdivisions,
			extraCeilingSubdivisions: extraCeilingSubdivisions
		);

		return normalize
			? wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall)
			: wall;
	}

	public static SectorWall GetPositiveXWallData(this Room room, int x, int z, bool normalize)
	{
		Block block = room.Blocks[x, z];
		Block neighborBlock = room.Blocks[x + 1, z];

		int startMinY = neighborBlock.Floor.XnZn,
			startMaxY = neighborBlock.Ceiling.XnZn,

			endMinY = neighborBlock.Floor.XnZp,
			endMaxY = neighborBlock.Ceiling.XnZp,

			qaStartY = block.Floor.XpZn,
			qaEndY = block.Floor.XpZp,

			wsStartY = block.Ceiling.XpZn,
			wsEndY = block.Ceiling.XpZp;

		List<WallSplit>
			extraFloorSubdivisions = new(),
			extraCeilingSubdivisions = new();

		for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
		{
			extraFloorSubdivisions.Add(new WallSplit(
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp)));
		}

		for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
		{
			extraCeilingSubdivisions.Add(new WallSplit(
				block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn),
				block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp)));
		}

		if (block.WallPortal is not null)
		{
			// Get the adjoining room of the portal
			Room adjoiningRoom = block.WallPortal.AdjoiningRoom;

			if (room.Alternated &&
				room.AlternateBaseRoom is not null &&
				adjoiningRoom.Alternated &&
				adjoiningRoom.AlternateRoom is not null)
			{
				adjoiningRoom = adjoiningRoom.AlternateRoom;
			}

			// Get the near block in current room
			Block nearBlock = room.Blocks[1, z];

			int qaNearStart = nearBlock.Floor.XnZn;
			int qaNearEnd = nearBlock.Floor.XnZp;

			if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
				qaNearStart = qaNearEnd;
			else if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
				qaNearEnd = qaNearStart;

			int wsNearStart = nearBlock.Ceiling.XnZn;
			int wsNearEnd = nearBlock.Ceiling.XnZp;

			if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
				wsNearStart = wsNearEnd;
			else if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
				wsNearEnd = wsNearStart;

			// Now get the facing block on the adjoining room and calculate the correct heights
			int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);

			Block adjoiningBlock = adjoiningRoom.GetBlockTry(adjoiningRoom.NumXSectors - 2, facingZ) ?? Block.Empty;

			int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XpZn;
			int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XpZp;

			if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
				qAportal = qBportal;
			else if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
				qBportal = qAportal;

			qaStartY = room.Position.Y + qaNearStart;
			qaEndY = room.Position.Y + qaNearEnd;
			qaStartY = Math.Max(qaStartY, qAportal) - room.Position.Y;
			qaEndY = Math.Max(qaEndY, qBportal) - room.Position.Y;

			int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZn;
			int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZp;

			if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
				wAportal = wBportal;
			else if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
				wBportal = wAportal;

			wsStartY = room.Position.Y + wsNearStart;
			wsEndY = room.Position.Y + wsNearEnd;
			wsStartY = Math.Min(wsStartY, wAportal) - room.Position.Y;
			wsEndY = Math.Min(wsEndY, wBportal) - room.Position.Y;

			WallSplit newSubdivision;

			for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp));

				if (i >= extraFloorSubdivisions.Count)
					extraFloorSubdivisions.Add(newSubdivision);
				else
					extraFloorSubdivisions[i] = newSubdivision;
			}

			for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp));

				if (i >= extraCeilingSubdivisions.Count)
					extraCeilingSubdivisions.Add(newSubdivision);
				else
					extraCeilingSubdivisions[i] = newSubdivision;
			}
		}

		if (block.Floor.DiagonalSplit is DiagonalSplit.XnZn)
		{
			qaStartY = block.Floor.XpZp;
			qaEndY = block.Floor.XpZp;
		}
		else if (block.Floor.DiagonalSplit is DiagonalSplit.XnZp)
		{
			qaStartY = block.Floor.XpZn;
			qaEndY = block.Floor.XpZn;
		}

		if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
		{
			startMinY = neighborBlock.Floor.XnZp;
			endMinY = neighborBlock.Floor.XnZp;
		}
		else if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
		{
			startMinY = neighborBlock.Floor.XnZn;
			endMinY = neighborBlock.Floor.XnZn;
		}

		if (block.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
		{
			wsStartY = block.Ceiling.XpZp;
			wsEndY = block.Ceiling.XpZp;
		}
		else if (block.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
		{
			wsStartY = block.Ceiling.XpZn;
			wsEndY = block.Ceiling.XpZn;
		}

		if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
		{
			startMaxY = neighborBlock.Ceiling.XnZp;
			endMaxY = neighborBlock.Ceiling.XnZp;
		}
		else if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
		{
			startMaxY = neighborBlock.Ceiling.XnZn;
			endMaxY = neighborBlock.Ceiling.XnZn;
		}

		var wall = new SectorWall
		(
			direction: Direction.PositiveX,

			start: new WallEnd
			(
				x: x + 1,
				z: z,
				minY: startMinY,
				maxY: startMaxY
			),

			end: new WallEnd
			(
				x: x + 1,
				z: z + 1,
				minY: endMinY,
				maxY: endMaxY
			),

			qa: new WallSplit
			(
				startY: qaStartY,
				endY: qaEndY
			),

			ws: new WallSplit
			(
				startY: wsStartY,
				endY: wsEndY
			),

			extraFloorSubdivisions: extraFloorSubdivisions,
			extraCeilingSubdivisions: extraCeilingSubdivisions
		);

		return normalize
			? wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall)
			: wall;
	}

	public static SectorWall GetNegativeXWallData(this Room room, int x, int z, bool normalize)
	{
		Block block = room.Blocks[x, z];
		Block neighborBlock = room.Blocks[x - 1, z];

		int startMinY = neighborBlock.Floor.XpZp,
			startMaxY = neighborBlock.Ceiling.XpZp,

			endMinY = neighborBlock.Floor.XpZn,
			endMaxY = neighborBlock.Ceiling.XpZn,

			qaStartY = block.Floor.XnZp,
			qaEndY = block.Floor.XnZn,

			wsStartY = block.Ceiling.XnZp,
			wsEndY = block.Ceiling.XnZn;

		List<WallSplit>
			extraFloorSubdivisions = new(),
			extraCeilingSubdivisions = new();

		for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
		{
			extraFloorSubdivisions.Add(new WallSplit(
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn)));
		}

		for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
		{
			extraCeilingSubdivisions.Add(new WallSplit(
				block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp),
				block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn)));
		}

		if (block.WallPortal is not null)
		{
			// Get the adjoining room of the portal
			Room adjoiningRoom = block.WallPortal.AdjoiningRoom;

			if (room.Alternated &&
				room.AlternateBaseRoom is not null &&
				adjoiningRoom.Alternated &&
				adjoiningRoom.AlternateRoom is not null)
			{
				adjoiningRoom = adjoiningRoom.AlternateRoom;
			}

			// Get the near block in current room
			Block nearBlock = room.Blocks[room.NumXSectors - 2, z];

			int qaNearStart = nearBlock.Floor.XpZp;
			int qaNearEnd = nearBlock.Floor.XpZn;

			if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
				qaNearStart = qaNearEnd;
			else if (nearBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
				qaNearEnd = qaNearStart;

			int wsNearStart = nearBlock.Ceiling.XpZp;
			int wsNearEnd = nearBlock.Ceiling.XpZn;

			if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
				wsNearStart = wsNearEnd;
			else if (nearBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
				wsNearEnd = wsNearStart;

			// Now get the facing block on the adjoining room and calculate the correct heights
			int facingZ = z + (room.Position.Z - adjoiningRoom.Position.Z);

			Block adjoiningBlock = adjoiningRoom.GetBlockTry(1, facingZ) ?? Block.Empty;

			int qAportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XnZp;
			int qBportal = adjoiningRoom.Position.Y + adjoiningBlock.Floor.XnZn;

			if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
				qAportal = qBportal;
			else if (adjoiningBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
				qBportal = qAportal;

			qaStartY = room.Position.Y + qaNearStart;
			qaEndY = room.Position.Y + qaNearEnd;
			qaStartY = Math.Max(qaStartY, qAportal) - room.Position.Y;
			qaEndY = Math.Max(qaEndY, qBportal) - room.Position.Y;

			int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZp;
			int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZn;

			if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
				wAportal = wBportal;
			else if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
				wBportal = wAportal;

			wsStartY = room.Position.Y + wsNearStart;
			wsEndY = room.Position.Y + wsNearEnd;
			wsStartY = Math.Min(wsStartY, wAportal) - room.Position.Y;
			wsEndY = Math.Min(wsEndY, wBportal) - room.Position.Y;

			WallSplit newSubdivision;

			for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn));

				if (i >= extraFloorSubdivisions.Count)
					extraFloorSubdivisions.Add(newSubdivision);
				else
					extraFloorSubdivisions[i] = newSubdivision;
			}

			for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn));

				if (i >= extraCeilingSubdivisions.Count)
					extraCeilingSubdivisions.Add(newSubdivision);
				else
					extraCeilingSubdivisions[i] = newSubdivision;
			}
		}

		if (block.Floor.DiagonalSplit is DiagonalSplit.XpZn)
		{
			qaStartY = block.Floor.XnZp;
			qaEndY = block.Floor.XnZp;
		}
		else if (block.Floor.DiagonalSplit is DiagonalSplit.XpZp)
		{
			qaStartY = block.Floor.XnZn;
			qaEndY = block.Floor.XnZn;
		}

		if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
		{
			startMinY = neighborBlock.Floor.XpZp;
			endMinY = neighborBlock.Floor.XpZp;
		}
		else if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
		{
			startMinY = neighborBlock.Floor.XpZn;
			endMinY = neighborBlock.Floor.XpZn;
		}

		if (block.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
		{
			wsStartY = block.Ceiling.XnZp;
			wsEndY = block.Ceiling.XnZp;
		}
		else if (block.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
		{
			wsStartY = block.Ceiling.XnZn;
			wsEndY = block.Ceiling.XnZn;
		}

		if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
		{
			startMaxY = neighborBlock.Ceiling.XpZp;
			endMaxY = neighborBlock.Ceiling.XpZp;
		}
		else if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
		{
			startMaxY = neighborBlock.Ceiling.XpZn;
			endMaxY = neighborBlock.Ceiling.XpZn;
		}

		var wall = new SectorWall
		(
			direction: Direction.NegativeX,

			start: new WallEnd
			(
				x: x,
				z: z + 1,
				minY: startMinY,
				maxY: startMaxY
			),

			end: new WallEnd
			(
				x: x,
				z: z,
				minY: endMinY,
				maxY: endMaxY
			),

			qa: new WallSplit
			(
				startY: qaStartY,
				endY: qaEndY
			),

			ws: new WallSplit
			(
				startY: wsStartY,
				endY: wsEndY
			),

			extraFloorSubdivisions: extraFloorSubdivisions,
			extraCeilingSubdivisions: extraCeilingSubdivisions
		);

		return normalize
			? wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall)
			: wall;
	}

	public static SectorWall GetDiagonalWallData(this Room room, int x, int z, bool isDiagonalCeiling, bool normalize)
	{
		Block block = room.Blocks[x, z];

		int startX, startZ, endX, endZ,
			startMinY, startMaxY, endMinY, endMaxY,
			qaStartY, qaEndY, wsStartY, wsEndY;

		List<WallSplit>
			extraFloorSubdivisions = new(),
			extraCeilingSubdivisions = new();

		switch (isDiagonalCeiling ? block.Ceiling.DiagonalSplit : block.Floor.DiagonalSplit)
		{
			case DiagonalSplit.XpZn:
				startX = x + 1;
				startZ = z + 1;

				endX = x;
				endZ = z;

				startMinY = isDiagonalCeiling
					? (block.IsAnyWall ? block.Floor.XnZp : block.Floor.XpZp) // DiagonalCeiling
					: block.Floor.XnZp; // DiagonalFloor

				startMaxY = isDiagonalCeiling
					? block.Ceiling.XnZp // DiagonalCeiling
					: (block.IsAnyWall ? block.Ceiling.XnZp : block.Ceiling.XpZp); // DiagonalFloor

				endMinY = isDiagonalCeiling
					? (block.IsAnyWall ? block.Floor.XnZp : block.Floor.XnZn) // DiagonalCeiling
					: block.Floor.XnZp; // DiagonalFloor

				endMaxY = isDiagonalCeiling
					? block.Ceiling.XnZp // DiagonalCeiling
					: (block.IsAnyWall ? block.Ceiling.XnZp : block.Ceiling.XnZn); // DiagonalFloor

				qaStartY = block.Floor.XpZp;
				qaEndY = block.Floor.XnZn;
				wsStartY = block.Ceiling.XpZp;
				wsEndY = block.Ceiling.XnZn;

				for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
				{
					extraFloorSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn)));
				}

				for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
				{
					extraCeilingSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp),
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn)));
				}

				break;

			case DiagonalSplit.XnZn:
				startX = x + 1;
				startZ = z;

				endX = x;
				endZ = z + 1;

				startMinY = isDiagonalCeiling
					? (block.IsAnyWall ? block.Floor.XpZp : block.Floor.XpZn) // DiagonalCeiling
					: block.Floor.XpZp; // DiagonalFloor

				startMaxY = isDiagonalCeiling
					? block.Ceiling.XpZp // DiagonalCeiling
					: (block.IsAnyWall ? block.Ceiling.XpZp : block.Ceiling.XpZn); // DiagonalFloor

				endMinY = isDiagonalCeiling
					? (block.IsAnyWall ? block.Floor.XpZp : block.Floor.XnZp) // DiagonalCeiling
					: block.Floor.XpZp; // DiagonalFloor

				endMaxY = isDiagonalCeiling
					? block.Ceiling.XpZp // DiagonalCeiling
					: (block.IsAnyWall ? block.Ceiling.XpZp : block.Ceiling.XnZp); // DiagonalFloor

				qaStartY = block.Floor.XpZn;
				qaEndY = block.Floor.XnZp;
				wsStartY = block.Ceiling.XpZn;
				wsEndY = block.Ceiling.XnZp;

				for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
				{
					extraFloorSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp)));
				}

				for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
				{
					extraCeilingSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn),
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp)));
				}

				break;

			case DiagonalSplit.XnZp:
				startX = x;
				startZ = z;

				endX = x + 1;
				endZ = z + 1;

				startMinY = isDiagonalCeiling
					? (block.IsAnyWall ? block.Floor.XpZn : block.Floor.XnZn) // DiagonalCeiling
					: block.Floor.XpZn; // DiagonalFloor

				startMaxY = isDiagonalCeiling
					? block.Ceiling.XpZn // DiagonalCeiling
					: (block.IsAnyWall ? block.Ceiling.XpZn : block.Ceiling.XnZn); // DiagonalFloor

				endMinY = isDiagonalCeiling
					? (block.IsAnyWall ? block.Floor.XpZn : block.Floor.XpZp) // DiagonalCeiling
					: block.Floor.XpZn; // DiagonalFloor

				endMaxY = isDiagonalCeiling
					? block.Ceiling.XpZn // DiagonalCeiling
					: (block.IsAnyWall ? block.Ceiling.XpZn : block.Ceiling.XpZp); // DiagonalFloor

				qaStartY = block.Floor.XnZn;
				qaEndY = block.Floor.XpZp;
				wsStartY = block.Ceiling.XnZn;
				wsEndY = block.Ceiling.XpZp;

				for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
				{
					extraFloorSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp)));
				}

				for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
				{
					extraCeilingSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn),
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp)));
				}

				break;

			default:
				startX = x;
				startZ = z + 1;

				endX = x + 1;
				endZ = z;

				startMinY = isDiagonalCeiling
					? (block.IsAnyWall ? block.Floor.XnZn : block.Floor.XnZp) // DiagonalCeiling
					: block.Floor.XnZn; // DiagonalFloor

				startMaxY = isDiagonalCeiling
					? block.Ceiling.XnZn // DiagonalCeiling
					: (block.IsAnyWall ? block.Ceiling.XnZn : block.Ceiling.XnZp); // DiagonalFloor

				endMinY = isDiagonalCeiling
					? (block.IsAnyWall ? block.Floor.XnZn : block.Floor.XpZn) // DiagonalCeiling
					: block.Floor.XnZn; // DiagonalFloor

				endMaxY = isDiagonalCeiling
					? block.Ceiling.XnZn // DiagonalCeiling
					: (block.IsAnyWall ? block.Ceiling.XnZn : block.Ceiling.XpZn); // DiagonalFloor

				qaStartY = block.Floor.XnZp;
				qaEndY = block.Floor.XpZn;
				wsStartY = block.Ceiling.XnZp;
				wsEndY = block.Ceiling.XpZn;

				for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
				{
					extraFloorSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn)));
				}

				for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
				{
					extraCeilingSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp),
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn)));
				}

				break;
		}

		var wall = new SectorWall
		(
			direction: Direction.Diagonal,

			start: new WallEnd
			(
				x: startX,
				z: startZ,
				minY: startMinY,
				maxY: startMaxY
			),

			end: new WallEnd
			(
				x: endX,
				z: endZ,
				minY: endMinY,
				maxY: endMaxY
			),

			qa: new WallSplit
			(
				startY: qaStartY,
				endY: qaEndY
			),

			ws: new WallSplit
			(
				startY: wsStartY,
				endY: wsEndY
			),

			extraFloorSubdivisions: extraFloorSubdivisions,
			extraCeilingSubdivisions: extraCeilingSubdivisions
		);

		return normalize
			? wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall)
			: wall;
	}
}
