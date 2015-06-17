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
	/// The set up is similar to the other input examples:  Load the plug-in assembly, create the input service and create
	/// each joystick as needed.  Then in the idle loop, we poll the joysticks for input.  If no XBox controller is found
	/// a prompt to plug one in is shown.
	/// 
	/// This example also shows how to use vibration and how to make a pressure sensitive trigger on the XBox controllers.
	/// 
	/// To activate the spray can effect press the trigger.  The harder it's pushed, the darker the color and the quicker 
	/// the trigger is pushed, the thicker the spray.  Use the secondary directional stick to spray in a given direction.  
	/// </remarks>
	public partial class formMain : Form
	{
		#region Variables.
		private GorgonInputService _service;					// Our service factory.
		private IList<GorgonJoystick> _joystick;				// Our XBox controllers.
		private PointF[] _stickPosition;						// Current stick position point for the spray.
		private SprayCan[] _sprayStates;					    // Spray states.
		private DrawingSurface _surface;						// Surface to draw on.
		#endregion

		#region Properties.
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
		/// <param name="joystick">Joystick to update.</param>
		/// <param name="index">Index of the joystick.</param>
		private void UpdateControllerLabels(GorgonJoystick joystick, int index)
		{			
			var panel = (Panel)panelControllers.Controls["panelController" + index];
			var label = (Label)panel.Controls["labelController" + index];			

			// Update the label visibility for the controller.
			if (joystick.IsConnected)
			{
				if (!panel.Visible)
				{
					panel.Visible = true;
				}
			}
			else
			{
				label.Text = string.Format("{0} ({1})", joystick.Name, _service.JoystickDevices[joystick.Name].ClassName);

				// Turn off the other ones since we don't want to clutter 
				// up the screen.
				if ((index > 0) && (panel.Visible))
				{
					panel.Visible = false;
				}				
				return;
			}			

			// Set the label information.
			label.Text = string.Format("{0} ({7}): Stick 1 Position {1}x{2}. Stick 2 Position {3}x{4}. Right Trigger Position {5}. Left Trigger Position {6}.",
									joystick.Name,
									joystick.X,
									joystick.Y,
									joystick.SecondaryX,
									joystick.SecondaryY,
									joystick.Throttle,
									joystick.Rudder,
									_service.JoystickDevices[joystick.Name].ClassName);
		}

	    /// <summary>
	    /// Function to draw a spray on the screen.
	    /// </summary>
	    /// <param name="joystick">Joystick to use for the spray.</param>
	    /// <param name="index">Index of the joystick.</param>
	    private void DrawSpray(GorgonJoystick joystick, int index)
		{
			SprayCan state = _sprayStates[index];

	        // Update the origin of the spray.
			state.Origin = _stickPosition[index];

			// Find out if we're spraying.
			if (joystick.Throttle > joystick.Capabilities.ThrottleAxisRange.Minimum) 
			{
				if ((!state.IsActive) && (!state.NeedReset))
				{
					// Convert the throttle value to a unit value.
					float throttleUnit = ((float)(joystick.Throttle - joystick.Capabilities.ThrottleAxisRange.Minimum) / joystick.Capabilities.ThrottleAxisRange.Range);

					// Set up the spray state.
					state.Position = state.Origin;
					state.Amount = joystick.Capabilities.ThrottleAxisRange.Range / 2.0f;
					state.Time = throttleUnit * 10.0f;
					state.VibrationAmount = joystick.Capabilities.VibrationMotorRanges[1].Maximum;
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
		/// Function to draw the joystick cursor.
		/// </summary>
		/// <param name="joystick">Joystick for the cursor.</param>
		/// <param name="index">Index of the joystick.</param>
		private void DrawJoystickCursor(GorgonJoystick joystick, int index)
		{
			var playerColorValue = (int)((uint)0xFF << (index * 8) | 0xFF000000);						// Get the color based on the joystick index.			
			var cursorSize = new Size(_surface.CursorSize.Width / 2, _surface.CursorSize.Height / 2);	// Get the cursor size with offset.

			// Transform the axis into a -1 .. 1 range.				
			var moveVector = new PointF(joystick.X - (float)joystick.Capabilities.XAxisRange.Minimum,
											joystick.Y - (float)joystick.Capabilities.YAxisRange.Minimum);

			moveVector = new PointF((moveVector.X / (joystick.Capabilities.XAxisRange.Range + 1) * 2.0f) - 1.0f,
									(moveVector.Y / (joystick.Capabilities.YAxisRange.Range + 1) * 2.0f) - 1.0f);

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
		/// Function to process idle time in the application.
		/// </summary>
		/// <returns><b>true</b> to continue processing, <b>false</b> to end processing.</returns>
		private bool Idle()
		{
			_surface.Clear(Color.White);			

			for (int i = 0; i < _joystick.Count; i++)
			{				
				var joystick = _joystick[i];

				joystick.Poll();

				// Do nothing if this joystick isn't connected.
				if (!joystick.IsConnected)
				{
					continue;
				}

				// Clear the drawing if the primary button is clicked.
				if (joystick.Button[0].IsPressed)
				{
					_surface.ClearDrawing();
				}

				// Draw the joystick cursor.
				DrawJoystickCursor(joystick, i);

				// Begin drawing.	
				DrawSpray(joystick, i);

				UpdateControllerLabels(joystick, i);
			}

			_surface.Render();

			return true;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			IGorgonPluginAssemblyCache assemblies = null;

			base.OnLoad(e);

			try
			{
				// Create the assembly cache.
				assemblies = new GorgonPluginAssemblyCache(GorgonApplication.Log);
				assemblies.Load(Program.PlugInPath + "Gorgon.Input.XInput.dll");

				// Create the plug services.
				IGorgonPluginService pluginService = new GorgonPluginService(assemblies, GorgonApplication.Log);

				// Create the input service factory.
				GorgonInputServiceFactory factory = new GorgonInputServiceFactory(pluginService, GorgonApplication.Log);

				// Create our factory.
				_service = factory.CreateService("Gorgon.Input.GorgonXInputPlugIn");

				// Ensure that we have and XBox controller to work with.
				if (_service.JoystickDevices.Count == 0)
				{
					GorgonDialogs.ErrorBox(this, "No XBox controllers were found on this system.\nThis example requires an XBox controller.");
					GorgonApplication.Quit();
					return;
				}

				// Enumerate the active joysticks.  We'll only take 3 of the 4 available xbox controllers.
				_joystick = _service.JoystickDevices.Take(3).Select(item => _service.CreateJoystick(this, item.Name)).ToArray();
				_stickPosition = new PointF[_joystick.Count];
				_sprayStates = new SprayCan[_joystick.Count];

				for (int i = 0; i < _joystick.Count; i++)
				{
					var joystick = _joystick[i];

					// Set a dead zone on the joystick.
					// A dead zone will stop input from the joystick until it reaches the outside
					// of the specified coordinates.
					_joystick[i].DeadZone.X = new GorgonRange(joystick.Capabilities.XAxisRange.Minimum / 4, joystick.Capabilities.XAxisRange.Maximum / 4);
					_joystick[i].DeadZone.Y = new GorgonRange(joystick.Capabilities.YAxisRange.Minimum / 4, joystick.Capabilities.YAxisRange.Maximum / 4);
					_joystick[i].DeadZone.SecondaryX = new GorgonRange(joystick.Capabilities.XAxisRange.Minimum / 128, joystick.Capabilities.XAxisRange.Maximum / 128);
					_joystick[i].DeadZone.SecondaryY = new GorgonRange(joystick.Capabilities.YAxisRange.Minimum / 128, joystick.Capabilities.YAxisRange.Maximum / 128);

					// Start at a random spot.					
					_stickPosition[i] = new Point(GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Width - 64),
					                              GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Height - 64));

					// Turn off spray for all controllers.
					_sprayStates[i] = new SprayCan(joystick, i);
				}

				// Check for connected controllers.
				while (!_joystick.Any(item => item.IsConnected))
				{
					if (MessageBox.Show(this,
					                    "There are no XBox controllers connected.\nPlease plug in an XBox controller and click OK.",
					                    "No Controllers",
					                    MessageBoxButtons.OKCancel,
					                    MessageBoxIcon.Warning) != DialogResult.Cancel)
					{
						continue;
					}

					GorgonApplication.Quit();
					return;
				}

				// Get the graphics interface for our panel.
				_surface = new DrawingSurface(panelDisplay);

				// Set up our idle loop.
				GorgonApplication.ApplicationIdleLoopMethod += Idle;
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
				if (assemblies != null)
				{
					assemblies.Dispose();
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs" /> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

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
