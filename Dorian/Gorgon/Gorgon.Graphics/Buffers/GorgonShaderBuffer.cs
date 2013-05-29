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
// Created: Monday, November 5, 2012 8:58:47 PM
// 
#endregion

using System;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A buffer for shaders.
	/// </summary>
	/// <remarks>This is a base object for buffers that can be bound to a shader.</remarks>
	public abstract class GorgonShaderBuffer
		: GorgonBuffer
	{
		#region Properties.
        /// <summary>
        /// Property to set or return the D3D CPU access flags.
        /// </summary>
        internal D3D.CpuAccessFlags D3DCPUAccessFlags
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the D3D usages.
        /// </summary>
        internal D3D.ResourceUsage D3DUsage
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the view cache for the buffer.
        /// </summary>
        internal GorgonViewCache ViewCache
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the default shader view for this buffer.
        /// </summary>
        public GorgonBufferShaderView DefaultShaderView
        {
            get;
            protected set;
        }

        // TODO: Get rid of this.
        /// <summary>
        /// Property to return the settings for the buffer.
        /// </summary>
        public IShaderBufferSettings Settings
        {
            get;
            private set;
        }
        #endregion

		#region Methods.
        /// <summary>
        /// Function used to initialize the buffer with data.
        /// </summary>
        /// <param name="data">Data to write.</param>
        /// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
        protected virtual void InitializeImpl(GorgonDataStream data)
        {
            // TODO: Get rid of this.
        }

        /// <summary>
        /// Gets the staging buffer impl.
        /// </summary>
        /// <returns></returns>
        protected virtual GorgonBuffer GetStagingBufferImpl()
        {
            // TODO: Get rid of this class.
            return null;
        }

        /// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			// If we're bound with a pixel or vertex shader, then unbind.
			if (Settings.Usage != BufferUsage.Staging)
			{
				Graphics.Shaders.Unbind(this);
			}

			if (IsLocked)
		    {
		        Unlock();
		    }
            
            // Wipe out the cache for this object.
            if (ViewCache != null)
            {
                ViewCache.Dispose();
                ViewCache = null;
            }

		    if (D3DResource == null)
		    {
		        return;
		    }

		    D3DResource.Dispose();
		    D3DResource = null;

            Gorgon.Log.Print("Destroyed {0} {1}.", LoggingLevel.Verbose, GetType().FullName, Name);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderBuffer" /> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="name">Name of the buffer.</param>
		/// <param name="settings">The settings for the buffer.</param>
		protected GorgonShaderBuffer(GorgonGraphics graphics, string name, IBufferSettings settings)
			: base(graphics, name, settings)
		{
            ViewCache = new GorgonViewCache(this);
        }
		#endregion
	}
}
