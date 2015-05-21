#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Sunday, March 18, 2012 11:24:50 AM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Properties;

namespace Gorgon.Graphics
{
	/// <summary>
	/// An include file for a shader.
	/// </summary>
	/// <remarks>Use this object to load in included external functions for a shader.  If the shader source contains an #include, it will try to locate that include file on the file system.  However, this does not work 
	/// when the files are loaded from a stream object (it wouldn't know where to find the include file).  So to facilitate this, this object will contain the source for the include file and will be looked up 
	/// -before- the file system is checked for the include file.
	/// <para>Gorgon does not use the #include keyword for HLSL, so it will not interfere with it and it will operate as expected.  However a new keyword must be used: 
	/// '#GorgonInclude "&lt;include name&gt;"[, "&lt;include path&gt;"]'.  This keyword takes 2 parameters unlike the 1 parameter for #include.  The first parameter is the name of the include file, this is defined by 
	/// the user and is the name of the include file in the <see cref="GorgonShaderBinding.IncludeFiles">include collection</see>. The second parameter is the path to the include file.  The first parameter is required, 
	/// but the second is optional.</para>
	/// <para>The include object is only for shaders with source code, therefore, the objects will be ignored when used with a binary shader.  Binary shaders should already have the required information in them.</para>
	/// </remarks>
	public struct GorgonShaderInclude
		: INamedObject, IEquatable<GorgonShaderInclude>
	{
		#region Variables.
		/// <summary>
		/// The name of the shader include file.
		/// </summary>
		public readonly string Name;
		/// <summary>
		/// The source code for the shader include file.
		/// </summary>
		public readonly string SourceCodeFile;
		#endregion

		#region Methods.
		/// <summary>
		/// Returns a <see cref="System.String" /> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="System.String" /> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			return string.Format(Resources.GORGFX_SHADER_INCLUDE_TOSTR, Name);
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
		/// Function to evaluate two instances for equality.
		/// </summary>
		/// <param name="left">The left instance to compare.</param>
		/// <param name="right">The right instance to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool Equals(ref GorgonShaderInclude left, ref GorgonShaderInclude right)
		{
			return (string.Equals(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonShaderInclude)
			{
				return Equals((GorgonShaderInclude)obj);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Equality operator.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if equal, FALSE if not.</returns>
		public static bool operator ==(GorgonShaderInclude left, GorgonShaderInclude right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns>TRUE if not equal, FALSE if equal.</returns>
		public static bool operator !=(GorgonShaderInclude left, GorgonShaderInclude right)
		{
			return !Equals(ref left, ref right);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderInclude"/> struct.
		/// </summary>
		/// <param name="includeName">Name of the include file.</param>
		/// <param name="includeSourceFile">The include source code file.</param>
		/// <remarks>The <paramref name="includeSourceFile"/> can be set to NULL (Nothing in VB.Net) or empty if the include line is pointing to a file.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="includeName"/> parameters is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the includeName parameter is empty.</exception>
		public GorgonShaderInclude(string includeName, string includeSourceFile)
		{
			if (includeName == null)
			{
				throw new ArgumentNullException("includeName");
			}

			if (string.IsNullOrWhiteSpace(includeName))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, "includeName");
			}

			if (includeSourceFile == null)
			{
				includeSourceFile = string.Empty;
			}

			Name = includeName;
			SourceCodeFile = includeSourceFile;
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of the include file.
		/// </summary>
		string INamedObject.Name
		{
			get 
			{
				return Name;
			}
		}
		#endregion

		#region IEquatable<GorgonShaderInclude> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonShaderInclude other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
