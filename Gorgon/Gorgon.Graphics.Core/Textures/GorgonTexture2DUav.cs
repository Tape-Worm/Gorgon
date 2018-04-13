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
// Created: July 22, 2017 10:31:48 AM
// 
#endregion

using System;
using System.IO;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using SharpDX.Mathematics.Interop;
using DXGI = SharpDX.DXGI;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides an unordered access view for a <see cref="GorgonTexture2D"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type of view allows for unordered access to a <see cref="GorgonTexture2D"/>. The texture must have been created with the <see cref="TextureBinding.UnorderedAccess"/> flag in its 
    /// <see cref="IGorgonTexture2DInfo.Binding"/> property.
    /// </para>
    /// <para>
    /// The unordered access allows a shader to read/write any part of a <see cref="GorgonGraphicsResource"/> by multiple threads without memory contention. This is done through the use of 
    /// <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476334(v=vs.85).aspx">atomic functions</a>.
    /// </para>
    /// <para>
    /// These types of views are most useful for <see cref="GorgonComputeShader"/> shaders, but can also be used by a <see cref="GorgonPixelShader"/> by passing a list of these views in to a 
    /// <see cref="GorgonDrawCallBase">draw call</see>.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Unordered access views do not support <see cref="GorgonTexture2D"/> textures with <see cref="GorgonMultisampleInfo">multisampling</see> enabled.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsResource"/>
    /// <seealso cref="GorgonTexture2D"/>
    /// <seealso cref="GorgonComputeShader"/>
    /// <seealso cref="GorgonPixelShader"/>
    /// <seealso cref="GorgonDrawCallBase"/>
    /// <seealso cref="GorgonMultisampleInfo"/>
    public sealed class GorgonTexture2DUav
        : GorgonUnorderedAccessView, IGorgonTexture2DInfo
    {
        #region Variables.
        // The texture bound to the view.
        private GorgonTexture2D _texture;
        // Rectangles used for clearing the view.
        private RawRectangle[] _clearRects;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the index of the first mip map in the resource to view.
        /// </summary>
        public int MipSlice
        {
            get;
        }

        /// <summary>
        /// Property to return the first array index or depth slice to use in the view.
        /// </summary>
        public int ArrayIndex
        {
            get;
        }

        /// <summary>
        /// Property to return the number of array indices to use in the view.
        /// </summary>
        public int ArrayCount
        {
            get;
        }

        /// <summary>
        /// Property to return whether the texture is a texture cube or not.
        /// </summary>
        public bool IsCubeMap => Texture?.IsCubeMap ?? false;

        /// <summary>
        /// Property to return the texture that is bound to this view.
        /// </summary>
        public GorgonTexture2D Texture => _texture;

        /// <summary>
        /// Property to return the format used to interpret this view.
        /// </summary>
        public BufferFormat Format
        {
            get;
        }

        /// <summary>
        /// Property to return information about the <see cref="Format"/> used by this view.
        /// </summary>
        public GorgonFormatInfo FormatInformation
        {
            get;
        }

        /// <summary>
        /// Property to return the bounding rectangle for the view.
        /// </summary>
        /// <remarks>
        /// This value is the full bounding rectangle of the first mip map level for the texture associated with the render target.
        /// </remarks>
        public DX.Rectangle Bounds
        {
            get;
        }

        /// <summary>
        /// Property to return the width of the texture in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the full width of the first mip map level for the texture associated with the view.
        /// </remarks>
        public int Width => Texture?.Width ?? 0;

        /// <summary>
        /// Property to return the height of the texture in pixels.
        /// </summary>
        /// <remarks>
        /// This value is the full height of the first mip map level for the texture associated with the view.
        /// </remarks>
        public int Height => Texture?.Height ?? 0;

        /// <summary>
        /// Property to return the name of the texture.
        /// </summary>
        string IGorgonNamedObject.Name => Texture?.Name ?? string.Empty;

        /// <summary>
        /// Property to return the number of mip-map levels for the texture.
        /// </summary>
        int IGorgonTexture2DInfo.MipLevels => Texture?.MipLevels ?? 0;

        GorgonMultisampleInfo IGorgonTexture2DInfo.MultisampleInfo => GorgonMultisampleInfo.NoMultiSampling;

        /// <summary>
        /// Property to return the intended usage flags for this texture.
        /// </summary>
        public ResourceUsage Usage => Texture?.Usage ?? ResourceUsage.Default;

        /// <summary>
        /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
        /// </summary>
        public TextureBinding Binding => Texture?.Binding ?? TextureBinding.None;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the view description for a 2D texture.
        /// </summary>
        /// <param name="texture">Texture to build a view description for.</param>
        /// <returns>The shader view description.</returns>
        private D3D11.UnorderedAccessViewDescription1 GetDesc2D(GorgonTexture2D texture)
        {
            return new D3D11.UnorderedAccessViewDescription1
                   {
                       Format = (DXGI.Format)Format,
                       Dimension = texture.ArrayCount > 1
                                       ? D3D11.UnorderedAccessViewDimension.Texture2DArray
                                       : D3D11.UnorderedAccessViewDimension.Texture2D,
                       Texture2DArray =
                       {
                           MipSlice =  MipSlice,
                           FirstArraySlice = ArrayCount,
                           ArraySize = ArrayIndex,
                           PlaneSlice = 0
                       }
                   };
        }

        /// <summary>
        /// Function to initialize the unordered access view.
        /// </summary>
        protected internal override void CreateNativeView()
        {
            D3D11.UnorderedAccessViewDescription1 desc = GetDesc2D(_texture);
            
            Graphics.Log.Print($"Creating 2D texture unordered access view for {_texture.Name}.", LoggingLevel.Verbose);

            try
            {
                Graphics.Log.Print("Unordered Access 2D view: Creating D3D 11 unordered access resource view.", LoggingLevel.Verbose);
                Graphics.Log.Print($"Unordered Access 2D View '{Texture.Name}': {Texture.ResourceType} -> Mip slice: {MipSlice}, Array Index: {ArrayIndex}, Array Count: {ArrayCount}",
                           LoggingLevel.Verbose);

                // Create our SRV.
                NativeView = new D3D11.UnorderedAccessView1(Resource.Graphics.D3DDevice, Resource.D3DResource, desc)
                             {
                                 DebugName = $"'{Texture.Name}'_D3D11UnorderedAccessView1_2D"
                             };

                this.RegisterDisposable(_texture.Graphics);
            }
            catch (DX.SharpDXException sDXEx)
            {
                if ((uint)sDXEx.ResultCode.Code == 0x80070057)
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                              string.Format(Resources.GORGFX_ERR_VIEW_CANNOT_CAST_FORMAT,
                                                            _texture.Format,
                                                            Format));
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            _texture = null;
            base.Dispose();
        }

        /// <summary>
        /// Function to return the width of the render target at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <param name="mipLevel">The mip level to evaluate.</param>
        /// <returns>The width of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the render target.</returns>
        public int GetMipWidth(int mipLevel)
        {
            mipLevel = mipLevel.Min(Texture.MipLevels).Max(MipSlice);
            return Width << mipLevel;
        }

        /// <summary>
        /// Function to return the height of the render target at the current <see cref="MipSlice"/> in pixels.
        /// </summary>
        /// <param name="mipLevel">The mip level to evaluate.</param>
        /// <returns>The height of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the render target.</returns>
        public int GetMipHeight(int mipLevel)
        {
            mipLevel = mipLevel.Min(Texture.MipLevels).Max(MipSlice);

            return Height << mipLevel;
        }

        /// <summary>
        /// Function to clear the contents of the texture for this view.
        /// </summary>
        /// <param name="color">Color to use when clearing the texture unordered access view.</param>
        /// <param name="rectangles">[Optional] Specifies which regions on the view to clear.</param>
        /// <remarks>
        /// <para>
        /// This will clear the texture unordered access view to the specified <paramref name="color"/>.  If a specific region should be cleared, one or more <paramref name="rectangles"/> should be passed 
        /// to the method.
        /// </para>
        /// <para>
        /// If the <paramref name="rectangles"/> parameter is <b>null</b>, or has a zero length, the entirety of the view is cleared.
        /// </para>
        /// <para>
        /// If this method is called with a 3D texture bound to the view, and with regions specified, then the regions are ignored.
        /// </para>
        /// </remarks>
        public void Clear(GorgonColor color, DX.Rectangle[] rectangles)
        {
            if ((rectangles == null) || (rectangles.Length == 0))
            {
                Clear(color.Red, color.Green, color.Blue, color.Alpha);
                return;
            }

            if ((_clearRects == null) || (_clearRects.Length < rectangles.Length))
            {
                _clearRects = new RawRectangle[rectangles.Length];
            }

            for (int i = 0; i < rectangles.Length; ++i)
            {
                _clearRects[i] = rectangles[i];
            }

            Resource.Graphics.D3DDeviceContext.ClearView(NativeView, color.ToRawColor4(), _clearRects, rectangles.Length);
        }

        /// <summary>
        /// Function to create a new texture that is bindable to the GPU as an unordered access resource.
        /// </summary>
        /// <param name="graphics">The graphics interface to use when creating the target.</param>
        /// <param name="info">The information about the texture.</param>
        /// <returns>A new <see cref="GorgonTexture2DUav"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or <paramref name="info"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This is a convenience method that will create a <see cref="GorgonTexture2D"/> and a <see cref="GorgonTexture2DUav"/> as a single object that users can use to apply a texture as an unordered  
        /// access resource. This helps simplify creation of a texture by executing some prerequisite steps on behalf of the user.
        /// </para>
        /// <para>
        /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonTexture2DUav"/> returned, disposal of either one will dispose of the other on your behalf. If 
        /// the user created a <see cref="GorgonTexture2DUav"/> from the <see cref="GorgonTexture2D.GetUnorderedAccessView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user knows 
        /// what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTexture2D"/>
        public static GorgonTexture2DUav CreateTexture(GorgonGraphics graphics, IGorgonTexture2DInfo info)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            var newInfo = new GorgonTexture2DInfo(info)
                          {
                              Usage = info.Usage == ResourceUsage.Staging ? ResourceUsage.Default : info.Usage,
                              Binding = (((info.Binding & TextureBinding.UnorderedAccess) != TextureBinding.UnorderedAccess)
                                             ? (info.Binding | TextureBinding.UnorderedAccess)
                                             : info.Binding) & ~TextureBinding.DepthStencil // There's now way we can build a depth/stencil from this method.
                          };

            var texture = new GorgonTexture2D(graphics, newInfo);
            GorgonTexture2DUav result = texture.GetUnorderedAccessView();
            result.OwnsResource = true;

            return result;
        }

        /// <summary>
        /// Function to load a texture from a <see cref="Stream"/>.
        /// </summary>
        /// <param name="graphics">The graphics interface that will own the texture.</param>
        /// <param name="stream">The stream containing the texture image data.</param>
        /// <param name="codec">The codec that is used to decode the the data in the stream.</param>
        /// <param name="size">[Optional] The size of the image in the stream, in bytes.</param>
        /// <param name="options">[Optional] Options used to further define the texture.</param>
        /// <returns>A new <see cref="GorgonTexture2DUav"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="stream"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="IOException">Thrown if the <paramref name="stream"/> is write only.</exception>
        /// <exception cref="EndOfStreamException">Thrown if reading the image would move beyond the end of the <paramref name="stream"/>.</exception>
        /// <remarks>
        /// <para>
        /// This will load an <see cref="IGorgonImage"/> from a <paramref name="stream"/> and put it into a <see cref="GorgonTexture2D"/> object and return a <see cref="GorgonTexture2DUav"/>.
        /// </para>
        /// <para>
        /// If the <paramref name="size"/> option is specified, then the method will read from the stream up to that number of bytes, so it is up to the user to provide an accurate size. If it is omitted 
        /// then the <c>stream length - stream position</c> is used as the total size.
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
        /// <para>
        /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonTexture2DUav"/> returned, disposal of either one will dispose of the other on your behalf. If 
        /// the user created a <see cref="GorgonTexture2DUav"/> from the <see cref="GorgonTexture2D.GetShaderResourceView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user knows 
        /// what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// </remarks>
        public static GorgonTexture2DUav FromStream(GorgonGraphics graphics, Stream stream, IGorgonImageCodec codec, long? size = null, GorgonTextureLoadOptions options = null)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (codec == null)
            {
                throw new ArgumentNullException(nameof(codec));
            }

            if (!stream.CanRead)
            {
                throw new IOException(Resources.GORGFX_ERR_STREAM_WRITE_ONLY);
            }

            if (size == null)
            {
                size = stream.Length - stream.Position;
            }

            if ((stream.Length - stream.Position) < size)
            {
                throw new EndOfStreamException();
            }

            using (IGorgonImage image = codec.LoadFromStream(stream, size))
            {
                GorgonTexture2D texture = image.ToTexture2D(graphics, options);
                GorgonTexture2DUav view =  texture.GetUnorderedAccessView();
                view.OwnsResource = true;
                return view;
            }
        }

        /// <summary>
        /// Function to load a texture from a file.
        /// </summary>
        /// <param name="graphics">The graphics interface that will own the texture.</param>
        /// <param name="filePath">The path to the file.</param>
        /// <param name="codec">The codec that is used to decode the the data in the stream.</param>
        /// <param name="options">[Optional] Options used to further define the texture.</param>
        /// <returns>A new <see cref="GorgonTexture2DUav"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="filePath"/>, or the <paramref name="codec"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This will load an <see cref="IGorgonImage"/> from a file on disk and put it into a <see cref="GorgonTexture2D"/> object and return a <see cref="GorgonTexture2DUav"/>.
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
        /// <para>
        /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonTexture2DUav"/> returned, disposal of either one will dispose of the other on your behalf. If 
        /// the user created a <see cref="GorgonTexture2DUav"/> from the <see cref="GorgonTexture2D.GetShaderResourceView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user knows 
        /// what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// </remarks>
        public static GorgonTexture2DUav FromFile(GorgonGraphics graphics, string filePath, IGorgonImageCodec codec, GorgonTextureLoadOptions options = null)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentEmptyException(nameof(filePath));
            }

            if (codec == null)
            {
                throw new ArgumentNullException(nameof(codec));
            }

            using (IGorgonImage image = codec.LoadFromFile(filePath))
            {
                GorgonTexture2D texture = image.ToTexture2D(graphics, options);
                GorgonTexture2DUav view = texture.GetUnorderedAccessView();
                view.OwnsResource = true;
                return view;
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonTexture2DUav"/> class.
        /// </summary>
        /// <param name="texture">The texture to view.</param>
        /// <param name="format">The format for the view.</param>
        /// <param name="formatInfo">Information about the format.</param>
        /// <param name="firstMipLevel">The first mip level to view.</param>
        /// <param name="arrayIndex">The first array index to view.</param>
        /// <param name="arrayCount">The number of array indices to view.</param>
        internal GorgonTexture2DUav(GorgonTexture2D texture,
                                  BufferFormat format,
                                  GorgonFormatInfo formatInfo,
                                  int firstMipLevel,
                                  int arrayIndex,
                                  int arrayCount)
            : base(texture)
        {
            _texture = texture;
            Format = format;

            FormatInformation = formatInfo;

            if (FormatInformation.IsTypeless)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
            }

            Bounds = new DX.Rectangle(0, 0, Width, Height);
            MipSlice = firstMipLevel;
            ArrayIndex = arrayIndex;
            ArrayCount = arrayCount;
        }
        #endregion
    }
}
