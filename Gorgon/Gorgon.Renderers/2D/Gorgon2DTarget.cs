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
// Created: Monday, August 12, 2013 12:39:04 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics;

namespace Gorgon.Renderers
{
	/// <summary>
	/// A render target item.
	/// </summary>
	/// <remarks>This is for internal use by Gorgon.</remarks>
	struct Gorgon2DTarget
		: IEquatable<Gorgon2DTarget>
	{
		#region Variables.
		/// <summary>
		/// The render target that's in use.
		/// </summary>
		public readonly GorgonRenderTargetView Target;
		/// <summary>
		/// The viewport for the render target.
		/// </summary>
		public readonly GorgonViewport Viewport;
		/// <summary>
		/// The width of the render target.
		/// </summary>
		public readonly int Width;
		/// <summary>
		/// The height of the render target.
		/// </summary>
		public readonly int Height;
		/// <summary>
		/// The swap chain attached to this render target (if applicable).
		/// </summary>
		public readonly GorgonSwapChain SwapChain;
		/// <summary>
		/// The depth/stencil buffer attached to the target (if applicable).
		/// </summary>
		public readonly GorgonDepthStencilView DepthStencil;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns></returns>
		public static bool Equals(ref Gorgon2DTarget left, ref Gorgon2DTarget right)
		{
			return ((left.Target == right.Target) && (left.Viewport.Equals(right.Viewport))
			        && (left.Width == right.Width) && (left.Height == right.Height)
					&& (left.DepthStencil == right.DepthStencil));
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
		/// <returns>
		///   <b>true</b> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is Gorgon2DTarget)
			{
				return ((Gorgon2DTarget)obj).Equals(this);
			}
			return base.Equals(obj);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return 281.GenerateHash(Target).GenerateHash(Viewport).GenerateHash(Width).GenerateHash(Height);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2DTarget"/> struct.
		/// </summary>
		/// <param name="target">The target.</param>
		/// <param name="depthStencil">The depth/stencil view.</param>
		public Gorgon2DTarget(GorgonRenderTargetView target, GorgonDepthStencilView depthStencil)
		{
			Target = target;
			DepthStencil = null;
			SwapChain = null;
			Height = 1;

			switch (target.Resource.ResourceType)
			{
				case ResourceType.Buffer:
					var buffer = (GorgonBuffer)target.Resource;
					var info = GorgonBufferFormatInfo.GetInfo(buffer.Settings.DefaultShaderViewFormat);

					Width = buffer.SizeInBytes / info.SizeInBytes;
					Viewport = new GorgonViewport(0, 0, Width, Height, 0, 1.0f);
					break;
				case ResourceType.Texture1D:
					var target1D = (GorgonRenderTarget1D)target.Resource;

					Width = target1D.Settings.Width;
					Viewport = target1D.Viewport;
					DepthStencil = depthStencil ?? target1D.DepthStencilBuffer;
					break;
				case ResourceType.Texture2D:
					var target2D = (GorgonRenderTarget2D)target.Resource;

					Width = target2D.Settings.Width;
					Height = target2D.Settings.Height;
					Viewport = target2D.Viewport;
					if (target2D.IsSwapChain)
					{
						SwapChain = (GorgonSwapChain)target2D;
					}
					DepthStencil = depthStencil ?? target2D.DepthStencilBuffer;
					break;
				case ResourceType.Texture3D:
					var target3D = (GorgonRenderTarget3D)target.Resource;

					Width = target3D.Settings.Width;
					Height = target3D.Settings.Height;
					Viewport = target3D.Viewport;
					break;
				default:
					throw new GorgonException(GorgonResult.CannotBind, "Could not bind a render target.  Resource type is unknown.  Should not see this error.");
			}
		}
		#endregion

		#region IEquatable<Gorgon2DTarget> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
		/// </returns>
		public bool Equals(Gorgon2DTarget other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
