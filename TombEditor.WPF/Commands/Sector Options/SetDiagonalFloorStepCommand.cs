using NLog;
using System.ComponentModel;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.WPF.Commands;

internal sealed class SetDiagonalFloorStepCommand : RoomGeometryCommand
{
	public SetDiagonalFloorStepCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : base(caller, editor, logger)
	{ }

	public override void Execute(object? parameter)
	{
		if (!CheckForRoomAndBlockSelection())
			return;

		Room room = Editor.SelectedRoom;
		RectangleInt2 area = Editor.SelectedSectors.Area;

		Editor.UndoManager.PushGeometryChanged(room);

		for (int x = area.X0; x <= area.X1; x++)
		{
			for (int z = area.Y0; z <= area.Y1; z++)
			{
				if (room.Blocks[x, z].Type == BlockType.BorderWall)
					continue;

				if (room.Blocks[x, z].Floor.DiagonalSplit != DiagonalSplit.None)
				{
					if (room.Blocks[x, z].Type == BlockType.Floor)
						room.Blocks[x, z].Transform(new RectTransformation { QuadrantRotation = -1 }, true);
				}
				else
				{
					// Now try to guess the floor split
					int maxHeight = int.MinValue;
					byte theCorner = 0;

					if (room.Blocks[x, z].Floor.XnZp > maxHeight)
					{
						maxHeight = room.Blocks[x, z].Floor.XnZp;
						theCorner = 0;
					}

					if (room.Blocks[x, z].Floor.XpZp > maxHeight)
					{
						maxHeight = room.Blocks[x, z].Floor.XpZp;
						theCorner = 1;
					}

					if (room.Blocks[x, z].Floor.XpZn > maxHeight)
					{
						maxHeight = room.Blocks[x, z].Floor.XpZn;
						theCorner = 2;
					}

					if (room.Blocks[x, z].Floor.XnZn > maxHeight)
					{
						maxHeight = room.Blocks[x, z].Floor.XnZn;
						theCorner = 3;
					}

					switch (theCorner)
					{
						case 0:
							room.Blocks[x, z].Floor.XpZp = maxHeight;
							room.Blocks[x, z].Floor.XnZn = maxHeight;
							room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.XnZp;
							break;

						case 1:
							room.Blocks[x, z].Floor.XnZp = maxHeight;
							room.Blocks[x, z].Floor.XpZn = maxHeight;
							room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.XpZp;
							break;

						case 2:
							room.Blocks[x, z].Floor.XpZp = maxHeight;
							room.Blocks[x, z].Floor.XnZn = maxHeight;
							room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.XpZn;
							break;

						case 3:
							room.Blocks[x, z].Floor.XnZp = maxHeight;
							room.Blocks[x, z].Floor.XpZn = maxHeight;
							room.Blocks[x, z].Floor.DiagonalSplit = DiagonalSplit.XnZn;
							break;
					}

					room.Blocks[x, z].Floor.SplitDirectionToggled = false;
					room.Blocks[x, z].FixHeights();
				}

				room.Blocks[x, z].Type = BlockType.Floor;
			}
		}

		CommitSmartBuildGeometry(room, area);
	}
}
