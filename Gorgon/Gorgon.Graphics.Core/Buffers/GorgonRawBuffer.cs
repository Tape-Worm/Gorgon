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
using Gorgon.Math;
using Gorgon.Native;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A raw buffer for holding byte data to pass to the GPU.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Raw buffers require a video device capable of a <see cref="IGorgonVideoDevice.RequestedFeatureLevel"/> of 11.0 or better.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public class GorgonRawBuffer
        : GorgonBufferCommon
    {
        #region Variables.
        // The information used to create the buffer.
        private readonly GorgonRawBufferInfo _info;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the usage flags for the buffer.
        /// </summary>
        protected override D3D11.ResourceUsage Usage => _info.Usage;

        /// <summary>
        /// Property to return the settings for the buffer.
        /// </summary>
        public IGorgonRawBufferInfo Info => _info;

        /// <summary>
        /// Property to return the default shader view.
        /// </summary>
        /// <remarks>
        /// This property is provided for convenience so that applications can immediately use a buffer in a shader without having to build a specialized <see cref="GorgonRawBufferView"/>.
        /// </remarks>
        public GorgonRawBufferView DefaultShaderView
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

            // Align to the nearest 4 bytes.
            SizeInBytes = (_info.SizeInBytes + 3) & ~3;

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

            Log.Print($"{Name} Raw Buffer: Creating D3D11 buffer. Size: {SizeInBytes} bytes", LoggingLevel.Simple);

            var bindFlags = D3D11.BindFlags.None;

            if ((_info.Binding == BufferBinding.None) && (_info.Usage == D3D11.ResourceUsage.Staging))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_NON_STAGING_NEEDS_BINDING, _info.Usage));
            }

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
                           OptionFlags = D3D11.ResourceOptionFlags.BufferAllowRawViews,
                           CpuAccessFlags = cpuFlags,
                           StructureByteStride = 0
                       };

            if ((initialData != null) && (initialData.Size > 0))
            {
                D3DResource = D3DBuffer = new D3D11.Buffer(Graphics.VideoDevice.D3DDevice(), new IntPtr(initialData.Address), desc)
                                          {
                                              DebugName = Name
                                          };
            }
            else
            {
                D3DResource = D3DBuffer = new D3D11.Buffer(Graphics.VideoDevice.D3DDevice(), desc)
                                          {
                                              DebugName = Name
                                          };
            }
            
            if ((_info.DefaultShaderViewElementType == null)
                || ((bindFlags & D3D11.BindFlags.ShaderResource) != D3D11.BindFlags.ShaderResource))
            {
                return;
            }

            DefaultShaderView = GetShaderResourceView(_info.DefaultShaderViewElementType.Value);
        }

        /// <summary>
        /// Function to retrieve the total number of elements in a buffer.
        /// </summary>
        /// <returns>The total number of elements.</returns>
        /// <remarks>
        /// <para>
        /// Use this to retrieve the number of elements that will be passed to a shader resource view.
        /// </para>
        /// </remarks>
        public int GetTotalElementCount() => SizeInBytes / 4;

        /// <summary>
        /// Function to create or retrieve a <see cref="GorgonRawBufferView"/> for this buffer.
        /// </summary>
        /// <param name="elementType">The type of data to interpret elements within the buffer as.</param>
        /// <param name="startElement">[Optional] The starting element to begin viewing at.</param>
        /// <param name="elementCount">[Optional] The number of elements to view.</param>
        /// <returns>The <see cref="GorgonRawBufferView"/> requested for this buffer.</returns>
        /// <exception cref="ArgumentException">Thrown if this buffer is a staging resource, or does not have a binding flag for shader access.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="startElement"/> and the <paramref name="elementCount"/> are larger than the total number of elements.</exception>
        /// <remarks>
        /// <para>
        /// To determine how many elements are in a buffer, use the <see cref="GetTotalElementCount"/> method.
        /// </para>
        /// <para>
        /// If the <paramref name="elementCount"/> is omitted (less than 1), then the entire buffer minus the <paramref name="startElement"/> will be available to the shader.
        /// </para>
        /// </remarks>
        public GorgonRawBufferView GetShaderResourceView(RawBufferElementType elementType, int startElement = 0, int elementCount = 0)
        {
            int totalElementCount = GetTotalElementCount();
            startElement = startElement.Max(0);

            // If we didn't specify a count, then do so now.
            if (elementCount < 1)
            {
                elementCount = totalElementCount - startElement;
            }

            var key = new BufferShaderViewKey(startElement, elementCount, elementType);
            var view = GetView(key) as GorgonRawBufferView;

            if (view != null)
            {
                return view;
            }
            
            view = new GorgonRawBufferView(this, elementType, startElement, elementCount, totalElementCount, Log);
            view.CreateNativeView();
            RegisterView(key, view);
            return view;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRawBuffer" /> class.
        /// </summary>
        /// <param name="name">Name of this buffer.</param>
        /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
        /// <param name="info">Information used to create the buffer.</param>
        /// <param name="initialData">[Optional] The initial data used to populate the buffer.</param>
        /// <param name="log">[Optional] The log interface used for debug logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/>, <paramref name="name"/>, or <paramref name="info"/> parameters are <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="name"/> is empty.
        /// <para>-or-</para>
        /// <para>Thrown if the size of the buffer is less than 4 bytes.</para>
        /// <para>-or-</para>
        /// <para>Thrown if this object is created with a device that does not have a feature level of 11.0 or better.</para>
        /// </exception>
        /// <exception cref="GorgonException">Thrown if the buffer is created with a usage of <c>Immutable</c>, but the <paramref name="initialData"/> parameter is <b>null</b>.</exception>
        public GorgonRawBuffer(GorgonGraphics graphics, string name, IGorgonRawBufferInfo info, IGorgonPointer initialData = null, IGorgonLog log = null)
            : base(graphics, name, log)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (info.SizeInBytes < 4)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_SIZE_TOO_SMALL, 4));
            }

            if (graphics.VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0), nameof(graphics));
            }

            BufferType = BufferType.Raw;
            _info = new GorgonRawBufferInfo(info);
            Initialize(initialData);
        }
        #endregion
    }
}
