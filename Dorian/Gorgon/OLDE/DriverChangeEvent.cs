#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
