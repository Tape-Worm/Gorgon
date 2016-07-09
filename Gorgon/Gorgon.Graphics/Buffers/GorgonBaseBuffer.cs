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
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Properties;
using Gorgon.Native;
using Gorgon.UI;
using SharpDX.DXGI;
using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
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
        private GorgonViewCache _viewCache;         // Cache used to hold views for the resource.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the D3D CPU access flags.
        /// </summary>
        private D3D11.CpuAccessFlags D3DCPUAccessFlags
        {
            get;
        }

        /// <summary>
        /// Property to set or return the D3D usages.
        /// </summary>
        private D3D11.ResourceUsage D3DUsage
        {
            get;
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
        internal D3D11.Buffer D3DBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the type of data in the resource.
        /// </summary>
        public override ResourceType ResourceType => ResourceType.Buffer;

	    /// <summary>
        /// Property to return the size of the resource, in bytes.
        /// </summary>
        public new int SizeInBytes => Settings.SizeInBytes;

	    /// <summary>
        /// Property to return the type of buffer.
        /// </summary>
        public abstract BufferType BufferType
        {
            get;
        }

        /// <summary>
        /// Property to return the settings for the buffer.
        /// </summary>
        public IBufferSettings Settings
        {
            get;
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
        private void CreateResources(IGorgonPointer data)
        {
            var desc = new D3D11.BufferDescription
            {
                BindFlags = D3D11.BindFlags.None,
                CpuAccessFlags = D3DCPUAccessFlags,
                OptionFlags = D3D11.ResourceOptionFlags.None,
                SizeInBytes = Settings.SizeInBytes,
                StructureByteStride = Settings.StructureSize,
                Usage = D3DUsage
            };

			// If this is a render target buffer, then ensure that we have a default resource.
			if ((IsRenderTarget) && (Settings.Usage != BufferUsage.Default))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_RENDERTARGET_NEED_DEFAULT);
			}

            // Set up the buffer.  If we're a staging buffer, then none of this stuff will 
			// work because staging buffers can't be bound to the pipeline, so just skip it.
	        if (Settings.Usage != BufferUsage.Staging)
	        {
		        switch (BufferType)
		        {
			        case BufferType.Constant:
				        desc.BindFlags = D3D11.BindFlags.ConstantBuffer;
				        break;
			        case BufferType.Index:
				        desc.BindFlags = D3D11.BindFlags.IndexBuffer;
				        break;
			        case BufferType.Vertex:
				        desc.BindFlags = D3D11.BindFlags.VertexBuffer;
				        break;
			        case BufferType.Structured:
				        if (!IsRenderTarget)
				        {
					        desc.OptionFlags = D3D11.ResourceOptionFlags.BufferStructured;
				        }
				        break;
		        }

		        // Update binding modifiers.
		        if (Settings.AllowShaderViews)
		        {
			        desc.BindFlags |= D3D11.BindFlags.ShaderResource;
		        }

		        if (Settings.AllowUnorderedAccessViews)
		        {
			        desc.BindFlags |= D3D11.BindFlags.UnorderedAccess;
		        }

		        if (!IsRenderTarget)
		        {
			        if (Settings.AllowIndirectArguments)
			        {
				        desc.OptionFlags = D3D11.ResourceOptionFlags.DrawIndirectArguments;
			        }
		        }
		        else
		        {
			        desc.BindFlags |= D3D11.BindFlags.RenderTarget;
		        }

				if (Settings.IsOutput)
				{
					desc.BindFlags |= D3D11.BindFlags.StreamOutput;
				}

		        if (Settings.AllowRawViews)
		        {
			        desc.OptionFlags = D3D11.ResourceOptionFlags.BufferAllowRawViews;
		        }
	        }

			// Create and initialize the buffer.
	        if (data != null)
            {
                D3DResource = D3DBuffer = new D3D11.Buffer(VideoDevice.D3DDevice(), new IntPtr(data.Address), desc);
            }
            else
            {
                D3DResource = D3DBuffer = new D3D11.Buffer(VideoDevice.D3DDevice(), desc);
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

			var info = new GorgonFormatInfo((Format)Settings.DefaultShaderViewFormat);
	        DefaultShaderView = OnGetShaderView(Settings.DefaultShaderViewFormat, 0,
	                                               Settings.SizeInBytes / info.SizeInBytes, false);
        }

        /// <summary>
        /// Function to create a default render target view.
        /// </summary>
        protected virtual void OnCreateDefaultRenderTargetView()
        {
            // Implemented on render target buffer.
        }

        /// <summary>
        /// Function to clean up the resource object.
        /// </summary>
        protected override void CleanUpResource()
        {
            if (D3DResource == null)
            {
                return;
            }
            
            if (_viewCache != null)
            {
                _viewCache.Dispose();
                _viewCache = null;
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

            GorgonApplication.Log.Print("Destroyed {0} Buffer '{1}'.", LoggingLevel.Verbose, BufferType, Name);
        }

		/// <summary>
		/// Function to retrieve the mapping mode for locking a buffer.
		/// </summary>
		/// <param name="flags">Flags to use when locking.</param>
		/// <returns>The D3D mapping mode.</returns>
	    internal static D3D11.MapMode GetMapMode(BufferLockFlags flags)
	    {
			if ((flags & BufferLockFlags.Read) == BufferLockFlags.Read)
			{
				return (flags & BufferLockFlags.Write) == BufferLockFlags.Write ? D3D11.MapMode.ReadWrite : D3D11.MapMode.Read;
			}

			if ((flags & BufferLockFlags.Discard) == BufferLockFlags.Discard)
			{
				return D3D11.MapMode.WriteDiscard;
			}

			return (flags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite
				? D3D11.MapMode.WriteNoOverwrite
				: D3D11.MapMode.Write;
	    }

        /// <summary>
        /// Function used to lock the underlying buffer for reading/writing.
        /// </summary>
        /// <param name="lockFlags">Flags used when locking the buffer.</param>
        /// <param name="context">A graphics context to use when locking the buffer.</param>
        /// <returns>A <see cref="GorgonPointerAlias"/> representing a pointer to the buffer data.</returns>		
        /// <remarks>
        /// Use the <paramref name="context"/> parameter to determine the context in which the buffer should be updated. This is necessary to use that context 
        /// to update the buffer because 2 threads may not access the same resource at the same time.  
        /// </remarks>
        protected virtual GorgonPointerAlias OnLock(BufferLockFlags lockFlags, GorgonGraphics context)
        {
#if DEBUG
            if ((lockFlags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite)
            {
                throw new ArgumentException(Resources.GORGFX_BUFFER_NO_OVERWRITE_NOT_VALID, nameof(lockFlags));
            }
#endif
            DX.DataStream lockStream;

            context.D3DDeviceContext.MapSubresource(D3DBuffer, GetMapMode(lockFlags), D3D11.MapFlags.None, out lockStream);

            return new GorgonPointerAlias(lockStream.DataPointer, lockStream.Length);
        }

        /// <summary>
        /// Function to update the buffer.
        /// </summary>
        /// <param name="data">Pointer to the data used to update the buffer.</param>
        /// <param name="offset">Offset, in bytes, into the buffer to start writing at.</param>
        /// <param name="size">The number of bytes to write.</param>
        /// <param name="context">A graphics context to use when updating the buffer.</param>
        /// <remarks>
        /// Use the <paramref name="context"/> parameter to determine the context in which the buffer should be updated. This is necessary to use that context 
        /// to update the buffer because 2 threads may not access the same resource at the same time.  
        /// </remarks>
        protected virtual void OnUpdate(GorgonPointerBase data, int offset, int size, GorgonGraphics context)
        {
            context.D3DDeviceContext.UpdateSubresource(
                new DX.DataBox
                {
                    DataPointer = new IntPtr(data.Address),
                    RowPitch = size,
                    SlicePitch = 0
                },
                D3DResource);
        }

        /// <summary>
        /// Function to retrieve an unordered access view for this buffer.
        /// </summary>
        /// <param name="format">Format of the buffer.</param>
        /// <param name="start">First element to map to the view.</param>
        /// <param name="count">The number of elements to map to the view.</param>
        /// <param name="isRaw"><b>true</b> if using a raw view to the buffer, <b>false</b> if not.</param>
        /// <param name="viewType">The type of view for a structured buffer.</param>
        /// <returns>An unordered access view for the buffer.</returns>
        /// <remarks>Use this to create/retrieve an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a Shader View, only one 
        /// unordered access view can be bound to the pipeline at any given time.
        /// <para>Raw views require that the format be set to <see cref="BufferFormat.R32_Typeless"/>.</para>
        /// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
        /// </remarks>
        /// <exception cref="GorgonException">Thrown when the usage for this buffer is set to Staging or Dynamic.
        /// <para>-or-</para>
        /// <para>Thrown when the video device feature level is not SM_5 or better.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the resource settings do not allow unordered access views.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the view could not be created.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="start"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
        /// number of elements in the buffer.</exception>
        protected GorgonBufferUnorderedAccessView OnGetUnorderedAccessView(BufferFormat format, int start, int count, bool isRaw, UnorderedAccessViewType viewType)
        {
            int elementCount;

            if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
            }

            if (!Settings.AllowUnorderedAccessViews)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_VIEW_NO_SUPPORT, "GorgonUnorderedAccessView"));
            }

            if ((Settings.Usage == BufferUsage.Staging) || (Settings.Usage == BufferUsage.Dynamic))
            {
                throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_VIEW_UNORDERED_NO_STAGING_DYNAMIC);
            }

            if (BufferType != BufferType.Structured)
            {
                if (format == BufferFormat.Unknown)
                {
                    throw new ArgumentException(Resources.GORGFX_VIEW_UNKNOWN_FORMAT, nameof(format));
                }

                if (Settings.AllowRawViews)
                {
                    if (format != BufferFormat.R32_Typeless)
                    {
                        throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_VIEW_UNORDERED_RAW_INVALID_FORMAT);
                    }

                    elementCount = SizeInBytes / 4;
                }
                else
                {
                    // Ensure the size of the data type fits the requested format.
                    var info = new GorgonFormatInfo((Format)format);
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

	        if (BufferType == BufferType.Structured)
	        {
		        return (GorgonBufferUnorderedAccessView)_viewCache.GetUnorderedAccessView(format, start, count, 0, viewType, isRaw);
	        }

			if ((VideoDevice.GetBufferFormatSupport((Format)format) & D3D11.FormatSupport.TypedUnorderedAccessView) != D3D11.FormatSupport.TypedUnorderedAccessView)
	        {
		        throw new GorgonException(GorgonResult.CannotCreate,
			        string.Format(Resources.GORGFX_VIEW_FORMAT_NOT_SUPPORTED, format));
	        }

	        return (GorgonBufferUnorderedAccessView)_viewCache.GetUnorderedAccessView(format, start, count, 0, viewType, isRaw);
        }

        /// <summary>
        /// Function to retrieve a new shader view for the buffer.
        /// </summary>
        /// <param name="format">The format of the view.</param>
        /// <param name="start">Starting element.</param>
        /// <param name="count">Element count.</param>
        /// <param name="isRaw"><b>true</b> if using a raw view of the buffer, <b>false</b> if not.</param>
        /// <returns>A shader view for the buffer.</returns>
        /// <exception cref="GorgonException">Thrown when the usage for this buffer is set to Staging.
        /// <para>-or-</para>
        /// <para>Thrown when the view could not be created.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="start"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
        /// number of elements in the buffer.</exception>
        /// <remarks>Use this to create/retrieve additional shader views for the buffer.  Multiple views of the same resource can be bound to multiple stages in the pipeline.
        /// <para>Raw views require that the buffer be created with the <see cref="Gorgon.Graphics.IBufferSettings.AllowRawViews">AllowRawViews</see> property set to <b>true</b> in its settings.</para>
        /// <para>Raw views can only be used on SM5 video devices or better. </para>
        /// <para>This function only applies to buffers that have not been created with a Usage of Staging.</para>
        /// </remarks>
        protected GorgonBufferShaderView OnGetShaderView(BufferFormat format, int start, int count, bool isRaw)
        {
            int elementCount;

            if (Settings.Usage == BufferUsage.Staging)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_VIEW_SRV_NO_STAGING_OR_DYNAMIC);
            }

            if (!Settings.AllowShaderViews)
            {
                throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_VIEW_NO_SUPPORT, "GorgonShaderView"));
            }

            // If we're using any other type than structured...
            if (BufferType != BufferType.Structured)
            {
                if (format == BufferFormat.Unknown)
                {
                    throw new ArgumentException(Resources.GORGFX_VIEW_UNKNOWN_FORMAT, nameof(format));
                }

                // Ensure the size of the data type fits the requested format.
                var info = new GorgonFormatInfo((Format)format);

                if (info.IsTypeless)
                {
                    throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_VIEW_NO_TYPELESS);
                }

                if (isRaw)
                {
                    if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                    {
                        throw new GorgonException(GorgonResult.CannotCreate,
                                                  string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
                    }

                    if (!Settings.AllowRawViews)
                    {
                        throw new GorgonException(GorgonResult.CannotBind, Resources.GORGFX_BUFFER_NO_RAW_VIEWS);
                    }

                    if (info.Group != (Format)BufferFormat.R32_Typeless)
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
                if (VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                              string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
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

            return _viewCache.GetBufferView(format, start, count, isRaw);
        }

        /// <summary>
        /// Function used to initialize the buffer with data.
        /// </summary>
        /// <param name="data">Data to write.</param>
        /// <remarks>Passing NULL (<i>Nothing</i> in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
        internal void Initialize(IGorgonPointer data)
        {
            GorgonApplication.Log.Print("Creating {0} Buffer '{1}'...", LoggingLevel.Verbose, BufferType, Name);

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

            if (IsRenderTarget)
            {
                OnCreateDefaultRenderTargetView();
            }
        }

        /// <summary>
        /// Function to return this buffer as a staging buffer.
        /// </summary>
        /// <returns>The new staging buffer.</returns>
        /// <exception cref="GorgonException">Thrown when the video device is a SM2_a_b video device.
        /// <para>-or-</para>
        /// <para>Thrown when the current buffer is immutable.</para>
        /// </exception>
        /// <remarks>Staging buffers are generic buffers with a staging usage applied.  
		/// <para>SM2_a_b video devices only support Vertex, Index and Constant buffers.  Therefore this method will throw an exception on those devices.</para></remarks>
        public GorgonBuffer_OLDEN GetStagingBuffer()
        {
#warning FAILURE: This will not run.
	        GorgonBuffer_OLDEN stagingBufferOlden = null;/* Graphics.Buffers.CreateBuffer(Name + " (Staging)", new GorgonBufferSettings
            {
                AllowIndirectArguments = false,
                AllowRawViews = false,
                AllowShaderViews = false,
                AllowUnorderedAccessViews = false,
                DefaultShaderViewFormat = BufferFormat.Unknown,
                IsOutput = false,
                SizeInBytes = SizeInBytes,
                Usage = BufferUsage.Staging
            });*/

            // Copy the contents of this buffer to our staging buffer.
            stagingBufferOlden.Copy(this);

            return stagingBufferOlden;
        }

        /// <summary>
        /// Function to copy the contents of the specified buffer to this buffer.
        /// </summary>
        /// <param name="buffer">Buffer to copy.</param>
        /// <remarks>This is used to copy data from one GPU buffer to another.  The size of the buffers must be the same.</remarks>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="buffer"/> size is not equal to the size of this buffer.</exception>
        /// <exception cref="GorgonException">Thrown when this buffer has a usage of Immutable.</exception>
        public void Copy(GorgonBaseBuffer buffer)
        {
#if DEBUG
            if (buffer.SizeInBytes != SizeInBytes)
            {
                throw new ArgumentException(Resources.GORGFX_BUFFER_SIZE_MISMATCH, nameof(buffer));
            }

            if (Settings.Usage == BufferUsage.Immutable)
            {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_BUFFER_IMMUTABLE);
            }
#endif
            VideoDevice.D3DDeviceContext().CopyResource(buffer.D3DResource, D3DResource);
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
        /// <param name="sourceOffset">Starting byte index to start copying from.</param>
        /// <param name="byteCount">The number of bytes to copy.</param>
        /// <param name="destOffset">The offset within the destination buffer.</param>
        /// <remarks>This is used to copy data from one GPU buffer to another.</remarks>
        /// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="sourceOffset"/> is less than 0 or larger than the size of the source <paramref name="buffer"/>.
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="byteCount"/> + sourceStartIndex is greater than the size of the source buffer, or less than 0.</para>
        /// <para>-or-</para>
        /// <para>Thrown when the <paramref name="destOffset"/> + byteCount is greater than the size of this buffer, or less than 0.</para>
        /// </exception>
        /// <exception cref="GorgonException">Thrown when this buffer has a usage of Immutable.</exception>
        public void Copy(GorgonBaseBuffer buffer, int sourceOffset, int byteCount, int destOffset)
        {
            int sourceByteIndex = sourceOffset + byteCount;
            int destByteIndex = destOffset + byteCount;

			buffer.ValidateObject("buffer");
            sourceOffset.ValidateRange("sourceOffset", 0, buffer.SizeInBytes);
			sourceByteIndex.ValidateRange("sourceOffset", 0, buffer.SizeInBytes);
            destOffset.ValidateRange("destOffset", 0, SizeInBytes);
			destByteIndex.ValidateRange("destOffset", 0, buffer.SizeInBytes);

#if DEBUG
            if (Settings.Usage == BufferUsage.Immutable)
            {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_BUFFER_IMMUTABLE);
            }
#endif
            VideoDevice.D3DDeviceContext().CopySubresourceRegion(buffer.D3DResource, 0, new D3D11.ResourceRegion
            {
                Top = 0,
                Bottom = 1,
                Left = sourceOffset,
                Right = sourceByteIndex,
                Front = 0,
                Back = 1
            }, D3DResource, 0, destOffset);
        }

		/// <summary>
		/// Function to write a single value type to the buffer.
		/// </summary>
		/// <typeparam name="T">Type of value type.</typeparam>
		/// <param name="data">Value type data to write into the buffer.</param>
		/// <param name="deferred">[Optional] A deferred context to use when updating the buffer.</param>
		/// <remarks>
		/// <para>
		/// This overload is useful for directly copying a value into the buffer without needing a pointer.  If the type of value is a struct and contains reference types (arrays, strings, and objects), 
		/// then these members will not be copied.  Some form of marshalling will be required in order to copy structures with reference types.  
		/// </para>
		/// <para>
		/// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to update the buffer.  If it is non-NULL, then it 
		/// will use the specified deferred context to clear the render target.
		/// <para>If you are using a deferred context, it is necessary to use that context to update the buffer because 2 threads may not access the same resource at the same time.  
		/// Passing a separate deferred context will alleviate that.</para>
		/// </para>
		/// <para>This will only work on buffers created with a usage type of [Default].</para>
		/// </remarks>
		/// <exception cref="GorgonException">Thrown when the buffer does not have a usage of Default.</exception>
		public void Update<T>(ref T data, GorgonGraphics deferred = null)
			where T : struct
		{
#if DEBUG
			if (Settings.Usage != BufferUsage.Default)
			{
				throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_NOT_DEFAULT_USAGE);
			}
#endif

#warning FAILURE: This will not run.
			if (deferred == null)
            {
                //deferred = Graphics;
            }

            deferred.D3DDeviceContext.UpdateSubresource(ref data, D3DResource, 0, DirectAccess.SizeOf<T>());
		}


		/// <summary>
		/// Function to write an array of value types to the buffer.
		/// </summary>
		/// <typeparam name="T">Type of value type.</typeparam>
		/// <param name="data">Value type data to write into the buffer.</param>
		/// <param name="deferred">[Optional] A deferred context to use when updating the buffer.</param>
		/// <remarks>
		/// <para>
		/// This overload is useful for directly copying a value into the buffer without needing a pointer.  If the type of value is a struct and contains reference types (arrays, strings, and objects), 
		/// then these members will not be copied.  Some form of marshalling will be required in order to copy structures with reference types.  
		/// </para>
		/// <para>
		/// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to update the buffer.  If it is non-NULL, then it 
		/// will use the specified deferred context to clear the render target.
		/// <para>If you are using a deferred context, it is necessary to use that context to update the buffer because 2 threads may not access the same resource at the same time.  
		/// Passing a separate deferred context will alleviate that.</para>
		/// </para>
		/// <para>This will only work on buffers created with a usage type of [Default].</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="data"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="GorgonException">Thrown when the buffer does not have a usage of Default.</exception>
		public virtual void Update<T>(T[] data, GorgonGraphics deferred = null)
			where T : struct
		{
			data.ValidateObject("data");

#if DEBUG
			if (Settings.Usage != BufferUsage.Default)
			{
				throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_NOT_DEFAULT_USAGE);
			}
#endif

#warning FAILURE: This will not run.
			if (deferred == null)
            {
                //deferred = Graphics;
            }

			deferred.D3DDeviceContext.UpdateSubresource(data, D3DResource, 0, DirectAccess.SizeOf<T>() * data.Length, 0,
				new D3D11.ResourceRegion(0, 0, 0, SizeInBytes, 1, 1));
		}
		
		/// <summary>
        /// Function to update the buffer.
        /// </summary>
        /// <param name="data">Pointer to the data used to update the buffer.</param>
        /// <param name="deferred">[Optional] A deferred context to use when updating the buffer.</param>
        /// <remarks>This method can only be used with buffers that have Default usage.  Other buffer usages will thrown an exception.
        /// <para>
        /// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to update the buffer.  If it is non-NULL, then it 
        /// will use the specified deferred context to clear the render target.
        /// <para>If you are using a deferred context, it is necessary to use that context to update the buffer because 2 threads may not access the same resource at the same time.  
        /// Passing a separate deferred context will alleviate that.</para>
        /// </para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="data"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="GorgonException">Thrown when the buffer usage is not set to default.</exception>
        public void Update(GorgonPointerBase data, GorgonGraphics deferred = null)
        {
			data.ValidateObject(nameof(data));

#if DEBUG
            if (Settings.Usage != BufferUsage.Default)
            {
                throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_NOT_DEFAULT_USAGE);
            }
#endif

#warning FAILURE: This will not run.
			if (deferred == null)
            {
                //deferred = Graphics;
            }

            OnUpdate(data, 0, (int)data.Size, deferred);
        }

        /// <summary>
        /// Function to unlock a locked buffer.
        /// </summary>
        /// <param name="deferred">[Optional] The deferred context that was used to lock the buffer.</param>
        /// <remarks>If <paramref name="deferred"/> is NULL (<i>Nothing</i> in VB.Net), then the immediate graphics context will be used to unlock the buffer.  Otherwise, the specified deferred context will be used 
        /// to unlock the buffer.
        /// <para>Ensure that the context used to lock the buffer is the same as the one passed to <paramref name="deferred"/>.</para>
        /// </remarks>
        public void Unlock(GorgonGraphics deferred = null)
        {
#warning FAILURE: This will not run.
			if (deferred == null)
            {
                //deferred = Graphics;
            }

            deferred.D3DDeviceContext.UnmapSubresource(D3DBuffer, 0);
        }

		/// <summary>
		/// Function to lock the buffer for reading/writing.
		/// </summary>
		/// <param name="lockFlags">The flags to use when locking the buffer.</param>
		/// <param name="deferred">[Optional] A deferred context to use when locking the buffer.</param>
		/// <returns>A <see cref="GorgonPointerAlias"/> representing a pointer to the buffer data.</returns>		
		/// <remarks>A data stream locked with this method does not have to be disposed of.  After it is <see cref="Unlock">unlocked</see>, the memory pointed 
		/// at by the stream will be considered invalid.  However, for the sake of following practice, it is a good idea to call the Dispose method 
		/// on the resulting data stream when finished.
		/// <para>This method only works on buffers with a Dynamic or Staging usage.  Immutable or default buffers will throw an exception when an attempt 
		/// is made to lock them.</para>
		/// <para>Some buffers may raise an exception with locking with certain <paramref name="lockFlags"/>.  This is dependant upon the type of buffer.</para>
		/// <para>
		/// If the <paramref name="deferred"/> parameter is NULL (<i>Nothing</i> in VB.Net), the immediate context will be used to lock the buffer.  If it is non-NULL, then it 
		/// will use the specified deferred context to clear the render target.
		/// <para>If you are using a deferred context, it is necessary to use that context to lock the buffer because 2 threads may not access the same resource at the same time.  
		/// Passing a separate deferred context will alleviate that.</para>
		/// <para>When locking using a deferred context, either the NoOverwrite or Discard flags must be provided along with a Write flag.  No other flags will work.</para>
		/// </para>
		/// </remarks>
		/// <exception cref="GorgonException">Thrown when the buffer is already locked.
		/// <para>-or-</para>
		/// <para>Thrown when the usage for the buffer does not allow the buffer to be locked.</para>		
		/// </exception>		
		/// <exception cref="System.ArgumentException">Thrown when a buffer is locked with with a <see cref="BufferLockFlags.Read"/> flag combined with <see cref="BufferLockFlags.Discard"/> or <see cref="BufferLockFlags.NoOverwrite"/>.
		/// <para>-or-</para>
		/// <para>Thrown when the lock is used with a deferred graphics context and the NoOverwrite and/or the Discard flags are not present.</para>
		/// </exception>
		public GorgonPointerAlias Lock(BufferLockFlags lockFlags, GorgonGraphics deferred = null)
        {
#if DEBUG
			if ((Settings.Usage != BufferUsage.Staging) && (Settings.Usage != BufferUsage.Dynamic))
			{
				throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GORGFX_BUFFER_USAGE_CANT_LOCK, Settings.Usage));
			}

			if ((Settings.Usage != BufferUsage.Staging) && ((lockFlags & BufferLockFlags.Read) == BufferLockFlags.Read))
			{
				throw new GorgonException(GorgonResult.AccessDenied, Resources.GORGFX_LOCK_CANNOT_READ_NON_STAGING);
			}

			if ((((lockFlags & BufferLockFlags.NoOverwrite) == BufferLockFlags.NoOverwrite)
				|| ((lockFlags & BufferLockFlags.Discard) == BufferLockFlags.Discard))
				&& ((lockFlags & BufferLockFlags.Read) == BufferLockFlags.Read))
			{
				throw new ArgumentException(Resources.GORGFX_LOCK_CANNOT_USE_WITH_READ, nameof(lockFlags));
			}

#endif

#warning FAILURE: This will not run.
			if (deferred == null)
            {
                //deferred = Graphics;
            }

            var result = OnLock(lockFlags, deferred);

            return result;
        }

		/// <summary>
		/// Function to return the default shader view for a buffer.
		/// </summary>
		/// <param name="buffer">Buffer to evaluate.</param>
		/// <returns>The default shader view for the buffer.</returns>
		public static GorgonShaderView ToShaderView(GorgonBaseBuffer buffer)
		{
			return buffer?.DefaultShaderView;
		}

		/// <summary>
		/// Implicit operator to return the default shader view for a buffer.
		/// </summary>
		/// <param name="buffer">Buffer to evaluate.</param>
		/// <returns>The default shader view for the buffer.</returns>
		public static implicit operator GorgonShaderView(GorgonBaseBuffer buffer)
		{
			return buffer?.DefaultShaderView;
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
            : base(graphics.VideoDevice, name)
        {
			_viewCache = new GorgonViewCache(this);

            Settings = settings;

            D3DUsage = (D3D11.ResourceUsage)settings.Usage;

            // Determine access rights.
            switch (settings.Usage)
            {
                case BufferUsage.Dynamic:
                    D3DCPUAccessFlags = D3D11.CpuAccessFlags.Write;
                    break;
                case BufferUsage.Immutable:
                    D3DCPUAccessFlags = D3D11.CpuAccessFlags.None;
                    break;
                case BufferUsage.Staging:
                    D3DCPUAccessFlags = D3D11.CpuAccessFlags.Write | D3D11.CpuAccessFlags.Read;
                    break;
                default:
                    D3DCPUAccessFlags = D3D11.CpuAccessFlags.None;
                    break;
            }
        }
        #endregion
    }
}
