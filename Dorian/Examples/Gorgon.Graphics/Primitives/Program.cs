#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Thursday, August 7, 2014 9:38:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
using GorgonLibrary.Graphics.Example.Properties;
using SlimMath;

namespace GorgonLibrary.Graphics.Example
{
	static class Program
	{
		// Main application form.
		private static FormMain _form;
		// Graphics interface.
		private static GorgonGraphics _graphics;
		// Primary swap chain.
		private static GorgonSwapChain _swapChain;
		// 2D renderer.
		private static Gorgon2D _renderer2D;
		// Font
		private static GorgonFont _font;
		// The layout of a vertex.
		private static GorgonInputLayout _vertexLayout;
		// The pixel shader.
		private static GorgonPixelShader _pixelShader;
		// The vertex shader.
		private static GorgonVertexShader _vertexShader;
		// World/view/project matrix combo.
		private static WorldViewProjection _wvp;
		// Triangle primitive.
		private static Triangle _triangle;
		// Plane primitive.
		private static Plane _plane;
		// Cube primitive.
		private static Cube _cube;
		// Light for our primitives.
		private static Light _light;
        // The texture to use.
	    private static GorgonTexture2D _texture;
        // Rotation value.
	    private static float _objRotation;

		/// <summary>
		/// Main application loop.
		/// </summary>
		/// <returns>TRUE to continue processing, FALSE to stop.</returns>
		private static bool Idle()
		{
			Matrix world;

			_swapChain.Clear(Color.CornflowerBlue, 1.0f);

		    _objRotation += 50.0f * GorgonTiming.Delta;

		    if (_objRotation > 359.9f)
		    {
		        _objRotation -= 359.9f;
		    }

            _triangle.Rotation = new Vector3(_objRotation, _objRotation, _objRotation);
            
		    /*world = _triangle.World;
            _wvp.UpdateWorldMatrix(ref world);

            _graphics.Shaders.PixelShader.Resources[0] = _triangle.Texture;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_triangle.VertexBuffer, Vertex3D.Size);
			_graphics.Input.IndexBuffer = _triangle.IndexBuffer;
			_graphics.Input.PrimitiveType = _triangle.PrimitiveType;

			_graphics.Output.DrawIndexed(0, 0, _triangle.IndexCount);
          
			_plane.Rotation = new Vector3(_objRotation, 0, 0);
			world = _plane.World;
			_wvp.UpdateWorldMatrix(ref world);

			_graphics.Shaders.PixelShader.Resources[0] = _plane.Texture;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_plane.VertexBuffer, Vertex3D.Size);
			_graphics.Input.IndexBuffer = _plane.IndexBuffer;
			_graphics.Input.PrimitiveType = _plane.PrimitiveType;

			_graphics.Output.DrawIndexed(0, 0, _plane.IndexCount);*/

			_cube.Rotation = new Vector3(_objRotation, _objRotation, _objRotation);
			world = _cube.World;
			_wvp.UpdateWorldMatrix(ref world);

			_graphics.Shaders.PixelShader.Resources[0] = _cube.Texture;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_cube.VertexBuffer, Vertex3D.Size);
			_graphics.Input.IndexBuffer = _cube.IndexBuffer;
			_graphics.Input.PrimitiveType = _cube.PrimitiveType;

			_graphics.Output.DrawIndexed(0, 0, _cube.IndexCount);

			var state = _renderer2D.Begin2D();
			_renderer2D.Drawing.DrawString(_font, string.Format("FPS: {0:0.0}, Delta: {1:0.000} ms", GorgonTiming.FPS, GorgonTiming.Delta * 1000), Vector2.Zero, Color.White);
			_renderer2D.Flush();
			_renderer2D.End2D(state);

			_swapChain.Flip();
			return true;
		}

		/// <summary>
		/// Function to initialize the application.
		/// </summary>
		private static void Initialize()
		{
			_form = new FormMain();
			
			_graphics = new GorgonGraphics();
			_swapChain = _graphics.Output.CreateSwapChain("Swap",
			                                              new GorgonSwapChainSettings
			                                              {
															  Window = _form,
															  IsWindowed = true,
															  DepthStencilFormat = BufferFormat.D24_UIntNormal_S8_UInt,
															  Format = BufferFormat.R8G8B8A8_UIntNormal
			                                              });

			_renderer2D = _graphics.Output.Create2DRenderer(_swapChain);

			_font = _graphics.Fonts.CreateFont("AppFont",
			                                   new GorgonFontSettings
			                                   {
												   FontFamilyName = "Calibri",
												   FontStyle = FontStyle.Bold,
												   FontHeightMode = FontHeightMode.Pixels,
												   AntiAliasingMode = FontAntiAliasMode.AntiAlias,
												   OutlineSize = 1,
												   OutlineColor1 = Color.Black,
												   Size = 16.0f
			                                   });

			_vertexShader = _graphics.Shaders.CreateShader<GorgonVertexShader>("VertexShader", "PrimVS", Resources.Shaders);
			_pixelShader = _graphics.Shaders.CreateShader<GorgonPixelShader>("PixelShader", "PrimPS", Resources.Shaders);
			_vertexLayout = _graphics.Input.CreateInputLayout("Vertex3D", typeof(Vertex3D), _vertexShader);

			_graphics.Shaders.VertexShader.Current = _vertexShader;
			_graphics.Shaders.PixelShader.Current = _pixelShader;
			_graphics.Input.Layout = _vertexLayout;
			_graphics.Input.PrimitiveType = PrimitiveType.TriangleList;

		    _texture = _graphics.Textures.CreateTexture<GorgonTexture2D>("UVTexture", Resources.UV);

			var depth = new GorgonDepthStencilStates
			            {
				            DepthComparison = ComparisonOperators.LessEqual,
				            IsDepthEnabled = true,
				            IsDepthWriteEnabled = true
			            };

			_graphics.Output.DepthStencilState.States = depth;
			_graphics.Output.SetRenderTarget(_swapChain, _swapChain.DepthStencilBuffer);
			_graphics.Rasterizer.States = GorgonRasterizerStates.CullBackFace;
			_graphics.Rasterizer.SetViewport(new GorgonViewport(0, 0, _form.ClientSize.Width, _form.ClientSize.Height, 0, 1.0f));
		    _graphics.Shaders.PixelShader.TextureSamplers[0] = GorgonTextureSamplerStates.LinearFilter;
            
			_wvp = new WorldViewProjection(_graphics);
			_wvp.UpdateProjection(75.0f, _form.ClientSize.Width, _form.ClientSize.Height);

			// When we resize, update the projection and viewport to match our client size.
			_form.Resize += (sender, args) =>
			                {
								_graphics.Rasterizer.SetViewport(new GorgonViewport(0, 0, _form.ClientSize.Width, _form.ClientSize.Height, 0, 1.0f));
								_wvp.UpdateProjection(75.0f, _form.ClientSize.Width, _form.ClientSize.Height);
			                };

			var fnU = new Vector3(0.5f, 1.0f, 0);
			var fnV = new Vector3(1.0f, 1.0f, 0);
			Vector3 faceNormal;
			Vector3.Cross(ref fnU, ref fnV, out faceNormal);
			faceNormal.Normalize();

			_triangle = new Triangle(_graphics, new Vertex3D
			                                    {
				                                    Position = new Vector4(-0.5f, -0.5f, 0.0f, 1),
													Normal = faceNormal,
													UV = new Vector2(0, 1.0f)
												}, new Vertex3D
												{
													Position = new Vector4(0, 0.5f, 0.0f, 1),
													Normal = faceNormal,
													UV = new Vector2(0.5f, 0.0f)
												}, new Vertex3D
												{
													Position = new Vector4(0.5f, -0.5f, 0.0f, 1),
													Normal = faceNormal,
													UV = new Vector2(1.0f, 1.0f)
												})
			            {
				            Texture = _texture,
				            Position = new Vector3(0, 0, 1.0f)
			            };

			_plane = new Plane(_graphics, new Vector2(1.0f, 1.0f), new RectangleF(0, 0, 1.0f, 1.0f), new Vector3(180, 0, 0), 8, 8)
			         {
				         Position = new Vector3(0, -0.25f, 1.0f),
						 Texture = _texture
			         };

			_cube = new Cube(_graphics, new Vector3(1, 1, 1), new RectangleF(0, 0, 1.0f, 1.0f), new Vector3(0), 8, 8)
			        {
				        Position = new Vector3(0, 0, 1.5f),
				        Texture = _texture
			        };

			_light = new Light(_graphics);
			var lightPosition = new Vector3(1.0f, 1.0f, -1.0f);
			_light.UpdateLightPosition(ref lightPosition);
		    GorgonColor color = GorgonColor.White;
            _light.UpdateSpecular(ref color, 256.0f);
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			try
			{
				Initialize();

				Gorgon.Run(_form, Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonDialogs.ErrorBox(null, ex));
			}
			finally
			{
				if (_light != null)
				{
					_light.Dispose();
				}

				if (_cube != null)
				{
					_cube.Dispose();
				}

				if (_plane != null)
				{
					_plane.Dispose();
				}

				if (_triangle != null)
				{
					_triangle.Dispose();
				}

				if (_wvp != null)
				{
					_wvp.Dispose();
				}

				if (_renderer2D != null)
				{
					_renderer2D.Dispose();
				}

				if (_swapChain != null)
				{
					_swapChain.Dispose();
				}

				if (_graphics != null)
				{
					_graphics.Dispose();
				}
			}
		}
	}
}
