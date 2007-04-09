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
// Created: Saturday, September 30, 2006 10:00:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Forms = System.Windows.Forms;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Input
{
	/// <summary>
	/// Abstract class for the input interface.
	/// </summary>
	public abstract class InputDevices
		: IDisposable, IPlugIn
	{
		#region Variables.
		private bool _bindMouse = false;						// Flag to indicate whether we want to bind the mouse or not.
		private bool _bindKeyboard = false;						// Flag to indicate whether we want to bind the keyboard or not.
		private bool _bindJoysticks = false;					// Flag to indicate whether we want to bind the joysticks or not.
		private Forms.Control _owner = null;					// Window that will own this input instance.
		private PlugInEntryPoint _plugInEntry = null;			// Plug in entry point.
		private bool _bindForm = false;							// We're bound to the form.

		/// <summary>Joystick list.</summary>
		protected JoystickList _joysticks = null;
		/// <summary>Interface for mouse data.</summary>
		protected Mouse _mouse = null;
		/// <summary>Interface for keyboard data.</summary>
		protected Keyboard _keyboard = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether we want to bind the mouse or not.
		/// </summary>
		public bool MouseEnabled
		{
			get
			{
				return _bindMouse;
			}
			set
			{
				_bindMouse = value;
				BindMouse();
			}
		}

		/// <summary>
		/// Property to set or return whether we want to bind the keyboard or not.
		/// </summary>
		public bool KeyboardEnabled
		{
			get
			{
				return _bindKeyboard;
			}
			set
			{
				_bindKeyboard = value;
				BindKeyboard();
			}
		}

		/// <summary>
		/// Property to set or return whether we want to bind joystick information or not.
		/// </summary>
		public bool JoysticksEnabled
		{
			get
			{
				return _bindJoysticks;
			}
			set
			{
				_bindJoysticks = value;
				BindJoysticks();
			}
		}

		/// <summary>
		/// Property to return the window that we're bound with.
		/// </summary>
		public Forms.Control Window
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Property to return the mouse interface.
		/// </summary>
		public Mouse Mouse
		{
			get
			{
				return _mouse;
			}
		}

		/// <summary>
		/// Property to return the keyboard interface.
		/// </summary>
		public Keyboard Keyboard
		{
			get
			{
				return _keyboard;
			}
		}

		/// <summary>
		/// Property to return the list of joysticks.
		/// </summary>
		public JoystickList Joysticks
		{
			get
			{
				return _joysticks;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the LostFocus event of the owner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void owner_LostFocus(object sender, EventArgs e)
		{
			if ((_mouse != null) && (_mouse.Exclusive))
				_mouse.Release();
			if ((_keyboard != null) && (_keyboard.Exclusive))
				_keyboard.Release();
		}

		/// <summary>
		/// Function to bind the mouse.
		/// </summary>
		protected abstract void BindMouse();

		/// <summary>
		/// Function to bind the keyboard.
		/// </summary>
		protected abstract void BindKeyboard();

		/// <summary>
		/// Function to bind joysticks.
		/// </summary>
		protected abstract void BindJoysticks();

		/// <summary>
		/// Function to initialize the input interface.
		/// </summary>
		/// <param name="control">Control to bind to the interface.</param>
		public virtual void Initialize(Forms.Control control)
		{			
			_owner = control;
			_owner.LostFocus += new EventHandler(owner_LostFocus);
			_bindForm = true;			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="entry">Plug-in entry wrapper.</param>
		public InputDevices(PlugInEntryPoint entry)
		{
			_plugInEntry = entry;
			_bindForm = false;
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~InputDevices()
		{
			Dispose(false);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_joysticks != null)
					_joysticks.Dispose();

				if (_mouse != null)
					_mouse.Dispose();

				if (_keyboard != null)
					_keyboard.Dispose();				

				if (_bindForm)
					_owner.LostFocus -= new EventHandler(owner_LostFocus);
			}

			_joysticks = null;
			_mouse = null;
			_keyboard = null;
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region IPlugIn Members
		/// <summary>
		/// Property to return the plug-in entry associated with this object.
		/// </summary>
		/// <value></value>
		PlugInEntryPoint IPlugIn.PlugInEntryPoint
		{
			get
			{
				return _plugInEntry;
			}
		}
		#endregion
	}
}
