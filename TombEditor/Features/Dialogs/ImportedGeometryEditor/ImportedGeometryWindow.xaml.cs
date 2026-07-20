#nullable enable

using System.Windows;
using TombLib.Forms;
using TombLib.LevelData;
using TombLib.WPF;

namespace TombEditor.Features.Dialogs.ImportedGeometryEditor;

public partial class ImportedGeometryWindow : Window
{
    public ImportedGeometryWindow()
    {
        InitializeComponent();
        this.HookModalAutoClose();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ImportedGeometryWindowViewModel oldViewModel)
            oldViewModel.EditMaterialsRequested -= OnEditMaterialsRequested;

        if (e.NewValue is ImportedGeometryWindowViewModel newViewModel)
            newViewModel.EditMaterialsRequested += OnEditMaterialsRequested;
    }

    private void OnEditMaterialsRequested(object? sender, ImportedGeometry model)
    {
        using var form = new FormMaterialEditor(model.Textures, Editor.Instance.Configuration);
        form.ShowDialog(this.GetWin32Window());
    }
}
