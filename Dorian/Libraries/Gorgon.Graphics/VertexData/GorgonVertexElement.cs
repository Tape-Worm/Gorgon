#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Tuesday, September 13, 2011 7:17:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// An element of a vertex.
	/// </summary>
	public class GorgonVertexElement
	{
		#region Properties.
		/// <summary>
		/// Property to return the context of the element.
		/// </summary>
		/// <remarks>This is a string value that corresponds to a shader input.  For example, to specify a position, the user would set this to "position".  
		/// These contexts can be named whatever the user wishes.  However, some APIs (such as Direct3D 9) only have a set number of contexts and thus the
		/// user should pass in one of the pre-defined constants for the context.
		/// </remarks>
		public string Context
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the index of the context.
		/// </summary>
		/// <remarks>This is used to denote the same context but at another index.  For example, to specify a second set of texture coordinates, set this 
		/// to 1.</remarks>
		public int Index
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the format of the data.
		/// </summary>
		/// <remarks>This is used to specify the format and type of the element.</remarks>
		public VertexElementFormat Format
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the offset of this element compared to other elements.
		/// </summary>
		/// <remarks>The format of the data dictates the offset of the element.  This value is optional.</remarks>
		public int Offset
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the vertex buffer slot this element will use.
		/// </summary>
		/// <remarks>Multiple vertex buffers can be used to identify parts of the same vertex.  This is used to minimize the amount of data being written to a 
		/// vertex buffer and provide better performance.</remarks>
		public int Slot
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return whether this data is instanced or per vertex.
		/// </summary>
		/// <remarks>Indicates that the element should be included in instancing.</remarks>
		public bool Instanced
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the number of instances to draw.
		/// </summary>
		/// <remarks>The number of times this element should be used before moving to the next element.</remarks>
		public int InstanceCount
		{
			get;
			internal set;
		}

		/// <summary>
		/// Property to return the size in bytes of this element.
		/// </summary>
		public int Size
		{
			get;
			internal set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the size, in bytes, of a format.
		/// </summary>
		/// <param name="format">Format to compare.</param>
		/// <returns>The size in bytes of the format, or 0 if the format cannot be used or determined.</returns>
		public static int SizeOf(VertexElementFormat format)
		{
			switch (format)
			{
				case VertexElementFormat.Byte:
				case VertexElementFormat.UByte:
					return sizeof(byte);
				case VertexElementFormat.Int16:
				case VertexElementFormat.UInt16:
					return sizeof(Int16);
				case VertexElementFormat.Color:
				case VertexElementFormat.UByte4:
				case VertexElementFormat.UByte4Normal:
				case VertexElementFormat.Int32Normal:
				case VertexElementFormat.UInt32Normal:
				case VertexElementFormat.Int32:
				case VertexElementFormat.UInt32:
					return sizeof(Int32);
				case VertexElementFormat.Int64Normal:
				case VertexElementFormat.UInt64Normal:
				case VertexElementFormat.Int64:
				case VertexElementFormat.UInt64:
					return sizeof(Int64);
				case VertexElementFormat.Int96:
				case VertexElementFormat.UInt96:
					return sizeof(Int64) + sizeof(Int32);
				case VertexElementFormat.Int128:
				case VertexElementFormat.UInt128:
					return sizeof(Int64) * 2;
				case VertexElementFormat.Float:
					return sizeof(float);
				case VertexElementFormat.Float2:
					return sizeof(float) * 2;
				case VertexElementFormat.Float3:
					return sizeof(float) * 3;
				case VertexElementFormat.Float4:
					return sizeof(float) * 4;
				default:
					return 0;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexElement"/> class.
		/// </summary>
		/// <param name="context">The context for the element.</param>
		/// <param name="format">The format and type of the element.</param>
		/// <param name="offset">The offset of the element within the vertex.</param>
		/// <param name="index">The index of the element.</param>
		/// <param name="slot">The vertex buffer slot for the element.</param>
		/// <param name="instanced">TRUE if using instanced data, FALSE if not.</param>
		/// <param name="instanceCount">Number of instances to use before moving to the next element.</param>
		public GorgonVertexElement(string context, VertexElementFormat format, int offset, int index, int slot, bool instanced, int instanceCount)
		{
			Context = context;
			Index = index;
			Format = format;
			Offset = offset;
			Size = SizeOf(format);
		}
		#endregion
	}
}
