
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: November 10, 2018 11:07:56 PM
// 


using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.UI;

/// <summary>
/// Common content view model parameters
/// </summary>
/// <remarks>
/// <para>
/// These parameters are meant to be passed to a view model based on the <see cref="ContentEditorViewModelBase{T}"/> type and will contain services from the host application, and any other information 
/// required for content editor view models
/// </para>
/// <para>
/// Content editor developers will use types derived from this type to pass custom initialization parameters to their content view models. 
/// </para>
/// </remarks>
/// <seealso cref="ContentEditorViewModelBase{T}"/>
/// <remarks>Initializes a new instance of the ContentViewModelInjectionCommon class.</remarks>
/// <param name="fileManager">The file manager for content files.</param>
/// <param name="file">The file that contains the content.</param>        
/// <param name="commonServices">The common services for the application.</param>
/// <exception cref="ArgumentNullException">Thrown any of the parameters are <b>null</b></exception>
public class ContentViewModelInjection(IContentFileManager fileManager, IContentFile file, IHostContentServices commonServices)
        : ViewModelInjection<IHostContentServices>(commonServices), IContentViewModelInjection
{
    /// <summary>
    /// Property to return the content file.
    /// </summary>
    public IContentFile File
    {
        get;
    } = file ?? throw new ArgumentNullException(nameof(file));

    /// <summary>Property to return the file manager for content files.</summary>
    public IContentFileManager ContentFileManager
    {
        get;
    } = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
}
