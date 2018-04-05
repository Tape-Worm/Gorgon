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
using Gorgon.Core;
using Gorgon.Native;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The type of data in the resource.
    /// </summary>
    public enum GraphicsResourceType
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
    /// Defines the intended usage resources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This defines how the resource should be used when rendering and whether or not it is CPU and/or GPU accessible.
    /// </para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue", Justification = "No. Fuck off.")]
    public enum ResourceUsage
    {
        /// <summary>
        /// <para>
        /// A resource that requires read and write access by the GPU. This is likely to be the most common usage choice.
        /// </para>
        /// </summary>
        Default = D3D11.ResourceUsage.Default,

        /// <summary>
        /// <para>
        /// A resource that can only be read by the GPU. It cannot be written by the GPU, and cannot be accessed at all by the CPU. This type of resource must be initialized when it is created, since it cannot be changed after creation.
        /// </para>
        /// </summary>
        Immutable = D3D11.ResourceUsage.Immutable,

        /// <summary>
        /// <para>
        /// A resource that is accessible by both the GPU (read only) and the CPU (write only). A dynamic resource is a good choice for a resource that will be updated by the CPU at least once per frame. To update a dynamic resource, use a  method.
        /// </para>
        /// </summary>
        Dynamic = D3D11.ResourceUsage.Dynamic,

        /// <summary>
        /// <para>
        /// A resource that supports data transfer (copy) from the GPU to the CPU.
        /// </para>
        /// </summary>
        Staging = D3D11.ResourceUsage.Staging
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
    /// A base resource class for resource objects such as textures and buffers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Objects that inherit from this class will be considered a resource object that may (depending on usage) be bound to the pipeline.
    /// </para>
    /// </remarks>
    public abstract class GorgonGraphicsResource
        : IGorgonNamedObject, IDisposable
    {
        #region Properties.
        /// <summary>
        /// Property to return the Direct 3D resource object bound to this object.
        /// </summary>
        internal D3D11.Resource D3DResource
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether or not the resource can be used in an unordered access view.
        /// </summary>
        protected internal abstract bool IsUavResource
        {
            get;
        }

        /// <summary>
        /// Property to return whether or not the resource can be bound as a shader resource.
        /// </summary>
        protected internal abstract bool IsShaderResource
        {
            get;
        }

        /// <summary>
        /// Property to return the graphics interface used to create this object.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }

        /// <summary>
        /// Property to return the type of data in the resource.
        /// </summary>
        public abstract GraphicsResourceType ResourceType
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
        public int SizeInBytes
        {
            get;
            protected set;
        }

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        /// <remarks>
        /// For best practises, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this 
        /// property.
        /// </remarks>
        public string Name
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Objects that override this method should be sure to call this base method or else a memory leak may occur.
        /// </para>
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
            "CA1816:CallGCSuppressFinalizeCorrectly",
            Justification = "I don't have a finalizer, plus, this method is completely overridable. Idiot.")]
        public virtual void Dispose()
        {
            D3DResource?.Dispose();
        }

        /// <summary>
        /// Function to set application specific data on the resource.
        /// </summary>
        /// <typeparam name="T">Type of data to copy into the resource.  The data must be a value type.</typeparam>
        /// <param name="guid">GUID to associate with the data.</param>
        /// <param name="data">Data to set.</param>
        /// <remarks>
        /// <para>
        /// Set <paramref name="data"/> to <b>null</b> to remove the data from the resource.
        /// </para>
        /// </remarks>
        public void SetApplicationData<T>(Guid guid, T? data)
            where T : struct
        {
            GorgonPointerTyped<T> dataPtr = null;

            if (D3DResource == null)
            {
                return;
            }

            try
            {
                if (data != null)
                {
                    dataPtr = new GorgonPointerTyped<T>();
                    T value = data.Value;
                    dataPtr.Write(ref value);
                    D3DResource.SetPrivateData(guid, (int)dataPtr.Size, new IntPtr(dataPtr.Address));
                }
                else
                {
                    D3DResource.SetPrivateData(guid, 0, IntPtr.Zero);
                }
            }
            finally
            {
                dataPtr?.Dispose();
            }
        }

        /// <summary>
        /// Function to return application specific data from the resource.
        /// </summary>
        /// <typeparam name="T">Type of data to copy into the resource.  The data must be a value type.</typeparam>
        /// <param name="guid">GUID to associate with the data.</param>
        /// <returns>The application specific data stored in the resource, or <b>null</b>.</returns>
        public T? GetApplicationData<T>(Guid guid)
            where T : struct
        {
            GorgonPointerTyped<T> dataPtr = null;

            if (D3DResource == null)
            {
                return null;
            }

            try
            {
                dataPtr = new GorgonPointerTyped<T>();
                int bytes = (int)dataPtr.Size;

                D3DResource.GetPrivateData(guid, ref bytes, new IntPtr(dataPtr.Address));

                if (bytes == 0)
                {
                    return null;
                }

                return dataPtr.Read<T>();
            }
            finally
            {
                dataPtr?.Dispose();
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGraphicsResource" /> class.
        /// </summary>
        /// <param name="graphics">The graphics interface used to create this resource.</param>
        /// <param name="name">Name of this resource.</param>
        /// <remarks>
        /// <para>
        /// Names for the resource are required, but do not need to be unique. These are used to help with debugging and can be used for managing resources in an application.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <b>null</b>.</exception> 
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        protected GorgonGraphicsResource(GorgonGraphics graphics, string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            Name = name;
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        }
        #endregion
    }
}
