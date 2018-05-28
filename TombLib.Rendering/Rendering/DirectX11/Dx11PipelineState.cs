using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TombLib.Rendering.DirectX11
{
    public class Dx11PipelineState : IDisposable
    {
        private static Assembly ThisAssembly = Assembly.GetExecutingAssembly();
        public readonly VertexShader VertexShader;
        public readonly PixelShader PixelShader;
        public readonly InputLayout InputLayout;

        public Dx11PipelineState(Dx11RenderingDevice device, string shaderName, InputElement[] inputElements)
        {
			// Vertex shader
			using (Stream VertexShaderStream = ThisAssembly.GetManifestResourceStream("DxShaders." + shaderName + "VS"))
			{
				if (VertexShaderStream == null)
					throw new Exception("Vertex shader for \"" + shaderName + "\" not found.");
				byte[] VertexShaderBytes = new byte[VertexShaderStream.Length];
				VertexShaderStream.Read(VertexShaderBytes, 0, VertexShaderBytes.Length);
				VertexShader = new VertexShader(device.Device, VertexShaderBytes);

				// Input layout
				InputLayout = new InputLayout(device.Device, VertexShaderBytes, inputElements);
			}

			// Pixel shader
			using (Stream PixelShaderStream = ThisAssembly.GetManifestResourceStream("DxShaders." + shaderName + "PS"))
			{
				if (PixelShaderStream == null)
					throw new Exception("Pixel shader for \"" + shaderName + "\" not found.");
				byte[] PixelShaderBytes = new byte[PixelShaderStream.Length];
				PixelShaderStream.Read(PixelShaderBytes, 0, PixelShaderBytes.Length);
				PixelShader = new PixelShader(device.Device, PixelShaderBytes);
			}
        }

        public void Apply(DeviceContext context)
        {
            context.VertexShader.Set(VertexShader);
            context.PixelShader.Set(PixelShader);
            context.InputAssembler.InputLayout = InputLayout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        }

        public void Apply(DeviceContext context, RenderingStateBuffer stateBuffer0)
        {
            Apply(context);
            var dxStateBuffer0 = (Dx11RenderingStateBuffer)stateBuffer0;
            context.PixelShader.SetConstantBuffer(0, dxStateBuffer0.ConstantBuffer);
            context.VertexShader.SetConstantBuffer(0, dxStateBuffer0.ConstantBuffer);
        }

        public void Dispose()
        {
            VertexShader?.Dispose();
            PixelShader?.Dispose();
            InputLayout?.Dispose();
        }
    }
}
