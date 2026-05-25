using System;
using System.IO;
using System.Reflection;

namespace TombLib.Rendering.Graphics.Rhi;

/// <summary>
/// Loads shader bytecode from the assembly's embedded resources produced by
/// HlslShaderCompile.targets. Resource names follow the convention:
/// <c>Shaders.&lt;Name&gt;.&lt;vs|ps&gt;.&lt;dxbc|spv|glsl&gt;</c>.
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

        // Entry point name kept verbatim from HLSL — the build pipeline
        // invokes DXC with `-E vs_main` / `-E ps_main`, so SPIR-V exposes the
        // same names (not "main"). DX11 doesn't care since it reads the
        // entry name from the bytecode header.
        string entry = stage == ShaderStage.Vertex ? "vs_main" : "ps_main";

        byte[] dxbc  = ReadResource($"Shaders.{name}.{suffix}.dxbc");
        byte[] spirv = ReadResource($"Shaders.{name}.{suffix}.spv");
        byte[] glsl  = ReadResource($"Shaders.{name}.{suffix}.glsl");
        return new ShaderBytecode(stage, dxbc, spirv, glsl, entry);
    }

    private static byte[] ReadResource(string logicalName)
    {
        using Stream? stream = _assembly.GetManifestResourceStream(logicalName);
        if (stream == null)
            throw new FileNotFoundException(
                $"Embedded shader resource not found: '{logicalName}'. " +
                "Check HlslShaderCompile.targets ran and the shader name is correct.");
        using var ms = new MemoryStream(checked((int)stream.Length));
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
