using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using GI = SlimDX.DXGI;
using D3D = SlimDX.Direct3D11;
using Shaders = SlimDX.D3DCompiler;

namespace GorgonLibrary.Graphics
{
	public class Test
		: IDisposable
	{
		private D3D.Device _device = null;
		private GorgonSwapChain _swapChain = null;
		private bool _disposed = false;
		private string _shader = string.Empty;
		private System.Windows.Forms.Form _form = null;

		private Shaders.ShaderBytecode _shaderCode = null;
		private D3D.Effect _effect = null;
		private D3D.InputLayout _layout = null;
		private D3D.Buffer _vertices = null;
		private D3D.EffectPass _pass = null;
		private D3D.VertexBufferBinding _binding = default(D3D.VertexBufferBinding);

		/// <summary>
		/// Initializes a new instance of the <see cref="Test"/> class.
		/// </summary>
		/// <param name="swapChain">The swap chain.</param>
		public Test(GorgonSwapChain swapChain)
		{
			string errors = string.Empty;
			Shaders.ShaderFlags flags = Shaders.ShaderFlags.Debug;

			_swapChain = swapChain;

			Gorgon.Log.Print("Test: Creating objects.", Diagnostics.GorgonLoggingLevel.Verbose);
			if (swapChain.Settings.VideoDevice.FeatureLevelVersion == new Version(11,0))
				_shader = "struct VS_IN\r\n{\r\n	float4 pos : POSITION;\r\n	float4 col : COLOR;\r\n};\r\n\r\nstruct PS_IN\r\n{\r\n	float4 pos : SV_POSITION;\r\n	float4 col : COLOR;\r\n};\r\n\r\nPS_IN VS( VS_IN input )\r\n{\r\n	PS_IN output = (PS_IN)0;\r\n	\r\n	output.pos = input.pos;\r\n	output.col = input.col;\r\n	\r\n	return output;\r\n}\r\n\r\nfloat4 PS( PS_IN input ) : SV_Target\r\n{\r\n	return input.col;\r\n}\r\n\r\ntechnique10 Rende\r\n{\r\n	pass P0\r\n	{\r\n		SetGeometryShader( 0 );\r\n		SetVertexShader( CompileShader( vs_5_0, VS() ) );\r\n		SetPixelShader( CompileShader( ps_5_0, PS() ) );\r\n	}\r\n}";
			if (swapChain.Settings.VideoDevice.FeatureLevelVersion.Major == 10)
			{
				_shader = "struct VS_IN\r\n{\r\n	float4 pos : POSITION;\r\n	float4 col : COLOR;\r\n};\r\n\r\nstruct PS_IN\r\n{\r\n	float4 pos : SV_POSITION;\r\n	float4 col : COLOR;\r\n};\r\n\r\nPS_IN VS( VS_IN input )\r\n{\r\n	PS_IN output = (PS_IN)0;\r\n	\r\n	output.pos = input.pos;\r\n	output.col = input.col;\r\n	\r\n	return output;\r\n}\r\n\r\nfloat4 PS( PS_IN input ) : SV_Target\r\n{\r\n	return input.col;\r\n}\r\n\r\ntechnique10 Rende\r\n{\r\n	pass P0\r\n	{\r\n		SetGeometryShader( 0 );\r\n		SetVertexShader( CompileShader( vs_4_0, VS() ) );\r\n		SetPixelShader( CompileShader( ps_4_0, PS() ) );\r\n	}\r\n}";
				flags |= Shaders.ShaderFlags.EnableBackwardsCompatibility;
			}
			if (swapChain.Settings.VideoDevice.FeatureLevelVersion == new Version(9, 3))
			{
				_shader = "struct VS_IN\r\n{\r\n	float4 pos : POSITION;\r\n	float4 col : COLOR;\r\n};\r\n\r\nstruct PS_IN\r\n{\r\n	float4 pos : SV_POSITION;\r\n	float4 col : COLOR;\r\n};\r\n\r\nPS_IN VS( VS_IN input )\r\n{\r\n	PS_IN output = (PS_IN)0;\r\n	\r\n	output.pos = input.pos;\r\n	output.col = input.col;\r\n	\r\n	return output;\r\n}\r\n\r\nfloat4 PS( PS_IN input ) : SV_Target\r\n{\r\n	return input.col;\r\n}\r\n\r\ntechnique10 Rende\r\n{\r\n	pass P0\r\n	{\r\n		SetGeometryShader( 0 );\r\n		SetVertexShader( CompileShader( vs_4_0_level_9_3, VS() ) );\r\n		SetPixelShader( CompileShader( ps_4_0_level_9_3, PS() ) );\r\n	}\r\n}";
				flags |= Shaders.ShaderFlags.EnableBackwardsCompatibility;
			}
			_device = swapChain.Settings.VideoDevice.D3DDevice;
			
			_shaderCode = Shaders.ShaderBytecode.Compile(_shader, "fx_5_0", flags, Shaders.EffectFlags.None, null, null, out errors );
			_effect = new D3D.Effect(_device, _shaderCode);
			_pass = _effect.GetTechniqueByIndex(0).GetPassByIndex(0);
			_layout = new D3D.InputLayout(_device, _pass.Description.Signature, new[] {
			new D3D.InputElement("POSITION", 0, GI.Format.R32G32B32A32_Float, 0, 0),
			new D3D.InputElement("COLOR", 0, GI.Format.R32G32B32A32_Float, 16, 0) 
			});

			using (DataStream stream = new DataStream(3 * 32, true, true))
			{
				stream.WriteRange(new[] {
			new SlimDX.Vector4(0.0f, 0.5f, 0.5f, 1.0f), new SlimDX.Vector4(1.0f, 0.0f, 0.0f, 1.0f),
			new SlimDX.Vector4(0.5f, -0.5f, 0.5f, 1.0f), new SlimDX.Vector4(0.0f, 1.0f, 0.0f, 1.0f),
			new SlimDX.Vector4(-0.5f, -0.5f, 0.5f, 1.0f), new SlimDX.Vector4(0.0f, 0.0f, 1.0f, 1.0f)});
				stream.Position = 0;

				_vertices = new D3D.Buffer(_device, stream, new D3D.BufferDescription()
				{
					BindFlags = D3D.BindFlags.VertexBuffer,
					CpuAccessFlags = D3D.CpuAccessFlags.None,
					OptionFlags = D3D.ResourceOptionFlags.None,
					SizeInBytes = 3 * 32,
					Usage = D3D.ResourceUsage.Default
				});
			}

			_binding = new D3D.VertexBufferBinding(_vertices, 32, 0);
			_form = Gorgon.GetTopLevelForm(_swapChain.Settings.Window);

			_form.Deactivate += new EventHandler(_form_Deactivate);
		}

		void _form_Deactivate(object sender, EventArgs e)
		{
			if (_form.WindowState == System.Windows.Forms.FormWindowState.Minimized)
			{
				_device.ImmediateContext.OutputMerger.SetTargets((D3D.RenderTargetView)null);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Run()
		{
			if (_form.WindowState != System.Windows.Forms.FormWindowState.Minimized)
			{
				_device.ImmediateContext.OutputMerger.SetTargets(_swapChain.D3DRenderTarget);
				_device.ImmediateContext.Rasterizer.SetViewports(_swapChain.D3DView);

				_device.ImmediateContext.ClearRenderTargetView(_swapChain.D3DRenderTarget, new SlimDX.Color4(1.0f, 1.0f, 1.0f, 1.0f));
				_device.ImmediateContext.InputAssembler.InputLayout = _layout;
				_device.ImmediateContext.InputAssembler.PrimitiveTopology = D3D.PrimitiveTopology.TriangleList;
				_device.ImmediateContext.InputAssembler.SetVertexBuffers(0, _binding);

				_pass.Apply(_device.ImmediateContext);
				_device.ImmediateContext.Draw(3, 0);

				_swapChain.GISwapChain.Present(0, GI.PresentFlags.None);
			}
		}

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Gorgon.Log.Print("Test: Destroy objects.", Diagnostics.GorgonLoggingLevel.Verbose);
					_device.ImmediateContext.OutputMerger.SetTargets((D3D.RenderTargetView)null);

					if (_shaderCode != null)
						_shaderCode.Dispose();
					if (_effect != null)
						_effect.Dispose();
					if (_layout != null)
						_layout.Dispose();
					if (_vertices != null)
						_vertices.Dispose();
				}

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
