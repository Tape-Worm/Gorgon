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
// Created: Saturday, May 24, 2008 1:08:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary
{
	/// <summary>
	/// Value type to indicate a range of values.
	/// </summary>
	/// <typeparam name="T">Type of data for the range.</typeparam>
	public struct Range<T>
	{
		/// <summary>
		/// Starting point in the range.
		/// </summary>
		public T Start;
		/// <summary>
		/// Ending point in the range.
		/// </summary>
		public T End;

		/// <summary>
		/// Initializes a new instance of the <see cref="Range&lt;T&gt;"/> struct.
		/// </summary>
		/// <param name="start">The starting value.</param>
		/// <param name="end">The ending value.</param>
		public Range(T start, T end)
		{
			Start = start;
			End = end;
		}
	}
}
