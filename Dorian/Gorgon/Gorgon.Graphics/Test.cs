using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimMath;
using DX = SharpDX;
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
	[StructLayout(LayoutKind.Explicit, Size = 128, Pack=1)]
	public struct TEMPGUY
	{
		/// <summary>
		/// 
		/// </summary>
		[FieldOffset(0)]
		public float value1;
		/// <summary>
		/// 
		/// </summary>
		[FieldOffset(16)]
		public Matrix value2;
		/// <summary>
		/// 
		/// </summary>
		[FieldOffset(80), MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
		public Vector4[] tempArray;
	}

	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Explicit, Size = 304, Pack=1)]
	public struct MatrixBuffer
	{
		/// <summary>
		/// /
		/// </summary>
		[FieldOffset(0)]
		public Matrix Projection;
		/// <summary>
		/// 
		/// </summary>
		[FieldOffset(64)]
		public Matrix View;
		/// <summary>
		///
		/// </summary>
		[FieldOffset(128), MarshalAs(UnmanagedType.ByValArray, SizeConst=3)]
		public Vector4[] Array;
		/// <summary>
		/// 
		/// </summary>
		[FieldOffset(176)]
		public TEMPGUY valueType;
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
		public Matrix World;
		/// <summary>
		/// 
		/// </summary>
		[FieldOffset(64)]
		public GorgonColor Alpha;
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
		private int count = 100000;
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
		//private D3D.Buffer _vertices = null;
		private GorgonVertexBuffer _vertices = null;
		//private GorgonVertexBuffer _uvs = null;
		//private GorgonVertexBuffer _cols = null;
		private GorgonIndexBuffer _index = null;
		//private D3D.EffectPass _pass = null;		
		private D3D.VertexBufferBinding _binding = default(D3D.VertexBufferBinding);
		//private D3D.VertexBufferBinding _binding2 = default(D3D.VertexBufferBinding);
		//private D3D.VertexBufferBinding _binding3 = default(D3D.VertexBufferBinding);
		//private float _rot = 0.0f;
		//private float _degreesPerSecond = 0.0f;
		//private D3D.EffectScalarVariable _alpha = null;
		//private D3D.EffectConstantBuffer _matrix = null;		
		//private float _currentTime = 0;
		//private int _maxPasses = 0;
		//private bool _timeSwitch = false;
		//private float _passes = 8.0f;
		private D3D.Texture2D _texture = null;		
		private D3D.ShaderResourceView _textureView = null;
		private D3D.Texture2D _texture2 = null;
		private D3D.ShaderResourceView _textureView2 = null;
		//private D3D.SamplerState _sampler = null;
		//private D3D.VertexShader _vs = null;
		//private D3D.PixelShader _ps = null;
		//private D3D.Buffer _changeBuffer = null;
		//private GorgonConstantBuffer _noChangeBuffer = null;
		//private GorgonConstantBuffer _changeBuffer = null;
		//private GorgonConstantBuffer _alphaTestData = null;
		//private D3D.Buffer _noChangeBuffer = null;
		//private GorgonDepthStencilState _depthStateAlpha = null;
		private GorgonDepthStencilStates _depthStateAlpha = GorgonDepthStencilStates.DefaultStates;
		//private DX.DataStream _changeStream = null;
		Random _rnd = new Random();
		private GorgonVertexShader _vs = null;
		private GorgonPixelShader _ps = null;
		private Vector4[] _pos = new Vector4[4];
		private vertex[] _sprite = null;
		private Matrix pvw = Matrix.Identity;
		private GorgonDataStream _tempStream = null;
			
		
		struct vertex
		{
			[InputElement(0, "POSITION")]
			public Vector4 Position;
			//[InputElement("COLOR", BufferFormat.R32G32B32A32_Float, 0, 0, 1)]			
			[InputElement(1, "COLOR")]
			public Vector4 Color;			
			//[InputElement("TEXTURECOORD", BufferFormat.R32G32_Float, 0, 0, 2)]
			[InputElement(2, "TEXTURECOORD")]
			public Vector2 UV;
		}


		private void Destroy()
		{
			_swapChain.Settings.Window.Resize -= new EventHandler(Window_Resize);

			if (_tempStream != null)
				_tempStream.Dispose();
			//if (_depthStateAlpha != null)
				//_depthStateAlpha.Dispose();
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
			if (_textureView != null)
				_textureView.Dispose();
			if (_texture != null)
			    _texture.Dispose();
			if (_textureView2 != null)
				_textureView2.Dispose();
			if (_texture2 != null)
				_texture2.Dispose();
			//if (_sampler != null)
				//_sampler.Dispose();
			//if (_vertices != null)
			//    _vertices.Dispose();
			//if (_index != null)
			//    _index.Dispose();
		}

		private void Initialize()
		{
			string errors = string.Empty;

			_graphics = _swapChain.Graphics;
			_swapChain.Settings.Window.Resize += new EventHandler(Window_Resize);
			_swapChain.AfterStateTransition += new EventHandler(Window_Resize);

			_shader = Encoding.UTF8.GetString(Properties.Resources.Test);

			_device = _graphics.VideoDevice.D3DDevice;
			
			_vs = _graphics.CreateVertexShader("TestVShader", "VS", _shader);
			_ps = _graphics.CreatePixelShader("TestPShader", "PS", _shader);
						
			GorgonInputLayout layout = _graphics.CreateInputLayout("Test Layout", typeof(vertex), _vs);

			int vertexSize = layout.GetSlotSize(0);			

			_vertices = _graphics.CreateVertexBuffer(4 * vertexSize * count, BufferUsage.Dynamic, null);
			//_cols = _graphics.CreateVertexBuffer(4 * 16 * count, BufferUsage.Dynamic, null);

			using (GorgonDataStream stream = new GorgonDataStream(count * 6 * 4))
			{
				int index = 0;
				for (int i = 0; i < count; i++)
				{
					stream.Write<int>(index);
					stream.Write<int>(index + 1);
					stream.Write<int>(index + 2);
					stream.Write<int>(index + 1);
					stream.Write<int>(index + 3);
					stream.Write<int>(index + 2);
					index += 4;
				}

				stream.Position = 0;
				_index = _graphics.CreateIndexBuffer((int)stream.Length, BufferUsage.Default, true, stream);
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
			info.MipFilter = D3D.FilterFlags.Point;
			info.MipLevels = 1;
			info.OptionFlags = D3D.ResourceOptionFlags.None;
			info.Usage = D3D.ResourceUsage.Default;

			//_texture = D3D.Resource.FromFile<D3D.Texture2D>(_device, @"..\..\..\..\Resources\Images\TextureTest.png", info);
			_texture = D3D.Resource.FromFile<D3D.Texture2D>(_device, @"..\..\..\..\Resources\BallDemo\BallDemo.png", info);
			_texture.DebugName = _swapChain.Name + " Test texture.";
			_textureView = new D3D.ShaderResourceView(_device, _texture);
			_textureView.DebugName = _swapChain.Name + " Test texture view.";

			_texture2 = D3D.Resource.FromFile<D3D.Texture2D>(_device, @"..\..\..\..\Resources\Images\VBback.jpg", info);
			_texture2.DebugName = _swapChain.Name + " Test texture 2.";
			_textureView2 = new D3D.ShaderResourceView(_device, _texture2);
			_textureView2.DebugName = _swapChain.Name + " Test texture view 2.";

			_form = Gorgon.GetTopLevelForm(_swapChain.Settings.Window);

			MatrixBuffer matrix = new MatrixBuffer();
			matrix.Projection = Matrix.PerspectiveFovLH(GorgonLibrary.Math.GorgonMathUtility.Radians(100.39f), (float)(_swapChain.Settings.VideoMode.Width) / (float)(_swapChain.Settings.VideoMode.Height), 0.1f, 1000.0f);
			//matrix.Projection = Matrix.OrthoLH(2.0f * 1.6f, 2.0f, 0.1f, 1000.0f);
			//matrix.Projection = Matrix.OrthoOffCenterLH((1.0f * 1.6f), 0, 0, -1.0f, 0.1f, 1000.0f);
			//matrix.Projection.Transpose();
			matrix.View = Matrix.LookAtLH(new Vector3(0, 0, -0.75f), new Vector3(0, 0, 1.0f), Vector3.UnitY);
			//matrix.View.Transpose();
			
			//Matrix.PerspectiveFovLH(GorgonLibrary.Math.GorgonMathUtility.Radians(75.0f), (float)(_swapChain.Settings.VideoMode.Width) / (float)(_swapChain.Settings.VideoMode.Height), 0.1f, 1000.0f);
			//matrix.Projection = Matrix.Identity;
			
			//matrix.View = Matrix.LookAtLH(new Vector3(0, 0, 0.75f), new Vector3(0, 0, -1.0f), Vector3.UnitY);
			matrix.Array = new Vector4[3];
			matrix.Array[0] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
			matrix.Array[1] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
			matrix.Array[2] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
			matrix.valueType = new TEMPGUY();
			matrix.valueType.value2 = matrix.View;
			matrix.valueType.tempArray = new Vector4[3];
			
			UpdateBuffer updatebuffer = new UpdateBuffer();
			updatebuffer.World = Matrix.Identity;
			updatebuffer.Alpha = new GorgonColor(1.0f, 1.0f, 1.0f, 1.0f);
			
			_depthStateAlpha.IsDepthEnabled = false;
			_depthStateAlpha.IsDepthWriteEnabled = false;

			_graphics.InputBindings.Layout = layout;
			_graphics.VertexShader.Current = _vs;
			_graphics.PixelShader.Current = _ps;
						
			//_graphics.VertexShader.ConstantBuffers.SetRange(0, new GorgonConstantBuffer[] { _noChangeBuffer, _changeBuffer });
			//_graphics.VertexShader.ConstantBuffers[0] = _graphics.PixelShader.ConstantBuffers[0] = _noChangeBuffer;

			_graphics.PixelShader.Samplers[0] = GorgonTextureSamplerStates.DefaultStates;
			//_graphics.PixelShader.Samplers[1] = GorgonTextureSamplerStates.DefaultStates;

			//using (GorgonDataStream stream = new GorgonDataStream(count * layout.GetSlotSize(2) * 4))
			//{
			//    for (int i = 0; i < (count * 4); i+=4)
			//    {					
			//        stream.Write(_sprite[i].UV);
			//        stream.Write(_sprite[i + 1].UV);
			//        stream.Write(_sprite[i + 2].UV);
			//        stream.Write(_sprite[i + 3].UV);
			//        //stream.Position += 32;
			//    }
			//    stream.Position = 0;
				
			//    _uvs = _graphics.CreateVertexBuffer((int)stream.Length, BufferUsage.Immutable, stream);
			//}

			//_cols = _graphics.CreateVertexBuffer(layout.GetSlotSize(1) * count * 4, BufferUsage.Dynamic);

			_device.ImmediateContext.OutputMerger.SetTargets((_swapChain.DepthStencil != null ? _swapChain.DepthStencil.D3DDepthStencilView : null), _swapChain.D3DRenderTarget);
			_binding = new D3D.VertexBufferBinding(_vertices.D3DVertexBuffer, vertexSize, 0);
			//_binding2 = new D3D.VertexBufferBinding(_cols.D3DVertexBuffer, layout.GetSlotSize(1), 0);
			//_binding3 = new D3D.VertexBufferBinding(_uvs.D3DVertexBuffer, layout.GetSlotSize(2), 0);
			_device.ImmediateContext.InputAssembler.SetVertexBuffers(0, _binding);
			//_device.ImmediateContext.InputAssembler.SetVertexBuffers(1, _binding2);
			//_device.ImmediateContext.InputAssembler.SetVertexBuffers(2, _binding3);
			_device.ImmediateContext.InputAssembler.SetIndexBuffer(_index.D3DIndexBuffer, GI.Format.R32_UInt, 0);
			_device.ImmediateContext.PixelShader.SetShaderResource(0, _textureView);
			//_device.ImmediateContext.PixelShader.SetShaderResource(1, _textureView2);
			pvw = matrix.valueType.value2 * matrix.Projection;

			_tempStream = new GorgonDataStream(_sprite.Length * vertexSize);

		}

		/// <summary>
		/// Handles the Resize event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void Window_Resize(object sender, EventArgs e)
		{
			MatrixBuffer matrix = new MatrixBuffer();
			
			_graphics.Rasterizer.SetViewport(_swapChain.Viewport);
			matrix.Projection = Matrix.PerspectiveFovLH(GorgonLibrary.Math.GorgonMathUtility.Radians(100.39f), (float)(_swapChain.Settings.VideoMode.Width) / (float)(_swapChain.Settings.VideoMode.Height), 0.1f, 1000.0f);
			matrix.View = Matrix.LookAtLH(new Vector3(0, 0, -0.75f), new Vector3(0, 0, 1.0f), Vector3.UnitY);			
			//matrix.Projection.Transpose();
			//matrix.View.Transpose();
/*			matrix.Array = new Vector4[3];
			matrix.Array[0] = new Vector4(0.50f, 1.0f, 1.0f, 1.0f);
			matrix.Array[1] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
			matrix.Array[2] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
			matrix.valueType.value2 = matrix.View;
			matrix.valueType.tempArray = new Vector4[3];
			
			using (var stream = _noChangeBuffer.GetBuffer())
			{
				stream.Position = 0;
				stream.Write(matrix.Projection);
				stream.Position = 192;
				stream.Write(matrix.View);
			}*/

			_device.ImmediateContext.OutputMerger.SetTargets((_swapChain.DepthStencil != null ? _swapChain.DepthStencil.D3DDepthStencilView : null), _swapChain.D3DRenderTarget);

			pvw = matrix.View * matrix.Projection;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Test"/> class.
		/// </summary>
		/// <param name="swapChain">The swap chain.</param>
		public Test(GorgonSwapChain swapChain)
		{
			_swapChain = swapChain;
			_sprite = new vertex[count * 4];

			Gorgon.Log.Print("Test: Creating objects.", Diagnostics.LoggingLevel.Verbose);

			scale = new float[count];
			scaleDelta = new float[count];
			scaleBounce = new bool[count];
			pos = new Vector2[count];
			hBounce = new bool[count];
			vBounce = new bool[count];
			vel = new Vector2[count];
			aBounce = new bool[count];
			alpha = new float[count];
			alphaDelta = new float[count];
			angle = new float[count];
			angleDelta = new float[count];
			checker = new bool[count];
			color = new Vector3[count];

			for (int i = 0; i < count; i++)
			{
				//scale[i] = ((float)_rnd.NextDouble() * 0.5f) + 0.5f;
				scale[i] = (float)(i + 1) / count;
				scaleDelta[i] = ((float)_rnd.NextDouble() * 1.5f) + 0.25f;
				scaleBounce[i] = false;
				vBounce[i] = _rnd.Next(0, 100) > 50;
				hBounce[i] = _rnd.Next(0, 100) < 50;
				pos[i] = Vector2.Zero;
				vel[i] = new Vector2(((float)_rnd.NextDouble() * 0.5f), ((float)_rnd.NextDouble() * 0.5f));
				angle[i] = 0.0f;
				angleDelta[i] = 1.0f;
				alpha[i] = scale[i];
				alphaDelta[i] = (float)_rnd.NextDouble() * 0.5f;
				aBounce[i] = false;
				checker[i] = _rnd.Next(0, 100) > 50;
				color[i] = new Vector3(((float)_rnd.NextDouble() * 0.961f) + 0.039f, ((float)_rnd.NextDouble() * 0.961f) + 0.039f, ((float)_rnd.NextDouble() * 0.961f) + 0.039f);
				pos[i].X = ((float)_rnd.NextDouble() * 2.0f) - 1.0f;
				pos[i].Y = ((float)_rnd.NextDouble() * 2.0f) - 1.0f;

				int spriteIndex = i * 4;
				if (checker[i])
				{
					_sprite[spriteIndex].UV = new Vector2(0.503f, 0.0f);
					_sprite[spriteIndex + 1].UV = new Vector2(1.0f, 0.0f);
					_sprite[spriteIndex + 2].UV = new Vector2(0.503f, 0.5f);
					_sprite[spriteIndex + 3].UV = new Vector2(1.0f, 0.5f);
				}
				else
				{
					_sprite[spriteIndex].UV = new Vector2(0.0f, 0.503f);
					_sprite[spriteIndex + 1].UV = new Vector2(0.5f, 0.503f);
					_sprite[spriteIndex + 2].UV = new Vector2(0.0f, 1.0f);
					_sprite[spriteIndex + 3].UV = new Vector2(0.5f, 1.0f);
				}
			}

			Initialize();
		}

		private bool _needsUpdate = true;
				
		/// <summary>
		/// Transform.
		/// </summary>
		/// <param name="delta"></param>
		public void Transform(float delta)
		{

			for (int i = 0; i < count; i++)
			{
				angle[i] += angleDelta[i] * delta;

				if (angle[i] > 360.0f)
				{
					angle[i] = angle[i] - 360.0f;
					angleDelta[i] = (float)_rnd.NextDouble() * 90.0f;
				}

				if (scaleBounce[i])
					alpha[i] -= scaleDelta[i] * delta;
				else
					alpha[i] += scaleDelta[i] * delta;

				if (alpha[i] > 1.0f)
					alpha[i] = 1.0f;

				if (alpha[i] < 0.0f)
				{
					alpha[i] = 0.0f;
					color[i] = new Vector3(((float)_rnd.NextDouble() * 0.961f) + 0.039f, ((float)_rnd.NextDouble() * 0.961f) + 0.039f, ((float)_rnd.NextDouble() * 0.961f) + 0.039f);
				}

				if (scaleBounce[i])				
					scale[i] -= scaleDelta[i] * delta;
				else
					scale[i] += scaleDelta[i] * delta;

				//scale[i] = 1.05f;

				if ((scale[i] < 0.05f) || (scale[i] > 1.0f))
				{
					if (scale[i] > 1.0f)
						scale[i] = 1.0f;
					if (scale[i] < 0.05f)
					{
						scale[i] = 0.05f;
						color[i] = new Vector3(((float)_rnd.NextDouble() * 0.961f) + 0.039f, ((float)_rnd.NextDouble() * 0.961f) + 0.039f, ((float)_rnd.NextDouble() * 0.961f) + 0.039f);
					}
					scaleBounce[i] = !scaleBounce[i];
					scaleDelta[i] = ((float)_rnd.NextDouble() * 1.5f) + 0.25f;
				}

				if (vBounce[i])
					pos[i].Y -= (vel[i].Y * delta);
				else
					pos[i].Y += (vel[i].Y * delta);

				if (hBounce[i])
					pos[i].X -= (vel[i].X * delta);
				else
					pos[i].X += (vel[i].X * delta);

				if (pos[i].X > 1.0f)
				{
					pos[i].X = 1.0f;
					vel[i].X = ((float)_rnd.NextDouble() * 0.5f) + 0.5f;
					hBounce[i] = !hBounce[i];
				}
				if (pos[i].Y > 1.0f)
				{
					pos[i].Y = 1.0f;
					vel[i].Y = ((float)_rnd.NextDouble() * 0.5f) + 0.5f;
					vBounce[i] = !vBounce[i];
				}

				if (pos[i].X < -1.0f)
				{
					pos[i].X = -1.0f;
					vel[i].X = (float)_rnd.NextDouble();
					hBounce[i] = !hBounce[i];
				}
				if (pos[i].Y < -1.0f)
				{
					pos[i].Y = -1.0f;
					vel[i].Y = (float)_rnd.NextDouble();
					vBounce[i] = !vBounce[i];
				}

			}
						
			//if (frames == 0)
			{

				Matrix trans = Matrix.Identity;
				Matrix world = Matrix.Identity;
				for (int i = 0; i < count * 4; i += 4)
				{
					int arrayindex = i / 4;

					trans = Matrix.Identity;
					//buffer.World = Matrix.Scaling(0.248f, 0.248f, 1.0f);// *Matrix.Translation(-0.5f + ((float)(i / 4) / (float)count), 0.25f, 0.0f);
					//buffer.World = Matrix.RotationZ(angle[arrayindex]);
					world = Matrix.RotationZ(-angle[arrayindex]) * (Matrix.Scaling(scale[arrayindex], scale[arrayindex], 1.0f)) * Matrix.Translation(pos[arrayindex].X, pos[arrayindex].Y, 0.0f);
					Matrix.Multiply(ref world, ref pvw, out trans);




					/*				buffer.World = Matrix.Identity;
									buffer.World = Matrix.Scaling(0.125f, 0.125f, 1.0f) * Matrix.Translation(-0.5f + ((float)i / 32767.0f), 0.25f, 0.0f);				
									//buffer.World = Matrix.Transpose(buffer.World);
									buffer.Alpha.Alpha = 1.0f;
									using (GorgonDataStream stream = _changeBuffer.GetBuffer())
									{
										stream.Write(buffer);
									}*/
					_sprite[i].Color = new GorgonColor(color[arrayindex].X, color[arrayindex].Y, color[arrayindex].Z, alpha[arrayindex]);
					//_sprite[i].Color = new GorgonColor(System.Drawing.Color.White);
					_sprite[i + 1].Color = _sprite[i].Color;
					_sprite[i + 2].Color = _sprite[i].Color;
					_sprite[i + 3].Color = _sprite[i].Color;

					_sprite[i].Position = Vector3.Transform(new Vector3(-0.1401f, 0.1401f, 0.0f), trans);						
					_sprite[i + 1].Position = Vector3.Transform(new Vector3(0.1401f, 0.1401f, 0.0f), trans);
					_sprite[i + 2].Position = Vector3.Transform(new Vector3(-0.1401f, -0.1401f, 0.0f), trans);
					_sprite[i + 3].Position = Vector3.Transform(new Vector3(0.1401f, -0.1401f, 0.0f), trans);

					//_sprite[i].Position = new Vector4(-0.5f, 0.5f, 0.0f, 1.0f);
					//_sprite[i + 1].Position = new Vector4(0.5f, 0.5f, 0.0f, 1.0f);
					//_sprite[i + 2].Position = new Vector4(-0.5f, -0.5f, 0.0f, 1.0f);
					//_sprite[i + 3].Position = new Vector4(0.5f, -0.5f, 0.0f, 1.0f);
				}

//				_tempStream.Position = 0;
//				_tempStream.WriteRange(_sprite);
				//_graphics.Context.UpdateSubresource(new DX.DataBox { DataPointer = _tempStream.BasePointer, RowPitch = (int)_tempStream.Length }, _vertices);
				/*_graphics.Context.UpdateSubresource(new DX.DataBox { DataPointer = _tempStream.BasePointer, RowPitch = (int)_tempStream.Length }, _vertices, 0, new D3D.ResourceRegion()
					{
						Left = 0,
						Right = 81920,
						Top = 0,
						Bottom = 1,
						Front = 0,
						Back = 1
					});*/
				//frames = Int32.MaxValue;				
			}

			_needsUpdate = true;

			//_tempStream.Position = 0;			
			//_tempStream.WriteRange(_sprite);
			//_tempStream.Position = 0;
			//_vertices.Update(_tempStream);
		}

		//int frames = 0;
		//int textPos = 0;
		float[] scale = null;
		float[] scaleDelta = null;
		Vector2[] pos = null;
		Vector2[] vel = null;
		float[] angle = null;
		float[] angleDelta = null;
		float[] alpha = null;
		float[] alphaDelta = null;
		bool[] scaleBounce = null;
		bool[] hBounce = null;
		bool[] vBounce = null;
		bool[] aBounce = null;
		bool[] checker = null;
		Vector3[] color = null;
		
		/// <summary>
		/// 
		/// </summary>
		public void Draw()
		{
			if (_needsUpdate)
			{
				using (GorgonDataStream vstream = _vertices.Lock(BufferLockFlags.Write | BufferLockFlags.Discard))
				{
					vstream.WriteRange(_sprite);
					//using (GorgonDataStream cstream = _cols.Lock(BufferLockFlags.Write | BufferLockFlags.Discard))
					//{
					//    for (int i = 0; i < count * 4; i++)
					//    {
					//        vstream.Write(_sprite[i].Position);
					//        cstream.Write(_sprite[i].Color);
					//    }
					//    _cols.Unlock();
					    _vertices.Unlock();
					//}
				}
				_needsUpdate = false;
			}

			_device.ImmediateContext.DrawIndexed(6 * count, 0, 0);
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
					Gorgon.Log.Print("Test: Destroy objects.", Diagnostics.LoggingLevel.Verbose);
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
