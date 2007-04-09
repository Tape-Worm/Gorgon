#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Tuesday, July 19, 2005 10:51:34 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using SharpUtilities.Collections;
using D3D = Microsoft.DirectX.Direct3D;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Vertex declaration system.
	/// From here we can set up our custom vertices.
	/// </summary>
	/// <remarks>
	/// Not all models are created equal.  Some will contain position data, and color information, some
	/// will contain texture coordinates, normals, etc...  This will allow us to manage different types
	/// of vertices so that we can efficiently use mesh data.
	/// </remarks>
	public class VertexType
		: IDisposable
	{
		#region Variables.
		private bool _changed;						// Flag to indicate that the vertex has changed.
		private bool _sizeChanged;					// Flag to indicate that the size has changed.
		private int _vertexSize;					// Size of the vertex in bytes.
		private D3D.VertexDeclaration _declaration;	// Direct 3D vertex declaration.
		private List<VertexField> _fields;			// List of vertex fields.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to create and return the Direct 3D vertex declaration.
		/// </summary>
		internal D3D.VertexDeclaration D3DVertexDeclaration
		{
			get
			{
				int i;				// Loop.

				// Only modify if needed.
				if (NeedsUpdate)
				{
					if (_declaration != null)
					{
						_declaration.Dispose();
						_declaration = null;
					}

					// Create the D3D elements array.
					D3D.VertexElement[] elements = new D3D.VertexElement[_fields.Count + 1];

					// Loop through and create the declaration.
					for (i = 0; i < _fields.Count; i++)
					{
						elements[i].DeclarationMethod = D3D.DeclarationMethod.Default;
						elements[i].Offset = _fields[i].Offset;
						elements[i].Stream = _fields[i].Stream;
						elements[i].DeclarationType = Converter.Convert(_fields[i].Type);
						elements[i].DeclarationUsage = Converter.Convert(_fields[i].Context);

						switch (_fields[i].Context)
						{
							case VertexFieldContext.Diffuse:
								elements[i].UsageIndex = 0;
								break;
							case VertexFieldContext.Specular:
								elements[i].UsageIndex = 1;
								break;
							default:
								elements[i].UsageIndex = _fields[i].Index;
								break;
						}
					}

					// The last item in a D3D declaration has to be the Vertex End item.
					elements[_fields.Count] = D3D.VertexElement.VertexDeclarationEnd;

					// Create the declaration.
					_declaration = new D3D.VertexDeclaration(Gorgon.Screen.Device, elements);

					_changed = false;
					_sizeChanged = true;
				}

				return _declaration;
			}
		}

		/// <summary>
		/// Property to return whether this type has changed or not.
		/// </summary>
		public bool NeedsUpdate
		{
			get
			{
				return _changed;
			}
		}

		/// <summary>
		/// Property to return the number of fields in the list.
		/// </summary>
		public int Count
		{
			get
			{
				return _fields.Count;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a field to this vertex.
		/// </summary>
		/// <param name="stream">Stream to bind this field with.</param>
		/// <param name="fieldOffset">Offset of this field within the vertex type..</param>
		/// <param name="context">Context of this field.</param>
		/// <param name="fieldType">Data type of the field.</param>
		public void CreateField(short stream,short fieldOffset, VertexFieldContext context, VertexFieldType fieldType)
		{
			CreateField(stream,fieldOffset,context,fieldType,0);
		}

		/// <summary>
		/// Function to add a field to this vertex.
		/// </summary>
		/// <param name="stream">Stream to bind this field with.</param>
		/// <param name="fieldOffset">Offset of this field within the vertex type..</param>
		/// <param name="context">Context of this field.</param>
		/// <param name="fieldType">Data type of the field.</param>
		/// <param name="index">Index of the field, only required for certain types.</param>
		public void CreateField(short stream,short fieldOffset, VertexFieldContext context, VertexFieldType fieldType, byte index)
		{
			VertexField newField;	// A new vertex field.

			newField = new VertexField(stream,fieldOffset, context, fieldType, index);
			_fields.Add(newField);

			_changed = true;
			_sizeChanged = true;
		}

		/// <summary>
		/// Function to remove a field from the vertex.
		/// </summary>
		/// <param name="index">Index of the field to remove.</param>
		public void Remove(int index)
		{
			_fields.RemoveAt(index);
			_changed = true;
			_sizeChanged = true;
		}

		/// <summary>
		/// Function to remove a field from the vertex.
		/// </summary>
		/// <param name="stream">Stream to which the field is bound.</param>
		/// <param name="context">Context of the field we wish to remove.</param>
		public void Remove(short stream, VertexFieldContext context)
		{
			int i;					// loop.
			for (i=_fields.Count-1;i>=0;i--) 
			{
				if ((_fields[i].Context == context) && (_fields[i].Stream == stream))
					Remove(i);
			}			
		}

		/// <summary>
		/// Function to update a vertex _fields[i].
		/// </summary>
		/// <param name="stream">Stream to which the field is bound.</param>
		/// <param name="fieldIndex">Index of the field to update</param>
		/// <param name="fieldOffset">Offset within the buffer.</param>
		/// <param name="context">Context of this field.</param>
		/// <param name="fieldType">Data type of the field.</param>
		/// <param name="index">Index of the field, only required for certain types.</param>
		public void UpdateField(short stream,int fieldIndex,short fieldOffset, VertexFieldContext context, VertexFieldType fieldType, byte index)
		{
			if ((fieldIndex < 0) || (fieldIndex >= _fields.Count))
				throw new IndexOutOfBoundsException(index);

			_fields[fieldIndex] = new VertexField(stream,fieldOffset, context, fieldType, index);

			_changed = true;
			_sizeChanged = true;
		}

		/// <summary>
		/// Function to update a vertex _fields[i].
		/// </summary>
		/// <param name="stream">Stream to which the field is bound.</param>
		/// <param name="fieldIndex">Index of the field to update</param>
		/// <param name="fieldOffset">Offset within the buffer.</param>
		/// <param name="context">Context of this field.</param>
		/// <param name="fieldType">Data type of the field.</param>
		public void UpdateField(short stream,int fieldIndex,short fieldOffset, VertexFieldContext context, VertexFieldType fieldType)
		{
			UpdateField(stream,fieldIndex,fieldOffset,context,fieldType,0);
		}


		/// <summary>
		/// Function to insert a vertex field at a specified index.
		/// </summary>
		/// <param name="stream">Stream to which the field will be bound.</param>
		/// <param name="fieldIndex">Index after which to insert.</param>
		/// <param name="fieldOffset">Offset of the field in the buffer.</param>
		/// <param name="context">Context of this field.</param>
		/// <param name="fieldType">Data type of this field.</param>
		public void InsertField(short stream,int fieldIndex,short fieldOffset, VertexFieldContext context, VertexFieldType fieldType)
		{
			InsertField(stream,fieldIndex,fieldOffset,context,fieldType,0);
		}

		/// <summary>
		/// Function to insert a vertex field at a specified index.
		/// </summary>
		/// <param name="stream">Stream to which the field will be bound.</param>
		/// <param name="fieldIndex">Index after which to insert.</param>
		/// <param name="fieldOffset">Offset of the field in the buffer.</param>
		/// <param name="context">Context of this field.</param>
		/// <param name="fieldType">Data type of this field.</param>
		/// <param name="index">Index of the vertex field, required for certain fields.</param>
		public void InsertField(short stream,int fieldIndex,short fieldOffset, VertexFieldContext context, VertexFieldType fieldType,byte index)
		{
			VertexField newField;	// New _fields[i].

			newField = new VertexField(stream,fieldOffset,context,fieldType,index);
			_fields.Insert(fieldIndex,newField);

			_changed = true;
			_sizeChanged = true;
		}

		/// <summary>
		/// Function to retrieve whether this vertex definition contains a field of a particular context type.
		/// </summary>
		/// <param name="stream">Stream to check.</param>
		/// <param name="context">Context to check for.</param>
		/// <returns>TRUE if context exists, FALSE if not.</returns>
		public bool HasFieldContext(short stream,VertexFieldContext context)
		{
			int i;	// loop.

            for (i=0;i<_fields.Count;i++) 
			{
				if ((_fields[i].Context == context) && (_fields[i].Stream == stream))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Function to retrieve whether this vertex definition contains a field of a particular context type.
		/// </summary>
		/// <param name="context">Context to check for.</param>
		/// <returns>TRUE if context exists, FALSE if not.</returns>
		public bool HasFieldContext(VertexFieldContext context)
		{
			int i;	// loop.

			for (i=0;i<_fields.Count;i++) 
			{
				if (_fields[i].Context == context)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Function to retrieve whether this vertex definition has a particular field type.
		/// </summary>
		/// <param name="stream">Stream to check.</param>
		/// <param name="fieldType">Type of data to check for.</param>
		/// <returns>TRUE if the type is present, FALSE if not.</returns>
		public bool HasFieldType(short stream,VertexFieldType fieldType)
		{
			int i;					// loop.

            for (i=0;i<_fields.Count;i++) 
			{
				if ((_fields[i].Type == fieldType) && (_fields[i].Stream == stream))
					return true;
			}

			return false;
		}

		/// <summary>
		/// Function to return the size of the vertex in bytes, _fieldsd on stream.
		/// </summary>
		/// <param name="stream">Stream index to check.</param>
		/// <returns>Size of the vertex in bytes.</returns>
		public int VertexSize(short stream)
		{
			int i;				// Loop.

			// Loop through and get sizes.
			if (_sizeChanged)
			{
				_vertexSize = 0;
				for (i = 0; i < _fields.Count; i++)
				{
					if (_fields[i].Stream == stream)
						_vertexSize += _fields[i].Bytes;
				}
				_sizeChanged = false;
			}

			return _vertexSize;
		}

		/// <summary>
		/// Function to invalidate the declaration.
		/// </summary>
		public void Invalidate()
		{
			_changed = true;
			_sizeChanged = true;
		}

		/// <summary>
		/// Function to clone this vertex type.
		/// </summary>
		/// <returns>A copy of this vertex type.</returns>
		public VertexType Clone()
		{
			VertexType newVertexType = new VertexType();	// New vertex type.

			// Copy.
			for (int i=0;i<_fields.Count;i++)
				newVertexType.CreateField(_fields[i].Stream,_fields[i].Offset,_fields[i].Context,_fields[i].Type,_fields[i].Index);

			return newVertexType;
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"></see> to compare with the current <see cref="T:System.Object"></see>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"></see> is equal to the current <see cref="T:System.Object"></see>; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is VertexType)
			{
				VertexType comparer = (VertexType)obj;		// Comparison object.

				if (comparer.Count == Count)
				{
					// Compare fields.
					for (int i = 0; i < _fields.Count; i++)
					{
						if (!comparer.Contains(_fields[i]))
							return false;
					}

					return true;
				}
			}
			
			return false;
		}

		/// <summary>
		/// Property to return whether a field exists within this type or not.
		/// </summary>
		/// <param name="field">Field value.</param>
		/// <returns>TRUE if it exists, FALSE if not.</returns>
		public bool Contains(VertexField field)
		{
			return _fields.Contains(field);
		}

		/// <summary>
		/// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override int GetHashCode()
		{
			int code = 0;		// Hash code.

			for (int i = 0;i<_fields.Count;i++)
				code ^= _fields[i].GetHashCode();

			return code;
		}
		#endregion

		#region Operators.
/*		/// <summary>
		/// Operator to test two vertex types for equality.
		/// </summary>
		/// <param name="left">Left vertex type to compare.</param>
		/// <param name="right">Right vertex type to compare.</param>
		/// <returns>TRUE if left and right are equal, FALSE if not.</returns>
		public static bool operator ==(VertexType left, VertexType right)
		{
/*			if ((Equals(left, null)) && (!Equals(right,null)))
				return false;

			if ((Equals(right, null)) && (!Equals(left, null)))
				return false;

			return Equals(left, right);
		}

		/// <summary>
		/// Operator to test two vertex types for inequality.
		/// </summary>
		/// <param name="left">Left vertex type to compare.</param>
		/// <param name="right">Right vertex type to compare.</param>
		/// <returns>TRUE if left and right are not equal, FALSE if they are.</returns>
		public static bool operator !=(VertexType left, VertexType right)
		{
			return !(left == right);
		}*/
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a vertex field by its index.
		/// </summary>
		public VertexField this[int index]
		{
			get
			{
				return this[index,0];
			}
		}

		/// <summary>
		/// Property to return a vertex field by its context.
		/// </summary>
		public VertexField this[VertexFieldContext context]
		{
			get
			{
				return this[context,0];
			}
		}

		/// <summary>
		/// Property to return a vertex field by its index.
		/// </summary>
		public VertexField this[int index,int fieldindex]
		{
			get
			{
				VertexField field;		// Field in question

				for (int i=0;i<_fields.Count;i++)
				{
					field = _fields[i];
					if (field.Index == fieldindex)
						return field;
				}

				throw new IndexOutOfBoundsException(index);
			}
		}

		/// <summary>
		/// Property to return a vertex field by its context.
		/// </summary>
		public VertexField this[VertexFieldContext context, int fieldindex]
		{
			get
			{
				VertexField field;		// Field in question

				for (int i=0;i<_fields.Count;i++) 
				{
					field = _fields[i];
					if ((field.Context == context) && (field.Index == fieldindex))
						return field;
				}

				throw new InvalidVertexFieldContextException(null);
			}
		}
		#endregion

		#region Constructors and Destructors.
		/// <summary>
		/// Constructor.
		/// </summary>
		public VertexType()
		{			
			_fields = new List<VertexField>();
			_changed = true;
			_sizeChanged = true;
			_vertexSize = 0;
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~VertexType()
		{
			Dispose(false);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to dispose all resources, FALSE to only release unmanaged.</param>
		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_declaration != null)
					_declaration.Dispose();

				_fields.Clear();
				_fields = null;
				_declaration = null;
			}
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
