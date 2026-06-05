#nullable enable

using System.Windows;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.ImportedGeometryEditor;

public partial class ImportedGeometryWindow : Window
{
    public ImportedGeometryWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
    }
}
