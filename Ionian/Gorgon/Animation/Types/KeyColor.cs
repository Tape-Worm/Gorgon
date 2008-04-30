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
using System.Drawing;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a color key.
	/// </summary>
	public class KeyColor
		: Key
	{
		#region Variables.
        private Color _color = Color.White;                 // Color
        private int _maskValue;								// Alpha mask value.
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
				if (Owner != null)
					Owner.Update();
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
		/// Function to apply key information to the owning object of the animation.
		/// </summary>
		/// <param name="prevKeyIndex">Previous key index.</param>
		/// <param name="previousKey">Key prior to this one.</param>
		/// <param name="nextKey">Key after this one.</param>
		protected internal override void Apply(int prevKeyIndex, Key previousKey, Key nextKey)
		{
			KeyColor previous = previousKey as KeyColor;
			KeyColor next = nextKey as KeyColor;

			if (previousKey == null)
				throw new ArgumentNullException("previousKey");

			if (nextKey == null)
				throw new ArgumentNullException("nextKey");

			if (previous == null)
				throw new AnimationTypeMismatchException("key at time index", previousKey.Time.ToString("0.0"), "KeyColor", previousKey.GetType().Name);

			if (next == null)
				throw new AnimationTypeMismatchException("key at time index", nextKey.Time.ToString("0.0"), "KeyColor", nextKey.GetType().Name);

			// Copy if we're at the same frame.
			if ((Time == 0) || (previous.InterpolationMode == InterpolationMode.None))
			{
				_color = previous.Color;
				_maskValue = previous.AlphaMaskValue;
			}
			else
			{
				float[] sourceComponents = new float[4];        // Source color components.
				float[] destComponents = new float[4];          // Destination color components.
				int mask = previous._maskValue;              // Alpha mask value.


				// Get components.
				sourceComponents[0] = Convert.ToSingle(previous.Color.R);
				sourceComponents[1] = Convert.ToSingle(previous.Color.G);
				sourceComponents[2] = Convert.ToSingle(previous.Color.B);
				sourceComponents[3] = Convert.ToSingle(previous.Color.A);
				destComponents[0] = Convert.ToSingle(next.Color.R);
				destComponents[1] = Convert.ToSingle(next.Color.G);
				destComponents[2] = Convert.ToSingle(next.Color.B);
				destComponents[3] = Convert.ToSingle(next.Color.A);

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
				_maskValue = Convert.ToInt32(Math.Round(mask + ((next._maskValue - mask) * Time)));
			}

			if (Owner != null)
				Owner.Update();
		}

		/// <summary>
		/// Function to use the key data to update a layer object.
		/// </summary>
		/// <param name="layerObject">Layer object to update.</param>
		protected internal override void UpdateLayerObject(Renderable layerObject)
		{
			layerObject.Color = _color;
			if (layerObject.AlphaMaskValue != _maskValue)
				layerObject.AlphaMaskValue = _maskValue;
		}

		/// <summary>
		/// Function to clone this key.
		/// </summary>
		/// <returns>A clone of this key.</returns>
		public override object Clone()
		{
			KeyColor newKey = null;         // Cloned key.

			newKey = new KeyColor(null, Time);
			newKey.InterpolationMode = InterpolationMode;
			newKey.Color = _color;
			newKey.AlphaMaskValue = _maskValue;

			return newKey;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Owner of this key.</param>
		/// <param name="timePosition">Position in time for this keyframe.</param>
		public KeyColor(TrackColor owner, float timePosition)
			: base(owner, timePosition)
		{
			// Get the size from the animation owner.
			if (Owner != null)
			{
				_color = Owner.Owner.Owner.Color;
				_maskValue = Owner.Owner.Owner.AlphaMaskValue;
			}
		}
		#endregion

		#region IKey Members
		#region Methods.
		#endregion
		#endregion
	}
}
