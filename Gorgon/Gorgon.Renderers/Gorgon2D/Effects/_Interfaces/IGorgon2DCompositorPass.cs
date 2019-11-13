#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: November 14, 2019 3:49:40 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Graphics;

namespace Gorgon.Renderers
{
    /// <summary>
    /// Information about a single pass in a <see cref="Gorgon2DCompositor"/>.
    /// </summary>
    public interface IGorgon2DCompositorPass
        : IGorgonNamedObject
    {
        /// <summary>
        /// Property to set or return the color to use when clearing the active render target.
        /// </summary>
        /// <remarks>
        /// <para>
        ///  If this value is set to <b>null</b>, then the current target will not be cleared.
        /// </para>
        /// <para>
        /// The default value is <b>null</b>.
        /// </para>
        /// </remarks>
        GorgonColor? ClearColor
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether the effect is enabled or not.
        /// </summary>
        bool Enabled
        {
            get;
            set;
        }
    }
}
