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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DX = SharpDX;
using GorgonLibrary.IO;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// This is used to initialize a 1 dimensional texture.
	/// </summary>
	public struct GorgonTexture1DData
		: ISubResourceData
	{
		#region Variables.
		private int _rowPitch;
		private GorgonDataStream _data;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert Gorgon texture 3D data into DirectX data boxes.
		/// </summary>
		/// <param name="data">Data to convert.</param>
		/// <returns>An array of boxes.</returns>
		internal static DX.DataStream[] Convert(IEnumerable<ISubResourceData> data)
		{
			if ((data == null) || (data.Count() == 0))
				return null;

			DX.DataStream[] streams = new DX.DataStream[data.Count()];

			for (int i = 0; i < data.Count(); i++)
				streams[i] = new DX.DataStream(data.ElementAt(i).Data.PositionPointer, data.ElementAt(i).Size, true, true);

			return streams;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTexture2DData"/> struct.
		/// </summary>
		/// <param name="data">Data to pass to the texture.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="data"/> parameter is NULL (Nothing in VB.Net).</exception>
		public GorgonTexture1DData(GorgonDataStream data)
		{
			GorgonDebug.AssertNull<GorgonDataStream>(data, "data");
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
					return 0;

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
		private int _rowPitch;
		private GorgonDataStream _data;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert Gorgon texture 2D data into DirectX data rectangles.
		/// </summary>
		/// <param name="data">Data to convert.</param>
		/// <returns>An array of rectangles.</returns>
		internal static DX.DataRectangle[] Convert(IEnumerable<ISubResourceData> data)
		{
			if ((data == null) || (data.Count() == 0))
				return null;

			DX.DataRectangle[] rectangles = new DX.DataRectangle[data.Count()];

			for (int i = 0; i < data.Count(); i++)
				rectangles[i] = new DX.DataRectangle(data.ElementAt(i).Data.PositionPointer, data.ElementAt(i).RowPitch);
			
			return rectangles;
		}
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
			GorgonDebug.AssertNull<GorgonDataStream>(data, "data");

			if (rowPitch <= 0)
				throw new ArgumentOutOfRangeException("rowPitch", "Number must be greater than 0.");

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
					return 0;

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
		private GorgonDataStream _data;
		private int _rowPitch;
		private int _slicePitch;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert Gorgon texture 3D data into DirectX data boxes.
		/// </summary>
		/// <param name="data">Data to convert.</param>
		/// <returns>An array of boxes.</returns>
		internal static DX.DataBox[] Convert(IEnumerable<ISubResourceData> data)
		{
			if ((data == null) || (data.Count() == 0))
				return null;

			DX.DataBox[] boxes = new DX.DataBox[data.Count()];

			for (int i = 0; i < data.Count(); i++)
				boxes[i] = new DX.DataBox(data.ElementAt(i).Data.PositionPointer, data.ElementAt(i).RowPitch, data.ElementAt(i).SlicePitch);

			return boxes;
		}
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
			GorgonDebug.AssertNull<GorgonDataStream>(data, "data");

			if (rowPitch <= 0)
				throw new ArgumentOutOfRangeException("rowPitch", "Number must be greater than 0.");
			if (slicePitch <= 0)
				throw new ArgumentOutOfRangeException("slicePitch", "Number must be greater than 0.");

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
					return 0;

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
