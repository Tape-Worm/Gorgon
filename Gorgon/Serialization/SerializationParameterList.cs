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
// Created: Saturday, August 12, 2006 2:50:34 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities.Collections;

namespace GorgonLibrary.Serialization
{
	/// <summary>
	/// Object to represent a list of parameters used for serialization.
	/// </summary>
	public class SerializationParameterList
		: BaseDynamicArray<SerializationParameter>
	{
		#region Methods.
		/// <summary>
		/// Function to return the index of a specific parameter.
		/// </summary>
		/// <param name="keyName">Name of the parameter.</param>
		/// <returns>Index of the parameter, -1 if not found.</returns>
		public int IndexOf(string keyName)
		{
			for (int i = 0; i < _items.Count; i++)
			{
				if (_items[i].Name == keyName)
					return i;
			}

			return -1;
		}

		/// <summary>
		/// Function to return whether the array contains a specific parameter or not.
		/// </summary>
		/// <param name="keyName">Name of the parameter.</param>
		/// <returns>TRUE if found, FALSE if not.</returns>
		public bool Contains(string keyName)
		{
			foreach (SerializationParameter parameter in this)
			{
				if (parameter.Name == keyName)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Function to add a serialization parameter to the list.
		/// </summary>
		/// <param name="parameter">Parameter to add.</param>
		public void AddParameter(SerializationParameter parameter)
		{
			_items.Add(parameter);
		}

		/// <summary>
		/// Function to add a parameter to the list.
		/// </summary>
		/// <typeparam name="T">Type of value to add.</typeparam>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="parameterValue">Value of the parameter.</param>
		public void AddParameter<T>(string parameterName, T parameterValue)
		{
			_items.Add(new SerializationParameter(parameterName, parameterValue));
		}

		/// <summary>
		/// Function to return a parameter from the list.
		/// </summary>
		/// <typeparam name="T">Type of item in the parameter.</typeparam>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <returns>Value stored at the parameter.</returns>
		public T GetParameter<T>(string parameterName)
		{
			if (!Contains(parameterName))
				throw new SharpUtilities.Collections.KeyNotFoundException(parameterName);

			return (T)this[IndexOf(parameterName)].Value;
		}

		/// <summary>
		/// Function to set the value for the parameter.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="parameterName">Name of the parameter.</param>
		/// <param name="parameterValue">Value to set.</param>
		public void SetParameter<T>(string parameterName, T parameterValue)
		{
			if (!Contains(parameterName))
				throw new SharpUtilities.Collections.KeyNotFoundException(parameterName);

			_items[IndexOf(parameterName)].Value = parameterValue;
		}

		/// <summary>
		/// Function to return a parameter from the list.
		/// </summary>
		/// <typeparam name="T">Type of item in the parameter.</typeparam>
		/// <param name="index">Index of the parameter to retrieve.</param>
		/// <returns>Value stored at the parameter.</returns>
		public T GetParameter<T>(int index)
		{
			if ((index < 0) || (index >= _items.Count))
				throw new SharpUtilities.Collections.IndexOutOfBoundsException(index);

			return (T)this[index].Value;
		}

		/// <summary>
		/// Function to set the value for the parameter.
		/// </summary>
		/// <typeparam name="T">Type of value.</typeparam>
		/// <param name="index">Index of the parameter to retrieve.</param>
		/// <param name="parameterValue">Value to set.</param>
		public void SetParameter<T>(int index, T parameterValue)
		{
			if ((index < 0) || (index >= _items.Count))
				throw new SharpUtilities.Collections.IndexOutOfBoundsException(index);

			_items[index].Value = parameterValue;
		}

		/// <summary>
		/// Function to clear the parameters.
		/// </summary>
		public void Clear()
		{
			base.ClearItems();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal SerializationParameterList()
			: base(16)
		{
		}
		#endregion		
	}
}
