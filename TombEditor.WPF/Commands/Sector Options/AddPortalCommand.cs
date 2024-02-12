using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TombEditor.WPF.ViewModels;
using TombLib;
using TombLib.Forms;
using TombLib.LevelData;

namespace TombEditor.WPF.Commands;

internal sealed class AddPortalCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null)
	: UnconditionalEditorCommand(caller, editor, logger)
{
	public override void Execute(object? parameter)
	{
		if (CheckForRoomAndBlockSelection())
		{
			try
			{
				AddPortal(Editor.SelectedRoom, Editor.SelectedSectors.Area);
			}
			catch (Exception exc)
			{
				Logger.Error(exc, "Unable to create portal");
				Editor.SendMessage("Unable to create portal.\nException: " + exc.Message, PopupType.Error);
			}
		}
	}

	private void AddPortal(Room room, RectangleInt2 area)
	{
		if (room.CornerSelected(area))
		{
			Editor.SendMessage("You have selected one of the four room's corners.", PopupType.Error);
			return;
		}

		// Check vertical space
		int floorLevel = int.MaxValue;
		int ceilingLevel = int.MinValue;

		for (int y = area.Y0; y <= area.Y1 + 1; ++y)
		{
			for (int x = area.X0; x <= area.X1 + 1; ++x)
			{
				floorLevel = room.GetHeightsAtPoint(x, y, BlockVertical.Floor)
					.Select(v => v + room.Position.Y)
					.Concat(new int[] { floorLevel })
					.Min();

				ceilingLevel = room.GetHeightsAtPoint(x, y, BlockVertical.Ceiling)
					.Select(v => v + room.Position.Y)
					.Concat(new int[] { ceilingLevel })
					.Max();
			}
		}

		// Check for possible candidates ...
		var candidates = new List<(PortalDirection PortalDirection, Room Room)>();

		if (floorLevel != int.MaxValue && ceilingLevel != int.MinValue)
		{
			bool couldBeFloorCeilingPortal = false;

			if (new RectangleInt2(1, 1, room.NumXSectors - 2, room.NumZSectors - 2).Contains(area))
			{
				for (int z = area.Y0; z <= area.Y1; ++z)
				{
					for (int x = area.X0; x <= area.X1; ++x)
					{
						if (!room.Blocks[x, z].IsAnyWall)
							couldBeFloorCeilingPortal = true;
					}
				}
			}

			foreach (Room neighborRoom in Editor.Level.ExistingRooms)
			{
				// Don't make a portal to the room itself
				// Don't list alternate rooms as separate candidates
				if (neighborRoom == room || neighborRoom == room.AlternateOpposite || neighborRoom.AlternateBaseRoom != null)
					continue;

				RectangleInt2 neighborArea = area + (room.SectorPos - neighborRoom.SectorPos);

				if (!new RectangleInt2(0, 0, neighborRoom.NumXSectors - 1, neighborRoom.NumZSectors - 1).Contains(neighborArea))
					continue;

				// Check if they vertically touch
				int neighborFloorLevel = int.MaxValue;
				int neighborCeilingLevel = int.MinValue;

				for (int y = neighborArea.Y0; y <= neighborArea.Y1 + 1; ++y)
				{
					for (int x = neighborArea.X0; x <= neighborArea.X1 + 1; ++x)
					{
						neighborFloorLevel = neighborRoom.GetHeightsAtPoint(x, y, BlockVertical.Floor)
							.Select(v => v + neighborRoom.Position.Y)
							.Concat(new int[] { neighborFloorLevel })
							.Min();

						neighborCeilingLevel = neighborRoom.GetHeightsAtPoint(x, y, BlockVertical.Ceiling)
							.Select(v => v + neighborRoom.Position.Y)
							.Concat(new int[] { neighborCeilingLevel })
							.Max();

						if (neighborRoom.AlternateOpposite != null)
						{
							neighborFloorLevel = neighborRoom.AlternateOpposite.GetHeightsAtPoint(x, y, BlockVertical.Floor)
								.Select(v => v + neighborRoom.AlternateOpposite.Position.Y)
								.Concat(new int[] { neighborFloorLevel })
								.Min();

							neighborCeilingLevel = neighborRoom.AlternateOpposite.GetHeightsAtPoint(x, y, BlockVertical.Ceiling)
								.Select(v => v + neighborRoom.AlternateOpposite.Position.Y)
								.Concat(new int[] { neighborCeilingLevel })
								.Max();
						}
					}
				}

				if (neighborFloorLevel == int.MaxValue || neighborCeilingLevel == int.MinValue)
					continue;

				if (!(floorLevel <= neighborCeilingLevel && ceilingLevel >= neighborFloorLevel))
					continue;

				// Decide on a direction
				if (couldBeFloorCeilingPortal && new RectangleInt2(1, 1, neighborRoom.NumXSectors - 2, neighborRoom.NumZSectors - 2).Contains(neighborArea))
				{
					if (Math.Abs(neighborCeilingLevel - floorLevel) < Math.Abs(neighborFloorLevel - ceilingLevel))
					{
						// Consider floor portal
						candidates.Add((PortalDirection.Floor, neighborRoom));
					}
					else
					{
						// Consider ceiling portal
						candidates.Add((PortalDirection.Ceiling, neighborRoom));
					}
				}

				if (area.Width == 0 && area.X0 == 0)
					candidates.Add((PortalDirection.WallNegativeX, neighborRoom));
				if (area.Width == 0 && area.X0 == room.NumXSectors - 1)
					candidates.Add((PortalDirection.WallPositiveX, neighborRoom));
				if (area.Height == 0 && area.Y0 == 0)
					candidates.Add((PortalDirection.WallNegativeZ, neighborRoom));
				if (area.Height == 0 && area.Y0 == room.NumZSectors - 1)
					candidates.Add((PortalDirection.WallPositiveZ, neighborRoom));
			}
		}

		if (candidates.Count > 1)
		{
			var viewModel = new ChooseRoomWindowViewModel("More than one possible room found that can be connected. Please choose one:",
				candidates.Select(candidate => candidate.Room), selectedRoom => Editor.SelectedRoom = selectedRoom);

			if (Editor.DialogService.ShowDialog(Caller, viewModel) != true || viewModel.SelectedRoom == null)
				return;

			candidates.RemoveAll(candidate => candidate.Room != viewModel.SelectedRoom);
		}

		if (candidates.Count != 1)
		{
			Editor.SendMessage("There are no possible room candidates for a portal.", PopupType.Error);
			return;
		}

		PortalDirection destinationDirection = candidates[0].PortalDirection;
		Room destination = candidates[0].Room;

		// Create portals
		var mainPortal = new PortalInstance(area, destinationDirection, destination);
		var portals = room.AddObject(Editor.Level, mainPortal).Cast<PortalInstance>().ToList();

		if (destinationDirection >= PortalDirection.WallPositiveZ) // If portal is any wall
		{
			// Remove all subdivisions from affected walls to expose geometry of the other room
			for (int z = area.Y0; z <= area.Y1; ++z)
			{
				for (int x = area.X0; x <= area.X1; ++x)
				{
					Block block = room.GetBlockTry(x, z);

					if (block == null)
						continue;

					block.ExtraFloorSubdivisions.Clear();
					block.ExtraCeilingSubdivisions.Clear();
				}
			}
		}

		// Update
		foreach (Room portalRoom in portals.Select(portal => portal.Room).Distinct())
		{
			portalRoom.BuildGeometry();
			portalRoom.RebuildLighting(Editor.Configuration.Rendering3D_HighQualityLightPreview);
		}

		foreach (PortalInstance portal in portals)
			Editor.ObjectChange(portal, ObjectChangeType.Add);

		// Reset selection
		Editor.Action = null;
		Editor.SelectedSectors = SectorSelection.None;
		Editor.SelectedObject = null;
		Editor.SelectedRooms = new[] { Editor.SelectedRoom };

		Editor.RoomSectorPropertiesChange(room);
		Editor.RoomSectorPropertiesChange(destination);

		// Undo
		Editor.UndoManager.PushSectorObjectCreated(mainPortal);
	}
}
