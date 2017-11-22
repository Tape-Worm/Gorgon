#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: November 21, 2017 8:44:47 PM
// 
#endregion

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines the support given to a specific <see cref="BufferFormat"/>.
    /// </summary>
    public class GorgonFormatSupportInfo
    {
        #region Properties.
        /// <summary>
        /// Property to return the format that is being queried for support.
        /// </summary>
        public BufferFormat Format
        {
            get;
        }

        /// <summary>
        /// Property to return the resource support for a format.
        /// </summary>
        public BufferFormatSupport FormatSupport
        {
            get;
        }

        /// <summary>
        /// Property to return the compute shader/uav support for a format.
        /// </summary>
        public ComputeShaderFormatSupport ComputeSupport
        {
            get;
        }

        /// <summary>
        /// Property to return the maximum multisample count and quality level support for the format.
        /// </summary>
        public GorgonMultisampleInfo MaxMultisampleCountQuality
        {
            get;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFormatSupportInfo"/> class.
        /// </summary>
        /// <param name="format">The format being queried.</param>
        /// <param name="formatSupport">The format support.</param>
        /// <param name="computeSupport">The compute support.</param>
        /// <param name="multisampleMax">The multisample maximum.</param>
        internal GorgonFormatSupportInfo(BufferFormat format,
                                         D3D11.FormatSupport formatSupport,
                                         D3D11.ComputeShaderFormatSupport computeSupport,
                                         GorgonMultisampleInfo multisampleMax)
        {
            Format = format;
            FormatSupport = (BufferFormatSupport)formatSupport;
            ComputeSupport = (ComputeShaderFormatSupport)computeSupport;
            MaxMultisampleCountQuality = multisampleMax;
        }
        #endregion

    }
}
