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
// Created: Saturday, September 30, 2006 10:04:18 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Input
{
	/// <summary>
	/// Entry point for the plug-in.
	/// </summary>
    public class GorgonInputPlugInEntry
		: PlugInEntryPoint 
	{
		#region Variables.
		private static InputDevices _input = null;			// Input interface object.
		private static object _syncLock = new object();		// Lock synchronization object.
		#endregion

		#region Methods.
        /// <summary>
        /// Function to create an input interface.
        /// </summary>
        /// <param name="parameters">Parameters to pass for construction.</param>
        /// <returns>New input object.</returns>
		protected override object CreateImplementation(object[] parameters)
		{
			try
			{
				if (_input == null)
				{
					// Lock the object in case of multiple threads.
					lock (_syncLock)
					{
						// If the object hasn't been created, create it.
						if (_input == null)
							_input = new GorgonInput(this);
					}
				}

				// Set as the active input if none exists.
				if (Gorgon.InputDevices == null)
					Gorgon.InputDevices = _input;

				return _input;
			}
			catch (Exception ex)
			{
				throw new CannotCreateInputException(ex);
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInPath">Path to the plug-in.</param>
		public GorgonInputPlugInEntry(string plugInPath)
			: base("Gorgon Raw Input Interface.", plugInPath, PlugInType.Input)
		{
		}
		#endregion
	}
}
