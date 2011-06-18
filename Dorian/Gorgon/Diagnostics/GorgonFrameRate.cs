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
	/// <remarks>This object is returned to the main application loop from the <see cref="M:GorgonLibrary.Gorgon.Go">Go</see> method.</remarks>
	public class GorgonFrameRate
	{
		#region Variables.
		private GorgonTimer _timer = null;					// Timer used in calculations.
		private long _frameCounter = 0;						// Frame counter.
		private long _averageCounter = 0;					// Counter for averages.
		private double _lastTime = 0.0;						// Last recorded time since update.
		private double _lastTimerValue = 0.0;				// Last timer value.
		private float _averageFPSTotal = 0.0f;				// Average FPS total.
		private float _averageDTTotal = 0.0f;				// Average draw time total.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the frame rate delta in milliseconds.
		/// </summary>
		public float FrameDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of frames that have been presented.
		/// </summary>
		public long FrameCount
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the average FPS.
		/// </summary>
		public float AverageFPS
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the highest FPS.
		/// </summary>
		public float HighestFPS
		{
			get;
			private set;
		}

		/// <summary>
		/// Propery to return the lowest FPS.
		/// </summary>
		public float LowestFPS
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the highest frame delta.
		/// </summary>
		public float HighestFrameDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the lowest frame delta.
		/// </summary>
		public float LowestFrameDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the average frame delta.
		/// </summary>
		public float AverageFrameDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the number of frames per second.
		/// </summary>
		public float FPS
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether to use a high resolution timer
		/// </summary>
		public static bool UseHighResolutionTimer
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to refresh the contents of the timing data.
		/// </summary>
		internal void Update()
		{
			double timerValue = 0.0;			// Value from the timer.
			double delta = 0.0;					// Frame delta.

			// If we switch the timer precision, then update it.
			if (UseHighResolutionTimer != _timer.IsHighResolution)
				Reset();
			
			timerValue = _timer.Milliseconds;
			delta = (timerValue - _lastTimerValue);

			FrameDelta = (float)delta;
			if ((!UseHighResolutionTimer) && (FrameDelta < 0.001f))
				FrameDelta = 1.0f;

			_lastTimerValue = timerValue;

			FrameCount++;
			_frameCounter++;
			delta = _lastTimerValue - _lastTime;

			// Wait until we get one second of information.
			if (delta >= 1000.0)
			{
				FPS = (float)(_frameCounter / delta) * 1000.0f;

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

				if (GorgonMathUtility.EqualFloat(AverageFPS, 0.0f))
					AverageFPS = FPS;
				if (GorgonMathUtility.EqualFloat(AverageFrameDelta, 0.0f))
					AverageFrameDelta = FrameDelta;

				if (_averageCounter > 10)
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
		}

		/// <summary>
		/// Function to clear the timing data.
		/// </summary>
		public void Reset()
		{
			if ((_timer == null) || (UseHighResolutionTimer != _timer.IsHighResolution))
				_timer = new GorgonTimer(UseHighResolutionTimer);
			UseHighResolutionTimer = _timer.IsHighResolution;
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
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFrameRate"/> class.
		/// </summary>
		internal GorgonFrameRate()
		{
			Reset();
		}
		#endregion
	}
}
