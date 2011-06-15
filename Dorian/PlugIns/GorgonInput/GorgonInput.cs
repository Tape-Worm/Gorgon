#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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

			#region Variables.
			private RawInputData _data = null;					// Raw input data.
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
					if (_data == null)
						_data = new RawInputData();

					_data.GetRawInputData(m.LParam);
					if (RawInputData != null)
						RawInputData(this, new RawInputEventArgs(_data));
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
			if (Window.Disposing)
				return;

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
					GorgonMouse mouse = Mouse as GorgonMouse;

					if ((mouse != null) && (mouse.Enabled) && (mouse.Acquired))
						mouse.GetRawData(e.Data.Mouse);
					break;
				case RawInputType.Keyboard:
					GorgonKeyboard keyboard = Keyboard as GorgonKeyboard;
					
					if ((keyboard != null) && (keyboard.Enabled) && (keyboard.Acquired))
						keyboard.GetRawData(e.Data.Keyboard);
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
