#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, August 24, 2013 11:01:08 AM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using D3DCommon = SharpDX.Direct3D;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A macro for a shader.
	/// </summary>
	public struct GorgonShaderMacro
		: IGorgonNamedObject, IEquatable<GorgonShaderMacro>
	{
		/// <summary>
		/// Name of the macro.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// Value for the macro.
		/// </summary>
		public readonly object Value;

		/// <summary>
		/// Function to convert this macro object into a D3D macro object.
		/// </summary>
		/// <returns>The D3D macro object.</returns>
		internal D3DCommon.ShaderMacro Convert()
		{
			return new D3DCommon.ShaderMacro(Name, Value);
		}

		/// <summary>
		/// Returns a <see cref="string" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="string" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GORGFX_SHADER_MACRO_TOSTR, Name);
		}

		/// <summary>
		/// Function to determine if two instances are equal or not.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonShaderMacro left, ref GorgonShaderMacro right)
		{
			return string.Equals(left.Name, right.Name, StringComparison.Ordinal);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/>, is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonShaderMacro)
			{
				return ((GorgonShaderMacro)obj).Equals(this);
			}
			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		/// <summary>
		/// Operator to compare two instances for equality.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool operator ==(GorgonShaderMacro left, GorgonShaderMacro right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Operator to compare two instances for inequality.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
		public static bool operator !=(GorgonShaderMacro left, GorgonShaderMacro right)
		{
			return !Equals(ref left, ref right);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderMacro"/> struct.
		/// </summary>
		/// <param name="name">The name of the macro.</param>
		/// <param name="value">[Optional] The value for the macro.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
		public GorgonShaderMacro(string name, string value = null)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (name.Length == 0)
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(name));
			}

			Name = name;
			Value = value;
		}

		#region IGorgonNamedObject Members
		/// <summary>
		/// Property to return the name of the named object.
		/// </summary>
		string IGorgonNamedObject.Name => Name;

		#endregion

		#region IEquatable<GorgonShaderMacro> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonShaderMacro other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
