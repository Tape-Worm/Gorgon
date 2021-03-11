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
// Created: Tuesday, June 4, 2013 8:25:47 PM
// 
#endregion

using System;
using SharpDX.DXGI;

namespace Gorgon.Graphics.Core
{
    /// <summary> 
    /// Settings for defining the set up for a swap chain.
    /// </summary>
    public class GorgonSwapChainInfo
        : IGorgonSwapChainInfo
    {
        #region Properties.
        /// <summary>
        /// Property to set or return whether the back buffer contents will be stretched to fit the size of the presentation target area (typically the client area of the window).
        /// </summary>
        /// <remarks>
        /// The default value for this value is <b>true</b>.
        /// </remarks>
        public bool StretchBackBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to use flip mode rather than a bitblt mode to present the back buffer to the presentation target.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this value to <b>true</b> will use the more performant flip model for displaying data from the swap chain back buffer, and setting it to <b>false</b> will use the bitblt model to copy the 
        /// back buffer data to the presentation target.
        /// </para>
        /// <para>
        /// When this value is set to <b>true</b>, the back buffer contents are kept after the back buffer is sent to the presentation target, when it is set to <b>false</b>, the contents may be erased (this 
        /// ultimately depends on the driver implementation).
        /// </para>
        /// <para>
        /// This value will default to <b>false</b>.
        /// </para>
        /// </remarks>
        public bool UseFlipMode
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Property to set or return the format of the swap chain back buffer.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="BufferFormat.R8G8B8A8_UNorm"/>
        /// </remarks>
        public BufferFormat Format
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the width of the swap chain back buffer.
        /// </summary>
        public int Width
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the height of the swap chain back buffer.
        /// </summary>
        public int Height
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the name of the swap chain.
        /// </summary>
	    public string Name
        {
            get;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonSwapChainInfo" /> class.
        /// </summary>
        /// <param name="desc">The DXGI swap chain description to copy from.</param>
        /// <param name="name">[Optional] The name of the swap chain.</param>
        internal GorgonSwapChainInfo(in SwapChainDescription1 desc, string name = null)
                    : this(name)
        {
            Format = (BufferFormat)desc.Format;
            Width = desc.Width;
            Height = desc.Height;
            UseFlipMode = desc.SwapEffect == SwapEffect.FlipSequential;
            StretchBackBuffer = desc.Scaling != Scaling.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonSwapChainInfo"/> class.
        /// </summary>
        /// <param name="info">A <see cref="IGorgonSwapChainInfo"/> to copy the settings from.</param>
        /// <param name="newName">[Optional] A new name for the swap chain.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public GorgonSwapChainInfo(IGorgonSwapChainInfo info, string newName = null)
        {
            if (info is null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            Name = string.IsNullOrEmpty(newName) ? info.Name : newName;
            Format = info.Format;
            Height = info.Height;
            StretchBackBuffer = info.StretchBackBuffer;
            UseFlipMode = info.UseFlipMode;
            Width = info.Width;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonSwapChainInfo"/> class.
        /// </summary>
        /// <param name="name">The name of the swap chain.</param>
        public GorgonSwapChainInfo(string name = null)
        {
            Name = string.IsNullOrEmpty(name) ? $"GorgonSwapChain_{Guid.NewGuid():N}" : name;
            StretchBackBuffer = true;
            Format = BufferFormat.R8G8B8A8_UNorm;
        }
        #endregion
    }
}