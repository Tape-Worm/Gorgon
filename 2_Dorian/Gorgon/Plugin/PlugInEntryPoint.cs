#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Saturday, April 07, 2007 3:09:03 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.Internal;

namespace GorgonLibrary.PlugIns
{
	/// <summary>
	/// Abstract class representing a plug-in entry point.
	/// </summary>
	[PlugIn()]
	public abstract class PlugInEntryPoint
		: NamedObject
	{
		#region Variables.
		private PlugInType _type = PlugInType.UserDefined;		// Plug-in type.
		private string _plugInPath = string.Empty;				// Plug-in path.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the plug-in type.
		/// </summary>
		public PlugInType PlugInType
		{
			get
			{
				return _type;
			}
		}

		/// <summary>
		/// Property to return the plug-in path.
		/// </summary>
		public string PlugInPath
		{
			get
			{
				return _plugInPath;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a new object from the plug-in.
		/// </summary>
		/// <param name="parameters">Parameters to pass.</param>
		/// <returns>The new object.</returns>
		protected abstract internal object CreateImplementation(object[] parameters);

        /// <summary>
        /// Function to return an interface to Gorgon's Direct 3D objects.
        /// </summary>
        /// <returns>An instance of the D3D objects interface.</returns>
        protected D3DObjects GetD3DObjects()
        {
            return new D3DObjects();
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		protected PlugInEntryPoint(string name, string plugInPath, PlugInType type)
			: base(name)
		{
			_plugInPath = plugInPath;
			_type = type;
		}
		#endregion
	}
}
