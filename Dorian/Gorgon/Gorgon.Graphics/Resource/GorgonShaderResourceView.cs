#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, May 11, 2013 1:56:53 PM
// 
#endregion

using System;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// A resource view to allow access to resource data inside of a shader.
    /// </summary>
    public abstract class GorgonView
    {
		#region Properties.
		/// <summary>
		/// Property to return information about the format used for the shader view.
		/// </summary>
		public GorgonBufferFormatInfo.GorgonFormatData FormatInfo
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the format of the view.
		/// </summary>
		public BufferFormat Format
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the resource bound to this view.
		/// </summary>
		public GorgonResource Resource
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
        /// Initializes a new instance of the <see cref="GorgonView"/> struct.
        /// </summary>
        /// <param name="format">The format of the resource view.</param>
        protected GorgonView(BufferFormat format)
        {
            Format = format;
            FormatInfo = format != BufferFormat.Unknown ? GorgonBufferFormatInfo.GetInfo(format) : null;
        }
        #endregion
    }
}
