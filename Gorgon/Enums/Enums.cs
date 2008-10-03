#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Tuesday, November 14, 2006 11:14:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary
{
    /// <summary>
    /// Enumeration for comparison functions for use with depth, stencil and alpha tests.
    /// </summary>
    public enum CompareFunctions
    {
        /// <summary>Only accept value if less than current.</summary>
        LessThan = 0,
        /// <summary>Only accept value if greater than current.</summary>
        GreaterThan = 1,
        /// <summary>Only accept value if equal to current.</summary>
        Equal = 2,
        /// <summary>Always accept value.</summary>
        Always = 3,
        /// <summary>Never accept value.</summary>
        Never = 4,
        /// <summary>Only accept value if less than or equal to current.</summary>
        LessThanOrEqual = 5,
        /// <summary>Only accept value if greater than or equal to current.</summary>
        GreaterThanOrEqual = 6,
        /// <summary>Only accept value if not equal.</summary>
        NotEqual = 7
    }

    /// <summary>
    /// Enumeration for vertical sync presentation intervals.
    /// </summary>
    [Flags]
    public enum VSyncIntervals
    {
        /// <summary>Immediate presentation.</summary>
        IntervalNone = 1,
        /// <summary>Wait one retrace before presenting.</summary>
        IntervalOne = 2,
        /// <summary>Wait two retraces before presenting.</summary>
        IntervalTwo = 4,
        /// <summary>Wait three retraces before presenting.</summary>
        IntervalThree = 8,
        /// <summary>Wait four retraces before presenting.</summary>
        IntervalFour = 16
    }

    /// <summary>
    /// Enumeration to indicate which targets to clear.
    /// </summary>
    [Flags]
    public enum ClearTargets
    {
        /// <summary>Clear nothing.</summary>
        None = 1,
        /// <summary>Clear the back buffer.</summary>
        BackBuffer = 2,
        /// <summary>Clear the depth buffer.</summary>
        DepthBuffer = 4,
        /// <summary>Clear the stencil buffer.</summary>
        StencilBuffer = 8
    }

    /// <summary>
    /// Enumeration for back buffer formats.
    /// </summary>
    public enum BackBufferFormats
    {
        /// <summary>32 bit color. 10 bits for Red, Green, Blue.  2 bits for Alpha.</summary>
        BufferRGB101010A2 = 0,
        /// <summary>32/24 bit color. 8 bits for Red, Green, Blue.</summary>
        BufferRGB888 = 1,
        /// <summary>15/16 bit color. 5 bits for Red, Green, Blue.</summary>
        BufferRGB555 = 2,
        /// <summary>16 bit color. 5 bits for Red, 6 bits for Green, 5 bits for blue.</summary>
        BufferRGB565 = 3,
        /// <summary>8 bit paletted color.  Unlikely that this mode will be supported.</summary>
        BufferP8 = 4
    }

    /// <summary>
    /// Enumeration for back buffer formats.
    /// </summary>
    public enum DepthBufferFormats
    {
        /// <summary>Unknown type.</summary>
        BufferUnknown = 0,
        /// <summary>15 bit depth buffer, 1 bit stencil.</summary>
        BufferDepth15Stencil1 = 1,
        /// <summary>16 bit depth buffer.</summary>
        BufferDepth16 = 2,
        /// <summary>16 bit depth buffer, lockable.</summary>
        BufferDepth16Lockable = 3,
        /// <summary>16 bit luminance.</summary>
        BufferLuminance16 = 4,
        /// <summary>24 bit depth buffer.</summary>
        BufferDepth24 = 5,
        /// <summary>24 bit depth buffer, 4 bit stencil.</summary>
        BufferDepth24Stencil4 = 6,
        /// <summary>24 bit depth buffer, 8 bit stencil.</summary>
        BufferDepth24Stencil8 = 7,
        /// <summary>24 bit depth buffer, 8 bit stencil, lockable.</summary>
        BufferDepth24Stencil8Lockable = 8,
        /// <summary>32 bit depth buffer.</summary>
        BufferDepth32 = 9,
        /// <summary>32 bit depth buffer, lockable.</summary>
        BufferDepth32Lockable = 10
    }
}
