#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;
using TombLib.Controls;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Wad;
using TombLib.Wad.Catalog;
using WadTool.Controls;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>One entry of the animation list.</summary>
public sealed partial class AnimListItem : ObservableObject
{
    [ObservableProperty] private string _label;
    public AnimationNode Node { get; }
    public AnimListItem(AnimationNode node, string label) { Node = node; _label = label; }
}

/// <summary>
/// WPF counterpart of the legacy <c>FormAnimationEditor</c> — built incrementally. The 3D view
/// (<see cref="PanelRenderingAnimationEditor"/>), the <see cref="AnimationTrackBar"/> timeline and the
/// <see cref="BezierCurveEditor"/> remain WinForms custom-rendered controls, hosted via
/// <c>WindowsFormsHost</c> and driven by this view model; the chrome is native WPF.
///
/// SLICE 1: animation list, read-only animation properties, playback and frame selection. Keyframe
/// editing, transforms, root motion, blending, sound and chained playback follow in later slices.
/// </summary>
public partial class AnimationEditorWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly DeviceManager _deviceManager;
    private readonly WadMoveableId _moveableId;
    private readonly AnimationEditor _editor;

    private PanelRenderingAnimationEditor? _panel;
    private AnimationTrackBar? _timeline;
    private BezierCurveEditor? _bezier;
    private DispatcherTimer? _playTimer;
    private bool _allowUpdate = true;
    private int _frameCount;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private AnimListItem? _selectedAnim;

    // Read-only animation properties (slice 1).
    [ObservableProperty] private string _animName = string.Empty;
    [ObservableProperty] private string _frameRate = string.Empty;
    [ObservableProperty] private string _endFrame = string.Empty;
    [ObservableProperty] private string _nextAnim = string.Empty;
    [ObservableProperty] private string _nextFrame = string.Empty;
    [ObservableProperty] private string _stateId = string.Empty;

    public ObservableCollection<AnimListItem> Animations { get; } = new();

    public AnimationEditorWindowViewModel(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMoveableId id)
    {
        _deviceManager = deviceManager;
        _moveableId = id;
        _editor = new AnimationEditor(tool, wad, id);
        _title = "Animation editor - " + _editor.Moveable.Id.ToString(_editor.Wad.GameVersion);
    }

    /// <summary>Called by the window once the hosted controls exist; mirrors the legacy ctor.</summary>
    public void AttachControls(PanelRenderingAnimationEditor panel, AnimationTrackBar timeline, BezierCurveEditor bezier)
    {
        _panel = panel;
        _timeline = timeline;
        _bezier = bezier;

        panel.Configuration = _editor.Tool.Configuration;

        WadMoveable skin;
        var skinId = new WadMoveableId(TrCatalog.GetMoveableSkin(_editor.Wad.GameVersion, _moveableId.TypeId));
        skin = _editor.Wad.Moveables.ContainsKey(skinId) ? _editor.Wad.Moveables[skinId] : _editor.Wad.Moveables[_moveableId];
        panel.InitializeRendering(_editor, _deviceManager, skin);

        _playTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
        _playTimer.Tick += (_, _) => OnPlayTick();

        _editor.Tool.EditorEventRaised += OnEditorEventRaised;

        RebuildAnimationsList();
        if (Animations.Count > 0)
            SelectedAnim = Animations[0];
    }

    public void Detach()
    {
        if (_playTimer is not null) _playTimer.Stop();
        _editor.Tool.EditorEventRaised -= OnEditorEventRaised;
    }

    private void OnEditorEventRaised(IEditorEvent obj)
    {
        if (obj is WadToolClass.AnimationEditorAnimationChangedEvent ||
            obj is WadToolClass.AnimationEditorCurrentAnimationChangedEvent ||
            obj is WadToolClass.AnimationEditorAnimcommandChangedEvent)
            _timeline?.Invalidate();

        if (obj is WadToolClass.MessageEvent message)
            PopUpInfo.Show(new PopUpInfo(), null, _panel, message.Message, message.Type);
    }

    private void RebuildAnimationsList()
    {
        Animations.Clear();
        foreach (var node in _editor.Animations)
            Animations.Add(new AnimListItem(node, "(" + node.Index + ") " + node.WadAnimation.Name));
    }

    partial void OnSelectedAnimChanged(AnimListItem? value) => SelectAnimation(value?.Node);

    private void SelectAnimation(AnimationNode? node)
    {
        if (_timeline is null || _panel is null)
            return;

        bool sameAnimSameSize = _editor.CurrentAnim != null && node != null && _editor.CurrentAnim.Index == node.Index &&
            _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == node.DirectXAnimation.KeyFrames.Count;

        _editor.CurrentAnim = node;

        _allowUpdate = false;
        if (node != null)
        {
            AnimName = node.WadAnimation.Name;
            FrameRate = node.WadAnimation.FrameRate.ToString();
            EndFrame = node.WadAnimation.EndFrame.ToString();
            NextAnim = node.WadAnimation.NextAnimation.ToString();
            NextFrame = node.WadAnimation.NextFrame.ToString();
            StateId = node.WadAnimation.StateId.ToString();
            if (_bezier is not null) _bezier.Value = node.WadAnimation.BlendCurve;
        }

        _timeline.Animation = node;

        if (_editor.ValidAnimationAndFrames)
        {
            _timeline.Minimum = 0;
            _timeline.Maximum = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;
        }

        if (!sameAnimSameSize)
        {
            if (!IsPlaying)
                _timeline.Value = 0;
            SelectFrame();
            _timeline.ResetSelection();
        }
        _allowUpdate = true;

        if (node?.DirectXAnimation.KeyFrames.Count > 0)
            _panel.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
        _panel.Invalidate();

        UpdateStatusLabel();
    }

    /// <summary>Called by the window when the timeline cursor changes.</summary>
    public void OnTimelineValueChanged() => SelectFrame();

    private void SelectFrame()
    {
        if (_panel is null || _timeline is null || !_editor.ValidAnimationAndFrames)
            return;

        int frameIndex = _timeline.Value;
        if (frameIndex >= _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count)
            frameIndex = _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1;

        _editor.CurrentFrameIndex = frameIndex;
        _panel.Model.BuildAnimationPose(_editor.CurrentAnim.DirectXAnimation.KeyFrames[frameIndex]);
        _panel.Invalidate();
        UpdateStatusLabel();
    }

    private void UpdateStatusLabel()
    {
        if (_timeline is null || _editor.CurrentAnim == null || _editor.CurrentAnim.DirectXAnimation.KeyFrames.Count == 0)
        {
            StatusText = string.Empty;
            return;
        }

        _frameCount = _timeline.Value * _editor.CurrentAnim.WadAnimation.FrameRate;
        StatusText = "Frame: " + _frameCount + " / " + Math.Max(_editor.GetRealNumberOfFrames() - 1, 0) +
                     "   Keyframe: " + _timeline.Value + " / " + Math.Max(_editor.CurrentAnim.DirectXAnimation.KeyFrames.Count - 1, 0);
    }

    private void OnPlayTick()
    {
        if (_timeline is null) return;
        if (_timeline.Value < _timeline.Maximum)
            _timeline.Value++;
        else
            _timeline.Value = _timeline.Minimum;
    }

    [RelayCommand]
    private void Play()
    {
        if (_playTimer is null) return;
        IsPlaying = !IsPlaying;
        if (IsPlaying) _playTimer.Start();
        else _playTimer.Stop();
    }

    [RelayCommand]
    private void GotoStart() { if (_timeline is not null) _timeline.Value = _timeline.Minimum; }
    [RelayCommand]
    private void GotoPrev() { if (_timeline is not null && _timeline.Value > _timeline.Minimum) _timeline.Value--; }
    [RelayCommand]
    private void GotoNext() { if (_timeline is not null && _timeline.Value < _timeline.Maximum) _timeline.Value++; }
    [RelayCommand]
    private void GotoEnd() { if (_timeline is not null) _timeline.Value = _timeline.Maximum; }

    [RelayCommand]
    private void Ok() => DialogResult = true;
    [RelayCommand]
    private void Cancel() => DialogResult = false;

    /// <summary>Saves on OK (legacy SaveChanges + WadChanged is done by the caller).</summary>
    public void HandleClosing()
    {
        if (_playTimer is not null) _playTimer.Stop();
        if (DialogResult == true)
            _editor.SaveChanges();
    }
}
