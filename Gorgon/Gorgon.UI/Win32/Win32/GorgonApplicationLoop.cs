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

namespace Gorgon.UI.Win32;

/// <summary>
/// This functionality to allow Windows to run their own custom loop during application idle time.
/// </summary>
public sealed class GorgonApplicationLoop
    : IDisposable
{
    // The idle callback method.
    private Func<bool>? _idle;
    // The logging interface for debug messaging.
    private readonly IGorgonLog _log = GorgonLog.NullLog;
    // Flag to indicate whether the object was disposed or not.
    private bool _disposed;
    // The cancel token source for cancelling our async loop. This is only ever triggered on cleanup (dispose or application shut down).
    private static CancellationTokenSource _cancellationTokenSource = new();
    // The instance.
    private static volatile GorgonApplicationLoop? _instance = null;
    // The lock used to create a single instance.
    private static readonly object _instanceLock = new();

    /// <summary>
    /// Property to return whether the application loop is running or not.
    /// </summary>
    public bool IsRunning => (_idle is not null);

    /// <summary>
    /// Property to set or return the amount of time, in milliseconds, to put the loop to sleep when the application is not focused.
    /// </summary>
    /// <remarks>
    /// This property is only used when the loop is run with allow background set to <b>true</b> when calling <see cref="Run(Func{bool}, bool)"/>.
    /// </remarks>
    /// <seealso cref="Run(Func{bool}, bool)"/>
    public int BackgroundSleepTime
    {
        get;
        set;
    } = 10;

    /// <summary>
    /// Property to return whether to allow execution of the idle callback when the application is not in focus.
    /// </summary>
    /// <remarks>
    /// This property is set when the <c>allowBackground</c> property of the <see cref="Run(Func{bool}, bool)"/> method is set.
    /// </remarks>
    /// <seealso cref="Run(Func{bool}, bool)"/>
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
            _cancellationTokenSource.Cancel();
        }

        _cancellationTokenSource.Dispose();

        _disposed = true;
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
    /// Function to handle processing of a synchronous idle method.
    /// </summary>
    /// <param name="idle">The idle function to execute.</param>
    private void HandleIdle(Func<bool> idle)
    {
        // Turn off idle event processing until we are finished.
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            if (!ProcessMessages())
            {
                Stop();
                return;
            }

            try
            {
                GorgonTiming.Update();

                bool idleResult = idle();

                if (!idleResult)
                {
                    Stop();
                    continue;
                }

                bool isForeground = IsForeGroundProcess();
                bool allowExecution = AllowBackgroundExecution || isForeground;

                // If we're in the foreground, or allow background execution or don't have a delay time, then 
                // continue execution as normal. Otherwise, give up CPU time to other processes.
                if ((isForeground) || (!allowExecution) || (BackgroundSleepTime <= 0))
                {
                    continue;
                }

                try
                {
                    Thread.Sleep(BackgroundSleepTime);

                    if ((_disposed) || (_cancellationTokenSource.IsCancellationRequested))
                    {
                        Stop();
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    // If the cancel is triggered, we've cleaned up, just leave.
                    Stop();
                    return;
                }
            }
            catch (OperationCanceledException)
            {
                Stop();
                return;
            }
        }
    }

    /// <summary>
    /// Function to process all window messages.
    /// </summary>
    /// <returns><b>true</b> if all messages have been processed, <b>false</b> when the application is signalled to quit.</returns>
    private bool ProcessMessages()
    {
        while (PInvoke.PeekMessage(out MSG msg, HWND.Null, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE))
        {
            PInvoke.TranslateMessage(in msg);
            PInvoke.DispatchMessage(in msg);

            if (msg.message == PInvoke.WM_QUIT)
            {
                Stop();
                return false;
            }
        }

        return true;
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
        if ((!IsRunning) || (_cancellationTokenSource.IsCancellationRequested) || (_disposed))
        {
            return;
        }

        _log.Print("Stopping current application loop.", LoggingLevel.Simple);
        _cancellationTokenSource.Cancel();

        GorgonTiming.Reset();
        _idle = null;
    }

    /// <summary>
    /// Function called to start running the idle loop.
    /// </summary>
    /// <param name="idleProcess">The function to call during idle time.</param>
    /// <param name="allowBackground">[Optional] <b>true</b> to allow the loop to keep running while the application is not in focus, <b>false</b> to pause it and wait.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the object was previously disposed.</exception>
    public void Run(Func<bool> idleProcess, bool allowBackground = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, typeof(GorgonApplicationLoop));

        if (IsRunning)
        {
            Stop();
        }

        _log.Print("Beginining application loop.", LoggingLevel.Simple);

        _cancellationTokenSource = new CancellationTokenSource();

        _idle = idleProcess;
        AllowBackgroundExecution = allowBackground;
        HandleIdle(_idle);
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