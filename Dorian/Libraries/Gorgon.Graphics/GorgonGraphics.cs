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
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The primary object for the graphics sub system.
	/// </summary>
	public class GorgonGraphics
		: IDisposable
	{
		#region Variables.
		private bool _disposed = false;				// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the current renderer interface.
		/// </summary>
		public GorgonRenderer Renderer
		{
			get;
			private set;
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
		/// Function to perform any clean up when a renderer is updated.
		/// </summary>
		private void CleanUp()
		{
			VideoDevices.Clear();
		}

		/// <summary>
		/// Function to return a renderer object for use with the graphics subsystem.
		/// </summary>
		/// <param name="plugInType">Fully qualified type name of the renderer plug-in</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="plugInType"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the plugInType parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the plug-in type was not found in any of the loaded plug-in assemblies.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the plug-in type was not a renderer plug-in.</para>
		/// </exception>		
		private void BindRenderer(string plugInType)
		{
			GorgonRendererPlugIn plugIn = null;

			GorgonUtility.AssertParamString(plugInType, "plugInType");

			if (!GorgonPlugInFactory.PlugIns.Contains(plugInType))
				throw new ArgumentException("The plug-in '" + plugInType + "' was not found in any of the loaded plug-in assemblies.", "plugInType");

			plugIn = GorgonPlugInFactory.PlugIns[plugInType] as GorgonRendererPlugIn;

			if (plugIn == null)
				throw new ArgumentException("The plug-in '" + plugInType + "' is not a renderer plug-in.", "plugInType");

			if (Renderer != null)
				Renderer.Dispose();

			Renderer = plugIn.GetRenderer();
			if (Renderer == null)
				throw new ArgumentException("The plug-in '" + plugInType + "' is not a renderer plug-in.", "plugInType");

			Renderer.Initialize(this);
			Gorgon.Log.Print("Graphics renderer interface '{0}' bound as current renderer.", Diagnostics.GorgonLoggingLevel.Simple, Renderer.Name);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="plugInType">Fully qualified type name of the renderer plug-in</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="plugInType"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the plugInType parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the plug-in type was not found in any of the loaded plug-in assemblies.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the plug-in type was not a renderer plug-in.</para>
		/// </exception>		
		public GorgonGraphics(string plugInType)
		{
			VideoDevices = new GorgonVideoDeviceCollection();
			BindRenderer(plugInType);

			Gorgon.Log.Print("Graphics interface created.", Diagnostics.GorgonLoggingLevel.Simple);
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
					CleanUp();

					if (this.Renderer != null)
					{
						Gorgon.Log.Print("Removing current renderer '{0}'.", Diagnostics.GorgonLoggingLevel.Simple, Renderer.Name);
						this.Renderer.Dispose();
					}

					Renderer = null;
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
