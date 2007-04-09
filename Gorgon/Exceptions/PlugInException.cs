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
// Created: Monday, May 01, 2006 4:48:01 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;

namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// Plug in error codes.
	/// </summary>
	public enum PlugInErrors
	{
		/// <summary>Cannot create the plug-in.</summary>
		CannotCreatePlugIn = 0x7FFF0001,
		/// <summary>Cannot load the specified module.</summary>
		CannotLoadModule = 0x7FFF0002,
		/// <summary>Module was not loaded.</summary>
		ModuleNotFound = 0x7FFF0003,
		/// <summary>Plug in not found.</summary>
		PlugInNotFound = 0x7FFF0004,
		/// <summary>Module is not a plug-in module.</summary>
		NotAPlugInModule = 0x7FFF0005,
        /// <summary>Invalid parameters passed to the plug-in.</summary>
        InvalidParameters = 0x7FFF0006
	}

	/// <summary>
	/// Base plug-in exception class.
	/// </summary>
	public abstract class PlugInException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Source exception.</param>
		public PlugInException(string message, PlugInErrors code, Exception ex)
			: base(message, (int)code, ex)
		{
			_errorCodeType = code.GetType();
		}
		#endregion
	}

	/// <summary>
	/// Cannot create plug-in exception.
	/// </summary>
	public class CannotCreatePlugInException 
		: PlugInException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in.</param>
		/// <param name="ex">Source exception.</param>
		public CannotCreatePlugInException(string plugInName,Exception ex)
			: base("Cannot create plug-in '" + plugInName + "'.", PlugInErrors.CannotCreatePlugIn,ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot load module exception.
	/// </summary>
	public class CannotLoadModuleException 
		: PlugInException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="moduleName">Name of the plug-in module.</param>
		/// <param name="ex">Source exception.</param>
		public CannotLoadModuleException(string moduleName, Exception ex)
			: base("Cannot load module '" + moduleName + "'.", PlugInErrors.CannotLoadModule, ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Module not found exception.
	/// </summary>
	public class ModuleNotFoundException 
		: PlugInException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="moduleName">Name of the plug-in module.</param>
		/// <param name="ex">Source exception.</param>
		public ModuleNotFoundException(string moduleName, Exception ex)
			: base("Module '" + moduleName + "' was not loaded.", PlugInErrors.ModuleNotFound, ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Plug-in not found exception.
	/// </summary>
	public class PlugInNotFoundException 
		: PlugInException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in.</param>
		/// <param name="ex">Source exception.</param>
		public PlugInNotFoundException(string plugInName, Exception ex)
			: base("Plug-in '" + plugInName + "' was not found in the module.", PlugInErrors.PlugInNotFound, ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Not a plug-in exception.
	/// </summary>
	public class NotAPlugInException 
		: PlugInException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="moduleName">Module name.</param>
		/// <param name="ex">Source exception.</param>
		public NotAPlugInException(string moduleName, Exception ex)
			: base("'" + moduleName + "' is not a valid plug-in module.", PlugInErrors.NotAPlugInModule, ex)
		{
		}
		#endregion
	}

    /// <summary>
    /// Invalid plug-in parameters exception.
    /// </summary>
    public class InvalidPlugInParametersException
        : PlugInException
    {
        #region Constructor.
        /// <summary>
        /// Constructor.
        /// </summary>        
        /// <param name="plugInName">Name of the plug-in.</param>
        /// <param name="error">Source of the error.</param>
        /// <param name="ex">Source exception.</param>
        public InvalidPlugInParametersException(string plugInName, string error, Exception ex)
            : base("'" + plugInName + "' was passed invalid parameters.\n" + error, PlugInErrors.InvalidParameters, ex)
        {
        }
        #endregion
    }
}
