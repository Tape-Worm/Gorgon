#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Saturday, July 09, 2005 5:31:48 PM
// 
#endregion

using System;

namespace GorgonLibrary
{
	/// <summary>
	/// Event delegate called when the driver is changing.
	/// </summary>
	/// <param name="sender">Sender of this event.</param>
	/// <param name="e">Arguments for the event.</param>
	public delegate void DriverChangingHandler(object sender, DriverChangingArgs e);

	/// <summary>
	/// Event delegate called when the driver has changed.
	/// </summary>
	/// <param name="sender">Sender of this event.</param>
	/// <param name="e">Arguments for the event.</param>
	public delegate void DriverChangedHandler(object sender, DriverChangedArgs e);


	/// <summary>
	/// Arguments for driver change events.
	/// </summary>
	public class DriverChangedArgs 
		: EventArgs
	{
		#region Variables.
		private Driver _fromDriver;		// Index of the driver that we're changing from.		
		private Driver _toDriver;		// Index of the driver that we're changing to.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the driver we're changing from.
		/// </summary>
		public Driver FromDriver
		{
			get
			{
				return _fromDriver;
			}
		}

		/// <summary>
		/// Property to return the driver we're switching to.
		/// </summary>
		public Driver ToDriver
		{
			get
			{
				return _toDriver;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fromDriver">Driver we're changing from.</param>
		/// <param name="toDriver">Driver we're changing to.</param>
		public DriverChangedArgs(Driver fromDriver, Driver toDriver)
		{
			_fromDriver = fromDriver;
			_toDriver = toDriver;
		}
		#endregion

	}

	/// <summary>
	/// Arguments for driver change events.
	/// </summary>
	public class DriverChangingArgs 
		: DriverChangedArgs
	{
		#region Variables.				
		private bool _cancel;			// Flag to indicate that we should cancel this change.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether we should cancel the action or not.
		/// </summary>
		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="fromDriver">Driver we're changing from.</param>
		/// <param name="toDriver">Driver we're changing to.</param>
		public DriverChangingArgs(Driver fromDriver,Driver toDriver)
			: base(fromDriver, toDriver)
		{
		}
		#endregion
	}	
}
