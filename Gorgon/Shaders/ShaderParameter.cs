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
// Created: Saturday, September 23, 2006 2:30:26 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using DX = Microsoft.DirectX;
using D3D = Microsoft.DirectX.Direct3D;
using SharpUtilities;
using SharpUtilities.Mathematics;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics.Shaders
{
	/// <summary>
	/// Object representing a shader parameter.
	/// </summary>
	public class ShaderParameter
		: NamedObject
	{
		#region Variables.
		private Shader _owner;								// Owner of the shader.
		private D3D.EffectHandle _effectHandle;				// Handle to the parameter.
		private IShaderValue _parameterValue;				// Parameter value.
		private bool _changed = false;						// Parameter has changed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the effect handle.
		/// </summary>
		internal D3D.EffectHandle D3DEffectHandle
		{
			get
			{
				return _effectHandle;
			}
		}

		/// <summary>
		/// Property to return the size of an array parameter.
		/// </summary>
		public int ArrayLength
		{
			get
			{
				if (_parameterValue == null)
					return -1;
				else
					return _parameterValue.ArrayLength;
			}
		}

		/// <summary>
		/// Property to return the type of the value.
		/// </summary>
		public ShaderParameterType ValueType
		{
			get
			{
				if (_parameterValue == null)
					return ShaderParameterType.Unknown;
				else
					return _parameterValue.Type;
			}
		}

		/// <summary>
		/// Property to return the owning shader for this parameter.
		/// </summary>
		public Shader Owner
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Property to return whether this value is an array or not.
		/// </summary>
		public bool IsArray
		{
			get
			{
				if (_parameterValue == null)
					return false;
				else
					return _parameterValue.IsArray;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve value(s) from the shader.
		/// </summary>
		private void RefreshValue()
		{
			if (_parameterValue == null)
				return;

			// Perform conversion if necessary:
			switch (_parameterValue.Type)
			{
				case ShaderParameterType.Color:
					if (_parameterValue.IsArray)
					{
						D3D.ColorValue[] value;		// Return value.
						Color[] colors = null;		// Return colors.

						value = _owner.D3DEffect.GetValueColorArray(_effectHandle, _parameterValue.ArrayLength);

						colors = new Color[_parameterValue.ArrayLength];
						for (int i = 0; i < value.Length; i++)
							colors[i] = Color.FromArgb(value[i].ToArgb());

						// Copy back.
						((ShaderValue<Color[]>)_parameterValue).Value = colors;
					}
					else
						((ShaderValue<Color>)_parameterValue).Value = Color.FromArgb(_owner.D3DEffect.GetValueColor(_effectHandle).ToArgb());
					break;
				case ShaderParameterType.Matrix:
					if (_parameterValue.IsArray)
					{
						DX.Matrix[] value;				// Return value.
						Matrix[] matrices = null;		// Return matrices.

						value = _owner.D3DEffect.GetValueMatrixArray(_effectHandle, _parameterValue.ArrayLength);

						matrices = new Matrix[_parameterValue.ArrayLength];
						for (int i = 0; i < value.Length; i++)
							matrices[i] = Converter.Convert(value[i]);

						((ShaderValue<Matrix[]>)_parameterValue).Value = matrices;
					}
					else
						((ShaderValue<Matrix>)_parameterValue).Value = Converter.Convert(_owner.D3DEffect.GetValueMatrix(_effectHandle));
					break;
				case ShaderParameterType.Image:
					((ShaderValue<Image>)_parameterValue).Value.D3DTexture = _owner.D3DEffect.GetValueTexture(_effectHandle);
					break;
				case ShaderParameterType.RenderImage:
					((ShaderValue<RenderImage>)_parameterValue).Value.Image.D3DTexture = _owner.D3DEffect.GetValueTexture(_effectHandle);
					break;
				case ShaderParameterType.Vector4D:
					if (_parameterValue.IsArray)
					{
						DX.Vector4[] value;				// Return value.
						Vector4D[] vectors = null;		// Return vectors.

						value = _owner.D3DEffect.GetValueVectorArray(_effectHandle, _parameterValue.ArrayLength);

						vectors = new Vector4D[_parameterValue.ArrayLength];
						for (int i = 0; i < value.Length; i++)
							vectors[i] = Converter.Convert(value[i]);

						((ShaderValue<Vector4D[]>)_parameterValue).Value = vectors;
					}
					else
						((ShaderValue<Vector4D>)_parameterValue).Value = Converter.Convert(_owner.D3DEffect.GetValueVector(_effectHandle));
					break;
				case ShaderParameterType.Boolean:
					if (_parameterValue.IsArray)
						((ShaderValue<bool[]>)_parameterValue).Value = _owner.D3DEffect.GetValueBooleanArray(_effectHandle, _parameterValue.ArrayLength);
					else
						((ShaderValue<bool>)_parameterValue).Value = _owner.D3DEffect.GetValueBoolean(_effectHandle);
					break;
				case ShaderParameterType.Float:
					if (_parameterValue.IsArray)
						((ShaderValue<float[]>)_parameterValue).Value = _owner.D3DEffect.GetValueFloatArray(_effectHandle, _parameterValue.ArrayLength);
					else
						((ShaderValue<float>)_parameterValue).Value = _owner.D3DEffect.GetValueFloat(_effectHandle);
					break;
				case ShaderParameterType.Integer:
					if (_parameterValue.IsArray)
						((ShaderValue<int[]>)_parameterValue).Value = _owner.D3DEffect.GetValueIntegerArray(_effectHandle, _parameterValue.ArrayLength);
					else
						((ShaderValue<int>)_parameterValue).Value = _owner.D3DEffect.GetValueInteger(_effectHandle);
					break;
				case ShaderParameterType.String:
					((ShaderValue<string>)_parameterValue).Value = _owner.D3DEffect.GetValueString(_effectHandle);
					break;
			}
		}

		/// <summary>
		/// Function to commit the parameter to an effect.
		/// </summary>
		internal void Commit()
		{
			// Do nothing if no parameter.
			if ((_parameterValue == null) || (!_changed))
				return;

			// Perform conversion if necessary:
			switch (_parameterValue.Type)
			{
				case ShaderParameterType.Color:
					if (_parameterValue.IsArray)
					{
						D3D.ColorValue[] colors = null;		// Color values.

						colors = new D3D.ColorValue[_parameterValue.ArrayLength];
						for (int i = 0; i < colors.Length; i++)
							colors[i] = D3D.ColorValue.FromColor(((ShaderValue<Color[]>)_parameterValue).Value[i]);

						_owner.D3DEffect.SetValue(_effectHandle, colors);
					}
					else
						_owner.D3DEffect.SetValue(_effectHandle, D3D.ColorValue.FromColor(((ShaderValue<Color>)_parameterValue).Value));
					break;
				case ShaderParameterType.Matrix:
					if (_parameterValue.IsArray)
					{
						DX.Matrix[] matrix = null;		// Matrices.
						
						matrix = new DX.Matrix[_parameterValue.ArrayLength];
						for (int i = 0; i < matrix.Length; i++)
							matrix[i] = Converter.Convert(((ShaderValue<Matrix[]>)_parameterValue).Value[i]);
						_owner.D3DEffect.SetValue(_effectHandle, matrix);
					}
					else
						_owner.D3DEffect.SetValue(_effectHandle, Converter.Convert(((ShaderValue<Matrix>)_parameterValue).Value));
					break;
				case ShaderParameterType.Image:
					_owner.D3DEffect.SetValue(_effectHandle, ((ShaderValue<Image>)_parameterValue).Value.D3DTexture);
					break;
				case ShaderParameterType.RenderImage:
					_owner.D3DEffect.SetValue(_effectHandle, ((ShaderValue<RenderImage>)_parameterValue).Value.Image.D3DTexture);
					break;
				case ShaderParameterType.Vector4D:
					if (_parameterValue.IsArray)
					{
						DX.Vector4[] vector = null;		// Vectors.

						vector = new DX.Vector4[_parameterValue.ArrayLength];

						for (int i = 0; i < vector.Length; i++)
							vector[i] = Converter.Convert(((ShaderValue<Vector4D[]>)_parameterValue).Value[i]);
						_owner.D3DEffect.SetValue(_effectHandle, vector);
					}
					else
						_owner.D3DEffect.SetValue(_effectHandle, Converter.Convert(((ShaderValue<Vector4D>)_parameterValue).Value));
					break;
				case ShaderParameterType.Boolean:
					if (_parameterValue.IsArray)
						_owner.D3DEffect.SetValue(_effectHandle, ((ShaderValue<bool[]>)_parameterValue).Value);
					else
						_owner.D3DEffect.SetValue(_effectHandle, ((ShaderValue<bool>)_parameterValue).Value);
					break;
				case ShaderParameterType.Float:
					if (_parameterValue.IsArray)
						_owner.D3DEffect.SetValue(_effectHandle, ((ShaderValue<float[]>)_parameterValue).Value);
					else
						_owner.D3DEffect.SetValue(_effectHandle, ((ShaderValue<float>)_parameterValue).Value);
					break;
				case ShaderParameterType.Integer:
					if (_parameterValue.IsArray)
						_owner.D3DEffect.SetValue(_effectHandle, ((ShaderValue<int[]>)_parameterValue).Value);
					else
						_owner.D3DEffect.SetValue(_effectHandle, ((ShaderValue<int>)_parameterValue).Value);
					break;
				case ShaderParameterType.String:
					_owner.D3DEffect.SetValue(_effectHandle, ((ShaderValue<string>)_parameterValue).Value);
					break;
			}

			_changed = false;
		}

		/// <summary>
		/// Function to set the value for the parameter.
		/// </summary>
		/// <typeparam name="T">Type of parameter.</typeparam>
		/// <param name="value">Value for the parameter.</param>
		public void SetValue<T>(T value)
		{
			if (value == null)
				_parameterValue = null;
			else
			{
				if ((_parameterValue != null) && (value.GetType() == _parameterValue.DataType))
					((ShaderValue<T>)_parameterValue).Value = value;
				else
					_parameterValue = new ShaderValue<T>(value);
			}
			_changed = true;
		}

		/// <summary>
		/// Function to return the cached value for the parameter.
		/// </summary>
		/// <param name="cached">TRUE to return cached values, FALSE to update values from the effect (slower).</param>
		/// <typeparam name="T">Type of the parameter.</typeparam>
		/// <returns>Value in the parameter.</returns>
		public T GetValue<T>(bool cached)
		{
			if (_parameterValue == null)
				return default(T);
			else
			{
				if (!cached)
					RefreshValue();

				return ((ShaderValue<T>)_parameterValue).Value;
			}
		}

		/// <summary>
		/// Function to return the cached value for the parameter.
		/// </summary>
		/// <typeparam name="T">Type of the parameter.</typeparam>
		/// <returns>Value in the parameter.</returns>
		public T GetValue<T>()
		{
			return GetValue<T>(true);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the parameter.</param>
		/// <param name="handle">Handle of the parameter.</param>
		/// <param name="shader">Owning shader for this parameter.</param>
		/// <param name="index">Index of the parameter.</param>
		internal ShaderParameter(string name, D3D.EffectHandle handle, Shader shader, int index)
			: base(name)
		{
			_owner = shader;
			_effectHandle = handle;
		}
		#endregion
	}
}
