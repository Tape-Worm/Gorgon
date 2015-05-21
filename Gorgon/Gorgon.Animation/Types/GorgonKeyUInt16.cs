#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Wednesday, October 3, 2012 8:58:29 PM
// 
#endregion

using System;
using Gorgon.IO;

namespace Gorgon.Animation
{
	/// <summary>
	/// A key frame that manipulates an unsigned 16 bit integer data type.
	/// </summary>
	public struct GorgonKeyUInt16
		: IKeyFrame
	{
		#region Variables.
		private readonly Type _dataType;								// Type of data for the key frame.

		/// <summary>
		/// Value to store in the key frame.
		/// </summary>
		public UInt16 Value;
		/// <summary>
		/// Time for the key frame in the animation.
		/// </summary>
		public float Time;
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonKeyUInt16" /> struct.
		/// </summary>
		/// <param name="time">The time for the key frame.</param>
		/// <param name="value">The value to apply to the key frame.</param>
		public GorgonKeyUInt16(float time, UInt16 value)
		{
			Time = time;
			_dataType = typeof(UInt16);
			Value = value;
		}
		#endregion

		#region IKeyFrame Members
		/// <summary>
		/// Property to set or return the time at which the key frame is stored.
		/// </summary>
		float IKeyFrame.Time
		{
			get
			{
				return Time;
			}
		}

		/// <summary>
		/// Property to return the type of data for this key frame.
		/// </summary>
		public Type DataType
		{
			get
			{
				return _dataType;
			}
		}

		/// <summary>
		/// Function to clone the key.
		/// </summary>
		/// <returns>The cloned key.</returns>
		public IKeyFrame Clone()
		{
			return new GorgonKeyUInt16(Time, Value);
		}

		/// <summary>
		/// Function to retrieve key frame data from data chunk.
		/// </summary>
		/// <param name="chunk">Chunk to read.</param>
		void IKeyFrame.FromChunk(GorgonChunkReader chunk)
		{
			Time = chunk.ReadFloat();
			Value = chunk.ReadUInt16();
		}

		/// <summary>
		/// Function to send the key frame data to the data chunk.
		/// </summary>
		/// <param name="chunk">Chunk to write.</param>
		void IKeyFrame.ToChunk(GorgonChunkWriter chunk)
		{
			chunk.WriteFloat(Time);
			chunk.WriteUInt16(Value);
		}
		#endregion
	}
}
