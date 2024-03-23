
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Sunday, October 16, 2011 2:10:22 PM
// 

using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Values to define the number and quality of multisampling
/// </summary>
/// <param name="Count">The number of samples per pixel.</param>
/// <param name="Quality">The quality for a sample.</param>
/// <remarks>
/// <para>
/// There is a performance penalty for setting the <see cref="Quality"/> value to higher levels
/// </para>
/// <para>
/// Setting the <see cref="Count"/> and <see cref="Quality"/> values to 1 and 0 respectively, will disable multisampling
/// </para>
/// <para>
/// If multisample anti-aliasing is being used, all bound render targets and depth buffers must have the same sample counts and quality levels
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// This value must be 0 or less than/equal to the quality returned by <see cref="IGorgonFormatSupportInfo.MaxMultisampleCountQuality"/>.  Failure to do so will cause an exception for objects that use this 
/// type
/// </para>
/// </note>
/// </para>
/// </remarks>
public record GorgonMultisampleInfo(int Count, int Quality)
{
    /// <summary>
    /// The default multisampling value.
    /// </summary>
    public static readonly GorgonMultisampleInfo NoMultiSampling = new(1, 0);

    /// <summary>
    /// A quality level for standard multisample quality.
    /// </summary>
    /// <remarks>
    /// This value is always supported in Gorgon because it can only use Direct 3D 10 or better devices, and these devices are required to implement this pattern.
    /// </remarks>
    public static readonly int StandardMultisamplePatternQuality = unchecked((int)0xffffffff);

    /// <summary>
    /// A pattern where all of the samples are located at the pixel center.
    /// </summary>
    public static readonly int CenteredMultisamplePatternQuality = unchecked((int)0xfffffffe);

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString() => string.Format(Resources.GORGFX_TOSTR_MULTISAMPLEINFO, Count, Quality);

}