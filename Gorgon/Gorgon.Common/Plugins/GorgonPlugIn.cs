﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, June 23, 2011 11:24:37 AM
// 
#endregion

using System.Reflection;
using Gorgon.Core;

namespace Gorgon.Plugins
{
	/// <summary>
	/// A base plug-in entry point object.
	/// </summary>
	/// <remarks>Plug-ins must implement this object as a proxy to create the actual concrete implementation object.</remarks>
	public abstract class GorgonPlugIn
		: INamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the assembly that contains this plug-in.
		/// </summary>
		public AssemblyName Assembly
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the path to the plug-in assembly.
		/// </summary>
		public string PlugInPath
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the description of the plug-in.
		/// </summary>
		public string Description
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonPlugIn"/> class.
		/// </summary>
		/// <param name="description">Optional description of the plug-in.</param>
		/// <remarks>Objects that implement this base class should pass in a hard coded description on the base constructor.</remarks>
		protected GorgonPlugIn(string description)
		{
			Description = description ?? string.Empty;

			Assembly = GetType().Assembly.GetName();
			PlugInPath = GetType().Assembly.ManifestModule.FullyQualifiedName;
			Name = GetType().FullName;
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		public string Name
		{
			get;
			private set;
		}
		#endregion
	}
}
