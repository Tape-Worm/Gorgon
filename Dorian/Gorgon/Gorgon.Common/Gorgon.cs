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
	/// Delegate for an application loop method.
	/// </summary>
	/// <param name="timingData">Data used for frame rate timing.</param>
	/// <returns>TRUE to continue executing the application, FALSE to end the application.</returns>
	/// <remarks>Use this to execute custom code during idle time in the application.
	/// <para>Returning FALSE will cause the application to shut down.  When the application shuts down, the main form, and any objects that are tracked by Gorgon (like the input or graphics) will be destroyed.</para>
	/// </remarks>
	public delegate bool ApplicationLoopMethod(GorgonFrameRate timingData);
	#endregion

	/// <summary>
	/// The gorgon application.
	/// </summary>
	/// <remarks>Use this to replace the Application.Run(new Form()) method in the startup function.
	/// <para>The application uses an <see cref="P:GorgonLibrary.Gorgon.ApplicationIdleLoop">idle loop method</see> to call the users code when it is running.  <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">A form</see> may also be assigned as the primary form for the application.</para>
	/// <para>An application is started by calling its <see cref="M:GorgonLibrary.Gorgon.Run">Run method</see>.  An application can be shut down by calling its <see cref="M:GorgonLibrary.Gorgon.Quit">Quit</see> method.  Applications with a main form will end when the form is closed.  
	/// Alternatively, the application can be terminated by returning FALSE from the idle loop method.</para>
	/// <para>Any objects created in Gorgon, such as the Graphics interface, will be destroyed when the application ends.</para>
	/// </remarks>
	public static class Gorgon
	{
		#region Classes.
		///// <summary>
		///// The application context for Gorgon.
		///// </summary>
		//internal class GorgonContext
		//    : ApplicationContext
		//{
		//    #region Variables.
		//    private GorgonFrameRate _timingData = null;								// Frame rate timing data.
		//    #endregion

		//    #region Properties.
		//    /// <summary>
		//    /// Property to return if the application has focus.
		//    /// </summary>
		//    private bool HasFocus
		//    {
		//        get
		//        {
		//            if ((Gorgon.AllowBackground) || (Gorgon.ApplicationForm == null))
		//                return true;

		//            if ((ApplicationForm.WindowState == FormWindowState.Minimized) || (!ApplicationForm.ContainsFocus))
		//                return false;

		//            return true;
		//        }
		//    }
		//    #endregion

		//    #region Methods.
		//    /// <summary>
		//    /// Handles the Idle event of the Application control.
		//    /// </summary>
		//    /// <param name="sender">The source of the event.</param>
		//    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		//    internal void Application_Idle(object sender, EventArgs e)
		//    {
		//        MSG message = new MSG();		// Message to retrieve.

		//        // We have nothing to execute, just leave.
		//        if ((ApplicationIdleLoopMethod == null) || (!IsRunning))
		//            return;

		//        while ((HasFocus) && (!Win32API.PeekMessage(out message, IntPtr.Zero, 0, 0, PeekMessageFlags.NoRemove)))
		//        {
		//            _timingData.Update();

		//            if (!ApplicationIdleLoopMethod(_timingData))
		//            {
		//                // Force an exit from the thread.
		//                ExitThread();
		//                return;
		//            }

		//            // Give up CPU time if we're not focused.
		//            if ((ApplicationForm != null) && (!ApplicationForm.ContainsFocus) && (UnfocusedSleepTime > 0))
		//                System.Threading.Thread.Sleep(UnfocusedSleepTime);
		//        }
		//    }

		//    /// <summary>
		//    /// Terminates the message loop of the thread.
		//    /// </summary>
		//    protected override void ExitThreadCore()
		//    {
		//        IsRunning = false;
		//        base.ExitThreadCore();
		//    }
		//    #endregion

		//    #region Constructor/Destructor.
		//    /// <summary>
		//    /// Initializes a new instance of the <see cref="Gorgon"/> class.
		//    /// </summary>
		//    /// <param name="form">The main form to use for the application.</param>
		//    public GorgonContext(Form form)
		//    {
		//        _timingData = new GorgonFrameRate();
		//        MainForm = form;
		//    }
		//    #endregion
		//}
		#endregion

		#region Constants.
		private const string _logFile = "GorgonLibrary";				// Log file application name.
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
		private static Form _mainForm = null;									// Main application form.
		private static GorgonFrameRate _timingData = null;						// Frame rate timing data.
		private static bool _quitSignalled = false;								// Flag to indicate that the application needs to close.
		private static ApplicationLoopMethod _loop = null;						// Application loop method.
		private static GorgonTrackedObjectCollection _trackedObjects = null;	// Tracked objects.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return if the application has focus.
		/// </summary>
		private static bool HasFocus
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

		/// <summary>
		/// Property to set or return the amount of time in milliseconds to sleep when the application window is not focused.
		/// </summary>
		/// <remarks>
		/// Set this value to 0 to use all CPU time when the application is not focused.  The default is 10 milliseconds.
		/// <para>This is handy in situations when the application is in the background and processing does not need to continue.  For laptops this means battery savings when the application is not focused.
		/// </para>
		/// <para>This property is ignore when <see cref="P:GorgonLibrary.Gorgon.AllowBackground">AllowBackground</see> is set to FALSE.</para>
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
		public static ApplicationLoopMethod ApplicationIdleLoopMethod
		{
			get
			{
				return _loop;
			}
			set
			{
				// We can't set all to NULL.
				if ((ApplicationForm == null) && (value == null) && (ApplicationContext == null))
					return;

				// Remove the previous event.
				if (IsRunning)
				{
					Application.Idle -= new EventHandler(Application_Idle);
					Log.Print("Application loop stopped.", LoggingLevel.Simple);
				}
				
				_loop = value;

				if ((value != null) && (IsRunning))
				{
					Log.Print("Application loop starting...", LoggingLevel.Simple);
					Application.Idle += new EventHandler(Application_Idle);
				}
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
				return Application.StartupPath;
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
		/// <remarks>This flag is set to TRUE when the <see cref="M:GorgonLibrary.Gorgon.Go">Go</see> method is called and FALSE when the <see cref="M:GorgonLibrary.Gorgon.Stop">Stop</see> method is called.</remarks>
		/// <value>TRUE if the application is running, and FALSE if not.</value>
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
				if (ApplicationContext != null)
					return ApplicationContext.MainForm;

				return _mainForm;
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
		/// Property to return the library log file interface.
		/// </summary>
		public static GorgonLogFile Log
		{
			get;
			private set;
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
			MSG message = new MSG();		// Message to retrieve.

			// We have nothing to execute, just leave.
			if ((ApplicationIdleLoopMethod == null) || (!IsRunning))
				return;

			while ((HasFocus) && (!Win32API.PeekMessage(out message, IntPtr.Zero, 0, 0, PeekMessageFlags.NoRemove)))
			{
				_timingData.Update();
				
				if (!ApplicationIdleLoopMethod(_timingData))
				{
					// Force an exit from the thread.
					Application.Exit();
					return;
				}

				// Give up CPU time if we're not focused.
				if ((ApplicationForm != null) && (!ApplicationForm.ContainsFocus) && (UnfocusedSleepTime > 0))
					System.Threading.Thread.Sleep(UnfocusedSleepTime);
			}
		}

		/// <summary>
		/// Method to initialize the main form and idle loop..
		/// </summary>
		/// <returns>TRUE if the application has signalled to quit before it starts running, FALSE to continue.</returns>
		private static bool Initialize()
		{
			// Display the form.
			if (ApplicationForm != null)
				ApplicationForm.Show();

			if ((ApplicationIdleLoopMethod != null) && (!_quitSignalled))
			{
				Log.Print("Application loop starting...", LoggingLevel.Simple);
				Application.Idle += new EventHandler(Application_Idle);
			}

			// Register quit handlers.
			Application.ApplicationExit += new EventHandler(Application_ApplicationExit);
			Application.ThreadExit += new EventHandler(Application_ThreadExit);

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
				ThreadExit(sender, e);
		}

		/// <summary>
		/// Handles the ApplicationExit event of the Application control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private static void Application_ApplicationExit(object sender, EventArgs e)
		{
			if (Exit != null)
				Exit(sender, e);
		}

		/// <summary>
		/// Method to clean up after an application exits.
		/// </summary>
		private static void CleanUp()
		{
			IsRunning = false;

			// Remove quit handlers.
			Application.ApplicationExit -= new EventHandler(Application_ApplicationExit);
			Application.ThreadExit -= new EventHandler(Application_ThreadExit);

			if (ApplicationIdleLoopMethod != null)
			{
				Application.Idle -= new EventHandler(Application_Idle);
				Log.Print("Application loop stopped.", LoggingLevel.Simple);
			}

			if (_trackedObjects != null)
				_trackedObjects.ReleaseAll();

			PlugIns.UnloadAll();

			Log.Print("Shutting down.", LoggingLevel.All);

			// Destroy log.
			if (!Log.IsClosed)
				Log.Close();
		}

		/// <summary>
		/// Method to quit the application.
		/// </summary>
		public static void Quit()
		{
			_quitSignalled = true;

			if (IsRunning)
			{
				Application.Exit();
				return;
			}			
		}

		/// <summary>
		/// Method to run a Gorgon application.
		/// </summary>
		/// <param name="context">Application context to use.</param>
		/// <param name="loop">Idle loop method for the application.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to both the <paramref name="context"/> and <paramref name="loop"/> parameters will raise an exception.</remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when the mainForm and the loop parameters are NULL.
		/// <para>-or-</para>
		/// Thrown when the application is already in a <see cref="P:GorgonLibrary.Gorgon.IsRunning">running state</see>.
		/// </exception>
		public static void Run(ApplicationContext context, ApplicationLoopMethod loop)
		{
			if ((context == null) && (loop == null))
				throw new InvalidOperationException("Cannot run an application without either a main form, or a idle method.");

			if (IsRunning)
				throw new InvalidOperationException("The application is already running.");
						
			ApplicationIdleLoopMethod = loop;
			ApplicationContext = context;

			try
			{
				if (Initialize())
					return;

				IsRunning = true;
				Application.Run(context);
			}
			catch (Exception ex)
			{
				throw GorgonException.Catch(ex);
			}
			finally
			{
				CleanUp();
			}
		}

		/// <summary>
		/// Method to run a Gorgon application.
		/// </summary>
		/// <param name="context">Application context to use.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="context"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the application is already in a <see cref="P:GorgonLibrary.Gorgon.IsRunning">running state</see>.</exception>
		public static void Run(ApplicationContext context)
		{
			GorgonDebug.AssertNull<ApplicationContext>(context, "context");

			Run(context, ApplicationIdleLoopMethod);
		}

		/// <summary>
		/// Method to run a Gorgon application.
		/// </summary>
		/// <param name="mainForm">Form to use as the main form for the application.</param>
		/// <param name="loop">Idle loop method for the application.</param>
		/// <remarks>Passing NULL (Nothing in VB.Net) to both the <paramref name="mainForm"/> and <paramref name="loop"/> parameters will raise an exception.</remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when the mainForm and the loop parameters are NULL.
		/// <para>-or-</para>
		/// Thrown when the application is already in a <see cref="P:GorgonLibrary.Gorgon.IsRunning">running state</see>.
		/// </exception>
		public static void Run(Form mainForm, ApplicationLoopMethod loop)
		{
			if ((mainForm == null) && (loop == null))
				throw new InvalidOperationException("Cannot run an application without either a main form, or a idle method.");

			if (IsRunning)
				throw new InvalidOperationException("The application is already running.");

			ApplicationIdleLoopMethod = loop;
			_mainForm = mainForm;

			try
			{
				if (Initialize())
					return;

				IsRunning = true;
				Application.Run(ApplicationForm);
			}
			catch (Exception ex)
			{
				throw GorgonException.Catch(ex);
			}
			finally
			{
				CleanUp();
			}
		}

		/// <summary>
		/// Method to run a Gorgon application.
		/// </summary>
		/// <param name="mainForm">Form to use as the main form for the application.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="mainForm"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the application is already in a <see cref="P:GorgonLibrary.Gorgon.IsRunning">running state</see>.</exception>
		public static void Run(Form mainForm)
		{
			GorgonDebug.AssertNull<Form>(mainForm, "mainForm");

			Run(mainForm, ApplicationIdleLoopMethod);
		}

		/// <summary>
		/// Method to run a Gorgon application.
		/// </summary>
		/// <param name="loop">Idle loop method for the application.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="loop"/> parameter is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.InvalidOperationException">Thrown when the application is already in a <see cref="P:GorgonLibrary.Gorgon.IsRunning">running state</see>.</exception>
		public static void Run(ApplicationLoopMethod loop)
		{
			GorgonDebug.AssertNull<ApplicationLoopMethod>(loop, "loop");

			if (IsRunning)
				throw new InvalidOperationException("The application is already running.");

			ApplicationIdleLoopMethod = loop;

			try
			{
				if (Initialize())
					return;

				IsRunning = true;
				Application.Run();
			}
			catch (Exception ex)
			{
				throw GorgonException.Catch(ex);
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
			Log = new GorgonLogFile(Gorgon._logFile, "Tape_Worm");
			_timingData = new GorgonFrameRate();

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
			Log.Print("Initializing...", LoggingLevel.All);
			Log.Print("Architecture: {0}", LoggingLevel.Verbose, GorgonComputerInfo.PlatformArchitecture);
			Log.Print("Processor count: {0}", LoggingLevel.Verbose, GorgonComputerInfo.ProcessorCount);
			Log.Print("Installed Memory: {0}", LoggingLevel.Verbose, GorgonComputerInfo.TotalPhysicalRAM.FormatMemory());
			Log.Print("Available Memory: {0}", LoggingLevel.Verbose, GorgonComputerInfo.AvailablePhysicalRAM.FormatMemory());
			Log.Print("Operating System: {0} ({1})", LoggingLevel.Verbose, GorgonComputerInfo.OperatingSystemVersionText, GorgonComputerInfo.OperatingSystemArchitecture); 

			// Default to using 10 milliseconds of sleep time when the application is not focused.
			UnfocusedSleepTime = 10;
		}
		#endregion
	}
}
