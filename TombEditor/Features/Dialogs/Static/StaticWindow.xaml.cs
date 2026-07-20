#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.Static;

public partial class StaticWindow : Window
{
    public StaticWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        Loaded += (_, _) => WindowConfiguration.ConfigureWindow(this, Editor.Instance.Configuration, "FormStatic");
    }
}
