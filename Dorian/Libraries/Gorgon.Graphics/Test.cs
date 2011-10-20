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

		private void Destroy()
		{
			if (_shaderCode != null)
				_shaderCode.Dispose();
			if (_effect != null)
				_effect.Dispose();
			if (_layout != null)
				_layout.Dispose();
			if (_vertices != null)
				_vertices.Dispose();
		}

		private void Initialize()
		{
			string errors = string.Empty;
			Shaders.ShaderFlags flags = Shaders.ShaderFlags.Debug;

			if (_swapChain.Settings.VideoDevice.FeatureLevelVersion == new Version(11, 0))
				_shader = Encoding.UTF8.GetString(Properties.Resources.TestTri11);
			if (_swapChain.Settings.VideoDevice.FeatureLevelVersion.Major == 10)
			{
				_shader = Encoding.UTF8.GetString(Properties.Resources.TestTri10);
				flags |= Shaders.ShaderFlags.EnableBackwardsCompatibility;
			}
			if (_swapChain.Settings.VideoDevice.FeatureLevelVersion == new Version(9, 3))
			{
				_shader = Encoding.UTF8.GetString(Properties.Resources.TestTri93);
				flags |= Shaders.ShaderFlags.EnableBackwardsCompatibility;
			}
			_device = _swapChain.Settings.VideoDevice.D3DDevice;

			_shaderCode = Shaders.ShaderBytecode.Compile(_shader, "fx_5_0", flags, Shaders.EffectFlags.None, null, null, out errors);
			_effect = new D3D.Effect(_device, _shaderCode);
			_pass = _effect.GetTechniqueByIndex(0).GetPassByIndex(0);
			_layout = new D3D.InputLayout(_device, _pass.Description.Signature, new[] {
			new D3D.InputElement("POSITION", 0, GI.Format.R32G32B32A32_Float, 0, 0),
			new D3D.InputElement("COLOR", 0, GI.Format.R32G32B32A32_Float, 16, 0) 
			});
			_layout.DebugName = _swapChain.Name + " Test Vertex Buffer";

			using (DataStream stream = new DataStream(3 * 32, true, true))
			{
				stream.WriteRange(new[] {
			new SlimDX.Vector4(0.0f, 1.0f, 1.0f, 1.0f), new SlimDX.Vector4(1.0f, 0.0f, 0.0f, 1.0f),
			new SlimDX.Vector4(1.0f, -1.0f, 1.0f, 1.0f), new SlimDX.Vector4(0.0f, 1.0f, 0.0f, 1.0f),
			new SlimDX.Vector4(-1.0f, -1.0f, 1.0f, 1.0f), new SlimDX.Vector4(0.0f, 0.0f, 1.0f, 1.0f)});
				stream.Position = 0;

				_vertices = new D3D.Buffer(_device, stream, new D3D.BufferDescription()
				{
					BindFlags = D3D.BindFlags.VertexBuffer,
					CpuAccessFlags = D3D.CpuAccessFlags.None,
					OptionFlags = D3D.ResourceOptionFlags.None,
					SizeInBytes = 3 * 32,
					Usage = D3D.ResourceUsage.Default
				});
				_vertices.DebugName = _swapChain.Name + " Test Vertex Buffer";
			}

			_binding = new D3D.VertexBufferBinding(_vertices, 32, 0);
			_form = Gorgon.GetTopLevelForm(_swapChain.Settings.Window);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Test"/> class.
		/// </summary>
		/// <param name="swapChain">The swap chain.</param>
		public Test(GorgonSwapChain swapChain)
		{
			_swapChain = swapChain;

			Gorgon.Log.Print("Test: Creating objects.", Diagnostics.GorgonLoggingLevel.Verbose);
			Initialize();
		}

		private bool _standBy = false;

		/// <summary>
		/// 
		/// </summary>
		public void Run()
		{
			SlimDX.Result result = GI.ResultCode.Success;

			if (_standBy)
			{
				result = _swapChain.GISwapChain.Present(0, GI.PresentFlags.Test);
				if (result == GI.ResultCode.Success)
					_standBy = false;
				else
				{
					//System.Threading.Thread.Sleep(10000);
					//_swapChain.UpdateSettings(true);
				}
			}
			else
			{
				_device.ImmediateContext.OutputMerger.SetTargets(_swapChain.D3DRenderTarget);
				_device.ImmediateContext.Rasterizer.SetViewports(_swapChain.D3DView);

				_device.ImmediateContext.ClearRenderTargetView(_swapChain.D3DRenderTarget, new SlimDX.Color4(1.0f, 1.0f, 1.0f, 1.0f));
				_device.ImmediateContext.InputAssembler.InputLayout = _layout;
				_device.ImmediateContext.InputAssembler.PrimitiveTopology = D3D.PrimitiveTopology.TriangleList;
				_device.ImmediateContext.InputAssembler.SetVertexBuffers(0, _binding);

				_pass.Apply(_device.ImmediateContext);
				_device.ImmediateContext.Draw(3, 0);

				result = _swapChain.GISwapChain.Present(0, GI.PresentFlags.None);
				if ((result != GI.ResultCode.Success) && (result.IsSuccess))
					_standBy = true;
				else
				{
					if (result != GI.ResultCode.Success)
						throw new Exception(result.Description);
				}
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
					Destroy();
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
