using System;

namespace TombLib.Rendering.Graphics.Rhi;

/// <summary>
/// Records a sequence of GPU commands for a single submission. Backends are
/// free to translate this into a real command buffer (Vulkan), a deferred
/// context (DX11), or a straight set of GL calls.
///
/// <para>Lifecycle: obtain one from <see cref="IRhiDevice.BeginCommandList"/>,
/// record any number of passes, then hand it to
/// <see cref="IRhiDevice.Submit"/>. A command list is single-use.</para>
///
/// <para>All Set* calls take effect inside the currently open pass. Calling
/// them outside <see cref="BeginPass"/> / <see cref="EndPass"/> is invalid.</para>
/// </summary>
public interface ICommandList
{
    /// <summary>Open a render pass with the given attachments / clear ops.</summary>
    void BeginPass(in PassDesc desc);

    /// <summary>End the currently open pass.</summary>
    void EndPass();

    /// <summary>Bind a graphics pipeline (shaders + fixed-function state).</summary>
    void SetPipeline(PipelineHandle pipeline);

    /// <summary>
    /// Bind resources at fixed slots. Slots not provided in the spans keep
    /// their previous binding for the current pass.
    /// </summary>
    void SetBindings(in Bindings bindings);

    /// <summary>Bind vertex buffers starting at slot 0.</summary>
    void SetVertexBuffers(ReadOnlySpan<VertexBufferBinding> buffers);

    /// <summary>Bind index buffer. Pass <c>default</c> to detach.</summary>
    void SetIndexBuffer(BufferHandle buffer, IndexFormat format, int offsetBytes = 0);

    /// <summary>
    /// Upload up to <see cref="RhiLimits.PushConstantSize"/> bytes of data
    /// visible to all shader stages of the bound pipeline. On DX11 / GL 4.3
    /// this is emulated by a ring-buffer-backed uniform buffer at b0; on
    /// Vulkan it maps to native push constants.
    /// </summary>
    void PushConstants(ReadOnlySpan<byte> data);

    /// <summary>Override viewport for the current pass.</summary>
    void SetViewport(int x, int y, int width, int height,
                     float minDepth = 0.0f, float maxDepth = 1.0f);

    /// <summary>Override scissor rect. Requires RasterizerState.ScissorEnable.</summary>
    void SetScissor(int x, int y, int width, int height);

    /// <summary>Non-indexed draw.</summary>
    void Draw(int vertexCount, int instanceCount = 1,
              int firstVertex = 0, int firstInstance = 0);

    /// <summary>Indexed draw. <paramref name="baseVertex"/> is added to each fetched index.</summary>
    void DrawIndexed(int indexCount, int instanceCount = 1,
                     int firstIndex = 0, int baseVertex = 0, int firstInstance = 0);

    /// <summary>
    /// Update a dynamic buffer (vertex, index, or constant) with new contents.
    /// Backends discard-and-rewrite via D3D11 MAP_DISCARD / VK staging /
    /// glBufferSubData. Cannot be called inside a render pass.
    /// </summary>
    void UpdateBuffer(BufferHandle buffer, int offsetBytes, ReadOnlySpan<byte> data);

    /// <summary>Optional GPU debug marker. No-op in release builds.</summary>
    void PushDebugGroup(string name);

    /// <summary>End the last <see cref="PushDebugGroup"/>.</summary>
    void PopDebugGroup();
}
