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

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Abstract class for the input interface.
	/// </summary>
	public abstract class Input
		: IDisposable
    {
        #region Variables.
        private Forms.Control _owner = null;					// Window that will own this input instance.
		private bool _bindForm;									// We're bound to the form.
		private InputPlugIn _plugIn = null;						// Plug-in that this object is created from.
		private bool _disposed;									// Flag to indicate that the object is disposed.
		private JoystickList _joysticks = null;					// Joystick list.
		private Mouse _mouse = null;							// Interface for mouse data.
		private Keyboard _keyboard = null;						// Interface for keyboard data.
#if DEBUG
		private static bool _requireSigned = false;				// Flag to require a signed plug-in.
#else
		private static bool _requireSigned = true;				// Flag to require a signed plug-in.
#endif
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the plug-in that created this object.
		/// </summary>
		protected InputPlugIn PlugIn
		{
			get
			{
				return _plugIn;
			}
		}

		/// <summary>
		/// Property to set or return whether the input plug-in requires signing.
		/// </summary>
		public static bool RequireSignedPlugIn
		{
			get
			{
				return _requireSigned;
			}
			set
			{
				_requireSigned = value;
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
		/// Function to create the keyboard interface.
		/// </summary>
		/// <returns>A new keyboard interface.</returns>
		protected abstract Keyboard CreateKeyboard();

		/// <summary>
		/// Function to create the mouse interface.
		/// </summary>
		/// <returns>A new mouse interface.</returns>
		protected abstract Mouse CreateMouse();

		/// <summary>
		/// Function to create the list of joystick interfaces.
		/// </summary>
		/// <returns>A new list of joystick interfaces.</returns>
		protected abstract JoystickList CreateJoystickList();

		/// <summary>
		/// Handles the LostFocus event of the owner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void Owner_LostFocus(object sender, EventArgs e)
		{
			if ((_mouse != null) && (_mouse.Exclusive))
				_mouse.Acquired = false;
			if ((_keyboard != null) && (_keyboard.Exclusive))
				_keyboard.Acquired = false;
		}

		/// <summary>
		/// Handles the MouseLeave event of the owner control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected void Owner_MouseLeave(object sender, EventArgs e)
		{
			// If we're not exclusive and we leave the control, we should reset
			// the button status or else the button(s) will remain in a pressed
			// state upon re-entry.  Regardless of whether a button is physically
			// pressed or not.
			if ((_mouse != null) && (!_mouse.Exclusive))
				_mouse.ResetButtons();
		}

		/// <summary>
		/// Function to load an input plug-in.
		/// </summary>
		/// <param name="plugInPath">Path to the plug-in DLL.</param>
		/// <param name="plugInName">Name of the plug-in to instance.</param>
		public static Input LoadInputPlugIn(string plugInPath, string plugInName)
		{
			InputPlugIn plugIn = null;			// Plug-in.			

			if (string.IsNullOrEmpty(plugInPath))
				throw new ArgumentNullException("plugInPath");

			if (string.IsNullOrEmpty(plugInName))
				throw new ArgumentNullException("plugInName");

			plugIn = PlugInFactory.Load(plugInPath, plugInName, _requireSigned) as InputPlugIn;

			if ((plugIn == null) || (plugIn.PlugInType != PlugInType.Input))
				throw new InputPlugInInvalidType(plugInPath);

			return plugIn.CreateImplementation(null) as Input;
		}

		/// <summary>
		/// Function to bind the input interface to a specific control.
		/// </summary>
		/// <param name="control">Control to bind to the interface.</param>
		public void Bind(Forms.Control control)
		{
			if (_owner != null)
			{
				_owner.LostFocus -= new EventHandler(Owner_LostFocus);
				_owner.MouseLeave -= new EventHandler(Owner_MouseLeave);

				// Disable all devices.
				if (_mouse != null)
				{
					_mouse.Dispose();
					_mouse = null;
				}

				if (_keyboard != null)
				{
					_keyboard.Dispose();
					_keyboard = null;
				}

				// Disable all joysticks.
				if ((_joysticks != null) && (_joysticks.Count > 0))
				{
					_joysticks.Dispose();
					_joysticks = null;
				}
			}

			_owner = control;
			if (control != null)
			{				
				_owner.LostFocus += new EventHandler(Owner_LostFocus);
				_owner.MouseLeave += new EventHandler(Owner_MouseLeave);
				_bindForm = true;

				if (_mouse == null)
					_mouse = CreateMouse();

				if (_keyboard == null)
					_keyboard = CreateKeyboard();

				if (_joysticks == null)
				{
					_joysticks = CreateJoystickList();
					if (_joysticks != null)
						_joysticks.Refresh();
				}
			}				
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugIn">Plug-in that created this object.</param>
		protected Input(InputPlugIn plugIn)
		{
			if (plugIn == null)
				throw new ArgumentNullException("plugIn");
			_plugIn = plugIn;
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
				if (!_disposed)
				{
					if (_joysticks != null)
						_joysticks.Dispose();

					if (_mouse != null)
						_mouse.Dispose();

					if (_keyboard != null)
						_keyboard.Dispose();

					if (_bindForm)
					{
						_owner.LostFocus -= new EventHandler(Owner_LostFocus);
						_owner.MouseLeave -= new EventHandler(Owner_MouseLeave);
					}

					// Unload this plug-in.
					_plugIn.Unload();
				}
				_disposed = true;
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
	}
}
