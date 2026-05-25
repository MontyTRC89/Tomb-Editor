using System;

namespace TombLib.Rendering.Graphics.Rhi;

// Opaque handles. Carry a 32-bit id only; the backend keeps the real GPU
// object in its own pool. Passing handles by value is free and they are
// allocation-less in the hot path. A default (id == 0) handle is invalid.

public readonly struct BufferHandle : IEquatable<BufferHandle>
{
    public readonly uint Id;
    public BufferHandle(uint id) => Id = id;
    public bool IsValid => Id != 0;
    public bool Equals(BufferHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is BufferHandle h && Equals(h);
    public override int GetHashCode() => (int)Id;
    public static bool operator ==(BufferHandle a, BufferHandle b) => a.Id == b.Id;
    public static bool operator !=(BufferHandle a, BufferHandle b) => a.Id != b.Id;
}

public readonly struct TextureHandle : IEquatable<TextureHandle>
{
    public readonly uint Id;
    public TextureHandle(uint id) => Id = id;
    public bool IsValid => Id != 0;
    public bool Equals(TextureHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is TextureHandle h && Equals(h);
    public override int GetHashCode() => (int)Id;
    public static bool operator ==(TextureHandle a, TextureHandle b) => a.Id == b.Id;
    public static bool operator !=(TextureHandle a, TextureHandle b) => a.Id != b.Id;
}

public readonly struct SamplerHandle : IEquatable<SamplerHandle>
{
    public readonly uint Id;
    public SamplerHandle(uint id) => Id = id;
    public bool IsValid => Id != 0;
    public bool Equals(SamplerHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is SamplerHandle h && Equals(h);
    public override int GetHashCode() => (int)Id;
    public static bool operator ==(SamplerHandle a, SamplerHandle b) => a.Id == b.Id;
    public static bool operator !=(SamplerHandle a, SamplerHandle b) => a.Id != b.Id;
}

public readonly struct PipelineHandle : IEquatable<PipelineHandle>
{
    public readonly uint Id;
    public PipelineHandle(uint id) => Id = id;
    public bool IsValid => Id != 0;
    public bool Equals(PipelineHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is PipelineHandle h && Equals(h);
    public override int GetHashCode() => (int)Id;
    public static bool operator ==(PipelineHandle a, PipelineHandle b) => a.Id == b.Id;
    public static bool operator !=(PipelineHandle a, PipelineHandle b) => a.Id != b.Id;
}

public readonly struct SwapchainHandle : IEquatable<SwapchainHandle>
{
    public readonly uint Id;
    public SwapchainHandle(uint id) => Id = id;
    public bool IsValid => Id != 0;
    public bool Equals(SwapchainHandle other) => Id == other.Id;
    public override bool Equals(object? obj) => obj is SwapchainHandle h && Equals(h);
    public override int GetHashCode() => (int)Id;
    public static bool operator ==(SwapchainHandle a, SwapchainHandle b) => a.Id == b.Id;
    public static bool operator !=(SwapchainHandle a, SwapchainHandle b) => a.Id != b.Id;
}
