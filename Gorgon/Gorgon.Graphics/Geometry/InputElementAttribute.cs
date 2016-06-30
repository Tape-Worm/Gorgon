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
// Created: Thursday, November 24, 2011 3:40:38 PM
// 
#endregion

using System;

namespace Gorgon.Graphics
{
	/// <summary>
	/// An attribute to mark a field in a structure as an input element.
	/// </summary>
	/// <remarks>Apply this to a field in a structure/object to allow the <see cref="Gorgon.Graphics.GorgonInputLayout">GorgonInputLayout</see> to parse the object and build
	/// an input element element list from it.
	/// <para>Using Unknown for the format will tell the library to try and figure out the type from the field/property.  Use this with caution, it will be very explicit 
	/// about the type it chooses.  This will only work on primitive types such as byte, (u)short, (u)int, float and double or the GorgonDX.Vector2/3/4D types.  Furthermore, it can only
	/// deduce the format from the type given, so expecting to use a float x,y,z will result in only x getting the attribute.  That is, it is not smart enough to deduce context.</para>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class InputElementAttribute
		: Attribute
	{
		#region Properties.
		/// <summary>
		/// Property to return the explicit order of the field.
		/// </summary>
		internal int ExplicitOrder
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether to use automatic calculation for the offset.
		/// </summary>
		internal bool AutoOffset
		{
			get;
			private set;
		}

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
			private set;
		}

		/// <summary>
		/// Property to return the index of the context.
		/// </summary>
		/// <remarks>This is used to denote the same context but at another index.  For example, to specify a second set of texture coordinates, set this 
		/// to 1.</remarks>
		public int Index
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the format of the data.
		/// </summary>
		/// <remarks>This is used to specify the format and type of the element.</remarks>
		public BufferFormat Format
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the vertex buffer slot this element will use.
		/// </summary>
		/// <remarks>Multiple vertex buffers can be used to identify parts of the same vertex.  This is used to minimize the amount of data being written to a 
		/// vertex buffer and provide better performance.</remarks>
		public int Slot
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether this data is instanced or per vertex.
		/// </summary>
		/// <remarks>Indicates that the element should be included in instancing.</remarks>
		public bool Instanced
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the offset of the element within the structure.
		/// </summary>
		public int Offset
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of instances to draw.
		/// </summary>
		/// <remarks>The number of times this element should be used before moving to the next element.</remarks>
		public int InstanceCount
		{
			get;
			private set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="InputElementAttribute"/> class.
		/// </summary>
		/// <param name="context">The context of the element.</param>
		/// <param name="format">The format/type of the element.</param>
		/// <param name="offset">Offset of the element in the structure.</param>
		/// <param name="index">The index for the element.</param>
		/// <param name="slot">The vertex buffer slot for the element.</param>
		/// <param name="instanced"><b>true</b> if used for instanced data, <b>false</b> if not.</param>
		/// <param name="instanceCount">The number of instances allowed.</param>
		public InputElementAttribute(string context, BufferFormat format, int offset, int index, int slot, bool instanced, int instanceCount)
		{
			if (slot < 0)
			{
				slot = 0;
			}
			if (slot > 15)
			{
				slot = 15;
			}

			if (!instanced)
			{
				instanceCount = 0;
			}

			Context = context;
			Format = format;
			Index = index;
			Slot = slot;
			Instanced = instanced;
			InstanceCount = instanceCount;
			Offset = offset;
			AutoOffset = false;
			ExplicitOrder = Int32.MaxValue;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InputElementAttribute"/> class.
		/// </summary>
		/// <param name="context">The context of the element.</param>
		/// <param name="format">The format/type of the element.</param>
		/// <param name="offset">Offset of the element in the structure.</param>
		/// <param name="index">The index for the element.</param>
		/// <param name="slot">The vertex buffer slot for the element.</param>
		public InputElementAttribute(string context, BufferFormat format, int offset, int index, int slot)
			: this(context, format, offset, index, slot, false, 0)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InputElementAttribute"/> class.
		/// </summary>
		/// <param name="context">The context of the element.</param>
		/// <param name="format">The format/type of the element.</param>
		/// <param name="offset">Offset of the element in the structure.</param>
		/// <param name="index">The index for the element.</param>
		public InputElementAttribute(string context, BufferFormat format, int offset, int index)
			: this(context, format, offset, index, 0, false, 0)
		{
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="InputElementAttribute"/> class.
		/// </summary>
		/// <param name="context">The context of the element.</param>
		/// <param name="format">The format/type of the element.</param>
		/// <param name="offset">Offset of the element in the structure.</param>
		public InputElementAttribute(string context, BufferFormat format, int offset)
			: this(context, format, offset, 0, 0, false, 0)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InputElementAttribute"/> class.
		/// </summary>
		/// <param name="fieldOrder">Explicit layout order of the field when being parsed from the type.</param>
		/// <param name="context">The context of the element.</param>
		/// <param name="format">The format/type of the element.</param>
		public InputElementAttribute(int fieldOrder, string context, BufferFormat format)
			: this(context, format, 0, 0, 0, false, 0)
		{
			ExplicitOrder = fieldOrder;
			Offset = 0;
			AutoOffset = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InputElementAttribute"/> class.
		/// </summary>
		/// <param name="fieldOrder">Explicit layout order of the field when being parsed from the type.</param>
		/// <param name="context">The context of the element.</param>
		public InputElementAttribute(int fieldOrder, string context)
			: this(context, BufferFormat.Unknown, 0, 0, 0, false, 0)
		{
			ExplicitOrder = fieldOrder;
			Offset = 0;
			AutoOffset = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="InputElementAttribute"/> class.
		/// </summary>
		/// <param name="fieldOrder">Explicit layout order of the field when being parsed from the type.</param>
		public InputElementAttribute(int fieldOrder)
			: this(string.Empty, BufferFormat.Unknown, 0, 0, 0, false, 0)
		{
			ExplicitOrder = fieldOrder;
			Offset = 0;
			AutoOffset = true;
		}
		#endregion
	}
}
