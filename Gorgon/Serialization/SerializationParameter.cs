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
// Created: Tuesday, October 24, 2006 1:35:30 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Object representing a serialization parameter.
	/// </summary>
	public class SerializationParameter
		: NamedObject
	{
		#region Variables.
		private object _value = null;			// Value for the parameter.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the value of the serialization parameter.
		/// </summary>
		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		/// <summary>
		/// Property to return the type of the value in the parameter.
		/// </summary>
		public Type Type
		{
			get
			{
				return _value.GetType();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the parameter.</param>
		/// <param name="value">Value for the parameter.</param>		
		public SerializationParameter(string name, object value)
			: base(name)
		{
			_value = value;			
		}
		#endregion
	}
}
