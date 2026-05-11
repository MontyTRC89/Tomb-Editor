using TombLib.Rendering;

namespace TombLib.Graphics
{
    // Process-wide singleton owning the rendering device.
    //
    // STATUS as of the SharpDX.Toolkit removal:
    //   ✅ ___LegacyEffects (Solid / Model / RoomGeometry .fx) — REMOVED.
    //   ✅ ___LegacyFont (SpriteFont) — REMOVED.
    //   ✅ ___LegacyDevice (SharpDX.Toolkit GraphicsDevice) — REMOVED. WadRenderer +
    //      Mesh family + ImportedGeometry now use raw SharpDX.Direct3D11.Device.
    //   ✅ Solid.fx, Model.fx, RoomGeometry.fx — REMOVED. Replaced by
    //      LinesShader / MeshShader / ImportedGeometryShader (HLSL pre-compiled).
    //
    // Remaining cleanup:
    //   STEP A  Drop the SharpDX.Toolkit / SharpDX.Toolkit.Graphics / SharpDX.Toolkit.Compiler
    //           DLL references from TombLib.Rendering.csproj (the few helpers in those
    //           DLLs that are still indirectly referenced — VertexElement attribute,
    //           IVertex interface — can either be removed or replaced with local stubs).
    //   STEP B  Replace SharpDX 2.4 binaries with Vortice.Direct3D11 (Vortice is the
    //           maintained drop-in replacement; SharpDX 2.4 has been archived since 2019).
    //   STEP C  Once on Vortice, plan a Vulkan backend via Silk.NET (Tappa 3 originale).
    public class DeviceManager
    {
        public static DeviceManager DefaultDeviceManager = new DeviceManager();

        public RenderingDevice Device;

        // The raw ID3D11Device. Exposed for components that still need it directly
        // (WadRenderer, ImportedGeometryTexture). Always equal to
        // ((Dx11RenderingDevice)Device).Device.
        public SharpDX.Direct3D11.Device D3D11Device { get; }

        public DeviceManager()
        {
            Device = new Rendering.DirectX11.Dx11RenderingDevice();
            D3D11Device = ((Rendering.DirectX11.Dx11RenderingDevice)Device).Device;
            LevelData.ImportedGeometry.Device = D3D11Device;
        }
    }
}
