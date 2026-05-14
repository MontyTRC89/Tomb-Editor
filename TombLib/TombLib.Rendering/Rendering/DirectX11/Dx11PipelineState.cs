using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using System;
using System.IO;
using System.Reflection;
using Format = Silk.NET.DXGI.Format;

namespace TombLib.Rendering.DirectX11
{
    // Holds the trio (VS, PS, InputLayout) that makes a complete pipeline state for one
    // shader pair. Compiled HLSL bytecode is loaded from embedded resources named
    // "DxShaders.<shaderName>VS" / "<shaderName>PS" — see TombLib.Rendering.csproj's
    // EmbedShaderFilesTarget for how the .cso files end up under that prefix.
    //
    // The InputLayout is created from the VS bytecode (DXGI requires the VS's signature
    // to validate the IL); the InputElement[] passed in describes the slot layout the
    // caller will use when binding vertex buffers.
    //
    // Apply() sets VS/PS/IL and forces topology to TriangleList — this is currently the
    // ONLY topology used by the new path. Lines/points still go through the legacy
    // SharpDX.Toolkit path; that's something the Tappa-1 line/debug abstraction will fix.
    public unsafe class Dx11PipelineState : IDisposable
    {
        private static Assembly ThisAssembly = Assembly.GetExecutingAssembly();
        public readonly ID3D11VertexShader* VertexShader;
        public readonly ID3D11PixelShader* PixelShader;
        public readonly ID3D11InputLayout* InputLayout;

        public Dx11PipelineState(Dx11RenderingDevice device, string shaderName, Dx11InputElement[] inputElements)
        {
			// Vertex shader
			using (Stream VertexShaderStream = ThisAssembly.GetManifestResourceStream("DxShaders." + shaderName + "VS"))
			{
				if (VertexShaderStream == null)
					throw new Exception("Vertex shader for \"" + shaderName + "\" not found.");
				byte[] VertexShaderBytes = new byte[VertexShaderStream.Length];
				VertexShaderStream.Read(VertexShaderBytes, 0, VertexShaderBytes.Length);

                ID3D11VertexShader* vs;
                fixed (byte* ptr = VertexShaderBytes)
                {
                    SilkMarshal.ThrowHResult(
                        device.Device->CreateVertexShader(ptr, (nuint)VertexShaderBytes.Length, null, &vs));
                }
                VertexShader = vs;
                Dx11RenderingDevice.SetDebugName((ID3D11DeviceChild*)VertexShader, shaderName);

                // Input layout — convert Dx11InputElement[] to native InputElementDesc[].
                // SemanticName is a byte* that must stay alive through CreateInputLayout,
                // so we pin each string via SilkMarshal.StringToPtr and free after the call.
                int elementCount = inputElements.Length;
                InputElementDesc* descs = stackalloc InputElementDesc[elementCount];
                nint* namePtrs = stackalloc nint[elementCount];

                try
                {
                    for (int i = 0; i < elementCount; i++)
                    {
                        namePtrs[i] = SilkMarshal.StringToPtr(inputElements[i].SemanticName);
                        descs[i] = new InputElementDesc
                        {
                            SemanticName = (byte*)namePtrs[i],
                            SemanticIndex = inputElements[i].SemanticIndex,
                            Format = inputElements[i].Format,
                            AlignedByteOffset = inputElements[i].AlignedByteOffset,
                            InputSlot = inputElements[i].InputSlot,
                            InputSlotClass = inputElements[i].InputSlotClass,
                            InstanceDataStepRate = inputElements[i].InstanceDataStepRate,
                        };
                    }

                    ID3D11InputLayout* il;
                    fixed (byte* vsPtr = VertexShaderBytes)
                    {
                        SilkMarshal.ThrowHResult(
                            device.Device->CreateInputLayout(descs, (uint)elementCount, vsPtr, (nuint)VertexShaderBytes.Length, &il));
                    }
                    InputLayout = il;
                    Dx11RenderingDevice.SetDebugName((ID3D11DeviceChild*)InputLayout, shaderName);
                }
                finally
                {
                    for (int i = 0; i < elementCount; i++)
                    {
                        if (namePtrs[i] != 0)
                            SilkMarshal.Free(namePtrs[i]);
                    }
                }
            }

			// Pixel shader
			using (Stream PixelShaderStream = ThisAssembly.GetManifestResourceStream("DxShaders." + shaderName + "PS"))
			{
				if (PixelShaderStream == null)
					throw new Exception("Pixel shader for \"" + shaderName + "\" not found.");
				byte[] PixelShaderBytes = new byte[PixelShaderStream.Length];
				PixelShaderStream.Read(PixelShaderBytes, 0, PixelShaderBytes.Length);

                ID3D11PixelShader* ps;
                fixed (byte* ptr = PixelShaderBytes)
                {
                    SilkMarshal.ThrowHResult(
                        device.Device->CreatePixelShader(ptr, (nuint)PixelShaderBytes.Length, null, &ps));
                }
                PixelShader = ps;
                Dx11RenderingDevice.SetDebugName((ID3D11DeviceChild*)PixelShader, shaderName);
            }
        }

        public void Apply(ID3D11DeviceContext* context)
        {
            context->VSSetShader(VertexShader, null, 0);
            context->PSSetShader(PixelShader, null, 0);
            context->IASetInputLayout(InputLayout);
            context->IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
        }

        public void Apply(ID3D11DeviceContext* context, RenderingStateBuffer stateBuffer0)
        {
            Apply(context);
            var dxStateBuffer0 = (Dx11RenderingStateBuffer)stateBuffer0;
            ID3D11Buffer* cb = dxStateBuffer0.ConstantBuffer;
            context->PSSetConstantBuffers(0, 1, &cb);
            context->VSSetConstantBuffers(0, 1, &cb);
        }

        public void Dispose()
        {
            if (VertexShader != null) VertexShader->Release();
            if (PixelShader != null) PixelShader->Release();
            if (InputLayout != null) InputLayout->Release();
        }
    }
}
