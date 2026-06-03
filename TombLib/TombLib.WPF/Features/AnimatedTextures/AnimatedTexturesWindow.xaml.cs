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
            Closed += OnClosed;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Detach();

            _viewModel = e.NewValue as AnimatedTexturesWindowViewModel;
            if (_viewModel == null)
                return;

            _viewModel.RequestClose += OnRequestClose;
            textureMapContainer.Child = _viewModel.TextureMap;
            _viewModel.TextureMap.MouseDown += OnTextureMapMouseDown;
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
