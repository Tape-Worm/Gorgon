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
// Created: April 18, 2018 10:52:38 PM
// 
#endregion

using System;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Provides the necessary information required to set up a index buffer.
    /// </summary>
    public class GorgonIndexBufferInfo
        : IGorgonIndexBufferInfo
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the intended usage for binding to the GPU.
        /// </summary>
        public ResourceUsage Usage
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the number of indices to store.
        /// </summary>
        /// <remarks>
        /// This value should be larger than 0, or else an exception will be thrown when the buffer is created.
        /// </remarks>
        public int IndexCount
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return whether to use 16 bit values for indices.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Specifying 16 bit indices might improve performance.
        /// </para>
        /// <para>
        /// The default value is <b>true</b>.
        /// </para>
        /// </remarks>
        public bool Use16BitIndices
        {
            get;
            set;
        }


        /// <summary>
        /// Property to return the binding used to bind this buffer to the GPU.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="VertexIndexBufferBinding.None"/>.
        /// </remarks>
        public VertexIndexBufferBinding Binding
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        public string Name
        {
            get;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonIndexBufferInfo"/> class.
        /// </summary>
        /// <param name="info">A <see cref="IGorgonIndexBufferInfo"/> to copy settings from.</param>
        /// <param name="newName">[Optional] The new name for the buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        public GorgonIndexBufferInfo(IGorgonIndexBufferInfo info, string newName = null)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            Name = string.IsNullOrEmpty(newName) ? info.Name : newName;
            Usage = info.Usage;
            Use16BitIndices = info.Use16BitIndices;
            IndexCount = info.IndexCount;
            Binding = info.Binding;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonIndexBufferInfo"/> class.
        /// </summary>
        /// <param name="name">[Optional] The name of the buffer.</param>
        public GorgonIndexBufferInfo(string name = null)
        {
            Name = string.IsNullOrEmpty(name) ? GorgonGraphicsResource.GenerateName(GorgonIndexBuffer.NamePrefix) : name;
            Usage = ResourceUsage.Default;
            Use16BitIndices = true;
        }
        #endregion
    }
}
