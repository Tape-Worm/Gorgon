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
