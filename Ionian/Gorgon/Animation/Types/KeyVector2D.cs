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
using System.Reflection;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A key frame for manipulating data of the type <see cref="GorgonLibrary.Vector2D"/>.
	/// </summary>
	public class KeyVector2D
		: KeyFrame
	{
		#region Variables.
		private Vector2D _value = Vector2D.Zero;			// Value of the key.
		private Spline _splineValue = null;					// Splined value.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the value of the key.
		/// </summary>
		public Vector2D Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set up the splines.
		/// </summary>
		private void SetupSplines()
		{
			// Remove previous points.
			_splineValue.Clear();

			// Add points to the spline.
			for (int i = 0; i < Owner.KeyCount; i++)
			{
				KeyVector2D key = Owner.GetKeyAtIndex(i) as KeyVector2D;
				_splineValue.AddPoint(key.Value);
			}

			// Recalculate tangents.
			_splineValue.RecalculateTangents();
		}

		/// <summary>
		/// Function to update the data within the key frame.
		/// </summary>
		/// <param name="keyData">Interpolated key data used to help calculate data between keys.</param>
		protected internal override void UpdateKeyData(Track.NearestKeys keyData)
		{
			// Cast to the appropriate types.
			KeyVector2D previous = keyData.PreviousKey as KeyVector2D;
			KeyVector2D next = keyData.NextKey as KeyVector2D;

			if (previous == null)
				throw new AnimationTypeMismatchException("key at time index", keyData.PreviousKey.Time.ToString("0.0"), "KeyVector2D", keyData.PreviousKey.GetType().Name);

			if (next == null)
				throw new AnimationTypeMismatchException("key at time index", keyData.NextKey.Time.ToString("0.0"), "KeyVector2D", keyData.NextKey.GetType().Name);

			// Copy if we're at the same frame.
			if ((Time == 0) || (previous.Owner.InterpolationMode == InterpolationMode.None))
				_value = previous.Value;
			else
			{
				// Calculate linear values.
				if (Owner.InterpolationMode == InterpolationMode.Linear)
				{
					_value = Vector2D.Add(previous.Value, Vector2D.Multiply(Vector2D.Subtract(next.Value, previous.Value), Time));
					if (Owner.RoundValues)
						_value = MathUtility.Round(_value);
				}
				else
				{
					// Calculate spline values.
					if (Owner.NeedsUpdate)
						SetupSplines();

					// Calculate transforms.
					_value = _splineValue[keyData.PreviousKeyIndex, Time];
				}
			}

			Owner.NeedsUpdate = true;
		}

		/// <summary>
		/// Function to retrieve data from the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		public override void ReadData(Serializer serializer)
		{
			string typeName = string.Empty;		// Type name of the key.

			typeName = serializer.ReadString("Type");
			if (string.Compare(typeName, "KeyVector2D", true) != 0)
				throw new AnimationTypeMismatchException("serialized key type", string.Empty, "KeyVector2D", typeName);

			Time = serializer.ReadSingle("Time");
			_value = new Vector2D(serializer.ReadSingle("X"), serializer.ReadSingle("Y"));
		}

		/// <summary>
		/// Function to persist the data into the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		public override void WriteData(Serializer serializer)
		{
			serializer.WriteGroupBegin("KeyFrame");
			serializer.Write("Type", "KeyVector2D");
			serializer.Write("Time", Time);
			serializer.Write("X", _value.X);
			serializer.Write("Y", _value.Y);
			serializer.WriteGroupEnd();
		}

		/// <summary>
		/// Function to perform an update of the bound property.
		/// </summary>
		public override void Update()
		{
			if (Owner == null)
				return;
			Owner.BoundProperty.SetValue(Owner.Owner.Owner, _value, null);
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public override KeyFrame Clone()
		{
			KeyVector2D clone = null;			// Cloned key.

			clone = new KeyVector2D(Time, _value);

			return clone;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyVector2D"/> class.
		/// </summary>
		/// <param name="time">The time (in milliseconds) at which this keyframe exists within the track.</param>
		/// <param name="value">Value for the key.</param>
		public KeyVector2D(float time, Vector2D value)
			: base(time)
		{
			_splineValue = new Spline();
			_splineValue.AutoCalculate = false;
			_value = value;
		}
		#endregion
	}
}
