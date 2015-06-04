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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Collections.Specialized;
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;
using Gorgon.IO;
using Gorgon.Native;
using Gorgon.Plugins;

namespace Gorgon.Core
{
	/// <summary>
	/// The gorgon application.
	/// </summary>
	/// <remarks>Use this to replace the Application.Run(new Form()) method in the startup function.
	/// <para>The application uses an <see cref="GorgonApplication.ApplicationIdleLoopMethod">idle loop method</see> to call the users code when it is running.  <see cref="GorgonApplication.ApplicationForm">A form</see> may also be assigned as the primary form for the application.</para>
	/// <para>An application is started by calling its <see cref="GorgonApplication.Run(System.Windows.Forms.Form, Func{bool})">Run method</see>.  An application can be shut down by calling its <see cref="M:GorgonLibrary.Gorgon.Quit">Quit</see> method.  Applications with a main form will end when the form is closed.  
	/// Alternatively, the application can be terminated by returning <b>false</b> from the idle loop method.</para>
	/// <para>Any objects created in Gorgon, such as the Graphics interface, will be destroyed when the application ends.</para>
	/// </remarks>
	public static class GorgonApplication
	{
		#region Constants.
		private const string LogFile = "GorgonLibrary";				// Log file application name.
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when the application is about to exit.
		/// </summary>
		/// <remarks>Be sure to remove any event handlers assigned to this event within the event handler, otherwise the application may still retain memory and cause a leak.</remarks>
		public static event EventHandler Exit;
		/// <summary>
		/// Event fired when a message pump thread is about to exit.
		/// </summary>
		/// <remarks>Be sure to remove any event handlers assigned to this event within the event handler, otherwise the application may still retain memory and cause a leak.</remarks>
		public static event EventHandler ThreadExit;
		#endregion

		#region Variables.
		// Tracked objects.
        private static readonly GorgonDisposableObjectCollection _trackedObjects = new GorgonDisposableObjectCollection();
		// Main application form.
        private static Form _mainForm;
		// Flag to indicate that the application needs to close.
		private static bool _quitSignalled;
		// Application loop method.														
		private static Func<bool> _loop;
		// Flag to indicate that frame rate timing has started.
        private static bool _timingStarted = true;
		// The log interface to use.
		private static IGorgonLog _log;
		// The dummy log interface.
		private readonly static GorgonLogDummy _dummyLog = new GorgonLogDummy();
		// A synchronization object for threads.
		private readonly static object _syncLock = new object();
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return if the application has focus.
		/// </summary>
		private static bool HasFocus
		{
			get
			{
			    if ((AllowBackground) || (ApplicationForm == null))
			    {
			        return true;
			    }

			    return (ApplicationForm.WindowState != FormWindowState.Minimized) && (ApplicationForm.ContainsFocus);
			}
		}

		/// <summary>
		/// Property to return the ID of the application thread.
		/// </summary>
		public static int ThreadID
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
		/// <para>This property is ignore when <see cref="P:GorgonLibrary.Gorgon.AllowBackground">AllowBackground</see> is set to <b>false</b>.</para>
		/// </remarks>
		public static int UnfocusedSleepTime
		{
			get;
			set;
		}

		/// <summary>
		/// Property to allow the idle loop to continue running while the window is not focused or minimized.
		/// </summary>
		/// <remarks>This is set to <b>true</b> by default, and this means that the code in the idle loop will continue to execute when the window is not focused or minimized.  When it is <b>false</b>, the application will suspend until it regains focus.
		/// <para>There will be a delay for code that is executing in the background when the <see cref="P:GorgonLibrary.Gorgon.UnfocusedSleepTime">UnfocusedSleepTime</see> property is set greater than 0.</para>
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
		/// </remarks>
		public static Func<bool> ApplicationIdleLoopMethod
		{
			get
			{
				return _loop;
			}
			set
			{
				// We can't set all to NULL.
			    if ((ApplicationForm == null)
			        && (value == null)
			        && (ApplicationContext == null))
			    {
			        return;
			    }

			    // Remove the previous event.
				if (IsRunning)
				{
					Application.Idle -= Application_Idle;
					Log.Print("Application loop stopped.", LoggingLevel.Simple);
				}
				
				_loop = value;

				if ((value == null) || (!IsRunning))
				{
					return;
				}

				Log.Print("Application loop starting...", LoggingLevel.Simple);
				Application.Idle += Application_Idle;
			}
		}

		/// <summary>
		/// Property to return the directory for the currently running application.
		/// </summary>
		/// <remarks>This does not include the name of the assembly that is executing.</remarks>
		public static string ApplicationDirectory
		{
			get
			{
				return Application.StartupPath.FormatDirectory(Path.DirectorySeparatorChar);
			}
		}

		/// <summary>
		/// Property to return the path for the currently running application.
		/// </summary>
		/// <remarks>This includes the name of the assembly that is executing.</remarks>
		public static string ApplicationPath
		{
			get
			{
				return Application.ExecutablePath;
			}
		}

		/// <summary>
		/// Property to return if the app is in a running state or not.
		/// </summary>
		/// <remarks>This flag is set to <b>true</b> when the application is in a running state and <b>false</b> when it is not.</remarks>
		/// <value><b>true</b> if the application is running, and <b>false</b> if not.</value>
		public static bool IsRunning
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the primary window for the application.
		/// </summary>
		public static Form ApplicationForm
		{
			get
			{
				return ApplicationContext != null ? ApplicationContext.MainForm : _mainForm;
			}
		}

		/// <summary>
		/// Property to set or return the application context.
		/// </summary>
		public static ApplicationContext ApplicationContext
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set return the library log file interface.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Setting this value to <b>null</b> will turn off logging for the application, but will not set the value to <b>null</b>. A dummy 
		/// version of the log, that does no actual logging, will be used instead.
		/// </para>
		/// <para>
		/// Ensure that the log being assigned to this property was created on the same thread as the application, otherwise an exception may be thrown.
		/// </para>
		/// </remarks>
		public static IGorgonLog Log
		{
			get
			{
				return _log;
			}
			set
			{
				lock (_syncLock)
				{
					if (_log != null)
					{
						_log.Close();
					}

					if (value == null)
					{
						_log = _dummyLog;
						return;
					}

					_log = value;

					InitializeLogger();
				}
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
		/// Handles the Idle event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private static void Application_Idle(object sender, EventArgs e)
		{
			MSG message;		            // Message to retrieve.

			// We have nothing to execute, just leave.
			if ((ApplicationIdleLoopMethod == null) || (!IsRunning))
			{
				return;
			}

			while ((HasFocus) && (!Win32API.PeekMessage(out message, IntPtr.Zero, 0, 0, PeekMessageFlags.NoRemove)))
			{
                // Reset the timer so that frame rate timing can start with the first iteration of the loop.
			    if (_timingStarted)
			    {
			        GorgonTiming.Reset();
			        _timingStarted = false;
			    }

				GorgonTiming.Update();
				
				if (!ApplicationIdleLoopMethod())
				{
					// Force an exit from the thread.
					Application.Exit();
					return;
				}

				// Give up CPU time if we're not focused.
				if ((ApplicationForm != null) && (!ApplicationForm.ContainsFocus) && (UnfocusedSleepTime > 0))
				{
					Thread.Sleep(UnfocusedSleepTime);
				}
			}
		}

		/// <summary>
		/// Function to perform initialization of the logger interface.
		/// </summary>
		private static void InitializeLogger()
		{
			if (Log.IsClosed)
			{
				// Open the log if it's closed so we can begin logging.
				try
				{
					_log.Open();
				}
				catch (Exception ex)
				{
					// Only note this in DEBUG mode.
					Debug.Print("Error opening the log file: {0}", ex.Message);
				}
			}

			// Display information
			Log.Print("Logging interface assigned. Initializing...", LoggingLevel.All);
			Log.Print("Architecture: {0}", LoggingLevel.Verbose, GorgonComputerInfo.PlatformArchitecture);
			Log.Print("Processor count: {0}", LoggingLevel.Verbose, GorgonComputerInfo.ProcessorCount);
			Log.Print("Installed Memory: {0}", LoggingLevel.Verbose, GorgonComputerInfo.TotalPhysicalRAM.FormatMemory());
			Log.Print("Available Memory: {0}", LoggingLevel.Verbose, GorgonComputerInfo.AvailablePhysicalRAM.FormatMemory());
			Log.Print("Operating System: {0} ({1})", LoggingLevel.Verbose, GorgonComputerInfo.OperatingSystemVersionText, GorgonComputerInfo.OperatingSystemArchitecture); 
			Log.Print(string.Empty, LoggingLevel.Verbose);
		}

		/// <summary>
		/// Function to initialize the main form and idle loop..
		/// </summary>
		/// <returns><b>true</b> if the application has signalled to quit before it starts running, <b>false</b> to continue.</returns>
		private static bool Initialize()
		{
			// Attach assembly resolving to deal with issues when loading assemblies with designers/type converters.
		    AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;

		    // Display the form.
			if ((ApplicationForm != null) && (!ApplicationForm.IsDisposed))
			{
				ApplicationForm.Show();
			}

			if ((ApplicationIdleLoopMethod != null) && (!_quitSignalled))
			{
				Log.Print("Application loop starting...", LoggingLevel.Simple);
				Application.Idle += Application_Idle;
			}

			// Register quit handlers.
			Application.ApplicationExit += Application_ApplicationExit;
			Application.ThreadExit += Application_ThreadExit;

			return _quitSignalled;
		}

		/// <summary>
		/// Handles the ThreadExit event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private static void Application_ThreadExit(object sender, EventArgs e)
		{
			if (ThreadExit != null)
			{
				ThreadExit(sender, e);
			}
		}

		/// <summary>
		/// Handles the ApplicationExit event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private static void Application_ApplicationExit(object sender, EventArgs e)
		{
			if (Exit != null)
			{
				Exit(sender, e);
			}
		}

		/// <summary>
		/// Function to clean up after an application exits.
		/// </summary>
		private static void CleanUp()
		{
			IsRunning = false;

			// Attach assembly resolving to deal with issues when loading assemblies with designers/type converters.
	        AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly;

		    // Remove quit handlers.
			Application.ApplicationExit -= Application_ApplicationExit;
			Application.ThreadExit -= Application_ThreadExit;

			if (ApplicationIdleLoopMethod != null)
			{
				Application.Idle -= Application_Idle;
				Log.Print("Application loop stopped.", LoggingLevel.Simple);
			}

			if (_trackedObjects != null)
			{
				_trackedObjects.Clear();
			}

			PlugIns.UnloadAll();

			// Reset the low resolution timer period on application end.
			if (GorgonTimer.UsingLowResTimers)
			{
				GorgonTimer.ResetLowResTimerPeriod();
			}

			Log.Print("Shutting down.", LoggingLevel.All);

			// Destroy log.
			if (!Log.IsClosed)
			{
				Log.Close();
			}
		}

		/// <summary>
		/// Function called when a type needs to be resolved from another assembly in the current app domain.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="args">Event arguments.</param>
		/// <returns>The assembly, if found, NULL if not.</returns>
		private static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
		{
            if ((PlugIns != null) && (PlugIns.AssemblyResolver != null))
            {
                return PlugIns.AssemblyResolver((AppDomain)sender, args);
            }

			return null;
		}

		/// <summary>
		/// Function to a list of objects being tracked by a type value.
		/// </summary>
		/// <typeparam name="T">Type to search for.</typeparam>
		/// <returns>A list of objects that match the type.</returns>
		public static IList<T> GetTrackedObjectsOfType<T>()
			where T : IDisposable
		{
			return (from trackedObject in _trackedObjects
				   where trackedObject is T
				   select (T)trackedObject).ToArray();
		}

		/// <summary>
		/// Function to quit the application.
		/// </summary>
		public static void Quit()
		{
			_quitSignalled = true;

			if (IsRunning)
			{
				Application.Exit();
			}			
		}

		/// <summary>
		/// Function to run a Gorgon application.
		/// </summary>
		/// <param name="context">Application context to use.</param>
		/// <param name="loop">[Optional] Idle loop method for the application.</param>
		/// <remarks>This method requires an application context, but the <paramref name="loop"/> parameter is optional.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="context"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the application is already in a <see cref="P:GorgonLibrary.Gorgon.IsRunning">running state</see>.</exception>
		public static void Run(ApplicationContext context, Func<bool> loop = null)
		{
		    if (context == null)
		    {
		        throw new InvalidOperationException(Resources.GOR_NOCONTEXT);
		    }

            if (IsRunning)
            {
                throw new InvalidOperationException(Resources.GOR_APPLICATION_ALREADY_RUNNING);
            }

		    if (loop != null)
		    {
		        ApplicationIdleLoopMethod = loop;
		    }

			ApplicationContext = context;

			try
			{
			    if (Initialize())
			    {
			        return;
			    }

			    IsRunning = true;
			    _timingStarted = true;
                Application.Run(context);
			}
			catch (Exception ex)
			{
				Log.LogException(ex);
				throw;
			}
			finally
			{
				CleanUp();
			}
		}

		/// <summary>
		/// Function to run a Gorgon application.
		/// </summary>
		/// <param name="mainForm">Form to use as the main form for the application.</param>
		/// <param name="loop">[Optional] Idle loop method for the application.</param>
		/// <remarks>A form is required to use this method, but the <paramref name="loop"/> parameter is optional.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="mainForm"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the application is already in a <see cref="P:GorgonLibrary.Gorgon.IsRunning">running state</see>.</exception>
		public static void Run(Form mainForm, Func<bool> loop = null)
		{
			if (mainForm == null)
			{
				throw new ArgumentNullException("mainForm");
			}

			if (IsRunning)
			{
				throw new InvalidOperationException(Resources.GOR_APPLICATION_ALREADY_RUNNING);
			}

			if (loop != null)
			{
				ApplicationIdleLoopMethod = loop;
			}

			_mainForm = mainForm;

			try
			{
				if (Initialize())
				{
					return;
				}

				IsRunning = true;
                _timingStarted = true;
				Application.Run(ApplicationForm);
			}
			catch (Exception ex)
			{
				Log.LogException(ex);
				throw;
			}
			finally
			{
				CleanUp();
			}
		}

		/// <summary>
		/// Function to run a Gorgon application.
		/// </summary>
		/// <param name="loop">Idle loop method for the application.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="loop"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the application is already in a <see cref="P:GorgonLibrary.Gorgon.IsRunning">running state</see>.</exception>
		public static void Run(Func<bool> loop)
		{
			if (loop == null)
			{
				throw new ArgumentNullException("loop");
			}

			if (IsRunning)
			{
			    throw new InvalidOperationException(Resources.GOR_APPLICATION_ALREADY_RUNNING);
			}

			ApplicationIdleLoopMethod = loop;

			try
			{
				if (Initialize())
				{
					return;
				}

				IsRunning = true;
                _timingStarted = true;
				Application.Run();
			}
			catch (Exception ex)
			{
				Log.LogException(ex);
				throw;
			}
			finally
			{
				CleanUp();
			}
		}

		/// <summary>
		/// Function to add an object for tracking by the main Gorgon interface.
		/// </summary>
		/// <param name="trackedObject">Object to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="trackedObject"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>This allows Gorgon to track objects and destroy them upon <see cref="GorgonApplication.Quit">termination</see>.</remarks>
		public static void AddTrackedObject(IDisposable trackedObject)
		{
		    if (trackedObject == null)
		    {
		        throw new ArgumentNullException("trackedObject");
		    }

		    _trackedObjects.Add(trackedObject);
		}

		/// <summary>
		/// Function to remove a tracked object from the Gorgon interface.
		/// </summary>
		/// <param name="trackedObject">Object to remove.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="trackedObject"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>This will -not- destroy the tracked object.</remarks>
		public static void RemoveTrackedObject(IDisposable trackedObject)
		{
			if (trackedObject == null)
			{
				throw new ArgumentNullException("trackedObject");
			}

			_trackedObjects.Remove(trackedObject);
		}

		/// <summary>
		/// Function to return the top level control that contains the child control.
		/// </summary>
		/// <param name="childControl">The child control that's nested within a container control.</param>
		/// <returns>The container control that contains the control, or the control pointed at by <paramref name="childControl"/> if the control has no parent.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="childControl"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		public static Control GetTopLevelControl(Control childControl)
		{
			if (childControl == null)
			{
				throw new ArgumentNullException("childControl");
			}

			Control parent = childControl;

			while (parent.Parent != null)
			{
				parent = parent.Parent;
			}

			return parent;
		}

		/// <summary>
		/// Function to return the top level form that contains the child control.
		/// </summary>
		/// <param name="childControl">The child control that's nested within a base windows form.</param>
		/// <returns>The windows form that contains the control, or NULL (<i>Nothing</i> in VB.Net) if the control is not embedded on a form at some level.</returns>
		/// <remarks>If the childControl is a form, then the method will return the childControl instance.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="childControl"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		public static Form GetTopLevelForm(Control childControl)
		{
		    if (childControl == null)
		    {
		        throw new ArgumentNullException("childControl");
		    }

		    var result = childControl as Form;

		    if (result != null)
		    {
		        return result;
		    }

		    Control parent = childControl.Parent;

			while (parent != null)
			{
				result = parent as Form;
				if (result != null)
				{
					break;
				}

				parent = parent.Parent;
			}

			return result;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes the <see cref="GorgonApplication"/> class.
		/// </summary>
		static GorgonApplication()
		{
			ThreadID = Thread.CurrentThread.ManagedThreadId;

			PlugIns = new GorgonPlugInFactory();
			Log = new GorgonLogFile(LogFile, "Tape_Worm");

			// Default to using 10 milliseconds of sleep time when the application is not focused.
			UnfocusedSleepTime = 10;

            // Initializing application timing.
            GorgonTiming.Reset();
		}
		#endregion
	}
}
