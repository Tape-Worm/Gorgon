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
	/// A key frame for manipulating data of the type <see cref="System.Int32"/>
	/// </summary>
	public class KeyInt32
		: KeyFrame
	{
		#region Variables.
		private int _value = 0;								// Value of the key.
		private Spline _splineValue = null;					// Splined value.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of data stored in the key.
		/// </summary>
		/// <value></value>
		public override Type DataType
		{
			get
			{
				return typeof(Int32);
			}
		}

		/// <summary>
		/// Property to set or return the value of the key.
		/// </summary>
		public int Value
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
				KeyInt32 key = Owner.GetKeyAtIndex(i) as KeyInt32;
				_splineValue.AddPoint(new Vector2D(key.Value, 0.0f));
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
			KeyInt32 previous = keyData.PreviousKey as KeyInt32;
			KeyInt32 next = keyData.NextKey as KeyInt32;

			if (previous == null)
				throw new ArgumentException("The previous key is not the expected type: KeyInt32", "keyData");

			if (next == null)
				throw new ArgumentException("The next key is not the expected type: KeyInt32", "keyData");

			// Copy if we're at the same frame.
			if ((Time == 0) || (previous.Owner.InterpolationMode == InterpolationMode.None))
				_value = previous.Value;
			else
			{
				// Calculate linear values.
				if (Owner.InterpolationMode == InterpolationMode.Linear)
					_value = previous.Value + (int)(Time * (float)(next.Value - previous.Value));
				else
				{
					// Calculate spline values.
					if (Owner.NeedsUpdate)
						SetupSplines();

					// Calculate transforms.
					_value = (int)MathUtility.Round(_splineValue[keyData.PreviousKeyIndex, Time].X);
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
			if (string.Compare(typeName, "KeyInt32", true) != 0)
				throw new GorgonException(GorgonErrors.CannotReadData, "Got an unexpected key type: " + typeName + ", expected: KeyInt32");

			Time = serializer.ReadSingle("Time");
			_value = serializer.ReadInt32("Value");
		}

		/// <summary>
		/// Function to persist the data into the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		public override void WriteData(Serializer serializer)
		{
			serializer.WriteGroupBegin("KeyFrame");
			serializer.Write("Type", "KeyInt32");
			serializer.Write("Time", Time);
			serializer.Write("Value", _value);
			serializer.WriteGroupEnd();
		}

		/// <summary>
		/// Function to perform an update of the bound property.
		/// </summary>
		public override void Update()
		{
			if (Owner == null)
				return;
			if (Owner.BoundProperty.PropertyType != typeof(Int32))
				Owner.BoundProperty.SetValue(Owner.Owner.Owner, Convert.ChangeType(_value, Owner.BoundProperty.PropertyType), null);
			else
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
			KeyInt32 clone = null;			// Cloned key.

			clone = new KeyInt32(Time, _value);

			return clone;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyInt32"/> class.
		/// </summary>
		/// <param name="time">The time (in milliseconds) at which this keyframe exists within the track.</param>
		/// <param name="value">Value for the key.</param>
		public KeyInt32(float time, int value)
			: base(time)
		{
			_splineValue = new Spline();
			_splineValue.AutoCalculate = false;
			_value = value;
		}
		#endregion
	}
}
