using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace WadTool.ViewModels;

// Provides view-level access for operations that require WinForms interop.
public interface IMainWindowHost : IWin32Window
{
    IReadOnlyList<IWadObjectId> DestSelectedIds { get; }
    IReadOnlyList<IWadObjectId> SourceSelectedIds { get; }
    void SelectDestinationObjects(IWadObjectId id);
    void SelectDestinationObjects(List<IWadObjectId> ids);
    void InvalidatePreview();
    void ResetPreviewCamera();
    void GarbageCollectPreview();
    void SetPreviewAnimate(bool animate);
    void SetPreviewObject(IWadObject obj);
    void ShowPopup(string message, PopupType type);
    void UpdateDestinationTree(Wad2 wad);
    void UpdateSourceTree(Wad2 wad);
    void UpdateDestinationMetadata();
}

public partial class MainWindowViewModel : ObservableObject
{
    private readonly WadToolClass _tool;
    private IMainWindowHost _host;

    #region Observable Properties

    [ObservableProperty] private string _windowTitle = "WadTool";
    [ObservableProperty] private string _statusText = "";
    [ObservableProperty] private bool _saveEnabled;
    [ObservableProperty] private bool _saveAsEnabled;
    [ObservableProperty] private bool _closeRefLevelEnabled;
    [ObservableProperty] private string _refLevelText = "(project not loaded)";
    [ObservableProperty] private double _refLevelOpacity = 0.5;
    [ObservableProperty] private bool _hasDestinationWad;
    [ObservableProperty] private bool _selectionActionsEnabled;
    [ObservableProperty] private string _sourceHeader = "Source";
    [ObservableProperty] private bool _editAnimationsVisible;
    [ObservableProperty] private bool _editSkeletonVisible;
    [ObservableProperty] private bool _editStaticModelVisible;
    [ObservableProperty] private bool _editSpriteSequenceVisible;

    #endregion

    #region View Events

    public event Action RecentWadsRefreshRequested;

    #endregion

    public WadToolClass Tool => _tool;

    public MainWindowViewModel(WadToolClass tool)
    {
        _tool = tool;
        _tool.EditorEventRaised += HandleEditorEvent;
    }

    public void SetHost(IMainWindowHost host)
    {
        _host = host;
        HandleEditorEvent(new InitEvent());
    }

    public void Cleanup()
    {
        _tool.EditorEventRaised -= HandleEditorEvent;
    }

    private class InitEvent : IEditorEvent { }

    #region Editor Event Handling

    private void HandleEditorEvent(IEditorEvent obj)
    {
        if (obj is InitEvent)
        {
            if (_tool.Configuration.Tool_MakeEmptyWadAtStartup)
            {
                _tool.DestinationWad = new Wad2 { GameVersion = TRVersion.Game.TR4 };
                _tool.RaiseEvent(new WadToolClass.DestinationWadChangedEvent());
            }

            _host?.SetPreviewAnimate(_tool.Configuration.RenderingItem_Animate);
        }

        if (obj is WadToolClass.MessageEvent msg)
            _host?.ShowPopup(msg.Message, msg.Type);

        if (obj is WadToolClass.DestinationWadChangedEvent || obj is InitEvent)
        {
            HasDestinationWad = _tool.DestinationWad != null;

            if (_tool.DestinationWad != null)
            {
                StatusText =
                    "Moveables: " + _tool.DestinationWad.Moveables.Count + " | " +
                    "Statics: " + _tool.DestinationWad.Statics.Count + " | " +
                    "Sprite sequences: " + _tool.DestinationWad.SpriteSequences.Count + " | " +
                    "Textures: " + _tool.DestinationWad.MeshTexturesUnique.Count + " | " +
                    "Texture infos: " + _tool.DestinationWad.MeshTexInfosUnique.Count;
            }
            else
                StatusText = "";

            _host?.UpdateDestinationTree(_tool.DestinationWad);
            _host?.InvalidatePreview();
        }

        if (obj is WadToolClass.SourceWadChangedEvent || obj is InitEvent)
        {
            var header = "Source";
            if (_tool?.SourceWad != null)
                header += string.IsNullOrEmpty(_tool.SourceWad.FileName)
                    ? " (Imported)"
                    : " (" + Path.GetFileName(_tool.SourceWad.FileName) + ")";
            SourceHeader = header;

            _host?.UpdateSourceTree(_tool.SourceWad);
            _host?.InvalidatePreview();
        }

        if (obj is WadToolClass.MainSelectionChangedEvent ||
            obj is WadToolClass.DestinationWadChangedEvent ||
            obj is WadToolClass.SourceWadChangedEvent ||
            obj is InitEvent)
        {
            UpdateSelectionState();
        }

        if (obj is WadToolClass.ReferenceLevelChangedEvent)
            UpdateRefLevelState();

        if (obj is WadToolClass.UnsavedChangesEvent unsaved)
            UpdateSaveState(unsaved.UnsavedChanges);

        if (obj is WadToolClass.SourceWadChangedEvent ||
            obj is WadToolClass.DestinationWadChangedEvent)
            _host?.GarbageCollectPreview();
    }

    private void UpdateSelectionState()
    {
        var selection = _tool.MainSelection;

        if (selection == null)
        {
            _host?.SetPreviewObject(null);
            EditAnimationsVisible = false;
            EditSkeletonVisible = false;
            EditStaticModelVisible = false;
            EditSpriteSequenceVisible = false;
        }
        else
        {
            var wad = _tool.GetWad(selection.Value.WadArea);

            if (selection.Value.Id is WadMoveableId)
            {
                var skin = wad.TryGet(new WadMoveableId(
                    TrCatalog.GetMoveableSkin(wad.GameVersion, ((WadMoveableId)selection.Value.Id).TypeId)));
                var msh = wad.TryGet(selection.Value.Id);
                if (skin != null && skin != msh)
                    _host?.SetPreviewObject(((WadMoveable)msh)?.ReplaceDummyMeshes((WadMoveable)skin));
                else
                    _host?.SetPreviewObject(msh);
            }
            else
                _host?.SetPreviewObject(wad.TryGet(selection.Value.Id));

            EditAnimationsVisible = selection.Value.Id is WadMoveableId;
            EditSkeletonVisible = selection.Value.Id is WadMoveableId;
            EditStaticModelVisible = selection.Value.Id is WadStaticId;
            EditSpriteSequenceVisible = selection.Value.Id is WadSpriteSequenceId;

            _host?.ResetPreviewCamera();
            _host?.InvalidatePreview();
        }

        _host?.InvalidatePreview();
    }

    private void UpdateRefLevelState()
    {
        if (_tool.ReferenceLevel != null)
        {
            CloseRefLevelEnabled = true;
            RefLevelText = Path.GetFileNameWithoutExtension(_tool.ReferenceLevel.Settings.LevelFilePath);
            RefLevelOpacity = 1.0;
        }
        else
        {
            CloseRefLevelEnabled = false;
            RefLevelText = "(project not loaded)";
            RefLevelOpacity = 0.5;
        }
    }

    private void UpdateSaveState(bool hasUnsavedChanges)
    {
        var title = "WadTool";

        if (_tool?.DestinationWad != null)
        {
            bool newOrImported = string.IsNullOrEmpty(_tool.DestinationWad.FileName);

            title += " - ";
            title += newOrImported ? "Untitled" : _tool.DestinationWad.FileName;
            title += hasUnsavedChanges ? "*" : "";

            bool reallyHasUnsavedChanges = newOrImported || hasUnsavedChanges;
            SaveAsEnabled = reallyHasUnsavedChanges;
            SaveEnabled = reallyHasUnsavedChanges;

            if (!hasUnsavedChanges)
                _host?.UpdateDestinationMetadata();
        }
        else
        {
            SaveEnabled = false;
            SaveAsEnabled = false;
        }

        WindowTitle = title;
    }

    #endregion

    #region Helpers

    public DialogResult CheckIfSaved()
    {
        if (SaveEnabled && !_tool.DestinationWad.WadIsEmpty)
            return DarkMessageBox.Show(_host as IWin32Window, "You have unsaved changes. Do you want to save changes?",
                "Confirm", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        else
            return DialogResult.OK;
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void NewWad()
    {
        var result = CheckIfSaved();
        if (result == DialogResult.Yes)
            WadActions.SaveWad(_tool, _host, _tool.DestinationWad, false);
        else if (result == DialogResult.Cancel)
            return;

        WadActions.CreateNewWad(_tool, _host);
        RecentWadsRefreshRequested?.Invoke();
    }

    [RelayCommand]
    private void OpenSource()
        => WadActions.LoadWadOpenFileDialog(_tool, _host, false);

    [RelayCommand]
    private void OpenDestination()
    {
        var result = CheckIfSaved();
        if (result == DialogResult.Yes)
            WadActions.SaveWad(_tool, _host, _tool.DestinationWad, false);
        else if (result == DialogResult.Cancel)
            return;

        WadActions.LoadWadOpenFileDialog(_tool, _host, true);
        RecentWadsRefreshRequested?.Invoke();
    }

    [RelayCommand]
    private void Save()
        => WadActions.SaveWad(_tool, _host, _tool.DestinationWad, false);

    [RelayCommand]
    private void SaveAs()
        => WadActions.SaveWad(_tool, _host, _tool.DestinationWad, true);

    [RelayCommand]
    private void OpenRefLevel()
        => WadActions.LoadReferenceLevel(_tool, _host);

    [RelayCommand]
    private void CloseRefLevel()
        => WadActions.UnloadReferenceLevel(_tool);

    [RelayCommand]
    private void RefLevelLabelClick()
    {
        if (_tool.ReferenceLevel == null)
            WadActions.LoadReferenceLevel(_tool, _host);
    }

    [RelayCommand]
    private void NewMoveable()
    {
        var result = WadActions.CreateObject(_tool, _host, new WadMoveable(new WadMoveableId()));
        if (result != null)
            _host?.SelectDestinationObjects(result);
    }

    [RelayCommand]
    private void NewStatic()
    {
        var result = WadActions.CreateObject(_tool, _host, new WadStatic(new WadStaticId()));
        if (result != null)
            _host?.SelectDestinationObjects(result);
    }

    [RelayCommand]
    private void NewSpriteSequence()
    {
        var result = WadActions.CreateObject(_tool, _host, new WadSpriteSequence(new WadSpriteSequenceId()));
        if (result != null)
            _host?.SelectDestinationObjects(result);
    }

    [RelayCommand]
    private void ConvertToStaticLighting()
        => WadActions.ConvertSelectedObjectLighting(_tool, _host,
            _host.DestSelectedIds.ToList(), WadMeshLightingType.VertexColors);

    [RelayCommand]
    private void ConvertToDynamicLighting()
        => WadActions.ConvertSelectedObjectLighting(_tool, _host,
            _host.DestSelectedIds.ToList(), WadMeshLightingType.Normals);

    [RelayCommand]
    private void ConvertToTiled()
        => WadActions.ConvertSelectedObjectUVMapping(_tool, _host,
            _host.DestSelectedIds.ToList(), false);

    [RelayCommand]
    private void ConvertToUVMapped()
        => WadActions.ConvertSelectedObjectUVMapping(_tool, _host,
            _host.DestSelectedIds.ToList(), true);

    [RelayCommand]
    private void EditItem()
        => WadActions.EditObject(_tool, _host, DeviceManager.DefaultDeviceManager);

    [RelayCommand]
    private void ChangeSlot()
    {
        var result = WadActions.ChangeSlot(_tool, _host);
        if (result != null)
            _host?.SelectDestinationObjects(result);
    }

    [RelayCommand]
    private void DeleteObject()
    {
        if (_host.DestSelectedIds.Count == 0)
            return;

        if (DarkMessageBox.Show(_host, "Do you really want to delete selected objects?", "Delete objects",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        foreach (var id in _host.DestSelectedIds)
            _tool.DestinationWad.Remove(id);

        _tool.MainSelection = null;
        _tool.WadChanged(WadArea.Destination);
    }

    [RelayCommand]
    private void AddObject()
        => CopyObject(false);

    [RelayCommand]
    private void AddObjectToDifferentSlot()
        => CopyObject(true);

    private void CopyObject(bool otherSlot)
    {
        var result = WadActions.CopyObject(_tool, _host,
            _host.SourceSelectedIds.ToList(), otherSlot);

        if (result != null)
        {
            if (result.Count > 0)
                _host?.SelectDestinationObjects(result);
            else
                _tool.SendMessage("No objects were copied because they are already in different slots.", PopupType.Warning);
        }
        else
            _tool.SendMessage("No objects were copied.", PopupType.Info);
    }

    [RelayCommand]
    private void EditAnimations()
        => WadActions.EditObject(_tool, _host, DeviceManager.DefaultDeviceManager);

    [RelayCommand]
    private void EditSkeleton()
        => WadActions.EditSkeleton(_tool, _host);

    [RelayCommand]
    private void EditStaticModel()
        => WadActions.EditObject(_tool, _host, DeviceManager.DefaultDeviceManager);

    [RelayCommand]
    private void EditSpriteSequence()
        => WadActions.EditObject(_tool, _host, DeviceManager.DefaultDeviceManager);

    [RelayCommand]
    private void ExitApplication()
        => System.Windows.Application.Current.MainWindow?.Close();

    #endregion
}
