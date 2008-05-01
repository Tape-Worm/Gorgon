#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Wednesday, April 30, 2008 1:43:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary
{
	/// <summary>
	/// Interface that defines an object that can clone itself.
	/// </summary>
	/// <typeparam name="T">Type of the object.</typeparam>
	/// <remarks>This is like <see cref="System.ICloneable"/> only except that it uses a generic type.  Why this wasn't added in .NET 2.0 I'll never know.</remarks>
	interface ICloneable<T>
	{
		/// <summary>
		/// Function to create a clone of an instance of an object.
		/// </summary>
		/// <returns>A clone of the object.</returns>
		T Clone();
	}
}
