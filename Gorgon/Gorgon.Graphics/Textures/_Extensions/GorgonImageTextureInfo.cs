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
// Created: July 20, 2016 10:40:11 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Imaging;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Provides information on how to convert a <see cref="GorgonImage"/> to a <see cref="GorgonTexture"/>.
	/// </summary>
	public class GorgonImageTextureInfo
		: IGorgonCloneable<GorgonImageTextureInfo>
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the intended usage for the texture.
		/// </summary>
		/// <remarks>
		/// This value is defaulted to <c>Default</c>.
		/// </remarks>
		public D3D11.ResourceUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return how the texture will be bound to the GPU.
		/// </summary>
		/// <remarks>
		/// This value is defaulted to <see cref="TextureBinding.ShaderResource"/>.
		/// </remarks>
		public TextureBinding Binding
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the multisampling information for the texture.
		/// </summary>
		/// <remarks>
		/// This value is defaulted to <see cref="GorgonMultiSampleInfo.NoMultiSampling"/>.
		/// </remarks>
		public GorgonMultiSampleInfo MultiSampleInfo
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clone an object.
		/// </summary>
		/// <returns>The cloned object.</returns>
		public GorgonImageTextureInfo Clone()
		{
			return new GorgonImageTextureInfo
			       {
				       Usage = Usage,
				       MultiSampleInfo = MultiSampleInfo,
				       Binding = Binding
			       };
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonImageTextureInfo"/> class.
		/// </summary>
		public GorgonImageTextureInfo()
		{
			Usage = D3D11.ResourceUsage.Default;
			Binding = TextureBinding.ShaderResource;
			MultiSampleInfo = GorgonMultiSampleInfo.NoMultiSampling;
		}
		#endregion
	}
}
