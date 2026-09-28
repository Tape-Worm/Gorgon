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
// Created: July 20, 2026 6:37:57 PM
//

namespace Gorgon.Graphics.Core.Codecs;

/// <summary>
/// The status returned by the <see cref="GorgonCodecGraphicsPsoCacheCommon.IsReadable(Stream)"/> method.
/// </summary>
public enum ReadStatus
{
    /// <summary>
    /// File or stream can be read.
    /// </summary>
    CanRead = 0,
    /// <summary>
    /// File or stream data is not a valid pipeline state object cache file.
    /// </summary>
    InvalidFile = 1,
    /// <summary>
    /// <para>
    /// The hardware, driver, and/or Gorgon version information in the cache does not match the current hardware, driver, or Gorgon version information on the system.
    /// </para>
    /// <para>
    /// When this happens, the cache must be rebuilt by the user.
    /// </para>
    /// </summary>
    DriverOrHardwareChanged = 2
}
