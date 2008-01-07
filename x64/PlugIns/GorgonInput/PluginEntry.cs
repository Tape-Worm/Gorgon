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

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Entry point for the plug-in.
	/// </summary>
    public class GorgonInputPlugInEntry
		: InputPlugIn
	{
		#region Variables.
		private static Input _input = null;					// Input interface object.
		private static object _syncLock = new object();		// Lock synchronization object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a description of the input interface.
		/// </summary>
		/// <value></value>
		public override string Description
		{
			get 
			{
				return "Gorgon raw input interface.";
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to unload this plug-in.
		/// </summary>
		protected override void Unload()
		{
			_input = null;
			base.Unload();
		}

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

				return _input;
			}
			catch (Exception ex)
			{
				throw new CannotCreateException("Cannot create input plug-in.", ex);
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInPath">Path to the plug-in.</param>
		public GorgonInputPlugInEntry(string plugInPath)
			: base("Gorgon.RawInput", plugInPath)
		{
		}
		#endregion
	}
}
