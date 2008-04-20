#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Sunday, October 01, 2006 8:43:41 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Forms = System.Windows.Forms;
using GorgonLibrary;
using GorgonLibrary.PlugIns;
using GorgonLibrary.InputDevices.Internal;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Object representing the main interface to the input library.
	/// </summary>
	public class GorgonInput
		: Input
	{
		#region Classes.
		/// <summary>
		/// Object representing a message loop filter.
		/// </summary>
		internal class MessageFilter
			: Forms.IMessageFilter
		{
			#region Events.
			/// <summary>
			/// Event fired when a raw input event occours.
			/// </summary>
			public event RawInputEventHandler RawInputData = null;
			#endregion

			#region IMessageFilter Members
			/// <summary>
			/// Filters out a message before it is dispatched.
			/// </summary>
			/// <param name="m">The message to be dispatched. You cannot modify this message.</param>
			/// <returns>
			/// true to filter the message and stop it from being dispatched; false to allow the message to continue to the next filter or control.
			/// </returns>
			public bool PreFilterMessage(ref System.Windows.Forms.Message m)
			{
				// Handle raw input messages.
				if ((WindowMessages)m.Msg == WindowMessages.RawInput)
				{
#if PLATFORM_X64
					RAWINPUTx64 input = new RAWINPUTx64();				// Raw input data.
					int dataSize = Marshal.SizeOf(typeof(RAWINPUTx64));	// Size of raw input data.
#else
					RAWINPUT input = new RAWINPUT();					// Raw input data.
					int dataSize = Marshal.SizeOf(typeof(RAWINPUT));	// Size of raw input data.
#endif
					int result = 0;										// Result from the raw data capture.

					// Get the data size.
					result = Win32API.GetRawInputData(m.LParam, RawInputCommand.Input, null, ref dataSize, Marshal.SizeOf(typeof(RAWINPUTHEADER)));
					if (result == -1)
						throw new InputCannotCaptureRawDataException();

					// Get the data.
					result = Win32API.GetRawInputData(m.LParam, RawInputCommand.Input, out input, ref dataSize, Marshal.SizeOf(typeof(RAWINPUTHEADER)));
					if ((result == -1) || (result != dataSize))
						throw new InputCannotCaptureRawDataException();

					// Send the event back.
					if (RawInputData != null)
						RawInputData(this, new RawInputEventArgs(input));

					return false;
				}

				return false;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private MessageFilter _messageFilter = null;				// Message filter.
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the RawInputData event of the _messageFilter control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.InputDevices.RawInputEventArgs"/> instance containing the event data.</param>
		private void messageFilter_RawInputData(object sender, RawInputEventArgs e)
		{
			if ((Mouse.Exclusive) && (!Mouse.Acquired))
			{
				// Attempt to recapture.
				if (Window.Focused)
					Mouse.Acquired = true;
				else
					return;
			}

			if ((Keyboard.Exclusive) && (!Keyboard.Acquired))
			{
				// Attempt to recapture.
				if (Window.Focused)
					Keyboard.Acquired = true;
				else
					return;
			}

			// Determine type of device.
			switch (e.Data.Header.Type)
			{
				case RawInputType.Mouse:
					if ((Mouse != null) && (Mouse.Enabled) && (Mouse.Acquired))
						((GorgonMouse)Mouse).GetRawData(e.Data.Mouse);
					break;
				case RawInputType.Keyboard:
					if ((Keyboard != null) && (Keyboard.Enabled) && (Keyboard.Acquired))
						((GorgonKeyboard)Keyboard).GetRawData(e.Data.Keyboard);
					break;
			}
		}

		/// <summary>
		/// Function to create the list of joystick interfaces.
		/// </summary>
		/// <returns>A new list of joystick interfaces.</returns>
		protected override JoystickList CreateJoystickList()
		{
			return new GorgonJoystickList(this);	
		}

		/// <summary>
		/// Function to create the keyboard interface.
		/// </summary>
		/// <returns>A new keyboard interface.</returns>
		protected override Keyboard CreateKeyboard()
		{
			return new GorgonKeyboard(this);	
		}

		/// <summary>
		/// Function to create the mouse interface.
		/// </summary>
		/// <returns>A new mouse interface.</returns>
		protected override Mouse CreateMouse()
		{
			return new GorgonMouse(this);	
		}

		/// <summary>
		/// Handles the LostFocus event of the owner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void Owner_LostFocus(object sender, EventArgs e)
		{
			base.Owner_LostFocus(sender, e);

			// Release joysticks.
			if (Joysticks != null)
			{
				foreach (GorgonJoystick stick in Joysticks)
					stick.DeviceLost = true;
			}
		}
	
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to perform clean up of all objects.  FALSE to clean up only unmanaged objects.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Unbind the joysticks.
				if (Joysticks != null)
				{
					foreach (GorgonJoystick stick in Joysticks)
						stick.Enabled = false;
				}

				// Disable mouse.
				if (Mouse != null)
					Mouse.Enabled = false;
				if (Keyboard != null)
					Keyboard.Enabled = false;

				_messageFilter.RawInputData -= new RawInputEventHandler(messageFilter_RawInputData);

				// Remove the filter.
				Forms.Application.RemoveMessageFilter(_messageFilter);
			}			

			_messageFilter = null;
			base.Dispose(disposing);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="inputPlugIn">Plug-in that created this object.</param>
		public GorgonInput(InputPlugIn inputPlugIn)
			: base(inputPlugIn)
		{
			// Add the message filter.
			_messageFilter = new MessageFilter();
			_messageFilter.RawInputData += new RawInputEventHandler(messageFilter_RawInputData);

			Forms.Application.AddMessageFilter(_messageFilter);
		}
		#endregion
	}
}
