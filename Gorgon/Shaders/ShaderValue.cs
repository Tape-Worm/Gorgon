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
// Created: Sunday, October 15, 2006 1:48:13 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;
using SharpUtilities.Mathematics;
using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;

namespace GorgonLibrary.Graphics.Shaders
{
	/// <summary>
	/// Object for shader parameter values.
	/// </summary>
	internal class ShaderValue<T>
		: IShaderValue
	{
		#region Variables.
		private int _size;						// Size in bytes of the parameter value.
		private ShaderParameterType _type;		// Type of data.
		private bool _isArray;					// Value is an array?
		private int _arrayLength;				// Length of the array.
		private Type _dataType = null;			// Data type.

		/// <summary>
		/// Value for the shader parameter value.
		/// </summary>
		public T Value;
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="defaultValue">Value for shader parameter.</param>
		internal ShaderValue(T defaultValue)
		{
			_dataType = typeof(T);

			Value = defaultValue;

			if (typeof(T).IsArray)
			{				
				_arrayLength = (defaultValue as Array).Length;
				_isArray = true;
			}
			else
			{
				_arrayLength = -1;
				_isArray = false;
			}

			_type = ShaderParameterType.Unknown;
			// Determine type.
			if ((defaultValue.GetType() == typeof(float)) || (defaultValue.GetType() == typeof(float[])))
				_type = ShaderParameterType.Float;
			if ((defaultValue.GetType() == typeof(int)) || (defaultValue.GetType() == typeof(int[])))
				_type = ShaderParameterType.Integer;
			if ((defaultValue.GetType() == typeof(bool)) || (defaultValue.GetType() == typeof(bool[])))
				_type = ShaderParameterType.Boolean;
			if ((defaultValue.GetType() == typeof(Vector4D)) || (defaultValue.GetType() == typeof(Vector4D[])))
				_type = ShaderParameterType.Vector4D;
			if ((defaultValue.GetType() == typeof(Matrix)) || (defaultValue.GetType() == typeof(Matrix[])))
				_type = ShaderParameterType.Matrix;
			if ((defaultValue.GetType() == typeof(Color)) || (defaultValue.GetType() == typeof(Color[])))
				_type = ShaderParameterType.Color;
			if (defaultValue.GetType() == typeof(RenderImage))
				_type = ShaderParameterType.RenderImage;
			if (defaultValue.GetType() == typeof(Image))
				_type = ShaderParameterType.Image;
			if (defaultValue.GetType() == typeof(string))
				_type = ShaderParameterType.String;

			// Get size in bytes.
			if (typeof(T).IsValueType)
				_size = Marshal.SizeOf(typeof(T));
			else
				_size = -1;
		}
		#endregion

		#region IShaderValue Members
		#region Properties.
		/// <summary>
		/// Property to return the size of the value in bytes.
		/// </summary>
		public int SizeOf
		{
			get
			{
				return _size;
			}
		}

		/// <summary>
		/// Property to return whether this value is an array or not.
		/// </summary>
		public bool IsArray
		{
			get
			{
				return _isArray;
			}
		}

		/// <summary>
		/// Property to return the type of data this value represents.
		/// </summary>
		public ShaderParameterType Type
		{
			get
			{
				return _type;
			}
		}

		/// <summary>
		/// Property to return the length of the array.
		/// </summary>
		public int ArrayLength
		{
			get
			{
				return _arrayLength;
			}
		}

		/// <summary>
		/// Property to return the .NET data type.
		/// </summary>
		public Type DataType
		{
			get
			{
				return _dataType;
			}
		}
		#endregion
		#endregion
	}
}
