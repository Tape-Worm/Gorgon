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
					_window.DesktopLocation = _location;
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
				_location = _window.DesktopLocation;
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
				_location = window.DesktopLocation;
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
		private bool _wasWindowed = true;							// Flag to indicate that the device was windowed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the object holding the current window state.
		/// </summary>
		protected FormStateRecord WindowState
		{
			get;
			private set;
		}
		#endregion
		
		#region Methods.
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
					if (Settings.BoundWindow is Form)
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
			if (Settings.IsWindowed)
			{
				if ((Settings.BoundForm.WindowState != FormWindowState.Minimized) && (Settings.BoundWindow.ClientSize.Width > 0) && (Settings.BoundWindow.ClientSize.Height > 0))
				{
					Settings.Width = newWidth;
					Settings.Height = newHeight;
					base.OnWindowResized(newWidth, newHeight);
				}
			}
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
		/// <remarks>Use this method to apply changes the <see cref="GorgonLibrary.Graphics.GorgonDeviceWindowSettings">dimensions, format, fullscreen/windowed state and depth information</see> for the device window.
		/// <para>The <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateNominator">RefreshRateNominator</see> and the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateDenominator">RefreshRateDenominator</see> 
		/// of the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindow.Settings">Settings</see> property are not relevant when in windowed mode.</para>
		/// <para>Device windows bound to child controls or device windows with extra <see cref="GorgonLibrary.Graphics.GorgonSwapChain">swap chains</see> attached to them cannot go full screen, setting the <see cref="P:GorgonDeviceWindowSettings.Windowed"/> setting to TRUE will throw an exception.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentException">Thrown when the window is a child control, or when there are extra swap chains belonging to this device window and setting the <see cref="P:GorgonLibrary.Graphics.GorgonDeviceWindowSettings.IsWindowed">GorgonDeviceWindowSettings.IsWindowed</see> property to FALSE.
		/// </exception>
		public virtual void UpdateSettings()
		{
			Form window = Settings.BoundWindow as Form;
			
			// Child controls and device windows with swap chains cannot go full screen.
			if (!Settings.IsWindowed)
			{
				if (window == null)
					throw new ArgumentException("Cannot switch to full screen with a child control.", "fullScreen");
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

			Gorgon.Log.Print("Updating device window '{0}' with settings: {1}x{2} Format: {3} Refresh Rate: {4}/{5}.", Diagnostics.GorgonLoggingLevel.Verbose, Name, Settings.DisplayMode.Width, Settings.DisplayMode.Height, Settings.DisplayMode.Format, Settings.DisplayMode.RefreshRateNumerator, Settings.DisplayMode.RefreshRateDenominator);
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

			_trackedObjects = new List<IDisposable>();

			_wasWindowed = Settings.IsWindowed;

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
