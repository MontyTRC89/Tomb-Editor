using System.Numerics;
using TombLib.Graphics;
using TombLib;
using TombLib.LevelData;
using TombLib.Rendering;
using TombLib.Controls;
using System;

namespace TombEditor.Controls.Panel3D
{
    public partial class Panel3D
    {
        public override void InitializeRendering(RenderingDevice device, bool antialias, ObjectRenderingQuality objectQuality)
        {
            base.InitializeRendering(device, antialias, objectQuality);

            // Fall back to half of the max. page count for texture allocation, if editor is in safe mode.
            // This workaround is needed for very old PCs which have troubles with providing D3D device caps.

            var texDescription = new RenderingTextureAllocator.Description();
            if (_editor.Configuration.Rendering3D_SafeMode)
                texDescription = new RenderingTextureAllocator.Description { Size = new VectorInt3(texDescription.Size.X, texDescription.Size.X, RenderingTextureAllocator.SafePageCount) };

            _renderingTextures = device.CreateTextureAllocator(texDescription);
            _renderingStateBuffer = device.CreateStateBuffer();
            _fontTexture = device.CreateTextureAllocator(new RenderingTextureAllocator.Description { Size = new VectorInt3(512, 512, 2) });

            _fontDefault = device.CreateFont(new RenderingFont.Description
            {
                FontName = _editor.Configuration.Rendering3D_FontName,
                FontSize = _editor.Configuration.Rendering3D_FontSize,
                FontIsBold = _editor.Configuration.Rendering3D_FontIsBold,
                TextureAllocator = _fontTexture
            });

            // Tappa-1 unified path: one shared dynamic line batch for the whole Panel3D.
            _linesBatch = device.CreateDrawingLines(new RenderingDrawingLines.Description { Dynamic = true });

            int atlasSize = objectQuality switch
            {
                ObjectRenderingQuality.High => 4096,
                ObjectRenderingQuality.Medium => 1024,
                _ => 512
            };
            int maxAllocationSize = objectQuality switch
            {
                ObjectRenderingQuality.High => 2048,
                ObjectRenderingQuality.Medium => 256,
                _ => 128
            };

            _wadRenderer = DeviceManager.DefaultDeviceManager.CreateWadRenderer(true, true, atlasSize, maxAllocationSize, false);
            _gizmo = new Gizmo(device);

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
