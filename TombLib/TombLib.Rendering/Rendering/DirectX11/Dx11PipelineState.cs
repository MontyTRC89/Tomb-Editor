using Vortice.Direct3D;
using Vortice.Direct3D11;
using System;
using System.IO;
using System.Reflection;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11PipelineState : IDisposable
    {
        private static readonly Assembly ThisAssembly = Assembly.GetExecutingAssembly();
        public readonly ID3D11VertexShader VertexShader;
        public readonly ID3D11PixelShader PixelShader;
        public readonly ID3D11InputLayout InputLayout;

        public Dx11PipelineState(Dx11RenderingDevice device, string shaderName, InputElementDescription[] inputElements)
        {
            // Vertex shader
            using (Stream vertexShaderStream = ThisAssembly.GetManifestResourceStream("DxShaders." + shaderName + "VS"))
            {
                if (vertexShaderStream == null)
                    throw new Exception("Vertex shader for \"" + shaderName + "\" not found.");
                byte[] vertexShaderBytes = new byte[vertexShaderStream.Length];
                vertexShaderStream.Read(vertexShaderBytes, 0, vertexShaderBytes.Length);
                VertexShader = device.Device.CreateVertexShader(vertexShaderBytes);

                // Input layout
                InputLayout = device.Device.CreateInputLayout(inputElements, vertexShaderBytes);
            }

            // Pixel shader
            using (Stream pixelShaderStream = ThisAssembly.GetManifestResourceStream("DxShaders." + shaderName + "PS"))
            {
                if (pixelShaderStream == null)
                    throw new Exception("Pixel shader for \"" + shaderName + "\" not found.");
                byte[] pixelShaderBytes = new byte[pixelShaderStream.Length];
                pixelShaderStream.Read(pixelShaderBytes, 0, pixelShaderBytes.Length);
                PixelShader = device.Device.CreatePixelShader(pixelShaderBytes);
            }
        }

        public void Apply(ID3D11DeviceContext context)
        {
            context.VSSetShader(VertexShader);
            context.PSSetShader(PixelShader);
            context.IASetInputLayout(InputLayout);
            context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
        }

        public void Apply(ID3D11DeviceContext context, RenderingStateBuffer stateBuffer0)
        {
            Apply(context);
            var dxStateBuffer0 = (Dx11RenderingStateBuffer)stateBuffer0;
            context.PSSetConstantBuffer(0, dxStateBuffer0.ConstantBuffer);
            context.VSSetConstantBuffer(0, dxStateBuffer0.ConstantBuffer);
        }

        public void Dispose()
        {
            VertexShader?.Dispose();
            PixelShader?.Dispose();
            InputLayout?.Dispose();
        }
    }
}
