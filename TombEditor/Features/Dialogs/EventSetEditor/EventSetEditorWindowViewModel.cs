#nullable enable

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLib.LevelData;
using TombLib.LevelData.VisualScripting;
using TombLib.Utils;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    /// <summary>
    /// WPF port of <c>FormEventSetEditor</c> (step 1: event-set management shell + level-script mode).
    /// Edits the level's live global/volume <see cref="EventSet"/> list (Cancel restores a backup,
    /// mirroring the WinForms form). The per-event node graph is, for now, edited by the hosted WinForms
    /// <c>TriggerManager</c>; that area is replaced by a pure-WPF node editor in later steps.
    /// </summary>
    public partial class EventSetEditorWindowViewModel : ObservableObject
    {
        private readonly Editor _editor;
        private readonly List<EventSet> _usedList;
        private VolumeInstance? _instance;

        private readonly List<EventSet> _backupList;
        private readonly Dictionary<VolumeInstance, int>? _backupVolumes;
        private readonly bool[] _backupVolumeState = new bool[2];

        private readonly IMessageService _messageService;
        private readonly ILocalizationService _localizationService;

        private bool _lockUi;
        public bool Cancelled { get; private set; }

        public Editor Editor => _editor;
        public bool GlobalMode { get; }
        public bool GenericMode => GlobalMode || _instance == null;
        public bool ShowVolumeOptions => !GlobalMode;

        public List<NodeFunction> NodeFunctions => ScriptingUtils.NodeFunctions;
        public List<string> ScriptFunctions { get; }

        private readonly ArgumentDataProvider _argumentProvider;

        public ObservableCollection<EventSet> Sets { get; } = new();
        public IReadOnlyList<EventType> EventTypes { get; }

        public EventSetEditorWindowViewModel(Editor editor, bool global, VolumeInstance? instance = null)
        {
            _editor = editor;
            _messageService = ServiceLocator.ResolveService<IMessageService>();
            _localizationService = ServiceLocator.ResolveService<ILocalizationService>().WithKeysFor(this);

            _usedList = global ? editor.Level.Settings.GlobalEventSets : editor.Level.Settings.VolumeEventSets;
            _instance = instance;
            GlobalMode = global;

            EventTypes = global ? Event.GlobalEventTypes : Event.VolumeEventTypes;
            ScriptFunctions = ScriptingUtils.GetAllFunctionNames(editor.Level.Settings.MakeAbsolute(editor.Level.Settings.TenLuaScriptFile));
            _argumentProvider = new ArgumentDataProvider(editor, ScriptFunctions);

            // Backup for Cancel.
            _backupList = _usedList.Select(s => s.Clone()).ToList();
            if (!GlobalMode)
            {
                _backupVolumes = new Dictionary<VolumeInstance, int>();
                foreach (var vol in editor.Level.GetAllObjects().OfType<VolumeInstance>())
                    _backupVolumes.Add(vol, editor.Level.Settings.VolumeEventSets.IndexOf(vol.EventSet));
                if (_instance != null)
                {
                    _backupVolumeState[0] = _instance.Enabled;
                    _backupVolumeState[1] = _instance.DetectInAdjacentRooms;
                }
            }

            foreach (var set in _usedList)
                Sets.Add(set);

            if (!GenericMode)
                SelectedSet = _instance!.EventSet;
            else
                SelectedSet = Sets.FirstOrDefault();
        }

        public bool HasSelectedSet => SelectedSet != null;

        public string Title => GenericMode
            ? _localizationService[GlobalMode ? "TitleGlobal" : "TitleVolume"]
            : _localizationService.Format("TitleVolumeSpecific", _instance!.ToShortString());

        /// <summary>Follows the editor's 3D selection to another volume (mirrors FormEventSetEditor.ChangeVolume).</summary>
        public void FollowVolume(VolumeInstance? instance)
        {
            if (GlobalMode)
                return;

            _instance = instance;
            OnPropertyChanged(nameof(GenericMode));
            OnPropertyChanged(nameof(Title));
            OnPropertyChanged(nameof(EnableVolume));
            OnPropertyChanged(nameof(DetectInAdjacentRooms));
            SelectedSet = _instance?.EventSet ?? SelectedSet;
        }

        /// <summary>Rebuilds the set list after an external change (mirrors FormEventSetEditor on EventSetsChangedEvent).</summary>
        public void RepopulateSets()
        {
            if (_repopulating)
                return;

            _repopulating = true;
            var current = SelectedSet;

            _lockUi = true;
            Sets.Clear();
            foreach (var set in _usedList)
                Sets.Add(set);
            _lockUi = false;

            if (!GenericMode)
                SelectedSet = _instance?.EventSet;
            else
                SelectedSet = current != null && _usedList.Contains(current) ? current : Sets.FirstOrDefault();

            _repopulating = false;
        }

        private bool _repopulating;

        // Selection.

        [ObservableProperty] private EventSet? _selectedSet;
        [ObservableProperty] private EventType _selectedEventType;
        [ObservableProperty] private Event? _currentEvent;

        partial void OnSelectedSetChanged(EventSet? value)
        {
            if (!GenericMode && value != null)
                _instance!.EventSet = value;

            _lockUi = true;

            if (value != null)
            {
                _name = value.Name;
                OnPropertyChanged(nameof(Name));
                LoadActivators(value);
                SelectedEventType = value.LastUsedEvent;
                CurrentEvent = value.Events.TryGetValue(value.LastUsedEvent, out var evt) ? evt : null;
            }
            else
            {
                _name = string.Empty;
                OnPropertyChanged(nameof(Name));
                CurrentEvent = null;
            }

            _lockUi = false;
            OnPropertyChanged(nameof(HasSelectedSet));
            RefreshCommandStates();
        }

        partial void OnSelectedEventTypeChanged(EventType value)
        {
            if (_lockUi || SelectedSet == null)
                return;
            SelectedSet.LastUsedEvent = value;
            CurrentEvent = SelectedSet.Events.TryGetValue(value, out var evt) ? evt : null;
        }

        // Per-event editor (node graph + level-script modes).

        private const int NodeGridSize = 256;
        private const double NodeGridStep = 8.0;

        [ObservableProperty] private NodeEditorViewModel? _nodeEditor;

        public bool HasCurrentEvent => CurrentEvent != null;

        partial void OnCurrentEventChanged(Event? value)
        {
            NodeEditor = value != null
                ? new NodeEditorViewModel(value, NodeFunctions, _argumentProvider, NodeGridSize, NodeGridStep)
                : null;

            OnPropertyChanged(nameof(HasCurrentEvent));
            OnPropertyChanged(nameof(IsNodeEditorMode));
            OnPropertyChanged(nameof(IsLevelScriptMode));
            OnPropertyChanged(nameof(EventFunction));
            OnPropertyChanged(nameof(EventArgument));
        }

        public bool IsNodeEditorMode
        {
            get => CurrentEvent != null && CurrentEvent.Mode == EventSetMode.NodeEditor;
            set
            {
                if (CurrentEvent == null || !value)
                    return;
                CurrentEvent.Mode = EventSetMode.NodeEditor;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLevelScriptMode));
            }
        }

        public bool IsLevelScriptMode
        {
            get => CurrentEvent != null && CurrentEvent.Mode == EventSetMode.LevelScript;
            set
            {
                if (CurrentEvent == null || !value)
                    return;
                CurrentEvent.Mode = EventSetMode.LevelScript;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNodeEditorMode));
            }
        }

        public string EventFunction
        {
            get => CurrentEvent?.Function ?? string.Empty;
            set { if (CurrentEvent != null) { CurrentEvent.Function = value; OnPropertyChanged(); } }
        }

        public string EventArgument
        {
            get => CurrentEvent?.Argument ?? string.Empty;
            set { if (CurrentEvent != null) { CurrentEvent.Argument = value; OnPropertyChanged(); } }
        }

        // Name (with validation).

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_lockUi || SelectedSet == null || value == SelectedSet.Name)
                {
                    SetProperty(ref _name, value);
                    return;
                }

                if (string.IsNullOrEmpty(value))
                {
                    _messageService.ShowError(_localizationService["NameEmpty"]);
                    SetProperty(ref _name, SelectedSet.Name);
                    return;
                }

                if (_usedList.Any(s => s.Name == value))
                {
                    _messageService.ShowError(_localizationService["NameExists"]);
                    SetProperty(ref _name, SelectedSet.Name);
                    return;
                }

                EditorActions.ReplaceEventSetNames(_usedList, SelectedSet.Name, value);
                SelectedSet.Name = value;
                SetProperty(ref _name, value);
                RefreshSetsView();
            }
        }

        // Volume options.

        [ObservableProperty] private bool _activatorLara;
        [ObservableProperty] private bool _activatorNpc;
        [ObservableProperty] private bool _activatorOtherMoveables;
        [ObservableProperty] private bool _activatorStatics;
        [ObservableProperty] private bool _activatorFlyBy;

        private void LoadActivators(EventSet set)
        {
            if (set is not VolumeEventSet volumeSet)
                return;
            ActivatorLara = (volumeSet.Activators & VolumeActivators.Player) != 0;
            ActivatorNpc = (volumeSet.Activators & VolumeActivators.NPCs) != 0;
            ActivatorOtherMoveables = (volumeSet.Activators & VolumeActivators.OtherMoveables) != 0;
            ActivatorStatics = (volumeSet.Activators & VolumeActivators.Statics) != 0;
            ActivatorFlyBy = (volumeSet.Activators & VolumeActivators.Flybys) != 0;
        }

        partial void OnActivatorLaraChanged(bool value) => ModifyActivators();
        partial void OnActivatorNpcChanged(bool value) => ModifyActivators();
        partial void OnActivatorOtherMoveablesChanged(bool value) => ModifyActivators();
        partial void OnActivatorStaticsChanged(bool value) => ModifyActivators();
        partial void OnActivatorFlyByChanged(bool value) => ModifyActivators();

        private void ModifyActivators()
        {
            if (GlobalMode || _lockUi || SelectedSet is not VolumeEventSet volumeSet)
                return;

            volumeSet.Activators =
                (ActivatorLara ? VolumeActivators.Player : 0) |
                (ActivatorNpc ? VolumeActivators.NPCs : 0) |
                (ActivatorOtherMoveables ? VolumeActivators.OtherMoveables : 0) |
                (ActivatorStatics ? VolumeActivators.Statics : 0) |
                (ActivatorFlyBy ? VolumeActivators.Flybys : 0);
            UpdateVolume();
        }

        public bool EnableVolume
        {
            get => _instance?.Enabled ?? false;
            set { if (_instance != null) { _instance.Enabled = value; OnPropertyChanged(); UpdateVolume(); } }
        }
        public bool DetectInAdjacentRooms
        {
            get => _instance?.DetectInAdjacentRooms ?? false;
            set { if (_instance != null) { _instance.DetectInAdjacentRooms = value; OnPropertyChanged(); UpdateVolume(); } }
        }

        private void UpdateVolume()
        {
            if (_instance?.Room != null)
                _editor.ObjectChange(_instance, ObjectChangeType.Change);
        }

        // Commands.

        [RelayCommand]
        private void NewSet()
        {
            string name = "New " + (GlobalMode ? "global" : "volume") + " event set " + (Sets.Count + 1);
            EventSet newSet = GlobalMode
                ? new GlobalEventSet { Name = name, LastUsedEvent = Event.GlobalEventTypes[_editor.Configuration.NodeEditor_DefaultGlobalEventToEdit] }
                : new VolumeEventSet { Name = name, LastUsedEvent = Event.VolumeEventTypes[_editor.Configuration.NodeEditor_DefaultEventToEdit] };

            foreach (var evt in newSet.Events)
                evt.Value.Mode = (EventSetMode)_editor.Configuration.NodeEditor_DefaultEventMode;

            _usedList.Add(newSet);
            Sets.Add(newSet);
            SelectedSet = newSet;
        }

        [RelayCommand]
        private void CloneSet()
        {
            if (SelectedSet == null)
                return;
            var clone = SelectedSet.Clone();
            clone.Name = SelectedSet.Name + " (copy)";
            _usedList.Add(clone);
            Sets.Add(clone);
            SelectedSet = clone;
        }

        [RelayCommand]
        private void DeleteSet()
        {
            if (SelectedSet == null)
                return;

            int index = Sets.IndexOf(SelectedSet);
            var toDelete = SelectedSet;
            EditorActions.DeleteEventSet(toDelete);
            Sets.Remove(toDelete);

            if (Sets.Count > 0)
                SelectedSet = Sets[System.Math.Min(index, Sets.Count - 1)];
            else
                SelectedSet = null;
        }

        [RelayCommand]
        private void UnassignSet()
        {
            if (!GenericMode)
                SelectedSet = null;
        }

        [RelayCommand]
        private void Ok() => RequestClose?.Invoke(this, false);

        [RelayCommand]
        private void Cancel() => RequestClose?.Invoke(this, true);

        public event System.EventHandler<bool>? RequestClose; // bool = cancelled

        public void Closing(bool cancelled)
        {
            if (cancelled)
            {
                Cancelled = true;
                RestoreState();
            }
            _editor.EventSetsChange();
        }

        private void RestoreState()
        {
            if (GlobalMode)
            {
                _editor.Level.Settings.GlobalEventSets = _backupList;
            }
            else
            {
                _editor.Level.Settings.VolumeEventSets = _backupList;

                foreach (var vol in _editor.Level.GetAllObjects().OfType<VolumeInstance>())
                {
                    if (_backupVolumes != null && _backupVolumes.TryGetValue(vol, out int index) && index >= 0)
                        vol.EventSet = _backupList[index];
                }

                if (_instance != null)
                {
                    _instance.Enabled = _backupVolumeState[0];
                    _instance.DetectInAdjacentRooms = _backupVolumeState[1];
                }
            }
        }

        private void RefreshSetsView()
        {
            // Rebuild the list so the renamed item re-evaluates its display text, preserving selection.
            var selected = SelectedSet;
            var items = Sets.ToList();
            _lockUi = true;
            Sets.Clear();
            foreach (var item in items)
                Sets.Add(item);
            _lockUi = false;
            SelectedSet = selected;
        }

        private void RefreshCommandStates()
        {
            CloneSetCommand.NotifyCanExecuteChanged();
            DeleteSetCommand.NotifyCanExecuteChanged();
        }
    }
}
