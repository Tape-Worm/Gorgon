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
// Created: March 14, 2019 8:35:00 PM
// 
#endregion

using System;
using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.IO;
using Gorgon.Renderers;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// Parameters for the <see cref="ISpriteContent"/> view model.
/// </summary>
internal class SpriteContentParameters
    : ContentViewModelInjection
{
    /// <summary>
    /// Property to return the sprite being edited.
    /// </summary>
    public GorgonSprite Sprite
    {
        get;
    }

    /// <summary>
    /// Property to return the content file for the sprite texture.
    /// </summary>
    public IContentFile SpriteTextureFile
    {
        get;
    }

    /// <summary>
    /// Property to return the services used for handling sprite data.
    /// </summary>
    public SpriteContentServices ContentServices
    {
        get;
    }

    /// <summary>
    /// Property to return the codec to use when reading/writing sprite data.
    /// </summary>
    public IGorgonSpriteCodec SpriteCodec
    {
        get;
    }

    /// <summary>
    /// Property to return the context for sprite clipping.
    /// </summary>
    public ISpriteClipContext SpriteClipContext
    {
        get;
    }

    /// <summary>
    /// Property to return the context for sprite vertex editing.
    /// </summary>
    public ISpriteVertexEditContext SpriteVertexEditContext
    {
        get;
    }

    /// <summary>
    /// Property to return the context for sprite picking.
    /// </summary>
    public ISpritePickContext SpritePickContext
    {
        get;
    }

    /// <summary>
    /// Property to return the view model for the anchor editor.
    /// </summary>
    public ISpriteAnchorEdit AnchorEditor
    {
        get;
    }

    /// <summary>
    /// Property to return the sprite texture wrapping state editor.
    /// </summary>
    public ISpriteTextureWrapEdit TextureWrappingEditor
    {
        get;
    }

    /// <summary>
    /// Property to return the settings view model.
    /// </summary>
    public ISettings Settings
    {
        get;
    }

    /// <summary>
    /// Property to return the view model for the sprite color editor.
    /// </summary>
    public ISpriteColorEdit ColorEditor
    {
        get;
    }

    /// <summary>
    /// Property to return the sampler builder service.
    /// </summary>
    public GorgonSamplerStateBuilder SamplerBuilder
    {
        get;
    }

    /// <summary>Initializes a new instance of the <see cref="SpriteContentParameters"/> class.</summary>
    /// <param name="sprite">The sprite data.</param>
    /// <param name="textureFile">The texture file linked to the sprite.</param>
    /// <param name="settings">The settings for the plug in.</param>
    /// <param name="spriteClipContext">The context for sprite clipping.</param>
    /// <param name="spritePickContext">The context for sprite picking.</param>
    /// <param name="spriteVertexEditContext">The context for sprite vertex editing.</param>
    /// <param name="colorEditor">The editor used to modify the sprite color.</param>
    /// <param name="anchorEditor">The editor used to modify the anchor.</param>
    /// <param name="textureWrapEditor">The editor used to modify texture wrapping.</param>
    /// <param name="contentServices">The services for handling sprite data.</param>
    /// <param name="codec">The sprite codec for the sprite file.</param>
    /// <param name="fileManager">The file manager for content files.</param>
    /// <param name="file">The file that contains the content.</param>
    /// <param name="commonServices">The common services for the application.</param>
    /// <exception cref="ArgumentNullException">Thrown when any of the required parameters are <b>null</b>.</exception>
    public SpriteContentParameters(GorgonSprite sprite,
                                   IContentFile textureFile,
                                   ISettings settings,
                                   ISpriteClipContext spriteClipContext,
                                   ISpritePickContext spritePickContext,
                                   ISpriteVertexEditContext spriteVertexEditContext,
                                   ISpriteColorEdit colorEditor,
                                   ISpriteAnchorEdit anchorEditor,
                                   ISpriteTextureWrapEdit textureWrapEditor,
                                   SpriteContentServices contentServices,
                                   IGorgonSpriteCodec codec,
                                   IContentFileManager fileManager, 
                                   IContentFile file,
                                   IHostContentServices commonServices)
        : base(fileManager, file, commonServices)
    {
        Sprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
        SpriteCodec = codec ?? throw new ArgumentNullException(nameof(codec));
        SpriteClipContext = spriteClipContext ?? throw new ArgumentNullException(nameof(spriteClipContext));
        SpritePickContext = spritePickContext ?? throw new ArgumentNullException(nameof(spritePickContext));
        SpriteVertexEditContext = spriteVertexEditContext ?? throw new ArgumentNullException(nameof(spriteVertexEditContext));
        ColorEditor = colorEditor ?? throw new ArgumentNullException(nameof(colorEditor));
        AnchorEditor = anchorEditor ?? throw new ArgumentNullException(nameof(anchorEditor));
        TextureWrappingEditor = textureWrapEditor ?? throw new ArgumentNullException(nameof(textureWrapEditor));
        SpriteTextureFile = textureFile;
        ContentServices = contentServices ?? throw new ArgumentNullException(nameof(contentServices));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }
}
