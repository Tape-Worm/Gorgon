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
// Created: July 22, 2017 10:16:16 AM
// 
#endregion

using System;
using System.Threading;
using Gorgon.Diagnostics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// An unordered access view for a <see cref="GorgonGraphicsResource"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This type of view allows for unordered access to a <see cref="GorgonGraphicsResource"/> like one of the <see cref="GorgonBuffer"/> types, a texture, etc... Any resource that needs a 
    /// unordered access must be created with the the <c>UnorderedAccess</c> flag.
    /// </para>
    /// <para>
    /// The unordered access allows a shader to read/write any part of a <see cref="GorgonGraphicsResource"/> by multiple threads without memory contention. This is done through the use of 
    /// <a target="_blank" href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476334(v=vs.85).aspx">atomic functions</a>.
    /// </para>
    /// <para>
    /// These types of views are most useful for <see cref="GorgonComputeShader"/> shaders, but can also be used by a <see cref="GorgonPixelShader"/> by passing a list of these views in to a 
    /// <see cref="GorgonDrawCallBase">draw call</see>.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// Unordered access views do not support a multisampled <see cref="GorgonTexture2D"/>.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsResource"/>
    /// <seealso cref="GorgonBuffer"/>
    /// <seealso cref="GorgonTexture2D"/>
    /// <seealso cref="GorgonComputeShader"/>
    /// <seealso cref="GorgonPixelShader"/>
    /// <seealso cref="GorgonDrawCallBase"/>
    public abstract class GorgonUnorderedAccessView
        : IDisposable, IGorgonGraphicsObject
    {
        #region Variables.
        // The D3D11 shader resource view.
        private D3D11.UnorderedAccessView1 _view;
        // The resource to bind to the GPU.
        private GorgonGraphicsResource _resource;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether the view owns the attached resource or not.
        /// </summary>
        protected bool OwnsResource
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the native Direct 3D 11 view.
        /// </summary>
        protected internal D3D11.UnorderedAccessView1 Native
        {
            get => _view;
            set => _view = value;
        }

        /// <summary>
        /// Property to return whether this view is disposed or not.
        /// </summary>
        public bool IsDisposed => _view == null;

        /// <summary>
        /// Property to return the resource bound to the view.
        /// </summary>
        public GorgonGraphicsResource Resource => _resource;

        /// <summary>
        /// Property to return the usage flag(s) for the resource.
        /// </summary>
        public ResourceUsage Usage => _resource?.Usage ?? ResourceUsage.Default;


        /// <summary>
        /// Property to return the format used to interpret this view.
        /// </summary>
        public BufferFormat Format
        {
            get;
        }

        /// <summary>
        /// Property to return information about the <see cref="Format"/> used by this view.
        /// </summary>
        public GorgonFormatInfo FormatInformation
        {
            get;
        }

        /// <summary>
        /// Property to return the graphics interface that the underlying resource.
        /// </summary>
        public GorgonGraphics Graphics => _resource?.Graphics;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the unordered access view.
        /// </summary>
        protected internal abstract void CreateNativeView();

        /// <summary>
        /// Function to clear the unordered access value with the specified values.
        /// </summary>
        /// <param name="value1">First value.</param>
        /// <param name="value2">Second value.</param>
        /// <param name="value3">Third value.</param>
        /// <param name="value4">Fourth value.</param>
        /// <remarks>
        /// <para>
        /// This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
        /// </para>
        /// <para>
        /// This method works on any unordered access view that does not require format conversion.  Unordered access views for raw/structured buffers only use the first value.
        /// </para>
        /// </remarks>
        public void Clear(int value1, int value2, int value3, int value4)
        {
            Resource.Graphics.D3DDeviceContext.ClearUnorderedAccessView(Native, new DX.Int4(value1, value2, value3, value4));
        }

        /// <summary>
        /// Function to clear the unordered access value with the specified value.
        /// </summary>
        /// <param name="value">Value used to clear.</param>
        /// <remarks>
        /// <para>
        /// This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
        /// </para>
        /// <para>
        /// This method works on any unordered access view that does not require format conversion.
        /// </para>
        /// </remarks>
        public void Clear(int value)
        {
            Resource.Graphics.D3DDeviceContext.ClearUnorderedAccessView(Native, new DX.Int4(value));
        }

        /// <summary>
        /// Function to clear the unordered access value with the specified values.
        /// </summary>
        /// <param name="value1">First value.</param>
        /// <param name="value2">Second value.</param>
        /// <param name="value3">Third value.</param>
        /// <param name="value4">Fourth value.</param>
        /// <remarks>
        /// <para>
        /// This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
        /// </para>
        /// <para>
        /// This method works on any unordered access view that does not require format conversion.  Unordered access views for raw/structured buffers only use the first value.
        /// </para>
        /// </remarks>
        public void Clear(float value1, float value2, float value3, float value4)
        {
            Resource.Graphics.D3DDeviceContext.ClearUnorderedAccessView(Native, new DX.Vector4(value1, value2, value3, value4));
        }

        /// <summary>
        /// Function to clear the unordered access value with the specified value.
        /// </summary>
        /// <param name="value">Value used to clear.</param>
        /// <remarks>
        /// <para>
        /// This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
        /// </para>
        /// <para>
        /// This method works on any unordered access view that does not require format conversion.
        /// </para>
        /// </remarks>
        public void Clear(float value)
        {
            Resource.Graphics.D3DDeviceContext.ClearUnorderedAccessView(Native, new DX.Vector4(value));
        }

        /// <summary>
        /// Function to clear the unordered access value with the specified values.
        /// </summary>
        /// <param name="values">Values used to clear.</param>
        /// <remarks>
        /// <para>
        /// This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
        /// </para>
        /// <para>
        /// This method works on any unordered access view that does not require format conversion.  Unordered access views for raw/structured buffers only use the first value in the vector.
        /// </para>
        /// </remarks>
        public void Clear(DX.Vector4 values)
        {
            Resource.Graphics.D3DDeviceContext.ClearUnorderedAccessView(Native, new DX.Vector4(values.X, values.Y, values.Z, values.W));
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            D3D11.UnorderedAccessView1 view = Interlocked.Exchange(ref _view, null);

            if (view == null)
            {
                return;
            }

            this.UnregisterDisposable(Resource.Graphics);

            if (OwnsResource)
            {
                Graphics.Log.Print($"Unordered Access View '{Resource.Name}': Releasing D3D11 Resource {Resource.ResourceType} because it owns it.", LoggingLevel.Simple);
                GorgonGraphicsResource resource = Interlocked.Exchange(ref _resource, null);
                resource?.Dispose();
            }
		    
            Graphics.Log.Print($"Unordered Access View '{Resource.Name}': Releasing D3D11 unordered access view.", LoggingLevel.Simple);
            view.Dispose();

            GC.SuppressFinalize(this);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonShaderResourceView"/> class.
        /// </summary>
        /// <param name="resource">The resource to bind to the view.</param>
        /// <param name="format">The format of the view.</param>
        /// <param name="formatInfo">Information about the format.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="resource"/>, or the <paramref name="formatInfo"/> parameter is <b>null</b>.</exception>
        protected GorgonUnorderedAccessView(GorgonGraphicsResource resource, BufferFormat format, GorgonFormatInfo formatInfo)
        {
            _resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Format = format;
            FormatInformation = formatInfo ?? throw new ArgumentNullException(nameof(formatInfo));
        }
        #endregion
    }
}
