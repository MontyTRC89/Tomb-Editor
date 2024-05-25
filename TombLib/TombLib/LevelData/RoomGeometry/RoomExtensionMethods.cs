using System;

namespace TombLib.LevelData;

public static class RoomExtensionMethods
{
	public static SectorWall GetPositiveZWallData(this Room room, int x, int z, bool normalize)
	{
		Block block = room.Blocks[x, z];
		Block neighborBlock = room.Blocks[x, z + 1];

		var wall = new SectorWall
		{
			Direction = Direction.PositiveZ,

			Start = new WallEnd
			{
				X = x + 1,
				Z = z + 1,
				MinY = neighborBlock.Floor.XpZn,
				MaxY = neighborBlock.Ceiling.XpZn
			},

			End = new WallEnd
			{
				X = x,
				Z = z + 1,
				MinY = neighborBlock.Floor.XnZn,
				MaxY = neighborBlock.Ceiling.XnZn
			}
		};

		wall.QA.StartY = block.Floor.XpZp;
		wall.QA.EndY = block.Floor.XnZp;
		wall.WS.StartY = block.Ceiling.XpZp;
		wall.WS.EndY = block.Ceiling.XnZp;

		for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
		{
			wall.FloorSubdivisions.Add(new WallSplit(
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp)));
		}

		for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
		{
			wall.CeilingSubdivisions.Add(new WallSplit(
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

			wall.QA.StartY = room.Position.Y + qaNearStart;
			wall.QA.EndY = room.Position.Y + qaNearEnd;
			wall.QA.StartY = Math.Max(wall.QA.StartY, qAportal) - room.Position.Y;
			wall.QA.EndY = Math.Max(wall.QA.EndY, qBportal) - room.Position.Y;

			int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZp;
			int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZp;

			if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
				wAportal = wBportal;
			else if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
				wBportal = wAportal;

			wall.WS.StartY = room.Position.Y + wsNearStart;
			wall.WS.EndY = room.Position.Y + wsNearEnd;
			wall.WS.StartY = Math.Min(wall.WS.StartY, wAportal) - room.Position.Y;
			wall.WS.EndY = Math.Min(wall.WS.EndY, wBportal) - room.Position.Y;

			WallSplit newSubdivision;

			for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp));

				if (i >= wall.FloorSubdivisions.Count)
					wall.FloorSubdivisions.Add(newSubdivision);
				else
					wall.FloorSubdivisions[i] = newSubdivision;
			}

			for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp));

				if (i >= wall.CeilingSubdivisions.Count)
					wall.CeilingSubdivisions.Add(newSubdivision);
				else
					wall.CeilingSubdivisions[i] = newSubdivision;
			}
		}

		if (block.Floor.DiagonalSplit is DiagonalSplit.XpZn)
		{
			wall.QA.StartY = block.Floor.XnZp;
			wall.QA.EndY = block.Floor.XnZp;
		}
		else if (block.Floor.DiagonalSplit is DiagonalSplit.XnZn)
		{
			wall.QA.StartY = block.Floor.XpZp;
			wall.QA.EndY = block.Floor.XpZp;
		}

		if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
		{
			wall.Start.MinY = neighborBlock.Floor.XpZn;
			wall.End.MinY = neighborBlock.Floor.XpZn;
		}
		else if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
		{
			wall.Start.MinY = neighborBlock.Floor.XnZn;
			wall.End.MinY = neighborBlock.Floor.XnZn;
		}

		if (block.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
		{
			wall.WS.StartY = block.Ceiling.XnZp;
			wall.WS.EndY = block.Ceiling.XnZp;
		}
		else if (block.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
		{
			wall.WS.StartY = block.Ceiling.XpZp;
			wall.WS.EndY = block.Ceiling.XpZp;
		}

		if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
		{
			wall.Start.MaxY = neighborBlock.Ceiling.XpZn;
			wall.End.MaxY = neighborBlock.Ceiling.XpZn;
		}
		else if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
		{
			wall.Start.MaxY = neighborBlock.Ceiling.XnZn;
			wall.End.MaxY = neighborBlock.Ceiling.XnZn;
		}

		if (normalize)
			wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall);

		return wall;
	}

	public static SectorWall GetNegativeZWallData(this Room room, int x, int z, bool normalize)
	{
		Block block = room.Blocks[x, z];
		Block neighborBlock = room.Blocks[x, z - 1];

		var wall = new SectorWall
		{
			Direction = Direction.NegativeZ,

			Start = new WallEnd
			{
				X = x,
				Z = z,
				MinY = neighborBlock.Floor.XnZp,
				MaxY = neighborBlock.Ceiling.XnZp
			},

			End = new WallEnd
			{
				X = x + 1,
				Z = z,
				MinY = neighborBlock.Floor.XpZp,
				MaxY = neighborBlock.Ceiling.XpZp
			}
		};

		wall.QA.StartY = block.Floor.XnZn;
		wall.QA.EndY = block.Floor.XpZn;
		wall.WS.StartY = block.Ceiling.XnZn;
		wall.WS.EndY = block.Ceiling.XpZn;

		for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
		{
			wall.FloorSubdivisions.Add(new WallSplit(
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn)));
		}

		for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
		{
			wall.CeilingSubdivisions.Add(new WallSplit(
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

			wall.QA.StartY = room.Position.Y + qaNearStart;
			wall.QA.EndY = room.Position.Y + qaNearEnd;
			wall.QA.StartY = Math.Max(wall.QA.StartY, qAportal) - room.Position.Y;
			wall.QA.EndY = Math.Max(wall.QA.EndY, qBportal) - room.Position.Y;

			int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZn;
			int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZn;

			if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
				wAportal = wBportal;
			else if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
				wBportal = wAportal;

			wall.WS.StartY = room.Position.Y + wsNearStart;
			wall.WS.EndY = room.Position.Y + wsNearEnd;
			wall.WS.StartY = Math.Min(wall.WS.StartY, wAportal) - room.Position.Y;
			wall.WS.EndY = Math.Min(wall.WS.EndY, wBportal) - room.Position.Y;

			WallSplit newSubdivision;

			for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn));

				if (i >= wall.FloorSubdivisions.Count)
					wall.FloorSubdivisions.Add(newSubdivision);
				else
					wall.FloorSubdivisions[i] = newSubdivision;
			}

			for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn));

				if (i >= wall.CeilingSubdivisions.Count)
					wall.CeilingSubdivisions.Add(newSubdivision);
				else
					wall.CeilingSubdivisions[i] = newSubdivision;
			}
		}

		if (block.Floor.DiagonalSplit is DiagonalSplit.XpZp)
		{
			wall.QA.StartY = block.Floor.XnZn;
			wall.QA.EndY = block.Floor.XnZn;
		}
		else if (block.Floor.DiagonalSplit is DiagonalSplit.XnZp)
		{
			wall.QA.StartY = block.Floor.XpZn;
			wall.QA.EndY = block.Floor.XpZn;
		}

		if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
		{
			wall.Start.MinY = neighborBlock.Floor.XnZp;
			wall.End.MinY = neighborBlock.Floor.XnZp;
		}
		else if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
		{
			wall.Start.MinY = neighborBlock.Floor.XpZp;
			wall.End.MinY = neighborBlock.Floor.XpZp;
		}

		if (block.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
		{
			wall.WS.StartY = block.Ceiling.XnZn;
			wall.WS.EndY = block.Ceiling.XnZn;
		}
		else if (block.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
		{
			wall.WS.StartY = block.Ceiling.XpZn;
			wall.WS.EndY = block.Ceiling.XpZn;
		}

		if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
		{
			wall.Start.MaxY = neighborBlock.Ceiling.XnZp;
			wall.End.MaxY = neighborBlock.Ceiling.XnZp;
		}
		else if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
		{
			wall.Start.MaxY = neighborBlock.Ceiling.XpZp;
			wall.End.MaxY = neighborBlock.Ceiling.XpZp;
		}

		if (normalize)
			wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall);

		return wall;
	}

	public static SectorWall GetPositiveXWallData(this Room room, int x, int z, bool normalize)
	{
		Block block = room.Blocks[x, z];
		Block neighborBlock = room.Blocks[x + 1, z];

		var wall = new SectorWall
		{
			Direction = Direction.PositiveX,

			Start = new WallEnd
			{
				X = x + 1,
				Z = z,
				MinY = neighborBlock.Floor.XnZn,
				MaxY = neighborBlock.Ceiling.XnZn
			},

			End = new WallEnd
			{
				X = x + 1,
				Z = z + 1,
				MinY = neighborBlock.Floor.XnZp,
				MaxY = neighborBlock.Ceiling.XnZp
			}
		};

		wall.QA.StartY = block.Floor.XpZn;
		wall.QA.EndY = block.Floor.XpZp;
		wall.WS.StartY = block.Ceiling.XpZn;
		wall.WS.EndY = block.Ceiling.XpZp;

		for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
		{
			wall.FloorSubdivisions.Add(new WallSplit(
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp)));
		}

		for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
		{
			wall.CeilingSubdivisions.Add(new WallSplit(
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

			wall.QA.StartY = room.Position.Y + qaNearStart;
			wall.QA.EndY = room.Position.Y + qaNearEnd;
			wall.QA.StartY = Math.Max(wall.QA.StartY, qAportal) - room.Position.Y;
			wall.QA.EndY = Math.Max(wall.QA.EndY, qBportal) - room.Position.Y;

			int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZn;
			int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XpZp;

			if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
				wAportal = wBportal;
			else if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
				wBportal = wAportal;

			wall.WS.StartY = room.Position.Y + wsNearStart;
			wall.WS.EndY = room.Position.Y + wsNearEnd;
			wall.WS.StartY = Math.Min(wall.WS.StartY, wAportal) - room.Position.Y;
			wall.WS.EndY = Math.Min(wall.WS.EndY, wBportal) - room.Position.Y;

			WallSplit newSubdivision;

			for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp));

				if (i >= wall.FloorSubdivisions.Count)
					wall.FloorSubdivisions.Add(newSubdivision);
				else
					wall.FloorSubdivisions[i] = newSubdivision;
			}

			for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp));

				if (i >= wall.CeilingSubdivisions.Count)
					wall.CeilingSubdivisions.Add(newSubdivision);
				else
					wall.CeilingSubdivisions[i] = newSubdivision;
			}
		}

		if (block.Floor.DiagonalSplit is DiagonalSplit.XnZn)
		{
			wall.QA.StartY = block.Floor.XpZp;
			wall.QA.EndY = block.Floor.XpZp;
		}
		else if (block.Floor.DiagonalSplit is DiagonalSplit.XnZp)
		{
			wall.QA.StartY = block.Floor.XpZn;
			wall.QA.EndY = block.Floor.XpZn;
		}

		if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XpZn)
		{
			wall.Start.MinY = neighborBlock.Floor.XnZp;
			wall.End.MinY = neighborBlock.Floor.XnZp;
		}
		else if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XpZp)
		{
			wall.Start.MinY = neighborBlock.Floor.XnZn;
			wall.End.MinY = neighborBlock.Floor.XnZn;
		}

		if (block.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
		{
			wall.WS.StartY = block.Ceiling.XpZp;
			wall.WS.EndY = block.Ceiling.XpZp;
		}
		else if (block.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
		{
			wall.WS.StartY = block.Ceiling.XpZn;
			wall.WS.EndY = block.Ceiling.XpZn;
		}

		if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
		{
			wall.Start.MaxY = neighborBlock.Ceiling.XnZp;
			wall.End.MaxY = neighborBlock.Ceiling.XnZp;
		}
		else if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
		{
			wall.Start.MaxY = neighborBlock.Ceiling.XnZn;
			wall.End.MaxY = neighborBlock.Ceiling.XnZn;
		}

		if (normalize)
			wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall);

		return wall;
	}

	public static SectorWall GetNegativeXWallData(this Room room, int x, int z, bool normalize)
	{
		Block block = room.Blocks[x, z];
		Block neighborBlock = room.Blocks[x - 1, z];

		var wall = new SectorWall
		{
			Direction = Direction.NegativeX,

			Start = new WallEnd
			{
				X = x,
				Z = z + 1,
				MinY = neighborBlock.Floor.XpZp,
				MaxY = neighborBlock.Ceiling.XpZp
			},

			End = new WallEnd
			{
				X = x,
				Z = z,
				MinY = neighborBlock.Floor.XpZn,
				MaxY = neighborBlock.Ceiling.XpZn
			}
		};

		wall.QA.StartY = block.Floor.XnZp;
		wall.QA.EndY = block.Floor.XpZp;
		wall.WS.StartY = block.Ceiling.XnZp;
		wall.WS.EndY = block.Ceiling.XpZp;

		for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
		{
			wall.FloorSubdivisions.Add(new WallSplit(
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
				block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn)));
		}

		for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
		{
			wall.CeilingSubdivisions.Add(new WallSplit(
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

			int wsNearStart = nearBlock.Ceiling.XpZn;
			int wsNearEnd = nearBlock.Ceiling.XpZp;

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

			wall.QA.StartY = room.Position.Y + qaNearStart;
			wall.QA.EndY = room.Position.Y + qaNearEnd;
			wall.QA.StartY = Math.Max(wall.QA.StartY, qAportal) - room.Position.Y;
			wall.QA.EndY = Math.Max(wall.QA.EndY, qBportal) - room.Position.Y;

			int wAportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZp;
			int wBportal = adjoiningRoom.Position.Y + adjoiningBlock.Ceiling.XnZn;

			if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
				wAportal = wBportal;
			else if (adjoiningBlock.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
				wBportal = wAportal;

			wall.WS.StartY = room.Position.Y + wsNearStart;
			wall.WS.EndY = room.Position.Y + wsNearEnd;
			wall.WS.StartY = Math.Min(wall.WS.StartY, wAportal) - room.Position.Y;
			wall.WS.EndY = Math.Min(wall.WS.EndY, wBportal) - room.Position.Y;

			WallSplit newSubdivision;

			for (int i = 0; i < adjoiningBlock.ExtraFloorSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn));

				if (i >= wall.FloorSubdivisions.Count)
					wall.FloorSubdivisions.Add(newSubdivision);
				else
					wall.FloorSubdivisions[i] = newSubdivision;
			}

			for (int i = 0; i < adjoiningBlock.ExtraCeilingSubdivisions.Count; i++)
			{
				newSubdivision = new WallSplit(adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp),
					adjoiningRoom.Position.Y - room.Position.Y + adjoiningBlock.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn));

				if (i >= wall.CeilingSubdivisions.Count)
					wall.CeilingSubdivisions.Add(newSubdivision);
				else
					wall.CeilingSubdivisions[i] = newSubdivision;
			}
		}

		if (block.Floor.DiagonalSplit is DiagonalSplit.XpZn)
		{
			wall.QA.StartY = block.Floor.XnZp;
			wall.QA.EndY = block.Floor.XnZp;
		}
		else if (block.Floor.DiagonalSplit is DiagonalSplit.XpZp)
		{
			wall.QA.StartY = block.Floor.XnZn;
			wall.QA.EndY = block.Floor.XnZn;
		}

		if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XnZn)
		{
			wall.Start.MinY = neighborBlock.Floor.XpZp;
			wall.End.MinY = neighborBlock.Floor.XpZp;
		}
		else if (neighborBlock.Floor.DiagonalSplit is DiagonalSplit.XnZp)
		{
			wall.Start.MinY = neighborBlock.Floor.XpZn;
			wall.End.MinY = neighborBlock.Floor.XpZn;
		}

		if (block.Ceiling.DiagonalSplit is DiagonalSplit.XpZn)
		{
			wall.WS.StartY = block.Ceiling.XnZp;
			wall.WS.EndY = block.Ceiling.XnZp;
		}
		else if (block.Ceiling.DiagonalSplit is DiagonalSplit.XpZp)
		{
			wall.WS.StartY = block.Ceiling.XnZn;
			wall.WS.EndY = block.Ceiling.XnZn;
		}

		if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZn)
		{
			wall.Start.MaxY = neighborBlock.Ceiling.XpZp;
			wall.End.MaxY = neighborBlock.Ceiling.XpZp;
		}
		else if (neighborBlock.Ceiling.DiagonalSplit is DiagonalSplit.XnZp)
		{
			wall.Start.MaxY = neighborBlock.Ceiling.XpZn;
			wall.End.MaxY = neighborBlock.Ceiling.XpZn;
		}

		if (normalize)
			wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall);

		return wall;
	}

	public static SectorWall GetDiagonalWallData(this Room room, int x, int z, bool isDiagonalCeiling, bool normalize)
	{
		Block block = room.Blocks[x, z];
		SectorWall wall;

		switch (isDiagonalCeiling ? block.Ceiling.DiagonalSplit : block.Floor.DiagonalSplit)
		{
			case DiagonalSplit.XpZn:
				wall = new SectorWall
				{
					Start = new WallEnd
					{
						X = x + 1,
						Z = z + 1,

						MinY = isDiagonalCeiling
							? (block.IsAnyWall ? block.Floor.XnZp : block.Floor.XpZp) // DiagonalCeiling
							: block.Floor.XnZp, // DiagonalFloor

						MaxY = isDiagonalCeiling
							? block.Ceiling.XnZp // DiagonalCeiling
							: (block.IsAnyWall ? block.Ceiling.XnZp : block.Ceiling.XpZp) // DiagonalFloor
					},

					End = new WallEnd
					{
						X = x,
						Z = z,

						MinY = isDiagonalCeiling
							? (block.IsAnyWall ? block.Floor.XnZp : block.Floor.XnZn) // DiagonalCeiling
							: block.Floor.XnZp, // DiagonalFloor

						MaxY = isDiagonalCeiling
							? block.Ceiling.XnZp // DiagonalCeiling
							: (block.IsAnyWall ? block.Ceiling.XnZp : block.Ceiling.XnZn) // DiagonalFloor
					}
				};

				wall.QA.StartY = block.Floor.XpZp;
				wall.QA.EndY = block.Floor.XnZn;
				wall.WS.StartY = block.Ceiling.XpZp;
				wall.WS.EndY = block.Ceiling.XnZn;

				for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
				{
					wall.FloorSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp),
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn)));
				}

				for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
				{
					wall.CeilingSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp),
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn)));
				}

				break;

			case DiagonalSplit.XnZn:
				wall = new SectorWall
				{
					Start = new WallEnd
					{
						X = x + 1,
						Z = z,

						MinY = isDiagonalCeiling
							? (block.IsAnyWall ? block.Floor.XpZp : block.Floor.XpZn) // DiagonalCeiling
							: block.Floor.XpZp, // DiagonalFloor

						MaxY = isDiagonalCeiling
							? block.Ceiling.XpZp // DiagonalCeiling
							: (block.IsAnyWall ? block.Ceiling.XpZp : block.Ceiling.XpZn) // DiagonalFloor
					},

					End = new WallEnd
					{
						X = x,
						Z = z + 1,

						MinY = isDiagonalCeiling
							? (block.IsAnyWall ? block.Floor.XpZp : block.Floor.XnZp) // DiagonalCeiling
							: block.Floor.XpZp, // DiagonalFloor

						MaxY = isDiagonalCeiling
							? block.Ceiling.XpZp // DiagonalCeiling
							: (block.IsAnyWall ? block.Ceiling.XpZp : block.Ceiling.XnZp) // DiagonalFloor
					}
				};

				wall.QA.StartY = block.Floor.XpZn;
				wall.QA.EndY = block.Floor.XnZp;
				wall.WS.StartY = block.Ceiling.XpZn;
				wall.WS.EndY = block.Ceiling.XnZp;

				for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
				{
					wall.FloorSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn),
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp)));
				}

				for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
				{
					wall.CeilingSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn),
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp)));
				}

				break;

			case DiagonalSplit.XnZp:
				wall = new SectorWall
				{
					Start = new WallEnd
					{
						X = x,
						Z = z,

						MinY = isDiagonalCeiling
							? (block.IsAnyWall ? block.Floor.XpZn : block.Floor.XnZn) // DiagonalCeiling
							: block.Floor.XpZn, // DiagonalFloor

						MaxY = isDiagonalCeiling
							? block.Ceiling.XpZn // DiagonalCeiling
							: (block.IsAnyWall ? block.Ceiling.XpZn : block.Ceiling.XnZn) // DiagonalFloor
					},

					End = new WallEnd
					{
						X = x + 1,
						Z = z + 1,

						MinY = isDiagonalCeiling
							? (block.IsAnyWall ? block.Floor.XpZn : block.Floor.XpZp) // DiagonalCeiling
							: block.Floor.XpZn, // DiagonalFloor

						MaxY = isDiagonalCeiling
							? block.Ceiling.XpZn // DiagonalCeiling
							: (block.IsAnyWall ? block.Ceiling.XpZn : block.Ceiling.XpZp) // DiagonalFloor
					}
				};

				wall.QA.StartY = block.Floor.XnZn;
				wall.QA.EndY = block.Floor.XpZp;
				wall.WS.StartY = block.Ceiling.XnZn;
				wall.WS.EndY = block.Ceiling.XpZp;

				for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
				{
					wall.FloorSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZn),
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZp)));
				}

				for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
				{
					wall.CeilingSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZn),
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZp)));
				}

				break;

			default:
				wall = new SectorWall
				{
					Start = new WallEnd
					{
						X = x,
						Z = z + 1,

						MinY = isDiagonalCeiling
							? (block.IsAnyWall ? block.Floor.XnZn : block.Floor.XnZp) // DiagonalCeiling
							: block.Floor.XnZn, // DiagonalFloor

						MaxY = isDiagonalCeiling
							? block.Ceiling.XnZn // DiagonalCeiling
							: (block.IsAnyWall ? block.Ceiling.XnZn : block.Ceiling.XnZp) // DiagonalFloor
					},

					End = new WallEnd
					{
						X = x + 1,
						Z = z,

						MinY = isDiagonalCeiling
							? (block.IsAnyWall ? block.Floor.XnZn : block.Floor.XpZn) // DiagonalCeiling
							: block.Floor.XnZn, // DiagonalFloor

						MaxY = isDiagonalCeiling
							? block.Ceiling.XnZn // DiagonalCeiling
							: (block.IsAnyWall ? block.Ceiling.XnZn : block.Ceiling.XpZn) // DiagonalFloor
					}
				};

				wall.QA.StartY = block.Floor.XnZp;
				wall.QA.EndY = block.Floor.XpZn;
				wall.WS.StartY = block.Ceiling.XnZp;
				wall.WS.EndY = block.Ceiling.XpZn;

				for (int i = 0; i < block.ExtraFloorSubdivisions.Count; i++)
				{
					wall.FloorSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XnZp),
						block.GetHeight(BlockVerticalExtensions.GetExtraFloorSubdivision(i), BlockEdge.XpZn)));
				}

				for (int i = 0; i < block.ExtraCeilingSubdivisions.Count; i++)
				{
					wall.CeilingSubdivisions.Add(new WallSplit(
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XnZp),
						block.GetHeight(BlockVerticalExtensions.GetExtraCeilingSubdivision(i), BlockEdge.XpZn)));
				}

				break;
		}

		wall.Direction = Direction.Diagonal;

		if (normalize)
			wall.Normalize(block.Floor.DiagonalSplit, block.Ceiling.DiagonalSplit, block.IsAnyWall);

		return wall;
	}
}
