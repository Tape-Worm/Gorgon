#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Thursday, May 16, 2013 9:24:11 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics.Test
{
	public class GraphicsFramework
		: IDisposable
	{
		#region Value Types.
		/// <summary>
		/// Vertex for testing.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct Vertex
		{
			[InputElement(0, "POSITION")]
			public Vector4 Position;
			[InputElement(1, "COLOR")]
			public Vector4 Color;
			[InputElement(2, "TEXCOORD")]
			public Vector2 TexCoord;
		}
		#endregion


		#region Variables.
		private TestForm _form;								// Test form for interactive testing.
		private bool _disposed;								// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the maximum timeout for the test.
		/// </summary>
		public int MaxTimeout
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the graphics instance.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the vertex shader.
		/// </summary>
		public GorgonVertexShader VertexShader
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the input layout.
		/// </summary>
		public GorgonInputLayout Layout
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the vertex buffer.
		/// </summary>
		public GorgonVertexBuffer Vertices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the index buffer.
		/// </summary>
		public GorgonIndexBuffer Indices
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the swap chain.
		/// </summary>
		public GorgonSwapChain Screen
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the pixel shader.
		/// </summary>
		public GorgonPixelShader PixelShader
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the function to run during idle time.
		/// </summary>
		public Action IdleFunc
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to process idle time.
		/// </summary>
		/// <returns>TRUE to continue processing, FALSE to stop.</returns>
		private bool Idle()
		{
			if (Graphics.Output.GetRenderTarget(0) == (GorgonRenderTargetView)Screen)
			{
				Screen.Clear(GorgonColor.Black);
			}

			if (IdleFunc != null)
			{
				IdleFunc();
			}

			Graphics.Output.DrawIndexed(0, 0, 6);

			if (Graphics.Output.GetRenderTarget(0) == (GorgonRenderTargetView)Screen)
			{
				Screen.Flip(1);
			}

			if ((MaxTimeout > 0) && (GorgonTiming.MillisecondsSinceStart > MaxTimeout))
			{
				_form.Close();
				return false;
			}

			return true;
		}

		/// <summary>
		/// Function to run the test.
		/// </summary>
		/// <returns>The test result for manual testing.</returns>
		public DialogResult Run()
		{
            _form.WindowState = FormWindowState.Minimized;
		    _form.Show();
            _form.WindowState = FormWindowState.Normal;

		    //_form.TopMost = true;

            Gorgon.Run(_form, Idle);

			return _form.TestResult;
		}

		/// <summary>
		/// Function to create a test scene.
		/// </summary>
		/// <param name="vs">Vertex Shader Code.</param>
		/// <param name="ps">Pixel shader code.</param>
		/// <param name="showTestPanel">TRUE to show the testing panel on the form, FALSE to hide it.</param>
		public void CreateTestScene(string vs, string ps, bool showTestPanel)
		{
			_form.ClientSize = new Size(1280, 800);
			_form.ShowTestPanel = showTestPanel;

			VertexShader = Graphics.Shaders.CreateShader<GorgonVertexShader>("VS",
																			   "TestVS",
																			   vs,
																			   true);
			PixelShader = Graphics.Shaders.CreateShader<GorgonPixelShader>("PS",
																			 "TestPS",
																			 ps,
																			 true);

			Layout = Graphics.Input.CreateInputLayout("Layout", typeof(Vertex), VertexShader);

			Vertices = Graphics.Buffers.CreateVertexBuffer("VB",
														   new[]
                                                               {
                                                                   new Vertex
                                                                       {
                                                                           Position = new Vector4(-0.5f, 0.5f, 0.5f, 1.0f),
                                                                           Color = new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                                                           TexCoord = new Vector2(0, 0)
                                                                       },
                                                                    new Vertex
                                                                        {
                                                                           Position = new Vector4(0.5f, -0.5f, 0.5f, 1.0f),
                                                                           Color = new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                                                           TexCoord = new Vector2(1, 1)
                                                                        },
                                                                    new Vertex
                                                                        {
                                                                           Position = new Vector4(-0.5f, -0.5f, 0.5f, 1.0f),
                                                                           Color = new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                                                                           TexCoord = new Vector2(0, 1)
                                                                        },
                                                                    new Vertex
                                                                        {
                                                                           Position = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
                                                                           Color = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
                                                                           TexCoord = new Vector2(1, 0)
                                                                        }
                                                               }, BufferUsage.Immutable);

			Indices = Graphics.Buffers.CreateIndexBuffer("IB",
			                                           new[]
				                                           {
					                                           0, 1, 2, 3, 1, 0
				                                           }, BufferUsage.Immutable);


			Graphics.Input.Layout = Layout;
			Graphics.Input.PrimitiveType = PrimitiveType.TriangleList;
			Graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(Vertices, 40);
			Graphics.Input.IndexBuffer = Indices;
			Graphics.Rasterizer.SetViewport(new GorgonViewport(0, 0, _form.panelDisplay.ClientSize.Width, _form.panelDisplay.ClientSize.Height, 0, 1.0f));
			Graphics.Rasterizer.States = GorgonRasterizerStates.DefaultStates;
			Graphics.Shaders.VertexShader.Current = VertexShader;
			Graphics.Shaders.PixelShader.Current = PixelShader;
			Graphics.Shaders.PixelShader.TextureSamplers[0] = GorgonTextureSamplerStates.DefaultStates;

			//_form.TopMost = true;

			Screen = Graphics.Output.CreateSwapChain("Screen",
													   new GorgonSwapChainSettings
													   {
														   Window = _form.panelDisplay,
														   IsWindowed = true
													   });

            Graphics.Output.SetRenderTarget(Screen, Screen.DepthStencilBuffer);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GraphicsFramework"/> class.
		/// </summary>
		public GraphicsFramework()
		{
			GorgonVideoDeviceEnumerator.Enumerate(true, true);
#error Test WARP/Refrast... for some reason the hardware colors are being mixed properly, but it looks like the software isn't.  This should be the same on both?
			_form = new TestForm();
			//Graphics = new GorgonGraphics(GorgonVideoDeviceEnumerator.VideoDevices.Single(item => item.VideoDeviceType == VideoDeviceType.ReferenceRasterizer));
			Graphics = new GorgonGraphics();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (!disposing)
			{
				return;
			}

			if (Screen != null)
			{
				Screen.Dispose();
				Screen = null;
			}

			if (Indices != null)
			{
				Indices.Dispose();
				Indices = null;
			}

			if (Vertices != null)
			{
				Vertices.Dispose();
				Vertices = null;
			}

			if (Layout != null)
			{
				Layout.Dispose();
				Layout = null;
			}

			if (PixelShader != null)
			{
				PixelShader.Dispose();
				PixelShader = null;
			}

			if (VertexShader != null)
			{
				VertexShader.Dispose();
				VertexShader = null;
			}

			if (Graphics != null)
			{
				Graphics.Dispose();
				Graphics = null;
			}

			if (_form != null)
			{
				_form.Dispose();
				_form = null;
			}

			_disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
		}
		#endregion
	}
}
