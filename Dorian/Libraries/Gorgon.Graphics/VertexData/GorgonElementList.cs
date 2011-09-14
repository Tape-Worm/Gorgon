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
// Created: Tuesday, September 13, 2011 9:05:52 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using GorgonLibrary.Math;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Size and type for a vertex element.
	/// </summary>
	public enum VertexElementFormat
	{
		Unknown = 0,
		/// <summary>
		/// Signed 8 bit byte.
		/// </summary>
		Byte = 1,
		/// <summary>
		/// Unsigned 8 bit byte.
		/// </summary>
		UByte = 2,
		/// <summary>
		/// Signed 16 bit integer.
		/// </summary>
		Int16 = 3,
		/// <summary>
		/// Unsigned 16 bit integer.
		/// </summary>
		UInt16 = 4,
		/// <summary>
		/// Signed 32 bit integer.
		/// </summary>
		Int32 = 5,
		/// <summary>
		/// Unsigned 32 bit integer.
		/// </summary>
		UInt32 = 6,
		/// <summary>
		/// Signed 64 bit integer.
		/// </summary>
		Int64 = 7,
		/// <summary>
		/// Unsigned 64 bit integer.
		/// </summary>
		UInt64 = 8,
		/// <summary>
		/// Signed 96 bit integer.
		/// </summary>
		Int96 = 9,
		/// <summary>
		/// Unsigned 96 bit integer.
		/// </summary>
		UInt96 = 10,
		/// <summary>
		/// Signed 128 bit integer.
		/// </summary>
		Int128 = 11,
		/// <summary>
		/// Unsigned 128 bit integer.
		/// </summary>
		UInt128 = 12,
		/// <summary>
		/// Floating point value.
		/// </summary>
		Float = 13,
		/// <summary>
		/// 2 floating point values.
		/// </summary>
		Float2 = 14,
		/// <summary>
		/// 3 floating point values.
		/// </summary>
		Float3 = 15,
		/// <summary>
		/// 4 floating point values.
		/// </summary>
		Float4 = 16
	}

	/// <summary>
	/// Used to describe the layout of a vertex.
	/// </summary>
	/// <remarks>This is a collection of vertex elements used to describe the layout of a vertex object.  The user can create this by hand using explicit element types, or 
	/// by passing the type of the vertex object to the <see cref="GorgonLibrary.Graphics.GorgonVertexElementList.GetFromType">GetFromType</see> method.
	/// <para>When using a type to determine the layout</para>
	/// </remarks>
	public class GorgonVertexElementList
		: IList<GorgonVertexElementList.VertexElement>
	{
		#region Classes.
		/// <summary>
		/// An element of a vertex.
		/// </summary>
		public class VertexElement
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
					case VertexElementFormat.Int32:
					case VertexElementFormat.UInt32:
						return sizeof(Int32);
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
			/// Initializes a new instance of the <see cref="VertexElement"/> class.
			/// </summary>
			/// <param name="context">The context.</param>
			/// <param name="format">The format.</param>
			/// <param name="offset">The offset.</param>
			/// <param name="index">The index.</param>
			/// <param name="slot">The slot.</param>
			/// <param name="instanced">if set to <c>true</c> [instanced].</param>
			/// <param name="instanceCount">The instance count.</param>
			internal VertexElement(string context, VertexElementFormat format, int offset, int index, int slot, bool instanced, int instanceCount)
			{
				Context = context;
				Index = index;
				Format = format;
				Offset = offset;
				Size = SizeOf(format);
			}
			#endregion
		}
		#endregion

		#region Variables.
		private List<VertexElement> _elements = null;		// List of elements.
		private bool _isUpdated = false;					// Flag to indicate that the vertex was updated.
		private Type[] _allowedTypes = null;				// Types allowed when pulling information from an object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the list has been updated.
		/// </summary>
		public bool IsUpdated
		{
			get
			{
				return _isUpdated;
			}
			protected set
			{
				_isUpdated = value;

				// Recalculate the size of the vertex.
				if (value)
					UpdateVertexSize();
			}
		}

		/// <summary>
		/// Property to return the vertex size in bytes.
		/// </summary>
		public int Size
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return an element by index.
		/// </summary>
		public GorgonVertexElementList.VertexElement this[int index]
		{
			get
			{
				return _elements[index];
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function used to determine the type of a field/property from its type.
		/// </summary>
		/// <param name="type">Type to use when evaluating.</param>
		/// <returns>The vertex element format.</returns>
		private VertexElementFormat GetElementType(Type type)
		{
			if (type == typeof(byte))
				return VertexElementFormat.Byte;
			if (type == typeof(SByte))
				return VertexElementFormat.UByte;

			if (type == typeof(Int32))
				return VertexElementFormat.Int32;
			if (type == typeof(UInt32))
				return VertexElementFormat.UInt32;

			if (type == typeof(Int16))
				return VertexElementFormat.Int16;
			if (type == typeof(UInt16))
				return VertexElementFormat.UInt16;

			if (type == typeof(Int64))
				return VertexElementFormat.Int64;
			if (type == typeof(UInt64))
				return VertexElementFormat.UInt64;

			if (type == typeof(float))
				return VertexElementFormat.Float;
			if (type == typeof(Vector2D))
				return VertexElementFormat.Float2;
			if (type == typeof(Vector3D))
				return VertexElementFormat.Float3;
			if (type == typeof(Vector4D))
				return VertexElementFormat.Float4;

			return VertexElementFormat.Unknown;
		}

		/// <summary>
		/// Function to retrieve the size, in bytes, of the entire vertex.
		/// </summary>
		private void UpdateVertexSize()
		{
			Size = _elements.Sum(item => item.Size);
		}

		/// <summary>
		/// Function to normalize the offsets in the element list.
		/// </summary>
		public void NormalizeOffsets()
		{
			int lastOffset = 0;

			foreach (var element in _elements)
			{
				element.Offset = lastOffset;
				lastOffset += element.Size;
			}

			IsUpdated = true;
		}

		/// <summary>
		/// Function to add a vertex element to the list.
		/// </summary>
		/// <param name="context">Context of the element.</param>
		/// <param name="format">Format of the element.</param>
		/// <param name="offset">Offset in bytes of the element.</param>
		/// <param name="index">Index of the element.</param>
		/// <param name="slot">Vertex buffer slot for the element.</param>
		/// <param name="instanced">TRUE if this element is instanced, FALSE if not.</param>
		/// <param name="instanceCount">If <paramref name="instanced"/> is TRUE, then the number of instances.  If FALSE, this parameter must be 0.</param>
		/// <returns>A new vertex element.</returns>
		/// <remarks>See the <see cref="GorgonLibrary.Graphics.GorgonVertexElementList.VertexElement">VertexElement</see> type for details on the various parameters.</remarks>
		/// <exception cref="System.ArgumentException">Thrown if the <paramref name="format"/> is not supported.
		/// <para>-or-</para>
		/// <para>Thrown if the <paramref name="offset"/> or <paramref name="context"/> is in use and the <paramref name="index"/> is the same.</para>
		/// <para>-or-</para>
		/// <para>Thrown is the <paramref name="slot"/> parameter is less than 0 or greater than 15.</para>
		/// </exception>
		public GorgonVertexElementList.VertexElement Add(string context, VertexElementFormat format, int? offset, int index, int slot, bool instanced, int instanceCount)
		{
			if (VertexElement.SizeOf(format) == 0)
				throw new ArgumentException("format", "'" + format.ToString() + "' is not a supported format.");

			if ((slot < 0) || (slot > 15))
				throw new ArgumentException("slot", "The value must be from 0 to 15.");

			// Find the highest offset and increment by the last vertex element size.
			if (offset == null) 
			{
				if (_elements.Count > 0)
				{
					var lastElement = (from elementItem in _elements
									   orderby elementItem.Offset descending
									   select elementItem).Single();

					offset = lastElement.Offset + lastElement.Size;
				}
				else
					offset = 0;		
			}

			if (_elements.Count(item => ((item.Offset == offset) || (string.Compare(item.Context, context, true) == 0)) && item.Index == index) > 0)
				throw new ArgumentException("context, offset", "The offset '" + offset.Value + "' or context '" + context + "' is in use by another element with the same index.");

			// Check for existing context at the same index.

			VertexElement element = new VertexElement(context, format, offset.Value, index, slot, instanced, (instanced ? instanceCount : 0));

			_elements.Add(element);

			IsUpdated = true;

			return element;
		}

		/// <summary>
		/// Function to add a vertex element to the list.
		/// </summary>
		/// <param name="context">Context of the element.</param>
		/// <param name="format">Format of the element.</param>
		/// <param name="offset">Offset in bytes of the element.</param>
		/// <param name="index">Index of the element.</param>
		/// <param name="slot">Vertex buffer slot for the element.</param>
		/// <returns>A new vertex element.</returns>
		/// <remarks>See the <see cref="GorgonLibrary.Graphics.GorgonVertexElementList.VertexElement">VertexElement</see> type for details on the various parameters.</remarks>
		/// <exception cref="System.ArgumentException">Thrown if the <paramref name="format"/> is not supported.
		/// <para>-or-</para>
		/// <para>Thrown if the <paramref name="offset"/> is in use and the <paramref name="index"/> is the same.</para>
		/// <para>-or-</para>
		/// <para>Thrown is the <paramref name="slot"/> parameter is less than 0 or greater than 15.</para>
		/// </exception>
		public GorgonVertexElementList.VertexElement Add(string context, VertexElementFormat format, int? offset, int index, int slot)
		{
			return Add(context, format, offset, index, slot, false, 0);
		}

		/// <summary>
		/// Function to add a vertex element to the list.
		/// </summary>
		/// <param name="context">Context of the element.</param>
		/// <param name="format">Format of the element.</param>
		/// <param name="offset">Offset in bytes of the element.</param>
		/// <param name="index">Index of the element.</param>
		/// <returns>A new vertex element.</returns>
		/// <remarks>See the <see cref="GorgonLibrary.Graphics.GorgonVertexElementList.VertexElement">VertexElement</see> type for details on the various parameters.</remarks>
		/// <exception cref="System.ArgumentException">Thrown if the <paramref name="format"/> is not supported.
		/// <para>-or-</para>
		/// <para>Thrown if the <paramref name="offset"/> is in use and the <paramref name="index"/> is the same.</para>
		/// </exception>
		public GorgonVertexElementList.VertexElement Add(string context, VertexElementFormat format, int? offset, int index)
		{
			return Add(context, format, offset, index, 0, false, 0);
		}

		/// <summary>
		/// Function to add a vertex element to the list.
		/// </summary>
		/// <param name="context">Context of the element.</param>
		/// <param name="format">Format of the element.</param>
		/// <param name="offset">Offset in bytes of the element.</param>
		/// <returns>A new vertex element.</returns>
		/// <remarks>See the <see cref="GorgonLibrary.Graphics.GorgonVertexElementList.VertexElement">VertexElement</see> type for details on the various parameters.</remarks>
		/// <exception cref="System.ArgumentException">Thrown if the <paramref name="format"/> is not supported.
		/// <para>-or-</para>
		/// <para>Thrown if the <paramref name="offset"/> is in use and the <paramref name="index"/> is the same.</para>
		/// </exception>
		public GorgonVertexElementList.VertexElement Add(string context, VertexElementFormat format, int? offset)
		{
			return Add(context, format, offset, 0, 0, false, 0);
		}

		/// <summary>
		/// Function to remove a vertex element by index.
		/// </summary>
		/// <param name="index">Index of the vertex element to remove.</param>
		public void Remove(int index)
		{
			_elements.RemoveAt(index);
			IsUpdated = true;
		}

		/// <summary>
		/// Function to retrieve the vertex layout from a specific type.
		/// </summary>
		/// <param name="type">Type of retrieve layout info from.</param>
		/// <remarks>Use this to create a vertex layout from a type.  Properties and fields in this type must be marked with the <see cref="GorgonLibrary.Graphics.VertexElementAttribute">VertexElementAttribute</see> in order for the element list to consider it and those fields or properties must be public.
		/// <para>Fields/properties marked with the attribute must be either a (u)byte, (u)short, (u)int, (u)long, float or one of the Vector2/3/4D types.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="type"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if a field/property type cannot be mapped to a <see cref="E:GorgonLibrary.Graphics.VertexElementFormat">VertexElementFormat</see>.</exception>
		public void GetFromType(Type type)
		{
			VertexElement element = null;
			IList<MemberInfo> members = type.GetMembers();
			int byteOffset = 0;

			if (type == null)
				throw new ArgumentNullException("type");

			// Get fields.
			_elements.Clear();

			// Get only properties and fields, and sort by explicit ordering (then by offset).
			var propertiesAndFields = from member in members
									  let memberAttribute = member.GetCustomAttributes(typeof(VertexElementAttribute), true) as IList<VertexElementAttribute>
									  where ((member.MemberType == MemberTypes.Property) || (member.MemberType == MemberTypes.Field)) && ((memberAttribute != null) && (memberAttribute.Count > 0))
									  orderby memberAttribute[0].ExplicitOrder, memberAttribute[0].Offset
									  select new { Name = member.Name, ReturnType = (member is FieldInfo ? ((FieldInfo)member).FieldType : ((PropertyInfo)member).PropertyType), Attribute = memberAttribute[0] };

			foreach (var item in propertiesAndFields)
			{
				VertexElementFormat format = item.Attribute.Format;
				string contextName = item.Attribute.Context;

				// Try to determine the format from the type.
				if (format == VertexElementFormat.Unknown)
				{
					format = GetElementType(item.ReturnType);
					if (format == VertexElementFormat.Unknown)
						throw new GorgonException(GorgonResult.CannotCreate, "The type '" + item.ReturnType.FullName.ToString() + "' for the property/field '" + item.Name + "' cannot be mapped to a VertexElementFormat.");
				}

				// Determine the context name from the field name.
				if (string.IsNullOrEmpty(contextName))
					contextName = item.Name;

				element = Add(contextName, format, (item.Attribute.AutoOffset ? byteOffset : item.Attribute.Offset), item.Attribute.Index, item.Attribute.Slot, item.Attribute.Instanced, item.Attribute.InstanceCount);
				byteOffset += element.Size;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexElementList"/> class.
		/// </summary>
		public GorgonVertexElementList()
		{
			_allowedTypes = new [] {		
									typeof(SByte),
									typeof(byte),
									typeof(ushort),
									typeof(short),
									typeof(int),
									typeof(uint),
									typeof(long),
									typeof(ulong),
									typeof(float),
									typeof(Vector2D),
									typeof(Vector3D),
									typeof(Vector4D)
								};

			_elements = new List<VertexElement>(16);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonVertexElementList"/> class.
		/// </summary>
		/// <param name="type">The type to parse for the layout.</param>
		public GorgonVertexElementList(Type type)
			: this()
		{
			GetFromType(type);
		}
		#endregion

		#region IList<VertexElement> Members
		#region Properties.
		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <returns>
		/// The element at the specified index.
		///   </returns>
		///   
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The property is set and the <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
		///   </exception>
		GorgonVertexElementList.VertexElement IList<GorgonVertexElementList.VertexElement>.this[int index]
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
		/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		/// <returns>
		/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
		/// </returns>
		public int IndexOf(GorgonVertexElementList.VertexElement item)
		{
			return _elements.IndexOf(item);
		}

		/// <summary>
		/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
		/// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
		///   </exception>
		void IList<GorgonVertexElementList.VertexElement>.Insert(int index, GorgonVertexElementList.VertexElement item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		/// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.
		///   </exception>
		///   
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.
		///   </exception>
		void IList<GorgonVertexElementList.VertexElement>.RemoveAt(int index)
		{
			Remove(index);
		}
		#endregion
		#endregion

		#region ICollection<VertexElement> Members
		#region Properties.
		/// <summary>
		/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <returns>
		/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		///   </returns>
		public int Count
		{
			get 
			{
				return _elements.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </summary>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
		///   </returns>
		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		///   </exception>
		void ICollection<GorgonVertexElementList.VertexElement>.Add(GorgonVertexElementList.VertexElement item)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		///   </exception>
		public void Clear()
		{
			_elements.Clear();
			IsUpdated = true;
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
		/// </returns>
		bool ICollection<GorgonVertexElementList.VertexElement>.Contains(GorgonVertexElementList.VertexElement item)
		{
			return _elements.Contains(item);
		}

		/// <summary>
		/// Copies to.
		/// </summary>
		/// <param name="array">The array.</param>
		/// <param name="arrayIndex">Index of the array.</param>
		void ICollection<GorgonVertexElementList.VertexElement>.CopyTo(GorgonVertexElementList.VertexElement[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </summary>
		/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
		/// <returns>
		/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">
		/// The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		///   </exception>
		public bool Remove(GorgonVertexElementList.VertexElement item)
		{
			IsUpdated = true;
			return _elements.Remove(item);			
		}
		#endregion
		#endregion

		#region IEnumerable<VertexElement> Members
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<GorgonVertexElementList.VertexElement> GetEnumerator()
		{
			foreach (var element in _elements)
				yield return element;
		}
		#endregion

		#region IEnumerable Members
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		#endregion
	}
}
