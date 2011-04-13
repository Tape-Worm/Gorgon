#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Wednesday, April 27, 2005 10:29:58 AM
// 
#endregion

using System;
using System.Drawing;
using System.Linq;
using Forms = System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;
using D3D9 = SlimDX.Direct3D9;
using DX = SlimDX;
using GorgonLibrary.FileSystems;
using GorgonLibrary.Internal;
using GorgonLibrary.Internal.Native;
using GorgonLibrary.Graphics;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary
{
	/// <summary>
	/// Enumeration containing platform IDs.
	/// </summary>
	public enum PlatformID
	{
		/// <summary>Unknown platform.</summary>
		Unknown = 0,
		/// <summary>x86 32-bit architecture.</summary>
		x86 = 1,
		/// <summary>x64 64-bit architecture.</summary>
		x64 = 2
	}

	/// <summary>
	/// The primary interface into gorgon.
	/// </summary>
	/// <remarks>This interface handles the initialization of Gorgon from internal data structures to the video mode to be used.  Users should call <see cref="M:GorgonLibrary.Gorgon.Initialize">Gorgon.Initialize</see> before doing anything
	/// and call <see cref="M:GorgonLibrary.Gorgon.Terminate">Gorgon.Terminate</see> when finished.<para>This static class is used to change the global states of objects such as a global rendering setting to which render target is current.
	/// It will also control the execution and rendering flow for the application.
	/// </para></remarks>
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
					long wParamValue = 0;
					if (IntPtr.Size == 4)
						wParamValue = m.WParam.ToInt32() & 0xFFF0;
					else
						wParamValue = m.WParam.ToInt64() & 0xFFF0;

					switch ((SysCommands)(wParamValue))
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
		public static event DriverChangingHandler DriverChanging;
		/// <summary>Event fired when a driver has changed.</summary>
		public static event DriverChangedHandler DriverChanged;
		/// <summary>Event fired when the device has been reset from a lost state.</summary>
		/// <example>
		/// An example of handling a resize of a render target after the window has been resized:
		/// <code>
		/// RenderImage myRenderTarget = null;	// Our render target.
		/// 
		/// public void DeviceReset(object sender, EventArgs e)
		/// {
		///    // Reize our render target to match the new dimensions of the screen.
		///    myRenderTarget.SetDimensions(Gorgon.Screen.Width, Gorgon.Screen.Height);
		/// }
		/// 
		/// public void Main()
		/// {
		///    // Do stuff, etc...
		/// 
		///    // Create a render target that is the same size as our screen.
		///    myRenderTarget = RenderTarget.CreateRenderImage("MyTarget", Gorgon.Screen.Width, Gorgon.Screen.Height)
		///    Gorgon.DeviceReset += new EventHandler(DeviceReset);
		/// }
		/// </code>
		/// </example>
		/// <remarks>
		/// A number of events can put the device into a lost state.  For example, resizing the control that is bound to the primary render window.
		/// <para>Some objects will need to be re-created during a device loss/reset event.  In this event you put in the code to rebuild the object that needs to be reset.  
		/// </para>
		/// </remarks>
		public static event EventHandler DeviceReset;
		/// <summary>Event fired when the device has been put into a lost state.</summary>
		/// <remarks>
		/// A number of events can put the device into a lost state.  For example, resizing the control that is bound to the primary render window.
		/// <para>Some objects will need to be re-created during a device loss/reset event.  In this event you put in clean up code to remove data for objects that need to be reset.</para>
		/// </remarks>		
		public static event EventHandler DeviceLost;
		/// <summary>
		/// Event fired when rendering begins for a new frame.
		/// </summary>
		/// <remarks>This event is where all the application logic and drawing should take place.  It is called once per frame and will return frame statistics to help with things like keeping sprite transformation speed independant of the processor speed.</remarks>
		public static event FrameEventHandler Idle;
		#endregion

		#region Variables.
		private static Logger _log;										// Log file.
		private static SysMessageFilter _messageFilter;					// Windows message filter.
		private static PreciseTimer _timer;								// Main Gorgon timer.
		private static DriverList _drivers = null;						// List of video drivers for the system.
		private static Driver _currentDriver = null;					// Current driver.
		private static Renderer _renderer = null;						// Interface to the renderer.
		private static FrameEventArgs _frameEventArgs = null;			// Frame event arguments.
		private static VideoMode _desktopVideoMode;						// Current video mode of the desktop.
		private static SpriteStateCache _stateCache = null;				// Sprite state cache.
		private static RenderTarget[] _currentTarget = null;			// Currently active render target(s).
		private static ClearTargets _clearTargets;						// Target buffers to clear.
		private static Viewport _clippingView = null;					// Clipping viewport.
		private static double _targetFrameTime = 0.0;					// Target frame time.
		private static IShaderRenderer _currentShader = null;			// Current shader.
#if INCLUDE_D3DREF
		private static bool _refDevice;									// Flag to indicate if we're using a reference device or HAL device.
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
				if (Screen == null)
					return true;

				if ((Screen.OwnerForm.WindowState == Forms.FormWindowState.Minimized) || ((!Screen.OwnerForm.ContainsFocus) && (!Screen.Windowed)))
					return false;

				if ((!AllowBackgroundRendering) && (!Screen.OwnerForm.ContainsFocus))
					return false;
	
				return true;
			}
		}

		/// <summary>
		/// Property to return the Direct 3D interface.
		/// </summary>
		internal static D3D9.Direct3D Direct3D
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the renderer interface.
		/// </summary>
		internal static Renderer Renderer
		{
			get
			{
				if (!IsInitialized)					
					throw new GorgonException(GorgonErrors.NotInitialized);

				return _renderer;
			}
		}

		/// <summary>
		/// Property to return the platform that this instance of Gorgon was compiled for.
		/// </summary>
		/// <remarks>When the library is compiled for 64-bit processors, then this will read x64, otherwise it'll be x86.  If the platform cannot be determined it will return unknown.</remarks>
		public static PlatformID Platform
		{
			get
			{
				// Set the platform ID here.
                switch (IntPtr.Size)
                {
                    case 4:
                        return PlatformID.x86;
                    case 8:
                        return PlatformID.x64;
                    default:
                        return PlatformID.Unknown;
                }
			}
		}

		/// <summary>
		/// Property to set or return the frame statistics text color.
		/// </summary>
		/// <remarks>This only applies if <see cref="GorgonLibrary.Gorgon.FrameStatsVisible">FrameStatsVisible</see> is TRUE.</remarks>
		/// <value>The color of the text for the frame statistics.</value>
		public static Color FrameStatsTextColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether or not the resizing the window will reset the display.
		/// </summary>
		/// <remarks>
		/// When TRUE, the contents of the window client area will be stretched to match the size of the resized client area.  However, no actual change to the <see cref="GorgonLibrary.VideoMode">video mode</see> will be made.  If it was set to 640x480 before the resizing, the screen dimensions after resizing the window will still be 640x480.  This will produce a blurry looking image when resized larger than the previous size.  When fast resizing is enabled, all resources are preserved because there was no actual change to the video device.
		/// <para>If FALSE, then the device will undergo a reset operation and the video mode will be adjusted to match the new client size of the owning control/window.  This will keep the sharpness of the rendered image regardless of the client size of the control/window. The caveat is that some resources will need to be re-loaded/re-created when the device reset occurs.  This is the exact behaviour of the system when a full screen application loses and regains focus.  Furthermore, window resizing will be much slower (hence the "Fast" portion of FastResize). </para>
		/// </remarks>
		/// <value>TRUE will enable a fast resizing, FALSE will disable it.</value>
		public static bool FastResize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the logo is shown or not.
		/// </summary>
		/// <value>TRUE to show the Gorgon logo in the lower right corner of the screen while rendering, FALSE to hide.</value>
		public static bool LogoVisible
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the frame timing data is shown or not.
		/// </summary>
		/// <value>TRUE to show the current frame statistics (i.e. Frames Per Second, frame delta time, etc...) in the upper left corner of the screen while rendering.  FALSE to hide.</value>
		public static bool FrameStatsVisible
		{
			get;
			set;
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
				return _clearTargets;
			}
			set
			{
				if ((value & ClearTargets.None) > 0)
					_clearTargets = ClearTargets.None;
				else
					_clearTargets = value;
			}
		}

		/// <summary>
		/// Property to return the render state cache for renderables.
		/// </summary>
		/// <example>
		/// To turn off blending for all sprites except the sprite named Bob we can do the following:
		/// <code>
		/// Sprite Bob;
		/// Sprite Joe;
		/// Sprite Blah;
		/// 
		/// // ... Time passes ... stuff is done, a grue eats someone, etc...
		/// 
		/// Gorgon.GlobalStateSettings.GlobalBlending = BlendingModes.None;
		/// // Keep the normal blending for the Bob sprite.
		/// Bob.BlendingMode = BlendingModes.Normal;
		/// </code>
		/// </example>
		/// <remarks>Changing the states in this property will result in that state being applied to all renderable objects.  The exception to this is when the state has been changed directly on the renderable object itself.</remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Gorgon.Initialize()</see> has not been called.</exception>
		/// <value>A list of global settings that <see cref="GorgonLibrary.Graphics.Sprite">sprites</see> and <see cref="GorgonLibrary.Graphics.TextSprite">text sprites</see> will use as initial and inherited values.</value>		
		public static SpriteStateCache GlobalStateSettings
		{
			get
			{
				if (!IsInitialized)
					throw new GorgonException(GorgonErrors.NotInitialized);

				return _stateCache;
			}
		}

		/// <summary>
		/// Property to set or return the currently active clipping viewport.
		/// </summary>
		/// <remarks>
		/// Use this property to clip any drawing outside of the rectangle region specified.  
		/// <para>Setting this property to NULL will set the clipping viewport dimensions to the size of the current render target dimensions.</para>
		/// </remarks>
		/// <value>A rectangular region that defines what area of the screen (or other render target) to update.</value>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Gorgon.Initialize()</see> has not been called.</exception>
		public static Viewport CurrentClippingViewport
		{
			get
			{
				return _clippingView;
			}
			set
			{
				if ((!IsInitialized) || (Screen == null))
					throw new GorgonException(GorgonErrors.NotInitialized);

				// Reset the view to the current target view.
				if (value == null)
					value = _currentTarget[0].DefaultView;

				// Don't bother to set the same clipping view.
				if ((_clippingView == value) && (!_clippingView.Updated))
					return;

				// Flush the renderer.
				_renderer.Render();

				// If we can support scissor testing, then use that.
				if (_currentDriver.SupportScissorTesting) 
				{
					// Set the scissor rectangle.
					if ((value == null) || (value == _currentTarget[0].DefaultView))
					{						
						_renderer.RenderStates.ScissorTesting = false;
						_clippingView = _currentTarget[0].DefaultView;
					}
					else
					{
						_renderer.RenderStates.ScissorTesting = true;
						_renderer.RenderStates.ScissorRectangle = value.ClippedDimensions;
						_clippingView = value;
					}
				}
				else
				{
					// If we don't support scissor testing, then we need to use the viewport
					// to clip the screen.
					if ((value == null) || (value == _currentTarget[0].DefaultView))
					{
						_renderer.CurrentProjectionMatrix = _currentTarget[0].ProjectionMatrix;
						_renderer.CurrentViewport = _currentTarget[0].DefaultView;
						_clippingView = _currentTarget[0].DefaultView;
					}
					else
					{
						_renderer.CurrentProjectionMatrix = value.ProjectionMatrix;
						_renderer.CurrentViewport = value;
						_clippingView = value;
					}
				}

				_clippingView.Updated = false;
			}
		}

		/// <summary>
		/// Property to return information about the current video mode.
		/// </summary>
		/// <remarks>If a video mode has not been set, and <see cref="M:GorgonLibrary.Gorgon.Initialize">Gorgon.Initialize()</see> has been called, then the current desktop video mode is returned instead.</remarks>
		/// <value>The currently active <see cref="VideoMode">video mode</see> information.</value>
		public static VideoMode CurrentVideoMode
		{
			get
			{
				if (Screen != null)
					return Screen.Mode;
				else
					return _desktopVideoMode;
			}
		}

		/// <summary>
		/// Property to set or return the currently active shader.
		/// </summary>
		/// <remarks>Use this to apply a shader to the rendering pass.  You can apply either a <see cref="GorgonLibrary.Graphics.Shader"/>, <see cref="GorgonLibrary.Graphics.ShaderTechnique"/> or a <see cref="GorgonLibrary.Graphics.ShaderPass"/>.  
		/// When applying a shader there's a very small performance hit on the first pass of rendering as it attempts to locate the first valid shader technique.</remarks>
		/// <value>A shader renderer output to apply to the scene when rendering.</value>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Gorgon.Initialize()</see> has not been called.
		/// <para>Thrown when <see cref="GorgonLibrary.Gorgon.SetMode(System.Windows.Forms.Control, Int32, Int32, GorgonLibrary.BackBufferFormats, Boolean, Boolean, Boolean, Int32, GorgonLibrary.VSyncIntervals)">Gorgon.SetMode()</see> has not been called.</para>
		/// </exception>
		public static IShaderRenderer CurrentShader
		{
			get
			{
				return _currentShader;
			}
			set
			{
				if (!IsInitialized)
					throw new GorgonException(GorgonErrors.NotInitialized);

				// No device?  Throw an exception.
				if (Screen == null)
					throw new GorgonException(GorgonErrors.NoDevice);
								
				if (_currentShader == value)
					return;

				// Force a flush of the rendering pipeline whenever we change this.
				Renderer.Render();
				
				_currentShader = value;
			}
		}

		/// <summary>
		/// Property to set or return the currently active render target.
		/// </summary>
		/// <remarks>
		/// Use this to change where sprites, primitives and text will be drawn.  This will allow for objects to be drawn off-screen or into another control that's bound with a <see cref="GorgonLibrary.Graphics.RenderWindow">RenderWindow</see>.
		/// <para>
		/// Set this property to NULL to continue drawing to the primary screen.</para>
		/// 	<para>Please note that when the render target is switched the <see cref="GorgonLibrary.Gorgon.CurrentClippingViewport">clipping viewport</see> is reset to the size of the render target being assigned.</para>
		/// </remarks>
		/// <seealso cref="GorgonLibrary.Gorgon.SetAdditionalRenderTarget">SetAdditionalRenderTarget</seealso>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Gorgon.Initialize()</see> has not been called.<para>
		/// Thrown when <see cref="GorgonLibrary.Gorgon.SetMode(System.Windows.Forms.Control, Int32, Int32, GorgonLibrary.BackBufferFormats, Boolean, Boolean, Boolean, Int32, GorgonLibrary.VSyncIntervals)">Gorgon.SetMode()</see> has not been called.</para></exception>
		public static RenderTarget CurrentRenderTarget
		{
			get
			{
				return _currentTarget[0];
			}
			set
			{
				if (value == null)
					value = Screen;

				SetAdditionalRenderTarget(0, value);
			}
		}

		/// <summary>
		/// Property to return the primary rendering window.
		/// </summary>
		/// <remarks>
		/// There is only one of these <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> objects active at any given time.  Other controls that have render targets use <see cref="GorgonLibrary.Graphics.RenderWindow">RenderWindow</see> objects.
		/// <para>When the primary video mode bound to the control is closed, then all other render targets are automatically destroyed.</para>
		/// </remarks>
		/// <value>The primary rendering window or the "Screen".  This can be any control and is the primary render target that is setup during the <see cref="GorgonLibrary.Gorgon.SetMode(System.Windows.Forms.Control, Int32, Int32, GorgonLibrary.BackBufferFormats, Boolean, Boolean, Boolean, Int32, GorgonLibrary.VSyncIntervals)">SetMode()</see> function.  As such, this is the initial render target when a video mode is set.</value>
		public static PrimaryRenderWindow Screen
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the video mode information for the desktop.
		/// </summary>
		/// <remarks></remarks>
		/// <value>The desktop <see cref="VideoMode">video mode</see> information.</value>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Gorgon.Initialize()</see> has not been called.</exception>
		public static VideoMode DesktopVideoMode
		{
			get
			{
				if (!IsInitialized)
					throw new GorgonException(GorgonErrors.NotInitialized);

				return _desktopVideoMode;
			}
		}

		/// <summary>
		/// Property to return the current frame timing stats.
		/// </summary>
		/// <remarks>This is usually used for internal purposes, please use the <see cref="GorgonLibrary.Graphics.FrameEventArgs">FrameEventArgs</see> timing information in the <see cref="M:GorgonLibrary.Idle">Idle event</see> instead.</remarks>
		/// <value>Returns a <see cref="GorgonLibrary.TimingData">TimingData</see> object containing information about the most current frame statistics.</value>
		public static TimingData FrameStats
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the library has been initialized or not.
		/// </summary>
		/// <value>TRUE if <see cref="M:GorgonLibrary.Gorgon.Initialize">Initialize()</see> has been called, FALSE if not.</value>
		public static bool IsInitialized
		{
			get;
			private set;
		}

#if INCLUDE_D3DREF
		/// <summary>
		/// Property to return whether we're using the Direct 3D reference device.
		/// </summary>
		/// <returns>TRUE if we're using a Direct 3D reference device, FALSE if not (HAL).</returns>
		/// <remarks>
		/// This is *ONLY* available when the library is compiled with INCLUDE_D3DREF.
		/// <para>The Direct X SDK MUST be installed to use this device.</para>
		/// 	<para>It should also be noted that the reference device is extremely slow and is only useful for diagnosing issues that appear in the HAL device.</para>
		/// </remarks>
		/// <value>TRUE to use the Direct 3D reference device, FALSE to exclude it.</value>
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
		/// <remarks>Usage of this property will require the SharpUtilities library and the SharpUtilities.Utility namespace.</remarks>
		/// <value>The internal logging interface used by the library.</value>
		public static Logger Log
		{
			get
			{
				return _log;
			}
		}

		/// <summary>
		/// Property to set or return the currently active driver.
		/// </summary>
		/// <remarks>
		/// Setting this property to another driver index will make the video mode reset and all data uploaded to the video card (such as textures) will be destroyed and will have to be reset after the video mode has been set again.
		/// <para>The reason it does this is due to the uncertainty of using multiple video cards: If video card A has only 640x480x16 and video card B has 800x600x32 and 640x480x32, and we are currently using B, then upon switching to A we have to allow the user to change to a video mode that is supported by card A.</para>
		/// 	<para>
		/// After the driver ID is changed, it is up to the client program to determine if the mode is supported and reset to that mode, else set to a default video mode.
		/// </para>
		/// 	<para>
		/// You should confirm how many drivers are installed in the system via the <see cref="DriverList">Drivers</see>.Count property before setting the driver index.
		/// </para>
		/// </remarks>
		/// <value>This will get/set the current video driver index, which is ranged from 0 to <see cref="GorgonLibrary.DriverList">DriverList</see>.Count - 1.</value>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Gorgon.Initialize()</see> has not been called.</exception>
		public static Driver CurrentDriver
		{
			get
			{
				if (!IsInitialized)
					throw new GorgonException(GorgonErrors.NotInitialized);

				return _currentDriver;
			}
			set
			{
				if (!IsInitialized)
					throw new GorgonException(GorgonErrors.NotInitialized);

				Stop();

				// Don't set the same driver twice.
				if ((value != null) && (value == _currentDriver))
					return;

				DriverChangingArgs changingArgs = new DriverChangingArgs(_currentDriver, value);

				Gorgon.OnDriverChanging(changingArgs);

				// Don't allow change if we cancel.
				if (changingArgs.Cancel)
					return;

				// Remove all buffers.
				if ((_currentDriver != null) && (_renderer != null))
					DeviceStateList.ForceRelease();

				// Remove all fonts.
				foreach (GorgonLibrary.Graphics.Font font in FontCache.Fonts)
					font.ReleaseResources();

				Gorgon.Log.Print("Gorgon", "Changing video drivers...", LoggingLevel.Simple);

				// Do actual driver change.
				if ((_renderer != null) && (!_renderer.IsDisposed))
				{
					if (_stateCache != null)
						_stateCache.Dispose();

					_renderer.Dispose();
				}
				_renderer = null;
				_stateCache = null;

				// Retrieve the desktop video mode for the driver.
				_desktopVideoMode = new VideoMode();
				_desktopVideoMode.Width = Gorgon.Direct3D.Adapters[value.DriverIndex].CurrentDisplayMode.Width;
				_desktopVideoMode.Height = Gorgon.Direct3D.Adapters[value.DriverIndex].CurrentDisplayMode.Height;
				_desktopVideoMode.RefreshRate = Gorgon.Direct3D.Adapters[value.DriverIndex].CurrentDisplayMode.RefreshRate;
				_desktopVideoMode.Format = Converter.Convert(Gorgon.Direct3D.Adapters[value.DriverIndex].CurrentDisplayMode.Format);

				_currentDriver = value;

				// Recreate renderer.
				_renderer = new Renderer();
				_stateCache = new SpriteStateCache();
				_currentTarget = new RenderTarget[_currentDriver.MaximumSimultaneousRenderTargets];				

				Gorgon.Log.Print("Gorgon", "Driver changed to {0} ({1}).", LoggingLevel.Simple, value.Description, value.DriverName);

				Gorgon.OnDriverChanged(new DriverChangedArgs(_currentDriver, value));
			}
		}

		/// <summary>
		/// Property to return a list of video drivers available for rendering.
		/// </summary>
		/// <remarks>If a driver does not have hardware acceleration, it will not be included in the list.</remarks>
		/// <value>The list of installed <see cref="GorgonLibrary.Driver">video drivers</see>.</value>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Gorgon.Initialize()</see> has not been called.</exception>
		public static DriverList Drivers
		{
			get
			{
				if (!IsInitialized)
					throw new GorgonException(GorgonErrors.NotInitialized);

				return _drivers;
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
			get;
			set;
		}

		/// <summary>
		/// Property to return if the app is in a running state or not.
		/// </summary>
		/// <remarks>This flag is set to TRUE when the <see cref="GorgonLibrary.Gorgon.Go">Go()</see> function is called and FALSE when the <see cref="GorgonLibrary.Gorgon.Stop">Stop()</see> function is called.</remarks>
		/// <value>TRUE if the application is running, and FALSE if not.</value>
		public static bool IsRunning
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether we want to allow a screensaver to run or not.
		/// </summary>
		/// <remarks>
		/// When this property is set to TRUE all power management/screen saver functionality will be suspended while the application is running.  
		/// <para>
		/// Due to an unknown reason (presumably this is for security purposes) in Windows, when the screensaver is set to return to the login/welcome screen after its deactivated the application will NOT suspend the screensaver/power management.  There currently is no workaround for this, however if you know of one, send the author an email explaining how to circumvent this.
		/// </para>
		/// </remarks>
		/// <value>Set this property to TRUE if you wish to allow the screensaver/power management to kick in.  Set to FALSE if you want to suspend the screensaver/power management.</value>
		public static bool AllowScreenSaver
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the minimum amount of time to wait between frames (in milliseconds).
		/// </summary>
		/// <example>
		/// The following code will limit the frame rate to 30 FPS:
		/// <code>
		/// Gorgon.MinimumFrameTime = PreciseTimer.FpsToMilliseconds(30);
		/// </code>
		/// </example>
		/// <remarks>
		/// This can be used as a frame rate limiting mechanism.  By stalling for the appropriate number of milliseconds before the frame is rendered the frame rate can be kept at a desired level.
		/// <para>The static function <see cref="GorgonLibrary.PreciseTimer.FpsToMilliseconds">PreciseTimer.FpsToMilliseconds</see> can be used to determine how many milliseconds are required for a particular frame rate.</para>
		/// </remarks>
		/// <value>This number will pause the renderer for the number of milliseconds requested before rendering the frame.  The lowest value that can be set is 0.001.</value>
		public static double MinimumFrameTime
		{
			get
			{
				return _targetFrameTime;
			}
			set
			{
				if (value < 0.0)
					value = 0.0;

				_targetFrameTime = value;
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
			while ((AppIdle) && (IsRunning))
			{
				if (HasFocus)
				{
					// Update the screen.
					if (FrameStats.Update())
					{
						// Call idle event.					
						OnIdle(sender, _frameEventArgs);

						if ((Screen != null) && (_currentTarget != null))
						{
							for (int i = 0; i < _currentTarget.Length; i++)
							{
								if (_currentTarget[i] != null)
									_currentTarget[i].Update();
							}

							// Give up some time if we don't have focus and we're windowed.
							if ((!Screen.OwnerForm.ContainsFocus) && (Screen.Windowed))
								System.Threading.Thread.Sleep(10);
						}
					}
				}
				else
					break;
			}
		}

		/// <summary>
		/// Function to trigger driver changing event.
		/// </summary>
		/// <param name="e">Parameters for event.</param>
		internal static void OnDriverChanging(DriverChangingArgs e)
		{
			if (DriverChanging != null)
				DriverChanging(_drivers, e);
		}

		/// <summary>
		/// Function to trigger driver changing event.
		/// </summary>
		/// <param name="e">Parameters for event.</param>
		internal static void OnDriverChanged(DriverChangedArgs e)
		{
			if (DriverChanged != null)
				DriverChanged(_drivers, e);
		}

		/// <summary>
		/// Function to trigger the idle event.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Frame arguments.</param>
		internal static void OnIdle(object sender, FrameEventArgs e)
		{
			if (Idle != null)
				Idle(sender, e);
		}

		/// <summary>
		/// Function to call the device lost event.
		/// </summary>
		internal static void OnDeviceLost()
		{
			if (DeviceLost != null)
				DeviceLost(_renderer, EventArgs.Empty);

			for (int i = 0; i < _currentTarget.Length; i++)
				_currentTarget[i] = null;

			_clippingView = null;
			DeviceStateList.DeviceWasLost();
			_timer.Reset();
			FrameStats.Reset();

			Gorgon.Log.Print("Gorgon", "Device has been put into a lost state.", LoggingLevel.Intermediate);
		}

		/// <summary>
		/// Functio to call the device reset event.
		/// </summary>
		internal static void OnDeviceReset()
		{
			_renderer.Reset();
			_timer.Reset();
			FrameStats.Reset();

			if (DeviceReset != null)
				DeviceReset(_renderer, EventArgs.Empty);

			Gorgon.Log.Print("Gorgon", "Device has been reset.", LoggingLevel.Intermediate);
		}

		/// <summary>
		/// Function to determine if a render target is active.
		/// </summary>
		/// <param name="target">Target to check.</param>
		/// <returns>TRUE if the render target is active, FALSE if not.</returns>
		internal static bool IsRenderTargetActive(RenderTarget target)
		{
			return _currentTarget.Count(isTarget => target == isTarget) > 0;
		}

		/// <summary>
		/// Function to set additional render targets.
		/// </summary>
		/// <param name="index">An index value from 0 to <see cref="GorgonLibrary.Driver.MaximumSimultaneousRenderTargets">Driver.MaximumSimultaneousRenderTargets</see>-1.</param>
		/// <param name="target">A render target to bind to the index.</param>
		/// <remarks>This will allow the user to set more than one render target at a time for simultaneous rendering.  Please note that using multiple render targets is only supported when
		/// using shaders.
		/// <para>An index of 0 will set the current primary render target which is identical to using <see cref="GorgonLibrary.Gorgon.CurrentRenderTarget">Gorgon.CurrentRenderTarget</see>.</para>
		/// <para>There are some limitations to setting multiple render targets:  The render targets should have the same width and height.  They can use a different 
		/// <see cref="GorgonLibrary.Graphics.ImageBufferFormats">image format</see> but the image formats of the targets must have the same bit count.  This restriction is lifted for 
		/// devices that have <see cref="GorgonLibrary.Driver.SupportMRTIndependentBitDepths">Driver.SupportMRTIndepenedentBitDepths</see> set to true.</para>
		/// <para>If <see cref="GorgonLibrary.Driver.SupportMRTPostPixelShaderBlending">Driver.SupportMRTPostPixelShaderBlending</see> is true then post pixel shader blending operations can 
		/// be performed by the video card.  However, if it is supported, the render target must queried to see if it will support post pixel shader blending operations via 
		/// the <see cref="GorgonLibrary.Graphics.RenderTarget.IsValidForMRTPostPixelShaderBlending">RenderTarget.IsValidForMRTPostPixelShaderBlending</see> property.</para>
		/// <para>Passing NULL (Nothing in Visual Basic) with an index of 0 will set the render target to the <see cref="GorgonLibrary.Gorgon.Screen">Gorgon.Screen</see> render target.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Gorgon.Initialize</see> has not been called, 
		/// <see cref="GorgonLibrary.Gorgon.SetMode(System.Windows.Forms.Control, Int32, Int32, GorgonLibrary.BackBufferFormats, Boolean, Boolean, Boolean, Int32, GorgonLibrary.VSyncIntervals)">Gorgon.SetMode</see> wasn't called or the target cannot be bound.</exception>
		public static void SetAdditionalRenderTarget(int index, RenderTarget target)
		{
			if (!IsInitialized)
				throw new GorgonException(GorgonErrors.NotInitialized);

			// No device?  Throw an exception.
			if (Screen == null)
				throw new GorgonException(GorgonErrors.NoDevice);

			if (index > _currentTarget.Length)
				throw new GorgonException(GorgonErrors.CannotBindTarget, "The index of the target exceeds the maximum number of simultaneous render targets for the system (" + _currentDriver.MaximumSimultaneousRenderTargets.ToString() + ").");

			if (index < 0)
				throw new GorgonException(GorgonErrors.CannotBindTarget, "The index of the target must be greater than zero.");

			if (!Screen.DeviceNotReset)
			{
				// Force a flush to the renderer.
				if (index == 0)
					_renderer.Render();

				// Force scissor testing to off.
				if ((_currentDriver.SupportScissorTesting) && (_renderer.RenderStates.ScissorTesting))
					_renderer.RenderStates.ScissorTesting = false;

				if ((index == 0) && (target == null))
					target = Screen;

				if (target != _currentTarget[index])
				{
					if (target != null)
						Screen.Device.SetRenderTarget(index, target.SurfaceBuffer);
					else
						Screen.Device.SetRenderTarget(index, null);

					if (index == 0)
						Screen.Device.DepthStencilSurface = target.DepthBuffer;

					if (target != null)
						target.Refresh();
					_currentTarget[index] = target;

					// Reset the view ports.
					_renderer.CurrentViewport = _currentTarget[0].DefaultView;
					_renderer.CurrentProjectionMatrix = _currentTarget[0].ProjectionMatrix;

					// Change the clipper to the render target dimensions.
					CurrentClippingViewport = _currentTarget[0].DefaultView;
				}
			}
		}

		/// <summary>
		/// Function to return an additional render target from the active render target list.
		/// </summary>
		/// <param name="index">An index value from 0 to <see cref="GorgonLibrary.Driver.MaximumSimultaneousRenderTargets">Driver.MaximumSimultaneousRenderTargets</see>-1.</param>
		/// <returns>The render target at the specified index.</returns>
		public static RenderTarget GetAdditionalRenderTarget(int index)
		{
			return _currentTarget[index];
		}

		/// <summary>
		/// Function to close the currently active video mode.
		/// </summary>
		/// <remarks>Some resources may have to be re-loaded or re-created when this function is called and another call to SetMode is made.</remarks>
		public static void CloseMode()
		{
			Stop();

			// Clean up resources.
			FontCache.DestroyAll();
			RenderTargetCache.DestroyAll();
			ShaderCache.DestroyAll();
			ImageCache.DestroyAll();

			// Destroy anything that's not tracked.
			DeviceStateList.ForceRelease();

			if (Screen != null)
				Screen.Dispose();
			Screen = null;
		}

		/// <summary>
		/// Function to set the video mode and primary rendering window.
		/// </summary>
		/// <remarks>
		/// When this function is successful it will create a new <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and assign it to the <see cref="GorgonLibrary.Gorgon.Screen">Screen</see> property, and will bind the primary render target to its owner control.
		/// <para>If the client size of the owning control does not match the width and height passed in to the function and the windowed parameter is TRUE, and the owning control is a Form, then the client size of the form will be adjusted to match that of the width and height passed in.  If the owning control is not a form, then the width and height will adjusted to the client size of the owning control.</para>
		/// 	<para>When in windowed mode, the vsync parameter and refresh rate parameter will be ignored.</para>
		/// 	<para>When going to full screen mode the width and height must match one of the video modes listed in the <see cref="GorgonLibrary.Driver.VideoModes">CurrentDriver.VideoModes</see> list.  If not, an exception will be thrown.</para>
		/// </remarks>
		/// <param name="owner">Control that will be bound to the <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and will be the initial canvas to receive drawing commands.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>
		/// <param name="format">Buffer format for the video mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>
		/// <param name="usestencil">TRUE to create a stencil buffer, FALSE to not create.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>
		/// <param name="vSyncInterval">V-sync interval for presentation.</param>
		/// <exception cref="ArgumentNullException">Thrown when NULL is passed in for the owner parameter.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when a video device object could not be created.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when full screen mode is chosen and the video mode does not match a video mode on the <see cref="GorgonLibrary.Driver.VideoModes">CurrentDriver.VideoModes</see> list.  Can also be thrown if the desktop video mode does not support hardware acceleration.</exception>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, bool usedepth, bool usestencil, int refresh, VSyncIntervals vSyncInterval)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			// Force the application to stop.
			Gorgon.Stop();

			// Create the video mode if we haven't set the primary window, otherwise
			// just reset the video device object.
			if (Screen == null)
			{
				Screen = new PrimaryRenderWindow(owner);
				Screen.SetMode(new VideoMode(width, height, refresh, format), windowed, usedepth, usestencil, false, vSyncInterval);
			}
			else
				Screen.SetMode(new VideoMode(width, height, refresh, format), windowed, usedepth, usestencil, true, vSyncInterval);

			// Reset the buffers.
			Geometry.UpdateVertexData(GlobalStateSettings.MaxSpritesPerBatch * 4);

			// Reset the target.
			CurrentRenderTarget = Screen;
			CurrentRenderTarget.DefaultView.Refresh(CurrentRenderTarget);
			CurrentClippingViewport = null;
		}

		/// <summary>
		/// Function to set the video mode and primary rendering window.
		/// </summary>
		/// <remarks>
		/// When this function is successful it will create a new <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and assign it to the <see cref="GorgonLibrary.Gorgon.Screen">Screen</see> property, and will bind the primary render target to its owner control.
		/// <para>If the client size of the owning control does not match the width and height passed in to the function and the windowed parameter is TRUE, and the owning control is a Form, then the client size of the form will be adjusted to match that of the width and height passed in.  If the owning control is not a form, then the width and height will adjusted to the client size of the owning control.</para>
		/// 	<para>When in windowed mode, the vsync parameter and refresh rate parameter will be ignored.</para>
		/// 	<para>When going to full screen mode the width and height must match one of the video modes listed in the <see cref="GorgonLibrary.Driver.VideoModes">CurrentDriver.VideoModes</see> list.  If not, an exception will be thrown.</para>
		/// <para>This overload of the function will default to <see cref="GorgonLibrary.VSyncIntervals">VSyncIntervals.None</see>.</para>
		/// </remarks>
		/// <param name="owner">Control that will be bound to the <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and will be the initial canvas to receive drawing commands.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>		
		/// <param name="format">Buffer format for the video mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>		
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>
		/// <param name="usestencil">TRUE to create a stencil buffer, FALSE to not create.</param>
		/// <param name="refresh">Refresh rate of the video mode.</param>
		/// <exception cref="ArgumentNullException">Thrown when NULL is passed in for the owner parameter.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when a video device object could not be created.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when full screen mode is chosen and the video mode does not match a video mode on the <see cref="GorgonLibrary.Driver.VideoModes">CurrentDriver.VideoModes</see> list.  Can also be thrown if the desktop video mode does not support hardware acceleration.</exception>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, bool usedepth, bool usestencil, int refresh)
		{
			SetMode(owner, width, height, format, windowed, usedepth, usestencil, refresh, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode and primary rendering window.
		/// </summary>
		/// <remarks>
		/// When this function is successful it will create a new <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and assign it to the <see cref="GorgonLibrary.Gorgon.Screen">Screen</see> property, and will bind the primary render target to its owner control.
		/// <para>If the client size of the owning control does not match the width and height passed in to the function and the windowed parameter is TRUE, and the owning control is a Form, then the client size of the form will be adjusted to match that of the width and height passed in.  If the owning control is not a form, then the width and height will adjusted to the client size of the owning control.</para>
		/// 	<para>When going to full screen mode the width and height must match one of the video modes listed in the <see cref="GorgonLibrary.Driver.VideoModes">CurrentDriver.VideoModes</see> list.  If not, an exception will be thrown.</para>
		/// <para>This overload of the function will use a default refresh rate of 60Hz and will default to <see cref="GorgonLibrary.VSyncIntervals">VSyncIntervals.None</see>.</para>
		/// </remarks>
		/// <param name="owner">Control that will be bound to the <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and will be the initial canvas to receive drawing commands.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>		
		/// <param name="format">Buffer format for the video mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>		
		/// <param name="usedepth">TRUE to create a depth buffer, FALSE to not create.</param>
		/// <param name="usestencil">TRUE to create a stencil buffer, FALSE to not create.</param>
		/// <exception cref="ArgumentNullException">Thrown when NULL is passed in for the owner parameter.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when a video device object could not be created.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when full screen mode is chosen and the video mode does not match a video mode on the <see cref="GorgonLibrary.Driver.VideoModes">CurrentDriver.VideoModes</see> list.  Can also be thrown if the desktop video mode does not support hardware acceleration.</exception>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, bool usedepth, bool usestencil)
		{
			SetMode(owner, width, height, format, windowed, usedepth, usestencil, 60, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode and primary rendering window.
		/// </summary>
		/// <remarks>
		/// When this function is successful it will create a new <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and assign it to the <see cref="GorgonLibrary.Gorgon.Screen">Screen</see> property, and will bind the primary render target to its owner control.
		/// <para>If the client size of the owning control does not match the width and height passed in to the function and the windowed parameter is TRUE, and the owning control is a Form, then the client size of the form will be adjusted to match that of the width and height passed in.  If the owning control is not a form, then the width and height will adjusted to the client size of the owning control.</para>
		/// 	<para>When in windowed mode, the vsync parameter and refresh rate parameter will be ignored.</para>
		/// 	<para>When going to full screen mode the width and height must match one of the video modes listed in the <see cref="GorgonLibrary.Driver.VideoModes">CurrentDriver.VideoModes</see> list.  If not, an exception will be thrown.</para>
		/// <para>This overload of the function will use a default refresh rate of 60Hz and will default to <see cref="GorgonLibrary.VSyncIntervals">VSyncIntervals.None</see>.  Stencil and depth buffers will not be created with this function.</para>		
		/// </remarks>
		/// <param name="owner">Control that will be bound to the <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and will be the initial canvas to receive drawing commands.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>		
		/// <param name="format">Buffer format for the video mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to go fullscreen.</param>		
		/// <exception cref="ArgumentNullException">Thrown when NULL is passed in for the owner parameter.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when a video device object could not be created.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when full screen mode is chosen and the video mode does not match a video mode on the <see cref="GorgonLibrary.Driver.VideoModes">CurrentDriver.VideoModes</see> list.  Can also be thrown if the desktop video mode does not support hardware acceleration.</exception>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed)
		{
			SetMode(owner, width, height, format, windowed, false, false, 60, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode and primary rendering window.
		/// </summary>
		/// <remarks>
		/// When this function is successful it will create a new <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and assign it to the <see cref="GorgonLibrary.Gorgon.Screen">Screen</see> property, and will bind the primary render target to its owner control.		
		/// <para>This overload will default to full screen mode.</para>
		/// 	<para>The width and height must match one of the video modes listed in the <see cref="GorgonLibrary.Driver.VideoModes">CurrentDriver.VideoModes</see> list.  If not, an exception will be thrown.</para>
		/// <para>This overload of the function will use a default refresh rate of 60Hz and will default to <see cref="GorgonLibrary.VSyncIntervals">VSyncIntervals.None</see>.  Stencil and depth buffers will not be created with this function</para>		
		/// </remarks>
		/// <param name="owner">Control that will be bound to the <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and will be the initial canvas to receive drawing commands.</param>
		/// <param name="width">Width of the video mode.</param>
		/// <param name="height">Height of the video mode.</param>		
		/// <param name="format">Buffer format for the video mode.</param>
		/// <exception cref="ArgumentNullException">Thrown when NULL is passed in for the owner parameter.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when a video device object could not be created.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when full screen mode is chosen and the video mode does not match a video mode on the <see cref="GorgonLibrary.Driver.VideoModes">CurrentDriver.VideoModes</see> list.  Can also be thrown if the desktop video mode does not support hardware acceleration.</exception>
		public static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format)
		{
			SetMode(owner, width, height, format, false, false, false, 60, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode and primary rendering window.
		/// </summary>
		/// <remarks>
		/// When this function is successful it will create a new <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and assign it to the <see cref="GorgonLibrary.Gorgon.Screen">Screen</see> property, and will bind the primary render target to its owner control.
		/// <para>In this overload of the function the width and height of the video mode are set to the client width and height of the owner control.  The video mode will also be forced into windowed mode and the format will be set to the desktop video mode format.</para>
		/// </remarks>
		/// <param name="owner">Control that will be bound to the <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and will be the initial canvas to receive drawing commands.</param>
		/// <exception cref="ArgumentNullException">Thrown when NULL is passed in for the owner parameter.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when a video device object could not be created.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when full screen mode is chosen and the video mode does not match a video mode on the <see cref="GorgonLibrary.Driver.VideoModes">CurrentDriver.VideoModes</see> list.  Can also be thrown if the desktop video mode does not support hardware acceleration.</exception>
		public static void SetMode(Forms.Control owner)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			SetMode(owner, owner.ClientSize.Width, owner.ClientSize.Height, DesktopVideoMode.Format, true, false, false, 60, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to start the engine rendering.
		/// </summary>
		/// <remarks>The application does not begin rendering right away when this function is called, it merely tells the library that the application is ready for rendering to begin when it's ready.</remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Gorgon.Initialize()</see> has not been called.</exception>
		/// <exception cref="InvalidOperationException">Thrown when <see cref="GorgonLibrary.Gorgon.SetMode(System.Windows.Forms.Control, Int32, Int32, GorgonLibrary.BackBufferFormats, Boolean, Boolean, Boolean, Int32, GorgonLibrary.VSyncIntervals)">SetMode()</see> has not been called.</exception>		
		public static void Go()
		{
			if (!IsInitialized)
				throw new GorgonException(GorgonErrors.NotInitialized);

			if ((Gorgon.Screen != null) && (_currentTarget == null))
				throw new InvalidOperationException("The render target is invalid.");

			if (IsRunning)
				return;

			// Enter render loop.
			_log.Print("Gorgon", "Entering main render loop...",LoggingLevel.Verbose);

			// Reset all timers.
			_timer.Reset();
			FrameStats.Reset();

			if (_currentTarget != null)
			{
				for (int i = 0; i < _currentTarget.Length; i++)
				{
					if (_currentTarget[i] != null)
						_currentTarget[i].Refresh();
				}
			}

			Forms.Application.Idle += new EventHandler(Run);

			IsRunning = true;
		}

		/// <summary>
		/// Function to stop the engine from rendering.
		/// </summary>
		/// <remarks>
		/// This will merely stop the rendering process, it can be restarted with the <see cref="GorgonLibrary.Gorgon.Go">Go()</see> function.
		/// <para>Note that this function does -not- affect the video mode.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Gorgon.Initialize()</see> has not been called.</exception>
		public static void Stop()
		{
			if (!IsInitialized)
				throw new GorgonException(GorgonErrors.NotInitialized);

			if (IsRunning)
			{
				Forms.Application.Idle -= new EventHandler(Run);
				IsRunning = false;

				// Reset all timers.
				_timer.Reset();
				FrameStats.Reset();

				_log.Print("Gorgon", "Main render loop stopped.", LoggingLevel.Verbose);
			}
		}

		/// <summary>
		/// Function to force the application to process any pending messages.
		/// </summary>
		/// <remarks>This function should be used when control over the message loop is necessary.</remarks>
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
			if ((IsRunning) && (Screen != null))
				Run(Screen, EventArgs.Empty);
		}

		/// <summary>
		/// Function to initialize Gorgon.
		/// </summary>
		/// <remarks>This function must be called before any other function.  This is because it will setup support data for use by Gorgon and its various objects.</remarks>
		public static void Initialize()
		{
			Initialize(false, false, false);
		}

		/// <summary>
		/// Function to initialize Gorgon.
		/// </summary>
		/// <param name="allowBackgroundRender">TRUE to allow rendering when the application loses focus, FALSE to suspend rendering.</param>
		/// <param name="allowScreenSaver">TRUE to allow the screen saver to run, FALSE to suspend it.</param>
		/// <remarks>This function must be called before any other function.  This is because it will setup support data for use by Gorgon and its various objects.</remarks>
		public static void Initialize(bool allowBackgroundRender, bool allowScreenSaver)
		{
			Initialize(allowBackgroundRender, allowScreenSaver, false);
		}

		/// <summary>
		/// Function to initialize Gorgon.
		/// </summary>
		/// <param name="allowBackgroundRender">TRUE to allow rendering when the application loses focus, FALSE to suspend rendering.</param>
		/// <param name="allowScreenSaver">TRUE to allow the screen saver to run, FALSE to suspend it.</param>
		/// <param name="checkDriverWHQL">TRUE to check for WHQL information, FALSE to ignore.</param>
		/// <remarks>This function must be called before any other function.  This is because it will setup support data for use by Gorgon and its various objects.</remarks>
		public static void Initialize(bool allowBackgroundRender, bool allowScreenSaver, bool checkDriverWHQL)
		{
			// Terminate if already initialized.
			if (IsInitialized)
				Terminate();

			IsInitialized = true;			

			// Initialize.
#if DEBUG
            DX.Configuration.EnableObjectTracking = true;
#else
            DX.Configuration.EnableObjectTracking = false;
#endif
			// We don't need exceptions with these errors.
			DX.Configuration.AddResultWatch(D3D9.ResultCode.DeviceLost, SlimDX.ResultWatchFlags.AlwaysIgnore);
			DX.Configuration.AddResultWatch(D3D9.ResultCode.DeviceNotReset, SlimDX.ResultWatchFlags.AlwaysIgnore);

			Direct3D = new D3D9.Direct3D();
			Direct3D.CheckWhql = checkDriverWHQL;

			try
			{
				// Open log object.
				string logPath = string.Empty;		// Log file path.

				logPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Tape_Worm\Gorgon\LibLog\";

				// Force the directory if necessary.
				try
				{
					if (!Directory.Exists(logPath))
						Directory.CreateDirectory(logPath);
				}
				finally
				{
					// Do nothing if we fail.
				}

				logPath += Assembly.GetExecutingAssembly().GetName().Name + "_" + Assembly.GetExecutingAssembly().GetName().Version.ToString();

				_log = new Logger(Assembly.GetEntryAssembly().GetName().Name, logPath);
				// Set this to intermediate, simple or none to have a smaller log file.
#if DEBUG
				_log.LogFilterLevel = LoggingLevel.Verbose;
#else
				_log.LogFilterLevel = LoggingLevel.None;
#endif

				try
				{
					_log.Open();
					GorgonException.Log = _log;
				}
				catch(GorgonException gEx)
				{
#if DEBUG
					// By rights, we should never see this error.  Better safe than sorry.
                    System.Windows.Forms.MessageBox.Show("Could not create a log file.\n" + gEx.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
#else
					Debug.WriteLine("Error opening log.\n" + gEx.Message);
#endif
				}

				Gorgon.Log.Print("Gorgon", "Initializing...", LoggingLevel.Simple);

				Gorgon.Log.Print("Gorgon", "Allow background processing: {0}", LoggingLevel.Verbose, allowBackgroundRender.ToString());
				Gorgon.Log.Print("Gorgon", "Allow screen saver: {0}", LoggingLevel.Verbose, allowScreenSaver.ToString());

				AllowBackgroundRendering = allowBackgroundRender;
				AllowScreenSaver = allowScreenSaver;
				IsRunning = false;				

				// Enumerate drivers and video modes.
				_drivers = new DriverList();
				_drivers.Refresh();

				// Create timing data.
				_timer = new PreciseTimer();

				// Create a message filter to trap screen saver messages.
				_messageFilter = new SysMessageFilter();
				Forms.Application.AddMessageFilter(_messageFilter);

				// Assign the initial driver, this will also create the renderer..
				CurrentDriver = _drivers[0];

				// Create timing statistics.
				FrameStats = new TimingData(_timer);

				// Create event arguments for idle event.
				_frameEventArgs = new FrameEventArgs(FrameStats);

				// Set default clear parameters.
				_clearTargets = ClearTargets.BackBuffer | ClearTargets.DepthBuffer | ClearTargets.StencilBuffer;

				FrameStatsTextColor = Color.White;

				_log.Print("Gorgon", "Initialized Successfully.", LoggingLevel.Simple);
			}
			catch (Exception ex)
			{
				IsInitialized = false;
				throw ex;
			}
		}

		/// <summary>
		/// Function to terminate Gorgon.
		/// </summary>
		/// <remarks>
		/// You must call this when finished with Gorgon, failure to do so can result in memory leaks.
		/// <para>This function will call <see cref="GorgonLibrary.Gorgon.CloseMode">CloseMode</see> implicitly.</para>
		/// </remarks>
		public static void Terminate()
		{
			// If the engine wasn't initialized, do nothing.
			if (!IsInitialized)
				return; 

			// Stop the engine.
			Stop();

			Forms.Application.RemoveMessageFilter(_messageFilter);

			// Unload fonts.
			FontCache.DestroyAll();

			// Unload all the file systems.
			FileSystemProviderCache.UnloadAll();

			if (FrameStats != null)
				FrameStats.Dispose();

			// Unload all plug-ins.
			PlugInFactory.DestroyAll();

			if (_stateCache != null)
				_stateCache.Dispose();
			
			// Remove all shaders.
			ShaderCache.DestroyAll();

			// Remove all render targets.
			RenderTargetCache.DestroyAll();

			// Remove all the images.
			ImageCache.DestroyAll();

			if ((_renderer != null) && (!_renderer.IsDisposed))
				_renderer.Dispose();

			if (Screen != null)
				Screen.Dispose();

			// Terminate Direct 3D.
			if (Direct3D != null)
				Direct3D.Dispose();
			Direct3D = null;

			_log.Print("Gorgon", "Shutting down.", LoggingLevel.Simple);

			// Destroy log.
			if (_log != null)
				_log.Dispose();

			_currentTarget = null;
			Screen = null;
			FrameStats = null;
			_stateCache = null;
			_timer = null;
			_log = null;
			_renderer = null;
			IsInitialized = false;
		}
		#endregion
    }
}
