using System;
using System.IO;
using System.Reflection;

namespace TombLib.RenderingV2.Rhi;

/// <summary>
/// Loads shader bytecode from the assembly's embedded resources produced by
/// HlslShaderCompile.V2.targets. Resource names follow the convention:
/// <c>ShadersV2.&lt;Name&gt;.&lt;vs|ps&gt;.&lt;dxbc|spv|glsl&gt;</c>.
///
/// <para>The library is backend-agnostic: it loads all three variants and
/// hands them out as <see cref="ShaderBytecode"/>. The backend picks the
/// matching field when building a pipeline.</para>
/// </summary>
public static class ShaderLibrary
{
    private static readonly Assembly _assembly = typeof(ShaderLibrary).Assembly;

    /// <summary>Load the vertex + pixel stages of a shader by base name (e.g. "Hello").</summary>
    public static (ShaderBytecode Vs, ShaderBytecode Ps) Load(string name)
    {
        return (LoadStage(name, ShaderStage.Vertex), LoadStage(name, ShaderStage.Fragment));
    }

    public static ShaderBytecode LoadStage(string name, ShaderStage stage)
    {
        string suffix = stage switch
        {
            ShaderStage.Vertex   => "vs",
            ShaderStage.Fragment => "ps",
            _ => throw new ArgumentOutOfRangeException(nameof(stage)),
        };

        byte[] dxbc  = ReadResource($"ShadersV2.{name}.{suffix}.dxbc");
        byte[] spirv = ReadResource($"ShadersV2.{name}.{suffix}.spv");
        byte[] glsl  = ReadResource($"ShadersV2.{name}.{suffix}.glsl");
        return new ShaderBytecode(stage, dxbc, spirv, glsl);
    }

    private static byte[] ReadResource(string logicalName)
    {
        using Stream? stream = _assembly.GetManifestResourceStream(logicalName);
        if (stream == null)
            throw new FileNotFoundException(
                $"Embedded shader resource not found: '{logicalName}'. " +
                "Check HlslShaderCompile.V2.targets ran and the shader name is correct.");
        using var ms = new MemoryStream(checked((int)stream.Length));
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
