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
using Gorgon.Graphics.Core.Properties;
using D3D = SharpDX.Direct3D;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A macro for a shader.
	/// </summary>
	public readonly struct GorgonShaderMacro
		: IGorgonNamedObject, IEquatable<GorgonShaderMacro>
	{
		/// <summary>
		/// The Direct 3D shader macro wrapped by this type.
		/// </summary>
		internal readonly D3D.ShaderMacro D3DShaderMacro;

		/// <summary>
		/// Property to return the name of the macro.
		/// </summary>
		public string Name => D3DShaderMacro.Name;

		/// <summary>
		/// Property to return the value for the macro.
		/// </summary>
		public object Value => D3DShaderMacro.Definition;

		/// <summary>
		/// Property to return the name of the named object.
		/// </summary>
		string IGorgonNamedObject.Name => Name;

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => string.Format(Resources.GORGFX_TOSTR_SHADER_MACRO, Name);

        /// <summary>
        /// Function to determine if two instances are equal or not.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool Equals(GorgonShaderMacro left, GorgonShaderMacro right) => string.Equals(left.Name, right.Name, StringComparison.Ordinal);

        /// <summary>
        /// Determines whether the specified <see cref="object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <b>true</b> if the specified <see cref="object" /> is equal to this instance; otherwise, <b>false</b>.
        /// </returns>
        public override bool Equals(object obj) => obj is GorgonShaderMacro macro ? macro.Equals(this) : base.Equals(obj);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => Name.GetHashCode();

        /// <summary>
        /// Operator to compare two instances for equality.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
        public static bool operator ==(GorgonShaderMacro left, GorgonShaderMacro right) => Equals(left, right);

        /// <summary>
        /// Operator to compare two instances for inequality.
        /// </summary>
        /// <param name="left">Left instance to compare.</param>
        /// <param name="right">Right instance to compare.</param>
        /// <returns><b>true</b> if not equal, <b>false</b> if equal.</returns>
        public static bool operator !=(GorgonShaderMacro left, GorgonShaderMacro right) => !Equals(left, right);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(GorgonShaderMacro other) => Equals(this, other);

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonShaderMacro"/> struct.
        /// </summary>
        /// <param name="name">The name of the macro.</param>
        /// <param name="value">[Optional] The value for the macro.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        public GorgonShaderMacro(string name, string value = null)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if (name.Length == 0)
			{
				throw new ArgumentEmptyException(nameof(name));
			}

			D3DShaderMacro = new D3D.ShaderMacro(name, value);
		}
	}
}
