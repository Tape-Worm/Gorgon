#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Tuesday, July 19, 2011 8:41:23 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The renderer interface.
	/// </summary>
	public abstract class GorgonRenderer
		: GorgonNamedObject, IDisposable
	{
		#region Variables.
		private bool _initialized = false;			// Flag to indicate that the renderer was initialized.
		private bool _disposed = false;				// Flag to indicate that the renderer was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface that is bound to this renderer.
		/// </summary>
		protected GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return a list of custom settings for the renderer.
		/// </summary>
		public GorgonCustomValueCollection<string> CustomSettings
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform shutdown for the renderer.
		/// </summary>
		private void Shutdown()
		{
			if (!_initialized)
				return;

			ShutdownRenderer();
			Graphics = null; 
			_initialized = false;
		}

		/// <summary>
		/// Function to return a list of all video devices installed on the system.
		/// </summary>
		/// <returns>An enumerable list of video devices.</returns>
		protected abstract IEnumerable<GorgonVideoDevice> GetVideoDevices();

		/// <summary>
		/// Function to perform the actual initialization for the renderer from a plug-in.
		/// </summary>
		/// <remarks>Implementors must implement this method and perform any one-time set up for the renderer.</remarks>
		protected abstract void InitializeRenderer();

		/// <summary>
		/// Function to perform the actual shut down for the renderer from a plug-in.
		/// </summary>
		/// <remarks>Implementors must implement this method and perform any one-time tear down for the renderer.</remarks>
		protected abstract void ShutdownRenderer();

		/// <summary>
		/// Function to perform initialization for the renderer.
		/// </summary>
		/// <param name="owner">The graphics interface that owns this object.</param>
		internal void Initialize(GorgonGraphics owner)
		{
			Version minSMVersion = new Version(3, 0);		// Minimum shader model version.
			
			if (_initialized)
				Shutdown();

			Graphics = owner;

			InitializeRenderer();
			
			// Add the list of video devices.
			var devices = GetVideoDevices();

			// Get device information.
			foreach (GorgonVideoDevice device in devices)
			{
				Gorgon.Log.Print("Video Device #{0}: {1}", Diagnostics.GorgonLoggingLevel.Simple, device.Index, device.Name);
				device.GetDeviceData();
				Gorgon.Log.Print("Head count for {0}: {1}", Diagnostics.GorgonLoggingLevel.Simple, device.Name, device.Outputs.Count);
			}

			// Filter those that aren't supported by Gorgon (SM 3.0)
			devices = from device in devices
					  where (device.PixelShaderVersion >= minSMVersion) && (device.VertexShaderVersion >= minSMVersion)
					  select device;

			if (devices.Count() == 0)
				throw new GorgonException(GorgonResult.CannotEnumerate, "Cannot enumerate devices.  Could not find any applicable video device installed on the system.  Please ensure there is at least one video device that supports hardware acceleration and is capable of Shader Model 3.0 or better.");

			Graphics.VideoDevices.AddRange(devices);
			
			_initialized = true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonRenderer"/> class.
		/// </summary>
		/// <param name="description">The description of the renderer.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="description"/> parameter is NULL (or Nothing) in VB.NET.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="description"/> parameter is an empty string.</exception>
		protected GorgonRenderer(string description)
			: base(description)
		{
			CustomSettings = new GorgonCustomValueCollection<string>();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Shutdown();						
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
