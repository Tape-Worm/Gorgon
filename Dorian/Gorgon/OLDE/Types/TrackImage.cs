#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Wednesday, April 30, 2008 10:50:41 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Animation track used to animate byte properties.
	/// </summary>
	public class TrackImage
		: Track
	{
		#region Properties.
		/// <summary>
		/// Property to set or return the interpolation mode for the track.
		/// </summary>
		/// <remarks>Image tracks don't use interpolation, and thus only use InterpolationMode.None.</remarks>
		public override InterpolationMode InterpolationMode
		{
			get
			{
				return InterpolationMode.None;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to return the key for a given frame time index.
		/// </summary>
		/// <returns>A key containing interpolated keyframe data.</returns>
		public override KeyFrame this[float timeIndex]
		{
			get
			{
				KeyFrame newKey = null;				// Key information.
				KeyFrame currentKey = null;			// Current key.

				// If we specify the exact key, then return it.
				if (Contains(timeIndex))
					return KeyList[timeIndex];

				// Get first frame.
				newKey = GetKeyAtIndex(0);

				// Find the key that matches the time index.
				for (int i = 0; i < KeyList.Count; i++)
				{
					currentKey = GetKeyAtIndex(i);

					if (currentKey.Time <= timeIndex)
					{
						newKey = GetKeyAtIndex(i);
						// If we have a key that matches the time index, then return it.
						if (currentKey.Time == timeIndex)
							return newKey;
					}
				}

				return newKey;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a keyframe.
		/// </summary>
		/// <returns>The new keyframe in the correct context.</returns>
		protected internal override KeyFrame CreateKey()
		{
			return new KeyImage(0, (Image)null);
		}
		#endregion
		
		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="TrackImage"/> class.
		/// </summary>
		/// <param name="property">Property that is bound to the track.</param>
		internal TrackImage(PropertyInfo property)
			: base(property, typeof(Image))
		{
		}
		#endregion
	}
}
