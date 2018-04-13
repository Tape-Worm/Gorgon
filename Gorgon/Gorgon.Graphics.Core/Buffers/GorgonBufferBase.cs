#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 9, 2016 3:54:15 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines the available modes for copying subresource data.
    /// </summary>
    public enum CopyMode
    {
        /// <summary>
        /// Data is copied into the buffer, overwriting existing data.
        /// </summary>
        None = 0,
        /// <summary>
        /// Data is copied into the buffer, but existing data cannot be overwritten.
        /// </summary>
        NoOverwrite = D3D11.CopyFlags.NoOverwrite,
        /// <summary>
        /// Data is copied into the buffer, but any existing data is discarded.
        /// </summary>
        Discard = D3D11.CopyFlags.Discard
    }

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

    /// <summary>
    /// A base class for buffers.
    /// </summary>
    public abstract class GorgonBufferBase
        : GorgonGraphicsResource
    {
        #region Variables.
        // A cache of shader views for the buffer.
        private readonly Dictionary<BufferShaderViewKey, GorgonShaderResourceView> _shaderViews = new Dictionary<BufferShaderViewKey, GorgonShaderResourceView>();
        // A cache of unordered access views for the buffer.
        private readonly Dictionary<BufferShaderViewKey, GorgonUnorderedAccessView> _uavs = new Dictionary<BufferShaderViewKey, GorgonUnorderedAccessView>();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the log used for debugging.
        /// </summary>
        protected IGorgonLog Log
        {
            get;
        }

        /// <summary>
        /// Property to return the usage flags for the buffer.
        /// </summary>
        internal abstract ResourceUsage Usage
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether or not the buffer is directly readable by the CPU.
        /// </summary>
        /// <remarks>
        /// If this value is <b>false</b>, then the buffer can still be read, but it will take a slower path by copying to a staging buffer.
        /// </remarks>
        internal bool IsCpuReadable
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether or not the user has requested that the buffer be readable from the CPU.
        /// </summary>
        internal abstract bool RequestedCpuReadable
        {
            get;
        }

        /// <summary>
        /// Property to return the D3D 11 buffer.
        /// </summary>
        internal D3D11.Buffer NativeBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the type of data in the resource.
        /// </summary>
        public override GraphicsResourceType ResourceType => GraphicsResourceType.Buffer;

        /// <summary>
        /// Property to return the type of buffer.
        /// </summary>
        public BufferType BufferType
        {
            get;
            protected set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the total number of elements that can be placed in the buffer.
        /// </summary>
        /// <param name="info">Information about the element format.</param>
        /// <returns>The number of elements in the buffer.</returns>
        protected int GetTotalElementCount(GorgonFormatInfo info)
        {
            if (info.IsTypeless)
            {
                return 0;
            }

            return SizeInBytes / info.SizeInBytes;
        }

        /// <summary>
        /// Function to retrieve the appropriate CPU flags for the buffer.
        /// </summary>
        /// <param name="requestReadAccess"><b>true</b> if the buffer needs direct read access, <b>false</b> if not.</param>
        /// <param name="binding">The binding flags being used by the buffer.</param>
        /// <returns>The D3D11 CPU access flags.</returns>
        internal D3D11.CpuAccessFlags GetCpuFlags(bool requestReadAccess, D3D11.BindFlags binding)
        {
            D3D11.CpuAccessFlags result = D3D11.CpuAccessFlags.None;

            //RequestedCpuReadable = requestReadAccess;

            switch (Usage)
            {
                case ResourceUsage.Dynamic:
                    result = D3D11.CpuAccessFlags.Write;
                    break;
                case ResourceUsage.Staging:
                    // Staging resources are implicitly readable.
                    result = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
                    //RequestedCpuReadable = true;
                    break;
                case ResourceUsage.Default:
                    if ((binding != D3D11.BindFlags.ShaderResource) 
                        && (binding != D3D11.BindFlags.UnorderedAccess)
                        && (binding != (D3D11.BindFlags.ShaderResource | D3D11.BindFlags.UnorderedAccess)))
                    {
                        break;
                    }

                    result = requestReadAccess ? D3D11.CpuAccessFlags.Read : D3D11.CpuAccessFlags.None;
                    break;
            }

            IsCpuReadable = (result & D3D11.CpuAccessFlags.Read) == D3D11.CpuAccessFlags.Read;
            return result;
        }

        /// <summary>
        /// Function to return a cached shader resource view.
        /// </summary>
        /// <param name="key">The key associated with the view.</param>
        /// <returns>The shader resource view for the buffer, or <b>null</b> if no resource view is registered.</returns>
        internal GorgonShaderResourceView GetView(BufferShaderViewKey key)
        {
            return _shaderViews.TryGetValue(key, out GorgonShaderResourceView view) ? view : null;
        }

        /// <summary>
        /// Function to register the shader resource view in the cache.
        /// </summary>
        /// <param name="key">The unique key for the shader view.</param>
        /// <param name="view">The view to register.</param>
        internal void RegisterView(BufferShaderViewKey key, GorgonShaderResourceView view)
        {
            _shaderViews[key] = view;
        }

        /// <summary>
        /// Function to validate the bindings for a given buffer.
        /// </summary>
        /// <param name="usage">The usage flags for the buffer.</param>
        /// <param name="binding">The bindings to apply to the buffer.</param>
        /// <param name="structureSize">The size of a structure within the buffer, in bytes.</param>
        protected static void ValidateBufferBindings(ResourceUsage usage, BufferBinding binding, int structureSize)
        {
            if (structureSize > 2048)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_STRUCTURE_SIZE_INVALID, structureSize));
            }

            if ((usage != ResourceUsage.Staging) && (binding == BufferBinding.None))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_NON_STAGING_NEEDS_BINDING, usage));
            }
            
            switch (usage)
            {
                case ResourceUsage.Immutable:
                    if ((binding & BufferBinding.StreamOut) == BufferBinding.StreamOut)
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_BUFFER_IMMUTABLE_STAGING_SO);
                    }

                    if ((binding & BufferBinding.UnorderedAccess) == BufferBinding.UnorderedAccess)
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_BUFFER_IMMUTABLE_STAGING_UAV);
                    }
                    break;
                case ResourceUsage.Staging:
                    if (binding != BufferBinding.None)
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_CANNOT_BE_BOUND_TO_GPU, binding));
                    }
                    break;
            }

            if (usage != ResourceUsage.Dynamic)
            {
                return;
            }

            if ((binding & BufferBinding.StreamOut) == BufferBinding.StreamOut)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_BUFFER_IMMUTABLE_STAGING_SO);
            }

            if ((binding & BufferBinding.UnorderedAccess) == BufferBinding.UnorderedAccess)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_BUFFER_IMMUTABLE_STAGING_UAV);
            }
        }

        /// <summary>
        /// Function to return a cached shader resource view.
        /// </summary>
        /// <param name="key">The key associated with the view.</param>
        /// <returns>The shader resource view for the buffer, or <b>null</b> if no resource view is registered.</returns>
        internal GorgonUnorderedAccessView GetUav(BufferShaderViewKey key)
        {
            return _uavs.TryGetValue(key, out GorgonUnorderedAccessView view) ? view : null;
        }

        /// <summary>
        /// Function to register an unordered access view in the cache.
        /// </summary>
        /// <param name="key">The unique key for the shader view.</param>
        /// <param name="view">The view to register.</param>
        internal void RegisterUav(BufferShaderViewKey key, GorgonUnorderedAccessView view)
        {
            _uavs[key] = view;
        }

        /// <summary>
        /// Function to retrieve a copy of this buffer as a staging resource.
        /// </summary>
        /// <returns>The staging buffer to retrieve.</returns>
        public GorgonBufferBase GetStaging()
        {
            var clone = new GorgonBuffer($"{Name}_Staging",
                                         Graphics,
                                         new GorgonBufferInfo
                                         {
                                             Usage = ResourceUsage.Staging,
                                             SizeInBytes = SizeInBytes,
                                             Binding = BufferBinding.None,
                                             DefaultShaderViewFormat = BufferFormat.Unknown,
                                             AllowCpuRead = true
                                         });
            Graphics.Copy(this, clone);

            return clone;
        }
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Objects that override this method should be sure to call this base method or else a memory leak may occur.
        /// </para>
        /// </remarks>
        public override void Dispose()
        {
            // Remove any cached views for this buffer.
            foreach (KeyValuePair<BufferShaderViewKey, GorgonShaderResourceView> view in _shaderViews)
            {
                view.Value.Dispose();
            }

            _shaderViews.Clear();

            foreach (KeyValuePair<BufferShaderViewKey, GorgonUnorderedAccessView> view in _uavs)
            {
                view.Value.Dispose();
            }

            _uavs.Clear();
            base.Dispose();
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBufferBase" /> class.
        /// </summary>
        /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
        /// <param name="name">Name of this buffer.</param>
        /// <param name="log">[Optional] The log interface used for debug logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, or the <paramref name="name"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        protected GorgonBufferBase(string name, GorgonGraphics graphics, IGorgonLog log)
            : base(graphics, name)
        {
            Log = log ?? GorgonLogDummy.DefaultInstance;
        }
        #endregion
    }
}