#nullable enable

using System;
using System.Windows;

namespace TombEditor.Features.Dialogs.FindTextures;

public partial class FindTexturesWindow : Window
{
    private FindTexturesWindowViewModel? _viewModel;

    public FindTexturesWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Closed += OnClosed;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.RequestClose -= OnRequestClose;

        _viewModel = e.NewValue as FindTexturesWindowViewModel;

        if (_viewModel is not null)
            _viewModel.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(object? sender, EventArgs e) => Close();

    private void OnClosed(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
            _viewModel.RequestClose -= OnRequestClose;

        _viewModel?.Cleanup();
    }
}
