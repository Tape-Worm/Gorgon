#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Saturday, January 06, 2007 2:13:16 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a renderable object.
	/// </summary>
	public abstract class Renderable
		: NamedObject, IRenderableStates, ICloneable<Renderable>, IAnimated
	{
		#region Variables.
		private bool _inheritSmoothing;						// Flag to indicate that we inherit smoothing settings from the layer.
		private bool _inheritBlending;						// Flag to indicate that we inherit blending settings from the layer.
        private bool _inheritAlphaBlending;					// Flag to indicate that we inherit alpha blending settings from the layer.
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
		private bool _inheritRotation;						// Flag to indicate that we inherit rotation from a parent renderable.
		private bool _inheritScale;							// Flag to indicate that we inherit scale from a parent renderable.
		private bool _inheritDepthWriteEnable;				// Flag to indicate that we inherit the depth buffer write flag.
		private bool _inheritDepthBias;						// Flag to indicate that we inherit the depth bias.
		private bool _inheritDepthTestCompare;				// Flag to indicate that we inherit the depth testing comparison operator.
		private float _depthBias;							// Depth bias.
		private bool _depthWriteEnabled;					// Depth writing enabled flag.
		private CompareFunctions _depthCompare;				// Depth test comparison function.
		private BlendingModes _blending;					// Blending mode.
		private AlphaBlendOperation _sourceBlend;			// Source blending operation.
		private AlphaBlendOperation _destBlend;				// Destination blending operation.
        private AlphaBlendOperation _sourceAlphaBlend;	    // Source blending operation.
        private AlphaBlendOperation _destAlphaBlend;		// Destination blending operation.
		private CompareFunctions _maskFunction;				// Alpha mask function.
		private int _maskValue;								// Alpha value to mask for.
		private Smoothing _smoothing;						// Smoothing operation.
		private ImageAddressing _wrapHMode;					// Horizontal image wrapping mode.
		private ImageAddressing _wrapVMode;					// Horizontal image wrapping mode.
		private StencilOperations _stencilPassOperation;	// Stencil pass operation.
		private StencilOperations _stencilFailOperation;	// Stencil fail operation.
		private StencilOperations _stencilZFailOperation;	// Stencil Z fail operation.
		private CompareFunctions _stencilCompare;			// Stencil compare operation.
		private int _stencilReference;						// Stencil reference value.
		private int _stencilMask;							// Stencil mask value.
		private bool _useStencil;							// Flag to indicate whether to use the stencil or not.
		private Image _image = null;						// Image that this object is bound with.		        
        private bool _needParentUpdate = true;				// Flag to indicate that parent information needs updating.
		private bool _needAABBUpdate = true;				// Flag to indicate that the AABB needs updating.
		private Renderable _parent = null;					// Object that is the parent of this object.
		private RenderableChildren _children = null;		// Children for this object.		
		private AnimationList _animations = null;			// Animation list.
		private Drawing.RectangleF _AABB;					// Axis aligned bounding box for the object.
		private bool _dimensionsChanged = true;				// Flag to indicate that the size of the object has changed.
		private bool _imageCoordinatesChanged = true;		// Flag to indicate that the coordinates for the image have changed.
		private Vector2D _size;								// Size of the object.
		private Vector2D _position;							// Position of the object.
		private float _rotation;							// Rotation of the object.
		private Vector2D _scale;							// Scale of the object.
		private Vector2D _axis;								// Axis of the object.
		private Vector2D _finalPosition = Vector2D.Zero;	// Final position.
		private Vector2D _finalScale = Vector2D.Unit;		// Final scale.
		private float _finalRotation = 0.0f;				// Final rotation.
		private Vector2D _parentPosition = Vector2D.Zero;	// Parent position.
		private Vector2D _parentScale = Vector2D.Zero;		// Parent scale.
		private float _parentRotation = 0;					// Parent rotation.
		private float _depth = 0.0f;						// Depth level of the renderable.
		private VertexTypeList.PositionDiffuse2DTexture1[] _vertices;	// Vertices for the object.
        #endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the parent position.
		/// </summary>
		protected Vector2D ParentPosition
		{
			get
			{
				return _parentPosition;
			}
			set
			{
				_parentPosition = value;
			}
		}

		/// <summary>
		/// Property to set or return the parent rotation.
		/// </summary>
		protected float ParentRotation
		{
			get
			{
				return _parentRotation;
			}
			set
			{
				_parentRotation = value;
			}
		}

		/// <summary>
		/// Property to set or return the parent scale.
		/// </summary>
		protected Vector2D ParentScale
		{
			get
			{
				return _parentScale;
			}
			set
			{
				_parentScale = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the size of the object has been updated.
		/// </summary>
		protected bool IsSizeUpdated
		{
			get
			{
				return _dimensionsChanged;
			}
			set
			{
				_dimensionsChanged = value;
			}
		}

		/// <summary>
		/// Property to set or return whether image properties have been updated.
		/// </summary>
		protected bool IsImageUpdated
		{
			get
			{
				return _imageCoordinatesChanged;
			}
			set
			{
				_imageCoordinatesChanged = value;
			}
		}

		/// <summary>
		/// Property to return the vertices for this object.
		/// </summary>
		protected VertexTypeList.PositionDiffuse2DTexture1[] Vertices
		{
			get
			{
				return _vertices;
			}
		}

		/// <summary>
		/// Property to set or return whether the AABB for the object needs updating.
		/// </summary>
		protected bool IsAABBUpdated
		{
			get
			{
				return _needAABBUpdate;
			}
			set
			{
				_needAABBUpdate = value;
			}
		}

		/// <summary>
		/// Property to return the size of the buffer that will hold the renderable contents.
		/// </summary>
		protected int BufferSize
		{
			get
			{
				return Geometry.VertexCount;
			}
		}

		/// <summary>
		/// Property to set or return the color of the border when the wrapping mode is set to Border.
		/// </summary>
		public Drawing.Color BorderColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to enable the use of the stencil buffer or not.
		/// </summary>
		public virtual bool StencilEnabled
		{
			get
			{
				if (_inheritUseStencil)
					return Gorgon.GlobalStateSettings.GlobalStencilEnabled;
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
		public virtual int StencilReference
		{
			get
			{
				if (_inheritStencilReference)
					return Gorgon.GlobalStateSettings.GlobalStencilReference;
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
		public virtual int StencilMask
		{
			get
			{
				if (_inheritStencilMask)
					return Gorgon.GlobalStateSettings.GlobalStencilMask;

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
		public virtual StencilOperations StencilPassOperation
		{
			get
			{
				if (_inheritStencilPassOperation)
					return Gorgon.GlobalStateSettings.GlobalStencilPassOperation;

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
		public virtual StencilOperations StencilFailOperation
		{
			get
			{
				if (_inheritStencilFailOperation)
					return Gorgon.GlobalStateSettings.GlobalStencilFailOperation;

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
		public virtual StencilOperations StencilZFailOperation
		{
			get
			{
				if (_inheritStencilZFailOperation)
					return Gorgon.GlobalStateSettings.GlobalStencilZFailOperation;

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
		public virtual CompareFunctions StencilCompare
		{
			get
			{
				if (_inheritStencilCompare)
					return Gorgon.GlobalStateSettings.GlobalStencilCompare;

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
		public virtual AlphaBlendOperation SourceBlend
		{
			get
			{
				if (_inheritBlending)
					return Gorgon.GlobalStateSettings.GlobalSourceBlend;
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
		public virtual AlphaBlendOperation DestinationBlend
		{
			get
			{
				if (_inheritBlending)
					return Gorgon.GlobalStateSettings.GlobalDestinationBlend;
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
        /// Property to set or return the source blending operation.
        /// </summary>
        public virtual AlphaBlendOperation SourceBlendAlpha
        {
            get
            {
                if (_inheritAlphaBlending)
                    return Gorgon.GlobalStateSettings.GlobalAlphaSourceBlend;
                else
                    return _sourceAlphaBlend;
            }
            set
            {
                _sourceAlphaBlend = value;
                _inheritAlphaBlending = false;
            }
        }

        /// <summary>
        /// Property to set or return the destination blending operation.
        /// </summary>
        public virtual AlphaBlendOperation DestinationBlendAlpha
        {
            get
            {
                if (_inheritAlphaBlending)
                    return Gorgon.GlobalStateSettings.GlobalAlphaDestinationBlend;
                else
                    return _destAlphaBlend;
            }
            set
            {
                _destAlphaBlend = value;
                _inheritAlphaBlending = false;
            }
        }

		/// <summary>
		/// Property to set or return the wrapping mode to use.
		/// </summary>
		public virtual ImageAddressing WrapMode
		{
			get
			{
				if (_inheritHorizontalWrap)
					return Gorgon.GlobalStateSettings.GlobalHorizontalWrapMode;
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
		public virtual ImageAddressing HorizontalWrapMode
		{
			get
			{
				if (_inheritHorizontalWrap)
					return Gorgon.GlobalStateSettings.GlobalHorizontalWrapMode;
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
		public virtual ImageAddressing VerticalWrapMode
		{
			get
			{
				if (_inheritVerticalWrap)
					return Gorgon.GlobalStateSettings.GlobalVerticalWrapMode;
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
		/// Property to set or return whether we inherit rotation from our parent renderable.
		/// </summary>
		public bool InheritRotation
		{
			get
			{
				return _inheritRotation;
			}
			set
			{
				_inheritRotation = value;
				_needParentUpdate = true;

				if (_parent == null)
					return;

				_parent.UpdateChildren();
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit scale from our parent renderable.
		/// </summary>
		public bool InheritScale
		{
			get
			{
				return _inheritScale;
			}
			set
			{

				_inheritScale = value;
				_needParentUpdate = true;

				if (_parent == null)
					return;

				_parent.UpdateChildren();
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit smoothing from the layer.
		/// </summary>
		public virtual bool InheritSmoothing
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
		public virtual bool InheritBlending
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
        /// Property to set or return whether we inherit alpha blending from the layer.
        /// </summary>
        public virtual bool InheritAlphaBlending
        {
            get
            {
                return _inheritAlphaBlending;
            }
            set
            {
                _inheritAlphaBlending = value;
            }
        }

		/// <summary>
		/// Property to set or return whether we inherit the alpha mask function from the layer.
		/// </summary>
		public virtual bool InheritAlphaMaskFunction
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
		public virtual bool InheritAlphaMaskValue
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
		public virtual bool InheritHorizontalWrapping
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
		public virtual bool InheritVerticalWrapping
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
		public virtual bool InheritStencilEnabled
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
		public virtual bool InheritStencilReference
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
		public virtual bool InheritStencilMask
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
		public virtual bool InheritStencilPassOperation
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
		public virtual bool InheritStencilFailOperation
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
		public virtual bool InheritStencilZFailOperation
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
		public virtual bool InheritStencilCompare
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
		/// Property to set or return whether we inherit the depth bias.
		/// </summary>
		public virtual bool InheritDepthBias
		{
			get
			{
				return _inheritDepthBias;
			}
			set
			{
				_inheritDepthBias = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the depth writing enabled flag.
		/// </summary>
		public virtual bool InheritDepthWriteEnabled
		{
			get
			{
				return _inheritDepthWriteEnable;
			}
			set
			{
				_inheritDepthWriteEnable = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the depth testing function.
		/// </summary>
		public virtual bool InheritDepthTestFunction
		{
			get
			{
				return _inheritDepthTestCompare;
			}
			set
			{
				_inheritDepthTestCompare = value;
			}
		}

		/// <summary>
		/// Property to set or return the type of smoothing for the sprites.
		/// </summary>
		public virtual Smoothing Smoothing
		{
			get
			{
				if (_inheritSmoothing)
					return Gorgon.GlobalStateSettings.GlobalSmoothing;
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
		public virtual CompareFunctions AlphaMaskFunction
		{
			get
			{
				if (_inheritMaskFunction)
					return Gorgon.GlobalStateSettings.GlobalAlphaMaskFunction;
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
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(int)), EditorMinMax(0, 255)]
		public virtual int AlphaMaskValue
		{
			get
			{
				if (_inheritMaskValue)
					return Gorgon.GlobalStateSettings.GlobalAlphaMaskValue;
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
		public virtual BlendingModes BlendingMode
		{
			get
			{
				if (_inheritBlending)
					return Gorgon.GlobalStateSettings.GlobalBlending;
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
		/// Property to set or return whether to enable the depth buffer (if applicable) writing or not.
		/// </summary>
		public virtual bool DepthWriteEnabled
		{
			get
			{
				if (_inheritDepthWriteEnable)
					return Gorgon.GlobalStateSettings.GlobalDepthWriteEnabled;
				else
					return _depthWriteEnabled;
			}
			set
			{
				_depthWriteEnabled = value;
				_inheritDepthWriteEnable = false;
			}
		}

		/// <summary>
		/// Property to set or return (if applicable) the depth buffer bias.
		/// </summary>
		public virtual float DepthBufferBias
		{
			get
			{
				if (_inheritDepthBias)
					return Gorgon.GlobalStateSettings.GlobalDepthBufferBias;
				else
					return _depthBias;
			}
			set
			{
				_depthBias = value;
				_inheritDepthBias = false;
			}
		}

		/// <summary>
		/// Property to set or return the depth buffer (if applicable) testing comparison function.
		/// </summary>
		public virtual CompareFunctions DepthTestFunction
		{
			get
			{
				if (_inheritDepthTestCompare)
					return Gorgon.GlobalStateSettings.GlobalDepthBufferTestFunction;
				else
					return _depthCompare;
			}
			set
			{
				_depthCompare = value;
				_inheritDepthTestCompare = false;
			}
		}

		/// <summary>
		/// Property to return the AABB for the object.
		/// </summary>
		public Drawing.RectangleF AABB
		{
			get
			{
				if (IsAABBUpdated)
					UpdateAABB();
				return _AABB;
			}
		}

		/// <summary>
		/// Property to set or return the offset within the source image to start drawing from.
		/// </summary>
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(Vector2D)), EditorRoundValues()]
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
		/// Property to set or return the depth of the renderable object.
		/// </summary>
		public virtual float Depth
		{
			get
			{
				return _depth;
			}
			set
			{
				if (value < 0.0f)
					value = 0.0f;
				if (value > 1.0f)
					value = 1.0f;

				_depth = value;
				_needAABBUpdate = true;
				_needParentUpdate = true;
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
        public Renderable Parent
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
		/// Property to set or return a uniform scale across the X and Y axis.
		/// </summary>
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(float))]
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
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(Drawing.Color))]
		public abstract Drawing.Color Color
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the opacity.
		/// </summary>		
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(int)), EditorMinMax(0, 255)]
		public abstract int Opacity
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the axis of the sprite.
		/// </summary>
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(Vector2D)), EditorCanDrag(EditorDragType.Axis)]
		public virtual Vector2D Axis
		{
			get
			{
				return _axis;
			}
			set
			{
				_axis = value;
			}
		}

		/// <summary>
		/// Property to set or return the scale of the sprite.
		/// </summary>
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(Vector2D))]
		public virtual Vector2D Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;
			}
		}

		/// <summary>
		/// Property to set or return the size of the object with scaling applied.
		/// </summary>
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(Vector2D))]
		public virtual Vector2D ScaledDimensions
		{
			get
			{
				return Vector2D.Multiply(_scale, _size);
			}
			set
			{
				Scale = Vector2D.Divide(value, _size);
			}
		}

		/// <summary>
		/// Property to set or return the scaled width of the object.
		/// </summary>
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(float))]
		public virtual float ScaledWidth
		{
			get
			{
				return _scale.X * _size.X;
			}
			set
			{
				SetScale(value / _size.X, _scale.Y);
			}
		}

		/// <summary>
		/// Property to set or return the scaled height of the object.
		/// </summary>
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(float))]
		public virtual float ScaledHeight
		{
			get
			{
				return _scale.Y * _size.Y;
			}
			set
			{
				SetScale(_scale.X, value / _size.Y);
			}
		}

		/// <summary>
		/// Property to set or return the image that this object is bound with.
		/// </summary>
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(Image), InterpolationMode.None)]
		public virtual Image Image
		{
			get
			{
				return _image;
			}
			set
			{
				_image = value;
				_imageCoordinatesChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the position of the object.
		/// </summary>
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(Vector2D)), EditorRoundValues(), EditorCanDrag(EditorDragType.Sprite)]
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
                    ((Renderable)this).UpdateChildren();
			}
		}

		/// <summary>
		/// Property to set or return the rotation angle in degrees.
		/// </summary>
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(float))]
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
                    ((Renderable)this).UpdateChildren();
			}
		}

		/// <summary>
		/// Property to set or return the size of the object.
		/// </summary>
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(Vector2D)), EditorRoundValues()]
		public virtual Vector2D Size
		{
			get
			{
				return _size;
			}
			set
			{
				// Do nothing if there's no change.
				if (value == _size)
					return;

				_size = value;
				_needAABBUpdate = true;
				_dimensionsChanged = true;
				_imageCoordinatesChanged = true;
			}
		}

		/// <summary>
		/// Property to set or return the width of the object.
		/// </summary>
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(float))]
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
		/// <remarks>Animations can be applied to this property.</remarks>
		[Animated(typeof(float))]
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


		/// <summary>
		/// Property to set or return the culling mode.
		/// </summary>
		CullingMode IRenderableStates.CullingMode
		{
			get
			{
				return CullingMode.Clockwise;
			}
			set
			{
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set the AABB for the object.
		/// </summary>
		/// <param name="min">Minimum coordinates for the AABB.</param>
		/// <param name="max">Maximum coordinates for the AABB.</param>
		protected void SetAABB(Vector2D min, Vector2D max)
		{
			_AABB.X = min.X;
			_AABB.Y = min.Y;
			_AABB.Width = max.X - min.X;
			_AABB.Height = max.Y - min.Y;
		}

		/// <summary>
		/// Function to set the AABB for the object.
		/// </summary>
		/// <param name="rect">The rectangle to use as the AABB.</param>
		protected void SetAABB(Drawing.RectangleF rect)
		{
			_AABB = rect;
		}

		/// <summary>
		/// Function to initialize the vertices for the object.
		/// </summary>
		/// <param name="vertexCount">Number of vertices for the object.</param>
		protected void InitializeVertices(int vertexCount)
		{
			if (vertexCount < 0)
				_vertices = null;
			else
				_vertices = new VertexTypeList.PositionDiffuse2DTexture1[vertexCount];
		}

		/// <summary>
		/// Function to notify that we need an update from the parent.
		/// </summary>
		protected void NotifyParent()
		{
			_needParentUpdate = true;
		}

		/// <summary>
		/// Function to return whether the specified image is currently set.
		/// </summary>
		/// <param name="image">Image to test.</param>
		/// <returns>TRUE if set, FALSE if not.</returns>
		protected bool IsImageCurrent(Image image)
		{
			return image == Gorgon.Renderer.GetImage(0);
		}

		/// <summary>
		/// Function to set the current image.
		/// </summary>
		/// <param name="image">Image to set.</param>
		protected void SetCurrentImage(Image image)
		{
			Gorgon.Renderer.SetImage(0, image);
		}

		/// <summary>
		/// Function to start the rendering process.
		/// </summary>
		/// <param name="flush">TRUE to flush the contents of the child renderable buffers immediately after rendering is complete, FALSE to hold the contents until the end of frame or state change.</param>
		/// <remarks>Setting flush to TRUE can slow the rendering process down significantly, only use when absolutely necessary.</remarks>
		protected void BeginRendering(bool flush)
		{
			Renderable child = null;								// Child object.
			bool stateChanged = false;								// Flag to indicate that the state has changed.
			int vertexCount = 0;									// Number of vertices to render.

			if (Children.Count > 0)
			{
				for (int i = 0; i < Children.Count; i++)
				{
					child = Children[i] as Renderable;
					if (child != null)
						child.Draw(flush);
				}
			}

			if (Vertices != null)
				vertexCount = Vertices.Length + Geometry.VerticesWritten;
			else
				vertexCount = Geometry.VerticesWritten;

			stateChanged = Gorgon.GlobalStateSettings.StateChanged(this, Image);
			if (((vertexCount >= Geometry.VertexCount) || (stateChanged)) && (Geometry.VerticesWritten != 0))
				Gorgon.Renderer.Render();

			// Apply animations.
			if (Animations.Count > 0)
				ApplyAnimations();

			// Set states.
			if (stateChanged)
			{
				Gorgon.GlobalStateSettings.SetStates(this);
				// Set the currently active image.
				Gorgon.Renderer.SetImage(0, Image);
			}
		}

		/// <summary>
		/// Function to flush queued data to the renderer.
		/// </summary>
		protected void FlushToRenderer()
		{
			Gorgon.Renderer.Render();
		}

		/// <summary>
		/// Function to write the vertex data to the buffer.
		/// </summary>
		/// <param name="start">Starting index.</param>
		/// <param name="length">Number of vertices.</param>
		/// <returns>The number of vertices written to the buffer thus far.</returns>
		protected int WriteVertexData(int start, int length)
		{
			if (length == 0)
				return 0;

			// Write the vertices.
			Geometry.VertexCache.WriteData(start, Geometry.VertexOffset + Geometry.VerticesWritten, length, Vertices);

			return Geometry.VerticesWritten;
		}

		/// <summary>
		/// Function to end the rendering process.
		/// </summary>
		/// <param name="flush">TRUE to flush the contents of the buffers immediately after rendering is complete, FALSE to hold the contents until the end of frame or state change.</param>
		/// <remarks>Setting flush to TRUE can slow the rendering process down significantly, only use when absolutely necessary.</remarks>
		protected void EndRendering(bool flush)
		{
			if (flush)
				Gorgon.Renderer.Render();
		}


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
				if (_inheritScale)
					_finalScale = Vector2D.Multiply(_scale, _parentScale);
				else
					_finalScale = _scale;

                // Get the rotation.
				if (_inheritRotation)
					_finalRotation = _parentRotation + _rotation;
				else
					_finalRotation = _rotation;

                // Update the translation.
                _finalPosition = Vector2D.Add(_finalPosition, _parentPosition);
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
        protected virtual void SetBlendMode(BlendingModes value)
		{
			switch (value & ~BlendingModes.ColorAdditive)
			{
				case BlendingModes.Additive:
					_sourceBlend = AlphaBlendOperation.SourceAlpha;
					_destBlend = AlphaBlendOperation.One;
					break;
				case BlendingModes.Color:
					_sourceBlend = AlphaBlendOperation.SourceColor;
					_destBlend = AlphaBlendOperation.DestinationColor;
					break;
				case BlendingModes.ModulatedInverse:
					_sourceBlend = AlphaBlendOperation.InverseSourceAlpha;
					_destBlend = AlphaBlendOperation.SourceAlpha;
					break;
				case BlendingModes.None:
					_sourceBlend = AlphaBlendOperation.One;
					_destBlend = AlphaBlendOperation.Zero;
					break;				
				case BlendingModes.Modulated:
					_sourceBlend = AlphaBlendOperation.SourceAlpha;
					_destBlend = AlphaBlendOperation.InverseSourceAlpha;
					break;
                case BlendingModes.PreMultiplied:
                    _sourceBlend = AlphaBlendOperation.One;
                    _destBlend = AlphaBlendOperation.InverseSourceAlpha;
                    break;
				case BlendingModes.Inverted:
					_sourceBlend = AlphaBlendOperation.InverseDestinationColor;
					_destBlend = AlphaBlendOperation.InverseSourceColor;
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
		/// Function to set the parent of this object.
		/// </summary>
		/// <param name="parent">Parent of the object.</param>
		protected internal void SetParent(Renderable parent)
		{
			_parent = parent;

			if (parent != null)
			{
				_needParentUpdate = true;
				parent.UpdateChildren();
			}
		}

		/// <summary>
		/// Function to return the number of vertices for this object.
		/// </summary>
		/// <returns>An array of vertices used for this renderable.</returns>
		protected internal abstract BatchVertex[] GetVertices();

		/// <summary>
		/// Function to update children.
		/// </summary>
		public void UpdateChildren()
		{
			if (_children.Count < 1)
				return;

			// Update children.
			for (int i = 0; i < _children.Count; i++)
			{
				_children[i]._needParentUpdate = true;

				_children[i]._parentPosition = FinalPosition;
				_children[i]._parentRotation = FinalRotation;
				_children[i]._parentScale = FinalScale;
				_children[i].UpdateChildren();
			}
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
		/// Function to force an update on the object.
		/// </summary>
		public virtual void Refresh()
		{
			_needAABBUpdate = true;
			_needParentUpdate = true;
			_imageCoordinatesChanged = true;
			_dimensionsChanged = true;
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
		protected Renderable(string name) 
			: base(name)
		{
			_size = new Vector2D(1.0f, 1.0f);
			_position = Vector2D.Zero;
			_rotation = 0.0f;
			_vertices = null;
			_dimensionsChanged = true;
			_imageCoordinatesChanged = true;
			BlendingMode = BlendingModes.Modulated;
			_maskValue = 1;
			_maskFunction = CompareFunctions.GreaterThan;
			_smoothing = Smoothing.None;
			_inheritBlending = true;
            _inheritAlphaBlending = true;
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
			_inheritRotation = true;
			_inheritScale = true;
			_inheritDepthWriteEnable = true;
			_inheritDepthBias = true;
			_inheritDepthTestCompare = true;
			BorderColor = Drawing.Color.Black;
			_wrapHMode = ImageAddressing.Clamp;
			_wrapVMode = ImageAddressing.Clamp;
			_stencilCompare = CompareFunctions.Always;
			_useStencil = false;
			_stencilPassOperation = StencilOperations.Keep;
			_stencilFailOperation = StencilOperations.Keep;
			_stencilZFailOperation = StencilOperations.Keep;
			_stencilReference = 0;
			_stencilMask = -1;
			_depthBias = 0.0f;
			_depthWriteEnabled = true;
			_depthCompare = CompareFunctions.LessThanOrEqual;
            _parent = null;
            _children = new RenderableChildren(this);
			_animations = new AnimationList(this);

            //Couldn't find where to put these, so this should work in the mean-time.
            //feel free to change or remove this comment if this is the correct place.
            _sourceAlphaBlend = AlphaBlendOperation.SourceAlpha;
            _destAlphaBlend = AlphaBlendOperation.InverseSourceAlpha;
			
			_axis = Vector2D.Zero;
			_AABB = new Drawing.RectangleF(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
		}
		#endregion

		#region ICloneable<T> Members
		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public abstract Renderable Clone();
		#endregion

		#region IAnimated Members
		/// <summary>
		/// Property to return a list of animations attached to the object.
		/// </summary>
		public AnimationList Animations
		{
			get
			{
				return _animations;
			}
		}

		/// <summary>
		/// Function to apply the current time in an animation to the objects animated properties.
		/// </summary>
		/// <remarks>This function will do the actual work of updating the properties (marked with the <see cref="GorgonLibrary.Graphics.AnimatedAttribute">Animated</see> attribute).
		/// It does this by applying the values at the current interpolated (or actual depending on time) key frame to the property that's bound to the track.</remarks>
		public void ApplyAnimations()
		{
			// Update animations.
			for (int i = 0; i < _animations.Count; i++)
				_animations[i].ApplyAnimation();
		}
		#endregion
	}
}
