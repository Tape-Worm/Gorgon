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
// Created: September 17, 2018 7:58:15 AM
// 
#endregion

using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;

namespace Gorgon.Editor.UI;

/// <summary>
/// Defines values to inject into content view models.
/// </summary>
public interface IContentViewModelInjection
    : IViewModelInjection<IHostContentServices>
{
    /// <summary>
    /// Property to return the content file.
    /// </summary>
    IContentFile File
    {
        get;
    }

    /// <summary>
    /// Property to return the file manager for content files.
    /// </summary>
    IContentFileManager ContentFileManager
    {
        get;
    }
}
