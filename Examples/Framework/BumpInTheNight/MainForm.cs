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
		private RenderImage _lightBuffer = null;								// Buffer used to render the lit image.
		private RenderImage _colorBuffer = null;								// Color buffer - used to draw diffuse sprites.
		private RenderImage _normalBuffer = null;								// Normal map buffer - used to draw normal maps.
		private Sprite _bufferSprite = null;									// Buffer sprite.
		private Sprite _torch = null;											// The torch sprite.
		private Sprite _lightSprite = null;										// Lighting sprite.
		private Sprite _bumpSprite = null;										// Bump map sprite.
		private Image _torchImage = null;										// Image used for torch.
		private Image _bumpNormal = null;										// Normal map.
		private Image _bumpColor = null;										// Color map.
		private Image _lightSource = null;										// "Light source" - Gradient circle.
		private FXShader _bumpShader = null;									// Bump mapping shader.
		private float _angle = 0.0f;											// Bump sprite rotation angle.
		private Vector3D _lightPosition = Vector2D.Zero;						// Light position.
		private Vector2D _lightSpriteScale = Vector2D.Zero;						// Scaling for lighting sprite.
		#endregion

		#region Methods.
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
			base.OnLogicUpdate(e);

			_angle += 12.0f * e.FrameDeltaTime;

			_bumpSprite.SetPosition(_colorBuffer.Width / 2.0f, _colorBuffer.Height / 2.0f);
			_bumpShader.Parameters["Position"].SetValue(_lightPosition);

			_torch.Position = _lightPosition;
			_torch.Animations[0].Advance(e.FrameDeltaTime * 1000.0f);
			_lightSprite.Position = _lightPosition;

			_bufferSprite.ImageOffset = Vector2D.Subtract(_lightPosition, _lightSprite.Axis);
			
			_bumpSprite.Rotation = _angle;
		}

		/// <summary>
		/// Function to perform rendering updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnFrameUpdate(FrameEventArgs e)
		{
			base.OnFrameUpdate(e);

			// Draw the sprites to the normal and diffuse buffers.
			Gorgon.CurrentRenderTarget = _normalBuffer;
			_normalBuffer.Clear(Drawing.Color.FromArgb(255, 127, 127, 127));
			_bumpSprite.Image = _bumpNormal;
			_bumpSprite.Draw();
			Gorgon.CurrentRenderTarget = _colorBuffer;
			_colorBuffer.Clear(Drawing.Color.White);
			_bumpSprite.Image = _bumpColor;
			_bumpSprite.Draw();

			// Draw to our lit buffer.
			Gorgon.CurrentRenderTarget = _lightBuffer;
			_lightBuffer.Clear(Drawing.Color.Black);

			// Draw bump mapped result.
			Gorgon.CurrentShader = _bumpShader.Techniques["BumpSmooth"];			
			_bufferSprite.Image = _colorBuffer.Image;
			_bufferSprite.ImageOffset = new Vector2D(_lightPosition.X, _lightPosition.Y) - _lightSprite.Axis;
			_bufferSprite.Draw();
			Gorgon.CurrentShader = null;
			_lightBuffer.BlendingMode = BlendingModes.Modulated;
			// Draw light map to obscure the lit image.
			if (_rnd.Next(1000) > 512)
			{
				_bumpShader.Parameters["Intensity"].SetValue((float)_rnd.NextDouble() * 0.3f + 0.7f);
				_bumpShader.Parameters["Color"].SetValue(Drawing.Color.FromArgb(255, _rnd.Next(180, 255), _rnd.Next(128, 192)));
				_lightSource.Blit(-_lightSource.Width * 0.05f, -_lightSource.Height * 0.05f, _lightSpriteScale.X + (_lightSpriteScale.X * 0.10f), _lightSpriteScale.Y + (_lightSpriteScale.Y * 0.10f));
			}
			else
			{
				_bumpShader.Parameters["Intensity"].SetValue(1.2f);
				_bumpShader.Parameters["Color"].SetValue(Drawing.Color.White);
				_lightSource.Blit(0, 0, _lightSpriteScale.X, _lightSpriteScale.Y);
			}
			_lightBuffer.BlendingMode = BlendingModes.None;
			Gorgon.CurrentRenderTarget = null;			
			
			// Finally blit the lit image to the screen.
			_lightSprite.Draw();
			_torch.Draw();
		}

		/// <summary>
		/// Function called when the mouse is moved.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseMovement(MouseInputEventArgs e)
		{
			base.OnMouseMovement(e);
			_lightPosition = new Vector3D(e.Position.X, e.Position.Y, 25.0f);
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
			_lightSpriteScale = new Vector2D(Gorgon.Screen.Width / 2.5f, Gorgon.Screen.Width / 2.5f);
			_lightBuffer.SetDimensions((int)_lightSpriteScale.X, (int)_lightSpriteScale.Y);
			_colorBuffer.SetDimensions(Gorgon.Screen.Width, Gorgon.Screen.Height);
			_normalBuffer.SetDimensions(_colorBuffer.Width, _colorBuffer.Height);

			_bufferSprite.SetSize(_colorBuffer.Width, _colorBuffer.Height);
			_bumpShader.Parameters["TextureSize"].SetValue(new Vector2D(Gorgon.Screen.Width, Gorgon.Screen.Height));
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		protected override void Initialize()
		{
			FileSystems[ApplicationName].Mount();

			_lightPosition = new Vector3D(Input.Mouse.Position.X, Input.Mouse.Position.Y, 25.0f);
			_torchImage = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/Torch.png");
			_lightSource = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/Lightmap.png");
			_torch = Sprite.FromFileSystem(FileSystems[ApplicationName], "/Sprites/Torch.gorSprite");

			_lightSpriteScale = new Vector2D(Gorgon.Screen.Width / 2.5f, Gorgon.Screen.Width / 2.5f);
			
			_lightBuffer = new RenderImage("Buffer", (int)_lightSpriteScale.X, (int)_lightSpriteScale.Y, ImageBufferFormats.BufferRGB888X8);
			_colorBuffer = new RenderImage("ColorBuffer", Gorgon.Screen.Width, Gorgon.Screen.Height, ImageBufferFormats.BufferRGB888X8);
			_normalBuffer = new RenderImage("NormalBuffer", _colorBuffer.Width, _colorBuffer.Height, ImageBufferFormats.BufferRGB888X8);			
			
			_bufferSprite = new Sprite("BackBuffer", _colorBuffer);
			_bufferSprite.WrapMode = ImageAddressing.Clamp;

			_lightSprite = new Sprite("LightSprite", _lightBuffer);
			_lightSprite.SetAxis(_lightBuffer.Width / 2.0f, _lightBuffer.Height / 2.0f);

			_bumpNormal = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/normalmap.png");
			_bumpColor = Image.FromFileSystem(FileSystems[ApplicationName], "/Images/colormap.png");
			_bumpSprite = new Sprite("BumpSprite", _bumpColor);
			_bumpSprite.UniformScale = Gorgon.Screen.Height / 480.0f;

#if DEBUG
			_bumpShader = FXShader.FromFileSystem(FileSystems[ApplicationName], "/Shaders/bumpmap.fx", ShaderCompileOptions.Debug);
#else
			_bumpShader = FXShader.FromFileSystem(FileSystems[ApplicationName], "/Shaders/bumpmap.fx", ShaderCompileOptions.OptimizationLevel3);
#endif

			_bumpShader.Parameters["ColorMap"].SetValue(_colorBuffer);
			_bumpShader.Parameters["NormalMap"].SetValue(_normalBuffer);
			_bumpShader.Parameters["TextureSize"].SetValue(new Vector2D(Gorgon.Screen.Width, Gorgon.Screen.Height));

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