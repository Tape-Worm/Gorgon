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
// Created: Wednesday, May 29, 2013 11:59:53 PM
// 
#endregion

using System;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.IO;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
    /// <summary>
    /// Type of buffer.
    /// </summary>
    public enum BufferType
    {
        /// <summary>
        /// Buffer is a generic buffer with no special purpose.
        /// </summary>
        Generic = 0,
        /// <summary>
        /// Buffer holds shader constants.
        /// </summary>
        Constant = 1,
        /// <summary>
        /// Buffer holds vertex information.
        /// </summary>
        Vertex = 2,
        /// <summary>
        /// Buffer holds index information.
        /// </summary>
        Index = 3,
        /// <summary>
        /// Buffer holds structured data.
        /// </summary>
        Structured = 4,
    }

    /// <summary>
    /// Buffer usage types.
    /// </summary>
    public enum BufferUsage
    {
        /// <summary>
        /// Allows read/write access to the buffer from the GPU.
        /// </summary>
        Default = D3D.ResourceUsage.Default,
        /// <summary>
        /// Can only be read by the GPU, cannot be written to or read from by the CPU, and cannot be written to by the GPU.
        /// </summary>
        /// <remarks>Pre-initialize any buffer created with this usage, or else you will not be able to after it's been created.</remarks>
        Immutable = D3D.ResourceUsage.Immutable,
        /// <summary>
        /// Allows read access by the GPU and write access by the CPU.
        /// </summary>
        Dynamic = D3D.ResourceUsage.Dynamic,
        /// <summary>
        /// Allows reading/writing by the CPU and can be copied to a GPU compatiable buffer (but not used directly by the GPU).
        /// </summary>
        Staging = D3D.ResourceUsage.Staging
    }

    /// <summary>
    /// Flags used when locking the buffer for reading/writing.
    /// </summary>
    [Flags]
    public enum BufferLockFlags
    {
        /// <summary>
        /// Lock the buffer for reading.
        /// </summary>
        /// <remarks>This flag is mutually exclusive.</remarks>
        Read = 1,
        /// <summary>
        /// Lock the buffer for writing.
        /// </summary>
        Write = 2,
        /// <summary>
        /// Lock the buffer for writing, but guarantee that we will not overwrite a part of the buffer that's already in use.
        /// </summary>
        NoOverwrite = 4,
        /// <summary>
        /// Lock the buffer for writing, but mark its contents as invalid.
        /// </summary>
        Discard = 8
    }

    /// <summary>
    /// The base class for a buffer.
    /// </summary>
    public abstract class GorgonBaseBuffer
        : GorgonResource
    {
        #region Variables.
        private static readonly object _syncLock = new object();        // Synchronization lock for threading.

        private GorgonViewCache _viewCache;         // Cache used to hold views for the resource.
        private DX.DataStream _lockStream;			// Stream used to lock the buffer for read/write.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the D3D CPU access flags.
        /// </summary>
        private D3D.CpuAccessFlags D3DCPUAccessFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the D3D usages.
        /// </summary>
        private D3D.ResourceUsage D3DUsage
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return whether this buffer can be used as a render target.
		/// </summary>
		protected bool IsRenderTarget
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the D3D buffer object.
        /// </summary>
        internal D3D.Buffer D3DBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the type of data in the resource.
        /// </summary>
        public override ResourceType ResourceType
        {
            get
            {
                return ResourceType.Buffer;
            }
        }

        /// <summary>
        /// Property to return the size of the resource, in bytes.
        /// </summary>
        public override int SizeInBytes
        {
            get
            {
                return Settings.SizeInBytes;
            }
        }

        /// <summary>
        /// Property to return the type of buffer.
        /// </summary>
        public abstract BufferType BufferType
        {
            get;
        }

        /// <summary>
        /// Property to return whether the buffer is locked or not.
        /// </summary>
        public bool IsLocked
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the settings for the buffer.
        /// </summary>
        public IBufferSettings Settings
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the default shader view for this buffer.
        /// </summary>
        public virtual GorgonShaderView DefaultShaderView
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create the resources for the buffer.
        /// </summary>
        /// <param name="data">The initial data for the buffer.</param>
        private void CreateResources(GorgonDataStream data)
        {
            var desc = new D3D.BufferDescription
            {
                BindFlags = D3D.BindFlags.None,
                CpuAccessFlags = D3DCPUAccessFlags,
                OptionFlags = D3D.ResourceOptionFlags.None,
                SizeInBytes = Settings.SizeInBytes,
                StructureByteStride = Settings.StructureSize,
                Usage = D3DUsage
            };

			// If this is a render target buffer, then ensure that we have a default resource.
			if ((IsRenderTarget) && (Settings.Usage != BufferUsage.Default))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_RT_NEED_DEFAULT);
			}

            // Set up the buffer.  If we're a staging buffer, then none of this stuff will 
			// work because staging buffers can't be bound to the pipeline, so just skip it.
	        if (Settings.Usage != BufferUsage.Staging)
	        {
		        switch (BufferType)
		        {
			        case BufferType.Constant:
				        desc.BindFlags = D3D.BindFlags.ConstantBuffer;
				        break;
			        case BufferType.Index:
				        desc.BindFlags = D3D.BindFlags.IndexBuffer;
				        break;
			        case BufferType.Vertex:
				        desc.BindFlags = D3D.BindFlags.VertexBuffer;
				        break;
			        case BufferType.Structured:
				        if (!IsRenderTarget)
				        {
					        desc.OptionFlags = D3D.ResourceOptionFlags.BufferStructured;
				        }
				        break;
		        }

		        // Update binding modifiers.
		        if (Settings.AllowShaderViews)
		        {
			        desc.BindFlags |= D3D.BindFlags.ShaderResource;
		        }

		        if (Settings.AllowUnorderedAccessViews)
		        {
			        desc.BindFlags |= D3D.BindFlags.UnorderedAccess;
		        }

		        if (!IsRenderTarget)
		        {
			        if (Settings.AllowIndirectArguments)
			        {
				        desc.OptionFlags = D3D.ResourceOptionFlags.DrawIndirectArguments;
			        }
		        }
		        else
		        {
			        desc.BindFlags |= D3D.BindFlags.RenderTarget;
		        }

		        if (Settings.AllowRawViews)
		        {
			        desc.OptionFlags = D3D.ResourceOptionFlags.BufferAllowRawViews;
		        }
	        }

			// Create and initialize the buffer.
	        if (data != null)
            {
                long position = data.Position;

                using (var dxStream = new DX.DataStream(data.BasePointer, data.Length - position, true, true))
                {
                    D3DResource = D3DBuffer = new D3D.Buffer(Graphics.D3DDevice, dxStream, desc);
                }
            }
            else
            {
                D3DResource = D3DBuffer = new D3D.Buffer(Graphics.D3DDevice, desc);
            }
        }

        /// <summary>
        /// Function to create the default shader view.
        /// </summary>
        protected virtual void OnCreateDefaultShaderView()
        {
            // Staging buffers cannot create shader views.
            if ((Settings.Usage == BufferUsage.Staging)
                || (Settings.DefaultShaderViewFormat == BufferFormat.Unknown))
            {
                return;
            }

			var info = GorgonBufferFormatInfo.GetInfo(Settings.DefaultShaderViewFormat);
	        DefaultShaderView = OnCreateShaderView(Settings.DefaultShaderViewFormat, 0,
	                                               Settings.SizeInBytes / info.SizeInBytes, false);
        }

        /// <summary>
        /// Function to clean up the resource object.
        /// </summary>
        protected override void CleanUpResource()
        {
            if (IsLocked)
            {
                Unlock();
            }

            if (D3DResource == null)
            {
                return;
            }

            // Unbind us from any shaders.
            if (Settings.AllowShaderViews)
            {
                Graphics.Shaders.UnbindResource(this);
            }

			if (IsRenderTarget)
			{
				GorgonRenderStatistics.RenderTargetCount--;
				GorgonRenderStatistics.RenderTargetSize -= D3DBuffer.Description.SizeInBytes;
			}

            switch (BufferType)
            {
                case BufferType.Constant:
                    GorgonRenderStatistics.ConstantBufferCount--;
                    GorgonRenderStatistics.ConstantBufferSize -= D3DBuffer.Description.SizeInBytes;
                    break;
                case BufferType.Index:
                    GorgonRenderStatistics.IndexBufferCount--;
                    GorgonRenderStatistics.IndexBufferSize -= D3DBuffer.Description.SizeInBytes;
                    break;
                case BufferType.Vertex:
                    GorgonRenderStatistics.VertexBufferCount--;
                    GorgonRenderStatistics.VertexBufferSize -= D3DBuffer.Description.SizeInBytes;
                    break;
                case BufferType.Structured:
                    GorgonRenderStatistics.StructuredBufferCount--;
                    GorgonRenderStatistics.StructuredBufferSize -= D3DBuffer.Description.SizeInBytes;
                    break;
                default:
                    GorgonRenderStatistics.BufferCount--;
                    GorgonRenderStatistics.BufferSize -= D3DBuffer.Description.SizeInBytes;
                    break;
            }

            D3DResource.Dispose();
            D3DResource = null;
            D3DBuffer = null;

            Gorgon.Log.Print("Destroyed {0} Buffer '{1}'.", LoggingLevel.Verbose, BufferType, Name);
        }

        /// <summary>
        /// Function used to lock the underlying buffer for reading/writing.
        /// </summary>
        /// <param name="lockFlags">Flags used when locking the buffer.</param>
        /// <returns>A data stream containing the buffer data.</returns>		
        protected virtual GorgonDataStream OnLock(BufferLockFlags lockFlags)
        {
#if DEBUG
            if (((lockFlags & BufferLockFlags.Discard) != BufferLockFlags.Discard)
                || ((lockFlags & BufferLockFlags.Write) != BufferLockFlags.Write))
            {
                throw new ArgumentException(Resources.GORGFX_BUFFER_LOCK_NOT_WRITE_DISCARD, "lockFlags");
            }

            if ((lockFlags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite)
            {
                throw new ArgumentException(Resources.GORGFX_BUFFER_NO_OVERWRITE_NOT_VALID, "lockFlags");
            }
#endif

            Graphics.Context.MapSubresource(D3DBuffer, D3D.MapMode.WriteDiscard, D3D.MapFlags.None, out _lockStream);

            return new GorgonDataStream(_lockStream.DataPointer, (int)_lockStream.Length);
        }

        /// <summary>
        /// Function called to unlock the underlying data buffer.
        /// </summary>
        protected virtual void OnUnlock()
        {
            Graphics.Context.UnmapSubresource(D3DBuffer, 0);
            _lockStream.Dispose();
            _lockStream = null;
        }

        /// <summary>
        /// Function to update the buffer.
        /// </summary>
        /// <param name="stream">Stream containing the data used to update the buffer.</param>
        /// <param name="offset">Offset, in bytes, into the buffer to start writing at.</param>
        /// <param name="size">The number of bytes to write.</param>
        protected virtual void OnUpdate(GorgonDataStream stream, int offset, int size)
        {
            Graphics.Context.UpdateSubresource(
                new DX.DataBox
                {
                    DataPointer = (stream.PositionPointer + offset),
                    RowPitch = size,
                    SlicePitch = 0
                },
                D3DResource);
        }

        /// <summary>
        /// Function to create an unordered access view for this buffer.
        /// </summary>
        /// <param name="format">Format of the buffer.</param>
        /// <param name="start">First element to map to the view.</param>
        /// <param name="count">The number of elements to map to the view.</param>
        /// <param name="isRaw">TRUE if using a raw view to the buffer, FALSE if not.</param>
        /// <returns>A new unordered access view for the buffer.</returns>
        /// <remarks>Use this to create an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a Shader View, only one 
        /// unordered access view can be bound to the pipeline at any given time.
        /// <para>Raw views require that the format be set to R32 (typeless).</para>
        /// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
        /// </remarks>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this buffer is set to Staging or Dynamic.
        /// <para>-or-</para>
        /// <para>Thrown when the video device feature level is not SM_5 or better.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the resource settings do not allow unordered access views.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the view could not be created.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="start"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
        /// number of elements in the buffer.</exception>
        protected GorgonBufferUnorderedAccessView OnCreateUnorderedAccessView(BufferFormat format, int start, int count, bool isRaw)
        {
            int elementCount;

            if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_REQUIRES_SM, "SM5"));
            }

            if (!Settings.AllowUnorderedAccessViews)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_BUFFER_NO_UNORDERED_VIEWS);
            }

            if ((Settings.Usage == BufferUsage.Staging) || (Settings.Usage == BufferUsage.Dynamic))
            {
                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_VIEW_UNORDERED_NO_STAGING_DYNAMIC);
            }

            if (BufferType != BufferType.Structured)
            {
                if (format == BufferFormat.Unknown)
                {
                    throw new ArgumentException(Resources.GORGFX_VIEW_UNKNOWN_FORMAT, "format");
                }

                if (Settings.AllowRawViews)
                {
                    if (format != BufferFormat.R32)
                    {
                        throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_VIEW_UNORDERED_RAW_INVALID_FORMAT);
                    }

                    elementCount = SizeInBytes / 4;
                }
                else
                {
                    // Ensure the size of the data type fits the requested format.
                    var info = GorgonBufferFormatInfo.GetInfo(format);
                    elementCount = SizeInBytes / info.SizeInBytes;
                }
            }
            else
            {
                isRaw = false;
                elementCount = SizeInBytes / Settings.StructureSize;
                format = BufferFormat.Unknown;
            }

            if (((start + count) > elementCount)
                || (start < 0)
                || (count < 1))
            {
	            throw new ArgumentException(string.Format(Resources.GORGFX_VIEW_ELEMENT_OUT_OF_RANGE, elementCount,
	                                                      (elementCount - start)));
            }

            var view = new GorgonBufferUnorderedAccessView(this, format, start, count, isRaw);

            view.Initialize();

            return view;
        }

        /// <summary>
        /// Function to create a new shader view for the buffer.
        /// </summary>
        /// <param name="format">The format of the view.</param>
        /// <param name="start">Starting element.</param>
        /// <param name="count">Element count.</param>
        /// <param name="isRaw">TRUE if using a raw view of the buffer, FALSE if not.</param>
        /// <returns>A new shader view for the buffer.</returns>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the usage for this buffer is set to Staging.
        /// <para>-or-</para>
        /// <para>Thrown when the view could not be created.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="start"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
        /// number of elements in the buffer.</exception>
        /// <remarks>Use this to create additional shader views for the buffer.  Multiple views of the same resource can be bound to multiple stages in the pipeline.
        /// <para>Raw views require that the buffer be created with the <see cref="GorgonLibrary.Graphics.IBufferSettings.AllowRawViews">AllowRawViews</see> property set to TRUE in its settings.</para>
        /// <para>Raw views can only be used on SM5 video devices or better. </para>
        /// <para>This function only applies to buffers that have not been created with a Usage of Staging.</para>
        /// </remarks>
        protected GorgonBufferShaderView OnCreateShaderView(BufferFormat format, int start, int count, bool isRaw)
        {
            int elementCount;

            if (Settings.Usage == BufferUsage.Staging)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_SRV_NO_STAGING);
            }

            // If we're using any other type than structured...
            if (BufferType != BufferType.Structured)
            {
                if (format == BufferFormat.Unknown)
                {
                    throw new ArgumentException(Resources.GORGFX_VIEW_UNKNOWN_FORMAT, "format");
                }

                // Ensure the size of the data type fits the requested format.
                var info = GorgonBufferFormatInfo.GetInfo(format);

                if (info.IsTypeless)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_NO_TYPELESS);
                }

                if (isRaw)
                {
                    if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
                    {
                        throw new GorgonException(GorgonResult.CannotCreate,
                                                  string.Format(Resources.GORGFX_REQUIRES_SM, "SM5"));
                    }

                    if (!Settings.AllowRawViews)
                    {
                        throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_BUFFER_NO_RAW_VIEWS);
                    }

                    if (info.Group != BufferFormat.R32)
                    {
                        throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_VIEW_RAW_INVALID_FORMAT);
                    }

                    elementCount = SizeInBytes / 4;
                }
                else
                {
                    elementCount = SizeInBytes / info.SizeInBytes;
                }
            }
            else
            {
                // We cannot use structured buffers on anything less than a SM5 video device.
                if (Graphics.VideoDevice.SupportedFeatureLevel < DeviceFeatureLevel.SM5)
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                              string.Format(Resources.GORGFX_REQUIRES_SM, "SM5"));
                }

                elementCount = Settings.SizeInBytes / Settings.StructureSize;
                format = BufferFormat.Unknown;
                isRaw = false;
            }

            if (((start + count) > elementCount)
                || (start < 0)
                || (count < 1))
            {
				throw new ArgumentException(string.Format(Resources.GORGFX_VIEW_ELEMENT_OUT_OF_RANGE, elementCount,
														  (elementCount - start)));
			}

            lock(_syncLock)
            {
                if (_viewCache == null)
                {
                    _viewCache = new GorgonViewCache(this);    
                }

                return _viewCache.GetBufferView(format, start, count, isRaw);
            }
        }

        /// <summary>
        /// Function used to initialize the buffer with data.
        /// </summary>
        /// <param name="data">Data to write.</param>
        /// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
        internal void Initialize(GorgonDataStream data)
        {
            Gorgon.Log.Print("Creating {0} Buffer '{1}'...", LoggingLevel.Verbose, BufferType, Name);

            CreateResources(data);

            // Update our statistics.
            switch (BufferType)
            {
                case BufferType.Constant:
                    GorgonRenderStatistics.ConstantBufferCount++;
                    GorgonRenderStatistics.ConstantBufferSize += D3DBuffer.Description.SizeInBytes;
                    break;
                case BufferType.Index:
                    GorgonRenderStatistics.IndexBufferCount++;
                    GorgonRenderStatistics.IndexBufferSize += D3DBuffer.Description.SizeInBytes;
                    break;
                case BufferType.Vertex:
                    GorgonRenderStatistics.VertexBufferCount++;
                    GorgonRenderStatistics.VertexBufferSize += D3DBuffer.Description.SizeInBytes;
                    break;
                case BufferType.Structured:
                    GorgonRenderStatistics.StructuredBufferCount++;
                    GorgonRenderStatistics.StructuredBufferSize += D3DBuffer.Description.SizeInBytes;
                    break;
                default:
                    GorgonRenderStatistics.BufferCount++;
                    GorgonRenderStatistics.BufferSize += D3DBuffer.Description.SizeInBytes;
                    break;
            }

			if (IsRenderTarget)
			{
				GorgonRenderStatistics.RenderTargetCount++;
				GorgonRenderStatistics.RenderTargetSize += D3DBuffer.Description.SizeInBytes;
			}

            // Create any default shader views that are required.
            OnCreateDefaultShaderView();
        }

        /// <summary>
        /// Function to return this buffer as a staging buffer.
        /// </summary>
        /// <returns>The new staging buffer.</returns>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the video device is a SM2_a_b video device.
        /// <para>-or-</para>
        /// <para>Thrown when the current buffer is immutable.</para>
        /// </exception>
        /// <remarks>Staging buffers are generic buffers with a staging usage applied.  
		/// <para>SM2_a_b video devices only support Vertex, Index and Constant buffers.  Therefore this method will throw an exception on those devices.</para></remarks>
        public GorgonBuffer GetStagingBuffer()
        {
            var stagingBuffer = Graphics.Buffers.CreateBuffer(Name + " (Staging)", new GorgonBufferSettings
            {
                AllowIndirectArguments = false,
                AllowRawViews = false,
                AllowShaderViews = false,
                AllowUnorderedAccessViews = false,
                DefaultShaderViewFormat = BufferFormat.Unknown,
                IsOutput = false,
                SizeInBytes = SizeInBytes,
                Usage = BufferUsage.Staging
            });

            // Copy the contents of this buffer to our staging buffer.
            stagingBuffer.Copy(this);

            return stagingBuffer;
        }

        /// <summary>
        /// Function to copy the contents of the specified buffer to this buffer.
        /// </summary>
        /// <param name="buffer">Buffer to copy.</param>
        /// <remarks>This is used to copy data from one GPU buffer to another.  The size of the buffers must be the same.</remarks>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="buffer"/> size is not equal to the size of this buffer.</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when this buffer has a usage of Immutable.</exception>
        public void Copy(GorgonBaseBuffer buffer)
        {
#if DEBUG
            if (buffer.SizeInBytes != SizeInBytes)
            {
                throw new ArgumentException(Resources.GORGFX_BUFFER_SIZE_MISMATCH, "buffer");
            }

            if (Settings.Usage == GorgonLibrary.Graphics.BufferUsage.Immutable)
            {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_BUFFER_IMMUTABLE);
            }
#endif
            Graphics.Context.CopyResource(buffer.D3DResource, D3DResource);
        }

        /// <summary>
        /// Function to copy the contents of the specified buffer to this buffer.
        /// </summary>
        /// <param name="buffer">Buffer to copy.</param>
        /// <param name="sourceStartingIndex">Starting byte index to start copying from.</param>
        /// <param name="byteCount">The number of bytes to copy.</param>
        /// <remarks>This is used to copy data from one GPU buffer to another.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="sourceStartingIndex"/> is less than 0 or larger than the size of the source <paramref name="buffer"/>.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="byteCount"/> + sourceStartIndex is greater than the size of the source buffer, or less than 0.</para>
        /// </exception>
        /// <exception cref="System.InvalidOperationException">Thrown when this buffer has a usage of Immutable.</exception>
        public void Copy(GorgonBaseBuffer buffer, int sourceStartingIndex, int byteCount)
        {
            Copy(buffer, sourceStartingIndex, byteCount, 0);
        }

        /// <summary>
        /// Function to copy the contents of the specified buffer to this buffer.
        /// </summary>
        /// <param name="buffer">Buffer to copy.</param>
        /// <param name="sourceStartingIndex">Starting byte index to start copying from.</param>
        /// <param name="byteCount">The number of bytes to copy.</param>
        /// <param name="destOffset">The offset within the destination buffer.</param>
        /// <remarks>This is used to copy data from one GPU buffer to another.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="sourceStartingIndex"/> is less than 0 or larger than the size of the source <paramref name="buffer"/>.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="byteCount"/> + sourceStartIndex is greater than the size of the source buffer, or less than 0.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="destOffset"/> + byteCount is greater than the size of this buffer, or less than 0.</para>
        /// </exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when this buffer has a usage of Immutable.</exception>
        public void Copy(GorgonBaseBuffer buffer, int sourceStartingIndex, int byteCount, int destOffset)
        {
            int endByteIndex = sourceStartingIndex + byteCount;

            GorgonDebug.AssertNull(buffer, "buffer");
            GorgonDebug.AssertParamRange(sourceStartingIndex, 0, buffer.SizeInBytes, "sourceStartingIndex");
            GorgonDebug.AssertParamRange(endByteIndex, 0, buffer.SizeInBytes, "sourceStartingIndex");
            GorgonDebug.AssertParamRange(destOffset, 0, SizeInBytes, "destOffset");

#if DEBUG
            if (Settings.Usage == GorgonLibrary.Graphics.BufferUsage.Immutable)
            {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_BUFFER_IMMUTABLE);
            }
#endif
            Graphics.Context.CopySubresourceRegion(buffer.D3DResource, 0, new D3D.ResourceRegion
            {
                Top = 0,
                Bottom = 1,
                Left = sourceStartingIndex,
                Right = endByteIndex,
                Front = 0,
                Back = 1
            }, D3DResource, 0, destOffset);
        }

        /// <summary>
        /// Function to update the buffer.
        /// </summary>
        /// <param name="stream">Stream containing the data used to update the buffer.</param>
        /// <remarks>This method can only be used with buffers that have Default usage.  Other buffer usages will thrown an exception.
        /// <para>This method will respect the <see cref="GorgonLibrary.IO.GorgonDataStream.Position">Position</see> property of the data stream.  
        /// This means that it will start reading from the stream at the current position.  To read from the beginning of the stream, set the position 
        /// to 0.</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (Nothing in VB.Net).</exception>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the buffer usage is not set to default.</exception>
        public void Update(GorgonDataStream stream)
        {
            GorgonDebug.AssertNull(stream, "stream");

#if DEBUG
            if (Settings.Usage != GorgonLibrary.Graphics.BufferUsage.Default)
            {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_NOT_DEFAULT_USAGE);
            }
#endif

            OnUpdate(stream, 0, (int)(stream.Length - stream.Position));
        }

        /// <summary>
        /// Function to unlock a locked buffer.
        /// </summary>
        public void Unlock()
        {
            if (!IsLocked)
            {
                return;
            }

            OnUnlock();

            IsLocked = false;
        }

        /// <summary>
        /// Function to lock the buffer for reading/writing.
        /// </summary>
        /// <param name="lockFlags">The flags to use when locking the buffer.</param>
        /// <returns>A data stream pointing to the memory used by the buffer.</returns>
        /// <remarks>A data stream locked with this method does not have to be disposed of.  After it is <see cref="GorgonLibrary.Graphics.GorgonBaseBuffer.Unlock">unlocked</see>, the memory pointed 
        /// at by the stream will be considered invalid.  However, for the sake of following practice, it is a good idea to call the Dispose method 
        /// on the resulting data stream when finished.
        /// <para>This method only works on buffers with a Dynamic or Staging usage.  Immutable or default buffers will throw an exception when an attempt 
        /// is made to lock them.</para>
        /// <para>Some buffers may raise an exception with locking with certain <paramref name="lockFlags"/>.  This is dependant upon the type of buffer.</para>
        /// </remarks>
        /// <exception cref="GorgonLibrary.GorgonException">Thrown when the buffer is already locked.
        /// <para>-or-</para>
        /// <para>Thrown when the usage for the buffer does not allow the buffer to be locked.</para>		
        /// </exception>		
        /// <exception cref="System.ArgumentException">Thrown when a constant buffer is locked with any other flag other than Discard.
        /// <para>-or-</para>
        /// <para>Thrown when an index/vertex buffer is locked with with a read flag, or a write flag without discard or nooverwrite.</para>
        /// </exception>
        public GorgonDataStream Lock(BufferLockFlags lockFlags)
        {
#if DEBUG
            if (IsLocked)
            {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_BUFFER_ALREADY_LOCKED);
            }

            if ((Settings.Usage == BufferUsage.Default) || (Settings.Usage == BufferUsage.Immutable))
            {
                throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_BUFFER_USAGE_CANT_LOCK, Settings.Usage));
            }
#endif

            GorgonDataStream result = OnLock(lockFlags);

            IsLocked = true;

            return result;
        }

		/// <summary>
		/// Function to return the default shader view for a buffer.
		/// </summary>
		/// <param name="buffer">Buffer to evaluate.</param>
		/// <returns>The default shader view for the buffer.</returns>
		public static GorgonShaderView ToShaderView(GorgonBaseBuffer buffer)
		{
			return buffer == null ? null : buffer.DefaultShaderView;
		}

		/// <summary>
		/// Implicit operator to return the default shader view for a buffer.
		/// </summary>
		/// <param name="buffer">Buffer to evaluate.</param>
		/// <returns>The default shader view for the buffer.</returns>
		public static implicit operator GorgonShaderView(GorgonBaseBuffer buffer)
		{
			return buffer == null ? null : buffer.DefaultShaderView;
		}
	    #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBaseBuffer"/> class.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="name">The name.</param>
        /// <param name="settings">The settings.</param>
        protected GorgonBaseBuffer(GorgonGraphics graphics, string name, IBufferSettings settings)
            : base(graphics, name)
        {
            Settings = settings;

            D3DUsage = (D3D.ResourceUsage)settings.Usage;

            // Determine access rights.
            switch (settings.Usage)
            {
                case BufferUsage.Dynamic:
                    D3DCPUAccessFlags = D3D.CpuAccessFlags.Write;
                    break;
                case BufferUsage.Immutable:
                    D3DCPUAccessFlags = D3D.CpuAccessFlags.None;
                    break;
                case BufferUsage.Staging:
                    D3DCPUAccessFlags = D3D.CpuAccessFlags.Write | D3D.CpuAccessFlags.Read;
                    break;
                default:
                    D3DCPUAccessFlags = D3D.CpuAccessFlags.None;
                    break;
            }
        }
        #endregion
    }
}
