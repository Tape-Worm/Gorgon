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
using SharpUtilities.Native.Win32;
using GorgonLibrary;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Input
{
	/// <summary>
	/// Object representing the main interface to the input library.
	/// </summary>
	public class GorgonInput
		: InputDevices
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
					RAWINPUT input = new RAWINPUT();					// Raw input data.
					int dataSize = Marshal.SizeOf(typeof(RAWINPUT));	// Size of raw input data.
					int result = 0;										// Result from the raw data capture.

					// Get the data size.
					result = Win32API.GetRawInputData(m.LParam, RawInputCommand.Input, null, ref dataSize, Marshal.SizeOf(typeof(RAWINPUTHEADER)));
					if (result == -1)
						throw new CannotCaptureRawInputException("Error retrieving the size of the data.", null);

					// Get the data.
					result = Win32API.GetRawInputData(m.LParam, RawInputCommand.Input, out input, ref dataSize, Marshal.SizeOf(typeof(RAWINPUTHEADER)));
					if (result == -1)
						throw new CannotCaptureRawInputException("Error retrieving the raw input data.", null);
					if (result != dataSize)
						throw new CannotCaptureRawInputException("Data size mismatch while retrieving the data.", null);

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
		/// <param name="e">The <see cref="GorgonLibrary.Input.RawInputEventArgs"/> instance containing the event data.</param>
		private void messageFilter_RawInputData(object sender, RawInputEventArgs e)
		{
			if ((_mouse.Exclusive) && (!_mouse.Acquired))
			{
				// Attempt to recapture.
				if (Window.Focused)
					_mouse.Acquire();
				else
					return;
			}

			if ((_keyboard.Exclusive) && (!_keyboard.Acquired))
			{
				// Attempt to recapture.
				if (Window.Focused)
					_keyboard.Acquire();
				else
					return;
			}

			// Determine type of device.
			switch (e.Data.Header.Type)
			{
				case RawInputType.Mouse:
					if ((_mouse != null) && (_mouse.Bound) && (_mouse.Acquired))
						((GorgonMouse)_mouse).GetRawData(e.Data.Mouse);
					break;
				case RawInputType.Keyboard:
					if ((_keyboard != null) && (_keyboard.Bound) && (_keyboard.Acquired))
						((GorgonKeyboard)_keyboard).GetRawData(e.Data.Keyboard);
					break;
			}
		}

		/// <summary>
		/// Handles the LostFocus event of the owner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void owner_LostFocus(object sender, EventArgs e)
		{
			base.owner_LostFocus(sender, e);

			// Release joysticks.
			if (_joysticks != null)
			{
				foreach (GorgonJoystick stick in _joysticks)
					stick.DeviceLost = true;
			}
		}

		/// <summary>
		/// Function to bind the mouse
		/// </summary>
		protected override void BindMouse()
		{
			if (!MouseEnabled)
			{
				// Disable the mouse.
				((GorgonMouse)_mouse).Unbind();
				return;
			}

			// Bind the mouse.
			_mouse = new GorgonMouse(this);
			((GorgonMouse)_mouse).Bind();
		}

		/// <summary>
		/// Function to bind the keyboard.
		/// </summary>
		protected override void BindKeyboard()
		{
			if (!KeyboardEnabled)
			{
				// Disable the mouse.
				((GorgonKeyboard)_keyboard).Unbind();
				return;
			}

			// Bind the mouse.
			_keyboard = new GorgonKeyboard(this);
			((GorgonKeyboard)_keyboard).Bind();			
		}
		
		/// <summary>
		/// Function to bind joysticks.
		/// </summary>
		protected override void BindJoysticks()
		{
			if (!JoysticksEnabled) 
			{
				if (_joysticks != null)
				{
					// Bind each stick.
					foreach (GorgonJoystick stick in _joysticks)
						stick.Unbind();

					// Destroy the list.
					_joysticks.Dispose();
					_joysticks = null;
				}				
				return;
			}

			// Create list.
			if (_joysticks == null)
				_joysticks = new GorgonJoystickList(this);

			// Bind each stick.
			foreach (GorgonJoystick stick in _joysticks)
				stick.Bind();
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
				if (_joysticks != null)
				{
					foreach (GorgonJoystick stick in _joysticks)
						stick.Unbind();
				}

				// Disable mouse.
				if (_mouse != null)
					MouseEnabled = false;
				if (_keyboard != null)
					KeyboardEnabled = false;

				_messageFilter.RawInputData -= new RawInputEventHandler(messageFilter_RawInputData);

				// Remove the filter.
				Forms.Application.RemoveMessageFilter(_messageFilter);
			}			

			_messageFilter = null;
			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to initialize the input interface.
		/// </summary>
		/// <param name="control">Control to bind to the interface.</param>
		public override void Initialize(Forms.Control control)
		{			
			base.Initialize(control);

			// Enable the mouse.
			MouseEnabled = true;
			KeyboardEnabled = true;
			JoysticksEnabled = true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="entry">Plug entry point wrapper.</param>
		public GorgonInput(PlugInEntryPoint entry)
			: base(entry)
		{
			// Add the message filter.
			_messageFilter = new MessageFilter();
			_messageFilter.RawInputData += new RawInputEventHandler(messageFilter_RawInputData);
			Forms.Application.AddMessageFilter(_messageFilter);
		}
		#endregion
	}
}
