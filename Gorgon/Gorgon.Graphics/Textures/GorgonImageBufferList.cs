#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Sunday, October 5, 2014 12:40:08 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Diagnostics;
using DX = SharpDX;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A container for a list of image buffers.
	/// </summary>
	public sealed class GorgonImageBufferList
		: IReadOnlyList<GorgonImageBuffer>, IList<GorgonImageBuffer>
	{
		#region Variables.
		// List of buffers.
		private GorgonImageBuffer[] _buffers;
		// Image that owns this buffer.
		private readonly GorgonImageData _image;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the databoxes for the buffers.
		/// </summary>
		internal DX.DataBox[] DataBoxes
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the offsets of the mip map levels.
		/// </summary>
		internal Tuple<int, int>[] MipOffsetSize
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the buffer for the given mip map level and depth slice.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the array index or the depth slice parameters are larger than their respective boundaries, or less than 0.</exception>
		/// <remarks>To get the array length, or the mip map count, use the <see cref="P:GorgonLibrary.Graphics.GorgonImageData.Settings">Settings</see> property.
		/// <para>To get the depth slice count, use the <see cref="Gorgon.Graphics.GorgonImageData.GetDepthCount">GetDepthCount</see> method.</para>
		/// <para>The <paramref name="arrayIndexDepthSlice"/> parameter is used as an array index if the image is 1D or 2D.  If it is a 3D image, then the value indicates a depth slice.</para>
		/// </remarks>
		public GorgonImageBuffer this[int mipLevel, int arrayIndexDepthSlice = 0]
		{
			get
			{
				Tuple<int, int> offsetSize;

				GorgonDebug.AssertParamRange(mipLevel, 0, _image.Settings.MipCount, "mipLevel");

				if (_image.Settings.ImageType == ImageType.Image3D)
				{
					GorgonDebug.AssertParamRange(arrayIndexDepthSlice, 0, _image.Settings.Depth, "arrayIndexDepthSlice");
					offsetSize = MipOffsetSize[mipLevel];
					return _buffers[offsetSize.Item1 + arrayIndexDepthSlice];
				}

				GorgonDebug.AssertParamRange(arrayIndexDepthSlice, 0, _image.Settings.ArrayCount, "arrayIndexDepthSlice");
				offsetSize = MipOffsetSize[mipLevel + (arrayIndexDepthSlice * _image.Settings.MipCount)];
				return _buffers[offsetSize.Item1];
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear the buffer list and associated data streams.
		/// </summary>
		/// <param name="disposing"><b>true</b> when calling from a dispose method, <b>false</b> if not.</param>
		internal void ClearBuffers(bool disposing)
		{
			if (_buffers == null)
			{
				return;
			}

			foreach (GorgonImageBuffer buffer in _buffers)
			{
				buffer.Data.Dispose();
			}

			_buffers = !disposing ? new GorgonImageBuffer[0] : null;
		}

		/// <summary>
		/// Function to create a list of buffers to use.
		/// </summary>
		/// <param name="data">Data to copy/reference.</param>
		internal unsafe void CreateBuffers(byte* data)
		{
			int bufferIndex = 0;
			var formatInfo = GorgonBufferFormatInfo.GetInfo(_image.Settings.Format);	// Format information.
			
			// Allocate enough room for the array and mip levels.
			_buffers = new GorgonImageBuffer[GorgonImageData.GetDepthSliceCount(_image.Settings.Depth, _image.Settings.MipCount) * _image.Settings.ArrayCount];	

			MipOffsetSize = new Tuple<int, int>[_image.Settings.MipCount * _image.Settings.ArrayCount];	// Offsets for the mip maps.
			DataBoxes = new DX.DataBox[_image.Settings.ArrayCount * _image.Settings.MipCount];			// Create the data boxes for textures.

			// Enumerate array indices. (For 1D and 2D only, 3D will always be 1)
			for (int array = 0; array < _image.Settings.ArrayCount; array++)
			{
				int mipWidth = _image.Settings.Width;
				int mipHeight = _image.Settings.Height;
				int mipDepth = _image.Settings.Depth;

				// Enumerate mip map levels.
				for (int mip = 0; mip < _image.Settings.MipCount; mip++)
				{
					int arrayIndex = mip + (array * _image.Settings.MipCount);
					var pitchInformation = formatInfo.GetPitch(mipWidth, mipHeight, PitchFlags.None);

					// Get data box for texture upload.
					DataBoxes[arrayIndex] = new DX.DataBox(new IntPtr(data), pitchInformation.RowPitch, pitchInformation.SlicePitch);

					// Calculate buffer offset by mip.
					MipOffsetSize[arrayIndex] = new Tuple<int, int>(bufferIndex, mipDepth);

					// Enumerate depth slices.
					for (int depth = 0; depth < mipDepth; depth++)
					{
						// Get mip information.						
						_buffers[bufferIndex] = new GorgonImageBuffer(data, pitchInformation, mip, array, depth, mipWidth, mipHeight,
																		mipDepth, _image.Settings.Format);

						data += pitchInformation.SlicePitch;
						bufferIndex++;
					}

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
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonImageBufferList"/> class.
		/// </summary>
		/// <param name="image">The image that owns this list.</param>
		internal GorgonImageBufferList(GorgonImageData image)
		{
			_image = image;
			_buffers = new GorgonImageBuffer[0];
		}
		#endregion

		#region IReadOnlyList<GorgonImageBuffer> Members
		/// <summary>
		/// Gets the element at the specified index in the read-only list.
		/// </summary>
		/// <exception cref="System.NotSupportedException">This list is read only.</exception>
		GorgonImageBuffer IReadOnlyList<GorgonImageBuffer>.this[int index]
		{
			get
			{
				return this[index];
			}
		}
		#endregion

		#region IList<GorgonImageBuffer> Members
		#region Properties.
		/// <summary>
		/// Gets the element at the specified index in the read-only list.
		/// </summary>
		/// <exception cref="System.NotSupportedException">This list is read only.</exception>
		GorgonImageBuffer IList<GorgonImageBuffer>.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Property to return the index of a buffer.
		/// </summary>
		/// <param name="item">Buffer to find.</param>
		/// <returns>The index of the buffer, or -1 if not found.</returns>
		public int IndexOf(GorgonImageBuffer item)
		{
			return Array.IndexOf(_buffers, item);
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
		/// <exception cref="System.NotSupportedException">This list is read only.</exception>
		void IList<GorgonImageBuffer>.Insert(int index, GorgonImageBuffer item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="System.NotSupportedException">This list is read only.</exception>
		void IList<GorgonImageBuffer>.RemoveAt(int index)
		{
			throw new NotSupportedException();
		}
		#endregion
		#endregion

		#region ICollection<GorgonImageBuffer> Members
		#region Properties.
		/// <summary>
		/// Gets the number of elements in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				return _buffers.Length;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
		/// </summary>
		bool ICollection<GorgonImageBuffer>.IsReadOnly
		{
			get
			{
				return true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <exception cref="System.NotSupportedException">This list is read only.</exception>
		void ICollection<GorgonImageBuffer>.Add(GorgonImageBuffer item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <exception cref="System.NotSupportedException">This list is read only.</exception>
		void ICollection<GorgonImageBuffer>.Clear()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
		/// </returns>
		public bool Contains(GorgonImageBuffer item)
		{
			return _buffers.Contains(item);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		/// <exception cref="System.NotSupportedException"></exception>
		void ICollection<GorgonImageBuffer>.CopyTo(GorgonImageBuffer[] array, int arrayIndex)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
		/// <returns>
		/// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
		/// </returns>
		/// <exception cref="System.NotSupportedException">This list is read only.</exception>
		bool ICollection<GorgonImageBuffer>.Remove(GorgonImageBuffer item)
		{
			throw new NotSupportedException();
		}
		#endregion
		#endregion

		#region IEnumerable<GorgonImageBuffer> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<GorgonImageBuffer> GetEnumerator()
		{
			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (GorgonImageBuffer buffer in _buffers)
			{
				yield return buffer;
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
		IEnumerator IEnumerable.GetEnumerator()
		{
			return _buffers.GetEnumerator();
		}
		#endregion
	}
}
