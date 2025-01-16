
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
// Created: Saturday, June 18, 2011 10:35:53 AM
// 

using System.ComponentModel;
using Gorgon.Native;
using Gorgon.Windows.Properties;

namespace Gorgon.Timing;

/// <summary>
/// An implementation of the <see cref="IGorgonTimer"/> interface
/// </summary>
/// <remarks>
/// <para>
/// This implementation uses the <a href="https://msdn.microsoft.com/en-us/library/ms644904(v=vs.85).aspx" target="_blank">Query Performance Counter</a>. It provides accuracy of 1 microsecond. 
/// </para>
/// <para>
/// <note type="caution">
///		<h3>Issues</h3>  
///		<para>
///		On older processors this functionality may not be supported at all. If Gorgon can't detect support for this type of timer, it will throw an exception. In order to avoid this, use the 
///		<see cref="SupportsQpc"/> static method on this class to determine if the system will support the timer. This should not be an issue with a reasonably modern system
///		</para> 
///		<para>
///		This is an issue on systems with Windows versions before Windows Vista because multi-core systems may not report the correct time stamp. Since Gorgon requires Windows 10 at minimum, 
///		this should not be an issue
///		</para>
///		<para>
///		There may be a performance hit using this timer, depending on your operating system version and hardware
///		</para>
///		<para>
///		For a comprehensive overview of the Qpc timer, go to <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/dn553408(v=vs.85).aspx" target="_blank">https://msdn.microsoft.com/en-us/library/windows/desktop/dn553408(v=vs.85).aspx</a>
///		</para>
/// </note>
/// </para>
/// </remarks>
public sealed class GorgonTimerQpc
    : IGorgonTimer
{

    // Flag to indicate that the timer counter is queried.
    private bool _queried;
    // Frequency for the timer.
    private long _frequency;
    // Starting timer time.
    private long _startTime;
    // Current number of ticks elapsed.
    private long _currentTicks;
    // Number of microseconds elapsed.
    private double _microSeconds;

    /// <summary>
    /// Property to return the number of milliseconds elapsed since the timer was started.
    /// </summary>
    public double Milliseconds => Microseconds / 1000;

    /// <summary>
    /// Property to return the number of microseconds elapsed since the timer was started.
    /// </summary>
    public double Microseconds
    {
        get
        {
            GetQpcTime();
            return _microSeconds;
        }
    }

    /// <summary>
    /// Property to return the number of seconds elapsed since the timer was started.
    /// </summary>
    public double Seconds => Milliseconds / 1000.0;

    /// <summary>
    /// Property to return the number of minutes elapsed since the timer was started.
    /// </summary>
    public double Minutes => Seconds / 60.0;

    /// <summary>
    /// Property to return the number of hours elapsed since the timer was started.
    /// </summary>
    public double Hours => Minutes / 60.0;

    /// <summary>
    /// Property to return the number of days elapsed since the timer was started.
    /// </summary>
    public double Days => Hours / 24.0;

    /// <summary>
    /// Property to return the number of ticks since the timer was started.
    /// </summary>
    public long Ticks
    {
        get
        {
            GetQpcTime();
            return _currentTicks;
        }
    }

    /// <summary>
    /// Property to return whether this timer has a resolution of less than 1 millisecond or not.
    /// </summary>
    public bool IsHighResolution => true;

    /// <summary>
    /// Function to return the Query Performance Counter time.
    /// </summary>
    private void GetQpcTime()
    {
        if (!_queried)
        {
            Reset();
        }

        unchecked
        {

            KernelApi.QueryPerformanceCounter(out long currentTime);
            _currentTicks = currentTime - _startTime;
            _microSeconds = (_currentTicks * 1000000.0) / _frequency;
        }
    }

    /// <summary>
    /// Function to return whether the system supports the query performance counter.
    /// </summary>
    /// <returns><b>true</b> if the system supports the timer, <b>false</b> if it does not.</returns>
    public static bool SupportsQpc() => KernelApi.QueryPerformanceFrequency(out long _);

    /// <summary>
    /// Function to reset the timer.
    /// </summary>
    public void Reset()
    {
        if (!KernelApi.QueryPerformanceFrequency(out _frequency))
        {
            throw new Win32Exception(Resources.GOR_ERR_TIME_QPC_NOT_AVAILABLE);
        }

        KernelApi.QueryPerformanceCounter(out _startTime);
        _queried = true;
    }
}
