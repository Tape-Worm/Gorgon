using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using D3D12 = SharpDX.Direct3D12;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.Graphics.RenderTargets;
using Gorgon.Math;

namespace Gorgon.Graphics
{
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
	/// items are accessible from the <see cref="Gorgon.Graphics.GorgonVideoDeviceEnumerator">GorgonVideoDeviceEnumerator</see> class.</para>
	/// <para>These objects can also be used in a deferred context.  This means that when a graphics object is deferred, it can be used in a multi threaded environment to allow set up of 
	/// a scene by recording commands sent to the video device for execution later on the rendering process.  This is handy where multiple passes for the same scene are required (e.g. a deferred renderer).</para>
	/// <para>Please note that this object requires Direct3D 11 (but not necessarily a Direct3D 11 video card) and at least Windows Vista Service Pack 2 or higher.  
	/// Windows XP and operating systems before it will not work, and an exception will be thrown if this object is created on those platforms.</para>
	/// <para>Deferred graphics contexts require a video device with a feature level of SM5 or better.</para>
	/// </remarks>
	public class GorgonGraphics
		: IDisposable
    {
		#region Variables.
		// The Direct 3D debugging interface.
		private static D3D12.DebugInterface _debug;
		// The log to use for tracking debug messages.
		private readonly IGorgonLog _log;
		// The default video device to use when no video device is specified.
		private VideoDevice _videoDevice;
		// The command management interface for this graphics object.
		private GraphicsCommander _graphicsCommander;
		// The render target view allocator.
		private RtvAllocator _rtvAllocator;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the command manager for this object.
		/// </summary>
		internal GraphicsCommander GraphicsCommander => _graphicsCommander;

		/// <summary>
		/// Property to return the Direct 3D 12 device instance used by this object.
		/// </summary>
		internal D3D12.Device D3DDevice => _videoDevice.D3DDevice;

		/// <summary>
		/// Property to return the DXGI adapter used by this object.
		/// </summary>
		internal DXGI.Adapter3 DXGIAdapter => _videoDevice.DXGIAdapter;

		/// <summary>
		/// Property to return the render target view allocator.
		/// </summary>
		internal RtvAllocator RenderTargetViewAllocator => _rtvAllocator;

		/// <summary>
		/// Property to return the <see cref="IGorgonVideoDevice"/> used by this instance.
		/// </summary>
		/// <remarks>
		/// Users may use this to determine the level of support for specific features (e.g. Multi-sampling).
		/// </remarks>
		public IGorgonVideoDevice VideoDevice => _videoDevice;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create or return the video device.
		/// </summary>
		/// <param name="userVideoDevice">The video device supplied by the user in the constructor of this object.</param>
		/// <param name="featureLevel">A feature level requested by the user. If this value is null, then the highest feature level is used.</param>
		/// <returns>The default video device with the highest feature level.</returns>
		private VideoDevice GetVideoDevice(GorgonVideoDeviceInfo userVideoDevice, DeviceFeatureLevel? featureLevel)
		{
			if (userVideoDevice == null)
			{
				var videoDevices = new GorgonVideoDeviceList(_log);
				videoDevices.Enumerate();

				_log.Print("No video device specified.", LoggingLevel.Verbose);

				userVideoDevice = (from deviceInfo in videoDevices
				                   where deviceInfo.VideoDeviceType == VideoDeviceType.Hardware
				                         && ((featureLevel == null)
				                             || (deviceInfo.SupportedFeatureLevel >= featureLevel.Value))
				                   orderby deviceInfo.SupportedFeatureLevel descending, deviceInfo.Index ascending
				                   select deviceInfo).FirstOrDefault();
				
				if (userVideoDevice == null)
				{
					throw new GorgonException(GorgonResult.DriverError, Resources.GORGFX_ERR_NO_SUITABLE_VIDEO_DEVICE_FOUND);
				}
			}

			// If we've not specified a feature level, or the feature level exceeds the requested device feature level, then 
			// fall back to the device feature level.
			if ((featureLevel == null) || (userVideoDevice.SupportedFeatureLevel < featureLevel.Value))
			{
				featureLevel = userVideoDevice.SupportedFeatureLevel;
			}

			_log.Print($"Using video device '{userVideoDevice.Name}' at feature level [{featureLevel.Value}] for Direct 3D 12.", LoggingLevel.Simple);

			return new VideoDevice(userVideoDevice, featureLevel.Value, _log);
		}

		/// <summary>
		/// Function to find a display mode supported by the Gorgon.
		/// </summary>
		/// <param name="output">The output to use when looking for a video mode.</param>
		/// <param name="videoMode">The <see cref="GorgonVideoMode"/> used to find the closest match.</param>
		/// <param name="newMode">A <see cref="GorgonVideoMode"/> that is the nearest match for the provided video mode.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="output"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// Users may leave the <see cref="GorgonVideoMode"/> values at unspecified (either 0, or default enumeration values) to indicate that these values should not be used in the search.
		/// </para>
		/// <para>
		/// The following members in <see cref="GorgonVideoMode"/> may be skipped (if not listed, then this member must be specified):
		/// <list type="bullet">
		///		<item>
		///			<description><see cref="GorgonVideoMode.Width"/> and <see cref="GorgonVideoMode.Height"/>.  Both values must be set to 0 if not filtering by width or height.</description>
		///		</item>
		///		<item>
		///			<description><see cref="GorgonVideoMode.RefreshRate"/> should be set to <see cref="GorgonRationalNumber.Empty"/> in order to skip filtering by refresh rate.</description>
		///		</item>
		///		<item>
		///			<description><see cref="GorgonVideoMode.Scaling"/> should be set to <see cref="VideoModeDisplayModeScaling.Unspecified"/> in order to skip filtering by the scaling mode.</description>
		///		</item>
		///		<item>
		///			<description><see cref="GorgonVideoMode.ScanlineOrdering"/> should be set to <see cref="VideoModeScanlineOrder.Unspecified"/> in order to skip filtering by the scanline order.</description>
		///		</item>
		/// </list>
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// The <see cref="GorgonVideoMode.Format"/> member must be one of the UNorm format types and cannot be set to <see cref="BufferFormat.Unknown"/>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public void FindClosestMode(GorgonVideoOutputInfo output, ref GorgonVideoMode videoMode, out GorgonVideoMode newMode)
		{
			if (output == null)
			{
				throw new ArgumentNullException(nameof(output));
			}

			using (DXGI.Output output1 = _videoDevice.DXGIAdapter.GetOutput(output.Index))
			{
				using (DXGI.Output4 output4 = output1.QueryInterface<DXGI.Output4>())
				{
					DXGI.ModeDescription1 newModeDesc;
					DXGI.ModeDescription1 oldModeDesc = videoMode.ToModeDesc();

					output4.FindClosestMatchingMode1(ref oldModeDesc, out newModeDesc, _videoDevice.D3DDevice);

					newMode = new GorgonVideoMode(newModeDesc);
				}
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			// We do this here in case someone gets funny about trying to dispose this object across multiple threads.
			// That said, DON'T DO THAT!
			RtvAllocator rtvAllocator = Interlocked.Exchange(ref _rtvAllocator, null);
			GraphicsCommander graphicsCommander = Interlocked.Exchange(ref _graphicsCommander, null);
			VideoDevice device = Interlocked.Exchange(ref _videoDevice, null);
			
			// Ensure that we're finished rendering before shut down.
			graphicsCommander?.WaitForGPUIdle();
			
			graphicsCommander?.Dispose();
			rtvAllocator.Dispose();
			device?.Dispose();
		}

		/// <summary>
		/// Property to set or return whether to enable debugging information for analyzing issues with rendering.
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <summary>
		/// Function to enable debugging diagnostic information for the application.
		/// </summary>
		/// <param name="enableComObjectTracking">[Optional] <b>true</b> to track all COM objects created and notify when they are not released, or <b>false</b> to disable.</param>
		/// <remarks>
		/// <para>
		/// Gorgon uses Direct 3D for rendering, and Direct 3D has a facility through its SDK layers dll to show debug messages in the output window during execution of the application. This method will 
		/// turn those messages on (provided the application is registered for debugging through the graphics control panel). To turn them off, call <see cref="DisableDebugging"/>. 
		/// </para>
		/// <para>
		/// Because this method turns on debugging information, application performance will be impaired due to aggressive checking of parameters and other debug functionality. Ensure that this method 
		/// is not called in release builds by wrapping it in a conditional define.
		/// </para>
		/// <para>
		/// When enabling COM object tracking by setting the <paramref name="enableComObjectTracking"/> value to <b>true</b>, the application performance will be less than optimal. Only enable this when 
		/// absolutely required to track down potential resource leaks. Please note that some SharpDX exceptions may show up in the output window when this is set to <b>true</b>, these exceptions may be 
		/// safely ignored.
		/// </para>
		/// <para>
		/// <note type="note">
		/// <para>
		/// Applications should pair this call with a call to <see cref="DisableDebugging"/>.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		/// <seealso cref="DisableDebugging"/>
		public static void EnableDebugging(bool enableComObjectTracking = false)
		{
			_debug?.Dispose();

			DX.Configuration.EnableObjectTracking = enableComObjectTracking;
			_debug = D3D12.DebugInterface.Get();
			_debug.EnableDebugLayer();
		}

		/// <summary>
		/// Function to disable debugging diagnostic information for the application.
		/// </summary>
		/// <remarks>
		/// This method will turn off the output of debug information that was enabled by calling <see cref="EnableDebugging"/>. 
		/// </remarks>
		/// <seealso cref="EnableDebugging"/>
		public static void DisableDebugging()
		{
			_debug?.Dispose();
			DX.Configuration.EnableObjectTracking = false;
		}
		#endregion

		#region Constructor/Finalizer.
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
		/// // If the device does not support 12.0, then the device with the nearest feature level (e.g. 11.1) will be used instead.
		/// var graphics = new GorgonGraphics(null, DeviceFeatureLevel.FeatureLevel12_0);
		/// 
		/// // Create using the requested device and the requested feature level:
		/// // If the device does not support 12.0, then the highest feature level supported by the device will be used (e.g. 11.1).
		/// IGorgonVideoDeviceList videoDevices = new GorgonVideoDeviceList(log);
		/// videoDevices.Enumerate(true);
		/// var graphics = new GorgonGraphics(videoDevices[0], DeviceFeatureLevel.FeatureLevel12_0); 
		/// ]]>
		/// </code>
		/// </example>
		/// <seealso cref="DeviceFeatureLevel"/>
		/// <seealso cref="GorgonVideoDeviceInfo"/>
		public GorgonGraphics(GorgonVideoDeviceInfo videoDeviceInfo = null, DeviceFeatureLevel? featureLevel = null, IGorgonLog log = null)
		{
			if ((featureLevel != null) && (featureLevel == DeviceFeatureLevel.Unsupported))
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_FEATURE_LEVEL_INVALID, featureLevel));	
			}

			_log = log ?? GorgonLogDummy.DefaultInstance;
			_videoDevice = GetVideoDevice(videoDeviceInfo, featureLevel);
			_rtvAllocator = new RtvAllocator(this);
			_graphicsCommander = new GraphicsCommander(this);
		}

		/// <summary>
		/// Initializes static members of the <see cref="GorgonGraphics"/> class.
		/// </summary>
		static GorgonGraphics()
		{
			// Turn this off, we want the user to handle errors in shader compilation gracefully.
			DX.Configuration.ThrowOnShaderCompileError = false;
		}
		#endregion
	}
}
