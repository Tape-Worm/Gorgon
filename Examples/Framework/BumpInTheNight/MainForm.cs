#region MIT.
// 
// Examples.
// Copyright (C) 2008 Michael Winsor
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
// Created: Thursday, October 02, 2008 10:46:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.Framework;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;
using Dialogs;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	public partial class MainForm 
		: GorgonApplicationWindow
	{
		#region Variables.
		private Random _rnd = new Random();										// Random number generator.
		private RenderImage _buffer = null;										// Buffer used to render the lit image.
		private RenderImage _lightSource = null;								// Our light source.
		private Sprite _torch = null;											// The torch sprite.
		private Sprite _bufferSprite = null;									// Buffer sprite.
		private Sprite _bumpSprite = null;										// Bump map sprite.
		private Image _torchImage = null;										// Image used for torch.
		private Image _bumpNormal = null;										// Normal map.
		private Image _bumpColor = null;										// Color map.
		private FXShader _bumpShader = null;									// Bump mapping shader.
		private float _angle = 0.0f;											// Bump sprite rotation angle.
		private Vector3D _lightPosition = Vector2D.Zero;						// Light position.
		private Vector2D _lightSpriteScale = new Vector2D(2.5f, 2.5f);			// Scaling for lighting sprite.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw the light source shading for the light.
		/// </summary>
		private void DrawLightSource()
		{
			float alpha = 0;

			_lightSource.BlendingMode = BlendingModes.None;
			_lightSource.Clear(Drawing.Color.Black);			
			_lightSource.BeginDrawing();

			for (int i = 0; i < _lightSource.Width; i++)
			{
				alpha = 255.0f - (((float)i / (float)_lightSource.Width) * 255.0f);
				_lightSource.FilledCircle(_lightSource.Width / 2.0f, _lightSource.Height / 2.0f, (_lightSource.Width - i) / 2.125f, Drawing.Color.FromArgb((int)alpha, Drawing.Color.Black));
			}
			_lightSource.EndDrawing();

			_lightSource.BlendingMode = BlendingModes.Modulated;
		}

		/// <summary>
		/// Function to validate the chosen video driver.
		/// </summary>
		/// <param name="driver">Driver to validate.</param>
		/// <returns>
		/// TRUE if the driver is valid, FALSE if not.
		/// </returns>
		protected override bool OnValidateDriver(Driver driver)
		{
			if ((driver.PixelShaderVersion < new Version(2, 0)) || (driver.VertexShaderVersion < new Version(2, 0)))
			{
				UI.ErrorBox(this, "This example requires a shader model 2 card.\nSelected driver pixel shader version (min. 2.0): " + 
					Gorgon.CurrentDriver.PixelShaderVersion.ToString() + "\n" + 
					"Selected driver vertex shader version (min. 2.0): " + Gorgon.CurrentDriver.VertexShaderVersion.ToString());				
				return false;
			}

			return true;
		}

		/// <summary>
		/// Function to perform logic updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnLogicUpdate(FrameEventArgs e)
		{
			Matrix rotate = Matrix.Identity;			// Rotation matrix.
			Matrix scale = Matrix.Identity;			// Scaling matrix.
			Matrix world = Matrix.Identity;			// World matrix.

			base.OnLogicUpdate(e);

			_angle += 12.0f * e.FrameDeltaTime;

			_bumpSprite.SetPosition(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height / 2.0f);

			scale.Scale(_bumpSprite.Scale);
			rotate.RotateZ(-_bumpSprite.Rotation);
			world = Matrix.Multiply(rotate, scale);
			world.Translate(Vector2D.Add(Vector2D.Negate(_bumpSprite.Position), _bumpSprite.Axis));

			_bumpShader.Parameters["worldMatrix"].SetValue(world);
			_bumpShader.Parameters["Position"].SetValue(_lightPosition);

			_bufferSprite.Position = _torch.Position = _lightPosition;
			_torch.Animations[0].Advance(e.FrameDeltaTime * 1000.0f);

			_bumpSprite.Rotation = _angle;
			_bumpSprite.Position = Vector2D.Subtract(_bumpSprite.Position, Vector2D.Subtract(_lightPosition, _bufferSprite.Axis));
		}

		/// <summary>
		/// Function to perform rendering updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnFrameUpdate(FrameEventArgs e)
		{
			base.OnFrameUpdate(e);
			_buffer.Clear(Drawing.Color.White);
			Gorgon.CurrentRenderTarget = _buffer;
			Gorgon.CurrentShader = _bumpShader.Techniques["BumpSmooth"];
			_bumpSprite.Draw();
			Gorgon.CurrentShader = null;
			if (_rnd.Next(1000) > 512)
				_lightSource.Blit(-_lightSource.Width * 0.05f, -_lightSource.Height * 0.05f, _lightSource.Width + (_lightSource.Width * 0.10f), _lightSource.Height + (_lightSource.Height * 0.10f));
			else
				_lightSource.Blit(0, 0);

			Gorgon.CurrentRenderTarget = null;

			_bufferSprite.Draw();
			_torch.Draw();
		}

		/// <summary>
		/// Function called when the mouse is moved.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseMovement(MouseInputEventArgs e)
		{
			base.OnMouseMovement(e);
			_lightPosition = new Vector3D(e.Position.X, e.Position.Y, -25.0f);
		}

		/// <summary>
		/// Function called when the video device is set to a lost state.
		/// </summary>
		protected override void OnDeviceLost()
		{
			base.OnDeviceLost();
		}

		/// <summary>
		/// Function called when the video device has recovered from a lost state.
		/// </summary>
		protected override void OnDeviceReset()
		{
			base.OnDeviceReset();
			_lightSource.SetDimensions((int)(Gorgon.Screen.Width / _lightSpriteScale.X), (int)(Gorgon.Screen.Width / _lightSpriteScale.Y));
			_buffer.SetDimensions(_lightSource.Width, _lightSource.Height);
			_bufferSprite.SetAxis(_buffer.Width / 2, _buffer.Height / 2);
			_bufferSprite.SetSize(_buffer.Width, _buffer.Height);

			DrawLightSource();
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		protected override void Initialize()
		{
			FileSystems[ApplicationName].Mount();

			_lightPosition = new Vector3D(Input.Mouse.Position.X, Input.Mouse.Position.Y, -25.0f);
			_torchImage = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/Torch.png");
			_torch = Sprite.FromFileSystem(FileSystems[ApplicationName], "/Sprites/Torch.gorSprite");

			_lightSource = new RenderImage("LightSource", (int)(Gorgon.Screen.Width / _lightSpriteScale.X), (int)(Gorgon.Screen.Width / _lightSpriteScale.Y), ImageBufferFormats.BufferRGB888A8);
			_buffer = new RenderImage("Buffer", _lightSource.Width, _lightSource.Height, ImageBufferFormats.BufferRGB888X8);
			DrawLightSource();

			_bufferSprite = new Sprite("BackBuffer", _buffer);
			_bufferSprite.SetAxis(_buffer.Width / 2, _buffer.Height / 2);

			_bumpNormal = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/normalmap.png");
			_bumpColor = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/colormap.png");
			_bumpSprite = new Sprite("BumpSprite", _bumpColor);

#if DEBUG
			_bumpShader = FXShader.FromFileSystem(FileSystems[ApplicationName], "/Shaders/bumpmap.fx", ShaderCompileOptions.Debug);
#else
			_bumpShader = FXShader.FromFileSystem(FileSystems[ApplicationName], "/Shaders/bumpmap.fx", ShaderCompileOptions.OptimizationLevel3);
#endif

			_bumpShader.Parameters["ColorMap"].SetValue(_bumpColor);
			_bumpShader.Parameters["NormalMap"].SetValue(_bumpNormal);

			_bumpSprite.SetAxis(_bumpSprite.Width / 2.0f, _bumpSprite.Height / 2.0f);
			_torch.Animations[0].AnimationState = AnimationState.Playing;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
			: base(@".\BumpInTheNight.xml")
		{
			InitializeComponent();
		}
		#endregion
	}
}