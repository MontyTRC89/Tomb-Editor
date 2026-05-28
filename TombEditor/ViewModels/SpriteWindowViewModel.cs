#nullable enable

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using TombLib.LevelData;
using TombLib.Wad.Catalog;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.ViewModels;

public partial class SpriteWindowViewModel : ObservableObject, IModalDialogViewModel
{
	private readonly SpriteInstance _instance;
	private readonly Editor _editor;
	private readonly IDialogService _dialogService;

	[ObservableProperty] private bool? _dialogResult;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(PreviewSpriteIndex))]
	private int _selectedSpriteIndex = -1;

	public ObservableCollection<string> Sprites { get; } = new();

	/// <summary>Index used by the WinForms preview panel. -1 → empty.</summary>
	public int PreviewSpriteIndex => SelectedSpriteIndex < 0 ? 0 : SelectedSpriteIndex;

	public SpriteWindowViewModel(
		SpriteInstance instance,
		IDialogService? dialogService = null,
		ILocalizationService? localizationService = null)
	{
		_instance = instance;
		_editor = Editor.Instance;
		_dialogService = ServiceLocator.ResolveService(dialogService);
		_ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

		_editor.EditorEventRaised += OnEditorEvent;
		Populate();
	}

	private void OnEditorEvent(IEditorEvent obj)
	{
		if (obj is Editor.LoadedWadsChangedEvent || obj is Editor.LevelChangedEvent)
			Populate();
	}

	private void Populate()
	{
		Sprites.Clear();

		var sequences = _editor.Level.Settings.WadGetAllSpriteSequences();
		var version = _editor.Level.Settings.GameVersion;

		foreach (var seq in sequences.Values)
		{
			string name = TrCatalog.GetSpriteSequenceName(version, seq.Id.TypeId);
			for (int i = 0; i < seq.Sprites.Count; i++)
				Sprites.Add($"{name}, Frame {i}");
		}

		SelectedSpriteIndex = _instance.SpriteID < Sprites.Count ? _instance.SpriteID : (Sprites.Count > 0 ? 0 : -1);
	}

	public void Detach() => _editor.EditorEventRaised -= OnEditorEvent;

	[RelayCommand]
	private void Confirm()
	{
		if (SelectedSpriteIndex >= 0)
			_instance.SetSequenceAndFrame(SelectedSpriteIndex);

		DialogResult = true;
		_dialogService.Close(this);
	}

	[RelayCommand]
	private void Cancel()
	{
		DialogResult = false;
		_dialogService.Close(this);
	}
}
