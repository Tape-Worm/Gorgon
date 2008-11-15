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
// Created: Thursday, October 12, 2006 3:53:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Object representing a list of joysticks.
	/// </summary>
	public abstract class JoystickList
		: BaseCollection<Joystick>, IDisposable
	{
		#region Variables.
		private Input _owner = null;		// Owning input interface.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the joystick by index.
		/// </summary>
		public Joystick this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return the joystick by name.
		/// </summary>
		public Joystick this[string key]
		{
			get
			{
				return GetItem(key);
			}
		}

		/// <summary>
		/// Property to return the owning input interface.
		/// </summary>
		public Input Owner
		{
			get
			{
				return _owner;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear the list.
		/// </summary>
		protected override void ClearItems()
		{
			// Destroy the joysticks.
			foreach (Joystick stick in this)
				stick.Dispose();

 			base.ClearItems();
		}

		/// <summary>
		/// Function to enumerate joysticks.
		/// </summary>
		public abstract void Refresh();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Owning input interface.</param>
		protected internal JoystickList(Input owner)
			: base(4, true)
		{
			_owner = owner;
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
				ClearItems();
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
