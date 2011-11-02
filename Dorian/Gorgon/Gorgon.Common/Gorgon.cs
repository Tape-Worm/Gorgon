#region MIT.
// 
// Gorgon.
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
// Created: Wednesday, November 02, 2011 10:11:10 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Collections.Specialized;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;
using GorgonLibrary.PlugIns;

namespace GorgonLibrary
{
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
	/// The gorgon application.
	/// </summary>
	/// <remarks>Use this to replace the Application.Run(new Form()) method in the startup function.</remarks>
	public static class Gorgon
	{
		#region Classes.
		/// <summary>
		/// The application context for Gorgon.
		/// </summary>
		internal class GorgonContext
			: ApplicationContext
		{
			#region Variables.
			private GorgonFrameRate _timingData = null;								// Frame rate timing data.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return if the application has focus.
			/// </summary>
			private bool HasFocus
			{
				get
				{
					if ((Gorgon.AllowBackground) || (Gorgon.ApplicationForm == null))
						return true;

					if ((ApplicationForm.WindowState == FormWindowState.Minimized) || (!ApplicationForm.ContainsFocus))
						return false;

					return true;
				}
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Handles the Idle event of the Application control.
			/// </summary>
			/// <param name="sender">The source of the event.</param>
			/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
			internal void Application_Idle(object sender, EventArgs e)
			{
				MSG message = new MSG();		// Message to retrieve.

				while ((HasFocus) && (!Win32API.PeekMessage(out message, IntPtr.Zero, 0, 0, PeekMessageFlags.NoRemove)))
				{
					_timingData.Update();

					if (!ApplicationIdleLoop(_timingData))
					{
						// Force an exit from the thread.
						ExitThread();
						return;
					}

					// Give up CPU time if we're not focused.
					if ((ApplicationForm != null) && (!ApplicationForm.ContainsFocus) && (UnfocusedSleepTime > 0))
						System.Threading.Thread.Sleep(UnfocusedSleepTime);
				}
			}

			/// <summary>
			/// Terminates the message loop of the thread.
			/// </summary>
			protected override void ExitThreadCore()
			{
				IsRunning = false;
				base.ExitThreadCore();
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="Gorgon"/> class.
			/// </summary>
			/// <param name="form">The main form to use for the application.</param>
			public GorgonContext(Form form)
			{
				_timingData = new GorgonFrameRate();
				MainForm = form;
			}
			#endregion
		}
		#endregion

		#region Constants.
		private const string _logFile = "GorgonLibrary";				// Log file application name.
		#endregion

		#region Variables.
		private static readonly object _syncLock = new object();				// Sync lock.
		private static GorgonLogFile _log = null;								// Log file.
		private static GorgonContext _context = null;							// Gorgon application context.
		private static ApplicationLoop _loop = null;							// Application loop.
		private static GorgonTrackedObjectCollection _trackedObjects = null;	// Tracked objects.
		#endregion

		#region Properties.		
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
		/// Property to return the primary window for the application.
		/// </summary>
		public static Form ApplicationForm
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the library log file interface.
		/// </summary>
		public static GorgonLogFile Log
		{
			get
			{
				if (_log == null)
				{
					lock (_syncLock)
					{
						if (_log == null)
							_log = new GorgonLogFile(Gorgon._logFile, "Tape_Worm");
					}
				}

				return _log;
			}
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
		/// Method to quit the application.
		/// </summary>
		public static void Quit()
		{
			if ((!IsRunning) || (_context == null))
				return;

			_context.ExitThread();			
		}

		/// <summary>
		/// Method to run a Gorgon application.
		/// </summary>
		/// <param name="mainForm">Main application form.</param>
		/// <param name="loop">Idle loop code for the application.</param>
		public static void Run(Form mainForm, ApplicationLoop loop)
		{
			_context = new GorgonContext(mainForm);
			ApplicationForm = mainForm;

			try
			{
				if (ApplicationForm != null)
					Log.Print("Using window '{1} ({2})' at '0x{0}' as the application window.", GorgonLoggingLevel.Verbose, ApplicationForm.Handle.FormatHex(), ApplicationForm.Name, ApplicationForm.Text);

				// Display the form.
				if (ApplicationForm != null)
					ApplicationForm.Show();

				ApplicationIdleLoop = loop;

				IsRunning = true;

				if (ApplicationIdleLoop != null)
				{
					Log.Print("Application loop starting...", GorgonLoggingLevel.Simple);
					Application.Idle += new EventHandler(_context.Application_Idle);
				}

				Application.Run(_context);
			}
			catch (Exception ex)
			{
				throw GorgonException.Catch(ex);
			}
			finally
			{
				IsRunning = false;

				if (ApplicationIdleLoop != null)
				{
					Application.Idle -= new EventHandler(_context.Application_Idle);
					Log.Print("Application loop stopped.", GorgonLoggingLevel.Simple);
				}

				if (_trackedObjects != null)
					_trackedObjects.ReleaseAll();

				PlugIns.UnloadAll();

				Log.Print("Shutting down.", GorgonLoggingLevel.All);

				// Destroy log.
				if (!Log.IsClosed)
					Log.Close();

				if (_context != null)
					_context.Dispose();
				_context = null;
			}
		}

		/// <summary>
		/// Method to run a Gorgon application.
		/// </summary>
		/// <param name="loop">Idle loop code for the application.</param>
		public static void Run(ApplicationLoop loop)
		{
			Run(null, loop);
		}

		/// <summary>
		/// Function to force the application to process any pending messages.
		/// </summary>
		/// <remarks>This method should be used when control over the message loop is necessary.</remarks>
		/// <returns>TRUE if WM_QUIT is received, FALSE if not.</returns>
		public static bool ProcessMessages()
		{
			if (_context == null)
				return true;

			MSG message = new MSG();		// Message to retrieve.

			// Forward the messages.
			while (Win32API.PeekMessage(out message, IntPtr.Zero, 0, 0, PeekMessageFlags.Remove))
			{
				if (message.Message == WindowMessages.Quit)
				{
					_context.ExitThread();
					return true;
				}
				Win32API.TranslateMessage(ref message);
				Win32API.DispatchMessage(ref message);
			}

			return false;
		}	

		/// <summary>
		/// Function to add an object for tracking by the main Gorgon interface.
		/// </summary>
		/// <param name="trackedObject">Object to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="trackedObject"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <remarks>This allows Gorgon to track objects and destroy them upon <see cref="M:GorgonLibrary.Gorgon.Terminate">termination</see>.</remarks>
		public static void AddTrackedObject(IDisposable trackedObject)
		{
			if (trackedObject == null)
				throw new ArgumentNullException("trackedObject");

			_trackedObjects.Add(trackedObject);
		}

		/// <summary>
		/// Function to remove a tracked object from the Gorgon interface.
		/// </summary>
		/// <param name="trackedObject">Object to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="trackedObject"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <remarks>This will -not- destroy the tracked object.</remarks>
		public static void RemoveTrackedObject(IDisposable trackedObject)
		{
			if (trackedObject == null)
				throw new ArgumentNullException("trackedObject");

			_trackedObjects.Remove(trackedObject);
		}

		/// <summary>
		/// Function to return the top level form that contains the child control.
		/// </summary>
		/// <param name="childControl">The child control that's nested within a base windows form.</param>
		/// <returns>The windows form that contains the control, or NULL (Nothing in VB.Net) if the control is not embedded on a form at some level.</returns>
		/// <remarks>If the childControl is a form, then the method will return the childControl instance.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="childControl"/> parameter is NULL (Nothing in VB.Net).</exception>
		public static Form GetTopLevelForm(Control childControl)
		{
			Form result = null;
			Control parent = null;

			if (childControl == null)
				throw new ArgumentNullException("childControl");

			result = childControl as Form;

			if (result != null)
				return result;

			parent = childControl.Parent;

			while (parent != null)
			{
				result = parent as Form;
				if (result != null)
					break;

				parent = parent.Parent;
			}

			return result;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes the <see cref="Gorgon"/> class.
		/// </summary>
		static Gorgon()		
		{
			_trackedObjects = new GorgonTrackedObjectCollection();
			PlugIns = new GorgonPlugInFactory();

			// Re-open the log if it's closed.
			if (Log.IsClosed)
			{
				try
				{
					Log.Open();
				}
#if DEBUG
				catch (Exception ex)
				{
					// Only note this in DEBUG mode.
					UI.GorgonDialogs.ErrorBox(ApplicationForm, ex);
#else
				catch
				{
#endif
				}
			}

			GorgonException.Log = Log;
			Log.Print("Initializing...", GorgonLoggingLevel.All);
			Log.Print("Architecture: {0}", GorgonLoggingLevel.Verbose, GorgonComputerInfo.PlatformArchitecture);
			Log.Print("Installed Memory: {0}", GorgonLoggingLevel.Verbose, GorgonComputerInfo.TotalPhysicalRAM.FormatMemory());
			Log.Print("Available Memory: {0}", GorgonLoggingLevel.Verbose, GorgonComputerInfo.AvailablePhysicalRAM.FormatMemory());

			// Default to using 10 milliseconds of sleep time when the application is not focused.
			UnfocusedSleepTime = 10;
		}
		#endregion
	}
}
