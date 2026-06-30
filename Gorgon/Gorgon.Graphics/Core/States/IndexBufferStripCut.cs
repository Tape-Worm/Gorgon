// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: June 17, 2026 3:33:16 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Values that describe an identifier that is used to handle primitive restart for strip topologies.
/// </summary>
/// <remarks>
/// <para>
/// This value is only used with primitive topologies that use triangle or line strips.
/// </para>
/// </remarks>
public enum IndexBufferStripCutIdentifier
{
    /// <summary>
    /// All indices point to actual vertices.
    /// </summary>
    Disabled = 0,
    /// <summary>
    /// <para>
    /// Specify this to restart when an index value of <c>0xFFFF</c> (<c>-1 signed</c>) is found. 
    /// </para>
    /// <para>
    /// This applies to 16-bit indices only.
    /// </para>
    /// </summary>
    StopWith16BitMax = 1,
    /// <summary>
    /// <para>
    /// Specify this to restart when an index value of <c>0xFFFFFFFF</c> (<c>-1 signed</c>) is found.
    /// </para>
    /// <para>
    /// This applies to 32-bit indices only.
    /// </para>
    /// </summary>
    StopWith32BitMax = 2
}
