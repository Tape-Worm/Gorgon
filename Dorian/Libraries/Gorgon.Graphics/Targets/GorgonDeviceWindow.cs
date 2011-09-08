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
		: GorgonWindowTarget<GorgonDeviceWindowSettings>, IObjectTracker
	{
		#region Variables.
		private bool _disposed = false;										// Flag to indicate that the object was already disposed.
		private bool _wasMaximized = false;									// Flag to indicate that the window was maximized.
		private IList<IDisposable> _trackedObjects = null;					// List of tracked objects.
		private bool _wasWindowed = true;									// Flag to indicate that the device was windowed.
		private GorgonRenderTarget _currentTarget = null;					// Current render target.
		private GorgonSwapChain[] _swapChains = null;						// Swap chains for a multi-head device window.
		private int _currentHead = -1;										// Currently active head.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the object holding the current window state.
		/// </summary>
		protected FormStateRecord[] WindowState
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the swap chains available for use by each head.
		/// </summary>
		protected GorgonSwapChain[] SwapChains
		{
			get
			{
				return _swapChains;
			}
		}

		/// <summary>
		/// Property to return whether the device window is a multi-head device window or not.
		/// </summary>
		public bool IsMultiHead
		{
			get
			{
				return Settings.HeadSettings.Count != 0;
			}
		}

		/// <summary>
		/// Property to return whether the target has a depth buffer attached to it.
		/// </summary>
		public override bool HasDepthBuffer
		{
			get 
			{
				GorgonBufferFormat format = GorgonBufferFormat.Unknown;

				if (CurrentHead == 0)
					format = Settings.DepthStencilFormat;
				else
					format = Settings.HeadSettings[CurrentHead - 1].DepthStencilFormat;

				switch (format)
				{
					case GorgonBufferFormat.D24_Float_S8_UInt:
					case GorgonBufferFormat.D24_UIntNormal_X4S4_UInt:
					case GorgonBufferFormat.D15_UIntNormal_S1_UInt:
					case GorgonBufferFormat.D24_UIntNormal_S8_UInt:
					case GorgonBufferFormat.D32_Float:
					case GorgonBufferFormat.D32_UIntNormal:
					case GorgonBufferFormat.D32_Float_Lockable:
					case GorgonBufferFormat.D24_UIntNormal_X8:
					case GorgonBufferFormat.D16_UIntNormal_Lockable:
					case GorgonBufferFormat.D16_UIntNormal:
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Property to set or return the current render target.
		/// </summary>
		public GorgonRenderTarget CurrentTarget
		{
			get
			{
				return _currentTarget;
			}
			set
			{
				if (value == null)
					value = this;

				// Don't set the same target.
				if (_currentTarget == value)
					return;
				
				SetRenderTargetImpl(value);

				_currentTarget = value;
			}
		}

		/// <summary>
		/// Property to return whether the target has a stencil buffer attaached to it.
		/// </summary>
		/// <remarks>This will return the whether a stencil exists for the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindow.CurrentHead">current head</see> in a multi-head device window.</remarks>
		public override bool HasStencilBuffer
		{
			get 
			{
				GorgonBufferFormat format = GorgonBufferFormat.Unknown;

				if (CurrentHead == 0)
					format = Settings.DepthStencilFormat;
				else
					format = Settings.HeadSettings[CurrentHead - 1].DepthStencilFormat;

				switch (format)
				{
					case GorgonBufferFormat.D32_Float_S8X24_UInt:
					case GorgonBufferFormat.D24_Float_S8_UInt:
					case GorgonBufferFormat.D24_UIntNormal_X4S4_UInt:
					case GorgonBufferFormat.D15_UIntNormal_S1_UInt:
					case GorgonBufferFormat.D24_UIntNormal_S8_UInt:
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Property to return the number of active heads on the device window.
		/// </summary>
		/// <remarks>This includes the master device window in the count.</remarks>
		public int HeadCount
		{
			get
			{
				return _swapChains.Length;
			}
		}

		/// <summary>
		/// Property to set or return the currently active head in a multi-head device window.
		/// </summary>
		public int CurrentHead
		{
			get
			{
				return _currentHead;
			}
			set
			{
				if (value == _currentHead)
					return;

				if (value < 0)
					value = 0;
				if (value >= _swapChains.Length)
					value = _swapChains.Length - 1;

				_currentHead = value;
				_currentTarget = this;

				SetRenderTargetImpl(this);
			}
		}

		/// <summary>
		/// Property to return the depth/stencil buffer surface object a device window.
		/// </summary>
		/// <remarks>This will return the depth/stencil surface for the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindow.CurrentHead">current head</see> in a multi-head device window.</remarks>
		public override GorgonSurface DepthStencilSurface
		{
			get
			{
				return _swapChains[CurrentHead].DepthStencilSurface;
			}
			protected set
			{
			}
		}

		/// <summary>
		/// Property to return the back buffer surface for a device window.
		/// </summary>
		/// <remarks>This will return the back buffer surface for the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindow.CurrentHead">current head</see> in a multi-head device window.</remarks>
		public override GorgonSurface Surface
		{
			get
			{
				return _swapChains[CurrentHead].Surface;
			}
			protected set
			{				
			}
		}

		/// <summary>
		/// Property to return the internal swap chain associated with this device window.
		/// </summary>
		/// <remarks>This will return the swap chain for the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindow.CurrentHead">current head</see> in a multi-head device window.</remarks>
		public GorgonSwapChain SwapChain
		{
			get
			{
				return _swapChains[CurrentHead - 1];
			}
		}
		#endregion
		
		#region Methods.
		/// <summary>
		/// Function to determine if an object belongs to this device window, and throw an exception if it is not.
		/// </summary>
		/// <param name="childObject">Child object to check.</param>
		private void CheckValidObject(IDeviceWindowChild childObject)
		{
			if (childObject == null)
				return;

			if (childObject.DeviceWindow != this)
				throw new GorgonException(GorgonResult.AccessDenied, "The specificed object is was not created by this device window.");
		}

		/// <summary>
		/// Function to add an item to the object tracker collection.
		/// </summary>
		/// <param name="trackedObject">Object to track.</param>
		private void AddToObjectTracker(IDisposable trackedObject)
		{
			if (trackedObject == null)
				return;

			_trackedObjects.Add(trackedObject);
		}

		/// <summary>
		/// Function called when the device is about to be reset.
		/// </summary>
		protected override void OnBeforeDeviceReset()
		{
			_currentHead = -1;
			_currentTarget = null;
			base.OnBeforeDeviceReset();
		}

		/// <summary>
		/// Function called when the device has been reset.
		/// </summary>
		protected override void OnAfterDeviceReset()
		{
			CurrentHead = 0;
			base.OnAfterDeviceReset();
		}

		/// <summary>
		/// Function to set the current render target.
		/// </summary>
		/// <param name="target">Target to set.</param>
		protected abstract void SetRenderTargetImpl(GorgonRenderTarget target);

		/// <summary>
		/// Function to clean up the tracked objects and remove this object from any trackers.
		/// </summary>
		protected void CleanUpTrackedObjects()
		{
			((IObjectTracker)this).CleanUpTrackedObjects();

			// Remove us from anything tracking us.
			((IObjectTracker)Graphics).RemoveTrackedObject(this);
		}

		/// <summary>
		/// Function to clear out any outstanding resources when the object is disposed.
		/// </summary>
		/// <remarks>Implementors must call this method to clean up outstanding resources in the Dispose method.</remarks>
		protected override void CleanUpResources()
		{
			base.CleanUpResources();

			CleanUpTest();
			CleanUpTrackedObjects();

			_currentHead = 0;
			_currentTarget = null;
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
					_currentTarget = null;

					foreach (var state in WindowState)
					{
						if (state != null)
							state.Restore(false, false);
					}
					
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
			if (Settings.IsWindowed)
			{				
				base.OnWindowResized(newWidth, newHeight);
			}
		}

		/// <summary>
		/// Function to create a swap chain.
		/// </summary>
		/// <param name="name">Name of the swap chain.</param>
		/// <param name="settings">Settings for the swap chain.</param>
		/// <returns>A new Gorgon swap chain.</returns>
		protected abstract GorgonSwapChain CreateSwapChainImpl(string name, GorgonSwapChainSettings settings);

		/// <summary>
		/// Function to initialize the device window.
		/// </summary>
		internal override void Initialize()
		{
			base.Initialize();
			CurrentHead = 0;
		}

		/// <summary>
		/// Function to validate the swap chain settings.
		/// </summary>
		/// <param name="settings">Settings to validate.</param>
		/// <param name="currentInstance">The current instance of the swap chain if we're resetting.</param>
		internal void ValidateSwapChainSettings(GorgonSwapChainSettings settings, GorgonSwapChain currentInstance)
		{
			IObjectTracker tracker = this as IObjectTracker;

			if (settings == null)
				throw new ArgumentNullException("settings");

			// Ensure that we're not already using this window as a device window.
			if (Graphics.CheckWindowBound(settings, currentInstance))
				throw new ArgumentException("The specified window is already assigned to a device window or swap chain.", "settings");

			// If we haven't specified a size, or we're using a child control, then assume the size of the bound window.
			if (settings.Dimensions == System.Drawing.Size.Empty)
				settings.Dimensions = settings.BoundWindow.ClientSize;

			// Use the default desktop display format if we haven't picked a format.
			if (settings.Format == GorgonBufferFormat.Unknown)
				settings.Format = Settings.Output.DefaultVideoMode.Format;

			// Check back buffer and depth formats.
			Graphics.CheckFormats(settings, Settings.Output, true);

			// We can only enable multi-sampling when we have a discard swap effect and have non-lockable depth buffers.
			Graphics.CheckValidMSAA(settings, Settings.Device);
		}

		/// <summary>
		/// Function to create a swap chain.
		/// </summary>
		/// <param name="name">The name of the swap chain.</param>
		/// <param name="settings">The settings for the swap chain.</param>
		/// <returns>A new Gorgon swap chain.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.BoundWindow">BoundWindow</see> property of the settings parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the BoundWindow property of the settings parameter has already been used as a swap chain window.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the depth/stencil format is not support or when the backbuffer format cannot be used with the display format.</exception>
		public GorgonSwapChain CreateSwapChain(string name, GorgonSwapChainSettings settings)
		{
			GorgonSwapChain swapChain = null;

			ValidateSwapChainSettings(settings, null);

			Gorgon.Log.Print("Creating new swap chain '{0}'.", Diagnostics.GorgonLoggingLevel.Simple, name);
						
			// Create and initialize swap chain.
			swapChain = CreateSwapChainImpl(name, settings);
			swapChain.Initialize();

			AddToObjectTracker(swapChain);

			Gorgon.Log.Print("Swap chain '{0}' created successfully.", Diagnostics.GorgonLoggingLevel.Simple, name);

			return swapChain;
		}		

		/// <summary>
		/// Function to update the device window.
		/// </summary>
		/// <remarks>Use this method to apply changes the <see cref="GorgonLibrary.Graphics.GorgonDeviceWindowSettings">dimensions, format, fullscreen/windowed state and depth information</see> for the device window.
		/// <para>The <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateNominator">RefreshRateNominator</see> and the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateDenominator">RefreshRateDenominator</see> 
		/// of the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindow.Settings">Settings</see> property are not relevant when in windowed mode.</para>
		/// <para>Device windows bound to child controls or device windows with extra <see cref="GorgonLibrary.Graphics.GorgonSwapChain">swap chains</see> attached to them cannot go full screen, setting the <see cref="P:GorgonDeviceWindowSettings.Windowed"/> setting to TRUE will throw an exception.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentException">Thrown when the window is a child control, or when there are extra swap chains belonging to this device window and setting the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.IsWindowed">GorgonDeviceWindowSettings.IsWindowed</see> property to FALSE.
		/// </exception>
		public override void UpdateSettings()
		{
			Form window = Settings.BoundWindow as Form;

			// Force to full screen when multi-head settings are present.
			if (Settings.HeadSettings.Count > 0)
				Settings.IsWindowed = false;

			Graphics.ValidateDeviceWindowSettings(Settings, this);
			
			// Child controls and device windows with swap chains cannot go full screen.
			if (!Settings.IsWindowed)
			{
				if (_trackedObjects.Count(item => item is GorgonSwapChain) > 0)
					throw new ArgumentException("This device window has additional swap chains, could not switch to full screen.", "fullScreen");
			}

			RemoveEventHandlers();

			// Ideally, we want the window to use the desktop resolution when we switch to full screen and the window is maximized.
			// When we switch back, we should keep the window maximized.  This wouldn't be a big deal except there's some sort of
			// weird bug in windows forms/d3d9 where upon switching to full screen mode with a window maximized, the mouse cursor
			// tends to just pass through the window to the underlying window (if any), causing all kinds of weirdness.  So
			// our solution is to restore the window to a normal state, and apply the changes.

			// If we're switching back to windowed mode and the window was previously maximized, then re-maximize it.
			if ((Settings.IsWindowed) && (!_wasWindowed) && (_wasMaximized))
				window.WindowState = FormWindowState.Maximized;

			// Store whether the window was previously maximized.
			_wasMaximized = ((Settings.IsWindowed != _wasWindowed) && (window.WindowState == FormWindowState.Maximized));
			_wasWindowed = Settings.IsWindowed;
			UpdateResources();
			AddEventHandlers();

			Gorgon.Log.Print("Updating device window '{0}' with settings: {1}x{2} Format: {3} Refresh Rate: {4}/{5}.", Diagnostics.GorgonLoggingLevel.Intermediate, Name, Settings.DisplayMode.Width, Settings.DisplayMode.Height, Settings.DisplayMode.Format, Settings.DisplayMode.RefreshRateNumerator, Settings.DisplayMode.RefreshRateDenominator);
			Gorgon.Log.Print("'{0}' information:", Diagnostics.GorgonLoggingLevel.Verbose, Name);
			Gorgon.Log.Print("\tLayout: {0}x{1} Format: {2} Refresh Rate: {3}/{4}", Diagnostics.GorgonLoggingLevel.Verbose, Settings.DisplayMode.Width, Settings.DisplayMode.Height, Settings.DisplayMode.Format, Settings.DisplayMode.RefreshRateNumerator, Settings.DisplayMode.RefreshRateDenominator);
			Gorgon.Log.Print("\tDepth/Stencil: {0} (Format: {1})", Diagnostics.GorgonLoggingLevel.Verbose, Settings.DepthStencilFormat != GorgonBufferFormat.Unknown, Settings.DepthStencilFormat);
			Gorgon.Log.Print("\tWindowed: {0}", Diagnostics.GorgonLoggingLevel.Verbose, Settings.IsWindowed);
			Gorgon.Log.Print("\tMSAA: {0}", Diagnostics.GorgonLoggingLevel.Verbose, Settings.MSAAQualityLevel.Level != GorgonMSAALevel.None);
			if (Settings.MSAAQualityLevel.Level != GorgonMSAALevel.None)
				Gorgon.Log.Print("\t\tMSAA Quality: {0}  Level: {1}", Diagnostics.GorgonLoggingLevel.Verbose, Settings.MSAAQualityLevel.Quality, Settings.MSAAQualityLevel.Level);
			Gorgon.Log.Print("\tBackbuffer Count: {0}", Diagnostics.GorgonLoggingLevel.Verbose, Settings.BackBufferCount);
			Gorgon.Log.Print("\tDisplay Function: {0}", Diagnostics.GorgonLoggingLevel.Verbose, Settings.DisplayFunction);
			Gorgon.Log.Print("\tV-Sync interval: {0}", Diagnostics.GorgonLoggingLevel.Verbose, Settings.VSyncInterval);
			Gorgon.Log.Print("\tVideo surface: {0}", Diagnostics.GorgonLoggingLevel.Verbose, Settings.WillUseVideo);

			foreach (var settings in Settings.HeadSettings)
			{
				Gorgon.Log.Print("Setting multi head settings for output '{0}' with settings: {1}x{2} Format: {3} Refresh Rate: {4}/{5}.", Diagnostics.GorgonLoggingLevel.Intermediate, settings.Output.Name, settings.DisplayMode.Width, settings.DisplayMode.Height, settings.DisplayMode.Format, settings.DisplayMode.RefreshRateNumerator, settings.DisplayMode.RefreshRateDenominator);
				Gorgon.Log.Print("\tLayout: {0}x{1} Format: {2} Refresh Rate: {3}/{4}", Diagnostics.GorgonLoggingLevel.Verbose, settings.DisplayMode.Width, settings.DisplayMode.Height, settings.DisplayMode.Format, settings.DisplayMode.RefreshRateNumerator, settings.DisplayMode.RefreshRateDenominator);
				Gorgon.Log.Print("\tDepth/Stencil: {0} (Format: {1})", Diagnostics.GorgonLoggingLevel.Verbose, settings.DepthStencilFormat != GorgonBufferFormat.Unknown, settings.DepthStencilFormat);
				Gorgon.Log.Print("\tMSAA: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.MSAAQualityLevel.Level != GorgonMSAALevel.None);
				if (settings.MSAAQualityLevel.Level != GorgonMSAALevel.None)
					Gorgon.Log.Print("\t\tMSAA Quality: {0}  Level: {1}", Diagnostics.GorgonLoggingLevel.Verbose, settings.MSAAQualityLevel.Quality, settings.MSAAQualityLevel.Level);
				Gorgon.Log.Print("\tBackbuffer Count: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.BackBufferCount);
				Gorgon.Log.Print("\tDisplay Function: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.DisplayFunction);
				Gorgon.Log.Print("\tV-Sync interval: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.VSyncInterval);
				Gorgon.Log.Print("\tVideo surface: {0}", Diagnostics.GorgonLoggingLevel.Verbose, settings.WillUseVideo);
			}

			Gorgon.Log.Print("Device window '{0}' updated.", Diagnostics.GorgonLoggingLevel.Simple, Name);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDeviceWindow"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="graphics">The graphics instance that owns this render target.</param>
		/// <param name="settings">Device window settings.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameters is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		/// <remarks>A fullscreen video mode must have a a Windows Form object as the bound window.
		/// <para>The <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateNominator">RefreshRateNominator</see> and the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateDenominator">RefreshRateDenominator</see> 
		/// of the <see cref="GorgonLibrary.Graphics.GorgonVideoMode">GorgonVideoMode</see> type are not relevant when fullScreen is set to FALSE.</para>
		/// </remarks>
		protected GorgonDeviceWindow(GorgonGraphics graphics, string name, GorgonDeviceWindowSettings settings)
			: base(graphics, name, settings)
		{
			Form window = settings.BoundWindow as Form;

			_swapChains = new GorgonSwapChain[settings.HeadSettings.Count + 1];			
						
			_trackedObjects = new List<IDisposable>();

			_wasWindowed = Settings.IsWindowed;

			if (window != null)
			{
				WindowState = new FormStateRecord[settings.HeadSettings.Count + 1];
				WindowState[0] = new FormStateRecord(window);

				for (int i = 0; i < settings.HeadSettings.Count; i++)
					WindowState[i + 1] = new FormStateRecord(settings.HeadSettings[i].BoundForm);
			}
			else
				WindowState = new FormStateRecord[0];
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
				Gorgon.Log.Print("Destroying {0} child objects:", Diagnostics.GorgonLoggingLevel.Verbose, trackedObjects.Count());

				foreach (var trackedObject in trackedObjects)
					trackedObject.Dispose();
			}

			_trackedObjects.Clear();
		}
		#endregion
		#endregion
	}
}
