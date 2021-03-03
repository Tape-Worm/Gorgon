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
// Created: Friday, July 19, 2013 10:13:11 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A view to allow 3D texture based render targets to be bound to the pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A render target view allows a render target (such as a <see cref="GorgonSwapChain"/> or a texture to be bound to the GPU pipeline as a render target resource.
    /// </para>
    /// <para>
    /// The view can bind the entire resource, or a sub section of the resource as required. It will also allow for casting of the format to allow for reinterpreting the data stored within the the render 
    /// target. 
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonSwapChain"/>
    /// <seealso cref="GorgonTexture2D"/>
    /// <seealso cref="GorgonTexture3D"/>
    public sealed class GorgonRenderTarget3DView
		: GorgonRenderTargetView, IGorgonTexture3DInfo, IGorgonImageInfo
	{
		#region Properties.
		/// <summary>
		/// Property to return the type of image data.
		/// </summary>
		ImageType IGorgonImageInfo.ImageType => ImageType.Image3D;

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
		int IGorgonImageInfo.MipCount => Texture?.Depth ?? 0;

		/// <summary>
		/// Property to return the total number of images there are in an image array.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This only applies to 1D and 2D images.  This parameter will be set to a value of 1 for a 3D image.
		/// </para>
		/// </remarks>
		int IGorgonImageInfo.ArrayCount => 1;

		/// <summary>
		/// Property to return whether the size of the texture is a power of 2 or not.
		/// </summary>
		bool IGorgonImageInfo.IsPowerOfTwo => ((Width == 0) || (Width & (Width - 1)) == 0)
											  && ((Height == 0) || (Height & (Height - 1)) == 0)
											  && ((Depth == 0) || (Depth & (Depth - 1)) == 0);

		/// <summary>
		/// Property to return the texture bound to this render target view.
		/// </summary>
		public GorgonTexture3D Texture
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the mip slice to use for the view.
		/// </summary>
		public int MipSlice
		{
			get;
		}

		/// <summary>
		/// Property to return the number of depth slices in the view.
		/// </summary>
		public int DepthSliceCount
		{
			get;
		}

		/// <summary>
		/// Property to return the first depth slice in the view.
		/// </summary>
		public int StartDepthSlice
		{
			get;
		}

		/// <summary>
		/// Property to return the width of the render target in pixels.
		/// </summary>
		/// <remarks>
		/// This value is the full width of the first mip map level for the texture associated with the render target.
		/// </remarks>
		public override int Width => Texture?.Width ?? 0;

		/// <summary>
		/// Property to return the height of the render target in pixels.
		/// </summary>
		/// <remarks>
		/// This value is the full width of the first mip map level for the texture associated with the render target.
		/// </remarks>
		public override int Height => Texture?.Height ?? 0;

		/// <summary>
		/// Property to return the depth of the texture, in slices.
		/// </summary>
		public int Depth => Texture?.Depth ?? 0;

		/// <summary>
		/// Property to return the width of the render target at the current <see cref="MipSlice"/> in pixels.
		/// </summary>
		/// <remarks>
		/// This value is the width of the mip map level assigned to <see cref="MipSlice"/> for the texture associated with the render target.
		/// </remarks>
		public int MipWidth
		{
			get;
			private set;
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
			private set;
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
			private set;
		}

		/// <summary>
		/// Property to return the name of the texture.
		/// </summary>
		string IGorgonNamedObject.Name => Texture?.Name ?? string.Empty;

		/// <summary>
		/// Property to return the number of mip-map levels for the texture.
		/// </summary>
		int IGorgonTexture3DInfo.MipLevels => Texture?.MipLevels ?? 0;

		/// <summary>
		/// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
		/// </summary>
		public override TextureBinding Binding => Texture?.Binding ?? TextureBinding.None;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform the creation of a specific kind of view.
		/// </summary>
		/// <returns>The view that was created.</returns>
		private protected override D3D11.ResourceView OnCreateNativeView()
		{
			Graphics.Log.Print($"Render Target 3D View '{Texture.Name}': Creating D3D11 render target view.", LoggingLevel.Simple);

			var desc = new D3D11.RenderTargetViewDescription1
			{
				Format = (DXGI.Format)Format,
				Dimension = D3D11.RenderTargetViewDimension.Texture3D,
				Texture3D =
														  {
															  DepthSliceCount = DepthSliceCount,
															  FirstDepthSlice = StartDepthSlice,
															  MipSlice = MipSlice
														  }
			};

			MipWidth = (Width >> MipSlice).Max(1);
			MipHeight = (Height >> MipSlice).Max(1);

			Bounds = new DX.Rectangle(0, 0, Width, Height);

			Graphics.Log.Print($"Render Target 3D View '{Texture.Name}': {Texture.ResourceType} -> Mip slice: {MipSlice}, Starting depth slide: {StartDepthSlice}, Depth slice count: {DepthSliceCount}",
							   LoggingLevel.Verbose);

			Native = new D3D11.RenderTargetView1(Texture.Graphics.D3DDevice, Texture.D3DResource, desc)
			{
				DebugName = $"'{Texture.Name}'_D3D11RenderTargetView1_3D"
			};

			return Native;
		}

		/// <summary>
		/// Function to convert a texel coordinate into a pixel coordinate and a depth slice.
		/// </summary>
		/// <param name="texelCoordinates">The texel coordinates to convert.</param>
		/// <returns>The pixel coordinates.</returns>
		public (DX.Point, int) ToPixel(DX.Vector3 texelCoordinates)
		{
			float width = Texture.Width;
			float height = Texture.Height;

			return (new DX.Point((int)(texelCoordinates.X * width), (int)(texelCoordinates.Y * height)), (int)(texelCoordinates.Z * Depth));
		}

		/// <summary>
		/// Function to convert a pixel coordinate into a texel coordinate.
		/// </summary>
		/// <param name="pixelCoordinates">The pixel coordinate to convert.</param>
		/// <returns>The texel coordinates.</returns>
		public DX.Vector3 ToTexel(DX.Point pixelCoordinates)
		{
			float width = Texture.Width;
			float height = Texture.Height;

			return new DX.Vector3(pixelCoordinates.X / width, pixelCoordinates.Y / height, Depth / (float)Depth);
		}

		/// <summary>
		/// Function to convert a texel size into a pixel size.
		/// </summary>
		/// <param name="texelCoordinates">The texel size to convert.</param>
		/// <returns>The pixel size.</returns>
		public DX.Size2 ToPixel(DX.Size2F texelCoordinates)
		{
			float width = Texture.Width;
			float height = Texture.Height;

			return new DX.Size2((int)(texelCoordinates.Width * width), (int)(texelCoordinates.Height * height));
		}

		/// <summary>
		/// Function to convert a pixel size into a texel size.
		/// </summary>
		/// <param name="pixelCoordinates">The pixel size to convert.</param>
		/// <returns>The texel size.</returns>
		public DX.Size2F ToTexel(DX.Size2 pixelCoordinates)
		{
			float width = Texture.Width;
			float height = Texture.Height;

			return new DX.Size2F(pixelCoordinates.Width / width, pixelCoordinates.Height / height);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			Texture = null;
			base.Dispose();
		}

		/// <summary>
		/// Function to create a new render target that is bindable to the GPU.
		/// </summary>
		/// <param name="graphics">The graphics interface to use when creating the target.</param>
		/// <param name="info">The information about the texture.</param>
		/// <returns>A new <see cref="GorgonRenderTarget3DView"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or <paramref name="info"/> parameter is <b>null</b>.</exception>
		/// <remarks>
		/// <para>
		/// This is a convenience method that will create a <see cref="GorgonTexture3D"/> and a <see cref="GorgonRenderTarget3DView"/> as a single object that users can use to apply a render target texture. 
		/// This helps simplify creation of a render target by executing some prerequisite steps on behalf of the user.
		/// </para>
		/// <para>
		/// Since the <see cref="GorgonTexture3D"/> created by this method is linked to the <see cref="GorgonRenderTarget3DView"/> returned, disposal of either one will dispose of the other on your behalf. 
		/// If the user created a <see cref="GorgonRenderTarget3DView"/> from the <see cref="GorgonTexture3D.GetRenderTargetView"/> method on the <see cref="GorgonTexture3D"/>, then it's assumed the user 
		/// knows what they are doing and will handle the disposal of the texture and view on their own.
		/// </para>
		/// </remarks>
		/// <seealso cref="GorgonTexture3D"/>
		public static GorgonRenderTarget3DView CreateRenderTarget(GorgonGraphics graphics, IGorgonTexture3DInfo info)
		{
			if (graphics is null)
			{
				throw new ArgumentNullException(nameof(graphics));
			}

			if (info is null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			TextureBinding binding = TextureBinding.RenderTarget;

			if ((info.Binding & TextureBinding.ShaderResource) == TextureBinding.ShaderResource)
			{
				binding |= TextureBinding.ShaderResource;
			}

			if ((info.Binding & TextureBinding.ShaderResource) == TextureBinding.ReadWriteView)
			{
				binding |= TextureBinding.ReadWriteView;
			}

			var newInfo = new GorgonTexture3DInfo(info)
			{
				// Can't see a reason to use anything other than default for rtvs
				Usage = ResourceUsage.Default,
				Binding = binding
			};

			var texture = new GorgonTexture3D(graphics, newInfo);
			GorgonRenderTarget3DView result = texture.GetRenderTargetView();
			result.OwnsResource = true;

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderTarget3DView" /> class.
		/// </summary>
		/// <param name="texture">The render target texture to bind.</param>
		/// <param name="format">The format of the render target view.</param>
		/// <param name="formatInfo">The format information.</param>
		/// <param name="mipSlice">The mip slice to use in the view.</param>
		/// <param name="startDepthSlice">The first depth slice to use in the view.</param>
		/// <param name="depthSliceCount">The number of depth slices to use in the view.</param>
		internal GorgonRenderTarget3DView(GorgonTexture3D texture, BufferFormat format, GorgonFormatInfo formatInfo, int mipSlice, int startDepthSlice, int depthSliceCount)
			: base(texture, format, formatInfo)
		{
			Texture = texture;
			MipSlice = mipSlice;
			StartDepthSlice = startDepthSlice;
			DepthSliceCount = depthSliceCount;
			MipWidth = (Width >> MipSlice).Max(1);
			MipHeight = (Height >> MipSlice).Max(1);
			Bounds = new DX.Rectangle(0, 0, Width, Height);
		}
		#endregion
	}
}