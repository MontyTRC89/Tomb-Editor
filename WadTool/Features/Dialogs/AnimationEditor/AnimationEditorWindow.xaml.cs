#nullable enable

using System;
using System.ComponentModel;
using System.Windows;
using TombLib.Controls;
using TombLib.WPF;
using WadTool.Controls;

namespace WadTool.Features.Dialogs.AnimEditor;

/// <summary>
/// WPF port of the legacy <c>FormAnimationEditor</c> (built incrementally). The 3D view, the timeline
/// (<see cref="AnimationTrackBar"/>) and the blend-curve editor (<see cref="BezierCurveEditor"/>) remain
/// WinForms custom-rendered controls, hosted via <c>WindowsFormsHost</c>; everything else is WPF bound
/// to <see cref="AnimationEditorWindowViewModel"/>.
/// </summary>
public partial class AnimationEditorWindow : Window
{
    private readonly PanelRenderingAnimationEditor _panel;
    private readonly AnimationTrackBar _timeline;
    private readonly BezierCurveEditor _bezier;
    private AnimationEditorWindowViewModel? _vm;

    public AnimationEditorWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();

        _panel = new PanelRenderingAnimationEditor();
        _timeline = new AnimationTrackBar();
        _bezier = new BezierCurveEditor();
        panelRenderingHost.Child = _panel;
        timelineHost.Child = _timeline;
        bezierHost.Child = _bezier;

        _timeline.ValueChanged += OnTimelineValueChanged;

        DataContextChanged += OnDataContextChanged;
        Closing += OnClosingHandler;
        Closed += OnClosed;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        _vm = e.NewValue as AnimationEditorWindowViewModel;
        _vm?.AttachControls(_panel, _timeline, _bezier);
    }

    private void OnTimelineValueChanged(object? sender, EventArgs e) => _vm?.OnTimelineValueChanged();

    private void OnClosingHandler(object? sender, CancelEventArgs e) => _vm?.HandleClosing();

    private void OnClosed(object? sender, EventArgs e)
    {
        _timeline.ValueChanged -= OnTimelineValueChanged;
        _vm?.Detach();
    }
}
