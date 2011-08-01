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
		: GorgonSwapChain
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
		/// Property to set or return whether this device window is full screen or windowed.
		/// </summary>
		public bool IsWindowed
		{
			get;
			private set;
		}
		#endregion
		
		#region Methods.
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
					RemoveEventHandlers();

					CleanUpTest();

					// Remove us from the tracking.
					if (Graphics.DeviceWindows.Contains(this))
						Graphics.DeviceWindows.Remove(this);

					if (BoundWindow is Form)
						_originalWindowState.Restore(false, false);
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}
		
		/// <summary>
		/// Function to update the device window.
		/// </summary>
		/// <param name="mode">Video mode dimensions to use.</param>
		/// <param name="depthStencilFormat">Format of the depth buffer.</param>
		/// <param name="fullScreen">TRUE to use full screen, or FALSE to use windowed mode.</param>
		/// <remarks>Use this function to change the dimensions, format, fullscreen/windowed state and depth information for the device window.
		/// <para>The <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateNominator">RefreshRateNominator</see> and the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateDenominator">RefreshRateDenominator</see> 
		/// of the <see cref="GorgonLibrary.Graphics.GorgonVideoMode">GorgonVideoMode</see> type are not relevant when <param name="fullScreen"/> is set to FALSE.</para>
		/// <para>Device windows bound to child controls cannot go full screen, setting the <paramref name="fullScreen"/> parameter to TRUE will have no effect.</para>
		/// </remarks>
		public void Update(GorgonVideoMode mode, GorgonBufferFormat depthStencilFormat, bool fullScreen)
		{
			// Child controls cannot go full screen.
			if (!(BoundWindow is Form))
				fullScreen = false;

			RemoveEventHandlers();
			UpdateTargetInformation(mode, depthStencilFormat);
			IsWindowed = !fullScreen;
			UpdateRenderTarget();
			AddEventHandlers();
		}

		#region Remove this shit.
		/// <summary>
		/// 
		/// </summary>
		public abstract void SetupTest();

		/// <summary>
		/// 
		/// </summary>
		public abstract void RunTest();

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
		/// <param name="mode">A video mode structure defining the width, height and format of the render target.</param>
		/// <param name="depthStencilFormat">The depth buffer format (if required) for the target.</param>
		/// <param name="fullScreen">TRUE to go full screen, FALSE to stay windowed.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <param name="device"/>, <param name="output"/> or <param name="window"> parameters are NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.</exception>
		/// <remarks>When passing TRUE to <paramref name="fullScreen"/>, then the <paramref name="window"/> parameter must be a Windows Form object.
		/// <para>The <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateNominator">RefreshRateNominator</see> and the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.RefreshRateDenominator">RefreshRateDenominator</see> 
		/// of the <see cref="GorgonLibrary.Graphics.GorgonVideoMode">GorgonVideoMode</see> type are not relevant when <param name="fullScreen"/> is set to FALSE.</para>
		/// </remarks>
		protected GorgonDeviceWindow(GorgonGraphics graphics, string name, GorgonVideoDevice device, GorgonVideoOutput output, Control window, GorgonVideoMode mode, GorgonBufferFormat depthStencilFormat, bool fullScreen)
			: base(graphics, name, window, mode, depthStencilFormat)
		{
			if (device == null)
				throw new ArgumentNullException("device");
			if (output == null)
				throw new ArgumentNullException("output");

			// For child controls, do not go to full screen.
			if (!(window is Form))
				fullScreen = false;
						
			VideoDevice = device;
			VideoOutput = output;
			IsWindowed = !fullScreen;

			if (window is Form)
			{
				_originalWindowState = new FormStateRecord(window as Form);
				WindowState = new FormStateRecord(window as Form);
			}
		}
		#endregion
	}
}
