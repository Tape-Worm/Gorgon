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
using System.Drawing;
using GorgonLibrary.Serialization;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A key frame for manipulating data of the type <see cref="System.Drawing.Color"/>
	/// </summary>
	public class KeyColor
		: KeyFrame
	{
		#region Variables.
		private Color _value = Color.Transparent;			// Value of the key.
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
				return typeof(Color);
			}
		}

		/// <summary>
		/// Property to set or return the value of the key.
		/// </summary>
		public Color Value
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
		/// Function to update the data within the key frame.
		/// </summary>
		/// <param name="keyData">Interpolated key data used to help calculate data between keys.</param>
		protected internal override void UpdateKeyData(Track.NearestKeys keyData)
		{
			float[] source = new float[4];		// Source color components.
			float[] dest = new float[4];		// Destination color components.

			// Cast to the appropriate types.
			KeyColor previous = keyData.PreviousKey as KeyColor;
			KeyColor next = keyData.NextKey as KeyColor;


			if (previous == null)
				throw new ArgumentException("The previous key is not the expected type: KeyColor", "keyData");

			if (next == null)
				throw new ArgumentException("The next key is not the expected type: KeyColor", "keyData");

			// Copy if we're at the same frame.
			if ((Time == 0) || (previous.Owner.InterpolationMode == InterpolationMode.None))
				_value = previous.Value;
			else
			{
				// Get color components.
				source[0] = previous.Value.R;
				source[1] = previous.Value.G;
				source[2] = previous.Value.B;
				source[3] = previous.Value.A;

				dest[0] = next.Value.R;
				dest[1] = next.Value.G;
				dest[2] = next.Value.B;
				dest[3] = next.Value.A;

				// Interpolate the colors.
				for (int i = 0; i < 4; i++)
				{
					dest[i] = (source[i] + ((dest[i] - source[i]) * Time));

					if (dest[i] < 0)
						dest[i] = 0;
					if (dest[i] > 255)
						dest[i] = 255;
				}

				_value = Color.FromArgb((int)dest[3], (int)dest[0], (int)dest[1], (int)dest[2]);
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
			if (string.Compare(typeName, "KeyColor", true) != 0)
				throw new GorgonException(GorgonErrors.CannotReadData, "Got an unexpected key type: " + typeName + ", expected: KeyColor");

			Time = serializer.ReadSingle("Time");
			_value = Color.FromArgb(serializer.ReadInt32("Value"));
		}

		/// <summary>
		/// Function to persist the data into the serializer stream.
		/// </summary>
		/// <param name="serializer">Serializer that's calling this function.</param>
		public override void WriteData(Serializer serializer)
		{
			serializer.WriteGroupBegin("KeyFrame");
			serializer.Write("Type", "KeyColor");
			serializer.Write("Time", Time);
			serializer.Write("Value", _value.ToArgb());
			serializer.WriteGroupEnd();
		}

		/// <summary>
		/// Function to perform an update of the bound property.
		/// </summary>
		public override void Update()
		{
			if (Owner == null)
				return;
			if (Owner.BoundProperty.PropertyType == typeof(int))
				Owner.BoundProperty.SetValue(Owner.Owner.Owner, _value.ToArgb(), null);
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
			KeyColor clone = null;			// Cloned key.

			clone = new KeyColor(Time, _value);

			return clone;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="KeyColor"/> class.
		/// </summary>
		/// <param name="time">The time (in milliseconds) at which this keyframe exists within the track.</param>
		/// <param name="value">Color value for the key.</param>
		public KeyColor(float time, Color value)
			: base(time)
		{
			_value = value;
		}
		#endregion
	}
}
