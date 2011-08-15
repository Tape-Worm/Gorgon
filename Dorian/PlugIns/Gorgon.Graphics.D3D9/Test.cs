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
		}
		
		private VertexBuffer _vb = null;
		private VertexDeclaration _vdecl = null;
		private Vertex[] triangle = new Vertex[3];
		private float _pos = -0.75f;
		private Device _device = null;
		private GorgonDeviceWindow _window = null;
		private Matrix _Yrot = Matrix.Identity;
		private float _angle = 0.0f;
		//private int _currentTime = 0;


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
																		new VertexElement(0, 12, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0)});

			_vb = new VertexBuffer(device, 3 * 16, Usage.WriteOnly, VertexFormat.None, Pool.Managed);
			_vb.Lock(0, 0, LockFlags.None).WriteRange(new[] {
		                new Vertex() { Color = Color.Red.ToArgb(), Position = new Vector3(0.0f, 0.5f, 0.0f) },
                new Vertex() { Color = Color.Blue.ToArgb(), Position = new Vector3(0.5f, -0.5f, 0.025f) },
                new Vertex() { Color = Color.Green.ToArgb(), Position = new Vector3(-0.5f, -0.5f, 0.0f) }
            });

			_vb.Unlock();

			device.SetRenderState(RenderState.Lighting, false);
			device.SetRenderState(RenderState.CullMode, Cull.None);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dt"></param>
		public void Run(float dt)
		{
			if (_window.IsReady)
			{
				Viewport view = new Viewport(0, 0, _window.Mode.Width, _window.Mode.Height, 0.0f, 1.0f);

				_Yrot = Matrix.RotationY(GorgonLibrary.Math.GorgonMathUtility.Radians(_angle));
				
				_angle += (5.000f * dt);
				if (_angle > 360.0f)
					_angle = 0.0f;

				//if (_currentTime == 0)
				//    _currentTime= Environment.TickCount;

				//float time = (float)(Environment.TickCount - _currentTime) / 1000.0f;

				//Gorgon.ApplicationWindow.Text = "Angle: " + _angle.ToString("0.0") + " Time: " + time.ToString("0.0");
				_device.Viewport = view;
				_device.SetTransform(TransformState.Projection, Matrix.PerspectiveFovLH(GorgonLibrary.Math.GorgonMathUtility.Radians(75.0f), (float)_window.Mode.Width/(float)_window.Mode.Height, 0.1f, 1000.0f));
				_device.SetTransform(TransformState.World, _Yrot);
				_device.SetTransform(TransformState.View, Matrix.LookAtLH(new Vector3(0, 0, _pos), new Vector3(0, 0, 1.0f), Vector3.UnitY));

				_device.BeginScene();
				switch (_window.DepthStencilFormat)
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
					
				_device.SetStreamSource(0, _vb, 0, 16);
				_device.VertexDeclaration = _vdecl;
				_device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
				_device.EndScene();
			}

			_window.Display();
		}

		/// <summary>
		/// 
		/// </summary>
		public void ShutDown()
		{
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
		}
	}
}
