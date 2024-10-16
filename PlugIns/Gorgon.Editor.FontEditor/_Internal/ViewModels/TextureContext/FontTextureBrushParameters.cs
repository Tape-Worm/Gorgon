﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: September 10, 2021 12:46:10 PM
// 

using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// Parameters for the <see cref="FontTextureBrush"/> view model
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="FontTextureBrushParameters" /> class.</remarks>
/// <param name="imageLoader">The service used to load images.</param>
/// <param name="hostServices">The services from the host application.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
internal class FontTextureBrushParameters(ImageLoadService imageLoader, IHostContentServices hostServices)
        : HostedPanelViewModelParameters(hostServices)
{
    /// <summary>
    /// Property to return the service used to load texture images.
    /// </summary>
    public ImageLoadService ImageLoader
    {
        get;
    } = imageLoader ?? throw new ArgumentNullException(nameof(imageLoader));

}
