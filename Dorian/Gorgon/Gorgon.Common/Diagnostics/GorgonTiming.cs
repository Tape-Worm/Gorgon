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
// Created: Saturday, June 18, 2011 10:29:46 AM
// 
#endregion

using GorgonLibrary.Math;

namespace GorgonLibrary.Diagnostics
{	
	/// <summary>
	/// Timing data for code within a Gorgon Idle loop.
	/// </summary>
	/// <remarks>This object is used to calculate the time it takes for a single iteration (but, continuously) of the idle loop to execute, the frames per second, and the time elapsed since the application started as well as peaks, lows and averages for those values.
	/// <para>This object will automatically gather data if your application has an <see cref="GorgonLibrary.Gorgon.ApplicationIdleLoopMethod">idle loop</see> assigned.  Otherwise, if a custom idle time polling method is 
	/// used, then the user should call <see cref="GorgonLibrary.Diagnostics.GorgonTiming.Reset">Reset</see> before starting the application loop, and <see cref="GorgonLibrary.Diagnostics.GorgonTiming.Update">Update</see> at the beginning of the idle loop.</para>
	/// </remarks>
	public static class GorgonTiming
	{
		#region Variables.
		private static float _baseTime = 0;							// Base time at which the system started.
		private static bool _useHRTimer = true;						// Flag to indicate that we're using a high resolution timer.
		private static GorgonTimer _timer = null;					// Timer used in calculations.
		private static long _frameCounter = 0;						// Frame counter.
		private static long _averageCounter = 0;					// Counter for averages.
		private static double _lastTime = 0.0;						// Last recorded time since update.
		private static double _lastTimerValue = 0.0;				// Last timer value.
		private static float _averageFPSTotal = 0.0f;				// Average FPS total.
		private static float _averageSDTTotal = 0.0f;				// Average draw time total.
		private static float _averageDTTotal = 0.0f;				// Average draw time total.
		private static bool _firstCall = true;						// Flag to indicate that this is the first call.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to scale the frame delta times.
		/// </summary>
		/// <remarks>Setting this value to 0 will pause, and a negative value will move things in reverse when using <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.ScaledDelta">ScaledDelta</see>.</remarks>
		public static float TimeScale
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the number of seconds since a Gorgon application was started.
		/// </summary>
		/// <remarks>This property starts counting at the first start of a Gorgon application and will continue to end of the application.
		/// <para>This value is not affected by the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.TimeScale">TimeScale</see> property.</para>
		/// </remarks>
		public static float SecondsSinceStart
		{
			get
			{
				return MillisecondsSinceStart / 1000.0f;
			}
		}

		/// <summary>
		/// Property to return the number of milliseconds since a Gorgon application was started.
		/// </summary>
		/// <remarks>This property starts counting at the first start of a Gorgon application and will continue to end of the application.
		/// <para>This value is not affected by the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.TimeScale">TimeScale</see> property.</para>
		/// </remarks>
		public  static float MillisecondsSinceStart
		{
			get
			{
				return (float)_timer.Milliseconds - _baseTime;
			}
		}

		/// <summary>
		/// Property to return the number of seconds to run the idle loop for a single iteration.
		/// </summary>
		/// <remarks>This is the same as the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.FrameDelta">FrameDelta</see> property when the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.TimeScale">TimeScale</see> is set to 1.0f, otherwise, this value is not affected by TimeScale.  
		/// <para>Because it is unaffected by TimeScale, this value is the one that should be used when measuring performance.</para></remarks>
		public static float Delta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of seconds to run the idle loop for a single iteration with scaling.
		/// </summary>
		/// <remarks>This value is affected by the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.TimeScale">TimeScale</see> property.</remarks>
		public static float ScaledDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of frames that have been presented.
		/// </summary>
		public static long FrameCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the average FPS.
		/// </summary>
		public static float AverageFPS
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the highest FPS.
		/// </summary>
		public static float HighestFPS
		{
			get;
			private set;
		}

		/// <summary>
		/// Propery to return the lowest FPS.
		/// </summary>
		public static float LowestFPS
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the highest idle loop delta.
		/// </summary>
		/// <remarks>This value is affected not by the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.TimeScale">TimeScale</see> property because it is meant to be used in performance measurement.</remarks>
		public static float HighestDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the lowest idle loop delta.
		/// </summary>
		/// <remarks>This value is affected not by the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.TimeScale">TimeScale</see> property because it is meant to be used in performance measurement.</remarks>
		public static float LowestDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the average number of seconds to run the idle loop for a single iteration.
		/// </summary>
		/// <remarks>This value can be used to get a smoother delta value over time.
		/// <para>This value is not affected by the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.TimeScale">TimeScale</see> property.</para>
		/// </remarks>
		public static float AverageDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the average number of seconds to run the idle loop for a single iteration.
		/// </summary>
		/// <remarks>This value can be used to get a smoother delta value over time.
		/// <para>This value is affected by the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.TimeScale">TimeScale</see> property.</para>
		/// </remarks>
		public static float AverageScaledDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of frames per second.
		/// </summary>
		public static float FPS
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether to use a high resolution timer
		/// </summary>
		public static bool UseHighResolutionTimer
		{
			get
			{
				return _useHRTimer;
			}

			set
			{
				_useHRTimer = value;
				Reset();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to gather timing data.
		/// </summary>
		/// <remarks>You do not need to call this method unless you've got your own mechanism for handling an idle time loop.</remarks>
		public static void Update()
		{
			double theTime = 0.0;				// Time value.
			double delta = 0.0;					// Frame delta.

			if (!_firstCall)
			{
				Reset();
				_firstCall = true;
			}

			do
			{
				theTime = _timer.Milliseconds;
				delta = (theTime - _lastTimerValue);
			} while ((delta < 0.0001) && (delta != 0.0));

			// If our delta since the last time was too high, then don't allow any movement until
			// the simulation is caught up.
			if (delta > 200)
				delta = 0;

			Delta = (float)delta / 1000.0f;
			ScaledDelta = Delta * TimeScale;

			_lastTimerValue = theTime;

			unchecked
			{
				FrameCount++;

				_frameCounter++;
				delta = _lastTimerValue - _lastTime;

				FPS = (float)(_frameCounter / delta) * 1000.0f;

				// Wait until we get one second of information.
				if (delta >= 1000.0)
				{
					_lastTime = _lastTimerValue;
					_frameCounter = 0;

					if (FPS > HighestFPS)
						HighestFPS = FPS;
					if (FPS < LowestFPS)
						LowestFPS = FPS;

					if (Delta > HighestDelta)
						HighestDelta = Delta;
					if (Delta < LowestDelta)
						LowestDelta = Delta;
				}

				if (_averageCounter > 0)
				{
					AverageFPS = _averageFPSTotal / _averageCounter;
					AverageScaledDelta = _averageSDTTotal / _averageCounter;
					AverageDelta = _averageDTTotal / _averageCounter;
				}
				else
				{
					AverageFPS = FPS;
					AverageScaledDelta = ScaledDelta;
					AverageDelta = Delta;
				}

				_averageFPSTotal += FPS;
				_averageSDTTotal += ScaledDelta;
				_averageDTTotal += Delta;
				_averageCounter++;

				// Don't overflow.
				if (_averageCounter > 2147483600)
				{
					_averageCounter = 0;
					_averageFPSTotal = 0;
					_averageDTTotal = 0;
					_averageSDTTotal = 0;
				}
			}
		}

		/// <summary>
		/// Function to clear the timing data and reset any timers.
		/// </summary>
		/// <remarks>You do not need to call this method unless you've got your own mechanism for handling an idle time loop.</remarks>
		public static void Reset()
		{
			if ((_timer == null) || (UseHighResolutionTimer != _timer.IsHighResolution))
				_timer = new GorgonTimer(UseHighResolutionTimer);

			_useHRTimer = _timer.IsHighResolution;
			HighestFPS = float.MinValue;
			LowestFPS = float.MaxValue;
			AverageFPS = 0.0f;
			HighestDelta = float.MinValue;
			LowestDelta = float.MaxValue;
			AverageScaledDelta = 0.0f;
			AverageDelta = 0.0f;
			Delta = 0.0f;
			ScaledDelta = 0.0f;
			FPS = 0.0f;
			FrameCount = 0;
			_averageCounter = 0;
			_frameCounter = 0;
			_lastTime = 0.0;
			_lastTimerValue = 0.0;
			_averageFPSTotal = 0.0f;
			_averageSDTTotal = 0.0f;
			_timer.Reset();
			_baseTime = (float)_timer.Milliseconds;
		}


		/// <summary>
		/// Function to convert the desired frames per second to milliseconds.
		/// </summary>
		/// <param name="fps">Desired frames per second.</param>
		/// <returns>Frames per second in milliseconds.</returns>
		public static double FpsToMilliseconds(double fps)
		{
			if (fps > 0)
				return 1000 / fps;
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
				return 1000000 / fps;
			else
				return 0;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonTiming"/> class.
		/// </summary>
		static GorgonTiming()
		{
			Reset();
			TimeScale = 1;
		}
		#endregion
	}
}
