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
// Created: Thursday, October 12, 2006 4:25:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using SharpUtilities;
using SharpUtilities.Native.Win32;
using SharpUtilities.Utility;
using GorgonLibrary;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Object representing a list of joysticks.
	/// </summary>
	public class GorgonJoystickList
		: JoystickList 
	{
		#region Methods.
		/// <summary>
		/// Function used in the enumeration of the joysticks.
		/// </summary>
		/// <param name="ID">ID of the joystick.</param>
		/// <param name="name">Name of the joystick.</param>
		/// <param name="caps">Capabilities of the joystick.</param>
		/// <param name="threshold">Joystick threshold.</param>
		/// <param name="connected">TRUE if connected, FALSE if not.</param>
		/// <returns>TRUE to continue enumeration, FALSE to stop.</returns>
		private bool Enumerator(int ID, string name, JOYCAPS caps, int threshold, bool connected)
		{
			GorgonJoystick joystick  = null;		// Joystick.

			if (!Contains(name))
			{
				joystick = new GorgonJoystick(ID, name, caps, threshold, Owner);
				AddItem(name, joystick);
			}
			return true;
		}

		/// <summary>
		/// Function to refresh the joystick list.
		/// </summary>
		public override void Refresh()
		{
			// Remove the joysticks.
			ClearItems();

			// Enumerate joysticks.
			Utilities.EnumerateJoysticks(Enumerator, false);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Owning input interface.</param>
		public GorgonJoystickList(Input owner)
			: base(owner)
		{
		}
		#endregion
	}
}
