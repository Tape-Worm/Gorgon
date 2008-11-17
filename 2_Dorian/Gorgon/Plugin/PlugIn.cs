#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Friday, November 14, 2008 10:29:18 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// Type of plug-in.
	/// </summary>
	public enum PlugInType
	{
		/// <summary>
		/// Plug-in is a file system plug-in.
		/// </summary>
		FileSystem = 1,
		/// <summary>
		/// Plug-in is a renderer.
		/// </summary>
		Renderer = 2,
		/// <summary>
		/// Plug-in is a platform plug-in.
		/// </summary>
		Platform = 3,
		/// <summary>
		/// Plug-in is an input plug-in.
		/// </summary>
		Input = 4,
		/// <summary>
		/// Plug-in is user-defined.
		/// </summary>
		User = 0x7FFFFFF0
	}

	/// <summary>
	/// Abstract class representing a plug-in entry point.
	/// </summary>	
	public abstract class PlugIn
		: NamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the plug-in type.
		/// </summary>
		public PlugInType PlugInType
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the description of this plug-in.
		/// </summary>
		public string Description
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the plug-in path.
		/// </summary>
		public string PlugInPath
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the plug-in meta data.
		/// </summary>
		private void GetMetaData()
		{
			PlugInDescriptionAttribute[] attributes = null;			// List of plug-in attributes.

			attributes = GetType().GetCustomAttributes(typeof(PlugInDescriptionAttribute), false) as PlugInDescriptionAttribute[];

			if ((attributes == null) || (attributes.Length == 0))
				throw new GorgonException(GorgonErrors.InvalidPlugin, "A plug-in in '" + PlugInPath + "' does not contain any plug-ins marked with PlugInDescriptionAttribute");

			if (string.IsNullOrEmpty(attributes[0].PlugInName))
				throw new GorgonException(GorgonErrors.InvalidPlugin, "A plug-in in '" + PlugInPath + "' has no name.");

			Name = attributes[0].PlugInName;
			Description = attributes[0].Description;
			PlugInType = attributes[0].PlugInType;
		}

		/// <summary>
		/// Function to perform clean up when this plug-in is unloaded.
		/// </summary>
		protected internal virtual void Unload()
		{			
		}

		/// <summary>
		/// Function to create a new object from the plug-in.
		/// </summary>
		/// <param name="parameters">Parameters to pass.</param>
		/// <returns>The new object.</returns>
		protected abstract internal object CreateImplementation(object[] parameters);
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PlugIn"/> class.
		/// </summary>
		/// <param name="plugInPath">Path to the plug-in.</param>
		protected PlugIn(string plugInPath)
			: base("PLUGIN")
		{
			PlugInPath = plugInPath;
			GetMetaData();
		}
		#endregion
	}
}
