#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Wednesday, April 27, 2005 10:29:58 AM
// 
#endregion

using System;
using System.Drawing;
using Forms = System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Native;
using SharpUtilities.Native.Win32;
using D3D = Microsoft.DirectX.Direct3D;
using DX = Microsoft.DirectX;
using GorgonLibrary.FileSystems;
using GorgonLibrary.Internal;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Fonts;
using GorgonLibrary.Graphics.Shaders;
using GorgonLibrary.Timing;
using GorgonLibrary.PlugIns;
using GorgonLibrary.Input;

namespace GorgonLibrary
{
	/// <summary>
	/// Main class for gorgon.
	/// </summary>
	/// <remarks>
	/// This is the main entry point for Gorgon.  All gorgon objects are stored here and accessible through their related properties.
	/// 
	/// Objects that are created from this object class are registered with this class and as such this object will be responisble for clean up 
	/// of those objects (that is, you won't have to clean up all the objects you create) unless you need to destroy those objects yourself, 
	/// and then those objects should be destroyed via their property collection interfaces (i.e. Remove()/Destroy() functions in the collection).
	/// 
	/// This object is also responsible for storing information about the current running state, such as video modes, current video adapter 
	/// and some global flags and events.
	/// </remarks>
	public static class Gorgon
	{
		#region Classes.
		/// <summary>
		/// Class to stop screensaver messages.
		/// </summary>
		private class SysMessageFilter : Forms.IMessageFilter
		{
			#region IMessageFilter Members
			/// <summary>
			/// Function to trap messages.
			/// </summary>
			/// <param name="m">Message data.</param>
			/// <returns>TRUE if trapped, FALSE if skipped.</returns>
			public bool PreFilterMessage(ref Forms.Message m)
			{
				if ((((WindowMessages)m.Msg) == WindowMessages.SysCommand) && (!Gorgon.AllowScreenSaver))
				{
					// Trap screen saver.
					switch ((SysCommands)(m.WParam.ToInt32() & 0xFFF0))
					{
						case SysCommands.MonitorPower:
						case SysCommands.ScreenSave:
							Gorgon.Log.Print("Gorgon", "Screen saver activated, disabling.", LoggingLevel.Verbose);
							m.Result = IntPtr.Zero;
							return true;
					}
				}
				return false;
			}
			#endregion
		}
		#endregion

		#region Events.
		/// <summary>Event fired when a driver is currently being changed.</summary>
		public static event DriverChangingHandler OnDriverChanging;
		/// <summary>Event fired when a driver has changed.</summary>
		public static event DriverChangedHandler OnDriverChanged;
		/// <summary>Event fired when the device has been reset from a lost state.</summary>
		public static event EventHandler OnDeviceReset;
		/// <summary>Event fired when the device has been put into a lost state.</summary>
		public static event EventHandler OnDeviceLost;
		#endregion

		#region Variables.
		private static DeviceStateList _deviceStateList = null;			// List of device state objects.
		private static Logger _log;										// Log file.
		private static Renderer _renderer;								// Renderer object.
		private static bool _backgroundProcessing;						// Flag to indicate that we want to continue rendering in the background.
		private static bool _running;									// Flag to indicate whether the app is in a running state.
		private static ClearTargets _clearTargets;						// Target buffers to clear.
		private static SysMessageFilter _messageFilter;					// Windows message filter.
		private static bool _allowScreenSaver;							// Flag to indicate whether we want to allow the screensaver to run or not.
		private static PreciseTimer _timer;								// Main Gorgon timer.
		private static bool _initialized = false;						// Flag to inidcate whether Gorgon has been initialized or not.
		private static PlugInManager _pluginList = null;				// Plug in list.
		private static DriverList _drivers = null;						// List of video drivers for the system.
		private static Driver _currentDriver = null;					// Current driver.
		private static VideoMode _desktopVideoMode;						// Current video mode of the desktop.
		private static ShaderManager _shaders = null;					// Global shader manager.
		private static bool _fastResize = false;						// Flag to indicate that the screen should be scaled if the window is resized, otherwise a device reset occours.
		private static InputDevices _input = null;						// Input interface.
		private static bool _enableLogo = false;						// TRUE to show the logo overlay, FALSE to turn it off.
		private static bool _enableStats = false;						// TRUE to show frame rate stats, FALSE to turn it off.
        private static FileSystemManager _fileSystems = null;			// File system manager.
		private static StateManager _stateManager = null;				// State manager.
		private static ImageManager _images = null;						// Global image manager.
#if INCLUDE_D3DREF
		private static bool _refDevice = false;							// Flag to indicate if we're using a reference device or HAL device.
#endif
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return if an application is in an idle state or not.
		/// </summary>
		private static bool AppIdle
		{
			get
			{
				MSG message;		// Message to retrieve.

				return !Win32API.PeekMessage(out message, IntPtr.Zero, 0, 0, PeekMessageFlags.NoRemove);
			}
		}

		/// <summary>
		/// Property to return if the application has focus.
		/// </summary>
		private static bool HasFocus
		{
			get
			{
				if ((Screen.OwnerForm.WindowState == Forms.FormWindowState.Minimized) || ((!Screen.OwnerForm.ContainsFocus) && (!Screen.Windowed)))
					return false;

				if (!_backgroundProcessing)
				{
					if (!Screen.OwnerForm.ContainsFocus)
						return false;
				} 
	
				return true;
			}
		}

		/// <summary>
		/// Property to return the device state object list.
		/// </summary>
		internal static DeviceStateList DeviceStateList
		{
			get
			{
				return _deviceStateList;
			}
		}

        /// <summary>
        /// Property to return the renderer interface.
        /// </summary>
        internal static Renderer Renderer
        {
            get
            {
                if (!_initialized)
                    throw new NotInitializedException(null);

                return _renderer;
            }
        }

		/// <summary>
		/// Property to return the current render target.
		/// </summary>
		public static RenderTarget CurrentRenderTarget
		{
			get
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				return _renderer.CurrentRenderTarget;
			}
		}

		/// <summary>
		/// Property to return the state manager interface.
		/// </summary>
		public static StateManager StateManager
		{
			get
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				return _stateManager;
			}
		}

		/// <summary>
		/// Property to return the timer that Gorgon is using.
		/// </summary>
		public static PreciseTimer Timer
		{
			get
			{
				return _timer;
			}
		}

		/// <summary>
		/// Property to return the interface to the internal images.
		/// </summary>
		public static ImageManager ImageManager
		{
			get
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				return _images;
			}
		}
		
		/// <summary>
		/// Property to return the shader list.
		/// </summary>
		public static ShaderManager Shaders
		{
			get
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				return _shaders;
			}
		}

		/// <summary>
		/// Property to return the input interface.
		/// </summary>
		public static InputDevices InputDevices
		{
			get
			{
				return _input;
			}
			set
			{
				// If this is the same plug-in, do nothing.
				if (_input == value)
					return;

				// Remove the previous plug-in.				
				if (_input != null)
					_input.Dispose();

				_input = value;
			}
		}

		/// <summary>
		/// Property to set or return whether or not the device will reset when the window is resized.  A value of TRUE will just cause the screen to be scaled and a lower quality image will be used, FALSE will perform the device reset.
		/// </summary>
		public static bool FastResize
		{
			get
			{
				return _fastResize;
			}
			set
			{
				_fastResize = value;
			}
		}

        /// <summary>
        /// Property to return the virtual file systems interface.
        /// </summary>
        public static FileSystemManager FileSystems
        {
            get
            {
                if (!_initialized)
                    throw new NotInitializedException(null);

                return _fileSystems;
            }
        }

#if INCLUDE_D3DREF
		/// <summary>
		/// Property to return whether we're using the reference device.
		/// This is *ONLY* available when the library is compiled with INCLUDE_D3DREF.
		/// </summary>
		/// <returns>TRUE if we're using a ref device, FALSE if not (HAL).</returns>
		public static bool UseReferenceDevice
		{
			get
			{
				return _refDevice;
			}
			set
			{
				_refDevice = value;
			}
		}
#endif

		/// <summary>
		/// Property to return the logging object.
		/// </summary>
		/// <remarks>This logging interface requires a reference to the SharpUtilities library.</remarks>
		/// <value>This is a read-only property to return the internal static logging interface.</value>
		public static Logger Log
		{
			get
			{
				return _log;
			}
		}

		/// <summary>
		/// Property to return the primary render target.
		/// </summary>
		/// <remarks>This is the primary render target that holds the initial video mode.</remarks>
		public static RenderWindow Screen
		{
			get
			{
				if (_renderer == null)
					return null;

				if (!_initialized)
					throw new NotInitializedException(null);

				return _renderer.RenderTargets.Screen;
			}
		}

		/// <summary>
		/// Property to set or return the currently active driver.
		/// </summary>
		/// <remarks>
		/// Setting this property to another driver index will make the video mode reset and all data uploaded to the video card (such as textures) will be destroyed and will have to be reset after the video mode has been set again.
		/// <para>The reason it does this is due to the uncertainty of using multiple video cards:
		/// If video card A has only 640x480x16 and video card B has 800x600x32 and 640x480x32, and we are currently using B, and switching to A then there's no way either of B's modes are supported on A and thus no way to set the video mode without an exception.</para>
		/// <para>
		/// After the driver ID is changed, it is up to the client program to determine if the mode is supported and reset to that mode, else set to a default video mode.
		/// </para>
		/// <para>
		/// You should confirm how many drivers are installed in the system via the <see cref="DriverList">Drivers.Count</see> property before setting the driver index.
		/// </para>
		/// </remarks>
		/// <value>This will get/set the current video driver index, which is ranged from 0 to <seealso cref="DriverList">DriverList</seealso>.Count-1.</value>
		public static Driver Driver
		{
			get
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				return _currentDriver;
			}
			set
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				Stop();

				// Don't set the same driver twice.
				if ((value != null) && (value == _currentDriver))
					return;

				DriverChangingArgs changingArgs = new DriverChangingArgs(_currentDriver, value);

				Gorgon.DriverChanging(changingArgs);

				// Don't allow change if we cancel.
				if (changingArgs.Cancel)
					return;

				// Remove all buffers.
				if ((_currentDriver != null) && (_renderer != null))
					_deviceStateList.ForceRelease();

				Gorgon.Log.Print("Gorgon", "Changing video drivers...", LoggingLevel.Simple);

				// Do actual driver change.
				if (_renderer != null)
					_renderer.Dispose();
				_renderer = null;

				// Retrieve the desktop video mode for the driver.
				_desktopVideoMode = new VideoMode();
				_desktopVideoMode.Width = D3D.Manager.Adapters[value.DriverIndex].CurrentDisplayMode.Width;
				_desktopVideoMode.Height = D3D.Manager.Adapters[value.DriverIndex].CurrentDisplayMode.Height;
				_desktopVideoMode.RefreshRate = D3D.Manager.Adapters[value.DriverIndex].CurrentDisplayMode.RefreshRate;
				_desktopVideoMode.Format = Converter.Convert(D3D.Manager.Adapters[value.DriverIndex].CurrentDisplayMode.Format);

				_currentDriver = value;

				// Restart the renderer.
				_renderer = new Renderer();

				// TODO: Destroy more stuff.

				Gorgon.Log.Print("Gorgon", "Driver changed to {0} ({1}).", LoggingLevel.Simple, value.Description, value.DriverName);

				Gorgon.DriverChanged(new DriverChangedArgs(_currentDriver, value));
			}
		}

		/// <summary>
		/// Property to return a list of video drivers.
		/// </summary>
		/// <value>This is a read-only property that will return a list of installed <seealso cref="Driver">video drivers</seealso>.</value>
		public static DriverList Drivers
		{
			get
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				return _drivers;
			}
		}

		/// <summary>
		/// Property to set or return which buffers are cleared per frame.
		/// </summary>
		/// <example>
		/// To clear the back buffer and depth buffer, you should specify:
		/// <code>
		/// gorgonObject.ClearEachFrame = ClearTargets.BackBuffer | ClearTargets.DepthBuffer;
		/// </code>
		/// </example>
		/// <example>
		/// To clear no buffers:
		/// <code>
		/// gorgonObject.ClearEachFrame = ClearTargets.None;
		/// </code>
		/// </example>
		/// <remarks>
		/// All the <seealso cref="ClearTargets">ClearTargets</seealso> fields are inclusive, except the ClearTargets.None.  
		/// Setting the field to None will tell the system that no buffers are to be cleared.
		/// <para>
		/// Clearing the buffer(s) each frame can be an expensive process, especially on computers not equipped with the latest and greatest hardware.  Thus, for optimization, it's best to clear only what you need.  If you aren't making use of your stencil buffer, then don't bother to clear it each frame and so forth.
		/// </para>
		/// </remarks>
		/// <value>
		/// Set to one or a combination of the <see cref="ClearTargets">ClearTargets</see> fields to tell the system which buffers should be cleared each frame.
		/// <para>
		/// The property can be set to one or more of:
		/// <list type="table">
		/// 			<listheader>
		/// 				<term>Buffer type.</term>
		/// 				<description>Action.</description>
		/// 			</listheader>
		/// 			<item>
		/// 				<term>None</term>
		/// 				<description>Don't clear any buffers.  This flag cannot be used in conjunction with any of the other flags.</description>
		/// 			</item>
		/// 			<item>
		/// 				<term>BackBuffer</term>
		/// 				<description>Clear the back buffer.</description>
		/// 			</item>
		/// 			<item>
		/// 				<term>DepthBuffer</term>
		/// 				<description>Clear the depth buffer.</description>
		/// 			</item>
		/// 			<item>
		/// 				<term>StencilBuffer</term>
		/// 				<description>Clear the stencil buffer.</description>
		/// 			</item>
		/// 		</list>
		/// 	</para>
		/// </value>
		public static ClearTargets ClearEachFrame
		{
			get
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				return _clearTargets;
			}
			set
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				if ((value & ClearTargets.None) > 0)
					_clearTargets = ClearTargets.None;
				else
					_clearTargets = value;
			}
		}

		/// <summary>
		/// Property to set or return a flag that controls background rendering.
		/// </summary>
		/// <remarks>
		/// When this property is set to TRUE the CPU usage will jump to 100% while the application is running.  This is because it will continuously update and receive messages from the system while in the background.  This may cause the system to slow down (if you have a slow video card/CPU) when background rendering is on.
		/// <para>When the property is set to FALSE, the CPU usage will only be at 100% when the application is in the foreground and will throttle back to 0% when the application is in the background.  This is much more efficient, however the drawback is that the application will not update while in the background.</para>
		/// </remarks>
		/// <value>Setting this to TRUE will allow the engine to render while the window is not in the foreground or minimized.  Setting this to FALSE will halt rendering until the window is in the foreground.</value>
		public static bool AllowBackgroundRendering
		{
			get
			{
				return _backgroundProcessing;
			}
			set
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				_backgroundProcessing = value;
			}
		}

		/// <summary>
		/// Property to return if the app is in a running state or not.
		/// </summary>
		/// <remarks>This flag is set to TRUE when the <seealso cref="Go">Go()</seealso> function is called and FALSE when the <seealso cref="Stop">Stop()</seealso> function is called.</remarks>
		/// <value>This is a read-only property that will return TRUE if the application is running, and FALSE if not.</value>
		public static bool IsRunning
		{
			get
			{
				return _running;
			}
		}

		/// <summary>
		/// Property to return the currently active video mode.
		/// </summary>
		/// <remarks>This property will return a <seealso cref="VideoMode">VideoMode</seealso> value type.</remarks>
		/// <value>This is a read-only property that will return the currently active <see cref="VideoMode">video mode</see> information.</value>
		public static VideoMode VideoMode
		{
			get
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				if (Screen != null)
					return Screen.Mode;
				else
					return _desktopVideoMode;
			}
		}

		/// <summary>
		/// Property to return the video mode of the current desktop.
		/// </summary>
		/// <remarks>This property will return a <seealso cref="VideoMode">VideoMode</seealso> value type.</remarks>
		/// <value>This is a read-only property that will return the desktop <see cref="VideoMode">video mode</see> information.</value>
		public static VideoMode DesktopVideoMode
		{
			get
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				return _desktopVideoMode;
			}
		}

		/// <summary>
		/// Property to return the render target management interface.
		/// </summary>
		public static RenderTargetList RenderTargets
		{
			get
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				return _renderer.RenderTargets;
			}
		}

		/// <summary>
		/// Property to set or return whether we want to allow a screensaver to run or not.
		/// </summary>
		/// <remarks>
		/// When this property is set to TRUE all power management/screen saver functionality will be suspended while the application is running.  
		/// <para>
		/// Due to an odd quirk in XP (and perhaps Windows 2000) and you set the screensaver to return to the login/welcome screen after its deactivated the application will NOT suspend the screensaver/power management.  There currently is no workaround for this, however if you know of one, send the author an email explaining how to circumvent this.
		/// </para>
		/// </remarks>
		/// <value>Set this property to TRUE if you wish to allow the screensaver/power management to kick in.  Set to FALSE if you want to suspend the screensaver/power management.</value>
		public static bool AllowScreenSaver
		{
			get
			{
				return _allowScreenSaver;
			}
			set
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				_allowScreenSaver = value;
			}
		}

		/// <summary>
		/// Property to return the plug in manager interface.
		/// </summary>
		public static PlugInManager PlugIns
		{
			get
			{
				if (!_initialized)
					throw new NotInitializedException(null);

				return _pluginList;
			}
		}

		/// <summary>
		/// Property to set or return whether the logo is shown or not.
		/// </summary>
		public static bool LogoVisible
		{
			get
			{
				return _enableLogo;
			}
			set
			{
				_enableLogo = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the logo is shown or not.
		/// </summary>
		public static bool FrameStatsVisible
		{
			get
			{
				return _enableStats;
			}
			set
			{
				_enableStats = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to actually do processing for the application.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		private static void Run(object sender, EventArgs e)
		{
			// No renderer active, leave.
			if (_renderer == null)
				return;

			while ((AppIdle) && (_running))
			{
				// Do many things in here.
				if (HasFocus)
				{
					// Render each render target.
					// If we only have the screen optimize a bit by only rendering that.
					if (_renderer.RenderTargets.Count < 2)
						_renderer.RenderTargets.Screen.Update();
					else
					{
						for (int i = 0; i < _renderer.RenderTargets.Count; i++)
							_renderer.RenderTargets[i].Update();
					}
					

					// Give up some time if we don't have focus and we're windowed.
					if ((!Screen.OwnerForm.ContainsFocus) && (Screen.Windowed))
						System.Threading.Thread.Sleep(0);
				}
				else
					System.Threading.Thread.Sleep(250);
			}
		}

		/// <summary>
		/// Function to trigger driver changing event.
		/// </summary>
		/// <param name="e">Parameters for event.</param>
		internal static void DriverChanging(DriverChangingArgs e)
		{
			if (OnDriverChanging != null)
				OnDriverChanging(_drivers, e);
		}

		/// <summary>
		/// Function to trigger driver changing event.
		/// </summary>
		/// <param name="e">Parameters for event.</param>
		internal static void DriverChanged(DriverChangedArgs e)
		{
			if (OnDriverChanged != null)
				OnDriverChanged(_drivers, e);
		}

		/// <summary>
		/// Function to trigger the device lost event.
		/// </summary>
		internal static void DeviceLost()
		{
			if (OnDeviceLost != null)
				OnDeviceLost(_renderer, EventArgs.Empty);
		}

		/// <summary>
		/// Function to trigger the device reset event.
		/// </summary>
		internal static void DeviceReset()
		{
			if (OnDeviceReset != null)
				OnDeviceReset(_renderer, EventArgs.Empty);
		}

		/// <summary>
		/// Function to close an open video mode.
		/// </summary>
		public static void CloseMode()
		{
			Stop();

			// Close the video mode and remove any associated render targets.
			_renderer.RenderTargets.Clear();
		}

		/// <summary>
		/// Function to start the engine rendering.
		/// </summary>
		/// <remarks>The application does not begin rendering right away when this function is called, it merely tells the engine that the application is ready for rendering to begin when it's ready.</remarks>
		public static void Go()
		{
			if (!_initialized)
				throw new NotInitializedException(null);

			if (Screen == null)
				throw new InvalidRenderTargetException("The primary window has not been created, call SetMode() first.", null);

			if (_running)
				return;

			// Enter render loop.
			_log.Print("Gorgon", "Entering main render loop...",LoggingLevel.Verbose);

			// Reset all timers.
			_timer.Reset();

			if (_renderer.CurrentRenderTarget != null)
				_renderer.CurrentRenderTarget.Refresh();
			Forms.Application.Idle += new EventHandler(Run);

			_running = true;
		}

		/// <summary>
		/// Function to stop the engine from rendering.
		/// </summary>
		public static void Stop()
		{
			if (!_initialized)
				throw new NotInitializedException(null);

			if (_running)
			{
				Forms.Application.Idle -= new EventHandler(Run);
				_running = false;

				// Reset all timers.
				_timer.Reset();

				_log.Print("Gorgon", "Main render loop stopped.", LoggingLevel.Verbose);
			}
		}

		/// <summary>
		/// Function to force the application to process messages.
		/// </summary>
		public static void ProcessMessages()
		{
			MSG message;		// Message to retrieve.

			// Forward the messages.
			while (Win32API.PeekMessage(out message, IntPtr.Zero, 0, 0, PeekMessageFlags.Remove))
			{
				Win32API.TranslateMessage(ref message);
				Win32API.DispatchMessage(ref message);
			}

			// Continue on.
			if ((_running) && (Gorgon.Screen != null))
				Run(Gorgon.Screen, EventArgs.Empty);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>
		/// <param name="format">Buffer format for the video mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>
		/// <param name="usestencil">TRUE to create a stencil buffer, FALSE to not create.</param>
		/// <param name="vSyncInterval">V-sync interval for presentation.</param>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, int refresh, bool usedepth, bool usestencil, VSyncIntervals vSyncInterval)
		{
			if (!_initialized)
				throw new NotInitializedException(null);

			// Force the application to stop.
			Stop();

			_renderer.RenderTargets.CreatePrimaryRenderWindow(owner, width, height, format, windowed, refresh, usedepth, usestencil, vSyncInterval);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode, but will not create a stencil buffer.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>
		/// <param name="format">Buffer format for the video mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>
		/// <param name="vSyncInterval">V-sync interval for presentation.</param>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, int refresh, bool usedepth, VSyncIntervals vSyncInterval)
		{
			SetMode(owner, width, height, format, windowed, refresh, usedepth, false, vSyncInterval);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode, but will not create a stencil buffer and will create/enable a depth buffer.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>
		/// <param name="format">Buffer format for the video mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>
		/// <param name="vSyncInterval">V-sync interval for presentation.</param>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, int refresh, VSyncIntervals vSyncInterval)
		{
			SetMode(owner, width, height, format, windowed, refresh, false, false, vSyncInterval);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode, but will not create the stencil buffer, will create/enable a depth buffer and will default the refresh rate to 60Hz.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>
		/// <param name="format">Buffer format for the video mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="vSyncInterval">V-sync interval for presentation.</param>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, VSyncIntervals vSyncInterval)
		{
			SetMode(owner, width, height, format, windowed, 60, false, false, vSyncInterval);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="mode">Video mode to set.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>
		/// <param name="usestencil">TRUE to create a stencil buffer, FALSE to not create.</param>
		/// <param name="vSyncInterval">V-sync interval for presentation.</param>
		public static void SetMode(Forms.Control owner, VideoMode mode, bool windowed, int refresh, bool usedepth, bool usestencil, VSyncIntervals vSyncInterval)
		{
			SetMode(owner, mode.Width, mode.Height, mode.Format, windowed, refresh, usedepth, usestencil, vSyncInterval);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode, but will not create a stencil buffer.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="mode">Video mode to set.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>		
		/// <param name="vSyncInterval">V-sync interval for presentation.</param>
		public static void SetMode(Forms.Control owner, VideoMode mode, bool windowed, int refresh, bool usedepth, VSyncIntervals vSyncInterval)
		{
			SetMode(owner, mode.Width, mode.Height, mode.Format, windowed, refresh, usedepth, false, vSyncInterval);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode, but will not create a stencil buffer and will create/enable a depth buffer.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="mode">Video mode to set.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>		
		/// <param name="vSyncInterval">V-sync interval for presentation.</param>
		public static void SetMode(Forms.Control owner, VideoMode mode, bool windowed, int refresh, VSyncIntervals vSyncInterval)
		{
			SetMode(owner, mode.Width, mode.Height, mode.Format, windowed, refresh, false, false, vSyncInterval);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode, but will not create the stencil buffer, will create/enable a depth buffer and will default the refresh rate to 60Hz.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="mode">Video mode to set.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>		
		/// <param name="vSyncInterval">V-sync interval for presentation.</param>
		public static void SetMode(Forms.Control owner, VideoMode mode, bool windowed, VSyncIntervals vSyncInterval)
		{
			SetMode(owner, mode.Width, mode.Height, mode.Format, windowed, 60, false, false, vSyncInterval);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>
		/// <param name="format">Buffer format for the video mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>
		/// <param name="usestencil">TRUE to create a stencil buffer, FALSE to not create.</param>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, int refresh, bool usedepth, bool usestencil)
		{
			SetMode(owner, width, height, format, windowed, refresh, usedepth, usestencil, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode, but will not create a stencil buffer.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>
		/// <param name="format">Buffer format for the video mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, int refresh, bool usedepth)
		{
			SetMode(owner, width, height, format, windowed, refresh, usedepth, false, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode, but will not create a stencil buffer and will create/enable a depth buffer.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>
		/// <param name="format">Buffer format for the video mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, int refresh)
		{
			SetMode(owner, width, height, format, windowed, refresh, false, false, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode, but will not create the stencil buffer, will create/enable a depth buffer and will default the refresh rate to 60Hz.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>
		/// <param name="format">Buffer format for the video mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed)
		{
			SetMode(owner, width, height, format, windowed, 60, false, false, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="mode">Video mode to set.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>
		/// <param name="usestencil">TRUE to create a stencil buffer, FALSE to not create.</param>
		public static void SetMode(Forms.Control owner, VideoMode mode, bool windowed, int refresh, bool usedepth, bool usestencil)
		{
			SetMode(owner, mode.Width, mode.Height, mode.Format, windowed, refresh, usedepth, usestencil, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode, but will not create a stencil buffer.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="mode">Video mode to set.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>		
		public static void SetMode(Forms.Control owner, VideoMode mode, bool windowed, int refresh, bool usedepth)
		{
			SetMode(owner, mode.Width, mode.Height, mode.Format, windowed, refresh, usedepth, false, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode, but will not create a stencil buffer and will create/enable a depth buffer.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="mode">Video mode to set.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>		
		public static void SetMode(Forms.Control owner, VideoMode mode, bool windowed, int refresh)
		{
			SetMode(owner, mode.Width, mode.Height, mode.Format, windowed, refresh, false, false, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode, but will not create the stencil buffer, will create/enable a depth buffer and will default the refresh rate to 60Hz.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="mode">Video mode to set.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>		
		public static void SetMode(Forms.Control owner, VideoMode mode, bool windowed)
		{
			SetMode(owner, mode.Width, mode.Height, mode.Format, windowed, 60, false, false, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode sized to the control, in windowed mode.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>
		/// <param name="usestencil">TRUE to create a stencil buffer, FALSE to not create.</param>
		public static void SetMode(Forms.Control owner, bool usedepth, bool usestencil)
		{
			SetMode(owner, owner.ClientSize.Width, owner.ClientSize.Height, DesktopVideoMode.Format, true, 60, usedepth, usestencil, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode sized to the control, in windowed mode, with no stencil buffer.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>
		public static void SetMode(Forms.Control owner, bool usedepth)
		{
			SetMode(owner, owner.ClientSize.Width, owner.ClientSize.Height, DesktopVideoMode.Format, true, 60, usedepth,false, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode.
		/// </summary>
		/// <remarks>
		/// This will create a video mode sized to the control, in windowed mode, with no stencil buffer and with a depth buffer.
		/// </remarks>
		/// <param name="owner">Owning control of this object.</param>
		public static void SetMode(Forms.Control owner)
		{
			SetMode(owner, owner.ClientSize.Width, owner.ClientSize.Height, DesktopVideoMode.Format, true, 60, false, false, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to initialize Gorgon.
		/// </summary>
		/// <remarks>This function must be called first.</remarks>
		public static void Initialize()
		{
			Initialize(false, false);
		}

		/// <summary>
		/// Function to initialize Gorgon.
		/// </summary>
		/// <param name="allowBackgroundRender">TRUE to allow rendering when the application loses focus, FALSE to suspend rendering.</param>
		/// <remarks>This function must be called first.</remarks>
		public static void Initialize(bool allowBackgroundRender)
		{
			Initialize(false, allowBackgroundRender);
		}

		/// <summary>
		/// Function to initialize Gorgon.
		/// </summary>
		/// <param name="allowScreenSaver">TRUE to allow the screen saver to run, FALSE to suspend it.</param>
		/// <param name="allowBackgroundRender">TRUE to allow rendering when the application loses focus, FALSE to suspend rendering.</param>
		/// <remarks>This function must be called first.</remarks>
		public static void Initialize(bool allowScreenSaver, bool allowBackgroundRender)
		{
			// Terminate if already initialized.
			if (_initialized)
				Terminate();

			_initialized = true;

			try
			{
				// Open log object.
				// Calling from a mixed mode DLL seems to return null for the entry assembly.
				// That means trouble for our modeller plug-in(s), guess we'll have to use the assembly directory.
				if (Assembly.GetEntryAssembly() != null)
					_log = new Logger("Gorgon", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
				else
					_log = new Logger("Gorgon", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

				// Set this to intermediate, simple or none to have a smaller log file.
#if DEBUG
				_log.LogFilterLevel = LoggingLevel.Verbose;
				//_log.LogFilterLevel = LoggingLevel.Intermediate;
				//_log.LogFilterLevel = LoggingLevel.Simple;
				//_log.UseConsole = true;			
#else
				_log.LogFilterLevel = LoggingLevel.None;
#endif

				try
				{
					_log.Open();
				}
				catch
				{
#if DEBUG
					// By rights, we should never see this error.  Better safe than sorry.
					UI.ErrorBox(null, "Could not create the log file.");
#endif
				}

				Gorgon.Log.Print("Gorgon", "Initializing...", LoggingLevel.Simple);

				Gorgon.Log.Print("Gorgon", "Allow background processing: {0}", LoggingLevel.Verbose, allowBackgroundRender.ToString());
				Gorgon.Log.Print("Gorgon", "Allow screen saver: {0}", LoggingLevel.Verbose, allowScreenSaver.ToString());

				_backgroundProcessing = allowBackgroundRender;
				_allowScreenSaver = allowScreenSaver;
				_running = false;

				_clearTargets = ClearTargets.BackBuffer | ClearTargets.DepthBuffer | ClearTargets.StencilBuffer;

				// Enumerate drivers and video modes.
				_drivers = new DriverList();

				// Create device state object list.
				_deviceStateList = new DeviceStateList();

				// Create plug-in manager.
				_pluginList = new PlugInManager();

				// Create timing data.
				_timer = new PreciseTimer();

				// Create internal image list.
				_images = new ImageManager();

				// Create shader list.
				_shaders = new ShaderManager();

				// Create a message filter to trap screen saver messages.
				_messageFilter = new SysMessageFilter();
				Forms.Application.AddMessageFilter(_messageFilter);

				// Assign the initial driver, this will also create the renderer..
				Driver = _drivers[0];

				// Create state manager.
				_stateManager = new StateManager(_renderer);

			    // Create the file system manager.
				_fileSystems = new FileSystemManager();

				_log.Print("Gorgon", "Initialized Successfully.", LoggingLevel.Simple);
			}
			catch (SharpException sEx)
			{
				_initialized = false;
				throw sEx;
			}
			catch (Exception ex)
			{
				_initialized = false;
				throw ex;
			}
		}

		/// <summary>
		/// Function to terminate the Gorgon object.
		/// </summary>
		/// <remarks>You must call this when finished with Gorgon.</remarks>
		public static void Terminate()
		{
			// If the engine wasn't initialized, do nothing.
			if (!_initialized)
				return; 

			// Stop the engine.
			Stop();

			Forms.Application.RemoveMessageFilter(_messageFilter);

			if (_fileSystems != null)
				_fileSystems.Dispose();

			if (_stateManager != null)
				_stateManager.Dispose();

			if (_images != null)
				_images.Dispose();

			if (_shaders != null)
				_shaders.Dispose();

			if (_pluginList != null)
				_pluginList.Clear();

			if (_renderer != null)
				_renderer.Dispose();

			if (_deviceStateList != null)
				_deviceStateList.Dispose();

			_log.Print("Gorgon","Shutting down.", LoggingLevel.Simple);

			// Destroy log.
			if (_log != null)
				_log.Dispose();

			_images = null;
			_stateManager = null;
			_fileSystems = null;
			_input = null;
			_shaders = null;
			_timer = null;
			_pluginList = null;
			_log = null;
			_renderer = null;
			_initialized = false;
			_deviceStateList = null;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		static Gorgon()
		{
#if DEBUG
			// This is here just for house keeping purposes, you may get rid of it
			// if you so require.  I just don't like running from the GAC with a
			// debug assembly, especially if they're littering log files everywhere.
			if (Assembly.GetExecutingAssembly().GlobalAssemblyCache)
				throw new IllegalOperationException("This is a DEBUG assembly, please do not use it from the GAC.\n\nThanks,\nThe Mgmt.", null);
#endif
		}
		#endregion
    }
}
