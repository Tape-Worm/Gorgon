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
// Created: Sunday, July 03, 2011 9:14:39 AM
// 
#endregion

using Gorgon.IO.GorPack;
using Gorgon.IO.GorPack.Properties;

namespace Gorgon.IO
{
	/// <summary>
	/// Plug-in entry point for the Gorgon packed file file system provider plug-in.
	/// </summary>
	public class GorgonGorPackPlugIn
		: GorgonFileSystemProviderPlugIn
    {
        #region Constants.
        /// <summary>
	    /// The pack file header.
	    /// </summary>
        public const string GorPackHeader = "GORPACK1.SharpZip.BZ2";
        #endregion

        #region Variables.
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a new file system provider instance.
        /// </summary>
        /// <returns>
        /// The file system provider.
        /// </returns>
        protected override GorgonFileSystemProvider OnCreateProvider()
        {
            return new GorgonGorPackProvider(Resources.GORFS_DESC);
        }
        #endregion

        #region Constructor.
        /// <summary>
		/// Initializes a new instance of the <see cref="GorgonGorPackPlugIn"/> class.
		/// </summary>
		public GorgonGorPackPlugIn()
            : base(Resources.GORFS_PLUGIN_DESC)
		{
        }
        #endregion
    }
}
