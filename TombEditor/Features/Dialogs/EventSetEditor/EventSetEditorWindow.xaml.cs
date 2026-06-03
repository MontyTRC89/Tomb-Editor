#nullable enable

using System;
using System.Windows;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    public partial class EventSetEditorWindow : Window
    {
        private EventSetEditorWindowViewModel? _viewModel;
        private Editor? _editor;
        private bool _cancelled;
        private bool _levelChanged;

        public EventSetEditorWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Closed += OnClosed;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_viewModel != null)
                _viewModel.RequestClose -= OnRequestClose;
            if (_editor != null)
                _editor.EditorEventRaised -= OnEditorEvent;

            _viewModel = e.NewValue as EventSetEditorWindowViewModel;
            if (_viewModel == null)
                return;

            _viewModel.RequestClose += OnRequestClose;
            _editor = _viewModel.Editor;
            _editor.EditorEventRaised += OnEditorEvent;
        }

        private void OnEditorEvent(IEditorEvent obj)
        {
            // The backup we hold belongs to the old level, so a level switch must close without restoring.
            if (obj is Editor.LevelChangedEvent)
            {
                _levelChanged = true;
                Close();
            }
        }

        private void OnRequestClose(object? sender, bool cancelled)
        {
            _cancelled = cancelled;
            Close();
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            if (!_levelChanged)
                _viewModel?.Closing(_cancelled);

            if (_viewModel != null)
                _viewModel.RequestClose -= OnRequestClose;
            if (_editor != null)
                _editor.EditorEventRaised -= OnEditorEvent;
        }
    }
}
