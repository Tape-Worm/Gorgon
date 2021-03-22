#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 13, 2016 8:45:14 PM
// 
#endregion

using System;
using System.Xml.Linq;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core
{
#if NET5_0_OR_GREATER
    /// <summary>
    /// Information used to create a texture object.
    /// </summary>
    /// <param name="Width">The width of the texture, in pixels.</param>
    /// <param name="Height">The height of the texture, in pixels.</param>
    /// <param name="Format">The texel format for the texture.</param>
    public record GorgonTexture2DInfo(int Width, int Height, BufferFormat Format)
        : IGorgonTexture2DInfo, IGorgonImageInfo
    {
        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTexture2DInfo"/> class.
        /// </summary>
        /// <param name="info">A <see cref="IGorgonTexture2DInfo"/> to copy settings from.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public GorgonTexture2DInfo(IGorgonTexture2DInfo info)
            : this(info?.Width ?? throw new ArgumentNullException(nameof(info)), info.Height, info.Format)
        {
            Name = info.Name;
            ArrayCount = info.ArrayCount;
            Binding = info.Binding;
            IsCubeMap = info.IsCubeMap;
            MipLevels = info.MipLevels;
            MultisampleInfo = info.MultisampleInfo;
            Usage = info.Usage;
            Shared = info.Shared;
        }
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the type of image data.
        /// </summary>
        ImageType IGorgonImageInfo.ImageType => IsCubeMap ? ImageType.ImageCube : ImageType.Image2D;

        /// <summary>
        /// Property to return the depth of an image, in pixels.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This applies to 3D images only.  This parameter will be set to a value of 1 for a 1D or 2D image.
        /// </para>
        /// </remarks>
        int IGorgonImageInfo.Depth => 1;

        /// <summary>
        /// Property to return whether the image data is using premultiplied alpha.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value has no meaning for this type.
        /// </para>
        /// </remarks>
        bool IGorgonImageInfo.HasPreMultipliedAlpha => false;

        /// <summary>
        /// Property to return the number of mip map levels in the image.
        /// </summary>
        int IGorgonImageInfo.MipCount => MipLevels;

        /// <summary>
        /// Property to return whether the size of the texture is a power of 2 or not.
        /// </summary>
        bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0)
                                              && ((Height == 0) || (Height & (Height - 1)) == 0);

        /// <summary>
        /// Property to return the name of the texture.
        /// </summary>
        public string Name
        {
            get;
            init;
        } = GorgonGraphicsResource.GenerateName(GorgonTexture2D.NamePrefix);

        /// <summary>
        /// Property to return the number of array levels for a 1D or 2D texture.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When this value is greater than 0, the texture will be used as a texture array. If the texture is supposed to be a cube map, then this value should be a multiple of 6 (1 for each face in the cube).
        /// </para>
        /// <para>
        /// This value is defaulted to 1.
        /// </para>
        /// </remarks>
        public int ArrayCount
        {
            get;
            init;
        } = 1;

        /// <summary>
        /// Property to return whether this 2D texture is a cube map.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When this value is set to <b>true</b>, then the texture is defined as a cube map using the <see cref="ArrayCount"/> as the number of faces. Because of this, the <see cref="ArrayCount"/> value 
        /// must be a multiple of 6. If it is not, then the array count will be adjusted to meet the requirement.
        /// </para>
        /// <para>
        /// This value is defaulted to <b>false</b>.
        /// </para>
        /// </remarks>
        public bool IsCubeMap
        {
            get;
            init;
        }

        /// <summary>
        /// Property to return the number of mip-map levels for the texture.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the texture is multisampled, this value must be set to 1.
        /// </para>
        /// <para>
        /// This value is defaulted to 1.
        /// </para>
        /// </remarks>
        public int MipLevels
        {
            get;
            init;
        } = 1;

        /// <summary>
        /// Property to return the multisample quality and count for this texture.
        /// </summary>
        /// <remarks>
        /// This value is defaulted to <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.
        /// </remarks>
        public GorgonMultisampleInfo MultisampleInfo
        {
            get;
            init;
        } = GorgonMultisampleInfo.NoMultiSampling;

        /// <summary>
        /// Property to return the intended usage flags for this texture.
        /// </summary>
        /// <remarks>
        /// This value is defaulted to <c>Default</c>.
        /// </remarks>
        public ResourceUsage Usage
        {
            get;
            init;
        } = ResourceUsage.Default;

        /// <summary>
        /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the <see cref="Usage"/> property is set to <c>Staging</c>, then the texture must be created with a value of <see cref="TextureBinding.None"/> as staging textures do not 
        /// support bindings of any kind. If this value is set to anything other than <see cref="TextureBinding.None"/>, an exception will be thrown.
        /// </para>
        /// <para>
        /// This value is defaulted to <see cref="TextureBinding.ShaderResource"/>. 
        /// </para>
        /// </remarks>
        public TextureBinding Binding
        {
            get;
            init;
        } = TextureBinding.ShaderResource;

        /// <summary>Property to return whether this texture can be shared with other graphics interfaces.</summary>
        /// <remarks>
        /// Settings this flag to <b>true</b> allows the texture to be used with external graphics interfaces such as a Direct3D device. This is useful for providing interoperation between systems.
        /// </remarks>
        public bool Shared
        {
            get;
            init;
        }
        #endregion
    }
#else
    /// <summary>
    /// Information used to create a texture object.
    /// </summary>
    public class GorgonTexture2DInfo
        : IGorgonTexture2DInfo, IGorgonImageInfo
    {
        #region Properties.
        /// <summary>
        /// Property to return the type of image data.
        /// </summary>
        ImageType IGorgonImageInfo.ImageType => IsCubeMap ? ImageType.ImageCube : ImageType.Image2D;

        /// <summary>
        /// Property to return the depth of an image, in pixels.
        /// </summary>
        int IGorgonImageInfo.Depth => 1;

        /// <summary>
        /// Property to return whether the image data is using premultiplied alpha.
        /// </summary>
        bool IGorgonImageInfo.HasPreMultipliedAlpha => false;

        /// <summary>
        /// Property to return the number of mip map levels in the image.
        /// </summary>
        int IGorgonImageInfo.MipCount => MipLevels;

        /// <summary>
        /// Property to return whether the size of the texture is a power of 2 or not.
        /// </summary>
        bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0)
                                              && ((Height == 0) || (Height & (Height - 1)) == 0);

        /// <summary>
        /// Property to return the name of the texture.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the width of the texture, in pixels.
        /// </summary>
        public int Width
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the height of the texture, in pixels.
        /// </summary>
        public int Height
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the number of array levels for a 1D or 2D texture.
        /// </summary>
        public int ArrayCount
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether this 2D texture is a cube map.
        /// </summary>
        public bool IsCubeMap
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the format of the texture.
        /// </summary>
        public BufferFormat Format
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the number of mip-map levels for the texture.
        /// </summary>
        public int MipLevels
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the multisample quality and count for this texture.
        /// </summary>
        public GorgonMultisampleInfo MultisampleInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the intended usage flags for this texture.
        /// </summary>
        public ResourceUsage Usage
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
        /// </summary>
        public TextureBinding Binding
        {
            get;
            set;
        }

        /// <summary>Property to return whether this texture can be shared with other graphics interfaces.</summary>
        public bool Shared
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to copy another information object to this object with a new name.
        /// </summary>
        /// <param name="name">The name of the object.</param>
        /// <param name="info">The destination for the copied data.</param>
        internal void Copy(string name, IGorgonTexture2DInfo info)
        {
            Name = name;
            Format = info.Format;
            ArrayCount = info.ArrayCount;
            Binding = info.Binding;
            Height = info.Height;
            IsCubeMap = info.IsCubeMap;
            MipLevels = info.MipLevels;
            MultisampleInfo = info.MultisampleInfo;
            Usage = info.Usage;
            Width = info.Width;
            Shared = info.Shared;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="GorgonTexture2DInfo" /> class.</summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="format">The format.</param>
        public GorgonTexture2DInfo(int width, int height, BufferFormat format)
        {
            Width = width;
            Height = height;
            Format = format;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTexture2DInfo"/> class.
        /// </summary>
        /// <param name="info">A <see cref="IGorgonTexture2DInfo"/> to copy settings from.</param>
        /// <param name="newName">[Optional] The new name for the texture.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public GorgonTexture2DInfo(IGorgonTexture2DInfo info, string newName = null) => Copy(string.IsNullOrEmpty(newName) ? info.Name : newName, info);
        #endregion
    }
#endif
}
