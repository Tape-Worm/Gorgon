using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// 
	/// </summary>
	public class Test
	{
		private struct Vertex
		{
			public Vector3 Position;
			public int Color;
			public Vector2 UV;
		}
		
		private VertexBuffer _vb = null;
		private IndexBuffer _ib = null;
		private VertexDeclaration _vdecl = null;
		private Vertex[] triangle = new Vertex[3];
		private float _pos = -0.75f;
		private Device _device = null;
		private GorgonDeviceWindow _window = null;
		private Matrix _Yrot = Matrix.Identity;
		private float _angle = 0.0f;
		private int maxPasses = 0;
		private float _dps = 0.0f;
		private float _currentTime = 0;
		private bool _timeSwitch = false;
		private Texture _image = null;
		private static Surface _primarySurface = null;
		private Surface _swapSurface = null;
		private SwapChain _swapChain = null;
		private int outputIndex = 0;


		/// <summary>
		/// 
		/// </summary>
		/// <param name="window"></param>
		/// <param name="device"></param>
		public Test(GorgonDeviceWindow window, Device device)
		{
			window.AfterDeviceReset += new EventHandler(window_AfterDeviceReset);
			_window = window;
			_device = device;

			_vdecl = new VertexDeclaration(device, new VertexElement[] {new VertexElement(0, 0, DeclarationType.Float3, DeclarationMethod.Default, DeclarationUsage.Position, 0),
																		new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0),
																		new VertexElement(0, 16, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0)});

			_vb = new VertexBuffer(device, 4 * 24, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
			_ib = new IndexBuffer(device, 12, Usage.WriteOnly, Pool.Managed, true);
			_vb.Lock(0, 0, LockFlags.None).WriteRange(
				new[] {
						new Vertex() { Color = Color.White.ToArgb(), Position = new Vector3(-0.5f, 0.5f, 0.0f), UV = new Vector2(0, 0.0f)},
						new Vertex() { Color = Color.White.ToArgb(), Position = new Vector3(0.5f, -0.5f, 0.025f), UV = new Vector2(1.0f, 1.0f) },
						new Vertex() { Color = Color.White.ToArgb(), Position = new Vector3(-0.5f, -0.5f, 0.0f), UV = new Vector2(0.0f, 1.0f) },
						new Vertex() { Color = Color.White.ToArgb(), Position = new Vector3(0.5f, 0.5f, 0.0f), UV = new Vector2(1.0f, 0.0f) }				
			});

			_vb.Unlock();

			_ib.Lock(0, 0, LockFlags.None).WriteRange(
				new short[] 
				{
					1,
					2,
					0,
					0,
					3,
					1
				}
			);
			_ib.Unlock();

			device.SetRenderState(RenderState.Lighting, false);
			device.SetRenderState(RenderState.CullMode, Cull.None);
			if (_window.Settings.MSAAQualityLevel.Level != GorgonMSAALevel.None) 
			{
				device.SetRenderState(RenderState.MultisampleAntialias, true);
				device.SetRenderState(RenderState.MultisampleMask, 0xFF);
			}

			_image = Texture.FromFile(_device, @"..\..\..\..\Resources\Images\VBback.jpg");
			maxPasses = 0;
			outputIndex = _window.Settings.Device.Outputs.IndexOf(_window.Settings.Output);
			if (outputIndex == 0)
				_primarySurface = _swapSurface;
		}


		/// <summary>
		/// 
		/// </summary>
		/// <param name="dt"></param>
		public void Run(float dt)
		{
			if (_window.IsReady)
			{
				//Viewport view = new Viewport(0, 0, _window.Settings.Width, _window.Settings.Height, 0.0f, 1.0f);
				_swapChain = _device.GetSwapChain(outputIndex);
				_swapSurface = _swapChain.GetBackBuffer(0);

				_device.SetRenderTarget(0, _swapSurface);
				//_device.Viewport = view;

				_device.BeginScene();

				switch (_window.Settings.DepthStencilFormat)
				{
					case GorgonBufferFormat.D32_Float:
					case GorgonBufferFormat.D32_UIntNormal:
					case GorgonBufferFormat.D32_Float_Lockable:
					case GorgonBufferFormat.D24_UIntNormal_X8:
					case GorgonBufferFormat.D16_UIntNormal_Lockable:
					case GorgonBufferFormat.D16_UIntNormal:
						_device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new Color4(0, 0, 0, 0), 1.0f, 0);
						break;
					case GorgonBufferFormat.D24_Float_S8_UInt:
					case GorgonBufferFormat.D24_UIntNormal_X4S4_UInt:
					case GorgonBufferFormat.D15_UIntNormal_S1_UInt:
					case GorgonBufferFormat.D24_UIntNormal_S8_UInt:
						_device.Clear(ClearFlags.All, new Color4(0, 0, 0, 0), 1.0f, 0);
						break;
					default:
						_device.Clear(ClearFlags.Target, new Color4(0, 0, 0, 0), 1.0f, 0);
						break;
				}

				_device.SetTransform(TransformState.Projection, Matrix.PerspectiveFovLH(GorgonLibrary.Math.GorgonMathUtility.Radians(75.0f), (float)_window.Settings.Width / (float)_window.Settings.Height, 0.1f, 1000.0f));
				_device.SetTransform(TransformState.View, Matrix.LookAtLH(new Vector3(0, 0, _pos), new Vector3(0, 0, 1.0f), Vector3.UnitY));

				_device.SetStreamSource(0, _vb, 0, 24);
				_device.Indices = _ib;
				_device.VertexDeclaration = _vdecl;

				_device.SetTexture(0, _image);

				for (int i = 0; i <= maxPasses; i++)
				{
					float passAngle = 0.0f;

					if (maxPasses == 0)
						passAngle = GorgonLibrary.Math.GorgonMathUtility.Radians(_angle - (maxPasses - (i * (_dps))));
					else
						passAngle = GorgonLibrary.Math.GorgonMathUtility.Radians(_angle - (maxPasses - (i * (_dps / GorgonLibrary.Math.GorgonMathUtility.Pow(maxPasses, 2.25f)))));

					//_Yrot = Matrix.RotationY(passAngle);
					_Yrot = Matrix.RotationY(passAngle);
					_Yrot = _Yrot * Matrix.RotationX(passAngle);
					_Yrot = _Yrot * Matrix.RotationZ(passAngle);

					//if (_currentTime == 0)
					//    _currentTime= Environment.TickCount;

					//float time = (float)(Environment.TickCount - _currentTime) / 1000.0f;

					//Gorgon.ApplicationWindow.Text = "Angle: " + _angle.ToString("0.0") + " Time: " + time.ToString("0.0");
					_device.SetTransform(TransformState.World, _Yrot);

					if (_window.Settings.MSAAQualityLevel.Level != GorgonMSAALevel.None)
						_device.SetRenderState(RenderState.MultisampleMask, ((int)GorgonLibrary.Math.GorgonMathUtility.Pow(2, (8 - (maxPasses - i))) - 1) & 0xFF);
					_device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
				}
				_device.EndScene();

				_dps = GorgonLibrary.Math.GorgonMathUtility.Abs((GorgonLibrary.Math.GorgonMathUtility.Cos(GorgonLibrary.Math.GorgonMathUtility.Radians(_angle)) * _currentTime)) + 5.0f;

				_angle += (_dps * dt);
				if (_angle > 360.0f)
				{
					_angle = 0.0f;
					_timeSwitch = !_timeSwitch;
				}

				if (!_timeSwitch)
					_currentTime += dt * 45.0f;
				else
					_currentTime -= dt * 45.0f;


				if (_dps > 360.0f)
					_currentTime = 0.0f;

				if ((_window.Settings.MSAAQualityLevel.Level != GorgonMSAALevel.None))
				{
					maxPasses += 1;
					if (maxPasses > 8)
						maxPasses = 8;
				}

				//_device.SetRenderTarget(0, _primarySurface);
				if (_swapSurface != null)
					_swapSurface.Dispose();
				if (_swapChain != null)
					_swapChain.Dispose();
			}
			
			/*if (outputIndex > 0)
				_swapChain.Present(Present.None);*/
		}

		/// <summary>
		/// 
		/// </summary>
		public void ShutDown()
		{
			if (_swapSurface != null)
				_swapSurface.Dispose();
			if (_swapChain != null)
				_swapChain.Dispose();
			if (_image != null)
				_image.Dispose();
			if (_ib != null)
				_ib.Dispose();
			if (_vdecl != null)
				_vdecl.Dispose();
			if (_vb != null)
				_vb.Dispose();
		}

		/// <summary>
		/// Handles the AfterDeviceReset event of the window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		void window_AfterDeviceReset(object sender, EventArgs e)
		{
			_device.SetRenderState(RenderState.Lighting, false);
			_device.SetRenderState(RenderState.CullMode, Cull.None);
			if (_window.Settings.MSAAQualityLevel.Level != GorgonMSAALevel.None)
			{
				_device.SetRenderState(RenderState.MultisampleAntialias, true);
				_device.SetRenderState(RenderState.MultisampleMask, 0xFF);
			}

			if (_swapSurface != null)
				_swapSurface.Dispose();
			if (_swapChain != null)
				_swapChain.Dispose();

			outputIndex = _window.Settings.Device.Outputs.IndexOf(_window.Settings.Output);
			_swapChain = _device.GetSwapChain(outputIndex);
			_swapSurface = _swapChain.GetBackBuffer(0);
		}
	}
}
