// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: March 25, 2025 11:14:31 PM
//

using Gorgon.Diagnostics;
using Gorgon.Timing;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Gorgon.UI.WindowsForms;

/// <summary>
/// This functionality to allow Windows Forms applications to run their own custom loop during application idle time.
/// </summary>
public sealed class GorgonApplicationLoop
    : IDisposable
{
    // The idle callback method.
    private Func<bool>? _idle;
    // The idle callback task.
    private Func<CancellationToken, Task<bool>>? _idleTask;
    // The logging interface for debug messaging.
    private readonly IGorgonLog _log = GorgonLog.NullLog;
    // The synchronization object for multiple threads.
    private readonly object _syncLock = new();
    // Flag to indicate whether the object was disposed or not.
    private bool _disposed;
    // The cancel token source for cancelling our async loop. This is only ever triggered on cleanup (dispose or application shut down).
    private static CancellationTokenSource _cancellationTokenSource = new();
    // The instance.
    private static volatile GorgonApplicationLoop? _instance = null;
    // The lock used to create a single instance.
    private static readonly object _instanceLock = new();
    // Flags to indicate that the events are assigned.
    private static int _idleAssigned = 0;
    private static int _exitAssigned = 0;

    /// <summary>
    /// Property to return whether the application loop is running or not.
    /// </summary>
    public bool IsRunning => (_idle is not null) || (_idleTask is not null);

    /// <summary>
    /// Property to set or return the amount of time, in milliseconds, to put the loop to sleep when the application is not focused.
    /// </summary>
    /// <remarks>
    /// This property is only used when the loop is run with allow background set to <b>true</b> when calling <see cref="Run(Func{bool}, bool)"/> or <see cref="Run(Func{CancellationToken, Task{bool}}, bool)"/>.
    /// </remarks>
    /// <seealso cref="Run(Func{bool}, bool)"/>
    /// <seealso cref="Run(Func{CancellationToken, Task{bool}}, bool)"/>
    public int BackgroundSleepTime
    {
        get;
        set;
    } = 10;

    /// <summary>
    /// Property to return whether to allow execution of the idle callback when the application is not in focus.
    /// </summary>
    /// <remarks>
    /// This property is set when the <c>allowBackground</c> property of the <see cref="Run(Func{bool}, bool)"/> or the <see cref="Run(Func{CancellationToken, Task{bool}}, bool)"/> methods is set.
    /// </remarks>
    /// <seealso cref="Run(Func{bool}, bool)"/>
    /// <seealso cref="Run(Func{CancellationToken, Task{bool}}, bool)"/>
    public bool AllowBackgroundExecution
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to dispose of managed/unmanaged objects and perform clean up.
    /// </summary>
    /// <param name="disposing"><b>true</b> to clean up unmanaged and managed objects, <b>false</b> to clean up unmanaged objects only.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _instance = null;

        if (disposing)
        {
            Stop();
        }
        else
        {
            // If we only get the finalizer for this object, at least unlink us from the application.
            DisableExit();
            DisableIdle();

            _cancellationTokenSource.Cancel();
        }

        _cancellationTokenSource.Dispose();

        _disposed = true;
    }

    /// <summary>
    /// Function to enable idle event tracking.
    /// </summary>
    private void EnableIdle()
    {
        if (Interlocked.Exchange(ref _idleAssigned, 1) == 1)
        {
            return;
        }

        Application.Idle += Application_Idle;
    }

    /// <summary>
    /// Function to disable idle event tracking.
    /// </summary>
    private void DisableIdle()
    {
        if (Interlocked.Exchange(ref _idleAssigned, 0) == 0)
        {
            return;
        }

        Application.Idle -= Application_Idle;
    }

    /// <summary>
    /// Function to enable exit event tracking.
    /// </summary>
    private void EnableExit()
    {
        if (Interlocked.Exchange(ref _exitAssigned, 1) == 1)
        {
            return;
        }

        Application.ApplicationExit += Application_ApplicationExit;
    }

    /// <summary>
    /// Function to disable exit event tracking.
    /// </summary>
    private void DisableExit()
    {
        if (Interlocked.Exchange(ref _exitAssigned, 0) == 0)
        {
            return;
        }

        Application.ApplicationExit -= Application_ApplicationExit;
    }

    /// <summary>
    /// Function to determine if this process is the active application.
    /// </summary>
    /// <returns><b>true</b> if the process is active, or <b>false</b> if not.</returns>
    private static bool IsForeGroundProcess()
    {
        HWND hwnd = PInvoke.GetForegroundWindow();

        if (hwnd == HWND.Null)
        {
            return false;
        }

        unsafe
        {
            uint processID = 0;
            uint threadID = PInvoke.GetWindowThreadProcessId(hwnd, &processID);

            if ((processID == 0) || (threadID == 0))
            {
                return false;
            }

            return processID == Environment.ProcessId;
        }
    }

    /// <summary>
    /// Function called when the application is shutting down.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void Application_ApplicationExit(object? sender, EventArgs e) => Stop();

    /// <summary>
    /// Function to handle processing of an async idle method.
    /// </summary>
    /// <param name="idle">The idle function to execute.</param>
    /// <returns><b>true</b> if the loop should continue, <b>false</b> if not.</returns>
    private async Task<bool> HandleIdleAsync(Func<CancellationToken, Task<bool>> idle)
    {
        // Turn off idle event processing until we are finished.
        DisableIdle();

        bool idleResult;

        try
        {
            idleResult = await idle(_cancellationTokenSource.Token);

            if (_cancellationTokenSource.IsCancellationRequested)
            {
                // We've cleaned up if we've cancelled, so we don't need to do anything.
                return false;
            }

            EnableIdle();
        }
        catch (OperationCanceledException)
        {
            // We've cleaned up if we've cancelled, so we don't need to do anything.
            return false;
        }

        return idleResult;
    }

    /// <summary>
    /// Function called while the application is in an idle state.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private async void Application_Idle(object? sender, EventArgs e)
    {
        Func<bool>? idle = _idle;
        Func<CancellationToken, Task<bool>>? idleTask = _idleTask;

        if ((idle is null) && (idleTask is null))
        {
            return;
        }

        // This should never really happen, but paranoia rules the day.
        if ((idle is not null) && (idleTask is not null))
        {
            _idle = null;
            _idleTask = null;
            _log.PrintError("Both idle type callbacks are active!! This should not happen.", LoggingLevel.Simple);
            return;
        }

        bool allowExecution = AllowBackgroundExecution || IsForeGroundProcess();

        if (!GorgonTiming.TimingStarted)
        {
            GorgonTiming.StartTiming(new GorgonTimer());
        }

        // If there are no messages to process, we exit and let Windows Forms handle any incoming messages. Otherwise, we can run our application loop.
        while ((allowExecution) && (!PInvoke.PeekMessage(out MSG msg, HWND.Null, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_NOREMOVE)))
        {
            GorgonTiming.Update();

            bool idleResult = false;

            if (idle is not null)
            {
                idleResult = idle();
            }
            else if (idleTask is not null)
            {
                idleResult = await HandleIdleAsync(idleTask);
            }

            // If the idle callback returns false, then stop running.
            if (!idleResult)
            {
                Stop();
                return;
            }

            bool isForeground = IsForeGroundProcess();
            allowExecution = AllowBackgroundExecution || isForeground;

            // If we're in the foreground, or allow background execution or don't have a delay time, then 
            // continue execution as normal. Otherwise, give up CPU time to other processes.
            if ((isForeground) || (!allowExecution) || (BackgroundSleepTime <= 0))
            {
                continue;
            }

            // Stop reentrant behaviour while we wait.
            DisableIdle();

            try
            {
                await Task.Delay(BackgroundSleepTime, _cancellationTokenSource.Token);

                if ((_disposed) || (_cancellationTokenSource.IsCancellationRequested))
                {
                    return;
                }

                EnableIdle();
            }
            catch (OperationCanceledException)
            {
                // If the cancel is triggered, we've cleaned up, just leave.
                return;
            }
        }
    }

    /// <summary>
    /// Function to create an instance of an application loop.
    /// </summary>
    /// <param name="log">[Optional] The log used for debug messaging.</param>    
    /// <returns>A new instance of the <see cref="GorgonApplicationLoop"/> object, or if an instance already exists, the existing instance.</returns>
    public static GorgonApplicationLoop Create(IGorgonLog? log = null)
    {
        lock (_instanceLock)
        {
            _instance ??= new GorgonApplicationLoop(log ?? GorgonLog.NullLog);
            return _instance;
        }
    }

    /// <summary>
    /// Function to stop the loop from executing.
    /// </summary>
    public void Stop()
    {
        lock (_syncLock)
        {
            if ((!IsRunning) || (_cancellationTokenSource.IsCancellationRequested) || (_disposed))
            {
                return;
            }

            _log.Print("Stopping current application loop.", LoggingLevel.Simple);

            DisableExit();
            DisableIdle();

            _cancellationTokenSource.Cancel();

            GorgonTiming.Reset();
            _idleTask = null;
            _idle = null;
        }
    }

    /// <summary>
    /// Function called to start running the idle loop.
    /// </summary>
    /// <param name="idleProcess">The function to call during idle time.</param>
    /// <param name="allowBackground">[Optional] <b>true</b> to allow the loop to keep running while the application is not in focus, <b>false</b> to pause it and wait.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the object was previously disposed.</exception>
    public void Run(Func<bool> idleProcess, bool allowBackground = false)
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, typeof(GorgonApplicationLoop));

            if (IsRunning)
            {
                Stop();
            }

            _log.Print("Beginining application loop.", LoggingLevel.Simple);

            _cancellationTokenSource = new CancellationTokenSource();

            _idleTask = null;
            _idle = idleProcess;
            AllowBackgroundExecution = allowBackground;

            EnableIdle();
            EnableExit();
        }
    }

    /// <summary>
    /// Function called to start running an asynchronous idle loop.
    /// </summary>
    /// <param name="idleProcess">The function to call during idle time.</param>
    /// <param name="allowBackground">[Optional] <b>true</b> to allow the loop to keep running while the application is not in focus, <b>false</b> to pause it and wait.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the object was previously disposed.</exception>
    public void Run(Func<CancellationToken, Task<bool>> idleProcess, bool allowBackground = false)
    {
        lock (_syncLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, typeof(GorgonApplicationLoop));

            if (IsRunning)
            {
                Stop();
            }

            _log.Print("Beginining application loop.", LoggingLevel.Simple);

            _cancellationTokenSource = new CancellationTokenSource();

            _idle = null;
            _idleTask = idleProcess;
            AllowBackgroundExecution = allowBackground;

            EnableIdle();
            EnableExit();
        }
    }

    /// <summary>
    /// Function to dispose of managed/unmanaged objects and perform clean up.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer for the application loop.
    /// </summary>
    ~GorgonApplicationLoop() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonApplicationLoop"/> class.
    /// </summary>
    /// <param name="log">The log used for debug messaging.</param>
    private GorgonApplicationLoop(IGorgonLog log) => _log = log ?? GorgonLog.NullLog;
}

