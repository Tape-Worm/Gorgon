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
using Dialogs;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Main application form.
	/// </summary>
	public partial class MainForm 
		: Form
	{
		#region Variables.
		private FXShader _cloakShader = null;				// Cloaking shader.
		private FXShader _scratchShader = null;				// Film scratch shader.
		private ImageShader _imageShader = null;			// Image shader.
		private Image _noiseImage = null;					// Noise image.
		private Image _backgroundImage = null;				// Background image.
		private RenderImage _backTarget = null;				// Background target.
		private RenderImage _grainTarget = null;			// Film grain target.
		private RenderImage _finalBuffer = null;			// Final post composite buffer.
		private Sprite _grainSprite = null;					// Grain sprite.
		private Sprite _bufferSprite = null;				// Buffer sprite.
		private Image _shipImage = null;					// Ship image.
		private Sprite _ship = null;						// Ship sprite.
		private Sprite _background = null;					// background sprite.
		private Vector4D _mouse = Vector4D.Zero;			// Mouse position.
		private bool _cloaking = false;						// Flag to indicate that we're cloaking.
		private bool _cloakDir = false;						// Cloaking direction.
		private bool _cloakFade = false;					// Cloaking fade.
		private float _cloakAmount = 1.0f;					// Cloak amount.
		private float _cloakIndex = 6.5f;					// Refraction amount.
		private float _degrees = 0.0f;						// Rotation in degrees.
		private PreciseTimer _frameTimer;					// Frame timer.		
		private Random _rnd = new Random();					// Random number generator.
		private Vector2D _offset = Vector2D.Zero;			// Offset.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set up the cloak on the sprite.
		/// </summary>
		private void InstallCloak()
		{
			_cloakShader.Parameters["spriteDimensions"].SetValue(new Vector4D(_ship.Width, _ship.Height, 0, 0));
			_cloakShader.Parameters["backbufferSize"].SetValue(new Vector4D(_backTarget.Width, _backTarget.Height, 0, 0));
			_cloakShader.Parameters["background"].SetValue(_backTarget.Image);
			_cloakShader.Parameters["sprite"].SetValue(_shipImage);
			_cloakShader.Parameters["cloakAmount"].SetValue(1.0f);
			_cloakShader.Parameters["refractionIndex"].SetValue(_cloakIndex);
		}

		/// <summary>
		/// Function to draw the film grain effect.
		/// </summary>
		private void DrawGrain()
		{
			int c = 0;						// Color.
			Vector2D pos = Vector2D.Zero;	// Position.

			Gorgon.CurrentRenderTarget = _grainTarget;
			_grainTarget.Clear(Drawing.Color.FromArgb((byte)_rnd.Next(32), 255, 255, 255));
			
			_grainTarget.BeginDrawing();
			for (int i = 0; i < 25; i++)
			{
				c = (byte)_rnd.Next(255);

				_grainTarget.SetPoint(_rnd.Next(_grainTarget.Width), _rnd.Next(_grainTarget.Height), Drawing.Color.FromArgb(255, c, c, c));

				// Create random "hair" lines.
				if (_rnd.Next(100) > 97)
				{
					pos.X = _rnd.Next((int)_grainSprite.Width);
					pos.Y = _rnd.Next((int)_grainSprite.Height);
					if (c < 128)
						c = 0;
					for (int j = 0; j < _rnd.Next(_grainTarget.Width / 4); j++)
					{						
						if (c > 0)
							_grainTarget.SetPoint(pos.X + j, pos.Y, Drawing.Color.FromArgb(255, c, c, c));
						else
							_grainTarget.SetPoint(pos.X + j, pos.Y, Drawing.Color.FromArgb(255, 0, 0, 0));
						pos.Y += _rnd.Next(2) - 1;
					}
				}
			}
			_grainTarget.EndDrawing();
		}

		/// <summary>
		/// Function to draw the cloaked ship.
		/// </summary>
		/// <param name="frameTime">Frame delta time.</param>
		private void DrawCloakedShip(float frameTime)
		{
			Vector2D scale = Vector2D.Zero;

			_ship.SetPosition(_mouse.X, _mouse.Y);
			_ship.Rotation = _degrees;

			_ship.UniformScale = ((_ship.Position.Y * (1.0f - 0.25f)) / (Gorgon.Screen.Height * (1.0f - 0.25f))) + 0.25f;
			if (_ship.UniformScale < 0.5f)
				_ship.UniformScale = 0.5f;

			// Draw the reverse transformed background.
			Gorgon.CurrentRenderTarget = _backTarget;

			scale.X = (float)Gorgon.Screen.Width / _background.Image.Width;
			scale.Y = (float)Gorgon.Screen.Height / _background.Image.Height;

            _background.Axis = Vector2D.Divide(_ship.Position, scale);
			_background.Position = _ship.Axis;
			_background.Rotation = -_degrees;
            _background.Scale = Vector2D.Divide(scale, _ship.Scale);
			_background.Draw();
			Gorgon.CurrentRenderTarget = _finalBuffer;

			// Do cloak.
			if (_cloaking)
			{
				if (!_cloakDir)
				{
					if (_cloakFade)
						_cloakAmount -= 0.55f * frameTime;
					_cloakIndex -= 2.0f * frameTime;
				}
				else
				{
					_cloakAmount += 0.55f * frameTime;
					if (_cloakFade)
						_cloakIndex += 2.0f * frameTime;
				}

				if (_cloakAmount <= 0.0f)
				{
					_cloakAmount = 0.0f;
					_cloaking = false;
					_cloakDir = !_cloakDir;
				}

				if (_cloakAmount > 1.0f)
				{
					_cloakAmount = 1.0f;
					_cloakFade = true;
				}

				if (_cloakIndex < 1.0f)
				{
					_cloakIndex = 1.0f;
					_cloakFade = true;
				}

				if (_cloakIndex > 6.5f)
				{
					_cloakIndex = 6.5f;
					_cloaking = false;
					_cloakDir = !_cloakDir;
				}

				_cloakShader.Parameters["cloakAmount"].SetValue(_cloakAmount);
				_cloakShader.Parameters["refractionIndex"].SetValue(_cloakIndex);
			}

			Gorgon.CurrentShader = _cloakShader;
			_ship.Draw();
		}

		/// <summary>
		/// Handles the Idle event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Graphics.FrameEventArgs"/> instance containing the event data.</param>
		private void Gorgon_Idle(object sender, FrameEventArgs e)
		{
			Gorgon.Screen.Clear(Drawing.Color.Black);
			// Do nothing here.  When we need to update, we will.
			_finalBuffer.Clear(Drawing.Color.Black);
			Gorgon.CurrentRenderTarget = _finalBuffer;			

			_backgroundImage.Blit(0, 0, Gorgon.Screen.Width, Gorgon.Screen.Height);
			DrawCloakedShip(e.FrameDeltaTime);			

			Gorgon.CurrentShader = null;

			_scratchShader.Parameters["Timer"].SetValue((float)DateTime.Now.Millisecond * e.FrameDeltaTime);

			if (_frameTimer.Milliseconds > _rnd.Next(90) + 90)
			{
				_offset.X = (float)_rnd.Next(6) - 3;
				_offset.Y = (float)_rnd.Next(6) - 3;
			}

			if (_frameTimer.Milliseconds > 250)
			{
				DrawGrain();
				_frameTimer.Reset();
			}
			Gorgon.CurrentRenderTarget = null;

			_bufferSprite.Position = _offset;
			Gorgon.CurrentShader = _scratchShader;
			_bufferSprite.Draw();

			Gorgon.CurrentShader = null;
			_grainSprite.Opacity = (byte)(_rnd.Next(128) + 127);
			_grainSprite.Draw();
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		private void Initialize()
		{
			// Set smoothing mode to all the sprites.
			Gorgon.GlobalStateSettings.GlobalSmoothing = Smoothing.Smooth;

			// Load the images.
			_shipImage = Image.FromFile(@"..\..\..\..\Resources\Images\Ship.png");
			_backgroundImage = Image.FromFile(@"..\..\..\..\Resources\Images\Nebula.png");
			
			// Create the sprites.
			_ship = new Sprite("Ship", _shipImage);
			_ship.SetAxis(_ship.Width / 2.0f, _ship.Height / 2.0f);			

			_background = new Sprite("back", _backgroundImage);
			_background.BlendingMode = BlendingModes.None;
			_background.AlphaMaskFunction = CompareFunctions.Always;

			// Create the back buffer.			
			_backTarget = new RenderImage("BackTarget", Gorgon.Screen.Width, Gorgon.Screen.Height, ImageBufferFormats.BufferRGB888X8);
			_finalBuffer = new RenderImage("FinalBuffer", Gorgon.Screen.Width, Gorgon.Screen.Height, ImageBufferFormats.BufferRGB888X8);
			_grainTarget = new RenderImage("Grain", 512, 512, ImageBufferFormats.BufferRGB888A8);
			_grainTarget.Clear(Drawing.Color.Black);

			_grainSprite = new Sprite("GrainSprite", _grainTarget);
			_grainSprite.BlendingMode = BlendingModes.Additive;
			_grainSprite.Smoothing = Smoothing.Smooth;
			_grainSprite.SetScale(Gorgon.Screen.Width / _grainSprite.Width, Gorgon.Screen.Height / _grainSprite.Height);
			_bufferSprite = new Sprite("FinalBufferSprite", _finalBuffer);

			// Get shader.
#if DEBUG
			_cloakShader = FXShader.FromFile(@"..\..\..\..\Resources\Shaders\Cloak.fx", ShaderCompileOptions.Debug);
			_scratchShader = FXShader.FromFile(@"..\..\..\..\Resources\Shaders\post_scratched_film.fx", ShaderCompileOptions.Debug);
			_imageShader = new ImageShader("ScratchBuffer", _scratchShader.GetShaderFunction("noise_2d", "tx_1_0", ShaderCompileOptions.Debug));
#else
			_cloakShader = FXShader.FromFile(@"..\..\..\..\Resources\Shaders\Cloak.fx", ShaderCompileOptions.OptimizationLevel3);
			_scratchShader = FXShader.FromFile(@"..\..\..\..\Resources\Shaders\post_scratched_film.fx", ShaderCompileOptions.OptimizationLevel3);
			_imageShader = new ImageShader("ScratchBuffer", _scratchShader.GetShaderFunction("noise_2d", "tx_1_0", ShaderCompileOptions.OptimizationLevel3));
#endif
			InstallCloak();

			// Set up noise image.			
			_noiseImage = new Image("NoiseImage", 128, 128, ImageBufferFormats.BufferRGB888A8, true);
			_noiseImage.FillFromShader(_imageShader);
			_scratchShader.Parameters["Noise2DTex"].SetValue(_noiseImage);
			_scratchShader.Parameters["SceneTexture"].SetValue(_finalBuffer);

			_frameTimer = new PreciseTimer();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseWheel"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			if (e.Delta < 0)
				_degrees -= 4.0f;
			if (e.Delta > 0)
				_degrees += 4.0f;

			if (_degrees > 360.0f)
				_degrees = 0.0f;
			if (_degrees < 0.0f)
				_degrees = 360.0f;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs"></see> that contains the event data.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			_mouse.X = e.X;
			_mouse.Y = e.Y;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.KeyDown"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs"></see> that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.KeyCode == Keys.Escape)
				Close();

			if (e.KeyCode == Keys.S)
				Gorgon.FrameStatsVisible = !Gorgon.FrameStatsVisible;

			if (e.KeyCode == Keys.C)
			{
				_cloakFade = false;
				_cloaking = true;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			// Perform clean up.
			Gorgon.Terminate();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				// Initialize Gorgon
				// Set it up so that we won't be rendering in the background, but allow the screensaver to activate.
				Gorgon.Initialize(false, true);

				// Display the logo.
				Gorgon.LogoVisible = true;
				Gorgon.FrameStatsVisible = false;
				
				if (Gorgon.CurrentDriver.PixelShaderVersion < new Version(2, 0))
				{
					UI.ErrorBox(this, "This example requires a shader model 2 capable video card.\nSee details for more information.", "Required pixel shader version: 2.0\n" + Gorgon.CurrentDriver.Description + " pixel shader version: " + Gorgon.CurrentDriver.PixelShaderVersion.ToString());
					Application.Exit();
					return;
				}

				// Set the video mode.
				ClientSize = new Drawing.Size(640, 480);
				Gorgon.SetMode(this);

				// Set an ugly background color.
				Gorgon.Screen.BackgroundColor = Drawing.Color.FromArgb(0, 0, 16);

				// Initialize.
				Initialize();

				// Assign idle event.
				Gorgon.Idle += new FrameEventHandler(Gorgon_Idle);
				Gorgon.DeviceReset += new EventHandler(Gorgon_DeviceReset);

				// Begin rendering.
				Gorgon.Go();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to initialize the application.", ex);
			}
		}

		/// <summary>
		/// Handles the DeviceReset event of the Gorgon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Gorgon_DeviceReset(object sender, EventArgs e)
		{
			_noiseImage.FillFromShader(_imageShader);
			_backTarget.SetDimensions(Gorgon.Screen.Width, Gorgon.Screen.Height);
			_finalBuffer.SetDimensions(Gorgon.Screen.Width, Gorgon.Screen.Height);
			_grainSprite.SetScale(Gorgon.Screen.Width / _grainSprite.Width, Gorgon.Screen.Height / _grainSprite.Height);
			_bufferSprite.Width = Gorgon.Screen.Width;
			_bufferSprite.Height = Gorgon.Screen.Height;
			_background.Width = Gorgon.Screen.Width;
			_background.Height = Gorgon.Screen.Height;

			_scratchShader.Parameters["Noise2DTex"].SetValue(_noiseImage);
			_scratchShader.Parameters["SceneTexture"].SetValue(_finalBuffer);
			_frameTimer.Reset();
			InstallCloak();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
		}
		#endregion
	}
}