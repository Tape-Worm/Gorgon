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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Input;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Plugins;
using Gorgon.Renderers;
using Gorgon.UI;
using GorgonMouseButtons = Gorgon.Input.MouseButtons;
using SlimMath;
using MouseButtons = System.Windows.Forms.MouseButtons;

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
	/// J - Switch to raw input and joysticks (if available).  Press J to cycle through joysticks and to get back to the win forms keyboard/mouse interface.
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
		// The graphics interface.
		private GorgonGraphics _graphics;
		// Primary swap chain.
		private GorgonSwapChain _screen;
		// The 2D renderer.
		private Gorgon2D _2D;
		// Text font. 
		private GorgonFont _font;
		// Input factory.
		private GorgonRawInput _input;                         
		// Mouse object.
		private GorgonRawMouse _mouse;
		// Gaming device drivers.
		private IReadOnlyList<IGorgonGamingDeviceDriver> _drivers;
		// Joystick list.
		private List<IGorgonGamingDevice> _joystickList;
		// Joystick.
		private IGorgonGamingDevice _joystick;
		// Text sprite object.
		private GorgonText _messageSprite;
		// Back buffer.
		private GorgonRenderTarget2D _backBuffer;
		// Backup image.
		private GorgonTexture2D _backupImage;
		// Pen radius.
		private float _radius = 6.0f;
		// Blend mode.
		private BlendingMode _blendMode = BlendingMode.Modulate;
		// Joystick index counter.
		private int _counter = -2;
		// Flag to indicate that we are using windows forms input.
		private bool _useWinFormsInput = true;
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the <see cref="E:KeyDown" /> event.
		/// </summary>
		/// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Escape:
					Close();			// Close
					break;
				case Keys.F:
					_screen.UpdateSettings(!_screen.Settings.IsWindowed);
					break;
				case Keys.Down:
					_radius -= 1.0f;
					if (_radius < 2.0f)
					{
						_radius = 2.0f;
					}
					break;
				case Keys.Up:
					_radius += 1.0f;
					if (_radius > 10.0f)
					{
						_radius = 10.0f;
					}
					break;
				case Keys.F1:
					_blendMode = BlendingMode.Modulate;
					break;
				case Keys.F2:
					_blendMode = BlendingMode.Additive;
					break;
				case Keys.F3:
					_blendMode = BlendingMode.None;
					break;
				case Keys.C:
					// Fill the back up image with white.
					using (var imageLock = _backupImage.Lock(BufferLockFlags.Write))
					{
						// We really shouldn't be using DirectAccess here as it's unsupported. 
						// This will require that the "Lock" method return a pointer instead of a stream, or make the lock
						// value return functionality to read/write data directly.
						//DirectAccess.FillMemory(imageLock.Data.BaseIntPtr, 0xff, (int)imageLock.Data.Length);
						var ptr = new GorgonPointerAlias(imageLock.Data.BaseIntPtr, imageLock.Data.Length);
						ptr.Fill(0xff);
					}

					_backBuffer.CopySubResource(_backupImage,
			            new Rectangle(0, 0, _backBuffer.Settings.Width, _backBuffer.Settings.Height));
			        break;
				case Keys.J:
					if (_joystickList.Count != 0)
					{
						// Disable if we go beyond the end of the list.
						_counter++;

						if (_counter == -1)
						{
							// Clip the mouse cursor to our client area.
							Cursor.Clip = _mouse.PositionConstraint = RectangleToScreen(ClientRectangle);
							// Set the position to the current mouse position.
							_mouse.Position = Cursor.Position;

							_input.RegisterDevice(_mouse);
							_useWinFormsInput = false;
							_messageSprite.Text = "Using mouse and keyboard (Raw Input)";
							break;
						}

						if ((_counter >= _joystickList.Count) && (_joystick != null))
						{
							if (!_useWinFormsInput)
							{
								Cursor.Clip = Rectangle.Empty;
								_input.UnregisterDevice(_mouse);
							}

							_useWinFormsInput = true;
							_joystick = null;
							_counter = -2;
							_messageSprite.Text = "Using mouse and keyboard (Windows Forms).";
							break;
						}						

						// Move to the next joystick.
						_joystick = _joystickList[_counter];
						_messageSprite.Text = "Using joystick " + _joystick.Info.Description;
					}
					break;
			}			
		}

		/// <summary>
		/// Handles the <see cref="E:MouseWheel" /> event.
		/// </summary>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			if (!_useWinFormsInput)
			{
				return;
			}

			_radius += e.Delta.Sign();

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
		/// Handles the <see cref="E:MouseDown" /> event.
		/// </summary>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (!_useWinFormsInput)
			{
				return;
			}

			GorgonMouseButtons buttons = GorgonMouseButtons.None;

			if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
			{
				buttons |= GorgonMouseButtons.Left;
			}

			if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
			{
				buttons |= GorgonMouseButtons.Right;
			}

			MouseInput(this, new GorgonMouseEventArgs(buttons, GorgonMouseButtons.None, e.Location, e.Delta, Point.Empty, e.Delta, 0, false));
		}

		/// <summary>
		/// Handles the <see cref="E:MouseMove" /> event.
		/// </summary>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (!_useWinFormsInput)
			{
				return;
			}

			GorgonMouseButtons buttons = GorgonMouseButtons.None;

			if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
			{
				buttons |= GorgonMouseButtons.Left;
			}

			if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
			{
				buttons |= GorgonMouseButtons.Right;
			}

			MouseInput(this, new GorgonMouseEventArgs(buttons, GorgonMouseButtons.None, e.Location, e.Delta, Point.Empty, e.Delta, 0, false));
		}

		/// <summary>
		/// Handles the button and movement events of the _mouse control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonMouseEventArgs" /> instance containing the event data.</param>
		private void MouseInput(object sender, GorgonMouseEventArgs e)
		{
			if (_joystick != null)
			{
				return;
			}

			Color drawColor = Color.Black;		// Drawing color.
			Point position = _useWinFormsInput ? e.Position : PointToClient(e.Position);

		    if (e.Buttons == GorgonMouseButtons.None)
		    {
		        return;
		    }

		    // Draw to the back buffer.
		    _2D.Target = _backBuffer;
		    _2D.Drawing.Blending.DestinationAlphaBlend = BlendType.One;
		    _2D.Drawing.BlendingMode = _blendMode;

		    if ((e.Buttons & GorgonMouseButtons.Left) == GorgonMouseButtons.Left)
		    {
		        drawColor = Color.FromArgb(64, 0, 0, 192);
		    }

		    if ((e.Buttons & GorgonMouseButtons.Right) == GorgonMouseButtons.Right)
		    {
		        drawColor = Color.FromArgb(64, 192, 0, 0);
		    }

		    // Draw the pen.
		    var penPosition = new RectangleF(position.X - (_radius / 2.0f), position.Y - (_radius / 2.0f), _radius, _radius);
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
			// Cursor position.
			Vector2 cursorPosition = PointToClient(!_useWinFormsInput ? _mouse.Position : Cursor.Position);

			if (!_useWinFormsInput)
			{
				Cursor.Position = PointToScreen((Point)cursorPosition);
			}

			// Dump to the screen.
			_2D.Drawing.BlendingMode = BlendingMode.None;
			_2D.Drawing.Blit(_backBuffer, Vector2.Zero);

			if (_joystick != null)
			{
				// Poll the joystick.
				_joystick.Poll();

				GorgonRange xAxisRange = _joystick.Info.AxisInfo[GamingDeviceAxis.XAxis].Range;
				GorgonRange yAxisRange = _joystick.Info.AxisInfo[GamingDeviceAxis.YAxis].Range;

				// Adjust position to match screen coordinates.
				cursorPosition = new Vector2(_joystick.Axis[GamingDeviceAxis.XAxis].Value - xAxisRange.Minimum, 
											 _joystick.Axis[GamingDeviceAxis.YAxis].Value - yAxisRange.Minimum);
				cursorPosition.X = cursorPosition.X / (xAxisRange.Range + 1) * _screen.Settings.Width;
				cursorPosition.Y = _screen.Settings.Height - (cursorPosition.Y / (yAxisRange.Range + 1) * _screen.Settings.Height);
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
			if ((_joystick != null) && (_joystick.Button[0] == GamingDeviceButtonState.Down))
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
					_2D.Drawing.FilledRectangle(penPosition, Color.Black);
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
		}

		/// <summary>
		/// Handles the <see cref="E:Activated" /> event.
		/// </summary>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

			// If the window loses focus, some of our gaming devices will become unacquired. 
			// This means that no data will be received from the gaming devices until they are acquired.
			// Doing this on window activation is the best way to ensure that happens.
			foreach (var joystick in _joystickList)
			{
				joystick.IsAcquired = true;
			}
		}

		protected override void OnDeactivate(EventArgs e)
		{
			base.OnDeactivate(e);

			// Mark each gaming device as unacquired if we lose focus.
			foreach (var joystick in _joystickList)
			{
				joystick.IsAcquired = true;
			}
		}

		/// <summary>
		/// Handles the <see cref="E:FormClosing" /> event.
		/// </summary>
		/// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			// Always dispose your devices when the window is shutting down.
			// Failure to do so can lead to unpredictable results.
			foreach (IGorgonGamingDevice joystick in _joystickList)
			{
				joystick.Dispose();
			}

			foreach (IGorgonGamingDeviceDriver driver in _drivers)
			{
				driver.Dispose();
			}

			_input.UnregisterDevice(_mouse);
			_input?.Dispose();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			GorgonPluginAssemblyCache assemblyCache = null;
			base.OnLoad(e);

			try
			{
				// Load the assembly.
				assemblyCache = new GorgonPluginAssemblyCache(GorgonApplication.Log);

				// Create the plugin service.
				GorgonPluginService plugInService = new GorgonPluginService(assemblyCache, GorgonApplication.Log);

				// Create the factory to retrieve gaming device drivers.
				GorgonGamingDeviceDriverFactory factory = new GorgonGamingDeviceDriverFactory(plugInService);

				// Create the raw input interface.
				_input = new GorgonRawInput(this, GorgonApplication.Log);

				// Get available gaming device driver plug ins.
				assemblyCache.Load(Program.PlugInPath + "Gorgon.Input.DirectInput.dll");
				assemblyCache.Load(Program.PlugInPath + "Gorgon.Input.XInput.dll");

				_drivers = factory.LoadAllDrivers();

				_joystickList = new List<IGorgonGamingDevice>();

				// Get all gaming devices from the drivers.
				foreach (IGorgonGamingDeviceDriver driver in _drivers)
				{
					IReadOnlyList<IGorgonGamingDeviceInfo> infoList = driver.EnumerateGamingDevices(true);

					foreach (IGorgonGamingDeviceInfo info in infoList)
					{
						IGorgonGamingDevice device = driver.CreateGamingDevice(info);

						// Turn off dead zones for this example.
						foreach (GorgonGamingDeviceAxis axis in device.Axis)
						{
							axis.DeadZone = GorgonRange.Empty;
						}
						
						_joystickList.Add(device);
					}
				}

				// Create mouse.
				_mouse = new GorgonRawMouse();

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

				// Create text sprite.
				_messageSprite = _2D.Renderables.CreateText("Message", _font, "Using mouse and keyboard (Windows Forms).");
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
					// See the KeyDown event on how this should be implemented in the future.
					//DirectAccess.FillMemory(textureData.Data.BaseIntPtr, 0xff, (int)textureData.Data.Length);
					var data = new GorgonPointerAlias(textureData.Data.BaseIntPtr, textureData.Data.Length);
					data.Fill(0xff);
				}

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

				// Enable the mouse.
				Cursor = Cursors.Cross;
				_mouse.MouseButtonDown += MouseInput;
				_mouse.MouseMove += MouseInput;
				_mouse.MouseWheelMove += (sender, args) =>
				                         {
											 _radius += args.WheelDelta.Sign();

											 if (_radius < 2.0f)
											 {
												 _radius = 2.0f;
											 }
											 if (_radius > 10.0f)
											 {
												 _radius = 10.0f;
											 }
										 };

				// Set the mouse position.
				_mouse.Position = new Point(ClientSize.Width / 2, ClientSize.Height / 2);

				GorgonApplication.IdleMethod = Gorgon_Idle;
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), GorgonApplication.Log);
				GorgonApplication.Quit();
			}
			finally
			{
				assemblyCache?.Dispose();
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