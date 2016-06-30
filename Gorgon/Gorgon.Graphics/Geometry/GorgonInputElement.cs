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
using Gorgon.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Properties;
using SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// An input element for a buffer.
	/// </summary>
	/// <remarks>This defines the layout of an item of data for a buffer.  Typically this is used with a Vertex buffer to define a specific element for a vertex.</remarks>
	public struct GorgonInputElement
		: IGorgonNamedObject, IEquatable<GorgonInputElement>
	{
		#region Variables.
		/// <summary>
		/// The context of the element.
		/// </summary>
		/// <remarks>This is a string value that corresponds to a shader input.  For example, to specify a position, the user would set this to "position".  
		/// These contexts can be named whatever the user wishes.  This must map to a corresponding element in the shader.
		/// </remarks>
		public readonly string Context;
		/// <summary>
		/// The index of the context.
		/// </summary>
		/// <remarks>This is used to denote the same context but at another index.  For example, to specify a second set of texture coordinates, set this 
		/// to 1.</remarks>
		public readonly int Index;
		/// <summary>
		/// The format of the data.
		/// </summary>
		/// <remarks>This is used to specify the format and type of the element.</remarks>
		public readonly BufferFormat Format;
		/// <summary>
		/// The offset of this element compared to other elements.
		/// </summary>
		/// <remarks>The format of the data dictates the offset of the element.  This value is optional.</remarks>
		public readonly int Offset;
		/// <summary>
		/// The vertex buffer slot this element will use.
		/// </summary>
		/// <remarks>Multiple vertex buffers can be used to identify parts of the same vertex.  This is used to minimize the amount of data being written to a 
		/// vertex buffer and provide better performance.</remarks>
		public readonly int Slot;
		/// <summary>
		/// Indicates whether this data is instanced or per vertex.
		/// </summary>
		/// <remarks>Indicates that the element should be included in instancing.</remarks>
		public readonly bool Instanced;
		/// <summary>
		/// The number of instances to draw.
		/// </summary>
		/// <remarks>The number of times this element should be used before moving to the next element.</remarks>
		public readonly int InstanceCount;
		/// <summary>
		/// Property to return the size in bytes of this element.
		/// </summary>
		public readonly int Size;
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

			return new D3D.InputElement(Context, Index, (Format)Format, Offset, Slot,
			                            (Instanced
				                             ? D3D.InputClassification.PerInstanceData
				                             : D3D.InputClassification.PerVertexData), instanceCount);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <b>true</b> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonInputElement)
			{
				return Equals((GorgonInputElement)obj);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">The first instance to compare.</param>
		/// <param name="right">The second instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonInputElement left, ref GorgonInputElement right)
		{
			return (string.Equals(left.Context, right.Context)) && (left.Format == right.Format) && (left.Index == right.Index) &&
					(left.InstanceCount == right.InstanceCount) && (left.Instanced == right.Instanced) && (left.Offset == right.Offset) &&
					(left.Size == right.Size) && (left.Slot == right.Slot);
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(GorgonInputElement left, GorgonInputElement right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(GorgonInputElement left, GorgonInputElement right)
		{
			return !Equals(ref left, ref right);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return
				281.GenerateHash(Context.GetHashCode())
				   .GenerateHash(Index)
				   .GenerateHash(Format)
				   .GenerateHash(Offset)
				   .GenerateHash(Slot)
				   .GenerateHash(Instanced.GetHashCode())
				   .GenerateHash(InstanceCount)
				   .GenerateHash(Size);
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
		/// <param name="instanced"><b>true</b> if using instanced data, <b>false</b> if not.</param>
		/// <param name="instanceCount">Number of instances to use before moving to the next element.</param>
		/// <remarks>The slot value must be between 0 and 15 (inclusive).  A value outside of this range will cause an exception to be thrown.</remarks>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="format"/> parameter is not supported.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> parameter is less than 0 or greater than 15.</exception>
		public GorgonInputElement(string context, BufferFormat format, int offset, int index, int slot, bool instanced, int instanceCount)
		{
			var formatInfo = new GorgonFormatInfo((Format)format);

            if (formatInfo.BitDepth == 0)
            {
	            throw new ArgumentException(string.Format(Resources.GORGFX_FORMAT_NOT_SUPPORTED, format), nameof(format));
            }

            if ((slot < 0) || (slot > 15))
            {
				throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_VALUE_OUT_OF_RANGE, slot, 16));
            }
            
            Context = context;
			Index = index;
			Format = format;
			Offset = offset;
			Size = formatInfo.SizeInBytes;
			Slot = slot;
			Instanced = instanced;
			InstanceCount = instanceCount;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputElement"/> struct.
		/// </summary>
		/// <param name="source">The source element.</param>
		/// <param name="offset">The offset of the element.</param>
		internal GorgonInputElement(GorgonInputElement source, int offset)
			: this(source.Context, source.Format, offset, source.Index, source.Slot, source.Instanced, source.InstanceCount)
		{
		}
		#endregion

		#region IGorgonNamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		string IGorgonNamedObject.Name => Context;

		#endregion

		#region IEquatable<GorgonInputElement> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the other parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonInputElement other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
