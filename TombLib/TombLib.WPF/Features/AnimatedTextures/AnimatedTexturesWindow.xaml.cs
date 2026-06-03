#nullable enable

using System;
using System.Windows;

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
            textureMapHost.Child = _viewModel.TextureMap.Control;
            _viewModel.TextureMap.DoubleClick += OnTextureMapDoubleClick;
        }

        private void OnTextureMapDoubleClick(object? sender, EventArgs e)
        {
            if (_viewModel?.AddFrameCommand.CanExecute(null) == true)
                _viewModel.AddFrameCommand.Execute(null);
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
            textureMapHost.Child = null;
        }

        private void Detach()
        {
            if (_viewModel == null)
                return;

            _viewModel.RequestClose -= OnRequestClose;
            if (_viewModel.TextureMap != null)
                _viewModel.TextureMap.DoubleClick -= OnTextureMapDoubleClick;
        }
    }
}
