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
// Created: TOBEREPLACED
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Example
{
	/// <summary>
	/// Cloud layer for a nebula.
	/// </summary>
	public class NebulaLayer
		: IDisposable
	{
		#region Variables.
		private Vector2D _initialOffset = Vector2D.Zero;				// Initial offset.
		private Vector2D _initialScale = Vector2D.Unit;					// Initial scale.
		private float _initialAngle = 0;								// Initial rotation angle.
		private Vector2D _offsetRange = Vector2D.Zero;					// Range to translate when animating.
		private Vector2D _scaleRange = Vector2D.Unit;					// Range to scale when animating.
		private float _angleRange = 0;									// Range to rotate when rotating.
		private Sprite _layerSprite = null;								// Layer sprite.
		private bool _allowRotate = false;								// Flag to indicate that we should rotate this layer over time.
		private bool _allowTranslate = false;							// Flag to indicate that we should translate this layer over time.
		private bool _allowScale = false;								// Flag to indicate that we should scale this layer over time.
		private float _angle = 0;										// Angle of rotation.
		private Vector2D _offset = Vector2D.Zero;						// Translation offset.
		private Vector2D _scale = Vector2D.Unit;						// Scale.
		private float _angleVelocity = 0;								// Rotation velocity.
		private Vector2D _offsetVelocity = Vector2D.Zero;				// Offset velocity.
		private Vector2D _scaleVelocity = Vector2D.Zero;				// Scale velocity.
		private Drawing.Color _tint = Drawing.Color.White;				// Tinting.
		private BlendingModes _blendMode = BlendingModes.Modulated;		// Blending mode.
		private byte _opacity = 255;									// Opacity of the layer.
		private LightningEffect _lightningEffect = null;				// Lightning effect.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the blending mode of the layer.
		/// </summary>
		public BlendingModes BlendMode
		{
			get
			{
				return _blendMode;
			}
			set
			{
				_blendMode = value;
			}
		}

		/// <summary>
		/// Property to set or return the tinting for the layer.
		/// </summary>
		public Drawing.Color Tint
		{
			get
			{
				return _tint;
			}
			set
			{
				_tint = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to allow scaling animation.
		/// </summary>
		public bool AllowScale
		{
			get
			{
				return _allowScale;
			}
			set
			{
				_allowScale = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to allow rotation animation.
		/// </summary>
		public bool AllowRotation
		{
			get
			{
				return _allowRotate;
			}
			set
			{
				_allowRotate = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to allow scaling animation.
		/// </summary>
		public bool AllowTranslation
		{
			get
			{
				return _allowTranslate;
			}
			set
			{
				_allowTranslate = value;
			}
		}

		/// <summary>
		/// Property to set or return the initial scale of the layer.
		/// </summary>
		public Vector2D Scale
		{
			get
			{
				return _initialScale;
			}
			set
			{
				_initialScale = value;
				_scale = value;
			}
		}

		/// <summary>
		/// Property to set or return the initial angle of rotation of the layer (in degrees).
		/// </summary>
		public float Angle
		{
			get
			{
				return _initialAngle;
			}
			set
			{
				_initialAngle = value;
				_angle = value;
			}
		}

		/// <summary>
		/// Property to set or return the opacity level.
		/// </summary>
		public byte Opacity
		{
			get
			{
				return _opacity;
			}
			set
			{
				_opacity = value;				
			}
		}

		/// <summary>
		/// Property to set or return the initial offset of the layer.
		/// </summary>
		public Vector2D Offset
		{
			get
			{
				return _initialOffset;
			}
			set
			{
				_initialOffset = value;
				_offset = value;
			}
		}

		/// <summary>
		/// Property to set or return the range of scale for the layer.
		/// </summary>
		public Vector2D ScaleRange
		{
			get
			{
				return _scaleRange;
			}
			set
			{
				_scaleRange = value;
			}
		}

		/// <summary>
		/// Property to set or return the range of the angle animation (in degrees).
		/// </summary>
		public float AngleRange
		{
			get
			{
				return _angleRange;
			}
			set
			{
				_angleRange = value;
			}
		}

		/// <summary>
		/// Property to set or return the offset velocity of the layer.
		/// </summary>
		public Vector2D OffsetVelocity
		{
			get
			{
				return _offsetVelocity;
			}
			set
			{
				_offsetVelocity = value;
			}
		}

		/// <summary>
		/// Property to set or return the scale velocity for the layer.
		/// </summary>
		public Vector2D ScaleVelocity
		{
			get
			{
				return _scaleVelocity;
			}
			set
			{
				_scaleVelocity = value;
			}
		}

		/// <summary>
		/// Property to set or return the velocity of the angle animation (in degrees).
		/// </summary>
		public float AngleVelocity
		{
			get
			{
				return _angleVelocity;
			}
			set
			{
				_angleVelocity = value;
			}
		}

		/// <summary>
		/// Property to set or return the offset range of the layer.
		/// </summary>
		public Vector2D OffsetRange
		{
			get
			{
				return _offsetRange;
			}
			set
			{
				_offsetRange = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the LightningEvent event of the _lightningEffect control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Example.LightningFlashEventArgs"/> instance containing the event data.</param>
		private void _lightningEffect_LightningEvent(object sender, LightningFlashEventArgs e)
		{
			if (e.BlendMode != BlendingModes.None)
				_blendMode = e.BlendMode;
			else
				_blendMode = BlendingModes.Modulated;

			_tint = e.Color;
		}

		/// <summary>
		/// Function to update the layer over time.
		/// </summary>
		/// <param name="frameTime">Frame delta time.</param>
		public void Update(float frameTime)
		{
			if (_allowScale)
			{
                _scale = Vector2D.Add(_scale, Vector2D.Multiply(_scaleVelocity, frameTime));

				if ((_scale.X < _initialScale.X) || (_offset.X > _initialScale.X + _scaleRange.X))
				{
					_scaleVelocity.X = -_scaleVelocity.X;
					if (_scale.X < _initialScale.X)
						_scale.X = _initialScale.X;
					if (_scale.X > _initialScale.X + _scaleRange.X)
						_scale.X = _initialScale.X + _scaleRange.X;

				}

				if ((_scale.Y < _initialScale.Y) || (_offset.Y > _initialScale.Y + _scaleRange.Y))
				{
					_scaleVelocity.Y = -_scaleVelocity.Y;
					if (_scale.Y < _initialScale.Y)
						_scale.Y = _initialScale.Y;
					if (_scale.Y > _initialScale.Y + _scaleRange.Y)
						_scale.Y = _initialScale.Y + _scaleRange.Y;
				}
			}

			if (_allowTranslate)
			{
                _offset = Vector2D.Add(_offset, Vector2D.Multiply(_offsetVelocity, frameTime));

				if ((_offset.X < -_offsetRange.X) || (_offset.X > _offsetRange.X))
				{
					_offsetVelocity.X = -_offsetVelocity.X;
					if (_offset.X < -_offsetRange.X)
						_offset.X = -_offsetRange.X;
					if (_offset.X > _offsetRange.X)
						_offset.X = _offsetRange.X;

				}

				if ((_offset.Y < -_offsetRange.Y) || (_offset.Y > _offsetRange.Y))
				{
					_offsetVelocity.Y = -_offsetVelocity.Y;
					if (_offset.Y < -_offsetRange.Y)
						_offset.Y = -_offsetRange.Y;
					if (_offset.Y > _offsetRange.Y)
						_offset.Y = _offsetRange.Y;
				}
			}

			if (_allowRotate)
			{
				_angle += _angleVelocity * frameTime;
				if ((_angle < -_angleRange) || (_angle > _angleRange))
				{
					_angleVelocity = -_angleVelocity;
					if (_angle < -_angleRange)
						_angle = -_angleRange;
					if (_angle > _angleRange)
						_angle = _angleRange;
				}
			}
		}

		/// <summary>
		/// Function to draw the layer.
		/// </summary>
		public void Draw()
		{
			// Update the sprite.
			_layerSprite.Opacity = _opacity;
			_layerSprite.Color = _tint;
			_layerSprite.BlendingMode = _blendMode;
			_layerSprite.Scale = _scale;
			_layerSprite.Position = _offset;
			_layerSprite.Rotation = _angle;

			_layerSprite.Draw();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="NebulaLayer"/> class.
		/// </summary>
		/// <param name="layerSprite">Sprite used to represent the layer.</param>
		/// <param name="lightningEffect">Lightning effect.</param>
		public NebulaLayer(Sprite layerSprite, LightningEffect lightningEffect)
		{
			_layerSprite = layerSprite;
			_lightningEffect = lightningEffect;
			_lightningEffect.LightningEvent += new LightningFlashEventHandler(_lightningEffect_LightningEvent);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				_lightningEffect.LightningEvent -= new LightningFlashEventHandler(_lightningEffect_LightningEvent);
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
