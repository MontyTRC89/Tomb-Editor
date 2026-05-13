using System;
using System.Runtime.InteropServices;
using System.Text;
using Silk.NET.Shaderc;
using Silk.NET.Vulkan;

namespace TombLib.Rendering.Vulkan
{
    // Compiles inline GLSL strings to SPIR-V at runtime via shaderc, then wraps
    // the bytecode in a VkShaderModule. Used by every Drawing* class — keeps
    // the shader source physically next to the .cs that defines its vertex
    // layout / cbuffer struct, which is far easier to evolve than a separate
    // build-time .spv pipeline. Compilation runs once at Drawing* init; the
    // resulting modules live for the device's lifetime.
    public sealed class VulkanShaderCompiler : IDisposable
    {
        private readonly Shaderc _shaderc;
        private readonly unsafe Compiler* _compiler;

        public unsafe VulkanShaderCompiler()
        {
            _shaderc = Shaderc.GetApi();
            _compiler = _shaderc.CompilerInitialize();
            if (_compiler == null)
                throw new InvalidOperationException("Failed to initialise shaderc compiler.");
        }

        public unsafe void Dispose()
        {
            if (_compiler != null) _shaderc.CompilerRelease(_compiler);
            _shaderc?.Dispose();
        }

        // Compile GLSL source → SPIR-V bytecode. Throws on compile error with
        // the shaderc diagnostic message.
        public unsafe byte[] CompileGlslToSpirv(string source, ShaderKind kind, string name)
        {
            byte[] sourceBytes = Encoding.UTF8.GetBytes(source);
            CompileOptions* opts = _shaderc.CompileOptionsInitialize();
            try
            {
                _shaderc.CompileOptionsSetSourceLanguage(opts, SourceLanguage.Glsl);
                _shaderc.CompileOptionsSetTargetEnv(opts, TargetEnv.Vulkan, (uint)EnvVersion.Vulkan13);
                _shaderc.CompileOptionsSetOptimizationLevel(opts, OptimizationLevel.Performance);

                CompilationResult* result;
                fixed (byte* src = sourceBytes)
                fixed (byte* fileName = Encoding.UTF8.GetBytes(name + "\0"))
                fixed (byte* entry = "main\0"u8)
                {
                    result = _shaderc.CompileIntoSpv(_compiler,
                        src, (nuint)sourceBytes.Length, kind,
                        fileName, entry, opts);
                }

                try
                {
                    CompilationStatus status = _shaderc.ResultGetCompilationStatus(result);
                    if (status != CompilationStatus.Success)
                    {
                        byte* msg = _shaderc.ResultGetErrorMessage(result);
                        string err = Marshal.PtrToStringAnsi((IntPtr)msg) ?? "<no message>";
                        throw new InvalidOperationException($"shaderc compile failed ({status}) for {name}:\n{err}");
                    }

                    nuint len = _shaderc.ResultGetLength(result);
                    byte* bytes = _shaderc.ResultGetBytes(result);
                    byte[] spirv = new byte[(int)len];
                    Marshal.Copy((IntPtr)bytes, spirv, 0, (int)len);
                    return spirv;
                }
                finally
                {
                    _shaderc.ResultRelease(result);
                }
            }
            finally
            {
                _shaderc.CompileOptionsRelease(opts);
            }
        }

        // Build a VkShaderModule from SPIR-V bytecode. Caller owns the module's
        // lifetime — dispose it via Vk.DestroyShaderModule when done.
        public static unsafe ShaderModule CreateShaderModule(Vk vk, Device device, byte[] spirv)
        {
            fixed (byte* p = spirv)
            {
                ShaderModuleCreateInfo info = new ShaderModuleCreateInfo
                {
                    SType = StructureType.ShaderModuleCreateInfo,
                    CodeSize = (nuint)spirv.Length,
                    PCode = (uint*)p,
                };
                ShaderModule module;
                VkCheck.Ok(vk.CreateShaderModule(device, in info, null, &module));
                return module;
            }
        }
    }
}
