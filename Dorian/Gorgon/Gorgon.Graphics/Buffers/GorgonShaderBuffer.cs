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
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A buffer for shaders.
	/// </summary>
	/// <remarks>This is a base object for buffers that can be bound to a shader.</remarks>
	public abstract class GorgonShaderBuffer
		: GorgonBaseBuffer
	{
		#region Properties.
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

		/// <summary>
		/// Property to return the settings for a shader buffer.
		/// </summary>
		public new IShaderBufferSettings Settings
		{
			get
			{
				return (IShaderBufferSettings)base.Settings;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			// If we're bound with a pixel or vertex shader, then unbind.
			if (Settings.Usage != BufferUsage.Staging)
			{
				Graphics.Shaders.VertexShader.Resources.Unbind(this);
				Graphics.Shaders.PixelShader.Resources.Unbind(this);
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
