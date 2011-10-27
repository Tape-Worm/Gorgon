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
using System.Drawing;
using SlimDX;
using GI = SlimDX.DXGI;
using D3D = SlimDX.Direct3D11;
using GorgonLibrary.Collections;
using GorgonLibrary.Collections.Specialized;
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
		/// <summary>
		/// Used to limit the feature levels that Gorgon will use.  This may not reflect the actual hardware.
		/// </summary>
		internal static readonly DeviceFeatureLevel[] GorgonFeatureLevels = Enum.GetValues(typeof(DeviceFeatureLevel)) as DeviceFeatureLevel[];
		
		private bool _disposed = false;										// Flag to indicate that the object was disposed.
		private Version _minimumSupportedFeatureLevel = new Version(9, 3);	// Minimum supported feature level version.		
		private GorgonTrackedObjectCollection _trackedObjects = null;		// Tracked objects.		
		#endregion

		#region Constants.
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
		/// Property to set or return the initial swap chain.
		/// </summary>
		internal GorgonSwapChain InitialSwapChain
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the maximum feature level to use for any video device.
		/// </summary>
		public DeviceFeatureLevel MaxFeatureLevel
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

		/// <summary>
		/// Property to set or return whether swap chains should reset their full screen setting on regaining focus.
		/// </summary>
		/// <remarks>
		/// This will control whether Gorgon will try to reacquire full screen mode when a full screen swap chain window regains focus.  When this is set to FALSE, and the window 
		/// containing the full screen swap chain loses focus, it will revert to windowed mode and remain in windowed mode.  When set to TRUE, it will try to reacquire full screen mode.
		/// <para>The default value for this is TRUE.  However, for a full screen multimonitor scenario, this should be set to FALSE.</para>
		/// </remarks>
		public bool ResetFullscreenOnFocus
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the currently active full screen swap chains.
		/// </summary>
		/// <returns>A list of full screen swap chains.</returns>
		internal IEnumerable<GorgonSwapChain> GetFullscreenSwapChains()
		{
			return (from item in _trackedObjects
					let swapChain = item as GorgonSwapChain
					where !swapChain.Settings.IsWindowed
					select swapChain);
		}

		/// <summary>
		/// Function to remove a tracked object from the collection.
		/// </summary>
		/// <param name="trackedObject">Tracked object to remove.</param>
		internal void RemoveTrackedObject(IDisposable trackedObject)
		{
			_trackedObjects.Remove(trackedObject);
		}

		/// <summary>
		/// Function to perform updating of the swap chain settings.
		/// </summary>
		/// <param name="settings">Settings to change.</param>
		/// <param name="currentSwapChain">Swap chain that is being updated.</param>
		internal void ValidateSwapChainSettings(GorgonSwapChainSettings settings, GorgonSwapChain currentSwapChain)
		{
			D3D.Device d3dDevice = null;
			IntPtr monitor = IntPtr.Zero;
			GorgonVideoOutput output = null;

			// Default to using the default Gorgon application window.
			if (settings.Window == null)
			{
				settings.Window = Gorgon.ApplicationForm;

				// No application window, then we're out of luck.
				if (settings.Window == null)
					throw new ArgumentException("No window to bind with the swap chain.", "settings");
			}

			// Force to windowed mode if we're binding to a child control on a form.
			if (!(settings.Window is Form))
				settings.IsWindowed = true;

			monitor = Win32API.GetMonitor(settings.Window);		// Get the monitor that the window is on.

			// Find the video output for the window.
			output = (from videoDevice in VideoDevices
					  from videoOutput in videoDevice.Outputs
					  where videoOutput.Handle == monitor
					  select videoOutput).SingleOrDefault();

			if (output == null)
				throw new GorgonException(GorgonResult.CannotCreate, "Could not find the video output for the specified window.");

			// If we've not defined a video device, find out which monitor has the window, and then determine the video device from that.
			if (settings.VideoDevice == null)
			{
				var device = (from videoDevice in VideoDevices
							  where videoDevice.Outputs.Contains(output)
							  select videoDevice).SingleOrDefault();

				// This should never happen, but if it does, we'll be ready for it.
				if (device == null)
					device = VideoDevices[0];

				settings.VideoDevice = device;
			}

			// Get the Direct 3D device instance.
			d3dDevice = settings.VideoDevice.D3DDevice;

			// If we've not defined a video mode, determine the best mode to use.
			if (settings.VideoMode == null)
				settings.VideoMode = new GorgonVideoMode(settings.Window.ClientSize.Width, settings.Window.ClientSize.Height, output.DefaultVideoMode.Format, output.DefaultVideoMode.RefreshRateNumerator, output.DefaultVideoMode.RefreshRateDenominator);

			// If going full screen, ensure that whatever mode we've chosen can be used, otherwise go to the closest match.
			if (!settings.IsWindowed)
			{
				// Check to ensure that no other swap chains on the video output if we're going to full screen mode.
				var swapChainCount = (from item in _trackedObjects
				                    let swapChain = item as GorgonSwapChain
				                    where ((swapChain != null) && (swapChain.VideoOutput == output) && (!swapChain.Settings.IsWindowed) && (swapChain.Settings.Window != settings.Window))
				                    select item).Count();

				if (swapChainCount > 0)
				    throw new GorgonException(GorgonResult.CannotCreate, "There is already a swap chain active on the video output '" + output.Name + "'.");

				swapChainCount = (from item in _trackedObjects
								  let swapChain = item as GorgonSwapChain
								  where ((swapChain != null) && (!swapChain.Settings.IsWindowed))
								  select item).Count();

				// Disable auto reset if we have more than one full screen swap chain.
				//if (swapChainCount > 0)
				//    ResetFullscreenOnFocus = false;

				var modeCount = (from mode in output.VideoModes
								 where ((mode.Width == settings.VideoMode.Value.Width) && (mode.Height == settings.VideoMode.Value.Height) &&
									 (mode.Format == settings.VideoMode.Value.Format) && (mode.RefreshRateNumerator == settings.VideoMode.Value.RefreshRateNumerator) &&
									 (mode.RefreshRateDenominator == settings.VideoMode.Value.RefreshRateDenominator))
								 select mode).Count();

				// We couldn't find the mode in the list, find the nearest match.
				if (modeCount == 0)
					settings.VideoMode = output.FindMode(settings.VideoMode.Value);
			}
			else
			{
				// We don't need a refresh rate for windowed mode.
				settings.VideoMode = new GorgonVideoMode(settings.VideoMode.Value.Width, settings.VideoMode.Value.Height, settings.VideoMode.Value.Format);
			}

			// If we don't pass a format, use the default format.
			if (settings.VideoMode.Value.Format == GorgonBufferFormat.Unknown)
				settings.VideoMode = new GorgonVideoMode(settings.VideoMode.Value.Width, settings.VideoMode.Value.Height, output.DefaultVideoMode.Format);

			// Ensure that the selected video format can be used.
			if (!settings.VideoDevice.SupportsDisplayFormat(settings.VideoMode.Value.Format))
				throw new ArgumentException("Cannot use the format '" + settings.VideoMode.Value.Format.ToString() + "' for display on the video device '" + settings.VideoDevice.Name + "'.");

			// Check multi sampling levels.
			if (settings.SwapEffect == SwapEffect.Sequential)
				settings.MultiSample = new GorgonMultiSampling(1, 0);

			int quality = settings.VideoDevice.GetMultiSampleQuality(settings.VideoMode.Value.Format, settings.MultiSample.Count);

			// Ensure that the quality of the sampling does not exceed what the card can do.
			if ((settings.MultiSample.Quality >= quality) || (settings.MultiSample.Quality < 0))
				throw new ArgumentException("Video device '" + settings.VideoDevice.Name + "' does not support multisampling with a count of '" + settings.MultiSample.Count.ToString() + "' and a quality of '" + settings.MultiSample.Quality.ToString() + " with a format of '" + settings.VideoMode.Value.Format + "'");

			// Force 2 buffers for discard.
			if ((settings.BufferCount < 2) && (settings.SwapEffect == SwapEffect.Discard))
				settings.BufferCount = 2;

			// Perform window handling.
			settings.Window.Visible = true;
			settings.Window.Enabled = true;
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
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is less than 0 or not less than the value returned by <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice">GorgonVideoDevice.GetMultiSampleQuality</see>.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		/// <remarks>This will create our output swap chains for display to a window or control.  All functionality for sending or retrieving data from the video device can be accessed through the swap chain.
		/// <para>Passing default settings for the <see cref="GorgonLibrary.Graphics.GorgonSwapChainSettings">settings parameters</see> will make Gorgon choose the closest possible settings appropriate for the video device and output that the window is on.  For example, passing NULL (Nothing in VB.Net) to 
		/// the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.VideoMode">GorgonSwapChainSettings.VideoMode</see> parameter will make Gorgon find the closest video mode available to the current window size and desktop format (for the output).</para>
		/// <para>If the multisampling quality in the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is higher than what the video device can support, an exception will be raised.  To determine 
		/// what the maximum quality for the sample count for the video device should be, call the <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice.GetMultiSampleQuality">GorgonVideoDevice.GetMultiSampleQuality</see> method.</para>
		/// </remarks>
		public GorgonSwapChain CreateSwapChain(string name, GorgonSwapChainSettings settings)
		{
			GorgonSwapChain swapChain = null;

			if (settings == null)
				throw new ArgumentNullException("settings");

			ValidateSwapChainSettings(settings, null);

			swapChain = new GorgonSwapChain(this, name, settings);
			swapChain.Initialize();
			_trackedObjects.Add(swapChain);			

			return swapChain;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="featureLevel">The maximum feature level to support for the devices enumerated.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="featureLevel"/> parameter is invalid.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when Gorgon could not find any video devices that are capable of using Direct 3D 11, or the down level interfaces (Direct 3D 10.1, 10 or 9.3).</exception>
		/// <remarks>The use may pass multiple feature levels to the featureLevel parameter to allow only specific feature levels available.  For example, passing new GorgonGraphics(DeviceFeatureLevel.10_0_SM4 | DeviceFeatureLevel.9_0_SM3) will only allow functionality
		/// for both Direct3D 10, and Direct 3D 9.
		/// <para>If a feature level is not supported by the hardware, then Gorgon will not use that feature level.  If no feature levels are available (e.g. calling new GorgonGraphics(DeviceFeatureLevel.11_0_SM5) with a Direct 3D 9 or 10 card) then an exception will be raised.</para>
		/// </remarks>
		public GorgonGraphics(DeviceFeatureLevel featureLevel)
		{
			if (featureLevel == DeviceFeatureLevel.Unsupported)
				throw new ArgumentException("Must supply a known feature level.", "featureLevel");

			MaxFeatureLevel = featureLevel;
			_trackedObjects = new GorgonTrackedObjectCollection();
			ResetFullscreenOnFocus = true;

			Gorgon.Log.Print("Gorgon Graphics initializing...", Diagnostics.GorgonLoggingLevel.Simple);

			Gorgon.Log.Print("Creating DXGI interface...", GorgonLoggingLevel.Verbose);
			GIFactory = new GI.Factory1();
			
#if DEBUG
			SlimDX.Configuration.EnableObjectTracking = true;
#else
			SlimDX.Configuration.EnableObjectTracking = false;
#endif

			VideoDevices = new GorgonVideoDeviceCollection(this);

			if (VideoDevices.Count == 0)
				throw new GorgonException(GorgonResult.CannotCreate, "There were no video devices found on this system that can use Direct 3D 11/SM5, 10.x/SM4 or 9.0/SM3.");

			Gorgon.AddTrackedObject(this);

			Gorgon.Log.Print("Gorgon Graphics initialized.", Diagnostics.GorgonLoggingLevel.Simple);
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when Gorgon could not find any video devices that are capable of using Direct 3D 11, or the down level interfaces (Direct 3D 10.1, 10 or 9.3).</exception>
		public GorgonGraphics()
			: this(DeviceFeatureLevel.Level11_0_SM5 | DeviceFeatureLevel.Level10_1_SM4 | DeviceFeatureLevel.Level10_0_SM4 | DeviceFeatureLevel.Level9_0_SM3)
		{
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

					InitialSwapChain = null;

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
