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
// Created: July 1, 2020 12:31:46 AM
// 
#endregion

using System;
using Gorgon.Animation;
using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The parameters for the <see cref="IKeyEditorContext"/> context view model.
/// </summary>
internal class KeyEditorContextParameters
    : ViewModelInjection<IHostContentServices>
{
    /// <summary>
    /// Property to return the content view model that owns the view model.
    /// </summary>
    public IAnimationContent Content
    {
        get;
    }

    /// <summary>
    /// Property to return the file manager.
    /// </summary>
    public IContentFileManager FileManager
    {
        get;
    }

    /// <summary>
    /// Property to return the services hosted by the plug in.
    /// </summary>
    public ContentServices ContentServices
    {
        get;
    }

    /// <summary>
    /// Property to return the key editor for floating point values.
    /// </summary>
    public IKeyValueEditor FloatValueKeyEditor
    {
        get;
    }

    /// <summary>
    /// Property to return the key value editor for color values.
    /// </summary>
    public IColorValueEditor ColorValueKeyEditor
    {
        get;
    }

    /// <summary>
    /// Property to return the controller for sprite animations.
    /// </summary>
    public GorgonSpriteAnimationController AnimationController
    {
        get;
    }

    /// <summary>Initializes a new instance of the <see cref="KeyEditorContextParameters"/> class.</summary>
    /// <param name="content">The animation content that owns the view model.</param>
    /// <param name="fileManager">The file manager for the project.</param>
    /// <param name="floatValueKeyEditor">The key editor for floating point values</param>
    /// <param name="controller">The sprite animation controller.</param>
    /// <param name="contentServices">The services hosted by the plug in.</param>
    /// <param name="hostServices">The services from the host application.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
    public KeyEditorContextParameters(IAnimationContent content, IContentFileManager fileManager, IKeyValueEditor floatValueKeyEditor, IColorValueEditor colorValueKeyEditor, GorgonSpriteAnimationController controller, ContentServices contentServices, IHostContentServices hostServices)
        : base(hostServices)
    {
        Content = content ?? throw new ArgumentNullException(nameof(content));
        FileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
        ContentServices = contentServices ?? throw new ArgumentNullException(nameof(contentServices));
        FloatValueKeyEditor = floatValueKeyEditor ?? throw new ArgumentNullException(nameof(floatValueKeyEditor));
        ColorValueKeyEditor = colorValueKeyEditor ?? throw new ArgumentNullException(nameof(colorValueKeyEditor));
        AnimationController = controller ?? throw new ArgumentNullException(nameof(controller));
    }
}
