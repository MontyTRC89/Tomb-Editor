#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Memo;

public partial class MemoWindow : Window
{
    public MemoWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += OnLoaded;
    }

    // "FormMemo" key preserves existing user-config Window_FormMemo_Position/Size/Maximized.
    private void OnLoaded(object sender, RoutedEventArgs e)
        => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormMemo");
}
