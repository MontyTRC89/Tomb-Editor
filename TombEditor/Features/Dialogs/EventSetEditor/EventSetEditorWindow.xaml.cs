#nullable enable

using System;
using System.Windows;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    public partial class EventSetEditorWindow : Window
    {
        private EventSetEditorWindowViewModel? _viewModel;
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
                _viewModel.RequestClose -= OnRequestClose;

            _viewModel = e.NewValue as EventSetEditorWindowViewModel;
            if (_viewModel == null)
                return;

            _viewModel.RequestClose += OnRequestClose;
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
                _viewModel.RequestClose -= OnRequestClose;
        }
    }
}
