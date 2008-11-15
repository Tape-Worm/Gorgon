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
using GorgonLibrary.Framework;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Utilities;
using GorgonLibrary.InputDevices;

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
		private List<ParticleEmitter> _emitters = null;							// List of particle emitters.
		private RenderImage _particleImage = null;								// Particle image.
		private RenderImage _screen = null;										// Primary target.
		private Sprite _particleSprite = null;									// Particle sprite.
		private bool _clearScreen = false;										// Flag to indicate that we should clear each frame.
		private PreciseTimer _timer = new PreciseTimer();						// Timer object.
		private PreciseTimer _splatTimer = new PreciseTimer();					// Particle splat timer.
		private TextSprite _text = null;										// Help text.
		private bool _inverted = false;											// Flag to invert the screen.
		private bool _showHelp = true;											// Flag to show help text.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform logic updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnLogicUpdate(FrameEventArgs e)
		{
			base.OnLogicUpdate(e);

			int i = 0;

			if (_inverted)
			{
				_text.Color = Drawing.Color.White;
				_particleSprite.BlendingMode = BlendingModes.Additive;
				_screen.BackgroundColor = Drawing.Color.Black;
			}
			else
			{
				_text.Color = Drawing.Color.Black;
				_particleSprite.BlendingMode = BlendingModes.Modulated;
				_screen.BackgroundColor = Drawing.Color.White;
			}

			while (i < _emitters.Count)
			{
				if (_emitters[i].Age < _emitters[i].ParticleLifetimeRange.End)
				{
					_emitters[i].Update(e.FrameDeltaTime);
					i++;
				}
				else
					_emitters.Remove(_emitters[i]);
			}
		}

		/// <summary>
		/// Function to perform rendering updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnFrameUpdate(FrameEventArgs e)
		{
			base.OnFrameUpdate(e);

			int i = 0;

			Gorgon.CurrentRenderTarget = _screen;

			if (_clearScreen)
				_screen.Clear();
			else
			{
				if (_timer.Milliseconds > 12.5)
				{
					if (!_inverted)
					{
						_screen.BlendingMode = BlendingModes.Additive;
						_screen.FilledRectangle(0, 0, _screen.Width, _screen.Height, Drawing.Color.FromArgb(1, _rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255)));
						_screen.BlendingMode = BlendingModes.None;
					}
					else
					{
						_screen.BlendingMode = BlendingModes.Modulated;
						_screen.FilledRectangle(0, 0, _screen.Width, _screen.Height, Drawing.Color.FromArgb(1, _rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255)));
						_screen.BlendingMode = BlendingModes.None;
					}
					_timer.Reset();
				}
			}

			for (i = 0; i < _emitters.Count; i++)
				_emitters[i].Draw();

			Gorgon.CurrentRenderTarget = null;			
			_screen.Blit();
			if (_showHelp)
			{
				_text.Text = "H - Show/hide this help text.\nI - Invert and use additive blending.\nLeft mouse button - Splat some particles.\nRight mouse button - Splat multicolored particles.\nMiddle mouse button - Toggle clear each frame.\nCTRL + F - Show frame statistics.\nESC - Exit.\n\nParticle emitter count: " + _emitters.Count.ToString();
				_text.Draw();
			}
		}

		/// <summary>
		/// Function called when the video device is set to a lost state.
		/// </summary>
		protected override void OnDeviceLost()
		{
			base.OnDeviceLost();
		}

		/// <summary>
		/// Function called when a keyboard key is pushed down.
		/// </summary>
		/// <param name="e">Keyboard event parameters.</param>
		protected override void OnKeyboardKeyDown(KeyboardInputEventArgs e)
		{
			base.OnKeyboardKeyDown(e);

			if (e.Key == KeyboardKeys.I)
				_inverted = !_inverted;
			if (e.Key == KeyboardKeys.H)
				_showHelp = !_showHelp;
		}

		/// <summary>
		/// Function called when the video device has recovered from a lost state.
		/// </summary>
		protected override void OnDeviceReset()
		{
			base.OnDeviceReset();
			_screen.Width = Gorgon.Screen.Width;
			_screen.Height = Gorgon.Screen.Height;
			_screen.Clear();
		}

		/// <summary>
		/// Function to create a particle.
		/// </summary>
		private void CreateParticle()
		{
			Drawing.Color particleColor;

			_particleImage.Clear(Drawing.Color.Transparent);
			_particleImage.BeginDrawing();
			for (int x = 64; x > 0; x--)
			{
				particleColor = Drawing.Color.FromArgb(255 - ((x * 4) - 1), 255, 255, 255);
				_particleImage.FilledCircle(32.0f, 32.0f, x / 2, particleColor);
			}
			_particleImage.EndDrawing();
		}

		/// <summary>
		/// Function to create a particle emitter.
		/// </summary>
		/// <param name="position">The position of the emitter.</param>
		/// <returns>A new emitter.</returns>
		private ParticleEmitter CreateEmitter(Vector2D position)
		{
			ParticleEmitter emitter = null;

			emitter = new ParticleEmitter("Emitter" + _emitters.Count.ToString(), _particleSprite, position);
			emitter.ColorRange = new Range<Drawing.Color>(Drawing.Color.Black, Drawing.Color.FromArgb(0, Drawing.Color.Black));
			emitter.ContinuousParticles = false;
			emitter.EmitterLifetime = 1.5f;
			emitter.ParticleLifetimeRange = new Range<float>(0.0f, (float)(_rnd.NextDouble() * 10.0) + 5.0f);
			emitter.ParticleSizeRange = new Range<float>((float)(_rnd.NextDouble() * 1.5) + 0.25f, 0.0f);
			emitter.ParticleSpeedRange = new Range<float>(0.0f, (float)(_rnd.NextDouble() * 10.0) + 4.0f);
			emitter.TangentialAccelerationRange = new Range<float>(0.0f, 0.0f);
			emitter.RadialAccelerationRange = new Range<float>(0.0f, (float)(_rnd.NextDouble() * 10.0) + 4.0f);
			emitter.ParticleRotationRange = new Range<float>(-10.0f, 10.0f);
			emitter.Spread = (float)(_rnd.NextDouble() * 360.0);
			emitter.EmitterScale = 1.0f;
			_splatTimer.Reset();

			_emitters.Add(emitter);
			return emitter;
		}

		/// <summary>
		/// Function called when the mouse is moved.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseMovement(MouseInputEventArgs e)
		{
			base.OnMouseMovement(e);

			ParticleEmitter emitter = null;
			Drawing.Color startColor;
			Drawing.Color endColor;
			int alpha = 255;

			if (_clearScreen)
				alpha = _rnd.Next(64, 192);

			if (_splatTimer.Milliseconds >= 10)
			{
				switch (e.Buttons)
				{
					case GorgonLibrary.InputDevices.MouseButtons.Button1:
						startColor = Drawing.Color.FromArgb(alpha, _rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255));
						endColor = Drawing.Color.FromArgb(0, startColor);

						emitter = CreateEmitter(e.Position);
						emitter.ColorRange = new Range<System.Drawing.Color>(startColor, endColor);
						break;

					case GorgonLibrary.InputDevices.MouseButtons.Button2:
						startColor = Drawing.Color.FromArgb(alpha, Drawing.Color.Black);
						endColor = Drawing.Color.FromArgb(0, Drawing.Color.White);

						emitter = CreateEmitter(e.Position);
						emitter.ColorRange = new Range<System.Drawing.Color>(startColor, endColor);
						break;
				}
				_splatTimer.Reset();
			}
		}

		/// <summary>
		/// Function called when a mouse button is pushed down.
		/// </summary>
		/// <param name="e">Mouse event parameters.</param>
		protected override void OnMouseButtonDown(MouseInputEventArgs e)
		{
			base.OnMouseButtonDown(e);

			ParticleEmitter emitter = null;
			Drawing.Color startColor;
			Drawing.Color endColor;
			int alpha = 255;

			if (e.Buttons == GorgonLibrary.InputDevices.MouseButtons.Button3)
				_clearScreen = !_clearScreen;
			else
			{
				if (_clearScreen)
					alpha = _rnd.Next(64, 192);

				if (e.Buttons == GorgonLibrary.InputDevices.MouseButtons.Button1)
				{
					startColor = Drawing.Color.FromArgb(alpha, _rnd.Next(0, 255), _rnd.Next(0, 255), _rnd.Next(0, 255));
					endColor = Drawing.Color.FromArgb(0, startColor);
				}
				else
				{
					startColor = Drawing.Color.FromArgb(alpha, Drawing.Color.Black);
					endColor = Drawing.Color.FromArgb(0, Drawing.Color.White);
				}

				emitter = CreateEmitter(e.Position);
				emitter.ColorRange = new Range<System.Drawing.Color>(startColor, endColor);
			}			
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		protected override void Initialize()
		{
			try
			{
				_text = new TextSprite("Help Text", string.Empty, FrameworkFont, Drawing.Color.Black);
				
				_particleImage = new RenderImage("ParticleImage", 64, 64, ImageBufferFormats.BufferRGB888A8);
				_screen = new RenderImage("Screen", Gorgon.Screen.Width, Gorgon.Screen.Height, ImageBufferFormats.BufferRGB888X8);				

				CreateParticle();

				_particleSprite = new Sprite("ParticleSprite", _particleImage);
				_particleSprite.Axis = new Vector2D(32, 32);

				CreateEmitter(new Vector2D(Gorgon.Screen.Width / 2.0f, Gorgon.Screen.Height / 2.0f));
				_screen.Clear();
			}
			catch (Exception ex)
			{
				UI.ErrorBox(this, "Unable to initialize the application.", ex);
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
			: base(@".\ParticleAcceleration.xml")
		{
			InitializeComponent();
			_emitters = new List<ParticleEmitter>();
		}
		#endregion
	}
}