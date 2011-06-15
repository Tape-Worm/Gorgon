#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Saturday, September 23, 2006 2:30:26 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a shader parameter.
	/// </summary>
	public class ShaderParameter
		: NamedObject, IShaderParameter
	{
		#region Variables.
		private FXShader _owner;							// Owner of the shader.
		private D3D9.EffectHandle _effectHandle;			// Handle to the parameter.
		private D3D9.ParameterDescription _desc;			// Parameter description data.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the handle for the parameter.
		/// </summary>
		internal D3D9.EffectHandle Handle
		{
			get
			{
				return _effectHandle;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="desc">Description of the parameter.</param>
		/// <param name="handle">Handle of the parameter.</param>
		/// <param name="shader">Owning shader for this parameter.</param>
		/// <param name="index">Index of the parameter.</param>
		internal ShaderParameter(D3D9.ParameterDescription desc, D3D9.EffectHandle handle, FXShader shader, int index)
			: base(desc.Name)
		{
			_owner = shader;
			_effectHandle = handle;			
			_desc = desc;
		}
		#endregion

		#region IShaderParameter Members
		#region Properties.
		/// <summary>
		/// Property to return the size of an array parameter.
		/// </summary>
		public int ArrayLength
		{
			get
			{
				return _desc.Elements;
			}
		}

		/// <summary>
		/// Property to return the type of the value.
		/// </summary>
		public ShaderParameterType ValueType
		{
			get
			{
				switch (_desc.Type)
				{
					case SlimDX.Direct3D9.ParameterType.Bool:
						return ShaderParameterType.Boolean;
					case SlimDX.Direct3D9.ParameterType.Float:
						return ShaderParameterType.Float;
					case SlimDX.Direct3D9.ParameterType.Int:
						return ShaderParameterType.Integer;
					case SlimDX.Direct3D9.ParameterType.String:
						return ShaderParameterType.String;
					case SlimDX.Direct3D9.ParameterType.Texture:
					case SlimDX.Direct3D9.ParameterType.Texture1D:
					case SlimDX.Direct3D9.ParameterType.Texture2D:
					case SlimDX.Direct3D9.ParameterType.Texture3D:
					case SlimDX.Direct3D9.ParameterType.TextureCube:
						return ShaderParameterType.Image;
					default:
						return ShaderParameterType.Unknown;
				}
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
				return _desc.Elements > 0;
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
			return _owner.D3DEffect.GetValue<bool>(_effectHandle);
		}

		/// <summary>
		/// Function to return a boolean value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A boolean value array.</returns>
		public bool[] GetBooleanArray(int count)
		{
			if (!IsArray)
				return null;

			// Force to array length.
			if ((count > ArrayLength) || (count < 1))
				count = ArrayLength;

			return _owner.D3DEffect.GetValue<bool>(_effectHandle, count);
		}

		/// <summary>
		/// Function to return a floating point value.
		/// </summary>
		/// <returns>A boolean value.</returns>
		public float GetFloat()
		{
			return _owner.D3DEffect.GetValue<float>(_effectHandle);
		}

		/// <summary>
		/// Function to return a floating point value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A floating point value array.</returns>
		public float[] GetFloatArray(int count)
		{
			if (!IsArray)
				return null;

			// Force to array length.
			if ((count > ArrayLength) || (count < 1))
				count = ArrayLength;

			return _owner.D3DEffect.GetValue<float>(_effectHandle, count);
		}

		/// <summary>
		/// Function to return an integer value.
		/// </summary>
		/// <returns>An integer value.</returns>
		public int GetInteger()
		{
			return _owner.D3DEffect.GetValue<int>(_effectHandle);
		}

		/// <summary>
		/// Function to return an integer value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>An integer value array.</returns>
		public int[] GetIntegerArray(int count)
		{
			if (!IsArray)
				return null;

			// Force to array length.
			if ((count > ArrayLength) || (count < 1))
				count = ArrayLength;

			return _owner.D3DEffect.GetValue<int>(_effectHandle, count);
		}

		/// <summary>
		/// Function to return a string value.
		/// </summary>
		/// <returns>A string value.</returns>
		public string GetString()
		{
			return _owner.D3DEffect.GetString(_effectHandle);
		}

		/// <summary>
		/// Function to return a 4D vector value.
		/// </summary>
		/// <returns>A 4D vector value.</returns>
		public Vector4D GetVector4D()
		{
			return Converter.Convert(_owner.D3DEffect.GetValue<DX.Vector4>(_effectHandle));
		}

		/// <summary>
		/// Function to return a 4D vector value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A 4D vector value array.</returns>
		public Vector4D[] GetVector4DArray(int count)
		{
			if (!IsArray)
				return null;

			// Force to array length.
			if ((count > ArrayLength) || (count < 1))
				count = ArrayLength;

			Vector4D[] vectors = new Vector4D[count];		// Gorgon vectors.
			DX.Vector4[] d3dVectors;						// D3D vectors.

			// Get matrices.
			d3dVectors = _owner.D3DEffect.GetValue<DX.Vector4>(_effectHandle, count);

			for (int i = 0; i < count; i++)
				vectors[i] = Converter.Convert(d3dVectors[i]);

			return vectors;
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
			Matrix result = Converter.Convert(_owner.D3DEffect.GetValue<DX.Matrix>(_effectHandle));
			if (transpose)
				result = result.Transpose;
			return result;
		}

		/// <summary>
		/// Function to return a matrix value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <param name="transpose">TRUE if returning a transposed matrix, FALSE if not.</param>
		/// <returns>A matrix value array.</returns>
		public Matrix[] GetMatrixArray(int count, bool transpose)
		{
			if (!IsArray)
				return null;

			// Force to array length.
			if ((count > ArrayLength) || (count < 1))
				count = ArrayLength;

			Matrix[] matrices = new Matrix[count];			// Gorgon matrix.
			DX.Matrix[] d3dMatrices;						// D3D matrices.

			// Get matrices.
			d3dMatrices = _owner.D3DEffect.GetValue<DX.Matrix>(_effectHandle, count);
			for (int i = 0; i < count; i++)
			{
				matrices[i] = Converter.Convert(d3dMatrices[i]);
				if (transpose)
					matrices[i] = matrices[i].Transpose;
			}

			return matrices;
		}

		/// <summary>
		/// Function to return a color value.
		/// </summary>
		/// <returns>A color value.</returns>
		public Color GetColor()
		{
			return _owner.D3DEffect.GetValue<DX.Color4>(_effectHandle).ToColor();
		}

		/// <summary>
		/// Function to return a color value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A color value array.</returns>
		public Color[] GetColorArray(int count)
		{
			if (!IsArray)
				return null;

			// Force to array length.
			if ((count > ArrayLength) || (count < 1))
				count = ArrayLength;

			Color[] colors = new Color[count];		// Drawing colors
			DX.Color4[] d3dColors;                  // D3D color values.

			// Get colors.
			d3dColors = _owner.D3DEffect.GetValue<DX.Color4>(_effectHandle, count);

			for (int i = 0; i < count; i++)
				colors[i] = d3dColors[i].ToColor();

			return colors;
		}

		/// <summary>
		/// Function to return an image value.
		/// </summary>
		/// <param name="sourceImage">Image to use to store the shader texture.</param>		
		public void GetImage(Image sourceImage)
		{
			if (sourceImage == null)
				throw new ArgumentNullException("sourceImage");

			// Bind to the image.
			sourceImage.D3DTexture = (D3D9.Texture)_owner.D3DEffect.GetTexture(_effectHandle);
		}

		/// <summary>
		/// Function to return an image value.
		/// </summary>
		/// <returns>An image value.</returns>
		public Image GetImage()
		{
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
			_owner.D3DEffect.SetValue(_effectHandle, value);
		}

		/// <summary>
		/// Function to set a boolean array parameter value.
		/// </summary>
		/// <param name="value">An array of boolean values.</param>
		public void SetValue(bool[] value)
		{
			_owner.D3DEffect.SetValue(_effectHandle, value);
		}

		/// <summary>
		/// Function to set a floating point parameter value.
		/// </summary>
		/// <param name="value">A floating point value.</param>
		public void SetValue(float value)
		{
			_owner.D3DEffect.SetValue(_effectHandle, value);
		}

		/// <summary>
		/// Function to set a floating point array parameter value.
		/// </summary>
		/// <param name="value">An array of floating point values.</param>
		public void SetValue(float[] value)
		{
			_owner.D3DEffect.SetValue(_effectHandle, value);
		}

		/// <summary>
		/// Function to set an integer parameter value.
		/// </summary>
		/// <param name="value">An integer value.</param>
		public void SetValue(int value)
		{
			_owner.D3DEffect.SetValue(_effectHandle, value);
		}

		/// <summary>
		/// Function to set an integer array parameter value.
		/// </summary>
		/// <param name="value">An array of integer values.</param>
		public void SetValue(int[] value)
		{
			_owner.D3DEffect.SetValue(_effectHandle, value);
		}

		/// <summary>
		/// Function to set a string parameter value.
		/// </summary>
		/// <param name="value">A string value.</param>
		public void SetValue(string value)
		{
			_owner.D3DEffect.SetString(_effectHandle, value);
		}

		/// <summary>
		/// Function to set a 4D vector parameter value.
		/// </summary>
		/// <param name="value">A 4D vector value.</param>
		public void SetValue(Vector4D value)
		{
			_owner.D3DEffect.SetValue(_effectHandle, Converter.Convert(value));
		}

		/// <summary>
		/// Function to set a 4D vector parameter array value.
		/// </summary>
		/// <param name="value">An array of 4D vector values.</param>
		public void SetValue(Vector4D[] value)
		{
			_owner.D3DEffect.SetValue(_effectHandle, Converter.Convert(value));
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
			if (transpose)
				_owner.D3DEffect.SetValue(_effectHandle, DX.Matrix.Transpose(Converter.Convert(value)));
			else
				_owner.D3DEffect.SetValue(_effectHandle, Converter.Convert(value));
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

			if (transpose)
			{
				foreach (Matrix matrix in value)
					SetValue(matrix, true);
			}
			else
				_owner.D3DEffect.SetValue(_effectHandle, Converter.Convert(value));
		}

		/// <summary>
		/// Function to set a color parameter value
		/// </summary>
		/// <param name="value">A color value.</param>
		public void SetValue(Color value)
		{
			_owner.D3DEffect.SetValue(_effectHandle, new DX.Color4(value));
		}

		/// <summary>
		/// Function to set an array of color parameter values
		/// </summary>
		/// <param name="value">An array of color values.</param>
		public void SetValue(Color[] value)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			
			DX.Color4[] colors = new DX.Color4[value.Length];		// Color values.

			// Convert the array.
			for (int i = 0; i < value.Length; i++)
				colors[i] = new DX.Color4(value[i]);

			_owner.D3DEffect.SetValue(_effectHandle, colors);
		}

		/// <summary>
		/// Function to set an image parameter value.
		/// </summary>
		/// <param name="image">Image to set.</param>
		public void SetValue(Image image)
		{
			if (image != null)
				_owner.D3DEffect.SetTexture(_effectHandle, image.D3DTexture);
			else
				_owner.D3DEffect.SetTexture(_effectHandle, null);
		}

		/// <summary>
		/// Function to set a render image parameter value.
		/// </summary>
		/// <param name="image">Render image to set.</param>
		public void SetValue(RenderImage image)
		{
			if (image != null)
				_owner.D3DEffect.SetTexture(_effectHandle, image.Image.D3DTexture);
			else
				_owner.D3DEffect.SetTexture(_effectHandle, null);
		}
		#endregion
		#endregion
	}
}
