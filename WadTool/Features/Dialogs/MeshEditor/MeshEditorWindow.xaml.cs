#nullable enable

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TombLib.Forms;
using TombLib.WPF;
using WadTool.Controls;

namespace WadTool.Features.Dialogs.MeshEditor;

/// <summary>
/// WPF port of the legacy <c>FormMeshEditor</c>. The 3D mesh view (<see cref="PanelRenderingMesh"/>)
/// and the texture-map control (<see cref="PanelTextureMap"/>) remain WinForms, hosted via
/// <c>WindowsFormsHost</c>; everything around them is native WPF bound to
/// <see cref="MeshEditorWindowViewModel"/>.
/// </summary>
public partial class MeshEditorWindow : Window
{
    private readonly PanelRenderingMesh _panelMesh;
    private readonly PanelTextureMap _panelTextureMap;
    private readonly PopUpInfo _popup = new();
    private MeshEditorWindowViewModel? _vm;

    public MeshEditorWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        _panelMesh = new PanelRenderingMesh();
        _panelTextureMap = new PanelTextureMap();
        meshHost.Child = _panelMesh;
        textureMapHost.Child = _panelTextureMap;
        _panelTextureMap.SelectedTextureChanged += OnTextureSelectionChanged;

        meshTree.SelectedItemChanged += OnTreeSelectedItemChanged;
        meshTree.MouseDoubleClick += OnTreeMouseDoubleClick;

        DataContextChanged += OnDataContextChanged;
        PreviewKeyDown += OnPreviewKeyDown;
        Closing += OnClosingHandler;
        Closed += OnClosed;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_vm is not null)
        {
            _vm.MessageRaised -= OnMessageRaised;
            _vm.EnsureSelectionVisible -= OnEnsureSelectionVisible;
        }

        _vm = e.NewValue as MeshEditorWindowViewModel;
        if (_vm is null)
            return;

        _vm.MessageRaised += OnMessageRaised;
        _vm.EnsureSelectionVisible += OnEnsureSelectionVisible;
        _vm.AttachPanels(_panelMesh, _panelTextureMap);
    }

    private void OnClosingHandler(object? sender, CancelEventArgs e)
    {
        if (_vm is not null && !_vm.HandleClosing())
            e.Cancel = true;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _panelTextureMap.SelectedTextureChanged -= OnTextureSelectionChanged;
        meshTree.SelectedItemChanged -= OnTreeSelectedItemChanged;
        meshTree.MouseDoubleClick -= OnTreeMouseDoubleClick;

        if (_vm is not null)
        {
            _vm.MessageRaised -= OnMessageRaised;
            _vm.EnsureSelectionVisible -= OnEnsureSelectionVisible;
            _vm.Detach();
        }
    }

    private void OnTreeSelectedItemChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
        => _vm?.SelectTreeNode(e.NewValue as MeshEditorTreeNode);

    private void OnTreeMouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (meshTree.SelectedItem is MeshEditorTreeNode { IsMesh: true } && _vm?.RenameCommand.CanExecute(null) == true)
            _vm.RenameCommand.Execute(null);
    }

    private void OnTextureSelectionChanged(object? sender, EventArgs e) => _vm?.NotifyTextureSelectionChanged();

    private void OnMessageRaised(string message, PopupType type) => PopUpInfo.Show(_popup, null, _panelMesh, message, type);

    private void OnEnsureSelectionVisible()
    {
        // Best-effort: bring the selected tree item into view once the container exists.
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (meshTree.ItemContainerGenerator.ContainerFromItem(meshTree.SelectedItem) is FrameworkElement fe)
                fe.BringIntoView();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_vm is null || !_vm.ShowEditingTools)
            return;

        bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

        switch (e.Key)
        {
            case Key.Escape:
                _vm.ClearSelection();
                break;
            case Key.Z when ctrl:
                _vm.UndoCommand.Execute(null);
                break;
            case Key.Y when ctrl:
                _vm.RedoCommand.Execute(null);
                break;
            case Key.F when ctrl:
                if (_vm.IsFaceMode) _vm.FindTextureCommand.Execute(null);
                break;
            case Key.OemMinus:
            case Key.OemPlus:
            case Key.OemTilde:
            case Key.OemBackslash:
                if (shift) _vm.MirrorTextureCommand.Execute(null);
                else _vm.RotateTextureCommand.Execute(null);
                break;
        }
    }
}
