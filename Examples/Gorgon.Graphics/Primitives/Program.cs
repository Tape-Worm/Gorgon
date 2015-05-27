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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Input;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.UI;
using Gorgon.Graphics.Example.Properties;
using SlimMath;

namespace Gorgon.Graphics.Example
{
	[StructLayout(LayoutKind.Sequential, Pack = 16, Size = 16)]
	struct Material
	{
		public Vector2 UVOffset;
		public float SpecularPower;
	}

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
		// The pixel shader.
		private static GorgonPixelShader _bumpShader;
		// The vertex shader.
		private static GorgonVertexShader _vertexShader;
		// The pixel shader.
		private static GorgonPixelShader _normalPixelShader;
		// The water shader.
		private static GorgonPixelShader _waterShader;
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
		// Sphere primitive.
		private static Sphere _clouds;
		// Icosphere primitive.
		private static IcoSphere _icoSphere;
		// Light for our primitives.
		private static Light _light;
        // The texture to use.
	    private static GorgonTexture2D _texture;
		// Earth texture.
		private static GorgonTexture2D _earf;
        // Rotation value.
	    private static float _objRotation;
		// Rotation value.
		private static float _cloudRotation;
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
		// Lock to sphere.
		private static bool _lock;
		// Mouse sensitivity.
		private static float _sensitivity = 0.5f;
		// Normal map.
		private static GorgonTexture2D _normalMap;
		// Normal map.
		private static GorgonTexture2D _normalEarfMap;
		// Spec map.
		private static GorgonTexture2D _specMap;
		// Spec map.
		private static GorgonTexture2D _specEarfMap;
		// CLoud map.
		private static GorgonTexture2D _cloudMap;
		// Gorgon normal map.
		private static GorgonTexture2D _gorgNrm;
		// Offset buffer for material.
		private static GorgonConstantBuffer _materialBuffer;
		// Material for objects.
		private static Material _material;

		/// <summary>
		/// Main application loop.
		/// </summary>
		/// <returns><c>true</c> to continue processing, <c>false</c> to stop.</returns>
		private static bool Idle()
		{
			Matrix world;

			_swapChain.Clear(Color.CornflowerBlue, 1.0f);

			_cloudRotation += 2.0f * GorgonTiming.Delta;
		    _objRotation += 50.0f * GorgonTiming.Delta;

			if (_cloudRotation > 359.9f)
			{
				_cloudRotation -= 359.9f;
			}

		    if (_objRotation > 359.9f)
		    {
		        _objRotation -= 359.9f;
		    }

			ProcessKeys();

			_material.UVOffset = new Vector2(0, _material.UVOffset.Y - 0.125f * GorgonTiming.Delta);

			_graphics.Shaders.PixelShader.Current = _waterShader;
			
			if (_material.UVOffset.Y < 0.0f)
			{
				_material.UVOffset = new Vector2(0, 1.0f + _material.UVOffset.Y);
			}

			_materialBuffer.Update(ref _material);

			world = _triangle.World;
			_wvp.UpdateWorldMatrix(ref world);

			_graphics.Shaders.PixelShader.Resources[0] = _triangle.Texture;
			_graphics.Shaders.PixelShader.Resources[1] = _normalMap;
			_graphics.Shaders.PixelShader.Resources[2] = _specMap;
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
			

			var worldRot = _icoSphere.Rotation;
			worldRot.Y += 4.0f * GorgonTiming.Delta;
			_icoSphere.Rotation = worldRot;
			world = _icoSphere.World;
			_wvp.UpdateWorldMatrix(ref world);
			_graphics.Shaders.PixelShader.Current = _bumpShader;
			_graphics.Shaders.PixelShader.Resources[0] = _icoSphere.Texture;
			_graphics.Shaders.PixelShader.Resources[1] = _normalEarfMap;
			_graphics.Shaders.PixelShader.Resources[2] = _specEarfMap;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_icoSphere.VertexBuffer, Vertex3D.Size);
			_graphics.Input.IndexBuffer = _icoSphere.IndexBuffer;
			_graphics.Input.PrimitiveType = _icoSphere.PrimitiveType;

			_graphics.Output.DrawIndexed(0, 0, _icoSphere.IndexCount);

			var cubeMat = new Material
			                   {
				                   UVOffset = Vector2.Zero,
				                   SpecularPower = 0.0f
			                   };
			_materialBuffer.Update(ref cubeMat);

			_cube.Rotation = new Vector3(_objRotation, _objRotation, _objRotation);
			world = _cube.World;
			_wvp.UpdateWorldMatrix(ref world);
			_graphics.Shaders.PixelShader.Resources[0] = _cube.Texture;
			_graphics.Shaders.PixelShader.Resources[1] = _gorgNrm;

			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_cube.VertexBuffer, Vertex3D.Size);
			_graphics.Input.IndexBuffer = _cube.IndexBuffer;
			_graphics.Input.PrimitiveType = _cube.PrimitiveType;

			_graphics.Output.DrawIndexed(0, 0, _cube.IndexCount);
			
			var sphereMat = new Material
			{
				UVOffset = Vector2.Zero,
				SpecularPower = 0.75f
			};
			_materialBuffer.Update(ref sphereMat);

			_yPos = (_objRotation.Radians().Sin().Abs() * 2.0f) - 1.10f;
			_sphere.Position = new Vector3(-2.0f, _yPos, 0.75f);
			_sphere.Rotation = new Vector3(_objRotation, _objRotation, 0);
			world = _sphere.World;
			_wvp.UpdateWorldMatrix(ref world);

			_graphics.Shaders.PixelShader.Current = _pixelShader;
			_graphics.Shaders.PixelShader.Resources[0] = _sphere.Texture;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_sphere.VertexBuffer, Vertex3D.Size);
			_graphics.Input.IndexBuffer = _sphere.IndexBuffer;
			_graphics.Input.PrimitiveType = _sphere.PrimitiveType;

			_graphics.Output.DrawIndexed(0, 0, _sphere.IndexCount);
			
			sphereMat = new Material
			{
				UVOffset = Vector2.Zero,
				SpecularPower = 0.0f
			};
			_materialBuffer.Update(ref sphereMat);

			_graphics.Shaders.PixelShader.Resources[0] = _clouds.Texture;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_clouds.VertexBuffer, Vertex3D.Size);
			_graphics.Input.IndexBuffer = _clouds.IndexBuffer;
			_graphics.Input.PrimitiveType = _clouds.PrimitiveType;

			_clouds.Rotation = new Vector3(0, _cloudRotation, 0);
			world = _clouds.World;
			_wvp.UpdateWorldMatrix(ref world);

			_graphics.Output.BlendingState.States = GorgonBlendStates.AdditiveBlending;

			_graphics.Output.DrawIndexed(0, 0, _clouds.IndexCount);

			_graphics.Output.BlendingState.States = GorgonBlendStates.NoBlending;

			world = _sphere.World;
			_wvp.UpdateWorldMatrix(ref world);
			_graphics.Input.Layout = _normalVertexLayout;
			_graphics.Input.PrimitiveType = PrimitiveType.LineList;
			_graphics.Shaders.PixelShader.Current = _normalPixelShader;
			_graphics.Shaders.VertexShader.Current = _normalVertexShader;
			_graphics.Input.IndexBuffer = null;
			_graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(_sphere.Normals, Vector4.SizeInBytes);

			_graphics.Output.Draw(0, _sphere.VertexCount * 2);

			 _graphics.Input.Layout = _vertexLayout;
			_graphics.Shaders.VertexShader.Current = _vertexShader;

			var state = _renderer2D.Begin2D();
			_renderer2D.Drawing.DrawString(_font,
			                               string.Format(
			                                             "FPS: {0:0.0}, Delta: {1:0.000} ms Tris: {3:0} CamRot: {2} Mouse: {4:0}x{5:0} Sensitivity: {6:0.0##}",
			                                             GorgonTiming.FPS,
			                                             GorgonTiming.Delta * 1000,
			                                             _cameraRotation,
			                                             (_triangle.TriangleCount) + (_plane.TriangleCount) + (_cube.TriangleCount) + (_sphere.TriangleCount) + (_icoSphere.TriangleCount) + (_clouds.TriangleCount),
			                                             _mouse.Position.X,
			                                             _mouse.Position.Y,
														 _sensitivity),
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

			if (!_lock)
			{
				Matrix.RotationYawPitchRoll(_cameraRotation.X.Radians(),
				                            _cameraRotation.Y.Radians(),
				                            _cameraRotation.Z.Radians(),
				                            out rotMatrix);
				Vector3.TransformCoordinate(ref forward, ref rotMatrix, out lookAt);
				Vector3.TransformCoordinate(ref right, ref rotMatrix, out rightDir);
				Vector3.Cross(ref lookAt, ref rightDir, out upDir);
			}
			else
			{
				upDir = up;
				rightDir = right;
				lookAt = _sphere.Position;
			}

			Vector3.Multiply(ref rightDir, cameraDir.X, out rightDir);
			Vector3.Add(ref _cameraPosition, ref rightDir, out _cameraPosition);
			Vector3.Multiply(ref lookAt, cameraDir.Z, out forward);
			Vector3.Add(ref _cameraPosition, ref forward, out _cameraPosition);
			Vector3.Multiply(ref upDir, cameraDir.Y, out up);
			Vector3.Add(ref _cameraPosition, ref up, out _cameraPosition);

			//lookAt.Normalize();
			if (!_lock)
			{
				Vector3.Add(ref _cameraPosition, ref lookAt, out lookAt);
			}

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
			_bumpShader = _graphics.Shaders.CreateShader<GorgonPixelShader>("PixelShader", "PrimPSBump", Resources.Shaders);
			_waterShader = _graphics.Shaders.CreateShader<GorgonPixelShader>("PixelShader", "PrimPSWaterBump", Resources.Shaders);
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
			_earf = _graphics.Textures.CreateTexture<GorgonTexture2D>("Earf", Resources.earthmap1k);
			_normalMap = _graphics.Textures.FromMemory<GorgonTexture2D>("RainNRM", Resources.Rain_Height_NRM, new GorgonCodecDDS());
			_normalEarfMap = _graphics.Textures.FromMemory<GorgonTexture2D>("EarfNRM", Resources.earthbump1k_NRM, new GorgonCodecDDS());
			_specMap = _graphics.Textures.FromMemory<GorgonTexture2D>("RainSPC", Resources.Rain_Height_SPEC, new GorgonCodecDDS());
			_specEarfMap = _graphics.Textures.CreateTexture<GorgonTexture2D>("EarfSPC", Resources.earthspec1k);
			_cloudMap = _graphics.Textures.CreateTexture<GorgonTexture2D>("EarfClouds", Resources.earthcloudmap);
			_gorgNrm = _graphics.Textures.CreateTexture<GorgonTexture2D>("EarfClouds", Resources.normalmap);

			var depth = new GorgonDepthStencilStates
			            {
				            DepthComparison = ComparisonOperator.LessEqual,
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

			_cube = new Cube(_graphics, new Vector3(1, 1, 1), new RectangleF(0, 0, 1.0f, 1.0f), new Vector3(45.0f, 0, 0), 1, 1)
			        {
				        Position = new Vector3(0, 0, 1.5f),
				        Texture = _texture
			        };

			_sphere = new Sphere(_graphics, 1.0f, new RectangleF(0.0f, 0.0f, 1.0f, 1.0f), Vector3.Zero, 64, 64)
			          {
				          Position = new Vector3(-2.0f, 1.0f, 0.75f),
				          Texture = _earf
			          };

			_clouds = new Sphere(_graphics, 5.175f, new RectangleF(0.0f, 0.0f, 1.0f, 1.0f), Vector3.Zero, 16, 16)
			          {
				          Position = new Vector3(10, 2, 9.5f),
				          Texture = _cloudMap
			          };

			_icoSphere = new IcoSphere(_graphics, 5.0f, new RectangleF(0, 0, 1, 1), Vector3.Zero, 3)
			             {
							 Rotation = new Vector3(0, -45.0f, 0),
				             Position = new Vector3(10, 2, 9.5f),
				             Texture = _earf
			             };

			_graphics.Shaders.PixelShader.TextureSamplers[0] = new GorgonTextureSamplerStates
			                                                   {
																   TextureFilter = TextureFilter.Linear,
																   HorizontalAddressing = TextureAddressing.Wrap,
																   VerticalAddressing = TextureAddressing.Wrap,
																   DepthAddressing = TextureAddressing.Wrap,
																   ComparisonFunction = ComparisonOperator.Always
			                                                   };
			_graphics.Shaders.PixelShader.TextureSamplers[2] = new GorgonTextureSamplerStates
			                                                   {
				                                                   TextureFilter = TextureFilter.Linear,
				                                                   HorizontalAddressing = TextureAddressing.Wrap,
				                                                   VerticalAddressing = TextureAddressing.Wrap,
				                                                   DepthAddressing = TextureAddressing.Wrap,
				                                                   ComparisonFunction = ComparisonOperator.Always
			                                                   };

			_graphics.Shaders.PixelShader.TextureSamplers[1] = new GorgonTextureSamplerStates
			                                                   {
				                                                   TextureFilter = TextureFilter.Linear,
				                                                   HorizontalAddressing = TextureAddressing.Wrap,
				                                                   VerticalAddressing = TextureAddressing.Wrap,
				                                                   DepthAddressing = TextureAddressing.Wrap,
				                                                   ComparisonFunction = ComparisonOperator.Always
			                                                   };

			_material = new Material
			            {
				            UVOffset = Vector2.Zero,
				            SpecularPower = 1.0f
			            };

			_materialBuffer = _graphics.Buffers.CreateConstantBuffer("uvOffset", ref _material, BufferUsage.Default);

			_graphics.Shaders.PixelShader.ConstantBuffers[2] = _materialBuffer;

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
			_light.UpdateAttenuation(16.0f, 2);

			var eye = Vector3.Zero;
			var lookAt = Vector3.UnitZ;
			var up = Vector3.UnitY;
			_wvp.UpdateViewMatrix(ref eye, ref lookAt, ref up);

			_cameraRotation = Vector2.Zero;

			GorgonApplication.PlugIns.LoadPlugInAssembly(Application.StartupPath + @"\Gorgon.Input.Raw.dll");

			_input = GorgonInputFactory.CreateInputFactory("GorgonLibrary.Input.GorgonRawPlugIn");
			_keyboard = _input.CreateKeyboard(_form);
			_mouse = _input.CreatePointingDevice(_form);

			_keyboard.KeyDown += (sender, args) =>
			                     {
				                     if (args.Key == KeyboardKeys.L)
				                     {
					                     _lock = !_lock;
				                     }
			                     };

			_mouse.PointingDeviceDown += Mouse_Down;
			_mouse.PointingDeviceUp += Mouse_Up;
			_mouse.PointingDeviceWheelMove += (sender, args) =>
			                                  {
				                                  if (args.WheelDelta < 0)
				                                  {
					                                  _sensitivity -= 0.05f;

					                                  if (_sensitivity < 0.05f)
					                                  {
						                                  _sensitivity = 0.05f;
					                                  }
				                                  } else if (args.WheelDelta > 0)
				                                  {
					                                  _sensitivity += 0.05f;

					                                  if (_sensitivity > 2.0f)
					                                  {
						                                  _sensitivity = 2.0f;
					                                  }
				                                  }

			                                  };
			_mouse.PointingDeviceMove += (sender, args) =>
			                             {
				                             if (!_mouse.Exclusive)
				                             {
					                             return;
				                             }

				                             var delta = args.RelativePosition;
				                             _cameraRotation.Y += delta.Y * _sensitivity;//((360.0f * 0.002f) * delta.Y.Sign());
				                             _cameraRotation.X += delta.X * _sensitivity;//((360.0f * 0.002f) * delta.X.Sign());
				                             _mouseStart = _mouse.Position;
				                             _mouse.RelativePosition = PointF.Empty;
			                             };

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

				GorgonApplication.Run(_form, Idle);
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, _ => GorgonDialogs.ErrorBox(null, _), true);
			}
			finally
			{
				if (_materialBuffer != null)
				{
					_materialBuffer.Dispose();
				}

				if (_normalEarfMap != null)
				{
					_normalEarfMap.Dispose();
				}

				if (_normalMap != null)
				{
					_normalMap.Dispose();
				}

				if (_specEarfMap != null)
				{
					_specEarfMap.Dispose();
				}

				if (_specMap != null)
				{
					_specMap.Dispose();
				}

				if (_cloudMap != null)
				{
					_cloudMap.Dispose();
				}

				if (_clouds != null)
				{
					_clouds.Dispose();
				}

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
