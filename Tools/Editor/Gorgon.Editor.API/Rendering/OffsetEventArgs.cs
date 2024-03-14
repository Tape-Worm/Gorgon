#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: February 8, 2020 12:24:23 AM
// 
#endregion

using System.Numerics;

namespace Gorgon.Editor.Rendering;

/// <summary>
/// Event arguments for the <see cref="IContentRenderer.OffsetChanged"/> event.
/// </summary>
public class OffsetEventArgs
    : EventArgs
{
    /// <summary>
    /// Property to return the offset.
    /// </summary>
    /// <remarks>
    /// This value is in world space, relative to the <see cref="IContentRenderer.RenderRegion"/>.
    /// </remarks>
    public Vector2 Offset
    {
        get;
        internal set;
    }
}
