using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;
using D3D12 = SharpDX.Direct3D12;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;

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
	/// <para>Along with the feature level, the graphics object can also take a <see cref="Gorgon.Graphics.GorgonVideoDevice">Video Device</see> object as a parameter.  Specifying a 
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

/*		// The log to use for tracking debug messages.
		private readonly IGorgonLog _log;
		// The video device to use for this graphics instance.
		private GorgonVideoDevice _videoDevice;
		// The default video device to use when no video device is specified.
		private Lazy<GorgonVideoDevice> _defaultVideoDevice;*/
		#endregion

		#region Properties.
		/*		/// <summary>
				/// Property to set or return whether to enable debugging information for analyzing issues with rendering.
				/// </summary>
				/// <remarks>
				/// <para>
				/// Gorgon uses Direct 3D for rendering, and Direct 3D has a facility through its SDK layers dll to show debug messages in the output window during execution of the application. Setting this value to 
				/// <b>true</b> will turn on these messages (provided the application is registered for debugging through the graphics control panel).
				/// </para>
				/// <para>
				/// When running a debug build of Gorgon, this value is automatically set to <b>true</b>.
				/// </para>
				/// <para>
				/// There may be some performance impact when this value is set to <b>true</b>.
				/// </para>
				/// </remarks>
				public static bool EnableDebug
				{
					get;
					set;
				}

				/// <summary>
				/// Property to set or return whether to enable tracking of the objects created by the Gorgon Graphics API.
				/// </summary>
				/// <remarks>
				/// <para>
				/// Direct3D is a COM based API, and since Gorgon uses Direct 3D underneath its objects, there is a chance that an object may not have been cleaned up due to dangling references. Setting this value to 
				/// <b>true</b> will show whether there are leaked Direct 3D resources in the debug output window.
				/// </para>
				/// <para>
				/// Setting this value to <b>true</b> may impact performance.
				/// </para>
				/// </remarks>
				public static bool EnableObjectTracking
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
				/// Property to return the <see cref="IGorgonVideoDevice"/> used by this instance.
				/// </summary>
				public IGorgonVideoDevice VideoDevice => _videoDevice ?? (_videoDevice = _defaultVideoDevice.Value);*/
		#endregion

		#region Methods.
		/*		/// <summary>
				/// Function to create the default video device.
				/// </summary>
				/// <returns>The default video device with the highest feature level.</returns>
				private GorgonVideoDevice CreateDefaultVideoDevice()
				{
					var videoDevices = new GorgonVideoDeviceList(_log);

					videoDevices.Enumerate();

					IGorgonVideoDeviceInfo info = (from deviceInfo in videoDevices
												   where deviceInfo.VideoDeviceType == VideoDeviceType.Hardware
												   orderby deviceInfo.SupportedFeatureLevel descending, deviceInfo.Index ascending
												   select deviceInfo).FirstOrDefault();

					if (info == null)
					{
						throw new GorgonException(GorgonResult.DriverError, Resources.GORGFX_ERR_NO_SUITABLE_VIDEO_DEVICE_FOUND);
					}

					return new GorgonVideoDevice(info, info.SupportedFeatureLevel);
				}*/

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
/*			if ((_videoDevice == null) || (_defaultVideoDevice == null) || (!_defaultVideoDevice.IsValueCreated))
			{
				return;
			}

			_defaultVideoDevice.Value.Dispose();
			_defaultVideoDevice = null;
			_videoDevice = null;*/
		}

		/// <summary>
		/// Function to enable debugging diagnostic information for the application.
		/// </summary>
		/// <param name="enableComObjectTracking">[Optional] <b>true</b> to track all COM objects created and notify when they are not released, or <b>false</b> to disable.</param>
		/// <remarks>
		/// <para>
		/// Applications should pair this call with a call to <see cref="DisableDebugging"/>. Otherwise, a potential memory leak may occur.
		/// </para>
		/// <para>
		/// When enabling COM object tracking, the application performance will be less than optimal. Only enable this when absolutely required.
		/// </para>
		/// </remarks>
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
		public static void DisableDebugging()
		{
			_debug?.Dispose();
			DX.Configuration.EnableObjectTracking = false;
		}
		#endregion

		#region Constructor/Finalizer.
		/*		/// <summary>
				/// Initializes a new instance of the <see cref="GorgonGraphics"/> class.
				/// </summary>
				/// <param name="videoDevice">[Optional] A <see cref="IGorgonVideoDevice"/> to use for this graphic instance.</param>
				/// <param name="log">[Optional] The log to use for debugging.</param>
				/// <remarks>
				/// When the <paramref name="videoDevice"/> is set to <b>null</b> (<i>Nothing</i> in VB.Net), Gorgon will use the first video device with the highest capable feature level.
				/// </remarks>
				public GorgonGraphics(IGorgonVideoDevice videoDevice = null, IGorgonLog log = null)
				{
					_log = log ?? GorgonLogDummy.DefaultInstance;
					// We require the concrete version of this interface in order to retrieve internal information.
					// If this value is not a GorgonVideoDevice instance, then we will fall back to using the default 
					// device.
					_videoDevice = videoDevice as GorgonVideoDevice;
					_defaultVideoDevice = new Lazy<GorgonVideoDevice>(CreateDefaultVideoDevice);
				}

				/// <summary>
				/// Initializes static members of the <see cref="GorgonGraphics" /> class.
				/// </summary>
				static GorgonGraphics()
				{
					// Turn this off, we want the user to handle errors in shader compilation gracefully.
					SharpDX.Configuration.ThrowOnShaderCompileError = false;

		#if DEBUG
					EnableDebug = true;
		#endif
				}*/
		#endregion
	}
}
