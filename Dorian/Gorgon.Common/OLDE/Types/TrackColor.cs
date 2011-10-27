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
using System.Drawing;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Animation track used to animate color properties.
	/// </summary>
	public class TrackColor
		: Track
	{
		#region Properties.
		/// <summary>
		/// Property to return the key for a given frame time index.
		/// </summary>
		/// <returns>A key containing interpolated keyframe data.</returns>
		public override KeyFrame this[float timeIndex]
		{
			get
			{
				KeyColor newKey = null;				// Key information.
				NearestKeys keyData;					// Nearest key information.

				// If we specify the exact key, then return it.
				if (Contains(timeIndex))
					return KeyList[timeIndex];

				// If we're at the last key, then don't progress any further.
				if (timeIndex > GetKeyAtIndex(KeyList.Count - 1).Time)
					return GetKeyAtIndex(KeyList.Count - 1);

				// Get the nearest key information.
				keyData = FindNearest(timeIndex);

				// Get an instance of the key.
				newKey = new KeyColor(keyData.KeyTimeDelta, Color.Transparent);
				newKey.Owner = this;				

				// Apply the transformation.
				newKey.UpdateKeyData(keyData);

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
			return new KeyColor(0.0f, Color.Transparent);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="TrackColor"/> class.
		/// </summary>
		/// <param name="property">Property that is bound to the track.</param>
		internal TrackColor(PropertyInfo property)
			: base(property, typeof(Color))
		{
		}
		#endregion
	}
}
