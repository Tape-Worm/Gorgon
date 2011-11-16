using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimDX;
using GorgonLibrary.Math;
using GI = SlimDX.DXGI;
using D3D = SlimDX.Direct3D11;
using Shaders = SlimDX.D3DCompiler;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 128, Pack=1)]
	public struct MatrixBuffer
	{
		/// <summary>
		/// /
		/// </summary>
		[FieldOffset(0)]
		public SlimDX.Matrix Projection;
		/// <summary>
		/// 
		/// </summary>
		[FieldOffset(64)]
		public SlimDX.Matrix View;
	}

	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 80,Pack =1)]
	public struct UpdateBuffer
	{
		/// <summary>
		/// 
		/// </summary>
		[FieldOffset(0)]
		public SlimDX.Matrix World;
		/// <summary>
		/// 
		/// </summary>
		[FieldOffset(64)]
		public float Alpha;
		///// <summary>
		///// Padding because the struct must be divisble by 16.
		///// </summary>
		//[FieldOffset(68)]
		//public long Dummy1;
		///// <summary>
		///// 
		///// </summary>
		//[FieldOffset(76)]
		//public int Dummy2;
	}

	/// <summary>
	/// 
	/// </summary>
	public class Test
		: IDisposable
	{
		private D3D.Device _device = null;
		private GorgonSwapChain _swapChain = null;
		private bool _disposed = false;
		private string _shader = string.Empty;
		private System.Windows.Forms.Form _form = null;

		private Shaders.ShaderBytecode _vsShaderCode = null;
		private Shaders.ShaderBytecode _psShaderCode = null;
		//private D3D.Effect _effect = null;
		private D3D.InputLayout _layout = null;
		private D3D.Buffer _vertices = null;
		private D3D.Buffer _index = null;
		//private D3D.EffectPass _pass = null;		
		private D3D.RasterizerState _rastState = null;
		private D3D.VertexBufferBinding _binding = default(D3D.VertexBufferBinding);
		private float _rot = 0.0f;
		private float _degreesPerSecond = 0.0f;
		//private D3D.EffectScalarVariable _alpha = null;
		private D3D.BlendState _blend = null;
		//private D3D.EffectConstantBuffer _matrix = null;		
		private float _currentTime = 0;
		private int _maxPasses = 0;
		private bool _timeSwitch = false;
		private float _passes = 8.0f;
		private D3D.Texture2D _texture = null;		
		private D3D.ShaderResourceView _textureView = null;
		private D3D.SamplerState _sampler = null;
		private D3D.VertexShader _vs = null;
		private D3D.PixelShader _ps = null;
		private D3D.Buffer _changeBuffer = null;
		private D3D.Buffer _noChangeBuffer = null;
		private SlimDX.DataStream _changeStream = null;
			

		struct vertex
		{
			public SlimDX.Vector4 Position;
			public SlimDX.Vector4 Color;
			public SlimDX.Vector2 UV;
		}


		private void Destroy()
		{
			_swapChain.Settings.Window.Resize -= new EventHandler(Window_Resize);

			if (_textureView != null)
				_textureView.Dispose();
			if (_changeStream != null)
				_changeStream.Dispose();
			if (_changeBuffer != null)
				_changeBuffer.Dispose();
			if (_noChangeBuffer != null)
				_noChangeBuffer.Dispose();
			if (_blend != null)
				_blend.Dispose();
			if (_rastState != null)
				_rastState.Dispose();
			if (_vsShaderCode != null)
				_vsShaderCode.Dispose();
			if (_psShaderCode != null)
				_psShaderCode.Dispose();
			if (_vs != null)
				_vs.Dispose();
			if (_ps != null)
				_ps.Dispose();
			//if (_effect != null)
			//    _effect.Dispose();
			if (_layout != null)
				_layout.Dispose();
			if (_texture != null)
				_texture.Dispose();
			if (_sampler != null)
				_sampler.Dispose();
			if (_vertices != null)
				_vertices.Dispose();
			if (_index != null)
				_index.Dispose();
		}

		private void Initialize()
		{
			string errors = string.Empty;
			Shaders.ShaderFlags flags = Shaders.ShaderFlags.Debug;

			_swapChain.Settings.Window.Resize += new EventHandler(Window_Resize);

			if ((_swapChain.Graphics.VideoDevice.HardwareFeatureLevels & DeviceFeatureLevel.SM5) == DeviceFeatureLevel.SM5)
				_shader = Encoding.UTF8.GetString(Properties.Resources.TestTri11);
			if (((_swapChain.Graphics.VideoDevice.HardwareFeatureLevels & DeviceFeatureLevel.SM4_1) == DeviceFeatureLevel.SM4_1) ||
				((_swapChain.Graphics.VideoDevice.HardwareFeatureLevels & DeviceFeatureLevel.SM4) == DeviceFeatureLevel.SM4))
			{
				_shader = Encoding.UTF8.GetString(Properties.Resources.TestTri10);
				flags |= Shaders.ShaderFlags.EnableBackwardsCompatibility;
			}
			if ((_swapChain.Graphics.VideoDevice.HardwareFeatureLevels & DeviceFeatureLevel.SM2_a_b) == DeviceFeatureLevel.SM2_a_b)
			{
				_shader = Encoding.UTF8.GetString(Properties.Resources.TestTri93);
				flags |= Shaders.ShaderFlags.EnableBackwardsCompatibility;
			}
			_device = _swapChain.Graphics.VideoDevice.D3DDevice;
			
			
			_vsShaderCode = Shaders.ShaderBytecode.Compile(_shader, "VS", "vs_4_0_level_9_3",  flags, Shaders.EffectFlags.None, null, null, out errors);
			_vs = new D3D.VertexShader(_device, _vsShaderCode);
			_psShaderCode = Shaders.ShaderBytecode.Compile(_shader, "PS", "ps_4_0_level_9_3", flags, Shaders.EffectFlags.None, null, null, out errors);
			_ps = new D3D.PixelShader(_device, _psShaderCode);

			_layout = new D3D.InputLayout(_device, _vsShaderCode, new[] {
			new D3D.InputElement("POSITION", 0, GI.Format.R32G32B32A32_Float, 0, 0),
			new D3D.InputElement("COLOR", 0, GI.Format.R32G32B32A32_Float, 16, 0),
			new D3D.InputElement("TEXTURECOORD", 0, GI.Format.R32G32_Float, 32, 0)
			});

			int vertexSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(vertex));
			_layout.DebugName = _swapChain.Name + " Test Vertex Buffer";

			using (DataStream stream = new DataStream(4 * vertexSize, true, true))
			{
				stream.WriteRange(new vertex[] {
			new vertex() { Position = new SlimDX.Vector4(-0.5f, 0.5f, 0.0f, 0.0f), Color = new SlimDX.Vector4(1.0f, 1.0f, 1.0f, 1.0f), UV = new SlimDX.Vector2(0, 0)},
			new vertex() { Position = new SlimDX.Vector4(0.5f, 0.5f, 0.0f, 0.0f), Color = new SlimDX.Vector4(1.0f, 1.0f, 1.0f, 1.0f), UV = new SlimDX.Vector2(1.0f, 0)},
			new vertex() { Position = new SlimDX.Vector4(-0.5f, -0.5f, 0.0f, 0.0f), Color = new SlimDX.Vector4(1.0f, 1.0f, 1.0f, 1.0f), UV = new SlimDX.Vector2(0, 1.0f)},
				new vertex() { Position = new SlimDX.Vector4(0.5f, -0.5f, 0.12f, 0.0f), Color = new SlimDX.Vector4(1.0f, 1.0f, 1.0f, 1.0f), UV = new SlimDX.Vector2(1.0f, 1.0f)}});
				stream.Position = 0;

				_vertices = new D3D.Buffer(_device, stream, new D3D.BufferDescription()
				{
					BindFlags = D3D.BindFlags.VertexBuffer,
					CpuAccessFlags = D3D.CpuAccessFlags.None,
					OptionFlags = D3D.ResourceOptionFlags.None,
					SizeInBytes = 4 * vertexSize,
					Usage = D3D.ResourceUsage.Default
				});
				_vertices.DebugName = _swapChain.Name + " Test Vertex Buffer";
			}

			using (DataStream stream = new DataStream(6 * 4, true, true))
			{
				stream.WriteRange(new int[] {
					0, 1, 2, 2, 1, 3
				});
				stream.Position = 0;

				_index = new D3D.Buffer(_device, stream, new D3D.BufferDescription()
				{
					BindFlags = D3D.BindFlags.IndexBuffer,
					CpuAccessFlags = D3D.CpuAccessFlags.None,
					OptionFlags = D3D.ResourceOptionFlags.None,
					SizeInBytes = 6 * 4,
					Usage = D3D.ResourceUsage.Default				
				});
				_index.DebugName = _swapChain.Name + " Test Index Buffer";
			}

			_texture = D3D.Texture2D.FromFile(_device, @"..\..\..\..\Resources\Images\VBback.jpg");

			_textureView = new D3D.ShaderResourceView(_device, _texture);
						
			D3D.SamplerDescription sampleDesc = new D3D.SamplerDescription();
			sampleDesc.AddressU = D3D.TextureAddressMode.Clamp;
			sampleDesc.AddressV = D3D.TextureAddressMode.Clamp;
			sampleDesc.AddressW = D3D.TextureAddressMode.Clamp;
			sampleDesc.BorderColor = new SlimDX.Color4(System.Drawing.Color.Black);
			sampleDesc.ComparisonFunction = D3D.Comparison.Never;
			sampleDesc.Filter = D3D.Filter.MinMagMipLinear;
			sampleDesc.MaximumAnisotropy = 16;
			sampleDesc.MaximumLod = 3.402823466e+38f;
			sampleDesc.MinimumLod = 0;
			sampleDesc.MipLodBias = 0.0f;

			_sampler = D3D.SamplerState.FromDescription(_device, sampleDesc);			

			_binding = new D3D.VertexBufferBinding(_vertices, vertexSize, 0);
			_form = Gorgon.GetTopLevelForm(_swapChain.Settings.Window);

			D3D.RasterizerStateDescription desc = new D3D.RasterizerStateDescription();
			desc.FillMode = D3D.FillMode.Solid;
			desc.CullMode = D3D.CullMode.None;
			desc.IsFrontCounterclockwise = false;
			desc.DepthBias = 0;
			desc.SlopeScaledDepthBias = 0;
			desc.DepthBiasClamp = 0;
			desc.IsDepthClipEnabled = true;
			desc.IsScissorEnabled = false;
			desc.IsMultisampleEnabled = true;
			desc.IsAntialiasedLineEnabled = false;
			_rastState = D3D.RasterizerState.FromDescription(_device, desc);


			MatrixBuffer matrix = new MatrixBuffer();
			matrix.Projection = Matrix.Transpose(Matrix.PerspectiveFovLH(GorgonLibrary.Math.GorgonMathUtility.Radians(75.0f), (float)_swapChain.Settings.VideoMode.Value.Width / (float)_swapChain.Settings.VideoMode.Value.Height, 0.1f, 1000.0f));
			//matrix.Projection = Matrix.Identity;
			matrix.View = Matrix.Transpose(Matrix.LookAtLH(new Vector3(0, 0, 0.75f), new Vector3(0, 0, -1.0f), Vector3.UnitY));
			//SlimDX.Matrix.Transpose(ref matrix.Projection, out matrix.Projection);
			//SlimDX.Matrix.Transpose(ref matrix.View, out matrix.View);

			using (DataStream noChangeStream = new DataStream(Marshal.SizeOf(typeof(MatrixBuffer)), true, true))
			{
				noChangeStream.Write<MatrixBuffer>(matrix);
				noChangeStream.Position = 0;
				_noChangeBuffer = new D3D.Buffer(_device, noChangeStream, new D3D.BufferDescription()
					{
						BindFlags = D3D.BindFlags.ConstantBuffer,
						CpuAccessFlags = D3D.CpuAccessFlags.None,
						OptionFlags = D3D.ResourceOptionFlags.None,
						SizeInBytes = Marshal.SizeOf(typeof(MatrixBuffer)),
						StructureByteStride = 0,
						Usage = D3D.ResourceUsage.Default
					});
			}

			UpdateBuffer updatebuffer = new UpdateBuffer();
			updatebuffer.World = SlimDX.Matrix.Identity;
			//SlimDX.Matrix.Transpose(ref updatebuffer.World, out updatebuffer.World);
			updatebuffer.Alpha = 1.0f;
			_changeStream = new DataStream(Marshal.SizeOf(typeof(UpdateBuffer)), true, true);
			_changeStream.Write<UpdateBuffer>(updatebuffer);
			_changeStream.Position = 0;

			_changeBuffer = new D3D.Buffer(_device, _changeStream, new D3D.BufferDescription()
			{
				BindFlags = D3D.BindFlags.ConstantBuffer,
				CpuAccessFlags = D3D.CpuAccessFlags.None,
				OptionFlags = D3D.ResourceOptionFlags.None,
				SizeInBytes = Marshal.SizeOf(typeof(UpdateBuffer)),
				StructureByteStride = 0,
				Usage = D3D.ResourceUsage.Default
			});

			_changeStream.Position = 0;
			_device.ImmediateContext.UpdateSubresource(new DataBox(0, 0, _changeStream), _changeBuffer, 0);
			
			CreateBlend();
		}

		/// <summary>
		/// Handles the Resize event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void Window_Resize(object sender, EventArgs e)
		{
			MatrixBuffer matrix = new MatrixBuffer();
			matrix.Projection = Matrix.Transpose(Matrix.PerspectiveFovLH(GorgonLibrary.Math.GorgonMathUtility.Radians(75.0f), (float)_swapChain.Settings.VideoMode.Value.Width / (float)_swapChain.Settings.VideoMode.Value.Height, 0.1f, 1000.0f));
			matrix.View = Matrix.Transpose(Matrix.LookAtLH(new Vector3(0, 0, 0.75f), new Vector3(0, 0, -1.0f), Vector3.UnitY));
			using (DataStream noChangeStream = new DataStream(Marshal.SizeOf(typeof(MatrixBuffer)), true, true))
			{
				noChangeStream.Write<MatrixBuffer>(matrix);
				noChangeStream.Position = 0;
				_device.ImmediateContext.UpdateSubresource(new DataBox(0, 0, noChangeStream), _noChangeBuffer, 0);
			}
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
		/// Transform.
		/// </summary>
		/// <param name="delta"></param>
		public void Transform(float delta)
		{
			_degreesPerSecond = GorgonLibrary.Math.GorgonMathUtility.Abs((GorgonLibrary.Math.GorgonMathUtility.Cos(GorgonLibrary.Math.GorgonMathUtility.Radians(_rot)) * _currentTime)) + 5.0f;

			_rot += (_degreesPerSecond * delta);
			if (_rot > 360.0f)
			{			
				_rot = 0.0f;
				_timeSwitch = !_timeSwitch;
			}

			if (!_timeSwitch)
				_currentTime += delta * 45.0f;
			else
				_currentTime -= delta * 45.0f;

			if (_degreesPerSecond > 360.0f)
				_currentTime = 0.0f;

			_maxPasses += 1;
			if (_maxPasses > (int)_passes)
				_maxPasses = (int)_passes;
		}

		/// <summary>
		/// Creates the blend.
		/// </summary>
		public void CreateBlend()
		{
			D3D.BlendStateDescription desc;

			if (_blend != null)
			{
				_blend.Dispose();
				_blend = null;
			}

			desc = new D3D.BlendStateDescription();
			desc.AlphaToCoverageEnable = false;
			desc.IndependentBlendEnable = false;
			desc.RenderTargets[0].BlendEnable = true;
			desc.RenderTargets[0].BlendOperation = D3D.BlendOperation.Add;
			desc.RenderTargets[0].BlendOperationAlpha = D3D.BlendOperation.Add;
			desc.RenderTargets[0].DestinationBlend = D3D.BlendOption.InverseSourceAlpha;
			desc.RenderTargets[0].DestinationBlendAlpha = D3D.BlendOption.Zero;
			desc.RenderTargets[0].SourceBlend = D3D.BlendOption.SourceAlpha;
			desc.RenderTargets[0].SourceBlendAlpha = D3D.BlendOption.Zero;
			desc.RenderTargets[0].RenderTargetWriteMask = D3D.ColorWriteMaskFlags.All;

			_blend = D3D.BlendState.FromDescription(_device, desc);
		}

		/// <summary>
		/// 
		/// </summary>
		public void Draw()
		{
			SlimDX.Result result = GI.ResultCode.Success;
			UpdateBuffer buffer = new UpdateBuffer();			
			

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
				_device.ImmediateContext.ClearRenderTargetView(_swapChain.D3DRenderTarget, new SlimDX.Color4(1.0f, 0.0f, 0.0f, 0.0f));
				_device.ImmediateContext.Rasterizer.State = _rastState;
				_device.ImmediateContext.OutputMerger.BlendState = _blend;
				_device.ImmediateContext.OutputMerger.SetTargets(_swapChain.D3DRenderTarget);
				_device.ImmediateContext.Rasterizer.SetViewports(_swapChain.D3DView);

				_device.ImmediateContext.InputAssembler.InputLayout = _layout;
				_device.ImmediateContext.InputAssembler.PrimitiveTopology = D3D.PrimitiveTopology.TriangleList;
				_device.ImmediateContext.InputAssembler.SetVertexBuffers(0, _binding);
				_device.ImmediateContext.InputAssembler.SetIndexBuffer(_index, GI.Format.R32_UInt, 0);

				_device.ImmediateContext.VertexShader.Set(_vs);
				_device.ImmediateContext.PixelShader.Set(_ps);

				_device.ImmediateContext.VertexShader.SetConstantBuffer(_noChangeBuffer, 0);
				_device.ImmediateContext.VertexShader.SetConstantBuffer(_changeBuffer, 1);
				_device.ImmediateContext.PixelShader.SetConstantBuffer(_changeBuffer, 1);
				_device.ImmediateContext.PixelShader.SetShaderResource(_textureView, 0);
				_device.ImmediateContext.PixelShader.SetSampler(_sampler, 0);

				float passAngle = GorgonLibrary.Math.GorgonMathUtility.Radians(_rot - (_passes - (_passes * (_degreesPerSecond / GorgonLibrary.Math.GorgonMathUtility.Pow(_passes, 2.25f)))));
				buffer.World = Matrix.RotationZ(passAngle);
				buffer.World = SlimDX.Matrix.Multiply(buffer.World, SlimDX.Matrix.RotationZ(passAngle));
				buffer.World = SlimDX.Matrix.Multiply(buffer.World, SlimDX.Matrix.RotationY(passAngle));
				buffer.World = Matrix.Transpose(buffer.World);
				buffer.Alpha = 1.0f;
				_changeStream.Position = 0;
				_changeStream.Write<UpdateBuffer>(buffer);
				_changeStream.Position = 0;
				_device.ImmediateContext.UpdateSubresource(new DataBox(0, 0, _changeStream), _changeBuffer, 0);
				_device.ImmediateContext.DrawIndexed(6, 0, 0);

				buffer.Alpha = 0.0f;
				//step = 1.0f;
				for (int i = 0; i < (int)_passes; i++)
				{
					passAngle = 0.0f;

					passAngle = GorgonLibrary.Math.GorgonMathUtility.Radians(_rot - (_passes - (i * (_degreesPerSecond / GorgonLibrary.Math.GorgonMathUtility.Pow(_passes, 2.25f)))));					

					buffer.Alpha += (1.0f / (_passes * 2.5f));
					buffer.World = Matrix.RotationZ(passAngle);
					buffer.World = SlimDX.Matrix.Multiply(buffer.World, SlimDX.Matrix.RotationZ(passAngle));
					buffer.World = SlimDX.Matrix.Multiply(buffer.World, SlimDX.Matrix.RotationY(passAngle));
					buffer.World = Matrix.Transpose(buffer.World);
					_changeStream.Position = 0;
					_changeStream.Write<UpdateBuffer>(buffer);
					_changeStream.Position = 0;
					_device.ImmediateContext.UpdateSubresource(new DataBox(0, 0, _changeStream), _changeBuffer, 0);

					_device.ImmediateContext.DrawIndexed(6, 0, 0);
				}

				//buffer.World = Matrix.Identity;
				//buffer.World *= Matrix.Scaling(0.5f, 0.5f, 1.0f);
				//buffer.World *= Matrix.Translation(0.5f, 0.25f, 0.0f);
				//buffer.World = Matrix.Transpose(buffer.World);
				//buffer.Alpha = 1.0f;
				//_changeStream.Position = 0;
				//_changeStream.Write<UpdateBuffer>(buffer);
				//_changeStream.Position = 0;
				//_device.ImmediateContext.UpdateSubresource(new DataBox(0, 0, _changeStream), _changeBuffer, 0);
				//_device.ImmediateContext.DrawIndexed(6, 0, 0);

				//System.Threading.Thread.Sleep(16);
				
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
