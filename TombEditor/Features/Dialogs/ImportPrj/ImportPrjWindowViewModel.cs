#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using MvvmDialogs;
using System.IO;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.ImportPrj;

public partial class ImportPrjWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly ILocalizationService _localizationService;

    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private string _soundsPath = string.Empty;
    [ObservableProperty] private bool _respectMousepatchOnFlybyHandling;
    [ObservableProperty] private bool _useHalfPixelCorrection;

    public string PrjPath { get; }
    public string PrjFileName { get; }

    public ImportPrjWindowViewModel(
        string prjPath,
        bool respectMousepatch,
        bool useHalfPixelCorrection,
        ILocalizationService? localizationService = null)
    {
        _localizationService = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        PrjPath = prjPath;
        PrjFileName = Path.GetFileNameWithoutExtension(prjPath);
        _respectMousepatchOnFlybyHandling = respectMousepatch;
        _useHalfPixelCorrection = useHalfPixelCorrection;
    }

    [RelayCommand]
    private void BrowseSoundsCatalog()
    {
        var dialog = new OpenFileDialog
        {
            Title = _localizationService["BrowseSoundsTitle"],
            Filter = "Sound catalogs|*.xml;*.txt|All files|*.*",
            CheckFileExists = true
        };

        if (dialog.ShowDialog() == true)
            SoundsPath = dialog.FileName;
    }

    [RelayCommand]
    private void Confirm() => DialogResult = true;

    [RelayCommand]
    private void Cancel() => DialogResult = false;
}
