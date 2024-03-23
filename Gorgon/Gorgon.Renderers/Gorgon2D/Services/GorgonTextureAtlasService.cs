
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
// Created: May 2, 2019 9:24:26 AM
// 


using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Properties;

namespace Gorgon.Renderers.Services;

/// <summary>
/// A service used to generate a 2D texture atlas from a series of separate sprites
/// </summary>
/// <remarks>
/// <para>
/// To get the best performance when rendering sprites, batching is essential. In order to achieve this, rendering sprites that use the same texture is necessary. This is where building a texture atlas 
/// comes in. A texture atlas is a combination of multiple sprite images into a single texture, each sprite that is embedded into the atlas uses a different set of texture coordinates. When rendering 
/// these sprites with the atlas, only a single texture change is required
/// </para>
/// <para>
/// When generating an atlas, a series of existing sprites, that reference different textures is fed to the service and each sprite's dimensions is fit into a texture region, with the image from the 
/// original sprite texture transferred and packed into the new atlas texture. Should the texture not have enough empty space for the sprites, array indices will be used to store the extra sprites. 
/// In the worst case, multiple textures will be generated
/// </para>
/// </remarks>
public class GorgonTextureAtlasService
    : IGorgonTextureAtlasService
{
    // The 2D renderer used to generate the data.
    private readonly Gorgon2D _renderer;
    // The graphics interface used to generate the data.
    private readonly GorgonGraphics _graphics;
    // The size of the texture to generate.
    private GorgonPoint _textureSize = new(1024, 1024);
    // The number of array indices in the texture to generate.
    private int _arrayCount;

    /// <summary>
    /// Property to set or return the size of the texture to generate.
    /// </summary>
    /// <remarks>
    /// The maximum texture size is limited to the <see cref="IGorgonVideoAdapterInfo.MaxTextureWidth"/>, and <see cref="IGorgonVideoAdapterInfo.MaxTextureHeight"/> supported by the video adapter, and 
    /// the minimum is 256x256.
    /// </remarks>
    public GorgonPoint TextureSize
    {
        get => _textureSize;
        set => _textureSize = new GorgonPoint(value.X.Min(_graphics.VideoAdapter.MaxTextureWidth).Max(256),
                                        value.Y.Min(_graphics.VideoAdapter.MaxTextureHeight).Max(256));
    }

    /// <summary>Property to set or return the number of array indices available in the generated texture.</summary>
    /// <remarks>
    /// The maximum array indices is limited to the <see cref="IGorgonVideoAdapterInfo.MaxTextureArrayCount"/>, and the minimum is 1.
    /// </remarks>
    public int ArrayCount
    {
        get => _arrayCount;
        set => _arrayCount = value.Max(1).Min(_graphics.VideoAdapter.MaxTextureArrayCount);
    }

    /// <summary>Property to set or return the amount of padding, in pixels, to place around each sprite.</summary>        
    public int Padding
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the base name for the texture.
    /// </summary>
    public string BaseTextureName
    {
        get;
        set;
    }



    /// <summary>
    /// Function to calculate the size to the next power of two.
    /// </summary>
    /// <param name="size">The size to evaluate.</param>
    /// <returns>The power of 2 size.</returns>
    private GorgonPoint CalcPowerOfTwo(GorgonPoint size)
    {
        int w = 1;
        int h = 1;
        while (w < size.X)
        {
            w <<= 1;
        }

        while (h < size.Y)
        {
            h <<= 1;
        }

        return new GorgonPoint(w, h);
    }

    /// <summary>
    /// Function to retrieve the maximum texture size based on the sprites passed in.
    /// </summary>
    /// <param name="sprites">The sprites to evaluate.</param>
    /// <param name="maxTextureSize">The current maximum texture size.</param>
    /// <returns></returns>
    private GorgonPoint GetMaxTextureSize(IEnumerable<GorgonSprite> sprites, GorgonPoint maxTextureSize)
    {
        // Get all texture regions.
        IEnumerable<GorgonRectangle> spriteRegions = sprites.Select(item =>
        {
            GorgonRectangle pixelRegion = item.Texture.Texture.ToPixel(item.TextureRegion);
            if (Padding != 0)
            {
                pixelRegion = GorgonRectangle.Expand(pixelRegion, Padding);
            }
            return pixelRegion;
        })
        .OrderByDescending(item => item.Height);

        // If we have a sprite that's too large, then expand the total texture bounds.
        foreach (GorgonRectangle rect in spriteRegions)
        {
            if ((rect.Width <= maxTextureSize.X) && (rect.Height <= maxTextureSize.Y))
            {
                continue;
            }

            GorgonPoint newMaxSize = CalcPowerOfTwo(new GorgonPoint(rect.Width, rect.Height));

            if ((newMaxSize.X - rect.Width) < rect.Width / 4)
            {
                newMaxSize.X *= 2;
            }

            if ((newMaxSize.Y - rect.Height) < rect.Height / 4)
            {
                newMaxSize.Y *= 2;
            }

            maxTextureSize = newMaxSize;
        }

        return maxTextureSize;
    }

    /// <summary>
    /// Function to perform the calculations necessary to determine the sprite regions and array indices on a texture atlas.
    /// </summary>
    /// <param name="sprites">The sprites to evaluate.</param>
    /// <param name="maxTextureSize">The maximum size for a texture atlas.</param>
    /// <param name="maxArrayCount">The maximum number of array indices for an atlas.</param>
    /// <returns>The list of rectangles/array indices and a flag to indicate whether the sprites were already on an atlas or not.</returns>
    private (IReadOnlyList<IReadOnlyDictionary<int, TextureRects>> rects, bool noChange) CalculateRegions(IReadOnlyList<GorgonSprite> sprites, GorgonPoint maxTextureSize, int maxArrayCount)
    {
        List<IReadOnlyDictionary<int, TextureRects>> result = [];
        Dictionary<int, TextureRects> rects = [];
        GorgonRectangle spriteRegion;
        TextureRects newRect = null;
        GorgonRectangle textureBounds = new(0, 0, maxTextureSize.X, maxTextureSize.Y);

        // Scan for same texture.
        if (sprites.All(item => item.Texture.Texture == sprites[0].Texture.Texture))
        {
            // Everything's the same. So we're done.
            foreach (GorgonSprite sprite in sprites)
            {
                if (!rects.TryGetValue(sprite.TextureArrayIndex, out newRect))
                {
                    rects[sprite.TextureArrayIndex] = newRect = new TextureRects(new GorgonRectangle(0, 0, sprite.Texture.Texture.Width, sprite.Texture.Texture.Height), sprite.TextureArrayIndex);
                }

                newRect.SpriteRegion[sprite] = sprite.Texture.Texture.ToPixel(sprite.TextureRegion);
            }

            result.Add(rects);
            return (result, true);
        }

        // Get all texture regions.
        List<(GorgonSprite sprite, GorgonRectangle spriteRegion)> spriteRegions = [.. sprites.Select(item =>
        {
            GorgonRectangle region = item.Texture.Texture.ToPixel(item.TextureRegion);

            if (Padding != 0)
            {
                region = GorgonRectangle.Expand(region, Padding);
            }

            return (item, region);
        })
        .OrderByDescending(item => item.region.Height)];

        int array = 0;
        result.Add(rects);

        while (spriteRegions.Count > 0)
        {
            SpritePacker.CreateRoot(textureBounds.Width, textureBounds.Height);
            (GorgonSprite sprite, GorgonRectangle region)[] activeRegions = [.. spriteRegions];

            // Pack as many as possible into the current array/texture rect list.
            foreach ((GorgonSprite sprite, GorgonRectangle region) sprite in activeRegions)
            {
                // If a sprite can't find into the actual texture size, then stop right away.
                // Better to not have anything come back than have a single sprite missing out of many.
                if ((sprite.region.Width > textureBounds.Width) || (sprite.region.Height > textureBounds.Height))
                {
                    result.Clear();
                    return (result, false);
                }

                GorgonRectangle? newRegion = SpritePacker.Add(new GorgonPoint(sprite.region.Width, sprite.region.Height));

                if ((newRegion is null) || (newRegion.Value.Width == 0) || (newRegion.Value.Height == 0))
                {
                    continue;
                }

                spriteRegion = newRegion.Value;

                if (!rects.TryGetValue(array, out newRect))
                {
                    rects[array] = newRect = new TextureRects(textureBounds, array);
                }

                if (Padding != 0)
                {
                    spriteRegion = GorgonRectangle.Expand(spriteRegion, -Padding);
                }

                newRect.SpriteRegion[sprite.sprite] = spriteRegion;
                spriteRegions.Remove(sprite);
            }

            if (spriteRegions.Count == 0)
            {
                break;
            }

            // Increment to the next array value.
            ++array;

            // If we've exceeded our array count, then create a new texture.
            if (array >= maxArrayCount)
            {
                array = 0;
                rects = [];
                result.Add(rects);
            }
        }

        return (result, false);
    }

    /// <summary>
    /// Function to retrieve a list of all the regions occupied by the provided sprites on the texture.
    /// </summary>
    /// <param name="sprites">The list of sprites to evaluate.</param>
    /// <returns>A dictionary of sprites passed in, containing the region on the texture, and the array index for the sprite.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sprites"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This will return the new texture index, pixel coordinates and array index for each sprite on the texture atlas. The key values for the dictionary are the same sprite references passed in to the 
    /// method. Multiple <c>textureIndex</c> values are possible if the method could not find enough room to accomodate the sprites within the <see cref="TextureSize"/> and <see cref="ArrayCount"/>.
    /// </para>
    /// <para>
    /// If all <paramref name="sprites"/> already share the same texture, then this method will return <b>false</b>, and method will return with the original sprite locations since they're already 
    /// on an atlas.
    /// </para>
    /// <para>
    /// If a sprite in the <paramref name="sprites"/> list does not have an attached texture, then it will be ignored.
    /// </para>
    /// <para>
    /// If any of the <paramref name="sprites"/> cannot fit into the defined <see cref="TextureSize"/>, and <see cref="ArrayCount"/>, or there are no sprites in the <paramref name="sprites"/> 
    /// parameter, then this method will return an empty dictionary. To determine the best sizing, use the <see cref="GetBestFit"/> method. 
    /// </para>
    /// <para>
    /// The values of the dictionary contain the index of the texture (this could be greater than 0 if more than one texture is required), the pixel coordinates of the sprite on the atlas, and the 
    /// texture array index where the sprite will be placed.
    /// </para>
    /// </remarks>
    /// <seealso cref="GetBestFit"/>
    public IReadOnlyDictionary<GorgonSprite, (int textureIndex, GorgonRectangle region, int arrayIndex)> GetSpriteRegions(IEnumerable<GorgonSprite> sprites)
    {
        if (sprites is null)
        {
            throw new ArgumentNullException(nameof(sprites));
        }

        // Filter out empty sprites.
        IReadOnlyList<GorgonSprite> filtered = sprites.Where(item => (item.Texture is not null) && (!item.TextureRegion.Width.EqualsEpsilon(0)) && (!item.TextureRegion.Height.EqualsEpsilon(0)))
                                                      .Distinct()
                                                      .ToArray();

        Dictionary<GorgonSprite, (int textureIndex, GorgonRectangle rect, int arrayindex)> result = [];

        (IReadOnlyList<IReadOnlyDictionary<int, TextureRects>> rects, _) = CalculateRegions(filtered, TextureSize, ArrayCount);

        for (int i = 0; i < rects.Count; i++)
        {
            IReadOnlyDictionary<int, TextureRects> rectList = rects[i];
            foreach (KeyValuePair<int, TextureRects> item in rectList.Where(item => item.Key < ArrayCount))
            {
                foreach (KeyValuePair<GorgonSprite, GorgonRectangle> region in item.Value.SpriteRegion)
                {
                    result[region.Key] = (i, region.Value, item.Value.ArrayIndex);
                }
            }
        }

        return result;
    }


    /// <summary>
    /// Function to determine the best texture size and array count for the texture atlas based on the sprites passed in.
    /// </summary>
    /// <param name="sprites">The sprites to evaluate.</param>
    /// <param name="minTextureSize">The minimum size for the texture.</param>
    /// <param name="minArrayCount">The minimum array count for the texture.</param>
    /// <returns>A tuple containing the texture size, and array count.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="sprites"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// This will calculate how large the resulting atlas will be, and how many texture indices it will contain for the given <paramref name="sprites"/>.
    /// </para>
    /// <para>
    /// The <paramref name="minTextureSize"/>, and <paramref name="minArrayCount"/> are used to limit the size of the texture and array count to a value that is no smaller than these parameters. 
    /// </para>
    /// <para>
    /// If a sprite in the <paramref name="sprites"/> list does not have an attached texture, it will be ignored when calculating the size and will have no impact on the final result.
    /// </para>
    /// <para>
    /// If the <paramref name="sprites"/> cannot all fit into the texture atlas because it would exceed the maximum width and height of a texture for the video adapter, then this 
    /// value return 0x0 for the width and height, and 0 for the array count. If the array count maximum for the video card is exceeded, then the maximum width and height will be returned, but 
    /// the array count will be 0.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    public (GorgonPoint textureSize, int arrayCount) GetBestFit(IEnumerable<GorgonSprite> sprites, GorgonPoint minTextureSize, int minArrayCount)
    {
        if (sprites is null)
        {
            throw new ArgumentNullException(nameof(sprites));
        }

        // Filter out empty sprites.
        IReadOnlyList<GorgonSprite> filtered = sprites.Where(item => (item.Texture is not null) && (!item.TextureRegion.Width.EqualsEpsilon(0)) && (!item.TextureRegion.Height.EqualsEpsilon(0)))
                                                      .Distinct()
                                                      .ToArray();

        // If we've got all empty sprites, then leave, there's nothing to calculate.
        if (filtered.Count == 0)
        {
            return (GorgonPoint.Zero, 0);
        }

        minArrayCount = minArrayCount.Max(1).Min(_graphics.VideoAdapter.MaxTextureArrayCount);
        minTextureSize = new GorgonPoint(minTextureSize.X.Max(16).Min(_graphics.VideoAdapter.MaxTextureWidth),
                                      minTextureSize.Y.Max(16).Min(_graphics.VideoAdapter.MaxTextureHeight));

        GorgonPoint maxSize = GetMaxTextureSize(filtered, minTextureSize);

        if ((maxSize.X > _graphics.VideoAdapter.MaxTextureWidth) || (maxSize.Y > _graphics.VideoAdapter.MaxTextureHeight))
        {
            return (GorgonPoint.Zero, 0);
        }

        (IReadOnlyList<IReadOnlyDictionary<int, TextureRects>> rects, bool noChange) = CalculateRegions(filtered, maxSize, 1);

        if (rects.Count > _graphics.VideoAdapter.MaxTextureArrayCount)
        {
            return (maxSize, 0);
        }

        return rects.Count == 0 ? (GorgonPoint.Zero, 0) : (maxSize, rects.Count.Max(minArrayCount));
    }

    /// <summary>
    /// Function to generate the textures and sprites for the texture atlas.
    /// </summary>
    /// <param name="regions">The texture(s), array index and region for each sprite.</param>
    /// <param name="textureFormat">The format for the resulting texture(s).</param>
    /// <returns>A <see cref="GorgonTextureAtlas"/> object containing the new texture(s) and sprites.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="regions"/> parameter is <b>null</b>.</exception>
    /// <exception cref="GorgonException">Thrown if the <paramref name="textureFormat"/> is not compatible with render targets.</exception>
    /// <remarks>
    /// <para>
    /// Use this method to generate the final textures and sprites that comprise the texture atlas. All sprites returned are mapped to the new texture(s) and array indices.  
    /// </para>
    /// <para>
    /// The <see cref="GorgonTextureAtlas"/> returned will return 1 or more textures that will comprise the atlas. The method will generate multiple textures if the sprites do not all fit within 
    /// the <see cref="TextureSize"/> and <see cref="ArrayCount"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="regions"/> parameter is a list of non-atlased sprites and the associated texture index, texture region (in pixels), and the index on the texture array that the sprite will 
    /// reside. This value can be retrieved by calling the <see cref="GetSpriteRegions(IEnumerable{GorgonSprite})"/> method. 
    /// </para>
    /// <para>
    /// When passing the <paramref name="textureFormat"/>, the format should be compatible with render target formats. This can be determined by checking the 
    /// <see cref="IGorgonFormatSupportInfo.IsRenderTargetFormat"/> on the <see cref="GorgonGraphics"/>.<see cref="GorgonGraphics.FormatSupport"/> property. If the texture format is not supported as a 
    /// render target texture, an exception will be thrown.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonFormatSupportInfo"/>
    /// <seealso cref="GetSpriteRegions(IEnumerable{GorgonSprite})"/>
    /// <seealso cref="GorgonTextureAtlas"/>
    public GorgonTextureAtlas GenerateAtlas(IReadOnlyDictionary<GorgonSprite, (int textureIndex, GorgonRectangle region, int arrayIndex)> regions, BufferFormat textureFormat)
    {
        if (regions is null)
        {
            throw new ArgumentNullException(nameof(regions));
        }

        if ((textureFormat == BufferFormat.Unknown)
            || (!_graphics.FormatSupport.TryGetValue(textureFormat, out IGorgonFormatSupportInfo info))
            || (!info.IsRenderTargetFormat))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GOR2D_ERR_ATLAS_INVALID_FORMAT, textureFormat));
        }

        if (regions.Count == 0)
        {
            return new GorgonTextureAtlas([], Array.Empty<(GorgonSprite, GorgonSprite)>());
        }

        // Get the total number of textures.
        int textureCount = regions.Max(item => item.Value.textureIndex) + 1;
        GorgonTexture2DView[] srvs = new GorgonTexture2DView[textureCount];
        GorgonRenderTarget2DView rtv = null;
        GorgonRenderTargetView original = _graphics.RenderTargets[0];
        List<(GorgonSprite, GorgonSprite)> sprites = new(regions.Count);
        HashSet<GorgonRenderTargetView> rtvs = [];
        string textureName = $"{(string.IsNullOrWhiteSpace(BaseTextureName) ? "texture_atlas" : BaseTextureName)}_{{0}}.dds";

        try
        {
            // Create the new destinations.
            for (int textureIndex = 0; textureIndex < textureCount; ++textureIndex)
            {
                srvs[textureIndex] = GorgonTexture2DView.CreateTexture(_graphics, new GorgonTexture2DInfo(TextureSize.X, TextureSize.Y, textureFormat)
                {
                    Name = string.Format(textureName, textureIndex),
                    ArrayCount = ArrayCount,
                    Binding = TextureBinding.ShaderResource | TextureBinding.RenderTarget,
                    Usage = ResourceUsage.Default
                });
            }

            foreach (KeyValuePair<GorgonSprite, (int textureIndex, GorgonRectangle spriteRegion, int arrayIndex)> region in regions)
            {
                GorgonTexture2DView srv = srvs[region.Value.textureIndex];
                rtv = srv.Texture.GetRenderTargetView(arrayIndex: region.Value.arrayIndex);

                if (!rtvs.Contains(rtv))
                {
                    rtv.Clear(GorgonColors.BlackTransparent);
                    rtvs.Add(rtv);
                }

                _graphics.SetRenderTarget(rtv);

                _renderer.Begin(Gorgon2DBatchState.NoBlend);

                _renderer.DrawFilledRectangle(region.Value.spriteRegion,
                                            GorgonColors.White,
                                            region.Key.Texture,
                                            region.Key.TextureRegion,
                                            region.Key.TextureArrayIndex,
                                            GorgonSamplerState.PointFiltering);

                _renderer.End();

                _graphics.SetRenderTarget(original);

                sprites.Add((region.Key, new GorgonSprite(region.Key)
                {
                    Texture = srv,
                    TextureRegion = srv.ToTexel(region.Value.spriteRegion),
                    TextureArrayIndex = region.Value.arrayIndex
                }));
            }

            return new GorgonTextureAtlas(srvs, sprites);
        }
        finally
        {
            foreach (GorgonRenderTargetView cachedRtv in rtvs)
            {
                cachedRtv.Dispose();
            }

            _graphics.SetRenderTarget(original);
        }
    }



    /// <summary>Initializes a new instance of the <see cref="GorgonTextureAtlasService"/> class.</summary>
    /// <param name="renderer">The 2D renderer to use when generating the atlas data.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <strong>null</strong>.</exception>
    public GorgonTextureAtlasService(Gorgon2D renderer)
    {
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _graphics = _renderer.Graphics;
    }

}
