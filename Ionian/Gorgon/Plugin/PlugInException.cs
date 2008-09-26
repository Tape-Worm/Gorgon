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
		: GorgonException
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
		: GorgonException
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
