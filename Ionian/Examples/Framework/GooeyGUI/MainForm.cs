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
// Created: TOBEREPLACED
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
using GorgonLibrary.GUI;
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
		private Random _rnd = new Random();					// Random number generator.
		private GUISkin _skin = null;						// GUI skin.
		private Desktop _desktop = null;					// The root container for the GUI.
		private GUIWindow _window = null;					// A window in which to display our stuff.
		private GUIButton _button = null;					// Close button.
		private GUICanvas _canvas = null;					// Drawing canvas.
		private ParticleEmitter _emitter = null;			// Particle emitter.
		private GUICheckBox _check = null;					// Checkbox.
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Clicked event of the button control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void button_Clicked(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Function to perform rendering updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnFrameUpdate(FrameEventArgs e)
		{
			base.OnFrameUpdate(e);

			_desktop.Draw();
		}

		/// <summary>
		/// Function to perform logic updates.
		/// </summary>
		/// <param name="e">Frame event parameters.</param>
		protected override void OnLogicUpdate(FrameEventArgs e)
		{
			base.OnLogicUpdate(e);
			
			_desktop.Update(e.FrameDeltaTime);
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
		}

		/// <summary>
		/// Function called before Gorgon is shut down.
		/// </summary>
		/// <returns>TRUE if successful, FALSE if not.</returns>
		/// <remarks>Users should override this function to perform clean up when the application closes.</remarks>
		protected override bool OnGorgonShutDown()
		{
			if (_button != null)
				_button.Clicked -= new EventHandler(button_Clicked);

			if (_canvas != null)
			{
				_canvas.CanvasDraw -= new EventHandler(_canvas_CanvasDraw);
				_canvas.CanvasUpdate -= new UpdateEventHandler(_canvas_CanvasUpdate);
			}

			if (_desktop != null)
				_desktop.Dispose();

			if (_skin != null)
				_skin.Dispose();

			_desktop = null;
			_skin = null;

			return true;
		}

		/// <summary>
		/// Function to provide initialization for our example.
		/// </summary>
		protected override void Initialize()
		{		
			_skin = GUISkin.FromFile(FileSystems["GUISkin"]);
			_desktop = new Desktop(Input, _skin);
			_window = new GUIWindow("Window", Gorgon.Screen.Width / 4, Gorgon.Screen.Height / 4, Gorgon.Screen.Width - (Gorgon.Screen.Width / 4) * 2, Gorgon.Screen.Height - (Gorgon.Screen.Height / 4) * 2);
			_desktop.Windows.Add(_window);
			_window.Text = "This is a GUI window.";

			_check = new GUICheckBox("Check");
			_check.Owner = _window;
			_check.Text = "Check me to change color.";
			_check.Position = new System.Drawing.Point(_window.ClientArea.Width / 8, 0);
			_check.Size = new Drawing.Size(_window.ClientArea.Width - (_window.ClientArea.Width / 8 * 2), 16);
			_check.ForeColor = Drawing.Color.Black;

			_canvas = new GUICanvas("Canvas");
			_canvas.Owner = _window;
			_canvas.Position = new Drawing.Point(_window.ClientArea.Width / 8, 20);
			_canvas.Size = new Drawing.Size(_window.ClientArea.Width - (_window.ClientArea.Width / 8) * 2, _window.ClientArea.Height - 60);
			_canvas.BorderStyle = PanelBorderStyle.Single;
			_canvas.CanvasDraw += new EventHandler(_canvas_CanvasDraw);
			_canvas.CanvasUpdate += new UpdateEventHandler(_canvas_CanvasUpdate);

			_button = new GUIButton("CloseButton");
			_button.Owner = _window;
			_button.Position = new Drawing.Point(_window.ClientArea.Width / 4, _window.ClientArea.Height - 20);
			_button.Size = new Drawing.Size(_window.ClientArea.Width - (_window.ClientArea.Width / 4) * 2, 20);
			_button.Text = "Click to close.";
			_button.TextAlignment = Alignment.Center;
			_button.Clicked += new EventHandler(button_Clicked);
		
			_desktop.ShowDesktopBackground = true;
			_desktop.BackgroundColor = Drawing.Color.Tan;
			_desktop.FocusRectangleColor = Drawing.Color.FromArgb(128, Drawing.Color.Red);
			_desktop.FocusRectangleBlend = BlendingModes.Additive;
			_desktop.FocusRectangleOutline = false;

			_emitter = new ParticleEmitter("Emitter", new Sprite("Particle", Image.FromFileSystem(FileSystems["GUISkin"], "/Images/particle.png")),
				new Vector2D(_canvas.ClientArea.Width / 2, _canvas.ClientArea.Height / 2));
			_emitter.ParticleSprite.BlendingMode = BlendingModes.Additive;
			_emitter.ParticleSprite.Axis = new Vector2D(_emitter.ParticleSprite.Width / 2, _emitter.ParticleSprite.Height / 2);
			_emitter.ColorRange = new Range<Drawing.Color>(Drawing.Color.Yellow, Drawing.Color.FromArgb(0, 255, 255, 255));
			_emitter.ContinuousParticles = true;
			_emitter.EmitterLifetime = 1.5f;
			_emitter.ParticleLifetimeRange = new Range<float>(0.0f, 15.0f);
			_emitter.ParticleSizeRange = new Range<float>(1.5f, 0.0f);
			_emitter.ParticleSpeedRange = new Range<float>(0.0f, 10.0f);
			_emitter.TangentialAccelerationRange = new Range<float>(0.0f, 0.0f);
			_emitter.RadialAccelerationRange = new Range<float>(0.0f, 10.0f);
			_emitter.ParticleRotationRange = new Range<float>(-10.0f, 10.0f);
			_emitter.Spread = 360.0f;
			_emitter.EmitterScale = 1.0f;
		}

		/// <summary>
		/// Handles the CanvasUpdate event of the _canvas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.GUI.UpdateEventArgs"/> instance containing the event data.</param>
		private void _canvas_CanvasUpdate(object sender, UpdateEventArgs e)
		{
			// We have to transform the coordinates to be relative to the canvas.
			Vector2D position = e.ConvertToScreen(new Vector2D(_canvas.ClientArea.Width / 2, _canvas.ClientArea.Height / 2));
			
			if (_check.Checked)
				_emitter.ColorRange = new Range<Drawing.Color>(Drawing.Color.FromArgb(_rnd.Next(64, 255), _rnd.Next(64, 255), _rnd.Next(64, 255)), Drawing.Color.FromArgb(0, 255, 255, 255));
			else
				_emitter.ColorRange = new Range<Drawing.Color>(Drawing.Color.Yellow, Drawing.Color.FromArgb(0, 255, 255, 255));
			
			_emitter.Move(position.X, position.Y);
			_emitter.Update(e.FrameTimeDelta);
		}

		/// <summary>
		/// Handles the CanvasDraw event of the _canvas control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void _canvas_CanvasDraw(object sender, EventArgs e)
		{
			Gorgon.Screen.Clear(Drawing.Color.FromArgb(0, 0, 0, 48));
			_emitter.Draw();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		public MainForm()
			: base(@".\GooeyGUI.xml")
		{
			InitializeComponent();
		}
		#endregion
	}
}