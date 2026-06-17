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
using TombLib.Types;
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

/// <summary>One entry of the bounding-mesh / bone list (legacy <c>dgvBoundingMeshList</c>).</summary>
public sealed partial class MeshBoneItem : ObservableObject
{
    [ObservableProperty] private bool _checked = true;
    public int Index { get; }
    public string Name { get; }
    public MeshBoneItem(int index, string name) { Index = index; Name = name; }
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
    [ObservableProperty] private bool _isTombEngine;
    [ObservableProperty] private AnimListItem? _selectedAnim;

    // Editable animation properties (slice 2).
    [ObservableProperty] private string _animName = string.Empty;
    [ObservableProperty] private double _frameRate;
    [ObservableProperty] private double _endFrame;
    [ObservableProperty] private double _nextAnimation;
    [ObservableProperty] private double _nextFrame;
    [ObservableProperty] private double _startVertVel;
    [ObservableProperty] private double _endVertVel;
    [ObservableProperty] private double _startHorVel;
    [ObservableProperty] private double _endHorVel;
    [ObservableProperty] private double _blendFrameCount;
    [ObservableProperty] private string _stateIdText = string.Empty;

    // Per-bone transform + bounding box (slice 2).
    [ObservableProperty] private string _transformHeader = "Transform";
    [ObservableProperty] private int _transformModeIndex;
    [ObservableProperty] private double _rotationX, _rotationY, _rotationZ;
    [ObservableProperty] private double _translationX, _translationY, _translationZ;
    [ObservableProperty] private double _bBoxMinX, _bBoxMinY, _bBoxMinZ, _bBoxMaxX, _bBoxMaxY, _bBoxMaxZ;
    [ObservableProperty] private MeshBoneItem? _selectedBoneItem;

    // Root motion (TEN), blend preset, transport toggles (slice 3a).
    [ObservableProperty] private bool _rootPosX, _rootPosY, _rootPosZ, _rootRotX, _rootRotY, _rootRotZ;
    [ObservableProperty] private int _blendPresetIndex = -1;
    [ObservableProperty] private bool _chainPlayback;
    [ObservableProperty] private bool _soundPreview;
    [ObservableProperty] private string _soundConditionLabel = "Land";

    public ObservableCollection<AnimListItem> Animations { get; } = new();
    public ObservableCollection<MeshBoneItem> Bones { get; } = new();

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

        IsTombEngine = _editor.Wad.GameVersion == TRVersion.Game.TombEngine;
        _allowUpdate = false;
        ChainPlayback = _editor.Tool.Configuration.AnimationEditor_ChainPlayback;
        SoundPreview = _editor.Tool.Configuration.AnimationEditor_SoundPreview;
        SoundConditionLabel = _editor.Tool.Configuration.AnimationEditor_SoundPreviewType.ToString();
        _allowUpdate = true;

        Bones.Clear();
        for (int i = 0; i < panel.Model.Bones.Count; i++)
            Bones.Add(new MeshBoneItem(i, panel.Model.Bones[i].Name));

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
        if (obj is WadToolClass.AnimationEditorMeshSelectedEvent ||
            obj is WadToolClass.AnimationEditorGizmoPickedEvent ||
            obj is WadToolClass.AnimationEditorAnimationChangedEvent ||
            obj is WadToolClass.AnimationEditorCurrentAnimationChangedEvent)
        {
            _editor.MadeChanges = false;
            UpdateTransformUI();
        }

        if (obj is WadToolClass.AnimationEditorAnimationChangedEvent ||
            obj is WadToolClass.AnimationEditorCurrentAnimationChangedEvent ||
            obj is WadToolClass.AnimationEditorAnimcommandChangedEvent)
            _timeline?.Invalidate();

        if (obj is WadToolClass.AnimationEditorMeshSelectedEvent meshSelected && _panel is not null)
        {
            // Sync the bone list selection to the 3D-picked mesh.
            int index = meshSelected.Mesh is null ? -1 : meshSelected.Model.Meshes.IndexOf(meshSelected.Mesh);
            var item = Bones.FirstOrDefault(b => b.Index == index);
            if (item is not null && !ReferenceEquals(item, SelectedBoneItem))
            {
                bool prev = _allowUpdate;
                _allowUpdate = false; // avoid re-driving panel selection
                SelectedBoneItem = item;
                _allowUpdate = prev;
            }
        }

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

        _editor.MadeChanges = false;
        _allowUpdate = false;
        if (node != null)
        {
            AnimName = node.WadAnimation.Name;
            FrameRate = node.WadAnimation.FrameRate;
            EndFrame = node.WadAnimation.EndFrame;
            NextAnimation = node.WadAnimation.NextAnimation;
            NextFrame = node.WadAnimation.NextFrame;
            StartVertVel = node.WadAnimation.StartVelocity;
            EndVertVel = node.WadAnimation.EndVelocity;
            StartHorVel = node.WadAnimation.StartLateralVelocity;
            EndHorVel = node.WadAnimation.EndLateralVelocity;
            BlendFrameCount = node.WadAnimation.BlendFrameCount;
            StateIdText = node.WadAnimation.StateId.ToString();
            if (_bezier is not null) _bezier.Value = node.WadAnimation.BlendCurve;
            BlendPresetIndex = -1;

            RootPosX = node.WadAnimation.RootMotion.TranslationX;
            RootPosY = node.WadAnimation.RootMotion.TranslationY;
            RootPosZ = node.WadAnimation.RootMotion.TranslationZ;
            RootRotX = node.WadAnimation.RootMotion.RotationX;
            RootRotY = node.WadAnimation.RootMotion.RotationY;
            RootRotZ = node.WadAnimation.RootMotion.RotationZ;
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
        UpdateTransformUI();
        UpdateStatusLabel();
    }

    /// <summary>Reads the current keyframe's per-bone rotation/translation + bounding box into the UI (legacy UpdateTransformUI).</summary>
    private void UpdateTransformUI()
    {
        if (_panel is null || _editor.CurrentKeyFrame == null)
            return;

        _allowUpdate = false;
        int meshIndex = _panel.SelectedMesh == null ? 0 : _panel.Model.Meshes.IndexOf(_panel.SelectedMesh);

        RotationX = TombLib.MathC.RadToDeg(_editor.CurrentKeyFrame.Rotations[meshIndex].X);
        RotationY = TombLib.MathC.RadToDeg(_editor.CurrentKeyFrame.Rotations[meshIndex].Y);
        RotationZ = TombLib.MathC.RadToDeg(_editor.CurrentKeyFrame.Rotations[meshIndex].Z);
        TranslationX = _editor.CurrentKeyFrame.Translations[0].X;
        TranslationY = _editor.CurrentKeyFrame.Translations[0].Y;
        TranslationZ = _editor.CurrentKeyFrame.Translations[0].Z;
        if (TransformModeIndex < 0) TransformModeIndex = 0;

        BBoxMinX = _editor.CurrentKeyFrame.BoundingBox.Minimum.X;
        BBoxMinY = _editor.CurrentKeyFrame.BoundingBox.Minimum.Y;
        BBoxMinZ = _editor.CurrentKeyFrame.BoundingBox.Minimum.Z;
        BBoxMaxX = _editor.CurrentKeyFrame.BoundingBox.Maximum.X;
        BBoxMaxY = _editor.CurrentKeyFrame.BoundingBox.Maximum.Y;
        BBoxMaxZ = _editor.CurrentKeyFrame.BoundingBox.Maximum.Z;

        var boneName = _panel.Model.Bones[meshIndex].Name;
        if (string.IsNullOrEmpty(boneName)) boneName = "Bone " + meshIndex;
        TransformHeader = "Transform (" + boneName + ")";
        _allowUpdate = true;
    }

    /// <summary>Called by the window when the timeline selection changes (drives bounding-box range edits).</summary>
    public void OnTimelineSelectionChanged()
    {
        if (_timeline is null) return;
        _editor.Selection = _timeline.SelectionIsEmpty ? new TombLib.VectorInt2(-1, -1) : _timeline.Selection;
        UpdateStatusLabel();
    }

    private bool ValidAnim() => _allowUpdate && _editor.ValidAnimationAndFrames;

    private void PushUndoOnce()
    {
        if (!_editor.MadeChanges)
        {
            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _editor.CurrentAnim);
            _editor.MadeChanges = true;
        }
    }

    private void AfterParameterChange()
    {
        _timeline?.Invalidate();
        _panel?.Invalidate();
    }

    // ---- Editable animation properties ----

    partial void OnAnimNameChanged(string value)
    {
        if (!_allowUpdate || _editor.CurrentAnim == null) return;
        var newName = value.Trim();
        if (string.IsNullOrEmpty(newName) || newName == _editor.CurrentAnim.WadAnimation.Name) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.Name = newName;
        if (SelectedAnim is not null) SelectedAnim.Label = "(" + _editor.CurrentAnim.Index + ") " + newName;
    }

    partial void OnFrameRateChanged(double value)
    {
        if (!ValidAnim() || (byte)value == _editor.CurrentAnim.WadAnimation.FrameRate) return;
        PushUndoOnce();
        byte rate = (byte)TombLib.MathC.Clamp((float)value, 1, 255);
        _allowUpdate = false;
        EndFrame = Math.Round(_editor.CurrentAnim.WadAnimation.EndFrame / (_editor.CurrentAnim.WadAnimation.FrameRate / (float)rate));
        _allowUpdate = true;
        _editor.CurrentAnim.WadAnimation.FrameRate = rate;
        _editor.CurrentAnim.WadAnimation.EndFrame = (ushort)EndFrame;
        AfterParameterChange();
    }

    partial void OnEndFrameChanged(double value)
    {
        if (!ValidAnim() || (ushort)value == _editor.CurrentAnim.WadAnimation.EndFrame) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.EndFrame = (ushort)value;
        AfterParameterChange();
    }

    partial void OnNextAnimationChanged(double value)
    {
        if (!ValidAnim() || (ushort)value == _editor.CurrentAnim.WadAnimation.NextAnimation) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.NextAnimation = (ushort)value;
        AfterParameterChange();
    }

    partial void OnNextFrameChanged(double value)
    {
        if (!ValidAnim() || (ushort)value == _editor.CurrentAnim.WadAnimation.NextFrame) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.NextFrame = (ushort)value;
        AfterParameterChange();
    }

    partial void OnStartVertVelChanged(double value) => SetVelocity(v => _editor.CurrentAnim.WadAnimation.StartVelocity = v, _editor.CurrentAnim?.WadAnimation.StartVelocity, value);
    partial void OnEndVertVelChanged(double value) => SetVelocity(v => _editor.CurrentAnim.WadAnimation.EndVelocity = v, _editor.CurrentAnim?.WadAnimation.EndVelocity, value);
    partial void OnStartHorVelChanged(double value) => SetVelocity(v => _editor.CurrentAnim.WadAnimation.StartLateralVelocity = v, _editor.CurrentAnim?.WadAnimation.StartLateralVelocity, value);
    partial void OnEndHorVelChanged(double value) => SetVelocity(v => _editor.CurrentAnim.WadAnimation.EndLateralVelocity = v, _editor.CurrentAnim?.WadAnimation.EndLateralVelocity, value);

    private void SetVelocity(Action<float> setter, float? current, double value)
    {
        if (!ValidAnim() || current is null || Math.Abs(current.Value - (float)value) < 1e-6f) return;
        PushUndoOnce();
        setter((float)value);
        AfterParameterChange();
    }

    partial void OnBlendFrameCountChanged(double value)
    {
        if (!ValidAnim() || (ushort)value == _editor.CurrentAnim.WadAnimation.BlendFrameCount) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.BlendFrameCount = (ushort)value;
        AfterParameterChange();
    }

    /// <summary>Commits the state-id text box (legacy UpdateStateChange). Called by the window on commit.</summary>
    public void CommitStateId()
    {
        if (!_allowUpdate || _editor.CurrentAnim == null) return;

        ushort oldValue = _editor.CurrentAnim.WadAnimation.StateId;
        if (!ushort.TryParse(StateIdText.Trim(), out ushort newValue))
        {
            string searchString = StateIdText.Trim();
            int possibleID = -1;
            for (int i = 0; i < 2; i++)
            {
                possibleID = TrCatalog.TryToGetStateID(_editor.Wad.GameVersion, _editor.Moveable.Id.TypeId, searchString);
                if (possibleID >= 0) break;
                searchString = searchString.Replace(' ', '_');
            }
            if (possibleID < 0)
                return;
            newValue = (ushort)possibleID;
        }

        var possibleName = TrCatalog.GetStateName(_editor.Wad.GameVersion, _editor.Moveable.Id.TypeId, newValue);
        _allowUpdate = false;
        StateIdText = possibleName.Contains("Unknown") ? newValue.ToString() : possibleName;
        _allowUpdate = true;

        if (oldValue == newValue) return;
        PushUndoOnce();
        _editor.CurrentAnim.WadAnimation.StateId = newValue;
    }

    // ---- Bounding box ----

    partial void OnBBoxMinXChanged(double value) => SetBoundingBox();
    partial void OnBBoxMinYChanged(double value) => SetBoundingBox();
    partial void OnBBoxMinZChanged(double value) => SetBoundingBox();
    partial void OnBBoxMaxXChanged(double value) => SetBoundingBox();
    partial void OnBBoxMaxYChanged(double value) => SetBoundingBox();
    partial void OnBBoxMaxZChanged(double value) => SetBoundingBox();

    private void SetBoundingBox()
    {
        if (_timeline is null || !ValidAnim()) return;
        var bb = _editor.CurrentAnim.DirectXAnimation.KeyFrames[_timeline.Value].BoundingBox;
        var newMin = new System.Numerics.Vector3((float)BBoxMinX, (float)BBoxMinY, (float)BBoxMinZ);
        var newMax = new System.Numerics.Vector3((float)BBoxMaxX, (float)BBoxMaxY, (float)BBoxMaxZ);
        if (newMin == bb.Minimum && newMax == bb.Maximum) return;

        PushUndoOnce();
        var deltaMin = newMin - bb.Minimum;
        var deltaMax = newMax - bb.Maximum;

        int start = _timeline.Value, end = _timeline.Value;
        if (!_editor.SelectionIsEmpty) { start = _editor.Selection.X; end = _editor.Selection.Y; }

        for (int i = start; i <= end; i++)
        {
            var bb2 = _editor.CurrentAnim.DirectXAnimation.KeyFrames[i].BoundingBox;
            bb2.Minimum += deltaMin;
            bb2.Maximum += deltaMax;
            _editor.CurrentAnim.DirectXAnimation.KeyFrames[i].BoundingBox = bb2;
        }
        AfterParameterChange();
    }

    // ---- Per-bone transform ----

    partial void OnTransformModeIndexChanged(int value)
    {
        _editor.TransformMode = (AnimTransformMode)value;
        if (_allowUpdate && _editor.MadeChanges) UpdateTransform();
    }

    partial void OnRotationXChanged(double value) => UpdateTransform();
    partial void OnRotationYChanged(double value) => UpdateTransform();
    partial void OnRotationZChanged(double value) => UpdateTransform();
    partial void OnTranslationXChanged(double value) => UpdateTransform();
    partial void OnTranslationYChanged(double value) => UpdateTransform();
    partial void OnTranslationZChanged(double value) => UpdateTransform();

    private void UpdateTransform()
    {
        if (_panel is null || !_allowUpdate || _editor.CurrentKeyFrame == null)
            return;

        PushUndoOnce();
        int meshIndex = _panel.SelectedMesh == null ? 0 : _panel.Model.Meshes.IndexOf(_panel.SelectedMesh);
        _editor.UpdateTransform(meshIndex,
            new System.Numerics.Vector3(TombLib.MathC.DegToRad((float)RotationX), TombLib.MathC.DegToRad((float)RotationY), TombLib.MathC.DegToRad((float)RotationZ)),
            new System.Numerics.Vector3((float)TranslationX, (float)TranslationY, (float)TranslationZ));
        _panel.Model.BuildAnimationPose(_editor.CurrentKeyFrame);
        _panel.Invalidate();
    }

    // ---- Bone / mesh selection ----

    partial void OnSelectedBoneItemChanged(MeshBoneItem? value)
    {
        if (!_allowUpdate || _panel is null || value is null) return;
        _panel.SelectedMesh = value.Index < 0 ? null : _panel.Model.Meshes[value.Index];
        UpdateTransformUI();
        _panel.Invalidate();
    }

    // ---- Root motion (TEN) ----

    partial void OnRootPosXChanged(bool value) => ApplyRootMotion();
    partial void OnRootPosYChanged(bool value) => ApplyRootMotion();
    partial void OnRootPosZChanged(bool value) => ApplyRootMotion();
    partial void OnRootRotXChanged(bool value) => ApplyRootMotion();
    partial void OnRootRotYChanged(bool value) => ApplyRootMotion();
    partial void OnRootRotZChanged(bool value) => ApplyRootMotion();

    private void ApplyRootMotion()
    {
        if (!_allowUpdate || _editor.CurrentAnim == null) return;
        PushUndoOnce();
        var rootMotion = _editor.CurrentAnim.WadAnimation.RootMotion;
        rootMotion.TranslationX = RootPosX;
        rootMotion.TranslationY = RootPosY;
        rootMotion.TranslationZ = RootPosZ;
        rootMotion.RotationX = RootRotX;
        rootMotion.RotationY = RootRotY;
        rootMotion.RotationZ = RootRotZ;
        _editor.CurrentAnim.WadAnimation.RootMotion = rootMotion;
    }

    // ---- Blend curve preset ----

    partial void OnBlendPresetIndexChanged(int value)
    {
        if (_bezier is null || value < 0) return;
        _bezier.Value.Set(value switch
        {
            0 => BezierCurve2.Linear,
            1 => BezierCurve2.EaseIn,
            2 => BezierCurve2.EaseOut,
            _ => BezierCurve2.EaseInOut
        });
        _bezier.UpdateUI();
    }

    /// <summary>Called by the window when the blend-curve editor value changes (legacy resets the preset combo).</summary>
    public void OnBlendCurveEdited()
    {
        _allowUpdate = false;
        BlendPresetIndex = -1;
        _allowUpdate = true;
    }

    // ---- Transport toggles ----

    partial void OnChainPlaybackChanged(bool value) => _editor.Tool.Configuration.AnimationEditor_ChainPlayback = value;

    [RelayCommand]
    private void ToggleSound()
    {
        if (_editor.Tool.ReferenceLevel == null && !WadActions.LoadReferenceLevel(_editor.Tool, Owner)) return;
        _editor.Tool.Configuration.AnimationEditor_SoundPreview = !_editor.Tool.Configuration.AnimationEditor_SoundPreview;
        SoundPreview = _editor.Tool.Configuration.AnimationEditor_SoundPreview;
    }

    [RelayCommand]
    private void CycleSoundCondition()
    {
        if (_editor.Tool.ReferenceLevel == null && !WadActions.LoadReferenceLevel(_editor.Tool, Owner)) return;
        var t = _editor.Tool.Configuration.AnimationEditor_SoundPreviewType;
        bool isTEN = IsTombEngine;
        _editor.Tool.Configuration.AnimationEditor_SoundPreviewType = t switch
        {
            SoundPreviewType.Land => SoundPreviewType.LandWithMaterial,
            SoundPreviewType.LandWithMaterial => SoundPreviewType.Water,
            SoundPreviewType.Water => isTEN ? SoundPreviewType.Quicksand : SoundPreviewType.Land,
            SoundPreviewType.Quicksand when isTEN => SoundPreviewType.Underwater,
            _ => SoundPreviewType.Land
        };
        SoundConditionLabel = _editor.Tool.Configuration.AnimationEditor_SoundPreviewType.ToString();
    }

    private static System.Windows.Forms.IWin32Window Owner => TombLib.WPF.WinFormsDialogHelper.GetOpenFormOwner();

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
