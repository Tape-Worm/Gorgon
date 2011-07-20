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
		/// Function to return a list of driver capabilities for a given driver.
		/// </summary>
		/// <param name="index">Index of the driver to evaluate.</param>
		/// <returns>An enumerable list of driver capabilities.</returns>
		protected abstract IEnumerable<KeyValuePair<string, string>> GetDriverCapabilities(int index);

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
		/// Function to perform shutdown for the renderer.
		/// </summary>
		internal void Shutdown()
		{
			if (!_initialized)
				return;

			ShutdownRenderer();
			_initialized = false;
		}

		/// <summary>
		/// Function to perform initialization for the renderer.
		/// </summary>
		internal void Initialize()
		{
			if (_initialized)
				Shutdown();

			InitializeRenderer();
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
