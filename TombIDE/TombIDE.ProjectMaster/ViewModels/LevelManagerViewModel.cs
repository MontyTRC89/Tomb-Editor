using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DarkUI.WPF.Extensions;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using TombIDE.ProjectMaster.Models;
using TombIDE.Shared.NewStructure;
using TombIDE.Shared.SharedClasses;

namespace TombIDE.ProjectMaster.ViewModels;

public partial class LevelManagerViewModel : ObservableObject
{
	// Level list
	[ObservableProperty] private ObservableCollection<LevelModel> _levels;
	[ObservableProperty] private LevelModel _selectedLevel;

	// Selected level properties
	[ObservableProperty] private bool? _isUsingMostRecentlyModifiedPrj2File;
	[ObservableProperty] private ObservableCollection<string> _availablePrj2Files;
	[ObservableProperty] private string _selectedPrj2File;
	[ObservableProperty] private bool _includeBackups;

	private IGameProject _gameProject;

	public LevelManagerViewModel(IGameProject project)
	{
		_gameProject = project;
		RepopulateLevelList();
	}

	[RelayCommand]
	private void RepopulateLevelList()
		=> Levels = _gameProject.GetAllValidLevelProjects()
			.Select(level => new LevelModel(level, _gameProject.LevelsDirectoryPath))
			.ToObservableCollection();

	[RelayCommand]
	private void AddLevel()
	{
		using var form = new FormLevelSetup(_gameProject);

		if (form.ShowDialog() == DialogResult.OK)
		{
			Levels.Add(new LevelModel(form.CreatedLevel, _gameProject.LevelsDirectoryPath));
		}
	}

	[RelayCommand]
	private void ImportLevel()
	{
		// Import a level project
		// ...
	}

	[RelayCommand]
	private void OpenLevel()
	{
		if (SelectedLevel is null)
			return;

		// Open the selected level project
		// ...
	}

	[RelayCommand]
	private void RebuildLevel()
	{
		if (SelectedLevel is null)
			return;

		// Rebuild the selected level project
		// ...
	}

	[RelayCommand]
	private void RenameLevel()
	{
		if (SelectedLevel is null)
			return;

		// Rename the selected level project
		// ...
	}

	[RelayCommand]
	private void RemoveLevel()
	{
		if (SelectedLevel is null)
			return;

		// Remove the selected level project
		// ...
	}

	[RelayCommand]
	private void MoveLevelUpOnList()
	{
		if (SelectedLevel is null)
			return;

		int currentIndex = Levels.IndexOf(SelectedLevel);

		if (currentIndex == 0)
			return;

		Levels.Move(currentIndex, currentIndex - 1);

		_gameProject.KnownLevelProjectFilePaths.Clear();

		foreach (LevelModel level in Levels)
			_gameProject.KnownLevelProjectFilePaths.Add(level.Base.DirectoryPath);

		// Save the changes
		_gameProject.Save();
	}

	[RelayCommand]
	private void MoveLevelDownOnList()
	{
		if (SelectedLevel is null)
			return;

		int currentIndex = Levels.IndexOf(SelectedLevel);

		if (currentIndex == Levels.Count - 1)
			return;

		Levels.Move(currentIndex, currentIndex + 1);

		_gameProject.KnownLevelProjectFilePaths.Clear();

		foreach (LevelModel level in Levels)
			_gameProject.KnownLevelProjectFilePaths.Add(level.Base.DirectoryPath);

		// Save the changes
		_gameProject.Save();
	}

	[RelayCommand]
	private void OpenLevelFolder()
	{
		if (SelectedLevel is null)
			return;

		SharedMethods.OpenInExplorer(SelectedLevel.Base.DirectoryPath);
	}

	partial void OnSelectedLevelChanged(LevelModel value)
	{
		if (value is null)
		{
			// Reset all properties
			IsUsingMostRecentlyModifiedPrj2File = null;
			AvailablePrj2Files = null;
			SelectedPrj2File = null;

			return;
		}

		SelectedPrj2File = value.Base.TargetPrj2FileName;

		bool useMostRecentPrj2File = value.Base.TargetPrj2FileName is null;

		if (useMostRecentPrj2File != IsUsingMostRecentlyModifiedPrj2File) // Value changed
		{
			IsUsingMostRecentlyModifiedPrj2File = useMostRecentPrj2File; // Trigger the property changed event
		}
		else if (!useMostRecentPrj2File)
		{
			// Update the list of available .prj2 files
			AvailablePrj2Files = value.Base.GetPrj2Files(IncludeBackups)
				.Select(file => file.Name).ToObservableCollection();
		}
	}

	partial void OnIsUsingMostRecentlyModifiedPrj2FileChanged(bool? value)
	{
		if (SelectedLevel is null)
			return;

		if (value is true or null)
		{
			// Clear the list of available .prj2 files
			AvailablePrj2Files = null;

			// Set the target .prj2 file to null
			SelectedLevel.Base.TargetPrj2FileName = null;
		}
		else if (value is false)
		{
			// Populate the list of available .prj2 files
			AvailablePrj2Files = SelectedLevel.Base.GetPrj2Files(IncludeBackups)
				.Select(file => file.Name).ToObservableCollection();
		}

		// Save the changes
		SelectedLevel.Base.Save();
		SelectedLevel.Update();
	}

	partial void OnSelectedPrj2FileChanged(string value)
	{
		if (SelectedLevel is null)
			return;

		// Set the target .prj2 file to the selected one
		SelectedLevel.Base.TargetPrj2FileName = value;

		// Save the changes
		SelectedLevel.Base.Save();
		SelectedLevel.Update();
	}

	partial void OnIncludeBackupsChanged(bool value)
	{
		if (SelectedLevel is null)
			return;

		// Update the list of available .prj2 files
		AvailablePrj2Files = SelectedLevel.Base.GetPrj2Files(value)
			.Select(file => file.Name).ToObservableCollection();
	}
}
