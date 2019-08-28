#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: May 20, 2018 11:49:55 AM
// 
#endregion

using System;
using System.Diagnostics;
using System.Threading;
using Gorgon.Diagnostics;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// The base class for a view that allows the GPU to access a resource in various ways.
    /// </summary>
    public abstract class GorgonResourceView
        : IDisposable, IGorgonGraphicsObject, IEquatable<GorgonResourceView>
    {
        #region Variables.
        // The resource being viewed.
        private GorgonGraphicsResource _resource;
        // The resource view.
        private D3D11.ResourceView _view;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the logging interface use for debug messages.
        /// </summary>
        protected IGorgonLog Log => Graphics.Log;

        /// <summary>
        /// Property to set or return whether the view owns the attached resource or not.
        /// </summary>
        protected bool OwnsResource
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether or not the object is disposed.
        /// </summary>
        public bool IsDisposed => Resource == null;

        /// <summary>
        /// Property to return the resource bound to the view.
        /// </summary>
        public GorgonGraphicsResource Resource => _resource;

        /// <summary>
        /// Property to return the usage flag(s) for the resource.
        /// </summary>
        public ResourceUsage Usage => Resource?.Usage ?? ResourceUsage.None;

        /// <summary>
        /// Property to return the graphics interface that built this object.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform the creation of a specific kind of view.
        /// </summary>
        /// <returns>The view that was created.</returns>
        private protected abstract D3D11.ResourceView OnCreateNativeView();

        /// <summary>
        /// Function to initialize the buffer view.
        /// </summary>
        internal void CreateNativeView()
        {
            _view = OnCreateNativeView();
            Debug.Assert(_view != null, "No view was created.");
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public virtual void Dispose()
        {
            D3D11.ResourceView view = Interlocked.Exchange(ref _view, null);
            GorgonGraphicsResource resource = Interlocked.Exchange(ref _resource, null);

            if ((view == null) && (resource == null))
            {
                return;
            }

            if (resource != null)
            {
                Log.Print($"Resource View '{resource.Name}': Releasing D3D11 resource view.", LoggingLevel.Simple);

                if (OwnsResource)
                {
                    Log.Print($"Resource View '{resource.Name}': Releasing D3D11 Resource {resource.ResourceType} because it owns it.", LoggingLevel.Simple);
                    resource.Dispose();
                }
            }

            view?.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals(GorgonResourceView other)
        {
            if (other == null)
            {
                return false;
            }

#pragma warning disable IDE0046 // Convert to conditional expression
            if (other == this)
            {
                return true;
            }
#pragma warning restore IDE0046 // Convert to conditional expression

            return other._view == _view;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="obj">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="obj" /> parameter; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj) => Equals(obj as GorgonResourceView);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => _view.GetHashCode();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonResourceView"/> class.
        /// </summary>
        /// <param name="resource">The resource to view.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="resource"/> parameter is <b>null</b>.</exception>
        protected GorgonResourceView(GorgonGraphicsResource resource)
        {
            _resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Graphics = _resource.Graphics;
        }
        #endregion
    }
}
