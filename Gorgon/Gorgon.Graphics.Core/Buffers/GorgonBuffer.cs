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
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A generic buffer for holding data to pass to shaders on the GPU.
    /// </summary>
    public sealed class GorgonBuffer
        : GorgonBufferCommon, IGorgonBufferInfo
    {
        #region Constants.
        /// <summary>
        /// The prefix to assign to a default name.
        /// </summary>
        internal const string NamePrefix = nameof(GorgonBuffer);
        #endregion

        #region Variables.
        // The information used to create the buffer.
        private GorgonBufferInfo _info;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the bind flags used for the D3D 11 resource.
        /// </summary>
        internal override D3D11.BindFlags BindFlags => Native?.Description.BindFlags ?? D3D11.BindFlags.None;

        /// <summary>
        /// Property to return the usage for the resource.
        /// </summary>
        public override ResourceUsage Usage => _info.Usage;

        /// <summary>
        /// Property to return the size, in bytes, of the resource.
        /// </summary>
        public override int SizeInBytes => _info.SizeInBytes;

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        /// <remarks>
        /// For best practises, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this 
        /// property.
        /// </remarks>
        public override string Name => _info.Name;

        /// <summary>
        /// Property to return whether or not the buffer is directly readable by the CPU via one of the <see cref="GorgonBufferCommon.GetData{T}(int, int?)"/> methods.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Buffers must meet the following criteria in order to qualify for direct CPU read:
        /// <list type="bullet">
        ///     <item>Must have a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Default"/> (or <see cref="ResourceUsage.Staging"/>).</item>
        ///     <item>Must have a <see cref="Binding"/> of <see cref="BufferBinding.Shader"/> (if the buffer is not a <see cref="ResourceUsage.Staging"/> buffer).</item>
        /// </list>
        /// </para>
        /// <para>
        /// If this value is <b>false</b>, then the buffer can still be read, but it will take a slower path by copying to a staging buffer and reading that when calling the
        /// <see cref="GorgonBufferCommon.GetData{T}(int, int?)"/> methods.
        /// </para>
        /// <para>
        /// <note type="information">
        /// Any buffer created with a <see cref="GorgonGraphicsResource.Usage"/> of <see cref="ResourceUsage.Staging"/> will always be directly readable by the CPU. Therefore, this value will always
        /// return <b>true</b> in that case.
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonBufferCommon.GetData{T}(Span{T}, int, int?)"/>
        /// <seealso cref="GorgonBufferCommon.GetData{T}(out T, int)"/>
        /// <seealso cref="GorgonBufferCommon.GetData{T}(int, int?)"/>
        public override bool IsCpuReadable =>
            ((_info.AllowCpuRead) && (Usage == ResourceUsage.Default) && ((Binding & BufferBinding.Shader) == BufferBinding.Shader))
            || (Usage == ResourceUsage.Staging);


        /// <summary>
        /// Property to set or return whether to allow the CPU read access to the buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This value controls whether or not the CPU can directly access the buffer for reading. If this value is <b>false</b>, the buffer still can be read, but will be done through an intermediate
        /// staging buffer, which is obviously less performant. 
        /// </para>
        /// <para>
        /// This value is treated as <b>false</b> if the buffer does not have a <see cref="IGorgonBufferInfo.Binding"/> containing the <see cref="BufferBinding.Shader"/> flag, and does not have a <see cref="IGorgonBufferInfo.Usage"/> of
        /// <see cref="ResourceUsage.Default"/>. This means any reads will be done through an intermediate staging buffer, impacting performance.
        /// </para>
        /// <para>
        /// If the <see cref="IGorgonBufferInfo.Usage"/> property is set to <see cref="ResourceUsage.Staging"/>, then this value is treated as <b>true</b> because staging buffers are CPU only and as such, can be read
        /// directly by the CPU regardless of this value.
        /// </para>
        /// <para>
        /// The default for this value is <b>false</b>.
        /// </para>
        /// </remarks>
        bool IGorgonBufferInfo.AllowCpuRead => _info.AllowCpuRead;

        /// <summary>
        /// Property to return the size, in bytes, of an individual structure in a structured buffer.
        /// </summary>
        /// <remarks>
        /// If this value is greater than 0, then the buffer is treated as a structured buffer type on the GPU.
        /// </remarks>
        public int StructureSize => _info.StructureSize;

        /// <summary>
        /// Property to return whether to allow raw unordered views of the buffer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When this value is <b>true</b>, then unordered access views for this buffer can use byte addressing to read/write the buffer in a shader. If it is <b>false</b>, then the SRV format or 
        /// <see cref="IGorgonBufferInfo.StructureSize"/> will determine how to address data in the buffer.
        /// </para>
        /// </remarks>
        public bool AllowRawView => _info.AllowRawView;

        /// <summary>
        /// Property to return the type of binding for the GPU.
        /// </summary>
        public BufferBinding Binding => _info.Binding;

        /// <summary>
        /// Property to return whether the buffer will contain indirect argument data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This flag only applies to buffers with a <see cref="Binding"/> of <see cref="BufferBinding.ReadWrite"/>, and/or <see cref="BufferBinding.Shader"/>. If the <see cref="Binding"/> does not
        /// contain one of these flags, then this will always return <b>false</b>.
        /// </para>
        /// </remarks>
        public bool IndirectArgs => (_info.IndirectArgs) && (((Binding & BufferBinding.Shader) == BufferBinding.Shader) ||
                                                             ((Binding & BufferBinding.ReadWrite) == BufferBinding.ReadWrite));
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the binding flags for the buffer.
        /// </summary>
        /// <param name="binding">The buffer binding type.</param>
        /// <returns>A set of Direct3D 11 binding flags.</returns>
        private static D3D11.BindFlags GetBindingFlags(BufferBinding binding)
        {
            D3D11.BindFlags bindFlags = D3D11.BindFlags.None;

            if ((binding & BufferBinding.Shader) == BufferBinding.Shader)
            {
                bindFlags |= D3D11.BindFlags.ShaderResource;
            }

            if ((binding & BufferBinding.ReadWrite) == BufferBinding.ReadWrite)
            {
                bindFlags |= D3D11.BindFlags.UnorderedAccess;
            }

            if ((binding & BufferBinding.StreamOut) == BufferBinding.StreamOut)
            {
                bindFlags |= D3D11.BindFlags.StreamOutput;
            }

            return bindFlags;
        }

        /// <summary>
        /// Function to build the description structure used to create the buffer.
        /// </summary>
        /// <param name="bufferInfo">The parameters used to create the buffer.</param>
        /// <returns>A new D3D 11 buffer description used to create the buffer.</returns>
        private D3D11.BufferDescription BuildBufferDesc(ref GorgonBufferInfo bufferInfo)
        {
            D3D11.BindFlags bindFlags = GetBindingFlags(bufferInfo.Binding);
            var resourceUsage = (D3D11.ResourceUsage)bufferInfo.Usage;

            // Round up to the nearest multiple of 4.
            int structureSize = (bufferInfo.StructureSize + 3) & ~3;
            int sizeInBytes = bufferInfo.SizeInBytes;

            if (structureSize > 2048)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_STRUCTURE_SIZE_INVALID, structureSize));
            }

            if (bufferInfo.AllowRawView)
            {
                // Raw buffers use 4 byte alignment.
                sizeInBytes = (sizeInBytes + 3) & ~3;
            }

            ValidateBufferBindings(bufferInfo.Usage, bufferInfo.Binding, structureSize);

            D3D11.ResourceOptionFlags options = D3D11.ResourceOptionFlags.None;

            if (bufferInfo.IndirectArgs)
            {
                options |= D3D11.ResourceOptionFlags.DrawIndirectArguments;
            }

            if (bufferInfo.AllowRawView)
            {
                options |= D3D11.ResourceOptionFlags.BufferAllowRawViews;
            }

            if (structureSize > 0)
            {
                options |= D3D11.ResourceOptionFlags.BufferStructured;
            }

            if ((bufferInfo.SizeInBytes != sizeInBytes) || (bufferInfo.StructureSize != structureSize))
            {
#if NET6_0_OR_GREATER
                bufferInfo = bufferInfo with
                {
                    StructureSize = structureSize,
                    SizeInBytes = sizeInBytes
                };
#endif
            }

            return new D3D11.BufferDescription
            {
                SizeInBytes = sizeInBytes,
                Usage = resourceUsage,
                BindFlags = bindFlags,
                OptionFlags = options,
                CpuAccessFlags = GetCpuFlags(bufferInfo.Usage, bufferInfo.Binding, bufferInfo.IndirectArgs),
                StructureByteStride = structureSize
            };
        }

        /// <summary>
        /// Function to retrieve the appropriate CPU flags for the buffer.
        /// </summary>
        /// <param name="usage">The intended usage for the buffer.</param>
        /// <param name="binding">The binding flags being used by the buffer.</param>
        /// <param name="useIndirectArgs"><b>true</b> if the buffer will contain indirect arguments, <b>false</b> if not.</param>
        /// <returns>The D3D11 CPU access flags.</returns>
        private D3D11.CpuAccessFlags GetCpuFlags(ResourceUsage usage, BufferBinding binding, bool useIndirectArgs)
        {
            D3D11.CpuAccessFlags result = D3D11.CpuAccessFlags.None;

            if (useIndirectArgs)
            {
                return result;
            }

            switch (usage)
            {
                case ResourceUsage.Dynamic:
                    result = D3D11.CpuAccessFlags.Write;
                    break;
                case ResourceUsage.Staging:
                    // Staging resources are implicitly readable.
                    result = D3D11.CpuAccessFlags.Read | D3D11.CpuAccessFlags.Write;
                    break;
                case ResourceUsage.Default:
                    if (binding is not BufferBinding.Shader and not BufferBinding.ReadWrite and not (BufferBinding.Shader | BufferBinding.ReadWrite))
                    {
                        break;
                    }

                    result = _info.AllowCpuRead ? D3D11.CpuAccessFlags.Read : D3D11.CpuAccessFlags.None;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Function used to initalize the buffer.
        /// </summary>
        /// <param name="initialData">The data to copy into the buffer on creation.</param>
        private void Initialize(ReadOnlySpan<byte> initialData)
        {
            if (_info.SizeInBytes < 1)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_BUFFER_SIZE_TOO_SMALL, 1));
            }

            if ((_info.Usage == ResourceUsage.Immutable) && (initialData.IsEmpty))
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_BUFFER_IMMUTABLE_REQUIRES_DATA);
            }

            D3D11.BufferDescription desc = BuildBufferDesc(ref _info);

            // Implicitly allow reading for staging resources.
            if (_info.Usage == ResourceUsage.Staging)
            {
#if NET6_0_OR_GREATER
                _info = _info with
                {
                    AllowCpuRead = true
                };
#endif
            }

            Log.Print($"{Name} Generic Buffer: Creating D3D11 buffer. Size: {SizeInBytes} bytes", LoggingLevel.Simple);

            D3DResource = Native = ResourceFactory.Create(Graphics.D3DDevice, Name, in desc, initialData);
        }

        /// <summary>
        /// Function to retrieve a copy of this buffer as a staging resource.
        /// </summary>
        /// <returns>The staging buffer to retrieve.</returns>
        protected override GorgonBufferCommon GetStagingInternal() => GetStaging();

        /// <summary>
        /// Function to retrieve a copy of this buffer as a staging resource.
        /// </summary>
        /// <returns>The staging buffer to retrieve.</returns>
        public GorgonBuffer GetStaging()
        {
            var buffer = new GorgonBuffer(Graphics,
                                                   new GorgonBufferInfo(_info)
                                                   {
                                                       Name = $"{Name} Staging",
                                                       Binding = BufferBinding.None,
                                                       Usage = ResourceUsage.Staging
                                                   });

            CopyTo(buffer);

            return buffer;
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
        public int GetTotalElementCount(BufferFormat format) => format == BufferFormat.Unknown ? 0 : GetTotalElementCount(new GorgonFormatInfo(format));


        /// <summary>
        /// Function to retrieve the total number of structured elements in a buffer.
        /// </summary>
        /// <returns>The total number of structured elements.</returns>
        /// <remarks>
        /// <para>
        /// Use this to retrieve the number of elements that will be passed to a <see cref="GorgonStructuredView"/> or <see cref="GorgonStructuredReadWriteView"/>.
        /// </para>
        /// <para>
        /// If this buffer has a <see cref="IGorgonBufferInfo.StructureSize"/> of 0, then this value will return 0 since it is not a structured buffer.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonStructuredView"/>
        /// <seealso cref="GorgonStructuredReadWriteView"/>
        public int GetTotalStructuredElementCount() => _info.StructureSize > 0 ? SizeInBytes / _info.StructureSize : 0;

        /// <summary>
        /// Function to retrieve the total number of raw elements in a buffer.
        /// </summary>
        /// <returns>The total number of raw elements.</returns>
        /// <remarks>
        /// <para>
        /// Use this to retrieve the number of elements that will be passed to a <see cref="GorgonRawReadWriteView"/> or <see cref="GorgonRawView"/>.
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonStructuredView"/>
        /// <seealso cref="GorgonStructuredReadWriteView"/>
        public int GetTotalRawElementCount() => SizeInBytes / 4;

        /// <summary>
        /// Function to create or retrieve a <see cref="GorgonBufferView"/> for this buffer.
        /// </summary>
        /// <param name="format">The format of the view</param>
        /// <param name="startElement">[Optional] The starting element to begin viewing at.</param>
        /// <param name="elementCount">[Optional] The number of elements to view.</param>
        /// <returns>The <see cref="GorgonBufferView"/> requested for this buffer.</returns>
        /// <exception cref="GorgonException">Thrown if this buffer is a staging resource, or does not have a binding flag for shader access.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="format"/> is typeless.</exception>
        /// <remarks>
        /// <para>
        /// This will create a shader resource view that makes a buffer accessible to shaders. This allows viewing of the buffer data in a different format, or even a subsection of the buffer from within 
        /// the shader.
        /// </para>
        /// <para>
        /// The <paramref name="format"/> parameter is used present the buffer data as a specific <see cref="BufferFormat"/> type to the shader. 
        /// </para>
        /// <para>
        /// The <paramref name="startElement"/> parameter defines the starting data element to allow access to within the shader. If this value falls outside of the range of available elements, then it 
        /// will be clipped to the upper and lower bounds of the element range. If this value is left at 0, then first element is viewed. 
        /// </para>
        /// <para>
        /// The <paramref name="elementCount"/> parameter defines how many elements to allow access to inside of the view. If this value falls outside of the range of available elements, then it will be 
        /// clipped to the upper or lower bounds of the element range. If this value is left at 0, then the entire buffer is viewed.
        /// </para>
        /// </remarks>
        /// <seealso cref="BufferFormat"/>
        public GorgonBufferView GetShaderResourceView(BufferFormat format, int startElement = 0, int elementCount = 0)
        {
            if (format == BufferFormat.Unknown)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_VIEW_UNKNOWN_FORMAT, nameof(format));
            }

            if (Usage == ResourceUsage.Staging)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_BUFFER_STAGING_CANNOT_BE_BOUND_TO_GPU);
            }

            if ((Binding & BufferBinding.Shader) != BufferBinding.Shader)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_INCORRECT_BINDING, BufferBinding.Shader));
            }

            var formatInfo = new GorgonFormatInfo(format);

            if (formatInfo.IsTypeless)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_VIEW_NO_TYPELESS));
            }

            int totalElementCount = GetTotalElementCount(formatInfo);
            startElement = startElement.Min(totalElementCount - 1).Max(0);

            // If we didn't specify a count, then do so now.
            if (elementCount < 1)
            {
                elementCount = totalElementCount - startElement;
            }

            elementCount = elementCount.Min(totalElementCount - startElement).Max(1);

            var key = new BufferShaderViewKey(startElement, elementCount, format);
            if (GetView(key) is GorgonBufferView view)
            {
                return view;
            }

            view = new GorgonBufferView(this, format, formatInfo, startElement, elementCount, totalElementCount);
            view.CreateNativeView();
            RegisterView(key, view);
            return view;
        }

        /// <summary>
        /// Function to create or retrieve a <see cref="GorgonStructuredView"/> for this buffer.
        /// </summary>
        /// <param name="startElement">[Optional] The starting element to begin viewing at.</param>
        /// <param name="elementCount">[Optional] The number of elements to view.</param>
        /// <returns>The <see cref="GorgonStructuredView"/> requested for this buffer.</returns>
        /// <exception cref="GorgonException">Thrown if this buffer is a staging resource, or does not have a binding flag for shader access.</exception>
        /// <remarks>
        /// <para>
        /// This will create an unordered access view that makes a buffer accessible to shaders. This allows viewing of the buffer data in a different format, or even a subsection of the buffer from within 
        /// the shader.
        /// </para>
        /// <para>
        /// The <paramref name="startElement"/> parameter defines the starting data element to allow access to within the shader. If this value falls outside of the range of available elements, then it 
        /// will be clipped to the upper and lower bounds of the element range. If this value is left at 0, then first element is viewed. 
        /// </para>
        /// <para>
        /// The <paramref name="elementCount"/> parameter defines how many elements to allow access to inside of the view. If this value falls outside of the range of available elements, then it will be 
        /// clipped to the upper or lower bounds of the element range. If this value is left at 0, then the entire buffer is viewed.
        /// </para>
        /// </remarks>
        public GorgonStructuredView GetStructuredView(int startElement = 0, int elementCount = 0)
        {
            if (Usage == ResourceUsage.Staging)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_BUFFER_STAGING_CANNOT_BE_BOUND_TO_GPU);
            }

            if ((Binding & BufferBinding.Shader) != BufferBinding.Shader)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_BUFFER_INCORRECT_BINDING, BufferBinding.Shader));
            }

            int totalElementCount = GetTotalStructuredElementCount();
            startElement = startElement.Max(0);

            // If we didn't specify a count, then do so now.
            if (elementCount < 1)
            {
                elementCount = totalElementCount - startElement;
            }

            elementCount = elementCount.Min(totalElementCount - startElement).Max(1);

            var key = new BufferShaderViewKey(startElement, elementCount, 0);

            if (GetView(key) is GorgonStructuredView view)
            {
                return view;
            }

            view = new GorgonStructuredView(this, startElement, elementCount, totalElementCount);
            view.CreateNativeView();
            RegisterView(key, view);
            return view;
        }

        /// <summary>
        /// Function to create a new <see cref="GorgonBufferReadWriteView"/> for this buffer.
        /// </summary>
        /// <param name="format">The format for the view.</param>
        /// <param name="startElement">[Optional] The first element to start viewing from.</param>
        /// <param name="elementCount">[Optional] The number of elements to view.</param>
        /// <returns>A <see cref="GorgonBufferReadWriteView"/> used to bind the buffer to a shader.</returns>
        /// <exception cref="GorgonException">
        /// <para>Thrown when this buffer does not have a <see cref="BufferBinding"/> of <see cref="BufferBinding.ReadWrite"/>.</para>
        /// <para>-or-</para>
        /// <para>Thrown when this buffer has a usage of <see cref="ResourceUsage.Staging"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="format"/> is typeless or is not a supported format for unordered access views.</exception>        
        /// <remarks>
        /// <para>
        /// This will create an unordered access view that makes a buffer accessible to shaders using unordered access to the data. This allows viewing of the buffer data in a 
        /// different format, or even a subsection of the buffer from within the shader.
        /// </para>
        /// <para>
        /// The <paramref name="format"/> parameter is used present the buffer data as another format type to the shader. 
        /// </para>
        /// <para>
        /// The <paramref name="startElement"/> parameter defines the starting data element to allow access to within the shader. If this value falls outside of the range of available elements, then it 
        /// will be clipped to the upper and lower bounds of the element range. If this value is left at 0, then first element is viewed.
        /// </para>
        /// <para>
        /// The <paramref name="elementCount"/> parameter defines how many elements to allow access to inside of the view. If this value falls outside of the range of available elements, then it will be 
        /// clipped to the upper or lower bounds of the element range. If this value is left at 0, then the entire buffer is viewed.
        /// </para>
        /// </remarks>
        public GorgonBufferReadWriteView GetReadWriteView(BufferFormat format, int startElement = 0, int elementCount = 0)
        {
            if ((Usage == ResourceUsage.Staging)
                || ((Binding & BufferBinding.ReadWrite) != BufferBinding.ReadWrite))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_RESOURCE_NOT_VALID, Name));
            }

            if ((!Graphics.FormatSupport.TryGetValue(format, out IGorgonFormatSupportInfo support))
                || ((support.FormatSupport & BufferFormatSupport.TypedUnorderedAccessView) != BufferFormatSupport.TypedUnorderedAccessView))
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_UAV_FORMAT_INVALID, format), nameof(format));
            }

            // Ensure the size of the data type fits the requested format.
            var info = new GorgonFormatInfo(format);

            if (info.IsTypeless)
            {
                throw new ArgumentException(Resources.GORGFX_ERR_VIEW_NO_TYPELESS, nameof(format));
            }

            int totalElementCount = GetTotalElementCount(format);

            startElement = startElement.Min(totalElementCount - 1).Max(0);

            if (elementCount <= 0)
            {
                elementCount = totalElementCount - startElement;
            }

            elementCount = elementCount.Min(totalElementCount - startElement).Max(1);

            var key = new BufferShaderViewKey(startElement, elementCount, format);
            GorgonBufferReadWriteView result = GetReadWriteView<GorgonBufferReadWriteView>(key);

            if (result is not null)
            {
                return result;
            }

            result = new GorgonBufferReadWriteView(this, format, info, startElement, elementCount, totalElementCount);
            result.CreateNativeView();
            RegisterReadWriteView(key, result);

            return result;
        }

        /// <summary>
        /// Function to create a new <see cref="GorgonStructuredReadWriteView"/> for this buffer.
        /// </summary>
        /// <param name="startElement">[Optional] The first element to start viewing from.</param>
        /// <param name="elementCount">[Optional] The number of elements to view.</param>
        /// <param name="uavType">[Optional] The type of uav to create.</param>
        /// <returns>A <see cref="GorgonStructuredReadWriteView"/> used to bind the buffer to a shader.</returns>
        /// <exception cref="GorgonException">
        /// Thrown when this buffer does not have a <see cref="BufferBinding"/> of <see cref="BufferBinding.ReadWrite"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this buffer has a usage of <see cref="ResourceUsage.Staging"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will create an unordered access view that makes a buffer accessible to shaders using unordered access to the data. This allows viewing of the buffer data in a 
        /// different format, or even a subsection of the buffer from within the shader.
        /// </para>
        /// <para>
        /// The <paramref name="startElement"/> parameter defines the starting data element to allow access to within the shader. If this value falls outside of the range of available elements, then it 
        /// will be clipped to the upper and lower bounds of the element range. If this value is left at 0, then first element is viewed.
        /// </para>
        /// <para>
        /// The <paramref name="elementCount"/> parameter defines how many elements to allow access to inside of the view. If this value falls outside of the range of available elements, then it will be 
        /// clipped to the upper or lower bounds of the element range. If this value is left at 0, then the entire buffer is viewed.
        /// </para>
        /// <para>
        /// The <paramref name="uavType"/> parameter specifies whether the buffer can be used as an <see cref="StructuredBufferReadWriteViewType.Append"/>/Consume buffer or
        /// <see cref="StructuredBufferReadWriteViewType.Counter"/> by the shader.
        /// </para>
        /// </remarks>
        public GorgonStructuredReadWriteView GetStructuredReadWriteView(int startElement = 0, int elementCount = 0, StructuredBufferReadWriteViewType uavType = StructuredBufferReadWriteViewType.None)
        {
            if ((Usage == ResourceUsage.Staging)
                || ((Binding & BufferBinding.ReadWrite) != BufferBinding.ReadWrite))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_RESOURCE_NOT_VALID, Name));
            }

            // Ensure the size of the data type fits the requested format.
            int totalElementCount = GetTotalStructuredElementCount();

            startElement = startElement.Min(totalElementCount - 1).Max(0);

            if (elementCount <= 0)
            {
                elementCount = totalElementCount - startElement;
            }

            elementCount = elementCount.Min(totalElementCount - startElement).Max(1);

            var key = new BufferShaderViewKey(startElement, elementCount, (int)uavType);
            GorgonStructuredReadWriteView result = GetReadWriteView<GorgonStructuredReadWriteView>(key);

            if (result is not null)
            {
                return result;
            }

            result = new GorgonStructuredReadWriteView(this, uavType, startElement, elementCount, totalElementCount);
            result.CreateNativeView();
            RegisterReadWriteView(key, result);

            return result;
        }

        /// <summary>
        /// Function to create a new <see cref="GorgonRawView"/> for this buffer.
        /// </summary>
        /// <param name="elementType">The type of data to interpret elements within the buffer as.</param>
        /// <param name="startElement">[Optional] The first element to start viewing from.</param>
        /// <param name="elementCount">[Optional] The number of elements to view.</param>
        /// <returns>A <see cref="GorgonRawReadWriteView"/> used to bind the buffer to a shader.</returns>
        /// <exception cref="GorgonException">Thrown when this buffer does not have a <see cref="BufferBinding"/> of <see cref="BufferBinding.ReadWrite"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this buffer has a usage of <see cref="ResourceUsage.Staging"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will create a shader resource view that makes a buffer accessible to shaders. This allows viewing of the buffer data in a different format, or even a subsection of the buffer from within
        /// the shader.
        /// </para>
        /// <para>
        /// The <paramref name="elementType"/> parameter is used present the raw buffer data as another type to the shader. 
        /// </para>
        /// <para>
        /// The <paramref name="startElement"/> parameter defines the starting data element to allow access to within the shader. If this value falls outside of the range of available elements, then it 
        /// will be clipped to the upper and lower bounds of the element range. If this value is left at 0, then first element is viewed.
        /// </para>
        /// <para>
        /// To determine how many elements are in a buffer, use the <see cref="GetTotalElementCount"/> method.
        /// </para>
        /// <para>
        /// The <paramref name="elementCount"/> parameter defines how many elements to allow access to inside of the view. If this value falls outside of the range of available elements, then it will be 
        /// clipped to the upper or lower bounds of the element range. If this value is left at 0, then the entire buffer is viewed.
        /// </para>
        /// </remarks>
        public GorgonRawView GetRawView(RawBufferElementType elementType, int startElement = 0, int elementCount = 0)
        {
            if ((Usage == ResourceUsage.Staging)
                || ((Binding & BufferBinding.Shader) != BufferBinding.Shader))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_SRV_NOT_VALID, Name));
            }

            int totalElementCount = GetTotalRawElementCount();
            startElement = startElement.Min(totalElementCount - 1).Max(0);

            // If we didn't specify a count, then do so now.
            if (elementCount < 1)
            {
                elementCount = totalElementCount - startElement;
            }

            elementCount = elementCount.Min(totalElementCount - startElement).Max(1);

            var key = new BufferShaderViewKey(startElement, elementCount, elementType);

            if (GetView(key) is GorgonRawView view)
            {
                return view;
            }

            view = new GorgonRawView(this, elementType, startElement, elementCount, totalElementCount);
            view.CreateNativeView();
            RegisterView(key, view);
            return view;
        }

        /// <summary>
        /// Function to create a new <see cref="GorgonRawReadWriteView"/> for this buffer.
        /// </summary>
        /// <param name="elementType">The type of data to interpret elements within the buffer as.</param>
        /// <param name="startElement">[Optional] The first element to start viewing from.</param>
        /// <param name="elementCount">[Optional] The number of elements to view.</param>
        /// <returns>A <see cref="GorgonRawReadWriteView"/> used to bind the buffer to a shader.</returns>
        /// <exception cref="GorgonException">Thrown when this buffer does not have a <see cref="BufferBinding"/> of <see cref="BufferBinding.ReadWrite"/>.
        /// <para>-or-</para>
        /// <para>Thrown when this buffer has a usage of <see cref="ResourceUsage.Staging"/>.</para>
        /// </exception>
        /// <remarks>
        /// <para>
        /// This will create an unordered access view that makes a buffer accessible to shaders using unordered access to the data. This allows viewing of the buffer data in a 
        /// different format, or even a subsection of the buffer from within the shader.
        /// </para>
        /// <para>
        /// The <paramref name="elementType"/> parameter is used present the raw buffer data as another type to the shader. 
        /// </para>
        /// <para>
        /// The <paramref name="startElement"/> parameter defines the starting data element to allow access to within the shader. If this value falls outside of the range of available elements, then it 
        /// will be clipped to the upper and lower bounds of the element range. If this value is left at 0, then first element is viewed.
        /// </para>
        /// <para>
        /// To determine how many elements are in a buffer, use the <see cref="GetTotalElementCount"/> method.
        /// </para>
        /// <para>
        /// The <paramref name="elementCount"/> parameter defines how many elements to allow access to inside of the view. If this value falls outside of the range of available elements, then it will be 
        /// clipped to the upper or lower bounds of the element range. If this value is left at 0, then the entire buffer is viewed.
        /// </para>
        /// </remarks>
        public GorgonRawReadWriteView GetRawReadWriteView(RawBufferElementType elementType, int startElement = 0, int elementCount = 0)
        {
            if ((Usage == ResourceUsage.Staging)
                || ((Binding & BufferBinding.ReadWrite) != BufferBinding.ReadWrite))
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_UAV_RESOURCE_NOT_VALID, Name));
            }

            // Ensure the size of the data type fits the requested format.
            int totalElementCount = GetTotalRawElementCount();

            startElement = startElement.Min(totalElementCount - 1).Max(0);

            if (elementCount <= 0)
            {
                elementCount = totalElementCount - startElement;
            }

            elementCount = elementCount.Min(totalElementCount - startElement).Max(1);

            var key = new BufferShaderViewKey(startElement, elementCount, elementType);
            GorgonRawReadWriteView result = GetReadWriteView<GorgonRawReadWriteView>(key);

            if (result is not null)
            {
                return result;
            }

            result = new GorgonRawReadWriteView(this, startElement, elementCount, totalElementCount, elementType);
            result.CreateNativeView();
            RegisterReadWriteView(key, result);

            return result;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBuffer" /> class.
        /// </summary>
        /// <param name="graphics">The <see cref="GorgonGraphics"/> object used to create and manipulate the buffer.</param>
        /// <param name="info">Information used to create the buffer.</param>
        /// <param name="initialData">[Optional] The initial data used to populate the buffer.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown if the size of the buffer is less than 1 byte.</exception>
        /// <exception cref="GorgonException">Thrown if the buffer is created with a usage of <see cref="ResourceUsage.Immutable"/>, but the <paramref name="initialData"/> parameter is <b>null</b>.
        /// <para>-or-</para>
        /// <para>A value on the <paramref name="info"/> parameter is incorrect.</para>
        /// </exception>
        public GorgonBuffer(GorgonGraphics graphics, GorgonBufferInfo info, ReadOnlySpan<byte> initialData = default)
            : base(graphics)
        {
            _info = new GorgonBufferInfo(info ?? throw new ArgumentNullException(nameof(info)));

            Initialize(initialData);
        }
        #endregion
    }
}
