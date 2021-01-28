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

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// An attribute to mark a field in a value type as an input element.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Apply this to a field in a value type to allow the <see cref="GorgonInputLayout.CreateUsingType{T}"/> method to parse the value type and build an input element element list from it.
    /// </para>
    /// <para>
    /// Using Unknown for the format will tell the library to try and figure out the type from the field/property.  This will only work on members that return the following types:
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
    /// If the type of the member is not on this list, then an exception will be thrown when the input layout is generated.
    /// </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
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
        }

        /// <summary>
        /// Property to return whether to use automatic calculation for the offset.
        /// </summary>
        internal bool AutoOffset
        {
            get;
        }

        /// <summary>
        /// Property to return the context of the element.
        /// </summary>
        /// <remarks>
        /// This is a string value that corresponds to a shader semantic.  For example, to specify a normal, the user would set this to "Normal".  With the exception of the position element (which must be 
        /// named "SV_Position"), these contexts can be any name as long as it maps to a corresponding vertex element in the shader.
        /// </remarks>
        public string Context
        {
            get;
        }

        /// <summary>
        /// Property to set or return the index of the context.
        /// </summary>
        /// <remarks>
        /// This is used to differentiate between elements with the same <see cref="Context"/>. For example, to define a 2nd set of texture coordinates, use the same <see cref="Context"/> for the element 
        /// and define this value as 1 in the constructor.
        /// </remarks>
        public int Index
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the format of the data.
        /// </summary>
        /// <remarks>
        /// This is used to specify the type of data for the element, and will also determine how many bytes the element will occupy.
        /// </remarks>
        public BufferFormat Format
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the vertex buffer slot this element will use.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Multiple vertex buffers can be used to identify parts of the same vertex.  This is used to minimize the amount of data being written to a vertex buffer and provide better performance.
        /// </para>
        /// <para>
        /// This value has a valid range of 0 to 15, inclusive.
        /// </para>
        /// </remarks>
        public int Slot
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether this data is instanced or per vertex.
        /// </summary>
        /// <remarks>
        /// Indicates that the element should be included in instancing.
        /// </remarks>
        public bool Instanced
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the offset of the element within the structure.
        /// </summary>
        /// <remarks>
        /// This is used to determine the order in which an element will appear after another element. For example, if the previous element has a format of <see cref="BufferFormat.R32G32B32A32_Float"/> and an offset of 0, 
        /// then this value needs to be set to 16. If this element were to use a format of <see cref="BufferFormat.R32G32_Float"/>, then the following element would have an offset of 16 + 8 (24).
        /// </remarks>
        public int Offset
        {
            get;
        }

        /// <summary>
        /// Property to set or return the number of instances to draw.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The number of instances to draw using the same per-instance data before moving to the next element.
        /// </para>
        /// <para>
        /// If the <see cref="Instanced"/> value is set to <b>false</b>, then this value will be set to 0.
        /// </para>
        /// </remarks>
        public int InstanceCount
        {
            get;
            set;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="InputElementAttribute"/> class.
        /// </summary>
        /// <param name="context">The context of the element.</param>
        /// <param name="format">The format/type of the element.</param>
        /// <param name="offset">Offset of the element in the structure.</param>
        public InputElementAttribute(string context, BufferFormat format, int offset)
        {

            Context = context;
            Format = format;
            Index = 0;
            Slot = 0;
            Instanced = false;
            InstanceCount = 0;
            Offset = offset;
            AutoOffset = false;
            ExplicitOrder = int.MaxValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputElementAttribute"/> class.
        /// </summary>
        /// <param name="fieldOrder">Explicit layout order of the field when being parsed from the type.</param>
        /// <param name="context">The context of the element.</param>
        /// <remarks>
        /// <para>
        /// Using this constructor will indicate that the ordering of the elements within the layout will be based on the <paramref name="fieldOrder"/> passed to this constructor. All offsets for the elements 
        /// will automaticaly be derived from the <see cref="Format"/>, if specified.  Otherwise, the type of the member will be used instead.
        /// </para>
        /// </remarks>
        public InputElementAttribute(int fieldOrder, string context)
            : this(context, BufferFormat.Unknown, 0)
        {
            ExplicitOrder = fieldOrder;
            Offset = 0;
            AutoOffset = true;
        }
        #endregion
    }
}
