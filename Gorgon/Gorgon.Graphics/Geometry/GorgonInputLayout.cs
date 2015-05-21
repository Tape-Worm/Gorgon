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
using Gorgon.Core;
using SlimMath;
using D3D = SharpDX.Direct3D11;
using Gorgon.Graphics.Properties;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Defines the layout of an item in a buffer.
	/// </summary>
	/// <remarks>This is a collection of input elements used to describe the layout of an input object.  The user can create this by hand using explicit element types, or 
	/// by passing the type of .
	/// </remarks>
	public class GorgonInputLayout
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
	    private GorgonInputElement[] _elements;                 // Elements used to build the layout.
		private bool _disposed;									// Flag to indicate that the object was disposed.
		private IDictionary<int, int> _slotSizes;				// List of slot sizes.

		// Type mapping for types.
		private static readonly Dictionary<Type, BufferFormat> _typeMapping = new Dictionary<Type, BufferFormat>
			{
				{
					typeof(byte), BufferFormat.R8_UInt
				},
				{
					typeof(sbyte), BufferFormat.R8_Int
				},
				{
					typeof(Int32), BufferFormat.R32_Int
				},
				{
					typeof(UInt32), BufferFormat.R32_UInt
				},
				{
					typeof(Int16), BufferFormat.R16_Int
				},
				{
					typeof(UInt16), BufferFormat.R16_Int
				},
				{
					typeof(Int64), BufferFormat.R32G32_Int
				},
				{
					typeof(UInt64), BufferFormat.R32G32_UInt
				},
				{
					typeof(float), BufferFormat.R32_Float
				},
				{
					typeof(Vector2), BufferFormat.R32G32_Float
				},
				{
					typeof(Vector3), BufferFormat.R32G32B32_Float
				},
				{
					typeof(Vector4), BufferFormat.R32G32B32A32_Float
				},
				{
					typeof(GorgonColor), BufferFormat.R32G32B32A32_Float
				},
			};
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
        /// Function to determine if an element already exists with the same context, index and slot.
        /// </summary>
        /// <param name="element"></param>
        private void FindDuplicateElements(GorgonInputElement element)
        {
            if (_elements.Any(
		            elementItem =>
		            !string.IsNullOrWhiteSpace(elementItem.Context) &&
		            ((element.Offset == elementItem.Offset) ||
		             (string.Equals(element.Context, elementItem.Context, StringComparison.OrdinalIgnoreCase))) &&
		            element.Index == elementItem.Index && element.Slot == elementItem.Slot))
            {
				throw new ArgumentException(
					string.Format(Resources.GORGFX_LAYOUT_ELEMENT_IN_USE, element.Offset, element.Context), "element");
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
        /// Function to retrieve the input layout from a specific type.
        /// </summary>
        /// <param name="type">Type of retrieve layout info from.</param>
        /// <remarks>Use this to create an input element layout from a type.  Properties and fields in this type must be marked with the <see cref="Gorgon.Graphics.InputElementAttribute">GorgonInputElementAttribute</see> in order for the element list to consider it and those fields or properties must be public.
        /// <para>Fields/properties marked with the attribute must be either a (u)byte, (u)short, (u)int, (u)long, float or one of the Vector2/3/4D types.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="type"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="GorgonException">Thrown if a field/property type cannot be mapped to a <see cref="Gorgon.Graphics.BufferFormat">GorgonBufferFormat</see>.</exception>
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

                // Try to determine the format from the type.
                if (format == BufferFormat.Unknown)
                {
					if (!_typeMapping.ContainsKey(item.ReturnType))
					{
						throw new GorgonException(GorgonResult.CannotCreate,
												  string.Format(Resources.GORGFX_LAYOUT_INVALID_ELEMENT_TYPE,
																item.ReturnType.FullName));
					}

					format = _typeMapping[item.ReturnType];
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

			UpdateVertexSize();
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

			UpdateVertexSize();
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

			// If the shader that's linked to this layout is gone, then don't bother with a new layout object.
	        if (Shader.D3DByteCode == null)
	        {
		        return null;
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
		    if ((_slotSizes != null) && (_slotSizes.Count != 0))
		    {
		        return _slotSizes[slot];
		    }

		    UpdateVertexSize();

		    Debug.Assert(_slotSizes != null, "_slotSizes != null");

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
            : base(name)
		{
			Graphics = graphics;
			Shader = shader;
		    GorgonRenderStatistics.InputLayoutCount++;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
			    if (D3DLayout != null)
			    {
			        D3DLayout.Dispose();
			    }

                Graphics.RemoveTrackedObject(this);
                GorgonRenderStatistics.InputLayoutCount--;
			}

			D3DLayout = null;
			_disposed = true;
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
	}
}
