#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using TombLib.Wad;

namespace WadTool.Features.Dialogs.MeshEditor;

/// <summary>
/// One node of the mesh-list <see cref="System.Windows.Controls.TreeView"/> (multi-mesh mode).
/// Group / object nodes carry no payload; leaf nodes carry the mesh reference (legacy
/// <c>FormMeshEditor.MeshTreeNode</c> tag: object id + mesh index + the <see cref="WadMesh"/>).
/// </summary>
public sealed partial class MeshEditorTreeNode : ObservableObject
{
    [ObservableProperty] private string _text;
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private bool _isExpanded;

    /// <summary>Set on leaf nodes only.</summary>
    public IWadObjectId? ObjectId { get; }
    public int MeshIndex { get; }
    public WadMesh? WadMesh { get; }
    public bool IsSkin { get; }

    public ObservableCollection<MeshEditorTreeNode> Children { get; } = new();

    /// <summary>True for a leaf node that actually references a mesh.</summary>
    public bool IsMesh => WadMesh is not null;

    public MeshEditorTreeNode(string text, IWadObjectId? objectId = null, int meshIndex = 0, WadMesh? wadMesh = null, bool isSkin = false)
    {
        _text = text;
        ObjectId = objectId;
        MeshIndex = meshIndex;
        WadMesh = wadMesh;
        IsSkin = isSkin;
    }
}
