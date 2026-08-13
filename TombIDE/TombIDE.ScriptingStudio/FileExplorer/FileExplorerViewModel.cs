#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic.FileIO;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Interop;
using TombIDE.ScriptingStudio.Shell;
using TombIDE.Shared.SharedClasses;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.WPF.Services.Abstract;

namespace TombIDE.ScriptingStudio.FileExplorer;

public sealed partial class FileExplorerViewModel : ObservableObject, IDisposable
{
	private readonly IDialogService _dialogService;
	private readonly IMessageService _messageService;
	private readonly ILocalizationService _localizationService;
	private readonly IWin32DialogOwnerProvider _dialogOwnerProvider;
	private readonly FileSystemWatcher _fileSystemWatcher;

	private string _commentPrefix = string.Empty;
	private string _excludedDirectoryFilter = string.Empty;
	private string _filter = "*.*";
	private NotifyFilters _notifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
	private string _rootDirectoryPath = string.Empty;

	public FileExplorerViewModel(
		IDialogService dialogService,
		IMessageService messageService,
		ILocalizationService localizationService,
		IWin32DialogOwnerProvider dialogOwnerProvider)
	{
		ArgumentNullException.ThrowIfNull(dialogService);
		ArgumentNullException.ThrowIfNull(messageService);
		ArgumentNullException.ThrowIfNull(localizationService);
		ArgumentNullException.ThrowIfNull(dialogOwnerProvider);

		_dialogService = dialogService;
		_messageService = messageService;
		_localizationService = localizationService.WithKeysFor(this);
		_dialogOwnerProvider = dialogOwnerProvider;

		_fileSystemWatcher = new FileSystemWatcher
		{
			EnableRaisingEvents = false,
			Filter = "*.*",
			IncludeSubdirectories = true,
			NotifyFilter = _notifyFilter
		};

		_fileSystemWatcher.Changed += FileSystemWatcher_Changed;
		_fileSystemWatcher.Created += FileSystemWatcher_Created;
		_fileSystemWatcher.Deleted += FileSystemWatcher_Deleted;
		_fileSystemWatcher.Renamed += FileSystemWatcher_Renamed;
	}

	public ObservableCollection<FileExplorerItemViewModel> RootNodes { get; } = [];

	public string Title => _localizationService["Title"];

	public bool IsEmpty => RootNodes.Count == 0;

	public string CommentPrefix
	{
		get => _commentPrefix;
		set => SetProperty(ref _commentPrefix, value ?? string.Empty);
	}

	public string ExcludedDirectoryFilter
	{
		get => _excludedDirectoryFilter;
		set
		{
			if (!SetProperty(ref _excludedDirectoryFilter, value ?? string.Empty))
				return;

			UpdateFileList();
		}
	}

	public string Filter
	{
		get => _filter;
		set
		{
			if (!SetProperty(ref _filter, string.IsNullOrWhiteSpace(value) ? "*.*" : value))
				return;

			UpdateFileList();
		}
	}

	public NotifyFilters NotifyFilter
	{
		get => _notifyFilter;
		set
		{
			if (!SetProperty(ref _notifyFilter, value))
				return;

			_fileSystemWatcher.NotifyFilter = value;
		}
	}

	public string RootDirectoryPath
	{
		get => _rootDirectoryPath;
		set
		{
			if (!SetProperty(ref _rootDirectoryPath, value ?? string.Empty))
				return;

			UpdateWatcherState();
			UpdateFileList();

			CreateNewFileRequestedCommand.NotifyCanExecuteChanged();
			CreateNewFolderRequestedCommand.NotifyCanExecuteChanged();
		}
	}

	[ObservableProperty]
	[NotifyCanExecuteChangedFor(nameof(OpenSelectedItemCommand))]
	[NotifyCanExecuteChangedFor(nameof(OpenSourceViewCommand))]
	[NotifyCanExecuteChangedFor(nameof(RenameSelectedItemCommand))]
	[NotifyCanExecuteChangedFor(nameof(DeleteSelectedItemCommand))]
	[NotifyCanExecuteChangedFor(nameof(OpenInExplorerCommand))]
	private FileExplorerItemViewModel? _selectedItem;

	public event FileSystemEventHandler? FileChanged;
	public event FileSystemEventHandler? FileCreated;
	public event FileSystemEventHandler? FileDeleted;
	public event FileOpenedEventHandler? FileOpened;
	public event RenamedEventHandler? FileRenamed;

	public void Dispose()
	{
		_fileSystemWatcher.EnableRaisingEvents = false;
		_fileSystemWatcher.Changed -= FileSystemWatcher_Changed;
		_fileSystemWatcher.Created -= FileSystemWatcher_Created;
		_fileSystemWatcher.Deleted -= FileSystemWatcher_Deleted;
		_fileSystemWatcher.Renamed -= FileSystemWatcher_Renamed;
		_fileSystemWatcher.Dispose();
	}

	public string? CreateNewFile()
	{
		string? targetDirectory = GetTargetDirectoryPath();

		if (string.IsNullOrWhiteSpace(targetDirectory))
			return null;

		string suggestedName = "untitled" + GetDefaultFileExtension();

		while (true)
		{
			string? requestedName = PromptForName(_localizationService["NewFileTitle"], _localizationService["EnterFileName"], suggestedName);

			if (requestedName is null)
				return null;

			string finalName = EnsureDefaultExtension(requestedName);
			string filePath = Path.Combine(targetDirectory, finalName);

			if (File.Exists(filePath))
			{
				_messageService.ShowError(_localizationService["FileAlreadyExistsMessage"], _localizationService["ErrorTitle"]);
				suggestedName = requestedName;
				continue;
			}

			try
			{
				string relativePath = filePath.StartsWith(RootDirectoryPath, StringComparison.OrdinalIgnoreCase)
					? filePath[RootDirectoryPath.Length..]
					: filePath;

				File.WriteAllText(filePath, $"{CommentPrefix} FILE: {relativePath}{Environment.NewLine}");
				UpdateFileList();
				FileOpened?.Invoke(this, new FileOpenedEventArgs(filePath));
				return filePath;
			}
			catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
			{
				_messageService.ShowError(ex.Message, _localizationService["ErrorTitle"]);
				suggestedName = requestedName;
			}
		}
	}

	public void CreateNewFolder()
	{
		string? targetDirectory = GetTargetDirectoryPath();

		if (string.IsNullOrWhiteSpace(targetDirectory))
			return;

		string suggestedName = _localizationService["DefaultFolderName"];

		while (true)
		{
			string? requestedName = PromptForName(_localizationService["NewFolderTitle"], _localizationService["EnterFolderName"], suggestedName);

			if (requestedName is null)
				return;

			string folderPath = Path.Combine(targetDirectory, requestedName);

			if (Directory.Exists(folderPath))
			{
				_messageService.ShowError(_localizationService["FolderAlreadyExistsMessage"], _localizationService["ErrorTitle"]);
				suggestedName = requestedName;
				continue;
			}

			try
			{
				Directory.CreateDirectory(folderPath);
				UpdateFileList();
				return;
			}
			catch (Exception ex) when (ex is ArgumentException or IOException or NotSupportedException or UnauthorizedAccessException)
			{
				_messageService.ShowError(ex.Message, _localizationService["ErrorTitle"]);
				suggestedName = requestedName;
			}
		}
	}

	public void UpdateFileList()
	{
		string? selectedPath = SelectedItem?.FullPath;
		var expandedPaths = new HashSet<string>(EnumerateExpandedPaths(RootNodes), StringComparer.OrdinalIgnoreCase);

		RootNodes.Clear();

		if (string.IsNullOrWhiteSpace(RootDirectoryPath) || !Directory.Exists(RootDirectoryPath))
		{
			SelectedItem = null;
			OnPropertyChanged(nameof(IsEmpty));
			return;
		}

		var rootDirectory = new DirectoryInfo(RootDirectoryPath);
		FileExplorerItemViewModel rootNode = CreateDirectoryNode(rootDirectory, expandedPaths, true);
		RootNodes.Add(rootNode);
		OnPropertyChanged(nameof(IsEmpty));

		if (!string.IsNullOrWhiteSpace(selectedPath))
			SelectPath(selectedPath);
	}

	[RelayCommand(CanExecute = nameof(CanCreateItems))]
	private void CreateNewFileRequested()
		=> CreateNewFile();

	[RelayCommand(CanExecute = nameof(CanCreateItems))]
	private void CreateNewFolderRequested()
		=> CreateNewFolder();

	[RelayCommand(CanExecute = nameof(CanDeleteSelectedItem))]
	private void DeleteSelectedItem()
	{
		if (SelectedItem is null)
			return;

		string message = SelectedItem.IsDirectory
			? _localizationService.Format("DeleteFolderMessage", SelectedItem.Name)
			: _localizationService.Format("DeleteFileMessage", SelectedItem.Name);

		bool confirmed = _messageService.ShowConfirmation(
			message,
			_localizationService["DeleteTitle"],
			defaultValue: false,
			isRisky: true);

		if (!confirmed)
			return;

		try
		{
			if (SelectedItem.IsDirectory)
				FileSystem.DeleteDirectory(SelectedItem.FullPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
			else
				FileSystem.DeleteFile(SelectedItem.FullPath, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);

			UpdateFileList();
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			_messageService.ShowError(ex.Message, _localizationService["ErrorTitle"]);
		}
	}

	[RelayCommand(CanExecute = nameof(CanOpenInExplorer))]
	private void OpenInExplorer()
	{
		if (SelectedItem is null)
			return;

		SharedMethods.OpenInExplorer(SelectedItem.FullPath);
	}

	[RelayCommand(CanExecute = nameof(CanOpenSelectedItem))]
	private void OpenSelectedItem()
	{
		if (SelectedItem is null || SelectedItem.IsDirectory)
			return;

		FileOpened?.Invoke(this, new FileOpenedEventArgs(SelectedItem.FullPath));
	}

	[RelayCommand(CanExecute = nameof(CanOpenSelectedItem))]
	private void OpenSourceView()
	{
		if (SelectedItem is null || SelectedItem.IsDirectory)
			return;

		FileOpened?.Invoke(this, FileOpenedEventArgs.CreateSourceView(SelectedItem.FullPath));
	}

	[RelayCommand(CanExecute = nameof(CanRenameSelectedItem))]
	private void RenameSelectedItem()
	{
		if (SelectedItem is null)
			return;

		string initialName = SelectedItem.IsDirectory
			? Path.GetFileName(SelectedItem.FullPath)
			: Path.GetFileNameWithoutExtension(SelectedItem.FullPath);

		string? parentDirectory = Path.GetDirectoryName(SelectedItem.FullPath);

		if (string.IsNullOrWhiteSpace(parentDirectory))
			return;

		while (true)
		{
			string? requestedName = PromptForName(_localizationService["RenameTitle"], _localizationService["EnterNewName"], initialName);

			if (requestedName is null || string.Equals(requestedName, initialName, StringComparison.OrdinalIgnoreCase))
				return;

			string targetName = SelectedItem.IsDirectory
				? requestedName
				: requestedName + Path.GetExtension(SelectedItem.FullPath);

			string newPath = Path.Combine(parentDirectory, targetName);

			if (File.Exists(newPath) || Directory.Exists(newPath))
			{
				_messageService.ShowError(_localizationService["ItemAlreadyExistsMessage"], _localizationService["ErrorTitle"]);
				initialName = requestedName;
				continue;
			}

			try
			{
				if (SelectedItem.IsDirectory)
					Directory.Move(SelectedItem.FullPath, newPath);
				else
					File.Move(SelectedItem.FullPath, newPath);

				UpdateFileList();
				return;
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				_messageService.ShowError(ex.Message, _localizationService["ErrorTitle"]);
				initialName = requestedName;
			}
		}
	}

	private bool CanCreateItems()
		=> !string.IsNullOrWhiteSpace(RootDirectoryPath) && Directory.Exists(RootDirectoryPath);

	private bool CanDeleteSelectedItem()
		=> SelectedItem is not null && IsModifiableItem(SelectedItem);

	private bool CanOpenInExplorer()
		=> SelectedItem is not null;

	private bool CanOpenSelectedItem()
		=> SelectedItem?.IsFile is true && IsSupportedFileFormat(SelectedItem.FullPath);

	private bool CanRenameSelectedItem()
		=> SelectedItem is not null && IsModifiableItem(SelectedItem);

	private FileExplorerItemViewModel CreateDirectoryNode(DirectoryInfo directoryInfo, ISet<string> expandedPaths, bool isRoot = false)
	{
		var directoryNode = new FileExplorerItemViewModel(directoryInfo.Name, directoryInfo.FullName, true)
		{
			IsExpanded = isRoot || expandedPaths.Contains(directoryInfo.FullName)
		};

		foreach (DirectoryInfo childDirectory in GetDirectories(directoryInfo))
			directoryNode.Children.Add(CreateDirectoryNode(childDirectory, expandedPaths));

		foreach (FileInfo file in GetFiles(directoryInfo))
			directoryNode.Children.Add(new FileExplorerItemViewModel(file.Name, file.FullName, false));

		return directoryNode;
	}

	private string EnsureDefaultExtension(string fileName)
	{
		if (!string.IsNullOrEmpty(Path.GetExtension(fileName)))
			return fileName;

		string defaultExtension = GetDefaultFileExtension();
		return string.IsNullOrEmpty(defaultExtension)
			? fileName
			: fileName + defaultExtension;
	}

	private IEnumerable<string> EnumerateExpandedPaths(IEnumerable<FileExplorerItemViewModel> nodes)
	{
		foreach (FileExplorerItemViewModel node in nodes)
		{
			if (node.IsExpanded)
				yield return node.FullPath;

			foreach (string childPath in EnumerateExpandedPaths(node.Children))
				yield return childPath;
		}
	}

	private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
	{
		if (!ShouldHandlePath(e.FullPath))
			return;

		RunOnUiThread(() => FileChanged?.Invoke(this, e));
	}

	private void FileSystemWatcher_Created(object sender, FileSystemEventArgs e)
	{
		if (!ShouldHandlePath(e.FullPath))
			return;

		RunOnUiThread(() =>
		{
			UpdateFileList();
			FileCreated?.Invoke(this, e);
		});
	}

	private void FileSystemWatcher_Deleted(object sender, FileSystemEventArgs e)
	{
		if (!ShouldHandlePath(e.FullPath))
			return;

		RunOnUiThread(() =>
		{
			UpdateFileList();
			FileDeleted?.Invoke(this, e);
		});
	}

	private void FileSystemWatcher_Renamed(object sender, RenamedEventArgs e)
	{
		if (!ShouldHandlePath(e.FullPath) && !ShouldHandlePath(e.OldFullPath))
			return;

		RunOnUiThread(() =>
		{
			UpdateFileList();
			FileRenamed?.Invoke(this, e);
		});
	}

	private string GetDefaultFileExtension()
	{
		foreach (string pattern in GetFilterPatterns())
		{
			if (string.Equals(pattern, "*.*", StringComparison.Ordinal))
				return string.Empty;

			if (pattern.StartsWith("*.", StringComparison.Ordinal))
				return pattern[1..];
		}

		return string.Empty;
	}

	private IEnumerable<DirectoryInfo> GetDirectories(DirectoryInfo directoryInfo)
	{
		DirectoryInfo[] directories;

		try
		{
			directories = directoryInfo.GetDirectories();
		}
		catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
		{
			yield break;
		}

		foreach (DirectoryInfo directory in directories.OrderBy(static directory => directory.Name, StringComparer.OrdinalIgnoreCase))
		{
			if (IsExcludedDirectory(directory.FullName))
				continue;

			yield return directory;
		}
	}

	private IEnumerable<FileInfo> GetFiles(DirectoryInfo directoryInfo)
	{
		if (string.Equals(Filter, "*.*", StringComparison.Ordinal))
		{
			FileInfo[] allFiles;

			try
			{
				allFiles = directoryInfo.GetFiles();
			}
			catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
			{
				yield break;
			}

			foreach (FileInfo file in allFiles.OrderBy(static file => file.Name, StringComparer.OrdinalIgnoreCase))
				yield return file;

			yield break;
		}

		var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		foreach (string pattern in GetFilterPatterns())
		{
			FileInfo[] files;

			try
			{
				files = directoryInfo.GetFiles(pattern);
			}
			catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
			{
				continue;
			}

			foreach (FileInfo file in files.OrderBy(static file => file.Name, StringComparer.OrdinalIgnoreCase))
			{
				if (seenPaths.Add(file.FullName))
					yield return file;
			}
		}
	}

	private IEnumerable<string> GetFilterPatterns()
	{
		return (string.IsNullOrWhiteSpace(Filter) ? "*.*" : Filter)
			.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
	}

	private string? GetTargetDirectoryPath()
	{
		if (SelectedItem is null)
			return string.IsNullOrWhiteSpace(RootDirectoryPath) ? null : RootDirectoryPath;

		if (SelectedItem.IsDirectory)
			return SelectedItem.FullPath;

		return Path.GetDirectoryName(SelectedItem.FullPath);
	}

	private bool IsExcludedDirectory(string fullPath)
	{
		return !string.IsNullOrWhiteSpace(ExcludedDirectoryFilter)
			&& fullPath.EndsWith(ExcludedDirectoryFilter, StringComparison.OrdinalIgnoreCase);
	}

	private bool IsExcludedPath(string fullPath)
	{
		return !string.IsNullOrWhiteSpace(ExcludedDirectoryFilter)
			&& fullPath.Contains(ExcludedDirectoryFilter, StringComparison.OrdinalIgnoreCase);
	}

	private bool IsModifiableItem(FileExplorerItemViewModel item)
	{
		if (string.Equals(item.FullPath, RootDirectoryPath, StringComparison.OrdinalIgnoreCase))
			return false;

		if (!item.IsFile)
			return true;

		string scriptPath = Path.Combine(RootDirectoryPath, "script.txt");
		string defaultLanguagePath = Path.Combine(RootDirectoryPath, "english.txt");

		return !string.Equals(item.FullPath, scriptPath, StringComparison.OrdinalIgnoreCase)
			&& !string.Equals(item.FullPath, defaultLanguagePath, StringComparison.OrdinalIgnoreCase);
	}

	private bool IsSupportedFileFormat(string fullPath)
	{
		if (string.Equals(Filter, "*.*", StringComparison.Ordinal))
			return true;

		string extension = Path.GetExtension(fullPath);

		foreach (string pattern in GetFilterPatterns())
		{
			if (string.Equals(pattern, "*.*", StringComparison.Ordinal))
				return true;

			if (pattern.StartsWith("*.", StringComparison.Ordinal)
				&& string.Equals(extension, pattern[1..], StringComparison.OrdinalIgnoreCase))
				return true;
		}

		return false;
	}

	private string? PromptForName(string title, string label, string initialValue)
	{
		string currentValue = initialValue;

		while (true)
		{
			var inputBox = new InputBoxWindowViewModel(
				title: title,
				label: label,
				placeholder: currentValue);

			bool? dialogResult = ShowInputBoxDialog(inputBox);

			if (dialogResult is not true)
				return null;

			string sanitizedValue = PathHelper.RemoveIllegalPathSymbols(inputBox.Value).Trim();

			if (!string.IsNullOrWhiteSpace(sanitizedValue))
				return sanitizedValue;

			_messageService.ShowError(_localizationService["InvalidNameMessage"], _localizationService["ErrorTitle"]);
			currentValue = inputBox.Value;
		}
	}

	private bool? ShowInputBoxDialog(InputBoxWindowViewModel inputBox)
	{
		try
		{
			return _dialogService.ShowDialog(this, inputBox);
		}
		catch (ViewNotRegisteredException)
		{
			var window = new InputBoxWindow { DataContext = inputBox };
			PropertyChangedEventHandler? propertyChangedHandler = null;

			propertyChangedHandler = (_, e) =>
			{
				if (e.PropertyName == nameof(InputBoxWindowViewModel.DialogResult) && inputBox.DialogResult.HasValue)
					window.DialogResult = inputBox.DialogResult;
			};

			inputBox.PropertyChanged += propertyChangedHandler;

			try
			{
				if (_dialogOwnerProvider.GetOwner() is { } owner)
					new WindowInteropHelper(window).Owner = owner.Handle;

				return window.ShowDialog();
			}
			finally
			{
				inputBox.PropertyChanged -= propertyChangedHandler;
			}
		}
	}

	private void RunOnUiThread(Action action)
	{
		if (System.Windows.Application.Current?.Dispatcher is null || System.Windows.Application.Current.Dispatcher.CheckAccess())
		{
			action();
			return;
		}

		System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
	}

	private void SelectPath(string fullPath)
	{
		foreach (FileExplorerItemViewModel node in RootNodes)
			node.ClearSelection();

		foreach (FileExplorerItemViewModel node in RootNodes)
		{
			if (!node.TrySelectPath(fullPath))
				continue;

			SelectedItem = FindSelectedItem(node);
			return;
		}

		SelectedItem = null;
	}

	private bool ShouldHandlePath(string fullPath)
	{
		if (IsExcludedPath(fullPath))
			return false;

		if (Directory.Exists(fullPath))
			return true;

		return !Path.HasExtension(fullPath) || IsSupportedFileFormat(fullPath);
	}

	private void UpdateWatcherState()
	{
		_fileSystemWatcher.EnableRaisingEvents = false;

		if (string.IsNullOrWhiteSpace(RootDirectoryPath) || !Directory.Exists(RootDirectoryPath))
			return;

		_fileSystemWatcher.Path = RootDirectoryPath;
		_fileSystemWatcher.NotifyFilter = NotifyFilter;
		_fileSystemWatcher.EnableRaisingEvents = true;
	}

	private static FileExplorerItemViewModel? FindSelectedItem(FileExplorerItemViewModel node)
	{
		if (node.IsSelected)
			return node;

		foreach (FileExplorerItemViewModel child in node.Children)
		{
			FileExplorerItemViewModel? selectedItem = FindSelectedItem(child);

			if (selectedItem is not null)
				return selectedItem;
		}

		return null;
	}
}
