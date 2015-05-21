#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, May 20, 2013 11:16:34 PM
// 
#endregion

namespace Gorgon.Graphics
{
	/// <summary>
	/// A generic buffer interface.
	/// </summary>
	/// <remarks>Buffers are a core resource object that can be used to store and/or transfer data to and from the GPU.
	/// <para>There are 5 buffer types in Gorgon:  Generic, Vertex, Index, Constant and Structured.  All of these buffer types, except generic, are used to convey specific types of information to and from the GPU. 
	/// <list type="table">
	/// <listheader>
	///     <term>Type</term>
	///     <description>Description</description>
	/// </listheader>
    /// <item>
    ///     <term>Generic</term>
    ///     <description>Holds arbitrary information.</description>
    /// </item>
    /// <item>
	///     <term>Vertex</term>
	///     <description>Holds vertex information for rendering geometry.</description>
	/// </item>
    /// <item>
    ///     <term>Index</term>
    ///     <description>Holds index information for organization of vertices.</description>
    /// </item>
    /// <item>
    ///     <term>Constant</term>
    ///     <description>Specialized buffers that hold information to be passed to a shader.  This allows sending of multiple values at once for more efficient usage.</description>
    /// </item>
    /// <item>
    ///     <term>Structured</term>
    ///     <description>A buffer containing highly structured data.</description>
    /// </item>
    /// </list>
	/// </para>
	/// <para>Generic buffers can only be created by SM4 or better devices.</para>
    /// <para>The generic buffer is intended to be used with the [RW]Buffer&lt;&gt; HLSL type.</para></remarks>
	public class GorgonBuffer
		: GorgonBaseBuffer
	{
		#region Properties.
        /// <summary>
        /// Property to return the type of buffer.
        /// </summary>
        public override BufferType BufferType
        {
            get
            {
                return BufferType.Generic;
            }
        }

        /// <summary>
        /// Property to return the settings for the buffer.
        /// </summary>
        public new GorgonBufferSettings Settings
        {
            get
            {
                return (GorgonBufferSettings)base.Settings;
            }
        }
		#endregion

		#region Methods.
        /// <summary>
        /// Function to retrieve an unordered access view for this buffer.
        /// </summary>
        /// <param name="format">Format of the buffer.</param>
        /// <param name="start">First element to map to the view.</param>
        /// <param name="count">The number of elements to map to the view.</param>
        /// <param name="useRaw">TRUE to use a raw view, FALSE to use a standard view.</param>
        /// <returns>An unordered access view for the buffer.</returns>
        /// <remarks>Use this to create/retrieve an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a <see cref="GetShaderView">Shader View</see>, only one 
        /// unordered access view can be bound to the pipeline at any given time.
        /// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
        /// </remarks>
		/// <exception cref="Gorgon.GorgonException">Thrown when the view could not be created or retrieved from the cache.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="start"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
        /// number of elements in the buffer.</exception>
        public virtual GorgonBufferUnorderedAccessView GetUnorderedAccessView(BufferFormat format, int start, int count, bool useRaw)
        {
            return OnGetUnorderedAccessView(format, start, count, useRaw, UnorderedAccessViewType.Standard);
        }

        /// <summary>
        /// Function to retrieve a new shader view for the buffer.
        /// </summary>
        /// <param name="format">The format of the view.</param>
        /// <param name="start">Starting element.</param>
        /// <param name="count">Element count.</param>
        /// <param name="useRaw">TRUE to use a raw view, FALSE to use a standard view.</param>
        /// <returns>A shader view for the buffer.</returns>
        /// <exception cref="Gorgon.GorgonException">Thrown when the view could not be created or retrieved from the cache.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="start"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
        /// number of elements in the buffer.</exception>
        /// <remarks>Use this to create/retrieve additional shader views for the buffer.  Multiple views of the same resource can be bound to multiple stages in the pipeline.
        /// <para>This function only applies to buffers that have not been created with a Usage of Staging.</para>
        /// </remarks>
        public virtual GorgonBufferShaderView GetShaderView(BufferFormat format, int start, int count, bool useRaw)
        {
            return OnGetShaderView(format, start, count, useRaw);
        }
        #endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBuffer" /> class.
		/// </summary>
		/// <param name="graphics">The graphics interface used to create this object.</param>
		/// <param name="name">Name of the buffer.</param>
		/// <param name="settings">Settings for the buffer.</param>
		internal GorgonBuffer(GorgonGraphics graphics, string name, IBufferSettings settings)
			: base(graphics, name, settings)
		{
		}
		#endregion
	}
}
