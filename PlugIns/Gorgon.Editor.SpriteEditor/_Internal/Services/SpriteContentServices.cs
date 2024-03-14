
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
// Created: May 7, 2020 4:32:25 PM
// 


using Gorgon.Editor.Services;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// Services for the sprite editor
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="SpriteContentServices"/> class.</remarks>
/// <param name="newSpriteService">The service used to create sprites.</param>
/// <param name="textureService">The service used to manage sprite textures.</param>
/// <param name="undoService">The undo service used to undo/redo operations.</param>
/// <param name="builder">The builder used to create sampler states.</param>
/// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
internal class SpriteContentServices(NewSpriteService newSpriteService, SpriteTextureService textureService, IUndoService undoService, GorgonSamplerStateBuilder builder)
{
    /// <summary>
    /// Property to return the service for creating new sprites.
    /// </summary>
    public NewSpriteService NewSpriteService
    {
        get;
    } = newSpriteService ?? throw new ArgumentNullException(nameof(newSpriteService));

    /// <summary>
    /// Property to return the service used to handle sprite textures.
    /// </summary>
    public SpriteTextureService TextureService
    {
        get;
    } = textureService ?? throw new ArgumentNullException(nameof(textureService));

    /// <summary>
    /// Property to set or return the service used to handle undo/redo.
    /// </summary>
    public IUndoService UndoService
    {
        get;
        set;
    } = undoService ?? throw new ArgumentNullException(nameof(undoService));

    /// <summary>
    /// Property to return the builder used to create sampler states.
    /// </summary>
    public GorgonSamplerStateBuilder SampleStateBuilder
    {
        get;
    } = builder ?? throw new ArgumentNullException(nameof(builder));
}
