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
// Created: May 19, 2020 12:54:37 PM
// 
#endregion

using System;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// Parameters for the <see cref="ISpriteAnchorEdit"/> view model.
/// </summary>
internal class SpriteAnchorEditParameters
    : HostedPanelViewModelParameters
{
    /// <summary>
    /// Property to return the boundaries for the anchor point.
    /// </summary>
    public DX.Rectangle Bounds
    {
        get;
    }

    /// <summary>Initializes a new instance of the <see cref="SpriteAnchorEditParameters"/> class.</summary>
    /// <param name="bounds">The boundaries for the anchor point.</param>
    /// <param name="hostServices">The common services from the host application.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
    public SpriteAnchorEditParameters(DX.Rectangle bounds, IHostContentServices hostServices)
        : base(hostServices) => Bounds = bounds;
}
