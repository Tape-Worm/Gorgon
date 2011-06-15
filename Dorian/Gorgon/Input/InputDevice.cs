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
		private bool _bound;				// Flag to indicate whether the device was bound.
		private bool _acquired;				// Flag to indicate whether the device is in an acquired state.
		private bool _exclusive;			// Flag to indicate whether the device has exclusive access to the device.
		private bool _background;			// Flag to indicate whether the device will poll in the background.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the input interface owner for this device.
		/// </summary>
		protected Input InputInterface
		{
			get;
			private set;
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
				_acquired = value;

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
				Acquired = value;

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
			InputInterface = owner;
		}
		#endregion
	}
}
