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
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Gorgon.Graphics
{
	/// <summary>
	/// A depth/stencil view for textures.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is a depth/stencil view to allow a <see cref="GorgonTexture"/> to be bound to the GPU pipeline as a depth/stencil resource.
	/// </para>
	/// <para>
	/// Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow the resource to be cast to any 
	/// format within the same group.	
	/// </para>
	/// </remarks>
	public class GorgonDepthStencilView
    {
		#region Variables.
		// A log for debug logging.
		private readonly IGorgonLog _log;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct3D depth/stencil view.
		/// </summary>
		internal D3D11.DepthStencilView D3DView
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the key for the resource view.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This key can be used to sort, or define a unique resource view for use in caching. Users may set this key however they see fit to meet their caching/sorting needs. However, it is recommended 
		/// that this key be left alone, and never altered after it's been applied to a cache since it should be a unique value.
		/// </para>
		/// <para>
		/// By default, Gorgon will set the view parameters as a 64 bit unsigned integer value when the view is created. This key is composed of the following bits:
		/// <para>
		/// <list type="table">
		///		<listheader>
		///			<term>Bits</term>
		///			<term>Value</term>		
		///		</listheader>
		///		<item>
		///			<term>0 - 3 (4 bits)</term>
		///			<term><see cref="MipSlice">Mip slice.</see></term>
		///		</item>
		///		<item>
		///			<term>4 - 14 (11 bits)</term>
		///			<term><see cref="ArrayIndex">Array Index.</see></term>
		///		</item>
		///		<item>
		///			<term>15 - 25 (11 bits)</term>
		///			<term><see cref="ArrayIndex">Array Count.</see></term>
		///		</item>
		///		<item>
		///			<term>26 - 33 (8 bits)</term>
		///			<term><see cref="Format"/></term>
		///		</item>
		///		<item>
		///			<term>34 - 35 (2 bits)</term>
		///			<term><see cref="Flags"/></term>
		///		</item>
		/// </list>
		/// </para>
		/// </para>
		/// <para>
		/// For example, a <see cref="MipSlice"/> of 2, and a <see cref="ArrayIndex"/> of 4, with an <see cref="ArrayCount"/> of 2 and a <see cref="Format"/> 
		/// of <c>R8G8B8A8_UNorm"</c> (28) with no depth <see cref="Flags"/> would yield a key of: <c>1879113794</c>. 
		/// <br/>
		/// Or, a <see cref="MipSlice"/> of 0, with a <see cref="ArrayIndex"/> of 0, and a <see cref="ArrayCount"/> of 1, and the same buffer format would yield a key of: <c>1879048193</c>.
		/// </para>
		/// </remarks>
		public ulong Key
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the texture bound to this view.
		/// </summary>
		public GorgonTexture Texture
		{
			get;
		}

		/// <summary>
		/// Property to return the format for this view.
		/// </summary>
		public DXGI.Format Format
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
		/// <para>
		/// This is only valid on video devices with a feature level of SM5 or better.
		/// </para>
		/// </remarks>
		public D3D11.DepthStencilViewFlags Flags
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
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the description for a 1D depth/stencil view.
		/// </summary>
		/// <returns>The direct 3D 1D depth/stencil view description.</returns>
		private D3D11.DepthStencilViewDescription GetDesc1D()
		{
			// Set up for arrayed and multisampled texture.
			if (Texture.Info.ArrayCount > 1)
			{
				return new D3D11.DepthStencilViewDescription
				{
					Format = Format,
					Dimension = D3D11.DepthStencilViewDimension.Texture1DArray,
					Texture1DArray =
					{
						MipSlice = MipSlice,
						FirstArraySlice = ArrayIndex,
						ArraySize = ArrayCount
					}
				};
			}

			return new D3D11.DepthStencilViewDescription
			{
				Format = Format,
				Dimension = D3D11.DepthStencilViewDimension.Texture1D,
				Texture1D =
				{
					MipSlice = MipSlice
				}
			};
		}

		/// <summary>
		/// Function to retrieve the description for a 2D depth/stencil view.
		/// </summary>
		/// <returns>The direct 3D 2D depth/stencil view description.</returns>
		private D3D11.DepthStencilViewDescription GetDesc2D()
		{
			bool isMultiSampled = Texture.Info.MultiSampleInfo != GorgonMultiSampleInfo.NoMultiSampling;

			// Set up for arrayed and multisampled texture.
			if (Texture.Info.ArrayCount > 1)
			{
				return new D3D11.DepthStencilViewDescription
				{
					Format = Format,
					Dimension = isMultiSampled
									? D3D11.DepthStencilViewDimension.Texture2DMultisampledArray
									: D3D11.DepthStencilViewDimension.Texture2DArray,
					Texture2DArray =
					{
						MipSlice = isMultiSampled ? ArrayIndex : MipSlice,
						FirstArraySlice = isMultiSampled ? ArrayCount : ArrayIndex,
						ArraySize = isMultiSampled ? 0 : ArrayCount
					}
				};
			}

			return new D3D11.DepthStencilViewDescription
			{
				Format = Format,
				Dimension = isMultiSampled
								? D3D11.DepthStencilViewDimension.Texture2DMultisampled
								: D3D11.DepthStencilViewDimension.Texture2D,
				Texture2D =
				{
					MipSlice = isMultiSampled ? 0 : MipSlice
				}
			};
		}

		/// <summary>
		/// Function to perform initialization of the view.
		/// </summary>
		private void Initialize()
		{
			D3D11.DepthStencilViewDescription desc = default(D3D11.DepthStencilViewDescription);

			_log.Print($"'{Texture.Name}': Creating D3D11 depth/stencil view.", LoggingLevel.Simple);

			desc.Dimension = D3D11.DepthStencilViewDimension.Unknown;

			switch (Texture.ResourceType)
			{
				case ResourceType.Texture1D:
					desc = GetDesc1D();
					break;
				case ResourceType.Texture2D:
					desc = GetDesc2D();
					break;
			}

			if (desc.Dimension == D3D11.DepthStencilViewDimension.Unknown)
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_CANNOT_BIND_UNKNOWN_RESOURCE);
			}

			_log.Print($"Depth/Stencil View '{Texture.Name}': {Texture.ResourceType} -> Mip slice: {MipSlice}, Array Index: {ArrayIndex}, Array Count: {ArrayCount}",
					   LoggingLevel.Verbose);

			D3DView = new D3D11.DepthStencilView(Texture.Graphics.VideoDevice.D3DDevice(), Texture.D3DResource, desc)
			          {
				          DebugName = $"'{Texture.Name}': D3D11 depth/stencil view"
			          };
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			if (D3DView != null)
			{
				_log.Print($"Destroying depth/stencil view for {Texture.Name}.", LoggingLevel.Verbose);
			}

			D3DView?.Dispose();
		}
		#endregion

		#region Constructor/Destructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencilView"/> class.
		/// </summary>
		/// <param name="texture">The resource to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		/// <param name="firstMipLevel">The mip level to use for the view.</param>
		/// <param name="firstArrayIndex">The first array index to use for the view.</param>
		/// <param name="arrayCount">The number of array indices to use for the view.</param>
		/// <param name="flags">Depth/stencil view flags.</param>
		/// <param name="log">[Optional] The log used for debugging.</param>
		public GorgonDepthStencilView(GorgonTexture texture,
		                              DXGI.Format format,
		                              int firstMipLevel = 0,
		                              int firstArrayIndex = 0,
		                              int arrayCount = 0,
		                              D3D11.DepthStencilViewFlags flags = D3D11.DepthStencilViewFlags.None,
									  IGorgonLog log = null)
		{
			_log = log ?? GorgonLogDummy.DefaultInstance;

			if (texture == null)
			{
				throw new ArgumentNullException(nameof(texture));
			}

			Texture = texture;

			format = format == DXGI.Format.Unknown ? Texture.Info.Format : format;

			// Only allow the depth/stencil formats.
			switch (format)
			{
				case DXGI.Format.D16_UNorm:
				case DXGI.Format.D24_UNorm_S8_UInt:
				case DXGI.Format.D32_Float:
				case DXGI.Format.D32_Float_S8X24_UInt:
					break;
				default:
					format = DXGI.Format.Unknown;
					break;
			}

			Format = format;

			if (Format == DXGI.Format.Unknown)
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_UNKNOWN_FORMAT, Format), nameof(texture));
			}

			MipSlice = Texture.Info.MipLevels <= 0 ? 0 : firstMipLevel.Max(0).Min(Texture.Info.MipLevels - 1);

			ArrayIndex = firstArrayIndex.Max(0).Min(Texture.Info.ArrayCount - 1);

			if (arrayCount == 0)
			{
				arrayCount = Texture.Info.ArrayCount;
			}

			ArrayCount = arrayCount.Min(arrayCount - ArrayIndex).Max(1);
			
			Flags = flags;
			Initialize();

			// The key for a texture shader view is broken up into the following layout.
			// Bits: [35 - 34]   [33 - 26]   [25 - 15]     [14 - 4]		[3 - 0]
			//       Flags       Format      Array/Depth   Array/Depth  Mip slice
			//                               Count         Index
			Key = (((uint)Flags) & 0x3) << 34
				  | (((uint)Format) & 0xff) << 26
				  | (((uint)ArrayCount) & 0x7ff) << 15
				  | (((uint)ArrayIndex) & 0x7ff) << 4
				  | (uint)MipSlice;
		}

		#endregion
	}
}
