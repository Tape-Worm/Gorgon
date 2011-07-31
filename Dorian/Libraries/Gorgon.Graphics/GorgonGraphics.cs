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
using GorgonLibrary.Collections;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The primary object for the graphics sub system.
	/// </summary>
	public abstract class GorgonGraphics
		: GorgonNamedObject, IDisposable
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
			internal set;
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
		/// Function to enumerate the available video devices on the system.
		/// </summary>
		private void EnumerateVideoDevices()
		{
			VideoDevices = new GorgonVideoDeviceCollection(GetVideoDevices());
			VideoDevices.GetCapabilities();

			// Log device information.
			Gorgon.Log.Print("{0} video devices installed.", Diagnostics.GorgonLoggingLevel.Simple, VideoDevices.Count);
			foreach (var device in VideoDevices)
			{
				Gorgon.Log.Print("Info for Video Device #{0} - {1}:", Diagnostics.GorgonLoggingLevel.Simple, device.Index, device.Name);
				Gorgon.Log.Print("\tHead count: {0}", Diagnostics.GorgonLoggingLevel.Simple, device.Outputs.Count);
				Gorgon.Log.Print("\tDevice Name: {0}", Diagnostics.GorgonLoggingLevel.Simple, device.DeviceName);
				Gorgon.Log.Print("\tDriver Name: {0}", Diagnostics.GorgonLoggingLevel.Simple, device.DriverName);
				Gorgon.Log.Print("\tDriver Version: {0}", Diagnostics.GorgonLoggingLevel.Simple, device.DriverVersion.ToString());
				Gorgon.Log.Print("\tRevision: {0}", Diagnostics.GorgonLoggingLevel.Simple, device.Revision);
				Gorgon.Log.Print("\tDevice ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, GorgonUtility.FormatHex(device.DeviceID));
				Gorgon.Log.Print("\tSub System ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, GorgonUtility.FormatHex(device.SubSystemID));
				Gorgon.Log.Print("\tVendor ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, GorgonUtility.FormatHex(device.VendorID));
				Gorgon.Log.Print("\tDevice GUID: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.DeviceGUID);

				Gorgon.Log.Print("#{0} - {1} device capabilities:", Diagnostics.GorgonLoggingLevel.Verbose, device.Index, device.Name);
				Gorgon.Log.Print("\tPixel Shader Version: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.Capabilities.PixelShaderVersion);
				Gorgon.Log.Print("\tVertex Shader Version: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.Capabilities.VertexShaderVersion);
				Gorgon.Log.Print("\tAlpha Comparison Flags: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.Capabilities.AlphaComparisonFlags);
				Gorgon.Log.Print("\tDepth Comparison Flags: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.Capabilities.DepthComparisonFlags);
				Gorgon.Log.Print(string.Empty, Diagnostics.GorgonLoggingLevel.Verbose, Name);
				Gorgon.Log.Print("\t{1} {0} Specific Custom Capabilities:", Diagnostics.GorgonLoggingLevel.Verbose, Name, device.Name);

				foreach (var capability in device.Capabilities.CustomCapabilities)
					Gorgon.Log.Print("\t\t{0}: {1}", Diagnostics.GorgonLoggingLevel.Verbose, capability.Name, capability.Value);

				Gorgon.Log.Print(string.Empty, Diagnostics.GorgonLoggingLevel.Verbose, Name);

				// List the video modes for each head.
				for (int i = 0; i < device.Outputs.Count; i++)
				{
					var output = device.Outputs[i];
					Gorgon.Log.Print("#{0} - {1} Head #{2} ({3}) Video Modes:", Diagnostics.GorgonLoggingLevel.Verbose, device.Index, device.Name, i, output.Name);

					foreach (var mode in output.VideoModes)
						Gorgon.Log.Print("\t{0}x{1} Refresh Rate: {2}/{3} Format: {4}", Diagnostics.GorgonLoggingLevel.Verbose, mode.Width, mode.Height, mode.RefreshRateNumerator, mode.RefreshRateDenominator, mode.Format);
				}
			}
		}

		/// <summary>
		/// Function to initialize the graphics interface.
		/// </summary>
		protected abstract void InitializeGraphics();

		/// <summary>
		/// Function to perform clean up in the plug-in.
		/// </summary>
		protected abstract void CleanUpGraphics();

		/// <summary>
		/// Function to create the renderer interface.
		/// </summary>
		/// <returns>The renderer interface.</returns>
		protected abstract GorgonRenderer CreateRenderer();

		/// <summary>
		/// Function to perform any clean up when a renderer is updated.
		/// </summary>
		protected void CleanUp()
		{
			Gorgon.Log.Print("{0} shutting down...", Diagnostics.GorgonLoggingLevel.Simple, Name);

			Renderer = null;
			VideoDevices = null;

			CleanUpGraphics();
			Gorgon.Log.Print("{0} shut down successfully", Diagnostics.GorgonLoggingLevel.Simple, Name);
		}	

		/// <summary>
		/// Function to return a list of all video devices installed on the system.
		/// </summary>
		/// <returns>An enumerable list of video devices.</returns>
		protected abstract IEnumerable<KeyValuePair<string, GorgonVideoDevice>> GetVideoDevices();

		/// <summary>
		/// Function to create a device window in back end API.
		/// </summary>
		/// <param name="name">Name of the window.</param>
		/// <param name="window">Window to bind to the device window.</param>
		/// <param name="mode">Video mode to set.</param>
		/// <param name="depthStencilFormat">Format for the depth/stencil buffer.</param>
		/// <param name="fullScreen">TRUE to use fullscreen mode, FALSE to use windowed.</param>
		/// <returns>A device window.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="window"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown if the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if the <paramref name="mode"/> is a video mode that cannot be used.</para>
		/// </exception>
		protected abstract GorgonDeviceWindow CreateDeviceWindowImpl(string name, Control window, GorgonVideoMode mode, GorgonBufferFormat depthStencilFormat, bool fullScreen);

		/// <summary>
		/// Function to create a device window.
		/// </summary>
		/// <param name="name">Name of the window.</param>
		/// <param name="window">Window to bind to the device window.</param>
		/// <param name="mode">Video mode to set.</param>
		/// <param name="depthStencilFormat">Format for the depth/stencil buffer.</param>
		/// <param name="fullScreen">TRUE to use fullscreen mode, FALSE to use windowed.</param>
		/// <returns>A device window.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="window"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown if the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if the <paramref name="mode"/> is a video mode that cannot be used.</para>
		/// </exception>
		public GorgonDeviceWindow CreateDeviceWindow(string name, Control window, GorgonVideoMode mode, GorgonBufferFormat depthStencilFormat, bool fullScreen)
		{
			GorgonDeviceWindow target = null;

			if (window == null)
				throw new ArgumentNullException("window");

			target = CreateDeviceWindowImpl(name, window, mode, depthStencilFormat, fullScreen);
			target.Initialize();

			return target;
		}

		/// <summary>
		/// Function to create a device window.
		/// </summary>
		/// <param name="name">Name of the window.</param>
		/// <param name="mode">Video mode to set.</param>
		/// <param name="depthStencilFormat">Format for the depth/stencil buffer.</param>
		/// <param name="fullScreen">TRUE to use fullscreen mode, FALSE to use windowed.</param>
		/// <returns>A device window.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown if the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if the <paramref name="mode"/> is a video mode that cannot be used.</para>
		/// </exception>
		public GorgonDeviceWindow CreateDeviceWindow(string name, GorgonVideoMode mode, GorgonBufferFormat depthStencilFormat, bool fullScreen)
		{
			return CreateDeviceWindow(name, Gorgon.ApplicationWindow, mode, depthStencilFormat, fullScreen);
		}

		/// <summary>
		/// Function to return a graphics system.
		/// </summary>
		/// <param name="plugInType">Fully qualified type name of the graphics plug-in</param>
		/// <param name="customSettings">A named list of custom settings to pass to the renderer.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="plugInType"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the plugInType parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the plug-in type was not found in any of the loaded plug-in assemblies.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the plug-in type was not a graphics plug-in.</para>
		/// </exception>		
		public static GorgonGraphics CreateGraphics(string plugInType, IList<GorgonNamedValue<string>> customSettings)
		{
			GorgonGraphicsPlugIn plugIn = null;
			GorgonGraphics graphics = null;

			GorgonUtility.AssertParamString(plugInType, "plugInType");

			if (!GorgonPlugInFactory.PlugIns.Contains(plugInType))
				throw new ArgumentException("The plug-in '" + plugInType + "' was not found in any of the loaded plug-in assemblies.", "plugInType");

			plugIn = GorgonPlugInFactory.PlugIns[plugInType] as GorgonGraphicsPlugIn;

			if (plugIn == null)
				throw new ArgumentException("The plug-in '" + plugInType + "' is not a graphics plug-in.", "plugInType");

			graphics = plugIn.GetGraphics();
			if (graphics == null)
				throw new ArgumentException("The plug-in '" + plugInType + "' is not a graphics plug-in.", "plugInType");

			Gorgon.Log.Print("Graphics interface '{0}' initializing...", Diagnostics.GorgonLoggingLevel.Simple, graphics.Name);
			if (customSettings != null)
			{
				foreach (var namedValue in customSettings)
				{
					if (graphics.CustomSettings.Contains(namedValue.Name))
						graphics.CustomSettings[namedValue.Name] = namedValue.Value;
				}
			}
			graphics.InitializeGraphics();
			graphics.EnumerateVideoDevices();
			graphics.CreateRenderer();
			Gorgon.Log.Print("Graphics interface '{0}' initialized successfully.", Diagnostics.GorgonLoggingLevel.Simple, graphics.Name);

			return graphics;
		}

		/// <summary>
		/// Function to return a graphics system.
		/// </summary>
		/// <param name="plugInType">Fully qualified type name of the graphics plug-in</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="plugInType"/> parameter is NULL (Nothing in VB.Net)</exception>
		/// <exception cref="System.ArgumentException">Thrown when the plugInType parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the plug-in type was not found in any of the loaded plug-in assemblies.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the plug-in type was not a graphics plug-in.</para>
		/// </exception>		
		public static GorgonGraphics CreateGraphics(string plugInType)
		{
			return CreateGraphics(plugInType, null);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		protected GorgonGraphics(string name)
			: base(name)
		{
			CustomSettings = new GorgonCustomValueCollection<string>();
			VideoDevices = null;
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
