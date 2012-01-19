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
using GorgonLibrary.Native;

namespace GorgonLibrary.Diagnostics
{
	/// <summary>
	/// A timer interface for handling situations where higher accuracy is required for time measurement.
	/// </summary>
	public class GorgonTimer
	{
		#region Variables.
		private static bool _usingLowRes = false;		// Flag to indicate that we're using low resolution timers.
		private bool _isQPFtimer = false;				// Flag to indicate that this is a Query Performance timer.
		private long _frequency = 0;					// Frequency for the timer.
		private long _startTime = 0;					// Starting timer time.
		private long _startTick = 0;					// Starting tick.
		private long _currentTicks = 0;					// Current number of ticks elapsed.
		private double _microSeconds = 0.0;				// Number of microseconds elapsed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether we're using low resolution timers.
		/// </summary>
		internal static bool UsingLowResTimers
		{
			get
			{
				return _usingLowRes;
			}
		}

		/// <summary>
		/// Property to return the number of milliseconds elapsed since the timer was started.
		/// </summary>
		public double Milliseconds
		{
			get
			{
				return Microseconds / 1000;
			}
		}

		/// <summary>
		/// Property to return the number of microseconds elapsed since the timer was started.
		/// </summary>
		public double Microseconds
		{
			get
			{
				GetTime();
				return _microSeconds;
			}			
		}

		/// <summary>
		/// Property to return the number of seconds elapsed since the timer was started.
		/// </summary>
		public double Seconds
		{
			get
			{
				return Milliseconds / 1000.0;
			}
		}

		/// <summary>
		/// Property to return the number of minutes elapsed since the timer was started.
		/// </summary>
		public double Minutes
		{
			get
			{
				return Seconds / 60.0;
			}
		}

		/// <summary>
		/// Property to return the number of hours elapsed since the timer was started.
		/// </summary>
		public double Hours
		{
			get
			{
				return Minutes / 60.0;
			}
		}

		/// <summary>
		/// Property to return the number of days elapsed since the timer was started.
		/// </summary>
		public double Days
		{
			get
			{
				return Hours / 24.0;
			}
		}

		/// <summary>
		/// Property to return the number of ticks since the timer was started.
		/// </summary>
		public long Ticks
		{
			get
			{
				GetTime();
				return _currentTicks;
			}
		}

		/// <summary>
		/// Property to return whether this timer is high resolution or not.
		/// </summary>
		/// <remarks>Implementors need to set this value.</remarks>
		public bool IsHighResolution
		{
			get
			{
				return _isQPFtimer;
			}
			set
			{
				_isQPFtimer = value;
				Reset();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to return the Query Performance Counter time.
		/// </summary>
		private void GetQPFTime()
		{
			long currentTime = 0;

			Win32API.QueryPerformanceCounter(out currentTime);
			_currentTicks =  currentTime - _startTime;
			_microSeconds = (_currentTicks * 1000000.0) / _frequency;
		}

		/// <summary>
		/// Function to retrieve the timeGetTime time data.
		/// </summary>
		private void GetWin32Time()
		{
			long currentTime = Win32API.timeGetTime();
			long ticks = Environment.TickCount;

			// Handle wrap around every ~50 days.
			if (currentTime < _startTime)
			{
				long diff = 4294967295 - _startTime;
				_startTime = diff;
				currentTime += diff;
			}

			if (ticks < _startTick)
			{
				long diff = 4294967295 - _startTick;
				_startTick = diff;
				ticks += diff;
			}

			_microSeconds = (currentTime - _startTime) * 1000.0;
			_currentTicks = ticks - _startTick;
		}

		/// <summary>
		/// Function to retrieve the time data.
		/// </summary>
		/// <returns>The number of clock ticks since the timer was started.</returns>
		private void GetTime()
		{
			if (_isQPFtimer)
				GetQPFTime();
			else
				GetWin32Time();
		}

		/// <summary>
		/// Function to reset the low resolution timer period.
		/// </summary>
		internal static void ResetLowResTimerPeriod()
		{
			if (_usingLowRes)
			{
				_usingLowRes = false;
				Win32API.timeEndPeriod(1);
			}
		}

		/// <summary>
		/// Function to reset the timer.
		/// </summary>
		public void Reset()
		{
			if (_isQPFtimer)
			{
				if (Win32API.QueryPerformanceFrequency(out _frequency))
					Win32API.QueryPerformanceCounter(out _startTime);
				else
					_isQPFtimer = false;
			}

			if (!_isQPFtimer)
			{
				if (_usingLowRes)
				{
					Win32API.timeBeginPeriod(1);
					_usingLowRes = true;
				}

				_startTick = Environment.TickCount;
				_startTime = Win32API.timeGetTime();
			}
		}
		#endregion

		#region Constructors/Destructors.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTimer"/> class.
		/// </summary>
		/// <param name="useHighResolution">TRUE to use a high resolution timer, FALSE to use the standard timer.</param>
		/// <remarks>The high resolution timer uses QueryPerformanceCounter, and the standard timer uses timeGetTime.</remarks>
		public GorgonTimer(bool useHighResolution)
		{
			if (useHighResolution)
			{
				if (Win32API.QueryPerformanceFrequency(out _frequency))
					_isQPFtimer = true;
			}
			else
			{
				if (!_usingLowRes)
				{
					_usingLowRes = true;
					Win32API.timeBeginPeriod(1);
				}
			}

			Reset();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonTimer"/> class.
		/// </summary>
		public GorgonTimer() 
			: this(true)
		{
		}
		#endregion
	}
}
