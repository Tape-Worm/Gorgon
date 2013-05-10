#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, February 08, 2012 3:17:25 PM
// 
#endregion

using System;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// This is used to initialize a 1 dimensional texture.
	/// </summary>
	public struct GorgonTexture1DData
		: ISubResourceData
	{
		#region Variables.
		private readonly int _rowPitch;
		private readonly GorgonDataStream _data;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2DData"/> struct.
		/// </summary>
		/// <param name="data">Data to pass to the texture.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
		public GorgonTexture1DData(GorgonDataStream data)
		{
			GorgonDebug.AssertNull(data, "data");

			_data = data;
			_rowPitch = (int)_data.Length;
		}
		#endregion

		#region ISubResourceData Members
		/// <summary>
		/// Property to return size, in bytes, of the data being passed to the sub resource.
		/// </summary>
		public int Size
		{
			get
			{
				if (_data == null)
				{
					return 0;
				}

				return (int)_data.Length;
			}
		}

		/// <summary>
		/// Property to return the number of bytes between each line in a texture.
		/// </summary>
		/// <remarks>This is only used for 2D and 3D textures and has no meaning elsewhere.</remarks>
		int ISubResourceData.RowPitch
		{
			get
			{
				return _rowPitch;
			}
		}

		/// <summary>
		/// Gets the slice pitch.
		/// </summary>
		int ISubResourceData.SlicePitch
		{
			get
			{
				return 0;
			}
		}

		/// <summary>
		/// Property to return the data to pass to a resource (like a texture).
		/// </summary>
		public GorgonDataStream Data
		{
			get
			{
				return _data;
			}
		}
		#endregion
	}

	/// <summary>
	/// This is used to initialize a 2 dimensional texture.
	/// </summary>
	public struct GorgonTexture2DData
		: ISubResourceData
	{
		#region Variables.
		private readonly int _rowPitch;
		private readonly GorgonDataStream _data;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2DData"/> struct.
		/// </summary>
		/// <param name="data">Data to assign to the texture.</param>
		/// <param name="rowPitch">Number of bytes between each line in the texture.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="rowPitch"/> parameter is less than or equal to 0.</exception>
		public GorgonTexture2DData(GorgonDataStream data, int rowPitch)
		{
			GorgonDebug.AssertNull(data, "data");

#if DEBUG
			if (rowPitch <= 0)
			{
				throw new ArgumentOutOfRangeException("rowPitch");
			}
#endif

			_data = data;
			_rowPitch = rowPitch;
		}
		#endregion

		#region ISubResourceData Members
		/// <summary>
		/// Property to return size, in bytes, of the data being passed to the sub resource.
		/// </summary>
		public int Size
		{
			get
			{
				if (_data == null)
				{
					return 0;
				}

				return (int)_data.Length;
			}
		}

		/// <summary>
		/// Property to return the number of bytes between each line in a texture.
		/// </summary>
		/// <remarks>This is only used for 2D and 3D textures and has no meaning elsewhere.</remarks>
		public int RowPitch
		{
			get 
			{
				return _rowPitch;
			}
		}

		/// <summary>
		/// Gets the slice pitch.
		/// </summary>
		int ISubResourceData.SlicePitch
		{
			get 
			{
				return Size;
			}
		}

		/// <summary>
		/// Property to return the data to pass to a resource (like a texture).
		/// </summary>
		public GorgonDataStream Data
		{
			get 
			{
				return _data;
			}
		}
		#endregion
	}

	/// <summary>
	/// This is used to initialize a 3 dimensional texture.
	/// </summary>
	public struct GorgonTexture3DData
		: ISubResourceData
	{
		#region Variables.
		private readonly GorgonDataStream _data;
		private readonly int _rowPitch;
		private readonly int _slicePitch;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture3DData"/> struct.
		/// </summary>
		/// <param name="data">The data used to initialize the texture.</param>
		/// <param name="rowPitch">The number of bytes between each row of the texture.</param>
		/// <param name="slicePitch">The number of bytes between each depth slice of the texture.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="rowPitch"/> or the <paramref name="slicePitch"/> parameters are less than or equal to 0.
		/// </exception>
		public GorgonTexture3DData(GorgonDataStream data, int rowPitch, int slicePitch)
		{
			GorgonDebug.AssertNull(data, "data");

#if DEBUG
			if (rowPitch <= 0)
			{
				throw new ArgumentOutOfRangeException("rowPitch");
			}

			if (slicePitch <= 0)
			{
				throw new ArgumentOutOfRangeException("slicePitch");
			}
#endif
			_data = data;
			_rowPitch = rowPitch;
			_slicePitch = slicePitch;
		}
		#endregion

		#region ISubResourceData Members
		/// <summary>
		/// Property to return size, in bytes, of the data being passed to the sub resource.
		/// </summary>
		public int Size
		{
			get 
			{
				if (_data == null)
				{
					return 0;
				}

				return (int)_data.Length;
			}
		}

		/// <summary>
		/// Property to return the number of bytes between each line in a texture.
		/// </summary>
		/// <remarks>This is only used for 2D and 3D textures and has no meaning elsewhere.</remarks>
		public int RowPitch
		{
			get 
			{
				return _rowPitch;
			}
		}

		/// <summary>
		/// Property to return the number of bytes between each depth slice in a 3D texture.
		/// </summary>
		/// <remarks>This is only used for 3D textures and has no meaning elsewhere.</remarks>
		public int SlicePitch
		{
			get 
			{
				return _slicePitch;
			}
		}

		/// <summary>
		/// Property to return the data to pass to a resource (like a texture).
		/// </summary>
		public GorgonDataStream Data
		{
			get 
			{
				return _data;
			}
		}
		#endregion
	}
}
