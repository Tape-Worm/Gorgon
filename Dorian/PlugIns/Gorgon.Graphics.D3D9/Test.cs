using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics.D3D9
{
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
		private float _pos = -0.2f;
		private Device _device = null;
		private GorgonDeviceWindow _window = null;


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
                new Vertex() { Color = Color.Blue.ToArgb(), Position = new Vector3(0.5f, -0.5f, 0.0f) },
                new Vertex() { Color = Color.Green.ToArgb(), Position = new Vector3(-0.5f, -0.5f, 0.0f) }
            });

			_vb.Unlock();

			device.SetRenderState(RenderState.Lighting, false);
		}

		public void Run()
		{
			if (_window.IsReady)
			{
				Viewport view = new Viewport(0, 0, _window.TargetInformation.Width, _window.TargetInformation.Height, 0.0f, 1.0f);

				_device.Viewport = view;
				_device.SetTransform(TransformState.Projection, Matrix.PerspectiveLH(1.0f, 1.0f, 0.1f, 10.0f));
				_device.SetTransform(TransformState.World, Matrix.Identity);
				_device.SetTransform(TransformState.View, Matrix.LookAtLH(new Vector3(0, 0, _pos), new Vector3(0, 0, 1.0f), Vector3.UnitY));

				_device.BeginScene();
				_device.Clear(ClearFlags.All, new Color4(0, 0, 0, 0), 1.0f, 0);
				_device.SetStreamSource(0, _vb, 0, 16);
				_device.VertexDeclaration = _vdecl;
				_device.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
				_device.EndScene();
			}

			_window.Display();
		}

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
		}
	}
}
