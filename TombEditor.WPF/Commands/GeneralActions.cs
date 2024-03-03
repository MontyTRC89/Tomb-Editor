using DarkUI.Forms;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using MvvmDialogs.FrameworkDialogs.SaveFile;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using TombEditor.Forms;
using TombLib;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.LevelData.IO;
using TombLib.Rendering;
using TombLib.Utils;
using TombLib.Wad;

namespace TombEditor.WPF.Commands;

internal static class GeneralActions
{
	private static readonly Logger logger = LogManager.GetCurrentClassLogger();

	private enum SmoothGeometryEditingType
	{
		None,
		Floor,
		Wall,
		Any
	}

	public static void CancelAnyAction(this Editor editor)
	{
		if (editor.Action != null)
			editor.Action = null;
		else
		{
			editor.SelectedSectors = SectorSelection.None;
			editor.SelectedObject = null;
			editor.SelectedRooms = new[] { editor.SelectedRoom };
		}
	}

	public static void CommitSmartBuildGeometry(this Editor editor, Room room, RectangleInt2 area, Logger? logger = null)
	{
		var watch = Stopwatch.StartNew();
		room.SmartBuildGeometry(area, editor.Configuration.Rendering3D_HighQualityLightPreview);
		watch.Stop();

		logger?.Debug($"Edit geometry time: {watch.ElapsedMilliseconds} ms");
		editor.RoomGeometryChange(room);
	}

	public static bool ContinueOnFileDrop(this Editor editor, INotifyPropertyChanged caller, string description)
	{
		if (!editor.HasUnsavedChanges || editor.Level.Settings.HasUnknownData)
			return true;

		MessageBoxResult result = editor.DialogService.ShowMessageBox(caller,
			"Your unsaved changes will be lost. Do you want to save?",
			description,
			MessageBoxButton.YesNoCancel,
			MessageBoxImage.Question,
			MessageBoxResult.No);

		return result switch
		{
			MessageBoxResult.No => true,
			MessageBoxResult.Yes => editor.SaveLevel(caller, false),
			_ => false,
		};
	}

	public static bool SaveLevel(this Editor editor, INotifyPropertyChanged caller, bool askForPath)
	{
		// Disable saving if level has unknown data (i.e. new prj2 version opened in old editor version)
		if (editor.Level.Settings.HasUnknownData)
		{
			editor.SendMessage("Project is in read-only mode because it was created in newer version of Tomb Editor.\nUse newest Tomb Editor version to edit and save this project.", PopupType.Warning);
			return false;
		}

		string fileName = editor.Level.Settings.LevelFilePath;

		// Show save dialog if necessary
		if (askForPath || string.IsNullOrEmpty(fileName))
		{
			var settings = new SaveFileDialogSettings
			{
				Title = "Save level",
				Filter = "Tomb Editor Level (*.prj2)|*.prj2|All Files (*.*)|*.*"
			};

			if (editor.DialogService.ShowSaveFileDialog(caller, settings) == true)
				fileName = settings.FileName;
		}

		if (string.IsNullOrEmpty(fileName))
			return false;

		// Save level
		try
		{
			Prj2Writer.SaveToPrj2(fileName, editor.Level);
			GC.Collect();
		}
		catch (Exception exc)
		{
			logger.Error(exc, "Unable to save to \"" + fileName + "\".");
			editor.SendMessage("There was an error while saving project file.\nException: " + exc.Message, PopupType.Error);
			return false;
		}

		// Update state
		if (editor.Level.Settings.LevelFilePath != fileName)
		{
			AddProjectToRecent(fileName);
			editor.Level.Settings.LevelFilePath = fileName;
			editor.LevelFileNameChange();
		}

		editor.HasUnsavedChanges = false;
		return true;
	}

	private static void AddProjectToRecent(string fileName)
	{
		Properties.Settings.Default.RecentProjects ??= [];

		Properties.Settings.Default.RecentProjects.RemoveAll(s => s == fileName);
		Properties.Settings.Default.RecentProjects.Insert(0, fileName);

		if (Properties.Settings.Default.RecentProjects.Count > 10)
			Properties.Settings.Default.RecentProjects.RemoveRange(10, Properties.Settings.Default.RecentProjects.Count - 10);

		Properties.Settings.Default.Save();
	}

	public static bool ConvertLevelToTombEngine(this Editor editor, INotifyPropertyChanged caller)
	{
		string? fileName = null;

		var settings = new OpenFileDialogSettings
		{
			Title = "Select project to convert",
			Filter = "Tomb Editor Level (*.prj2)|*.prj2|Room Edit Level (*.prj)|*.prj"
		};

		if (editor.DialogService.ShowOpenFileDialog(caller, settings) == true)
			fileName = settings.FileName;

		if (string.IsNullOrEmpty(fileName))
			return false;

		string newLevel = string.Empty;

		using (var form = new FormOperationDialog("TombEngine level converter", false, true, (progressReporter, cancelToken) =>
			newLevel = TombEngineConverter.Start(fileName, owner, progressReporter, cancelToken)))
		{
			if (form.ShowDialog(owner) != DialogResult.OK || string.IsNullOrEmpty(newLevel))
				return false;
			else
			{
				editor.OpenLevel(caller, newLevel);
				return true;
			}
		}
	}

	public static bool OpenLevel(this Editor editor, INotifyPropertyChanged caller, string? fileName = null, bool silent = false)
	{
		if (!editor.ContinueOnFileDrop(caller, "Open level"))
			return false;

		if (string.IsNullOrEmpty(fileName))
		{
			var settings = new OpenFileDialogSettings
			{
				Title = "Open Tomb Editor level",
				Filter = "Tomb Editor Level (*.prj2)|*.prj2"
			};

			if (editor.DialogService.ShowOpenFileDialog(caller, settings) == true)
				fileName = settings.FileName;
		}

		if (string.IsNullOrEmpty(fileName))
			return false;

		Level? newLevel = null;

		try
		{
			using (var form = new FormOperationDialog("Open level", true, true, (progressReporter, cancelToken) =>
				newLevel = Prj2Loader.LoadFromPrj2(fileName, progressReporter, cancelToken, new Prj2Loader.Settings())))
			{
				// Make sure form displays correctly if we're running in silent mode without parent window
				if (owner == null)
				{
					form.StartPosition = FormStartPosition.CenterScreen;
					form.ShowInTaskbar = true;
				}

				if (form.ShowDialog(owner) != DialogResult.OK || newLevel == null)
					return false;

				bool hasUnsavedChanges = false;

				// Check if the level has legacy sound system and should be loaded in early versions of TE
				if (newLevel.Settings.SoundSystem != SoundSystem.Xml)
				{
					DarkMessageBox.Show(owner, "This project is not compatible with this Tomb Editor version.\nUse version 1.3.15 or earlier and re-save this project in it.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					newLevel = null;
					return false;
				}

				if (!silent)
				{
					foreach (Room r in newLevel.ExistingRooms)
						r.RebuildLighting(editor.Configuration.Rendering3D_HighQualityLightPreview);

					AddProjectToRecent(fileName);
				}

				editor.Level = newLevel;
				newLevel = null;
				GC.Collect(); // Clean up memory
				editor.HasUnsavedChanges = hasUnsavedChanges;

				if (!silent && editor.Level.Settings.HasUnknownData)
					editor.SendMessage("This project was created in newer version of Tomb Editor.\nSome data was lost. Project is in read-only mode.", PopupType.Warning);

				return true;
			}
		}
		catch (Exception exc)
		{
			logger.Error(exc, "Unable to open \"" + fileName + "\"");

			if (exc is FileNotFoundException)
			{
				RemoveProjectFromRecent(fileName);
				if (!silent)
					editor.SendMessage("Project file not found!", PopupType.Warning);
				editor.LevelFileNameChange();  // Updates recent files on the main form
			}
			else if (!silent)
				editor.SendMessage("There was an error while opening project file. File may be in use or may be corrupted. \nException: " + exc.Message, PopupType.Error);
			return false;
		}
	}

	private static void RemoveProjectFromRecent(string fileName)
	{
		Properties.Settings.Default.RecentProjects.RemoveAll(s => s == fileName);
		Properties.Settings.Default.Save();
	}

	public static void OpenLevelPrj(this Editor editor, INotifyPropertyChanged caller, string? fileName = null)
	{
		if (!editor.ContinueOnFileDrop(caller, "Open level"))
			return;

		if (string.IsNullOrEmpty(fileName))
		{
			var settings = new OpenFileDialogSettings
			{
				Title = "Select PRJ to import",
				Filter = "Room Edit Level (*.prj)|*.prj"
			};

			if (editor.DialogService.ShowOpenFileDialog(caller, settings) == true)
				fileName = settings.FileName;

			if (string.IsNullOrEmpty(fileName))
				return;
		}

		using (var formImport = new FormImportPrj(fileName, editor.Configuration.Editor_RespectFlybyPatchOnPrjImport, editor.Configuration.Editor_UseHalfPixelCorrectionOnPrjImport))
		{
			if (formImport.ShowDialog(owner) != DialogResult.OK)
				return;

			Level newLevel = null;
			using (var form = new FormOperationDialog("Import PRJ", false, false, (progressReporter, cancelToken) =>
				newLevel = PrjLoader.LoadFromPrj(formImport.PrjPath, formImport.SoundsPath,
				formImport.RespectMousepatchOnFlybyHandling,
				formImport.UseHalfPixelCorrection,
				progressReporter, cancelToken)))
			{
				if (form.ShowDialog(owner) != DialogResult.OK || newLevel == null)
					return;

				foreach (Room r in newLevel.ExistingRooms)
					r.RebuildLighting(editor.Configuration.Rendering3D_HighQualityLightPreview);

				editor.Level = newLevel;
				newLevel = null;
				GC.Collect(); // Clean up memory
			}
		}
	}

	public static void EditSectorGeometry(this Editor editor, Room room, RectangleInt2 area, ArrowType arrow, BlockVertical vertical, int increment, bool smooth, bool oppositeDiagonalCorner = false, bool autoSwitchDiagonals = false, bool autoUpdateThroughPortal = true, bool disableUndo = false)
	{
		if (!disableUndo)
		{
			if (smooth)
				editor.UndoManager.PushGeometryChanged(editor.SelectedRoom.AndAdjoiningRooms);
			else
			{
				HashSet<Room> affectedRooms = room.GetAdjoiningRoomsFromArea(area);
				affectedRooms.Add(room);

				editor.UndoManager.PushGeometryChanged(affectedRooms);
			}
		}

		if (smooth)
		{
			// Scan selection and decide if the selected zone is wall-only, floor-only, or both.
			// It's needed to force smoothing function to edit either only wall sections or floor sections,
			// in case user wants to smoothly edit only wall splits or actual floor height.

			SmoothGeometryEditingType smoothEditingType = SmoothGeometryEditingType.None;

			for (int x = area.X0; x <= area.X1; x++)
			{
				for (int z = area.Y0; z <= area.Y1; z++)
				{
					if (smoothEditingType != SmoothGeometryEditingType.Wall && room.Blocks[x, z].Type == BlockType.Floor)
						smoothEditingType = SmoothGeometryEditingType.Floor;
					else if (smoothEditingType != SmoothGeometryEditingType.Floor && room.Blocks[x, z].Type != BlockType.Floor)
						smoothEditingType = SmoothGeometryEditingType.Wall;
					else
					{
						smoothEditingType = SmoothGeometryEditingType.Any;
						break;
					}
				}
				if (smoothEditingType == SmoothGeometryEditingType.Any)
					break;
			}

			VectorInt2 startCoord = area.Start;
			bool[] corners = [true, true, true, true];

			// Adjust editing area to exclude the side on which the arrow starts
			// This is a superset of the behaviour of the old editor to smooth edit a single edge or side.
			switch (arrow)
			{
				case ArrowType.EdgeE:
					area = new RectangleInt2(area.X0 + 1, area.Y0, area.X1, area.Y1);
					break;
				case ArrowType.EdgeN:
					area = new RectangleInt2(area.X0, area.Y0 + 1, area.X1, area.Y1);
					break;
				case ArrowType.EdgeW:
					area = new RectangleInt2(area.X0, area.Y0, area.X1 - 1, area.Y1);
					break;
				case ArrowType.EdgeS:
					area = new RectangleInt2(area.X0, area.Y0, area.X1, area.Y1 - 1);
					break;
				case ArrowType.CornerNE:
					area = new RectangleInt2(area.X0 + 1, area.Y0 + 1, area.X1, area.Y1);
					break;
				case ArrowType.CornerNW:
					area = new RectangleInt2(area.X0, area.Y0 + 1, area.X1 - 1, area.Y1);
					break;
				case ArrowType.CornerSW:
					area = new RectangleInt2(area.X0, area.Y0, area.X1 - 1, area.Y1 - 1);
					break;
				case ArrowType.CornerSE:
					area = new RectangleInt2(area.X0 + 1, area.Y0, area.X1, area.Y1 - 1);
					break;
			}

			void smoothEdit(RoomBlockPair pair, BlockEdge edge)
			{
				if (pair.Block == null) return;

				if (vertical.IsOnFloor() && (pair.Block.Floor.DiagonalSplit == DiagonalSplit.None || vertical.IsExtraFloorSubdivision()) ||
					vertical.IsOnCeiling() && (pair.Block.Ceiling.DiagonalSplit == DiagonalSplit.None || vertical.IsExtraCeilingSubdivision()))
				{
					if (smoothEditingType == SmoothGeometryEditingType.Any ||
					   !pair.Block.IsAnyWall && smoothEditingType == SmoothGeometryEditingType.Floor ||
						pair.Block.IsAnyWall && smoothEditingType == SmoothGeometryEditingType.Wall)
					{
						pair.Room.ChangeBlockHeight(pair.Pos.X, pair.Pos.Y, vertical, edge, increment);
						pair.Block.FixHeights(vertical);
					}
				}
			}

			var cornerBlocks = new RoomBlockPair[4]
			{
					room.GetBlockTryThroughPortal(area.X1 + 1, area.Y0 - 1),
					room.GetBlockTryThroughPortal(area.X0 - 1, area.Y0 - 1),
					room.GetBlockTryThroughPortal(area.X0 - 1, area.Y1 + 1),
					room.GetBlockTryThroughPortal(area.X1 + 1, area.Y1 + 1)
			};

			// Unique case of editing single corner
			if (area.Width == -1 && area.Height == -1 && arrow > ArrowType.EdgeW)
			{
				BlockEdge origin = BlockEdge.XnZn;
				switch (arrow)
				{
					case ArrowType.CornerNE: origin = BlockEdge.XpZp; break;
					case ArrowType.CornerNW: origin = BlockEdge.XnZp; break;
					case ArrowType.CornerSE: origin = BlockEdge.XpZn; break;
				}
				var originBlock = room.GetBlockTryThroughPortal(startCoord);
				var originHeight = originBlock.Block.GetHeight(vertical, origin) + originBlock.Room.Position.Y;
				for (int i = 0; i < 4; i++)
					corners[i] = originHeight == cornerBlocks[i].Block.GetHeight(vertical, (BlockEdge)i) + cornerBlocks[i].Room.Position.Y;
			}

			// Smoothly change sectors on the corners
			if (corners[0]) smoothEdit(cornerBlocks[0], BlockEdge.XnZp);
			if (corners[1]) smoothEdit(cornerBlocks[1], BlockEdge.XpZp);
			if (corners[2]) smoothEdit(cornerBlocks[2], BlockEdge.XpZn);
			if (corners[3]) smoothEdit(cornerBlocks[3], BlockEdge.XnZn);

			// Smoothly change sectors on the sides
			for (int x = area.X0; x <= area.X1; x++)
			{
				smoothEdit(room.GetBlockTryThroughPortal(x, area.Y0 - 1), BlockEdge.XnZp);
				smoothEdit(room.GetBlockTryThroughPortal(x, area.Y0 - 1), BlockEdge.XpZp);

				smoothEdit(room.GetBlockTryThroughPortal(x, area.Y1 + 1), BlockEdge.XnZn);
				smoothEdit(room.GetBlockTryThroughPortal(x, area.Y1 + 1), BlockEdge.XpZn);
			}

			for (int z = area.Y0; z <= area.Y1; z++)
			{
				smoothEdit(room.GetBlockTryThroughPortal(area.X0 - 1, z), BlockEdge.XpZp);
				smoothEdit(room.GetBlockTryThroughPortal(area.X0 - 1, z), BlockEdge.XpZn);

				smoothEdit(room.GetBlockTryThroughPortal(area.X1 + 1, z), BlockEdge.XnZp);
				smoothEdit(room.GetBlockTryThroughPortal(area.X1 + 1, z), BlockEdge.XnZn);
			}

			arrow = ArrowType.EntireFace;
		}

		for (int x = area.X0; x <= area.X1; x++)
			for (int z = area.Y0; z <= area.Y1; z++)
			{
				var targetBlock = room.Blocks[x, z];
				var targetRoom = room;
				var targetPos = new VectorInt2(x, z);

				var lookupBlock = room.GetBlockTryThroughPortal(x, z);

			EditBlock:
				{
					if (arrow == ArrowType.EntireFace)
					{
						if (vertical == BlockVertical.Floor || vertical == BlockVertical.Ceiling)
							targetRoom.RaiseBlockStepWise(targetPos.X, targetPos.Y, vertical, oppositeDiagonalCorner, increment, autoSwitchDiagonals);
						else
							targetRoom.RaiseBlock(targetPos.X, targetPos.Y, vertical, increment, false);
					}
					else
					{
						var currentSplit = vertical.IsOnFloor() ? targetBlock.Floor.DiagonalSplit : targetBlock.Ceiling.DiagonalSplit;
						var incrementInvalid = vertical.IsOnFloor() ? increment < 0 : increment > 0;
						BlockEdge[] corners = new BlockEdge[2] { BlockEdge.XnZp, BlockEdge.XnZp };
						DiagonalSplit[] splits = new DiagonalSplit[2] { DiagonalSplit.None, DiagonalSplit.None };

						switch (arrow)
						{
							case ArrowType.EdgeN:
							case ArrowType.CornerNW:
								corners[0] = BlockEdge.XnZp;
								corners[1] = BlockEdge.XpZp;
								splits[0] = DiagonalSplit.XpZn;
								splits[1] = arrow == ArrowType.CornerNW ? DiagonalSplit.XnZp : DiagonalSplit.XnZn;
								break;
							case ArrowType.EdgeE:
							case ArrowType.CornerNE:
								corners[0] = BlockEdge.XpZp;
								corners[1] = BlockEdge.XpZn;
								splits[0] = DiagonalSplit.XnZn;
								splits[1] = arrow == ArrowType.CornerNE ? DiagonalSplit.XpZp : DiagonalSplit.XnZp;
								break;
							case ArrowType.EdgeS:
							case ArrowType.CornerSE:
								corners[0] = BlockEdge.XpZn;
								corners[1] = BlockEdge.XnZn;
								splits[0] = DiagonalSplit.XnZp;
								splits[1] = arrow == ArrowType.CornerSE ? DiagonalSplit.XpZn : DiagonalSplit.XpZp;
								break;
							case ArrowType.EdgeW:
							case ArrowType.CornerSW:
								corners[0] = BlockEdge.XnZn;
								corners[1] = BlockEdge.XnZp;
								splits[0] = DiagonalSplit.XpZp;
								splits[1] = arrow == ArrowType.CornerSW ? DiagonalSplit.XnZn : DiagonalSplit.XpZn;
								break;
						}

						if (arrow <= ArrowType.EdgeW)
						{
							if (targetBlock.Type != BlockType.Wall && currentSplit != DiagonalSplit.None && vertical <= BlockVertical.Ceiling)
								continue;

							for (int i = 0; i < 2; i++)
								if (currentSplit != splits[i])
									targetRoom.ChangeBlockHeight(targetPos.X, targetPos.Y, vertical, corners[i], increment);
						}
						else
						{
							if (targetBlock.Type != BlockType.Wall && currentSplit != DiagonalSplit.None && vertical <= BlockVertical.Ceiling)
							{
								if (currentSplit == splits[1])
								{
									if (targetBlock.GetHeight(vertical, corners[0]) == targetBlock.GetHeight(vertical, corners[1]) && incrementInvalid)
										continue;
								}
								else if (autoSwitchDiagonals && currentSplit == splits[0] && targetBlock.GetHeight(vertical, corners[0]) == targetBlock.GetHeight(vertical, corners[1]) && !incrementInvalid)
									targetBlock.Transform(new RectTransformation { QuadrantRotation = 2 }, vertical.IsOnFloor());
								else
									continue;
							}
							targetRoom.ChangeBlockHeight(targetPos.X, targetPos.Y, vertical, corners[0], increment);
						}
					}
					targetBlock.FixHeights(vertical);
				}

				if (autoUpdateThroughPortal && lookupBlock.Block != targetBlock)
				{
					targetBlock = lookupBlock.Block;
					targetRoom = lookupBlock.Room;
					targetPos = lookupBlock.Pos;
					goto EditBlock;
				}

				// FIXME: VERY SLOW CODE! Since we need to update geometry in adjoining block through portal, and each block may contain portal to different room,
				// we need to find a way to quickly update geometry in all possible adjoining rooms in area. Until then, this function is used on per-sector basis.

				if (lookupBlock.Room != room)
					editor.CommitSmartBuildGeometry(lookupBlock.Room, new RectangleInt2(lookupBlock.Pos, lookupBlock.Pos));
			}

		editor.CommitSmartBuildGeometry(room, area);
	}
}
