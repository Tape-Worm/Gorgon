
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Wednesday, November 02, 2011 10:11:10 AM
// 

using System.Globalization;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO;
using Gorgon.Native;
using Gorgon.Timing;
using Gorgon.Windows.Properties;

namespace Gorgon.UI;

/// <summary>
/// An application class for windows <see cref="Form"/> applications
/// </summary> 
/// <remarks>
/// <para>
/// This class is meant as a replacement for the standard windows forms <see cref="Application"/> class. It expands the functionality of the application class by exposing useful functionality for 
/// working with the main application <see cref="Form"/>
/// </para>
/// <para>
/// One of the key components to this class is the introduction of a proper Idle loop. This allows an application to perform operations while the windows message pump is in an idle state. This is useful 
/// for things like games, or applications that require constant interaction with other systems. To set an idle loop you may pass a method to execute in one of the <see cref="Run(Form, Func{bool})"/> 
/// method overloads. Or, if you choose, you may assign an idle method to execute at any point in the application life cycle by assigning a method to the <see cref="IdleMethod"/>
/// </para> 
/// <para>
/// Like the <see cref="Application"/> class, this class also provides the ability to pass a windows <see cref="Form"/>, or <see cref="ApplicationContext"/> to one of the <see cref="Run(Form, Func{bool})"/> 
/// overloads. 
/// </para>
/// <para>
/// <note type="tip">
/// <para>
/// When passing a form to the <see cref="Run(Form,Func{bool})"/> method, that form automatically becomes the main application window. The application will exit when this form is closed. 
/// The main form of an application may be retrieved from the <see cref="MainForm"/> property. 
/// </para>
/// <para>
/// If this is not suitable, one of the other <see cref="Run(Func{bool})"/> overloads will allow you to finely control the life cycle of your application
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
///			// You should always catch any unhandled exceptions and either log them, or display them, or something
///			// It can get real ugly for your users otherwise
///			ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), GorgonApplication.Log);
///		}
///		finally
///		{
///			// Do your clean up here
///		}
///	}
/// ]]>
/// </code>
/// </example>
public static class GorgonApplication
{

    // Log file application name.
    private const string LogFile = "GorgonLibrary";
    // Peek message NoRemove flag.
    private const uint PeekMessageNoRemove = 0;

    // Event fired when the application is about to exit.
    private static event EventHandler ExitEvent;

    // Event fired when a message pump thread is about to exit.
    private static event EventHandler ThreadExitEvent;

    /// <summary>
    /// Event fired when the application is about to exit.
    /// </summary>
    public static event EventHandler Exit
    {
        add
        {
            if (value is null)
            {
                ExitEvent = null;
                return;
            }

            ExitEvent += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }

            ExitEvent -= value;
        }
    }

    /// <summary>
    /// Event fired when a message pump thread is about to exit.
    /// </summary>
    public static event EventHandler ThreadExit
    {
        add
        {
            if (value is null)
            {
                ThreadExitEvent = null;
                return;
            }

            ThreadExitEvent += value;
        }
        remove
        {
            if (value is null)
            {
                return;
            }

            ThreadExitEvent -= value;
        }
    }

    // Main application form.
    private static Form _mainForm;
    // Flag to indicate that the application needs to close.
    private static bool _quitSignalled;
    // Application loop method.														
    private static Func<bool> _loop;
    // The log interface to use.
    private static IGorgonLog _log;
    // The dummy log interface.
    private static readonly IGorgonLog _dummyLog = GorgonLog.NullLog;
    // A synchronization object for threads.
    private static readonly object _syncLock = new();
    // The number of milliseconds to sleep while the application is unfocused but running in the background.
    private static int _unfocusedSleepTime = 16;
    // An atomic to ensure that run is only called by 1 thread at a time.
    private static int _runAtomic;
    // Event used to put the application to sleep.
    private static readonly ManualResetEventSlim _unfocusedTimeout = new(false, 20);

    /// <summary>
    /// Property to return information about the computer.
    /// </summary>
    public static IGorgonComputerInfo ComputerInfo
    {
        get;
    }

    /// <summary>
    /// Property to set or return the current <see cref="CultureInfo"/> for the application.
    /// </summary>
    /// <remarks>
    /// This is a pass through for the <see cref="Application.CurrentCulture"/> property.
    /// </remarks>
    /// <seealso cref="Application.CurrentCulture"/>
    public static CultureInfo CurrentCulture
    {
        get => Application.CurrentCulture;
        set => Application.CurrentCulture = value;
    }

    /// <summary>
    /// Property to return the current <see cref="InputLanguage"/> for the application.
    /// </summary>
    /// <remarks>
    /// This is a pass through for the <see cref="Application.CurrentInputLanguage"/> property.
    /// </remarks>
    /// <seealso cref="Application.CurrentInputLanguage"/>
    public static InputLanguage CurrentInputLanguage => Application.CurrentInputLanguage;

    /// <summary>
    /// Property to return a read-only list of all the forms that are open in the application.
    /// </summary>
    /// <remarks>
    /// This is a pass through for the <see cref="Application.OpenForms"/> property.
    /// </remarks>
    /// <seealso cref="Application.OpenForms"/>
    public static FormCollection OpenForms => Application.OpenForms;

    /// <summary>
    /// Property to return if the application is running in the foreground or background.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This determines whether the <see cref="MainForm"/> is focused (i.e. a top level, active window). It does this by ensuring the <see cref="Form.WindowState"/> is not set to 
    /// <see cref="FormWindowState.Minimized"/>, and the window, or one of its child controls, currently has input focus.
    /// </para>
    /// <para>
    /// This only applies to the <see cref="MainForm"/>. If Gorgon is required to keep processing unfettered, then set the <see cref="AllowBackground"/> property to <b>true</b>. 
    /// </para>
    /// <para>
    /// If <see cref="AllowBackground"/> is <b>true</b>, and the main form is not active, but another form belonging to the executing application is active, then processing will continue at full speed. 
    /// But, if the entire application does not have focus, then Gorgon will limit the processing time to cycle every 16 milliseconds so as to not hog all the CPU.
    /// </para>
    /// <para>
    /// If no <see cref="MainForm"/> is assigned, then this property will always return <b>false</b>. 
    /// </para>
    /// </remarks>
    public static bool IsForeground => ((MainForm is not null) && (MainForm.WindowState != FormWindowState.Minimized) && (MainForm.ContainsFocus));

    /// <summary>
    /// Property to return the ID of the application UI thread.
    /// </summary>
    public static int ThreadID
    {
        get;
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
        get => _unfocusedSleepTime;
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
    /// When set to <b>true</b>, this will allow an application to process messages and execute the <see cref="IdleMethod"/> while the application is unfocused or minimized. If it 
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
    /// Setting this value to <b>null</b> when no <see cref="MainForm"/> or <see cref="ApplicationContext"/> is assigned will lead to an exception being thrown. This is 
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
    /// Thrown when an attempt to set this property to <b>null</b> is made and there is no <see cref="MainForm"/> or <see cref="ApplicationContext"/> 
    /// attached to the application.
    /// </exception>
    public static Func<bool> IdleMethod
    {
        get => _loop;
        set
        {
            // If we have no application form, or context and we try disable the loop, leave. 
            // We do this because if there's no context or form, then there's nothing to run and that's an invalid state.
            if ((value is null)
                && (MainForm is null)
                && (ApplicationContext is null))
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

            if ((value is null) || (!IsRunning))
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
    public static DirectoryInfo StartupPath
    {
        get;
    }

    /// <summary>
    /// Property to return the path for the currently running application.
    /// </summary>
    /// <remarks>
    /// This includes the file name of the assembly that is executing as well as the directory that the application was executed from.
    /// </remarks>
    public static string ExecutablePath => Application.ExecutablePath;

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
    /// <para>
    /// This property can be used to retrieve the <see cref="Form"/> that is the primary form for the application. When this form closes, the application will shut down.
    /// </para>
    /// <para>
    /// This value is set when the <see cref="Run(Form,Func{bool})"/> is used to run the application, or a form has been assigned to the <see cref="ApplicationContext.MainForm"/> 
    /// property.
    /// </para>
    /// </remarks>
    public static Form MainForm => ApplicationContext is not null ? ApplicationContext.MainForm : _mainForm;

    /// <summary>
    /// Property to set or return the current application context.
    /// </summary>
    /// <remarks>
    /// This value will return <b>null</b> if this application wasn't started with the <see cref="Run(System.Windows.Forms.ApplicationContext,Func{bool})"/> method.
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
        get => _log;
        set
        {
            lock (_syncLock)
            {
                try
                {
                    _log?.LogEnd();

                    if (value is null)
                    {
                        _log = _dummyLog;
                        return;
                    }

                    _log = value;

                    InitializeLogger();
                }
                catch
                {
                    // We couldn't set up the logging, so fall back to the null log.
                    _log = GorgonLog.NullLog;
                }
            }
        }
    }

    /// <summary>
    /// Handles the Idle event of the Application control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private static void Application_Idle(object sender, EventArgs e)
    {
        // Windows message to retrieve.

        // We have nothing to execute, so just leave.
        if ((IdleMethod is null) || (!IsRunning))
        {
            return;
        }

        // Check for application focus. If we didn't assign a main form, or one was not assigned to the application context, then 
        // run regardless since we have an idle method to execute.
        bool appShouldProcess = MainForm is null || AllowBackground || IsForeground;

        if (!GorgonTiming.TimingStarted)
        {
            if (GorgonTimerQpc.SupportsQpc())
            {
                GorgonTiming.StartTiming<GorgonTimerQpc>();
            }
            else
            {
                GorgonTimerMultimedia.BeginTiming();
                GorgonTiming.StartTiming<GorgonTimerMultimedia>();
            }
        }

        while ((appShouldProcess) && (!UserApi.PeekMessage(out MSG _, IntPtr.Zero, 0, 0, PeekMessageNoRemove)))
        {
            // Only update the time when we have focus, otherwise reset the time so we don't get super weird values
            // spiking in the background.
            if ((IsForeground) || (AllowBackground))
            {
                GorgonTiming.Update();
            }
            else
            {
                GorgonTiming.Reset();
                continue;
            }

            if (!IdleMethod())
            {
                // Force an exit from the thread.
                Quit();
                return;
            }

            // Give up CPU time if we're not focused.
            if ((MainForm is null) || (MainForm.ContainsFocus) || (_unfocusedSleepTime <= 0) || (Form.ActiveForm is not null))
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
        if (Log is null)
        {
            return;
        }

        Log.LogStart();

        // Display information
        Log.Print("Logging interface assigned. Initializing...", LoggingLevel.All);
        Log.Print($"Architecture: {ComputerInfo.PlatformArchitecture}", LoggingLevel.Verbose);
        Log.Print($"Processor count: {ComputerInfo.ProcessorCount}", LoggingLevel.Verbose);
        Log.Print($"Installed Memory: {ComputerInfo.TotalPhysicalRAM.FormatMemory()}", LoggingLevel.Verbose);
        Log.Print($"Available Memory: {ComputerInfo.AvailablePhysicalRAM.FormatMemory()}", LoggingLevel.Verbose);
        Log.Print($"Operating System: {ComputerInfo.OperatingSystemVersionText} ({ComputerInfo.OperatingSystemArchitecture})", LoggingLevel.Verbose);
        Log.Print(string.Empty, LoggingLevel.Verbose);
    }

    /// <summary>
    /// Function to initialize the main form and idle loop..
    /// </summary>
    private static void Initialize()
    {
        // Initialize the timing data.
        // This will start the application uptime counter.  But since there's no actual timer set on this yet,
        // no statistical timing will be collected until the application begins its run in Application_Idle.
        GorgonTiming.Reset();

        // Notify that we're in a running state.
        IsRunning = true;

        // Display the form.
        if ((MainForm is not null) && (!MainForm.IsDisposed))
        {
            MainForm.Show();
        }

        if ((IdleMethod is not null) && (!_quitSignalled))
        {
            Log.Print("Application loop starting...", LoggingLevel.Simple);
            Application.Idle += Application_Idle;
        }

        // Register quit handlers.
        Application.ApplicationExit += Application_ApplicationExit;
        Application.ThreadExit += Application_ThreadExit;

        // If anything kills the application early, then capture the quit state.
        IsRunning = !_quitSignalled;
    }

    /// <summary>
    /// Handles the ThreadExit event of the Application control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private static void Application_ThreadExit(object sender, EventArgs e)
    {
        Application.ThreadExit -= Application_ThreadExit;

        // This will only allow one thread to actually call this.
        // It also has the added benefit of unassigning the exit handler when the application shuts down.
        EventHandler exitHandler = Interlocked.Exchange(ref ThreadExitEvent, null);
        exitHandler?.Invoke(sender, e);
    }

    /// <summary>
    /// Handles the ApplicationExit event of the Application control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private static void Application_ApplicationExit(object sender, EventArgs e)
    {
        Application.ApplicationExit -= Application_ApplicationExit;

        // This will only allow one thread to actually call this.
        // It also has the added benefit of unassigning the exit handler when the application shuts down.
        EventHandler exitHandler = Interlocked.Exchange(ref ExitEvent, null);
        exitHandler?.Invoke(sender, e);
    }

    /// <summary>
    /// Function to clean up events and other objects after an application exits.
    /// </summary>
    private static void CleanUp()
    {
        IsRunning = false;

        _unfocusedTimeout.Dispose();

        // Remove quit handlers.
        Application.ApplicationExit -= Application_ApplicationExit;
        Application.ThreadExit -= Application_ThreadExit;
        ExitEvent = null;
        ThreadExitEvent = null;

        if (IdleMethod is not null)
        {
            Application.Idle -= Application_Idle;
            _loop = null;
            Log.Print("Application loop stopped.", LoggingLevel.Simple);
        }

        // Reset the low resolution timer period on application end.
        if (!GorgonTimerQpc.SupportsQpc())
        {
            GorgonTimerMultimedia.EndTiming();
        }

        Log.Print("Shutting down.", LoggingLevel.All);

        // Destroy log.
        _log?.LogEnd();
    }

    /// <summary>
    /// Function to add a message filter to intercept application messages.
    /// </summary>
    /// <param name="filter">A <see cref="IMessageFilter"/> used to intercept an application message.</param>
    /// <remarks>
    /// <para>
    /// This is a pass through for the <see cref="Application.AddMessageFilter"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="Application.AddMessageFilter"/>
    public static void AddMessageFilter(IMessageFilter filter) => Application.AddMessageFilter(filter);

    /// <summary>
    /// Function to remove a message filter added with the <see cref="AddMessageFilter"/> method.
    /// </summary>
    /// <param name="filter">A <see cref="IMessageFilter"/> used to intercept an application message.</param>
    /// <remarks>
    /// <para>
    /// This is a pass through for the <see cref="Application.RemoveMessageFilter"/> method.
    /// </para>
    /// </remarks>
    /// <seealso cref="Application.RemoveMessageFilter"/>
    public static void RemoveMessageFilter(IMessageFilter filter) => Application.RemoveMessageFilter(filter);

    /// <summary>
    /// Function that will signal the application to begin shutdown.
    /// </summary>
    /// <remarks>
    /// This will signal the application to tell it that it is time to shut down. This shut down will not happen immediately as the application needs to finish up whatever processing it is doing 
    /// before exiting.
    /// </remarks>
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
    /// Function to begin execution of a <see cref="GorgonApplication"/>.
    /// </summary>
    /// <param name="context">The <see cref="System.Windows.Forms.ApplicationContext"/> to use for this application.</param>
    /// <param name="idleMethod">[Optional] A method to execute while the application is idle.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="context"/> parameter is <b>null</b>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the application is already executing. Check the <see cref="IsRunning"/> property before calling this method.</exception>
    /// <remarks>
    /// <para>
    /// This method begins execution of the application by starting its messaging pump and processing application messages. 
    /// </para>
    /// <para>
    /// This particular overload takes a <see cref="System.Windows.Forms.ApplicationContext"/> which allows for more fine grained control over the lifetime 
    /// of the application.
    /// </para>
    /// <para>
    /// The <paramref name="idleMethod"/> is called while the application is idle (i.e. not processing messages). User code may execute within this method 
    /// to allow for processing while the application is not doing anything.
    /// </para>
    /// <para>
    /// <note type="tip">
    /// <para>
    /// The <paramref name="idleMethod"/> takes a <see cref="Func{TResult}"/> that returns a <see cref="bool"/> value. When this value is <b>true</b>, the application will continue processing as normal, 
    /// but when it returns <b>false</b>, the application will exit. 
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Only one thread may call this method at any given time. If this method is executed by a thread, and another attempts to execute it, an exception will be thrown.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="System.Windows.Forms.ApplicationContext"/>
    /// <seealso cref="IsRunning"/>
    /// <seealso cref="IdleMethod"/>
    public static void Run(ApplicationContext context, Func<bool> idleMethod = null)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_ALREADY_RUNNING);
        }

        if (Interlocked.Increment(ref _runAtomic) > 1)
        {
            throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_RUN_ONLY_FROM_ONE_THREAD);
        }

        if (idleMethod is not null)
        {
            IdleMethod = idleMethod;
        }

        ApplicationContext = context ?? throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_NOCONTEXT);

        try
        {
            Initialize();

            if (!IsRunning)
            {
                return;
            }

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
    /// Function to begin execution of a <see cref="GorgonApplication"/>.
    /// </summary>
    /// <param name="mainForm">The main application <see cref="Form"/> to use for this application.</param>
    /// <param name="idleMethod">[Optional] A method to execute while the application is idle.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="mainForm"/> parameter is <b>null</b>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the application is already executing. Check the <see cref="IsRunning"/> property before calling this method.</exception>
    /// <remarks>
    /// <para>
    /// This method begins execution of the application by starting its messaging pump and processing application messages. 
    /// </para>
    /// <para>
    /// This overload uses the <paramref name="mainForm"/> passed to it as the main application form. When this window closes, the application will shut down. 
    /// </para>
    /// <para>
    /// The <paramref name="idleMethod"/> is called while the application is idle (i.e. not processing messages). User code may execute within this method to allow for processing while the application 
    /// is not doing anything.
    /// </para>
    /// <para>
    /// <note type="tip">
    /// <para>
    /// The <paramref name="idleMethod"/> takes a <see cref="Func{TResult}"/> that returns a <see cref="bool"/> value. When this value is <b>true</b>, the application will continue processing as normal, 
    /// but when it returns <b>false</b>, the application will exit. 
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Only one thread may call this method at any given time. If this method is executed by a thread, and another attempts to execute it, an exception will be thrown.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="System.Windows.Forms.ApplicationContext"/>
    /// <seealso cref="IsRunning"/>
    /// <seealso cref="IdleMethod"/>
    /// <seealso cref="MainForm"/>
    public static void Run(Form mainForm, Func<bool> idleMethod = null)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_ALREADY_RUNNING);
        }

        if (Interlocked.Increment(ref _runAtomic) > 1)
        {
            throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_RUN_ONLY_FROM_ONE_THREAD);
        }

        if (idleMethod is not null)
        {
            IdleMethod = idleMethod;
        }

        _mainForm = mainForm ?? throw new ArgumentNullException(nameof(mainForm));

        try
        {
            Initialize();

            if (!IsRunning)
            {
                return;
            }

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
    /// Function to begin execution of a <see cref="GorgonApplication"/>.
    /// </summary>
    /// <param name="idleMethod">A method to execute while the application is idle.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="idleMethod"/> parameter is <b>null</b>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the application is already executing. Check the <see cref="IsRunning"/> property before calling this method.</exception>
    /// <remarks>
    /// <para>
    /// This method begins execution of the application by starting its messaging pump and processing application messages. 
    /// </para>
    /// <para>
    /// This overload only uses the <see cref="IdleMethod"/> for its execution. This is called when the application has no messages to process in its message pump. Because of this, there is no indication 
    /// of whether the application is in the foreground or background as it is really always running in the background. When this method is chosen, the <see cref="IsForeground"/> property will always return 
    /// <b>false</b>. Applications that use this should take care to balance the CPU usage of the <paramref name="idleMethod"/>. 
    /// </para>
    /// <para>
    /// The <paramref name="idleMethod"/> takes a <see cref="Func{TResult}"/> that returns a <see cref="bool"/> value. When this value is <b>true</b>, the application will continue processing as normal, 
    /// but when it returns <b>false</b>, the application will exit. 
    /// </para>
    /// <para>
    /// <note type="important">
    /// <para>
    /// Only one thread may call this method at any given time. If this method is executed by a thread, and another attempts to execute it, an exception will be thrown.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="IsForeground"/>
    /// <seealso cref="System.Windows.Forms.ApplicationContext"/>
    /// <seealso cref="IsRunning"/>
    /// <seealso cref="IdleMethod"/>
    public static void Run(Func<bool> idleMethod)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_ALREADY_RUNNING);
        }

        if (Interlocked.Increment(ref _runAtomic) > 1)
        {
            throw new InvalidOperationException(Resources.GOR_ERR_APPLICATION_RUN_ONLY_FROM_ONE_THREAD);
        }

        IdleMethod = idleMethod ?? throw new ArgumentNullException(nameof(idleMethod));

        try
        {
            Initialize();

            if (!IsRunning)
            {
                return;
            }

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
    /// Initializes the <see cref="GorgonApplication"/> class.
    /// </summary>
    static GorgonApplication()
    {
        ComputerInfo = new GorgonComputerInfo();
        ThreadID = Thread.CurrentThread.ManagedThreadId;

        try
        {
            Log = new GorgonTextFileLog(LogFile, "Tape_Worm", typeof(GorgonApplication).Assembly.GetName().Version);
        }
        catch
        {
            Log = GorgonLog.NullLog;
        }

        StartupPath = new DirectoryInfo(Application.StartupPath.FormatDirectory(Path.DirectorySeparatorChar));
    }
}
