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
// Created: Thursday, January 31, 2013 8:29:10 AM
// 

// This code was adapted from the SharpDX ToolKit by Alexandre Mutel.
#region SharpDX License.
// Copyright (c) 2010-2013 SharpDX - Alexandre Mutel
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

// SharpDX is available from http://sharpdx.org
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Native;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// A container for raw image data that can be sent to or read from a image.
    /// </summary>
    /// <typeparam name="T">Type of image settings to use.  Must implement the <see cref="GorgonLibrary.Graphics.ITextureSettings">ITextureSettings</see> interface.</typeparam>
    /// <remarks>This object will allow pixel manipulation of image data.  It will break a image into buffers, such as a series of buffers 
    /// for arrays, mip-map levels and depth slices.</remarks>
    public unsafe class GorgonImageData<T>
        : IDisposable, IEnumerable<GorgonImageData<T>.ImageBuffer>, System.Collections.IEnumerable
        where T : class, ITextureSettings
	{
		#region Classes.
		/// <summary>
		/// An image buffer containing data about the image.
		/// </summary>
		public class ImageBuffer
		{
			#region Properties.
			/// <summary>
			/// Property to return the mip map level this buffer represents.
			/// </summary>
			public int MipLevel
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the array this buffer represents.
			/// </summary>
			/// <remarks>For 3D images, this will always be 0.</remarks>
			public int ArrayIndex
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the depth slice index.
			/// </summary>
			/// <remarks>For 1D or 2D images, this will always be 0.</remarks>
			public int SliceIndex
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return the data stream for the image data.
			/// </summary>
			public GorgonDataStream Data
			{
				get;
				private set;
			}

			/// <summary>
			/// Property to return information about the pitch of the data for this buffer.
			/// </summary>
			public GorgonFormatPitch PitchInformation
			{
				get;
				private set;
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="ImageBuffer" /> class.
			/// </summary>
			/// <param name="dataStart">The data start.</param>
			/// <param name="pitchInfo">The pitch info.</param>
			/// <param name="mipLevel">Mip map level.</param>
			/// <param name="arrayIndex">Array index.</param>
			/// <param name="sliceIndex">Slice index.</param>
			internal unsafe ImageBuffer(void *dataStart, GorgonFormatPitch pitchInfo, int mipLevel, int arrayIndex, int sliceIndex)
			{
				Data = new GorgonDataStream(dataStart, pitchInfo.SlicePitch);
				PitchInformation = pitchInfo;
				MipLevel = mipLevel;
				ArrayIndex = arrayIndex;
				SliceIndex = sliceIndex;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private bool _disposed = false;                             // Flag to indicate whether the object was disposed.
        private GorgonDataStream _imageData = null;                 // Base image data buffer.
		private ImageBuffer[] _buffers = null;						// Buffers for access to the arrays, slices and mip maps.
		private Tuple<int, int>[] _mipOffsetSize = null;			// Offsets and sizes in the buffer list for mip maps.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the settings for the image.
        /// </summary>
        public T Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the number of bytes, in total, that this image occupies.
        /// </summary>
        public int SizeInBytes
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return the total number of buffers allocated.
		/// </summary>
		public int Count
		{
			get
			{
				return _buffers.Length;
			}
		}

		/// <summary>
		/// Property to return the buffer for the given array index, mip map level and depth slice.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the array index, mip level or depth slice parameters are larger than their respective boundaries, or less than 0.</exception>
		/// <remarks>To get the array length and/or the mip map count, use the <see cref="P:GorgonLibrary.Graphics.GorgonImageData{T}.Settings">Settings</see> property.
		/// <para>To get the depth slice count, use the <see cref="P:GorgonLibrary.Graphics.GorgonImageData{T}.GetDepthCount">GetDepthCount</see> method.</para>
		/// </remarks>
		public ImageBuffer this[int arrayIndex, int mipLevel, int depthSlice]
		{
			get
			{
				GorgonDebug.AssertParamRange(arrayIndex, 0, Settings.ArrayCount, "arrayIndex");
				GorgonDebug.AssertParamRange(mipLevel, 0, Settings.MipCount, "mipLevel");

				Tuple<int, int> offsetSize = _mipOffsetSize[mipLevel + (arrayIndex * Settings.MipCount)];

				GorgonDebug.AssertParamRange(depthSlice, 0, offsetSize.Item2, "depthSlice");

				return _buffers[offsetSize.Item1 + depthSlice];
			}
		}

		/// <summary>
		/// Property to return the buffer for the given mip map level and depth slice.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the array index or the depth slice parameters are larger than their respective boundaries, or less than 0.</exception>
		/// <remarks>To get the array length and/or the mip map count, use the <see cref="P:GorgonLibrary.Graphics.GorgonImageData{T}.Settings">Settings</see> property.
		/// </remarks>
		public ImageBuffer this[int arrayIndex, int mipLevel]
		{
			get
			{
				GorgonDebug.AssertParamRange(arrayIndex, 0, Settings.ArrayCount, "arrayIndex");
				GorgonDebug.AssertParamRange(mipLevel, 0, Settings.MipCount, "mipLevel");

				Tuple<int, int> offsetSize = _mipOffsetSize[mipLevel + (arrayIndex * Settings.MipCount)];

				return _buffers[offsetSize.Item1];
			}
		}

		/// <summary>
		/// Property to return the buffer for the given mip map level.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the mip map level parameter is larger than the number of mip levels, or less than 0.</exception>
		/// <remarks>To get the mip map count, use the <see cref="P:GorgonLibrary.Graphics.GorgonImageData{T}.Settings">Settings</see> property.
		/// </remarks>
		public ImageBuffer this[int mipLevel]
		{
			get
			{
				GorgonDebug.AssertParamRange(mipLevel, 0, Settings.MipCount, "mipLevel");

				Tuple<int, int> offsetSize = _mipOffsetSize[mipLevel];

				return _buffers[offsetSize.Item1];
			}
		}
		#endregion

        #region Methods.
		/// <summary>
		/// Function to retrieve the number of buffers required for the image data.
		/// </summary>
		/// <returns>The number of buffers required for the image data.</returns>
		private int GetBufferCount()
		{
			int bufferCount = 0;
			int depth = Settings.Depth;

			// We're not using a depth image at this point (or the depth image only has 1 slice).
			if (depth < 2)
			{
				return Settings.ArrayCount * Settings.MipCount;
			}
			
			for (int i = 0; i < Settings.MipCount; i++)
			{
				bufferCount += depth;

				if (depth > 1)
				{
					depth >>= 1;
				}				
			}

			return bufferCount;
		}

		/// <summary>
		/// Function to initialize the image data.
		/// </summary>
		/// <param name="data">Pre-existing data to use.</param>
		private void Initialize(void* data)
        {
			int bufferIndex = 0;
			var formatInfo = GorgonBufferFormatInfo.GetInfo(Settings.Format);	// Format information.

            // Create a buffer large enough to hold our data.
            if (data == null)
            {
                _imageData = new GorgonDataStream(SizeInBytes);
            }
            else
            {
                _imageData = new GorgonDataStream(data, SizeInBytes);
            }

			// Create buffers.
			_buffers = new ImageBuffer[GetBufferCount()];										// Allocate enough room for the array and mip levels.
			_mipOffsetSize = new Tuple<int, int>[Settings.MipCount * Settings.ArrayCount];		// Offsets for the mip maps.
			byte* imageData = (byte*)_imageData.UnsafePointer;									// Start at the beginning of our data block.
			
			// Enumerate array indices. (For 1D and 2D only, 3D will always be 1)
			for (int array = 0; array < Settings.ArrayCount; array++)
			{
				int mipWidth = Settings.Height;
				int mipHeight = Settings.Width;
				int mipDepth = Settings.Depth;

				// Enumerate mip map levels.
				for (int mip = 0; mip < Settings.MipCount; mip++)
				{
					// Allocate a jagged array because our depth size changes with each mip level.
					for (int depth = 0; depth < mipDepth; depth++)
					{
						// Get mip information.
						var pitchInformation = formatInfo.GetPitch(mipWidth, mipHeight, PitchFlags.None);
						imageData += pitchInformation.SlicePitch;

						_buffers[bufferIndex] = new ImageBuffer(imageData, pitchInformation, mip, array, depth);
					}

					_mipOffsetSize[mip + (array * Settings.MipCount)] = new Tuple<int, int>(bufferIndex, mipDepth);
					bufferIndex += mipDepth;

					if (mipWidth > 1)
					{
						mipWidth >>= 1;
					}
					if (mipHeight > 1)
					{
						mipHeight >>= 1;
					}
					if (mipDepth > 1)
					{
						mipDepth >>= 1;
					}
				}
			}
        }

		/// <summary>
		/// Function to sanitize the image settings.
		/// </summary>
		private void SanitizeSettings()
        {
			Settings.ArrayCount = 1.Max(Settings.ArrayCount);
			Settings.Width = 1.Max(Settings.Width);
			Settings.Height = 1.Max(Settings.Height);
			Settings.Depth = 1.Max(Settings.Depth);

			// If this is using 3D texture settings, then set the array count to 1.
			if (Settings is GorgonTexture3DSettings)
			{
				Settings.ArrayCount = 1;
			}
			else
			{
				Settings.Depth = 1;
			}

			// Ensure mip values do not exceed more than what's available based on width, height and/or depth.
			if (Settings.MipCount > 1)
			{
				Settings.MipCount = Settings.MipCount.Min(Settings.CalculateMipLevels());
			}

			// Create mip levels if we didn't specify any.
			if (Settings.MipCount == 0)
			{
				Settings.MipCount = Settings.CalculateMipLevels();
			}
        }

		/// <summary>
		/// Function to return the number of depth slices for a given mip map slice.
		/// </summary>
		/// <param name="mipLevel">The mip map level to look up.</param>
		/// <returns>The number of depth slices for the given mip map level.</returns>
		/// <remarks>For 1D and 2D images, the mip level will always return 1.</remarks>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="mipLevel"/> parameter exceeds the number of mip maps for the image or is less than 0.</exception>
		public int GetDepthCount(int mipLevel)
		{
			GorgonDebug.AssertParamRange(mipLevel, 0, Settings.MipCount, "mipLevel");

			if (Settings.Depth <= 1)
			{
				return 1;
			}

			return _mipOffsetSize[mipLevel].Item2; 
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageData{T}" /> class.
        /// </summary>
        /// <param name="settings">The settings to describe an image.</param>
        /// <param name="data">Pointer to pre-existing image data.</param>
        /// <param name="dataSize">Size of the data, in bytes.  This parameter is ignored if the data parameter is NULL.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the image format is unknown or is unsupported.</exception>
        /// <remarks>If the <paramref name="data"/> pointer is NULL, then a buffer will be created, otherwise the buffer that the pointer is pointing 
        /// at must be large enough to accomodate the size of the image described in the settings parameter and will be validated against the 
        /// <paramref name="dataSize"/> parameter.  
        /// <para>If the user passes NULL to the data parameter, then the dataSize parameter is ignored.</para>
        /// <para>If the buffer is passed in via the pointer, then the user is responsible for freeing that memory 
        /// once they are done with it.</para></remarks>
        public GorgonImageData(T settings, void *data, int dataSize)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            if (settings.Format == BufferFormat.Unknown)
            {
                throw new GorgonException(GorgonResult.CannotCreate, "The image format is not known.");
            }

            Settings = settings;            
            SanitizeSettings();
			SizeInBytes = Settings.CalculateMipLevels();

            // Validate the image size.
            if ((data != null) && (SizeInBytes > dataSize))
            {
                throw new ArgumentOutOfRangeException("There is a mismatch between the image size, and the buffer size.", "dataSize");
            }

            Initialize(data);
        }

		/// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageData{T}" /> class.
        /// </summary>
        /// <param name="settings">The settings to describe an image.</param>
        /// <param name="data">Pointer to pre-existing image data.</param>
        /// <param name="dataSize">Size of the data, in bytes.  This parameter is ignored if the data parameter is NULL.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the image format is unknown or is unsupported.</exception>
        /// <remarks>If the <paramref name="data"/> pointer is NULL, then a buffer will be created, otherwise the buffer that the pointer is pointing 
        /// at must be large enough to accomodate the size of the image described in the settings parameter and will be validated against the 
        /// <paramref name="dataSize"/> parameter.  
        /// <para>If the user passes NULL to the data parameter, then the dataSize parameter is ignored.</para>
        /// <para>If the buffer is passed in via the pointer, then the user is responsible for freeing that memory 
        /// once they are done with it.</para></remarks>
		public GorgonImageData(T settings, IntPtr data, int dataSize)
			: this(settings, data == IntPtr.Zero ? null : data.ToPointer(), dataSize)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonImageData{T}" /> class.
		/// </summary>
		/// <param name="settings">The settings to describe an image.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the image format is unknown or is unsupported.</exception>
		public GorgonImageData(T settings)
			: this(settings, null, 0)
		{
		}
		#endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_imageData != null)
                    {
                        _imageData.Dispose();
                    }

					if (_buffers != null)
					{
						foreach (var buffer in _buffers)
						{
							if (buffer.Data != null)
							{
								buffer.Data.Dispose();
							}
						}
					}
                }

				_buffers = null;
                _imageData = null;
                _disposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    
		#region IEnumerable<T> Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.Generic.IEnumerator{T}" /> object that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<GorgonImageData<T>.ImageBuffer> GetEnumerator()
		{
			foreach (var item in _buffers)
			{
				yield return item;
			}
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _buffers.GetEnumerator();
		}
		#endregion
	}
}
