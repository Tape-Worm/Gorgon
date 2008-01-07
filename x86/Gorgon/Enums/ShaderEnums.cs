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
// Created: Saturday, October 14, 2006 5:41:37 PM
// 
#endregion

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Enumeration containing shader parameter types.
	/// </summary>
	public enum ShaderParameterType
	{
		/// <summary>
		/// Boolean value.
		/// </summary>
		Boolean = 1,
		/// <summary>
		/// Floating point value.
		/// </summary>
		Float = 2,
		/// <summary>
		/// Integer value.
		/// </summary>
		Integer = 3,
		/// <summary>
		/// Image.
		/// </summary>
		Image = 4,
		/// <summary>
		/// Render image.
		/// </summary>
		RenderImage = 5,
		/// <summary>
		/// Color value.
		/// </summary>
		Color = 6,
		/// <summary>
		/// Matrix value.
		/// </summary>
		Matrix = 7,
		/// <summary>
		/// 4 dimensional vector.
		/// </summary>
		Vector4D = 8,
		/// <summary>
		/// String value.
		/// </summary>
		String = 9,
		/// <summary>
		/// Unknown type.
		/// </summary>
		Unknown = 0
	}
}