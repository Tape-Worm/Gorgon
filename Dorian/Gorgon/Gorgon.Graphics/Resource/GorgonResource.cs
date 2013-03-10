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
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;
using System.Runtime.InteropServices;
using GorgonLibrary.Native;

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
		private bool _disposed = false;				// Flag to indicate that the object was disposed.
		private GorgonResourceView _view = null;	// The view of the resource when bound to a shader.
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
		/// Property to set or return whether this resource can be used as a raw resource in a shader.
		/// </summary>
		public bool IsRaw
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return the default view for the resource.
		/// </summary>
		public GorgonResourceView DefaultView
		{
			get;
			internal set;
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
		/// Property to set or return the view for the resource.
		/// </summary>
		public virtual GorgonResourceView View
		{
			get
			{
				if (_view == null)
				{
					return DefaultView;
				}

				return _view;
			}
			set
			{
				// If we're changing the view, ensure it's not bound.
				Graphics.Shaders.Unbind(this);

				if ((value == DefaultView) && (_view != DefaultView))
				{
					_view.Resource = null;
					_view = null;
					return;
				}

				if ((value == null) && (_view != null))
				{
					_view.Resource = null;					
				}

				_view = value;
				
				if (_view != null)
				{					
					_view.Resource = this;
					_view.BuildResourceView();
				}				
			}			
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
					D3DResource.EvictionPriority = (int)D3DResource.EvictionPriority;
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
		/// Function to create a default resource view object.
		/// </summary>
		protected virtual void CreateDefaultResourceView()
		{
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
