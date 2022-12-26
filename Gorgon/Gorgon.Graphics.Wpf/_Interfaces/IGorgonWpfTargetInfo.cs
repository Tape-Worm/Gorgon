#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: January 6, 2021 1:41:50 PM
// 
#endregion

using System.Windows.Controls;
using Gorgon.Core;

namespace Gorgon.Graphics.Wpf;

/// <summary>
/// Read only information used to define a WPF render target.
/// </summary>
public interface IGorgonWpfTargetInfo
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the image that will receive the rendered data.
    /// </summary>
    /// <remarks>
    /// This is a WPF image control that Gorgon will render into. The image source for this must be set to a <c>D3D11Image</c> control.
    /// </remarks>
    Image RenderImage
    {
        get;
    }
}