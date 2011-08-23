using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The primary render target for a window.
	/// </summary>
	/// <remarks>
	/// These objects are used to as the primary render target for a window.
	/// </remarks>
	public abstract class GorgonDeviceWindow
		: GorgonSwapChainBase, IObjectTracker
	{
		#region Classes.
		/// <summary>
		/// Records the form state.
		/// </summary>
		protected class FormStateRecord
		{
			#region Variables.
			private Point _location;					// Window location.
			private Size _size;							// Window size.
			private FormBorderStyle _border;			// Window border.
			private bool _topMost;						// Topmost flag.
			private bool _sysMenu;						// System menu flag.
			private bool _maximizeButton;				// Maxmimize button flag.
			private bool _minimizeButton;				// Minimize button flag.
			private bool _visible;						// Visible flag.
			private bool _enabled;						// Enabled flag.
			private Form _window;						// Window.
			#endregion

			#region Methods.
			/// <summary>
			/// Function to restore the original window state.
			/// </summary>
			/// <param name="keepSize">TRUE to keep the size of the window, FALSE to restore it.</param>
			/// <param name="dontMove">TRUE to keep the window from moving, FALSE to restore the last location.</param>
			public void Restore(bool keepSize, bool dontMove)
			{
				if (_window == null)
					return;

				if (!dontMove)
					_window.Location = _location;
				if (!keepSize)
					_window.Size = _size;
				_window.FormBorderStyle = _border;
				_window.TopMost = _topMost;
				_window.ControlBox = _sysMenu;
				_window.MaximizeBox = _maximizeButton;
				_window.MinimizeBox = _minimizeButton;
				_window.Enabled = _enabled;
				_window.Visible = _visible;
			}

			/// <summary>
			/// Function to update the form state.
			/// </summary>
			public void Update()
			{
				_location = _window.Location;
				_size = _window.Size;
				_border = _window.FormBorderStyle;
				_topMost = _window.TopMost;
				_sysMenu = _window.ControlBox;
				_maximizeButton = _window.MaximizeBox;
				_minimizeButton = _window.MinimizeBox;
				_enabled = _window.Enabled;
				_visible = _window.Visible;
			}
			#endregion

			#region Constructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="FormStateRecord"/> struct.
			/// </summary>
			/// <param name="window">The window.</param>
			public FormStateRecord(Form window)
			{
				_location = window.Location;
				_size = window.Size;
				_border = window.FormBorderStyle;
				_topMost = window.TopMost;
				_sysMenu = window.ControlBox;
				_maximizeButton = window.MaximizeBox;
				_minimizeButton = window.MinimizeBox;
				_enabled = window.Enabled;
				_visible = window.Visible;
				_window = window;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private bool _disposed = false;								// Flag to indicate that the object was already disposed.
		private FormStateRecord _originalWindowState = null;		// Original window state.
		private bool _wasMaximized = false;							// Flag to indicate that the window was maximized.
		private IList<IDisposable> _trackedObjects = null;			// List of tracked objects.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the device window settings.
		/// </summary>
		protected GorgonDeviceWindowSettings Settings
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the object holding the current window state.
		/// </summary>
		protected FormStateRecord WindowState
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the video device that this device window is bound with.
		/// </summary>
		public GorgonVideoDevice VideoDevice
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return which output on the device is being used with this device window.
		/// </summary>
		public GorgonVideoOutput VideoOutput
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether this device window is full screen or windowed.
		/// </summary>
		public bool IsWindowed
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the video mode for this device window.
		/// </summary>
		public GorgonVideoMode Mode
		{
			get;
			private set;
		}
		#endregion
		
		#region Methods.
		/// <summary>
		/// Function to clear out any outstanding resources when the object is disposed.
		/// </summary>
		/// <remarks>Implementors must call this function to clean up outstanding resources in the Dispose method.</remarks>
		protected override void CleanUpResources()
		{
			base.CleanUpResources();

			CleanUpTest();
			((IObjectTracker)this).CleanUpTrackedObjects();

			// Remove us from anything tracking us.
			((IObjectTracker)Graphics).RemoveTrackedObject(this);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (BoundWindow is Form)
						_originalWindowState.Restore(false, false);

					Gorgon.Log.Print("Device window '{0}' destroyed.", Diagnostics.GorgonLoggingLevel.Simple, Name);
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function called when the window is resized.
		/// </summary>
		/// <param name="newWidth">New width of the window.</param>
		/// <param name="newHeight">New height of the window.</param>
		protected override void OnWindowResized(int newWidth, int newHeight)
		{
			// We should not care about window resizing when in full screen mode.
			if (IsWindowed)
			{
				if ((BoundForm.WindowState != FormWindowState.Minimized) && (BoundWindow.ClientSize.Width > 0) && (BoundWindow.ClientSize.Height > 0))
				{
					UpdateTargetInformation(new GorgonVideoMode(newWidth, newHeight, Format, Mode.RefreshRateNumerator, Mode.RefreshRateDenominator), DepthStencilFormat, MultiSampleAALevel);
					base.OnWindowResized(newWidth, newHeight);
				}
			}
		}

		/// <summary>
		/// Function to update the information about the swap chain.
		/// </summary>
		/// <param name="mode">Video mode to use when updating.</param>
		/// <param name="depthStencilFormat">Format of the depth/stencil buffer.</param>
		/// <param name="msaaLevel">Multi-sampling anti-aliasing quality level.</param>
		protected void UpdateTargetInformation(GorgonVideoMode mode, GorgonBufferFormat depthStencilFormat, GorgonMSAAQualityLevel? msaaLevel)
		{
			UpdateTargetInformation(mode.Width, mode.Height, mode.Format, depthStencilFormat, msaaLevel);
			Mode = mode;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="window"></param>
		/// <param name="format"></param>
		/// <param name="depthStencilFormat"></param>
		/// <returns></returns>
		public GorgonSwapChain CreateSwapChain(string name, Control window, GorgonBufferFormat format, GorgonBufferFormat depthStencilFormat)
		{
			return null;
		}

		/// <summary>
		/// Function to update the device window.
		/// </summary>
		/// <param name="windowed">TRUE to go into windowed mode, FALSE to go into full screen mode.</param>
		/// <remarks>Use this overload to change the dimensions, format, fullscreen/windowed state and depth information for the device window.
		/// Device windows bound to child controls or device windows with extra <see cref="GorgonLibrary.Graphics.GorgonSwapChainBase">swap chains</see> attached to them cannot go full screen, setting the <see cref="P:GorgonDeviceWindowSettings.Windowed"/> setting to TRUE will throw an exception.
		/// </remarks>
		/// <exception cref="System.ArgumentException">Thrown when the window is a child control, or when there are extra swap chains belonging to this device window and setting the GorgonDeviceWindowSettings.Windowed setting is TRUE.
		/// </exception>
		public void Update(bool windowed)
		{
			if (IsWindowed == windowed)
				return;

			Settings.Windowed = windowed;
			Update(Settings);
		}

		/// <summary>
		/// Function to update the device window.
		/// </summary>
		/// <param name="settings">Settings for the device window.</param>
		/// <remarks>Use this function to change the dimensions, format, fullscreen/windowed state and depth information for the device window.
		/// <para>The <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateNominator">RefreshRateNominator</see> and the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateDenominator">RefreshRateDenominator</see> 
		/// of the <see cref="GorgonLibrary.Graphics.GorgonVideoMode">GorgonVideoMode</see> type are not relevant when fullScreen is set to FALSE.</para>
		/// <para>Device windows bound to child controls or device windows with extra <see cref="GorgonLibrary.Graphics.GorgonSwapChain">swap chains</see> attached to them cannot go full screen, setting the <see cref="P:GorgonDeviceWindowSettings.Windowed"/> setting to TRUE will throw an exception.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentException">Thrown when the window is a child control, or when there are extra swap chains belonging to this device window and setting the GorgonDeviceWindowSettings.Windowed setting is TRUE.
		/// </exception>
		public void Update(GorgonDeviceWindowSettings settings)
		{
			Form window = BoundWindow as Form;
			
			// Child controls and device windows with swap chains cannot go full screen.
			if (!settings.Windowed)
			{
				if (window == null)
					throw new ArgumentException("Cannot switch to full screen with a child control.", "fullScreen");
				if (_trackedObjects.Count(item => item is GorgonSwapChain) > 0)
					throw new ArgumentException("This device window has extra swap chains, cannot switch to full screen.", "fullScreen");
			}

			RemoveEventHandlers();
						
			// Ideally, we want the window to use the desktop resolution when we switch to full screen and the window is maximized.
			// When we switch back, we should keep the window maximized.  This wouldn't be a big deal except there's some sort of
			// weird bug in windows forms/d3d9 where upon switching to full screen mode with a window maximized, the mouse cursor
			// tends to just pass through the window to the underlying window (if any), causing all kinds of weirdness.  So
			// our solution is to restore the window to a normal state, and apply the changes.

			// If we're switching back to windowed mode and the window was previously maximized, then re-maximize it.
			if ((settings.Windowed) && (!IsWindowed) && (_wasMaximized))
				window.WindowState = FormWindowState.Maximized;

			// Store whether the window was previously maximized.
			_wasMaximized = ((settings.Windowed == !IsWindowed) && (window.WindowState == FormWindowState.Maximized));

			// If we didn't pass in the display settings, then use the current settings.
			if (settings.DisplayMode == null)
				settings.DisplayMode = new GorgonVideoMode(Mode.Width, Mode.Height, Mode.Format, Mode.RefreshRateNumerator, Mode.RefreshRateDenominator);
			
			UpdateTargetInformation(settings.DisplayMode.Value, settings.DepthStencilFormat, settings.AdvancedSettings.MSAAQualityLevel);
			IsWindowed = settings.Windowed;
			UpdateResources();
			AddEventHandlers();

			Gorgon.Log.Print("Updating device window '{0}' with settings: {1}x{2} Format: {3} Refresh Rate: {4}/{5}.", Diagnostics.GorgonLoggingLevel.Verbose, Name, settings.DisplayMode.Value.Width, settings.DisplayMode.Value.Height, settings.DisplayMode.Value.Format, settings.DisplayMode.Value.RefreshRateNumerator, settings.DisplayMode.Value.RefreshRateDenominator);
			Gorgon.Log.Print("'{0}' information:", Diagnostics.GorgonLoggingLevel.Verbose, Name);
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

			Gorgon.Log.Print("Device window '{0}' updated.", Diagnostics.GorgonLoggingLevel.Simple, Name);
		}

		#region Remove this shit.
		/// <summary>
		/// 
		/// </summary>
		public abstract void SetupTest();

		/// <summary>
		/// 
		/// </summary>
		public abstract void RunTest(float dt);

		/// <summary>
		/// 
		/// </summary>
		public abstract void CleanUpTest();

		#endregion
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindow"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="graphics">The graphics instance that owns this render target.</param>
		/// <param name="device">Video device to use.</param>
		/// <param name="output">Video output on the device to use.</param>
		/// <param name="settings">Device window settings.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="device"/> and <paramref name="output"/> parameters are NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		/// <remarks>A fullscreen video mode must have a a Windows Form object as the bound window.
		/// <para>The <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateNominator">RefreshRateNominator</see> and the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateDenominator">RefreshRateDenominator</see> 
		/// of the <see cref="GorgonLibrary.Graphics.GorgonVideoMode">GorgonVideoMode</see> type are not relevant when fullScreen is set to FALSE.</para>
		/// </remarks>
		protected GorgonDeviceWindow(GorgonGraphics graphics, string name, GorgonVideoDevice device, GorgonVideoOutput output, GorgonDeviceWindowSettings settings)
			: base(graphics, name, settings.BoundWindow, settings.DisplayMode.Value.Width, settings.DisplayMode.Value.Height, settings.DisplayMode.Value.Format, settings.DepthStencilFormat, settings.AdvancedSettings.MSAAQualityLevel)
		{
			Form window = settings.BoundWindow as Form;

			if (device == null)
				throw new ArgumentNullException("device");
			if (output == null)
				throw new ArgumentNullException("output");
			
			_trackedObjects = new List<IDisposable>();

			Settings = settings;
			Mode = settings.DisplayMode.Value;
			VideoDevice = device;
			VideoOutput = output;
			IsWindowed = settings.Windowed;

			if (window != null)
			{
				_originalWindowState = new FormStateRecord(window);
				WindowState = new FormStateRecord(window);
			}
		}		
		#endregion

		#region IObjectTracker Members
		#region Properties.
		/// <summary>
		/// Property to return an enumerable list of tracked objects.
		/// </summary>
		/// <value></value>
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

			if (trackedObjects.Count() > 0)
			{
				Gorgon.Log.Print("Destroying child {0} objects:", Diagnostics.GorgonLoggingLevel.Verbose, trackedObjects.Count());

				foreach (var trackedObject in trackedObjects)
					trackedObject.Dispose();
			}

			_trackedObjects.Clear();
		}
		#endregion
		#endregion
	}
}
