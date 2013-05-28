﻿#region MIT.
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
// Created: Monday, January 09, 2012 7:06:42 AM
// 
#endregion

using System;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.IO;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A generic buffer object.
	/// </summary>
	/// <remarks>This generic buffer object is unable to be bound with shaders, but can be bound with output streams to retrieve data from shaders that support output data streams.</remarks>
	public class GorgonBuffer
		: GorgonBaseBuffer
	{
		#region Properties.
		/// <summary>
		/// Property to return the settings for the buffer.
		/// </summary>
		public new GorgonBufferSettings Settings
		{
			get
			{
				return (GorgonBufferSettings)base.Settings;
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to retrieve the staging buffer for this buffer.
        /// </summary>
        /// <returns>
        /// The staging buffer for this buffer.
        /// </returns>
        protected override GorgonBaseBuffer GetStagingBufferImpl()
        {
            // TODO: Add this when generic buffer type gets added to buffers interface.

            return null;
        }

		/// <summary>
		/// Function to clean up the resource object.
		/// </summary>
		protected override void CleanUpResource()
		{
		    if (IsLocked)
		    {
		        Unlock();
		    }

		    if (D3DResource == null)
		    {
		        return;
		    }

		    GorgonRenderStatistics.BufferCount--;
		    GorgonRenderStatistics.BufferSize -= D3DBuffer.Description.SizeInBytes;

		    D3DResource.Dispose();
		    D3DResource = null;

            Gorgon.Log.Print("Destroyed {0} {1}.", LoggingLevel.Verbose, GetType().FullName, Name);
		}

		/// <summary>
		/// Function used to initialize the buffer with data.
		/// </summary>
		/// <param name="data">Data to write.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to the <paramref name="data"/> parameter should ignore the initialization and create the backing buffer as normal.</remarks>
		protected override void InitializeImpl(GorgonDataStream data)
		{
			if (D3DResource != null)
			{
				D3DResource.Dispose();
				D3DResource = null;
			}

		    var desc = new D3D11.BufferDescription
		        {
		            BindFlags = Settings.IsOutput ? D3D11.BindFlags.StreamOutput : D3D11.BindFlags.None,
		            CpuAccessFlags = D3DCPUAccessFlags,
		            OptionFlags = D3D11.ResourceOptionFlags.None,
		            SizeInBytes = SizeInBytes,
		            StructureByteStride = 0,
		            Usage = D3DUsage
		        };

		    if (data != null)
		    {
		        long position = data.Position;

		        using(var dxStream = new DX.DataStream(data.BasePointer, data.Length - position, true, true))
		        {
		            D3DResource = new D3D11.Buffer(Graphics.D3DDevice, dxStream, desc);
		        }
		    }
		    else
		    {
		        D3DResource = new D3D11.Buffer(Graphics.D3DDevice, desc);
		    }

		    GorgonRenderStatistics.BufferCount++;
			GorgonRenderStatistics.BufferSize += ((D3D11.Buffer)D3DResource).Description.SizeInBytes;
		}
		#endregion

		#region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonBuffer" /> class.
        /// </summary>
        /// <param name="graphics">The graphics interface used to create this object.</param>
        /// <param name="name">Name of the buffer.</param>
        /// <param name="settings">Settings for the buffer.</param>
		internal GorgonBuffer(GorgonGraphics graphics, string name, IBufferSettings settings)
			: base(graphics, name, settings)
        {
		}
		#endregion
	}
}
