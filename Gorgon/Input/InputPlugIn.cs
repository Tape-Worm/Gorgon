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
// Created: Friday, October 26, 2007 11:07:37 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Abstract interface for an input plug-in.
	/// </summary>
	public abstract class InputPlugIn
		: PlugInEntryPoint
	{
		#region Properties.
		/// <summary>
		/// Property to return a description of the input interface.
		/// </summary>
		public abstract string Description
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to unload this plug-in.
		/// </summary>
		protected internal virtual void Unload()
		{
			if (PlugInFactory.PlugIns.Contains(Name))
				PlugInFactory.PlugIns.Remove(Name);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="InputPlugIn"/> class.
		/// </summary>
		/// <param name="name">The name of the plug-in.</param>
		/// <param name="inputDLLPath">The input DLL path.</param>
		protected InputPlugIn(string name, string inputDLLPath)
			: base(name, inputDLLPath, PlugInType.Input)
		{
		}
		#endregion
	}
}
