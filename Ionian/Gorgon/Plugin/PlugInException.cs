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

namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// Cannot create plug-in.
	/// </summary>
	public class PlugInCannotCreateException 
		: CannotCreateException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in.</param>
		/// <param name="ex">Source exception.</param>
		public PlugInCannotCreateException(string plugInName, Exception ex)
			: base("Cannot create plug-in '" + plugInName + "'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in.</param>
		public PlugInCannotCreateException(string plugInName)
			: this(plugInName, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Cannot load module.
	/// </summary>
	public class ModuleCannotLoadException 
		: CannotLoadException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="moduleName">Name of the plug-in module.</param>
		/// <param name="ex">Source exception.</param>
		public ModuleCannotLoadException(string moduleName, Exception ex)
			: base("Cannot load module '" + moduleName + "'.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="moduleName">Name of the plug-in module.</param>
		public ModuleCannotLoadException(string moduleName)
			: this(moduleName, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Module not found exception.
	/// </summary>
	public class ModuleNotFoundException 
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="moduleName">Name of the plug-in module.</param>
		/// <param name="ex">Source exception.</param>
		public ModuleNotFoundException(string moduleName, Exception ex)
			: base("Module '" + moduleName + "' was not loaded.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="moduleName">Name of the plug-in module.</param>
		public ModuleNotFoundException(string moduleName)
			: this(moduleName, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Plug-in not found exception.
	/// </summary>
	public class PlugInNotFoundException 
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in.</param>
		/// <param name="ex">Source exception.</param>
		public PlugInNotFoundException(string plugInName, Exception ex)
			: base("Plug-in '" + plugInName + "' was not found in the module.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in.</param>
		public PlugInNotFoundException(string plugInName)
			: this(plugInName, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Not a valid plug-in.
	/// </summary>
	public class PlugInNotValidException 
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="moduleName">Module name.</param>
		/// <param name="ex">Source exception.</param>
		public PlugInNotValidException(string moduleName, Exception ex)
			: base("'" + moduleName + "' is not a valid plug-in module.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="moduleName">Module name.</param>
		public PlugInNotValidException(string moduleName)
			: this(moduleName, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Plug-in not signed or not signed correctly.
	/// </summary>
	public class PlugInSigningException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PlugInSigningException"/> class.
		/// </summary>
		/// <param name="moduleName">Name of the module that caused the signing exception.</param>
		/// <param name="ex">Inner exception.</param>
		public PlugInSigningException(string moduleName, Exception ex)
			: base("Plug-in '" + moduleName + "' is not signed or not signed with a valid key.", ex)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PlugInSigningException"/> class.
		/// </summary>
		/// <param name="moduleName">Name of the module that caused the signing exception.</param>
		public PlugInSigningException(string moduleName)
			: this(moduleName, null)
		{
		}
		#endregion
	}

	/// <summary>
	/// Plug-in already loaded exception.
	/// </summary>
	public class PlugInAlreadyLoadedException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in that is already loaded.</param>
		/// <param name="ex">Source exception.</param>
		public PlugInAlreadyLoadedException(string plugInName, Exception ex)
			: base("'" + plugInName + "' is already loaded.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="plugInName">Name of the plug-in that is already loaded.</param>
		public PlugInAlreadyLoadedException(string plugInName)
			: this(plugInName, null)
		{
		}
		#endregion
	}
}
