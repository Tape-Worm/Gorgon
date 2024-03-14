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
// Created: July 31, 2020 3:05:13 PM
// 
#endregion

using System;
using System.Numerics;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// Arguments for the <see cref="VertexEditService.VerticesChanged"/> event.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="VertexChangedEventArgs"/> class.</remarks>
/// <param name="vertexIndex">Index of the vertex being modified</param>
/// <param name="oldPos">The old position for the vertex.</param>
/// <param name="newPos">The new position for the vertex.</param>
internal class VertexChangedEventArgs(int vertexIndex, Vector2 oldPos, Vector2 newPos)
        : EventArgs
{
    /// <summary>
    /// Property to return the previous position of the vertex.
    /// </summary>
    public Vector2 PreviousPosition
    {
        get;
    } = oldPos;

    /// <summary>
    /// Property to return the new position of the vertex.
    /// </summary>
    public Vector2 NewPosition
    {
        get;
    } = newPos;

    /// <summary>
    /// Property to return the index of the vertex being changed.
    /// </summary>
    public int VertexIndex
    {
        get;
    } = vertexIndex;
}
