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

using Gorgon.Core;

namespace GorgonLibrary.Diagnostics
{	
	/// <summary>
	/// Timing data for code within a Gorgon Idle loop.
	/// </summary>
	/// <remarks>This object is used to calculate the time it takes for a single iteration (but, continuously) of the idle loop to execute, the frames per second, and the time elapsed since the application started as well as peaks, lows and averages for those values.
	/// <para>This object will automatically gather data if your application has an <see cref="Gorgon.ApplicationIdleLoopMethod">idle loop</see> assigned.  Otherwise, if a custom idle time polling method is 
	/// used, then the user should call <see cref="GorgonLibrary.Diagnostics.GorgonTiming.Reset">Reset</see> before starting the application loop, and <see cref="GorgonLibrary.Diagnostics.GorgonTiming.Update">Update</see> at the beginning of the idle loop.</para>
	/// </remarks>
	public static class GorgonTiming
	{
		#region Variables.
		private static bool _useHighResTimer = true;        // Flag to indicate that we're using a high resolution timer.
		private static GorgonTimer _timer;					// Timer used in calculations.
	    private static GorgonTimer _appTimer;               // Timer used to measure how long the application has been running.
		private static long _frameCounter;					// Frame counter.
		private static long _averageCounter;				// Counter for averages.
		private static double _lastTime;					// Last recorded time since update.
		private static double _lastTimerValue;				// Last timer value.
		private static float _averageFPSTotal;				// Average FPS total.
		private static float _averageScaledDeltaTotal;		// Average scaled delta draw time total.
		private static float _averageDeltaTotal;			// Average delta draw time total.
		private static long _maxAverageCount = 500;			// Maximum number of iterations for average reset.
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
		/// <remarks>This property starts counting at the first start of a Gorgon application and will continue to the end of the application.
		/// <para>This value is not affected by the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.TimeScale">TimeScale</see> property.</para>
		/// </remarks>
		public static float SecondsSinceStart
		{
			get
			{
                if (_appTimer == null)
                {
                    return float.NaN;
                }

                return MillisecondsSinceStart / 1000.0f;
			}
		}

		/// <summary>
		/// Property to return the number of milliseconds since a Gorgon application was started.
		/// </summary>
		/// <remarks>This property starts counting at the first start of a Gorgon application and will continue to the end of the application.
		/// <para>This value is not affected by the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.TimeScale">TimeScale</see> property.</para>
		/// </remarks>
		public static float MillisecondsSinceStart
		{
			get
			{
			    if (_appTimer == null)
			    {
			        return float.NaN;
			    }

				return (float)_appTimer.Milliseconds;
			}
		}

		/// <summary>
		/// Property to return the number of seconds to run the idle loop for a single iteration.
		/// </summary>
		/// <remarks>This is the same as the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.ScaledDelta">ScaledDelta</see> property when the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.TimeScale">TimeScale</see> is set to 1.0f, otherwise, this value is not affected by TimeScale.  
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
		public static ulong FrameCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the average FPS.
		/// </summary>
		/// <para>Note that the averages calculations are affected by the length of time it takes to execute a single iteration of the idle loop.</para>
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
		/// <para>Note that the averages calculations are affected by the length of time it takes to execute a single iteration of the idle loop.</para>
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
		/// <para>Note that the averages calculations are affected by the length of time it takes to execute a single iteration of the idle loop.</para>
		/// </remarks>
		public static float AverageScaledDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the maximum number of iterations before an average value is reset.
		/// </summary>
		/// <remarks>This only applies to the <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.AverageFPS">AverageFPS</see>, <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.AverageFPS">AverageDelta</see> and <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.AverageFPS">AverageScaledDelta</see> properties.
		/// <para>Note that the higher the value assigned to this property, the longer it'll take for the averages to compute, this is in addition to any overhead from the time it takes to execute a single iteration of the idle loop.</para>
		/// </remarks>
		public static long MaxAverageCount
		{
			get
			{
				return _maxAverageCount;
			}
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
				return _useHighResTimer;
			}

			set
			{
				_useHighResTimer = value;
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
			double theTime;				// Time value.
			double frameDelta;			// Frame delta.

			do
			{
				theTime = _timer.Milliseconds;
				frameDelta = (theTime - _lastTimerValue);

				// ReSharper disable once CompareOfFloatsByEqualityOperator
			} while ((frameDelta < 0.000001) && (frameDelta != 0.0));

			// If our delta since the last time was too high, then don't allow any movement until
			// the simulation is caught up.
			if (frameDelta > 200)
			{
				frameDelta = 0;
			}

			Delta = (float)frameDelta / 1000.0f;

			// If the delta is 0, then put in the smallest possible positive value.
			if (Delta < 1e-6f)
			{
				Delta = 1e-6f;
			}

			ScaledDelta = Delta * TimeScale;

			_lastTimerValue = theTime;

			unchecked
			{
				FrameCount++;

				frameDelta = _lastTimerValue - _lastTime;

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

					if (FPS > HighestFPS)
					{
						HighestFPS = FPS;
					}

					if (FPS < LowestFPS)
					{
						LowestFPS = FPS;
					}

					if (Delta > HighestDelta)
					{
						HighestDelta = Delta;
					}

					if (Delta < LowestDelta)
					{
						LowestDelta = Delta;
					}
				}

				if (_averageCounter > 0)
				{
					AverageFPS = _averageFPSTotal / _averageCounter;
					AverageScaledDelta = _averageScaledDeltaTotal / _averageCounter;
					AverageDelta = _averageDeltaTotal / _averageCounter;
				}
				else
				{
					AverageFPS = FPS;
					AverageScaledDelta = ScaledDelta;
					AverageDelta = Delta;
				}

				_averageFPSTotal += FPS;
				_averageScaledDeltaTotal += ScaledDelta;
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
				_averageScaledDeltaTotal = 0;
			}
		}

		/// <summary>
		/// Function to clear the timing data and reset any timers.
		/// </summary>
		/// <remarks>You do not need to call this method unless you've got your own mechanism for handling an idle time loop.
		/// <para>Values set by the user (e.g. <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.UseHighResolutionTimer">UseHighResolutionTimer</see>, <see cref="P:GorgonLibrary.Diagnostics.GorgonTiming.AverageFPS">MaxAverageCount</see>, etc...) will not be reset.  
		/// The exception to this is if a high resolution timer is required but is not supported by the hardware.</para>
		/// </remarks>
		public static void Reset()
		{
			if ((_timer == null) || (UseHighResolutionTimer != _timer.IsHighResolution))
			{
				_timer = new GorgonTimer(UseHighResolutionTimer);
			}

			_useHighResTimer = _timer.IsHighResolution;
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
			_averageScaledDeltaTotal = 0.0f;
			_timer.Reset();
		}

		/// <summary>
		/// Function to convert the desired frames per second to milliseconds.
		/// </summary>
		/// <param name="fps">Desired frames per second.</param>
		/// <returns>Frames per second in milliseconds.</returns>
		public static double FpsToMilliseconds(double fps)
		{
		    return fps > 0 ? 1000/fps : 0;
		}

	    /// <summary>
		/// Function to convert the desired frames per second to microseconds.
		/// </summary>
		/// <param name="fps">Desired frames per second.</param>
		/// <returns>Frames per second in microseconds.</returns>
		public static double FpsToMicroseconds(double fps)
	    {
	        return fps > 0 ? 1000000/fps : 0;
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
            _appTimer = new GorgonTimer();
		}
		#endregion
	}
}
