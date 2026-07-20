#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.LevelSettings;

public partial class LevelSettingsWindow : Window
{
    public LevelSettingsWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}
