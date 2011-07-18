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

namespace GorgonLibrary.HID.WinFormsInput
{
	/// <summary>
	/// Winforms mouse interface.
	/// </summary>
	public class WinFormsPointingDevice
		: GorgonPointingDevice
	{
		#region Properties.

		#endregion

		#region Methods.		
		/// <summary>
		/// Function to set the visibility of the mouse cursor.
		/// </summary>
		/// <param name="bShow">TRUE to show, FALSE to hide.</param>
		/// <returns>-1 if no mouse is installed, 0 or greater for the number of times this function has been called with TRUE.</returns>
		[System.Runtime.InteropServices.DllImport("User32.dll"), System.Security.SuppressUnmanagedCodeSecurity()]
		private static extern int ShowCursor(bool bShow);

		/// <summary>
		/// Handles the MouseDoubleClick event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseDoubleClick(object sender, Forms.MouseEventArgs e)
		{
			GetMouseData(e, true, false);
		}

		/// <summary>
		/// Handles the MouseUp event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseUp(object sender, Forms.MouseEventArgs e)
		{
			GetMouseData(e, false, false);
		}

		/// <summary>
		/// Handles the MouseDown event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseDown(object sender, Forms.MouseEventArgs e)
		{
			GetMouseData(e, false, true);
		}

		/// <summary>
		/// Handles the MouseMove event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseMove(object sender, Forms.MouseEventArgs e)
		{
			GetMouseData(e, false, false);
		}

		/// <summary>
		/// Handles the MouseWheel event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void BoundWindow_MouseWheel(object sender, Forms.MouseEventArgs e)
		{
			GetMouseData(e, false, false);
		}

		/// <summary>
		/// Function to retrieve the mouse data.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		/// <param name="doubleClick">TRUE if the event is from a double click, FALSE if not.</param>
		/// <param name="down">TRUE if the mouse button is down, FALSE if not.</param>
		private void GetMouseData(Forms.MouseEventArgs e, bool doubleClick, bool down)
		{			 
			if ((BoundWindow == null) || (BoundWindow.Disposing))
				return;

			Button = PointingDeviceButtons.None;
			RelativePosition = Vector2D.Subtract(e.Location, Position);
			WheelDelta = 0;

			if (Position != e.Location)
			{
				Position = e.Location;
				OnPointingDeviceMove();
			}

			if (e.Delta != 0)
			{
				Wheel += e.Delta;
				WheelDelta = e.Delta;
				OnPointingDeviceWheelMove();
			}

			if ((e.Button & Forms.MouseButtons.Left) == Forms.MouseButtons.Left)
				Button |= PointingDeviceButtons.Button1 | PointingDeviceButtons.Left;
			if ((e.Button & Forms.MouseButtons.Right) == Forms.MouseButtons.Right)
				Button |= PointingDeviceButtons.Button2 | PointingDeviceButtons.Right;
			if ((e.Button & Forms.MouseButtons.Middle) == Forms.MouseButtons.Middle)
				Button |= PointingDeviceButtons.Button3 | PointingDeviceButtons.Middle;
			if ((e.Button & Forms.MouseButtons.XButton1) == Forms.MouseButtons.XButton1)
				Button |= PointingDeviceButtons.Button4;
			if ((e.Button & Forms.MouseButtons.XButton2) == Forms.MouseButtons.XButton2)
				Button |= PointingDeviceButtons.Button5;

			if ((e.Button != Forms.MouseButtons.None) && (!doubleClick))
			{
				if (down)
					OnPointingDeviceDown(Button);
				else
					OnPointingDeviceUp(Button, e.Clicks);
			}

			if (doubleClick)
				OnPointingDeviceUp(Button, 2);
		}

		/// <summary>
		/// Function to return whether a pointing device click is a double click or not.
		/// </summary>
		/// <param name="button">Button used for double click.</param>
		/// <returns>
		/// TRUE if it is a double click, FALSE if not.
		/// </returns>
		protected override bool IsDoubleClick(PointingDeviceButtons button)
		{
			// No need in winforms.
			return false;
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
			BoundWindow.MouseMove += new Forms.MouseEventHandler(BoundWindow_MouseMove);
			BoundWindow.MouseDown += new Forms.MouseEventHandler(BoundWindow_MouseDown);
			BoundWindow.MouseUp += new Forms.MouseEventHandler(BoundWindow_MouseUp);
			BoundWindow.MouseDoubleClick += new Forms.MouseEventHandler(BoundWindow_MouseDoubleClick);
			BoundWindow.MouseWheel += new Forms.MouseEventHandler(BoundWindow_MouseWheel);
		}

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected override void UnbindDevice()
		{
			BoundWindow.MouseMove -= new Forms.MouseEventHandler(BoundWindow_MouseMove);
			BoundWindow.MouseDown -= new Forms.MouseEventHandler(BoundWindow_MouseDown);
			BoundWindow.MouseUp -= new Forms.MouseEventHandler(BoundWindow_MouseUp);
			BoundWindow.MouseDoubleClick -= new Forms.MouseEventHandler(BoundWindow_MouseDoubleClick);
			BoundWindow.MouseWheel -= new Forms.MouseEventHandler(BoundWindow_MouseWheel);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WinFormsPointingDevice"/> class.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		/// <param name="boundWindow">The window to bind this device with.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the owner parameter is NULL (or Nothing in VB.NET).</exception>
		/// <remarks>Pass NULL (Nothing in VB.Net) to the <paramref name="boundWindow"/> parameter to use the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">Gorgon application window</see>.</remarks>
		internal WinFormsPointingDevice(GorgonWinFormsInputDeviceFactory owner, Forms.Control boundWindow)
			: base(owner, "Win Forms Mouse", boundWindow)
		{			
			Gorgon.Log.Print("Raw input pointing device interface created.", GorgonLoggingLevel.Verbose);

			// Default the position to the current screen position.
			Position = BoundWindow.PointToClient(Forms.Cursor.Position);
		}
		#endregion
	}
}
