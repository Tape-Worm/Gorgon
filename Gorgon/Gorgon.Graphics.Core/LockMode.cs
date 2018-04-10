#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: April 10, 2018 9:03:44 AM
// 
#endregion

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines the available modes for mapping a dynamic resource from GPU to CPU address space.
    /// </summary>
    public enum LockMode
    {
        /// <summary>
        /// <para>
        /// No lock mode.
        /// </para>
        /// <para>
        /// Dynamic resources cannot use this value. If this value is passed, then <see cref="WriteDiscard"/> is substituted in its place.
        /// </para>
        /// </summary>
        None = 0,
        /// <summary>
        /// <para>
        /// Resource is mapped for reading. The resource must have been created with read access
        /// </para>
        /// </summary>
        Read = D3D11.MapMode.Read,
        /// <summary>
        /// <para>
        /// Resource is mapped for writing. The resource must have been created with write
        /// </para>
        /// </summary>
        Write = D3D11.MapMode.Write,
        /// <summary>
        /// <para>
        /// Resource is mapped for reading and writing. The resource must have been created with read and write
        /// </para>
        /// </summary>
        ReadWrite = D3D11.MapMode.ReadWrite,
        /// <summary>
        /// <para>
        /// Resource is mapped for writing; the previous contents of the resource will be undefined. The resource must have been created with write access and dynamic usage
        /// </para>
        /// </summary>
        WriteDiscard = D3D11.MapMode.WriteDiscard,
        /// <summary>
        /// <para>
        /// Resource is mapped for writing; the existing contents of the resource cannot be overwritten. The resource must have been created with write access.
        /// </para>
        /// </summary>
        WriteNoOverwrite = D3D11.MapMode.WriteNoOverwrite
    }
}
