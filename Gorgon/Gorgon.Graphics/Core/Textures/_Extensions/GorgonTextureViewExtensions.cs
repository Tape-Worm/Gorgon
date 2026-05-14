// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: April 20, 2026 8:01:46 PM
//

using Gorgon.Core;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Extension methods for the <see cref="IGorgonTextureView{T}"/> type.
/// </summary>
public static class GorgonTextureViewExtensions
{
    extension(IGorgonTextureView<GorgonTexture>)
    {
        /// <summary>
        /// Function to create a 1D texture and its default view.
        /// </summary>
        /// <param name="graphics">The graphics interface associated with the texture and view.</param>
        /// <param name="name">The name of the texture and view.</param>
        /// <param name="format">The texel format for the texture and view.</param>
        /// <param name="width">The width of the texture, in pixels.</param>
        /// <param name="mipCount">[Optional] The number of mip map levels in the texture.</param>
        /// <param name="arrayCount">[Optional] The number of array indices in the texture.</param>
        /// <returns>A new <see cref="IGorgonTextureView{GorgonTexture}"/> and its associated <see cref="GorgonTextureCommon"/>.</returns>
        /// <exception cref="GorgonException">
        /// <b>Texture Exceptions</b>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[1]"/>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[4]"/>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[5]"/>
        /// <b>View Exceptions</b>
        /// <inheritdoc cref="TextureView{T}.ValidateTextureView(string, GorgonFormatInfo, GorgonFormatInfo, IReadOnlyList{BufferFormat}, byte, bool, bool)" path="/exception/para[2]"/>
        /// <inheritdoc cref="TextureView{T}.ValidateTextureView(string, GorgonFormatInfo, GorgonFormatInfo, IReadOnlyList{BufferFormat}, byte, bool, bool)" path="/exception/para[5]"/>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function is a convenience method that builds a 1D texture with a default view to pass to shaders as a shader resource. Textures created with this method will be destroyed when the default 
        /// view returned is disposed.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonTextureView{T}"/>
        public static IGorgonTextureView<GorgonTexture> Create1DTexture(GorgonGraphics graphics, string name, BufferFormat format, int width, short mipCount = 1, short arrayCount = 1)
        {
            GorgonTextureInfo info = GorgonTextureInfo.Create1DTextureInfo(format, width, mipCount, arrayCount);
            GorgonTexture texture = new(graphics, name, info);

            try
            {
                return texture.GetTextureView<GorgonTexture>(format, 0, mipCount, 0, texture.ArrayCount, 0, 0, true);
            }
            catch
            {
                texture.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Function to create a 2D texture and its default view.
        /// </summary>
        /// <param name="graphics"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='graphics']"/></param>
        /// <param name="name"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='name']"/></param>
        /// <param name="format"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='format']"/></param>
        /// <param name="width"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='width']"/></param>
        /// <param name="height">The height of the texture, in pixels.</param>
        /// <param name="mipCount"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='mipCount']"/></param>
        /// <param name="arrayCount"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='arrayCount']"/></param>
        /// <inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/returns"/>
        /// <exception cref="GorgonException">
        /// <b>Texture Exceptions</b>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[1]"/>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[3]"/>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[10]"/>
        /// <b>View Exceptions</b>
        /// <inheritdoc cref="TextureView{T}.ValidateTextureView(string, GorgonFormatInfo, GorgonFormatInfo, IReadOnlyList{BufferFormat}, byte, bool, bool)" path="/exception/para[2]"/>
        /// <inheritdoc cref="TextureView{T}.ValidateTextureView(string, GorgonFormatInfo, GorgonFormatInfo, IReadOnlyList{BufferFormat}, byte, bool, bool)" path="/exception/para[5]"/>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function is a convenience method that builds a 2D texture with a default view to pass to shaders as a shader resource. Textures created with this method will be destroyed when the default 
        /// view returned is disposed.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonTextureView{T}"/>
        public static IGorgonTextureView<GorgonTexture> Create2DTexture(GorgonGraphics graphics, string name, BufferFormat format, int width, int height, short mipCount = 1, short arrayCount = 1)
        {
            GorgonTextureInfo info = GorgonTextureInfo.Create2DTextureInfo(format, width, height, mipCount, arrayCount);
            GorgonTexture texture = new(graphics, name, info);

            try
            {
                return texture.GetTextureView<GorgonTexture>(format, 0, mipCount, 0, texture.ArrayCount, 0, 0, true);
            }
            catch
            {
                texture.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Function to create a cube texture and its default view.
        /// </summary>
        /// <param name="graphics"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='graphics']"/></param>
        /// <param name="name"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='name']"/></param>
        /// <param name="format"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='format']"/></param>
        /// <param name="width"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='width']"/></param>
        /// <param name="height"><inheritdoc cref="Create2DTexture(GorgonGraphics, string, BufferFormat, int, int, short, short)" path="/param[@name='height']"/></param>
        /// <param name="cubeCount">The number of cube textures. One cube has 6 array indices.</param>
        /// <param name="mipCount"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='mipCount']"/></param>    
        /// <inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/returns"/>
        /// <exception cref="GorgonException">
        /// <b>Texture Exceptions</b>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[1]"/>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[3]"/>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[9]"/>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[10]"/>
        /// <b>View Exceptions</b>
        /// <inheritdoc cref="TextureView{T}.ValidateTextureView(string, GorgonFormatInfo, GorgonFormatInfo, IReadOnlyList{BufferFormat}, byte, bool, bool)" path="/exception/para[2]"/>
        /// <inheritdoc cref="TextureView{T}.ValidateTextureView(string, GorgonFormatInfo, GorgonFormatInfo, IReadOnlyList{BufferFormat}, byte, bool, bool)" path="/exception/para[5]"/>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function is a convenience method that builds a cube texture with a default view to pass to shaders as a shader resource. Textures created with this method will be destroyed when the default 
        /// view returned is disposed.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonTextureView{T}"/>
        public static IGorgonTextureView<GorgonTexture> CreateCubeTexture(GorgonGraphics graphics, string name, BufferFormat format, int width, int height, short cubeCount = 1, short mipCount = 1)
        {
            GorgonTextureInfo info = GorgonTextureInfo.CreateTextureCubeInfo(format, width, height, mipCount, cubeCount);
            GorgonTexture texture = new(graphics, name, info);

            try
            {
                return texture.GetTextureView<GorgonTexture>(format, 0, mipCount, 0, texture.ArrayCount, 0, 0, true);
            }
            catch
            {
                texture.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Function to create a 1D texture and its default view.
        /// </summary>
        /// <param name="graphics"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='graphics']"/></param>
        /// <param name="name"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='name']"/></param>
        /// <param name="format"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='format']"/></param>
        /// <param name="width"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='width']"/></param>
        /// <param name="height"><inheritdoc cref="Create2DTexture(GorgonGraphics, string, BufferFormat, int, int, short, short)" path="/param[@name='height']"/></param>
        /// <param name="depth">The number of depth slices in the texture.</param>
        /// <param name="mipCount"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='mipCount']"/></param>
        /// <inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/returns"/>
        /// <exception cref="GorgonException">
        /// <b>Texture Exceptions</b>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[1]"/>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para[2]"/>    
        /// <b>View Exceptions</b>
        /// <inheritdoc cref="TextureView{T}.ValidateTextureView(string, GorgonFormatInfo, GorgonFormatInfo, IReadOnlyList{BufferFormat}, byte, bool, bool)" path="/exception/para[2]"/>
        /// <inheritdoc cref="TextureView{T}.ValidateTextureView(string, GorgonFormatInfo, GorgonFormatInfo, IReadOnlyList{BufferFormat}, byte, bool, bool)" path="/exception/para[5]"/>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function is a convenience method that builds a 3D texture with a default view to pass to shaders as a shader resource. Textures created with this method will be destroyed when the default 
        /// view returned is disposed.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonTextureView{T}"/>
        public static IGorgonTextureView<GorgonTexture> Create3DTexture(GorgonGraphics graphics, string name, BufferFormat format, int width, int height, short depth, short mipCount = 1)
        {
            GorgonTextureInfo info = GorgonTextureInfo.Create3DTextureInfo(format, width, height, depth, mipCount);
            GorgonTexture texture = new(graphics, name, info);

            try
            {
                return texture.GetTextureView<GorgonTexture>(format, 0, mipCount, 0, 1, 0, 0, true);
            }
            catch
            {
                texture.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Function to create a texture and its default view from a <see cref="IGorgonImage"/>.
        /// </summary>
        /// <param name="graphics"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='graphics']"/></param>
        /// <param name="name"><inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/param[@name='name']"/></param>
        /// <param name="image">The image to build the texture from.</param>
        /// <inheritdoc cref="Create1DTexture(GorgonGraphics, string, BufferFormat, int, short, short)" path="/returns"/>
        /// <exception cref="GorgonException">
        /// <b>Texture Exceptions</b>
        /// <inheritdoc cref="GorgonTextureCommon.ValidateInfo(GorgonTextureInfo)" path="/exception/para"/>
        /// <b>View Exceptions</b>
        /// <inheritdoc cref="TextureView{T}.ValidateTextureView(string, GorgonFormatInfo, GorgonFormatInfo, IReadOnlyList{BufferFormat}, byte, bool, bool)" path="/exception/para"/>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This function is a convenience method that derives a texture from a <see cref="IGorgonImage"/> with a default view to pass to shaders as a shader resource. Textures created with this method will 
        /// be destroyed when the default view returned is disposed.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonTextureView{T}"/>
        /// <seealso cref="IGorgonImage"/>
        public static IGorgonTextureView<GorgonTexture> CreateTexture(GorgonGraphics graphics, string name, IGorgonImage image)
        {
            GorgonTexture texture = GorgonTexture.FromImage(graphics, name, image);

            try
            {
                return texture.GetTextureView<GorgonTexture>(texture.Format, 0, texture.MipCount, 0, texture.ArrayCount, 0, 0, true);
            }
            catch
            {
                texture.Dispose();
                throw;
            }
        }
    }
}
