#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Sunday, February 12, 2012 7:37:43 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GorgonLibrary.Diagnostics;
using SharpDX.Direct3D;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A shader resource view.
	/// </summary>
	/// <remarks>Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow 
	/// the resource to be cast to any format within the same group.</remarks>
	public abstract class GorgonShaderView
		: IDisposable
	{
		#region Variables.
		private bool _disposed;				// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct3D shader resource view.
		/// </summary>
		internal D3D.ShaderResourceView D3DView
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the resource that the view is bound with.
		/// </summary>
		public GorgonResource Resource
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to return the format for the view.
        /// </summary>
        public BufferFormat Format
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return information about the view format.
		/// </summary>
		public GorgonBufferFormatInfo.GorgonFormatData FormatInformation
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				// Unbind this view since we're destroying it.
				Resource.Graphics.Shaders.Unbind(this);

				if (D3DView != null)
				{
					Gorgon.Log.Print("Destroying shader resource view for {0}.", LoggingLevel.Verbose, Resource.GetType().FullName);
					D3DView.Dispose();
				}

				D3DView = null;
			}

			_disposed = true;
		}

	    /// <summary>
	    /// Function to perform initialization of the shader view resource.
	    /// </summary>
	    protected abstract void InitializeImpl();

        /// <summary>
        /// Function to perform initialization of the shader view resource.
        /// </summary>
        internal void Initialize()
        {
            Gorgon.Log.Print("Creating shader resource view for {0}.", LoggingLevel.Verbose, Resource.GetType().FullName);
            InitializeImpl();
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderView"/> class.
		/// </summary>
		/// <param name="resource">The buffer to bind to the view.</param>
		/// <param name="format">The format of the view.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="resource"/> parameter is NULL (Nothing in VB.Net).</exception>
		protected GorgonShaderView(GorgonResource resource, BufferFormat format)
		{
			if (resource == null)
			{
				throw new ArgumentNullException("resource");
			}

			Resource = resource;
		    Format = format;
			FormatInformation = GorgonBufferFormatInfo.GetInfo(Format);
		}
		#endregion
	
		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public void Dispose()
		{
 			Dispose(true);
		}
		#endregion
	}

	/// <summary>
	/// A shader view for buffers.
	/// </summary>
	/// <remarks>Use a resource view to allow a shader access to the contents of a resource (or sub resource).  When the resource is created with a typeless format, this will allow 
	/// the resource to be cast to any format within the same group.</remarks>
	public sealed class GorgonBufferShaderView
		: GorgonShaderView
	{
		#region Variables.

		#endregion

		#region Properties.
        /// <summary>
        /// Property to return the offset of the view from the first element in the buffer.
        /// </summary>
        public int ElementStart
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the number of elements that the view will encompass.
        /// </summary>
        public int ElementCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Propery to return whether the buffer is using raw access or not.
        /// </summary>
        public bool IsRaw
        {
            get;
            private set;
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform initialization of the shader view resource.
		/// </summary>
		protected override void InitializeImpl()
		{
		    var desc = new D3D.ShaderResourceViewDescription
		        {
		            BufferEx = new D3D.ShaderResourceViewDescription.ExtendedBufferResource
		                {
		                    FirstElement = ElementStart,
		                    ElementCount = ElementCount,
                            Flags = IsRaw ? D3D.ShaderResourceViewExtendedBufferFlags.Raw 
                                          : D3D.ShaderResourceViewExtendedBufferFlags.None
		                },
		            Dimension = ShaderResourceViewDimension.ExtendedBuffer,
		            Format = (GI.Format)Format
		        };

            D3DView = new D3D.ShaderResourceView(Resource.Graphics.D3DDevice, Resource.D3DResource, desc);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBufferShaderView"/> class.
		/// </summary>
		/// <param name="buffer">The buffer to bind to the view.</param>
		/// <param name="format">Format of the view.</param>
		/// <param name="elementStart">The starting element for the view.</param>
		/// <param name="elementCount">The number of elements in the view.</param>
		internal GorgonBufferShaderView(GorgonShaderBuffer buffer, BufferFormat format, int elementStart, int elementCount)
			: base(buffer, format)
		{
		    ElementStart = elementStart;
		    ElementCount = elementCount;
		    IsRaw = buffer.Settings.IsRaw;
		}
		#endregion
	}

    /// <summary>
    /// A cache for shader resource views.
    /// </summary>
    class GorgonSRVCache<TK, TV>
        : IDisposable
        where TV : GorgonShaderView
    {
        #region Value Types.
        /// <summary>
        /// A key for the SRV cache.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        struct BufferViewKey
            : IEquatable<BufferViewKey>
        {
            #region Variables.
            [FieldOffset(0)]
            private readonly BufferFormat _format;
            [FieldOffset(4)]
            private readonly D3D.ShaderResourceViewDescription.ExtendedBufferResource _buffer;
            [FieldOffset(4)]
            private readonly D3D.ShaderResourceViewDescription.Texture1DResource _texture1D;
            [FieldOffset(4)]
            private readonly D3D.ShaderResourceViewDescription.Texture1DArrayResource _texture1DArray;
            [FieldOffset(4)]
            private readonly D3D.ShaderResourceViewDescription.Texture2DResource _texture2D;
            [FieldOffset(4)]
            private readonly D3D.ShaderResourceViewDescription.Texture2DArrayResource _texture2DArray;
            [FieldOffset(4)]
            private readonly D3D.ShaderResourceViewDescription.Texture2DMultisampledResource _texture2DMultiSample;
            [FieldOffset(4)]
            private readonly D3D.ShaderResourceViewDescription.Texture2DMultisampledArrayResource _texture2DMultSampleArray;
            [FieldOffset(4)]
            private readonly D3D.ShaderResourceViewDescription.Texture3DResource _texture3D;
            [FieldOffset(4)]
            private readonly D3D.ShaderResourceViewDescription.TextureCubeResource _textureCube;
            [FieldOffset(4)]
            private readonly D3D.ShaderResourceViewDescription.TextureCubeArrayResource _textureCubeArray;
            #endregion

            #region Methods.
            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                return 281.GenerateHash(_texture1DArray.ArraySize).GenerateHash(_texture1DArray.FirstArraySlice).GenerateHash(_texture1DArray.MipLevels).GenerateHash(_
                return 281.GenerateHash(_format).GenerateHash(_elementStart).GenerateHash(_elementCount).GenerateHash(_raw);
            }
            #endregion

            #region Constructor.
            /// <summary>
            /// Initializes a new instance of the <see cref="BufferViewKey"/> struct.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="start">The start.</param>
            /// <param name="count">The count.</param>
            /// <param name="isRaw">if set to <c>true</c> [is raw].</param>
            public BufferViewKey(BufferFormat format, int start, int count, bool isRaw)
            {
                _format = format;
                _elementStart = start;
                _elementCount = count;
                _raw = isRaw;
            }
            #endregion
        }
        #endregion

        #region Variables.
        // ReSharper disable StaticFieldInGenericType
        private static readonly object _syncLock = new object();                        // Synchronization object for threading.
        // ReSharper restore StaticFieldInGenericType

        private bool _disposed = false;                                                 // Flag to indicate that the object is disposed.
        private GorgonResource _resource;                                               // Resource bound to the SRVs.
        private Dictionary<BufferViewKey, GorgonBufferShaderView> _bufferViews;         // The cache of buffer views.
        
        // TODO: Add texture view types.
        #endregion

        #region Methods.
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                foreach (var item in _bufferViews)
                {
                    item.Value.Dispose();
                }

                _bufferViews.Clear();
            }

            _disposed = true;
        }

        /// <summary>
        /// Function to retrieve a buffer view.
        /// </summary>
        /// <param name="format">Format of the view.</param>
        /// <param name="start">Starting element for the view.</param>
        /// <param name="count">Number of elements for the view.</param>
        /// <returns>The cached buffer shader view.</returns>
        public GorgonBufferShaderView GetBufferView(BufferFormat format, int start, int count)
        {
            var buffer = (GorgonShaderBuffer)_resource;
            var key = new BufferViewKey(format, start, count, buffer.Settings.IsRaw);

            lock(_syncLock)
            {
                GorgonBufferShaderView result;

                if (!_bufferViews.TryGetValue(key, out result))
                {
                     result = new GorgonBufferShaderView(buffer, format, start, count);
                     _bufferViews.Add(key, result);
                }

                return result;
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonSRVCache"/> class.
        /// </summary>
        /// <param name="resource">The resource used to bind the cached SRVs.</param>
        public GorgonSRVCache(GorgonResource resource)
        {
            _bufferViews = new Dictionary<BufferViewKey, GorgonBufferShaderView>();
            _resource = resource;
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

	/*/// <summary>
	/// A view of a resource that can be bound to a shader.
	/// </summary>
	/// <remarks>A view allows a resource to be handled by a shader and optionally used to reinterpret the format of a resource.</remarks>
	public sealed class GorgonResourceView
    {
		#region Properties.
		/// <summary>
		/// Property to return the Direct3D shader resource view.
		/// </summary>
		internal D3D.ShaderResourceView D3DResourceView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the Direct3D unordered access resource view.
		/// </summary>
		internal D3D.UnorderedAccessView D3DUnorderedResourceView
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the resource that is bound to this view.
		/// </summary>
		public GorgonResource Resource
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the graphics interface that owns this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the format for the shader resource.
		/// </summary>
		/// <remarks>If this value returns Unknown, then no shader view was created for this view.</remarks>
		public BufferFormat ShaderViewFormat
		{
			get
			{
				if (D3DResourceView != null)
				{
					return (BufferFormat)D3DResourceView.Description.Format;
				}

				return BufferFormat.Unknown;
			}
		}

        /// <summary>
        /// Property to return the format for the unordered access view.
        /// </summary>
        /// <remarks>If this value returns Unknown, then no unordered access view was created for this view.</remarks>
	    public BufferFormat UnorderedAccessViewFormat
	    {
	        get
	        {
	            if (D3DUnorderedResourceView != null)
	            {
	                return (BufferFormat)D3DUnorderedResourceView.Description.Format;
	            }

	            return BufferFormat.Unknown;
	        }
	    }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to build a view for a texture resource.
		/// </summary>
		/// <param name="resource">The resource to build a view for.</param>
		private void BuildTextureView(GorgonTexture resource)
		{
			bool isMultisampled = resource.Settings.Multisampling.Count > 1 || resource.Settings.Multisampling.Quality > 0;
			D3D.ShaderResourceViewDescription desc = default(D3D.ShaderResourceViewDescription);
			D3D.UnorderedAccessViewDescription uavDesc = default(D3D.UnorderedAccessViewDescription);

			// Can't bind to staging resources.
			if (resource.Settings.Usage == BufferUsage.Staging)
			{
				throw new GorgonException(GorgonResult.CannotCreate, "Cannot create a view for staging objects.");
			}
			
			// Validate shader view format.
			if (resource.ShaderViewFormatInformation.IsTypeless)
			{
				throw new GorgonException(GorgonResult.CannotCreate,
				                          "Cannot create the shader view.  The format '" + resource.Settings.ShaderView.ToString() +
				                          "' is untyped.  A view requires a typed format.");
			}

			// Ensure that the shader view is in the same bit group as the resource format.
			if ((resource.Settings.Format != resource.Settings.ShaderView)
				&& (resource.ShaderViewFormatInformation.Group != resource.FormatInformation.Group))
			{
				throw new GorgonException(GorgonResult.CannotCreate,
											"Cannot create the shader view.  The format '" + resource.Settings.Format.ToString() +
											"' and the view format '" + resource.Settings.ShaderView.ToString() +
											"' are not part of the same group.");
			}

			// Validate unordered access view formats.
			if (resource.Settings.UnorderedAccessViewFormat != BufferFormat.Unknown)
			{
				if (resource.UnorderedAccessViewFormatInformation.IsTypeless)
				{
					throw new GorgonException(GorgonResult.CannotCreate,
											  "Cannot create the unordered access view.  The format '" + resource.Settings.UnorderedAccessViewFormat.ToString() +
											  "' is untyped.  A view requires a typed format.");
				}

                if (!Graphics.VideoDevice.SupportsUnorderedAccessViewFormat(resource.Settings.UnorderedAccessViewFormat))
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                              "Cannot create the unordered access with with the format ["
                                              + resource.Settings.UnorderedAccessViewFormat
                                              + "].  This format is not supported for unordered access views.");
                }

                if (((resource.UnorderedAccessViewFormatInformation.Group != BufferFormat.R32)
                    && (resource.UnorderedAccessViewFormatInformation.Group != resource.FormatInformation.Group))
                    || (resource.UnorderedAccessViewFormatInformation.BitDepth != resource.FormatInformation.BitDepth))
                {
                    throw new GorgonException(GorgonResult.CannotCreate,
                                                "Cannot create the unordered access view.  The format [" + resource.Settings.Format +
                                                "[ and the view format [" + resource.Settings.UnorderedAccessViewFormat.ToString() +
                                                "] are not part of the same group.");
                }
            }

			desc.Format = resource.Settings.ShaderView == BufferFormat.Unknown
				              ? (GI.Format)resource.Settings.Format
				              : (GI.Format)resource.Settings.ShaderView;
			uavDesc.Format = (GI.Format)resource.Settings.UnorderedAccessViewFormat;

			// Determine view type.
			switch (resource.ResourceType)
			{
				case ResourceType.Texture1D:
					if (resource.Settings.ArrayCount <= 1)
					{
						desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture1D;
						uavDesc.Dimension = D3D.UnorderedAccessViewDimension.Texture1D;
					}
					else
					{
						desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture1DArray;
						uavDesc.Dimension = D3D.UnorderedAccessViewDimension.Texture1DArray;
					}

					desc.Texture1DArray = new D3D.ShaderResourceViewDescription.Texture1DArrayResource()
						{
							MipLevels = resource.Settings.MipCount,
							MostDetailedMip = 0,
							ArraySize = resource.Settings.ArrayCount,
							FirstArraySlice = 0
						};

					uavDesc.Texture1DArray = new D3D.UnorderedAccessViewDescription.Texture1DArrayResource()
						{
							ArraySize = resource.Settings.ArrayCount,
							FirstArraySlice = 0,
							MipSlice = 0
						};
					break;
				case ResourceType.Texture2D:
					// Build UAV desc.
					uavDesc.Dimension = resource.Settings.ArrayCount <= 1
						                    ? D3D.UnorderedAccessViewDimension.Texture2D
						                    : D3D.UnorderedAccessViewDimension.Texture2DArray;

					uavDesc.Texture2DArray = new D3D.UnorderedAccessViewDescription.Texture2DArrayResource()
						{
							ArraySize = resource.Settings.ArrayCount,
							FirstArraySlice = 0,
							MipSlice = 0
						};

					// Build SRC desc.
					if (!resource.Settings.IsTextureCube)
					{
						if (isMultisampled)
						{
							desc.Dimension = resource.Settings.ArrayCount <= 1
								                 ? SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DMultisampled
								                 : SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DMultisampledArray;

							desc.Texture2DMSArray = new D3D.ShaderResourceViewDescription.Texture2DMultisampledArrayResource()
								{
									ArraySize = resource.Settings.ArrayCount,										
									FirstArraySlice = 0
								};
						}
						else
						{
							desc.Dimension = resource.Settings.ArrayCount <= 1
								                 ? SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D
								                 : SharpDX.Direct3D.ShaderResourceViewDimension.Texture2DArray;

							desc.Texture2DArray = new D3D.ShaderResourceViewDescription.Texture2DArrayResource()
								{
									MipLevels = resource.Settings.MipCount,
									MostDetailedMip = 0,
									ArraySize = resource.Settings.ArrayCount,
									FirstArraySlice = 0
								};
						}
					}
					else
					{
						if (resource.Settings.ArrayCount == 6)
						{
							desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube;
							desc.TextureCube = new D3D.ShaderResourceViewDescription.TextureCubeResource()
								{
									MipLevels = resource.Settings.MipCount,
									MostDetailedMip = 0
								};
						}
						else
						{
							desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCubeArray;
							desc.TextureCubeArray = new D3D.ShaderResourceViewDescription.TextureCubeArrayResource()
								{
									MipLevels = resource.Settings.MipCount,
									CubeCount = resource.Settings.ArrayCount / 6,
									First2DArrayFace = 0,
									MostDetailedMip = 0
								};
						}								
					}
					break;
				case ResourceType.Texture3D:
					desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture3D;
					desc.Texture3D = new D3D.ShaderResourceViewDescription.Texture3DResource()
						{
							MipLevels = resource.Settings.MipCount,
							MostDetailedMip = 0
						};

					uavDesc.Dimension = D3D.UnorderedAccessViewDimension.Texture3D;
					uavDesc.Texture3D = new D3D.UnorderedAccessViewDescription.Texture3DResource()
						{
							FirstWSlice = 0,
							MipSlice = 0,
							WSize = resource.Settings.Depth
						};
					break;
				default:
					throw new GorgonException(GorgonResult.CannotCreate, "Cannot create a resource view, the texture type [" + resource.ResourceType + "] is unknown.");
			}

			try
			{
                Gorgon.Log.Print("Gorgon resource view: Creating D3D 11 shader resource view.", LoggingLevel.Verbose);

				// Create our SRV.
				D3DResourceView = new D3D.ShaderResourceView(Graphics.D3DDevice, resource.D3DResource, desc)
					{
						DebugName = resource.ResourceType + " '" + resource.Name + "' Shader Resource View"
					};

				// Create our UAV.
				if (resource.Settings.UnorderedAccessViewFormat != BufferFormat.Unknown)
				{
                    Gorgon.Log.Print("Gorgon resource view: Creating D3D 11 unordered access resource view.", LoggingLevel.Verbose);

					D3DUnorderedResourceView = new D3D.UnorderedAccessView(Graphics.D3DDevice, resource.D3DResource, uavDesc)
						{
                            DebugName = resource.ResourceType + " '" + resource.Name + "' Unordered Access Resource View"
						};
				}
			}
			catch (SharpDX.SharpDXException sDXEx)
			{
				if ((uint)sDXEx.ResultCode.Code == 0x80070057)
				{
					throw new GorgonException(GorgonResult.CannotCreate,
						                        "Cannot create the shader view.  The format '" +
						                        resource.Settings.ShaderView.ToString() +
						                        "' is not compatible or cannot be cast to '" + resource.Settings.Format.ToString() +
						                        "'.");
				}
			}
		}

		/// <summary>
		/// Function to build the resource view for a shader buffer.
		/// </summary>
		/// <param name="buffer">Buffer to build the view for.</param>
		private void BuildBufferView(GorgonShaderBuffer buffer)
		{
			Type bufferType = buffer.GetType();
			var srvDesc = new D3D.ShaderResourceViewDescription();
			var uavDesc = new D3D.UnorderedAccessViewDescription();
			var structuredBuffer = buffer as GorgonStructuredBuffer;

			if (structuredBuffer == null)
			{
				return;
			}

			D3D.UnorderedAccessViewBufferFlags uavFlags;
			switch (structuredBuffer.StructuredBufferType)
			{
				case StructuredBufferType.Raw:
					uavFlags = D3D.UnorderedAccessViewBufferFlags.Raw;
					break;
				case StructuredBufferType.AppendConsume:
					uavFlags = D3D.UnorderedAccessViewBufferFlags.Append;
					break;
				case StructuredBufferType.Counter:
					uavFlags = D3D.UnorderedAccessViewBufferFlags.Counter;
					break;
				default:
					uavFlags = D3D.UnorderedAccessViewBufferFlags.None;
					break;
			}

			srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.ExtendedBuffer;
			srvDesc.BufferEx = new D3D.ShaderResourceViewDescription.ExtendedBufferResource()
				{
					ElementCount = structuredBuffer.ElementCount,
					FirstElement = 0,
					Flags = D3D.ShaderResourceViewExtendedBufferFlags.None
				};
			srvDesc.Format = GI.Format.Unknown;

			uavDesc.Dimension = D3D.UnorderedAccessViewDimension.Buffer;
			uavDesc.Format = GI.Format.Unknown;
			uavDesc.Buffer = new D3D.UnorderedAccessViewDescription.BufferResource()
				{
					ElementCount = structuredBuffer.ElementCount,
					FirstElement = 0,
					Flags = uavFlags
				};

            Gorgon.Log.Print("Gorgon resource view: Creating D3D 11 shader resource view.", LoggingLevel.Verbose);
			D3DResourceView = new D3D.ShaderResourceView(Graphics.D3DDevice, buffer.D3DResource, srvDesc);
            D3DResourceView.DebugName = bufferType + " Shader Resource View";

			if (structuredBuffer.IsUnorderedAccess)
			{
                Gorgon.Log.Print("Gorgon resource view: Creating D3D 11 unordered access resource view.", LoggingLevel.Verbose);
				D3DUnorderedResourceView = new D3D.UnorderedAccessView(Graphics.D3DDevice, buffer.D3DResource, uavDesc);
				D3DUnorderedResourceView.DebugName = bufferType + " Unordered Access View";
			}
		}

		/// <summary>
		/// Function to clean up the resource view objects.
		/// </summary>
		internal void CleanUp()
		{
			if (D3DResourceView != null)
			{
				Gorgon.Log.Print("Gorgon resource view: Destroying D3D 11 shader resource view.", LoggingLevel.Verbose);
				D3DResourceView.Dispose();
			}

			if (D3DUnorderedResourceView != null)
			{
				Gorgon.Log.Print("Gorgon resource view: Destroying D3D 11 unordered access resource view.", LoggingLevel.Verbose);
				D3DUnorderedResourceView.Dispose();
			}

			D3DResourceView = null;
			D3DUnorderedResourceView = null;
		}

		/// <summary>
		/// Function to build the resource view.
		/// </summary>
		internal void BuildResourceView()
		{
		    CleanUp();

			// Do nothing if we're not bound to a resource.
			if (Resource == null)
			{
				return;
			}

			// Determine the type of the resource.
			var buffer = Resource as GorgonShaderBuffer;
			if (buffer != null)
			{
				BuildBufferView(buffer);
				return;
			}
            
			var texture = Resource as GorgonTexture;
		    if (texture == null)
		    {
		        return;
		    }

		    BuildTextureView(texture);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonResourceView" /> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		/// <param name="resource">Resource to bind with.</param>
		internal GorgonResourceView(GorgonGraphics graphics, GorgonResource resource)	
		{
			Graphics = graphics;
		    Resource = resource;
		}
		#endregion
	}*/
}
