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
// Created: Saturday, April 07, 2007 3:09:03 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.Internal;

namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// Abstract class representing a plug-in entry point.
	/// </summary>
	[PlugIn()]
	public abstract class PlugInEntryPoint
		: NamedObject
	{
		#region Variables.
		private PlugInType _type = PlugInType.UserDefined;		// Plug-in type.
		private string _plugInPath = string.Empty;				// Plug-in path.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the plug-in type.
		/// </summary>
		public PlugInType PlugInType
		{
			get
			{
				return _type;
			}
		}

		/// <summary>
		/// Property to return the plug-in path.
		/// </summary>
		public string PlugInPath
		{
			get
			{
				return _plugInPath;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a new object from the plug-in.
		/// </summary>
		/// <param name="parameters">Parameters to pass.</param>
		/// <returns>The new object.</returns>
		protected abstract internal object CreateImplementation(object[] parameters);

        /// <summary>
        /// Function to return an interface to Gorgon's Direct 3D objects.
        /// </summary>
        /// <returns>An instance of the D3D objects interface.</returns>
        protected D3DObjects GetD3DObjects()
        {
            return new D3DObjects();
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		protected PlugInEntryPoint(string name, string plugInPath, PlugInType type)
			: base(name)
		{
			_plugInPath = plugInPath;
			_type = type;
		}
		#endregion
	}
}
