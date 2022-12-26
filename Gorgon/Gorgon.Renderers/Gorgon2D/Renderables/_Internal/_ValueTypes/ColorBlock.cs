#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: June 12, 2018 10:59:58 AM
// 
#endregion

using Gorgon.Graphics;

namespace Gorgon.Renderers;

/// <summary>
/// A key used to look up a color block.
/// </summary>
internal readonly struct ColorBlock
{
    #region Variables.
    /// <summary>
    /// The starting character index.
    /// </summary>
    public readonly int Start;

    /// <summary>
    /// The ending character index.
    /// </summary>
    public readonly int End;

    /// <summary>
    /// The color for the block.
    /// </summary>
    public readonly GorgonColor Color;
    #endregion

    #region Methods.
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorBlock"/> struct.
    /// </summary>
    /// <param name="start">The start character index.</param>
    /// <param name="end">The end character index.</param>
    /// <param name="color">The color for the block.</param>
    public ColorBlock(int start, int end, GorgonColor color)
    {
        Color = color;
        Start = start;
        End = end;
    }
    #endregion

}
