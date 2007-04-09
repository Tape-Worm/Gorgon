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
using SharpUtilities;

namespace GorgonLibrary.Graphics.Shaders
{
	/// <summary>
	/// Shader error codes.
	/// </summary>
	public enum ShaderErrors
	{
		/// <summary>Cannot retrieve the techniques from the effect.</summary>
		CannotRetrieveTechniques = 0x7FFF0001,
		/// <summary>Cannot retrieve the passes from the effect.</summary>
		CannotRetrievePasses = 0x7FFF0002,
		/// <summary>No techniques within the effect.</summary>
		NoTechniques = 0x7FFF0003,
		/// <summary>No passes within the technique.</summary>
		NoPasses = 0x7FFF0004,
		/// <summary>Cannot retrieve parameters.</summary>
		CannotRetrieveParameters = 0x7FFF0005
	}

	/// <summary>
	/// Base exception for shader exceptions.
	/// </summary>
	public abstract class ShaderException
		: ResourceException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Source exception.</param>
		public ShaderException(string message, ShaderErrors code, Exception ex)
			: base(message, (int)code, ex)
		{
			_errorCodeType = code.GetType();
		}
		#endregion
	}

	/// <summary>
	/// Cannot retrieve techniques exception.
	/// </summary>
	public class CannotRetrieveTechniquesException
		:ShaderException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shaderName">Name of the shader.</param>
		/// <param name="ex">Source exception.</param>
		public CannotRetrieveTechniquesException(string shaderName, Exception ex)
			: base("Could not retrieve the list of techniques from the shader: '" + shaderName +"'.", ShaderErrors.CannotRetrieveTechniques, ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot retrieve passes exception.
	/// </summary>
	public class CannotRetrievePassesException
		:ShaderException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="technique">Current technique.</param>
		/// <param name="ex">Source exception.</param>
		public CannotRetrievePassesException(ShaderTechnique technique, Exception ex)
			: base("Could not retrieve the list of passes from the technique: '" + technique.Name + "'.", ShaderErrors.CannotRetrievePasses, ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// No techniques exception.
	/// </summary>
	public class NoTechniquesException
		:ShaderException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shaderName">Name of the shader.</param>
		/// <param name="ex">Inner exception.</param>
		public NoTechniquesException(string shaderName, Exception ex)
			: base("There were no techniques within the shader: '" + shaderName + "'.", ShaderErrors.NoTechniques,ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// No passes exception.
	/// </summary>
	public class NoPassesException
		:ShaderException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="technique">Technique that is missing the passes.</param>
		/// <param name="ex">Inner exception.</param>
		public NoPassesException(ShaderTechnique technique, Exception ex)
			: base("There were no passes within the technique: '" + technique.Name + "'.", ShaderErrors.NoPasses,ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot retrieve techniques exception.
	/// </summary>
	public class CannotRetrieveParametersException
		:ShaderException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shaderName">Name of the shader.</param>
		/// <param name="ex">Source exception.</param>
		public CannotRetrieveParametersException(string shaderName, Exception ex)
			: base("Could not retrieve the list of parameters from the shader: '" + shaderName + "'.", ShaderErrors.CannotRetrieveParameters, ex)
		{
		}
		#endregion
	}
}
