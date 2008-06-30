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
// Created: Saturday, September 23, 2006 2:29:40 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a list of shader values.
	/// </summary>
	public class ShaderParameterList
		: Collection<IShaderParameter>
	{
		#region Methods.
		/// <summary>
		/// Function to add a parameter to the list.
		/// </summary>
		/// <param name="parameter">Parameter to add.</param>
		internal void Add(IShaderParameter parameter)
		{
			// Update collection.
			if (Contains(parameter.Name))
				SetItem(parameter.Name, parameter);
			else
				AddItem(parameter.Name, parameter);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal ShaderParameterList()
		{
		}
		#endregion
	}
}
