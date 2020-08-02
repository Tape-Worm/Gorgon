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
// Created: August 2, 2020 12:56:22 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Editor;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers;

namespace Gorgon.IO
{
    /// <summary>
    /// Loads <see cref="Gorgon2D"/> specific content from an editor file system.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This provides a set of convenience methods to load <see cref="Gorgon2D"/> content such as sprites from an editor file system.  
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// These methods load the data using the layout and metadata information as provided by the default plug ins for the editor.  Custom plug ins for sprite data, etc... may not work with these methods 
    /// unless those plug ins follow the same file layout as the default plug ins.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public interface IGorgonContentLoader
    {
        /// <summary>
        /// Property to return a list of codecs that can be used to load animation content data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Codecs added here are for external codecs. All built-in codecs for Gorgon will not appear in this list and are always used when loading files.
        /// </para>
        /// <para>
        /// If a codec in the list has the same fully qualified type name as a built-in Gorgon codec, then the external codec type will be used instead.
        /// </para>
        /// </remarks>
        IList<IGorgonAnimationCodec> ExternalAnimationCodecs
        {
            get;
        }

        /// <summary>
        /// Property to return a list of codecs that can be used to load image content data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Codecs added here are for external codecs. All built-in codecs for Gorgon will not appear in this list and are always used when loading files.
        /// </para>
        /// <para>
        /// If a codec in the list has the same fully qualified type name as a built-in Gorgon codec, then the external codec type will be used instead.
        /// </para>
        /// </remarks>
        IList<IGorgonImageCodec> ExternalImageCodecs
        {
            get;
        }

        /// <summary>
        /// Property to return a list of codecs that can be used to load sprite content data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Codecs added here are for external codecs. All built-in codecs for Gorgon will not appear in this list and are always used when loading files.
        /// </para>
        /// <para>
        /// If a codec in the list has the same fully qualified type name as a built-in Gorgon codec, then the external codec type will be used instead.
        /// </para>
        /// </remarks>
        IList<IGorgonSpriteCodec> ExternalSpriteCodecs
        {
            get;
        }

        /// <summary>
        /// Property to return the texture cache for the loader.
        /// </summary>
        GorgonTextureCache<GorgonTexture2D> TextureCache
        {
            get;
        }

        /// <summary>
        /// Function to load an image from the editor file system.
        /// </summary>
        /// <param name="path">The path to the image content.</param>
        /// <returns>A new <see cref="IGorgonImage"/> containing the image data from the file system.</returns>
        /// <remarks>
        /// <para>
        /// This method will load a <see cref="IGorgonImage"/> from a Gorgon Editor file system mounted as a <see cref="IGorgonFileSystem"/>.  
        /// </para>
        /// <para>
        /// If the image is not in a format known by Gorgon, then users should add the <see cref="IGorgonImageCodec"/> for reading the sprite data to the <see cref="ExternalImageCodecs"/> list. 
        /// Doing this will allow a user to create a custom image codec plug in and use that to read image data.
        /// </para>
        /// <para>
        /// <h2>Technical info</h2>
        /// <para>
        /// Plug ins must generate the following metadata for the files in the editor file system.
        /// </para>
        /// <para>
        /// The texture file metadata must have the following attributes: <c>Type</c> with a value set to the <see cref="CommonEditorContentTypes.ImageType"/>, and an attribute called <c>ImageCodec</c> 
        /// with the fully qualified type name of the image codec as its value or the texure will not load.
        /// </para>
        /// <para>
        /// If the image file has been marked as premultiplied in the editor, then the texture will be converted to use premultiplied alpha when loading. This is only done when the texture is read from the 
        /// file system, cached textures will left as-is.
        /// </para>
        /// </para>
        /// </remarks>        
        IGorgonImage LoadImage(string path);

        /// <summary>
        /// Function to load an animation from the editor file system.
        /// </summary>
        /// <param name="path">The path to the animation content.</param>
        /// <returns>A new <see cref="IGorgonAnimation"/> containing the animation data from the file system.</returns>
        /// <remarks>
        /// <para>
        /// This method will load a <see cref="IGorgonAnimation"/> from a Gorgon Editor file system mounted as a <see cref="IGorgonFileSystem"/>.  
        /// </para>
        /// <para>
        /// If the animation is not in a format known by Gorgon, then users should add the <see cref="IGorgonAnimationCodec"/> for reading the sprite data to the <see cref="ExternalAnimationCodecs"/> list. 
        /// Doing this will allow a user to create a custom image codec plug in and use that to read animation data.
        /// </para>
        /// <para>
        /// <h2>Technical info</h2>
        /// <para>
        /// Plug ins must generate the following metadata for the files in the editor file system.
        /// </para>
        /// <para>
        /// The animation file metadata must have the following attributes: <c>Type</c> with a value set to the <see cref="CommonEditorContentTypes.AnimationType"/>, and an attribute called <c>AnimationCodec</c> 
        /// with the fully qualified type name of the animation codec as its value or the animation will not load.
        /// </para>
        /// </para>
        /// </remarks>
        Task<IGorgonAnimation> LoadAnimationAsync(string path);

        /// <summary>
        /// Function to load an image as a texture from the editor file system.
        /// </summary>
        /// <param name="path">The path to the image context.</param>
        /// <param name="cache">[Optional] <b>true</b> to use the texture cache, <b>false</b> to load a new instance.</param>
        /// <returns>A new <see cref="GorgonTexture2D"/> containing the image data from the file system.</returns>
        /// <remarks>
        /// <para>
        /// This method will load a <see cref="GorgonTexture2D"/> from a Gorgon Editor file system mounted as a <see cref="IGorgonFileSystem"/>.  
        /// </para>
        /// <para>
        /// If the texture image is not in a format known by Gorgon, then users should add the <see cref="IGorgonImageCodec"/> for reading the sprite data to the <see cref="ExternalImageCodecs"/> list. 
        /// Doing this will allow a user to create a custom image codec plug in and use that to read image data.
        /// </para>
        /// <para>
        /// If the <paramref name="cache"/> parameter is set to <b>true</b>, then this method will load the data from the <see cref="TextureCache"/>. If the texture data is not in the cache, then it will 
        /// be added to the cache and returned. If the parameter is set to <b>false</b>, then the image data will be loaded as a new texture outside of the cache and it will be the responsibility of the 
        /// developer to manage its lifetime.
        /// </para>
        /// <para>
        /// <h2>Technical info</h2>
        /// <para>
        /// Plug ins must generate the following metadata for the files in the editor file system.
        /// </para>
        /// <para>
        /// The texture file metadata must have the following attributes: <c>Type</c> with a value set to the <see cref="CommonEditorContentTypes.ImageType"/>, and an attribute called <c>ImageCodec</c> 
        /// with the fully qualified type name of the image codec as its value or the texure will not load.
        /// </para>
        /// <para>
        /// If the image file has been marked as premultiplied in the editor, then the texture will be converted to use premultiplied alpha when loading. This is only done when the texture is read from the 
        /// file system, cached textures will left as-is.
        /// </para>
        /// </para>
        /// </remarks>        
        Task<GorgonTexture2D> LoadTextureAsync(string path, bool cache = true);

        /// <summary>
        /// Function to load sprite content from the editor file system.
        /// </summary>
        /// <param name="path">The path to the sprite to load.</param>
        /// <param name="overrideTexture">[Optional] When supplied, this will override the associated texture for the sprite.</param>
        /// <returns>The <see cref="GorgonSprite"/> content for the file system.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
        /// <exception cref="GorgonException">Thrown if the file system isn't a Gorgon Editor file system, or the file could not be read.</exception>
        /// <remarks>
        /// <para>
        /// This method will load a <see cref="GorgonSprite"/> from a Gorgon Editor file system mounted as a <see cref="IGorgonFileSystem"/>.  
        /// </para>
        /// <para>
        /// Providing the <paramref name="overrideTexture"/> will skip the texture loading and use the texture passed in.  In this case, the texture return value will be <b>null</b> as it is assumed the 
        /// user already knows about the texture resource and is managing the lifetime of the texture elsewhere.
        /// </para>
        /// <para>
        /// If the sprite is not in a format known by Gorgon, then users should add the <see cref="IGorgonSpriteCodec"/> for reading the sprite data to the <see cref="ExternalSpriteCodecs"/> list. 
        /// Doing this will allow a user to create a custom sprite codec plug in and use that to read sprite data.
        /// </para>
        /// <para>
        /// <h2>Technical info</h2>
        /// <para>
        /// Plug ins must generate the following metadata for the files in the editor file system.
        /// </para>
        /// <para>
        /// The sprite file metadata must have the following attributes: <c>Type</c> with a value of "Sprite", and <c>SpriteCodec</c>, and its associated texture must have a dependency type of <c>Image</c> 
        /// or else the sprite will not load.
        /// </para>
        /// <para>
        /// The associated texture file metadata must have the following attributes: <c>Type</c> with a value of "Image", and <c>ImageCodec</c> or the texure will not load.
        /// </para>
        /// <para>
        /// If the associated texture file has been marked as premultiplied in the editor, then the texture will be converted to use premultiplied alpha when loading. This is only done when the texture is 
        /// read from the file system, cached textures will left as-is.
        /// </para>
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// <b>Regarding sprite textures:</b> Gorgon will use the <see cref="GorgonTextureCache{GorgonTexture2D}"/> object to cache any textures loaded from the file system. This cache is exposed to the user 
        /// via the <see cref="TextureCache"/> property. It is up to the developer to ensure the cache is managed correctly for their applications. 
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        Task<GorgonSprite> LoadSpriteAsync(string path, GorgonTexture2DView overrideTexture = null);
    }
}
