// Pixel shader for RenderingDrawingLines — passes the interpolated vertex color
// straight through. Premultiplied alpha is the editor-wide convention; callers
// pass non-premultiplied vertex colors and rely on the BlendingPremultipliedAlpha
// blend state. If a future caller needs straight-alpha behaviour, expose a
// per-batch flag rather than branching here.
struct PixelInputType
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
};

float4 main(PixelInputType input) : SV_TARGET
{
    return input.Color;
}
