#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Saturday, June 8, 2013 4:53:00 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using System.Numerics;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;
using Gorgon.Memory;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines options for using a depth/stencil view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Limiting a depth-stencil buffer to read-only access allows more than one depth-stencil view to be bound to the pipeline.
    /// </para>
    /// </remarks>
    [Flags]
	public enum DepthStencilViewFlags
	{
		/// <summary>
		/// Depth and stencil buffers are read-write.
		/// </summary>
		None = D3D11.DepthStencilViewFlags.None,
		/// <summary>
		/// <para>
		/// Indicates that depth values are read only.
		/// </para>
		/// </summary>
		ReadOnlyDepth = D3D11.DepthStencilViewFlags.ReadOnlyDepth,
		/// <summary>
		/// <para>
		/// Indicates that stencil values are read only.
		/// </para>
		/// </summary>
		ReadOnlyStencil = D3D11.DepthStencilViewFlags.ReadOnlyStencil
	}

	/// <summary>
	/// A depth/stencil view for textures.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is a depth/stencil view to allow a <see cref="GorgonTexture2D"/> to be bound to the GPU pipeline as a depth/stencil resource.
	/// </para>
	/// <para>
	/// Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow the resource to be cast to any 
	/// format within the same group.	
	/// </para>
	/// </remarks>
	/// <seealso cref="GorgonTexture2D"/>
	public sealed class GorgonDepthStencil2DView
		: GorgonResourceView, IGorgonTexture2DInfo, IGorgonImageInfo
	{
		#region Properties.
		/// <summary>
		/// Property to return the native D3D depth/stencil view.
		/// </summary>
		internal D3D11.DepthStencilView Native
		{
			get;
			private set;
		}

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
		int IGorgonImageInfo.MipCount => Texture?.MipLevels ?? 0;

		/// <summary>
		/// Property to return the total number of images there are in an image array.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This only applies to 1D and 2D images.  This parameter will be set to a value of 1 for a 3D image.
		/// </para>
		/// </remarks>
		int IGorgonImageInfo.ArrayCount => Texture?.ArrayCount ?? 0;

		/// <summary>
		/// Property to return whether the size of the texture is a power of 2 or not.
		/// </summary>
		bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0)
											  && ((Height == 0) || (Height & (Height - 1)) == 0);

		/// <summary>
		/// Property to return the pixel format for an image.
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the value is set to <see cref="BufferFormat.Unknown"/>, then an exception will be thrown upon image creation.
		/// </para>
		/// </remarks>
		BufferFormat IGorgonImageInfo.Format => Texture?.Format ?? BufferFormat.Unknown;

		/// <summary>
		/// Property to return the texture bound to this view.
		/// </summary>
		public GorgonTexture2D Texture
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the format for this view.
		/// </summary>
		public BufferFormat Format
		{
			get;
		}

		/// <summary>
		/// Property to return the flags for this view.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will allow the depth/stencil buffer to be read simultaneously from the depth/stencil view and from a shader view.  It is not normally possible to bind a view of a resource to 2 parts of the 
		/// pipeline at the same time.  However, with the flags provided, read-only access may be granted to a part of the resource (depth or stencil) or all of it for all parts of the pipline.  This would bind 
		/// the depth/stencil as a read-only view and make it a read-only view accessible to shaders.
		/// </para>
		/// <para>
		/// This is only valid if the resource allows shader access.
		/// </para>
		/// </remarks>
		public DepthStencilViewFlags Flags
		{
			get;
		}

		/// <summary>
		/// Property to return the mip slice to use for the view.
		/// </summary>
		public int MipSlice
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
		/// Property to return the first array index to use in the view.
		/// </summary>
		public int ArrayIndex
		{
			get;
		}
		/// <summary>
		/// Property to return the format information for the <see cref="Format"/> of this view.
		/// </summary>
		public GorgonFormatInfo FormatInformation
		{
			get;
		}

		/// <summary>
		/// Property to return the width of the render target in pixels.
		/// </summary>
		/// <remarks>
		/// This value is the full width of the first mip map level for the texture associated with the render target.
		/// </remarks>
		public int Width => Texture?.Width ?? 0;

		/// <summary>
		/// Property to return the height of the render target in pixels.
		/// </summary>
		/// <remarks>
		/// This value is the full width of the first mip map level for the texture associated with the render target.
		/// </remarks>
		public int Height => Texture?.Height ?? 0;

		/// <summary>
		/// Property to return the width of the render target at the current <see cref="MipSlice"/> in pixels.
		/// </summary>
		/// <remarks>
		/// This value is the width of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the render target.
		/// </remarks>
		public int MipWidth
		{
			get;
		}

		/// <summary>
		/// Property to return the height of the render target at the current <see cref="MipSlice"/> in pixels.
		/// </summary>
		/// <remarks>
		/// This value is the height of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the render target.
		/// </remarks>
		public int MipHeight
		{
			get;
		}

		/// <summary>
		/// Property to return the bounding rectangle for the render target view.
		/// </summary>
		/// <remarks>
		/// This value is the full bounding rectangle of the first mip map level for the texture associated with the render target.
		/// </remarks>
		public DX.Rectangle Bounds
		{
			get;
		}

		/// <summary>
		/// Property to return the name of the texture.
		/// </summary>
		string IGorgonNamedObject.Name => Texture?.Name ?? string.Empty;

		/// <summary>
		/// Property to return whether the 2D texture is a cube map.
		/// </summary>
		public bool IsCubeMap => Texture?.IsCubeMap ?? false;

		/// <summary>
		/// Property to return the number of mip-map levels for the texture.
		/// </summary>
		int IGorgonTexture2DInfo.MipLevels => Texture?.MipLevels ?? 0;

		/// <summary>
		/// Property to return the multisample quality and count for the texture.
		/// </summary>
		public GorgonMultisampleInfo MultisampleInfo => Texture?.MultisampleInfo ?? GorgonMultisampleInfo.NoMultiSampling;

		/// <summary>
		/// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
		/// </summary>
		public TextureBinding Binding => Texture?.Binding ?? TextureBinding.None;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the description for a 2D depth/stencil view.
        /// </summary>
        /// <returns>The direct 3D 2D depth/stencil view description.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
        private D3D11.DepthStencilViewDescription GetDesc2D()
		{
			bool isMultisampled = Texture.MultisampleInfo != GorgonMultisampleInfo.NoMultiSampling;

            // Set up for arrayed and multisampled texture.
            if (Texture.ArrayCount > 1)
			{
				return new D3D11.DepthStencilViewDescription
				{
					Format = (Format)Format,
					Dimension = isMultisampled
									? D3D11.DepthStencilViewDimension.Texture2DMultisampledArray
									: D3D11.DepthStencilViewDimension.Texture2DArray,
					Texture2DArray =
					{
						MipSlice = isMultisampled ? ArrayIndex : MipSlice,
						FirstArraySlice = isMultisampled ? ArrayCount : ArrayIndex,
						ArraySize = isMultisampled ? 0 : ArrayCount
					}
				};
			}

            return new D3D11.DepthStencilViewDescription
			{
				Format = (Format)Format,
				Dimension = isMultisampled
								? D3D11.DepthStencilViewDimension.Texture2DMultisampled
								: D3D11.DepthStencilViewDimension.Texture2D,
				Texture2D =
				{
					MipSlice = isMultisampled ? 0 : MipSlice
				}
			};
		}

		/// <summary>
		/// Function to perform initialization of the view.
		/// </summary>
		private protected override D3D11.ResourceView OnCreateNativeView()
		{
			D3D11.DepthStencilViewDescription desc = GetDesc2D();

			Graphics.Log.Print($"'{Texture.Name}': Creating D3D11 depth/stencil view.", LoggingLevel.Simple);

			Graphics.Log.Print($"Depth/Stencil View '{Texture.Name}': {Texture.ResourceType} -> Mip slice: {MipSlice}, Array Index: {ArrayIndex}, Array Count: {ArrayCount}",
					   LoggingLevel.Verbose);

			Native = new D3D11.DepthStencilView(Texture.Graphics.D3DDevice, Texture.D3DResource, desc)
			{
				DebugName = $"'{Texture.Name}'_D3D11DepthStencilView1_2D"
			};

			return Native;
		}


		/// <summary>
		/// Function to create a new <see cref="GorgonDepthStencil2DView"/> for this texture.
		/// </summary>
		/// <param name="format">[Optional] The format for the view.</param>
		/// <param name="flags">[Optional] Flags to define how this view should be accessed by the shader.</param>
		/// <returns>A <see cref="GorgonDepthStencil2DView"/> used to bind the texture as a depth/stencil buffer.</returns>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> is not supported as a depth/stencil format.</exception>
		/// <exception cref="GorgonException">Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.DepthStencil"/>.
		/// <para>-or-</para>
		/// <para>Thrown when this texture has a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/>.</para>
		/// <para>-or-</para>
		/// <para>Thrown if this texture has a <see cref="Binding"/> of <see cref="TextureBinding.ShaderResource"/>, but the texture format is not a typeless format.</para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// The depth/stencil views take a <see cref="DepthStencilViewFlags"/> parameter that determine how a shader can access the depth buffer when it is bound to the pipeline for reading. By specifying 
		/// a single flag, the depth can write to the opposite plane (e.g. read only depth and write only stencil, write only depth and read only stencil) of the texture. This allows for multiple 
		/// depth/stencil views to be bound to the pipeline for reading and writing.
		/// </para>
		/// <para>
		/// If the <see cref="Binding"/> for the texture includes <see cref="TextureBinding.ShaderResource"/>, then the <paramref name="format"/> for the view and the <see cref="Format"/> for the texture
		/// must be specific values.  These values are listed below:
		/// <list type="table">
		///     <listheader><term>Depth Format</term><term>Texture Format</term></listheader>
		///     <item><term><see cref="BufferFormat.D32_Float_S8X24_UInt"/></term><term><see cref="BufferFormat.R32G8X24_Typeless"/></term></item>
		///     <item><term><see cref="BufferFormat.D24_UNorm_S8_UInt"/></term><term><see cref="BufferFormat.R24G8_Typeless"/></term></item>
		///     <item><term><see cref="BufferFormat.D32_Float"/></term><term><see cref="BufferFormat.R32_Typeless"/></term></item>
		///     <item><term><see cref="BufferFormat.D16_UNorm"/></term><term><see cref="BufferFormat.R16_Typeless"/></term></item>
		/// </list>
		/// </para>
		/// </remarks>
		public GorgonDepthStencil2DView GetDepthStencilView(BufferFormat format = BufferFormat.Unknown, DepthStencilViewFlags flags = DepthStencilViewFlags.None) => Texture.GetDepthStencilView(format, MipSlice, ArrayIndex, ArrayCount, flags);

		/// <summary>
		/// Function to create a new <see cref="GorgonTexture2DView"/> for this texture.
		/// </summary>
		/// <param name="format">[Optional] The format for the view.</param>
		/// <returns>A <see cref="GorgonTexture2DView"/> used to bind the texture to a shader.</returns>
		/// <exception cref="ArgumentException">Thrown if the <paramref name="format"/> is a typeless format.</exception>
		/// <exception cref="GorgonException">
		/// Thrown when this texture does not have a <see cref="TextureBinding"/> of <see cref="TextureBinding.ShaderResource"/>.
		/// <para>-or-</para>
		/// <para>Thrown when this texture has a usage of <see cref="ResourceUsage.Staging"/>.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the texture <see cref="Format"/> is not typeless, and the <see cref="Binding"/> is set to <see cref="TextureBinding.DepthStencil"/>.</para>
		/// <para></para>
		/// </exception>
		/// <remarks>
		/// <para>
		/// This will create a view that makes a texture accessible to shaders. This allows viewing of the texture data in a different format, or even a subsection of the texture from within the shader.
		/// </para>
		/// <para>
		/// The <paramref name="format"/> parameter is used present the texture data as another format type to the shader. If this value is left at the default of <see cref="BufferFormat.Unknown"/>, then 
		/// the format from the this texture is used. The <paramref name="format"/> must be castable to the format of this texture. If it is not, an exception will be thrown.
		/// </para>
		/// </remarks>
		public GorgonTexture2DView GetShaderResourceView(BufferFormat format = BufferFormat.Unknown) => Texture.GetShaderResourceView(format, MipSlice, 1, ArrayIndex, ArrayCount);

		/// <summary>
		/// Function to convert a rectangle of texel coordinates to pixel space.
		/// </summary>
		/// <param name="texelCoordinates">The texel coordinates to convert.</param>
		/// <returns>A rectangle containing the pixel space coordinates.</returns>
		public DX.Rectangle ToPixel(DX.RectangleF texelCoordinates)
		{
			float width = Texture.Width;
			float height = Texture.Height;

			return new DX.Rectangle((int)(texelCoordinates.X * width),
									 (int)(texelCoordinates.Y * height),
									 (int)(texelCoordinates.Width * width),
									 (int)(texelCoordinates.Height * height));
		}

		/// <summary>
		/// Function to convert a rectangle of pixel coordinates to texel space.
		/// </summary>
		/// <param name="pixelCoordinates">The pixel coordinates to convert.</param>
		/// <returns>A rectangle containing the texel space coordinates.</returns>
		public DX.RectangleF ToTexel(DX.Rectangle pixelCoordinates)
		{
			float width = Texture.Width;
			float height = Texture.Height;

			return new DX.RectangleF(pixelCoordinates.X / width, pixelCoordinates.Y / height, pixelCoordinates.Width / width, pixelCoordinates.Height / height);
		}

		/// <summary>
		/// Function to convert a size value from pixel coordinates to texel space.
		/// </summary>
		/// <param name="pixelSize">The pixel size to convert.</param>
		/// <returns>A size value containing the texel space coordinates.</returns>
		public DX.Size2F ToTexel(DX.Size2 pixelSize)
		{
			float width = Texture.Width;
			float height = Texture.Height;

			return new DX.Size2F(pixelSize.Width / width, pixelSize.Height / height);
		}

		/// <summary>
		/// Function to convert a size value from texel coordinates to pixel space.
		/// </summary>
		/// <param name="texelSize">The texel size to convert.</param>
		/// <returns>A size value containing the texel space coordinates.</returns>
		public DX.Size2 ToPixel(DX.Size2F texelSize)
		{
			float width = Texture.Width;
			float height = Texture.Height;

			return new DX.Size2((int)(texelSize.Width * width), (int)(texelSize.Height * height));
		}

		/// <summary>
		/// Function to convert a 2D vector value from pixel coordinates to texel space.
		/// </summary>
		/// <param name="pixelVector">The pixel size to convert.</param>
		/// <returns>A 2D vector containing the texel space coordinates.</returns>
		public Vector2 ToTexel(Vector2 pixelVector)
		{
			float width = Texture.Width;
			float height = Texture.Height;

			return new Vector2(pixelVector.X / width, pixelVector.Y / height);
		}

		/// <summary>
		/// Function to convert a 2D vector value from texel coordinates to pixel space.
		/// </summary>
		/// <param name="texelVector">The texel size to convert.</param>
		/// <returns>A 2D vector containing the pixel space coordinates.</returns>
		public Vector2 ToPixel(Vector2 texelVector)
		{
			float width = Texture.Width;
			float height = Texture.Height;

			return new Vector2(texelVector.X * width, texelVector.Y * height);
		}

		/// <summary>
		/// Function to clear the depth and stencil portion of the buffer for this view.
		/// </summary>
		/// <param name="depthValue">The depth value to write to the depth portion of the buffer.</param>
		/// <param name="stencilValue">The stencil value to write to the stencil portion of the buffer.</param>
		/// <remarks>
		/// <para>
		/// If the view <see cref="Format"/> does not have a stencil component, then the <paramref name="stencilValue"/> will be ignored. Likewise, if the <see cref="Format"/> lacks a depth component, 
		/// then the <paramref name="depthValue"/> will be ignored.
		/// </para>
		/// </remarks>
		public void Clear(float depthValue, byte stencilValue)
		{
			D3D11.DepthStencilClearFlags clearFlags = 0;

			if (FormatInformation.HasDepth)
			{
				clearFlags = D3D11.DepthStencilClearFlags.Depth;
			}

			if (FormatInformation.HasStencil)
			{
				clearFlags |= D3D11.DepthStencilClearFlags.Stencil;
			}

			Texture.Graphics.D3DDeviceContext.ClearDepthStencilView(Native, clearFlags, depthValue, stencilValue);

			ref GorgonGraphicsStatistics stats = ref Graphics.RwStatistics;
			unchecked
			{
				++stats._clearCount;
				++stats._stencilClearCount;
			}
		}

		/// <summary>
		/// Function to clear the depth portion of the buffer for this view.
		/// </summary> 
		/// <param name="depthValue">The depth value to write to the buffer.</param>
		/// <remarks>
		/// <para>
		/// If the view <see cref="Format"/> does not have a depth component, then this method will do nothing.
		/// </para>
		/// </remarks>
		public void ClearDepth(float depthValue)
		{
			if (!FormatInformation.HasDepth)
			{
				return;
			}

			Texture.Graphics.D3DDeviceContext.ClearDepthStencilView(Native, D3D11.DepthStencilClearFlags.Depth, depthValue, 0);

			ref GorgonGraphicsStatistics stats = ref Graphics.RwStatistics;
			unchecked
			{
				++stats._clearCount;
			}
		}

		/// <summary>
		/// Function to clear the stencil portion of the buffer for this view.
		/// </summary>
		/// <param name="stencilValue">The stencil value to write to the buffer.</param>
		/// <remarks>
		/// <para>
		/// If the view <see cref="Format"/> does not have a stencil component, then this method will do nothing.
		/// </para>
		/// </remarks>
		public void ClearStencil(byte stencilValue)
		{
			if (!FormatInformation.HasStencil)
			{
				return;
			}

			Texture.Graphics.D3DDeviceContext.ClearDepthStencilView(Native, D3D11.DepthStencilClearFlags.Stencil, 1.0f, stencilValue);

			ref GorgonGraphicsStatistics stats = ref Graphics.RwStatistics;
			unchecked
			{
				++stats._stencilClearCount;
			}
		}

		/// <summary>
		/// Function to clear the contents of the depth buffer for this view.
		/// </summary>
		/// <param name="depthValue">Depth value to use when clearing the depth view.</param>
		/// <param name="rectangles">[Optional] Specifies which regions on the view to clear.</param>
		/// <remarks>
		/// <para>
		/// This will clear the depth view to the specified <paramref name="depthValue"/>.  If a specific region should be cleared, one or more <paramref name="rectangles"/> should be passed to the 
		/// method.
		/// </para>
		/// <para>
		/// If the <paramref name="rectangles"/> parameter is <b>null</b>, or has a zero length, the entirety of the view is cleared.
		/// </para>
		/// <para>
		/// If this method is called with a 3D texture bound to the view, or the view references uses a <see cref="Buffer"/> with a stencil component, and with regions specified, then the regions are 
		/// ignored.
		/// </para>
		/// </remarks>
		public void Clear(float depthValue, ReadOnlySpan<DX.Rectangle> rectangles)
		{
			if ((rectangles.IsEmpty) || (FormatInformation.HasStencil))
			{
				Clear(depthValue, 0);
				return;
			}


			RawRectangle[] clearRects = GorgonArrayPool<RawRectangle>.SharedTiny.Rent(rectangles.Length);

			try
			{
				for (int i = 0; i < rectangles.Length; ++i)
				{
					clearRects[i] = rectangles[i];
				}

				Texture.Graphics.D3DDeviceContext.ClearView(Native, new DX.Color4(depthValue), clearRects, rectangles.Length);

				ref GorgonGraphicsStatistics stats = ref Graphics.RwStatistics;
				unchecked
				{
					++stats._clearCount;
				}
			}
			finally
			{
				GorgonArrayPool<RawRectangle>.SharedTiny.Return(clearRects);
			}
		}

		/// <summary>
		/// Function to create a new depth/stencil buffer that is bindable to the GPU.
		/// </summary>
		/// <param name="graphics">The graphics interface to use when creating the target.</param>
		/// <param name="info">The information about the depth/stencil texture.</param>
		/// <param name="viewFlags">[Optional] Flags used to determine if the depth buffer/stencil can be read by the GPU or not.</param>
		/// <returns>A new <see cref="GorgonDepthStencil2DView"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or <paramref name="info"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This is a convenience method that will create a <see cref="GorgonTexture2D"/> and a <see cref="GorgonDepthStencil2DView"/> as a single object that users can use to apply a depth/stencil texture. 
		/// This helps simplify creation of a render target by executing some prerequisite steps on behalf of the user.
		/// </para>
		/// <para>
		/// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonDepthStencil2DView"/> returned, disposal of either one will dispose of the other on your behalf. 
		/// If the user created a <see cref="GorgonDepthStencil2DView"/> from the <see cref="GorgonTexture2D.GetRenderTargetView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user 
		/// knows what they are doing and will handle the disposal of the texture and view on their own.
		/// </para>
		/// <para>
		/// To make the texture bindable on the GPU as a shader resource view, set the <see cref="IGorgonTexture2DInfo.Binding"/> to include the <see cref="TextureBinding.ShaderResource"/> flag in the value 
		/// and set the <paramref name="viewFlags"/> to <see cref="DepthStencilViewFlags.ReadOnlyDepth"/>, <see cref="DepthStencilViewFlags.ReadOnlyStencil"/> or both.
		/// </para> 
		/// </remarks>
		/// <seealso cref="GorgonTexture2D"/>
		public static GorgonDepthStencil2DView CreateDepthStencil(GorgonGraphics graphics, IGorgonTexture2DInfo info, DepthStencilViewFlags viewFlags = DepthStencilViewFlags.None)
		{
			if (graphics == null)
			{
				throw new ArgumentNullException(nameof(graphics));
			}

			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			TextureBinding binding = TextureBinding.DepthStencil;

			if ((info.Binding & TextureBinding.ShaderResource) == TextureBinding.ShaderResource)
			{
				if (viewFlags != DepthStencilViewFlags.None)
				{
					binding |= TextureBinding.ShaderResource;
				}
				else
				{
					// Do this to notify the user that something is amiss.
					graphics.Log.Print($"WARNING: Depth Stencil View {info.Name} - Depth/stencil texture has a binding of {TextureBinding.ShaderResource}, but has a view flags of {viewFlags}.  The view will not be bindable to the shader pipeline.",
									   LoggingLevel.Simple);

				}
			}
			else if (viewFlags != DepthStencilViewFlags.None)
			{
				// Do this to notify the user that something is amiss.
				graphics.Log.Print($"WARNING: Depth Stencil View {info.Name} - Depth/stencil view flag(s) are set to {viewFlags}, but the texture lacks a {TextureBinding.ShaderResource} binding.",
								   LoggingLevel.Simple);
			}

			var newInfo = new GorgonTexture2DInfo(info)
			{
				// Can't see a reason to use anything other than default for dsvs
				Usage = ResourceUsage.Default,
				Binding = binding
			};

			BufferFormat depthStencilFormat = newInfo.Format;

			if (((binding & TextureBinding.ShaderResource) == TextureBinding.ShaderResource)
				&& (viewFlags != DepthStencilViewFlags.None))
			{
				switch (newInfo.Format)
				{
					case BufferFormat.R32G8X24_Typeless:
						depthStencilFormat = BufferFormat.D32_Float_S8X24_UInt;
						break;
					case BufferFormat.R24G8_Typeless:
						depthStencilFormat = BufferFormat.D24_UNorm_S8_UInt;
						break;
					case BufferFormat.R16_Typeless:
						depthStencilFormat = BufferFormat.D16_UNorm;
						break;
					case BufferFormat.R32_Typeless:
						depthStencilFormat = BufferFormat.D32_Float;
						break;
				}
			}

			var texture = new GorgonTexture2D(graphics, newInfo);
			GorgonDepthStencil2DView result = texture.GetDepthStencilView(depthStencilFormat, flags: viewFlags);
			result.OwnsResource = true;

			return result;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			Texture = null;
			base.Dispose();
		}
		#endregion

		#region Constructor/Destructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencil2DView"/> class.
		/// </summary>
		/// <param name="texture">The resource to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		/// <param name="formatInfo">Information about the format.</param>
		/// <param name="mipSlice">The mip level to use for the view.</param>
		/// <param name="firstArrayIndex">The first array index to use for the view.</param>
		/// <param name="arrayCount">The number of array indices to use for the view.</param>
		/// <param name="flags">Depth/stencil view flags.</param>
		internal GorgonDepthStencil2DView(GorgonTexture2D texture,
									  BufferFormat format,
									  GorgonFormatInfo formatInfo,
									  int mipSlice,
									  int firstArrayIndex,
									  int arrayCount,
									  DepthStencilViewFlags flags)
			: base(texture)
		{
			Texture = texture ?? throw new ArgumentNullException(nameof(texture));
			Format = format;
			FormatInformation = formatInfo ?? throw new ArgumentNullException(nameof(formatInfo));

			MipSlice = texture.MipLevels <= 0 ? 0 : mipSlice.Max(0).Min(texture.MipLevels - 1);
			ArrayIndex = firstArrayIndex;
			ArrayCount = arrayCount;
			Flags = flags;
			MipWidth = (Width >> MipSlice).Max(1);
			MipHeight = (Height >> MipSlice).Max(1);

			Bounds = new DX.Rectangle(0, 0, Width, Height);
		}
		#endregion
	}
}
