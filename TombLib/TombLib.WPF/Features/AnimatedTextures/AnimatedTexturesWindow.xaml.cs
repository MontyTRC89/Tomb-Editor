#nullable enable

using System;
using System.Windows;
using System.Windows.Input;

namespace TombLib.WPF.Features.AnimatedTextures
{
    public partial class AnimatedTexturesWindow : Window
    {
        private AnimatedTexturesWindowViewModel? _viewModel;
        private bool _cancelled;

        public AnimatedTexturesWindow()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            Closed += OnClosed;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Detach();

            _viewModel = e.NewValue as AnimatedTexturesWindowViewModel;
            if (_viewModel == null)
                return;

            _viewModel.RequestClose += OnRequestClose;
            _viewModel.TextureMap.MouseDown += OnTextureMapMouseDown;
        }

        // Attach the externally-created texture map only once the window has its HwndSource, so the
        // hosted control (and the window) get the correct per-monitor DPI context. Attaching it earlier
        // (in DataContextChanged, before the window is shown) corrupted the popup coordinate transform.
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null && !ReferenceEquals(textureMapContainer.Child, _viewModel.TextureMap))
                textureMapContainer.Child = _viewModel.TextureMap;
        }

        private void OnRenameSet(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null || !_viewModel.HasSelectedSet)
                return;

            var dialog = new InputDialog("Rename animation set", "Name:", _viewModel.Name) { Owner = this };
            if (dialog.ShowDialog() == true)
                _viewModel.Name = dialog.Value;
        }

        private void OnTextureMapMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2 &&
                _viewModel?.AddFrameCommand.CanExecute(null) == true)
            {
                _viewModel.AddFrameCommand.Execute(null);
            }
        }

        private void OnRequestClose(object? sender, bool cancelled)
        {
            _cancelled = cancelled;
            Close();
        }

        private void OnClosed(object? sender, EventArgs e)
        {
            _viewModel?.Closing(_cancelled);
            Detach();
            textureMapContainer.Child = null;
        }

        private void Detach()
        {
            if (_viewModel == null)
                return;

            _viewModel.RequestClose -= OnRequestClose;
            _viewModel.TextureMap.MouseDown -= OnTextureMapMouseDown;
        }
    }
}
