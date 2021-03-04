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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Reflection;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines the layout of an input item within a buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This defines the layout of a piece of data within a buffer, specifically, a vertex within a vertex buffer. The layout is defined by a list of <see cref="GorgonInputElement"/> values that determine 
    /// how the data within the layout is arranged.
    /// </para>
    /// <para>
    /// Users may create a layout manually, or, derive it from a value type (<c>struct</c>). If deriving from a value type, then the members of the value type must be decorated with a 
    /// <see cref="InputElementAttribute"/> to define where the member is located within the layout data structure.
    /// </para>
    /// </remarks>
    public sealed class GorgonInputLayout
        : IGorgonNamedObject, IEquatable<GorgonInputLayout>, IDisposable
    {
        #region Variables.
        // Type mapping for types.
        private static readonly Dictionary<Type, BufferFormat> _typeMapping = new()
        {
                {
                    typeof(byte), BufferFormat.R8_UInt
                },
                {
                    typeof(sbyte), BufferFormat.R8_SInt
                },
                {
                    typeof(int), BufferFormat.R32_UInt
                },
                {
                    typeof(uint), BufferFormat.R32_UInt
                },
                {
                    typeof(short), BufferFormat.R16_UInt
                },
                {
                    typeof(ushort), BufferFormat.R16_UInt
                },
                {
                    typeof(long), BufferFormat.R32G32_SInt
                },
                {
                    typeof(ulong), BufferFormat.R32G32_UInt
                },
                {
                    typeof(float), BufferFormat.R32_Float
                },
                {
                    typeof(DX.Vector2), BufferFormat.R32G32_Float
                },
                {
                    typeof(DX.Vector3), BufferFormat.R32G32B32_Float
                },
                {
                    typeof(DX.Vector4), BufferFormat.R32G32B32A32_Float
                },
                {
                    typeof(GorgonColor), BufferFormat.R32G32B32A32_Float
                }
            };

        // Elements used to build the layout.
        private readonly GorgonInputElement[] _elements;
        // List of slot sizes.
        private Dictionary<int, int> _slotSizes;
        // The Direct 3D input layout.
        private D3D11.InputLayout _d3DInputLayout;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the Direct 3D input layout.
        /// </summary>
        internal D3D11.InputLayout D3DInputLayout => _d3DInputLayout;

        /// <summary>
        /// Property to return the graphics interface that owns the object.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }

        /// <summary>
        /// Property to return the shader that is bound with this input layout.
        /// </summary>
        public GorgonShader Shader
        {
            get;
        }

        /// <summary>
        /// Property to return the input object size in bytes.
        /// </summary>
        public int SizeInBytes
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the input elements for this layout.
        /// </summary>
        public IReadOnlyList<GorgonInputElement> Elements => _elements;

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        /// <remarks>
        /// For best practises, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this 
        /// property.
        /// </remarks>
        public string Name
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to convert this input layout into a Direct3D input layout.
        /// </summary>
        private void BuildD3DLayout()
        {
            // ReSharper disable once InconsistentNaming
            var d3dElements = new D3D11.InputElement[_elements.Length];

            for (int i = 0; i < _elements.Length; ++i)
            {
                d3dElements[i] = _elements[i].D3DInputElement;
            }

            _d3DInputLayout = new D3D11.InputLayout(Graphics.D3DDevice, Shader.D3DByteCode.Data, d3dElements)
            {
                DebugName = $"{Name} Direct 3D 11 Input Layout"
            };
        }

        /// <summary>
        /// Function to determine if an element already exists with the same context, index and slot.
        /// </summary>
        /// <param name="elements">The list of elements to compare against.</param>
        /// <param name="element">The element to search for.</param>
        /// <param name="index">The index of the current element.</param>
        /// <param name="parameterName">The name of the parameter being validated.</param>
        private static void FindDuplicateElements(IList<GorgonInputElement> elements, in GorgonInputElement element, int index, string parameterName)
        {
            for (int i = 0; i < elements.Count; ++i)
            {
                // Skip the element that we're testing against.
                if (index == i)
                {
                    continue;
                }

                GorgonInputElement currentElement = elements[i];

                if ((string.Equals(currentElement.Context, element.Context, StringComparison.OrdinalIgnoreCase))
                    && (currentElement.Index == element.Index) && (currentElement.Slot == element.Slot))
                {
                    throw new ArgumentException(
                        string.Format(Resources.GORGFX_ERR_LAYOUT_ELEMENT_IN_USE, element.Offset, element.Context), parameterName);
                }
            }
        }

        /// <summary>
        /// Function to retrieve the size, in bytes, of the input object.
        /// </summary>
        private void UpdateVertexSize()
        {
            int size = 0;
            _slotSizes = new Dictionary<int, int>();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < _elements.Length; ++i)
            {
                GorgonInputElement element = _elements[i];
                size += element.SizeInBytes;


                // Calculate the individual slot sizes.
                if (_slotSizes.TryGetValue(element.Slot, out int slotSize))
                {
                    slotSize += element.SizeInBytes;
                }
                else
                {
                    slotSize = element.SizeInBytes;
                }

                _slotSizes[element.Slot] = slotSize;
            }

            SizeInBytes = size;
        }

        /// <summary>
        /// Function to build a list of fields from the given type.
        /// </summary>
        /// <param name="type">The type to evaluate.</param>
        /// <returns>The list of field info values for the members of the type.</returns>
        internal static List<(FieldInfo Field, InputElementAttribute InputElement)> GetFieldInfoList(Type type)
        {
            FieldInfo[] members = type.GetFields();

            if (members.Length == 0)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_VERTEX_NO_FIELDS, type.FullName));
            }

            var result = new List<(FieldInfo, InputElementAttribute)>();

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < members.Length; ++i)
            {
                FieldInfo member = members[i];
                InputElementAttribute attribute = member.GetCustomAttribute<InputElementAttribute>();

                if (attribute is null)
                {
                    continue;
                }

                // If we have marshalled fields on here, then throw an exception. We don't support complex marshalling.
                if (!member.IsFieldSafeForNative())
                {
                    throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VERTEX_TYPE_NOT_VALID_FOR_NATIVE);
                }

                Type returnType = member.FieldType;

                if (!_typeMapping.ContainsKey(returnType))
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_LAYOUT_INVALID_ELEMENT_TYPE, returnType.FullName));
                }

                result.Add((member, attribute));
            }

            return result;
        }

        /// <summary>
        /// Function to build an input layout using the fields from a value type.
        /// </summary>
        /// <typeparam name="T">The type to evaluate. This must be an unmanaged value type.</typeparam>
        /// <param name="graphics">The graphics interface used to create the input layout.</param>
        /// <param name="shader">Vertex shader to bind the layout with.</param>
        /// <returns>A new <see cref="GorgonInputLayout"/> for the type passed to <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="shader"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when an element with the same context, slot and index appears more than once in the members of the <typeparamref name="T"/> type.</exception>
        /// <exception cref="GorgonException">Thrown when the type specified by <typeparamref name="T"/> is not safe for use with native functions (see <see cref="GorgonReflectionExtensions.IsFieldSafeForNative"/>).
        /// <para>-or-</para>
        /// <para>Thrown when the type specified by <typeparamref name="T"/> does not contain any public members.</para>
        /// <para>-or-</para>
        /// <para>Thrown if the type specified by <typeparamref name="T"/> does not have a <see cref="LayoutKind"/> of <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will build a new <see cref="GorgonInputLayout"/> using the fields within a value type (<c>struct</c>). Each of the members that are to be included in the layout must be decorated with a 
        /// <see cref="InputElementAttribute"/>. If a member is not decorated with this attribute, then it will be ignored.
        /// </para>
        /// <para>
        /// The type parameter <typeparamref name="T"/> must be an unmanaged value type (<c>struct</c>), reference types are not supported. The members of the type must also be public fields. Properties are not 
        /// supported. Futhermore, the struct must be decorated with a <see cref="StructLayoutAttribute"/> that defines a <see cref="LayoutKind"/> of <see cref="LayoutKind.Sequential"/> or 
        /// <see cref="LayoutKind.Explicit"/>. This is necessary to ensure that the member of the value type are in the correct order when writing to a <see cref="GorgonVertexBuffer"/> or when 
        /// generating a <see cref="GorgonInputLayout"/> from a type.
        /// </para>
        /// <para>
        /// If the type specified by <typeparamref name="T"/> has members that are not primitive types or value types with a <see cref="StructLayoutAttribute"/>, or the member has a 
        /// <see cref="MarshalAsAttribute"/>, then an exception is thrown.  Gorgon does not support marshalling of complex types for vertices.
        /// </para>
        /// <para>
        /// The types of the fields must be one of the following types:
        /// <para>
        /// <list type="bullet">
        ///		<item>
        ///			<description><see cref="byte"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="sbyte"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="short"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="ushort"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="int"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="uint"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="long"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="ulong"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="float"/></description>
        ///		</item>
        ///		<item>
        ///			<description><c>DX.Vector2</c></description>
        ///		</item>
        ///		<item>
        ///			<description><c>DX.Vector3</c></description>
        ///		</item>
        ///		<item>
        ///			<description><c>DX.Vector4</c></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="GorgonColor"/></description>
        ///		</item>
        /// </list>
        /// </para>
        /// If the type of the member does not match, an exception will be thrown.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonReflectionExtensions.IsFieldSafeForNative"/>
        public static GorgonInputLayout CreateUsingType<T>(GorgonGraphics graphics, GorgonVertexShader shader)
            where T : unmanaged => CreateUsingType(graphics, typeof(T), shader);

        /// <summary>
        /// Function to build an input layout using the fields from a value type.
        /// </summary>
        /// <param name="graphics">The graphics interface used to create the input layout.</param>
        /// <param name="type">The type to evaluate.</param>
        /// <param name="shader">Vertex shader to bind the layout with.</param>
        /// <returns>A new <see cref="GorgonInputLayout"/> for the type passed to <paramref name="type"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="type"/> or the <paramref name="shader"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when an element with the same context, slot and index appears more than once in the members of the <paramref name="type"/>.
        /// <para>-or-</para>
        /// <para>Thrown if the <paramref name="type"/> does not have a <see cref="LayoutKind"/> of <see cref="LayoutKind.Sequential"/> or <see cref="LayoutKind.Explicit"/>.</para>
        /// </exception>
        /// <exception cref="GorgonException">Thrown when the type specified by <paramref name="type"/> is not safe for use with native functions (see <see cref="GorgonReflectionExtensions.IsFieldSafeForNative"/>).
        /// <para>-or-</para>
        /// <para>Thrown when the type specified by <paramref name="type"/> does not contain any public members.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will build a new <see cref="GorgonInputLayout"/> using the fields within a value type (<c>struct</c>). Each of the members that are to be included in the layout must be decorated with a 
        /// <see cref="InputElementAttribute"/>. If a member is not decorated with this attribute, then it will be ignored.
        /// </para>
        /// <para>
        /// The <paramref name="type"/> parameter must be an unmanaged value type (<c>struct</c>), reference types are not supported. The members of the type must also be public fields. Properties are not 
        /// supported. Futhermore, the struct must be decorated with a <see cref="StructLayoutAttribute"/> that defines a <see cref="LayoutKind"/> of <see cref="LayoutKind.Sequential"/> or 
        /// <see cref="LayoutKind.Explicit"/>. This is necessary to ensure that the member of the value type are in the correct order when writing to a <see cref="GorgonVertexBuffer"/> or when 
        /// generating a <see cref="GorgonInputLayout"/> from a type.
        /// </para>
        /// <para>
        /// If the type specified by <paramref name="type"/> has members that are not primitive types or value types with a <see cref="StructLayoutAttribute"/>, or the member has a 
        /// <see cref="MarshalAsAttribute"/>, then an exception is thrown.  Gorgon does not support marshalling of complex types for vertices.
        /// </para>
        /// <para>
        /// The types of the fields must be one of the following types:
        /// <para>
        /// <list type="bullet">
        ///		<item>
        ///			<description><see cref="byte"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="sbyte"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="short"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="ushort"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="int"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="uint"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="long"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="ulong"/></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="float"/></description>
        ///		</item>
        ///		<item>
        ///			<description><c>DX.Vector2</c></description>
        ///		</item>
        ///		<item>
        ///			<description><c>DX.Vector3</c></description>
        ///		</item>
        ///		<item>
        ///			<description><c>DX.Vector4</c></description>
        ///		</item>
        ///		<item>
        ///			<description><see cref="GorgonColor"/></description>
        ///		</item>
        /// </list>
        /// </para>
        /// If the type of the member does not match, an exception will be thrown.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonReflectionExtensions.IsFieldSafeForNative"/>
        public static GorgonInputLayout CreateUsingType(GorgonGraphics graphics, Type type, GorgonVertexShader shader)
        {
            if (graphics is null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if ((type.IsAutoLayout) || ((!type.IsLayoutSequential) && (!type.IsExplicitLayout)))
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_LAYOUT_NOT_SEQUENTIAL_EXPLICIT, type.FullName));
            }

            if (shader is null)
            {
                throw new ArgumentNullException(nameof(shader));
            }

            int byteOffset = 0;
            List<(FieldInfo Field, InputElementAttribute InputElement)> members = GetFieldInfoList(type);

            if (members.Count == 0)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_VERTEX_NO_FIELDS, type.FullName));
            }

            var elements = new GorgonInputElement[members.Count];

            for (int i = 0; i < elements.Length; i++)
            {
                (FieldInfo Field, InputElementAttribute InputElement) = members[i];

                BufferFormat format = InputElement.Format;
                string contextName = InputElement.Context;

                // Try to determine the format from the type.
                if ((format == BufferFormat.Unknown) && (!_typeMapping.TryGetValue(Field.FieldType, out format)))
                {
                    throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_LAYOUT_INVALID_ELEMENT_TYPE, Field.FieldType.FullName));
                }

                var element = new GorgonInputElement(contextName, format,
                                                     (InputElement.AutoOffset ? byteOffset : InputElement.Offset),
                                                     InputElement.Index, InputElement.Slot, InputElement.Instanced,
                                                     InputElement.Instanced ? InputElement.InstanceCount : 0);

                FindDuplicateElements(elements, element, i, nameof(element));

                elements[i] = element;
                byteOffset += element.SizeInBytes;
            }

            return new GorgonInputLayout(graphics, type.Name, shader, elements);
        }

        /// <summary>
        /// Function to convert this input layout into a <see cref="GorgonStreamOutLayout"/>
        /// </summary>
        /// <param name="stream">[Optional] The output stream to use.</param>
        /// <param name="slot">[Optional] The associated stream output buffer that is bound to the pipeline.</param>
        /// <returns>A new <see cref="GorgonStreamOutLayout"/> derived from this input layout.</returns>
        /// <remarks>
        /// <para>
        /// When streaming data out from the GPU, the layout of that data must be defined as a <see cref="GorgonStreamOutLayout"/>, which is quite similar to an input layout. For convenience, this method 
        /// will create a new <see cref="GorgonStreamOutLayout"/> that uses the same semantics as the input layout.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonStreamOutLayout"/>
	    public GorgonStreamOutLayout ToStreamOutLayout(int stream = 0, byte slot = 0)
        {
            var elements = new GorgonStreamOutElement[_elements.Length];

            for (int i = 0; i < _elements.Length; ++i)
            {
                // Only allow 64 elements.
                if (i > 64)
                {
                    break;
                }

                GorgonInputElement inputElement = _elements[i];

                var info = new GorgonFormatInfo(inputElement.Format);

                elements[i] = new GorgonStreamOutElement(inputElement.Context, 0, (byte)info.ComponentCount, slot, inputElement.Index, stream);
            }

            return new GorgonStreamOutLayout(Name + " (SO)", elements);
        }

        /// <summary>
        /// Function to normalize the offsets in the element list.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is used to rebuild the offsets for the elements assigned to this layout. Ensure that this input layout is not bound to the pipeline before normalizing. Otherwise, an invalid binding will 
        /// occur. 
        /// </para>
        /// <para>
        /// This will use the order of the elements as they appear in the list passed to this object to determine the offsets.
        /// </para>
        /// </remarks>
        public void NormalizeOffsets()
        {
            int lastOffset = 0;

            var elements = new D3D11.InputElement[_elements.Length];

            for (int i = 0; i < _elements.Length - 1; i++)
            {
                GorgonInputElement oldElement = _elements[i];
                _elements[i] = new GorgonInputElement(oldElement.Context,
                                                      oldElement.Format,
                                                      lastOffset,
                                                      oldElement.Index,
                                                      oldElement.Slot,
                                                      oldElement.Instanced,
                                                      oldElement.InstanceCount);
                lastOffset += oldElement.SizeInBytes;
                elements[i] = _elements[i].D3DInputElement;
            }

            BuildD3DLayout();
        }

        /// <summary>
        /// Property to return the size, in bytes, of the elements for a given slot.
        /// </summary>
        /// <param name="slot">The slot index assigned to the elements.</param>
        /// <returns>The size of the elements in the slot, in bytes.</returns>
        public int GetSlotSize(int slot)
        {
            if ((_slotSizes is not null) && (_slotSizes.Count != 0))
            {
                return _slotSizes[slot];
            }

            UpdateVertexSize();

            Debug.Assert(_slotSizes is not null, "_slotSizes is not null");

            return _slotSizes[slot];
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="layout" /> parameter; otherwise, false.</returns>
        /// <param name="layout">An object to compare with this object.</param>
        public bool Equals(GorgonInputLayout layout)
        {
            if (layout?._elements.Length != _elements.Length)
            {
                return false;
            }

            for (int i = 0; i < _elements.Length; ++i)
            {
                if (!GorgonInputElement.Equals(in _elements[i], in layout._elements[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="obj" /> parameter; otherwise, false.</returns>
        /// <param name="obj">An object to compare with this object.</param>
        public override bool Equals(object obj) => Equals(obj as GorgonInputLayout);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            D3D11.InputLayout layout = Interlocked.Exchange(ref _d3DInputLayout, null);
            layout?.Dispose();

            this.UnregisterDisposable(Graphics);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonInputLayout"/> class.
        /// </summary>
        /// <param name="graphics">The video adapter interface used to create this input layout.</param>
        /// <param name="name">Name of the object.</param>
        /// <param name="shader">Vertex shader to bind the layout with.</param>
        /// <param name="elements">The input elements to assign to this layout.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, <paramref name="graphics"/>, <paramref name="shader"/>, or the <paramref name="elements"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/>, or the <paramref name="elements"/> parameter is empty.</exception>
        /// <exception cref="ArgumentException">Thrown when an element with the same context, slot and index appears more than once in the <paramref name="elements"/> parameter.</exception>
        public GorgonInputLayout(GorgonGraphics graphics, string name, GorgonVertexShader shader, IEnumerable<GorgonInputElement> elements)
        {
            if (name is null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            Name = name;

            // Make a copy so we don't allow changing of the original reference.
            _elements = elements?.ToArray() ?? throw new ArgumentNullException(nameof(elements));

            if (_elements.Length == 0)
            {
                throw new ArgumentEmptyException(nameof(elements));
            }

            _slotSizes = new Dictionary<int, int>();

            // Check for duplicated elements.
            for (int i = 0; i < _elements.Length; ++i)
            {
                FindDuplicateElements(_elements, _elements[i], i, nameof(elements));
            }

            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            Shader = shader ?? throw new ArgumentNullException(nameof(shader));

            UpdateVertexSize();
            BuildD3DLayout();

            this.RegisterDisposable(graphics);
        }
        #endregion
    }
}
