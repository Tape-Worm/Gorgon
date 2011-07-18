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
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;
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
				MSG message = new MSG();		// Message to retrieve.

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
				if ((ParentWindow.WindowState == Forms.FormWindowState.Minimized) || (!ParentWindow.ContainsFocus))
					return false;
	
				return true;
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
		/// Property to return the parent window interface for the <see cref="P:GorgonLibrary.Gorgon.ApplicationWindow">ApplicationWindow</see> window bound to 
		/// </summary>
		/// <remarks>This is often the same object pointed to by <seealso cref="P:GorgonLibrary.Gorgon.ApplicationWindow">ApplicationWindow</seealso>.  When the application window is set to a control, then this will be set to the parent form of the control.</remarks>
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
		/// Property to return if the app is in a running state or not.
		/// </summary>
		/// <remarks>This flag is set to TRUE when the <see cref="M:GorgonLibrary.Gorgon.Go">Go()</see> function is called and FALSE when the <see cref="M:GorgonLibrary.Gorgon.Stop">Stop()</see> function is called.</remarks>
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
		/// This will merely stop the rendering process, it can be restarted with the <see cref="M:GorgonLibrary.Gorgon.Go">Go()</see> function.
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
			MSG message = new MSG();		// Message to retrieve.

			// Forward the messages.
			while (Win32API.PeekMessage(out message, IntPtr.Zero, 0, 0, PeekMessageFlags.Remove))
			{
				Win32API.TranslateMessage(ref message);
				Win32API.DispatchMessage(ref message);
			}

			// Continue on.
			if (IsRunning)
				Application_Idle(ParentWindow, EventArgs.Empty);
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
		/// </remarks>
		public static void Terminate()
		{
			// If the engine wasn't initialized, do nothing.
			if (!IsInitialized)
				return; 

			// Stop the engine.
			Stop();

			Log.Print("Shutting down.", GorgonLoggingLevel.Simple);

			// Destroy log.
			if (Log != null)
				Log.Close();

			Log = null;
			IsInitialized = false;
		}
		#endregion
    }
}
