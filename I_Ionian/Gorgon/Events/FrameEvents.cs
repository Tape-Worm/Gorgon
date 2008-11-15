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
// Created: Monday, July 11, 2005 11:52:31 PM
// 
#endregion

using System;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Event delegate for frame events.
	/// </summary>
	/// <param name="sender">Sender of this event.</param>
	/// <param name="e">Event parameters.</param>
	public delegate void FrameEventHandler(object sender, FrameEventArgs e);

	/// <summary>
	/// Arguments for the frame rendering events.
	/// </summary>
	public class FrameEventArgs 
		: EventArgs
	{
		#region Variables.
		private TimingData _timingData = null;				// Timing data to grab information from.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the timing data.
		/// </summary>
		public TimingData TimingData
		{
			get
			{
				return _timingData;
			}
		}

		/// <summary>
		/// Property to return the frame delta time.
		/// </summary>
		public float FrameDeltaTime
		{
			get
			{
				return (float)(_timingData.FrameDrawTime / 1000.0);
			}
		}

		/// <summary>
		/// Property to return the current number of frames rendered.
		/// </summary>
		public long FrameCount
		{
			get
			{
				return _timingData.FrameCount;
			}
		}

		/// <summary>
		/// Property to return the current frames/second.
		/// </summary>
		public float FPS
		{
			get
			{
				return _timingData.CurrentFps;
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="timingData">Timing data to withdraw data from.</param>
		public FrameEventArgs(TimingData timingData)
		{
			_timingData = timingData;
		}
		#endregion
	}
}
