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
// Created: Monday, May 01, 2006 4:21:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.ComponentModel;


namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// Object to repesent a list of plugins.
	/// </summary>
	public class PlugInList
		: BaseCollection<PlugInEntryPoint>
	{
		#region Properties.
		/// <summary>
		/// Property to return a plug-in by its index.
		/// </summary>
		public PlugInEntryPoint this[int index]
		{
			get
			{
				return GetItem(index);
			}
		}

		/// <summary>
		/// Property to return a plug-in by its name.
		/// </summary>
		public PlugInEntryPoint this[string key]
		{
			get
			{
				return GetItem(key);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to remove all the plug-ins.
		/// </summary>
		internal void Clear()
		{
			ClearItems();
		}

		/// <summary>
		/// Function to add a plug-in to the list.
		/// </summary>
		/// <param name="plugIn">Plug-in to add.</param>
		internal void Add(PlugInEntryPoint plugIn)
		{
			if (plugIn == null)
				throw new ArgumentNullException("plugIn");
			if (Contains(plugIn.Name))
				throw new PlugInAlreadyLoadedException(plugIn.Name);

			AddItem(plugIn.Name, plugIn);
		}

		/// <summary>
		/// Function to remove a plug-in.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in to remove.</param>
		internal void Remove(string plugInName)
		{
			if (!Contains(plugInName))
				throw new PlugInNotFoundException(plugInName);

			RemoveItem(plugInName);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal PlugInList()
            : base(4, true)
		{
		}
		#endregion
	}
}
