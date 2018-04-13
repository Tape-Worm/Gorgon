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
using Drawing = System.Drawing;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.GdiPlus;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Extension methods used to create textures from images.
    /// </summary>
    public static class GorgonImageTextureExtensions
	{
        // Default load options.
        private static readonly GorgonTextureLoadOptions _defaultLoadOptions = new GorgonTextureLoadOptions();

	    /// <summary>
	    /// Function to create a <see cref="GorgonTexture2D"/> from a GDI+ bitmap.
	    /// </summary>
	    /// <param name="gdiBitmap">The GDI+ bitmap used to create the texture.</param>
	    /// <param name="graphics">The graphics interface used to create the texture.</param>
	    /// <param name="options">[Optional] Options used to further define the texture.</param>
	    /// <returns>A new <see cref="GorgonTexture2D"/> containing the data from the <paramref name="gdiBitmap"/>.</returns>
	    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="gdiBitmap"/>, or the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
	    /// <remarks>
	    /// <para>
	    /// A GDI+ bitmap is useful to holding image data in memory, but it cannot be sent to the GPU for use as a texture. This method allows an application to convert the GDI+ bitmap into a 
	    /// <see cref="GorgonTexture2D"/>. 
	    /// </para>
	    /// <para>
	    /// The resulting <see cref="GorgonTexture2D"/> will only contain a single mip level, and single array level. The only image type available will be 2D (i.e. image with a width and height). The GDI+ 
	    /// bitmap should have a 32bpp rgba format, or a 24bpp rgb format or else an exception will be thrown.
	    /// </para>
	    /// <para>
	    /// The optional <paramref name="options"/>parameter will define how Gorgon and shaders should handle the texture.  The <see cref="GorgonTextureLoadOptions"/> type contains the following:
	    /// <list type="bullet">
	    ///		<item>
	    ///			<term>Binding</term>
	    ///			<description>When defined, will indicate the <see cref="TextureBinding"/> that defines how the texture will be bound to the graphics pipeline. If it is omitted, then the binding will be 
	    ///         <see cref="TextureBinding.ShaderResource"/>.</description>
	    ///		</item>
	    ///		<item>
	    ///			<term>Usage</term>
	    ///			<description>When defined, will indicate the preferred usage for the texture. If it is omitted, then the usage will be set to <see cref="ResourceUsage.Default"/>.</description>
	    ///		</item>
	    ///		<item>
	    ///			<term>Multisample info</term>
	    ///			<description>When defined (i.e. not <b>null</b>), defines the multisampling to apply to the texture. If omitted, then the default is <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.</description>
	    ///		</item>
	    /// </list>
	    /// </para>
	    /// </remarks>
	    public static GorgonTexture2D ToTexture2D(this Drawing.Bitmap gdiBitmap,
	                                              GorgonGraphics graphics,
	                                              GorgonTextureLoadOptions options = null)
	    {
	        if (gdiBitmap == null)
	        {
	            throw new ArgumentNullException(nameof(gdiBitmap));
	        }

	        if (graphics == null)
	        {
	            throw new ArgumentNullException(nameof(graphics));
	        }

	        if (options == null)
	        {
	            options = _defaultLoadOptions;
	        }

	        if (string.IsNullOrEmpty(options.Name))
	        {
	            options.Name = GorgonGraphicsResource.GenerateName(GorgonTexture2D.NamePrefix);
	        }

	        using (IGorgonImage image = gdiBitmap.ToGorgonImage())
	        {
	            return new GorgonTexture2D(graphics, image, options);
	        }
	    }

	    /// <summary>
	    /// Function to create a <see cref="GorgonTexture2D"/> from a <see cref="GorgonImage"/>.
	    /// </summary>
	    /// <param name="image">The image used to create the texture.</param>
	    /// <param name="graphics">The graphics interface used to create the texture.</param>
	    /// <param name="options">[Optional] Options used to further define the texture.</param>
	    /// <returns>A new <see cref="GorgonTexture2D"/> containing the data from the <paramref name="image"/>.</returns>
	    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="image"/>, or the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
	    /// <exception cref="GorgonException">Thrown if the <paramref name="image"/> is not a 2D image or cube map.</exception>
	    /// <remarks>
	    /// <para>
	    /// A <see cref="GorgonImage"/> is useful to holding image data in memory, but it cannot be sent to the GPU for use as a texture. This method allows an application to convert the 
	    /// <see cref="GorgonImage"/> into a <see cref="GorgonTexture2D"/>. 
	    /// </para>
	    /// <para>
	    /// The resulting <see cref="GorgonTexture2D"/> will inherit the <see cref="ImageType"/> (converted to the appropriate <see cref="TextureType"/>), width, height (for 2D/3D images), depth (for 3D images), 
	    /// mip map count, array count (for 1D/2D images), and depth count (for 3D images). If the <see cref="GorgonImage"/> being converted has an <see cref="ImageType"/> of <see cref="ImageType.ImageCube"/> 
	    /// then the resulting texture will be set to a <see cref="TextureType.Texture2D"/>, and it will have its <see cref="IGorgonTexture2DInfo.IsCubeMap"/> flag set to <b>true</b>.
	    /// </para>
	    /// <para>
	    /// If specified, the <paramref name="options"/>parameter will define how Gorgon and shaders should handle the texture.  The <see cref="GorgonTextureLoadOptions"/> type contains the following:
	    /// <list type="bullet">
	    ///		<item>
	    ///			<term>Binding</term>
	    ///			<description>When defined, will indicate the <see cref="TextureBinding"/> that defines how the texture will be bound to the graphics pipeline. If it is omitted, then the binding will be 
	    ///         <see cref="TextureBinding.ShaderResource"/>.</description>
	    ///		</item>
	    ///		<item>
	    ///			<term>Usage</term>
	    ///			<description>When defined, will indicate the preferred usage for the texture. If it is omitted, then the usage will be set to <see cref="ResourceUsage.Default"/>.</description>
	    ///		</item>
	    ///		<item>
	    ///			<term>Multisample info</term>
	    ///			<description>When defined (i.e. not <b>null</b>), defines the multisampling to apply to the texture. If omitted, then the default is <see cref="GorgonMultisampleInfo.NoMultiSampling"/>.</description>
	    ///		</item>
	    /// </list>
	    /// </para>
	    /// </remarks>
	    public static GorgonTexture2D ToTexture2D(this IGorgonImage image,
	                                              GorgonGraphics graphics,
                                                  GorgonTextureLoadOptions options = null)
	    {
	        if (image == null)
	        {
	            throw new ArgumentNullException(nameof(image));
	        }

	        if (graphics == null)
	        {
	            throw new ArgumentNullException(nameof(graphics));
	        }

	        if (options == null)
	        {
                options = _defaultLoadOptions;
	        }

	        if (string.IsNullOrEmpty(options.Name))
	        {
	            options.Name = GorgonGraphicsResource.GenerateName(GorgonTexture2D.NamePrefix);
	        }

	        return new GorgonTexture2D(graphics, image, options);
	    }
	}
}
