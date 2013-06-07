using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using GorgonLibrary.Graphics.Test.Properties;
using GorgonLibrary.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;
using SlimMath;

namespace GorgonLibrary.Graphics.Test
{
	[TestClass]
	public class BouncyBalls3DTest
	{
		#region Value types.
		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Explicit, Size = 128, Pack = 1)]
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
			[FieldOffset(80), MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
			public Vector4[] tempArray;
		}

		/// <summary>
		/// 
		/// </summary>
		[StructLayout(LayoutKind.Explicit, Size = 304, Pack = 1)]
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
			[FieldOffset(128), MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
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
		[StructLayout(LayoutKind.Explicit, Size = 80, Pack = 1)]
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
		public struct SpriteBall
		{
			/// <summary>
			/// 
			/// </summary>
			public Vector3 Position;
			/// <summary>
			/// 
			/// </summary>
			public Vector3 Velocity;
			/// <summary>
			/// 
			/// </summary>
			public float Scale;
			/// <summary>
			/// 
			/// </summary>
			public float ScaleDelta;
			/// <summary>
			/// 
			/// </summary>
			public float Angle;
			/// <summary>
			/// 
			/// </summary>
			public float AngleDelta;
			/// <summary>
			/// 
			/// </summary>
			public Vector4 Color;
			/// <summary>
			/// 
			/// </summary>
			public float AlphaDelta;
			/// <summary>
			/// 
			/// </summary>
			public bool XBounce;
			/// <summary>
			/// 
			/// </summary>
			public bool YBounce;
			/// <summary>
			/// 
			/// </summary>
			public bool ZBounce;
			/// <summary>
			/// 
			/// </summary>
			public bool ScaleBouce;
			/// <summary>
			/// 
			/// </summary>
			public bool Checkered;
			/// <summary>
			/// 
			/// </summary>
			public bool AlphaBounce;
		}

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
		#endregion

		public const int Count = 1024;

		private string _shader = "Texture2D theTexture : register(t0); " +
									"SamplerState sample : register(s0); " + 
									"struct VS_IN " +
									"{ " +
									"	float4 pos : POSITION;" +
									"	float4 col : COLOR;" +
									"	float2 uv : TEXTURECOORD;" +
									"};" + 
									"struct PS_IN"+
									"{" +
									"	float4 pos : SV_POSITION;" +
									"	float4 col : COLOR;" +
									"	float2 uv : TEXTURECOORD;" +
									"};" + 
									"PS_IN VS( VS_IN input )" +
									"{" +
									"	return input;" +
									"}" +
									"float4 PS( PS_IN input ) : SV_Target" +
									"{		" +
									"	return theTexture.Sample(sample, input.uv) * input.col;" +
									"}";
		private GorgonVertexBuffer _vertices = null;
		private GorgonIndexBuffer _index = null;
		private GorgonTexture2D _texture = null;
		private GorgonTexture2D _texture2 = null;
		private GorgonInputLayout _layout = null;
		private GorgonPixelShader _ps;
		private GorgonVertexShader _vs;
		private GorgonSwapChain _swap = null;
		private GorgonGraphics _graphics = null;
		private TestForm _form = null;
		private GorgonDepthStencilStates _depthStateAlpha = GorgonDepthStencilStates.DefaultStates;
		private Vector4[] _pos = new Vector4[4];
		private vertex[] _sprite = null;
		private Matrix pvw = Matrix.Identity;
		private GorgonDataStream _tempStream = null;
		private float _aspect = 0.0f;
		private float _camPos = -0.75f;
		private bool _3d = false;
		private SpriteBall[] _balls = new SpriteBall[Count];

		/// <summary>
		/// Test clean up code.
		/// </summary>
		[TestCleanup]
		public void CleanUp()
		{
			if (_graphics != null)
			{
				_graphics.Dispose();
			}

			_graphics = null;

			if (_form != null)
			{
				_form.Dispose();
			}

			_form = null;
		}

		/// <summary>
		/// Test initialization code.
		/// </summary>
		[TestInitialize]
		public void Init()
		{
			_form = new TestForm
				{
					ShowTestPanel = true,
					ClientSize = new Size(1280, 800)
				};
			_form.WindowState = FormWindowState.Minimized;
			_form.Show();
			_form.WindowState = FormWindowState.Normal;

			_graphics = new GorgonGraphics();
			_swap = _graphics.Output.CreateSwapChain("Screen", new GorgonSwapChainSettings()
				{
					Window = _form.panelDisplay,
                    DepthStencilFormat = BufferFormat.D24_UIntNormal_S8_UInt
				});

			_swap.Resized += (sender, args) =>
			{
				var currentMatrix = new MatrixBuffer();

				_graphics.Rasterizer.SetViewport(_swap.Viewport);
				_aspect = (_swap.Settings.VideoMode.Width) / (float)(_swap.Settings.VideoMode.Height);
				currentMatrix.Projection = Matrix.PerspectiveFovLH(100.39f.Radians(), _aspect, 0.1f, 1000.0f);
				currentMatrix.View = Matrix.LookAtLH(new Vector3(0, 0, -0.75f), new Vector3(0, 0, 1.0f), Vector3.UnitY);

                _graphics.Output.SetRenderTarget(_swap, _swap.DepthStencilBuffer);

				pvw = currentMatrix.View * currentMatrix.Projection;
			};

			_swap.AfterStateTransition += (sender, args) =>
			{
				var currentMatrix = new MatrixBuffer();

				_graphics.Rasterizer.SetViewport(_swap.Viewport);
				_aspect = (_swap.Settings.VideoMode.Width) / (float)(_swap.Settings.VideoMode.Height);
				currentMatrix.Projection = Matrix.PerspectiveFovLH(100.39f.Radians(), _aspect, 0.1f, 1000.0f);
				currentMatrix.View = Matrix.LookAtLH(new Vector3(0, 0, -0.75f), new Vector3(0, 0, 1.0f), Vector3.UnitY);

                _graphics.Output.SetRenderTarget(_swap, _swap.DepthStencilBuffer);

				pvw = currentMatrix.View * currentMatrix.Projection;
			};

			var button = new Button()
				{
					Text = "3D",
					Location = new Point(90, 3)
				};

			button.Click += (sender, args) =>
				{
					_3d = !_3d;
					Matrix currentMatrix = Matrix.LookAtLH(new Vector3(0, 0, _camPos), new Vector3(0, 0, 1.0f), Vector3.UnitY);
					Matrix projection = Matrix.PerspectiveFovLH(100.39f.Radians(), _aspect, 0.1f,
					                                            1000.0f);
					pvw = currentMatrix * projection;
				};

			_form.panelInput.Controls.Add(button);

			_sprite = new vertex[Count * 4];

			for (int i = 0; i < Count; i++)
			{
				_balls[i].Scale = 1.0f;
				_balls[i].ScaleDelta = (GorgonRandom.RandomSingle() * 1.5f) + 0.25f;
				_balls[i].AlphaBounce = _balls[i].ScaleBouce = false;
				_balls[i].XBounce = GorgonRandom.RandomInt32(0, 100) > 50;
				_balls[i].YBounce = GorgonRandom.RandomInt32(0, 100) > 50;
				_balls[i].ZBounce = GorgonRandom.RandomInt32(0, 100) > 50;
				_balls[i].Velocity = new Vector3((GorgonRandom.RandomSingle() * 0.5f), (GorgonRandom.RandomSingle() * 0.5f), (GorgonRandom.RandomSingle() * 0.5f));
				_balls[i].Angle = 0.0f;
				_balls[i].AngleDelta = 1.0f;
				_balls[i].Color = new Vector4(1.0f);
				_balls[i].AlphaDelta = GorgonRandom.RandomSingle() * 0.5f;
			    _balls[i].Checkered = true;// GorgonRandom.RandomInt32(0, 100) > 50;
				_balls[i].Position = new Vector3((GorgonRandom.RandomSingle() * 2.0f) - 1.0f, (GorgonRandom.RandomSingle() * 2.0f) - 1.0f, GorgonRandom.RandomSingle());
			}

			_vs = _graphics.Shaders.CreateShader<GorgonVertexShader>("TestVShader", "VS", _shader, true);
			_ps = _graphics.Shaders.CreateShader<GorgonPixelShader>("TestPShader", "PS", _shader, true);

			_layout = _graphics.Input.CreateInputLayout("Input", typeof(vertex), _vs);

			int vertexSize = _layout.Size;
			int index = 0;
			var indices = new int[Count * 6 * sizeof(int)];

			for (int i = 0; i < indices.Length; i+=6)
			{
				indices[i] = index;
				indices[i + 1] = index + 1;
				indices[i + 2] = index + 2;
				indices[i + 3] = index + 1;
				indices[i + 4] = index + 3;
				indices[i + 5] = index + 2;
				index += 4;
			}

			_vertices = _graphics.Buffers.CreateVertexBuffer("Vertex", new GorgonBufferSettings()
				{
					SizeInBytes = 4 * vertexSize * Count,
					Usage = BufferUsage.Dynamic
				});
			_index = _graphics.Buffers.CreateIndexBuffer("Index", indices, BufferUsage.Immutable);

			_texture = _graphics.Textures.FromFile<GorgonTexture2D>("Balls", @"..\..\..\..\Resources\BallDemo\BallDemo.png", new GorgonCodecPNG());
			_texture2 = _graphics.Textures.FromFile<GorgonTexture2D>("VBBack", @"..\..\..\..\Resources\Images\VBback.jpg", new GorgonCodecJPEG());

			var matrix = new MatrixBuffer();
			_aspect = _swap.Settings.VideoMode.Width / (float)(_swap.Settings.VideoMode.Height);

			matrix.Projection = Matrix.PerspectiveFovLH(100.39f.Radians(), _aspect, 0.1f,
														1000.0f);
			matrix.View = Matrix.LookAtLH(new Vector3(0, 0, _camPos), new Vector3(0, 0, 1.0f), Vector3.UnitY);
			matrix.Array = new Vector4[3];
			matrix.Array[0] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
			matrix.Array[1] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
			matrix.Array[2] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
			matrix.valueType = new TEMPGUY
				{
					value2 = matrix.View,
					tempArray = new Vector4[3]
				};

			_depthStateAlpha.IsDepthEnabled = false;
			_depthStateAlpha.IsDepthWriteEnabled = false;

			_graphics.Input.Layout = _layout;
			_graphics.Shaders.VertexShader.Current = _vs;
			_graphics.Shaders.PixelShader.Current = _ps;

			_graphics.Shaders.PixelShader.TextureSamplers.SetRange(0, new[] { GorgonTextureSamplerStates.DefaultStates, GorgonTextureSamplerStates.DefaultStates });

			_graphics.Rasterizer.SetViewport(_swap.Viewport);
		    _graphics.Output.DepthStencilState.States = _depthStateAlpha;
            _graphics.Output.SetRenderTarget(_swap, _swap.DepthStencilBuffer); 
            _graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_vertices, vertexSize);
			_graphics.Input.IndexBuffer = _index;
			_graphics.Shaders.PixelShader.Resources.SetRange(0, new GorgonShaderView[] { _texture, _texture2 });
			_graphics.Output.BlendingState.States = GorgonBlendStates.ModulatedBlending;
			pvw = matrix.valueType.value2 * matrix.Projection;

			_tempStream = new GorgonDataStream(_sprite.Length * vertexSize);
		}

		[TestMethod]
		public void BouncyBalls3D()
		{
			Gorgon.Run(_form, () =>
				{
					_swap.Clear(Color.Black);
				    for (int i = 0; i < Count; i++)
				    {
				        _balls[i].Angle += _balls[i].AngleDelta * GorgonTiming.Delta;

				        if (_balls[i].Angle > 360.0f)
				        {
				            _balls[i].Angle = _balls[i].Angle - 360.0f;
				            _balls[i].AngleDelta = GorgonRandom.RandomSingle() * 90.0f;
				        }

				        if ((_balls[i].ScaleBouce) || (!_balls[i].ZBounce))
				            _balls[i].Color.W -= _balls[i].Velocity.Z * GorgonTiming.Delta;
				        else
				            _balls[i].Color.W += _balls[i].Velocity.Z * GorgonTiming.Delta;

				        if (_balls[i].Color.W > 1.0f)
				            _balls[i].Color.W = 1.0f;

				        if (_balls[i].Color.W < 0.0f)
				        {
				            _balls[i].Color.W = 0.0f;
				            _balls[i].Color = new Vector4((GorgonRandom.RandomSingle() * 0.961f) + 0.039f,
				                                          (GorgonRandom.RandomSingle() * 0.961f) + 0.039f,
				                                          (GorgonRandom.RandomSingle() * 0.961f) + 0.039f, 0.0f);
				        }

				        if (_balls[i].YBounce)
				            _balls[i].Position.Y -= (_balls[i].Velocity.Y * GorgonTiming.Delta);
				        else
				            _balls[i].Position.Y += (_balls[i].Velocity.Y * GorgonTiming.Delta);

				        if (_balls[i].XBounce)
				            _balls[i].Position.X -= (_balls[i].Velocity.X * GorgonTiming.Delta);
				        else
				            _balls[i].Position.X += (_balls[i].Velocity.X * GorgonTiming.Delta);
				        if (_balls[i].ZBounce)
				            _balls[i].Position.Z -= (_balls[i].Velocity.Z * GorgonTiming.Delta);
				        else
				            _balls[i].Position.Z += (_balls[i].Velocity.Z * GorgonTiming.Delta);

				        if (_balls[i].Position.X > (1.0f * _aspect))
				        {
				            _balls[i].Position.X = (1.0f * _aspect);
				            _balls[i].Velocity.X = (GorgonRandom.RandomSingle() * 0.5f);
				            _balls[i].XBounce = !_balls[i].XBounce;
				        }
				        if (_balls[i].Position.Y > (1.0f * _aspect))
				        {
				            _balls[i].Position.Y = (1.0f * _aspect);
				            _balls[i].Velocity.Y = (GorgonRandom.RandomSingle() * 0.5f);
				            _balls[i].YBounce = !_balls[i].YBounce;
				        }

				        if (_balls[i].Position.X < (-1.0f * _aspect))
				        {
				            _balls[i].Position.X = (-1.0f * _aspect);
				            _balls[i].Velocity.X = GorgonRandom.RandomSingle() * 0.5f;
				            _balls[i].XBounce = !_balls[i].XBounce;
				        }
				        if (_balls[i].Position.Y < (-1.0f * _aspect))
				        {
				            _balls[i].Position.Y = (-1.0f * _aspect);
				            _balls[i].Velocity.Y = GorgonRandom.RandomSingle() * 0.5f;
				            _balls[i].YBounce = !_balls[i].YBounce;
				        }


				        if (_balls[i].Position.Z < -1.0f)
				        {
				            _balls[i].Position.Z = -1.0f;
				            _balls[i].Velocity.Z = GorgonRandom.RandomSingle() * 0.5f;
				            _balls[i].ZBounce = !_balls[i].ZBounce;
				        }

				        if (!(_balls[i].Position.Z > 1.0f))
				        {
				            continue;
				        }

				        _balls[i].Position.Z = 1.0f;
				        _balls[i].Velocity.Z = GorgonRandom.RandomSingle() * 0.5f;
				        _balls[i].ZBounce = !_balls[i].ZBounce;
				    }

				    var sortPos = _balls.OrderByDescending(item => item.Position.Z).ToArray();
				    
				    for (int i = 0; i < Count * 4; i += 4)
				    {
				        int arrayindex = i / 4;

				        Matrix world;
				        if (_3d)
				        {
				            Quaternion rot = Quaternion.RotationYawPitchRoll(sortPos[arrayindex].Angle.Cos(), -sortPos[arrayindex].Angle.Sin() * 2.0f, -sortPos[arrayindex].Angle);
				            Matrix.RotationQuaternion(ref rot, out world);
				        }
				        else
				            world = Matrix.RotationZ(-sortPos[arrayindex].Angle);
				        world = world * (Matrix.Scaling(sortPos[arrayindex].Scale, sortPos[arrayindex].Scale, 1.0f)) *
				                Matrix.Translation(sortPos[arrayindex].Position.X, sortPos[arrayindex].Position.Y,
				                                    sortPos[arrayindex].Position.Z);
				        Matrix trans;
				        Matrix.Multiply(ref world, ref pvw, out trans);

				        _sprite[i].Color = new GorgonColor(sortPos[arrayindex].Color);

				        _sprite[i + 1].Color = _sprite[i].Color;
				        _sprite[i + 2].Color = _sprite[i].Color;
				        _sprite[i + 3].Color = _sprite[i].Color;

				        _sprite[i].Position = Vector3.Transform(new Vector3(-0.1401f, 0.1401f, 0.0f), trans);
				        _sprite[i + 1].Position = Vector3.Transform(new Vector3(0.1401f, 0.1401f, 0.0f), trans);
				        _sprite[i + 2].Position = Vector3.Transform(new Vector3(-0.1401f, -0.1401f, 0.0f), trans);
				        _sprite[i + 3].Position = Vector3.Transform(new Vector3(0.1401f, -0.1401f, 0.0f), trans);

				        if (sortPos[arrayindex].Checkered)
				        {
				            _sprite[i].UV = new Vector2(0.503f, 0.0f);
				            _sprite[i + 1].UV = new Vector2(1.0f, 0.0f);
				            _sprite[i + 2].UV = new Vector2(0.503f, 0.5f);
				            _sprite[i + 3].UV = new Vector2(1.0f, 0.5f);
				        }
				        else
				        {
				            _sprite[i].UV = new Vector2(0.0f, 0.503f);
				            _sprite[i + 1].UV = new Vector2(0.5f, 0.503f);
				            _sprite[i + 2].UV = new Vector2(0.0f, 1.0f);
				            _sprite[i + 3].UV = new Vector2(0.5f, 1.0f);
				        }
				    }

				    using(GorgonDataStream vstream = _vertices.Lock(BufferLockFlags.Write | BufferLockFlags.Discard))
				    {
				        vstream.WriteRange(_sprite);
				        _vertices.Unlock();
				    }

				    _graphics.Output.DrawIndexed(0, 0, 6 * Count);

				    _swap.Flip();

				    _form.Text = string.Format("FPS: {0:0.0}  DT: {1:0.000}ms", GorgonTiming.FPS, GorgonTiming.Delta * 1000);

				    return true;
				});

			Assert.IsTrue(_form.TestResult == DialogResult.Yes);
		}
	}
}
