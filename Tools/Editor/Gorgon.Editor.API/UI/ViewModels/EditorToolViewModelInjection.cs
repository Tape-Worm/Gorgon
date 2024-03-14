
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: May 25, 2020 8:57:16 PM
// 


using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;

namespace Gorgon.Editor.UI.ViewModels;

/// <summary>
/// Common editor tool view model parameters
/// </summary>
/// <remarks>
/// <para>
/// These parameters are meant to be passed to a view model based on the <see cref="EditorToolViewModelBase{T}"/> type and will contain services from the host application, and any other information 
/// required for editor tool plug in view models
/// </para>
/// <para>
/// Tool plug in developers will use types derived from this type to pass custom initialization parameters to their tool plug in view models. 
/// </para>
/// </remarks>
/// <seealso cref="EditorToolViewModelBase{T}"/>
/// <remarks>Initializes a new instance of the <see cref="EditorToolViewModelInjection"/> class.</remarks>
/// <param name="contentFileManager">The content file manager.</param>
/// <param name="hostServices">The host services.</param>
/// <exception cref="ArgumentNullException">Thown when the <paramref name="contentFileManager"/>, or the <paramref name="hostServices"/> parameter is <b>null</b>.</exception>
public class EditorToolViewModelInjection(IContentFileManager contentFileManager, IHostContentServices hostServices)
        : ViewModelInjection<IHostContentServices>(hostServices), IEditorToolViewModelInjection
{
    /// <summary>Property to return the file manager for content files.</summary>
    public IContentFileManager ContentFileManager
    {
        get;
    } = contentFileManager ?? throw new ArgumentNullException(nameof(contentFileManager));
}
