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
// Created: Sunday, October 15, 2006 2:11:31 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using D3D = Microsoft.DirectX.Direct3D;

namespace GorgonLibrary.Graphics.Shaders
{
	/// <summary>
	/// Interface for shader parameter values.
	/// </summary>
	internal interface IShaderValue
	{
		#region Properties.
		/// <summary>
		/// Property to return the size of the value in bytes.
		/// </summary>
		int SizeOf
		{
			get;
		}

		/// <summary>
		/// Property to return whether this value is an array or not.
		/// </summary>
		bool IsArray
		{
			get;
		}

		/// <summary>
		/// Property to return the type of data this value represents.
		/// </summary>
		ShaderParameterType Type
		{
			get;
		}

		/// <summary>
		/// Property to return the length of the array.
		/// </summary>
		int ArrayLength
		{
			get;
		}

		/// <summary>
		/// Property to return the .NET data type.
		/// </summary>
		Type DataType
		{
			get;
		}
		#endregion		
	}
}
