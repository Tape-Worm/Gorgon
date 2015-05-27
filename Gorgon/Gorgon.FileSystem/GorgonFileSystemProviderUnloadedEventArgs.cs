#region MIT
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
// Created: Tuesday, May 26, 2015 11:17:04 PM
// 
#endregion

using System;

namespace Gorgon.IO
{
	/// <summary>
	/// Event arguments for the <see cref="GorgonFileSystemProviderCollection.ProviderUnloaded"/> event.
	/// </summary>
	public class GorgonFileSystemProviderUnloadedEventArgs
		: EventArgs
	{
		/// <summary>
		/// Property to return the file system provider that is being unloaded.
		/// </summary>
		public GorgonFileSystemProvider Provider
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemProviderUnloadedEventArgs"/> class.
		/// </summary>
		/// <param name="provider">The provider.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="provider"/> parameter is <c>null</c> (Nothing in VB.Net).</exception>
		internal GorgonFileSystemProviderUnloadedEventArgs(GorgonFileSystemProvider provider)
		{
			if (provider == null)
			{
				throw new ArgumentNullException("provider");
			}

			Provider = provider;
		}
	}
}
