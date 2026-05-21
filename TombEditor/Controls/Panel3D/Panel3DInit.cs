using SharpDX.Toolkit.Graphics;
using System.Numerics;
using TombLib;
using TombLib.Controls;
using TombLib.Graphics;
using TombLib.Graphics.Primitives;
using TombLib.LevelData;
using TombLib.Rendering;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        public override void InitializeRendering(RenderingDevice device, bool antialias, ObjectRenderingQuality objectQuality)
        {
            // V2 renderer owns the HWND swapchain. The legacy init is bypassed
            // entirely: RenderingPanel's SwapChain stays null, the per-room
            // cache stays empty, _legacyDevice / _wadRenderer etc. remain
            // null. OnPaint dispatches straight to LevelRenderer.
            // The 'device', 'antialias' and 'objectQuality' parameters are
            // ignored — the V2 device is created fresh from Silk.NET.
            _v2Renderer = new TombEditor.Rendering.V2.LevelRenderer(
                Handle, ClientSize.Width, ClientSize.Height);

            // Gizmo: we keep using the legacy BaseGizmo class for picking +
            // drag math (DoPicking / MouseMoved / MouseUp) — V2 only swaps
            // out the rendering. The constructor still allocates a few
            // legacy GeometricPrimitive objects (cylinder / cube / cone) we
            // never end up drawing, but they cost ~nothing and reusing the
            // legacy class is cheaper than re-implementing 400 lines of
            // axis-pick / plane-intersect math.
            _gizmo = new Gizmo(DeviceManager.DefaultDeviceManager.___LegacyEffects["Solid"]);

            ResetCamera(true);
        }

        RenderingDrawingRoom CacheRoom(Room room)
        {
            var sectorTextures = new SectorTextureDefault
            {
                ColoringInfo = _editor.SectorColoringManager.ColoringInfo,
                DrawIllegalSlopes = ShowIllegalSlopes,
                DrawSlideDirections = ShowSlideDirections,
                ProbeAttributesThroughPortals = _editor.Configuration.UI_ProbeAttributesThroughPortals,
                HideHiddenRooms = DisablePickingForHiddenRooms
            };

            if (_editor.SelectedRoom == room)
            {
                sectorTextures.HighlightArea = _editor.HighlightedSectors.Area;
                sectorTextures.SelectionArea = _editor.SelectedSectors.Area;
                sectorTextures.SelectionArrow = _editor.SelectedSectors.Arrow;
            }

            return Device.CreateDrawingRoom(
                    new RenderingDrawingRoom.Description
                    {
                        Room = room,
                        TextureAllocator = _renderingTextures,
                        SectorTextureGet = sectorTextures.Get
                    });
        }
    }
}
