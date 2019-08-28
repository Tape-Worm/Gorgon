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
// Created: Saturday, June 18, 2011 10:35:53 AM
// 
#endregion

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Gorgon.Native;
using Gorgon.UI;
using Gorgon.Windows.Properties;

namespace Gorgon.Timing
{
    /// <summary>
    /// An implementation of the <see cref="IGorgonTimer"/> interface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This implementation uses the <a href="https://msdn.microsoft.com/en-us/library/dd757629(v=vs.85).aspx" target="_blank">Windows Multimedia timer</a>. It fairly stable across all hardware and provides 
    /// measurement of time down to 1 millisecond. 
    /// </para>
    /// <para>
    /// <note type="caution">
    /// <h3>Known issues</h3>
    /// There are a few caveats when using this timer:
    /// <list type="bullet">
    /// <item><description>This timer wraps around to 0 every 2^32 milliseconds (~49.71 days), and can cause issues in code relying on this type of timer. Gorgon will do its best to ensure this is not a problem.</description></item>
    /// <item><description> It has a default precision of 5 milliseconds, but Gorgon will attempt to change the resolution to 1 millisecond when using the <see cref="GorgonApplication"/> class.</description></item>
    /// </list>
    /// The second bullet point is important, since it will change the timer frequency not only for the application, but across the system. This may cause weird issues with other applications relying on this 
    /// timer. Also, since only the <see cref="GorgonApplication"/> class manages the period for the timer automatically, it'll be up to the user to set and reset the period frequency via the <see cref="BeginTiming"/> 
    /// and <see cref="EndTiming"/> static methods on this class if the <see cref="GorgonApplication"/> class is not used in your application. It is vitally important that the <see cref="EndTiming"/> method is called 
    /// when the <see cref="BeginTiming"/> method is used. Failure to do so can leave the operating system timing in a state where the task scheduler switches tasks more often and the power saving may not trigger 
    /// properly.
    /// </note>
    /// </para>
    /// </remarks>
    public sealed class GorgonTimerMultimedia
        : IGorgonTimer
    {
        #region Constants.
        // Error code returned when setting timer period.
        private const uint ErrorNoCanDo = 97;
        #endregion

        #region Variables.
        // Flag to indicate that the timer was initialized.
        private bool _initialized;
        // The last period for the timer.
        private static int? _lastPeriod;
        // Starting timer time.
        private long _startTime;
        // Starting tick.
        private long _startTick;
        // Current number of ticks elapsed.
        private long _currentTicks;
        // Number of milliseconds elapsed.
        private double _milliseconds;
        // Timer capabilities.
        private static TIMECAPS _timeCaps;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the number of milliseconds elapsed since the timer was started.
        /// </summary>
        public double Milliseconds
        {
            get
            {
                GetWin32Time();
                return _milliseconds;
            }
        }

        /// <summary>
        /// Property to return the number of microseconds elapsed since the timer was started.
        /// </summary>
        /// <remarks>
        /// For this type of timer, this value is pretty much meaningless as the best precision of the timer is 1 millisecond at best (assuming <see cref="BeginTiming"/> was called).
        /// </remarks>
        public double Microseconds => Milliseconds * 1000.0;

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
                GetWin32Time();
                return _currentTicks;
            }
        }

        /// <summary>
        /// Property to return whether this timer has a resolution of less than 1 millisecond or not.
        /// </summary>
        public bool IsHighResolution => false;

        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the timeGetTime time data.
        /// </summary>
        private void GetWin32Time()
        {
            if (!_initialized)
            {
                Reset();
            }

            long currentTime = WinMultimediaApi.timeGetTime();
            long ticks = Environment.TickCount;

            // Handle wrap around every ~50 days.
            if (currentTime < _startTime)
            {
                long diff = uint.MaxValue - _startTime;
                _startTime = diff;
                currentTime += diff;
            }

            if (ticks < _startTick)
            {
                long diff = uint.MaxValue - _startTick;
                _startTick = diff;
                ticks += diff;
            }

            unchecked
            {
                _milliseconds = (currentTime - _startTime);
                _currentTicks = ticks - _startTick;
            }
        }

        /// <summary>
        /// Function to set the timing period for the timer.
        /// </summary>
        /// <param name="period">[Optional] Minimum timer resolution, in milliseconds, for the application or device driver. A lower value specifies a higher (more accurate) resolution..</param>
        /// <remarks>
        /// <para>
        /// This will set the timing period for calls to the Win32 <a href="https://msdn.microsoft.com/en-us/library/dd757629(v=vs.85).aspx" target="_blank">timeGetTime</a> API function so that the resolution for the timer 
        /// can be adjusted for maximum performance.
        /// </para>
        /// <para>
        /// Calls to this method must be paired with the <see cref="EndTiming"/> static method, otherwise the system will be left in a state where the task scheduler switches tasks more often and the power 
        /// saving may not trigger properly.
        /// </para>
        /// </remarks>
        /// <exception cref="Win32Exception">Thrown when the <paramref name="period"/> value is out of range.</exception>
        public static void BeginTiming(int period = 1)
        {
            if (_lastPeriod != null)
            {
                EndTiming();
            }

            if (period < _timeCaps.MinPeriod)
            {
                period = (int)_timeCaps.MinPeriod;
            }

            if (WinMultimediaApi.timeBeginPeriod((uint)period) == ErrorNoCanDo)
            {
                throw new Win32Exception(Resources.GOR_ERR_TIME_CANNOT_BEGIN);
            }

            _lastPeriod = period;
        }

        /// <summary>
        /// Function to end the timing period for the timer.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This will end the current timing period for calls to the Win32 <a href="https://msdn.microsoft.com/en-us/library/dd757629(v=vs.85).aspx" target="_blank">timeGetTime</a> API function.
        /// </para>
        /// <para>
        /// Calls to this method must be paired with the <see cref="BeginTiming"/> static method, otherwise the system will be left in a state where the task scheduler switches tasks more often and the power 
        /// saving may not trigger properly. If <see cref="BeginTiming"/> was never called, then this method will do nothing.
        /// </para>
        /// </remarks>
        /// <exception cref="Win32Exception">Thrown when the last known period value is out of range.</exception>
        public static void EndTiming()
        {
            if (_lastPeriod == null)
            {
                return;
            }

            if (WinMultimediaApi.timeEndPeriod((uint)_lastPeriod.Value) == ErrorNoCanDo)
            {
                throw new Win32Exception(Resources.GOR_ERR_TIME_CANNOT_END);
            }

            _lastPeriod = null;
        }

        /// <summary>
        /// Function to reset the timer.
        /// </summary>
        /// <exception cref="Win32Exception">Thrown when timer information cannot be retrieved from the operating system.</exception>
        public void Reset()
        {
            if (WinMultimediaApi.timeGetDevCaps(ref _timeCaps, Unsafe.SizeOf<TIMECAPS>()) != 0)
            {
                throw new Win32Exception(Resources.GOR_ERR_TIME_CANNOT_BEGIN);
            }

            if (_lastPeriod != null)
            {
                int period = _lastPeriod.Value;
                EndTiming();
                BeginTiming(period);
            }

            _startTick = Environment.TickCount;
            _startTime = WinMultimediaApi.timeGetTime();
            _initialized = true;
        }
        #endregion
    }
}
