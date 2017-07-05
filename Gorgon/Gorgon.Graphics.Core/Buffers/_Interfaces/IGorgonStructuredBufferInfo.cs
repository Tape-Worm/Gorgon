#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 4, 2017 10:06:48 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The type of binding to use when binding to the GPU.
    /// </summary>
    [Flags]
    public enum StructuredBufferBinding
    {
        /// <summary>
        /// <para>
        /// No GPU access. This buffer will only be used on the CPU.
        /// </para>
        /// <para>
        /// This flag is mutally exclusive.
        /// </para>
        /// </summary>
        None = 0,
        /// <summary>
        /// The GPU will have access via the shaders as a shader resource.
        /// </summary>
        Shader = 1,
        /// <summary>
        /// <para>
        /// The GPU will have access via the shaders using unordered access.
        /// </para>
        /// <para>
        /// <b>TODO: Unordered access views are not implemented yet.</b>
        /// </para>
        /// </summary>
        UnorderedAccess = 2
    }

    /// <summary>
    /// Provides the necessary information required to set up a structured buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This provides an immutable view of the buffer information so that it cannot be modified after the buffer is created.
    /// </para>
    /// </remarks>
    public interface IGorgonStructuredBufferInfo
    {
        /// <summary>
        /// Property to return the intended usage for binding to the GPU.
        /// </summary>
        /// <remarks>
        /// The default value is <c>Default</c>.
        /// </remarks>
        D3D11.ResourceUsage Usage
        {
            get;
        }

        /// <summary>
        /// Property to return the size of the buffer, in bytes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value should be larger than 0, or else an exception will be thrown when the buffer is created.
        /// </para>
        /// </remarks>
        int SizeInBytes
        {
            get;
        }

        /// <summary>
        /// Property to return the type of binding for the GPU.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The type of binding should be used to determine what type of view to apply to the buffer when accessing it from shaders. This will also help determine how data will be interpreted.
        /// </para>
        /// <para>
        /// Different bindings may be applied at the same time by OR'ing the <see cref="StructuredBufferBinding"/> flags together.
        /// </para>
        /// <para>
        /// If the <see cref="Usage"/> is set to <c>Staging</c>, then this value must be set to <see cref="StructuredBufferBinding.None"/>, otherwise an exception will be raised when the buffer is created.
        /// </para>
        /// <para>
        /// The default value is <see cref="StructuredBufferBinding.Shader"/>
        /// </para>
        /// </remarks>
        StructuredBufferBinding Binding
        {
            get;
        }

        /// <summary>
        /// Property to set or return the size, in bytes, of an individual item in a structured buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value must be between 1 and 2048 and be a multiple of 4 or else an exception will be raised when the buffer is created.
        /// </para>
        /// <para>
        /// The default value is 0.
        /// </para>
        /// </remarks>
        int StructureSize
        {
            get;
            set;
        }
    }
}
