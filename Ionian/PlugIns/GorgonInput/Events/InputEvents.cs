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
// Created: Sunday, October 01, 2006 8:50:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.InputDevices.Internal;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Object representing event arguments for the raw input events.
	/// </summary>
	internal class RawInputEventArgs
		: EventArgs
	{
		#region Variables.
#if PLATFORM_X64
		private RAWINPUTx64 _inputData;	// Raw input data.
#else
		private RAWINPUT _inputData;		// Raw input data.
#endif
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the raw input data.
		/// </summary>
#if PLATFORM_X64
		public RAWINPUTx64 Data
#else
		public RAWINPUT Data
#endif
		{
			get
			{
				return _inputData;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="input">Raw input data.</param>
#if PLATFORM_X64
		public RawInputEventArgs(RAWINPUTx64 input)
#else
		public RawInputEventArgs(RAWINPUT input)
#endif
		{
			_inputData = input;
		}
		#endregion
	}

	/// <summary>
	/// Delegate for a raw input event.
	/// </summary>
	/// <param name="sender">Object that sent the event.</param>
	/// <param name="e">Event arguments.</param>
	internal delegate void RawInputEventHandler(object sender, RawInputEventArgs e);
}
