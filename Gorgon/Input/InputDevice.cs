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
// Created: Monday, October 02, 2006 1:10:07 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Abstract class for input devices.
	/// </summary>
	public abstract class InputDevice
	{
		#region Variables.
		private Input _owner;				// Input device interface owner.

		/// <summary>Flag to indicate that the device is bound.</summary>
		protected bool _bound;
		/// <summary>Flag to indicate that the device has been acquired.</summary>
		protected bool _acquired;
		/// <summary>Flag to indicate whether the owning window has exclusive access to the device.</summary>
		protected bool _exclusive;
		/// <summary>Flag to indicate whether this device will continue to send data even when the window is not focused.</summary>
		protected bool _background;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the input interface owner for this device.
		/// </summary>
		protected Input InputInterface
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Property to return whether the device is acquired or not.
		/// </summary>
		public virtual bool Acquired
		{
			get
			{
				return _acquired;
			}
			set
			{
				if (_bound)
				{
					if (value)
						BindDevice();
					else
						UnbindDevice();					
				}
			}
		}

		/// <summary>
		/// Property to set or return whether the window has exclusive access or not.
		/// </summary>
		public virtual bool Exclusive
		{
			get
			{
				return _exclusive;
			}
			set
			{
				_exclusive = value;
				if (_bound)
					BindDevice();
			}
		}

		/// <summary>
		/// Property to set or return whether to allow this device to keep sending data even if the window is not focused.
		/// </summary>
		public virtual bool AllowBackground
		{
			get
			{
				return _background;
			}
			set
			{
				_background = value;
				if (_bound)
					BindDevice();
			}
		}

		/// <summary>
		/// Property to return whether the device is bound or not.
		/// </summary>
		public bool Enabled
		{
			get
			{
				return _bound;
			}
			set
			{
				if (value)
					BindDevice();
				else
					UnbindDevice();

				_bound = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to bind the input device.
		/// </summary>
		protected abstract void BindDevice();

		/// <summary>
		/// Function to unbind the input device.
		/// </summary>
		protected abstract void UnbindDevice();

		/// <summary>
		/// Function to perform clean up on the object.
		/// </summary>
		protected internal virtual void Dispose()
		{
			if (_bound)
				UnbindDevice();
						
			_acquired = false;
			_bound = false;

			GC.SuppressFinalize(this);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">The control that owns this device.</param>
		protected internal InputDevice(Input owner)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");
			_owner = owner;
		}
		#endregion
	}
}
