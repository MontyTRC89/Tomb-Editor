#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using TombLib.LevelData;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.ViewModels;

public partial class MemoWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly MemoInstance _memo;
    private readonly IDialogService _dialogService;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _text;
    [ObservableProperty] private bool _alwaysDisplay;

    public MemoWindowViewModel(
        MemoInstance memo,
        IDialogService? dialogService = null,
        ILocalizationService? localizationService = null)
    {
        _memo = memo;
        _dialogService = ServiceLocator.ResolveService(dialogService);
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        _text = memo.Text ?? string.Empty;
        _alwaysDisplay = memo.AlwaysDisplay;
    }

    [RelayCommand]
    private void Confirm()
    {
        _memo.Text = Text;
        _memo.AlwaysDisplay = AlwaysDisplay;

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
