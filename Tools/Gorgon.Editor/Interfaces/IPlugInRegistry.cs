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
// Created: Wednesday, February 25, 2015 10:56:10 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.IO;
using Gorgon.PlugIns;

namespace Gorgon.Editor
{
	/// <summary>
	/// The interface to the plug-in registry.
	/// </summary>
	/// <remarks>The plug-in registry is responsible for loading and registering all the plug-ins located in the plug-ins path.</remarks>
	interface IPlugInRegistry
	{
		/// <summary>
		/// Event fired when an already loaded plug-in is disabled.
		/// </summary>
		event EventHandler<PlugInDisabledEventArgs> PlugInDisabled;

			/// <summary>
		/// Property to return a list of the disabled plug-ins.
		/// </summary>
		IReadOnlyList<DisabledPlugIn> DisabledPlugIns
		{
			get;
		}

		/// <summary>
		/// Property to return the collection of file system provider plug-ins.
		/// </summary>
		IReadOnlyDictionary<string, GorgonFileSystemProviderPlugIn> FileSystemPlugIns
		{
			get;
		}

		/// <summary>
		/// Function to determine if a plug-in is disabled.
		/// </summary>
		/// <param name="plugIn">The plug-in to query.</param>
		/// <returns><c>true</c> if disabled, <c>false</c> if not.</returns>
		bool IsDisabled(GorgonPlugIn plugIn);

		/// <summary>
		/// Function to scan for and load any plug-ins located in the plug-ins folder.
		/// </summary>
		void ScanAndLoadPlugIns();

		/// <summary>
		/// Function to disable a plug-in.
		/// </summary>
		/// <param name="plugIn">Plug-in to disable.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="plugIn"/> parameter is NULL (Nothing in VB.Net).</exception>
		void DisablePlugIn(GorgonPlugIn plugIn);
	}
}
