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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// An include file for a shader.
	/// </summary>
	/// <remarks>Use this object to load in #include definitions for a shader.  If the shader source contains an #include, it will try to locate that include file on the file system.  However, this does not work when the files are 
	/// loaded from a stream object (it wouldn't know where to find the include file).  So to facilitate this, this object will contain the source for the include file and will be looked up -before- the file system is 
	/// checked for the include file.
	/// <para>Gorgon does not use the #include keyword for HLSL, Ss it will not interfere with it.  However a new keyword must be used: '#GorgonInclude "&lt;include name&gt;"[, "&lt;include path&gt;"]'.  This keyword takes 2 parameters 
	/// unlike the 1 parameter for #include.  The first parameter is the name of the include file, this is defined by the user.  The second parameter is the path to the include file.  The first parameter is required, but the second is optional.</para>
	/// <para>The include object is only for shaders with source code, therefore, the objects will be ignored when used with a binary shader.  Binary shaders should already have the required information in them.</para>
	/// </remarks>
	public struct GorgonShaderInclude
		: INamedObject
	{
		#region Variables.
		private string _name;			// Name for the include file.
		private string _sourceCode;		// Source code for the include file.
		#endregion

		#region Properties.		
		/// <summary>
		/// Property to return the source code for the include file.
		/// </summary>
		public string SourceCode
		{
			get
			{
				return _sourceCode;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderInclude"/> struct.
		/// </summary>
		/// <param name="includeName">Name of the include file.</param>
		/// <param name="includeSource">The include source code.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="includeName"/> parameters is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the includeName parameter is empty.</exception>
		public GorgonShaderInclude(string includeName, string includeSource)
		{
			GorgonDebug.AssertParamString(includeName, "includeName");

			_name = includeName;
			_sourceCode = includeSource;
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of the include file.
		/// </summary>
		public string Name
		{
			get 
			{
				return _name;
			}
		}
		#endregion
	}
}
