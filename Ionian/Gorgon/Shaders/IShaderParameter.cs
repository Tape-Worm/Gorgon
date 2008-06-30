﻿#region LGPL.
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
// Created: Sunday, June 29, 2008 12:22:14 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Interface defining a shader parameter.
	/// </summary>
	public interface IShaderParameter
	{
		#region Properties.
		/// <summary>
		/// Property to return the name of the shader parameter.
		/// </summary>
		string Name
		{
			get;
		}

		/// <summary>
		/// Property to return the size of an array parameter.
		/// </summary>
		int ArrayLength
		{
			get;
		}

		/// <summary>
		/// Property to return the type of the value.
		/// </summary>
		ShaderParameterType ValueType
		{
			get;
		}

		/// <summary>
		/// Property to return the owning shader for this parameter.
		/// </summary>
		Shader Owner
		{
			get;
		}

		/// <summary>
		/// Property to return whether this value is an array or not.
		/// </summary>
		bool IsArray
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return a boolean value.
		/// </summary>
		/// <returns>A boolean value.</returns>
		bool GetBoolean();

		/// <summary>
		/// Function to return a boolean value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A boolean value array.</returns>
		bool[] GetBooleanArray(int count);

		/// <summary>
		/// Function to return a floating point value.
		/// </summary>
		/// <returns>A boolean value.</returns>
		float GetFloat();

		/// <summary>
		/// Function to return a floating point value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A floating point value array.</returns>
		float[] GetFloatArray(int count);

		/// <summary>
		/// Function to return an integer value.
		/// </summary>
		/// <returns>An integer value.</returns>
		int GetInteger();

		/// <summary>
		/// Function to return an integer value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>An integer value array.</returns>
		int[] GetIntegerArray(int count);

		/// <summary>
		/// Function to return a string value.
		/// </summary>
		/// <returns>A string value.</returns>
		string GetString();

		/// <summary>
		/// Function to return a 4D vector value.
		/// </summary>
		/// <returns>A 4D vector value.</returns>
		Vector4D GetVector4D();

		/// <summary>
		/// Function to return a 4D vector value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A 4D vector value array.</returns>
		Vector4D[] GetVector4DArray(int count);

		/// <summary>
		/// Function to return a matrix value.
		/// </summary>
		/// <returns>A matrix value.</returns>
		Matrix GetMatrix();

		/// <summary>
		/// Function to return a matrix value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A matrix value array.</returns>
		Matrix[] GetMatrixArray(int count);

		/// <summary>
		/// Function to return a matrix value.
		/// </summary>
		/// <param name="transpose">TRUE if returning a transposed matrix, FALSE if not.</param>
		/// <returns>A matrix value.</returns>
		Matrix GetMatrix(bool transpose);

		/// <summary>
		/// Function to return a matrix value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <param name="transpose">TRUE if returning a transposed matrix, FALSE if not.</param>
		/// <returns>A matrix value array.</returns>
		Matrix[] GetMatrixArray(int count, bool transpose);

		/// <summary>
		/// Function to return a color value.
		/// </summary>
		/// <returns>A color value.</returns>
		System.Drawing.Color GetColor();

		/// <summary>
		/// Function to return a color value array.
		/// </summary>
		/// <param name="count">Number of items to return.</param>
		/// <returns>A color value array.</returns>
		System.Drawing.Color[] GetColorArray(int count);

		/// <summary>
		/// Function to return an image value.
		/// </summary>
		/// <param name="sourceImage">Image to use to store the shader texture.</param>		
		void GetImage(Image sourceImage);

		/// <summary>
		/// Function to return an image value.
		/// </summary>
		/// <returns>An image value.</returns>
		Image GetImage();

		/// <summary>
		/// Function to set a boolean parameter value.
		/// </summary>
		/// <param name="value">A boolean value.</param>
		void SetValue(bool value);

		/// <summary>
		/// Function to set a boolean array parameter value.
		/// </summary>
		/// <param name="value">An array of boolean values.</param>
		void SetValue(bool[] value);

		/// <summary>
		/// Function to set a floating point parameter value.
		/// </summary>
		/// <param name="value">A floating point value.</param>
		void SetValue(float value);

		/// <summary>
		/// Function to set a floating point array parameter value.
		/// </summary>
		/// <param name="value">An array of floating point values.</param>
		void SetValue(float[] value);

		/// <summary>
		/// Function to set an integer parameter value.
		/// </summary>
		/// <param name="value">An integer value.</param>
		void SetValue(int value);

		/// <summary>
		/// Function to set an integer array parameter value.
		/// </summary>
		/// <param name="value">An array of integer values.</param>
		void SetValue(int[] value);

		/// <summary>
		/// Function to set a string parameter value.
		/// </summary>
		/// <param name="value">A string value.</param>
		void SetValue(string value);

		/// <summary>
		/// Function to set a 4D vector parameter value.
		/// </summary>
		/// <param name="value">A 4D vector value.</param>
		void SetValue(Vector4D value);

		/// <summary>
		/// Function to set a 4D vector parameter array value.
		/// </summary>
		/// <param name="value">An array of 4D vector values.</param>
		void SetValue(Vector4D[] value);

		/// <summary>
		/// Function to set a matrix value.
		/// </summary>
		/// <param name="value">A matrix value.</param>
		void SetValue(Matrix value);

		/// <summary>
		/// Function to set an array of matrix values.
		/// </summary>
		/// <param name="value">An array of matrix values.</param>
		void SetValue(Matrix[] value);

		/// <summary>
		/// Function to set a matrix value with option for transposed matrix.
		/// </summary>
		/// <param name="value">A matrix value.</param>
		/// <param name="transpose">TRUE to use transpose of the matrix, FALSE for standard matrix.</param>
		void SetValue(Matrix value, bool transpose);

		/// <summary>
		/// Function to set a matrix value with option for transposed matrix.
		/// </summary>
		/// <param name="value">An array of matrix values.</param>
		/// <param name="transpose">TRUE to use transpose of the matrix, FALSE for standard matrix.</param>
		void SetValue(Matrix[] value, bool transpose);

		/// <summary>
		/// Function to set a color parameter value
		/// </summary>
		/// <param name="value">A color value.</param>
		void SetValue(System.Drawing.Color value);

		/// <summary>
		/// Function to set an array of color parameter values
		/// </summary>
		/// <param name="value">An array of color values.</param>
		void SetValue(System.Drawing.Color[] value);

		/// <summary>
		/// Function to set an image parameter value.
		/// </summary>
		/// <param name="image">Image to set.</param>
		void SetValue(Image image);

		/// <summary>
		/// Function to set a render image parameter value.
		/// </summary>
		/// <param name="image">Render image to set.</param>
		void SetValue(RenderImage image);
		#endregion
	}
}
