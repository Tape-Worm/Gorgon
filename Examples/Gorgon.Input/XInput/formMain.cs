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
// Created: Friday, January 11, 2013 8:27:21 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Input;
using Gorgon.Plugins;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Examples
{
	/// <summary>
	/// Our main form for the example.
	/// </summary>
	/// <remarks>
	/// This example is exclusively for the XBox controller (although it could be made to work with a regular controller).  
	/// 
	/// Here we will load the XInput plug-in which will give us access to the functionality in the XBox controllers.  
	/// 
	/// The XBox controller interface will always return 4 controllers, even if there is only 1 controller connected.  Here
	/// we make use of all 4 controllers (if connected) to give a sort of multi-player drawing example.  We do this by 
	/// tracking the state of each controller in our idle loop.  
	/// 
	/// The set up is similar to the other input examples:  Load the driver plug-in assembly, create the xinput driver and 
	/// create each controller as needed.  Then in the idle loop, we poll the controller for input.  If no XBox controller is 
	/// found a prompt to plug one in is shown.
	/// 
	/// This example also shows how to use vibration and how to make a pressure sensitive trigger on the XBox controllers.
	/// 
	/// To activate the spray can effect press the trigger.  The harder it's pushed, the darker the color and the quicker 
	/// the trigger is pushed, the thicker the spray.  Use the secondary directional stick to spray in a given direction.  
	/// </remarks>
	public partial class formMain : Form
	{
		#region Variables.
		// Our XInput driver.
		private IGorgonGamingDeviceDriver _driver;
		// Our XBox controllers.
		private IList<IGorgonGamingDevice> _controllers;
		// Current stick position point for the spray.
		private PointF[] _stickPosition;
		// Spray states.
		private SprayCan[] _sprayStates;
		// Surface to draw on.
		private DrawingSurface _surface;                        
		#endregion

		#region Methods.
		/// <summary>
		/// Function called during the paint event of the controls that display controller information.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="PaintEventArgs" /> instance containing the event data.</param>
		private void ControllerControlsPaint(object sender, PaintEventArgs e)
		{
			var control = sender as Control;

			if (control != null)
			{
				e.Graphics.DrawLine(Pens.Black, Point.Empty, new Point(control.ClientRectangle.Right, 0));
			}
		}

		/// <summary>
		/// Function to update the controller labels.
		/// </summary>
		/// <param name="device">Controller to update.</param>
		/// <param name="index">Index of the controller.</param>
		private void UpdateControllerLabels(IGorgonGamingDevice device, int index)
		{			
			var panel = (Panel)panelControllers.Controls["panelController" + index];
			var label = (Label)panel.Controls["labelController" + index];			

			// Update the label visibility for the controller.
			if (device.IsConnected)
			{
				if (!panel.Visible)
				{
					panel.Visible = true;
				}
			}
			else
			{
				label.Text = $"{device.Info.Description}";

				// Turn off the other ones since we don't want to clutter 
				// up the screen.
				if ((index > 0) && (panel.Visible))
				{
					panel.Visible = false;
				}				
				return;
			}			

			// Set the label information.
			label.Text = $"{device.Info.Description}: " +
			             $"Left Stick Position {device.Axis[GamingDeviceAxis.LeftStickX].Value}x{device.Axis[GamingDeviceAxis.LeftStickY].Value}. " +
			             $"Right Stick Position {device.Axis[GamingDeviceAxis.RightStickX].Value}x{device.Axis[GamingDeviceAxis.RightStickY].Value}. " +
			             $"Right Trigger Position {device.Axis[GamingDeviceAxis.RightTrigger].Value}. " +
			             $"Left Trigger Position {device.Axis[GamingDeviceAxis.LeftTrigger].Value}.";
		}

	    /// <summary>
	    /// Function to draw a spray on the screen.
	    /// </summary>
	    /// <param name="device">Controller to use for the spray.</param>
	    /// <param name="index">Index of the controller.</param>
	    private void DrawSpray(IGorgonGamingDevice device, int index)
		{
			SprayCan state = _sprayStates[index];

	        // Update the origin of the spray.
			state.Origin = _stickPosition[index];

			GorgonRange throttleRange = device.Info.AxisInfo[GamingDeviceAxis.RightTrigger].Range;

			// Find out if we're spraying.
			if (device.Axis[GamingDeviceAxis.RightTrigger].Value > throttleRange.Minimum)
			{
				if ((!state.IsActive) && (!state.NeedReset))
				{
					// Convert the throttle value to a unit value.
					float throttleUnit = ((float)(device.Axis[GamingDeviceAxis.RightTrigger].Value - throttleRange.Minimum) / throttleRange.Range);

					// Set up the spray state.
					state.Position = state.Origin;
					state.Amount = throttleRange.Range / 2.0f;
					state.Time = throttleUnit * 10.0f;
					state.VibrationAmount = device.Info.VibrationMotorRanges[1].Maximum;
					state.SprayAlpha = (throttleUnit * 239.0f) + 16;
					state.IsActive = true;
				}
			}
			else
			{
				state.IsActive = false;
			}

		    if (!state.IsActive)
		    {
			    return;
		    }

		    // Update the state spray effect.
		    state.Update();
		    _surface.DrawPoint(Point.Round(state.Position), state.SprayColor, state.SprayPointSize);
		}

		/// <summary>
		/// Function to draw the controller cursor.
		/// </summary>
		/// <param name="controller">Controller for the cursor.</param>
		/// <param name="index">Index of the controller.</param>
		private void DrawControllerCursor(IGorgonGamingDevice controller, int index)
		{
			GorgonRange xRange = controller.Info.AxisInfo[GamingDeviceAxis.LeftStickX].Range;
			GorgonRange yRange = controller.Info.AxisInfo[GamingDeviceAxis.LeftStickY].Range;
			var playerColorValue = (int)((uint)0xFF << (index * 8) | 0xFF000000);						// Get the color based on the controller index.			
			var cursorSize = new Size(_surface.CursorSize.Width / 2, _surface.CursorSize.Height / 2);	// Get the cursor size with offset.

			// Transform the axis into a -1 .. 1 range.				
			var moveVector = new PointF(controller.Axis[GamingDeviceAxis.LeftStickX].Value - (float)xRange.Minimum,
											controller.Axis[GamingDeviceAxis.LeftStickY].Value - (float)yRange.Minimum);

			moveVector = new PointF((moveVector.X / (xRange.Range + 1) * 2.0f) - 1.0f,
									(moveVector.Y / (yRange.Range + 1) * 2.0f) - 1.0f);

			// Move at 100 units per second 
			float speed = panelDisplay.ClientSize.Width / 2.0f * GorgonTiming.Delta;
			var position = new PointF((speed * moveVector.X) + _stickPosition[index].X,
										(speed * -moveVector.Y) + _stickPosition[index].Y);


			// Limit the range of the positioning.
			if (position.X < -cursorSize.Width)
			{
				position = new PointF(panelDisplay.ClientRectangle.Right + cursorSize.Width, position.Y);
			}

			if (position.Y <= -cursorSize.Height)
			{
				position = new PointF(position.X, panelDisplay.ClientRectangle.Bottom + cursorSize.Height);
			}

			if (position.X > panelDisplay.ClientRectangle.Right + cursorSize.Width)
			{
				position = new PointF(-cursorSize.Width, position.Y);
			}

			if (position.Y > panelDisplay.ClientRectangle.Bottom + cursorSize.Height)
			{
				position = new PointF(position.X, -cursorSize.Height);
			}

			// Draw our cursor.
			_surface.DrawCursor(Point.Round(position), Color.FromArgb(playerColorValue));

			// Update our global position.
			_stickPosition[index] = position;			
		}

		/// <summary>
		/// Function to check to see if any of the devices are connected.
		/// </summary>
		private async Task CheckForConnectedDevices()
		{
			while ((GorgonApplication.IsRunning) && (_controllers != null))
			{
				if (_controllers.Any(item => item.IsConnected))
				{
					BeginInvoke(new Action(() =>
					                       {
						                       if (!labelMessage.Visible)
						                       {
							                       return;
						                       }

											   labelMessage.Visible = false;
						                       panelControllers.Visible = true;
					                       }));
				}
				else
				{
					BeginInvoke(new Action(() =>
					                       {
						                       if (!labelMessage.Visible)
						                       {
							                       labelMessage.Visible = true;
							                       panelControllers.Visible = false;
						                       }

						                       UpdateActiveControllers();
					                       }));
				}

				// This delay is implemented so we don't spam the main thread with messages.
				await Task.Delay(250);
			}
		}

		/// <summary>
		/// Function to process idle time in the application.
		/// </summary>
		/// <returns><b>true</b> to continue processing, <b>false</b> to end processing.</returns>
		private bool Idle()
		{
			if (labelMessage.Visible)
			{
				return true;
			}
			
			_surface.Clear(Color.White);			

			for (int i = 0; i < _controllers.Count; i++)
			{				
				var controller = _controllers[i];

				// Do nothing if this joystick isn't connected.
				if (!controller.IsConnected)
				{
					continue;
				}

				controller.Poll();

				// Clear the drawing if the primary button is clicked.
				if (controller.Button[0] == GamingDeviceButtonState.Down)
				{
					_surface.ClearDrawing();
				}

				// Draw the controller cursor.
				DrawControllerCursor(controller, i);

				// Begin drawing.	
				DrawSpray(controller, i);

				UpdateControllerLabels(controller, i);
			}

			_surface.Render();

			return true;
		}

		/// <summary>
		/// Function to initialize the controller and its dead zones.
		/// </summary>
		/// <param name="controller">The controller to initialize.</param>
		/// <returns><b>true</b> if the controller is connected and set up, <b>false</b> if not.</returns>
		private static bool SetupGamePad(IGorgonGamingDevice controller)
		{
			if (!controller.IsConnected)
			{
				return false;
			}

			/*GorgonRange xRange = controller.Info.AxisInfo[GamingDeviceAxis.LeftStickX].Range;
			GorgonRange yRange = controller.Info.AxisInfo[GamingDeviceAxis.LeftStickY].Range;
			GorgonRange x2Range = controller.Info.AxisInfo[GamingDeviceAxis.RightStickX].Range;
			GorgonRange y2Range = controller.Info.AxisInfo[GamingDeviceAxis.RightStickY].Range;

			controller.Axis[GamingDeviceAxis.LeftStickX].DeadZone = new GorgonRange(xRange.Minimum / 4, xRange.Maximum / 4);
			controller.Axis[GamingDeviceAxis.LeftStickY].DeadZone = new GorgonRange(yRange.Minimum / 4, yRange.Maximum / 4);
			controller.Axis[GamingDeviceAxis.RightStickX].DeadZone = new GorgonRange(x2Range.Minimum / 128, x2Range.Maximum / 128);
			controller.Axis[GamingDeviceAxis.RightStickY].DeadZone = new GorgonRange(y2Range.Minimum / 128, y2Range.Maximum / 128);*/

			return true;
		}

		/// <summary>
		/// Function to enumerate and update the active controllers list.
		/// </summary>
		private void UpdateActiveControllers()
		{
			IReadOnlyList<IGorgonGamingDeviceInfo> controllers = _driver.EnumerateGamingDevices(true);

			// Enumerate the active controllers.  We'll only take 3 of the 4 available xbox controllers, and only if they have the correct capabilities.
			_controllers = controllers.Where(item => (item.Capabilities & GamingDeviceCapabilityFlags.SupportsThrottle) == GamingDeviceCapabilityFlags.SupportsThrottle &&
													 (item.Capabilities & GamingDeviceCapabilityFlags.SupportsSecondaryXAxis) == GamingDeviceCapabilityFlags.SupportsSecondaryXAxis &&
													 (item.Capabilities & GamingDeviceCapabilityFlags.SupportsSecondaryYAxis) == GamingDeviceCapabilityFlags.SupportsSecondaryYAxis &&
													 (item.Capabilities & GamingDeviceCapabilityFlags.SupportsVibration) == GamingDeviceCapabilityFlags.SupportsVibration &&
													 (item.Capabilities & GamingDeviceCapabilityFlags.SupportsRudder) == GamingDeviceCapabilityFlags.SupportsRudder)
									  .Take(3)
									  .Select(item => _driver.CreateGamingDevice(item)).ToArray();

			for (int i = 0; i < _controllers.Count; i++)
			{
				if (!SetupGamePad(_controllers[i]))
				{
					continue;
				}

				// Start at a random spot.					
				_stickPosition[i] = new Point(GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Width - 64),
											  GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Height - 64));

				// Turn off spray for all controllers.
				_sprayStates[i] = new SprayCan(_controllers[i], i);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			GorgonPluginAssemblyCache assemblies = null;

			base.OnLoad(e);

			try
			{
				// Create the assembly cache.
				assemblies = new GorgonPluginAssemblyCache(GorgonApplication.Log);
				assemblies.Load(Program.PlugInPath + "Gorgon.Input.XInput.dll");

				// Create the plug services.
				GorgonPluginService pluginService = new GorgonPluginService(assemblies, GorgonApplication.Log);

				// Create the gaming device driver factory.
				GorgonGamingDeviceDriverFactory factory = new GorgonGamingDeviceDriverFactory(pluginService, GorgonApplication.Log);

				// Create our factory.
				_driver = factory.LoadDriver("Gorgon.Input.XInput.GorgonXInputDriver");

				_stickPosition = new PointF[3];
				_sprayStates = new SprayCan[3];

				// Get our list of active controllers.
				UpdateActiveControllers();

				// Get the graphics interface for our panel.
				_surface = new DrawingSurface(panelDisplay);

				// Set up our idle loop.
				GorgonApplication.IdleMethod += Idle;

				// Launch a background task to check for connected devices.
				Task.Run(() => CheckForConnectedDevices());
			}
			catch (Exception ex)
			{
				// We do this here instead of just calling the dialog because this
				// function will send the exception to the Gorgon log file.
				ex.Catch(_ => GorgonDialogs.ErrorBox(this, _), GorgonApplication.Log);
				GorgonApplication.Quit();
			}
			finally
			{
				assemblies?.Dispose();
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (_controllers != null)
			{
				foreach (IGorgonGamingDevice controller in _controllers)
				{
					controller.Dispose();
				}
			}

			if (_surface == null)
			{
				return;
			}

			_surface.Dispose();
			_surface = null;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="formMain" /> class.
		/// </summary>
		public formMain()
		{
			InitializeComponent();
		}
		#endregion
	}
}
