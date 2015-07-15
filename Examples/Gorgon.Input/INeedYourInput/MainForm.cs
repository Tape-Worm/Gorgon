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
// Created: Sunday, January 13, 2013 6:49:00 PM
// 
#endregion

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Input;
using Gorgon.Native;
using Gorgon.Plugins;
using Gorgon.Renderers;
using Gorgon.UI;
using SlimMath;

namespace Gorgon.Examples
{
	/// <summary>
	/// Main application form.
	/// </summary>
	/// <remarks>
	/// This is an updated version of the INeedYourInput example from the previous version of Gorgon.
	/// 
	/// The keys for the example are as follows:
	/// F - Switch between full screen and windowed mode.
	/// Up arrow - Increase pen radius.
	/// Down arrow - Decrease pen radius.
	/// F1, F2, F3 - Switch between modulated, additive and no blending respectively.
	/// C - Clear the buffer.
	/// J - Switch to joysticks (if available).  Press J to cycle through joysticks and to get back to the keyboard/mouse interface.
	/// ESC - Close the example.
	/// 
	/// Mouse controls:
	/// Left mouse button - Draw with blue pen.
	/// Right mouse button - Draw with red pen.
	/// Scroll wheel - Increase/decrease pen size.
	/// 
	/// Joystick control:
	/// Primary button - Draw with black pen.
	/// </remarks>
	public partial class MainForm 
		: Form
	{
		#region Variables.
		private GorgonGraphics _graphics;					        // The graphics interface.
		private GorgonSwapChain _screen;						    // Primary swap chain.
		private Gorgon2D _2D;								        // The 2D renderer.
		private GorgonFont _font;							        // Text font. 
		private GorgonInputService _input;					        // Input factory.
		private GorgonPointingDevice _mouse;					    // Mouse object.
		private GorgonJoystick[] _joystickList;				        // Joystick list.
		private GorgonJoystick _joystick;					        // Joystick.
		private GorgonKeyboard _keyboard;					        // Keyboard object.
		private GorgonText _messageSprite;					        // Text sprite object.
		private GorgonRenderTarget2D _backBuffer;				    // Back buffer.
		private GorgonTexture2D _backupImage;				        // Backup image.
		private float _radius = 6.0f;								// Pen radius.
		private BlendingMode _blendMode = BlendingMode.Modulate;	// Blend mode.
		private int _counter = -1;									// Joystick index counter.
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the KeyDown event of the _keyboard control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="KeyboardEventArgs" /> instance containing the event data.</param>
		private void _keyboard_KeyDown(object sender, KeyboardEventArgs e)
		{
			switch (e.Key)
			{
				case KeyboardKey.Escape:
					Close();			// Close
					break;
				case KeyboardKey.F:
					_screen.UpdateSettings(!_screen.Settings.IsWindowed);
					break;
				case KeyboardKey.Down:
					_radius -= 1.0f;
					if (_radius < 2.0f)
						_radius = 2.0f;
					break;
				case KeyboardKey.Up:
					_radius += 1.0f;
					if (_radius > 10.0f)
						_radius = 10.0f;
					break;
				case KeyboardKey.F1:
					_blendMode = BlendingMode.Modulate;
					break;
				case KeyboardKey.F2:
					_blendMode = BlendingMode.Additive;
					break;
				case KeyboardKey.F3:
					_blendMode = BlendingMode.None;
					break;
				case KeyboardKey.C:
					// Fill the back up image with white.
					using (var imageLock = _backupImage.Lock(BufferLockFlags.Write))
					{
#warning We really shouldn't be using DirectAccess here as it's unsupported. Replace with a "buffer" class or something.
						DirectAccess.FillMemory(imageLock.Data.BaseIntPtr, 0xff, (int)imageLock.Data.Length);
					}

					_backBuffer.CopySubResource(_backupImage,
			            new Rectangle(0, 0, _backBuffer.Settings.Width, _backBuffer.Settings.Height));
			        break;
				case KeyboardKey.J:
					if (_input.JoystickDevices.Count != 0)
					{
						// Disable if we go beyond the end of the list.
						_counter++;
						if ((_counter >= _input.JoystickDevices.Count) && (_joystick != null))
						{
							_joystick = null;
							_counter = -1;
							_messageSprite.Text = "Using mouse and keyboard.";
							break;
						}						

						// Move to the next joystick.
						_joystick = _joystickList[_counter];
						_messageSprite.Text = "Using joystick " + _joystick.Name;
					}
					break;
			}			
		}

		/// <summary>
		/// Handles the PointingDeviceWheelMove event of the _mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PointingDeviceEventArgs" /> instance containing the event data.</param>
		private void _mouse_PointingDeviceWheelMove(object sender, PointingDeviceEventArgs e)
		{
			if (e.WheelDelta > 0)
			{
				_radius += 1.0f;
			}
			if (e.WheelDelta < 0)
			{
				_radius -= 1.0f;
			}

			if (_radius < 2.0f)
			{
				_radius = 2.0f;
			}
			if (_radius > 10.0f)
			{
				_radius = 10.0f;
			}
		}

		/// <summary>
		/// Handles the button and movement events of the _mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PointingDeviceEventArgs" /> instance containing the event data.</param>
		private void MouseInput(object sender, PointingDeviceEventArgs e)
		{
			Color drawColor = Color.Black;		// Drawing color.

		    if (e.Buttons == PointingDeviceButtons.None)
		    {
		        return;
		    }

		    // Draw to the back buffer.
		    _2D.Target = _backBuffer;
		    _2D.Drawing.Blending.DestinationAlphaBlend = BlendType.One;
		    _2D.Drawing.BlendingMode = _blendMode;

		    if ((e.Buttons & PointingDeviceButtons.Left) == PointingDeviceButtons.Left)
		    {
		        drawColor = Color.FromArgb(64, 0, 0, 192);
		    }

		    if ((e.Buttons & PointingDeviceButtons.Right) == PointingDeviceButtons.Right)
		    {
		        drawColor = Color.FromArgb(64, 192, 0, 0);
		    }

		    // Draw the pen.
		    var penPosition = new RectangleF(e.Position.X - (_radius / 2.0f), e.Position.Y - (_radius / 2.0f), _radius, _radius);
		    if (_radius > 3.0f)
		    {
		        _2D.Drawing.FilledEllipse(penPosition, drawColor);
		    }
		    else
		    {
		        _2D.Drawing.FilledRectangle(penPosition, drawColor);
		    }

		    _2D.Drawing.Blending.DestinationAlphaBlend = BlendType.Zero;
		    _2D.Target = null;
		}

		/// <summary>
		/// Function to process during idle time.
		/// </summary>
		/// <returns><b>true</b> to continue processing, <b>false</b> to end processing.</returns>
		private bool Gorgon_Idle()
		{
			Vector2 cursorPosition = _mouse.Position;		// Cursor position.

			// Dump to the screen.
			_2D.Drawing.BlendingMode = BlendingMode.None;
			_2D.Drawing.Blit(_backBuffer, Vector2.Zero);

			if (_joystick != null)
			{
				// Poll the joystick.
				_joystick.Poll();

				// Adjust position to match screen coordinates.
				cursorPosition = new Vector2(_joystick.X - _joystick.Capabilities.XAxisRange.Minimum, 
											 _joystick.Y - _joystick.Capabilities.YAxisRange.Minimum);
				cursorPosition.X = cursorPosition.X / (_joystick.Capabilities.XAxisRange.Range + 1) * _screen.Settings.Width;
				cursorPosition.Y = _screen.Settings.Height - (cursorPosition.Y / (_joystick.Capabilities.YAxisRange.Range + 1) * _screen.Settings.Height);
			}

			// Draw cursor.
			_2D.Drawing.BlendingMode = BlendingMode.Inverted;
			if (_radius > 3.0f)
			{
				_2D.Drawing.FilledEllipse(new RectangleF(cursorPosition.X - (_radius / 2.0f), cursorPosition.Y - (_radius / 2.0f), _radius, _radius), Color.White);
			}
			else
			{
				_2D.Drawing.FilledRectangle(new RectangleF(cursorPosition.X - (_radius / 2.0f), cursorPosition.Y - (_radius / 2.0f), _radius, _radius), Color.White);
			}
						
			// If we have a joystick button down, then draw a black dot.
			if ((_joystick != null) && (_joystick.Button[0].IsPressed))
			{
				var penPosition = new RectangleF(cursorPosition.X - (_radius / 2.0f), cursorPosition.Y - (_radius / 2.0f), _radius, _radius);
				_2D.Drawing.BlendingMode = BlendingMode.Modulate;
				_2D.Target = _backBuffer;
				if (_radius > 3.0f)
				{
					_2D.Drawing.FilledEllipse(penPosition, Color.Black);
				}
				else
				{
					_2D.Drawing.FilledRectangle(penPosition, Color.White);
				}
				_2D.Target = null;
			}			

			_messageSprite.BlendingMode = BlendingMode.Modulate;
			_messageSprite.Draw();

			_2D.Render();

			return true;
		}

		/// <summary>
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			// We do this check because a Maximized state does not call the 
			// OnResizeEnd method.
			if (WindowState == FormWindowState.Maximized)
			{
				OnResizeEnd(e);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.ResizeEnd" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnResizeEnd(EventArgs e)
		{
			base.OnResizeEnd(e);
						
			var currentImageSize = new Size(_backBuffer.Settings.Width, _backBuffer.Settings.Height);

			// Copy the render target texture to a temporary buffer and resize the main buffer.
			// The copy the temporary buffer back to the main buffer.
            _backupImage.CopySubResource(_backBuffer,
                new Rectangle(Point.Empty, currentImageSize));

			_backBuffer.Dispose();
			_backBuffer = _graphics.Output.CreateRenderTarget("BackBuffer", new GorgonRenderTarget2DSettings
			{
				Width = ClientSize.Width,
                Height = ClientSize.Height,
				Format = BufferFormat.R8G8B8A8_UIntNormal
			});
			_backBuffer.Clear(Color.White);
		    _backBuffer.CopySubResource(_backupImage,
		        new Rectangle(0, 0, _backBuffer.Settings.Width, _backBuffer.Settings.Height));

			// Set the mouse range to the new size.
			_mouse.SetPositionRange(0, 0, ClientSize.Width, ClientSize.Height);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			IGorgonPluginAssemblyCache assemblyCache = null;
			base.OnLoad(e);

			try
			{
				// Load the assembly.
				assemblyCache = new GorgonPluginAssemblyCache(GorgonApplication.Log);
				assemblyCache.Load(Program.PlugInPath + "Gorgon.Input.Raw.DLL");

				// Create the plugin service.
				IGorgonPluginService plugInService = new GorgonPluginService(assemblyCache, GorgonApplication.Log);
				
				// Create the service.
				GorgonInputServiceFactory inputServiceFactory = new GorgonInputServiceFactory(plugInService, GorgonApplication.Log);
				_input = inputServiceFactory.CreateService("Gorgon.Input.GorgonRawPlugIn");

				// Create mouse, keyboard and joystick interfaces.
				_keyboard = _input.CreateKeyboard(this);
				_mouse = _input.CreateMouse(this);
				_joystickList = _input.JoystickDevices.Select(item => _input.CreateJoystick(this, item.Name)).ToArray();

				// Create the graphics interface.
				_graphics = new GorgonGraphics();
				_screen = _graphics.Output.CreateSwapChain("Screen",
				                                           new GorgonSwapChainSettings
				                                           {
					                                           Size = Settings.Default.Resolution,
					                                           Format = BufferFormat.R8G8B8A8_UIntNormal,
					                                           IsWindowed = Settings.Default.IsWindowed
				                                           });

				// For the backup image. Used to make it as large as the monitor that we're on.
				Screen currentScreen = Screen.FromHandle(Handle);

				// Relocate the window to the center of the screen.				
				Location = new Point(currentScreen.Bounds.Left + (currentScreen.WorkingArea.Width / 2) - ClientSize.Width / 2,
				                     currentScreen.Bounds.Top + (currentScreen.WorkingArea.Height / 2) - ClientSize.Height / 2);


				// Create the 2D renderer.
				_2D = _graphics.Output.Create2DRenderer(_screen);

				// Create the text font.
				_font = _graphics.Fonts.CreateFont("Arial_9pt",
				                                   new GorgonFontSettings
				                                   {
					                                   FontFamilyName = "Arial",
					                                   FontStyle = FontStyle.Bold,
					                                   AntiAliasingMode = FontAntiAliasMode.AntiAlias,
					                                   FontHeightMode = FontHeightMode.Points,
					                                   Size = 9.0f
				                                   });

				// Enable the mouse.
				Cursor = Cursors.Cross;
				_mouse.Enabled = true;
				_mouse.Exclusive = false;
				_mouse.PointingDeviceDown += MouseInput;
				_mouse.PointingDeviceMove += MouseInput;
				_mouse.PointingDeviceWheelMove += _mouse_PointingDeviceWheelMove;

				// Enable the keyboard.
				_keyboard.Enabled = true;
				_keyboard.Exclusive = false;
				_keyboard.KeyDown += _keyboard_KeyDown;

				// Create text sprite.
				_messageSprite = _2D.Renderables.CreateText("Message", _font, "Using mouse and keyboard.");
				_messageSprite.Color = Color.Black;

				// Create a back buffer.
				_backBuffer = _graphics.Output.CreateRenderTarget("BackBuffer",
				                                                  new GorgonRenderTarget2DSettings
				                                                  {
					                                                  Width = _screen.Settings.Width,
					                                                  Height = _screen.Settings.Height,
					                                                  Format = BufferFormat.R8G8B8A8_UIntNormal
				                                                  });
				_backBuffer.Clear(Color.White);

				var settings = new GorgonTexture2DSettings
				               {
					               Width = currentScreen.Bounds.Width,
					               Height = currentScreen.Bounds.Height,
					               Format = BufferFormat.R8G8B8A8_UIntNormal,
					               Usage = BufferUsage.Staging
				               };

				// Clear our backup image to white to match our primary screen.
				_backupImage = _graphics.Textures.CreateTexture("Backup", settings);
				using (var textureData = _backupImage.Lock(BufferLockFlags.Write))
				{
#warning We really shouldn't be using DirectAccess here as it's unsupported. Replace with a "buffer" class or something.
					DirectAccess.FillMemory(textureData.Data.BaseIntPtr, 0xff, (int)textureData.Data.Length);
				}

				// Set the mouse range and position.
				Cursor.Position = PointToScreen(new Point(Settings.Default.Resolution.Width / 2, Settings.Default.Resolution.Height / 2));
				_mouse.SetPosition(Settings.Default.Resolution.Width / 2, Settings.Default.Resolution.Height / 2);
				_mouse.SetPositionRange(0, 0, Settings.Default.Resolution.Width, Settings.Default.Resolution.Height);

				// Set gorgon events.
				_screen.AfterStateTransition += (sender, args) =>
				                                {
					                                OnResizeEnd(EventArgs.Empty);

					                                // Reposition after a state change.
					                                if (!args.IsWindowed)
					                                {
						                                return;
					                                }

					                                Screen monitor = Screen.FromHandle(Handle);
					                                Location = new Point(monitor.Bounds.Left + (monitor.WorkingArea.Width / 2) - args.Width / 2,
					                                                     monitor.Bounds.Top + (monitor.WorkingArea.Height / 2) - args.Height / 2);
					                                Cursor.Position = PointToScreen(Point.Round(_mouse.Position));
				                                };
				GorgonApplication.IdleMethod = Gorgon_Idle;
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), GorgonApplication.Log);
				GorgonApplication.Quit();
			}
			finally
			{
				if (assemblyCache != null)
				{
					assemblyCache.Dispose();
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
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