using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.ToolBarLayout;

public partial class ToolBarLayoutWindow : Window
{
    public ToolBarLayoutWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}
