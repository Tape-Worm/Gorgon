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
// Created: Thursday, August 01, 2013 8:10:32 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Properties;
using Gorgon.Math;
using Gorgon.Native;
using SharpDX.Mathematics.Interop;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A lock used to update texture data from the CPU.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A lock is used to modify a <see cref="GorgonTexture"/> from the CPU by returning a pointer to the texture location in memory. 
	/// </para>
	/// <para>
	/// To unlock the lock, just call the <see cref="IDisposable.Dispose"/> method on this object and the changes made will be uploaded to the GPU. If the texture that owns this lock is disposed before this 
	/// object is disposed, then this object will be invalid and disposed automatically.
	/// </para>
	/// <para>
	/// Locks can only be placed on <see cref="GorgonTexture"/> objects that were created with a <c>Dynamic</c> or <c>Staging</c> <see cref="GorgonTextureInfo.Usage"/>. If the usage for the texture is set to 
	/// <c>Staging</c>, then the lock can read the data in the texture, otherwise the lock will allow write-only access to the texture.
	/// </para>
	/// <para>
	/// <note type="caution">
	/// <para>
	/// When a texture is locked, the texture will be inaccessible to the GPU. Consequently, it is important to ensure the lock's duration is as short as possible.
	/// </para>
	/// </note>
	/// </para>
	/// <seealso cref="GorgonTexture.Lock"/>
	/// </remarks>
	public class GorgonTextureLockData
        : IGorgonImageBuffer, IDisposable 
    {
		#region Variables.
		// The cache that owns the lock.
		private readonly TextureLockCache _lockCache;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the texture is locked.
		/// </summary>
		public GorgonTexture Texture => _lockCache.Texture;

		/// <summary>
		/// Property to return the format of the buffer.
		/// </summary>
		public DXGI.Format Format
		{
			get;
		}

		/// <summary>
		/// Property to return the width for the current buffer.
		/// </summary>
		public int Width
		{
			get;
		}

		/// <summary>
		/// Property to return the height for the current buffer.
		/// </summary>
		/// <remarks>This is only valid for 2D and 3D images.</remarks>
		public int Height
		{
			get;
		}

		/// <summary>
		/// Property to return the depth for the current buffer.
		/// </summary>
		/// <remarks>This is only valid for 3D images.</remarks>
		public int Depth
		{
			get;
		}

		/// <summary>
		/// Property to return the mip map level this buffer represents.
		/// </summary>
		public int MipLevel
		{
			get;
		}

		/// <summary>
		/// Property to return the array this buffer represents.
		/// </summary>
		/// <remarks>
		/// For 3D images, this will always be 0.
		/// </remarks>
		public int ArrayIndex
		{
			get;
		}

		/// <summary>
		/// Property to return the depth slice index.
		/// </summary>
		/// <remarks>
		/// For a lock, this value is always 0.
		/// </remarks>
		int IGorgonImageBuffer.DepthSliceIndex => 0;

		/// <summary>
		/// Property to return the data stream for the image data.
		/// </summary>
		public IGorgonPointer Data
		{
			get;
		}

		/// <summary>
		/// Property to return information about the pitch of the data for this buffer.
		/// </summary>
		public GorgonPitchLayout PitchInformation
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to copy the image buffer data from this buffer into another.
		/// </summary>
		/// <param name="buffer">The buffer to copy into.</param>
		/// <param name="sourceRegion">[Optional] The region in the source to copy.</param>
		/// <param name="destX">[Optional] Horizontal offset in the destination buffer.</param>
		/// <param name="destY">[Optional] Vertical offset in the destination buffer.</param>
		/// <exception cref="NotSupportedException">This method is not supported for locks.</exception>
		void IGorgonImageBuffer.CopyTo(IGorgonImageBuffer buffer, DX.Rectangle? sourceRegion, int destX, int destY)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			_lockCache?.Unlock(this);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTextureLockData"/> class.
		/// </summary>
		/// <param name="cache">The cache containing the locks.</param>
		/// <param name="data">The data returned from the lock.</param>
		/// <param name="mipLevel">The mip level of the sub resource.</param>
		/// <param name="arrayIndex">Array index of the sub resource.</param>
		internal GorgonTextureLockData(TextureLockCache cache, DX.DataBox data, int mipLevel, int arrayIndex)
        {
            _lockCache = cache;

            Width = Texture.Info.Width;
            Height = Texture.Info.Height;
            Depth = Texture.Info.Depth;

            // Calculate the current size at the given mip level.
            for (int mip = 0; mip < mipLevel; ++mip)
            {
                if (Width > 1)
                {
                    Width >>= 1;
                }
                if (Height > 1)
                {
                    Height >>= 1;
                }
                if (Depth > 1)
                {
                    Depth >>= 1;
                }
            }

			Format = Texture.Info.Format;
			MipLevel = mipLevel;
			ArrayIndex = Texture.Info.TextureType != TextureType.Texture3D ? arrayIndex : 0;
			PitchInformation = new GorgonPitchLayout(data.RowPitch, data.SlicePitch);
            Data = new GorgonPointerAlias(data.DataPointer, data.SlicePitch);
        }
        #endregion
	}
}
