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
// Created: Tuesday, July 19, 2011 8:55:06 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX;
using GI = SlimDX.DXGI;
using D3D = SlimDX.Direct3D11;
using GorgonLibrary.Collections;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The primary object for the graphics sub system.
	/// </summary>
	public class GorgonGraphics
		: IDisposable
	{
		#region Variables.
		private bool _disposed = false;										// Flag to indicate that the object was disposed.
		private Version _minimumSupportedFeatureLevel = new Version(9, 3);	// Minimum supported feature level version.		
		private GorgonTrackedObjectCollection _trackedObjects = null;		// Tracked objects.
		#endregion

		#region Properties.		
		/// <summary>
		/// Property to return the DX GI factory.
		/// </summary>
		internal GI.Factory1 GIFactory
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the minimum supported feature level for Gorgon.
		/// </summary>
		public Version MinimumSupportedFeatureLevelVersion
		{
			get
			{
				return _minimumSupportedFeatureLevel;
			}
		}

		/// <summary>
		/// Property to return a list of video devices installed on the system.
		/// </summary>
		public GorgonVideoDeviceCollection VideoDevices
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to remove a tracked object from the collection.
		/// </summary>
		/// <param name="trackedObject">Tracked object to remove.</param>
		internal void RemoveTrackedObject(IDisposable trackedObject)
		{
			_trackedObjects.Remove(trackedObject);
		}

		/// <summary>
		/// Function to create a swap chain.
		/// </summary>
		/// <param name="name">Name of the swap chain.</param>
		/// <param name="settings">Settings for the swap chain.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.</exception>
		public GorgonSwapChain CreateSwapChain(string name, GorgonSwapChainSettings settings)
		{
			GorgonSwapChain swapChain = new GorgonSwapChain(this, name, settings);

			_trackedObjects.Add(swapChain);

			return swapChain;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		public GorgonGraphics()
		{
			_trackedObjects = new GorgonTrackedObjectCollection();

			Gorgon.Log.Print("Gorgon Graphics initializing...", Diagnostics.GorgonLoggingLevel.Simple);

			Gorgon.Log.Print("Creating DXGI interface...", GorgonLoggingLevel.Verbose);
			GIFactory = new GI.Factory1();

#if DEBUG
			SlimDX.Configuration.EnableObjectTracking = true;
#else
			SlimDX.Configuration.EnableObjectTracking = false;
#endif

			VideoDevices = new GorgonVideoDeviceCollection(this);

			Gorgon.AddTrackedObject(this);

			Gorgon.Log.Print("Gorgon Graphics initialized.", Diagnostics.GorgonLoggingLevel.Simple);
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
					Gorgon.Log.Print("Gorgon Graphics shutting down...", Diagnostics.GorgonLoggingLevel.Simple);
					
					_trackedObjects.ReleaseAll();
					
					VideoDevices.Clear();

					Gorgon.Log.Print("Removing DXGI factory interface...", GorgonLoggingLevel.Verbose);

					if (GIFactory != null)
						GIFactory.Dispose();

					GIFactory = null;

					// Remove us from the object tracker.
					Gorgon.RemoveTrackedObject(this);

					Gorgon.Log.Print("Gorgon Graphics shut down successfully", Diagnostics.GorgonLoggingLevel.Simple);
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
