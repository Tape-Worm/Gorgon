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
using SharpDX;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Collections;
using GorgonLibrary.Collections.Specialized;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
#region Event Arguments.
	/// <summary>
	/// Arguments for the before device change event.
	/// </summary>
	public class GorgonBeforeDeviceChangeEventArgs
		: GorgonCancelEventArgs
	{
		#region Variables.
		private GorgonVideoDevice _newDevice = null;		// New video device.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the current video device.
		/// </summary>
		public GorgonVideoDevice CurrentDevice
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the new video device.
		/// </summary>
		public GorgonVideoDevice NewDevice
		{
			get
			{
				return _newDevice;
			}
			set
			{
				if (value == null)
					_newDevice = CurrentDevice;
				else
					_newDevice = value;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonBeforeDeviceChangeEventArgs"/> class.
		/// </summary>
		/// <param name="currentDevice">The current device.</param>
		/// <param name="newDevice">The new device.</param>
		public GorgonBeforeDeviceChangeEventArgs(GorgonVideoDevice currentDevice, GorgonVideoDevice newDevice)
			: base(false)
		{
			CurrentDevice = currentDevice;
			NewDevice = newDevice;
		}
		#endregion	
	}
#endregion

	/// <summary>
	/// The primary object for the graphics sub system.
	/// </summary>
	/// <remarks>This interface is used to create all objects (buffers, shaders, etc...) that are to be used for graphics.  An interface is tied to a single physical video device, to use 
	/// multiple video devices, create additional graphics interfaces and assign the device to the <see cref="GorgonLibrary.Graphics.GorgonGraphics.VideoDevice">VideoDevice</see> property.
	/// <para>This object will enumerate video devices, monitor outputs (for multi-head adapters), and video modes for each of the video devices in the system upon creation.  These
	/// items are accessible in the <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.VideoDevices">VideoDevices</see> property.  The user may force a new enumeration by calling the 
	/// <see cref="M:GorgonLibrary.Graphics.GorgonGraphics.GorgonVideoDeviceCollection.Refresh">VideoDevices.Refresh</see> method.  Please note that doing so will invalidate any objects that were created with 
	/// this interface, and consequently they will need to be <see cref="E:GorgonLibrary.Graphics.GorgonGraphics.BeforeDeviceEnumeration">destroyed before enumeration</see> and <see cref="E:GorgonLibrary.Graphics.GorgonGraphics.AfterDeviceEnumeration">recreated</see>.  This will also reset the current video device will be set to the first video device in the list.</para>
	/// <para>When switching video devices, ensure that all of your objects created by this interface are destroyed.  If they are not, then the graphics interface will attempt to 
	/// destroy any objects that it is aware of for you.  While this may be a convenience, it is better practice to handle the <see cref="E:GorgonLibrary.Graphics.GorgonGraphics.BeforeVideoDeviceChange">BeforeVideoDeviceChange</see> 
	/// event yourself.  You may recreate the objects in the <see cref="E:GorgonLibrary.Graphics.GorgonGraphics.AfterVideoDeviceChange">AfterVideoDeviceChange</see> event using the same interface.</para>
	/// <para>Please note that this object requires Direct3D 11 (but not necessarily a Direct3D 11 video card) and at least Windows Vista Service Pack 2 or higher.  Windows XP and operating systems before it will not work, and an exception will be thrown 
	/// if this object is created on those platforms.</para>
	/// </remarks>
	public class GorgonGraphics
		: IDisposable
	{
		#region Variables.
		/// <summary>
		/// Used to limit the feature levels that Gorgon will use.  This may not reflect the actual hardware.
		/// </summary>
		internal static readonly DeviceFeatureLevel[] GorgonFeatureLevels = Enum.GetValues(typeof(DeviceFeatureLevel)) as DeviceFeatureLevel[];

		private bool _rasterStateChanged = false;							// Flag to indicate that the rasterizer state had changed.
		private bool _disposed = false;										// Flag to indicate that the object was disposed.
		private Version _minimumSupportedFeatureLevel = new Version(9, 3);	// Minimum supported feature level version.		
		private GorgonVideoDevice _videoDevice = null;						// Video device to use.
		private GorgonRasterizerState _defaultRasterState = null;			// Default rasterizer state.
		private GorgonRasterizerState _rasterState = null;					// Rasterizer state.
		#endregion

		#region Constants.
		#endregion

		#region Events.
		/// <summary>
		/// Event that is fired before a video device is changed.
		/// </summary>
		public event EventHandler<GorgonBeforeDeviceChangeEventArgs> BeforeVideoDeviceChange;

		/// <summary>
		/// Event that is fired before a device enumeration.
		/// </summary>
		public event EventHandler<GorgonCancelEventArgs> BeforeDeviceEnumeration;

		/// <summary>
		/// Event that is fired after a device enumeration.
		/// </summary>
		public event EventHandler AfterDeviceEnumeration;

		/// <summary>
		/// Event that is fired after a video device is changed.
		/// </summary>
		public event EventHandler AfterVideoDeviceChange;
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
		/// Property to return the list of tracked objects.
		/// </summary>
		internal GorgonTrackedObjectCollection TrackedObjects
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current device context.
		/// </summary>
		internal D3D.DeviceContext Context
		{
			get
			{
				return VideoDevice.D3DDevice.ImmediateContext;
			}
		}

		/// <summary>
		/// Property to set or return the current rasterizer state.
		/// </summary>
		public GorgonRasterizerState RasterizerState
		{
			get
			{
				if (_rasterState == null)
					return _defaultRasterState;
				else
					return _rasterState;
			}
			set
			{
				if (value != _rasterState)
				{
					_rasterState = value;
					_rasterStateChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to return a list of viewports to apply when rendering.
		/// </summary>
		public GorgonViewportCollection Viewports
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to set or return whether object tracking is disabled.
		/// </summary>
		/// <remarks>This will enable SharpDX's object tracking to ensure references are destroyed upon application exit.
		/// <para>The default value for DEBUG mode is TRUE, and for RELEASE it is set to FALSE.  Disabling object tracking will
		/// give a slight performance increase.</para>
		/// </remarks>
		public bool IsObjectTrackingEnabled
		{
			get
			{
				return SharpDX.Configuration.EnableObjectTracking;
			}
			set
			{
				SharpDX.Configuration.EnableObjectTracking = value;
			}
		}

		/// <summary>
		/// Property to set or return the video device to use for this graphics interface.
		/// </summary>
		/// <remarks>When this value is set to NULL (Nothing in VB.Net), then the first video device in the <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.VideoDevices">VideoDevices</see> collection will be returned.
		/// <para>When the device is changed, all resources associated with the device (swap chains, buffers, etc...) will be destroyed.  The user will be responsible for re-creating these resources, and should do so in the 
		/// <see cref="E:GorgonLibrary.Graphics.GorgonGraphics.AfterDeviceChange">AfterDeviceChange</see> event.</para>
		/// </remarks>
		public GorgonVideoDevice VideoDevice
		{
			get
			{
				return _videoDevice;
			}
			set	
			{
				// Force the first device.
				if (value == null)
				{
					// Ensure that we have a video device to use.
					if (VideoDevices.Count > 0)
						value = VideoDevices[0];
					else
						return;
				}

				// If we're not changing anything, then leave.
				if (value == _videoDevice)
					return;

				// Create the D3D device.
				value.CreateDevice(MaxFeatureLevel);

				// Don't fire the event until we have a current device.
				if (_videoDevice != null)
				{
					GorgonBeforeDeviceChangeEventArgs e = new GorgonBeforeDeviceChangeEventArgs(_videoDevice, value);
					OnBeforeDeviceChange(e);

					// User canceled the change, or set the same device, then leave.
					if ((e.Cancel) || (e.CurrentDevice == e.NewDevice))
					{
						// Destroy the D3D device.
						value.ReleaseDevice();
						return;
					}

					// If we've changed devices, force its creation here.
					if (value != e.NewDevice)
					{
						value = e.NewDevice;
						value.CreateDevice(MaxFeatureLevel);
					}
				}

				// Destroy all the objects created by this instance.
				TrackedObjects.ReleaseAll();

				// Release the current D3D device.
				if (_videoDevice != null)
					_videoDevice.ReleaseDevice();
	
				_videoDevice = value;
				CreateDefaultStates();

				OnAfterDeviceChange();
			}
		}

		/// <summary>
		/// Property to return the maximum feature level to use for any video device.
		/// </summary>
		/// <remarks>This is set in the constructor and is a means for artifically enforcing a specific feature level for a video device.</remarks>
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
		/// Function to create any default states.
		/// </summary>
		private void CreateDefaultStates()
		{
			// Set default states.
			RasterizerState = _defaultRasterState = CreateRasterizerState();
		}

		/// <summary>
		/// Function to fire the <see cref="E:GorgonLibrary.Graphics.GorgonGraphics.BeforeDeviceChange">BeforeDeviceChange</see> event.
		/// </summary>
		/// <param name="e">Event arguments.</param>
		protected virtual void OnBeforeDeviceChange(GorgonBeforeDeviceChangeEventArgs e)
		{
			if (BeforeVideoDeviceChange != null)
				BeforeVideoDeviceChange(this, e);
		}

		/// <summary>
		/// Function to fire the <see cref="E:GorgonLibrary.Graphics.GorgonGraphics.AfterDeviceChange">AfterDeviceChange</see> event.
		/// </summary>
		protected virtual void OnAfterDeviceChange()
		{
			if (AfterVideoDeviceChange != null)
				AfterVideoDeviceChange(this, EventArgs.Empty);
		}

		/// <summary>
		/// Method to clean up devices when the device list is being enumerated.
		/// </summary>
		/// <remarks>TRUE if canceled, FALSE if not.</remarks>
		internal bool CleanUpDeviceOnEnumeration()
		{
			// Don't fire the event until we have a current device.
			if (_videoDevice != null)
			{
				GorgonCancelEventArgs e = new GorgonCancelEventArgs(false);
				if (BeforeDeviceEnumeration != null)
					BeforeDeviceEnumeration(this, e);

				// User canceled the change, leave.
				if (e.Cancel)
					return true;
			}

			// Destroy all the objects created by this instance.
			TrackedObjects.ReleaseAll();

			// Release the current D3D device.
			if (_videoDevice != null)
				_videoDevice.ReleaseDevice();

			_videoDevice = null;

			return false;
		}

		/// <summary>
		/// Method to initialize the current device object.
		/// </summary>
		internal void DeviceEnumerationComplete()
		{
			VideoDevice = null;

			if (AfterDeviceEnumeration != null)
				AfterDeviceEnumeration(this, EventArgs.Empty);
		}

		/// <summary>
		/// Function to return the currently active full screen swap chains.
		/// </summary>
		/// <returns>A list of full screen swap chains.</returns>
		internal IEnumerable<GorgonSwapChain> GetFullscreenSwapChains()
		{
			return (from item in TrackedObjects
					let swapChain = item as GorgonSwapChain
					where (swapChain != null) && (!swapChain.Settings.IsWindowed)
					select swapChain);
		}

		/// <summary>
		/// Function to remove a tracked object from the collection.
		/// </summary>
		/// <param name="trackedObject">Tracked object to remove.</param>
		internal void RemoveTrackedObject(IDisposable trackedObject)
		{
			TrackedObjects.Remove(trackedObject);
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

			GorgonDebug.AssertNull<GorgonSwapChainSettings>(settings, "settings");

			GorgonSwapChain.ValidateSwapChainSettings(this, settings);

			swapChain = new GorgonSwapChain(this, name, settings);
			TrackedObjects.Add(swapChain);
			swapChain.Initialize();

			return swapChain;
		}

		/// <summary>
		/// Function to create a depth/stencil buffer.
		/// </summary>
		/// <param name="name">Name of the depth/stencil buffer.</param>
		/// <param name="settings">Settings to apply to the depth/stencil buffer.</param>
		/// <returns>A new depth/stencil buffer.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonDepthStencilSettings.Format">GorgonDepthStencilSettings.Format</see> property is set to Unknown or is unsupported.</para>
		/// </exception>
		/// <remarks>
		/// A depth buffer may be paired with a swapchain or render target through its DepthStencil property.  When pairing the depth/stencil to the render target, Ensure that the depth/stencil buffer width, height and multisample settings match that of the render target that it is paired with.
		/// <para>The texture for a depth/stencil may be used in a shader for cards that have a feature level of SM_4_1 or better, and can be set to do so by setting the <see cref="P:GorgonLibrary.Graphics.GorgonDepthStencilSettings.TextureFormat">GorgonDepthStencilSettings.TextureFormat</see> property to a typeless format. 
		/// If this is attempted on a video device that has a feature level of SM_4_0 or below, then an exception will be raised.</para>
		/// </remarks>
		public GorgonDepthStencil CreateDepthStencil(string name, GorgonDepthStencilSettings settings)
		{
			GorgonDepthStencil depthBuffer = null;

			GorgonDebug.AssertNull<GorgonDepthStencilSettings>(settings, "settings");

			GorgonDepthStencil.ValidateSettings(this, settings);

			depthBuffer = new GorgonDepthStencil(this, name, settings);
			TrackedObjects.Add(depthBuffer);
			depthBuffer.UpdateSettings();

			return depthBuffer;
		}

		/// <summary>
		/// Function to create a rasterizer state object.
		/// </summary>
		/// <returns>A new rasterizer state object.</returns>
		public GorgonRasterizerState CreateRasterizerState()
		{
			GorgonRasterizerState state = null;

			state = new GorgonRasterizerState(this);
			TrackedObjects.Add(state);
			return state;
		}

		/// <summary>
		/// Function to apply the viewports defined in the viewports collection.
		/// </summary>
		public void ApplyViewports()
		{
			if (Viewports.HasChanged)
			{
				if (Viewports.Count > 1)
					Context.Rasterizer.SetViewports(Viewports.Convert());
				else
					Context.Rasterizer.SetViewport(Viewports[0].Region.X, Viewports[0].Region.Y, Viewports[0].Region.Width, Viewports[0].Region.Height, Viewports[0].MinimumZ, Viewports[0].MaximumZ);

				Viewports.HasChanged = false;
			}
		}

		/// <summary>
		/// Function to apply a specific viewport.
		/// </summary>
		/// <param name="index">Index of the viewport to apply.</param>
		/// <exception cref="System.IndexOutOfRangeException">Thrown when the index is less than 0 or greater than the number of view ports.</exception>
		public void ApplyViewport(int index)
		{
			GorgonDebug.AssertRange(index, Viewports.Count);
			Context.Rasterizer.SetViewport(Viewports[index].Region.X, Viewports[index].Region.Y, Viewports[index].Region.Width, Viewports[index].Region.Height, Viewports[index].MinimumZ, Viewports[index].MaximumZ);
		}

		/// <summary>
		/// Function to apply any outstanding state objects.
		/// </summary>
		public void ApplyStates()
		{
			// Ensure the states have changed before applying.
			if ((_rasterStateChanged) || ((RasterizerState != null) && (RasterizerState.HasChanged)))
				Context.Rasterizer.State = _rasterState.Convert();
		}

		/// <summary>
		/// Function to draw the scene.
		/// </summary>
		/// <remarks>This method applies the various states, shaders and renders the polygons in the vertex buffer to the scene.</remarks>
		public void Draw()
		{
			ApplyViewports();
			ApplyStates();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="featureLevel">The maximum feature level to support for the devices enumerated.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="featureLevel"/> parameter is invalid.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when Gorgon could not find any video devices that are capable of using Direct 3D 11, or the down level interfaces (Direct 3D 10.1, 10 or 9.3).
		/// <para>-or-</para>
		/// <para>Thrown if the operating system version is not supported.  Gorgon Graphics requires at least Windows Vista Service Pack 2 or higher.</para>
		/// </exception>
		/// <remarks>The use may pass multiple feature levels to the featureLevel parameter to allow only specific feature levels available.  For example, passing new GorgonGraphics(DeviceFeatureLevel.10_0_SM4 | DeviceFeatureLevel.9_0_SM3) will only allow functionality
		/// for both Direct3D 10, and Direct 3D 9 capable video devices.
		/// <para>If a feature level is not supported by the hardware, then Gorgon will not use that feature level.  If no feature levels are available (e.g. calling new GorgonGraphics(DeviceFeatureLevel.11_0_SM5) with a Direct 3D 9 or 10 video device) then an exception will be raised.</para>
		/// </remarks>
		public GorgonGraphics(DeviceFeatureLevel featureLevel)
		{
			if (featureLevel == DeviceFeatureLevel.Unsupported)
				throw new ArgumentException("Must supply a known feature level.", "featureLevel");

			if (GorgonComputerInfo.OperatingSystemVersion.Major < 6)
				throw new GorgonException(GorgonResult.CannotCreate, "The Gorgon Graphics interface requires Windows Vista Service Pack 2 or greater.");

			Viewports = new GorgonViewportCollection();
			MaxFeatureLevel = featureLevel;
			TrackedObjects = new GorgonTrackedObjectCollection();
			ResetFullscreenOnFocus = true;

			Gorgon.Log.Print("Gorgon Graphics initializing...", Diagnostics.GorgonLoggingLevel.Simple);

			Gorgon.Log.Print("Creating DXGI interface...", GorgonLoggingLevel.Verbose);
			GIFactory = new GI.Factory1();
			
#if DEBUG
			SharpDX.Configuration.EnableObjectTracking = true;
#else
			SharpDX.Configuration.EnableObjectTracking = false;
#endif

			VideoDevices = new GorgonVideoDeviceCollection(this);
			VideoDevices.Refresh();

			if (VideoDevices.Count == 0)
				throw new GorgonException(GorgonResult.CannotCreate, "There were no video devices found on this system that can use Direct 3D 11/SM5, 10.x/SM4 or 9.0/SM3.");

			Gorgon.AddTrackedObject(this);

			Gorgon.Log.Print("Gorgon Graphics initialized.", Diagnostics.GorgonLoggingLevel.Simple);
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when Gorgon could not find any video devices that are capable of using Direct 3D 11, or the down level interfaces (Direct 3D 10.1, 10 or 9.3).
		/// <para>-or-</para>
		/// <para>Thrown if the operating system version is not supported.  Gorgon Graphics requires at least Windows Vista Service Pack 2 or higher.</para>
		/// </exception>
		public GorgonGraphics()
			: this(DeviceFeatureLevel.SM5 | DeviceFeatureLevel.SM4_1 | DeviceFeatureLevel.SM4 | DeviceFeatureLevel.SM2_a_b)
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
					
					TrackedObjects.ReleaseAll();

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
