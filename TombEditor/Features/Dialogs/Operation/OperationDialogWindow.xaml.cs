using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Operation;

public partial class OperationDialogWindow : Window
{
    public OperationDialogWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is OperationDialogWindowViewModel vm)
        {
            var interop = new WindowInteropHelper(this);
            vm.SetWindowHandle(interop.Handle);

            // The dialog is hidden from the taskbar, so build/open progress must be shown on the
            // owning (main) window's taskbar button instead of this dialog's own handle.
            vm.SetTaskbarWindowHandle(interop.Owner != IntPtr.Zero ? interop.Owner : interop.Handle);

            vm.LogEntries.CollectionChanged += OnLogChanged;
            vm.PropertyChanged += OnVmPropertyChanged;
            vm.Start();
        }
    }

    private void OnClosing(object sender, CancelEventArgs e)
    {
        if (DataContext is OperationDialogWindowViewModel vm)
        {
            if (!vm.TryRequestClose())
                e.Cancel = true;
        }
    }

    // Match the legacy lstLog.ScrollToCaret(): auto-scroll on new log entry.
    private void OnLogChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && LogList.Items.Count > 0)
            LogList.ScrollIntoView(LogList.Items[LogList.Items.Count - 1]);
    }

    // Match the legacy butOk.Focus(): focus OK as soon as the operation reports success.
    private void OnVmPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OperationDialogWindowViewModel.IsOkEnabled)
            && DataContext is OperationDialogWindowViewModel { IsOkEnabled: true })
            OkButton.Focus();
    }
}
