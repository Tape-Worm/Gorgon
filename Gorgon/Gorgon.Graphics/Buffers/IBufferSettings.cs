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
// Created: Sunday, May 12, 2013 11:50:20 PM
// 
#endregion

namespace Gorgon.Graphics
{
    /// <summary>
    /// Settings for a buffer.
    /// </summary>
    public interface IBufferSettings
    {
        /// <summary>
        /// Property to set or return the usage for the buffer.
        /// </summary>
        /// <remarks>The default value is Default.</remarks>
        BufferUsage Usage
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to allow unordered access to the buffer.
        /// </summary>
        /// <remarks>
        /// This value must be set to FALSE if <see cref="IsOutput"/> is set to TRUE.
        /// <para>Unordered access views require a video device with SM5 capabilities.</para>
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        bool AllowUnorderedAccessViews
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
        bool AllowShaderViews
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the format for the default shader view.
        /// </summary>
        /// <remarks> 
        /// Setting this value to any other value than Unknown will create a default shader view for the buffer that will encompass the entire buffer with the specified format.
        /// <para>This value does not apply to constant or structured buffers.</para>
        /// <para>The default value is Unknown.</para>
        /// </remarks>
        BufferFormat DefaultShaderViewFormat
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether a buffer will allow raw views.
        /// </summary>
        /// <remarks>This value must be set to FALSE if <see cref="AllowShaderViews"/> or <see cref="AllowUnorderedAccessViews"/> is set to FALSE.
        /// <para>This value does not apply to structured buffers.</para>
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        bool AllowRawViews
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
        bool AllowIndirectArguments
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to allow this buffer to be used for stream output.
        /// </summary>
        /// <remarks>
        /// This value must be set to FALSE if <see cref="AllowUnorderedAccessViews"/> is set to TRUE.
        /// <para>The default value is FALSE.</para>
        /// </remarks>
        bool IsOutput
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the size of the buffer, in bytes.
        /// </summary>
        /// <remarks>
        /// For a <see cref="Gorgon.Graphics.GorgonConstantBuffer">constant buffer</see>, this value must be a multiple of 16.
        /// <para>The default value is 0.</para>
        /// </remarks>
        int SizeInBytes
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the size, in bytes, of an individual item in a structured buffer.
        /// </summary>
        /// <remarks>This value is only applicable to a structured buffer.  All other buffer types will return 0 for this setting.
        /// <para>This value must be between 1 and 2048 and a multiple of 4.</para>
        /// <para>The default value is 0.</para>
        /// </remarks>
        int StructureSize
        {
            get;
            set;
        }
    }
}
