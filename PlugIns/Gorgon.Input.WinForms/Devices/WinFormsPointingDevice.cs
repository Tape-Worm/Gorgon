#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Monday, July 18, 2011 6:44:31 AM
// 
#endregion

using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;

namespace Gorgon.Input.WinForms
{
	/// <summary>
	/// Winforms pointing device interface.
	/// </summary>
	public class WinFormsPointingDevice
		: GorgonPointingDevice
	{
		#region Enumerations.
		/// <summary>
		/// Enumerate for pointing device state.
		/// </summary>
		private enum ButtonState
		{
			/// <summary>
			/// No button press event.
			/// </summary>
			None = 0,
			/// <summary>
			/// Down button press event.
			/// </summary>
			Down = 1,
			/// <summary>
			/// Up button press event.
			/// </summary>
			Up = 2
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the MouseDoubleClick event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			GetMouseData(e, true, ButtonState.None);
		}

		/// <summary>
		/// Handles the MouseUp event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseUp(object sender, MouseEventArgs e)
		{
			GetMouseData(e, false, ButtonState.Up);
		}

		/// <summary>
		/// Handles the MouseDown event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseDown(object sender, MouseEventArgs e)
		{
			GetMouseData(e, false, ButtonState.Down);
		}

		/// <summary>
		/// Handles the MouseMove event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseMove(object sender, MouseEventArgs e)
		{
			GetMouseData(e, false, (e.Button != MouseButtons.None ? ButtonState.Down : ButtonState.None));
		}

		/// <summary>
		/// Handles the MouseWheel event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseWheel(object sender, MouseEventArgs e)
		{
			GetMouseData(e, false, ButtonState.None);
		}

		/// <summary>
		/// Function to retrieve the pointing device data.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		/// <param name="doubleClick"><c>true</c> if the event is from a double click, <c>false</c> if not.</param>
		/// <param name="state">The state for the button event.</param>
		private void GetMouseData(MouseEventArgs e, bool doubleClick, ButtonState state)
		{
		    if ((BoundControl == null) || (BoundControl.Disposing))
		    {
		        return;
		    }

		    OnPointingDeviceWheelMove(e.Delta);

			// Get button data.
			if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
			{
			    if (state == ButtonState.Down)
			    {
			        OnPointingDeviceDown(PointingDeviceButtons.Left);
			    }

			    if ((state == ButtonState.Up) || (doubleClick))
			    {
			        OnPointingDeviceUp(PointingDeviceButtons.Left, e.Clicks);
			    }
			}

			if ((e.Button & MouseButtons.Right) == MouseButtons.Right)
			{
			    if (state == ButtonState.Down)
			    {
			        OnPointingDeviceDown(PointingDeviceButtons.Right);
			    }

			    if ((state == ButtonState.Up) || (doubleClick))
			    {
			        OnPointingDeviceUp(PointingDeviceButtons.Right, e.Clicks);
			    }
			}

			if ((e.Button & MouseButtons.Middle) == MouseButtons.Middle)
			{
			    if (state == ButtonState.Down)
			    {
			        OnPointingDeviceDown(PointingDeviceButtons.Middle);
			    }

			    if ((state == ButtonState.Up) || (doubleClick))
			    {
			        OnPointingDeviceUp(PointingDeviceButtons.Middle, e.Clicks);
			    }
			}
			if ((e.Button & MouseButtons.XButton1) == MouseButtons.XButton1)
			{
			    if (state == ButtonState.Down)
			    {
			        OnPointingDeviceDown(PointingDeviceButtons.Button4);
			    }

			    if ((state == ButtonState.Up) || (doubleClick))
			    {
			        OnPointingDeviceUp(PointingDeviceButtons.Button4, e.Clicks);
			    }
			}
			if ((e.Button & MouseButtons.XButton2) == MouseButtons.XButton2)
			{
			    if (state == ButtonState.Down)
			    {
			        OnPointingDeviceDown(PointingDeviceButtons.Button5);
			    }

			    if ((state == ButtonState.Up) || (doubleClick))
			    {
			        OnPointingDeviceUp(PointingDeviceButtons.Button5, e.Clicks);
			    }
			}

			OnPointingDeviceMove(e.Location, true);
		}

		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			BoundControl.MouseMove += BoundWindow_MouseMove;
			BoundControl.MouseDown += BoundWindow_MouseDown;
			BoundControl.MouseUp += BoundWindow_MouseUp;
			BoundControl.MouseDoubleClick += BoundWindow_MouseDoubleClick;
			// Bind this to the form because some controls won't have focus and won't be able to fire the event.
			BoundTopLevelForm.MouseWheel += BoundWindow_MouseWheel;
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			BoundControl.MouseMove -= BoundWindow_MouseMove;
			BoundControl.MouseDown -= BoundWindow_MouseDown;
			BoundControl.MouseUp -= BoundWindow_MouseUp;
			BoundControl.MouseDoubleClick -= BoundWindow_MouseDoubleClick;
			BoundTopLevelForm.MouseWheel -= BoundWindow_MouseWheel;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WinFormsPointingDevice"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		internal WinFormsPointingDevice(GorgonInputFactory owner)
			: base(owner, "Win Forms Mouse")
		{
			AllowExclusiveMode = false;
			GorgonApplication.Log.Print("Windows forms input pointing device interface created.", LoggingLevel.Verbose);
		}
		#endregion
	}
}
