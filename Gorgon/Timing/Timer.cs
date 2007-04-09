#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Sunday, September 25, 2005 12:00:29 PM
// 
#endregion

using System;
using System.Diagnostics;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Native.Win32;

namespace GorgonLibrary.Timing
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
		private long _startTime;			// Starting timer time.
		private bool _useHighResolution;	// Flag to indicate that we're using a high resolution timer.
		private long _frequency;			// Frequency of the timer.
		private double _milliseconds;		// Milliseconds.
		private long _ticks;				// Clock ticks.
		private long _lastTicks;			// Last tick count.
		private long _startingTick;			// Starting clock tick.
		private long _adjustStartTick;		// Adjustment starting tick.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the number of milliseconds elapsed since the timer was started.
		/// </summary>
		public double Milliseconds
		{
			get
			{
				GetTime();
				return _milliseconds;
			}
		}

		/// <summary>
		/// Property to return the number of microseconds elapsed since the timer was started.
		/// </summary>
		public double Microseconds
		{
			get
			{
				return Milliseconds * 1000;
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
				_milliseconds = ((double)_ticks/(double)_frequency) * 1000;

				// The QueryPerformance counter drifts on some motherboards.
				// See Microsoft KB: Q274323 for the explanation.
				ulong check = (ulong)Environment.TickCount - (ulong)_adjustStartTick;	// Get tick count.
				long msDrift = (long)(check - _milliseconds);							// Get any millisecond drifts.

				// If we're out by 100 milliseconds either way, then adjust the time.
				if ((msDrift < -100) || (msDrift > 100))
				{
					// Get adjustment amount.
					long adjust = Math.Min(msDrift * _frequency / 1000, _ticks - _lastTicks);
					_startingTick += adjust;
					_ticks -= adjust;

					// Recalcuate the timing.
					_milliseconds = ((double)_ticks / (double)_frequency) * 1000;
				}

				_lastTicks = _ticks;
			}
			else
			{
				currentTime = Win32API.timeGetTime();
				_milliseconds = currentTime - _startTime;
				_ticks = Environment.TickCount - _startingTick;
			}
		}

		/// <summary>
		/// Function to reset the timer.
		/// </summary>
		public void Reset()
		{
			_milliseconds = 0;
			_ticks = 0;
			_lastTicks = 0;
			_adjustStartTick = _startingTick = Environment.TickCount;

			// Determine which type of timer to use.
			if (_useHighResolution)
			{
				// If this fails, then drop to the lower precision timer.
				if (!Win32API.QueryPerformanceCounter(ref _startTime))
				{
					_startTime = Win32API.timeGetTime();
					_useHighResolution = false;
				}
			}
			else
			{
				_startTime = Win32API.timeGetTime();
				_useHighResolution = false;
			}
		}

		/// <summary>
		/// Function to convert the desired frames per second to milliseconds.
		/// </summary>
		/// <param name="FPS">Desired frames per second.</param>
		/// <returns>Frames per second in milliseconds.</returns>
		public static double FPSToMilliseconds(double FPS)
		{
			if (FPS > 0)
				return (1000/(FPS + 3));
			else
				return 0;
		}

		/// <summary>
		/// Function to convert the desired frames per second to microseconds.
		/// </summary>
		/// <param name="FPS">Desired frames per second.</param>
		/// <returns>Frames per second in microseconds.</returns>
		public static double FPSToMicroseconds(double FPS)
		{
			if (FPS > 0)
				return (1000000/(FPS + 3));
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
			_frequency = 0;
			_useHighResolution = false;
			_adjustStartTick = _startingTick = Environment.TickCount;
			_lastTicks = 0;

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
