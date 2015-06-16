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
using Gorgon.Core.Properties;

namespace Gorgon.Timing
{	
	/// <summary>
	/// Timing data for code within a Gorgon Idle loop.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This object is used to calculate the time it takes for a single iteration of the idle loop to execute. It will gather statistics such as the frames per second, the time elapsed since the application started 
	/// and peaks, lows and averages for those values.
	/// </para>
	/// <para>
	/// This object will automatically gather data if your application has an <see cref="GorgonApplication.ApplicationIdleLoopMethod"/> assigned.  Otherwise, if a custom idle time polling method is 
	/// used, then the user should call assign a timer to the <see cref="Timer"/> property, then call <see cref="Reset"/> before starting the application loop. Once the loop is running, the loop should 
	/// <see cref="Update"/> at the beginning of the idle loop.
	/// </para>
	/// </remarks>
	/// <example>
	/// The application loop usage with the <see cref="GorgonApplication.ApplicationIdleLoopMethod"/> assigned by one of the <see cref="O:Gorgon.Core.GorgonApplication.Run"/> methods:
	/// <code>
	///	public static bool MyLoop()
	/// {
	///     Console.CursorLeft = 0;
	///     Console.CursorTop = 0;
	///		Console.WriteLine("FPS: {0}", GorgonTiming.FPS);
	/// 
	///		return true;
	/// }
	/// 
	/// public static Main()
	/// {
	///		GorgonApplication.Run(MyLoop);
	/// }
	/// </code>
	/// A custom application loop using the <see cref="GorgonTiming"/> class:
	/// <code>
	/// // This assumes the Win32 API call to PeekMessage is imported.
	/// public void DoLoop()
	/// {
	///		MSG message;  // Win32 Message structure.
	/// 
	///		// Before loop execution.
	///		GorgonTiming.Timer = new GorgonTimerQpc();  // Or GorgonTimerMultimedia()
	/// 
	///		GorgonTiming.Reset();
	/// 
	///		while (!API.PeekMessage(out message, IntPtr.Zero, 0, 0, PeekMessage.NoRemove))
	///     {
	///			GorgonTiming.Update();
	/// 
	///			// Do your processing.
	///		}
	/// }
	/// </code>
	/// </example>
	public static class GorgonTiming
	{
		#region Variables.
		// Timer used in calculations.
		private static IGorgonTimer _timer;
		// Timer used to measure how long the application has been running.
		private static IGorgonTimer _appTimer;
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
		private static float _averageScaledDeltaTotal;
		// Average delta draw time total.
		private static float _averageDeltaTotal;
		// Maximum number of iterations for average reset.
		private static long _maxAverageCount = 500;			
		#endregion

		#region Properties.
		/// <summary>
		/// Property to scale the frame delta times.
		/// </summary>
		/// <remarks>Setting this value to 0 will pause, and a negative value will move things in reverse when using <see cref="ScaledDelta"/>.</remarks>
		public static float TimeScale
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the number of seconds since a Gorgon application was started.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This property starts counting at the first call to one of the <see cref="O:Gorgon.Core.GorgonApplication.Run"/> methods. If this property is called prior to that, then 
		/// it will return 0.
		/// </para>
		/// <para>This value is not affected by the <see cref="TimeScale"/> property.</para>
		/// </remarks>
		public static float SecondsSinceStart
		{
			get
			{
                if (_appTimer == null)
                {
                    return 0;
                }

                return MillisecondsSinceStart / 1000.0f;
			}
		}

		/// <summary>
		/// Property to return the number of milliseconds since a Gorgon application was started.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This property starts counting at the first call to one of the <see cref="O:Gorgon.Core.GorgonApplication.Run"/> methods. If this property is called prior to that, then 
		/// it will return 0.
		/// </para>
		/// <para>This value is not affected by the <see cref="TimeScale"/> property.</para>
		/// </remarks>
		public static float MillisecondsSinceStart
		{
			get
			{
			    if (_appTimer == null)
			    {
			        return 0;
			    }

				return (float)_appTimer.Milliseconds;
			}
		}

		/// <summary>
		/// Property to return the number of seconds to run the idle loop for a single iteration.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This is a value to indicate how long it took an iteration of the idle loop to execute. Including the time it took for a video device to draw data. This is the preferred value 
		/// to read when checking for performance.
		/// </para>
		/// <para>
		/// The article at <a href="https://www.mvps.org/directx/articles/fps_versus_frame_time.htm" target="_blank">https://www.mvps.org/directx/articles/fps_versus_frame_time.htm</a> has more 
		/// information on using frame delta instead of frames per second. 
		/// </para>
		/// <para>
		/// This is the same as the <see cref="ScaledDelta"/> property when the <see cref="TimeScale">TimeScale</see> is set to 1.0f, otherwise, this value is not affected by <see cref="TimeScale"/>.  
		/// </para>
		/// <para>
		/// Because it is unaffected by TimeScale, this value is the one that should be used when measuring performance.
		/// </para>
		/// </remarks>
		public static float Delta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of seconds to run the idle loop for a single iteration with scaling.
		/// </summary>
		/// <remarks>This value is affected by the <see cref="TimeScale"/> property.</remarks>
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
		/// <remarks>
		/// Note that the averaged/min/max calculations are affected by the length of time it takes to execute a single iteration of the idle loop and will not have meaningful data until the 
		/// application loop begins processing after a call to one of the <see cref="O:Gorgon.Core.GorgonApplication.Run"/> methods.
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
		/// application loop begins processing after a call to one of the <see cref="O:Gorgon.Core.GorgonApplication.Run"/> methods.
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
		/// application loop begins processing after a call to one of the <see cref="O:Gorgon.Core.GorgonApplication.Run"/> methods.
		/// </remarks>
		public static float LowestFPS
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the highest idle loop delta.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Note that the averaged/min/max calculations are affected by the length of time it takes to execute a single iteration of the idle loop and will not have meaningful data until the 
		/// application loop begins processing after a call to one of the <see cref="O:Gorgon.Core.GorgonApplication.Run"/> methods.
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
		/// application loop begins processing after a call to one of the <see cref="O:Gorgon.Core.GorgonApplication.Run"/> methods.
		/// </para>
		/// <para>
		/// This value is not affected by the <see cref="TimeScale"/> property because it is meant to be used in performance measurement.
		/// </para>
		/// </remarks>
		public static float LowestDelta
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
		/// application loop begins processing after a call to one of the <see cref="O:Gorgon.Core.GorgonApplication.Run"/> methods.
		/// </para>
		/// <para>
		/// This value is not affected by the <see cref="TimeScale"/> property because it is meant to be used in performance measurement.
		/// </para>
		/// </remarks>
		public static float AverageDelta
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
		/// application loop begins processing after a call to one of the <see cref="O:Gorgon.Core.GorgonApplication.Run"/> methods.
		/// </para>
		/// <para>
		/// This value is affected by the <see cref="TimeScale"/> property.
		/// </para>
		/// </remarks>
		public static float AverageScaledDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the maximum number of iterations before an average value is reset.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This only applies to the <see cref="AverageFPS"/>, <see cref="AverageDelta"/> and <see cref="AverageScaledDelta"/> properties.
		/// </para>
		/// <para>
		/// Note that the higher the value assigned to this property, the longer it'll take for the averages to compute, this is in addition to any overhead from the time it takes to execute a single 
		/// iteration of the idle loop.
		/// </para>
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
		/// See the article at <a href="https://www.mvps.org/directx/articles/fps_versus_frame_time.htm" target="_blank">https://www.mvps.org/directx/articles/fps_versus_frame_time.htm</a> for more 
		/// information on using frame delta instead of frames per second. 
		/// </para>
		/// </remarks>
		public static float FPS
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the timer to use when calculating timings.
		/// </summary>
		/// <remarks>
		/// <para>
		/// This value must be assigned before calling the <see cref="Update"/> or <see cref="Reset"/> methods.
		/// </para>
		/// <para>
		/// This value cannot be set to <b>null</b> (<i>Nothing</i> in VB.Net).
		/// </para>
		/// </remarks>
		public static IGorgonTimer Timer
		{
			get
			{
				return _timer;
			}
			set
			{
				if (value == null)
				{
					return;
				}

				_timer = value;
				Reset();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to gather timing data.
		/// </summary>
		/// <remarks>
		/// <para>
		/// There is no need to call this method manually from your application if using the <see cref="GorgonApplication"/> class to run your application. The only time this method 
		/// should be called is when you have a custom application loop that needs timing.
		/// </para>
		/// <para>
		/// Ensure that the <see cref="Timer"/> property is assigned with an appropriate <see cref="IGorgonTimer"/> before calling this method, otherwise and exception will be thrown 
		/// when in DEBUG mode. If in release mode, and no timer is assigned, then the application will likely crash (this is done for performance reasons).
		/// </para>
		/// </remarks>
		/// <exception cref="GorgonException"><b>[DEBUG Only]</b> Thrown when a call is made to this method before assigning the <see cref="Timer"/> property.</exception>
		public static void Update()
		{
			double theTime;				// Time value.
			double frameDelta;			// Frame delta.

#if DEBUG
			if (_timer == null)
			{
				throw new GorgonException(GorgonResult.NotInitialized, Resources.GOR_ERR_TIMING_NO_TIMER);
			}
#endif

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
		/// <remarks>
		/// <para>
		/// Like <see cref="Update"/>, you do not need to call this method unless you have your own mechanism for handling an idle time loop.
		/// </para>
		/// <para>
		/// Values set by the user (e.g. <see cref="MaxAverageCount"/>, etc...) will not be reset.
		/// </para>
		/// <para>
		/// Ensure that the <see cref="Timer"/> property is assigned with an appropriate <see cref="IGorgonTimer"/> before calling this method, otherwise 
		/// and exception will be thrown.
		/// </para>
		/// </remarks>
		/// <exception cref="GorgonException">Thrown when a call is made to this method before assigning the <see cref="Timer"/> property.</exception>
		public static void Reset()
		{
			if (_timer == null)
			{
				throw new GorgonException(GorgonResult.NotInitialized, Resources.GOR_ERR_TIMING_NO_TIMER);
			}

			if (_appTimer == null)
			{
				if (_timer.IsHighResolution)
				{
					_appTimer = new GorgonTimerQpc();
				}
				else
				{
					_appTimer = new GorgonTimerMultimedia();
				}
			}

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
			TimeScale = 1;
		}
		#endregion
	}
}
