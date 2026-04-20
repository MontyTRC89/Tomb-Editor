using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System.Collections.ObjectModel;
using TombLib.LevelData;
using TombLib.WPF.Services;

namespace WadTool.ViewModels;

public partial class NewWad2WindowViewModel : ObservableObject, IModalDialogViewModel
{
    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty] private TRVersion.Game _selectedVersion = TRVersion.Game.TR4;

    public ObservableCollection<TRVersion.Game> AvailableVersions { get; }

    private readonly IDialogService _dialogService;

    public NewWad2WindowViewModel(IDialogService? dialogService = null)
    {
        _dialogService = ServiceLocator.ResolveService(dialogService);
        AvailableVersions = new ObservableCollection<TRVersion.Game>(TRVersion.NativeVersions);
    }

    [RelayCommand]
    private void Confirm()
    {
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
