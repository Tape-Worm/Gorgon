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
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;

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
		private GorgonGraphicsPlugIn _plugIn = null;			// Plug-in that created this object.
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
				Gorgon.Log.Print("\tDevice ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, device.DeviceID.FormatHex());
				Gorgon.Log.Print("\tSub System ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, device.SubSystemID.FormatHex());
				Gorgon.Log.Print("\tVendor ID: 0x{0}", Diagnostics.GorgonLoggingLevel.Verbose, device.VendorID.FormatHex());
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
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.MSAAQualityLevel">MSAAQualityLevel</see> property of the settings parameter has a value that cannot be supported by the device.  
		/// The user can check to see if a MSAA value is supported by using <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice.GetMultiSampleQuality">GetMultiSampleQuality</see> method on the video device object.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if the requested video mode is not available for full screen (this will depend on the back end API implementation).</exception>
		protected abstract GorgonDeviceWindow CreateDeviceWindowImpl(string name, GorgonDeviceWindowSettings settings);

		/// <summary>
		/// Function to create a fullscreen multi-head device window.
		/// </summary>
		/// <param name="name">Name of the window.</param>
		/// <param name="multiHeadSettings">Multi-head device window settings.</param>
		/// <returns>A device window.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown if the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.DisplayMode">DisplayMode</see> property of the <paramref name="multiHeadSettings"/>.Settings parameter is a video mode that cannot be used.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">BoundWindow</see> property of the multiHeadSettings.Settings parameter is already a device window.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">BoundWindow</see> property of the multiHeadSettings.Settings parameter is a child control.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.MSAAQualityLevel">MSAAQualityLevel</see> property of the multiHeadSettings.Settings parameter has a value that cannot be supported by the device.  
		/// The user can check to see if a MSAA value is supported by using <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice.GetMultiSampleQuality">GetMultiSampleQuality</see> method on the video device object.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">BoundWindow</see> property of the multiHeadSettings.Settings parameter is NULL (Nothing in VB.Net).</para>		
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if the requested video mode is not available for full screen (this will depend on the back end API implementation).</exception>
		protected abstract GorgonMultiHeadDeviceWindow CreateDeviceWindowImpl(string name, GorgonMultiHeadSettings multiHeadSettings);

		/// <summary>
		/// Function to create a fullscreen multi-head device window.
		/// </summary>
		/// <param name="name">Name of the window.</param>
		/// <param name="multiHeadSettings">Multi-head device window settings.</param>
		/// <returns>A full screen multi-head device window.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown if the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.DisplayMode">DisplayMode</see> property of the <paramref name="multiHeadSettings"/>.Settings parameter is a video mode that cannot be used.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">BoundWindow</see> property of the multiHeadSettings.Settings parameter is already a device window.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">BoundWindow</see> property of the multiHeadSettings.Settings parameter is a child control.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.MSAAQualityLevel">MSAAQualityLevel</see> property of the multiHeadSettings.Settings parameter has a value that cannot be supported by the device.  
		/// The user can check to see if a MSAA value is supported by using <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice.GetMultiSampleQuality">GetMultiSampleQuality</see> method on the video device object.</para>
		/// <para>-or-</para>
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">BoundWindow</see> property of the multiHeadSettings.Settings parameter is NULL (Nothing in VB.Net).</para>		
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if the requested video mode is not available for full screen (this will depend on the back end API implementation).</exception>
		/// <remarks>
		/// This overload is used for creating a full screen multi-head device window.  These allow multi-head video devices to operate with shared data in fullscreen mode.  The first element in the <see cref="P:GorgonLibrary.Graphics.GorgonMultiHeadsettings.Settings">multiHeadSettings.Settings</see> must be the master output (first VideoOutput in the GorgonVideoDevice.Outputs property).
		/// <para>
		/// If the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.DisplayMode">DisplayMode</see> width or height are 0 (Nothing in VB.Net), then
		/// the client width and height of the window will be used, and the default display format will be used.
		/// </para>
		/// <para>
		/// Multi-head device windows bound to child controls cannot go full screen.  If one of the BoundWindow settings is a child control then an exception will be thrown.  
		/// Due to the requirement that these be full screen device windows, the multi-head device window must be used with a form.
		/// </para>
		/// <para>For some implementations (e.g. Direct 3D 9), an element in the multiHeadSettings.Settings for each of the heads must be included.</para>
		/// <para>The device window will use these default settings (unless specified by the user):
		/// <list type="table">
		/// <listheader>
		///		<term>Property</term>
		///		<description>Default Value</description>
		/// </listheader>
		///		<item><term>BackBufferCount</term><description>2</description></item>
		///		<item><term>DisplayFunction</term><description>Discard</description></item>
		///		<item><term>MSAAQualityLevel</term><description>NULL (Nothing in VB.Net), which indicates no MSAA.</description></item>
		///		<item><term>VSyncInterval</term><description>None</description></item>
		///		<item><term>WillUseVideo</term><description>FALSE</description></item>
		/// </list>		
		/// </para>
		/// </remarks>
		public GorgonMultiHeadDeviceWindow CreateDeviceWindow(string name, GorgonMultiHeadSettings multiHeadSettings)
		{
			GorgonMultiHeadDeviceWindow target = null;
			IntPtr monitorHandle = IntPtr.Zero;

			if (multiHeadSettings == null)
				throw new ArgumentNullException("multiHeadSettings");

			if (multiHeadSettings.Settings.Count() < 1)
				throw new ArgumentException("There were no device settings in the multi-head settings.", "multiHeadSettings");

			// Force all settings to use the same windowed/full screen state as the first (master head) in the list.
			foreach (var setting in multiHeadSettings.Settings)
			{
				setting.IsWindowed = false;

				// Don't allow full screen if the bound window is a child control.
				if (!(setting.BoundWindow is Form))
					throw new ArgumentException("Cannot switch to full screen with a child control.", "fullScreen");

				// Make sure none of the windows are in use already.
				var inUse = _trackedObjects.Count(item =>
				{
					GorgonDeviceWindow devWindow = item as GorgonDeviceWindow;
					return ((devWindow != null) && (devWindow.Settings.BoundWindow == setting.BoundWindow));
				}) > 0;

				if (inUse)					
					throw new ArgumentException("The specified window is already a device window.", "window");

				// If we haven't specified a size, or we're using a child control, then assume the size of the bound window.
				if (setting.Dimensions == System.Drawing.Size.Empty)
					setting.Dimensions = setting.BoundWindow.ClientSize;

				// Ensure that the first setting is for the master video output.
				if ((setting == multiHeadSettings.Settings[0]) && (setting.Output != setting.Device.Outputs[0]))
					throw new ArgumentException("The video output for the first setting is not the master video output.", "multiHeadSettings");
			}

			Gorgon.Log.Print("Creating new multi-head device window '{0}'.", Diagnostics.GorgonLoggingLevel.Simple, name);
			target = CreateDeviceWindowImpl(name, multiHeadSettings);
	
			for (int i = 0; i < multiHeadSettings.Settings.Count; i++)
			{
				var settings = multiHeadSettings.Settings[i];
				Gorgon.Log.Print("Initializing head '{0}' with settings: {1}x{2} Format: {3} Refresh Rate: {4}/{5}.", Diagnostics.GorgonLoggingLevel.Verbose, i, settings.DisplayMode.Width, settings.DisplayMode.Height, settings.DisplayMode.Format, settings.DisplayMode.RefreshRateNumerator, settings.DisplayMode.RefreshRateDenominator);
				Gorgon.Log.Print("\tLayout: {0}x{1} Format: {2} Refresh Rate: {3}/{4}", Diagnostics.GorgonLoggingLevel.Verbose, settings.DisplayMode.Width, settings.DisplayMode.Height, settings.DisplayMode.Format, settings.DisplayMode.RefreshRateNumerator, settings.DisplayMode.RefreshRateDenominator);
				Gorgon.Log.Print("\tDepth/Stencil: {0} (Format: {1})", Diagnostics.GorgonLoggingLevel.Verbose, settings.DepthStencilFormat != GorgonBufferFormat.Unknown, settings.DepthStencilFormat);
				Gorgon.Log.Print("\tWindowed: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.IsWindowed);
				Gorgon.Log.Print("\tMSAA: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.MSAAQualityLevel.Level != GorgonMSAALevel.None);
				if (settings.MSAAQualityLevel.Level != GorgonMSAALevel.None)
					Gorgon.Log.Print("\t\tMSAA Quality: {0}  Level: {1}", Diagnostics.GorgonLoggingLevel.Verbose, settings.MSAAQualityLevel.Quality, settings.MSAAQualityLevel.Level);
				Gorgon.Log.Print("\tBackbuffer Count: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.BackBufferCount);
				Gorgon.Log.Print("\tDisplay Function: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.DisplayFunction);
				Gorgon.Log.Print("\tV-Sync interval: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.VSyncInterval);
				Gorgon.Log.Print("\tVideo surface: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.WillUseVideo);
			}

			target.Initialize();

			_trackedObjects.Add(target);

			Gorgon.Log.Print("Multi-head device window '{0}' created succesfully.", Diagnostics.GorgonLoggingLevel.Simple, name);

			return target;
		}

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
		/// <para>Thrown if the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.MSAAQualityLevel">MSAAQualityLevel</see> property of the settings parameter has a value that cannot be supported by the device.  
		/// The user can check to see if a MSAA value is supported by using <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice.GetMultiSampleQuality">GetMultiSampleQuality</see> method on the video device object.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if the requested video mode is not available for full screen (this will depend on the back end API implementation).</exception>
		/// <remarks>
		/// If the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.DisplayMode">DisplayMode</see> width or height are 0 (Nothing in VB.Net), then
		/// the client width and height of the window will be used, and the default display format will be used.
		/// <para>
		/// If the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.BoundWindow">settings.BoundWindow</see> property is set to NULL (Nothing in VB.Net), then it will use the <see cref="GorgonLibrary.Gorgon.ApplicationForm">default Gorgon application window</see>.
		/// </para>
		/// <para>
		/// Device windows bound to child controls cannot go full screen, setting the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.IsWindowed">IsWindowed</see> property to FALSE will have no effect.  Also, the width and height of the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.DisplayMode">settings.DisplayMode</see> property will 
		/// use the client width and height of the window.
		/// </para>
		/// <para>The device window will use these default settings (unless specified by the user):
		/// <list type="table">
		/// <listheader>
		///		<term>Property</term>
		///		<description>Default Value</description>
		/// </listheader>
		///		<item><term>BackBufferCount</term><description>2</description></item>
		///		<item><term>DisplayFunction</term><description>Discard</description></item>
		///		<item><term>MSAAQualityLevel</term><description>NULL (Nothing in VB.Net), which indicates no MSAA.</description></item>
		///		<item><term>VSyncInterval</term><description>None</description></item>
		///		<item><term>WillUseVideo</term><description>FALSE</description></item>
		/// </list>		
		/// </para>
		/// </remarks>
		public GorgonDeviceWindow CreateDeviceWindow(string name, GorgonDeviceWindowSettings settings)
		{
			GorgonDeviceWindow target = null;
			IntPtr monitorHandle = IntPtr.Zero;

			// For child controls, do not go to full screen.
			if ((!(settings.BoundWindow is Form)) && (!settings.IsWindowed))
				throw new ArgumentException("Cannot switch to full screen with a child control.", "fullScreen");

			// Ensure that we're not already using this window as a device window.
			var inUse = _trackedObjects.Count(item =>
			{
				GorgonDeviceWindow devWindow = item as GorgonDeviceWindow;
				return ((devWindow != null) && (devWindow.Settings.BoundWindow == settings.BoundWindow));
			}) > 0;

			if (inUse)
				throw new ArgumentException("The specified window is already a device window.", "window");

			// If we haven't specified a size, or we're using a child control, then assume the size of the bound window.
			if ((settings.Dimensions == System.Drawing.Size.Empty) || (!(settings.BoundWindow is Form)))
				settings.Dimensions = settings.BoundWindow.ClientSize;

			Gorgon.Log.Print("Creating new device window '{0}'.", Diagnostics.GorgonLoggingLevel.Simple, name);

			// Find out which device and output contain the window.
			if (settings.Output == null)
			{
				monitorHandle = Win32API.GetMonitor(settings.BoundWindow);
				if (monitorHandle == IntPtr.Zero)
					throw new GorgonException(GorgonResult.CannotCreate, "Could not create the device window.  Could not locate the monitor on which the window is placed.");

				// Find the correct video output.
				var videoOutput = (from device in VideoDevices
								   from output in device.Outputs
								   where output.Handle == monitorHandle
								   select output).Single();
				
				settings.Output = videoOutput;
			}

			if ((settings.Device == null) || (settings.Device.Outputs.Contains(settings.Output)))
			{
				var videoDevice = (from device in VideoDevices
								   from output in device.Outputs
								   where output == settings.Output
								   select device).Single();

				// Find the first device that contains the window.
				settings.Device = videoDevice;
			}
			
			Gorgon.Log.Print("\tWindow 0x{0} is located on device: '{1}', using output: '{2}'.", GorgonLoggingLevel.Verbose,settings.BoundWindow.Handle.FormatHex(), settings.Device.Name, settings.Output.Name);
			target = CreateDeviceWindowImpl(name, settings);
			Gorgon.Log.Print("Initializing new device window '{0}' with settings: {1}x{2} Format: {3} Refresh Rate: {4}/{5}.", Diagnostics.GorgonLoggingLevel.Verbose, name, settings.DisplayMode.Width, settings.DisplayMode.Height, settings.DisplayMode.Format, settings.DisplayMode.RefreshRateNumerator, settings.DisplayMode.RefreshRateDenominator);
			target.Initialize();

			_trackedObjects.Add(target);

			Gorgon.Log.Print("'{0}' information:", Diagnostics.GorgonLoggingLevel.Verbose, name);
			Gorgon.Log.Print("\tLayout: {0}x{1} Format: {2} Refresh Rate: {3}/{4}", Diagnostics.GorgonLoggingLevel.Verbose, settings.DisplayMode.Width, settings.DisplayMode.Height, settings.DisplayMode.Format, settings.DisplayMode.RefreshRateNumerator, settings.DisplayMode.RefreshRateDenominator);
			Gorgon.Log.Print("\tDepth/Stencil: {0} (Format: {1})", Diagnostics.GorgonLoggingLevel.Verbose, settings.DepthStencilFormat != GorgonBufferFormat.Unknown, settings.DepthStencilFormat);
			Gorgon.Log.Print("\tWindowed: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.IsWindowed);
			Gorgon.Log.Print("\tMSAA: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.MSAAQualityLevel.Level != GorgonMSAALevel.None);
			if (settings.MSAAQualityLevel.Level != GorgonMSAALevel.None)
				Gorgon.Log.Print("\t\tMSAA Quality: {0}  Level: {1}", Diagnostics.GorgonLoggingLevel.Verbose, settings.MSAAQualityLevel.Quality, settings.MSAAQualityLevel.Level);
			Gorgon.Log.Print("\tBackbuffer Count: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.BackBufferCount);
			Gorgon.Log.Print("\tDisplay Function: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.DisplayFunction);
			Gorgon.Log.Print("\tV-Sync interval: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.VSyncInterval);
			Gorgon.Log.Print("\tVideo surface: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.WillUseVideo);

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
		/// <returns>A graphics interface.</returns>
		/// <remarks>When the graphics interface is created, it is only created once.  This means that subsequent calls to CreateGraphics (with the same plug-in) will always return a single instance until the first instance of the graphics object is disposed.</remarks>
		public static GorgonGraphics CreateGraphics(string plugInType, IList<GorgonNamedValue<string>> customSettings)
		{
			GorgonGraphicsPlugIn plugIn = null;
			GorgonGraphics graphics = null;

			GorgonDebug.AssertParamString(plugInType, "plugInType");

			if (!Gorgon.PlugIns.Contains(plugInType))
				throw new ArgumentException("The plug-in '" + plugInType + "' was not found in any of the loaded plug-in assemblies.", "plugInType");

			plugIn = Gorgon.PlugIns[plugInType] as GorgonGraphicsPlugIn;

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
			graphics._plugIn = plugIn;
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
		/// <returns>A graphics interface.</returns>
		/// <remarks>When the graphics interface is created, it is only created once.  This means that subsequent calls to CreateGraphics (with the same plug-in) will always return a single instance until the first instance of the graphics object is disposed.</remarks>
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
					// Detach this instance from the plug-in that created it.
					_plugIn.GraphicsInstance = null;

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
