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
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using SharpDX.Mathematics.Interop;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


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
        : IDisposable, IGorgonTexture2DInfo, IGorgonGraphicsObject
    {
		#region Variables.
        // The texture linked to the view.
        private GorgonTexture2D _texture;
        // The D3D11 depth stencil view.
        private D3D11.DepthStencilView _view;
        // Clear rectangles.
        private RawRectangle[] _clearRects;
        // Flag to indicate that the view owns the texture resource.
        private bool _ownsTexture;
		#endregion

		#region Properties.
        /// <summary>
        /// Property to return the Direct3D depth/stencil view.
        /// </summary>
        internal D3D11.DepthStencilView Native => _view;

        /// <summary>
        /// Property to return whether this object is disposed or not.
        /// </summary>
        public bool IsDisposed => _view == null;

        /// <summary>
        /// Property to return the texture bound to this view.
        /// </summary>
        public GorgonTexture2D Texture => _texture;

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
        /// Property to return the intended usage flags for the texture.
        /// </summary>
        public ResourceUsage Usage => Texture?.Usage ?? ResourceUsage.Default;

        /// <summary>
        /// Property to return the flags to determine how the texture will be bound with the pipeline when rendering.
        /// </summary>
        public TextureBinding Binding => Texture?.Binding ?? TextureBinding.None;

        /// <summary>
        /// Property to return the graphics interface that built the texture.
        /// </summary>
        public GorgonGraphics Graphics => Texture?.Graphics;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the description for a 2D depth/stencil view.
        /// </summary>
        /// <returns>The direct 3D 2D depth/stencil view description.</returns>
        private D3D11.DepthStencilViewDescription GetDesc2D()
		{
			bool isMultisampled = Texture.MultisampleInfo != GorgonMultisampleInfo.NoMultiSampling;

			// Set up for arrayed and multisampled texture.
			if (Texture.ArrayCount > 1)
			{
				return new D3D11.DepthStencilViewDescription
				{
					Format = (DXGI.Format)Format,
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
				Format = (DXGI.Format)Format,
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
		internal void CreateNativeView()
		{
			D3D11.DepthStencilViewDescription desc = GetDesc2D();

			Graphics.Log.Print($"'{Texture.Name}': Creating D3D11 depth/stencil view.", LoggingLevel.Simple);

		    Graphics.Log.Print($"Depth/Stencil View '{Texture.Name}': {Texture.ResourceType} -> Mip slice: {MipSlice}, Array Index: {ArrayIndex}, Array Count: {ArrayCount}",
					   LoggingLevel.Verbose);

		    _view = new D3D11.DepthStencilView(Texture.Graphics.D3DDevice, Texture.D3DResource, desc)
		            {
		                DebugName = $"'{Texture.Name}'_D3D11DepthStencilView1_2D"
		            };

            this.RegisterDisposable(Texture.Graphics);
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
        public void Clear(float depthValue, DX.Rectangle[] rectangles)
        {
            if ((rectangles == null) || (rectangles.Length == 0) || (FormatInformation.HasStencil))
            {
                Clear(depthValue, 0);
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

            Texture.Graphics.D3DDeviceContext.ClearView(_view, new DX.Color4(depthValue), _clearRects, rectangles.Length);
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
            result._ownsTexture = true;

            return result;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            D3D11.DepthStencilView view = Interlocked.Exchange(ref _view, null);

            if (view == null)
            {
                return;
            }

            this.UnregisterDisposable(Texture.Graphics);

            if (_ownsTexture)
            {
                Graphics.Log.Print($"Depth/Stencil View '{Texture.Name}': Releasing D3D11 texture because it owns it.", LoggingLevel.Simple);

                GorgonTexture2D texture = Interlocked.Exchange(ref _texture, null);
                texture?.Dispose();
            }
		    
            Graphics.Log.Print($"Destroying depth/stencil view for {Texture.Name}.", LoggingLevel.Simple);
			view.Dispose();
		}
		#endregion

		#region Constructor/Destructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencil2DView"/> class.
		/// </summary>
		/// <param name="texture">The resource to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		/// <param name="mipSlice">The mip level to use for the view.</param>
		/// <param name="firstArrayIndex">The first array index to use for the view.</param>
		/// <param name="arrayCount">The number of array indices to use for the view.</param>
		/// <param name="flags">Depth/stencil view flags.</param>
		internal GorgonDepthStencil2DView(GorgonTexture2D texture,
		                              BufferFormat format,
		                              int mipSlice,
		                              int firstArrayIndex,
		                              int arrayCount,
		                              DepthStencilViewFlags flags)
		{
		    _texture = texture ?? throw new ArgumentNullException(nameof(texture));

		    if ((texture.Binding & TextureBinding.DepthStencil) != TextureBinding.DepthStencil)
		    {
		        throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_RESOURCE_NOT_DEPTHSTENCIL, texture.Name), nameof(texture));
		    }

		    FormatInformation = new GorgonFormatInfo(format);

		    if (FormatInformation.IsTypeless)
		    {
		        throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
		    }

		    if (firstArrayIndex + arrayCount > texture.ArrayCount)
		    {
		        throw new ArgumentException(string.Format(Resources.GORGFX_ERR_TEXTURE_VIEW_ARRAY_OUT_OF_RANGE,
		                                                  firstArrayIndex,
		                                                  arrayCount,
		                                                  texture.ArrayCount));
		    }

		    if ((!texture.FormatInformation.IsTypeless) && ((texture.Binding & TextureBinding.ShaderResource) == TextureBinding.ShaderResource))
		    {
		        throw new ArgumentException(Resources.GORGFX_ERR_DEPTHSTENCIL_TYPED_SHADER_RESOURCE, nameof(texture));
		    }

		    Format = format;

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
