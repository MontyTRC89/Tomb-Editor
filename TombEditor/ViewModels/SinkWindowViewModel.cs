#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using TombLib;
using TombLib.LevelData;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.ViewModels;

public partial class SinkWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly SinkInstance _sink;
    private readonly IDialogService _dialogService;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private short _strength;

    public SinkWindowViewModel(
        SinkInstance sink,
        IDialogService? dialogService = null,
        ILocalizationService? localizationService = null)
    {
        _sink = sink;
        _dialogService = ServiceLocator.ResolveService(dialogService);
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        // Strength is stored as 0..31 internally but presented as 1..32 in the UI
        // to match the historical FormSink behaviour.
        _strength = (short)MathC.Clamp(sink.Strength + 1, 1, 32);
    }

    [RelayCommand]
    private void Confirm()
    {
        _sink.Strength = (short)(Strength - 1);

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
