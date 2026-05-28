#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MvvmDialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using TombLib.LevelData;
using TombLib.Utils;
using TombLib.WPF.Services;
using TombLib.WPF.Services.Abstract;

namespace TombEditor.ViewModels;

public sealed record PortalEffectItem(PortalEffectType Effect, string DisplayName);

public partial class PortalWindowViewModel : ObservableObject, IModalDialogViewModel
{
    private readonly PortalInstance _instance;
    private readonly IDialogService _dialogService;

    [ObservableProperty] private bool? _dialogResult;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsClassicMirror))]
    private PortalEffectItem _selectedEffect;

    [ObservableProperty] private bool _reflectMoveables;
    [ObservableProperty] private bool _reflectStatics;
    [ObservableProperty] private bool _reflectSprites;
    [ObservableProperty] private bool _reflectLights;

    public IReadOnlyList<PortalEffectItem> Effects { get; }

    public bool IsClassicMirror => SelectedEffect.Effect == PortalEffectType.ClassicMirror;

    public PortalWindowViewModel(
        PortalInstance instance,
        IDialogService? dialogService = null,
        ILocalizationService? localizationService = null)
    {
        _instance = instance;
        _dialogService = ServiceLocator.ResolveService(dialogService);
        _ = ServiceLocator.ResolveService(localizationService).WithKeysFor(this);

        Effects = Enum.GetValues<PortalEffectType>()
            .Select(e => new PortalEffectItem(e, e.ToString().SplitCamelcase()))
            .ToList();

        _selectedEffect = Effects.FirstOrDefault(e => e.Effect == instance.Effect) ?? Effects[0];
        _reflectMoveables = instance.Properties.ReflectMoveables;
        _reflectStatics = instance.Properties.ReflectStatics;
        _reflectSprites = instance.Properties.ReflectSprites;
        _reflectLights = instance.Properties.ReflectLights;
    }

    [RelayCommand]
    private void Confirm()
    {
        _instance.Effect = SelectedEffect.Effect;
        _instance.Properties.ReflectMoveables = ReflectMoveables;
        _instance.Properties.ReflectStatics = ReflectStatics;
        _instance.Properties.ReflectSprites = ReflectSprites;
        _instance.Properties.ReflectLights = ReflectLights;

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
