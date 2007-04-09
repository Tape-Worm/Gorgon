#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Saturday, January 06, 2007 2:13:16 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using SharpUtilities;
using SharpUtilities.Mathematics;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Shaders;
using GorgonLibrary.Graphics.Animations;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Object representing a renderable object.
	/// </summary>
	public abstract class Renderable<V>
		: NamedObject, IAnimatable
		where V : struct
	{
		#region Variables.
		private bool _inheritSmoothing;						// Flag to indicate that we inherit smoothing settings from the layer.
		private bool _inheritBlending;						// Flag to indicate that we inherit blending settings from the layer.
		private bool _inheritMaskFunction;					// Flag to indicate that we inherit the alpha mask settings from the layer.
		private bool _inheritMaskValue;						// Flag to indicate that we inherit the alpha mask value settings from the layer.
		private bool _inheritHorizontalWrap;				// Flag to indicate that we inherit the horizontal wrap settings from the layer.
		private bool _inheritVerticalWrap;					// Flag to indicate that we inherit the vertical wrap settings from the layer.
		private bool _inheritStencilPassOperation;			// Flag to indicate that we inherit the stencil pass operation from the layer.
		private bool _inheritStencilFailOperation;			// Flag to indicate that we inherit the stencil fail operation from the layer.
		private bool _inheritStencilZFailOperation;			// Flag to indicate that we inherit the stencil z fail operation from the layer.
		private bool _inheritStencilCompare;				// Flag to indicate that we inherit the stencil comparison operator from the layer.
		private bool _inheritStencilReference;				// Flag to indicate that we inherit the stencil reference from the layer.
		private bool _inheritStencilMask;					// Flag to indicate that we inherit the stencil mask from the layer.
		private bool _inheritUseStencil;					// Flag to indicate that we inherit the stencil enabled flag from the layer.
		private Blending _blending;							// Blending mode.
		private AlphaBlendOperation _sourceBlend;			// Source blending operation.
		private AlphaBlendOperation _destBlend;				// Destination blending operation.
		private CompareFunctions _maskFunction;				// Alpha mask function.
		private int _maskValue;								// Alpha value to mask for.
		private Smoothing _smoothing;						// Smoothing operation.
		private Viewport _clipping;							// Clipping window.
		private Shader _shader;								// Shader for the object.
		private ImageAddressing _wrapHMode;					// Horizontal image wrapping mode.
		private ImageAddressing _wrapVMode;					// Horizontal image wrapping mode.
		private StencilOperations _stencilPassOperation;	// Stencil pass operation.
		private StencilOperations _stencilFailOperation;	// Stencil fail operation.
		private StencilOperations _stencilZFailOperation;	// Stencil Z fail operation.
		private CompareFunctions _stencilCompare;			// Stencil compare operation.
		private int _stencilReference;						// Stencil reference value.
		private int _stencilMask;							// Stencil mask value.
		private bool _useStencil;							// Flag to indicate whether to use the stencil or not.
        private bool _disposed = false;                     // Flag to indicate that the object has been disposed.
		private Image _image = null;						// Image that this object is bound with.
        
        /// <summary>Flag to indicate that parent information needs updating.</summary>
        protected bool _needParentUpdate = true;
        /// <summary>Final position.</summary>
        protected Vector2D _finalPosition = Vector2D.Zero;
        /// <summary>Final scale.</summary>
        protected Vector2D _finalScale = Vector2D.Unit;
        /// <summary>Final rotation.</summary>
        protected float _finalRotation = 0.0f;
        /// <summary>Object that is the parent of this object.</summary>
        protected IRenderable _parent = null;
        /// <summary>Children for this object.</summary>
        protected RenderableChildren _children = null;
		/// <summary>Flag to indicate that the AABB needs updating.</summary>
		protected bool _needAABBUpdate = true;
		/// <summary>Flag to indicate that the </summary>
		protected bool _dimensionsChanged;
		/// <summary>Flag to indicate that the coordinates for the image have changed.</summary>
		protected bool _imageCoordinatesChanged;
		/// <summary>Size of the object.</summary>
		protected Vector2D _size;
		/// <summary>Vertices for the object.</summary>
		protected V[] _vertices;
		/// <summary>Position of the object.</summary>
		protected Vector2D _position;
		/// <summary>Rotation of the object.</summary>
		protected float _rotation;		
		/// <summary>Scale of the sprite.</summary>
		protected Vector2D _scale;
		/// <summary>Axis of the sprite.</summary>
		protected Vector2D _axis;
		/// <summary>Axis aligned bounding box for the object.</summary>
		protected Drawing.RectangleF _AABB;
        /// <summary>List of animations.</summary>
        protected AnimationList _animations = null;
        /// <summary>Parent position.</summary>
        protected Vector2D _parentPosition = Vector2D.Zero;
        /// <summary>Parent scale.</summary>
        protected Vector2D _parentScale = Vector2D.Zero;
        /// <summary>Parent rotation.</summary>
        protected float _parentRotation = 0;
        #endregion

		#region Properties.
        /// <summary>
        /// Property to set or return animations for the object.
        /// </summary>        
        public AnimationList Animations
        {
            get 
            {
                return _animations;
            }
        }

        /// <summary>
        /// Property to return the final position.
        /// </summary>
        public Vector2D FinalPosition
        {
            get
            {
                GetParentTransform();
                return _finalPosition;
            }
        }

        /// <summary>
        /// Property to return the final scale.
        /// </summary>
        public Vector2D FinalScale
        {
            get
            {
                GetParentTransform();
                return _finalScale;
            }
        }

        /// <summary>
        /// Property to return the final rotation.
        /// </summary>
        public float FinalRotation
        {
            get
            {
                GetParentTransform();
                return _finalRotation;
            }
        }

        /// <summary>
        /// Property to return the parent of this object.
        /// </summary>
        public IRenderable Parent
        {
            get
            {
                return _parent;
            }
        }

        /// <summary>
        /// Property to return the children of this object.
        /// </summary>
        public RenderableChildren Children
        {
            get
            {
                return _children;
            }
        }

        /// <summary>
        /// Property to return whether or not the object has been disposed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return _disposed;
            }
        }

		/// <summary>
		/// Property to set or return a uniform scale across the X and Y axis.
		/// </summary>
		public abstract float UniformScale
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the primitive style for the object.
		/// </summary>
		public abstract PrimitiveStyle PrimitiveStyle
		{
			get;
		}

		/// <summary>
		/// Property to return whether to use an index buffer or not.
		/// </summary>
		public abstract bool UseIndices
		{
			get;
		}

		/// <summary>
		/// Property to set or return the color.
		/// </summary>
		public abstract Drawing.Color Color
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the opacity.
		/// </summary>
		public abstract byte Opacity
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the axis of the sprite.
		/// </summary>
		public abstract Vector2D Axis
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the scale of the sprite.
		/// </summary>
		public abstract Vector2D Scale
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the size of the object with scaling applied.
		/// </summary>
		public virtual Vector2D ScaledDimensions
		{
			get
			{
				return new Vector2D(_scale.X * _size.X, _scale.Y * _size.Y);
			}
		}

		/// <summary>
		/// Property to set or return the image that this object is bound with.
		/// </summary>
		public virtual Image Image
		{
			get
			{
				return _image;
			}
			set
			{
				_image = value;
			}
		}

		/// <summary>
		/// Property to set or return the position of the object.
		/// </summary>
		public virtual Vector2D Position
		{
			get
			{
				return _position;
			}
			set
			{
				_needAABBUpdate = true;
                _needParentUpdate = true;
				_position = value;

                if (_children.Count > 0)
                    ((IRenderable)this).UpdateChildren();
			}
		}

		/// <summary>
		/// Property to set or return the rotation angle in degrees.
		/// </summary>
		public virtual float Rotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				_needAABBUpdate = true;
                _needParentUpdate = true;
				_rotation = value;

                if (_children.Count > 0)
                    ((IRenderable)this).UpdateChildren();
			}
		}

		/// <summary>
		/// Property to set or return the size of the object.
		/// </summary>
		public virtual Vector2D Size
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
				_needAABBUpdate = true;
				_dimensionsChanged = true;
				_imageCoordinatesChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the width of the object.
		/// </summary>
		public virtual float Width
		{
			get
			{
				return _size.X;
			}
			set
			{
				_needAABBUpdate = true;
				Size = new Vector2D(value, _size.Y);
			}
		}

		/// <summary>
		/// Property to set or return the height of the object.
		/// </summary>
		public virtual float Height
		{
			get
			{
				return _size.Y;
			}
			set
			{
				_needAABBUpdate = true;
				Size = new Vector2D(_size.X, value);
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to get the parent transform.
        /// </summary>
        protected void GetParentTransform()
        {
            if (!_needParentUpdate)
                return;

            if (_parent == null)
            {
                _finalPosition = _position;
                _finalRotation = _rotation;
                _finalScale = _scale;
            }
            else
            {
                float angle = MathUtility.Radians(_parentRotation);     // Angle in radians.
                float cos = MathUtility.Cos(angle);                     // Cached cosine.
                float sin = MathUtility.Sin(angle);                     // Cached sine.

                // Rotate around the offset from the parent.
                _finalPosition.X = ((_position.X * _parentScale.X) * cos) - ((_position.Y * _parentScale.Y) * sin);
                _finalPosition.Y = ((_position.X * _parentScale.X) * sin) + ((_position.Y * _parentScale.Y) * cos);

                // Get the scale.
                _finalScale = _scale * _parentScale;

                // Get the rotation.
                _finalRotation = _parentRotation + _rotation;                

                // Update the translation.
                _finalPosition += _parentPosition;
            }

            _needParentUpdate = false;
        }

		/// <summary>
		/// Function to update the dimensions for an object.
		/// </summary>
		protected abstract void UpdateDimensions();

		/// <summary>
		/// Function to update a transformation to the object.
		/// </summary>
		protected virtual void UpdateTransform()
		{
		}

        /// <summary>
        /// Function to set the blending modes.
        /// </summary>
        /// <param name="value">Blending value.</param>
        protected virtual void SetBlendMode(Blending value)
		{
			switch (value)
			{
				case Blending.Additive:
					_sourceBlend = AlphaBlendOperation.SourceAlpha;
					_destBlend = AlphaBlendOperation.One;
					break;
				case Blending.Burn:
					_sourceBlend = AlphaBlendOperation.DestinationColor;
					_destBlend = AlphaBlendOperation.SourceColor;
					break;
				case Blending.Dodge:
					_sourceBlend = AlphaBlendOperation.DestinationColor;
					_destBlend = AlphaBlendOperation.DestinationColor;
					break;
				case Blending.None:
					_sourceBlend = AlphaBlendOperation.One;
					_destBlend = AlphaBlendOperation.Zero;
					break;
				case Blending.Normal:
					_sourceBlend = AlphaBlendOperation.SourceAlpha;
					_destBlend = AlphaBlendOperation.InverseSourceAlpha;
					break;
                case Blending.PreMultiplied:
                    _sourceBlend = AlphaBlendOperation.One;
                    _destBlend = AlphaBlendOperation.InverseSourceAlpha;
                    break;
			}
		}

		/// <summary>
		/// Function to update the source image positioning.
		/// </summary>
		protected virtual void UpdateImageLayer()
		{
		}        

		/// <summary>
		/// Function to set the anchor axis of the sprite.
		/// </summary>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical position.</param>
		public virtual void SetAxis(float x, float y)
		{
			Axis = new Vector2D(x, y);
		}

		/// <summary>
		/// Function to set the scale of the sprite.
		/// </summary>
		/// <param name="x">Horizontal scale.</param>
		/// <param name="y">Vertical scale.</param>
		public virtual void SetScale(float x, float y)
		{
			Scale = new Vector2D(x, y);
		}

		/// <summary>
		/// Function to update the object position.
		/// </summary>
		/// <param name="x">Horizontal position of the object.</param>
		/// <param name="y">Vertical position of the object.</param>
		public virtual void SetPosition(float x,float y)
		{
			Position = new Vector2D(x, y);
		}

		/// <summary>
		/// Function to update the size of the object.
		/// </summary>
		/// <param name="width">Width of the object.</param>
		/// <param name="height">Height of the object.</param>
		public virtual void SetSize(float width, float height)
		{
			Size = new Vector2D(width, height);
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <param name="flush">TRUE to flush the buffers after drawing, FALSE to only flush on state change.</param>
		public abstract void Draw(bool flush);

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		public virtual void Draw()
		{
			Draw(false);
		}

		/// <summary>
		/// Function to update AABB.
		/// </summary>
		public virtual void UpdateAABB()
		{
			_AABB = new Drawing.RectangleF(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the object.</param>
		public Renderable(string name) 
			: base(name)
		{
			_size = new Vector2D(1.0f, 1.0f);
			_position = Vector2D.Zero;
			_rotation = 0.0f;
			_vertices = null;
			_dimensionsChanged = true;
			_imageCoordinatesChanged = true;
			BlendingMode = Blending.Normal;
			_maskValue = 1;
			_maskFunction = CompareFunctions.GreaterThan;
			_smoothing = Smoothing.None;
			_inheritBlending = true;
			_inheritMaskFunction = true;
			_inheritMaskValue = true;
			_inheritSmoothing = true;
			_inheritHorizontalWrap = true;
			_inheritVerticalWrap = true;
			_inheritStencilPassOperation = true;
			_inheritStencilFailOperation = true;
			_inheritStencilZFailOperation = true;
			_inheritStencilCompare = true;
			_inheritStencilReference = true;
			_inheritStencilMask = true;
			_inheritUseStencil = true;
			_clipping = null;
			_wrapHMode = ImageAddressing.Clamp;
			_wrapVMode = ImageAddressing.Clamp;
			_stencilCompare = CompareFunctions.Always;
			_useStencil = false;
			_stencilPassOperation = StencilOperations.Keep;
			_stencilFailOperation = StencilOperations.Keep;
			_stencilZFailOperation = StencilOperations.Keep;
			_stencilReference = 0;
			_stencilMask = -1;
            _parent = null;
            _children = new RenderableChildren(this);
			
			_axis = new Vector2D(1.0f, 1.0f);
			_AABB = new Drawing.RectangleF(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
		}
		#endregion

		#region IRenderable Members
		/// <summary>
		/// Property to set or return whether to enable the use of the stencil buffer or not.
		/// </summary>
		public bool StencilEnabled
		{
			get
			{
				if (_inheritUseStencil)
					return Gorgon.StateManager.StencilEnabled;
				else
					return _useStencil;
			}
			set
			{
				_useStencil = value;
				_inheritUseStencil = false;
			}
		}

		/// <summary>
		/// Property to set or return the reference value for the stencil buffer.
		/// </summary>
		public int StencilReference
		{
			get
			{
				if (_inheritStencilReference)
					return Gorgon.StateManager.StencilReference;
				else
					return _stencilReference;
			}
			set
			{
				_stencilReference = value;
				_inheritStencilReference = false;
			}
		}

		/// <summary>
		/// Property to set or return the mask value for the stencil buffer.
		/// </summary>
		public int StencilMask
		{
			get
			{
				if (_inheritStencilMask)
					return Gorgon.StateManager.StencilMask;

				return _stencilMask;
			}
			set
			{
				_stencilMask = value;
				_inheritStencilMask = false;
			}
		}

		/// <summary>
		/// Property to set or return the operation for passing stencil values.
		/// </summary>
		public StencilOperations StencilPassOperation
		{
			get
			{
				if (_inheritStencilPassOperation)
					return Gorgon.StateManager.StencilPassOperation;

				return _stencilPassOperation;
			}
			set
			{
				_stencilPassOperation = value;
				_inheritStencilPassOperation = false;
			}
		}

		/// <summary>
		/// Property to set or return the operation for the failing stencil values.
		/// </summary>
		public StencilOperations StencilFailOperation
		{
			get
			{
				if (_inheritStencilFailOperation)
					return Gorgon.StateManager.StencilFailOperation;

				return _stencilFailOperation;
			}
			set
			{
				_stencilFailOperation = value;
				_inheritStencilFailOperation = false;
			}
		}

		/// <summary>
		/// Property to set or return the stencil operation for the failing depth values.
		/// </summary>
		public StencilOperations StencilZFailOperation
		{
			get
			{
				if (_inheritStencilZFailOperation)
					return Gorgon.StateManager.StencilZFailOperation;

				return _stencilZFailOperation;
			}
			set
			{
				_stencilZFailOperation = value;
				_inheritStencilZFailOperation = false;
			}
		}

		/// <summary>
		/// Property to set or return the stencil comparison function.
		/// </summary>
		public CompareFunctions StencilCompare
		{
			get
			{
				if (_inheritStencilCompare)
					return Gorgon.StateManager.StencilCompare;

				return _stencilCompare;
			}
			set
			{
				_stencilCompare = value;
				_inheritStencilCompare = false;
			}
		}

		/// <summary>
		/// Property to set or return the source blending operation.
		/// </summary>
		public AlphaBlendOperation SourceBlend
		{
			get
			{
				if (_inheritBlending)
					return Gorgon.StateManager.SourceBlend;
				else
					return _sourceBlend;
			}
			set
			{
				_sourceBlend = value;
				_inheritBlending = false;
			}
		}

		/// <summary>
		/// Property to set or return the destination blending operation.
		/// </summary>
		public AlphaBlendOperation DestinationBlend
		{
			get
			{
				if (_inheritBlending)
					return Gorgon.StateManager.DestinationBlend;
				else
					return _destBlend;
			}
			set
			{
				_destBlend = value;
				_inheritBlending = false;
			}
		}

		/// <summary>
		/// Property to set or return the wrapping mode to use.
		/// </summary>
		public ImageAddressing WrapMode
		{
			get
			{
				if (_inheritHorizontalWrap)
					return Gorgon.StateManager.HorizontalWrapMode;
				else
					return _wrapHMode;
			}
			set
			{
				HorizontalWrapMode = VerticalWrapMode = value;
			}
		}

		/// <summary>
		/// Property to set or return the horizontal wrapping mode to use.
		/// </summary>
		public ImageAddressing HorizontalWrapMode
		{
			get
			{
				if (_inheritHorizontalWrap)
					return Gorgon.StateManager.HorizontalWrapMode;
				else
					return _wrapHMode;
			}
			set
			{
				_wrapHMode = value;
				_inheritHorizontalWrap = false;
			}
		}

		/// <summary>
		/// Property to set or return the vertical wrapping mode to use.
		/// </summary>
		public ImageAddressing VerticalWrapMode
		{
			get
			{
				if (_inheritVerticalWrap)
					return Gorgon.StateManager.VerticalWrapMode;
				else
					return _wrapVMode;
			}
			set
			{
				_wrapVMode = value;
				_inheritVerticalWrap = false;
			}
		}

		/// <summary>
		/// Property to set or return a shader effect for this object.
		/// </summary>
		public virtual Shader Shader
		{
			get
			{
				return _shader;
			}
			set
			{
				_shader = value;
			}
		}

		/// <summary>
		/// Property to set or return the clipping rectangle for this object.
		/// </summary>
		public Viewport ClippingViewport
		{
			get
			{
				if ((_clipping == null) && (Gorgon.Renderer.CurrentRenderTarget != null))
					return Gorgon.Renderer.CurrentRenderTarget.ClippingViewport;

				return _clipping;
			}
			set
			{
				_clipping = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit smoothing from the layer.
		/// </summary>
		public bool InheritSmoothing
		{
			get
			{
				return _inheritSmoothing;
			}
			set
			{
				_inheritSmoothing = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit blending from the layer.
		/// </summary>
		public bool InheritBlending
		{
			get
			{
				return _inheritBlending;
			}
			set
			{
				_inheritBlending = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the alpha mask function from the layer.
		/// </summary>
		public bool InheritAlphaMaskFunction
		{
			get
			{
				return _inheritMaskFunction;
			}
			set
			{
				_inheritMaskFunction = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the alpha mask value from the layer.
		/// </summary>
		public bool InheritAlphaMaskValue
		{
			get
			{
				return _inheritMaskValue;
			}
			set
			{
				_inheritMaskValue = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the horizontal wrapping from the layer.
		/// </summary>
		public bool InheritHorizontalWrapping
		{
			get
			{
				return _inheritHorizontalWrap;
			}
			set
			{
				_inheritHorizontalWrap = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the vertical wrapping from the layer.
		/// </summary>
		public bool InheritVerticalWrapping
		{
			get
			{
				return _inheritVerticalWrap;
			}
			set
			{
				_inheritVerticalWrap = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil enabled flag from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilEnabled
		{
			get
			{
				return _inheritUseStencil;
			}
			set
			{
				_inheritUseStencil = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil reference from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilReference
		{
			get
			{
				return _inheritStencilReference;
			}
			set
			{
				_inheritStencilReference = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil mask from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilMask
		{
			get
			{
				return _inheritStencilMask;
			}
			set
			{
				_inheritStencilMask = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil pass operation from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilPassOperation
		{
			get
			{
				return _inheritStencilPassOperation;
			}
			set
			{
				_inheritStencilPassOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil failed operation from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilFailOperation
		{
			get
			{
				return _inheritStencilFailOperation;
			}
			set
			{
				_inheritStencilFailOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil z-failed operation from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilZFailOperation
		{
			get
			{
				return _inheritStencilZFailOperation;
			}
			set
			{
				_inheritStencilZFailOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil compare from the layer.
		/// </summary>
		/// <value></value>
		public bool InheritStencilCompare
		{
			get
			{
				return _inheritStencilCompare;
			}
			set
			{
				_inheritStencilCompare = value;
			}
		}

		/// <summary>
		/// Property to set or return the type of smoothing for the sprites.
		/// </summary>
		public Smoothing Smoothing
		{
			get
			{
				if (_inheritSmoothing)
					return Gorgon.StateManager.LayerSmoothing;
				else
					return _smoothing;
			}
			set
			{
				_smoothing = value;
				_inheritSmoothing = false;
			}
		}

		/// <summary>
		/// Property to set or return the function used for alpha masking.
		/// </summary>
		public CompareFunctions AlphaMaskFunction
		{
			get
			{
				if (_inheritMaskFunction)
					return Gorgon.StateManager.LayerAlphaMaskFunction;
				else
					return _maskFunction;
			}
			set
			{
				_maskFunction = value;
				_inheritMaskFunction = false;
			}
		}

		/// <summary>
		/// Property to set or return the alpha value used for alpha masking.
		/// </summary>
		public int AlphaMaskValue
		{
			get
			{
				if (_inheritMaskValue)
					return Gorgon.StateManager.LayerAlphaMaskValue;
				else
					return _maskValue;
			}
			set
			{
				_maskValue = value;
				_inheritMaskValue = false;
			}
		}

		/// <summary>
		/// Property to set the blending mode.
		/// </summary>
		public Blending BlendingMode
		{
			get
			{
				if (_inheritBlending)
					return Gorgon.StateManager.LayerBlending;
				else
					return _blending;
			}
			set
			{
				_blending = value;
				_inheritBlending = false;
				SetBlendMode(value);
			}
		}

		/// <summary>
		/// Property to return the AABB for the object.
		/// </summary>
		public Drawing.RectangleF AABB
		{
			get
			{
				return _AABB;
			}
		}

		/// <summary>
		/// Property to set or return the offset within the source image to start drawing from.
		/// </summary>
		public virtual Vector2D ImageOffset
		{
			get
			{
				return Vector2D.Zero;
			}
			set
			{
			}
		}
		
		/// <summary>
        /// Function to set the parent of this object.
        /// </summary>
        /// <param name="parent">Parent of the object.</param>
        void IRenderable.SetParent(IRenderable parent)
        {
            _parent = parent;
            _needParentUpdate = true;
            parent.UpdateChildren();            
        }

        /// <summary>
        /// Function to update children.
        /// </summary>
        void IRenderable.UpdateChildren()
        {
            Renderable<V> child = null;        // Child object.

            if (_children.Count < 1)
                return;

            // Update children.
            for (int i = 0; i < _children.Count; i++)
            {
                child = (Renderable<V>)_children[i].Child;
                if (child != null)
                {
                    child._needParentUpdate = true;

                    child._parentPosition = FinalPosition;
                    child._parentRotation = FinalRotation;
                    child._parentScale = FinalScale;
                    _children[i].Child.UpdateChildren();
                }
            }
        }

		/// <summary>
		/// Function to apply animations to the object.
		/// </summary>
		void IAnimatable.ApplyAnimations()
		{
			if (_animations == null)
				return;

			// Update animations.
			for (int i = 0; i < _animations.Count; i++)
				_animations[i].ApplyAnimation();
		}
        #endregion
	}
}
