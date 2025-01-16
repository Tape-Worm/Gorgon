
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: July 20, 2016 10:40:09 PM
// 

using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.GdiPlus;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Extension methods used to create textures from images
/// </summary>
public static class GorgonImageTextureExtensions
{
    // Default load options.
    private static readonly GorgonTexture2DLoadOptions _defaultLoadOptions = new();

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
    ///		<item>
    ///		    <term>ConvertToPremultipliedAlpha</term>
    ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static GorgonTexture2D ToTexture2D(this Bitmap gdiBitmap,
                                              GorgonGraphics graphics,
                                              GorgonTexture2DLoadOptions options = null)
    {
        if (gdiBitmap is null)
        {
            throw new ArgumentNullException(nameof(gdiBitmap));
        }

        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        options ??= _defaultLoadOptions;

        if (string.IsNullOrEmpty(options.Name))
        {
            options.Name = GorgonGraphicsResource.GenerateName(GorgonTexture2D.NamePrefix);
        }

        using IGorgonImage image = gdiBitmap.ToGorgonImage();
        if (options.ConvertToPremultipliedAlpha)
        {
            image.BeginUpdate()
                 .ConvertToPremultipliedAlpha()
                 .EndUpdate();
        }

        return new GorgonTexture2D(graphics, image, options);
    }

    /// <summary>
    /// Function to create a <see cref="GorgonTexture2DView"/> from a GDI+ bitmap.
    /// </summary>
    /// <param name="gdiBitmap">The GDI+ bitmap used to create the texture.</param>
    /// <param name="graphics">The graphics interface used to create the texture.</param>
    /// <param name="options">[Optional] Options used to further define the texture.</param>
    /// <returns>A new <see cref="GorgonTexture2D"/> containing the data from the <paramref name="gdiBitmap"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="gdiBitmap"/>, or the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// A GDI+ bitmap is useful to holding image data in memory, but it cannot be sent to the GPU for use as a texture. This method allows an application to convert the GDI+ bitmap into a 
    /// <see cref="GorgonTexture2DView"/>. This view will be usable by pixel shaders.
    /// </para>
    /// <para>
    /// The resulting <see cref="GorgonTexture2DView"/> will only contain a single mip level, and single array level. The only image type available will be 2D (i.e. image with a width and height). The GDI+ 
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
    ///		<item>
    ///		    <term>ConvertToPremultipliedAlpha</term>
    ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static GorgonTexture2DView ToTexture2DView(this Bitmap gdiBitmap, GorgonGraphics graphics, GorgonTexture2DLoadOptions options = null)
    {
        if (gdiBitmap is null)
        {
            throw new ArgumentNullException(nameof(gdiBitmap));
        }

        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        options ??= _defaultLoadOptions;

        if (string.IsNullOrEmpty(options.Name))
        {
            options.Name = GorgonGraphicsResource.GenerateName(GorgonTexture2D.NamePrefix);
        }

        using IGorgonImage image = gdiBitmap.ToGorgonImage();
        if (options.ConvertToPremultipliedAlpha)
        {
            image.BeginUpdate()
                 .ConvertToPremultipliedAlpha()
                 .EndUpdate();
        }

        return GorgonTexture2DView.CreateTexture(graphics, new GorgonTexture2DInfo(image.Width, image.Height, image.Format)
        {
            Name = options.Name,
            ArrayCount = 1,
            MipLevels = 1,
            Binding = TextureBinding.ShaderResource | options.Binding,
            Usage = options.Usage,
            IsCubeMap = (options.IsTextureCube is null) ? image.ImageType == ImageDataType.ImageCube : options.IsTextureCube.Value,
            MultisampleInfo = options.MultisampleInfo
        }, image);
    }

    /// <summary>
    /// Function to create a <see cref="GorgonTexture2DView"/> from a <see cref="GorgonImage"/>.
    /// </summary>
    /// <param name="image">The image used to create the texture.</param>
    /// <param name="graphics">The graphics interface used to create the texture.</param>
    /// <param name="options">[Optional] Options used to further define the texture.</param>
    /// <returns>A new <see cref="GorgonTexture2DView"/> containing the data from the <paramref name="image"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="image"/>, or the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// A <see cref="GorgonImage"/> is useful to holding image data in memory, but it cannot be sent to the GPU for use as a texture. This method allows an application to convert the 
    /// <see cref="GorgonImage"/> into a <see cref="GorgonTexture2DView"/>. This view will be usable by pixel shaders.
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
    ///		<item>
    ///		    <term>ConvertToPremultipliedAlpha</term>
    ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static GorgonTexture2DView ToTexture2DView(this IGorgonImage image,
                                              GorgonGraphics graphics,
                                              GorgonTexture2DLoadOptions options = null)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        options ??= _defaultLoadOptions;

        if (string.IsNullOrEmpty(options.Name))
        {
            options.Name = GorgonGraphicsResource.GenerateName(GorgonTexture2D.NamePrefix);
        }

        if (options.ConvertToPremultipliedAlpha)
        {
            image.BeginUpdate()
                 .ConvertToPremultipliedAlpha()
                 .EndUpdate();
        }

        return GorgonTexture2DView.CreateTexture(graphics, new GorgonTexture2DInfo(image.Width, image.Height, image.Format)
        {
            Name = options.Name,
            ArrayCount = image.ArrayCount,
            MipLevels = image.MipCount,
            Binding = TextureBinding.ShaderResource | options.Binding,
            Usage = options.Usage,
            IsCubeMap = (options.IsTextureCube is null) ? image.ImageType == ImageDataType.ImageCube : options.IsTextureCube.Value,
            MultisampleInfo = options.MultisampleInfo
        }, image);
    }

    /// <summary>
    /// Function to create a <see cref="GorgonTexture3D"/> from a <see cref="GorgonImage"/>.
    /// </summary>
    /// <param name="image">The image used to create the texture.</param>
    /// <param name="graphics">The graphics interface used to create the texture.</param>
    /// <param name="options">[Optional] Options used to further define the texture.</param>
    /// <returns>A new <see cref="GorgonTexture3D"/> containing the data from the <paramref name="image"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="image"/>, or the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// A <see cref="GorgonImage"/> is useful to holding image data in memory, but it cannot be sent to the GPU for use as a texture. This method allows an application to convert the 
    /// <see cref="GorgonImage"/> into a <see cref="GorgonTexture3D"/>. 
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
    ///			<description>This is not available on 3D textures, and is ignored.</description>
    ///		</item>
    ///		<item>
    ///		    <term>ConvertToPremultipliedAlpha</term>
    ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static GorgonTexture3D ToTexture3D(this IGorgonImage image,
                                              GorgonGraphics graphics,
                                              GorgonTextureLoadOptions options = null)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        options ??= _defaultLoadOptions;

        if (string.IsNullOrEmpty(options.Name))
        {
            options.Name = GorgonGraphicsResource.GenerateName(GorgonTexture3D.NamePrefix);
        }

        if (options.ConvertToPremultipliedAlpha)
        {
            image.BeginUpdate()
                 .ConvertToPremultipliedAlpha()
                 .EndUpdate();
        }

        return new GorgonTexture3D(graphics, image, options);
    }

    /// <summary>
    /// Function to create a <see cref="GorgonTexture3DView"/> from a <see cref="GorgonImage"/>.
    /// </summary>
    /// <param name="image">The image used to create the texture.</param>
    /// <param name="graphics">The graphics interface used to create the texture.</param>
    /// <param name="options">[Optional] Options used to further define the texture.</param>
    /// <returns>A new <see cref="GorgonTexture3DView"/> containing the data from the <paramref name="image"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="image"/>, or the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// A <see cref="GorgonImage"/> is useful to holding image data in memory, but it cannot be sent to the GPU for use as a texture. This method allows an application to convert the 
    /// <see cref="GorgonImage"/> into a <see cref="GorgonTexture3DView"/>. This view will be usable by pixel shaders.
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
    ///			<description>This is not available on 3D textures, and is ignored.</description>
    ///		</item>
    ///		<item>
    ///		    <term>ConvertToPremultipliedAlpha</term>
    ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static GorgonTexture3DView ToTexture3DView(this IGorgonImage image,
                                              GorgonGraphics graphics,
                                              GorgonTextureLoadOptions options = null)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        options ??= _defaultLoadOptions;

        if (string.IsNullOrEmpty(options.Name))
        {
            options.Name = GorgonGraphicsResource.GenerateName(GorgonTexture3D.NamePrefix);
        }

        if (options.ConvertToPremultipliedAlpha)
        {
            image.BeginUpdate()
                 .ConvertToPremultipliedAlpha()
                 .EndUpdate();
        }

        return GorgonTexture3DView.CreateTexture(graphics, new GorgonTexture3DInfo(image.Width, image.Height, image.Depth, image.Format)
        {
            Name = options.Name,
            MipLevels = image.MipCount,
            Binding = TextureBinding.ShaderResource | options.Binding,
            Usage = options.Usage
        }, image);
    }

    /// <summary>
    /// Function to create a <see cref="GorgonTexture2D"/> from a <see cref="GorgonImage"/>.
    /// </summary>
    /// <param name="image">The image used to create the texture.</param>
    /// <param name="graphics">The graphics interface used to create the texture.</param>
    /// <param name="options">[Optional] Options used to further define the texture.</param>
    /// <returns>A new <see cref="GorgonTexture2D"/> containing the data from the <paramref name="image"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="image"/>, or the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// A <see cref="GorgonImage"/> is useful to holding image data in memory, but it cannot be sent to the GPU for use as a texture. This method allows an application to convert the 
    /// <see cref="GorgonImage"/> into a <see cref="GorgonTexture2D"/>. 
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
    ///		<item>
    ///		    <term>ConvertToPremultipliedAlpha</term>
    ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static GorgonTexture2D ToTexture2D(this IGorgonImage image,
                                              GorgonGraphics graphics,
                                              GorgonTexture2DLoadOptions options = null)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        options ??= _defaultLoadOptions;

        if (string.IsNullOrEmpty(options.Name))
        {
            options.Name = GorgonGraphicsResource.GenerateName(GorgonTexture2D.NamePrefix);
        }

        if (options.ConvertToPremultipliedAlpha)
        {
            image.BeginUpdate()
                 .ConvertToPremultipliedAlpha()
                 .EndUpdate();
        }

        return new GorgonTexture2D(graphics, image, options);
    }

    /// <summary>
    /// Function to create a <see cref="GorgonTexture1D"/> from a <see cref="GorgonImage"/>.
    /// </summary>
    /// <param name="image">The image used to create the texture.</param>
    /// <param name="graphics">The graphics interface used to create the texture.</param>
    /// <param name="options">[Optional] Options used to further define the texture.</param>
    /// <returns>A new <see cref="GorgonTexture1D"/> containing the data from the <paramref name="image"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="image"/>, or the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// A <see cref="GorgonImage"/> is useful to holding image data in memory, but it cannot be sent to the GPU for use as a texture. This method allows an application to convert the 
    /// <see cref="GorgonImage"/> into a <see cref="GorgonTexture1D"/>. 
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
    ///			<description>This is not available on 1D textures, and is ignored.</description>
    ///		</item>
    ///		<item>
    ///		    <term>ConvertToPremultipliedAlpha</term>
    ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static GorgonTexture1D ToTexture1D(this IGorgonImage image,
                                              GorgonGraphics graphics,
                                              GorgonTextureLoadOptions options = null)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        options ??= _defaultLoadOptions;

        if (string.IsNullOrEmpty(options.Name))
        {
            options.Name = GorgonGraphicsResource.GenerateName(GorgonTexture1D.NamePrefix);
        }

        if (options.ConvertToPremultipliedAlpha)
        {
            image.BeginUpdate()
                 .ConvertToPremultipliedAlpha()
                 .EndUpdate();
        }

        return new GorgonTexture1D(graphics, image, options);
    }

    /// <summary>
    /// Function to create a <see cref="GorgonTexture1DView"/> from a <see cref="GorgonImage"/>.
    /// </summary>
    /// <param name="image">The image used to create the texture.</param>
    /// <param name="graphics">The graphics interface used to create the texture.</param>
    /// <param name="options">[Optional] Options used to further define the texture.</param>
    /// <returns>A new <see cref="GorgonTexture1DView"/> containing the data from the <paramref name="image"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="image"/>, or the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
    /// <remarks>
    /// <para>
    /// A <see cref="GorgonImage"/> is useful to holding image data in memory, but it cannot be sent to the GPU for use as a texture. This method allows an application to convert the 
    /// <see cref="GorgonImage"/> into a <see cref="GorgonTexture1DView"/>. 
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
    ///			<description>This is not available on 1D textures, and is ignored.</description>
    ///		</item>
    ///		<item>
    ///		    <term>ConvertToPremultipliedAlpha</term>
    ///		    <description>Converts the image to premultiplied alpha before uploading the image data to the texture.</description>
    ///		</item>
    /// </list>
    /// </para>
    /// </remarks>
    public static GorgonTexture1DView ToTexture1DView(this IGorgonImage image, GorgonGraphics graphics, GorgonTextureLoadOptions options = null)
    {
        if (image is null)
        {
            throw new ArgumentNullException(nameof(image));
        }

        if (graphics is null)
        {
            throw new ArgumentNullException(nameof(graphics));
        }

        options ??= _defaultLoadOptions;

        if (string.IsNullOrEmpty(options.Name))
        {
            options.Name = GorgonGraphicsResource.GenerateName(GorgonTexture1D.NamePrefix);
        }

        if (options.ConvertToPremultipliedAlpha)
        {
            image.BeginUpdate()
                 .ConvertToPremultipliedAlpha()
                 .EndUpdate();
        }

        return GorgonTexture1DView.CreateTexture(graphics, new GorgonTexture1DInfo(image.Width, image.Format)
        {
            Name = options.Name,
            ArrayCount = image.ArrayCount,
            MipLevels = image.MipCount,
            Binding = TextureBinding.ShaderResource | options.Binding,
            Usage = options.Usage
        }, image);
    }
}
