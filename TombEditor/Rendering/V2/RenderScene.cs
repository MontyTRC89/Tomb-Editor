using System.Drawing;
using System.Numerics;
using TombLib.Graphics;
using TombLib.LevelData;

namespace TombEditor.Rendering.V2;

/// <summary>
/// Immutable per-frame description of what the V2 renderer should draw.
/// Built by Panel3D right before <see cref="LevelRenderer.RenderFrame"/>;
/// renderer passes read it without touching editor state directly.
///
/// <para>Kept minimal on purpose: each new feature (objects, sprites, etc.)
/// adds fields here as its pass is implemented.</para>
/// </summary>
public readonly struct RenderScene
{
    public readonly Level     Level;
    public readonly Camera    Camera;
    public readonly Size      ViewportSize;
    public readonly Matrix4x4 ViewProjection;

    public RenderScene(Level level, Camera camera, Size viewportSize)
    {
        Level          = level;
        Camera         = camera;
        ViewportSize   = viewportSize;
        ViewProjection = camera.GetViewProjectionMatrix(viewportSize.Width, viewportSize.Height);
    }
}
