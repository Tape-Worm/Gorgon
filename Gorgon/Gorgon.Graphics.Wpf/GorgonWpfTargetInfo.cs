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
// Created: January 6, 2021 1:29:04 PM
// 
#endregion

using System.Windows.Controls;

namespace Gorgon.Graphics.Wpf;

/// <summary>
/// Information used to define a WPF render target.
/// </summary>
public class GorgonWpfTargetInfo 
    : IGorgonWpfTargetInfo
{
    #region Properties.
    /// <summary>
    /// Property to return the name of the WPF target.
    /// </summary>
    public string Name
    {
        get;
    }

    /// <summary>
    /// Property to return the image that will receive the rendered data.
    /// </summary>
    /// <remarks>
    /// This is a WPF image control that Gorgon will render into. The image source for this must be set to a <c>D3D11Image</c> control.
    /// </remarks>
    public Image RenderImage
    {
        get;
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="GorgonWpfTargetInfo" /> class.</summary>
    /// <param name="info">The information object to clone.</param>
    /// <param name="newName">[Optional] The new name to assign to the target.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
    public GorgonWpfTargetInfo(IGorgonWpfTargetInfo info, string newName = null)
    {
        if (info is null)
        {
            throw new ArgumentNullException(nameof(info));
        }

        Name = string.IsNullOrEmpty(newName) ? info.Name : newName;
        RenderImage = info.RenderImage;
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonWpfTargetInfo" /> class.</summary>
    /// <param name="renderImage">The image that will receive the rendered data.</param>
    /// <param name="name">[Optional] The name of this object.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderImage"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// The <paramref name="renderImage"/> parameter is a WPF image control. The image source for the image control must be a <c>D3D11Image</c> control.
    /// </para>
    /// </remarks>
    public GorgonWpfTargetInfo(Image renderImage, string name = null)
    {
        RenderImage = renderImage ?? throw new ArgumentNullException(nameof(renderImage));
        Name = string.IsNullOrEmpty(name) ? $"WPF_RenderTarget_{Guid.NewGuid():N}" : name;
    }
    #endregion
}
