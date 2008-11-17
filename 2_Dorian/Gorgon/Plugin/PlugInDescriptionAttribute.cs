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
// Created: Saturday, November 15, 2008 1:01:06 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// Attribute used to describe a plug-in.
	/// </summary>
	/// <remarks>Place this attribute over any class that inherits from <see cref="GorgonLibrary.PlugIns.PlugIn">PlugIn</see>.</remarks>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class PlugInDescriptionAttribute
		: Attribute
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the name of the plug-in.
		/// </summary>
		public string PlugInName
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the description of a plug-in.
		/// </summary>
		public string Description
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the type of plug-in.
		/// </summary>
		public PlugInType PlugInType
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PlugInDescriptionAttribute"/> class.
		/// </summary>
		/// <param name="name">The name of the plug-in.</param>
		/// <param name="type">The type of plug-in.</param>
		public PlugInDescriptionAttribute(string name, PlugInType type)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			PlugInName = name;
			PlugInType = type;
		}
		#endregion
	}
}
