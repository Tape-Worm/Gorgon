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
	/// Data used for frame rate timing operations.
	/// </summary>
	public static class GorgonTiming
	{
		#region Variables.
		private static GorgonTimer _baseTimer = null;				// Base timer to determine over all system time.
		private static float _baseTime = 0;							// Base time at which the system started.
		private static bool _useHRTimer = true;						// Flag to indicate that we're using a high resolution timer.
		private static GorgonTimer _timer = null;					// Timer used in calculations.
		private static long _frameCounter = 0;						// Frame counter.
		private static long _averageCounter = 0;					// Counter for averages.
		private static double _lastTime = 0.0;						// Last recorded time since update.
		private static double _lastTimerValue = 0.0;				// Last timer value.
		private static float _averageFPSTotal = 0.0f;				// Average FPS total.
		private static float _averageDTTotal = 0.0f;				// Average draw time total.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the number of seconds since a Gorgon application was started.
		/// </summary>
		/// <remarks>This property starts counting at the first start of a Gorgon application and will continue to end of the application.</remarks>
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
		/// <remarks>This property starts counting at the first start of a Gorgon application and will continue to end of the application.</remarks>
		public static float MillisecondsSinceStart
		{
			get
			{
				return (float)_baseTimer.Milliseconds - _baseTime;
			}
		}

		/// <summary>
		/// Property to return the frame rate delta in milliseconds.
		/// </summary>
		public static float FrameDelta
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
		/// Property to return the highest frame delta.
		/// </summary>
		public static float HighestFrameDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the lowest frame delta.
		/// </summary>
		public static float LowestFrameDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the average frame delta.
		/// </summary>
		public static float AverageFrameDelta
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
		/// Function to refresh the contents of the timing data.
		/// </summary>
		internal static void Update()
		{
			double theTime = 0.0;				// Time value.
			double delta = 0.0;					// Frame delta.

			do
			{
				theTime = _timer.Milliseconds;
				delta = (theTime - _lastTimerValue);
			} while ((delta < 0.0001) && (delta != 0.0));

			// If our delta since the last time was too high, then don't allow any movement until
			// the simulation is caught up.
			if (delta > 200)
				delta = 0;

			FrameDelta = (float)delta / 1000.0f;

			_lastTimerValue = theTime;

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

				if (FrameDelta > HighestFrameDelta)
					HighestFrameDelta = FrameDelta;
				if (FrameDelta < LowestFrameDelta)
					LowestFrameDelta = FrameDelta;
			}

			if (AverageFPS == 0.0f)
				AverageFPS = FPS;
			if (AverageFrameDelta == 0.0f)
				AverageFrameDelta = FrameDelta;

			if (_averageCounter > 60)
			{
				AverageFPS = _averageFPSTotal / _averageCounter;
				AverageFrameDelta = _averageDTTotal / _averageCounter;
				_averageDTTotal = 0.0f;
				_averageFPSTotal = 0.0f;
				_averageCounter = 0;
			}
			else
			{
				_averageFPSTotal += FPS;
				_averageDTTotal += FrameDelta;
				_averageCounter++;
			}
		}

		/// <summary>
		/// Function to clear the timing data and reset any timers.
		/// </summary>
		internal static void Reset()
		{
			if ((_timer == null) || (UseHighResolutionTimer != _timer.IsHighResolution))
				_timer = new GorgonTimer(UseHighResolutionTimer);

			if ((_baseTimer == null) || (UseHighResolutionTimer != _baseTimer.IsHighResolution))
				_baseTimer = new GorgonTimer(UseHighResolutionTimer);

			_useHRTimer = _timer.IsHighResolution;
			HighestFPS = float.MinValue;
			LowestFPS = float.MaxValue;
			AverageFPS = 0.0f;
			HighestFrameDelta = float.MinValue;
			LowestFrameDelta = float.MaxValue;
			AverageFrameDelta = 0.0f;
			FrameDelta = 0.0f;
			FPS = 0.0f;
			FrameCount = 0;
			_averageCounter = 0;
			_frameCounter = 0;
			_lastTime = 0.0;
			_lastTimerValue = 0.0;
			_averageFPSTotal = 0.0f;
			_averageDTTotal = 0.0f;
			_timer.Reset();
			_baseTimer.Reset();
			_baseTime = (float)_baseTimer.Milliseconds;
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
		}
		#endregion
	}
}
