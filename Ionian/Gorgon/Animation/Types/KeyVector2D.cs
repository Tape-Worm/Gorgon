#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Wednesday, April 30, 2008 1:52:49 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A key frame 
	/// </summary>
	public class KeyVector2D
		: Key
	{
		#region Variables.
		private Vector2D _value = Vector2D.Zero;			// Value of the key.
		private Spline _splineValue = null;					// Splined value.
		#endregion

		#region Properties.
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
			// Cast to the appropriate types.
			KeyVector2D previous = previousKey as KeyVector2D;
			KeyVector2D next = nextKey as KeyVector2D;

			if (previousKey == null)
				throw new ArgumentNullException("previousKey");

			if (nextKey == null)
				throw new ArgumentNullException("nextKey");

			if (previous == null)
				throw new AnimationTypeMismatchException("key at time index", previousKey.Time.ToString("0.0"), "KeyVector2D", previousKey.GetType().Name);

			if (next == null)
				throw new AnimationTypeMismatchException("key at time index", nextKey.Time.ToString("0.0"), "KeyVector2D", nextKey.GetType().Name);

			// Copy if we're at the same frame.
			if ((Time == 0) || (previous.InterpolationMode == InterpolationMode.None))
				_value = previous._value;
			else
			{
				// Calculate linear values.
				if (previous.InterpolationMode == InterpolationMode.Linear)
					_value = MathUtility.Round(Vector2D.Add(previous._value, Vector2D.Multiply(Vector2D.Subtract(next._value, previous._value), Time)));
				else
				{
					// Calculate spline values.
					if (Owner.NeedsUpdate)
						SetupSplines();

					// Calculate transforms.
					_value = _splineValue[prevKeyIndex, Time];
				}
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Function to perform an update of the bound property.
		/// </summary>
		public override void Update()
		{
			Owner.BoundProperty.SetValue(Owner.Owner.Owner, _value, null);
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public override Key Clone()
		{
			KeyVector2D clone = null;			// Cloned key.

			clone = new KeyVector2D(null, Time);
			clone.InterpolationMode = InterpolationMode;
			clone._value = _value;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyVector2D"/> class.
		/// </summary>
		/// <param name="owner">The owner of this key frame.</param>
		/// <param name="time">The time (in milliseconds) at which this keyframe exists within the track.</param>
		internal KeyVector2D(Track owner, float time)
			: base(owner, time)
		{
		}
		#endregion
	}
}
