#nullable enable

using NLog;
using System;
using System.ComponentModel;
using System.Windows;
using TombLib;
using TombLib.Forms.ViewModels;
using TombLib.Forms.Views;
using TombLib.LevelData;
using TombLib.NG;
using TombLib.Utils;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Trigger;

public partial class TriggerWindow : Window
{
    private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private readonly Level _level;
    private readonly TriggerInstance _trigger;
    private readonly Action<ObjectInstance> _selectObject;
    private readonly Action<Room> _selectRoom;

    private bool _dialogIsUpdating;
    private TriggerWindowViewModel? _vm;
    private string? _scriptWithComments;

    public TriggerWindow(TriggerInstance trigger, Level level,
        Action<ObjectInstance> selectObject, Action<Room> selectRoom)
    {
        InitializeComponent();

        _level = level;
        _trigger = trigger;
        _selectObject = selectObject;
        _selectRoom = selectRoom;

        foreach (var ctl in new[] { paramTriggerType, paramPlugin, paramTargetType, paramTarget, paramTimer, paramExtra })
        {
            ctl.Level = level;
            ctl.ViewObject += selectObject;
            ctl.ViewRoom += selectRoom;
            ctl.ParameterChanged += (_, _) => UpdateDialog();
        }

        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormTrigger");
        Initialize(_trigger);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _vm = e.NewValue as TriggerWindowViewModel;
        if (_vm is null)
            return;

        _vm.IsNG = _level.IsNG;
        _vm.IsTombEngine = _level.IsTombEngine;
        _vm.Title = _level.IsTombEngine ? "Classic trigger editor" : "Trigger editor";

        _vm.OnConfirm           = ConfirmAndClose;
        _vm.OnCopyToClipboard   = CopyToClipboard;
        _vm.OnCopyWithComments  = CopyWithComments;
        _vm.OnCopyAsAnimcommand = CopyAsAnimcommand;
        _vm.OnSearchTrigger     = SearchTrigger;

        _vm.PropertyChanged += OnVmPropertyChanged;
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (_dialogIsUpdating)
            return;

        if (e.PropertyName == nameof(TriggerWindowViewModel.RawMode))
        {
            paramTriggerType.RawMode = _vm!.RawMode;
            paramPlugin.RawMode      = _vm.RawMode;
            paramTargetType.RawMode  = _vm.RawMode;
            paramTarget.RawMode      = _vm.RawMode;
            paramTimer.RawMode       = _vm.RawMode;
            paramExtra.RawMode       = _vm.RawMode;
        }
    }

    public void Initialize(TriggerInstance trigger)
    {
        AllocateNewScriptIds(trigger);
        UpdateDialog();

        _dialogIsUpdating = true;
        try
        {
            if (_vm is not null)
            {
                _vm.Bit1 = (trigger.CodeBits & (1 << 0)) != 0;
                _vm.Bit2 = (trigger.CodeBits & (1 << 1)) != 0;
                _vm.Bit3 = (trigger.CodeBits & (1 << 2)) != 0;
                _vm.Bit4 = (trigger.CodeBits & (1 << 3)) != 0;
                _vm.Bit5 = (trigger.CodeBits & (1 << 4)) != 0;
                _vm.OneShot = trigger.OneShot;
            }

            paramTriggerType.Parameter = new TriggerParameterUshort((ushort)trigger.TriggerType);
            paramTargetType.Parameter = new TriggerParameterUshort((ushort)trigger.TargetType);
            paramPlugin.Parameter = trigger.Plugin;

            if (trigger.TriggerType == TriggerType.ConditionNg)
            {
                paramTimer.Parameter = trigger.Timer;
                paramTarget.Parameter = trigger.Target;
            }
            else
            {
                paramTarget.Parameter = trigger.Target;
                paramTimer.Parameter = trigger.Timer;
            }

            paramExtra.Parameter = trigger.Extra;
        }
        finally { _dialogIsUpdating = false; }

        UpdateExportToTrigger();
    }

    private void UpdateDialog()
    {
        if (_dialogIsUpdating)
            return;

        _dialogIsUpdating = true;
        try
        {
            paramTriggerType.ParameterRange = NgParameterInfo.GetTriggerTypeRange(_level.Settings).ToParameterRange();
            paramTargetType.ParameterRange = NgParameterInfo.GetTargetTypeRange(_level.Settings, CurrentTriggerType).ToParameterRange();

            bool isEvent = CurrentTargetType is TriggerTargetType.VolumeEvent or TriggerTargetType.GlobalEvent;
            bool isConditionNg = isEvent || CurrentTriggerType == TriggerType.ConditionNg || CurrentTargetType == TriggerTargetType.ActionNg;

            if (_level.IsNG)
                paramPlugin.ParameterRange = NgParameterInfo.GetPluginRange(_level.Settings);

            if (isConditionNg)
            {
                paramTimer.ParameterRange = NgParameterInfo.GetTimerRange(_level.Settings, CurrentTriggerType, CurrentTargetType, paramTarget.Parameter, paramPlugin.Parameter);
                paramTarget.ParameterRange = NgParameterInfo.GetTargetRange(_level.Settings, CurrentTriggerType, CurrentTargetType, paramTimer.Parameter, paramPlugin.Parameter);
            }
            else
            {
                paramTarget.ParameterRange = NgParameterInfo.GetTargetRange(_level.Settings, CurrentTriggerType, CurrentTargetType, paramTimer.Parameter, paramPlugin.Parameter);
                paramTimer.ParameterRange = NgParameterInfo.GetTimerRange(_level.Settings, CurrentTriggerType, CurrentTargetType, paramTarget.Parameter, paramPlugin.Parameter);
            }

            paramExtra.ParameterRange = NgParameterInfo.GetExtraRange(
                _level.Settings, CurrentTriggerType, CurrentTargetType, paramTarget.Parameter, paramTimer.Parameter, paramPlugin.Parameter,
                out bool isButtons);

            if (_vm is not null)
                _vm.OneShotEnabled = !isEvent;

            if (isButtons && _vm is not null)
            {
                ushort selectedExtraKey = paramExtra.Parameter is TriggerParameterUshort extraParam
                    ? extraParam.Key
                    : (ushort)0;

                _vm.Bit1 = (selectedExtraKey & 1) != 0;
                _vm.Bit2 = (selectedExtraKey & 2) != 0;
                _vm.Bit3 = (selectedExtraKey & 4) != 0;
                _vm.Bit4 = (selectedExtraKey & 8) != 0;
                _vm.Bit5 = (selectedExtraKey & 16) != 0;
                _vm.OneShot = (selectedExtraKey & 32) != 0;
            }
        }
        finally { _dialogIsUpdating = false; }

        UpdateExportToTrigger();
    }

    private TriggerType CurrentTriggerType => paramTriggerType.Parameter is TriggerParameterUshort u
        ? (TriggerType)u.Key
        : TriggerType.Trigger;

    private TriggerTargetType CurrentTargetType => paramTargetType.Parameter is TriggerParameterUshort u
        ? (TriggerTargetType)u.Key
        : TriggerTargetType.Object;

    private byte CodeBits
    {
        get
        {
            if (_vm is null)
                return 0;

            byte b = 0;

            if (_vm.Bit1)
                b |= 1 << 0;

            if (_vm.Bit2)
                b |= 1 << 1;

            if (_vm.Bit3)
                b |= 1 << 2;

            if (_vm.Bit4)
                b |= 1 << 3;

            if (_vm.Bit5)
                b |= 1 << 4;

            return b;
        }
    }

    private TriggerInstance TestTrigger => new(new RectangleInt2())
    {
        TriggerType = CurrentTriggerType,
        TargetType = CurrentTargetType,
        Plugin = paramPlugin.Parameter,
        Target = paramTarget.Parameter,
        Timer = paramTimer.Parameter,
        Extra = paramExtra.Parameter,
        CodeBits = CodeBits,
        OneShot = _vm?.OneShot ?? false
    };

    private void ConfirmAndClose()
    {
        if (!NgParameterInfo.TriggerIsValid(_level.Settings, TestTrigger))
        {
            var result = MessageBox.Show(this,
                "The currently selected trigger data is not valid for the engine.",
                "Trigger invalid",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.OK)
                return;
        }

        _trigger.TriggerType = CurrentTriggerType;
        _trigger.TargetType = CurrentTargetType;
        _trigger.Plugin = paramPlugin.Parameter;
        _trigger.Target = paramTarget.Parameter;
        _trigger.Timer = paramTimer.Parameter;
        _trigger.Extra = paramExtra.Parameter;
        _trigger.CodeBits = CodeBits;
        _trigger.OneShot = _vm?.OneShot ?? false;

        if (_vm is not null)
            _vm.DialogResult = true;
    }

    private void UpdateExportToTrigger()
    {
        if (_vm is null)
            return;

        if (!_level.IsNG)
        {
            _vm.ScriptEnabled = false;
            _vm.CopyAsAnimcommandEnabled = false;
            return;
        }

        try
        {
            _vm.ScriptText = NgParameterInfo.ExportToScriptTrigger(_level, TestTrigger, null);
            _scriptWithComments = NgParameterInfo.ExportToScriptTrigger(_level, TestTrigger, null, true);
            _vm.ScriptEnabled = true;
            _vm.CopyAsAnimcommandEnabled = CurrentTargetType is not TriggerTargetType.ParameterNg and not TriggerTargetType.ActionNg;
        }
        catch (NgParameterInfo.ExceptionScriptNotSupported)
        {
            _vm.ScriptText = "Not supported";
            _scriptWithComments = null;
            _vm.ScriptEnabled = false;
            _vm.CopyAsAnimcommandEnabled = false;
        }
        catch (NgParameterInfo.ExceptionScriptIdMissing)
        {
            _vm.ScriptText = "Click to generate";
            _scriptWithComments = null;
            _vm.ScriptEnabled = false;
            _vm.CopyAsAnimcommandEnabled = false;
        }
        catch (Exception exc)
        {
            _vm.ScriptText = "Check all fields";
            _scriptWithComments = null;
            _vm.ScriptEnabled = false;
            _vm.CopyAsAnimcommandEnabled = false;
            _logger.Debug(exc, "\"ExportToScriptTrigger\" failed.");
        }
    }

    private void AllocateNewScriptIds(TriggerInstance trigger)
    {
        if (_level.Settings.GameVersion != TRVersion.Game.TRNG)
            return;

        if (paramTarget.Parameter is IHasScriptID t && !t.ScriptId.HasValue)
            t.AllocateNewScriptId();

        if (paramTimer.Parameter is IHasScriptID timer && !timer.ScriptId.HasValue)
            timer.AllocateNewScriptId();

        if (paramExtra.Parameter is IHasScriptID extra && !extra.ScriptId.HasValue)
            extra.AllocateNewScriptId();
    }

    private void CopyToClipboard()
    {
        if (_vm is { ScriptText: { Length: > 0 } text })
            Clipboard.SetText(text);
    }

    private void CopyWithComments()
    {
        if (!string.IsNullOrEmpty(_scriptWithComments))
            Clipboard.SetText(_scriptWithComments);
    }

    private void CopyAsAnimcommand()
    {
        var frameVm = new InputBoxWindowViewModel(
            title: "Specify animcommand frame number",
            label: "Enter value from -1 (any frame) to 254:",
            placeholder: "-1");
        if (!ShowInputBox(frameVm) || !int.TryParse(frameVm.Value, out int frame) || frame < -1 || frame > 254)
        {
            if (frameVm.DialogResult == true)
                MessageBox.Show(this, "Frame number is invalid. Maximum is 254.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var result = NgParameterInfo.ExportToScriptTrigger(_level, TestTrigger, frame);
        if (string.IsNullOrEmpty(result))
            return;

        var outVm = new InputBoxWindowViewModel(
            title: "Export as SetPosition animcommand",
            label: "Put these values into X, Y and Z fields in WadTool:",
            placeholder: result);
        ShowInputBox(outVm);
    }

    private void SearchTrigger()
    {
        var vm = new InputBoxWindowViewModel(title: "Import trigger from script", label: "Enter a script command:");
        if (!ShowInputBox(vm))
            return;

        var imported = NgParameterInfo.ImportFromScriptTrigger(_level, vm.Value);
        if (imported is null)
        {
            MessageBox.Show(this, "Script entry is invalid. Trigger can't be imported.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        Initialize(imported);
    }

    private bool ShowInputBox(InputBoxWindowViewModel vm)
    {
        var win = new InputBoxWindow { DataContext = vm, Owner = this, WindowStartupLocation = WindowStartupLocation.CenterOwner };
        win.HookModalAutoClose();
        win.ShowDialog();
        return vm.DialogResult == true;
    }
}
