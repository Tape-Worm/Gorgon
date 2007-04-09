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
// Created: Tuesday, November 21, 2006 12:08:22 AM
// 
#endregion

using System;
using SharpUtilities.Mathematics;
using System.Drawing;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics.Animations
{
	/// <summary>
	/// Object representing a color key.
	/// </summary>
	public class KeyColor
		: IKey<KeyColor>
	{
		#region Variables.
        private Color _color = Color.White;                 // Color
		private TrackColor _owner;						    // Owning track.
		private InterpolationMode _interpolation;			// Interpolation mode.
		private float _frameTime = 0;						// Time index for the keyframe.
        private int _maskValue = 0;                         // Alpha mask value.
		#endregion

		#region Properties.
        /// <summary>
        /// Property to set or return the color.
        /// </summary>
        public Color Color
        {
            get
            {
                return _color;
            }
            set
            {
                _color = value;
                if (_owner != null)
                    _owner.Updated = true;
            }
        }

        /// <summary>
        /// Property to set or return the alpha mask value.
        /// </summary>
        public int AlphaMaskValue
        {
            get
            {
                return _maskValue;
            }
            set
            {
                _maskValue = value;
                if (_maskValue < 0)
                    _maskValue = 0;
                if (_maskValue > 255)
                    _maskValue = 255;
            }
        }
		#endregion

		#region Methods.
		/// <summary>
		/// Function to assign this key to a track.
		/// </summary>
		/// <param name="track">Track to assign to.</param>
		internal void AssignKey(TrackColor track)
		{
			_owner = track;
			_owner.Updated = true;
		}		
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">_owner of this key.</param>
		/// <param name="timePosition">Position in time for this keyframe.</param>
		public KeyColor(TrackColor owner, float timePosition)
		{
			_owner = owner;
			_interpolation = InterpolationMode.Linear;
			_frameTime = timePosition;

			// Get the size from the animation owner.
			if (_owner != null)
			{
				_color = _owner.Owner.Owner.Color;
				_maskValue = _owner.Owner.Owner.AlphaMaskValue;
			}
		}
		#endregion

		#region IKey<KeyColor> Members
		#region Properties.
		/// <summary>
		/// Property to set or return the interpolation mode.
		/// </summary>
		/// <value></value>
		public InterpolationMode InterpolationMode
		{
			get
			{
				return _interpolation;
			}
			set
			{
				_interpolation = value;
				if (_owner != null)
					_owner.Updated = true;
			}
		}

		/// <summary>
		/// Property to set or return the time index (in milliseconds) for this keyframe.
		/// </summary>
		/// <value></value>
		public float Time
		{
			get
			{
				return _frameTime;
			}
			set
			{
				_frameTime = value;
				if (_owner != null)
					_owner.Updated = true;
			}
		}

		/// <summary>
		/// Property to return the owning track for this key.
		/// </summary>
		/// <value></value>
		public Track<KeyColor> Owner
		{
			get
			{
				return _owner;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply key information to the owning object of the animation.
		/// </summary>
		/// <param name="prevKeyIndex">Previous key index.</param>
		/// <param name="previousKey">Key prior to this one.</param>
		/// <param name="nextKey">Key after this one.</param>
		void IKey<KeyColor>.Apply(int prevKeyIndex, KeyColor previousKey, KeyColor nextKey)
		{
			// Copy if we're at the same frame.
			if ((Time == 0) || (previousKey.InterpolationMode == InterpolationMode.None))
			{
				_color = previousKey.Color;
				_maskValue = previousKey.AlphaMaskValue;
			}
			else
			{
				float[] sourceComponents = new float[4];        // Source color components.
				float[] destComponents = new float[4];          // Destination color components.
				int mask = previousKey._maskValue;              // Alpha mask value.


				// Get components.
				sourceComponents[0] = Convert.ToSingle(previousKey.Color.R);
				sourceComponents[1] = Convert.ToSingle(previousKey.Color.G);
				sourceComponents[2] = Convert.ToSingle(previousKey.Color.B);
				sourceComponents[3] = Convert.ToSingle(previousKey.Color.A);
				destComponents[0] = Convert.ToSingle(nextKey.Color.R);
				destComponents[1] = Convert.ToSingle(nextKey.Color.G);
				destComponents[2] = Convert.ToSingle(nextKey.Color.B);
				destComponents[3] = Convert.ToSingle(nextKey.Color.A);

				// Calculate interpolated colors.
				for (int i = 0; i < 4; i++)
				{
					destComponents[i] = (sourceComponents[i] + ((destComponents[i] - sourceComponents[i]) * Time));
					destComponents[i] = (float)Math.Round(destComponents[i]);

					// Clamp values.
					if (destComponents[i] > 255)
						destComponents[i] = 255.0f;

					if (destComponents[i] < 0)
						destComponents[i] = 0;
				}

				// Create color.
				_color = Color.FromArgb(Convert.ToInt32(destComponents[3]), Convert.ToInt32(destComponents[0]), Convert.ToInt32(destComponents[1]), Convert.ToInt32(destComponents[2]));

				// Interpolate mask value.
				_maskValue = Convert.ToInt32(Math.Round(mask + ((nextKey._maskValue - mask) * Time)));
			}

			if (_owner != null)
				_owner.Updated = true;
		}

		/// <summary>
		/// Function to use the key data to update a layer object.
		/// </summary>
		/// <param name="layerObject">Layer object to update.</param>
		void IKey<KeyColor>.UpdateLayerObject(IAnimatable layerObject)
		{
            layerObject.Color = _color;
            if (layerObject.AlphaMaskValue != _maskValue)
                layerObject.AlphaMaskValue = _maskValue;
		}

		/// <summary>
		/// Function to copy this key into a new time.
		/// </summary>
		/// <param name="newTime">Time index to place the copy into.</param>
		/// <returns>The copy of the this key.</returns>
		public KeyColor CopyTo(float newTime)
		{
			KeyColor copy = Clone();		// Create a copy.

			// Assign the time and owner.
			copy.Time = newTime;
			Owner.AddKey(copy);

			return copy;
		}

		/// <summary>
		/// Function to clone this key.
		/// </summary>
		/// <returns>A clone of this key.</returns>
		public KeyColor Clone()
		{
			KeyColor newKey = null;         // Cloned key.

			newKey = new KeyColor(null, _frameTime);
			newKey.InterpolationMode = _interpolation;
            newKey.Color = _color;
			newKey.AlphaMaskValue = _maskValue;

			return newKey;
		}
		#endregion
		#endregion
	}
}
