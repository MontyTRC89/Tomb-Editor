#nullable enable

using System;
using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.EventSetEditor
{
    public partial class EventSetEditorWindow : Window
    {
        public EventSetEditorWindow()
        {
            InitializeComponent();
            // This window is shown modeless, so the hook falls back to Close() when the
            // view-model sets DialogResult (OK/Cancel buttons, level switch).
            this.HookModalAutoClose();
            Closed += OnClosed;
        }

        private void OnClosed(object? sender, EventArgs e)
            => (DataContext as EventSetEditorWindowViewModel)?.OnWindowClosed();
    }
}
