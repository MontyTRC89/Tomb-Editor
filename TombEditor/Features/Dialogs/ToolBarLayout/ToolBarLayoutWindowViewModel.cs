#nullable enable

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.ToolBarLayout;

public partial class ToolBarLayoutWindowViewModel : ObservableObject, IModalDialogViewModel
{
	public const string SeparatorMarker = "|";
	public const string SeparatorDisplay = "[ Separator ]";

	private readonly Editor _editor;
	private readonly IReadOnlyList<string> _universe;

	[ObservableProperty] private bool? _dialogResult;
	[ObservableProperty] private string? _selectedAvailable;
	[ObservableProperty] private string? _selectedCurrent;

	public ObservableCollection<string> Available { get; } = new();
	public ObservableCollection<string> Current { get; } = new();

	public ToolBarLayoutWindowViewModel(
		Editor editor,
		IEnumerable<string> availableButtonNames,
		ILocalizationService? localizationService = null)
	{
		_editor = editor;
		_ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

		_universe = availableButtonNames.ToList();

		LoadFrom(editor.Configuration.UI_ToolbarButtons);
	}

	private void LoadFrom(IEnumerable<string> currentConfig)
	{
		Current.Clear();
		Available.Clear();

		var used = new HashSet<string>();

		foreach (var entry in currentConfig)
		{
			if (entry == SeparatorMarker)
				Current.Add(SeparatorDisplay);
			else if (_universe.Contains(entry))
			{
				Current.Add(entry);
				used.Add(entry);
			}
		}

		// Available always exposes the separator (can be added multiple times).
		Available.Add(SeparatorDisplay);
		foreach (var name in _universe)
			if (!used.Contains(name))
				Available.Add(name);
	}

	[RelayCommand]
	private void Add()
	{
		if (SelectedAvailable is null)
			return;

		// Insert at current position (or end). Separator stays in Available; others move.
		int insertAt = SelectedCurrent is null ? Current.Count : Current.IndexOf(SelectedCurrent);
		if (insertAt < 0)
			insertAt = Current.Count;

		Current.Insert(insertAt, SelectedAvailable);
		if (SelectedAvailable != SeparatorDisplay)
			Available.Remove(SelectedAvailable);
	}

	[RelayCommand]
	private void Remove()
	{
		if (SelectedCurrent is null)
			return;

		string item = SelectedCurrent;
		Current.Remove(item);
		if (item != SeparatorDisplay && !Available.Contains(item))
			Available.Add(item);
	}

	[RelayCommand]
	private void MoveUp()
	{
		if (SelectedCurrent is null)
			return;
		int idx = Current.IndexOf(SelectedCurrent);
		if (idx <= 0)
			return;
		Current.Move(idx, idx - 1);
	}

	[RelayCommand]
	private void MoveDown()
	{
		if (SelectedCurrent is null)
			return;
		int idx = Current.IndexOf(SelectedCurrent);
		if (idx < 0 || idx >= Current.Count - 1)
			return;
		Current.Move(idx, idx + 1);
	}

	[RelayCommand]
	private void RestoreDefaults() => LoadFrom(new Configuration().UI_ToolbarButtons);

	[RelayCommand]
	private void Apply()
	{
		_editor.Configuration.UI_ToolbarButtons = Current
			.Select(s => s == SeparatorDisplay ? SeparatorMarker : s)
			.ToArray();
		_editor.ConfigurationChange(false, false, true);
	}

	[RelayCommand]
	private void Ok()
	{
		Apply();
		DialogResult = true;
	}

	[RelayCommand]
	private void Cancel() => DialogResult = false;
}
