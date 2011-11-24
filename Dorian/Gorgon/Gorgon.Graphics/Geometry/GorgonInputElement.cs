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
// Created: Wednesday, November 23, 2011 9:54:30 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// An input element for a buffer.
	/// </summary>
	/// <remarks>This defines the layout of an item of data for a buffer.  Typically this is used with a Vertex buffer to define a specific element for a vertex.</remarks>
	public class GorgonInputElement
		: INamedObject
	{
		#region Properties.
		/// <summary>
		/// Property to return the context of the element.
		/// </summary>
		/// <remarks>This is a string value that corresponds to a shader input.  For example, to specify a position, the user would set this to "position".  
		/// These contexts can be named whatever the user wishes.  This must map to a corresponding element in the shader.
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
		public GorgonBufferFormat Format
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

		#region Method.
		/// <summary>
		/// Function to convert this Gorgon input element into a Direct3D input element.
		/// </summary>
		/// <returns>The direct 3D input element.</returns>
		internal D3D.InputElement Convert()
		{
			int instanceCount = InstanceCount;

			if (!Instanced)
				instanceCount = 0;

			return new D3D.InputElement(Context, Index, (SharpDX.DXGI.Format)Format, Offset, Slot, (Instanced ? D3D.InputClassification.PerInstanceData : D3D.InputClassification.PerVertexData), instanceCount);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputElement"/> class.
		/// </summary>
		/// <param name="context">The context for the element.</param>
		/// <param name="format">The format and type of the element.</param>
		/// <param name="offset">The offset of the element within the vertex.</param>
		/// <param name="index">The index of the element.</param>
		/// <param name="slot">The vertex buffer slot for the element.</param>
		/// <param name="instanced">TRUE if using instanced data, FALSE if not.</param>
		/// <param name="instanceCount">Number of instances to use before moving to the next element.</param>
		public GorgonInputElement(string context, GorgonBufferFormat format, int offset, int index, int slot, bool instanced, int instanceCount)
		{
			Context = context;
			Index = index;
			Format = format;
			Offset = offset;
			Size = GorgonBufferFormatInfo.GetInfo(format).SizeInBytes;
			Slot = slot;
			Instanced = instanced;
			InstanceCount = InstanceCount;
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		string INamedObject.Name
		{
			get 
			{
				return Context;
			}
		}
		#endregion
	}
}
