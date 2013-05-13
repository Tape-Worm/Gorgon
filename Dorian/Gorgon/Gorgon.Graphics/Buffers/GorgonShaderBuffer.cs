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
using System.Collections.Generic;
using GorgonLibrary.IO;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A buffer for shaders.
	/// </summary>
	/// <remarks>This is a base object for buffers that can be bound to a shader.</remarks>
	public abstract class GorgonShaderBuffer
		: GorgonBuffer
	{
		#region Variables.
	    private Dictionary<Tuple<BufferFormat, int, int>, D3D.ShaderResourceView> _cache = null;    // A cache of shader resource views for the buffer.
	    private GorgonBufferShaderView _defaultSRV;                                                 // The default shader resource view for a shader bound buffer.
	    private D3D.UnorderedAccessView _defaultUAV;                                                // The default unordered access view for a shader bound buffer.
		#endregion

		#region Properties.
        /// <summary>
        /// Property to return the default shader resource view for the buffer.
        /// </summary>
	    internal D3D.ShaderResourceView DefaultSRV
	    {
	        get
	        {
	            return _defaultSRV == null ? null : _defaultSRV.D3DView;
	        }
	    }
		/// <summary>
		/// Property to return the settings for a shader buffer.
		/// </summary>
		public IShaderBufferSettings Settings
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
			// If we're bound with a pixel or vertex shader, then unbind.
			if (BufferUsage != BufferUsage.Staging)
			{
				Graphics.Shaders.VertexShader.Resources.Unbind(this);
				Graphics.Shaders.PixelShader.Resources.Unbind(this);
			}

			if (IsLocked)
		    {
		        Unlock();
		    }

            // Destroy the default shader resource view.
            if (_defaultSRV != null)
            {
                _defaultSRV.Dispose();
                _defaultSRV = null;
            }

            // Destroy the default unordered access view.
            if (_defaultUAV != null)
            {
                _defaultUAV.Dispose();
                _defaultUAV = null;
            }

            // Wipe out the cache for this object.
            foreach (var item in _cache)
            {
                
            }

		    if (D3DResource == null)
		    {
		        return;
		    }

		    D3DResource.Dispose();
		    D3DResource = null;
		}

        public GorgonBufferShaderView CreateView(BufferFormat format, int start, int count)
        {
            GorgonBufferShaderView result;

            return result;
        }
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderBuffer" /> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="settings">The settings for the buffer.</param>
		/// <param name="totalSize">The total size of the buffer, in bytes.</param>
		protected GorgonShaderBuffer(GorgonGraphics graphics, IShaderBufferSettings settings, int totalSize)
			: base(graphics, (settings.AllowCPUWrite ? BufferUsage.Dynamic : BufferUsage.Default), totalSize, settings.IsOutput)
		{
			Settings = settings;
		}
		#endregion
	}
}
