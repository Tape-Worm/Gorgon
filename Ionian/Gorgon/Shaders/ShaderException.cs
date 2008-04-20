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
