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
// Created: Sunday, December 9, 2012 4:43:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GorgonLibrary.Native;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The type of data in the resource.
	/// </summary>
	public enum ResourceType
	{
		/// <summary>
		/// Unknown data type.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// One of the buffer types.
		/// </summary>
		Buffer = 1,
		/// <summary>
		/// A 1 dimensional texture.
		/// </summary>
		Texture1D = 2,
		/// <summary>
		/// A 2 dimensional texture.
		/// </summary>
		Texture2D = 3,
		/// <summary>
		/// A 3 dimensional texture.
		/// </summary>
		Texture3D = 4
	}

	/// <summary>
	/// Priority used to evict a resource from video memory.
	/// </summary>
	public enum EvictionPriority
		: uint
	{
		/// <summary>
		/// Unknown.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// The resource is unused and can be evicted as soon as another resource requires the memory that the resource occupies.
		/// </summary>
		Minimum = 0x28000000,
		/// <summary>
		/// The placement of the resource is not critical, and minimal work is performed to find a location for the resource.
		/// </summary>
		Low = 0x50000000,
		/// <summary>
		/// The placement of the resource is important, but not critical, for performance.
		/// </summary>
		Normal = 0x78000000,
		/// <summary>
		/// The resource is place in a preferred location instea of a low/normal priority resource.
		/// </summary>
		High = 0xa0000000,
		/// <summary>
		/// The resource is evicted only if there's no other way to resolve a memory requirement.
		/// </summary>
		Maximum = 0xc8000000
	}

	/// <summary>
	/// Used to define an object as a resource.
	/// </summary>
	/// <remarks>Objects that inherit from this class will be considered a resource object that may (depending on usage) be bound to the pipeline.</remarks>
	public abstract class GorgonResource
		: IDisposable
	{
		#region Variables.
	    private Dictionary<GorgonShaderView, D3D.ShaderResourceView> _shaderViewCache;  // A cache of shader resource views.
		private bool _disposed;				                                            // Flag to indicate that the object was disposed.
		private GorgonShaderView _shaderView;	                                        // The current view for the resource..
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the Direct 3D resource object bound to this object.
		/// </summary>
		internal virtual D3D.Resource D3DResource
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the default shader view for this resource.
        /// </summary>
        internal KeyValuePair<GorgonShaderView, D3D.ShaderResourceView> DefaultView
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return whether this resource can be used as a raw resource in a shader.
		/// </summary>
		public bool IsRaw
		{
			get;
			protected set;
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
        /// Property to set or return the current shader view.
        /// </summary>
        public virtual GorgonShaderView? ShaderView
        {
            get
            {
                return _shaderView;
            }
            set
            {
                
            }
        }

		/// <summary>
		/// Property to set or return the view for the resource.
		/// </summary>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when an attempt to bind a view to the resource is made and the view is already bound to another resource.</exception>
		public virtual GorgonResourceView View
		{
			get;
            protected set;
		}

		/// <summary>
		/// Property to return the type of data in the resource.
		/// </summary>
		public abstract ResourceType ResourceType
		{
			get;
		}

		/// <summary>
		/// Property to set or return the 
		/// </summary>
		public EvictionPriority EvictionPriority
		{
			get
			{
				if (D3DResource == null)
				{
					return EvictionPriority.Minimum;
				}

				return (EvictionPriority)D3DResource.EvictionPriority;
			}
			set
			{
				if (D3DResource != null)
				{
					D3DResource.EvictionPriority = (int)value;
				}
			}
		}

		/// <summary>
		/// Property to return the size, in bytes, of the resource.
		/// </summary>
		public abstract int SizeInBytes
		{
			get;
		}		
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected abstract void CleanUpResource();

        /// <summary>
        /// Function to set the current resource view for this resource.
        /// </summary>
        /// <param name="srvFormat">Shader resource view format.</param>
        /// <param name="uavFormat">Unordered access view format</param>
        protected void SetResourceView(BufferFormat srvFormat, BufferFormat uavFormat)
        {
            // If there's no change, then do nothing.
			if ((_view != null) && (_view.ShaderViewFormat == srvFormat) && (_view.UnorderedAccessViewFormat == uavFormat))
			{
				return;
			}

            // Check the cache for this view.
            var lookUp = new Tuple<BufferFormat, BufferFormat>(srvFormat, uavFormat);

            // If it's in the cache, then just re-use it.
            if (_viewCache.ContainsKey(lookUp))
            {
                _view = _viewCache[lookUp];

                Graphics.Shaders.Reseat(this);
                return;
            }

            // It's not in the cache.  So we need to build it.
            _view = new GorgonResourceView(Graphics, this);
            _view.BuildResourceView();

            // Re-apply to the shaders with the new view.
            Graphics.Shaders.Reseat(this);
            
        }

		/// <summary>
		/// Function to set application specific data on the resource.
		/// </summary>
		/// <typeparam name="T">Type of data to copy into the resource.  The data must be a value type.</typeparam>
		/// <param name="guid">GUID to associate with the data.</param>
		/// <param name="data">Data to set.</param>
		/// <remarks>Set <paramref name="data"/> to NULL (Nothing in VB.Net) to remove the data from the resource.</remarks>
		public void SetApplicationData<T>(Guid guid, T? data)
			where T : struct
		{
			int bytes = 0;
			IntPtr dataPtr = IntPtr.Zero;

			if (D3DResource == null)
			{
				return;
			}

			bytes = DirectAccess.SizeOf<T>();

			try
			{
				if (data != null)
				{
					T value = data.Value;
					dataPtr = Marshal.AllocHGlobal(bytes);
					dataPtr.Write<T>(ref value);
					D3DResource.SetPrivateData(guid, bytes, dataPtr);
				}
				else
				{
					D3DResource.SetPrivateData(guid, 0, IntPtr.Zero);
				}
			}
			finally
			{
				if (dataPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(dataPtr);
				}				
			}
		}

		/// <summary>
		/// Function to return application specific data from the resource.
		/// </summary>
		/// <typeparam name="T">Type of data to copy into the resource.  The data must be a value type.</typeparam>
		/// <param name="guid">GUID to associate with the data.</param>
		/// <returns>The application specific data stored in the resource, or NULL.</returns>
		public T? GetApplicationData<T>(Guid guid)
			where T : struct
		{
			int bytes = 0;
			IntPtr dataPtr = IntPtr.Zero;
			T result = default(T);

			if (D3DResource == null)
			{
				return null;
			}

			try
			{
				bytes = DirectAccess.SizeOf<T>();
				dataPtr = Marshal.AllocHGlobal(bytes);

				if (dataPtr != IntPtr.Zero)
				{
					D3DResource.GetPrivateData(guid, ref bytes, dataPtr);
					dataPtr.Read<T>(out result);

					return result;
				}
				else
				{
					return null;
				}
			}
			finally
			{
				if (dataPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(dataPtr);
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonResource" /> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		protected GorgonResource(GorgonGraphics graphics)
		{
			IsRaw = false;
			Graphics = graphics;
            _viewCache = new Dictionary<Tuple<BufferFormat, BufferFormat>, GorgonResourceView>();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Graphics.RemoveTrackedObject(this);

					CleanUpResource();

                    // Destroy all views in the cache.
                    foreach (var item in _viewCache)
                    {
                        item.Value.CleanUp();
                    }

                    _viewCache.Clear();
				}

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
