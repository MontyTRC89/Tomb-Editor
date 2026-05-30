#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.Numerics;
using TombLib.LevelData;
using TombLib.Wad.Catalog;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.Transform;

public partial class TransformWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly PositionBasedObjectInstance _instance;
    private readonly Editor _editor;

    private readonly Vector3 _backupPosition;
    private readonly Vector3 _backupRotation;
    private readonly Vector3 _backupScale;

    private bool _loading;
    private bool _undoSaved;

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty] private decimal _translationX;
    [ObservableProperty] private decimal _translationY;
    [ObservableProperty] private decimal _translationZ;

    [ObservableProperty] private decimal _rotationX;
    [ObservableProperty] private decimal _rotationY;
    [ObservableProperty] private decimal _rotationZ;

    [ObservableProperty] private decimal _scaleX;
    [ObservableProperty] private decimal _scaleY;
    [ObservableProperty] private decimal _scaleZ;

    public bool IsRotationYEnabled { get; }
    public bool IsRotationXEnabled { get; }
    public bool IsRotationZEnabled { get; }
    public bool IsScaleEnabled { get; }

    public TransformWindowViewModel(
        PositionBasedObjectInstance instance,
        Editor? editor = null,
        ILocalizationService? localizationService = null)
    {
        _instance = instance;
        _editor = editor ?? Editor.Instance;
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        _backupPosition = instance.Position;
        _backupScale = instance is IScaleable scaleable
            ? new Vector3(scaleable.Scale)
            : Vector3.One;
        _backupRotation = new Vector3(
            instance is IRotateableYX rotYX ? rotYX.RotationX : 0.0f,
            instance is IRotateableY rotY ? rotY.RotationY : 0.0f,
            instance is IRotateableYXRoll rotYXR ? rotYXR.Roll : 0.0f);

        IsRotationYEnabled = instance is IRotateableY;

        if (_editor.SelectedObject is MoveableInstance moveable)
        {
            bool freelyRotateable = TrCatalog.IsFreelyRotateable(_editor.Level.Settings.GameVersion, moveable.WadObjectId.TypeId);
            IsRotationXEnabled = freelyRotateable;
            IsRotationZEnabled = freelyRotateable;
        }
        else
        {
            IsRotationXEnabled = instance is IRotateableYX;
            IsRotationZEnabled = instance is IRotateableYXRoll;
        }

        // Static meshes in non-TombEngine builds use stamp-only scaling, not a freely-set scale.
        IsScaleEnabled = instance is IScaleable && !(instance is StaticInstance && !_editor.Level.IsTombEngine);

        LoadFromInstance();
    }

    private void LoadFromInstance()
    {
        _loading = true;

        TranslationX = (decimal)_instance.Position.X + _instance.Room.Position.X * (int)Level.SectorSizeUnit;
        TranslationY = (decimal)-(_instance.Position.Y + _instance.Room.Position.Y);
        TranslationZ = (decimal)_instance.Position.Z + _instance.Room.Position.Z * (int)Level.SectorSizeUnit;

        if (_instance is IRotateableY rotY)
            RotationY = (decimal)rotY.RotationY;

        if (_instance is IRotateableYX rotYX)
            RotationX = (decimal)rotYX.RotationX;

        if (_instance is IRotateableYXRoll rotYXR)
            RotationZ = (decimal)rotYXR.Roll;

        if (_instance is IScaleable scaleable)
        {
            ScaleX = (decimal)scaleable.Scale;
            ScaleY = (decimal)scaleable.Scale;
            ScaleZ = (decimal)scaleable.Scale;
        }

        _loading = false;
    }

    // The original FormTransform applied changes on each numeric Validated event
    // so the user could see the live result before pressing OK. Mirror that here:
    // every value change after construction commits to the instance (single undo entry).

    partial void OnTranslationXChanged(decimal value) => CommitIfReady();
    partial void OnTranslationYChanged(decimal value) => CommitIfReady();
    partial void OnTranslationZChanged(decimal value) => CommitIfReady();
    partial void OnRotationXChanged(decimal value) => CommitIfReady();
    partial void OnRotationYChanged(decimal value) => CommitIfReady();
    partial void OnRotationZChanged(decimal value) => CommitIfReady();

    partial void OnScaleXChanged(decimal value)
    {
        if (_loading)
            return;

        // Keep Y/Z mirrored to X — uniform scale until per-axis support is added.
        _loading = true;
        ScaleY = value;
        ScaleZ = value;
        _loading = false;

        CommitIfReady();
    }

    private void CommitIfReady()
    {
        if (_loading)
            return;

        if (!_undoSaved)
        {
            _editor.UndoManager.PushObjectTransformed(_instance);
            _undoSaved = true;
        }

        SaveToInstance();
        _editor.ObjectChange(_instance, ObjectChangeType.Change);
    }

    private void SaveToInstance()
    {
        _instance.Position = new Vector3(
            (float)TranslationX - _instance.Room.Position.X * (int)Level.SectorSizeUnit,
            (float)-TranslationY - _instance.Room.Position.Y,
            (float)TranslationZ - _instance.Room.Position.Z * (int)Level.SectorSizeUnit);

        if (_instance is IRotateableY rotY)
            rotY.RotationY = (float)RotationY;

        if (_instance is IRotateableYX rotYX)
            rotYX.RotationX = (float)RotationX;

        if (_instance is IRotateableYXRoll rotYXR)
            rotYXR.Roll = (float)RotationZ;

        if (_instance is IScaleable scaleable)
            scaleable.Scale = (float)ScaleX;
    }

    [RelayCommand]
    private void Confirm()
    {
        SaveToInstance();
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        _instance.Position = _backupPosition;

        if (_instance is IScaleable scaleable)
            scaleable.Scale = _backupScale.X;

        if (_instance is IRotateableY rotY)
            rotY.RotationY = _backupRotation.Y;

        if (_instance is IRotateableYX rotYX)
            rotYX.RotationX = _backupRotation.X;

        if (_instance is IRotateableYXRoll rotYXR)
            rotYXR.Roll = _backupRotation.Z;

        _editor.ObjectChange(_instance, ObjectChangeType.Change);
        DialogResult = false;
    }
}
