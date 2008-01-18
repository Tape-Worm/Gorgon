#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Friday, October 26, 2007 11:07:37 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Abstract interface for an input plug-in.
	/// </summary>
	public abstract class InputPlugIn
		: PlugInEntryPoint
	{
		#region Properties.
		/// <summary>
		/// Property to return a description of the input interface.
		/// </summary>
		public abstract string Description
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to unload this plug-in.
		/// </summary>
		protected internal virtual void Unload()
		{
			if (PlugInFactory.PlugIns.Contains(Name))
				PlugInFactory.PlugIns.Remove(Name);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="InputPlugIn"/> class.
		/// </summary>
		/// <param name="name">The name of the plug-in.</param>
		/// <param name="inputDLLPath">The input DLL path.</param>
		protected InputPlugIn(string name, string inputDLLPath)
			: base(name, inputDLLPath, PlugInType.Input)
		{
		}
		#endregion
	}
}
