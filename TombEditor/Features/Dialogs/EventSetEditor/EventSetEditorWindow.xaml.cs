#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TombEditor.Controls;
using TombLib.LevelData.VisualScripting;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    public partial class EventSetEditorWindow : Window
    {
        private EventSetEditorWindowViewModel? _viewModel;
        private TriggerManager? _triggerManager;
        private List<TriggerNode>? _clipboard;
        private bool _cancelled;

        public EventSetEditorWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Closed += OnClosed;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.RequestClose -= OnRequestClose;
            }

            _viewModel = e.NewValue as EventSetEditorWindowViewModel;
            if (_viewModel == null)
                return;

            // Temporary: host the existing WinForms node editor until the pure-WPF node editor lands.
            _triggerManager = new TriggerManager();
            _triggerManager.Initialize(_viewModel.Editor, _viewModel.NodeFunctions, _viewModel.ScriptFunctions);
            triggerManagerHost.Child = _triggerManager;
            _triggerManager.Event = _viewModel.CurrentEvent;

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            _viewModel.RequestClose += OnRequestClose;
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EventSetEditorWindowViewModel.CurrentEvent) && _triggerManager != null)
                _triggerManager.Event = _viewModel!.CurrentEvent;
        }

        private void OnRequestClose(object? sender, bool cancelled)
        {
            _cancelled = cancelled;
            Close();
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            _viewModel?.Closing(_cancelled);

            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
                _viewModel.RequestClose -= OnRequestClose;
            }

            _triggerManager?.Dispose();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (_triggerManager == null || Keyboard.Modifiers != ModifierKeys.Control)
                return;

            switch (e.Key)
            {
                case Key.C:
                    var copied = _triggerManager.CopyNodes(false);
                    if (copied.Count > 0)
                        _clipboard = copied;
                    break;
                case Key.X:
                    _clipboard = _triggerManager.CopyNodes(true);
                    break;
                case Key.V:
                    _triggerManager.PasteNodes(_clipboard);
                    break;
            }
        }
    }
}
