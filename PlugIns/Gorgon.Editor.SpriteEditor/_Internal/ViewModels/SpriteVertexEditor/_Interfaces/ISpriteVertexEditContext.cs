
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: April 10, 2019 8:26:04 AM
// 


using System.Numerics;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The view model for the vertex editor
/// </summary>
internal interface ISpriteVertexEditContext
    : IEditorContext
{
    /// <summary>
    /// Property to set or return the vertex offset for the selected vertex.
    /// </summary>
    Vector2 Offset
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the vertex index.
    /// </summary>
    int SelectedVertexIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the command used to reset the vertex offset back to 0x0.
    /// </summary>
    IEditorCommand<object> ResetOffsetCommand
    {
        get;
    }

    /// <summary>
    /// Property to set or return the command used to apply the updated clipping coordinates.
    /// </summary>
    IEditorCommand<object> ApplyCommand
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the command used to cancel the sprite clipping operation.
    /// </summary>
    IEditorCommand<object> CancelCommand
    {
        get;
    }

    /// <summary>
    /// Property to set or return the vertices that are being edited.
    /// </summary>
    IReadOnlyList<Vector2> Vertices
    {
        get;
        set;
    }
}
