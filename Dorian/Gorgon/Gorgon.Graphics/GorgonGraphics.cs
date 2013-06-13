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
using GorgonLibrary.Collections.Specialized;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;
using GorgonLibrary.Native;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Operators used for comparison operations.
	/// </summary>
	public enum ComparisonOperators
	{
		/// <summary>
		/// Unknown.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// Never pass the comparison.
		/// </summary>
		Never = 1,
		/// <summary>
		/// If the source data is less than the destination data, the comparison passes.
		/// </summary>
		Less = 2,
		/// <summary>
		/// If the source data is equal to the destination data, the comparison passes.
		/// </summary>
		Equal = 3,
		/// <summary>
		/// If the source data is less than or equal to the destination data, the comparison passes.
		/// </summary>
		LessEqual = 4,
		/// <summary>
		/// If the source data is greater than the destination data, the comparison passes.
		/// </summary>
		Greater = 5,
		/// <summary>
		/// If the source data is not equal to the destination data, the comparison passes.
		/// </summary>
		NotEqual = 6,
		/// <summary>
		/// If the source data is greater than or equal to the destination data, the comparison passes.
		/// </summary>
		GreaterEqual = 7,
		/// <summary>
		/// Always pass the comparison.
		/// </summary>
		Always = 8,
	}

	/// <summary>
	/// The primary object for the graphics sub system.
	/// </summary>
	/// <remarks>This interface is used to create all objects (buffers, shaders, etc...) that are to be used for graphics.  An interface is tied to a single physical video device, to use 
	/// multiple video devices, create additional graphics interfaces and assign the device to the <see cref="GorgonLibrary.Graphics.GorgonGraphics.VideoDevice">VideoDevice</see> property.
	/// <para>This object will enumerate video devices, monitor outputs (for multi-head adapters), and video modes for each of the video devices in the system upon creation.  These
    /// items are accessible from the <see cref="GorgonLibrary.Graphics.GorgonVideoDeviceEnumerator">GorgonVideoDeviceEnumerator</see> class. </para>
	/// <para>Please note that this object requires Direct3D 11 (but not necessarily a Direct3D 11 video card) and at least Windows Vista Service Pack 2 or higher.  
	/// Windows XP and operating systems before it will not work, and an exception will be thrown if this object is created on those platforms.</para>
	/// </remarks>
	public class GorgonGraphics
		: IDisposable
	{
		#region Variables.
		private static bool _isDWMEnabled = true;						    // Flag to indicate that the desktop window manager compositor is enabled.
		private static readonly bool _dontEnableDWM;						// Flag to indicate that we should not enable the DWM.
		private bool _disposed;									            // Flag to indicate that the object was disposed.
		private readonly GorgonDisposableObjectCollection _trackedObjects;	// Tracked objects.
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
		/// Property to return the DXGI adapter to use.
		/// </summary>
		internal GI.Adapter1 Adapter
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
#if DEBUG
			    if (D3DDevice == null)
			    {
			        throw new GorgonException(GorgonResult.NotInitialized, "No D3D device object was found.");
			    }
#endif

				return D3DDevice.ImmediateContext;
			}
		}

		/// <summary>
		/// Property to return the Direct3D 11 device object.
		/// </summary>
		internal D3D.Device D3DDevice
		{
			get;
			private set;
		}
					
		/// <summary>
		/// Property to set or return whether DWM composition is enabled or not.
		/// </summary>
		/// <remarks>This property will have no effect on systems that initially have the desktop window manager compositor disabled.</remarks>
		public static bool IsDWMCompositionEnabled
		{
			get
			{
				return _isDWMEnabled;
			}
			set
			{
				if (!value)
				{
					if (_isDWMEnabled)
					{
						Win32API.DwmEnableComposition(0);
						_isDWMEnabled = false;
					}
				}
				else
				{
					if ((!_isDWMEnabled) && (!_dontEnableDWM))
					{
						Win32API.DwmEnableComposition(1);
						_isDWMEnabled = true;
					}
				}
			}
		}

		/// <summary>
		/// Property to return the input geometry interface.
		/// </summary>
		/// <remarks>
		/// The input interface covers items such as the vertex buffer, index buffer, bindings of the aforementioned buffers, the primitive type, etc...
		/// </remarks>
		public GorgonInputGeometry Input
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to return the interface for buffers.
        /// </summary>
        public GorgonBuffers Buffers
        {
            get;
            private set;
        }

		/// <summary>
		/// Property to return the shader interface.
		/// </summary>
		/// <remarks>This is used to create shaders, create constant buffers and bind them to the pipeline.</remarks>
		public GorgonShaderBinding Shaders
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current rasterizer states.
		/// </summary>
		public GorgonRasterizerRenderState Rasterizer
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
		/// Property to return the output merging interface.
		/// </summary>
		/// <remarks>This is responsible for setting blending states, depth/stencil states, creating render targets, etc...</remarks>
		public GorgonOutputMerger Output
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the textures interface.
		/// </summary>
		public GorgonTextures Textures
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the fonts interface.
		/// </summary>
		public GorgonFonts Fonts
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the video device to use for this graphics interface.
		/// </summary>
		public GorgonVideoDevice VideoDevice
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
		/// Function to clean up the categorized interfaces.
		/// </summary>
		private void DestroyInterfaces()
		{
		    if (Fonts != null)
		    {
		        Fonts.CleanUp();
		    }

		    Fonts = null;

		    if (Textures != null)
		    {
		        Textures.CleanUp();
		    }

		    Textures = null;

		    if (Shaders != null)
		    {
		        Shaders.CleanUp();
		    }

		    Shaders = null;

		    if (Output != null)
		    {
		        Output.CleanUp();
		    }

		    Output = null;

		    if (Rasterizer != null)
		    {
                Rasterizer.CleanUp();
		    }
		}

		/// <summary>
		/// Function to return the currently active full screen swap chains.
		/// </summary>
		/// <returns>A list of full screen swap chains.</returns>
		internal IEnumerable<GorgonSwapChain> GetFullscreenSwapChains()
		{
			return (from item in _trackedObjects
					let swapChain = item as GorgonSwapChain
					where (swapChain != null) && (!swapChain.Settings.IsWindowed)
					select swapChain);
		}

		/// <summary>
		/// Function to add a new object to the object tracker.
		/// </summary>
		/// <param name="trackedObject">Object to track.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="trackedObject"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <remarks>Use this to have the graphics interface track your custom object so that it will be disposed when the graphics interface shuts down.</remarks>
		public void AddTrackedObject(IDisposable trackedObject)
		{
			GorgonDebug.AssertNull(trackedObject, "trackedObject");

			_trackedObjects.Add(trackedObject);
		}

		/// <summary>
		/// Function to remove a tracked object from the object tracker.
		/// </summary>
		/// <param name="trackedObject">Tracked object to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="trackedObject"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <remarks>If your custom object is being tracked by the graphics interface, then this must be called in the dispose method to remove it from the tracker.</remarks>
		public void RemoveTrackedObject(IDisposable trackedObject)
		{
			GorgonDebug.AssertNull(trackedObject, "trackedObject");

			_trackedObjects.Remove(trackedObject);
		}

		/// <summary>
		/// Function to retrieve a list of objects created by this interface by its type.
		/// </summary>
		/// <typeparam name="T">Type of object to retrieve.</typeparam>
		/// <returns>A list of objects of the specified type.</returns>
		public IList<T> GetGraphicsObjectOfType<T>()
			where T : IDisposable
		{
			return (from trackedObject in _trackedObjects
						  where trackedObject is T
						  select (T)trackedObject).ToArray();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="device">Video device to use.</param>
		/// <param name="featureLevel">The maximum feature level to support for the devices enumerated.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="featureLevel"/> parameter is invalid.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when Gorgon could not find any video devices that are Shader Model 5, or the down level interfaces (Shader Model 4, and lesser).
		/// <para>-or-</para>
		/// <para>Thrown if the operating system version is not supported.  Gorgon Graphics requires at least Windows Vista Service Pack 2 or higher.</para>
		/// </exception>
		/// <remarks>
		/// The <paramref name="device"/> parameter is the video device that should be used with Gorgon.  If the user passes NULL (Nothing in VB.Net), then the primary device will be used. 
        /// To determine the devices on the system, check the <see cref="GorgonLibrary.Graphics.GorgonVideoDeviceEnumerator">GorgonVideoDeviceEnumerator</see> class.  The primary device will be the first device in this collection. 
		/// <para>The user may pass in a feature level to the featureLevel parameter to limit the feature levels available.  Note that the feature levels imply all feature levels up until the feature level passed in, for example, passing <c>DeviceFeatureLevel.SM4</c> will only allow functionality 
		/// for both Shader Model 4, and Shader Model 2/3 capable video devices, while DeviceFeatureLevel.SM4_1 will include Shader Model 4 with a 4.1 profile and Shader model 2/3 video devices.</para>
		/// <para>If a feature level is not supported by the hardware, then Gorgon will not use that feature level.  That is, passing a SM5 feature level with a SM4 card will only use a SM4 feature level.  If the user omits the feature level (in one of the constructor 
		/// overloads), then Gorgon will use the best available feature level for the video device being used.</para>
		/// </remarks>
		public GorgonGraphics(GorgonVideoDevice device, DeviceFeatureLevel featureLevel)
		{
		    if (featureLevel == DeviceFeatureLevel.Unsupported)
		    {
                throw new ArgumentException(Resources.GORGFX_FEATURE_LEVEL_UNKNOWN);
		    }

		    if (GorgonComputerInfo.OperatingSystemVersion.Major < 6)
		    {
		        throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_INVALID_OS);
		    }

		    _trackedObjects = new GorgonDisposableObjectCollection();
			ResetFullscreenOnFocus = true;

			Gorgon.Log.Print("Gorgon Graphics initializing...", LoggingLevel.Simple);
			
#if DEBUG
			if (!SharpDX.Configuration.EnableObjectTracking)
			{
				SharpDX.Configuration.EnableObjectTracking = true;
			}
#else
			SharpDX.Configuration.EnableObjectTracking = false;
#endif

			if (device == null)
			{
				if (GorgonVideoDeviceEnumerator.VideoDevices.Count == 0)
				{
					GorgonVideoDeviceEnumerator.Enumerate(false, false);
				}				

				// Use the first device in the list.
				device = GorgonVideoDeviceEnumerator.VideoDevices[0];
			}

			if (device != null)
			{
				VideoDevice = device;

			    var D3DDeviceData = VideoDevice.GetDevice(VideoDevice.VideoDeviceType, featureLevel);

				// Create the DXGI factory for the video device.
                GIFactory = D3DDeviceData.Item1;
                Adapter = D3DDeviceData.Item2;
                D3DDevice = D3DDeviceData.Item3;

                D3DDevice.ImmediateContext.ClearState();
				VideoDevice.Graphics = this;
			}

			Gorgon.AddTrackedObject(this);

			// Create interfaces.
			Rasterizer = new GorgonRasterizerRenderState(this);
			Input = new GorgonInputGeometry(this);
			Shaders = new GorgonShaderBinding(this);
			Output = new GorgonOutputMerger(this);
			Textures = new GorgonTextures(this);
			Fonts = new GorgonFonts(this);
            Buffers = new GorgonBuffers(this);

            // Set default states.
		    Rasterizer.States = GorgonRasterizerStates.DefaultStates;
		    Output.BlendingState.States = GorgonBlendStates.DefaultStates;
		    Output.DepthStencilState.States = GorgonDepthStencilStates.NoDepthStencil;

            // Initialize the shaders with default texture sampler settings.
            for (int i = 0; i < Shaders.VertexShader.TextureSamplers.Count; i++)
            {
                Shaders.VertexShader.TextureSamplers[i] = GorgonTextureSamplerStates.DefaultStates;
            }

            for (int i = 0; i < Shaders.PixelShader.TextureSamplers.Count; i++)
            {
                Shaders.PixelShader.TextureSamplers[i] = GorgonTextureSamplerStates.DefaultStates;
            }

            // TODO: For feature level capable devices, initialize Geometry, Hull, Domain and Compute shaders.


			Gorgon.Log.Print("Gorgon Graphics initialized.", LoggingLevel.Simple);
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="device">Video device to use.</param>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when Gorgon could not find any video devices that are Shader Model 5, or the down level interfaces (Shader Model 4, and lesser).
		/// <para>-or-</para>
		/// <para>Thrown if the operating system version is not supported.  Gorgon Graphics requires at least Windows Vista Service Pack 2 or higher.</para>
		/// </exception>
		/// <remarks>
		/// The <paramref name="device"/> parameter is the video device that should be used with Gorgon.  If the user passes NULL (Nothing in VB.Net), then the primary device will be used. 
        /// To determine the devices on the system, check the <see cref="GorgonLibrary.Graphics.GorgonVideoDeviceEnumerator">GorgonVideoDeviceEnumerator</see> class.  The primary device will be the first device in this collection. 
		/// </remarks>
		public GorgonGraphics(GorgonVideoDevice device)
			: this(device, DeviceFeatureLevel.SM5)
		{
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="featureLevel">The maximum feature level to support for the devices enumerated.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="featureLevel"/> parameter is invalid.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when Gorgon could not find any video devices that are Shader Model 5, or the down level interfaces (Shader Model 4, and lesser).
		/// <para>-or-</para>
		/// <para>Thrown if the operating system version is not supported.  Gorgon Graphics requires at least Windows Vista Service Pack 2 or higher.</para>
		/// </exception>
		/// <remarks>The user may pass in a feature level to the featureLevel parameter to limit the feature levels available.  Note that the feature levels imply all feature levels up until the feature level passed in, for example, passing <c>DeviceFeatureLevel.SM4</c> will only allow functionality 
		/// for both Shader Model 4, and Shader Model 2/3 capable video devices, while DeviceFeatureLevel.SM4_1 will include Shader Model 4 with a 4.1 profile and Shader model 2/3 video devices.
		/// <para>If a feature level is not supported by the hardware, then Gorgon will not use that feature level.</para>
		/// </remarks>
		public GorgonGraphics(DeviceFeatureLevel featureLevel)
			: this(null, featureLevel)
		{
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when Gorgon could not find any video devices that are Shader Model 5, or the down level interfaces (Shader Model 4, and lesser).
		/// <para>-or-</para>
		/// <para>Thrown if the operating system version is not supported.  Gorgon Graphics requires at least Windows Vista Service Pack 2 or higher.</para>
		/// </exception>
		public GorgonGraphics()
			: this(null, DeviceFeatureLevel.SM5)
		{
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		static GorgonGraphics()
		{
			Win32API.DwmIsCompositionEnabled(out _isDWMEnabled);

			if (!_isDWMEnabled)
			{
				_dontEnableDWM = true;
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
		    if (_disposed)
		    {
		        return;
		    }

		    if (disposing)
		    {
		        Gorgon.Log.Print("Gorgon Graphics shutting down...", LoggingLevel.Simple);

		        _trackedObjects.ReleaseAll();
		        DestroyInterfaces();

		        Gorgon.Log.Print("Removing D3D11 Device object...", LoggingLevel.Verbose);

		        // Destroy the video device interface.
		        if (D3DDevice != null)
		        {
		            Context.ClearState();
		            D3DDevice.Dispose();
		            D3DDevice = null;
		        }

		        if (Adapter != null)
		        {
		            Adapter.Dispose();
		            Adapter = null;
		        }

		        if (GIFactory != null)
		        {
		            GIFactory.Dispose();
		            GIFactory = null;
		        }

		        if (VideoDevice != null)
		        {
		            VideoDevice.Graphics = null;
		        }

		        Gorgon.Log.Print("Removing DXGI factory interface...", LoggingLevel.Verbose);
		        if (GIFactory != null)
		        {
		            GIFactory.Dispose();
		            GIFactory = null;
		        }

		        // Remove us from the object tracker.
		        Gorgon.RemoveTrackedObject(this);

		        Gorgon.Log.Print("Gorgon Graphics shut down successfully", LoggingLevel.Simple);
		    }					

		    _disposed = true;
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
