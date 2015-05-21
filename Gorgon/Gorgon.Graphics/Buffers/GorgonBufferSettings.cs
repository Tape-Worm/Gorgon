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
// Created: Wednesday, May 29, 2013 11:09:04 PM
// 
#endregion

using System;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Settings for a generic buffer.
	/// </summary>
	public class GorgonBufferSettings
		: IBufferSettings
	{
		/// <summary>
		/// Property to set or return the usage for the buffer.
		/// </summary>
		/// <para>The default value is Default.</para>
		public BufferUsage Usage
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to allow this buffer to be used for stream output.
		/// </summary>
		/// <remarks>
		/// The default value is FALSE.
		/// </remarks>
		public bool IsOutput
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of the buffer, in bytes.
		/// </summary>
		/// <remarks>
		/// This value must be a multiple of 16.
		/// </remarks>
		public int SizeInBytes
		{
			get;
			set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBufferSettings"/> class.
		/// </summary>
		public GorgonBufferSettings()
		{
			SizeInBytes = 0;
			IsOutput = false;
			Usage = BufferUsage.Default;
		    AllowUnorderedAccessViews = false;
		    AllowShaderViews = false;
            DefaultShaderViewFormat = BufferFormat.Unknown;
		    AllowRawViews = false;
		    AllowIndirectArguments = false;
		}

        /// <summary>
        /// Property to set or return whether to allow unordered access to the buffer.
        /// </summary>
        /// <remarks>
        /// This value must be set to FALSE if <see cref="IsOutput" /> is set to TRUE.
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        public bool AllowUnorderedAccessViews
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to allow shader resource views for this buffer.
        /// </summary>
        /// <remarks>
        /// This value does not apply to constant buffers.
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        public bool AllowShaderViews
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the format for the default shader view.
        /// </summary>
        /// <remarks>
        /// Setting this value to any other value than Unknown will create a default shader view for the buffer that will encompass the entire buffer with the specified format.
        /// <para>If <see cref="AllowRawViews" /> is set to TRUE, then this value should be set to one of: R32_Uint, R32_Int, R32_Float.</para>
        /// <para>This value does not apply to constant or structured buffers.</para>
        /// <para>The default value is Unknown.</para>
        /// </remarks>
        public BufferFormat DefaultShaderViewFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether a buffer will allow raw views.
        /// </summary>
        /// <remarks>
        /// This value must be set to FALSE if <see cref="AllowShaderViews" /> or <see cref="AllowUnorderedAccessViews" /> is set to FALSE.
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        public bool AllowRawViews
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether the buffer will be used as an indirect argument buffer.
        /// </summary>
        /// <remarks>
        /// This value does not apply to structured buffers or constant buffers.
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        public bool AllowIndirectArguments
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the size, in bytes, of an individual item in a structured buffer.
        /// </summary>
        /// <remarks>This value is only applicable to a structured buffer.  It will always return 0.</remarks>
        /// <exception cref="System.NotSupportedException">Thrown when an attempt to set a value to this property was made.</exception>
        int IBufferSettings.StructureSize
        {
            get
            {
                return 0;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
    }
}