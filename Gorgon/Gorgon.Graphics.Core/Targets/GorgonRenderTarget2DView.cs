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
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using SharpDX.Mathematics.Interop;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A view to allow 2D texture based render targets to be bound to the pipeline.
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
    public sealed class GorgonRenderTarget2DView
		: GorgonRenderTargetView, IGorgonTexture2DInfo
    {
		#region Variables.
        // Clear rectangles.
        private RawRectangle[] _clearRects;
        #endregion

		#region Properties.
        /// <summary>
        /// Property to return the texture bound to this render target view.
        /// </summary>
        public GorgonTexture2D Texture
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
        public override TextureBinding Binding => Texture?.Binding ?? TextureBinding.None;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the view description.
        /// </summary>
        /// <param name="isMultisampled"><b>true</b> if the texture is multisampled, <b>false</b> if not.</param>
        /// <returns>The view description.</returns>
        private D3D11.RenderTargetViewDescription1 GetDesc2D(bool isMultisampled)
        {
            // Set up for arrayed and multisampled texture.
            if (Texture.ArrayCount > 1)
            {
                return new D3D11.RenderTargetViewDescription1
                {
                    Format = (DXGI.Format)Format,
                    Dimension = isMultisampled
                        ? D3D11.RenderTargetViewDimension.Texture2DMultisampledArray
                        : D3D11.RenderTargetViewDimension.Texture2DArray,
                    Texture2DArray =
                    {
                        MipSlice = isMultisampled ? ArrayIndex : MipSlice,
                        FirstArraySlice = isMultisampled ? ArrayCount : ArrayIndex,
                        ArraySize = isMultisampled ? 0 : ArrayCount,
                        // TODO: I have no idea how to support this?  Probably won't need to for a while.
                        PlaneSlice = 0
                    }
                };
            }

            return new D3D11.RenderTargetViewDescription1
            {
                Format = (DXGI.Format)Format,
                Dimension = isMultisampled
                    ? D3D11.RenderTargetViewDimension.Texture2DMultisampled
                    : D3D11.RenderTargetViewDimension.Texture2D,
                Texture2D =
                {
                    MipSlice = isMultisampled ? 0 : MipSlice,
                    PlaneSlice = 0
                }
            };
        }

        /// <summary>
        /// Function to perform the creation of a specific kind of view.
        /// </summary>
        /// <returns>The view that was created.</returns>
        private protected override D3D11.ResourceView OnCreateNativeView()
        {
            Graphics.Log.Print($"Render Target 2D View '{Texture.Name}': Creating D3D11 render target view.", LoggingLevel.Simple);

            D3D11.RenderTargetViewDescription1 desc = GetDesc2D(!Texture.MultisampleInfo.Equals(GorgonMultisampleInfo.NoMultiSampling));

            if (desc.Dimension == D3D11.RenderTargetViewDimension.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_CANNOT_BIND_UNKNOWN_RESOURCE);
            }

            MipWidth = (Width >> MipSlice).Max(1);
            MipHeight = (Height >> MipSlice).Max(1);

            Bounds = new DX.Rectangle(0, 0, Width, Height);

            Graphics.Log.Print($"Render Target 2D View '{Texture.Name}': {Texture.ResourceType} -> Mip slice: {MipSlice}, Array Index: {ArrayIndex}, Array Count: {ArrayCount}",
                               LoggingLevel.Verbose);

            Native = new D3D11.RenderTargetView1(Texture.Graphics.D3DDevice, Texture.D3DResource, desc)
                     {
                         DebugName = $"'{Texture.Name}'_D3D11RenderTargetView1_2D"
                     };

            return Native;
        }

        /// <summary>
        /// Function to clear the contents of the render target for this view.
        /// </summary>
        /// <param name="color">Color to use when clearing the render target view.</param>
        /// <param name="rectangles">[Optional] Specifies which regions on the view to clear.</param>
        /// <remarks>
        /// <para>
        /// This will clear the render target view to the specified <paramref name="color"/>.  If a specific region should be cleared, one or more <paramref name="rectangles"/> should be passed to the 
        /// method.
        /// </para>
        /// <para>
        /// If the <paramref name="rectangles"/> parameter is <b>null</b>, or has a zero length, the entirety of the view is cleared.
        /// </para>
		/// </remarks>
        public void Clear(GorgonColor color, DX.Rectangle[] rectangles)
		{
		    if ((rectangles == null) || (rectangles.Length == 0))
		    {
                Clear(color);
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

		    Texture.Graphics.D3DDeviceContext.ClearView(Native, color.ToRawColor4(), _clearRects, rectangles.Length);
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
        /// <param name="arrayIndex">[Optional] The index of a texture array to slice the view at.</param>
        /// <param name="arrayCount">[Optioanl] The number of array indices to view.</param>
        /// <returns>A new <see cref="GorgonRenderTarget2DView"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or <paramref name="info"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// This is a convenience method that will create a <see cref="GorgonTexture2D"/> and a <see cref="GorgonRenderTarget2DView"/> as a single object that users can use to apply a render target texture. 
        /// This helps simplify creation of a render target by executing some prerequisite steps on behalf of the user.
        /// </para>
        /// <para>
        /// Since the <see cref="GorgonTexture2D"/> created by this method is linked to the <see cref="GorgonRenderTarget2DView"/> returned, disposal of either one will dispose of the other on your behalf. 
        /// If the user created a <see cref="GorgonRenderTarget2DView"/> from the <see cref="GorgonTexture2D.GetRenderTargetView"/> method on the <see cref="GorgonTexture2D"/>, then it's assumed the user 
        /// knows what they are doing and will handle the disposal of the texture and view on their own.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTexture2D"/>
        public static GorgonRenderTarget2DView CreateRenderTarget(GorgonGraphics graphics, IGorgonTexture2DInfo info, int arrayIndex = 0, int? arrayCount = null)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (info == null)
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

            var newInfo = new GorgonTexture2DInfo(info)
                          {
                              // Can't see a reason to use anything other than default for rtvs
                              Usage = ResourceUsage.Default,
                              Binding = binding
                          };

            var texture = new GorgonTexture2D(graphics, newInfo);
            GorgonRenderTarget2DView result = texture.GetRenderTargetView(arrayIndex: arrayIndex, arrayCount: arrayCount ?? texture.ArrayCount);
            result.OwnsResource = true;

            return result;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRenderTarget2DView" /> class.
        /// </summary>
        /// <param name="texture">The render target texture to bind.</param>
        /// <param name="format">The format of the render target view.</param>
        /// <param name="formatInfo">The format information.</param>
        /// <param name="mipSlice">The mip slice to use in the view.</param>
        /// <param name="arrayIndex">The first array index to use in the view.</param>
        /// <param name="arrayCount">The number of array indices to use in the view.</param>
        internal GorgonRenderTarget2DView(GorgonTexture2D texture, BufferFormat format, GorgonFormatInfo formatInfo, int mipSlice, int arrayIndex, int arrayCount)
            : base(texture, format, formatInfo)
        {
            Texture = texture;
            MipSlice = mipSlice;
            ArrayIndex = arrayIndex;
            ArrayCount = arrayCount;
            MipWidth = (Width >> MipSlice).Max(1);
            MipHeight = (Height >> MipSlice).Max(1);
            Bounds = new DX.Rectangle(0, 0, Width, Height);
        }
        #endregion
    }
}