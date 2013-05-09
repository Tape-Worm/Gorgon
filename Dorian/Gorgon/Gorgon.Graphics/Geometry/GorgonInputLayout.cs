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
// Created: Thursday, November 24, 2011 3:38:16 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using GorgonLibrary.Collections;
using GorgonLibrary.Diagnostics;
using SlimMath;
using D3D = SharpDX.Direct3D11;

#error This is currently being refactored.

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Defines the layout of an item in a buffer.
	/// </summary>
	/// <remarks>This is a collection of input elements used to describe the layout of an input object.  The user can create this by hand using explicit element types, or 
	/// by passing the type of the input object to the <see cref="GorgonLibrary.Graphics.GorgonInputLayout.InitializeFromType">GetLayoutFromType</see> method.
	/// </remarks>
	public class GorgonInputLayout
		: IDisposable, INamedObject 
	{
		#region Variables.
	    private GorgonInputElement[] _elements;                 // Elements used to build the layout.
		private bool _disposed;									// Flag to indicate that the object was disposed.
		private IDictionary<int, int> _slotSizes;				// List of slot sizes.
		private readonly Type[] _allowedTypes;					// Types allowed when pulling information from an object.
		private readonly string _name = string.Empty;			// Name of the object.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct3D input layout.
		/// </summary>
		internal D3D.InputLayout D3DLayout
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the graphics interface that created this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the shader that is bound with this input layout.
		/// </summary>
		public GorgonShader Shader
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the input object size in bytes.
		/// </summary>
		public int Size
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to return the input elements for this layout.
        /// </summary>
        public IEnumerable<GorgonInputElement> Elements
        {
            get
            {
                return _elements;
            }
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function used to determine the type of a field/property from its type.
		/// </summary>
		/// <param name="type">Type to use when evaluating.</param>
		/// <returns>The element format.</returns>
		private static BufferFormat GetElementType(Type type)
		{
			if (type == typeof(byte))
			{
				return BufferFormat.R8_UInt;
			}

			if (type == typeof(SByte))
			{
				return BufferFormat.R8_Int;
			}

			if (type == typeof(Int32))
			{
				return BufferFormat.R32_Int;
			}

			if (type == typeof(UInt32))
			{
				return BufferFormat.R32_UInt;
			}

			if (type == typeof(Int16))
			{
				return BufferFormat.R16_Int;
			}

			if (type == typeof(UInt16))
			{
				return BufferFormat.R16_UInt;
			}

			if (type == typeof(Int64))
			{
				return BufferFormat.R32G32_Int;
			}

			if (type == typeof(UInt64))
			{
				return BufferFormat.R32G32_UInt;
			}

			if (type == typeof(float))
			{
				return BufferFormat.R32_Float;
			}

			if (type == typeof(Vector2))
			{
				return BufferFormat.R32G32_Float;
			}

			if (type == typeof(Vector3))
			{
				return BufferFormat.R32G32B32_Float;
			}

			if (type == typeof(Vector4))
			{
				return BufferFormat.R32G32B32A32_Float;
			}

			return type == typeof(GorgonColor) ? BufferFormat.R32G32B32A32_Float : BufferFormat.Unknown;
		}		

        /// <summary>
        /// Function to determine if an element already exists with the same context, index and slot.
        /// </summary>
        /// <param name="element"></param>
        private void FindDuplicateElements(GorgonInputElement element)
        {
            if (
                _elements.Any(
                    elementItem =>
                    ((element.Offset == elementItem.Offset) ||
                     (String.Compare(element.Context, elementItem.Context, StringComparison.OrdinalIgnoreCase) == 0)) && element.Index == elementItem.Index &&
                    element.Slot == elementItem.Slot))
            {
                throw new ArgumentException(
                    "The offset '" + element.Offset + "' or context '" + element.Context +
                    "' is in use by another item with the same index or slot.", "element");
            }
        }

		/// <summary>
		/// Function to retrieve the size, in bytes, of the input object.
		/// </summary>
		private void UpdateVertexSize()
		{
			Size = _elements.Sum(item => item.Size);

		    _slotSizes = (from slot in _elements
		                  group slot by slot.Slot).ToDictionary(key => key.Key, value => value.Sum(item => item.Size));
		}

        /// <summary>
        /// Function to assign an input element to the list.
        /// </summary>
        /// <param name="elementIndex">Index of the element.</param>
        /// <param name="context">Context of the element.</param>
        /// <param name="format">Format of the element.</param>
        /// <param name="offset">Offset in bytes of the element.</param>
        /// <param name="index">Index of the element.</param>
        /// <param name="slot">Vertex buffer slot for the element.</param>
        /// <param name="instanced">TRUE if this element is instanced, FALSE if not.</param>
        /// <param name="instanceCount">If <paramref name="instanced"/> is TRUE, then the number of instances.  If FALSE, this parameter must be 0.</param>
        /// <returns>The size of the element, in bytes.</returns>
        private int SetElement(int elementIndex, string context, BufferFormat format, int? offset, int index, int slot, bool instanced, int instanceCount)
        {
            // Find the highest offset and increment by the last element size.
            if (offset == null)
            {
                if (_elements.Length > 0)
                {
                    var lastElement = (from elementItem in _elements
                                       orderby elementItem.Offset descending
                                       select elementItem).Single();

                    offset = lastElement.Offset + lastElement.Size;
                }
                else
                {
                    offset = 0;
                }
            }

            var element = new GorgonInputElement(context, format, offset.Value, index, slot, instanced,
                                                 (instanced ? instanceCount : 0));

            FindDuplicateElements(element);

            _elements[elementIndex] = element;

            return element.Size;
        }

        /// <summary>
        /// Function to retrieve the input layout from a specific type.
        /// </summary>
        /// <param name="type">Type of retrieve layout info from.</param>
        /// <remarks>Use this to create an input element layout from a type.  Properties and fields in this type must be marked with the <see cref="GorgonLibrary.Graphics.InputElementAttribute">GorgonInputElementAttribute</see> in order for the element list to consider it and those fields or properties must be public.
        /// <para>Fields/properties marked with the attribute must be either a (u)byte, (u)short, (u)int, (u)long, float or one of the Vector2/3/4D types.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="type"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown if a field/property type cannot be mapped to a <see cref="GorgonLibrary.Graphics.BufferFormat">GorgonBufferFormat</see>.</exception>
        internal void InitializeFromType(Type type)
		{
			IList<MemberInfo> members = type.GetMembers();
			int byteOffset = 0;

		    if (type == null)
		    {
		        throw new ArgumentNullException("type");
		    }

		    // Get only properties and fields, and sort by explicit ordering (then by offset).
		    var propertiesAndFields = (from member in members
		                              let memberAttribute =
		                                  member.GetCustomAttributes(typeof(InputElementAttribute), true) as
		                                  IList<InputElementAttribute>
		                              where
		                                  ((member.MemberType == MemberTypes.Property) ||
		                                   (member.MemberType == MemberTypes.Field)) &&
		                                  ((memberAttribute != null) && (memberAttribute.Count > 0))
		                              orderby memberAttribute[0].ExplicitOrder, memberAttribute[0].Offset
		                              select new
		                                  {
		                                      member.Name,
		                                      ReturnType =
		                                  (member is FieldInfo
		                                       ? ((FieldInfo)member).FieldType
		                                       : ((PropertyInfo)member).PropertyType),
		                                      Attribute = memberAttribute[0]
		                                  }).ToArray();

            _elements = new GorgonInputElement[propertiesAndFields.Length];

            for (int i = 0; i < _elements.Length; i++)
            {
                var item = propertiesAndFields[i];

                BufferFormat format = item.Attribute.Format;
                string contextName = item.Attribute.Context;

                if (!_allowedTypes.Contains(item.ReturnType))
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                              "The type '" + item.ReturnType.FullName +
                                              "' is not a valid type for an input element.");
                }

                // Try to determine the format from the type.
                if (format == BufferFormat.Unknown)
                {
                    format = GetElementType(item.ReturnType);
                    if (format == BufferFormat.Unknown)
                    {
                        throw new GorgonException(GorgonResult.CannotCreate,
                                                  "The type '" + item.ReturnType.FullName + "' for the property/field '" +
                                                  item.Name + "' cannot be mapped to a GorgonBufferFormat.");
                    }
                }

                // Determine the context name from the field name.
                if (string.IsNullOrEmpty(contextName))
                {
                    contextName = item.Name;
                }

                var element = new GorgonInputElement(contextName, format,
                                                     (item.Attribute.AutoOffset ? byteOffset : item.Attribute.Offset),
                                                     item.Attribute.Index, item.Attribute.Slot, item.Attribute.Instanced,
                                                     item.Attribute.InstanceCount);

                FindDuplicateElements(element);

                _elements[i] = element;
                byteOffset += element.Size;
            }
		}

        /// <summary>
        /// Function to initialize the elements from a list of elements.
        /// </summary>
        /// <param name="elements">Elements used to initialize the layout.</param>
        internal void InitializeFromList(IList<GorgonInputElement> elements)
        {
            _elements = new GorgonInputElement[elements.Count];

            // Copy the list into our internal element list.
            for (int i = 0; i < _elements.Length; i++)
            {
                var element = elements[i];

                FindDuplicateElements(element);

                _elements[i] = element;
            }
        }
        
        /// <summary>
		/// Function to convert this input layout into a Direct3D input layout.
		/// </summary>
		/// <param name="device">Direct 3D device object.</param>
		/// <returns>The Direct 3D 11 input layout.</returns>
		internal D3D.InputLayout Convert(D3D.Device device)
		{
		    if (D3DLayout != null)
		    {
		        return D3DLayout;
		    }

		    var elements = new D3D.InputElement[_elements.Length];

		    for (int i = 0; i < elements.Length; i++)
		    {
		        elements[i] = _elements[i].Convert();
		    }

		    D3DLayout = new D3D.InputLayout(device, Shader.D3DByteCode, elements)
		        {
		            DebugName = "Gorgon Input Layout '" + Name + "'"
		        };

		    return D3DLayout;
		}

		/// <summary>
		/// Function to normalize the offsets in the element list.
		/// </summary>
		public void NormalizeOffsets()
		{
			int lastOffset = 0;

			for (int i = 0; i < _elements.Length - 1; i++)
			{
				_elements[i] = new GorgonInputElement(_elements[i], lastOffset);
				lastOffset += _elements[i].Size;
			}
		}

		/// <summary>
		/// Property to return the size of the elements for a given slot in an input element.
		/// </summary>
		/// <param name="slot">Slot to count.</param>
		/// <returns>The size of the elements in the slot, in bytes.</returns>
		public int GetSlotSize(int slot)
		{
		    if ((_slotSizes == null) || (_slotSizes.Count == 0))
		    {
		        UpdateVertexSize();

                Debug.Assert(_slotSizes != null, "_slotSizes != null");
		    }

		    return _slotSizes[slot];
		}

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputLayout"/> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that created this object.</param>
		/// <param name="name">Name of the object.</param>
		/// <param name="shader">Vertex shader to bind the layout with.</param>
		internal GorgonInputLayout(GorgonGraphics graphics, string name, GorgonShader shader)
		{
			_name = name;
			Graphics = graphics;
			Shader = shader;

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
									typeof(Vector2),
									typeof(Vector3),
									typeof(Vector4),
									typeof(GorgonColor)
								};
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (D3DLayout != null)
						D3DLayout.Dispose();

					Graphics.RemoveTrackedObject(this);
				}

				D3DLayout = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region INamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		public string Name
		{
			get 
			{
				return _name;
			}
		}
		#endregion
	}
}
