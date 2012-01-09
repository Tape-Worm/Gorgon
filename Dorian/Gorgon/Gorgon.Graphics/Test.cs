using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpDX;
using GorgonLibrary.Math;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using D3DCommon = SharpDX.Direct3D;
using Shaders = SharpDX.D3DCompiler;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 176, Pack=1)]
	public struct MatrixBuffer
	{
		/// <summary>
		/// /
		/// </summary>
		[FieldOffset(0)]
		public SharpDX.Matrix Projection;
		/// <summary>
		/// 
		/// </summary>
		[FieldOffset(64)]
		public SharpDX.Matrix View;
		/// <summary>
		///
		/// </summary>
		[FieldOffset(128), MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
		public Vector4D[] Array;
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
		public SharpDX.Matrix World;
		/// <summary>
		/// 
		/// </summary>
		[FieldOffset(64)]
		public Vector4D Alpha;
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
		private GorgonGraphics _graphics = null;
		private GorgonSwapChain _swapChain = null;
		private bool _disposed = false;
		private string _shader = string.Empty;
		private System.Windows.Forms.Form _form = null;

		//private Shaders.ShaderBytecode _vsShaderCode = null;
		//private Shaders.ShaderBytecode _psShaderCode = null;
		//private D3D.Effect _effect = null;
		//private D3D.InputLayout _layout = null;
		private D3D.Buffer _vertices = null;
		private D3D.Buffer _index = null;
		//private D3D.EffectPass _pass = null;		
		private D3D.VertexBufferBinding _binding = default(D3D.VertexBufferBinding);
		private float _rot = 0.0f;
		private float _degreesPerSecond = 0.0f;
		//private D3D.EffectScalarVariable _alpha = null;
		//private D3D.EffectConstantBuffer _matrix = null;		
		private float _currentTime = 0;
		private int _maxPasses = 0;
		private bool _timeSwitch = false;
		private float _passes = 8.0f;
		private D3D.Texture2D _texture = null;		
		private D3D.ShaderResourceView _textureView = null;
		//private D3D.SamplerState _sampler = null;
		//private D3D.VertexShader _vs = null;
		//private D3D.PixelShader _ps = null;
		//private D3D.Buffer _changeBuffer = null;
		private GorgonConstantBuffer _noChangeBuffer = null;
		private GorgonConstantBuffer _changeBuffer = null;
		//private D3D.Buffer _noChangeBuffer = null;
		//private GorgonDepthStencilState _depthStateAlpha = null;
		private GorgonDepthStencilStates _depthStateAlpha = GorgonDepthStencilStates.DefaultStates;
		//private SharpDX.DataStream _changeStream = null;
		Random _rnd = new Random();
		private GorgonVertexShader _vs = null;
		private GorgonPixelShader _ps = null;		
			
		
		struct vertex
		{
			[InputElement(0, "POSITION")]
			public Vector4D Position;
			[InputElement(1, "COLOR")]
			public Vector4D Color;
			[InputElement(2, "TEXTURECOORD")]
			public Vector2D UV;
		}


		private void Destroy()
		{
			_swapChain.Settings.Window.Resize -= new EventHandler(Window_Resize);

			//if (_depthStateAlpha != null)
				//_depthStateAlpha.Dispose();
			if (_textureView != null)
				_textureView.Dispose();
			//if (_changeStream != null)
			//    _changeStream.Dispose();
			//if (_changeBuffer != null)
			//    _changeBuffer.Dispose();
			//if (_noChangeBuffer != null)
			//    _noChangeBuffer.Dispose();
			//if (_vsShaderCode != null)
				//_vsShaderCode.Dispose();
			//if (_psShaderCode != null)
			//    _psShaderCode.Dispose();
			//if (_vs != null)
				//_vs.Dispose();
			//if (_ps != null)
			//    _ps.Dispose();
			//if (_effect != null)
			//    _effect.Dispose();
			//if (_layout != null)
				//_layout.Dispose();
			if (_texture != null)
			    _texture.Dispose();
			//if (_sampler != null)
				//_sampler.Dispose();
			if (_vertices != null)
				_vertices.Dispose();
			if (_index != null)
				_index.Dispose();
		}

		private void Initialize()
		{
			string errors = string.Empty;

			_graphics = _swapChain.Graphics;
			_swapChain.Settings.Window.Resize += new EventHandler(Window_Resize);

			_shader = Encoding.UTF8.GetString(Properties.Resources.Test);

			_device = _graphics.VideoDevice.D3DDevice;
			
			_vs = _graphics.CreateVertexShader("TestVShader", "VS", _shader);
			_ps = _graphics.CreatePixelShader("TestPShader", "PS", _shader);
						
			GorgonInputLayout layout = _graphics.CreateInputLayout("Test Layout", typeof(vertex), _vs);

			int vertexSize = layout.Size;			

			using (DataStream stream = new DataStream(4 * vertexSize, true, true))
			{
				stream.WriteRange(new vertex[] {
				new vertex() { Position = new Vector4D(-0.5f, 0.5f, 0.0f, 0.0f), Color = new Vector4D(1.0f, 1.0f, 1.0f, 1.0f), UV = new Vector2D(0, 0)},
				new vertex() { Position = new Vector4D(0.5f, 0.5f, 0.0f, 0.0f), Color = new Vector4D(1.0f, 1.0f, 1.0f, 1.0f), UV = new Vector2D(1.0f, 0)},
				new vertex() { Position = new Vector4D(-0.5f, -0.5f, 0.0f, 0.0f), Color = new Vector4D(1.0f, 1.0f, 1.0f, 1.0f), UV = new Vector2D(0, 1.0f)},
				new vertex() { Position = new Vector4D(0.5f, -0.5f, 0.12f, 0.0f), Color = new Vector4D(1.0f, 1.0f, 1.0f, 1.0f), UV = new Vector2D(1.0f, 1.0f)}});
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
					2, 1, 0, 2, 3, 1
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

			D3D.ImageLoadInformation info = new D3D.ImageLoadInformation();
			info.BindFlags = D3D.BindFlags.ShaderResource;
			info.CpuAccessFlags = D3D.CpuAccessFlags.None;
			info.Depth = 0;
			info.Filter = D3D.FilterFlags.None;
			info.FirstMipLevel = 0;
			info.Format = GI.Format.B8G8R8A8_UNorm;
			info.Height = 0;
			info.Width = 0;
			info.MipFilter = D3D.FilterFlags.None;
			info.MipLevels = 1;
			info.OptionFlags = D3D.ResourceOptionFlags.None;
			info.Usage = D3D.ResourceUsage.Default;

			_texture = D3D.Resource.FromFile<D3D.Texture2D>(_device, @"..\..\..\..\Resources\Images\VBback.jpg", info);
			_texture.DebugName = _swapChain.Name + " Test texture.";
			_textureView = new D3D.ShaderResourceView(_device, _texture);
			_textureView.DebugName = _swapChain.Name + " Test texture view.";

			//D3D.SamplerStateDescription sampleDesc = new D3D.SamplerStateDescription();
			//sampleDesc.AddressU = D3D.TextureAddressMode.Clamp;
			//sampleDesc.AddressV = D3D.TextureAddressMode.Clamp;
			//sampleDesc.AddressW = D3D.TextureAddressMode.Clamp;
			//sampleDesc.BorderColor = System.Drawing.Color.Black;
			//sampleDesc.ComparisonFunction = D3D.Comparison.Never;
			//sampleDesc.Filter = D3D.Filter.MinMagMipLinear;
			//sampleDesc.MaximumAnisotropy = 16;
			//sampleDesc.MaximumLod = 3.402823466e+38f;
			//sampleDesc.MinimumLod = 0;
			//sampleDesc.MipLodBias = 0.0f;

			//_sampler = new D3D.SamplerState(_device, sampleDesc);
			//_sampler.DebugName = _swapChain + " Test sampler";

			_binding = new D3D.VertexBufferBinding(_vertices, vertexSize, 0);
			_form = Gorgon.GetTopLevelForm(_swapChain.Settings.Window);

			MatrixBuffer matrix = new MatrixBuffer();
			matrix.Projection = Matrix.Transpose(Matrix.PerspectiveFovLH(GorgonLibrary.Math.GorgonMathUtility.Radians(75.0f), (float)(_swapChain.Settings.VideoMode.Width) / (float)(_swapChain.Settings.VideoMode.Height), 0.1f, 1000.0f));
			//matrix.Projection = Matrix.Identity;
			matrix.View = Matrix.Transpose(Matrix.LookAtLH(new Vector3(0, 0, 0.75f), new Vector3(0, 0, -1.0f), Vector3.UnitY));
			matrix.Array = new Vector4D[3];
			matrix.Array[0] = new Vector4D(1.0f, 1.0f, 1.0f, 1.0f);
			matrix.Array[1] = new Vector4D(1.0f, 1.0f, 1.0f, 1.0f);
			matrix.Array[2] = new Vector4D(1.0f, 1.0f, 1.0f, 1.0f);
			//SharpDX.Matrix.Transpose(ref matrix.Projection, out matrix.Projection);
			//SharpDX.Matrix.Transpose(ref matrix.View, out matrix.View);

			UpdateBuffer updatebuffer = new UpdateBuffer();
			updatebuffer.World = SharpDX.Matrix.Identity;
			updatebuffer.Alpha = new Vector4D(1.0f, 1.0f, 1.0f, 1.0f);

			_noChangeBuffer = _graphics.CreateConstantBuffer<MatrixBuffer>(matrix, false);
			_changeBuffer = _graphics.CreateConstantBuffer<UpdateBuffer>(updatebuffer, false);

			//using (GorgonConstantBufferStream stream = _noChangeBuffer.Lock())
			//{
			//    stream.Write<Matrix>("Projection", matrix.Projection);
			//    stream.Write<Matrix>("View", matrix.View);
			//    stream.WriteRange<Vector4D>("Array", matrix.Array);
			//}
			//_noChangeBuffer.Unlock();

			//using (DataStream noChangeStream = new DataStream(Marshal.SizeOf(typeof(MatrixBuffer)), true, true))
			//{
			//    noChangeStream.Write<MatrixBuffer>(matrix);
			//    noChangeStream.Position = 0;
			//    _noChangeBuffer = new D3D.Buffer(_device, noChangeStream, new D3D.BufferDescription()
			//        {
			//            BindFlags = D3D.BindFlags.ConstantBuffer,
			//            CpuAccessFlags = D3D.CpuAccessFlags.None,
			//            OptionFlags = D3D.ResourceOptionFlags.None,
			//            SizeInBytes = Marshal.SizeOf(typeof(MatrixBuffer)),
			//            StructureByteStride = 0,
			//            Usage = D3D.ResourceUsage.Default
			//        });
			//    _noChangeBuffer.DebugName = _swapChain.Name + " Test No Change buffer";
			//}

			//SharpDX.Matrix.Transpose(ref updatebuffer.World, out updatebuffer.World);
			
			//_changeStream = new DataStream(Marshal.SizeOf(typeof(UpdateBuffer)), true, true);
			//_changeStream.Write<UpdateBuffer>(updatebuffer);
			//_changeStream.Position = 0;

			//_changeBuffer = new D3D.Buffer(_device, _changeStream, new D3D.BufferDescription()
			//{
			//    BindFlags = D3D.BindFlags.ConstantBuffer,
			//    CpuAccessFlags = D3D.CpuAccessFlags.None,
			//    OptionFlags = D3D.ResourceOptionFlags.None,
			//    SizeInBytes = Marshal.SizeOf(typeof(UpdateBuffer)),
			//    StructureByteStride = 0,
			//    Usage = D3D.ResourceUsage.Default
			//});
			//_changeBuffer.DebugName = _swapChain.Name + " Test Change buffer";

			//_changeStream.Position = 0;
			//_device.ImmediateContext.UpdateSubresource(new DataBox(_changeStream.DataPointer, 0, 0), _changeBuffer, 0);

			//_depthStateAlpha = _graphics.CreateDepthStencilState();
			_depthStateAlpha.IsDepthEnabled = false;
			_depthStateAlpha.IsDepthWriteEnabled = false;

			_graphics.InputBindings.Layout = layout;
			_graphics.VertexShader.Current = _vs;
			_graphics.PixelShader.Current = _ps;
						
			_graphics.VertexShader.ConstantBuffers.SetRange(0, new GorgonConstantBuffer[] { _noChangeBuffer, _changeBuffer });
			_graphics.PixelShader.ConstantBuffers[1] = _changeBuffer;

			_graphics.PixelShader.Samplers[0] = GorgonTextureSamplerStates.DefaultStates;
		}

		/// <summary>
		/// Handles the Resize event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void Window_Resize(object sender, EventArgs e)
		{
			MatrixBuffer matrix = new MatrixBuffer();
			matrix.Projection = Matrix.Transpose(Matrix.PerspectiveFovLH(GorgonLibrary.Math.GorgonMathUtility.Radians(75.0f), (float)_swapChain.Settings.VideoMode.Width / (float)_swapChain.Settings.VideoMode.Height, 0.1f, 1000.0f));
			matrix.View = Matrix.Transpose(Matrix.LookAtLH(new Vector3(0, 0, 0.75f), new Vector3(0, 0, -1.0f), Vector3.UnitY));
			_graphics.Rasterizer.SetViewport(_swapChain.Viewport);
			using (var stream = _noChangeBuffer.Lock())
			{
				stream.Write(matrix);
			}			
			_noChangeBuffer.Unlock();
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

		/// <summary>
		/// Transform.
		/// </summary>
		/// <param name="delta"></param>
		public void Transform(float delta)
		{
			_degreesPerSecond = GorgonLibrary.Math.GorgonMathUtility.Abs((GorgonLibrary.Math.GorgonMathUtility.Cos(GorgonLibrary.Math.GorgonMathUtility.Radians(_rot)) * _currentTime)) + 95.0f;

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
		/// 
		/// </summary>
		public void Draw()
		{
			SharpDX.Result result = Result.Ok;
			UpdateBuffer buffer = new UpdateBuffer();

			
			_device.ImmediateContext.OutputMerger.SetTargets((_swapChain.DepthStencil != null ? _swapChain.DepthStencil.D3DDepthStencilView : null), _swapChain.D3DRenderTarget);
				
			//_device.ImmediateContext.OutputMerger.SetTargets(_swapChain.D3DRenderTarget);

			//_graphics.SetViewport(_swapChain.Viewport);
						
			_device.ImmediateContext.InputAssembler.SetVertexBuffers(0, _binding);
			_device.ImmediateContext.InputAssembler.SetIndexBuffer(_index, GI.Format.R32_UInt, 0);

			//_device.ImmediateContext.VertexShader.Set(_vs.D3DShader);
			//_device.ImmediateContext.PixelShader.Set(_ps.D3DShader);
			
			
			//_device.ImmediateContext.VertexShader.SetConstantBuffer(0, _noChangeBuffer.D3DBuffer);
			//_device.ImmediateContext.VertexShader.SetConstantBuffer(1, _changeBuffer);
			//_device.ImmediateContext.PixelShader.SetConstantBuffer(1, _changeBuffer);
			_device.ImmediateContext.PixelShader.SetShaderResource(0, _textureView);
			//_device.ImmediateContext.PixelShader.SetSampler(0, _sampler);

			//SharpDX.DataBox box = _device.ImmediateContext.MapSubresource(_texture, 0, 0, D3D.MapMode.WriteDiscard, D3D.MapFlags.None);
			//byte[] texelbuffer = new byte[box.Data.Length];
			//_rnd.NextBytes(texelbuffer);
			//box.Data.WriteRange<byte>(texelbuffer);
			//box.Data.Dispose();
			//_device.ImmediateContext.UnmapSubresource(_texture, 0);

			float passAngle = 0.0f;
			buffer.Alpha = new GorgonColor(System.Drawing.Color.FromArgb(0, System.Drawing.Color.White));
			_graphics.DepthStencil.States = _depthStateAlpha;

			for (int i = 0; i < (int)_passes; i++)
			{
				passAngle = GorgonLibrary.Math.GorgonMathUtility.Radians(_rot - (_passes - (i * (_degreesPerSecond / GorgonLibrary.Math.GorgonMathUtility.Pow(_passes, 2.25f)))));

				if (i < (int)(_passes - 1))
					buffer.Alpha.W += (1.0f / (_passes * 3.25f));
				else
				{
					_graphics.DepthStencil.States = GorgonDepthStencilStates.DefaultStates;
					buffer.Alpha.W = 1.0f;
				}

				_graphics.Draw();
				buffer.World = Matrix.RotationZ(passAngle);
				buffer.World = SharpDX.Matrix.Multiply(buffer.World, SharpDX.Matrix.RotationZ(passAngle));
				buffer.World = SharpDX.Matrix.Multiply(buffer.World, SharpDX.Matrix.RotationY(passAngle));
				buffer.World = Matrix.Transpose(buffer.World);

				using (GorgonConstantBufferStream stream = _changeBuffer.Lock())
					stream.Write(buffer);

				_changeBuffer.Unlock();

				_device.ImmediateContext.DrawIndexed(6, 0, 0);
			}

			for (int i = 0; i < 1; i++)
			{
				buffer.World = Matrix.Identity;
				buffer.World *= Matrix.Scaling(0.5f, 0.5f, 1.0f);
				buffer.World *= Matrix.Translation(0.5f + ((float)i / 32767.0f) , 0.25f, 0.0f);
				buffer.World = Matrix.Transpose(buffer.World);
				buffer.Alpha.W = 1.0f;
				using (GorgonConstantBufferStream stream = _changeBuffer.Lock())
				{
					stream.Write(buffer);
				}
				_changeBuffer.Unlock();
				_device.ImmediateContext.DrawIndexed(6, 0, 0);
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
