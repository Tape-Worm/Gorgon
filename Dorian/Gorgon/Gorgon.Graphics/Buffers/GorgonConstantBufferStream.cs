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
// Created: Sunday, January 08, 2012 10:29:03 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using DX = SharpDX;
using GorgonLibrary.Native;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A stream used to read/write data to a constant buffer.
	/// </summary>
	public class GorgonConstantBufferStream
		: GorgonDataStream
	{
		#region Variables.
		private bool _disposed = false;								// Flag to indicate that the buffer was disposed.
		private IDictionary<string, int> _offsets = null;			// Offsets of the variables in the constant buffer.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the base DirectX stream.
		/// </summary>
		internal DX.DataStream BaseStream
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the constant buffer that owns this stream.
		/// </summary>
		public GorgonConstantBuffer ConstantBuffer
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the offsets from the data type in the constant buffer.
		/// </summary>
		private void GetOffsets()
		{
			// Get members.
			_offsets = (from member in ConstantBuffer.DataType.GetMembers()
						let memberAttrib = member.GetCustomAttributes(typeof(FieldOffsetAttribute), false) as IList<FieldOffsetAttribute>
						where ((memberAttrib != null) && (memberAttrib.Count > 0) && ((member.MemberType == System.Reflection.MemberTypes.Field) || (member.MemberType == System.Reflection.MemberTypes.Property)))
						select new { member.Name, memberAttrib[0].Value }).ToDictionary(item => item.Name, item => item.Value);
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				if (disposing) 
				{
					if (BaseStream != null)
						BaseStream.Dispose();
				}

				BaseStream = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Function to write an item to a variable within the buffer.
		/// </summary>
		/// <param name="member">Name of the variable to write.</param>
		/// <param name="item">Item to write.</param>
		/// <typeparam name="T">Type of data to read/write.</typeparam>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="member"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the member parameter is an empty string.</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer hasn't been locked before writing.</exception>
		/// <remarks>This method will only write to a specific variable member in the constant buffer.</remarks>
		public void Write<T>(string member, T item)
			where T : struct
		{
			GorgonDebug.AssertParamString(member, "member");

			Position = _offsets[member];
			Write<T>(item);
		}

		/// <summary>
		/// Function to write an array of items to a variable within the buffer.
		/// </summary>
		/// <param name="member">Name of the variable to write.</param>
		/// <param name="item">Item to write.</param>
		/// <typeparam name="T">Type of data to read/write.</typeparam>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="member"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the member parameter is an empty string.</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer hasn't been locked before writing.</exception>
		/// <remarks>This method will only write to a specific variable member in the constant buffer.  Please note that bounds checking is not enabled on this, and consequently the array may exceed the length of the buffer.
		/// <para>Buffers must be locked with the <see cref="M:GorgonLibrary.Graphics.GorgonConstantBuffer.Lock">Lock</see> method before writing.</para>
		/// </remarks>
		public void WriteRange<T>(string member, T[] item)
			where T : struct
		{
			WriteRange<T>(member, item, 0, item.Length);
		}

		/// <summary>
		/// Function to write an array of items to a variable within the buffer.
		/// </summary>
		/// <param name="member">Name of the variable to write.</param>
		/// <param name="item">Item to write.</param>
		/// <param name="index">Index in the array to start at.</param>
		/// <param name="count">Number of items in the array to write.</param>
		/// <typeparam name="T">Type of data to read/write.</typeparam>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="member"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the member parameter is an empty string.</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer hasn't been locked before writing.</exception>
		/// <remarks>This method will only write to a specific variable member in the constant buffer.  Please note that bounds checking is not enabled on this, and consequently the array may exceed the length of the buffer.
		/// <para>Buffers must be locked with the <see cref="M:GorgonLibrary.Graphics.GorgonConstantBuffer.Lock">Lock</see> method before writing.</para>
		/// </remarks>
		public void WriteRange<T>(string member, T[] item, int index, int count)
			where T : struct
		{
			GorgonDebug.AssertNull<T[]>(item, "item");
			GorgonDebug.AssertParamString(member, "member");

			Position = _offsets[member];
			for (int i = index; i < count; i++)
				Write<T>(item[i]);			
		}

		/// <summary>
		/// Function to write a value to a variable within the buffer using marshalling.
		/// </summary>
		/// <param name="member">Name of the variable to write.</param>
		/// <param name="value">Value to marhsal and write.</param>
		/// <param name="deleteContents">TRUE to remove any pre-allocated data within the data, FALSE to leave alone.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="member"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the member parameter is an empty string.</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the buffer hasn't been locked before writing.</exception>
		/// <remarks>This method will only write to a specific variable member in the constant buffer.  Please note that bounds checking is not enabled on this, and consequently the array may exceed the length of the buffer.
		/// <para>This method should be used only when marshalling of a value is required.  This method is quite slow and should be used sparingly.</para>
		/// <para>Buffers must be locked with the <see cref="M:GorgonLibrary.Graphics.GorgonConstantBuffer.Lock">Lock</see> method before writing.</para>
		/// <para>Passing FALSE to <paramref name="deleteContents"/> may result in a memory leak if the data within the structure was previously initialized.</para>
		/// </remarks>
		public void WriteMarhsal<T>(string member, T value, bool deleteContents)
		{
			GorgonDebug.AssertParamString(member, "member");

			Position = _offsets[member];
			WriteMarshal<T>(value, deleteContents);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBufferStream"/> class.
		/// </summary>
		/// <param name="buffer">Buffer that owns this stream.</param>
		/// <param name="baseStream">Base DirectX stream.</param>
		internal GorgonConstantBufferStream(GorgonConstantBuffer buffer, DX.DataStream baseStream)
			: base(baseStream.DataPointer, (int)baseStream.Length)
		{			
			StreamStatus = Native.StreamStatus.WriteOnly;
			ConstantBuffer = buffer;
			BaseStream = baseStream;
			GetOffsets();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonConstantBufferStream"/> class.
		/// </summary>
		/// <param name="buffer">Buffer that owns this stream.</param>
		/// <param name="size">Size of the buffer, in bytes.</param>
		internal GorgonConstantBufferStream(GorgonConstantBuffer buffer, int size)
			: base(size)
		{
			StreamStatus = Native.StreamStatus.WriteOnly;
			ConstantBuffer = buffer;
			BaseStream = null;
			GetOffsets();
		}
		#endregion
	}
}
