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
// Created: Monday, April 30, 2012 9:00:05 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.GorgonEditor
{
	/// <summary>
	/// Interface type for the plug-in.
	/// </summary>
	public enum EditorPlugInInterfaceType
	{
		/// <summary>
		/// Embedded panel.
		/// </summary>
		Panel = 0,
		/// <summary>
		/// Menu item.
		/// </summary>
		MenuItem = 1
	}

	/// <summary>
	/// Plug-in interface for the editor plug-ins.
	/// </summary>
	public abstract class GorgonEditorPlugIn
		: GorgonPlugIn
	{
		#region Properties.
		/// <summary>
		/// Property to return a detailed description of the editor plug-in.
		/// </summary>
		public abstract string DetailedDescription
		{
			get;
		}

		/// <summary>
		/// Property to return the name of the author of the plug-in.
		/// </summary>
		public abstract string Author
		{
			get;
		}

		/// <summary>
		/// Property to return the category for the editor.
		/// </summary>
		public abstract string EditorCategory
		{
			get;
		}

		/// <summary>
		/// Property to return the type of interface element this plug-in will represent.
		/// </summary>
		public abstract EditorPlugInInterfaceType InterfaceType
		{
			get;
		}
		#endregion

		#region Methods.
		
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonEditorPlugIn"/> class.
		/// </summary>
		/// <param name="description">Optional description of the plug-in.</param>
		protected GorgonEditorPlugIn(string description)
			: base(description)
		{			
		}
		#endregion
	}
}
