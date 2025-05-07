
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Saturday, June 18, 2011 10:35:53 AM
// 

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Gorgon.Core.Windows.Properties;
using Windows.Win32;

namespace Gorgon.Timing;

/// <summary>
/// A windows specific implementation of the <see cref="IGorgonTimer"/> interface using the query performance counter for high precision.
/// </summary>
/// <remarks>
/// <para>
/// This implementation uses the <a href="https://msdn.microsoft.com/en-us/library/ms644904(v=vs.85).aspx" target="_blank">Query Performance Counter</a>. It provides an accuracy of 1 microsecond. 
/// </para>
/// </remarks>
public sealed class GorgonTimer
    : IGorgonTimer
{
    // Flag to indicate that the timer counter is queried.
    private bool _queried;
    // Starting timer time.
    private long _startTime;
    // The time span that contains the time elapsed.
    private TimeSpan _timeSpan;

    /// <inheritdoc/>
    public TimeSpan Elapsed => _timeSpan;

    /// <inheritdoc/>
    public double Milliseconds
    {
        get
        {
            GetQpcTime();
            return _timeSpan.TotalMilliseconds;
        }
    }

    /// <inheritdoc/>
    public double Nanoseconds
    {
        get
        {
            GetQpcTime();
            return _timeSpan.TotalNanoseconds;
        }
    }

    /// <inheritdoc/>
    public double Microseconds
    {
        get
        {
            GetQpcTime();
            return _timeSpan.TotalMicroseconds;
        }
    }

    /// <inheritdoc/>
    public double Seconds
    {
        get
        {
            GetQpcTime();
            return _timeSpan.TotalSeconds;
        }
    }

    /// <inheritdoc/>
    public long Ticks
    {
        get
        {
            GetQpcTime();
            return _timeSpan.Ticks;
        }
    }

    /// <inheritdoc/>
    public bool IsHighResolution => true;

    /// <summary>
    /// Function to return the Query Performance Counter time.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void GetQpcTime()
    {
        unchecked
        {
            if (!_queried)
            {
                Reset();
            }

            PInvoke.QueryPerformanceCounter(out long currentTime);
            _timeSpan = new TimeSpan(currentTime - _startTime);
        }
    }

    /// <inheritdoc/>
    public void Reset()
    {
        if (!PInvoke.QueryPerformanceFrequency(out _))
        {
            throw new Win32Exception(Resources.GOR_ERR_TIME_QPC_NOT_AVAILABLE);
        }

        PInvoke.QueryPerformanceCounter(out _startTime);
        _queried = true;
        _timeSpan = default;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTimer"/> class.
    /// </summary>
    /// <exception cref="Win32Exception">Thrown if the system does not support QPC timers.</exception>
    public GorgonTimer()
    {
        Reset();
        GetQpcTime();
    }
}
