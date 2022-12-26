#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: February 5, 2019 7:46:30 PM
// 
#endregion

using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ImageEditor.ViewModels;

/// <summary>
/// The view model for the generate mip map settings view.
/// </summary>
internal interface IMipMapSettings
    : IHostedPanelViewModel
{
    /// <summary>
    /// Property to set or return the number of mip map levels.
    /// </summary>
    int MipLevels
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the maximum number of mip map levels based on the width, height and, if applicable, depth slices.
    /// </summary>
    int MaxMipLevels
    {
        get;
    }

    /// <summary>
    /// Property to set or return the mip map filter to use when creating the mip chain.
    /// </summary>
    ImageFilter MipFilter
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the command to execute for updating the current image information.
    /// </summary>
    IEditorCommand<IGorgonImage> UpdateImageInfoCommand
    {
        get;
    }
}
