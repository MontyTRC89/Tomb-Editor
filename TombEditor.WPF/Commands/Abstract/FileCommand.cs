using MvvmDialogs.FrameworkDialogs.SaveFile;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using TombLib.Forms;
using TombLib.LevelData.IO;

namespace TombEditor.WPF.Commands;

internal class FileOpenCommand : FileCommand
{
	public FileOpenCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : base(caller, editor, logger)
	{ }

	public override void Execute(object? parameter)
	{

	}
}

internal class FileSaveCommand : FileCommand
{
	private readonly bool _askForPath;

	public FileSaveCommand(INotifyPropertyChanged caller, Editor editor, bool askForPath, Logger? logger = null) : base(caller, editor, logger)
		=> _askForPath = askForPath;

	public override void Execute(object? parameter)
		=> SaveLevel(_askForPath);
}

public abstract class FileCommand : UnconditionalEditorCommand
{
	protected FileCommand(INotifyPropertyChanged caller, Editor editor, Logger? logger = null) : base(caller, editor, logger)
	{ }

	protected bool ContinueOnFileDrop(string description)
	{
		if (!Editor.HasUnsavedChanges || Editor.Level.Settings.HasUnknownData)
			return true;

		MessageBoxResult result = Editor.DialogService.ShowMessageBox(Caller,
			"Your unsaved changes will be lost. Do you want to save?",
			description,
			MessageBoxButton.YesNoCancel,
			MessageBoxImage.Question,
			MessageBoxResult.No);

		return result switch
		{
			MessageBoxResult.No => true,
			MessageBoxResult.Yes => SaveLevel(false),
			_ => false,
		};
	}

	protected bool SaveLevel(bool askForPath)
	{
		// Disable saving if level has unknown data (i.e. new prj2 version opened in old editor version)
		if (Editor.Level.Settings.HasUnknownData)
		{
			Editor.SendMessage("Project is in read-only mode because it was created in newer version of Tomb Editor.\nUse newest Tomb Editor version to edit and save this project.", PopupType.Warning);
			return false;
		}

		string fileName = Editor.Level.Settings.LevelFilePath;

		// Show save dialog if necessary
		if (askForPath || string.IsNullOrEmpty(fileName))
		{
			var settings = new SaveFileDialogSettings
			{
				Title = "Save level",
				Filter = "Tomb Editor Level (*.prj2)|*.prj2|All Files (*.*)|*.*"
			};

			if (Editor.DialogService.ShowSaveFileDialog(Caller, settings) == true)
				fileName = settings.FileName;
		}

		if (string.IsNullOrEmpty(fileName))
			return false;

		// Save level
		try
		{
			Prj2Writer.SaveToPrj2(fileName, Editor.Level);
			GC.Collect();
		}
		catch (Exception exc)
		{
			Logger.Error(exc, "Unable to save to \"" + fileName + "\".");
			Editor.SendMessage("There was an error while saving project file.\nException: " + exc.Message, PopupType.Error);
			return false;
		}

		// Update state
		if (Editor.Level.Settings.LevelFilePath != fileName)
		{
			AddProjectToRecent(fileName);
			Editor.Level.Settings.LevelFilePath = fileName;
			Editor.LevelFileNameChange();
		}

		Editor.HasUnsavedChanges = false;
		return true;
	}

	private static void AddProjectToRecent(string fileName)
	{
		Properties.Settings.Default.RecentProjects ??= new List<string>();

		Properties.Settings.Default.RecentProjects.RemoveAll(s => s == fileName);
		Properties.Settings.Default.RecentProjects.Insert(0, fileName);

		if (Properties.Settings.Default.RecentProjects.Count > 10)
			Properties.Settings.Default.RecentProjects.RemoveRange(10, Properties.Settings.Default.RecentProjects.Count - 10);

		Properties.Settings.Default.Save();
	}
}
