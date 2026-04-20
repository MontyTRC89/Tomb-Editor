using TombLib.Wad;

namespace WadTool.Controls
{
    public interface IMeshRenderingPanel
    {
        WadMesh Mesh { get; set; }
        int CurrentElement { get; set; }
        bool ResetCameraOnMeshChange { get; set; }
    }
}
