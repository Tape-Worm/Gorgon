#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 28, 2016 11:54:46 PM
// 
#endregion

using Gorgon.Core;

namespace Gorgon.Graphics.Core
{
    /// <summary> 
    /// Settings for defining the set up for a swap chain.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This provides an immutable view of the swap chain information so that it cannot be modified after the buffer is created.
    /// </para>
    /// </remarks>
    public interface IGorgonSwapChainInfo
        : IGorgonNamedObject
    {
        /// <summary>
        /// Property to return whether the back buffer contents will be stretched to fit the size of the presentation target area (typically the client area of the window).
        /// </summary>
        /// <remarks>
        /// The default value for this value is <b>true</b>.
        /// </remarks>
        bool StretchBackBuffer
        {
            get;
        }

        /// <summary>
        /// Property to return whether to use flip mode rather than a bitblt mode to present the back buffer to the presentation target.
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
        /// <para>
        /// <note type="warning">
        /// <para>
        /// Setting this value on Windows 7 will have no effect, it is only supported on Windows 8 or better.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        bool UseFlipMode
        {
            get;
        }

        /// <summary>
        /// Property to return the format of the swap chain back buffer.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="BufferFormat.R8G8B8A8_UNorm"/>
        /// </remarks>
        BufferFormat Format
        {
            get;
        }

        /// <summary>
        /// Property to return the width of the swap chain back buffer.
        /// </summary>
        int Width
        {
            get;
        }

        /// <summary>
        /// Property to return the height of the swap chain back buffer.
        /// </summary>
        int Height
        {
            get;
        }
    }
}