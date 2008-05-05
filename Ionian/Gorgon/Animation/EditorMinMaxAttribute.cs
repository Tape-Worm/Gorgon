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
// Created: Saturday, May 03, 2008 7:15:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Attribute for sprite editor to determine min/max.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class EditorMinMaxAttribute
		: Attribute
	{
		#region Variables.
		private float _min = float.MinValue;		// Minimum value.
		private float _max = float.MaxValue;		// Maximum value.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the minimum value.
		/// </summary>
		public float Minimum
		{
			get
			{
				return _min;
			}
		}

		/// <summary>
		/// Property to return the maximum value.
		/// </summary>
		public float Maximum
		{
			get
			{
				return _max;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="EditorMinMaxAttribute"/> class.
		/// </summary>
		/// <param name="min">The minimum value.</param>
		/// <param name="max">The maximum value.</param>
		public EditorMinMaxAttribute(float min, float max)
		{
			_min = min;
			_max = max;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="EditorMinMaxAttribute"/> class.
		/// </summary>
		public EditorMinMaxAttribute()
		{
		}
		#endregion
	}
}
