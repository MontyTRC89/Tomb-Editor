using NLog;
using System.ComponentModel;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;

namespace TombEditor.WPF.Commands;

internal sealed class SetDiagonalCeilingStepCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null)
	: SmartBuildGeometryCommand(caller, editor, logger)
{
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

				if (room.Blocks[x, z].Ceiling.DiagonalSplit != DiagonalSplit.None)
				{
					if (room.Blocks[x, z].Type == BlockType.Floor)
						room.Blocks[x, z].Transform(new RectTransformation { QuadrantRotation = -1 }, false);
				}
				else
				{
					// Now try to guess the floor split
					int minHeight = int.MaxValue;
					byte theCorner = 0;

					if (room.Blocks[x, z].Ceiling.XnZp < minHeight)
					{
						minHeight = room.Blocks[x, z].Ceiling.XnZp;
						theCorner = 0;
					}

					if (room.Blocks[x, z].Ceiling.XpZp < minHeight)
					{
						minHeight = room.Blocks[x, z].Ceiling.XpZp;
						theCorner = 1;
					}

					if (room.Blocks[x, z].Ceiling.XpZn < minHeight)
					{
						minHeight = room.Blocks[x, z].Ceiling.XpZn;
						theCorner = 2;
					}

					if (room.Blocks[x, z].Ceiling.XnZn < minHeight)
					{
						minHeight = room.Blocks[x, z].Ceiling.XnZn;
						theCorner = 3;
					}

					switch (theCorner)
					{
						case 0:
							room.Blocks[x, z].Ceiling.XpZp = minHeight;
							room.Blocks[x, z].Ceiling.XnZn = minHeight;
							room.Blocks[x, z].Ceiling.DiagonalSplit = DiagonalSplit.XnZp;
							break;

						case 1:
							room.Blocks[x, z].Ceiling.XnZp = minHeight;
							room.Blocks[x, z].Ceiling.XpZn = minHeight;
							room.Blocks[x, z].Ceiling.DiagonalSplit = DiagonalSplit.XpZp;
							break;

						case 2:
							room.Blocks[x, z].Ceiling.XpZp = minHeight;
							room.Blocks[x, z].Ceiling.XnZn = minHeight;
							room.Blocks[x, z].Ceiling.DiagonalSplit = DiagonalSplit.XpZn;
							break;

						case 3:
							room.Blocks[x, z].Ceiling.XnZp = minHeight;
							room.Blocks[x, z].Ceiling.XpZn = minHeight;
							room.Blocks[x, z].Ceiling.DiagonalSplit = DiagonalSplit.XnZn;
							break;
					}

					room.Blocks[x, z].Ceiling.SplitDirectionToggled = false;
					room.Blocks[x, z].FixHeights();
				}

				room.Blocks[x, z].Type = BlockType.Floor;
			}
		}

		CommitSmartBuildGeometry(room, area);
	}
}
