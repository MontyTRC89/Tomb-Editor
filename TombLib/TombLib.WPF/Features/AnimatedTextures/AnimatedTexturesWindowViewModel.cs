#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLib;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.WPF.Controls;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombLib.WPF.Features.AnimatedTextures
{
    /// <summary>
    /// WPF port of <c>FormAnimatedTextures</c> (step 1: set management, frame grid, basic settings).
    /// Edits the host's live <see cref="AnimatedTextureSet"/> list; Cancel restores a backup clone.
    /// Procedural-animation generation and the animated preview follow in later steps.
    /// </summary>
    public partial class AnimatedTexturesWindowViewModel : ObservableObject
    {
        private readonly IAnimatedTexturesContext _context;
        private readonly TextureMapBase _textureMap;
        private readonly IAnimatedTextureMap? _animatedMap;
        private readonly IMessageService _messageService;
        private readonly ILocalizationService _localizationService;
        private readonly TRVersion.Game _version;
        private readonly List<AnimatedTextureSet> _sets;
        private readonly List<AnimatedTextureSet> _backupSets = new();

        private bool _lockUi;

        public TextureMapBase TextureMap => _textureMap;

        public ObservableCollection<AnimatedTextureSet> Sets { get; } = new();
        public ObservableCollection<AnimatedTextureFrame> Frames { get; } = new();
        public List<Texture> AvailableTextures => _context.AvailableTextures;
        public IReadOnlyList<AnimatedTextureAnimationType> AnimationTypes { get; }

        [ObservableProperty] private AnimatedTextureSet? _selectedSet;
        [ObservableProperty] private AnimatedTextureFrame? _selectedFrame;

        public event EventHandler<bool>? RequestClose; // bool = cancelled

        public AnimatedTexturesWindowViewModel(IAnimatedTexturesContext context, TextureMapBase textureMap)
        {
            _context = context;
            _textureMap = textureMap;
            _animatedMap = textureMap as IAnimatedTextureMap;
            _messageService = ServiceLocator.ResolveService<IMessageService>();
            _localizationService = ServiceLocator.ResolveService<ILocalizationService>().WithKeysFor(this);
            _version = context.Version;
            _sets = context.AnimatedTextureSets;

            ProceduralPresets = BuildProceduralPresets();
            AnimationTypes = BuildAnimationTypes();
            InitNgOptions();

            foreach (var set in _sets)
                _backupSets.Add(set.Clone());

            context.OnAnimatedTexturesChanged = OnAnimatedTexturesChanged;
            context.OnContextInvalidated = () => RequestClose?.Invoke(this, false);

            Frames.CollectionChanged += OnFramesCollectionChanged;

            SelectedPreset = ProceduralPresets.FirstOrDefault();

            RebuildSets();
            SelectedSet = Sets.FirstOrDefault();

            // Initialise the texture map.
            if (_context.SelectedTexture.TextureIsInvisible)
                _textureMap.ResetVisibleTexture(_context.AvailableTextures.FirstOrDefault());
            else
                _textureMap.ShowTexture(_context.SelectedTexture);

            _currentTexture = _textureMap.VisibleTexture ?? _context.SelectedTexture.Texture ?? _context.AvailableTextures.FirstOrDefault();
        }

        // Current visible texture (combo above the map).
        private Texture _currentTexture;
        public Texture CurrentTexture
        {
            get => _currentTexture;
            set
            {
                if (!SetProperty(ref _currentTexture, value) || value == null)
                    return;
                if (_textureMap.VisibleTexture != value)
                    _textureMap.ResetVisibleTexture(value);
            }
        }

        private IReadOnlyList<AnimatedTextureAnimationType> BuildAnimationTypes()
        {
            var types = new List<AnimatedTextureAnimationType> { AnimatedTextureAnimationType.Frames };

            if (_version.Native() is TRVersion.Game.TR4 or TRVersion.Game.TR5)
                types.Add(AnimatedTextureAnimationType.UVRotate);

            if (_version == TRVersion.Game.TombEngine)
            {
                types.Add(AnimatedTextureAnimationType.Video);
                types.Add(AnimatedTextureAnimationType.UVRotate);
            }

            if (_version == TRVersion.Game.TRNG)
                types.Add(AnimatedTextureAnimationType.PFrames);

            return types;
        }

        // Set list.

        private void RebuildSets()
        {
            // Sync the observable list IN PLACE (never Clear) so the combo keeps its selection — a Clear
            // would null SelectedSet and, if triggered during a Frames change, re-enter Frames.Clear().
            _lockUi = true;

            while (Sets.Count > _sets.Count)
                Sets.RemoveAt(Sets.Count - 1);

            for (int i = 0; i < Sets.Count; i++)
                if (!ReferenceEquals(Sets[i], _sets[i]))
                    Sets[i] = _sets[i];

            while (Sets.Count < _sets.Count)
                Sets.Add(_sets[Sets.Count]);

            _lockUi = false;
        }

        private void OnAnimatedTexturesChanged()
        {
            RebuildSets();
            if (SelectedSet == null)
                SelectedSet = Sets.LastOrDefault();
        }

        partial void OnSelectedSetChanged(AnimatedTextureSet? value)
        {
            // Ignore selection changes raised while we are rebuilding collections (prevents re-entrancy).
            if (_lockUi)
                return;

            _lockUi = true;
            Frames.Clear();
            if (value != null)
            {
                foreach (var frame in value.Frames)
                    Frames.Add(frame);
                _name = value.Name ?? string.Empty;
                _selectedAnimationType = value.AnimationType;
                _fps = value.Fps;
                LoadSettings(value);
            }
            else
            {
                _name = string.Empty;
            }
            _lockUi = false;

            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(SelectedAnimationType));
            OnPropertyChanged(nameof(Fps));
            OnPropertyChanged(nameof(HasSelectedSet));
            RefreshSettingsState();
            RefreshCommandStates();

            if (_animatedMap != null)
                _animatedMap.SelectedSet = value;
            _textureMap.InvalidateVisual();
        }

        public bool HasSelectedSet => SelectedSet != null;

        private void OnFramesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (_lockUi || SelectedSet == null)
                return;

            // A frame add/remove does not change the set list, so only refresh the map.
            // (Rebuilding the set combo here would re-enter this collection's CollectionChanged.)
            SelectedSet.Frames = Frames.ToList();
            _textureMap.InvalidateVisual();
        }

        // Editable set name (inline; replaces the WinForms "edit name" input box).

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (_lockUi || SelectedSet == null)
                {
                    SetProperty(ref _name, value);
                    return;
                }
                SetProperty(ref _name, value);
                SelectedSet.Name = value;
                RebuildSets();
            }
        }

        // Settings.

        private AnimatedTextureAnimationType _selectedAnimationType;
        public AnimatedTextureAnimationType SelectedAnimationType
        {
            get => _selectedAnimationType;
            set
            {
                if (!SetProperty(ref _selectedAnimationType, value) || _lockUi || SelectedSet == null)
                    return;
                SelectedSet.AnimationType = value;
                RefreshSettingsState();
                _context.OnAnimatedTexturesChanged?.Invoke();
            }
        }

        private double _fps = 16.0;
        public double Fps
        {
            get => _fps;
            set
            {
                if (!SetProperty(ref _fps, value) || _lockUi || SelectedSet == null)
                    return;
                SelectedSet.Fps = (float)value;
            }
        }

        // Commands.

        [RelayCommand]
        private void NewSet()
        {
            var set = new AnimatedTextureSet { Name = "Animation #" + _sets.Count };
            _sets.Add(set);
            RebuildSets();
            SelectedSet = set;
        }

        [RelayCommand]
        private void CloneSet()
        {
            if (SelectedSet == null)
                return;
            var clone = SelectedSet.Clone();
            clone.Name = (SelectedSet.Name ?? "Animation") + " (copy)";
            _sets.Add(clone);
            RebuildSets();
            SelectedSet = clone;
        }

        [RelayCommand]
        private void DeleteSet()
        {
            if (SelectedSet == null)
                return;

            if (SelectedSet.Frames.Count > 0 &&
                !_messageService.ShowConfirmation(_localizationService.Format("DeleteSetConfirm", SelectedSet)))
                return;

            int index = Sets.IndexOf(SelectedSet);
            _sets.Remove(SelectedSet);
            RebuildSets();
            SelectedSet = Sets.Count > 0 ? Sets[Math.Min(index, Sets.Count - 1)] : null;
        }

        [RelayCommand]
        private void AddFrame()
        {
            var frame = GetSelectedFrame();
            if (frame == null || SelectedSet == null)
            {
                if (frame != null && SelectedSet == null)
                    NewSet();
                if (frame == null)
                    return;
            }

            Frames.Add(frame);
            SelectedFrame = frame;
        }

        [RelayCommand]
        private void UpdateFrame()
        {
            var source = GetSelectedFrame();
            if (source == null || SelectedFrame == null)
                return;

            SelectedFrame.Texture = source.Texture;
            SelectedFrame.TexCoord0 = source.TexCoord0;
            SelectedFrame.TexCoord1 = source.TexCoord1;
            SelectedFrame.TexCoord2 = source.TexCoord2;
            SelectedFrame.TexCoord3 = source.TexCoord3;
            _context.OnAnimatedTexturesChanged?.Invoke();
        }

        [RelayCommand]
        private void DeleteFrame()
        {
            if (SelectedFrame == null)
                return;

            int index = Frames.IndexOf(SelectedFrame);
            Frames.Remove(SelectedFrame);
            SelectedFrame = Frames.Count > 0 ? Frames[Math.Min(index, Frames.Count - 1)] : null;
        }

        [RelayCommand]
        private void MoveFrameUp()
        {
            int index = SelectedFrame == null ? -1 : Frames.IndexOf(SelectedFrame);
            if (index > 0)
                Frames.Move(index, index - 1);
        }

        [RelayCommand]
        private void MoveFrameDown()
        {
            int index = SelectedFrame == null ? -1 : Frames.IndexOf(SelectedFrame);
            if (index >= 0 && index < Frames.Count - 1)
                Frames.Move(index, index + 1);
        }

        private AnimatedTextureFrame? GetSelectedFrame()
        {
            TextureArea area = _textureMap.SelectedTexture;
            if (area.Texture is not LevelTexture && area.Texture is not WadTexture)
            {
                _messageService.ShowError(_localizationService["NoValidTextureRegion"], _localizationService["InvalidSelection"]);
                return null;
            }

            return new AnimatedTextureFrame
            {
                Texture = area.Texture,
                TexCoord0 = area.TexCoord0,
                TexCoord1 = area.TexCoord1,
                TexCoord2 = area.TexCoord2,
                TexCoord3 = area.TexCoord3
            };
        }

        [RelayCommand]
        private void Ok() => RequestClose?.Invoke(this, false);

        [RelayCommand]
        private void Cancel() => RequestClose?.Invoke(this, true);

        public void Closing(bool cancelled)
        {
            if (cancelled)
            {
                _sets.Clear();
                foreach (var set in _backupSets)
                    _sets.Add(set);
            }
            _context.OnAnimatedTexturesChanged?.Invoke();
        }

        private void RefreshCommandStates()
        {
            CloneSetCommand.NotifyCanExecuteChanged();
            DeleteSetCommand.NotifyCanExecuteChanged();
        }
    }
}
