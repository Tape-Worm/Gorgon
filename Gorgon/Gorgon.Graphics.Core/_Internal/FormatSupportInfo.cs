#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: November 21, 2017 8:44:47 PM
// 
#endregion

using Gorgon.Graphics.Imaging;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines the support given to a specific <see cref="BufferFormat"/>.
    /// </summary>
    internal class FormatSupportInfo : IGorgonFormatSupportInfo
    {
        #region Properties.
        /// <summary>
        /// Property to return the format that is being queried for support.
        /// </summary>
        public BufferFormat Format
        {
            get;
        }

        /// <summary>
        /// Property to return the resource support for a format.
        /// </summary>
        public BufferFormatSupport FormatSupport
        {
            get;
        }

        /// <summary>
        /// Property to return whether this format is suitable for use for presentation to the output device.
        /// </summary>
        public bool IsDisplayFormat => (FormatSupport & BufferFormatSupport.Display) == BufferFormatSupport.Display;

        /// <summary>
        /// Property to return whether this format is suitable for use as a render target.
        /// </summary>
        public bool IsRenderTargetFormat => (FormatSupport & BufferFormatSupport.RenderTarget) == BufferFormatSupport.RenderTarget;

        /// <summary>
        /// Property to return whether this format is suitable for use in a depth/stencil buffer.
        /// </summary>
        public bool IsDepthBufferFormat => (FormatSupport & BufferFormatSupport.DepthStencil) == BufferFormatSupport.DepthStencil;

        /// <summary>
        /// Property to return whether this format is suitable for use in a vertex buffer.
        /// </summary>
        public bool IsVertexBufferFormat => (FormatSupport & BufferFormatSupport.VertexBuffer) == BufferFormatSupport.VertexBuffer;

        /// <summary>
        /// Property to return whether this format is suitable for use in an index buffer.
        /// </summary>
        public bool IsIndexBufferFormat => (FormatSupport & BufferFormatSupport.VertexBuffer) == BufferFormatSupport.VertexBuffer;

        /// <summary>
        /// Property to return the compute shader/uav support for a format.
        /// </summary>
        public ComputeShaderFormatSupport ComputeSupport
        {
            get;
        }

        /// <summary>
        /// Property to return the maximum multisample count and quality level support for the format.
        /// </summary>
        public GorgonMultisampleInfo MaxMultisampleCountQuality
        {
            get;
        }
        #endregion

        #region Methods.

        /// <summary>
        /// Function to determine if a format is suitable for the texture type specified by <see cref="ImageType"/>.
        /// </summary>
        /// <param name="imageType">The image type to evaluate.</param>
        /// <returns><b>true</b> if suitable, <b>false</b> if not.</returns>
        public bool IsTextureFormat(ImageType imageType) => imageType switch
        {
            ImageType.Image1D => (FormatSupport & BufferFormatSupport.Texture1D) == BufferFormatSupport.Texture1D,
            ImageType.Image2D => (FormatSupport & BufferFormatSupport.Texture2D) == BufferFormatSupport.Texture2D,
            ImageType.Image3D => (FormatSupport & BufferFormatSupport.Texture3D) == BufferFormatSupport.Texture3D,
            ImageType.ImageCube => (FormatSupport & BufferFormatSupport.TextureCube) == BufferFormatSupport.TextureCube,
            _ => false,
        };
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatSupportInfo"/> class.
        /// </summary>
        /// <param name="format">The format being queried.</param>
        /// <param name="formatSupport">The format support.</param>
        /// <param name="computeSupport">The compute support.</param>
        /// <param name="multisampleMax">The multisample maximum.</param>
        public FormatSupportInfo(BufferFormat format,
                                         D3D11.FormatSupport formatSupport,
                                         D3D11.ComputeShaderFormatSupport computeSupport,
                                         GorgonMultisampleInfo multisampleMax)
        {
            Format = format;
            FormatSupport = (BufferFormatSupport)formatSupport;
            ComputeSupport = (ComputeShaderFormatSupport)computeSupport;
            MaxMultisampleCountQuality = multisampleMax;
        }
        #endregion
    }
}
