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

namespace GorgonLibrary.Graphics.Animations
{
	/// <summary>
	/// Object representing a transformation key.
	/// </summary>
	public class KeyTransform
		: IKey<KeyTransform>
	{
		#region Variables.
		private Spline _splineAxis;							// Axis spline.
		private Spline _splinePosition;						// Position spline.
		private Spline _splineScale;						// Scale spline.
		private Spline _splineRotation;						// Rotation spline.
        private Spline _splineSize;                         // Size spline.
        private Spline _splineImageOffset;                  // Size spline.
		private float _rotate = 0.0f;						// Rotation (in degrees).
        private Vector2D _size = Vector2D.Unit;             // Size of the object.
        private Vector2D _offset = Vector2D.Unit;           // Image offset of the object.
		private Vector2D _position = Vector2D.Zero;			// Position of the object.
		private Vector2D _scale = Vector2D.Unit;			// Scale of the object.
		private Vector2D _axis = Vector2D.Zero;				// Object axis.
		private TrackTransform _owner;						// Owning track.
		private InterpolationMode _interpolation;			// Interpolation mode.
		private float _frameTime = 0;						// Time index for the keyframe.
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
                if (_owner != null)
                    _owner.Updated = true;
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
                if (_owner != null)
                    _owner.Updated = true;
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
                if (_owner != null)
                    _owner.Updated = true;
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
                if (_owner != null)
                    _owner.Updated = true;
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
                if (_owner != null)
                    _owner.Updated = true;
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
                if (_owner != null)
                    _owner.Updated = true;
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
			for (int i = 0; i < _owner.KeyCount; i++)
			{
				KeyTransform key = _owner.GetAssignedKey(i);

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

			_owner.Updated = false;
		}

		/// <summary>
		/// Function to assign this key to a track.
		/// </summary>
		/// <param name="track">Track to assign to.</param>
		internal void AssignKey(TrackTransform track)
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
		public KeyTransform(TrackTransform owner, float timePosition)
		{
			_owner = owner;
			_interpolation = InterpolationMode.Linear;
			_frameTime = timePosition;
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
            if (_owner != null)
            {                
                _size = _owner.Owner.Owner.Size;
                _position = _owner.Owner.Owner.Position;
                _rotate = _owner.Owner.Owner.Rotation;
                _scale = _owner.Owner.Owner.Scale;
                _axis = _owner.Owner.Owner.Axis;
                _offset = _owner.Owner.Owner.ImageOffset;
            }
        }
		#endregion

		#region IKey<KeyTransform> Members
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
		public Track<KeyTransform> Owner
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
		void IKey<KeyTransform>.Apply(int prevKeyIndex, KeyTransform previousKey, KeyTransform nextKey)
		{
			// Copy if we're at the same frame.
			if ((Time == 0) || (previousKey.InterpolationMode == InterpolationMode.None))
			{
				_position = previousKey.Position;
				_rotate = previousKey.Rotation;
				_scale = previousKey.Scale;
                _axis = previousKey.Axis;
                _size = previousKey.Size;
                _offset = previousKey.ImageOffset;
			}
			else
			{
				// Calculate linear values.
				if (previousKey.InterpolationMode == InterpolationMode.Linear)
				{
					Vector2D position = Vector2D.Zero;      // Position.
					Vector2D scale = Vector2D.Zero;         // Scale.
					Vector2D axis = Vector2D.Zero;			// Axis of the object.
                    Vector2D size = Vector2D.Zero;          // Size.
                    Vector2D offset = Vector2D.Zero;        // Image offset.
					float rotation = 0;                     // Rotation.

					// Do translation.
					position = previousKey.Position;
					_position = (position + ((nextKey._position - position) * Time));
					_position.X = (float)Math.Round(_position.X);
					_position.Y = (float)Math.Round(_position.Y);

					// Do scale.
					scale = previousKey.Scale;
					_scale = (scale + ((nextKey._scale - scale) * Time));

					// Do rotation.
					rotation = previousKey.Rotation;
					_rotate = (rotation + ((nextKey._rotate - rotation) * Time));

					// Perform axis shifting.
					axis = previousKey.Axis;
					_axis = (axis + ((nextKey._axis - axis) * Time));
					_axis.X = (float)Math.Round(_axis.X);
					_axis.Y = (float)Math.Round(_axis.Y);

                    // Change image size.
                    size = previousKey.Size;
                    _size = (size + ((nextKey._size - size) * Time));
                    _size.X = (float)Math.Round(_size.X);
                    _size.Y = (float)Math.Round(_size.Y);

                    // Change image offset.
                    offset = previousKey.ImageOffset;
                    _offset = (offset + ((nextKey._offset - offset) * Time));
                    _offset.X = (float)Math.Round(_offset.X);
                    _offset.Y = (float)Math.Round(_offset.Y);
                }
				else
				{
					// Calculate spline values.
					if (_owner.Updated)
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

			if (_owner != null)
				_owner.Updated = true;
		}

		/// <summary>
		/// Function to use the key data to update a layer object.
		/// </summary>
		/// <param name="layerObject">Layer object to update.</param>
		void IKey<KeyTransform>.UpdateLayerObject(IAnimatable layerObject)
		{
			layerObject.Position = _position;
			layerObject.Scale = _scale;
			layerObject.Rotation = _rotate;
			layerObject.Axis = _axis;
            layerObject.Size = _size;
            layerObject.ImageOffset = _offset;
		}

		/// <summary>
		/// Function to copy this key into a new time.
		/// </summary>
		/// <param name="newTime">Time index to place the copy into.</param>
		/// <returns>The copy of the this key.</returns>
		public KeyTransform CopyTo(float newTime)
		{
			KeyTransform copy = Clone();		// Create a copy.

			// Assign the time and owner.
			copy.Time = newTime;
			Owner.AddKey(copy);

			return copy;
		}

		/// <summary>
		/// Function to clone this key.
		/// </summary>
		/// <returns>A clone of this key.</returns>
		public KeyTransform Clone()
		{
			KeyTransform newKey = null;         // Cloned key.

			newKey = new KeyTransform(null, _frameTime);
			newKey.InterpolationMode = _interpolation;
			newKey.Position = _position;
			newKey.Rotation = _rotate;
			newKey.Axis = _axis;
			newKey.Scale = _scale;
            newKey.Size = _size;
            newKey.ImageOffset = _offset;

			return newKey;
		}
		#endregion
		#endregion
	}
}
