﻿#region MIT.
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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using GorgonLibrary;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Input;
using GorgonLibrary.Math;
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
		// The layout of a vertex.
		private static GorgonInputLayout _normalVertexLayout;
		// The pixel shader.
		private static GorgonPixelShader _pixelShader;
		// The vertex shader.
		private static GorgonVertexShader _vertexShader;
		// The pixel shader.
		private static GorgonPixelShader _normalPixelShader;
		// The vertex shader.
		private static GorgonVertexShader _normalVertexShader;
		// World/view/project matrix combo.
		private static WorldViewProjection _wvp;
		// Triangle primitive.
		private static Triangle _triangle;
		// Plane primitive.
		private static Plane _plane;
		// Cube primitive.
		private static Cube _cube;
		// Sphere primitive.
		private static Sphere _sphere;
		// Light for our primitives.
		private static Light _light;
        // The texture to use.
	    private static GorgonTexture2D _texture;
        // Rotation value.
	    private static float _objRotation;
		// Input factory.
		private static GorgonInputFactory _input;
		// Keyboard interface.
		private static GorgonKeyboard _keyboard;
		// Mouse interface.
		private static GorgonPointingDevice _mouse;
		// Camera rotation amount.
		private static Vector3 _cameraRotation;
		// Camera position (eye).
		private static Vector3 _cameraPosition;
		// Mouse stating position.
		private static Vector2 _mouseStart;
		// Sphere position.
		private static float _yPos = 1.0f;

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

			if (_mouse.Exclusive)
			{
				var delta = new Vector2(_mouse.Position.X - _mouseStart.X, _mouse.Position.Y - _mouseStart.Y);
				_cameraRotation.Y += (360.0f * delta.Y.Sign() * GorgonTiming.Delta);
				_cameraRotation.X += (360.0f * delta.X.Sign() * GorgonTiming.Delta);
				_mouseStart = _mouse.Position;
			}

			ProcessKeys();

            //_triangle.Rotation = new Vector3(_objRotation, _objRotation, _objRotation);
            
		    world = _triangle.World;
            _wvp.UpdateWorldMatrix(ref world);

            _graphics.Shaders.PixelShader.Resources[0] = _triangle.Texture;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_triangle.VertexBuffer, Vertex3D.Size);
			_graphics.Input.IndexBuffer = _triangle.IndexBuffer;
			_graphics.Input.PrimitiveType = _triangle.PrimitiveType;

			_graphics.Output.DrawIndexed(0, 0, _triangle.IndexCount);
          
			//_plane.Rotation = new Vector3(_objRotation, 0, 0);
			world = _plane.World;
			_wvp.UpdateWorldMatrix(ref world);

			_graphics.Shaders.PixelShader.Resources[0] = _plane.Texture;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_plane.VertexBuffer, Vertex3D.Size);
			_graphics.Input.IndexBuffer = _plane.IndexBuffer;
			_graphics.Input.PrimitiveType = _plane.PrimitiveType;

			_graphics.Output.DrawIndexed(0, 0, _plane.IndexCount);

			_cube.Rotation = new Vector3(_objRotation, _objRotation, _objRotation);
			world = _cube.World;
			_wvp.UpdateWorldMatrix(ref world);

			_graphics.Shaders.PixelShader.Resources[0] = _cube.Texture;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_cube.VertexBuffer, Vertex3D.Size);
			_graphics.Input.IndexBuffer = _cube.IndexBuffer;
			_graphics.Input.PrimitiveType = _cube.PrimitiveType;

			_graphics.Output.DrawIndexed(0, 0, _cube.IndexCount);

			_yPos = (_objRotation.Radians().Sin().Abs() * 2.0f) - 1.25f;
			_sphere.Position = new Vector3(-2.0f, _yPos, 0.75f);
			_sphere.Rotation = new Vector3(_objRotation, _objRotation, 0);
			world = _sphere.World;
			_wvp.UpdateWorldMatrix(ref world);

			_graphics.Shaders.PixelShader.Resources[0] = _sphere.Texture;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_sphere.VertexBuffer, Vertex3D.Size);
			_graphics.Input.IndexBuffer = _sphere.IndexBuffer;
			_graphics.Input.PrimitiveType = _sphere.PrimitiveType;

			_graphics.Output.DrawIndexed(0, 0, _sphere.IndexCount);

			_graphics.Shaders.PixelShader.Resources[0] = _sphere.Texture;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_sphere.VertexBuffer, Vertex3D.Size);
			_graphics.Input.IndexBuffer = _sphere.IndexBuffer;
			_graphics.Input.PrimitiveType = _sphere.PrimitiveType;

			_graphics.Output.DrawIndexed(0, 0, _sphere.IndexCount);

			_graphics.Input.Layout = _normalVertexLayout;
			_graphics.Input.PrimitiveType = PrimitiveType.LineList;
			_graphics.Shaders.PixelShader.Resources[0] = null;
			_graphics.Shaders.PixelShader.Current = _normalPixelShader;
			_graphics.Shaders.VertexShader.Current = _normalVertexShader;
			_graphics.Input.IndexBuffer = null;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_sphere.Normals, Vector4.SizeInBytes);

			_graphics.Output.Draw(0, _sphere.VertexCount * 2);

			_graphics.Input.Layout = _vertexLayout;
			_graphics.Shaders.PixelShader.Current = _pixelShader;
			_graphics.Shaders.VertexShader.Current = _vertexShader;

			var state = _renderer2D.Begin2D();
			_renderer2D.Drawing.DrawString(_font,
			                               string.Format(
			                                             "FPS: {0:0.0}, Delta: {1:0.000} ms Secs: {3:0} CamRot: {2} Mouse: {4:0}x{5:0}, {6:0}x{7:0}",
			                                             GorgonTiming.FPS,
			                                             GorgonTiming.Delta * 1000,
			                                             _cameraRotation,
			                                             GorgonTiming.SecondsSinceStart,
			                                             _mouse.Position.X,
			                                             _mouse.Position.Y,
			                                             _mouse.RelativePosition.X,
			                                             _mouse.RelativePosition.Y),
			                               Vector2.Zero,
			                               Color.White);
			_renderer2D.Flush();
			_renderer2D.End2D(state);

			_swapChain.Flip();
			return true;
		}

		private static void ProcessKeys()
		{
			var pos = Vector3.Zero;
			var up = Vector3.UnitY;
			var right = Vector3.UnitX;
			Vector3 forward = Vector3.UnitZ;
			Vector3 upDir;
			Vector3 rightDir;
			Vector3 lookAt;
			Matrix rotMatrix;
			Vector3 cameraDir = Vector3.Zero;

			if (_keyboard.KeyStates[KeyboardKeys.Left] == KeyState.Down)
			{
				_cameraRotation.X -= 40.0f * GorgonTiming.Delta;
			} else
			if (_keyboard.KeyStates[KeyboardKeys.Right] == KeyState.Down)
			{
				_cameraRotation.X += 40.0f * GorgonTiming.Delta;
			} else
			if (_keyboard.KeyStates[KeyboardKeys.Up] == KeyState.Down)
			{
				_cameraRotation.Y -= 40.0f * GorgonTiming.Delta;
			} else
			if (_keyboard.KeyStates[KeyboardKeys.Down] == KeyState.Down)
			{
				_cameraRotation.Y += 40.0f * GorgonTiming.Delta;
			} else if (_keyboard.KeyStates[KeyboardKeys.PageUp] == KeyState.Down)
			{
				_cameraRotation.Z -= 40.0f * GorgonTiming.Delta;
			}
			else if (_keyboard.KeyStates[KeyboardKeys.PageDown] == KeyState.Down)
			{
				_cameraRotation.Z += 40.0f * GorgonTiming.Delta;
			}
			else if (_keyboard.KeyStates[KeyboardKeys.D] == KeyState.Down)
			{
				cameraDir.X = 2.0f * GorgonTiming.Delta;
			}
			else if (_keyboard.KeyStates[KeyboardKeys.A] == KeyState.Down)
			{
				cameraDir.X = -2.0f * GorgonTiming.Delta;
			}
			else if (_keyboard.KeyStates[KeyboardKeys.W] == KeyState.Down)
			{
				cameraDir.Z = 2.0f * GorgonTiming.Delta;
			}
			else if (_keyboard.KeyStates[KeyboardKeys.S] == KeyState.Down)
			{
				cameraDir.Z = -2.0f * GorgonTiming.Delta;
			}
			else if (_keyboard.KeyStates[KeyboardKeys.Q] == KeyState.Down)
			{
				cameraDir.Y = 2.0f * GorgonTiming.Delta;
			}
			else if (_keyboard.KeyStates[KeyboardKeys.E] == KeyState.Down)
			{
				cameraDir.Y = -2.0f * GorgonTiming.Delta;
			}

			Matrix.RotationYawPitchRoll(_cameraRotation.X.Radians(), _cameraRotation.Y.Radians(), _cameraRotation.Z.Radians(), out rotMatrix);
			Vector3.TransformCoordinate(ref forward, ref rotMatrix, out lookAt);
			Vector3.TransformCoordinate(ref right, ref rotMatrix, out rightDir);
			Vector3.Cross(ref lookAt, ref rightDir, out upDir);

			Vector3.Multiply(ref rightDir, cameraDir.X, out rightDir);
			Vector3.Add(ref _cameraPosition, ref rightDir, out _cameraPosition);
			Vector3.Multiply(ref lookAt, cameraDir.Z, out forward);
			Vector3.Add(ref _cameraPosition, ref forward, out _cameraPosition);
			Vector3.Multiply(ref upDir, cameraDir.Y, out up);
			Vector3.Add(ref _cameraPosition, ref up, out _cameraPosition);

			lookAt.Normalize();
			Vector3.Add(ref _cameraPosition, ref lookAt, out lookAt);
			
			_wvp.UpdateViewMatrix(ref _cameraPosition, ref lookAt, ref upDir);
		}

		/// <summary>
		/// Handles the Down event of the Mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">The <see cref="PointingDeviceEventArgs"/> instance containing the event data.</param>
		private static void Mouse_Down(object sender, PointingDeviceEventArgs args)
		{
			if (((args.Buttons & PointingDeviceButtons.Right) != PointingDeviceButtons.Right)
				&& (!_mouse.Exclusive))
			{
				return;
			}

			_mouseStart = args.Position;
			_mouse.Exclusive = true;
		}

		/// <summary>
		/// Handles the Up event of the Mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="args">The <see cref="PointingDeviceEventArgs"/> instance containing the event data.</param>
		private static void Mouse_Up(object sender, PointingDeviceEventArgs args)
		{
			if (((args.Buttons & PointingDeviceButtons.Right) != PointingDeviceButtons.Right)
				&& (_mouse.Exclusive))
			{
				return;
			}

			_mouse.Exclusive = false;
			_mouse.ShowCursor();
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
			_normalVertexShader = _graphics.Shaders.CreateShader<GorgonVertexShader>("NormalVertexShader", "NormalVS", Resources.Shaders);
			_normalPixelShader = _graphics.Shaders.CreateShader<GorgonPixelShader>("NormalPixelShader", "NormalPS", Resources.Shaders);
			_vertexLayout = _graphics.Input.CreateInputLayout("Vertex3D", typeof(Vertex3D), _vertexShader);
			_normalVertexLayout = _graphics.Input.CreateInputLayout("NormalVertex",
			                                                        new[]
			                                                        {
				                                                        new GorgonInputElement("SV_POSITION",
				                                                                               BufferFormat.R32G32B32A32_Float,
				                                                                               0,
				                                                                               0,
				                                                                               0,
				                                                                               false,
				                                                                               0),
			                                                        },
			                                                        _normalVertexShader);

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
				                                    Position = new Vector4(-12.5f, -1.5f, 12.5f, 1),
													Normal = faceNormal,
													UV = new Vector2(0, 1.0f)
												}, new Vertex3D
												{
													Position = new Vector4(0, 24.5f, 12.5f, 1),
													Normal = faceNormal,
													UV = new Vector2(0.5f, 0.0f)
												}, new Vertex3D
												{
													Position = new Vector4(12.5f, -1.5f, 12.5f, 1),
													Normal = faceNormal,
													UV = new Vector2(1.0f, 1.0f)
												})
			            {
				            Texture = _texture,
				            Position = new Vector3(0, 0, 1.0f)
			            };

			_plane = new Plane(_graphics, new Vector2(25.0f, 25.0f), new RectangleF(0, 0, 1.0f, 1.0f), new Vector3(90, 0, 0), 32, 32)
			         {
				         Position = new Vector3(0, -1.5f, 1.0f),
						 Texture = _texture
			         };

			_cube = new Cube(_graphics, new Vector3(1, 1, 1), new RectangleF(0, 0, 1.0f, 1.0f), new Vector3(45.0f, 0, 0), 8, 8)
			        {
				        Position = new Vector3(0, 0, 1.5f),
				        Texture = _texture
			        };

			_sphere = new Sphere(_graphics, 1.0f, new RectangleF(0.25f, 0.25f, 4.0f, 4.0f), Vector3.Zero, 64, 64)
			          {
				          Position = new Vector3(-2.0f, 1.0f, 0.75f),
				          Texture = _texture
			          };

			_graphics.Shaders.PixelShader.TextureSamplers[0] = new GorgonTextureSamplerStates
			                                                   {
																   TextureFilter = TextureFilter.Linear,
																   HorizontalAddressing = TextureAddressing.Wrap,
																   VerticalAddressing = TextureAddressing.Wrap,
																   DepthAddressing = TextureAddressing.Wrap,
																   ComparisonFunction = ComparisonOperators.Always
			                                                   };

			_light = new Light(_graphics);
			var lightPosition = new Vector3(1.0f, 1.0f, -1.0f);
			_light.UpdateLightPosition(ref lightPosition, 0);
		    GorgonColor color = GorgonColor.White;
            _light.UpdateSpecular(ref color, 256.0f, 0);

			lightPosition = new Vector3(-5.0f, 5.0f, 8.0f);
			_light.UpdateLightPosition(ref lightPosition, 1);
			color = Color.Yellow;
			_light.UpdateColor(ref color, 1);
			_light.UpdateSpecular(ref color, 2048.0f, 1);
			_light.UpdateAttenuation(10.0f, 1);

			lightPosition = new Vector3(5.0f, 3.0f, 10.0f);
			_light.UpdateLightPosition(ref lightPosition, 2);
			color = Color.Red;
			_light.UpdateColor(ref color, 2);
			_light.UpdateSpecular(ref color, 0.0f, 2);
			_light.UpdateAttenuation(16.0f, 2);

			var eye = Vector3.Zero;
			var lookAt = Vector3.UnitZ;
			var up = Vector3.UnitY;
			_wvp.UpdateViewMatrix(ref eye, ref lookAt, ref up);

			_cameraRotation = Vector2.Zero;

			Gorgon.PlugIns.LoadPlugInAssembly(Application.StartupPath + @"\Gorgon.Input.Raw.dll");

			_input = GorgonInputFactory.CreateInputFactory("GorgonLibrary.Input.GorgonRawPlugIn");
			_keyboard = _input.CreateKeyboard(_form);
			_mouse = _input.CreatePointingDevice(_form);

			_mouse.PointingDeviceDown += Mouse_Down;
			_mouse.PointingDeviceUp += Mouse_Up;
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
				if (_sphere != null)
				{
					_sphere.Dispose();
				}
					
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