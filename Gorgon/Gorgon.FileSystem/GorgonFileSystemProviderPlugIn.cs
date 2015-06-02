#region MIT.
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
// Created: Monday, June 27, 2011 9:01:38 AM
// 
#endregion

using Gorgon.Plugins;

namespace Gorgon.IO
{
	/// <summary>
	/// The base entry point for an Gorgon file system provider plug-in.
	/// </summary>
	public abstract class GorgonFileSystemProviderPlugIn
		: GorgonPlugIn
    {
        #region Variables.
	    private GorgonFileSystemProvider _provider;         // Cached provider instance.
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a new file system provider instance.
        /// </summary>
        /// <returns>The file system provider.</returns>
        protected abstract GorgonFileSystemProvider OnCreateProvider();

        /// <summary>
        /// Function to create a new or return an existing file system provider instance.
        /// </summary>
        /// <returns>The file system provider.</returns>
        internal GorgonFileSystemProvider CreateProvider()
        {
            return _provider ?? (_provider = OnCreateProvider());
        }
	    #endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFileSystemProviderPlugIn"/> class.
		/// </summary>
		/// <param name="description">Optional description of the plug-in.</param>
		/// <remarks>Objects that implement this base class should pass in a hard coded description on the base constructor.</remarks>
		protected GorgonFileSystemProviderPlugIn(string description)
			: base(description)
		{
		}
		#endregion
	}
}
