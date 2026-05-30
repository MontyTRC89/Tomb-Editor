#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad.Catalog;
using TombLib.WPF;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.ReplaceObject;

public partial class ReplaceObjectWindowViewModel : ObservableObject, IDisposable
{
	public enum ObjectSelectionType { None, Source, Destination }

	private enum ObjectSearchType { PrimaryAttributeOnly = 0, Full = 1 }

	private const string SelectNewObjPrompt = " [ Select object in level or drag-n-drop it from item browser ]";

	private readonly Editor _editor;
	private readonly IMessageService _messageService;
	private PositionBasedObjectInstance? _source;
	private PositionBasedObjectInstance? _dest;

	[ObservableProperty] private string _sourceText = SelectNewObjPrompt;
	[ObservableProperty] private string _destText = string.Empty;
	[ObservableProperty] private ObjectSelectionType _selectionType = ObjectSelectionType.Source;
	[ObservableProperty] private bool _isSelectSourceEnabled = true;
	[ObservableProperty] private bool _isSelectDestEnabled = true;
	[ObservableProperty] private bool _isReplaceEnabled;
	[ObservableProperty] private bool _isSearchTypeEnabled;
	[ObservableProperty] private bool _isReplaceTypeEnabled;
	[ObservableProperty] private int _selectedSearchTypeIndex;
	[ObservableProperty] private int _selectedReplaceTypeIndex;
	[ObservableProperty] private Brush? _sourceColor;
	[ObservableProperty] private Brush? _destColor;
	[ObservableProperty] private bool _isSourceColorVisible;
	[ObservableProperty] private bool _isDestColorVisible;
	[ObservableProperty] private bool _selectedRoomsOnly = true;
	[ObservableProperty] private string _resultText = string.Empty;

	public ObservableCollection<string> SearchTypes { get; } = new();
	public ObservableCollection<string> ReplaceTypes { get; } = new();

	public PositionBasedObjectInstance? SourceObject => _source;
	public PositionBasedObjectInstance? DestObject => _dest;

	public ReplaceObjectWindowViewModel(
		Editor editor,
		bool fromContext = false,
		IMessageService? messageService = null,
		ILocalizationService? localizationService = null)
	{
		_editor = editor;
		_messageService = ServiceLocator.ResolveService(messageService);
		_ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

		_editor.EditorEventRaised += OnEditorEvent;

		InitializeNewSearch();

		if (fromContext)
		{
			_editor.SelectedObject = null;
			SelectionType = ObjectSelectionType.Destination;
		}
	}

	partial void OnSelectionTypeChanged(ObjectSelectionType value)
	{
		var selectDest = value == ObjectSelectionType.Destination;
		IsSelectDestEnabled = value == ObjectSelectionType.None || !selectDest;
		IsSelectSourceEnabled = value == ObjectSelectionType.None || selectDest;
		UpdateLabels();
	}

	private void OnEditorEvent(IEditorEvent obj)
	{
		if (obj is Editor.LevelChangedEvent)
			InitializeNewSearch();

		if (obj is Editor.SelectedObjectChangedEvent selChanged)
		{
			if (selChanged.Current is PositionBasedObjectInstance pbi)
				ToggleItem(pbi);
		}

		if (obj is Editor.LoadedImportedGeometriesChangedEvent)
		{
			if (_source is ImportedGeometryInstance src && !_editor.Level.Settings.ImportedGeometries.Contains(src.Model))
				InitializeNewSearch();
			else if (_dest is ImportedGeometryInstance dst && !_editor.Level.Settings.ImportedGeometries.Contains(dst.Model))
				InitializeNewSearch();
		}
	}

	public void ToggleItem(PositionBasedObjectInstance? item, ObjectSelectionType? overrideType = null)
	{
		if (item is not IReplaceable)
			return;

		var type = overrideType ?? SelectionType;

		switch (type)
		{
			case ObjectSelectionType.Destination:
				if (_source is not null && item.GetType() != _source.GetType())
				{
					InitializeNewSearch(false);
					ToggleItem(item, type);
				}
				else
				{
					SetDest(item);
				}
				break;

			case ObjectSelectionType.Source:
				if (_dest is not null && item.GetType() != _dest.GetType())
				{
					InitializeNewSearch(false);
					ToggleItem(item, type);
				}
				else
				{
					SetSource(item);
					RepopulateUI();
				}
				break;
		}
	}

	private void SetSource(PositionBasedObjectInstance? value)
	{
		_source = (PositionBasedObjectInstance?)value?.Clone();
		var desc = GetDescription(_source as IReplaceable);
		if (!string.IsNullOrEmpty(desc))
			SourceText = desc;
		UpdateUI();
	}

	private void SetDest(PositionBasedObjectInstance? value)
	{
		_dest = (PositionBasedObjectInstance?)value?.Clone();
		var desc = GetDescription(_dest as IReplaceable);
		if (!string.IsNullOrEmpty(desc))
			DestText = desc;
		UpdateUI();
	}

	private string GetDescription(IReplaceable? instance)
	{
		if (instance is null)
			return string.Empty;

		// Same hand-rolled formatting the WinForms FormReplaceObject used —
		// TR-catalog names are needed because ItemType.ToString() always returns TR4 names.
		string result;
		if (instance is MoveableInstance mov)
			result = TrCatalog.GetMoveableName(_editor.Level.Settings.GameVersion, mov.ItemType.MoveableId.TypeId);
		else if (instance is StaticInstance stat)
			result = TrCatalog.GetStaticName(_editor.Level.Settings.GameVersion, stat.ItemType.StaticId.TypeId);
		else
			result = ((ObjectInstance)instance).ToShortString();

		switch (instance)
		{
			case MoveableInstance m:
				result += $" ({m.SecondaryAttribDesc}: {m.Ocb})";
				break;
			case StaticInstance s when _editor.Level.IsNG:
				result += $" ({s.SecondaryAttribDesc}: {s.Ocb})";
				break;
			case SinkInstance snk:
				result += $" ({snk.PrimaryAttribDesc}: {snk.Strength + 1})";
				break;
			case ImportedGeometryInstance ig:
				result += $" ({ig.SecondaryAttribDesc}: {ig.Scale:.0#})";
				break;
			case SoundSourceInstance ss:
				result += $" ({ss.PrimaryAttribDesc}: {ss.SoundId})";
				break;
		}

		return result;
	}

	private void InitializeNewSearch(bool resetSelectionType = true)
	{
		SetSource(null);
		SetDest(null);
		if (resetSelectionType)
			SelectionType = ObjectSelectionType.Source;
		RepopulateUI(resetLabels: true);
	}

	private void RepopulateUI(bool resetLabels = false)
	{
		ResultText = string.Empty;
		if (resetLabels)
			UpdateLabels();

		SearchTypes.Clear();
		ReplaceTypes.Clear();

		var primary = ((IReplaceable?)_source)?.PrimaryAttribDesc ?? string.Empty;
		var secondary = ((IReplaceable?)_source)?.SecondaryAttribDesc ?? string.Empty;

		var options = new[]
		{
			string.IsNullOrEmpty(primary) ? string.Empty : "Only " + primary,
			primary + " and " + secondary
		};

		foreach (var opt in options)
		{
			SearchTypes.Add(opt);
			ReplaceTypes.Add(opt);
		}

		SelectedSearchTypeIndex = 0;
		SelectedReplaceTypeIndex = 0;
	}

	private void UpdateUI()
	{
		IsReplaceEnabled = _source is not null && _dest is not null;

		IsSearchTypeEnabled = !(_source is null
			|| _source is SinkInstance
			|| _source is SpriteInstance
			|| _source is SoundSourceInstance
			|| (_source is StaticInstance && _editor.Level.Settings.GameVersion != TRVersion.Game.TRNG));

		IsReplaceTypeEnabled = !(_source is null
			|| _source is SinkInstance
			|| _source is SpriteInstance
			|| _source is SoundSourceInstance
			|| _source is LightInstance
			|| (_source is StaticInstance && _editor.Level.Settings.GameVersion != TRVersion.Game.TRNG));

		if (_source is LightInstance srcLight)
		{
			SourceColor = new Vector4(srcLight.Color * 0.5f, 1.0f).ToWPFBrush();
			IsSourceColorVisible = true;
		}
		else
			IsSourceColorVisible = false;

		if (_dest is LightInstance dstLight)
		{
			DestColor = new Vector4(dstLight.Color * 0.5f, 1.0f).ToWPFBrush();
			IsDestColorVisible = true;
		}
		else
			IsDestColorVisible = false;
	}

	private void UpdateLabels()
	{
		if (SelectionType == ObjectSelectionType.Source)
		{
			if (_source is null) SourceText = SelectNewObjPrompt;
			if (_dest is null) DestText = string.Empty;
		}
		else if (SelectionType == ObjectSelectionType.Destination)
		{
			if (_dest is null) DestText = SelectNewObjPrompt;
			if (_source is null) SourceText = string.Empty;
		}
	}

	[RelayCommand]
	private void SelectSource() => SelectionType = ObjectSelectionType.Source;

	[RelayCommand]
	private void SelectDest() => SelectionType = ObjectSelectionType.Destination;

	[RelayCommand]
	private void NewSearch() => InitializeNewSearch();

	[RelayCommand]
	private void SwapSourceDest()
	{
		var tmp = _source;
		SetSource(_dest);
		SetDest(tmp);
	}

	[RelayCommand]
	private void Replace()
	{
		if (_source is null || _dest is null)
			return;

		int roomCount = 0;
		int replCount = 0;

		var replSrc = (IReplaceable)_source;
		var replDest = (IReplaceable)_dest;
		bool fullSearch = SelectedSearchTypeIndex == (int)ObjectSearchType.Full;
		bool fullReplace = SelectedReplaceTypeIndex == (int)ObjectSearchType.Full;

		var undoList = new List<UndoRedoInstance>();

		var rooms = SelectedRoomsOnly ? _editor.SelectedRooms : (IEnumerable<Room>)_editor.Level.Rooms;
		foreach (var room in rooms)
		{
			if (room is null)
				continue;
			bool anyObjectsChanged = false;

			var matches = room.Objects
				.Where(item => item is IReplaceable r && r.ReplaceableEquals(replSrc, fullSearch));

			foreach (IReplaceable obj in matches)
			{
				undoList.Add(new ChangeObjectPropertyUndoInstance(_editor.UndoManager, (PositionBasedObjectInstance)obj));

				bool changed = obj.Replace(replDest, fullReplace);
				if (changed)
				{
					replCount++;
					_editor.ObjectChange((ObjectInstance)obj, ObjectChangeType.Change);
					anyObjectsChanged = true;
				}
			}

			if (anyObjectsChanged)
			{
				roomCount++;
				if (_source is LightInstance)
					room.RebuildLighting(_editor.Configuration.Rendering3D_HighQualityLightPreview);
			}
		}

		if (replCount > 0)
		{
			_editor.UndoManager.Push(undoList);
			ResultText = $"Replacement finished. Replaced {replCount} objects in {roomCount} room{(roomCount > 1 ? "s" : string.Empty)}.";
		}
		else
			ResultText = "No matching objects found. No replacements were made.";
	}

	public void Dispose() => _editor.EditorEventRaised -= OnEditorEvent;
}
