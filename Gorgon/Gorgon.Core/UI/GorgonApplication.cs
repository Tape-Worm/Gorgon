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
using System.Threading;
using System.Windows.Forms;
using Gorgon.Collections.Specialized;
using Gorgon.Core;
using Gorgon.Core.Properties;
using Gorgon.Diagnostics;
using Gorgon.IO;
using Gorgon.Native;
using Gorgon.Timing;

namespace Gorgon.UI
{
	/// <summary>
	/// An application object for windows <see cref="Form"/> applications.
	/// </summary> 
	/// <remarks>
	/// <para>
	/// This class is meant as a replacement for the standard windows forms <see cref="Application"/> class. It expands the functionality of the application class by exposing useful functionality for 
	/// working with the main application <see cref="Form"/>.
	/// </para>
	/// <para>
	/// One of the key components to this class is the introduction of a proper Idle loop. This allows an application to perform operations while the windows message pump is in an idle state. This is useful 
	/// for things like games, or applications that require constant interaction with other systems. To set an idle loop you may pass a method to execute in one of the <see cref="O:Gorgon.UI.GorgonApplication.Run">Run</see> 
	/// overloads. Or, if you choose, you may assign an idle method to execute at any point in the application life cycle by assigning a method to the <see cref="ApplicationIdleLoopMethod"/>.
	/// </para> 
	/// <para>
	/// Like the <see cref="Application"/> class, this class also provides the ability to pass a windows <see cref="Form"/>, or <see cref="ApplicationContext"/> to one of the <see cref="O:Gorgon.UI.GorgonApplication.Run">Run</see> 
	/// overloads. 
	/// </para>
	/// <para>
	/// <note type="tip">
	/// <para>
	/// When passing a form to the <see cref="Run(System.Windows.Forms.Form,System.Func{bool})"/> method, that form automatically becomes the main application window. The application will exit when this form is closed. 
	/// The main form of an application may be retrieved from the <see cref="MainForm"/> property. 
	/// </para>
	/// <para>
	/// If this is not suitable, one of the other <see cref="O:Gorgon.UI.GorgonApplication.Run">Run</see> overloads will allow you to finely control the life cycle of your application.
	/// </para>
	/// </note>
	/// </para>
	/// <para>
	/// This object can also be used to track the life cycle of <see cref="IDisposable"/> objects for the lifetime of the application by using the <see cref="AddTrackedObject"/> method. When the application shuts 
	/// down, the registered <see cref="IDisposable"/> objects will be automatically disposed. 
	/// </para>
	/// <para>
	/// <note type="tip">
	/// <para>
	/// If the application doesn't need to track a specific <see cref="IDisposable"/> object anymore, then a call to the <see cref="RemoveTrackedObject"/> method will remove it from the registration list and the 
	/// application will not dispose of it automatically.
	/// </para>
	/// </note>
	/// </para>
	/// </remarks>
	/// <example>
	/// An application that wishes to use this class instead of the <see cref="Application"/> class should follow this pattern:
	/// <code language="csharp">
	/// <![CDATA[
	/// // In program.cs:
	/// [STAThread]
	///	static void Main()
	///	{
	///		try
	///		{
	///			Application.EnableVisualStyles();
	///			Application.SetCompatibleTextRenderingDefault(false);
	///
	///			// Perhaps you could run initialization stuff here.			
	///
	///			// This replaces:
	///			// Application.Run(new YourMainForm());
	///			
	///			// This will run with a form and an idle method named "Idle"
	///			GorgonApplication.Run(new YourMainForm(), Idle);
	///		}
	///		catch (Exception ex)
	///		{
	///			// You should always catch any unhandled exceptions and either log them, or display them, or something.
	///			// It can get real ugly for your users otherwise.
	///			ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), GorgonApplication.Log);
	///		}
	///		finally
	///		{
	///			// Do your clean up here.
	///		}
	///	}
	/// ]]>
	/// </code>
	/// </example>
	public static class GorgonApplication
	{
		#region Constants.
		// Log file application name.
		private const string LogFile = "GorgonLibrary";
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when the application is about to exit.
		/// </summary>
		public static event EventHandler Exit;
		/// <summary>
		/// Event fired when a message pump thread is about to exit.
		/// </summary>
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
		// The log interface to use.
		private static IGorgonLog _log;
		// The dummy log interface.
		private readonly static GorgonLogDummy _dummyLog = new GorgonLogDummy();
		// A synchronization object for threads.
		private readonly static object _syncLock = new object();
		// The number of milliseconds to sleep while the application is unfocused but running in the background.
		private static int _unfocusedSleepTime = 10;
		// An atomic to ensure that run is only called by 1 thread at a time.
		private static int _runAtomic;
		// Timer used for timing the application.
		private static Lazy<IGorgonTimer> _applicationTimer;
		// Event used to put the application to sleep.
		private static readonly ManualResetEventSlim _unfocusedTimeout = new ManualResetEventSlim(false, 20);
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return if the main <see cref="MainForm"/> has focus.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This determines whether the <see cref="MainForm"/> is focused (i.e. a top level, active window). It does this by ensuring the <see cref="Form.WindowState"/> is not set to 
		/// <see cref="FormWindowState.Minimized"/>, and the window, or one of its child controls, currently has input focus.
		/// </para>
		/// <para>
		/// If no <see cref="MainForm"/> is assigned, then this property will always return <b>false</b>. Otherwise, if the <see cref="AllowBackground"/> property is set to <b>true</b>, then 
		/// this property will always return <b>true</b>. 
		/// </para>
		/// </remarks>
		public static bool HasFocus
		{
			get
			{
				if (MainForm == null)
				{
					return false;
				}

			    if (AllowBackground)
			    {
			        return true;
			    }

			    return (MainForm.WindowState != FormWindowState.Minimized) && (MainForm.ContainsFocus);
			}
		}

		/// <summary>
		/// Property to return the ID of the application UI thread.
		/// </summary>
		public static int ThreadID
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the amount of time in milliseconds to sleep when the application window is not focused or minimized.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Settings this property to a value other than zero will tell the <see cref="GorgonApplication"/> to put the message pump to sleep for the requested number of milliseconds while the application is not 
		/// focused. A value of zero, will tell the application to continuously process messages regardless of focus state.
		/// </para>
		/// <para>
		/// This has value for systems that run on batteries, like a laptop, where unnecessary processing can be detrimental to battery life. 
		/// </para>
		/// <para>
		/// <note type="tip">
		/// <para>
		/// This value has no effect if the <see cref="AllowBackground"/> property is set to <b>false</b>.
		/// </para>
		/// </note>
		/// </para>
		/// <para>
		/// <note type="warning">
		/// <para>
		/// Do not set this value to a large number, doing so will stall the message pump and cause issues for the application. A value of 10 milliseconds (the default) is small enough to keep the application 
		/// responsive.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public static int UnfocusedSleepTime
		{
			get
			{
				return _unfocusedSleepTime;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}

				_unfocusedSleepTime = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to allow the application message pump to continue running while the window is not focused or minimized.
		/// </summary>
		/// <remarks>
		/// <para>
		/// When set to <b>true</b>, this will allow an application to process messages and execute the <see cref="ApplicationIdleLoopMethod"/> while the application is unfocused or minimized. If it 
		/// is set to <b>false</b>, the application will pause until it regains focus again. 
		/// </para>
		/// <para>
		/// It is recommended that this property be left at the default setting of <b>false</b> unless it's absolutely necessary to run the application in the background. On systems that use batteries, like 
		/// laptops, setting this value to <b>true</b> could be detrimental to the battery life.
		/// </para>
		/// <para>
		/// <note type="tip">
		/// <para>
		/// If this property is set to <b>true</b>, there will be a delay corresponding to the value set on the <see cref="UnfocusedSleepTime"/> property. If that property is set to zero, then the 
		/// application will process messages as though it were focused.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public static bool AllowBackground
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the method to execute user code while the application is in an idle state.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This property can be set at any time during the lifetime of the application. As soon as the application stops processing messages, it will execute the <see cref="Func{TResult}"/> assigned to it.
		/// </para>
		/// <para>
		/// The property expects a function that returns a <see cref="bool"/> value. When this return value is <b>true</b>, then the application will continue processing messages. If it returns <b>false</b>, 
		/// then the application will exit its message loop and shut down.
		/// </para>
		/// <para>
		/// Setting this value to <b>null</b> (<i>Nothing</i> in VB.Net) when no <see cref="MainForm"/> or <see cref="ApplicationContext"/> is assigned will lead to an exception being thrown. This is 
		/// because this method is the only thing the application has to do, and without it, there'd be no application processing.
		/// </para>
		/// <para> 
		/// <note type="warning">
		/// <para>
		/// Do not set this to a non-<b>null</b> value when shutting down the application. Doing this will lead to undefined behaviour.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// Thrown when an attempt to set this property to <b>null</b> (<i>Nothing</i> in VB.Net) is made and there is no <see cref="MainForm"/> or <see cref="ApplicationContext"/> 
		/// attached to the application.
		/// </exception>
		public static Func<bool> ApplicationIdleLoopMethod
		{
			get
			{
				return _loop;
			}
			set
			{
				// If we have no application form, or context and we try disable the loop, leave. 
				// We do this because if there's no context or form, then there's nothing to run and that's an invalid state.
			    if ((value == null)
					&& (MainForm == null)
			        && (ApplicationContext == null))
			    {
			        throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_CANNOT_REMOVE_IDLE_LOOP);
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
		/// <remarks>
		/// This does not include the name of the assembly that is executing, only the directory that the application was executed from.
		/// </remarks>
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
		/// <remarks>
		/// This includes the file name of the assembly that is executing as well as the directory that the application was executed from.
		/// </remarks>
		public static string ApplicationPath
		{
			get
			{
				return Application.ExecutablePath;
			}
		}

		/// <summary>
		/// Property to return if the <see cref="GorgonApplication"/> is in a running state or not.
		/// </summary>
		/// <remarks>
		/// When this value is <b>true</b>, the application message pump is running and processing messages.
		/// </remarks>
		public static bool IsRunning
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the main windows <see cref="Form"/> for the application.
		/// </summary>
		/// <remarks>
		/// This value is set when the <see cref="Run(System.Windows.Forms.Form,System.Func{bool})"/> is used to run the application, or a form has been assigned to the <see cref="System.Windows.Forms.ApplicationContext.MainForm"/> 
		/// property.
		/// </remarks>
		public static Form MainForm
		{
			get
			{
				return ApplicationContext != null ? ApplicationContext.MainForm : _mainForm;
			}
		}

		/// <summary>
		/// Property to set or return the current application context.
		/// </summary>
		/// <remarks>
		/// This value will return <b>null</b> (<i>Nothing</i> in VB.Net) if this application wasn't started with the <see cref="Run(System.Windows.Forms.ApplicationContext,System.Func{bool})"/> method.
		/// </remarks>
		public static ApplicationContext ApplicationContext
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set return the application log file interface.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Setting this value to <b>null</b> will turn off logging for the application, but will not set the value to <b>null</b>. A dummy version of the log, that does no actual logging, will be used 
		/// instead.
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
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Idle event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private static void Application_Idle(object sender, EventArgs e)
		{
			MSG message;	// Windows message to retrieve.

			// We have nothing to execute, so just leave.
			if ((ApplicationIdleLoopMethod == null) || (!IsRunning))
			{
				return;
			}

			// Check for application focus. If we didn't assign a main form, or one was not assigned to the application context, then 
			// run regardless since we have an idle method to execute.
			while (((HasFocus) || (MainForm == null)) && (!Win32API.PeekMessage(out message, IntPtr.Zero, 0, 0, PeekMessageFlags.NoRemove)))
			{
                // Reset the timer so that frame rate timing can start with the first iteration of the loop.
			    if (!_applicationTimer.IsValueCreated)
			    {
				    GorgonTiming.Timer = _applicationTimer.Value;
			    }

				GorgonTiming.Update();
				
				if (!ApplicationIdleLoopMethod())
				{
					// Force an exit from the thread.
					Quit();
					return;
				}

				// Give up CPU time if we're not focused.
				if ((MainForm == null) || (MainForm.ContainsFocus) || (_unfocusedSleepTime <= 0))
				{
					continue;
				}

				// Put the application to bed until the elapsed timeout is triggered.
				_unfocusedTimeout.Wait(_unfocusedSleepTime);
				
				if (_quitSignalled)
				{
					break;
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
			// Initialize the timing data.
			// This will start the application uptime counter.  But since there's no actual timer set on this yet,
			// no statistical timing will be collected until the application begins its run in Application_Idle.
			GorgonTiming.Reset();

			// Initialize the timer. By creating them as lazy instantiated objects, we can ensure that they won't start 
			// until the application completes processing message its queue on first run.
			if (GorgonTimerQpc.SupportsQpc())
			{
				_applicationTimer = new Lazy<IGorgonTimer>(() => new GorgonTimerQpc());
			}
			else
			{
				// Set the period to 1 millisecond before using the multimedia timing, otherwise things may not be 
				// accurate. 
				_applicationTimer = new Lazy<IGorgonTimer>(() =>
				                                           {
					                                           GorgonTimerMultimedia.BeginTiming();
					                                           return new GorgonTimerMultimedia();
				                                           });
			}

		    // Display the form.
			if ((MainForm != null) && (!MainForm.IsDisposed))
			{
				MainForm.Show();
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
			// This will only allow one thread to actually call this.
			// It also has the added benefit of unassigning the exit handler when the application shuts down.
			EventHandler exitHandler = Interlocked.Exchange(ref ThreadExit, null);

			if (exitHandler != null)
			{
				exitHandler(sender, e);
			}

			Application.ThreadExit -= Application_ThreadExit;
		}

		/// <summary>
		/// Handles the ApplicationExit event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private static void Application_ApplicationExit(object sender, EventArgs e)
		{
			// This will only allow one thread to actually call this.
			// It also has the added benefit of unassigning the exit handler when the application shuts down.
			EventHandler exitHandler = Interlocked.Exchange(ref Exit, null);

			if (exitHandler != null)
			{
				exitHandler(sender, e);
			}

			Application.ApplicationExit -= Application_ApplicationExit;
		}

		/// <summary>
		/// Function to clean up after an application exits.
		/// </summary>
		private static void CleanUp()
		{
			IsRunning = false;

			_unfocusedTimeout.Dispose();

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

			// Reset the low resolution timer period on application end.
			if ((GorgonTiming.Timer != null) && (GorgonTiming.Timer is GorgonTimerMultimedia))
			{
				GorgonTimerMultimedia.EndTiming();
			}

			Log.Print("Shutting down.", LoggingLevel.All);

			// Destroy log.
			if (!Log.IsClosed)
			{
				Log.Close();
			}
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
			_unfocusedTimeout.Set();

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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="context"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the application is already in a <see cref="P:GorgonLibrary.Gorgon.IsRunning">running state</see>.</exception>
		public static void Run(ApplicationContext context, Func<bool> loop = null)
		{
		    if (context == null)
		    {
		        throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_NOCONTEXT);
		    }

            if (IsRunning)
            {
                throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_ALREADY_RUNNING);
            }

			if (Interlocked.Increment(ref _runAtomic) > 1)
			{
				throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_RUN_ONLY_FROM_ONE_THREAD);
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
                Application.Run(context);
			}
			catch (Exception ex)
			{
				_unfocusedTimeout.Set();
				Log.LogException(ex);
				throw;
			}
			finally
			{
				CleanUp();
				Interlocked.Decrement(ref _runAtomic);
			}
		}

		/// <summary>
		/// Function to run a Gorgon application.
		/// </summary>
		/// <param name="mainForm">Form to use as the main form for the application.</param>
		/// <param name="loop">[Optional] Idle loop method for the application.</param>
		/// <remarks>A form is required to use this method, but the <paramref name="loop"/> parameter is optional.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="mainForm"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the application is already in a <see cref="P:GorgonLibrary.Gorgon.IsRunning">running state</see>.</exception>
		public static void Run(Form mainForm, Func<bool> loop = null)
		{
			if (mainForm == null)
			{
				throw new ArgumentNullException("mainForm");
			}

			if (IsRunning)
			{
				throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_ALREADY_RUNNING);
			}

			if (Interlocked.Increment(ref _runAtomic) > 1)
			{
				throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_RUN_ONLY_FROM_ONE_THREAD);
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
				Application.Run(MainForm);
			}
			catch (Exception ex)
			{
				_unfocusedTimeout.Set();
				Log.LogException(ex);
				throw;
			}
			finally
			{
				CleanUp();
				Interlocked.Decrement(ref _runAtomic);
			}
		}

		/// <summary>
		/// Function to run a Gorgon application.
		/// </summary>
		/// <param name="loop">Idle loop method for the application.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="loop"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the application is already in a <see cref="P:GorgonLibrary.Gorgon.IsRunning">running state</see>.</exception>
		public static void Run(Func<bool> loop)
		{
			if (loop == null)
			{
				throw new ArgumentNullException("loop");
			}

			if (IsRunning)
			{
			    throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_ALREADY_RUNNING);
			}

			if (Interlocked.Increment(ref _runAtomic) > 1)
			{
				throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_RUN_ONLY_FROM_ONE_THREAD);
			}

			ApplicationIdleLoopMethod = loop;

			try
			{
				if (Initialize())
				{
					return;
				}

				IsRunning = true;
                Application.Run();
			}
			catch (Exception ex)
			{
				_unfocusedTimeout.Set();
				Log.LogException(ex);
				throw;
			}
			finally
			{
				CleanUp();
				Interlocked.Decrement(ref _runAtomic);
			}
		}

		/// <summary>
		/// Function to add an object for tracking by the main Gorgon interface.
		/// </summary>
		/// <param name="trackedObject">Object to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="trackedObject"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="trackedObject"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
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
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="childControl"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
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
		/// <returns>The windows form that contains the control, or <b>null</b> (<i>Nothing</i> in VB.Net) if the control is not embedded on a form at some level.</returns>
		/// <remarks>If the childControl is a form, then the method will return the childControl instance.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="childControl"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
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
			
			Log = new GorgonLogFile(LogFile, "Tape_Worm");
		}
		#endregion
	}
}
