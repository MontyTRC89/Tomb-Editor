#nullable enable

using System.ComponentModel;
using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.ResizeRoom;

public partial class ResizeRoomWindow : Window
{
    private readonly ResizeRoomGridControl _gridControl = new();

    public ResizeRoomWindow()
    {
        InitializeComponent();
		this.HookModalAutoClose();

        gridHost.Child = _gridControl;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ResizeRoomWindowViewModel oldVm)
        {
            oldVm.AreaChanged -= OnAreaChanged;
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        }

        if (e.NewValue is ResizeRoomWindowViewModel newVm)
        {
            _gridControl.Room = newVm.Room;
            _gridControl.ColorScheme = newVm.Editor.Configuration.UI_ColorScheme;
            SyncFromViewModel(newVm);

            newVm.AreaChanged += OnAreaChanged;
            newVm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnAreaChanged()
    {
        if (DataContext is ResizeRoomWindowViewModel vm)
        {
            SyncFromViewModel(vm);
            _gridControl.Invalidate();
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ResizeRoomWindowViewModel.UseFloor))
            OnAreaChanged();
    }

    private void SyncFromViewModel(ResizeRoomWindowViewModel vm)
    {
        _gridControl.NewArea = vm.NewArea;
        _gridControl.UseFloor = vm.UseFloor;
    }
}
