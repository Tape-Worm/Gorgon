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
		/// Function to check the back buffer and depth buffer formats.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		/// <param name="output">Output for the device.</param>
		/// <param name="isWindowed">TRUE if in windowed mode, FALSE if not.</param>
		internal void CheckFormats(GorgonRenderTargetSettings settings, GorgonVideoOutput output, bool isWindowed)
		{
			// Check to see if back buffer is supported.
			if (!output.SupportsBackBufferFormat(settings.Format, isWindowed))
				throw new GorgonException(GorgonResult.FormatNotSupported, "Cannot use the specified format '" + settings.Format.ToString() + "' with the display format '" + output.DefaultVideoMode.Format.ToString() + "'.");

			// Set up the depth buffer if necessary.
			if (settings.DepthStencilFormat != GorgonBufferFormat.Unknown)
			{
				if (!output.SupportsDepthFormat(settings.Format, settings.DepthStencilFormat, isWindowed))
					throw new GorgonException(GorgonResult.FormatNotSupported, "Cannot use the specified depth/stencil format '" + settings.DepthStencilFormat.ToString() + "'.");
			}
		}

		/// <summary>
		/// Function to determine if a full screen video mode is valid or not.
		/// </summary>
		/// <param name="output">Video output for the mode.</param>
		/// <param name="mode">Mode to check.</param>
		/// <returns>A tuple containing a valid refresh rate, otherwise NULL.</returns>
		internal Tuple<int, int> CheckValidVideoMode(GorgonVideoOutput output, GorgonVideoMode mode)
		{
			int count = 0;
			Tuple<int, int> result = null;

			// If we've not specified a refresh rate, then find the lowest refresh on the output.
			if ((mode.RefreshRateDenominator == 0) || (mode.RefreshRateNumerator == 0))
			{
				if (output.VideoModes.Count(item => item.Width == mode.Width &&
													item.Height == mode.Height &&
													item.Format == mode.Format) > 0)
				{
					var refresh = (from refreshMode in output.VideoModes
								   where ((refreshMode.Width == mode.Width) && (refreshMode.Height == mode.Height) && (refreshMode.Format == mode.Format))
								   orderby refreshMode.RefreshRateNumerator ascending
								   select new { refreshMode.RefreshRateNumerator, refreshMode.RefreshRateDenominator }).First();

					result = new Tuple<int, int>(refresh.RefreshRateNumerator, refresh.RefreshRateDenominator);
				}
			}
			else
				result = new Tuple<int, int>(mode.RefreshRateNumerator, mode.RefreshRateDenominator);

			mode.RefreshRateNumerator = result.Item1;
			mode.RefreshRateDenominator = result.Item2;

			count = output.VideoModes.Count(item => item == mode);

			if (count == 0)
				return null;
			else
				return result;
		}

		/// <summary>
		/// Function to determine if a MSAA setting is valid.
		/// </summary>
		/// <param name="settings">Settings to examine.</param>
		/// <param name="videoDevice">Video device to use for examination.</param>
		/// <returns>TRUE if valid, FALSE if not.</returns>
		internal void CheckValidMSAA(GorgonSwapChainSettings settings, GorgonVideoDevice videoDevice)
		{
			// We can only enable multi-sampling when we have a discard swap effect and have non-lockable depth buffers.
			if (settings.MSAAQualityLevel.Level != GorgonMSAALevel.None)
			{
				if ((settings.DisplayFunction == GorgonDisplayFunction.Discard) && (settings.DepthStencilFormat != GorgonBufferFormat.D16_UIntNormal_Lockable)
					&& (settings.DepthStencilFormat != GorgonBufferFormat.D32_Float_Lockable))
				{
					int? qualityLevel = videoDevice.GetMultiSampleQuality(settings.MSAAQualityLevel.Level, settings.Format, true);

					if ((qualityLevel == null) || (qualityLevel < settings.MSAAQualityLevel.Quality))
						throw new ArgumentException("The device cannot support the quality level '" + settings.MSAAQualityLevel.Level.ToString() + "' at a quality of '" + settings.MSAAQualityLevel.Quality.ToString() + "'");
				}
				else
					throw new ArgumentException("Cannot use multi sampling with device windows that don't have a Discard display function and/or use lockable depth/stencil buffers.");
			}
		}

		/// <summary>
		/// Function to determine if a window is already bound to a swap chain or device window.
		/// </summary>
		/// <param name="settings">Settings to examine.</param>
		/// <returns>TRUE if bound, FALSE if not.</returns>
		internal bool CheckWindowBound(GorgonSwapChainSettings settings)
		{
			IObjectTracker tracker = this as IObjectTracker;

			// Ensure that we're not already using this window as a device window.
			return tracker.TrackedObjects.Count(item =>
			{
				GorgonDeviceWindow deviceItem = item as GorgonDeviceWindow;

				// Check the head settings and swap chains.
				if (deviceItem != null)
				{
					if (deviceItem.IsMultiHead)
					{
						if (deviceItem.Settings.HeadSettings.Count(headItem => headItem.BoundWindow == settings.BoundWindow) > 0)
							return true;
					}

					// Check swap chains.
					if ((from trackedObject in ((IObjectTracker)deviceItem).TrackedObjects
						 let swapChain = trackedObject as GorgonSwapChain
						 where ((swapChain != null) && (swapChain.Settings.BoundWindow == settings.BoundWindow))
						 select swapChain).Count() > 0)
					{
						return true;
					}
				}

				return ((deviceItem != null) && (deviceItem.Settings.BoundWindow == settings.BoundWindow));
			}) > 0;
		}

		/// <summary>
		/// Function to validate any settings for a device window.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		internal void ValidateDeviceWindowSettings(GorgonDeviceWindowSettings settings)
		{
			IObjectTracker tracker = this as IObjectTracker;
			System.Windows.Forms.Form window = settings.BoundWindow as System.Windows.Forms.Form;
			IntPtr monitorHandle = IntPtr.Zero;

			if (settings == null)
				throw new ArgumentNullException("settings");

			// Force windowed mode off if we're using multi-head settings.
			if ((settings.HeadSettings.Count > 0) && (settings.IsWindowed))
				settings.IsWindowed = false;

			// For child controls, do not go to full screen.
			if ((window == null) && (!settings.IsWindowed))
				throw new ArgumentException("Cannot switch to full screen with a child control.", "settings");

			// Ensure that we're not already using this window as a device window.
			if (CheckWindowBound(settings))
				throw new ArgumentException("The specified window is already assigned to a device window or swap chain.", "settings");

			// If we haven't specified a size, or we're using a child control, then assume the size of the bound window.
			if ((settings.Dimensions == System.Drawing.Size.Empty) || (window == null))
				settings.Dimensions = settings.BoundWindow.ClientSize;

			// Find out which device and output contain the window.
			if (settings.Output == null) 
				settings.Output = VideoDevices.GetOutputFromControl(settings.BoundWindow);			

			if ((settings.Device == null) || (!settings.Device.Outputs.Contains(settings.Output)))
				settings.Device = settings.Output.VideoDevice;

			// Use the default desktop display format if we haven't picked a format.
			if (settings.Format == GorgonBufferFormat.Unknown)
				settings.Format = settings.Output.DefaultVideoMode.Format;

			// If we're maximized, then use the desktop resolution.
			if ((window != null) && (window.WindowState == System.Windows.Forms.FormWindowState.Maximized))
				settings.Dimensions = new System.Drawing.Size(settings.Output.DefaultVideoMode.Width, settings.Output.DefaultVideoMode.Height);

			// If we are not windowed, don't allow an unknown video mode.
			if (!settings.IsWindowed)
			{
				Tuple<int, int> refreshRate = CheckValidVideoMode(settings.Output, settings.DisplayMode);

				if (refreshRate == null)
				{
					throw new GorgonException(GorgonResult.CannotCreate, "Unable to set the video mode.  The mode '" + settings.Width.ToString() + "x" +
						settings.Height.ToString() + " Format: " + settings.Format.ToString() + " Refresh Rate: " +
						settings.RefreshRateNumerator.ToString() + "/" + settings.RefreshRateDenominator.ToString() +
						"' is not a valid full screen video mode for this video output.");
				}
				else
				{
					settings.RefreshRateNumerator = refreshRate.Item1;
					settings.RefreshRateDenominator = refreshRate.Item2;
				}
			}

			// Check back buffer and depth formats.
			CheckFormats(settings, settings.Output, settings.IsWindowed);

			// We can only enable multi-sampling when we have a discard swap effect and have non-lockable depth buffers.
			CheckValidMSAA(settings, settings.Device);

			// Ensure we don't have more settings than there are heads.
			if (settings.HeadSettings.Count >= settings.Device.Outputs.Count)
				throw new ArgumentException("Cannot have more head settings than there are video outputs.", "settings");

			// Check multi-head settings.
			foreach (var headSetting in settings.HeadSettings)
			{
				// Ensure that we're not already using this window as a device window.
				if (CheckWindowBound(settings))
					throw new ArgumentException("The specified window is already assigned to a device window or swap chain.", "settings");

				if (headSetting.Format == GorgonBufferFormat.Unknown)
					headSetting.Format = headSetting.Output.DefaultVideoMode.Format;

				if (headSetting.Dimensions == System.Drawing.Size.Empty)
					headSetting.Dimensions = headSetting.BoundForm.ClientSize;

				if (headSetting.BoundForm.WindowState == FormWindowState.Maximized)
					headSetting.Dimensions = new System.Drawing.Size(headSetting.Output.DefaultVideoMode.Width, headSetting.Output.DefaultVideoMode.Height);

				if ((settings.Output == headSetting.Output) || (settings.HeadSettings.Count(item => ((item != headSetting) && (item.Output == headSetting.Output))) > 0))
					throw new GorgonException(GorgonResult.CannotCreate, "Cannot have two head settings (or a master device window setting) with the same video output.");

				Tuple<int, int> refreshRate = CheckValidVideoMode(settings.Output, settings.DisplayMode);

				if (refreshRate == null)
				{
					throw new GorgonException(GorgonResult.CannotCreate, "Unable to set the video mode.  The mode '" + headSetting.Width.ToString() + "x" +
						headSetting.Height.ToString() + " Format: " + headSetting.Format.ToString() + " Refresh Rate: " +
						headSetting.RefreshRateNumerator.ToString() + "/" + headSetting.RefreshRateDenominator.ToString() +
						"' is not a valid full screen video mode for the video output '" + headSetting.Output.Name + "'.");
				}
				else
				{
					headSetting.RefreshRateNumerator = refreshRate.Item1;
					headSetting.RefreshRateDenominator = refreshRate.Item2;
				}

				// Check back buffer and depth formats.
				CheckFormats(headSetting, headSetting.Output, false);

				// We can only enable multi-sampling when we have a discard swap effect and have non-lockable depth buffers.
				CheckValidMSAA(headSetting, settings.Device);
			}
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
		/// <exception cref="GorgonLibrary.GorgonException">Thrown if the requested video mode is not available for full screen (this will depend on the back end API implementation).
		/// <para>-or-</para>
		/// <para>Thrown when the depth/stencil format cannot be used with the back buffer format, or when the backbuffer format cannot be used with the display format.</para>
		/// </exception>
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

			ValidateDeviceWindowSettings(settings);

			Gorgon.Log.Print("Creating new device window '{0}'.", Diagnostics.GorgonLoggingLevel.Simple, name);
			
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
