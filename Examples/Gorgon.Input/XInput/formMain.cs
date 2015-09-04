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
		private GorgonInputService2 _service;					// Our service factory.
		private IList<GorgonJoystick2> _joystick;				// Our XBox controllers.
		private PointF[] _stickPosition;						// Current stick position point for the spray.
		private SprayCan[] _sprayStates;					    // Spray states.
		private DrawingSurface _surface;                        // Surface to draw on.
		#endregion

		#region Methods.
		/// <summary>
		/// Forms the main_ activated. Seriously GhostDoc??? What the fuck is this?
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
		private void formMain_Activated(object sender, EventArgs e)
		{
			foreach (GorgonJoystick2 joystick in _joystick.Where(item => item.IsConnected))
			{
				joystick.IsAcquired = true;
			}
		}

		/// <summary>
		/// Forms the main_ deactivate.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
		private void formMain_Deactivate(object sender, EventArgs e)
		{
			foreach (GorgonJoystick2 joystick in _joystick.Where(item => item.IsConnected))
			{
				joystick.IsAcquired = false;
			}
		}

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
		private void UpdateControllerLabels(GorgonJoystick2 joystick, int index)
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
				label.Text = $"{joystick.Info.Description} ({joystick.Info.ClassName})";

				// Turn off the other ones since we don't want to clutter 
				// up the screen.
				if ((index > 0) && (panel.Visible))
				{
					panel.Visible = false;
				}				
				return;
			}			

			// Set the label information.
			label.Text = $"{joystick.Info.Description} ({joystick.Info.ClassName}): " +
			             $"Stick 1 Position {joystick.Axis[JoystickAxis.XAxis].Value}x{joystick.Axis[JoystickAxis.YAxis].Value}. " +
			             $"Stick 2 Position {joystick.Axis[JoystickAxis.XAxis2].Value}x{joystick.Axis[JoystickAxis.YAxis2].Value}. " +
			             $"Right Trigger Position {joystick.Axis[JoystickAxis.RightTrigger].Value}. " +
			             $"Left Trigger Position {joystick.Axis[JoystickAxis.LeftTrigger].Value}.";
		}

	    /// <summary>
	    /// Function to draw a spray on the screen.
	    /// </summary>
	    /// <param name="joystick">Joystick to use for the spray.</param>
	    /// <param name="index">Index of the joystick.</param>
	    private void DrawSpray(GorgonJoystick2 joystick, int index)
		{
			SprayCan state = _sprayStates[index];

	        // Update the origin of the spray.
			state.Origin = _stickPosition[index];

			GorgonRange throttleRange = joystick.Info.AxisInfo[JoystickAxis.Throttle].Range;

			// Find out if we're spraying.
			if (joystick.Axis[JoystickAxis.Throttle].Value > throttleRange.Minimum)
			{
				if ((!state.IsActive) && (!state.NeedReset))
				{
					// Convert the throttle value to a unit value.
					float throttleUnit = ((float)(joystick.Axis[JoystickAxis.Throttle].Value - throttleRange.Minimum) / throttleRange.Range);

					// Set up the spray state.
					state.Position = state.Origin;
					state.Amount = throttleRange.Range / 2.0f;
					state.Time = throttleUnit * 10.0f;
					state.VibrationAmount = joystick.Info.VibrationMotorRanges[1].Maximum;
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
		private void DrawJoystickCursor(GorgonJoystick2 joystick, int index)
		{
			GorgonRange xRange = joystick.Info.AxisInfo[JoystickAxis.XAxis].Range;
			GorgonRange yRange = joystick.Info.AxisInfo[JoystickAxis.YAxis].Range;
			var playerColorValue = (int)((uint)0xFF << (index * 8) | 0xFF000000);						// Get the color based on the joystick index.			
			var cursorSize = new Size(_surface.CursorSize.Width / 2, _surface.CursorSize.Height / 2);	// Get the cursor size with offset.

			// Transform the axis into a -1 .. 1 range.				
			var moveVector = new PointF(joystick.Axis[JoystickAxis.XAxis].Value - (float)xRange.Minimum,
											joystick.Axis[JoystickAxis.YAxis].Value - (float)yRange.Minimum);

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
				if (joystick.Button[0] == JoystickButtonState.Down)
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
		/// Function to initialize the game pad and its dead zones.
		/// </summary>
		/// <param name="gamePad">The game pad to initialize.</param>
		/// <returns><b>true</b> if the game pad is connected and set up, <b>false</b> if not.</returns>
		private bool SetupGamePad(GorgonJoystick2 gamePad)
		{
			if (!gamePad.IsConnected)
			{
				return false;
			}
			
			GorgonRange xRange = gamePad.Info.AxisInfo[JoystickAxis.XAxis].Range;
			GorgonRange yRange = gamePad.Info.AxisInfo[JoystickAxis.YAxis].Range;
			GorgonRange x2Range = gamePad.Info.AxisInfo[JoystickAxis.XAxis2].Range;
			GorgonRange y2Range = gamePad.Info.AxisInfo[JoystickAxis.YAxis2].Range;

			// Set a dead zone on the game pad.
			// A dead zone will stop input from the game pad until it reaches the outside
			// of the specified coordinates.
			gamePad.Axis[JoystickAxis.XAxis].DeadZone = new GorgonRange(xRange.Minimum / 4, xRange.Maximum / 4);
			gamePad.Axis[JoystickAxis.YAxis].DeadZone = new GorgonRange(yRange.Minimum / 4, yRange.Maximum / 4);
			gamePad.Axis[JoystickAxis.XAxis2].DeadZone = new GorgonRange(x2Range.Minimum / 128, x2Range.Maximum / 128);
			gamePad.Axis[JoystickAxis.YAxis2].DeadZone = new GorgonRange(y2Range.Minimum / 128, y2Range.Maximum / 128);

			gamePad.BindWindow(this);

			return true;
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

				// Create the input service factory.
				GorgonInputServiceFactory2 factory = new GorgonInputServiceFactory2(pluginService, GorgonApplication.Log);

				// Create our factory.
				_service = factory.CreateService("Gorgon.Input.GorgonXInputPlugIn");

				IReadOnlyList<IGorgonJoystickInfo2> joysticks = _service.EnumerateJoysticks();

				// Ensure that we have an XBox controller to work with.
				if (joysticks.Count == 0)
				{
					GorgonDialogs.ErrorBox(this, "No XBox controllers were found on this system.\nThis example requires an XBox controller.");
					GorgonApplication.Quit();
					return;
				}

				// Enumerate the active joysticks.  We'll only take 3 of the 4 available xbox controllers.
				_joystick = joysticks.Take(3).Select(item => new GorgonJoystick2(_service, item, GorgonApplication.Log)).ToArray();
				_stickPosition = new PointF[_joystick.Count];
				_sprayStates = new SprayCan[_joystick.Count];

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

				for (int i = 0; i < _joystick.Count; i++)
				{
					if (!SetupGamePad(_joystick[i]))
					{
						continue;
					}

					// Start at a random spot.					
					_stickPosition[i] = new Point(GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Width - 64),
					                              GorgonRandom.RandomInt32(64, panelDisplay.ClientSize.Height - 64));

					// Turn off spray for all controllers.
					_sprayStates[i] = new SprayCan(_joystick[i], i);
				}

				// Get the graphics interface for our panel.
				_surface = new DrawingSurface(panelDisplay);

				// Set up our idle loop.
				GorgonApplication.IdleMethod += Idle;
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

			if (_joystick != null)
			{
				foreach (GorgonJoystick2 joystick in _joystick)
				{
					joystick.UnbindWindow();
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
