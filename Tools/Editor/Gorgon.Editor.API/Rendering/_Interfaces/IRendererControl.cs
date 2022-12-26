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
// Created: August 26, 2018 7:07:58 PM
// 
#endregion

using Gorgon.Core;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.Rendering;

/// <summary>
/// A control that uses Gorgon rendering.
/// </summary>
public interface IRendererControl
    : IGorgonNamedObject
{
    #region Properties.
    /// <summary>
    /// Property to return the graphics context.
    /// </summary>
    IGraphicsContext GraphicsContext
    {
        get;
    }

    /// <summary>
    /// Property to return the swap chain assigned to the control.
    /// </summary>
    GorgonSwapChain SwapChain
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to begin rendering on the control.
    /// </summary>
    void Start();

    /// <summary>
    /// Function to cease rendering on the control.
    /// </summary>
    void Stop();
    #endregion
}
