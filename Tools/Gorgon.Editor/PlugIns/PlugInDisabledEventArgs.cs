#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Monday, March 9, 2015 8:51:32 PM
// 
#endregion

using System;
using Gorgon.Plugins;

namespace Gorgon.Editor
{
	/// <summary>
	/// Event arguments for the plug-in disabled event.
	/// </summary>
	class PlugInDisabledEventArgs
		: EventArgs
	{
		#region Properties.
		/// <summary>
		/// Property to return the plug-in that was disabled.
		/// </summary>
		public GorgonPlugin PlugIn
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PlugInDisabledEventArgs"/> class.
		/// </summary>
		/// <param name="plugIn">The plug in that was disabled.</param>
		public PlugInDisabledEventArgs(GorgonPlugin plugIn)
		{
			PlugIn = plugIn;
		}
		#endregion
	}
}
