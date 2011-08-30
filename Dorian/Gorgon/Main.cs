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
using System.Text;
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
	/// <remarks>This interface handles the initialization of Gorgon from internal data structures to the video mode to be used.  Users should call <see cref="M:GorgonLibrary.Gorgon.Initialize">Initialize</see> before doing anything
	/// and call <see cref="M:GorgonLibrary.Gorgon.Terminate">Terminate</see> when finished.</remarks>
	public static class Gorgon
	{
		#region Variables.
		private static ApplicationLoop _loop = null;					// Application loop.
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
				if ((AllowBackground) || (ApplicationForm == null))
					return true;

				if ((ApplicationForm.WindowState == Forms.FormWindowState.Minimized) || (!ApplicationForm.ContainsFocus))
					return false;
	
				return true;
			}
		}

		/// <summary>
		/// Property to return the directory for the currently running application.
		/// </summary>
		public static string ApplicationDirectory
		{
			get
			{
				Assembly runningAssembly = Assembly.GetEntryAssembly();

				if (runningAssembly == null)
					runningAssembly = Assembly.GetCallingAssembly();

				if (runningAssembly == null)
					return string.Empty;

				return Path.GetDirectoryName(runningAssembly.Location).FormatDirectory(Path.DirectorySeparatorChar);
			}
		}

		/// <summary>
		/// Property to return the path for the currently running application.
		/// </summary>
		public static string ApplicationPath
		{
			get
			{
				Assembly runningAssembly = Assembly.GetEntryAssembly();

				if (runningAssembly == null)
					runningAssembly = Assembly.GetCallingAssembly();

				if (runningAssembly == null)
					return string.Empty;

				return Path.GetDirectoryName(runningAssembly.Location).FormatDirectory(Path.DirectorySeparatorChar) + Path.GetFileName(runningAssembly.Location).FormatFileName();
			}
		}

		/// <summary>
		/// Property to return the total physical RAM available in bytes.
		/// </summary>
		public static long TotalPhysicalRAM
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the available physical RAM in bytes.
		/// </summary>
		public static long AvailablePhysicalRAM
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the application idle loop.
		/// </summary>
		/// <remarks>This is used to call the users code when the application is in an idle state.
		/// <para>Users should call the <see cref="M:GorgonLibrary.Gorgon.Stop">Stop</see> method before attempting to change the application idle funtion.</para></remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the application is in a <see cref="P:GorgonLibrary.Gorgon.IsInitialized">running state</see>.</exception>		
		public static ApplicationLoop ApplicationIdleLoop
		{
			get
			{
				return _loop;
			}
			set
			{
				if (IsRunning)
					throw new GorgonException(GorgonResult.AccessDenied, "Cannot assign a new idle method while the application is in a running state.");

				_loop = value;
			}
		}

		/// <summary>
		/// Property to return the primary window for the application.
		/// </summary>
		public static Forms.Form ApplicationForm
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
		/// <value>TRUE if <see cref="M:GorgonLibrary.Gorgon.Initialize">Initialize</see> has been called, FALSE if not.</value>
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
		/// <remarks>This flag is set to TRUE when the <see cref="M:GorgonLibrary.Gorgon.Go">Go</see> method is called and FALSE when the <see cref="M:GorgonLibrary.Gorgon.Stop">Stop</see> method is called.</remarks>
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

		/// <summary>
		/// Property to allow the idle loop to continue running while the window is not focused or minimized.
		/// </summary>
		/// <remarks>This is set to TRUE by default, and this means that the code in the idle loop will continue to execute when the window is not focused or minimized.  When it is FALSE, the application will suspend until it regains focus.
		/// <para>There may be a delay when the code is executing in the background if the <see cref="P:GorgonLibrary.Gorgon.UnfocusedSleepTime">UnfocusedSleepTime</see> property is set greater than 0.</para>
		/// </remarks>
		public static bool AllowBackground
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the plug-in factory interface.
		/// </summary>
		public static GorgonPlugInFactory PlugIns
		{
			get;
			private set;
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
				if ((ApplicationForm != null) && (!ApplicationForm.ContainsFocus) && (UnfocusedSleepTime > 0))
					System.Threading.Thread.Sleep(UnfocusedSleepTime);
			}
		}

		/// <summary>
		/// Function to return the path to the per-user roaming directory for a given application.
		/// </summary>
		/// <param name="applicationName">Name of the application.</param>
		/// <returns>The per-user roaming data directory.</returns>
		/// <remarks>The method will trim leading and trailing spaces.  If the parameter only contains whitespace, it will be treated as an empty string.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the parameter is NULL (or Nothing in VB.NET).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the parameter is an empty string.</exception>
		public static string GetUserApplicationPath(string applicationName)
		{
			StringBuilder outputDir = new StringBuilder(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

			GorgonDebug.AssertParamString(applicationName, "applicationName");

			if (outputDir[outputDir.Length - 1] != Path.DirectorySeparatorChar)
				outputDir.Append(Path.DirectorySeparatorChar);

			outputDir.Append(applicationName);

			return outputDir.ToString().FormatDirectory(Path.DirectorySeparatorChar);
		}

		/// <summary>
		/// Function to return the top level form that contains the child control.
		/// </summary>
		/// <param name="childControl">The child control that's nested within a base windows form.</param>
		/// <returns>The windows form that contains the control, or NULL (Nothing in VB.Net) if the control is not embedded on a form at some level.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="childControl"/> parameter is NULL (Nothing in VB.Net).</exception>
		public static Forms.Form GetTopLevelForm(Forms.Control childControl)
		{
			Forms.Form result = null;
			Forms.Control parent = null;

			if (childControl == null)
				throw new ArgumentNullException("childControl");

			result = childControl as Forms.Form;

			if (result != null)
				return result;

			parent = childControl.Parent;

			while (parent != null)
			{
				result = parent as Forms.Form;
				if (result != null)
					break;

				parent = parent.Parent;
			}

			return result;
		}

		/// <summary>
		/// Function to start the application message processing.
		/// </summary>		
		/// <remarks>The application does not begin running right away when this method is called, it merely tells the library that the application is ready to begin.
		/// <para>This overload will use the <see cref="GorgonLibrary.Gorgon.ApplicationIdleLoop">ApplicationIdleLoop</see> property as the default idle handler.  If this property is NULL (Nothing in VB.Net) then Gorgon will forego using a custom application idle loop and instead will use the standard WinForms Application.Run message loop.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Initialize</see> has not been called.</exception>
		public static void Go()
		{
			Go(ApplicationIdleLoop);
		}

		/// <summary>
		/// Function to start the application message processing.
		/// </summary>		
		/// <param name="idleLoop">Delegate function to use when the machine is in an idle state.</param>
		/// <remarks>The application does not begin running right away when this method is called, it merely tells the library that the application is ready to begin.
		/// <para>Passing NULL (Nothing in VB.Net) to the <paramref name="idleLoop"/> parameter will forego using a custom application idle loop and instead will use the standard WinForms Application.Run message loop.</para>
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Initialize</see> has not been called.</exception>
		public static void Go(ApplicationLoop idleLoop)
		{
			if (!IsInitialized)
				throw new GorgonException(GorgonResult.NotInitialized, "Please call Initialize() before calling this method.");

			if (IsRunning)
				return;

			_timingData = new GorgonFrameRate();
			ApplicationIdleLoop = idleLoop;

			if (ApplicationIdleLoop != null)
			{
				Log.Print("Application loop starting...", GorgonLoggingLevel.Simple);
				Forms.Application.Idle += new EventHandler(Application_Idle);
			}

			IsRunning = true;
		}
	
		/// <summary>
		/// Function to stop the application message process.
		/// </summary>
		/// <remarks>
		/// This will merely stop the message process, it can be restarted with the <see cref="M:GorgonLibrary.Gorgon.Go">Go</see> method.
		/// </remarks>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when <see cref="M:GorgonLibrary.Gorgon.Initialize">Initialize</see> has not been called.</exception>
		public static void Stop()
		{
			if (!IsInitialized)
				throw new GorgonException(GorgonResult.NotInitialized);

			if (IsRunning)
			{
				if (ApplicationIdleLoop != null)
				{
					Forms.Application.Idle -= new EventHandler(Application_Idle);
					Log.Print("Application loop stopped.", GorgonLoggingLevel.Verbose);
				}
				IsRunning = false;				
			}
		}

		/// <summary>
		/// Function to force the application to process any pending messages.
		/// </summary>
		/// <remarks>This method should be used when control over the message loop is necessary.</remarks>
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
				Application_Idle(ApplicationForm, EventArgs.Empty);
		}	

		/// <summary>
		/// Function to initialize Gorgon.
		/// </summary>
		/// <remarks>This method must be called before any other method.  This is because it will setup support data for use by Gorgon and its various objects.</remarks>
		public static void Initialize()
		{
			Initialize(null);
		}

		/// <summary>
		/// Function to initialize Gorgon.
		/// </summary>
		/// <param name="applicationForm">Windows form that will be used for the application.</param>
		/// <remarks>This method must be called before any other method.  This is because it will setup support data for use by Gorgon and its various objects.</remarks>
		public static void Initialize(Forms.Form applicationForm)
		{			
			// Terminate if already initialized.
			if (IsInitialized)
				Terminate();

			if (Log == null)
				Log = new GorgonLogFile("GorgonLibrary");

			// Re-open the log.
			if (Log.IsClosed)
				Log.Open();

			IsInitialized = true;			

			try
			{
				TotalPhysicalRAM = Win32API.TotalPhysicalRAM;
				AvailablePhysicalRAM = Win32API.AvailablePhysicalRAM;

				Log.Print("Initializing...", GorgonLoggingLevel.All);
				Log.Print("Architecture: {0}", GorgonLoggingLevel.Verbose, PlatformArchitecture.ToString());
				Log.Print("Installed Memory: {0}", GorgonLoggingLevel.Verbose, TotalPhysicalRAM.FormatMemory());
				Log.Print("Available Memory: {0}", GorgonLoggingLevel.Verbose, AvailablePhysicalRAM.FormatMemory());

				// Default to using 10 milliseconds of sleep time when the application is not focused.
				UnfocusedSleepTime = 10;
								
				// Get the primary application window.
				ApplicationForm = applicationForm;

				PlugIns = new GorgonPlugInFactory();

				if (ApplicationForm != null)
					Log.Print("Using window '{1} ({2})' at '0x{0}' as the application window.", GorgonLoggingLevel.Verbose, ApplicationForm.Handle.FormatHex(), ApplicationForm.Name, ApplicationForm.Text);

				IsRunning = false;

				Log.Print("Initialized Successfully.", GorgonLoggingLevel.All);
			}
			catch (Exception ex)
			{
				IsInitialized = false;
				throw ex;
			}
		}

		/// <summary>
		/// Function to terminate the application.
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

			PlugIns.UnloadAll();

			Log.Print("Shutting down.", GorgonLoggingLevel.All);

			// Destroy log.
			if (!Log.IsClosed)
				Log.Close();

			IsInitialized = false;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes the <see cref="Gorgon"/> class.
		/// </summary>
		static Gorgon()
		{
			// Open log object.
			Log = new GorgonLogFile("GorgonLibrary");

			try
			{
				// Set this to intermediate, simple or none to have a smaller log file.
#if !DEBUG
				Log.LogFilterLevel = GorgonLoggingLevel.NoLogging;
#endif

				Log.Open();
				GorgonException.Log = Log;
			}
			catch (Exception ex)
			{
#if DEBUG
					// By rights, we should never see this error.  Better safe than sorry.
					System.Windows.Forms.MessageBox.Show("Could not create a log file.\n" + ex.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
#else
				Debug.Print("Error opening log: {0}", ex.Message);
#endif
			}
		}
		#endregion
	}
}
