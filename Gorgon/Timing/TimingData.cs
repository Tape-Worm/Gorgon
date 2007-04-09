#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Saturday, July 22, 2006 12:33:28 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities.Native.Win32;

namespace GorgonLibrary.Timing
{
	/// <summary>
	/// Object for timing data like FPS, # of frames, etc...
	/// </summary>
	public class TimingData
	{
		#region Constants.
		private const int AverageFPSFrameMax = 5;		// Maximum number of frames to skip before average is calculated.
		#endregion

		#region Variables.
		private double _lastFrameTime;		// Last frame time.
		private double _currentFrameTime;	// Current frame time.
		private double _frameDrawTime;		// Time to draw a frame in milliseconds.
		private double _lastFPSFrameTime;	// Last FPS.
		private double _averageFPS;			// Average FPS.
		private double _highestFPS;			// Highest FPS.
		private double _lowestFPS;			// Lowest FPS.
		private double _currentFPS;			// Current FPS.
		private long _frameCount;			// Frame count.
		private PreciseTimer _timer;		// FPS timer.
		private int _frameAvgCounter;		// Counter for frame average.
		private double _frameAvgSum;		// Frame average sum.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the amount of time to draw a frame in milliseconds.
		/// </summary>
		public double FrameDrawTime
		{
			get
			{
				return _frameDrawTime;
			}
		}

		/// <summary>
		/// Property to return the average Frames Per Second.
		/// </summary>
		public double AverageFPS
		{
			get
			{
				return _averageFPS;
			}
		}

		/// <summary>
		/// Property to return the highest Frames Per Second.
		/// </summary>
		public double HighestFPS
		{
			get
			{
				return _highestFPS;
			}
		}

		/// <summary>
		/// Property to return the lowest Frames Per Second.
		/// </summary>
		public double LowestFPS
		{
			get
			{
				return _lowestFPS;
			}
		}

		/// <summary>
		/// Property to return the current Frames Per Second.
		/// </summary>
		public double CurrentFPS
		{
			get
			{
				return _currentFPS;
			}
		}

		/// <summary>
		/// Property to return the current number of frames drawn.
		/// </summary>
		public long FrameCount
		{
			get
			{
				return _frameCount;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to calculate the frames per second.
		/// </summary>
		private void GetFPS()
		{
			_frameCount++;

			if ((_currentFrameTime - _lastFPSFrameTime) >= 1000)
			{
				_currentFPS = ((double)_frameCount / (_currentFrameTime - _lastFPSFrameTime)) * 1000.0;

				if (_frameAvgCounter < AverageFPSFrameMax)
				{
					_averageFPS = _currentFPS;
					_frameAvgSum += _currentFPS;
					_frameAvgCounter++;
				}
				else
				{
					_averageFPS = _frameAvgSum / (double)AverageFPSFrameMax;

					if (_currentFPS > _highestFPS)
						_highestFPS = _currentFPS;

					if ((_currentFPS < _lowestFPS) || (_lowestFPS == 0))
						_lowestFPS = _currentFPS;

					_frameAvgCounter = 0;
					_frameAvgSum = 0.0;
				}

				_lastFPSFrameTime = _currentFrameTime;
				_frameCount = 0;
			}
		}

		/// <summary>
		/// Function to reset the timing data.
		/// </summary>
		public void Reset()
		{
			_timer.Reset();
			_lastFPSFrameTime = 0.0;
			_averageFPS = 0.0; 
			_highestFPS = 0.0;
			_lowestFPS = 0.0;
			_currentFPS = 0.0;
			_frameCount = 0;
			_lastFrameTime = 0.0;
			_currentFrameTime = 0.0;
			_frameDrawTime = 0.0;
			_frameAvgCounter = 0;
			_frameAvgSum = 0.0;
		}

		/// <summary>
		/// Function to end timing routine.
		/// </summary>
		public void Refresh()
		{
			_lastFrameTime = _currentFrameTime;
			_currentFrameTime = _timer.Milliseconds;
			_frameDrawTime = _currentFrameTime - _lastFrameTime;
			GetFPS();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="timer">Timer to use for data.</param>
		internal TimingData(PreciseTimer timer)
		{
			_timer = timer;
			if (!_timer.HighResolutionTimer)
				Win32API.timeBeginPeriod(1);
			Reset();			
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~TimingData()
		{
			Win32API.timeEndPeriod(1);
		}
		#endregion
	}
}
