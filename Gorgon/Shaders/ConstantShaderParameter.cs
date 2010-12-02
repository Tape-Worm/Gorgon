#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Sunday, June 29, 2008 12:42:29 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object used to wrap pixel/vertex shader constants.
	/// </summary>
	public unsafe class ConstantShaderParameter
		: NamedObject, IShaderParameter
	{
		#region Variables.
		private D3D9.EffectHandle _handle = null;							// Effect handle.
		private D3D9.ShaderBytecode _shaderCode = null;						// Interface to the shader function.
		private Shader _owner = null;										// Shader that owns this parameter.
		private bool _boolValue;											// Boolean value.
		private float _floatValue;											// Floating point value.
		private int _intValue;												// Integer value.
		private bool[] _boolValues = null;									// Boolean value array.
		private float[] _floatValues = null;								// Floating point value array.
		private int[] _intValues = null;									// Integer value array.
		private D3D9.BaseTexture _imageValue = null;						// Image value.
		private DX.Color4 _colorValue;										// Color value.
		private DX.Color4[] _colorValues = null;							// Array of color values.
		private DX.Matrix _matrixValue;										// Matrix value.
		private DX.Matrix[] _matrixValues = null;							// Matrix value array.
		private DX.Vector4 _vector4DValue;									// 4D vector value.
		private DX.Vector4[] _vector4DValues = null;						// 4D vector value array.
		private int _arrayLength = 0;										// Length of the array.
		private ShaderParameterType _type = ShaderParameterType.Unknown;	// Type of the parameter.
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve an array of values.
		/// </summary>
		/// <typeparam name="T">Type of the array.</typeparam>
		/// <param name="source">Source array to copy.</param>
		/// <param name="count">Number of elements to return.</param>
		/// <returns>An array with the specified amount of elements.</returns>
		private T[] GetArrayValues<T>(T[] source, int count)
		{
			if (source == null)
				return null;

			T[] output = new T[count];
			
			Array.Copy(source, output, count);

			return output;
		}

		/// <summary>
		/// Function to set an array of values.
		/// </summary>
		/// <typeparam name="T">Type of data in the array.</typeparam>
		/// <param name="source">Array to copy from.</param>
		private T[] SetArrayValues<T>(T[] source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			T[] output = new T[source.Length];

			Array.Copy(source, output, source.Length);
			_arrayLength = output.Length;
			return output;
		}

		/// <summary>
		/// Function to bind a texture sampler.
		/// </summary>
		/// <param name="sampler">Sampler to bind.</param>
		internal void SetTextureSampler(D3D9.EffectHandle sampler)
		{
			if (_type != ShaderParameterType.Image)
				throw new InvalidOperationException("The parameter is not an image type.");

			int textureUnit = -1;		// Texture unit.

			textureUnit = _shaderCode.ConstantTable.GetSamplerIndex(sampler);
			Gorgon.Screen.Device.SetTexture(textureUnit, _imageValue);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ConstantShaderParameter"/> class.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="handle">Handle to the shader constant.</param>
		/// <param name="function">Function to use for interfacing to the shader.</param>
		/// <param name="type">The type of the parameter value.</param>
		internal ConstantShaderParameter(string name, D3D9.EffectHandle handle, ShaderFunction function, ShaderParameterType type)
			: base(name)
		{
			if (handle == null)
				throw new ArgumentNullException("handle");
			if (function == null)
				throw new ArgumentNullException("handle");
			_type = type;
			_shaderCode = function.ByteCode;
			_handle = handle;
		}
		#endregion

		#region IShaderParameter Members
		#region Properties.
		/// <summary>
		/// Property to return the size of an array parameter.
		/// </summary>
		/// <value></value>
		public int ArrayLength
		{
			get 
			{
				return _arrayLength;
			}
		}

		/// <summary>
		/// Property to return the type of the value.
		/// </summary>
		/// <value></value>
		public ShaderParameterType ValueType
		{
			get 
			{
				return _type;
			}
			private set
			{
				_boolValue = default(bool);
				_floatValue = default(float);
				_intValue = default(int);
				_boolValues = null;
				_floatValues = null;
				_intValues = null;
				_imageValue = null;
				_colorValue = default(DX.Color4);
				_colorValues = null;
				_matrixValue = default(DX.Matrix);
				_matrixValues = null;
				_vector4DValue = default(DX.Vector4);
				_vector4DValues = null;				
				_arrayLength = 0;

				_type = value;
			}
		}

		/// <summary>
		/// Property to return the owning shader for this parameter.
		/// </summary>
		/// <value></value>
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
		/// <value></value>
		public bool IsArray
		{
			get 
			{
				return (_arrayLength != 0);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return a boolean value.
		/// </summary>
		/// <returns>A boolean value.</returns>
		public bool GetBoolean()
		{
			return _boolValue;
		}

		/// <summary>
		/// Function to return a boolean value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A boolean value array.</returns>
		public bool[] GetBooleanArray(int count)
		{
			return GetArrayValues<bool>(_boolValues, count);
		}

		/// <summary>
		/// Function to return a floating point value.
		/// </summary>
		/// <returns>A boolean value.</returns>
		public float GetFloat()
		{
			return _floatValue;
		}

		/// <summary>
		/// Function to return a floating point value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A floating point value array.</returns>
		public float[] GetFloatArray(int count)
		{
			return GetArrayValues<float>(_floatValues, count);
		}

		/// <summary>
		/// Function to return an integer value.
		/// </summary>
		/// <returns>An integer value.</returns>
		public int GetInteger()
		{
			return _intValue;
		}

		/// <summary>
		/// Function to return an integer value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>An integer value array.</returns>
		public int[] GetIntegerArray(int count)
		{
			return GetArrayValues<int>(_intValues, count);
		}

		/// <summary>
		/// Function to return a string value.
		/// </summary>
		/// <returns>A string value.</returns>
		string IShaderParameter.GetString()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Function to return a 4D vector value.
		/// </summary>
		/// <returns>A 4D vector value.</returns>
		public Vector4D GetVector4D()
		{
			return Converter.Convert(_vector4DValue);
		}

		/// <summary>
		/// Function to return a 4D vector value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A 4D vector value array.</returns>
		public Vector4D[] GetVector4DArray(int count)
		{
			Vector4D[] values = new Vector4D[count];

			for (int i = 0; i < count; i++)
				values[i] = Converter.Convert(_vector4DValues[i]);

			return values;
		}

		/// <summary>
		/// Function to return a matrix value.
		/// </summary>
		/// <returns>A matrix value.</returns>
		public Matrix GetMatrix()
		{
			return GetMatrix(false);
		}

		/// <summary>
		/// Function to return a matrix value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A matrix value array.</returns>
		public Matrix[] GetMatrixArray(int count)
		{
			return GetMatrixArray(count, false);
		}

		/// <summary>
		/// Function to return a matrix value.
		/// </summary>
		/// <param name="transpose">TRUE if returning a transposed matrix, FALSE if not.</param>
		/// <returns>A matrix value.</returns>
		public Matrix GetMatrix(bool transpose)
		{
			if (transpose)
				return Converter.Convert(DX.Matrix.Transpose(_matrixValue));
			else
				return Converter.Convert(_matrixValue);
		}

		/// <summary>
		/// Function to return a matrix value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <param name="transpose">TRUE if returning a transposed matrix, FALSE if not.</param>
		/// <returns>A matrix value array.</returns>
		public Matrix[] GetMatrixArray(int count, bool transpose)
		{
			Matrix[] values = new Matrix[count];

			for (int i = 0; i < values.Length; i++)
			{
				if (transpose)
					values[i] = Converter.Convert(DX.Matrix.Transpose(_matrixValues[i]));
				else
					values[i] = Converter.Convert(_matrixValues[i]);
			}

			return values;
		}

		/// <summary>
		/// Function to return a color value.
		/// </summary>
		/// <returns>A color value.</returns>
		public System.Drawing.Color GetColor()
		{
			return _colorValue.ToColor();
		}

		/// <summary>
		/// Function to return a color value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A color value array.</returns>
		public System.Drawing.Color[] GetColorArray(int count)
		{
			Drawing.Color[] values = new Drawing.Color[count];

			for (int i = 0; i < count; i++)
				values[i] = _colorValues[i].ToColor();

			return values;
		}

		/// <summary>
		/// Function to return an image value.
		/// </summary>
		/// <param name="sourceImage">Image to use to store the shader texture.</param>
		public void GetImage(Image sourceImage)
		{
			if (_imageValue == null)
				return;

			if (sourceImage == null)
				throw new ArgumentNullException("sourceImage");

			sourceImage.D3DTexture = (D3D9.Texture)_imageValue;
		}

		/// <summary>
		/// Function to return an image value.
		/// </summary>
		/// <returns>An image value.</returns>
		public Image GetImage()
		{
			if (_imageValue == null)
				return null;

			Image newImage = new Image("ShaderParameter." + Name + ".Image", ImageType.Normal, 1, 1, ImageBufferFormats.BufferUnknown, false);	// Empty image.			

			GetImage(newImage);

			return newImage;
		}

		/// <summary>
		/// Function to set a boolean parameter value.
		/// </summary>
		/// <param name="value">A boolean value.</param>
		public void SetValue(bool value)
		{
			this.ValueType = ShaderParameterType.Boolean;
			_boolValue = value;
			_shaderCode.ConstantTable.SetValue(Gorgon.Screen.Device, _handle, value);
		}

		/// <summary>
		/// Function to set a boolean array parameter value.
		/// </summary>
		/// <param name="value">An array of boolean values.</param>
		public void SetValue(bool[] value)
		{
			this.ValueType = ShaderParameterType.Boolean;
			_boolValues = SetArrayValues<bool>(value);
			_shaderCode.ConstantTable.SetValue(Gorgon.Screen.Device, _handle, value);
		}

		/// <summary>
		/// Function to set a floating point parameter value.
		/// </summary>
		/// <param name="value">A floating point value.</param>
		public void SetValue(float value)
		{
			this.ValueType = ShaderParameterType.Float;
			_floatValue = value;
			_shaderCode.ConstantTable.SetValue(Gorgon.Screen.Device, _handle, value);
		}

		/// <summary>
		/// Function to set a floating point array parameter value.
		/// </summary>
		/// <param name="value">An array of floating point values.</param>
		public void SetValue(float[] value)
		{
			this.ValueType = ShaderParameterType.Float;
			_floatValues = SetArrayValues<float>(value);
			_shaderCode.ConstantTable.SetValue(Gorgon.Screen.Device, _handle, value);
		}

		/// <summary>
		/// Function to set an integer parameter value.
		/// </summary>
		/// <param name="value">An integer value.</param>
		public void SetValue(int value)
		{
			this.ValueType = ShaderParameterType.Integer;
			_intValue = value;
			_shaderCode.ConstantTable.SetValue(Gorgon.Screen.Device, _handle, value);
		}

		/// <summary>
		/// Function to set an integer array parameter value.
		/// </summary>
		/// <param name="value">An array of integer values.</param>
		public void SetValue(int[] value)
		{
			this.ValueType = ShaderParameterType.Integer;
			_intValues = SetArrayValues<int>(value);
			_shaderCode.ConstantTable.SetValue(Gorgon.Screen.Device, _handle, value);
		}

		/// <summary>
		/// Function to set a string parameter value.
		/// </summary>
		/// <param name="value">A string value.</param>
		void IShaderParameter.SetValue(string value)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Function to set a 4D vector parameter value.
		/// </summary>
		/// <param name="value">A 4D vector value.</param>
		public void SetValue(Vector4D value)
		{
			this.ValueType = ShaderParameterType.Vector4D;
			_vector4DValue = Converter.Convert(value);
			_shaderCode.ConstantTable.SetValue(Gorgon.Screen.Device, _handle, _vector4DValue);
		}

		/// <summary>
		/// Function to set a 4D vector parameter array value.
		/// </summary>
		/// <param name="value">An array of 4D vector values.</param>
		public void SetValue(Vector4D[] value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			this.ValueType = ShaderParameterType.Vector4D;
			_vector4DValues = new DX.Vector4[value.Length];

			for (int i = 0; i < _vector4DValues.Length; i++)
				_vector4DValues[i] = Converter.Convert(value[i]);
			_shaderCode.ConstantTable.SetValue(Gorgon.Screen.Device, _handle, _vector4DValues);
			_arrayLength = value.Length;
		}

		/// <summary>
		/// Function to set a matrix value.
		/// </summary>
		/// <param name="value">A matrix value.</param>
		public void SetValue(Matrix value)
		{
			SetValue(value, false);
		}

		/// <summary>
		/// Function to set an array of matrix values.
		/// </summary>
		/// <param name="value">An array of matrix values.</param>
		public void SetValue(Matrix[] value)
		{
			SetValue(value, false);
		}

		/// <summary>
		/// Function to set a matrix value with option for transposed matrix.
		/// </summary>
		/// <param name="value">A matrix value.</param>
		/// <param name="transpose">TRUE to use transpose of the matrix, FALSE for standard matrix.</param>
		public void SetValue(Matrix value, bool transpose)
		{
			this.ValueType = ShaderParameterType.Matrix;
			_matrixValue = Converter.Convert(value);
			if (!transpose)
				_shaderCode.ConstantTable.SetValue(Gorgon.Screen.Device, _handle, _matrixValue);
			else
				_shaderCode.ConstantTable.SetValueTranspose(Gorgon.Screen.Device, _handle, _matrixValue);
		}

		/// <summary>
		/// Function to set a matrix value with option for transposed matrix.
		/// </summary>
		/// <param name="value">An array of matrix values.</param>
		/// <param name="transpose">TRUE to use transpose of the matrix, FALSE for standard matrix.</param>
		public void SetValue(Matrix[] value, bool transpose)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			this.ValueType = ShaderParameterType.Vector4D;
			_matrixValues = new DX.Matrix[value.Length];

			for (int i = 0; i < _matrixValues.Length; i++)
				_matrixValues[i] = Converter.Convert(value[i]);
			if (transpose)
				_shaderCode.ConstantTable.SetValueTranspose(Gorgon.Screen.Device, _handle, _matrixValues);
			else
				_shaderCode.ConstantTable.SetValue(Gorgon.Screen.Device, _handle, _matrixValues);
			_arrayLength = value.Length;
		}

		/// <summary>
		/// Function to set a color parameter value
		/// </summary>
		/// <param name="value">A color value.</param>
		public void SetValue(System.Drawing.Color value)
		{
			this.ValueType = ShaderParameterType.Color;
			_colorValue = new DX.Color4(value);
			_shaderCode.ConstantTable.SetValue(Gorgon.Screen.Device, _handle, value);
		}

		/// <summary>
		/// Function to set an array of color parameter values
		/// </summary>
		/// <param name="value">An array of color values.</param>
		public void SetValue(System.Drawing.Color[] value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			this.ValueType = ShaderParameterType.Color;
			_colorValues = new SlimDX.Color4[value.Length];

			for (int i = 0; i < value.Length; i++)
				_colorValues[i] = new DX.Color4(value[i]);
			_shaderCode.ConstantTable.SetValue(Gorgon.Screen.Device, _handle, _colorValues);
			_arrayLength = value.Length;
		}

		/// <summary>
		/// Function to set an image parameter value.
		/// </summary>
		/// <param name="image">Image to set.</param>
		public void SetValue(Image image)
		{
			this.ValueType = ShaderParameterType.Image;
			if (image != null)
				_imageValue = image.D3DTexture;
			else
				_imageValue = null;
		}

		/// <summary>
		/// Function to set a render image parameter value.
		/// </summary>
		/// <param name="image">Render image to set.</param>
		public void SetValue(RenderImage image)
		{
			if (image != null)
				SetValue(image.Image);
			else
				SetValue((Image)null);
		}
		#endregion
		#endregion
	}
}
