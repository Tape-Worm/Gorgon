﻿
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
// Created: Saturday, June 18, 2011 10:29:46 AM
// 

using Gorgon.Math;

namespace Gorgon.Timing;

/// <summary>
/// Timing data for code within a Gorgon Idle loop
/// </summary>
/// <remarks>
/// <para>
/// This class is used to calculate the time it takes for a single iteration an idle loop to execute. It will gather statistics such as the frames per second, the time elapsed since the application started 
/// and peaks, lows and averages for those values
/// </para>
/// <para>
/// To use this in a custom idle processing loop the user should initialize using the <see cref="StartTiming"/> method, and then, in the loop, call the <see cref="Update"/> method to populate the data 
/// with the most recent timings
/// </para>
/// </remarks>
/// <example>
/// When using the <c>Gorgon.UI.GorgonApplication</c> class, the timing code is automatically updated by its own idle loop:
/// <code lang="csharp">
/// <![CDATA[
///	public static bool MyLoop()
/// {
///     Console.CursorLeft = 0;
///     Console.CursorTop = 0;
///		Console.WriteLine($"FPS: {GorgonTiming.FPS}");
/// 
///		return true;
/// }
/// 
/// public static Main()
/// {
///		GorgonApplication.Run(MyLoop);
/// }
/// ]]>
/// </code> 
/// And here is a a custom application loop using the <see cref="GorgonTiming"/> class:
/// <code lang="csharp">
/// <![CDATA[
/// // This assumes the Win32 API call to PeekMessage is imported
/// public void DoLoop()
/// {
///		MSG message;  // Win32 Message structure
/// 
///		// Before loop execution
///     GorgonTiming.StartTiming(new GorgonTimerQpc());
/// 
///		while (!API.PeekMessage(out message, IntPtr.Zero, 0, 0, PeekMessage.NoRemove))
///     {
///			GorgonTiming.Update();
/// 
///			// Do your processing
///		}
/// }
/// ]]>
/// </code>
/// </example>
public static class GorgonTiming
{
    // Timer used in calculations.
    private static IGorgonTimer? _timer;
    // The value to indicate how long the application has been running.
    private static decimal _appTimer;
    // Frame counter.
    private static long _frameCounter;
    // Counter for averages.
    private static long _averageCounter;
    // Last recorded time since update.
    private static double _lastTime;
    // Last timer value.
    private static double _lastTimerValue;
    // Average FPS total.
    private static float _averageFPSTotal;
    // Average scaled delta draw time total.
    private static float _averageUnscaledDeltaTotal;
    // Average delta draw time total.
    private static float _averageDeltaTotal;
    // Maximum number of iterations for average reset.
    private static long _maxAverageCount = 500;
    // Flag to indicate that timing has started.
    private static int _timingStarted;
    // The lowest frames per second.
    private static float? _lowestFps;
    // The lowest frame delta.
    private static float? _lowestDelta;

    /// <summary>
    /// Property to set or return the maximum frame delta, in seconds.
    /// </summary>
    /// <remarks>
    /// This value is used to cap the <see cref="Delta"/> and <see cref="UnscaledDelta"/> values so that frames can appear more smoothly in case of a long delay between frames. By default, this is set to 
    /// 0.33333 seconds.
    /// </remarks>
    public static double MaximumFrameDelta
    {
        get;
        set;
    } = 0.333333333;

    /// <summary>
    /// Property to return whether or not the timing has been started by <see cref="StartTiming"/>.
    /// </summary>
    public static bool TimingStarted => _timingStarted != 0;

    /// <summary>
    /// Property to scale the frame delta times.
    /// </summary>
    /// <remarks>Setting this value to 0 will pause, and a negative value will move things in reverse when using <see cref="Delta"/>.</remarks>
    public static float TimeScale
    {
        get;
        set;
    } = 1.0f;

    /// <summary>
    /// Property to return the number of seconds since a Gorgon application was started.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property starts counting at the first call to one of the <c>GorgonApplication.Run</c> methods. If this property is called prior to that, then 
    /// it will return 0.
    /// </para>
    /// <para>
    /// This value is affected by the <see cref="TimeScale"/> property, and will not update if the application is not in the foreground unless the application allows for background processing.
    /// </para>
    /// </remarks>
    public static float SecondsSinceStart => MillisecondsSinceStart / 1000.0f;

    /// <summary>
    /// Property to return the number of milliseconds since a Gorgon application was started.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This property starts counting at the first call to one of the <c>GorgonApplication.Run</c> methods. If this property is called prior to that, then 
    /// it will return 0.
    /// </para>
    /// <para>
    /// This value is affected by the <see cref="TimeScale"/> property, and will not update if the application is not in the foreground unless the application allows for background processing.
    /// </para>
    /// </remarks>
    public static float MillisecondsSinceStart => (float)_appTimer;

    /// <summary>
    /// Property to return the number of seconds to run the idle loop for a single iteration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a value to indicate how long it took an iteration of the idle loop to execute. Including the time it took for a video device to draw data. This is the preferred value 
    /// to read when checking for performance.
    /// </para>
    /// <para>
    /// The article at <a href="https://cgvr.cs.ut.ee/wp/index.php/frame-rate-vs-frame-time/" target="_blank">https://cgvr.cs.ut.ee/wp/index.php/frame-rate-vs-frame-time/</a> has more 
    /// information on using frame delta instead of frames per second. 
    /// </para>
    /// <para>
    /// This is the same as the <see cref="Delta"/> property when the <see cref="TimeScale">TimeScale</see> is set to 1.0f, otherwise, this value is not affected by <see cref="TimeScale"/>.  
    /// </para>
    /// <para>
    /// Because it is unaffected by <see cref="TimeScale"/>, this value is the one that should be used when measuring performance.
    /// </para>
    /// </remarks>
    public static float UnscaledDelta
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the number of seconds to run the idle loop for a single iteration with scaling.
    /// </summary>
    /// <remarks>This value is affected by the <see cref="TimeScale"/> property.</remarks>
    public static float Delta
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the number of frames that have been presented.
    /// </summary>
    public static uint FrameCount
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the number of frames that have been presented (using an unsigned 64 bit value).
    /// </summary>
    public static ulong FrameCountULong
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the average FPS.
    /// </summary>
    /// <remarks>
    /// Note that the averaged/min/max calculations are affected by the length of time it takes to execute a single iteration of the idle loop and will not have meaningful data until the 
    /// application loop begins processing after a call to one of the <c>GorgonApplication.Run</c> methods.
    /// </remarks>
    public static float AverageFPS
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the highest FPS.
    /// </summary>
    /// <remarks>
    /// Note that the averaged/min/max calculations are affected by the length of time it takes to execute a single iteration of the idle loop and will not have meaningful data until the 
    /// application loop begins processing after a call to one of the <c>GorgonApplication.Run</c> methods.
    /// </remarks>
    public static float HighestFPS
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the lowest FPS.
    /// </summary>
    /// <remarks>
    /// Note that the averaged/min/max calculations are affected by the length of time it takes to execute a single iteration of the idle loop and will not have meaningful data until the 
    /// application loop begins processing after a call to one of the <c>GorgonApplication.Run</c> methods.
    /// </remarks>
    public static float LowestFPS => _lowestFps ?? 0.0f;

    /// <summary>
    /// Property to return the highest idle loop delta.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that the averaged/min/max calculations are affected by the length of time it takes to execute a single iteration of the idle loop and will not have meaningful data until the 
    /// application loop begins processing after a call to one of the <c>GorgonApplication.Run</c> methods.
    /// </para>
    /// <para>
    /// This value is not affected by the <see cref="TimeScale"/> property because it is meant to be used in performance measurement.
    /// </para>
    /// </remarks>
    public static float HighestDelta
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the lowest idle loop delta.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that the averaged/min/max calculations are affected by the length of time it takes to execute a single iteration of the idle loop and will not have meaningful data until the 
    /// application loop begins processing after a call to one of the <c>GorgonApplication.Run</c> methods.
    /// </para>
    /// <para>
    /// This value is not affected by the <see cref="TimeScale"/> property because it is meant to be used in performance measurement.
    /// </para>
    /// </remarks>
    public static float LowestDelta => _lowestDelta ?? 0.0f;

    /// <summary>
    /// Property to return the average number of seconds to run the idle loop for a single iteration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that the averaged/min/max calculations are affected by the length of time it takes to execute a single iteration of the idle loop and will not have meaningful data until the 
    /// application loop begins processing after a call to one of the <c>GorgonApplication.Run</c> methods.
    /// </para>
    /// <para>
    /// This value is not affected by the <see cref="TimeScale"/> property because it is meant to be used in performance measurement.
    /// </para>
    /// </remarks>
    public static float AverageUnscaledDelta
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the average number of seconds to run the idle loop for a single iteration.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that the averaged/min/max calculations are affected by the length of time it takes to execute a single iteration of the idle loop and will not have meaningful data until the 
    /// application loop begins processing after a call to one of the <c>GorgonApplication.Run</c> methods.
    /// </para>
    /// <para>
    /// This value is affected by the <see cref="TimeScale"/> property.
    /// </para>
    /// </remarks>
    public static float AverageDelta
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return the maximum number of iterations before an average value is reset.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This only applies to the <see cref="AverageFPS"/>, <see cref="AverageDelta"/> and <see cref="AverageUnscaledDelta"/> properties.
    /// </para>
    /// <para>
    /// Note that the higher the value assigned to this property, the longer it'll take for the averages to compute, this is in addition to any overhead from the time it takes to execute a single 
    /// iteration of the idle loop.
    /// </para>
    /// </remarks>
    public static long MaxAverageCount
    {
        get => _maxAverageCount;
        set
        {
            if (_maxAverageCount < 0)
            {
                _maxAverageCount = 0;
            }

            _maxAverageCount = value;
        }
    }

    /// <summary>
    /// Property to return the number of frames per second.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is a value to indicate how many frames of information can be sent to the display by a video device within 1 second. While it is useful as a metric, it is not a good 
    /// measure of performance because FPS values are non-linear. This means that a rate of 1000 FPS dropping to 500 FPS is not a sign of poor performance. Simply because the 
    /// video device may not have had any work to do prior to measurement, and on the second reading, it drew an object to the display. 
    /// </para>
    /// <para>
    /// The proper value to use for performance is the <see cref="Delta"/> value. This tells how many seconds have occurred between the last time a frame was drawn the beginning 
    /// of the next frame.
    /// </para>
    /// <para>
    /// See the article at <a href="https://cgvr.cs.ut.ee/wp/index.php/frame-rate-vs-frame-time/" target="_blank">https://cgvr.cs.ut.ee/wp/index.php/frame-rate-vs-frame-time/</a> for more 
    /// information on using frame delta instead of frames per second. 
    /// </para>
    /// </remarks>
    public static float FPS
    {
        get;
        private set;
    }

    /// <summary>
    /// Function to gather timing data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Ensure that the <see cref="StartTiming"/> was called prior to calling this method, or no meaningful data will be collected.
    /// </para>
    /// <para>
    /// <note type="tip">
    /// If you are using the <c>Gorgon.Windows.GorgonApplication</c> class, you do not need to call this method since it contains its own idle processing and therefore will 
    /// call this method on your behalf.
    /// </note>
    /// </para>
    /// </remarks>
    public static void Update()
    {
        double theTime;
        double frameDelta;

        // If we've not assigned a timer yet, then just leave, we can't gather anything meaningful without one.
        if (_timer is null)
        {
            return;
        }

        do
        {
            theTime = _timer.Milliseconds;
            frameDelta = (theTime - _lastTimerValue);
        } while (frameDelta is < 0.000001 and not 0.0);

        // Cap the maximum frame delta to smooth out glitches.
        double maxDelta = MaximumFrameDelta * 1000;
        if (frameDelta > maxDelta)
        {
            frameDelta = maxDelta;
        }

        UnscaledDelta = (float)frameDelta / 1000.0f;

        // If the delta is 0, then put in the smallest possible positive value.
        if (Delta < 1e-6f)
        {
            Delta = 1e-6f;
        }

        Delta = UnscaledDelta * TimeScale;

        _appTimer += (decimal)frameDelta * (decimal)TimeScale;

        _lastTimerValue = theTime;

        unchecked
        {
            FrameCount++;
            FrameCountULong++;

            frameDelta = (_lastTimerValue - _lastTime);

            // If the value is too small, then don't use this time value.
            if (frameDelta < 1e-6)
            {
                return;
            }

            _frameCounter++;
            FPS = (float)((_frameCounter / frameDelta) * 1000.0);

            // Wait until we get one second of information.
            if (frameDelta >= 1000.0)
            {
                _lastTime = _lastTimerValue;
                _frameCounter = 0;

                HighestFPS = HighestFPS.Max(FPS);
                _lowestFps = _lowestFps?.Min(FPS) ?? FPS;
                HighestDelta = HighestDelta.Max(Delta);
                _lowestDelta = _lowestDelta?.Min(Delta) ?? FPS;
            }

            if (_averageCounter > 0)
            {
                AverageFPS = _averageFPSTotal / _averageCounter;
                AverageUnscaledDelta = _averageUnscaledDeltaTotal / _averageCounter;
                AverageDelta = _averageDeltaTotal / _averageCounter;
            }
            else
            {
                AverageFPS = FPS;
                AverageUnscaledDelta = UnscaledDelta;
                AverageDelta = Delta;
            }

            _averageFPSTotal += FPS;
            _averageUnscaledDeltaTotal += UnscaledDelta;
            _averageDeltaTotal += Delta;
            _averageCounter++;

            // Reset the average.
            if (_averageCounter < _maxAverageCount)
            {
                return;
            }

            _averageCounter = 0;
            _averageFPSTotal = 0;
            _averageDeltaTotal = 0;
            _averageUnscaledDeltaTotal = 0;
        }
    }

    /// <summary>
    /// Function to initialize the timing data with an existing timer.
    /// </summary>
    /// <param name="timer">The timer to use for the timing data.</param>
    /// <remarks>
    /// <para>
    /// Applications must call this method prior to using this class. Otherwise, no data will be present.
    /// </para>
    /// </remarks>
    public static void StartTiming(IGorgonTimer timer)
    {
        if (Interlocked.CompareExchange(ref _timingStarted, 1, 0) == 1)
        {
            return;
        }

        _timer = timer;

        Reset();
    }

    /// <summary>
    /// Function to clear the timing data and reset any timers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Like <see cref="Update"/>, you do not need to call this method unless you have your own mechanism for handling an idle time loop.
    /// </para>
    /// <para>
    /// Values set by the user (e.g. <see cref="MaxAverageCount"/>, etc...) will not be reset.
    /// </para>
    /// </remarks>
    public static void Reset()
    {
        HighestFPS = 0;
        _lowestFps = null;
        AverageFPS = 0.0f;
        HighestDelta = 0;
        _lowestDelta = null;
        AverageUnscaledDelta = 0.0f;
        AverageDelta = 0.0f;
        Delta = 0.0f;
        UnscaledDelta = 0.0f;
        FPS = 0.0f;
        FrameCountULong = FrameCount = 0;
        _averageCounter = 0;
        _frameCounter = 0;
        _lastTime = 0.0;
        _lastTimerValue = 0.0;
        _averageFPSTotal = 0.0f;
        _averageUnscaledDeltaTotal = 0.0f;

        _timer?.Reset();
    }

    /// <summary>
    /// Function to convert the desired frames per second to milliseconds.
    /// </summary>
    /// <param name="fps">Desired frames per second.</param>
    /// <returns>Frames per second in milliseconds.</returns>
    public static double FpsToMilliseconds(double fps) => fps > 0 ? 1000 / fps : 0;

    /// <summary>
    /// Function to convert the desired frames per second to microseconds.
    /// </summary>
    /// <param name="fps">Desired frames per second.</param>
    /// <returns>Frames per second in microseconds.</returns>
    public static double FpsToMicroseconds(double fps) => fps > 0 ? 1000000 / fps : 0;
}
