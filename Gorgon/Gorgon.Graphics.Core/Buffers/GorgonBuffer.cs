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
// Created: July 4, 2017 10:05:54 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Graphics.Imaging;
using Gorgon.Math;
using Gorgon.Native;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A generic buffer for holding unstructured data to pass to the GPU.
    /// </summary>
    public class GorgonBuffer
        : GorgonBufferCommon
    {
        #region Variables.
        // The information used to create the buffer.
        private readonly GorgonBufferInfo _info;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the usage flags for the buffer.
        /// </summary>
        protected override D3D11.ResourceUsage Usage => _info.Usage;

        /// <summary>
        /// Property to return the settings for the buffer.
        /// </summary>
        public IGorgonBufferInfo Info => _info;

        /// <summary>
        /// Property to return the default shader view.
        /// </summary>
        /// <remarks>
        /// This property is provided for convenience so that applications can immediately use a buffer in a shader without having to build a specialized <see cref="GorgonBufferView"/>.
        /// </remarks>
        public GorgonBufferView DefaultShaderView
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function used to initalize the buffer.
        /// </summary>
        /// <param name="initialData">The data to copy into the buffer on creation.</param>
        private void Initialize(IGorgonPointer initialData)
        {
            if ((_info.Usage == D3D11.ResourceUsage.Immutable) && (initialData == null))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

            SizeInBytes = _info.SizeInBytes;

            D3D11.CpuAccessFlags cpuFlags = D3D11.CpuAccessFlags.None;

            switch (_info.Usage)
            {
                case D3D11.ResourceUsage.Staging:
                    cpuFlags = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;

                    if (_info.Binding != BufferBinding.None)
                    {
                        throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_CANNOT_BE_BOUND_TO_GPU, _info.Binding));
                    }
                    break;
                case D3D11.ResourceUsage.Dynamic:
                    cpuFlags = D3D11.CpuAccessFlags.Write;
                    break;
            }

            Log.Print(BufferType == BufferType.Generic
                          ? $"{Name} Generic Buffer: Creating D3D11 buffer. Size: {SizeInBytes} bytes"
                          : $"{Name} Indirect Argument Buffer: Creating D3D11 buffer. Size: {SizeInBytes} bytes",
                      LoggingLevel.Simple);

            var bindFlags = D3D11.BindFlags.None;

            if ((_info.Binding & BufferBinding.Shader) == BufferBinding.Shader)
            {
                bindFlags |= D3D11.BindFlags.ShaderResource;
            }

            if ((_info.Binding & BufferBinding.UnorderedAccess) == BufferBinding.UnorderedAccess)
            {
                bindFlags |= D3D11.BindFlags.UnorderedAccess;
            }

            if ((_info.Binding & BufferBinding.StreamOut) == BufferBinding.StreamOut)
            {
                bindFlags |= D3D11.BindFlags.StreamOutput;
            }
            
            var desc = new D3D11.BufferDescription
                       {
                           SizeInBytes = SizeInBytes,
                           Usage = _info.Usage,
                           BindFlags = bindFlags,
                           OptionFlags = BufferType == BufferType.IndirectArgument ? D3D11.ResourceOptionFlags.DrawIndirectArguments : D3D11.ResourceOptionFlags.None,
                           CpuAccessFlags = cpuFlags,
                           StructureByteStride = 0
                       };

            if ((initialData != null) && (initialData.Size > 0))
            {
                D3DResource = D3DBuffer = new D3D11.Buffer(Graphics.VideoDevice.D3DDevice(), new IntPtr(initialData.Address), desc);
            }
            else
            {
                D3DResource = D3DBuffer = new D3D11.Buffer(Graphics.VideoDevice.D3DDevice(), desc);
            }

            if ((_info.DefaultShaderViewFormat == DXGI.Format.Unknown)
                || ((bindFlags & D3D11.BindFlags.ShaderResource) != D3D11.BindFlags.ShaderResource))
            {
                return;
            }

            DefaultShaderView = GetShaderResourceView(_info.DefaultShaderViewFormat);
        }

        /// <summary>
        /// Function to retrieve the total number of elements in a buffer.
        /// </summary>
        /// <param name="format">The desired format for the view.</param>
        /// <returns>The total number of elements.</returns>
        /// <remarks>
        /// <para>
        /// Use this to retrieve the number of elements based on the <paramref name="format"/> that will be passed to a shader resource view.
        /// </para>
        /// </remarks>
        public int GetTotalElementCount(DXGI.Format format) => format == DXGI.Format.Unknown ? 0 : GetTotalElementCount(new GorgonFormatInfo(format));

        /// <summary>
        /// Function to create or retrieve a <see cref="GorgonBufferView"/> for this buffer.
        /// </summary>
        /// <param name="format">The format of the view</param>
        /// <param name="startElement">[Optional] The starting element to begin viewing at.</param>
        /// <param name="elementCount">[Optional] The number of elements to view.</param>
        /// <returns>The <see cref="GorgonBufferView"/> requested for this buffer.</returns>
        /// <exception cref="ArgumentException">Thrown if this buffer is a staging resource, or does not have a binding flag for shader access.
        /// <para>-or-</para>
        /// <para>Thrown if the <paramref name="format"/> is <c>Unknown</c>.</para>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="startElement"/> and the <paramref name="elementCount"/> are larger than the total number of elements.</exception>
        /// <remarks>
        /// <para>
        /// To determine how many elements are in a buffer, use the <see cref="GetTotalElementCount"/> method.
        /// </para>
        /// <para>
        /// If the <paramref name="elementCount"/> is omitted (less than 1), then the entire buffer minus the <paramref name="startElement"/> will be available to the shader.
        /// </para>
        /// </remarks>
        public GorgonBufferView GetShaderResourceView(DXGI.Format format, int startElement = 0, int elementCount = 0)
        {
            if (format == DXGI.Format.Unknown)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_VIEW_UNKNOWN_FORMAT, nameof(format));
            }

            var formatInfo = new GorgonFormatInfo(format);
            int totalElementCount = GetTotalElementCount(formatInfo);
            startElement = startElement.Max(0);

            // If we didn't specify a count, then do so now.
            if (elementCount < 1)
            {
                elementCount = totalElementCount - startElement;
            }

            var key = new BufferShaderViewKey(startElement, elementCount, format);
            var view = GetView(key) as GorgonBufferView;

            if (view != null)
            {
                return view;
            }
            
            view = new GorgonBufferView(this, format, startElement, elementCount, totalElementCount, formatInfo, Log);
            view.CreateNativeView();
            RegisterView(key, view);
            return view;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBuffer" /> class.
        /// </summary>
        /// <param name="name">Name of this buffer.</param>
        /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
        /// <param name="info">Information used to create the buffer.</param>
        /// <param name="initialData">[Optional] The initial data used to populate the buffer.</param>
        /// <param name="log">[Optional] The log interface used for debug logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="name"/>, or <paramref name="info"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>Thrown if the size of the buffer is less than 1 byte.</para>
        /// </exception>
        /// <exception cref="GorgonException">Thrown if the buffer is created with a usage of <c>Immutable</c>, but the <paramref name="initialData"/> parameter is <b>null</b>.</exception>
        public GorgonBuffer(GorgonGraphics graphics, string name, IGorgonBufferInfo info, IGorgonPointer initialData = null, IGorgonLog log = null)
            : base(graphics, name, log)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (info.SizeInBytes < 1)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_SIZE_TOO_SMALL, 1));
            }

            BufferType = BufferType.Generic;
            _info = new GorgonBufferInfo(info);
            Initialize(initialData);
        }
        #endregion
    }
}
