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
// Created: July 20, 2016 10:40:09 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Extension methods used to create textures from images.
	/// </summary>
	public static class GorgonImageTextureExtensions
	{
        /// <summary>
        /// Function to create a <see cref="GorgonTexture"/> from a <see cref="GorgonImage"/>.
        /// </summary>
        /// <param name="image">The image used to create the texture.</param>
        /// <param name="name">The name of the texture.</param>
        /// <param name="graphics">The graphics interface used to create the texture.</param>
        /// <param name="usage">[Optional] The intended usage for the texture.</param>
        /// <param name="binding">[Optional] The allowed bindings for the texture.</param>
        /// <param name="multiSampleInfo">[Optional] Multisampling information to apply to the texture.</param>
        /// <param name="log">[Optional] The log interface used for debugging.</param>
        /// <returns>A new <see cref="GorgonTexture"/> containing the data from the <paramref name="image"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="image"/>, <paramref name="graphics"/> or the <paramref name="name"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// A <see cref="GorgonImage"/> is useful to holding image data in memory, but it cannot be sent to the GPU for use as a texture. This method allows an application to convert the 
        /// <see cref="GorgonImage"/> into a <see cref="GorgonTexture"/>. 
        /// </para>
        /// <para>
        /// The resulting <see cref="GorgonTexture"/> will inherit the <see cref="ImageType"/> (converted to the appropriate <see cref="TextureType"/>), width, height (for 2D/3D images), depth (for 3D images), 
        /// mip map count, array count (for 1D/2D images), and depth count (for 3D images). If the <see cref="GorgonImage"/> being converted has an <see cref="ImageType"/> of <see cref="ImageType.ImageCube"/> 
        /// then the resulting texture will be set to a <see cref="TextureType.Texture2D"/>, and it will have its <see cref="IGorgonTextureInfo.IsCubeMap"/> flag set to <b>true</b>.
        /// </para>
        /// <para>
        /// The optional parameters define how Gorgon and shaders should handle the texture:
        /// <list type="bullet">
        ///		<item>
        ///			<term>Binding</term>
        ///			<description>When defined, will indicate the <see cref="TextureBinding"/> that defines how the texture will be bound to the graphics pipeline. If it is omitted, then the binding will be 
        ///         <see cref="TextureBinding.ShaderResource"/>.</description>
        ///		</item>
        ///		<item>
        ///			<term>Usage</term>
        ///			<description>When defined, will indicate the preferred usage for the texture. If it is omitted, then the usage will be set to <c>Default</c>.</description>
        ///		</item>
        ///		<item>
        ///			<term>Multisample info</term>
        ///			<description>When defined (i.e. not <b>null</b>), defines the multisampling to apply to the texture. If omitted, then the default is <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.</description>
        ///		</item>
        /// </list>
        /// </para>
        /// </remarks>
        public static GorgonTexture ToTexture(this IGorgonImage image,
		                                      string name,
											  GorgonGraphics graphics,
                                              D3D11.ResourceUsage usage = D3D11.ResourceUsage.Default,
                                              TextureBinding binding = TextureBinding.ShaderResource,
                                              GorgonMultisampleInfo? multiSampleInfo = null,
											  IGorgonLog log = null)
		{
			if (image == null)
			{
				throw new ArgumentNullException(nameof(image));
			}

			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

		    if (graphics == null)
		    {
		        throw new ArgumentNullException(nameof(graphics));
		    }

		    return new GorgonTexture(name, graphics, image, usage, binding, multiSampleInfo ?? GorgonMultisampleInfo.NoMultiSampling, log);
		}
	}
}
