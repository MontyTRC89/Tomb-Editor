#nullable enable

using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using TombLib.LevelData;
using TombLib.Wad;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.Features.Dialogs.QuickItemGroup;

public partial class QuickItemGroupWindowViewModel : ObservableObject, IModalDialogViewModel
{
    public sealed record QuickItem(IWadObjectId Id, string DisplayText);


    [ObservableProperty] private bool? _dialogResult;
    [ObservableProperty] private QuickItem? _selectedItem;

    public IReadOnlyList<QuickItem> Items { get; }

    public IWadObjectId? SelectedValue => SelectedItem?.Id;

    public QuickItemGroupWindowViewModel(
        Editor editor,
        ILocalizationService? localizationService = null)
    {
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        var settings = editor.Level.Settings;
        var version = settings.GameVersion;

        IEnumerable<IWadObjectId> ids = settings.WadGetAllMoveables().Keys
            .Cast<IWadObjectId>()
            .Concat(settings.WadGetAllStatics().Keys.Cast<IWadObjectId>());

        Items = ids
            .Select(id => new QuickItem(id, id.ToString(version)))
            .ToList();

        SelectedItem = Items.FirstOrDefault();
    }

    [RelayCommand]
    private void Confirm()
    {
        DialogResult = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogResult = false;
    }
}
