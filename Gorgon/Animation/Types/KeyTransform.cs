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
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a transformation key.
	/// </summary>
	public class KeyTransform
		: Key
	{
		#region Variables.
		private Spline _splineAxis;							// Axis spline.
		private Spline _splinePosition;						// Position spline.
		private Spline _splineScale;						// Scale spline.
		private Spline _splineRotation;						// Rotation spline.
        private Spline _splineSize;                         // Size spline.
        private Spline _splineImageOffset;                  // Size spline.
		private float _rotate;								// Rotation (in degrees).
        private Vector2D _size = Vector2D.Unit;             // Size of the object.
        private Vector2D _offset = Vector2D.Unit;           // Image offset of the object.
		private Vector2D _position = Vector2D.Zero;			// Position of the object.
		private Vector2D _scale = Vector2D.Unit;			// Scale of the object.
		private Vector2D _axis = Vector2D.Zero;				// Object axis.
		#endregion

		#region Properties.
        /// <summary>
        /// Property to set or return the position.
        /// </summary>
        public Vector2D Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = value;
				if (Owner != null)
					Owner.Update();
            }
        }

        /// <summary>
        /// Property to set or return the scale.
        /// </summary>
        public Vector2D Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
                if (Owner != null)
					Owner.Update();
            }
        }

        /// <summary>
        /// Property to set or return the rotation.
        /// </summary>
        public float Rotation
        {
            get
            {
                return _rotate;
            }
            set
            {
                _rotate = value;
                if (Owner != null)
					Owner.Update();
            }
        }

        /// <summary>
        /// Property to set or return the object axis.
        /// </summary>
        public Vector2D Axis
        {
            get
            {
                return _axis;
            }
            set
            {
                _axis = value;
                if (Owner != null)
					Owner.Update();
            }
        }

        /// <summary>
        /// Property to set or return the object size.
        /// </summary>
        public Vector2D Size
        {
            get
            {
                return _size;
            }
            set
            {
                _size = value;
                if (Owner != null)
					Owner.Update();
            }
        }

        /// <summary>
        /// Property to set or return the offset within the associated image for the object.
        /// </summary>
        public Vector2D ImageOffset
        {
            get
            {
                return _offset;
            }
            set
            {
                _offset = value;
                if (Owner != null)
					Owner.Update();
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
			_splinePosition.Clear();
			_splineRotation.Clear();
			_splineScale.Clear();
			_splineAxis.Clear();
            _splineImageOffset.Clear();

			// Add points to the spline.
			for (int i = 0; i < Owner.KeyCount; i++)
			{
				KeyTransform key = Owner.GetKeyAtIndex(i) as KeyTransform;

				_splinePosition.AddPoint(key.Position);
				_splineRotation.AddPoint(new Vector2D(key.Rotation, 0.0f));
				_splineScale.AddPoint(key.Scale);
				_splineAxis.AddPoint(key.Axis);
                _splineSize.AddPoint(key.Size);
                _splineImageOffset.AddPoint(key.ImageOffset);
			}

			// Recalculate tangents.
			_splinePosition.RecalculateTangents();
			_splineRotation.RecalculateTangents();
			_splineScale.RecalculateTangents();
			_splineAxis.RecalculateTangents();
            _splineSize.RecalculateTangents();
            _splineImageOffset.RecalculateTangents();			
		}

		/// <summary>
		/// Function to apply key information to the owning object of the animation.
		/// </summary>
		/// <param name="prevKeyIndex">Previous key index.</param>
		/// <param name="previousKey">Key prior to this one.</param>
		/// <param name="nextKey">Key after this one.</param>
		protected internal override void Apply(int prevKeyIndex, Key previousKey, Key nextKey)
		{
			KeyTransform previous = previousKey as KeyTransform;
			KeyTransform next = nextKey as KeyTransform;

			if (previousKey == null)
				throw new ArgumentNullException("previousKey");

			if (nextKey == null)
				throw new ArgumentNullException("nextKey");

			if (previous == null)
				throw new AnimationTypeMismatchException("key at time index", previousKey.Time.ToString("0.0"), "KeyTransform", previousKey.GetType().Name);

			if (next == null)
				throw new AnimationTypeMismatchException("key at time index", nextKey.Time.ToString("0.0"), "KeyTransform", nextKey.GetType().Name);

			// Copy if we're at the same frame.
			if ((Time == 0) || (previous.InterpolationMode == InterpolationMode.None))
			{
				_position = previous.Position;
				_rotate = previous.Rotation;
				_scale = previous.Scale;
				_axis = previous.Axis;
				_size = previous.Size;
				_offset = previous.ImageOffset;
			}
			else
			{
				// Calculate linear values.
				if (previous.InterpolationMode == InterpolationMode.Linear)
				{
					Vector2D position = Vector2D.Zero;      // Position.
					Vector2D scale = Vector2D.Zero;         // Scale.
					Vector2D axis = Vector2D.Zero;			// Axis of the object.
					Vector2D size = Vector2D.Zero;          // Size.
					Vector2D offset = Vector2D.Zero;        // Image offset.
					float rotation = 0;                     // Rotation.

					// Do translation.
					position = previous.Position;
					_position = (position + ((next._position - position) * Time));
					_position.X = (float)Math.Round(_position.X);
					_position.Y = (float)Math.Round(_position.Y);

					// Do scale.
					scale = previous.Scale;
					_scale = (scale + ((next._scale - scale) * Time));

					// Do rotation.
					rotation = previous.Rotation;
					_rotate = (rotation + ((next._rotate - rotation) * Time));

					// Perform axis shifting.
					axis = previous.Axis;
					_axis = (axis + ((next._axis - axis) * Time));
					_axis.X = (float)Math.Round(_axis.X);
					_axis.Y = (float)Math.Round(_axis.Y);

					// Change image size.
					size = previous.Size;
					_size = (size + ((next._size - size) * Time));
					_size.X = (float)Math.Round(_size.X);
					_size.Y = (float)Math.Round(_size.Y);

					// Change image offset.
					offset = previous.ImageOffset;
					_offset = (offset + ((next._offset - offset) * Time));
					_offset.X = (float)Math.Round(_offset.X);
					_offset.Y = (float)Math.Round(_offset.Y);
				}
				else
				{
					// Calculate spline values.
					if (Owner.NeedsUpdate)
						SetupSplines();

					// Calculate transforms.
					_position = _splinePosition[prevKeyIndex, Time];
					_position.X = (float)Math.Round(Position.X);
					_position.Y = (float)Math.Round(Position.Y);
					_scale = _splineScale[prevKeyIndex, Time];
					_rotate = _splineRotation[prevKeyIndex, Time].X;
					_axis = _splineAxis[prevKeyIndex, Time];
					_size = _splineSize[prevKeyIndex, Time];
					_offset = _splineImageOffset[prevKeyIndex, Time];
				}
			}

			if (Owner != null)
				Owner.Update();
		}

		/// <summary>
		/// Function to use the key data to update a layer object.
		/// </summary>
		/// <param name="layerObject">Layer object to update.</param>
		protected internal override void UpdateLayerObject(IAnimatable layerObject)
		{
			layerObject.Position = _position;
			layerObject.Scale = _scale;
			layerObject.Rotation = _rotate;
			layerObject.Axis = _axis;
			layerObject.Size = _size;
			layerObject.ImageOffset = _offset;
		}

		/// <summary>
		/// Function to clone this key.
		/// </summary>
		/// <returns>A clone of this key.</returns>
		public override object Clone()
		{
			KeyTransform newKey = null;         // Cloned key.

			newKey = new KeyTransform(null, Time);
			newKey.InterpolationMode = InterpolationMode;
			newKey.Position = _position;
			newKey.Rotation = _rotate;
			newKey.Axis = _axis;
			newKey.Scale = _scale;
			newKey.Size = _size;
			newKey.ImageOffset = _offset;

			return newKey;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Owner of this key.</param>
		/// <param name="timePosition">Position in time for this keyframe.</param>
		public KeyTransform(TrackTransform owner, float timePosition)
			: base(owner, timePosition)
		{
			_splinePosition = new Spline();
			_splinePosition.AutoCalculate = false;
			_splineRotation = new Spline();
			_splineRotation.AutoCalculate = false;
			_splineScale = new Spline();
			_splineScale.AutoCalculate = false;
			_splineAxis = new Spline();
			_splineAxis.AutoCalculate = false;
            _splineSize = new Spline();
            _splineSize.AutoCalculate = false;
            _splineImageOffset = new Spline();
            _splineImageOffset.AutoCalculate = false;
        
            // Get the size from the animation owner.
            if (Owner != null)
            {                
                _size = Owner.Owner.Owner.Size;
                _position = Owner.Owner.Owner.Position;
                _rotate = Owner.Owner.Owner.Rotation;
                _scale = Owner.Owner.Owner.Scale;
                _axis = Owner.Owner.Owner.Axis;
                _offset = Owner.Owner.Owner.ImageOffset;
            }

			InterpolationMode = InterpolationMode.Linear;
        }
		#endregion
	}
}
