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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gorgon.Collections.Specialized;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.Native;
using Gorgon.UI;
using D3D = SharpDX.Direct3D11;
using GI = SharpDX.DXGI;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Operators used for comparison operations.
	/// </summary>
	public enum ComparisonOperator
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
		Always = 8
	}

	/// <summary>
	/// The primary object for the graphics sub system.
	/// </summary>
	/// <remarks>This interface is used to create all objects (buffers, shaders, etc...) that are to be used for graphics.  An interface is tied to a single physical video device, to use 
	/// multiple video devices, create additional graphics interfaces and assign the device to the <see cref="Gorgon.Graphics.GorgonGraphics.VideoDevice">VideoDevice</see> property.
	/// <para>The constructor for this object can take a value known as a device feature level to specify the base line video device capabilities to use.  This feature level value specifies 
	/// what capabilities we have available. To have Gorgon use the best available feature level for your video device, you may call the GorgonGraphics constructor 
	/// without any parameters and it will use the best available feature level for your device.</para>
	/// <para>Along with the feature level, the graphics object can also take a <see cref="Gorgon.Graphics.VideoDevice">Video Device</see> object as a parameter.  Specifying a 
	/// video device will force Gorgon to use that video device for rendering. If a video device is not specified, then the first detected video device will be used.</para>
	/// <para>Please note that graphics objects cannot be shared between devices and must be duplicated.</para>
	/// <para>Objects created by this interface will be automatically tracked and disposed when this interface is disposed.  This is meant to help handle memory leak problems.  However, 
	/// it is important to note that this is not a good practice and the developer is responsible for calling Dispose on all objects that they create, graphics or otherwise.</para>
	/// <para>This object will enumerate video devices, monitor outputs (for multi-head adapters), and video modes for each of the video devices in the system upon creation.  These
    /// items are accessible from the <see cref="Gorgon.Graphics.GorgonVideoDeviceList">GorgonVideoDeviceEnumerator</see> class.</para>
    /// <para>These objects can also be used in a deferred context.  This means that when a graphics object is deferred, it can be used in a multi threaded environment to allow set up of 
    /// a scene by recording commands sent to the video device for execution later on the rendering process.  This is handy where multiple passes for the same scene are required (e.g. a deferred renderer).</para>
	/// <para>Please note that this object requires Direct3D 11 (but not necessarily a Direct3D 11 video card) and at least Windows Vista Service Pack 2 or higher.  
	/// Windows XP and operating systems before it will not work, and an exception will be thrown if this object is created on those platforms.</para>
    /// <para>Deferred graphics contexts require a video device with a feature level of SM5 or better.</para>
	/// </remarks>
    public sealed class GorgonGraphics
        : IDisposable
    {
		#region Variables.
		// Tracked objects.
		private GorgonDisposableObjectCollection _trackedObjects;
		// Flag to indicate that the desktop window manager compositor is enabled.
		private static bool _isDWMEnabled;                                                  
		// Flag to indicate that we should not enable the DWM.
		private static readonly bool _dontEnableDWM;                                        
		// A list of rendering commands for deferred contexts.
		private List<GorgonRenderCommands> _commands;                                       
		// The log interface used to log debug messages.
		private readonly IGorgonLog _log;
		// The video device to use for this graphics object.
		private VideoDevice _videoDevice;
		// Current D3D render targets.
		private D3D.RenderTargetView[] _currentTargets;
		// Currently allocated resources.
		private readonly ConcurrentDictionary<int, GorgonResource> _resources = new ConcurrentDictionary<int, GorgonResource>();
        #endregion

        #region Properties.
		/// <summary>
		/// Property to set or return the D3D device context.
		/// </summary>
		internal D3D.DeviceContext Context => _videoDevice?.Device?.ImmediateContext;

		/// <summary>
		/// Property to return the Direct3D 11 device object.
		/// </summary>
		internal D3D.Device D3DDevice => _videoDevice?.Device;

		/// <summary>
		/// Property to return the DXGI adapter to use.
		/// </summary>
		internal GI.Adapter1 Adapter => _videoDevice?.Adapter;

		/// <summary>
		/// Property to return the <see cref="IGorgonVideoDevice"/> associated with the graphics object.
		/// </summary>
		public IGorgonVideoDevice Device => _videoDevice;

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
	                if (!_isDWMEnabled)
	                {
		                return;
	                }

	                Win32API.DwmEnableComposition(0);
	                _isDWMEnabled = false;
                }
                else
                {
	                if ((_isDWMEnabled) || (_dontEnableDWM))
	                {
		                return;
	                }

	                Win32API.DwmEnableComposition(1);
	                _isDWMEnabled = true;
                }
            }
        }

        /// <summary>
        /// Property to return the immediate graphics object that owns this context.
        /// </summary>
        public GorgonGraphics ImmediateContext
        {
            get;
        }

        /// <summary>
        /// Property to return whether this context is deferred or not.
        /// </summary>
        public bool IsDeferred => ImmediateContext != this;

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
		public IGorgonVideoDevice VideoDevice => _videoDevice;

		/// <summary>
		/// Property to set or return whether object tracking is disabled.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will enable SharpDX's object tracking to ensure references are destroyed upon application exit.
		/// </para>
		/// <para>
		/// The default value for DEBUG mode is <b>true</b>, and for RELEASE it is set to <b>false</b>.  Disabling object tracking will give a slight performance increase.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// This flag <i>must</i> be set prior to creating any <see cref="GorgonGraphics"/> object, or else the flag will not take effect.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public static bool IsObjectTrackingEnabled
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
		/// Property to set or return whether debug output is enabled for the underlying graphics API.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This will enable debug output for the underlying graphics API that Gorgon uses to render (Direct 3D 11 at this time). When this is enabled, all functionality will have debugging information that will 
		/// output to the debug output console (Output window in Visual Studio) if the <c>Debug -> Enable Native Debugging</c> is turned on in the application project settings <i>and</i> the DirectX control panel 
		/// is set up to debug the application under Direct 3D 10/11(/12 for Windows 10) application list.
		/// </para>
		/// <para>
		/// When Gorgon is compiled in DEBUG mode, this flag defaults to <b>true</b>, otherwise it defaults to <b>false</b>.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// This flag <i>must</i> be set prior to creating any <see cref="GorgonGraphics"/> object, or else the flag will not take effect.
		/// </para>
		/// <para>
		/// The D3D11 SDK Layers DLL must be installed in order for this flag to work. If it is not, then the application may crash.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public static bool IsDebugEnabled
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return whether swap chains should reset their full screen setting on regaining focus.
        /// </summary>
        /// <remarks>
        /// This will control whether Gorgon will try to reacquire full screen mode when a full screen swap chain window regains focus.  When this is set to <b>false</b>, and the window 
        /// containing the full screen swap chain loses focus, it will revert to windowed mode and remain in windowed mode.  When set to <b>true</b>, it will try to reacquire full screen mode.
        /// <para>The default value for this is <b>true</b>.  However, for a full screen multimonitor scenario, this should be set to <b>false</b>.</para>
        /// </remarks>
        public bool ResetFullscreenOnFocus
        {
            get;
            set;
        }
        #endregion

        #region Methods.
		/// <summary>
		/// Function to create the video device.
		/// </summary>
		/// <param name="deviceInfo">The video device information used to create the device.</param>
		/// <param name="featureLevel">The requested feature level for the device.</param>
		private void CreateDevice(GorgonVideoDeviceInfo deviceInfo, DeviceFeatureLevel? featureLevel)
		{
			if (deviceInfo == null)
			{
				var videoDevices = new GorgonVideoDeviceList(_log);
#if DEBUG
				videoDevices.Enumerate(true);
#else
				videoDevices.Enumerate();
#endif

				_log.Print("No video device specified.", LoggingLevel.Verbose);

				deviceInfo = (from device in videoDevices
				              where (device.VideoDeviceType == VideoDeviceType.Hardware)
				                    && ((featureLevel == null)
				                        || (device.SupportedFeatureLevel >= featureLevel.Value))
				              orderby device.SupportedFeatureLevel descending, device.Index ascending
				              select device).FirstOrDefault();

				if (deviceInfo == null)
				{
					// Fall back to software in DEBUG mode.
#if DEBUG
					_log.Print("Unable to find a suitable hardware video device, falling back to software device...", LoggingLevel.Verbose);

					deviceInfo = (from device in videoDevices
								  where (device.VideoDeviceType == VideoDeviceType.Software)
										&& ((featureLevel == null)
											|| (device.SupportedFeatureLevel >= featureLevel.Value))
								  orderby device.SupportedFeatureLevel descending, device.Index ascending
								  select device).FirstOrDefault();

					if (deviceInfo == null)
					{
						throw new GorgonException(GorgonResult.DriverError, Resources.GORGFX_ERR_NO_SUITABLE_VIDEO_DEVICE_FOUND);
					}
#else
					throw new GorgonException(GorgonResult.DriverError, Resources.GORGFX_ERR_NO_SUITABLE_VIDEO_DEVICE_FOUND);
#endif
				}
			}

			// If we've not specified a feature level, or the feature level exceeds the requested device feature level, then 
			// fall back to the device feature level.
			if ((featureLevel == null) || (deviceInfo.SupportedFeatureLevel < featureLevel.Value))
			{
				featureLevel = deviceInfo.SupportedFeatureLevel;
			}

			_log.Print($"Using video device '{deviceInfo.Name}' at feature level [{featureLevel.Value}] for Direct 3D 11.", LoggingLevel.Simple);

			_videoDevice = new VideoDevice(deviceInfo, featureLevel.Value, _log);
		}

		/// <summary>
		/// Function to clean up the categorized interfaces.
		/// </summary>
		private void DestroyInterfaces()
        {
			Fonts?.CleanUp();
			Fonts = null;
			Textures?.CleanUp();
			Textures = null;
			Shaders?.CleanUp();
			Shaders = null;
			Output?.CleanUp();
			Output = null;
			Rasterizer?.CleanUp();
			Rasterizer = null;
        }

        /// <summary>
        /// Function to create and initialize the various state objects.
        /// </summary>
        private void CreateStates()
        {
            // Create interfaces.
            Rasterizer = new GorgonRasterizerRenderState(this);
            Input = new GorgonInputGeometry(this);
            Shaders = new GorgonShaderBinding(this);
            Output = new GorgonOutputMerger(this);
            Textures = new GorgonTextures(this);
            Fonts = new GorgonFonts(this);
            Buffers = new GorgonBuffers(this);

            ClearState();
        }

        /// <summary>
        /// Function to release the specified commands list.
        /// </summary>
        /// <param name="commands">Commands to release.</param>
        internal void ReleaseCommands(GorgonRenderCommands commands)
        {
            if ((_commands == null)
                || (commands == null)
                || (!_commands.Contains(commands)))
            {
                return;
            }

            _commands.Remove(commands);
        }

        /// <summary>
        /// Function to retrieve a list of all swap chains that are currently full screen.
        /// </summary>
        /// <returns>The list of full screen swap chains.</returns>
        internal IEnumerable<GorgonSwapChain> GetFullScreenSwapChains()
        {
            return (from graphicsObj in _trackedObjects
                    let swap = graphicsObj as GorgonSwapChain
                    where (swap != null) && (!swap.Settings.IsWindowed)
                    select swap);
        }

		/// <summary>
		/// Function to register a resource with the library.
		/// </summary>
		/// <param name="resource">The resource to register.</param>
		internal void RegisterResource(GorgonResource resource)
		{
			if (resource == null)
			{
				return;
			}

			if (!_resources.TryAdd(resource.ResourceID, resource))
			{
				throw new ArgumentException(string.Format(Resources.GORGFX_ERR_RESOURCE_ID_ALREADY_REGISTERED, resource.Name, resource.ResourceID));
			}
		}

		/// <summary>
		/// Function to unregister a previously registered resource.
		/// </summary>
		/// <param name="resource">The resource to remove.</param>
		internal void UnregisterResource(GorgonResource resource)
		{
			if (resource == null)
			{
				return;
			}

			GorgonResource oldResource;
			_resources.TryRemove(resource.ResourceID, out oldResource);
		}

        /// <summary>
        /// Function to add an object for tracking by the main Gorgon interface.
        /// </summary>
        /// <param name="trackedObject">Object to add.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="trackedObject"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <remarks>This allows Gorgon to track objects and destroy them upon <see cref="GorgonApplication.Quit">termination</see>.</remarks>
        public void AddTrackedObject(IDisposable trackedObject)
        {
            if (trackedObject == null)
            {
                throw new ArgumentNullException(nameof(trackedObject));
            }

            _trackedObjects.Add(trackedObject);
        }

        /// <summary>
        /// Function to remove a tracked object from the Gorgon interface.
        /// </summary>
        /// <param name="trackedObject">Object to remove.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="trackedObject"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <remarks>This will -not- destroy the tracked object.</remarks>
        public void RemoveTrackedObject(IDisposable trackedObject)
        {
            if (trackedObject == null)
            {
                throw new ArgumentNullException(nameof(trackedObject));
            }

            _trackedObjects.Remove(trackedObject);
        }

        /// <summary>
        /// Function to a list of objects being tracked by a type value.
        /// </summary>
        /// <typeparam name="T">Type to search for.</typeparam>
        /// <returns>A list of objects that match the type.</returns>
        public IList<T> GetTrackedObjectsOfType<T>()
            where T : IDisposable
        {
            return (from trackedObject in _trackedObjects
                    where trackedObject is T
                    select (T)trackedObject).ToArray();
        }

		/// <summary>
		/// Function to execute a <see cref="GorgonGraphicsCommand"/> on the immediate context for rendering.
		/// </summary>
		/// <param name="command">Command to execute.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="command"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		public void ExecuteCommand(GorgonGraphicsCommand command)
		{
			if (command == null)
			{
				throw new ArgumentNullException(nameof(command));
			}

			// Bind the render targets on the command.
			if (command.TargetsSet)
			{
				if ((_currentTargets == null) || (_currentTargets.Length != command.RenderTargetCount))
				{
					_currentTargets = new D3D.RenderTargetView[command.RenderTargetCount];
				}

				bool hasChanged = false;
				for (int i = 0; i < _currentTargets.Length; ++i)
				{
					if (command.RenderTargets[i] == _currentTargets[i])
					{
						continue;
					}

					_currentTargets[i] = command.RenderTargets[i];
					hasChanged = true;
				}

				if (hasChanged)
				{
					D3DDevice.ImmediateContext.OutputMerger.SetRenderTargets(null, _currentTargets);
				}
			}

			if (command.ClearTargetState != null)
			{
				D3DDevice.ImmediateContext.ClearRenderTargetView(command.ClearTargetState.Value.TargetView.D3DView, command.ClearTargetState.Value.Color.ToRawColor4());
			}
		}

        /// <summary>
        /// Function to create a deferred graphics context.
        /// </summary>
        /// <returns>A new graphics object as a deferred graphics context.</returns>
        /// <remarks>A deferred graphics context will allow for improved performance when used in a multi-threaded environment.  The deferred context takes rendering commands and queues them into a buffer 
        /// for execution later from the immediate context.  Use the <see cref="ExecuteDeferred"/> method to execute these commands from the immediate graphics object.
        /// <para>To use a deferred context the use needs to create a context with this method, then perform the rendering operations required.  Once the rendering operations are complete, then a call to 
        /// <see cref="FinalizeDeferred"/> is called and will return an object that will contain the command list.  Then a call to ExecuteDeferred using the command list object from the immediate context will 
        /// execute the commands.</para>
        /// <para>This method must be called from the immediate context.</para>
        /// </remarks>
        /// <exception cref="GorgonException">Thrown when the deferred context could not be created.</exception>
        public GorgonGraphics CreateDeferredGraphics()
        {
            if (IsDeferred)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_CANNOT_CREATE_CONTEXT_FROM_CONTEXT);
            }

            return new GorgonGraphics(this);
        }

        /// <summary>
        /// Function to finalize the deferred rendering context.
        /// </summary>
        /// <returns>An object containing the rendering commands to issue.</returns>
        /// <exception cref="System.NotSupportedException">Thrown when the current context is an immediate context.
        /// <para>-or-</para>
        /// <para>Thrown if the current video device does not have a feature level of SM5 or better.</para>
        /// </exception>
        /// <remarks>
        /// Use this method to finish recording of the rendering commands sent to a deferred context.  This method must be called from a deferred context.
        /// </remarks>
        public GorgonRenderCommands FinalizeDeferred()
        {
#if DEBUG
            if (!IsDeferred)
            {
                throw new NotSupportedException(Resources.GORGFX_CANNOT_USE_IMMEDIATE_CONTEXT);
            }
#endif

            var result = new GorgonRenderCommands(this);

            if (_commands == null)
            {
                _commands = new List<GorgonRenderCommands>();
            }

            _commands.Add(result);

            return result;
        }

        /// <summary>
        /// Function to execute rendering commands from a deferred context.
        /// </summary>
        /// <param name="commands">Commands to execute.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="commands"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.NotSupportedException">Thrown when the current context is a deferred context.
        /// <para>-or-</para>
        /// <para>Thrown if the current video device does not have a feature level of SM5 or better.</para>
        /// </exception>
        /// <remarks>
        /// Use this method to execute previously recorded rendering commands on the immediate context.  This method must be called from the immediate context.
        /// </remarks>
        public void ExecuteDeferred(GorgonRenderCommands commands)
        {
            commands.ValidateObject("commands");

#if DEBUG
            if (IsDeferred)
            {
                throw new NotSupportedException(Resources.GORGFX_CANNOT_USE_DEFERRED_CONTEXT);
            }
#endif

            Context.ExecuteCommandList(commands.D3DCommands, true);
        }

        /// <summary>
        /// Function to clear the states for the graphics object.
        /// </summary>
        /// <param name="flush">[Optional] <b>true</b> to flush the queued graphics object commands, <b>false</b> to leave as is.</param>
        /// <remarks>If <paramref name="flush"/> is set to <b>true</b>, then a performance penalty is incurred.</remarks>
        public void ClearState(bool flush = false)
        {
            if (flush)
            {
                Context.Flush();
            }

            if (IsDeferred)
            {
                Context.ClearState();
            }

            // Set default states.
            Input.Reset();
            Rasterizer.Reset();
            Output.Reset();
            Shaders.Reset();
        }
#endregion

#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="videoDeviceInfo">[Optional] A <see cref="GorgonVideoDeviceInfo"/> to specify the video device to use for this instance.</param>
		/// <param name="featureLevel">[Optional] The requested feature level for the video device used with this object.</param>
		/// <param name="log">[Optional] The log to use for debugging.</param>
		/// <exception cref="GorgonException">Thrown when the <paramref name="featureLevel"/> is set to <see cref="DeviceFeatureLevel.Unsupported"/>.</exception>
		/// <remarks>
		/// <para>
		/// When the <paramref name="videoDeviceInfo"/> is set to <b>null</b> (<i>Nothing</i> in VB.Net), Gorgon will use the first video device with feature level specified by <paramref name="featureLevel"/>  
		/// will be used. If the feature level requested is higher than what any device in the system can support, then the first device with the highest feature level will be used.
		/// </para>
		/// <para>
		/// When specifying a feature level, the device with the closest matching feature level will be used. If the <paramref name="videoDeviceInfo"/> is specified, then that device will be used at the 
		/// requested <paramref name="featureLevel"/>. If the requested <paramref name="featureLevel"/> is higher than what the <paramref name="videoDeviceInfo"/> will support, then Gorgon will use the 
		/// highest feature of the specified <paramref name="videoDeviceInfo"/>. 
		/// </para>
		/// <para>
		/// If Gorgon is compiled in DEBUG mode, and <see cref="GorgonVideoDeviceInfo"/> is <b>null</b>, then it will attempt to find the most appropriate hardware video device, and failing that, will fall 
		/// back to a software device (WARP).
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// The Gorgon Graphics library only works on Windows 7 or better. No other operating system is supported at this time.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		/// <example>
		/// <para>
		/// The following examples show the various ways the object can be configured:
		/// </para>
		/// <code lang="csharp">
		/// <![CDATA[
		/// // Create using the first video device with the highest feature level:
		/// var graphics = new GorgonGraphics();
		/// 
		/// // Create using a specific video device and use the highest feature level supported by that device:
		/// // Get a list of available video devices.
		/// IGorgonVideoDeviceList videoDevices = new GorgonVideoDeviceList(log);
		/// videoDevices.Enumerate(true);
		/// var graphics = new GorgonGraphics(videoDevices[0]);
		/// 
		/// // Create using the requested feature level and the first adapter that supports the nearest feature level requested:
		/// // If the device does not support 11.0, then the device with the nearest feature level (e.g. 10.1) will be used instead.
		/// var graphics = new GorgonGraphics(null, DeviceFeatureLevel.FeatureLevel11_0);
		/// 
		/// // Create using the requested device and the requested feature level:
		/// // If the device does not support 11.0, then the highest feature level supported by the device will be used (e.g. 10.1).
		/// IGorgonVideoDeviceList videoDevices = new GorgonVideoDeviceList(log);
		/// videoDevices.Enumerate(true);
		/// var graphics = new GorgonGraphics(videoDevices[0], DeviceFeatureLevel.FeatureLevel11_0); 
		/// ]]>
		/// </code>
		/// </example>
		/// <seealso cref="DeviceFeatureLevel"/>
		/// <seealso cref="GorgonVideoDeviceInfo"/>
		public GorgonGraphics(GorgonVideoDeviceInfo videoDeviceInfo = null, DeviceFeatureLevel? featureLevel = null, IGorgonLog log = null)
		{
			if ((GorgonComputerInfo.OperatingSystemVersion.Major < 6) && (GorgonComputerInfo.OperatingSystemVersion.Minor < 1))
			{
				throw new GorgonException(GorgonResult.CannotCreate, Resources.GORGFX_ERR_INVALID_OS);
			}

			if ((featureLevel != null) && ((featureLevel == DeviceFeatureLevel.Unsupported) || (!Enum.IsDefined(typeof(DeviceFeatureLevel), featureLevel.Value))))
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_INVALID, featureLevel));
			}

			_log = log ?? GorgonLogDummy.DefaultInstance;
			ResetFullscreenOnFocus = true;
            ImmediateContext = this;
			
			_log.Print("Gorgon Graphics initializing...", LoggingLevel.Simple);

            // Track our objects.
            _trackedObjects = new GorgonDisposableObjectCollection();

			CreateDevice(videoDeviceInfo, featureLevel);
            CreateStates();

			GorgonApplication.Log.Print("Gorgon Graphics initialized.", LoggingLevel.Simple);
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="featureLevel">The maximum feature level to support for the devices enumerated.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="featureLevel"/> parameter is invalid.</exception>
		/// <exception cref="GorgonException">Thrown when Gorgon could not find any video devices that are Shader Model 5, or the down level interfaces (Shader Model 4, and lesser).
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
		static GorgonGraphics()
		{
			Win32API.DwmIsCompositionEnabled(out _isDWMEnabled);

			if (!_isDWMEnabled)
			{
				_dontEnableDWM = true;
			}

			SharpDX.Configuration.ThrowOnShaderCompileError = false;

#if DEBUG
			IsDebugEnabled = true;
#endif
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonGraphics"/> class.
        /// </summary>
        /// <param name="graphics">The immediate graphics context.</param>
        internal GorgonGraphics(GorgonGraphics graphics)
        {
			throw new NotSupportedException();
        }
#endregion

#region IDisposable Members
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
			// TODO: This is not thread-safe
			_trackedObjects.Clear();
			DestroyInterfaces();

			VideoDevice device = Interlocked.Exchange(ref _videoDevice, null);
			device?.Dispose();

			// TODO: Find a better way to do this.
			/*
			if (!IsDeferred)
			{
				
			}
			else
			{
				
			}

			// Only clean up the context if the context is deferred.
			if (IsDeferred)
			{				
				return;
			}
			*/
		}
#endregion
	}
}
