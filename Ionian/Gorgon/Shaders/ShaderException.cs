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
// Created: Saturday, March 25, 2006 9:37:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Cannot retrieve techniques.
	/// </summary>
	public class ShaderCannotGetTechniquesException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shaderName">Name of the shader.</param>
		/// <param name="ex">Source exception.</param>
		public ShaderCannotGetTechniquesException(string shaderName, Exception ex)
			: base("Could not retrieve the list of techniques from the shader: '" + shaderName +"'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shaderName">Name of the shader.</param>
		public ShaderCannotGetTechniquesException(string shaderName)
			: base("Could not retrieve the list of techniques from the shader: '" + shaderName + "'.", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot retrieve passes.
	/// </summary>
	public class ShaderCannotGetPassesException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="technique">Current technique.</param>
		/// <param name="ex">Source exception.</param>
		public ShaderCannotGetPassesException(ShaderTechnique technique, Exception ex)
			: base("Could not retrieve the list of passes from the technique: '" + technique.Name + "'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="technique">Current technique.</param>
		public ShaderCannotGetPassesException(ShaderTechnique technique)
			: base("Could not retrieve the list of passes from the technique: '" + technique.Name + "'.", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Shader no techniques.
	/// </summary>
	public class ShaderHasNoTechniquesException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shaderName">Name of the shader.</param>
		/// <param name="ex">Inner exception.</param>
		public ShaderHasNoTechniquesException(string shaderName, Exception ex)
			: base("There were no techniques within the shader: '" + shaderName + "'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shaderName">Name of the shader.</param>
		public ShaderHasNoTechniquesException(string shaderName)
			: base("There were no techniques within the shader: '" + shaderName + "'.", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot retrieve techniques.
	/// </summary>
	public class ShaderCannotGetParametersException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shaderName">Name of the shader.</param>
		/// <param name="ex">Source exception.</param>
		public ShaderCannotGetParametersException(string shaderName, Exception ex)
			: base("Could not retrieve the list of parameters from the shader: '" + shaderName + "'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shaderName">Name of the shader.</param>
		public ShaderCannotGetParametersException(string shaderName)
			: base("Could not retrieve the list of parameters from the shader: '" + shaderName + "'.", null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Shader is invalid.
	/// </summary>
	public class ShaderNotValidException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public ShaderNotValidException(Exception ex)
			: base("This is not a valid shader.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public ShaderNotValidException()
			: this(null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Shader already exists.
	/// </summary>
	public class ShaderAlreadyExistsException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the shader.</param>
		/// <param name="ex">Source exception.</param>
		public ShaderAlreadyExistsException(string name, Exception ex)
			: base("The shader '" + name + "' is already loaded.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the shader.</param>
		public ShaderAlreadyExistsException(string name)
			: this(name, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Shader compile error.
	/// </summary>
	public class ShaderCompilerErrorException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the shader.</param>
		/// <param name="errors">List of errors from the shader compiler.</param>
		/// <param name="ex">Source exception.</param>
		public ShaderCompilerErrorException(string name, string errors, Exception ex)
			: base("The shader '" + name + "' could not be compiled.\nErrors:\n" + errors, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the shader.</param>
		/// <param name="errors">List of errors from the shader compiler.</param>
		public ShaderCompilerErrorException(string name, string errors)
			: this(name, errors, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Shader cannot create texture shader.
	/// </summary>
	public class ShaderCannotCreateTextureShaderException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="errors">The list of errors from the compiler.</param>
		/// <param name="ex">Source exception.</param>
		public ShaderCannotCreateTextureShaderException(string errors, Exception ex)
			: base("Unable to create the texture shader:\n" + errors + ".", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="errors">The list of errors from the compiler.</param>
		public ShaderCannotCreateTextureShaderException(string errors)
			: this(errors, null)
		{
		}
		#endregion
	}
}
