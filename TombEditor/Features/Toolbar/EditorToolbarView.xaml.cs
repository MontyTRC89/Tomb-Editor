#nullable enable

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TombEditor.Features.Dialogs.ToolBarLayout;

namespace TombEditor.Features.Toolbar;

/// <summary>
/// The editor's main button toolbar (port of the WinForms <c>MainView</c> tool strip). Hosted above
/// both the 3D view and the 2D map so it stays in the builder's expected spot regardless of which
/// document tab is active. Button commands resolve through <see cref="CommandHandler"/> exactly like
/// the legacy WinForms toolbar, so the control is purely declarative and stateless.
/// </summary>
public partial class EditorToolbarView : UserControl
{
    public EditorToolbarView()
    {
        InitializeComponent();
    }

    private void DrawObjectsMenu_SubmenuOpened(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem dropdown)
            return;

        foreach (var item in dropdown.Items.OfType<MenuItem>())
        {
            if (item.Tag is not string flagName || string.IsNullOrEmpty(flagName))
                continue;

            var prop = typeof(Configuration).GetProperty(flagName);
            if (prop is null || prop.PropertyType != typeof(bool))
                continue;

            item.IsCheckable = true;
            item.IsChecked = (bool)prop.GetValue(Editor.Instance.Configuration)!;
        }
    }

    private void CustomizeToolbar_Click(object sender, RoutedEventArgs e)
    {
        // The WPF toolbar's button set is currently hardcoded in XAML, so the
        // reordering committed via this dialog only affects the WinForms shell
        // (UI_ToolbarButtons in Configuration). Still surfaces the dialog so
        // users can manage the persisted button list for the legacy shell.
        var allCommands = CommandHandler.Commands.Select(c => c.Name).ToList();
        var vm = new ToolBarLayoutWindowViewModel(Editor.Instance, allCommands);
        var dialog = new ToolBarLayoutWindow
        {
            DataContext = vm,
            Owner = Window.GetWindow(this)
        };
        dialog.ShowDialog();
    }
}
