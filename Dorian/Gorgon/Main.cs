#region MIT.
// 
// 
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
// Created: Tuesday, June 14, 2011 8:41:48 PM
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
using GorgonLibrary.Diagnostics;
using GorgonLibrary.FileSystems;
using GorgonLibrary.Internal;
using GorgonLibrary.Graphics;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary
{
	#region Enumerations.
	/// <summary>
	/// CPU/OS platform type.
	/// </summary>
	/// <remarks>This is a replacement for the old PlatformID code in the 1.x version of </remarks>
	public enum PlatformArchitecture
	{
		/// <summary>
		/// x86 architecture.
		/// </summary>
		x86 = 0,
		/// <summary>
		/// x64 architecture.
		/// </summary>
		x64 = 1
	}
	#endregion
	
	#region Delegates.
	/// <summary>
	/// Delegate for an application loop.
	/// </summary>
	/// <param name="timingData">Data used for frame rate timing.</param>
	/// <returns>TRUE to continue processing, FALSE to stop.</returns>
	/// <remarks>Use this to define the main loop for your application.</remarks>
	public delegate bool ApplicationLoop(GorgonFrameRate timingData);
	#endregion

	/// <summary>
	/// The primary interface into gorgon.
	/// </summary>
	/// <remarks>This interface handles the initialization of Gorgon from internal data structures to the video mode to be used.  Users should call <see cref="M:GorgonLibrary.Initialize">Initialize</see> before doing anything
	/// and call <see cref="M:GorgonLibrary.Terminate">Terminate</see> when finished.<para>This static class is used to change the global states of objects such as a global rendering setting to which render target is current.
	/// It will also control the execution and rendering flow for the application.
	/// </para></remarks>
	public static class Gorgon
	{
		#region Variables.
		private static ApplicationLoop _loop = null;					// Application loop.
		private static GorgonDefaultAppLoop _defaultApp = null;			// Default application loop.
		private static GorgonFrameRate _timingData = null;				// Frame rate timing data.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return if an application is in an idle state or not.
		/// </summary>
		private static bool AppIdle
		{
			get
			{
				Win32.MSG message = new Win32.MSG();		// Message to retrieve.

				return !Win32.PeekMessage(ref message, IntPtr.Zero, 0, 0, PeekMessageFlags.NoRemove);
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
				// TODO: We will be removing this.
				return null;
			}
		}

		/// <summary>
		/// Property to set or return the application idle loop.
		/// </summary>
		/// <remarks>This is used to call the users code when the application is in an idle state.
		/// <para>Users should call the <see cref="M:GorgonLibrary.Stop">Stop()</see> method before attempting to change the application idle funtion.</para></remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the application is in a <see cref="P:GorgonLibrary.IsInitialized">running state</see>.</exception>		
		public static ApplicationLoop ApplicationIdleLoop
		{
			get
			{
				return _loop;
			}
			set
			{
				if (IsRunning)
					throw new GorgonException(GorgonResult.AccessDenied, "Cannot assign a new idle function while the application is in a running state.");

				if (value == null)
					_loop = new ApplicationLoop(_defaultApp.ApplicationIdle);
				else
					_loop = value;
			}
		}

		/// <summary>
		/// Property to return the parent window interface for the <see cref="GorgonLibrary.ApplicationWindow">ApplicationWindow</see> window bound to 
		/// </summary>
		/// <remarks>This is often the same object pointed to by <seealso cref="GorgonLibrary.ApplicationWindow">ApplicationWindow</seealso>.  When the application window is set to a control, then this will be set to the parent form of the control.</remarks>
		public static Forms.Form ParentWindow
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the application window bound to 
		/// </summary>
		public static Forms.Control ApplicationWindow
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the platform that this instance of Gorgon was compiled for.
		/// </summary>
		/// <remarks>When the library is compiled for 64-bit processors, then this will read x64, otherwise it'll be x86.  If the platform cannot be determined it will return unknown.</remarks>
		public static PlatformArchitecture PlatformArchitecture
		{
			get
			{
				if (Environment.Is64BitProcess)
					return PlatformArchitecture.x64;
				else
					return PlatformArchitecture.x86;
			}
		}

		/// <summary>
		/// Property to set or return the frame statistics text color.
		/// </summary>
		/// <remarks>This only applies if <see cref="GorgonLibrary.FrameStatsVisible">FrameStatsVisible</see> is TRUE.</remarks>
		/// <value>The color of the text for the frame statistics.</value>
		internal static Color FrameStatsTextColor
		{	// TODO: Remove this.
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
		internal static bool FastResize
		{	// TODO: Remove this.
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the logo is shown or not.
		/// </summary>
		/// <value>TRUE to show the Gorgon logo in the lower right corner of the screen while rendering, FALSE to hide.</value>
		internal static bool LogoVisible
		{   // TODO: Remove this.
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the frame timing data is shown or not.
		/// </summary>
		/// <value>TRUE to show the current frame statistics (i.e. Frames Per Second, frame delta time, etc...) in the upper left corner of the screen while rendering.  FALSE to hide.</value>
		internal static bool FrameStatsVisible
		{	// TODO: Remove this.
			get;
			set;
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
		/// GlobalStateSettings.GlobalBlending = BlendingModes.None;
		/// // Keep the normal blending for the Bob sprite.
		/// Bob.BlendingMode = BlendingModes.Normal;
		/// </code>
		/// </example>
		/// <remarks>Changing the states in this property will result in that state being applied to all renderable objects.  The exception to this is when the state has been changed directly on the renderable object itself.</remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Initialize">Initialize()</see> has not been called.</exception>
		/// <value>A list of global settings that <see cref="GorgonLibrary.Graphics.Sprite">sprites</see> and <see cref="GorgonLibrary.Graphics.TextSprite">text sprites</see> will use as initial and inherited values.</value>		
		internal static SpriteStateCache GlobalStateSettings
		{
			get
			{
				// TODO: We will be removing this.
				return null;
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
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Initialize">Initialize()</see> has not been called.</exception>
		internal static Viewport CurrentClippingViewport
		{
			get
			{
				// TODO: We will be removing this.
				return null;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to return information about the current video mode.
		/// </summary>
		/// <remarks>If a video mode has not been set, and <see cref="M:GorgonLibrary.Initialize">Initialize()</see> has been called, then the current desktop video mode is returned instead.</remarks>
		/// <value>The currently active <see cref="VideoMode">video mode</see> information.</value>
		internal static VideoMode CurrentVideoMode
		{
			get
			{
				// TODO: We will be removing this.
				return default(VideoMode);
			}
		}

		/// <summary>
		/// Property to set or return the currently active shader.
		/// </summary>
		/// <remarks>Use this to apply a shader to the rendering pass.  You can apply either a <see cref="GorgonLibrary.Graphics.Shader"/>, <see cref="GorgonLibrary.Graphics.ShaderTechnique"/> or a <see cref="GorgonLibrary.Graphics.ShaderPass"/>.  
		/// When applying a shader there's a very small performance hit on the first pass of rendering as it attempts to locate the first valid shader technique.</remarks>
		/// <value>A shader renderer output to apply to the scene when rendering.</value>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Initialize">Initialize()</see> has not been called.
		/// <para>Thrown when <see cref="GorgonLibrary.SetMode(System.Windows.Forms.Control, Int32, Int32, GorgonLibrary.BackBufferFormats, Boolean, Boolean, Boolean, Int32, GorgonLibrary.VSyncIntervals)">SetMode()</see> has not been called.</para>
		/// </exception>
		internal static IShaderRenderer CurrentShader
		{
			get
			{
				// TODO: We will be removing this.
				return null;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the currently active render target.
		/// </summary>
		/// <remarks>
		/// Use this to change where sprites, primitives and text will be drawn.  This will allow for objects to be drawn off-screen or into another control that's bound with a <see cref="GorgonLibrary.Graphics.RenderWindow">RenderWindow</see>.
		/// <para>
		/// Set this property to NULL to continue drawing to the primary screen.</para>
		/// 	<para>Please note that when the render target is switched the <see cref="GorgonLibrary.CurrentClippingViewport">clipping viewport</see> is reset to the size of the render target being assigned.</para>
		/// </remarks>
		/// <seealso cref="GorgonLibrary.SetAdditionalRenderTarget">SetAdditionalRenderTarget</seealso>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Initialize">Initialize()</see> has not been called.<para>
		/// Thrown when <see cref="GorgonLibrary.SetMode(System.Windows.Forms.Control, Int32, Int32, GorgonLibrary.BackBufferFormats, Boolean, Boolean, Boolean, Int32, GorgonLibrary.VSyncIntervals)">SetMode()</see> has not been called.</para></exception>
		internal static RenderTarget CurrentRenderTarget
		{
			get
			{
				// TODO: We will be removing this.
				return null;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to return the primary rendering window.
		/// </summary>
		/// <remarks>
		/// There is only one of these <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> objects active at any given time.  Other controls that have render targets use <see cref="GorgonLibrary.Graphics.RenderWindow">RenderWindow</see> objects.
		/// <para>When the primary video mode bound to the control is closed, then all other render targets are automatically destroyed.</para>
		/// </remarks>
		/// <value>The primary rendering window or the "Screen".  This can be any control and is the primary render target that is setup during the <see cref="GorgonLibrary.SetMode(System.Windows.Forms.Control, Int32, Int32, GorgonLibrary.BackBufferFormats, Boolean, Boolean, Boolean, Int32, GorgonLibrary.VSyncIntervals)">SetMode()</see> function.  As such, this is the initial render target when a video mode is set.</value>
		internal static PrimaryRenderWindow Screen
		{// TODO: We will be removing this.
			get;
			private set;
		}

		/// <summary>
		/// Property to return the video mode information for the desktop.
		/// </summary>
		/// <remarks></remarks>
		/// <value>The desktop <see cref="VideoMode">video mode</see> information.</value>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Initialize">Initialize()</see> has not been called.</exception>
		internal static VideoMode DesktopVideoMode
		{
			get
			{
				// TODO: We will be removing this.
				return default(VideoMode);
			}
		}

		/// <summary>
		/// Property to return whether the library has been initialized or not.
		/// </summary>
		/// <value>TRUE if <see cref="M:GorgonLibrary.Initialize">Initialize()</see> has been called, FALSE if not.</value>
		public static bool IsInitialized
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the library log file interface.
		/// </summary>
		public static GorgonLogFile Log
		{
			get;
			private set;
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
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Initialize">Initialize()</see> has not been called.</exception>
		internal static Driver CurrentDriver
		{
			get
			{
				// TODO: We will be removing this.
				return null;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to return a list of video drivers available for rendering.
		/// </summary>
		/// <remarks>If a driver does not have hardware acceleration, it will not be included in the list.</remarks>
		/// <value>The list of installed <see cref="GorgonLibrary.Driver">video drivers</see>.</value>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Initialize">Initialize()</see> has not been called.</exception>
		internal static DriverList Drivers
		{
			get
			{
				// TODO: We will be removing this.
				return null;
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
		internal static bool AllowBackgroundRendering
		{// TODO: We will be removing this.
			get;
			set;
		}

		/// <summary>
		/// Property to return if the app is in a running state or not.
		/// </summary>
		/// <remarks>This flag is set to TRUE when the <see cref="GorgonLibrary.Go">Go()</see> function is called and FALSE when the <see cref="GorgonLibrary.Stop">Stop()</see> function is called.</remarks>
		/// <value>TRUE if the application is running, and FALSE if not.</value>
		public static bool IsRunning
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the amount of time in milliseconds to sleep when the application window is not focused.
		/// </summary>
		/// <remarks>
		/// Set this value to 0 to use all CPU time when the application is not focused.  The default is 10 milliseconds.
		/// <para>This is handy in situations when the application is in the background and processing does not need to continue.  For laptops this means battery savings when the application is not focused.
		/// </para>
		/// </remarks>
		public static int UnfocusedSleepTime
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Idle event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private static void Application_Idle(object sender, EventArgs e)
		{
			while ((AppIdle) && (IsRunning))
			{
				if (!HasFocus)
					break;

				_timingData.Update();

				if (!ApplicationIdleLoop(_timingData))
				{
					Stop();
					break;
				}

				// Give up CPU time if we're not focused.
				if ((!ParentWindow.ContainsFocus) && (UnfocusedSleepTime > 0))
					System.Threading.Thread.Sleep(UnfocusedSleepTime);
			}
		}

		/// <summary>
		/// Function to set additional render targets.
		/// </summary>
		/// <param name="index">An index value from 0 to <see cref="GorgonLibrary.Driver.MaximumSimultaneousRenderTargets">Driver.MaximumSimultaneousRenderTargets</see>-1.</param>
		/// <param name="target">A render target to bind to the index.</param>
		/// <remarks>This will allow the user to set more than one render target at a time for simultaneous rendering.  Please note that using multiple render targets is only supported when
		/// using shaders.
		/// <para>An index of 0 will set the current primary render target which is identical to using <see cref="GorgonLibrary.CurrentRenderTarget">CurrentRenderTarget</see>.</para>
		/// <para>There are some limitations to setting multiple render targets:  The render targets should have the same width and height.  They can use a different 
		/// <see cref="GorgonLibrary.Graphics.ImageBufferFormats">image format</see> but the image formats of the targets must have the same bit count.  This restriction is lifted for 
		/// devices that have <see cref="GorgonLibrary.Driver.SupportMRTIndependentBitDepths">Driver.SupportMRTIndepenedentBitDepths</see> set to true.</para>
		/// <para>If <see cref="GorgonLibrary.Driver.SupportMRTPostPixelShaderBlending">Driver.SupportMRTPostPixelShaderBlending</see> is true then post pixel shader blending operations can 
		/// be performed by the video card.  However, if it is supported, the render target must queried to see if it will support post pixel shader blending operations via 
		/// the <see cref="GorgonLibrary.Graphics.RenderTarget.IsValidForMRTPostPixelShaderBlending">RenderTarget.IsValidForMRTPostPixelShaderBlending</see> property.</para>
		/// <para>Passing NULL (Nothing in Visual Basic) with an index of 0 will set the render target to the <see cref="GorgonLibrary.Screen">Screen</see> render target.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Initialize">Initialize</see> has not been called, 
		/// <see cref="GorgonLibrary.SetMode(System.Windows.Forms.Control, Int32, Int32, GorgonLibrary.BackBufferFormats, Boolean, Boolean, Boolean, Int32, GorgonLibrary.VSyncIntervals)">SetMode</see> wasn't called or the target cannot be bound.</exception>
		internal static void SetAdditionalRenderTarget(int index, RenderTarget target)
		{
			// TODO: We will be removing this.
		}

		/// <summary>
		/// Function to return an additional render target from the active render target list.
		/// </summary>
		/// <param name="index">An index value from 0 to <see cref="GorgonLibrary.Driver.MaximumSimultaneousRenderTargets">Driver.MaximumSimultaneousRenderTargets</see>-1.</param>
		/// <returns>The render target at the specified index.</returns>
		internal static RenderTarget GetAdditionalRenderTarget(int index)
		{
			// TODO: We will be removing this.
			return null;
		}

		/// <summary>
		/// Function to close the currently active video mode.
		/// </summary>
		/// <remarks>Some resources may have to be re-loaded or re-created when this function is called and another call to SetMode is made.</remarks>
		internal static void CloseMode()
		{
			// TODO: We will be removing this.
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
		/// When this function is successful it will create a new <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and assign it to the <see cref="GorgonLibrary.Screen">Screen</see> property, and will bind the primary render target to its owner control.
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
		internal static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, bool usedepth, bool usestencil, int refresh, VSyncIntervals vSyncInterval)
		{
			// TODO: We will be removing this.
		}

		/// <summary>
		/// Function to set the video mode and primary rendering window.
		/// </summary>
		/// <remarks>
		/// When this function is successful it will create a new <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and assign it to the <see cref="GorgonLibrary.Screen">Screen</see> property, and will bind the primary render target to its owner control.
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
		internal static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, bool usedepth, bool usestencil, int refresh)
		{
			SetMode(owner, width, height, format, windowed, usedepth, usestencil, refresh, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode and primary rendering window.
		/// </summary>
		/// <remarks>
		/// When this function is successful it will create a new <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and assign it to the <see cref="GorgonLibrary.Screen">Screen</see> property, and will bind the primary render target to its owner control.
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
		internal static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed, bool usedepth, bool usestencil)
		{
			SetMode(owner, width, height, format, windowed, usedepth, usestencil, 60, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode and primary rendering window.
		/// </summary>
		/// <remarks>
		/// When this function is successful it will create a new <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and assign it to the <see cref="GorgonLibrary.Screen">Screen</see> property, and will bind the primary render target to its owner control.
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
		internal static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format, bool windowed)
		{
			SetMode(owner, width, height, format, windowed, false, false, 60, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode and primary rendering window.
		/// </summary>
		/// <remarks>
		/// When this function is successful it will create a new <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and assign it to the <see cref="GorgonLibrary.Screen">Screen</see> property, and will bind the primary render target to its owner control.		
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
		internal static void SetMode(Forms.Control owner, int width, int height, BackBufferFormats format)
		{
			SetMode(owner, width, height, format, false, false, false, 60, VSyncIntervals.IntervalNone);
		}

		/// <summary>
		/// Function to set the video mode and primary rendering window.
		/// </summary>
		/// <remarks>
		/// When this function is successful it will create a new <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and assign it to the <see cref="GorgonLibrary.Screen">Screen</see> property, and will bind the primary render target to its owner control.
		/// <para>In this overload of the function the width and height of the video mode are set to the client width and height of the owner control.  The video mode will also be forced into windowed mode and the format will be set to the desktop video mode format.</para>
		/// </remarks>
		/// <param name="owner">Control that will be bound to the <see cref="GorgonLibrary.Graphics.PrimaryRenderWindow">PrimaryRenderWindow</see> and will be the initial canvas to receive drawing commands.</param>
		/// <exception cref="ArgumentNullException">Thrown when NULL is passed in for the owner parameter.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when a video device object could not be created.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when full screen mode is chosen and the video mode does not match a video mode on the <see cref="GorgonLibrary.Driver.VideoModes">CurrentDriver.VideoModes</see> list.  Can also be thrown if the desktop video mode does not support hardware acceleration.</exception>
		internal static void SetMode(Forms.Control owner)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			SetMode(owner, owner.ClientSize.Width, owner.ClientSize.Height, DesktopVideoMode.Format, true, false, false, 60, VSyncIntervals.IntervalNone);
		}


		/// <summary>
		/// Function to start the application message processing.
		/// </summary>
		/// <remarks>The application does not begin running right away when this function is called, it merely tells the library that the application is ready to begin.</remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Initialize">Initialize()</see> has not been called.</exception>
		public static void Go(ApplicationLoop idleLoop)
		{
			if (!IsInitialized)
				throw new GorgonException(GorgonResult.NotInitialized, "Please call Initialize() before calling this function.");

			if (IsRunning)
				return;

			_timingData = new GorgonFrameRate();
			ApplicationIdleLoop = idleLoop;

			Log.Print("Application loop starting...", GorgonLoggingLevel.Simple);

			if (!ApplicationWindow.Visible)
				ApplicationWindow.Visible = true;

			Forms.Application.Idle += new EventHandler(Application_Idle);
			IsRunning = true;
		}
	
		/// <summary>
		/// Function to stop the engine from rendering.
		/// </summary>
		/// <remarks>
		/// This will merely stop the rendering process, it can be restarted with the <see cref="GorgonLibrary.Go">Go()</see> function.
		/// <para>Note that this function does -not- affect the video mode.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Initialize">Initialize()</see> has not been called.</exception>
		public static void Stop()
		{
			if (!IsInitialized)
				throw new GorgonException(GorgonResult.NotInitialized);

			if (IsRunning)
			{
				Forms.Application.Idle -= new EventHandler(Application_Idle);
				IsRunning = false;

				Log.Print("Application loop stopped.", GorgonLoggingLevel.Verbose);
			}
		}

		/// <summary>
		/// Function to force the application to process any pending messages.
		/// </summary>
		/// <remarks>This function should be used when control over the message loop is necessary.</remarks>
		public static void ProcessMessages()
		{
			Win32.MSG message = new Win32.MSG();		// Message to retrieve.

			// Forward the messages.
			while (Win32.PeekMessage(ref message, IntPtr.Zero, 0, 0, PeekMessageFlags.Remove))
			{
				Win32.TranslateMessage(ref message);
				Win32.DispatchMessage(ref message);
			}

			// Continue on.
			if ((IsRunning) && (Screen != null))
				Application_Idle(Screen, EventArgs.Empty);
		}

		/// <summary>
		/// Function to initialize 
		/// </summary>
		/// <param name="applicationWindow">Windows form control that will be used for the application.</param>
		/// <remarks>This function must be called before any other function.  This is because it will setup support data for use by Gorgon and its various objects.</remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the <paramref name="applicationWindow"/> parameter is NULL (Nothing in VB.NET).</exception>
		public static void Initialize(Forms.Control applicationWindow)
		{
			if (applicationWindow == null)
				throw new ArgumentNullException("applicationWindow", "Gorgon requires a windows form or control to run.");

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
			Direct3D.CheckWhql = false;

			try
			{
				// Open log object.
				Log = new GorgonLogFile("GorgonLibrary");

				// Set this to intermediate, simple or none to have a smaller log file.
#if DEBUG
				Log.LogFilterLevel = GorgonLoggingLevel.Verbose;
#else
				Log.LogFilterLevel = GorgonLoggingLevel.None;
#endif

				try
				{
					Log.Open();
					GorgonException.Log = Log;
				}
				catch(Exception ex)
				{
#if DEBUG
					// By rights, we should never see this error.  Better safe than sorry.
					System.Windows.Forms.MessageBox.Show("Could not create a log file.\n" + ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
#else
					Debug.WriteLine("Error opening log.\n" + gEx.Message);
#endif
				}

				Log.Print("Initializing...", GorgonLoggingLevel.Simple);
				Log.Print("Architecture: {0}", GorgonLoggingLevel.Verbose, PlatformArchitecture.ToString());

				// Default to using 10 milliseconds of sleep time when the application is not focused.
				UnfocusedSleepTime = 10;

				ApplicationWindow = applicationWindow;

				// If the application window is a form, then assign it as its own parent.
				ParentWindow = ApplicationWindow as Forms.Form;

				// Find the parent of the control.
				Forms.Control parentControl = ApplicationWindow.Parent;

				while ((parentControl != null) && (ParentWindow == null))
				{
					ParentWindow = parentControl as Forms.Form;
					parentControl = parentControl.Parent;
				}

				if (ParentWindow == null)
					throw new GorgonException(GorgonResult.CannotEnumerate, "The window at '" + GorgonUtility.FormatHex(ApplicationWindow.Handle) + "' has no parent form.");
				else
				{
					Log.Print("Using window at '0x{0}' as the application window.", GorgonLoggingLevel.Verbose, GorgonUtility.FormatHex(ApplicationWindow.Handle));
					if (ParentWindow != ApplicationWindow)
						Log.Print("Using parent window of application window at '0x{0}'.", GorgonLoggingLevel.Verbose, GorgonUtility.FormatHex(ParentWindow.Handle));
				}

				AllowBackgroundRendering = true;
				IsRunning = false;

				//// Enumerate drivers and video modes.
				//_drivers = new DriverList();
				//_drivers.Refresh();

				//// Create timing data.
				//_timer = new PreciseTimer();

				//// Assign the initial driver, this will also create the renderer..
				//CurrentDriver = _drivers[0];

				//// Create timing statistics.
				//FrameStats = new TimingData(_timer);

				//// Create event arguments for idle event.
				//_frameEventArgs = new FrameEventArgs(FrameStats);

				//// Set default clear parameters.
				//_clearTargets = ClearTargets.BackBuffer | ClearTargets.DepthBuffer | ClearTargets.StencilBuffer;

				//FrameStatsTextColor = Color.White;

				Log.Print("Initialized Successfully.", GorgonLoggingLevel.Simple);
			}
			catch (Exception ex)
			{
				IsInitialized = false;
				throw ex;
			}
		}

		/// <summary>
		/// Function to terminate 
		/// </summary>
		/// <remarks>
		/// You must call this when finished with Gorgon, failure to do so can result in memory leaks.
		/// <para>This function will call <see cref="GorgonLibrary.CloseMode">CloseMode</see> implicitly.</para>
		/// </remarks>
		public static void Terminate()
		{
			// If the engine wasn't initialized, do nothing.
			if (!IsInitialized)
				return; 

			// Stop the engine.
			Stop();

			// Unload fonts.
			FontCache.DestroyAll();

			// Unload all the file systems.
			FileSystemProviderCache.UnloadAll();

			// Unload all plug-ins.
			PlugInFactory.DestroyAll();

			// Remove all shaders.
			ShaderCache.DestroyAll();

			// Remove all render targets.
			RenderTargetCache.DestroyAll();

			// Remove all the images.
			ImageCache.DestroyAll();

			if (Screen != null)
				Screen.Dispose();

			// Terminate Direct 3D.
			if (Direct3D != null)
				Direct3D.Dispose();
			Direct3D = null;

			Log.Print("Shutting down.", GorgonLoggingLevel.Simple);

			// Destroy log.
			if (Log != null)
				Log.Close();

			Screen = null;
			Log = null;
			IsInitialized = false;
		}
		#endregion
    }
}
