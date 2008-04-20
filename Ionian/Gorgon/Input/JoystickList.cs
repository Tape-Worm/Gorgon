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
