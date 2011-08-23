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
		: GorgonNamedObject, IDisposable, IObjectTracker
	{
		#region Variables.
		private bool _disposed = false;							// Flag to indicate that the object was disposed.
		private IList<IDisposable> _trackedObjects = null;		// List of tracked objects.
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
				Gorgon.Log.Print("\tVSync Interval Flags: {0}", Diagnostics.GorgonLoggingLevel.Verbose, device.Capabilities.VSyncIntervals);
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

			((IObjectTracker)this).CleanUpTrackedObjects();

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
		/// Function to create a device window.
		/// </summary>
		/// <param name="name">Name of the window.</param>
		/// <param name="settings">Device window settings.</param>
		/// <returns>A device window.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown if the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.DisplayMode">DisplayMode</see> property of the <paramref name="settings"/> parameter is a video mode that cannot be used.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">BoundWindow</see> property of the settings parameter is already a device window.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.IsWindowed">IsWindowed</see> property of the settings parameter is FALSE and the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">BoundWindow</see> property of the settings parameter is a child control.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.GorgonDeviceWindowAdvancedSettings.MSAAQualityLevel">MSAAQualityLevel</see> property of the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.AdvancedSettings">advanced settings</see> has a value that cannot be supported by the device.  
		/// The user can check to see if a MSAA value is supported by using <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice.SupportsMultiSampleQualityLevel">SupportsMultiSampleQualityLevel</see> method on the video device object.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if the requested video mode is not available for full screen (this will depend on the back end API implementation).</exception>
		/// <remarks>
		/// If the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.DisplayMode">settings.DisplayMode</see> is NULL (Nothing in VB.Net), then
		/// the client width and height of the window will be used, and the default display format will be used.
		/// <para>
		/// If the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">settings.BoundWindow</see> property is set to NULL (Nothing in VB.Net), then it will use the <see cref="GorgonLibrary.Gorgon.ApplicationForm">default Gorgon application window</see>.
		/// </para>
		/// <para>
		/// Device windows bound to child controls cannot go full screen, setting the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.IsWindowed">IsWindowed</see> property to FALSE will have no effect.  Also, the width and height of the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.DisplayMode">settings.DisplayMode</see> property will 
		/// use the client width and height of the window.
		/// </para>
		/// <para>The device window will use default <see cref="GorgonLibrary.Graphics.GorgonDeviceWindowSettings.GorgonDeviceWindowAdvancedSettings">advanced settings</see> (unless the user specifies different values):
		/// <list type="table">
		/// <listheader>
		///		<term>Property</term>
		///		<description>Default Value</description>
		/// </listheader>
		///		<item>BackBufferCount</item><description>2</description>
		///		<item>DisplayFunction</item><description>Discard</description>
		///		<item>MSAAQualityLevel</item><description>NULL (Nothing in VB.Net), which indicates no MSAA.</description>
		///		<item>VSyncInterval</item><description>None</description>
		///		<item>WillUseVideo</item><description>FALSE</description>
		/// </list>		
		/// </para>
		/// </remarks>
		protected abstract GorgonDeviceWindow CreateDeviceWindowImpl(string name, GorgonDeviceWindowSettings settings);

		/// <summary>
		/// Function to create a device window.
		/// </summary>
		/// <param name="name">Name of the window.</param>
		/// <param name="settings">Device window settings.</param>
		/// <returns>A device window.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown if the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.DisplayMode">DisplayMode</see> property of the <paramref name="settings"/> parameter is a video mode that cannot be used.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">BoundWindow</see> property of the settings parameter is already a device window.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.IsWindowed">IsWindowed</see> property of the settings parameter is FALSE and the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">BoundWindow</see> property of the settings parameter is a child control.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.GorgonDeviceWindowAdvancedSettings.MSAAQualityLevel">MSAAQualityLevel</see> property of the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.AdvancedSettings">advanced settings</see> has a value that cannot be supported by the device.  
		/// The user can check to see if a MSAA value is supported by using <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice.SupportsMultiSampleQualityLevel">SupportsMultiSampleQualityLevel</see> method on the video device object.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if the requested video mode is not available for full screen (this will depend on the back end API implementation).</exception>
		/// <remarks>
		/// If the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.DisplayMode">settings.DisplayMode</see> is NULL (Nothing in VB.Net), then
		/// the client width and height of the window will be used, and the default display format will be used.
		/// <para>
		/// If the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">settings.BoundWindow</see> property is set to NULL (Nothing in VB.Net), then it will use the <see cref="GorgonLibrary.Gorgon.ApplicationForm">default Gorgon application window</see>.
		/// </para>
		/// <para>
		/// Device windows bound to child controls cannot go full screen, setting the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.IsWindowed">IsWindowed</see> property to FALSE will have no effect.  Also, the width and height of the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.DisplayMode">settings.DisplayMode</see> property will 
		/// use the client width and height of the window.
		/// </para>
		/// <para>The device window will use default <see cref="GorgonLibrary.Graphics.GorgonDeviceWindowSettings.GorgonDeviceWindowAdvancedSettings">advanced settings</see> (unless the user specifies different values):
		/// <list type="table">
		/// <listheader>
		///		<term>Property</term>
		///		<description>Default Value</description>
		/// </listheader>
		///		<item>BackBufferCount</item><description>2</description>
		///		<item>DisplayFunction</item><description>Discard</description>
		///		<item>MSAAQualityLevel</item><description>NULL (Nothing in VB.Net), which indicates no MSAA.</description>
		///		<item>VSyncInterval</item><description>None</description>
		///		<item>WillUseVideo</item><description>FALSE</description>
		/// </list>		
		/// </para>
		/// </remarks>
		public GorgonDeviceWindow CreateDeviceWindow(string name, GorgonDeviceWindowSettings settings)
		{
			GorgonDeviceWindow target = null;

			// For child controls, do not go to full screen.
			if ((!(settings.BoundWindow is Form)) && (!settings.Windowed))
				throw new ArgumentException("Cannot switch to full screen with a child control.", "fullScreen");

			// Ensure that we're not already using this window as a device window.
			var inUse = _trackedObjects.Count(item =>
			{
				GorgonDeviceWindow devWindow = item as GorgonDeviceWindow;
				return ((devWindow != null) && (devWindow.BoundWindow == settings.BoundWindow));
			}) > 0;

			if (inUse)
				throw new ArgumentException("The specified window is already a device window.", "window");

			if (settings.DisplayMode == null)
				settings.DisplayMode = new GorgonVideoMode(settings.BoundWindow.ClientSize.Width, settings.BoundWindow.ClientSize.Height, GorgonBufferFormat.Unknown);

			if (!(settings.BoundWindow is Form))
				settings.DisplayMode = new GorgonVideoMode(settings.BoundWindow.ClientSize.Width, settings.BoundWindow.ClientSize.Height, settings.DisplayMode.Value.Format);

			Gorgon.Log.Print("Creating new device window '{0}'.", Diagnostics.GorgonLoggingLevel.Simple, name);
			target = CreateDeviceWindowImpl(name, settings);
			Gorgon.Log.Print("Initializing new device window '{0}' with settings: {1}x{2} Format: {3} Refresh Rate: {4}/{5}.", Diagnostics.GorgonLoggingLevel.Verbose, name, settings.DisplayMode.Value.Width, settings.DisplayMode.Value.Height, settings.DisplayMode.Value.Format, settings.DisplayMode.Value.RefreshRateNumerator, settings.DisplayMode.Value.RefreshRateDenominator);
			target.Initialize();

			_trackedObjects.Add(target);

			Gorgon.Log.Print("'{0}' information:", Diagnostics.GorgonLoggingLevel.Verbose, name);
			Gorgon.Log.Print("\tLayout: {0}x{1} Format: {2} Refresh Rate: {3}/{4}", Diagnostics.GorgonLoggingLevel.Verbose, settings.DisplayMode.Value.Width, settings.DisplayMode.Value.Height, settings.DisplayMode.Value.Format, settings.DisplayMode.Value.RefreshRateNumerator, settings.DisplayMode.Value.RefreshRateDenominator);
			Gorgon.Log.Print("\tDepth/Stencil: {0} (Format: {1})", Diagnostics.GorgonLoggingLevel.Verbose, settings.DepthStencilFormat != GorgonBufferFormat.Unknown, settings.DepthStencilFormat);
			Gorgon.Log.Print("\tWindowed: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.Windowed);
			Gorgon.Log.Print("\tMSAA: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.AdvancedSettings.MSAAQualityLevel != null);
			if (settings.AdvancedSettings.MSAAQualityLevel != null)
				Gorgon.Log.Print("\t\tMSAA Quality: {0}  Level: {1}", Diagnostics.GorgonLoggingLevel.Verbose, settings.AdvancedSettings.MSAAQualityLevel.Value.Quality, settings.AdvancedSettings.MSAAQualityLevel.Value.Level);
			Gorgon.Log.Print("\tBackbuffer Count: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.AdvancedSettings.BackBufferCount);
			Gorgon.Log.Print("\tDisplay Function: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.AdvancedSettings.DisplayFunction);
			Gorgon.Log.Print("\tV-Sync interval: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.AdvancedSettings.VSyncInterval);
			Gorgon.Log.Print("\tVideo surface: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.AdvancedSettings.WillUseVideo);

			Gorgon.Log.Print("Device window '{0}' created succesfully.", Diagnostics.GorgonLoggingLevel.Simple, name);

			return target;
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
			_trackedObjects = new List<IDisposable>();
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

		#region IObjectTracker Members
		#region Properties.
		/// <summary>
		/// Property to return an enumerable list of tracked objects.
		/// </summary>
		IEnumerable<IDisposable> IObjectTracker.TrackedObjects
		{
			get
			{
				return _trackedObjects;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to remove a tracked object from the list.
		/// </summary>
		/// <param name="trackedObject">The tracked object to remove.</param>
		void IObjectTracker.RemoveTrackedObject(IDisposable trackedObject)
		{
			if (_trackedObjects.Contains(trackedObject))
				_trackedObjects.Remove(trackedObject);
		}

		/// <summary>
		/// Function to clean up any objects being tracked.
		/// </summary>
		void IObjectTracker.CleanUpTrackedObjects()
		{
			var trackedObjects = _trackedObjects.ToArray();

			foreach (var trackedObject in trackedObjects)
				trackedObject.Dispose();
			
			_trackedObjects.Clear();
		}
		#endregion
		#endregion
	}
}
