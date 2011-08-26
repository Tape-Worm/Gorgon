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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Forms = System.Windows.Forms;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Math;

namespace GorgonLibrary.Input.WinForms
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
		/// Function to set the visibility of the pointing device cursor.
		/// </summary>
		/// <param name="bShow">TRUE to show, FALSE to hide.</param>
		/// <returns>-1 if no pointing device is installed, 0 or greater for the number of times this function has been called with TRUE.</returns>
		[System.Runtime.InteropServices.DllImport("User32.dll"), System.Security.SuppressUnmanagedCodeSecurity()]
		private static extern int ShowCursor(bool bShow);

		/// <summary>
		/// Handles the MouseDoubleClick event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseDoubleClick(object sender, Forms.MouseEventArgs e)
		{
			GetMouseData(e, true, ButtonState.None);
		}

		/// <summary>
		/// Handles the MouseUp event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseUp(object sender, Forms.MouseEventArgs e)
		{
			GetMouseData(e, false, ButtonState.Up);
		}

		/// <summary>
		/// Handles the MouseDown event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseDown(object sender, Forms.MouseEventArgs e)
		{
			GetMouseData(e, false, ButtonState.Down);
		}

		/// <summary>
		/// Handles the MouseMove event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseMove(object sender, Forms.MouseEventArgs e)
		{
			GetMouseData(e, false, (e.Button != Forms.MouseButtons.None ? ButtonState.Down : ButtonState.None));
		}

		/// <summary>
		/// Handles the MouseWheel event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseWheel(object sender, Forms.MouseEventArgs e)
		{
			GetMouseData(e, false, ButtonState.None);
		}

		/// <summary>
		/// Function to retrieve the pointing device data.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		/// <param name="doubleClick">TRUE if the event is from a double click, FALSE if not.</param>
		/// <param name="state">The state for the button event.</param>
		private void GetMouseData(Forms.MouseEventArgs e, bool doubleClick, ButtonState state)
		{			 
			if ((BoundControl == null) || (BoundControl.Disposing)) 
				return;
						
			OnPointingDeviceWheelMove(e.Delta);

			// Get button data.
			if ((e.Button & Forms.MouseButtons.Left) == Forms.MouseButtons.Left)
			{
				if (state == ButtonState.Down)
					OnPointingDeviceDown(PointingDeviceButtons.Left);
				if ((state == ButtonState.Up) || (doubleClick))
					OnPointingDeviceUp(PointingDeviceButtons.Left, e.Clicks);
			}

			if ((e.Button & Forms.MouseButtons.Right) == Forms.MouseButtons.Right)
			{
				if (state == ButtonState.Down)
					OnPointingDeviceDown(PointingDeviceButtons.Right);
				if ((state == ButtonState.Up) || (doubleClick))
					OnPointingDeviceUp(PointingDeviceButtons.Right, e.Clicks);
			}

			if ((e.Button & Forms.MouseButtons.Middle) == Forms.MouseButtons.Middle)
			{
				if (state == ButtonState.Down)
					OnPointingDeviceDown(PointingDeviceButtons.Middle);
				if ((state == ButtonState.Up) || (doubleClick))
					OnPointingDeviceUp(PointingDeviceButtons.Middle, e.Clicks);
			}
			if ((e.Button & Forms.MouseButtons.XButton1) == Forms.MouseButtons.XButton1)
			{
				if (state == ButtonState.Down)
					OnPointingDeviceDown(PointingDeviceButtons.Button4);
				if ((state == ButtonState.Up) || (doubleClick))
					OnPointingDeviceUp(PointingDeviceButtons.Button4, e.Clicks);
			}
			if ((e.Button & Forms.MouseButtons.XButton2) == Forms.MouseButtons.XButton2)
			{
				if (state == ButtonState.Down)
					OnPointingDeviceDown(PointingDeviceButtons.Button5);
				if ((state == ButtonState.Up) || (doubleClick))
					OnPointingDeviceUp(PointingDeviceButtons.Button5, e.Clicks);
			}

			OnPointingDeviceMove(e.Location, true);
		}

		/// <summary>
		/// Function that will hide the cursor and rewind the cursor visibility stack.
		/// </summary>
		protected override void ResetCursor()
		{
			int count = 0;

			count = ShowCursor(false);

			// Turn off the cursor.
			while (count >= 0)
				count = ShowCursor(false);

			ShowCursor(true);
		}


		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected override void BindDevice()
		{
			BoundControl.MouseMove += new Forms.MouseEventHandler(BoundWindow_MouseMove);
			BoundControl.MouseDown += new Forms.MouseEventHandler(BoundWindow_MouseDown);
			BoundControl.MouseUp += new Forms.MouseEventHandler(BoundWindow_MouseUp);
			BoundControl.MouseDoubleClick += new Forms.MouseEventHandler(BoundWindow_MouseDoubleClick);
			// Bind this to the form because some controls won't have focus and won't be able to fire the event.
			BoundTopLevelForm.MouseWheel += new Forms.MouseEventHandler(BoundWindow_MouseWheel);
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			BoundControl.MouseMove -= new Forms.MouseEventHandler(BoundWindow_MouseMove);
			BoundControl.MouseDown -= new Forms.MouseEventHandler(BoundWindow_MouseDown);
			BoundControl.MouseUp -= new Forms.MouseEventHandler(BoundWindow_MouseUp);
			BoundControl.MouseDoubleClick -= new Forms.MouseEventHandler(BoundWindow_MouseDoubleClick);
			BoundTopLevelForm.MouseWheel -= new Forms.MouseEventHandler(BoundWindow_MouseWheel);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WinFormsPointingDevice"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="boundWindow">The window to bind this device with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see>.</remarks>
		internal WinFormsPointingDevice(GorgonWinFormsDeviceFactory owner, Forms.Control boundWindow)
			: base(owner, "Win Forms Mouse", boundWindow)
		{			
			Gorgon.Log.Print("Raw input pointing device interface created.", GorgonLoggingLevel.Verbose);

			// Default the position to the current screen position.
			Position = BoundControl.PointToClient(Forms.Cursor.Position);
		}
		#endregion
	}
}
