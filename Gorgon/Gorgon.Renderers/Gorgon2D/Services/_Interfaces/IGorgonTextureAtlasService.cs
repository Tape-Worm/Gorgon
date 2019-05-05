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
// Created: May 2, 2019 9:11:18 AM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using DX = SharpDX;

namespace Gorgon.Renderers.Services
{
	/// <summary>
    /// A service used to generate a 2D texture atlas from a series of separate sprites.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To get the best performance when rendering sprites, batching is essential. In order to achieve this, rendering sprites that use the same texture is necessary. This is where building a texture atlas 
    /// comes in. A texture atlas is a combination of multiple sprite images into a single texture, each sprite that is embedded into the atlas uses a different set of texture coordinates. When rendering 
    /// these sprites with the atlas, only a single texture change is required.
    /// </para>
    /// <para>
    /// When generating an atlas, a series of existing sprites, that reference different textures is fed to the service and each sprite's dimensions is fit into a texture region, with the image from the 
    /// original sprite texture transferred and packed into the new atlas texture. Should the texture not have enough empty space for the sprites, array indices will be used to store the extra sprites. 
    /// In the worst case, multiple textures will be generated.
    /// </para>
    /// </remarks>
    public interface IGorgonTextureAtlasService
    {
        #region Properties.
		/// <summary>
        /// Property to set or return the amount of padding, in pixels, to place around each sprite.
        /// </summary>
		int Padding
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the size of the texture to generate.
        /// </summary>
        /// <remarks>
        /// The maximum texture size is limited to the <see cref="IGorgonVideoAdapterInfo.MaxTextureWidth"/>, and <see cref="IGorgonVideoAdapterInfo.MaxTextureHeight"/> supported by the video adapter, and 
        /// the minimum is 256x256.
        /// </remarks>
        DX.Size2 TextureSize
        {
            get;
            set;
        }

        /// <summary>Property to set or return the number of array indices available in the generated texture.</summary>
        /// <remarks>
        /// The maximum array indices is limited to the <see cref="IGorgonVideoAdapterInfo.MaxTextureArrayCount"/>, and the minimum is 1.
        /// </remarks>
        int ArrayCount
        {
            get;
            set;
        }

		/// <summary>
        /// Property to set or return the base name for the texture.
        /// </summary>
		string BaseTextureName
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to generate the textures and sprites for the texture atlas.
        /// </summary>
        /// <param name="regions">The texture(s), array index and region for each sprite.</param>
        /// <param name="textureFormat">The format for the resulting texture(s).</param>
        /// <returns>A <see cref="GorgonTextureAtlas"/> object containing the new texture(s) and sprites.</returns>
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
        /// <see cref="FormatSupportInfo.IsRenderTargetFormat"/> on the <see cref="GorgonGraphics"/>.<see cref="GorgonGraphics.FormatSupport"/> property. If the texture format is not supported as a 
        /// render target texture, an exception will be thrown.
        /// </para>
        /// </remarks>
        /// <seealso cref="GetSpriteRegions(IEnumerable{GorgonSprite})"/>
        /// <seealso cref="GorgonTextureAtlas"/>
        GorgonTextureAtlas GenerateAtlas(IReadOnlyDictionary<GorgonSprite, (int textureIndex, DX.Rectangle region, int arrayIndex)> regions, BufferFormat textureFormat);

        /// <summary>
        /// Function to retrieve a list of all the regions occupied by the provided sprites on the texture.
        /// </summary>
        /// <param name="sprites">The list of sprites to evaluate.</param>
        /// <returns>A dictionary of sprites passed in, containing the region on the texture, and the array index for the sprite.</returns>
        /// <remarks>
        /// <para>
        /// This will return the new pixel coordinates and array index for each sprite on the texture atlas. The key values for the dictionary are the same sprite references passed in to the method. 
        /// </para>
        /// <para>
        /// If all <paramref name="sprites"/> already share the same texture, then this method will return <b>false</b>, and <paramref name="result"/> will return with the original sprite texture and 
        /// original <paramref name="sprites"/> since the sprites are already part of an atlas.
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
        IReadOnlyDictionary<GorgonSprite, (int textureIndex, DX.Rectangle region, int arrayIndex)> GetSpriteRegions(IEnumerable<GorgonSprite> sprites);

        /// <summary>
        /// Function to determine the best texture size and array count for the texture atlas based on the sprites passed in.
        /// </summary>
        /// <param name="sprites">The sprites to evaluate.</param>
        /// <param name="minTextureSize">The minimum size for the texture.</param>
        /// <param name="minArrayCount">The minimum array count for the texture.</param>
        /// <returns>A tuple containing the texture size, and array count.</returns>
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
        (DX.Size2 textureSize, int arrayCount) GetBestFit(IEnumerable<GorgonSprite> sprites, DX.Size2 minTextureSize, int minArrayCount);
        #endregion
    }
}
