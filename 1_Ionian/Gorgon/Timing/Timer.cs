#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Sunday, September 25, 2005 12:00:29 PM
// 
#endregion

using System;
using System.Diagnostics;
using GorgonLibrary.Internal.Native;

namespace GorgonLibrary
{
	/// <summary>
	/// Object to handle timing operations.
	/// </summary>
	/// <remarks>
	/// Timers are used to regulate and measure time elapsed for frames, or maintaining consistent
	/// frame rate for animations, to just simple countdowns.
	/// </remarks>
	public class PreciseTimer
	{
		#region Variables.
		private static bool _driftAdjust = false;	// Flag to indicate that the timer needs drift adjustment.
		private long _startTime;					// Starting timer time.
		private bool _useHighResolution;			// Flag to indicate that we're using a high resolution timer.
		private long _frequency;					// Frequency of the timer.
		private long _ticks;						// Clock ticks.
		private long _lastTicks;					// Last tick count.
		private long _startingTick;					// Starting clock tick.
		private ulong _adjustStartTick;				// Adjustment starting tick.
		private double _microseconds;				// Microseconds.
		private double _milliseconds;				// Milliseconds.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the use the timer adjustments to compensate for timer drift.
		/// </summary>
		/// <remarks>This is caused by unexpected data across the PCI to ISA bridge, aka south bridge.  See Microsoft KB274323.</remarks>
		public static bool CompensateForDrift
		{
			get
			{
				return _driftAdjust;
			}
			set
			{
				_driftAdjust = value;
			}
		}

		/// <summary>
		/// Property to return the number of milliseconds elapsed since the timer was started.
		/// </summary>
		public double Milliseconds
		{
			get
			{
				if (_useHighResolution)
					return Microseconds / 1000;
				else
				{
					GetTime();
					return _milliseconds;
				}
			}
		}

		/// <summary>
		/// Property to return the number of microseconds elapsed since the timer was started.
		/// </summary>
		public double Microseconds
		{
			get
			{
				if (_useHighResolution)
				{
					GetTime();
					return _microseconds;
				}
				else
					return Milliseconds * 1000.0;
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
				return _ticks;
			}
		}

		/// <summary>
		/// Property to set or return whether to use the high resolution timer or not.
		/// </summary>
		public bool HighResolutionTimer
		{
			get
			{
				return _useHighResolution;
			}
			set
			{
				_useHighResolution = value;
				Reset();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to start the timing process.
		/// </summary>
		private void GetTime()
		{
			long currentTime = 0;		// Current time.

			if (_useHighResolution)
			{
				Win32API.QueryPerformanceCounter(ref currentTime);
				_ticks = currentTime - _startTime;
				_microseconds = ((double)_ticks / _frequency) * 1000000.0;

				// Check for unexpected leaps in the Win32 performance counter.  
				// (This is caused by unexpected data across the PCI to ISA bridge, aka south bridge.  See Microsoft KB274323.)
				if (_driftAdjust)
				{
					ulong check = (ulong)Environment.TickCount - _adjustStartTick;			// Get tick count.
					long msDrift = (long)((_microseconds / 1000) - check);					// Get any millisecond drifts.

					// If we're out by 100 milliseconds in either direction, then adjust the time.
					if ((msDrift < -100) || (msDrift > 100))
					{
						// Get adjustment amount.
						long adjust = Math.Min(msDrift * _frequency / 1000000, _ticks - _lastTicks);
						_startingTick += adjust;
						_ticks -= adjust;

						// Recalcuate the timing.
						_microseconds = (double)(_ticks * 1000000 / _frequency);
					}

					_lastTicks = _ticks;
				}
			}
			else
			{
				currentTime = Win32API.timeGetTime();
				_milliseconds = (currentTime - _startTime);
				_ticks = Environment.TickCount - _startingTick;
			}
		}

		/// <summary>
		/// Function to reset the timer.
		/// </summary>
		public void Reset()
		{
			_microseconds = 0;
			_milliseconds = 0;
			_ticks = 0;
			_lastTicks = 0;
			_startingTick = Environment.TickCount;
			_adjustStartTick = (ulong)_startingTick;

			// Determine which type of timer to use.
			if ((!_useHighResolution) || (!Win32API.QueryPerformanceCounter(ref _startTime)))
			{
				_startTime = Win32API.timeGetTime();
				_useHighResolution = false;
			}
		}

		/// <summary>
		/// Function to convert the desired frames per second to milliseconds.
		/// </summary>
		/// <param name="fps">Desired frames per second.</param>
		/// <returns>Frames per second in milliseconds.</returns>
		public static double FpsToMilliseconds(double fps)
		{
			if (fps > 0)
				return 1000/fps;
			else
				return 0;
		}

		/// <summary>
		/// Function to convert the desired frames per second to microseconds.
		/// </summary>
		/// <param name="fps">Desired frames per second.</param>
		/// <returns>Frames per second in microseconds.</returns>
		public static double FpsToMicroseconds(double fps)
		{
			if (fps > 0)
				return 1000000/fps;
			else
				return 0;
		}
		#endregion

		#region Constructors/Destructors.
		/// <summary>
		/// Constructor.
		/// </summary>
		public PreciseTimer() 
		{
			_startTime = 0;
			_useHighResolution = false;
			_startingTick = Environment.TickCount;
			_adjustStartTick = (ulong)_startingTick;

			if (Win32API.QueryPerformanceFrequency(ref _frequency))
			{
				if (Win32API.QueryPerformanceCounter(ref _startTime))
					_useHighResolution = true;
			}

			// Fall back on the low resolution timer if no high resolution one can be found.
			if (!_useHighResolution)
				_startTime = Win32API.timeGetTime();				
		}
		#endregion
	}
}
