﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: May 2, 2019 9:16:13 AM
// 

using Gorgon.Graphics.Core;

namespace Gorgon.Renderers.Services;

/// <summary>
/// The return result from the <see cref="IGorgonTextureAtlasService.GenerateAtlas"/> method
/// </summary>
/// <remarks>
/// <para>
/// This return result will contain the texture(s) for the atlas, plus update sprites that point at the correct placement on the texture atlas
/// </para>
/// </remarks>
public class GorgonTextureAtlas
{
    /// <summary>
    /// Property to return the textures generated by the <see cref="IGorgonTextureAtlasService"/>.
    /// </summary>
    public IReadOnlyList<GorgonTexture2DView> Textures
    {
        get;
    }

    /// <summary>
    /// Property to return the updated sprites from the <see cref="IGorgonTextureAtlasService"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// These sprites will have their <see cref="GorgonSprite.TextureRegion"/>, <see cref="GorgonSprite.TextureArrayIndex"/>, and <see cref="GorgonSprite.Texture"/> adjusted to point at the correct 
    /// place on the resulting <see cref="Textures"/>.
    /// </para>
    /// <para>
    /// The updated sprites are copies of the originals, they do not replace the sprites sent in to the service. 
    /// </para>
    /// </remarks>
    public IReadOnlyList<(GorgonSprite originalSprite, GorgonSprite newSprite)> Sprites
    {
        get;
    }

    /// <summary>Initializes a new instance of the <see cref="GorgonTextureAtlas"/> class.</summary>
    /// <param name="textures">The textures generated by the service.</param>
    /// <param name="sprites">The sprites generated by the service.</param>
    internal GorgonTextureAtlas(IReadOnlyList<GorgonTexture2DView> textures, IReadOnlyList<(GorgonSprite originalSprite, GorgonSprite newSprite)> sprites)
    {
        Textures = textures;
        Sprites = sprites;
    }
}
