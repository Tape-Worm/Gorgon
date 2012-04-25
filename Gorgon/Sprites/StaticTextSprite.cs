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
// Created: Wednesday, November 07, 2007 3:47:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a static text sprite.
	/// </summary>
	/// <remarks>This sprite type is used optimize text display by keeping the text static.  Using dynamic text in this object will greatly hinder performance.
	/// <para>This object is now obsolete and should not be used.  It will be removed in the next version.  Please use the <see cref="GorgonLibrary.Graphics.Batch">Batch</see> 
	/// object instead.</para></remarks>
	[Obsolete("This object is now obsolete and will be removed within the next version.  Please use the Batch object instead.")]
	public class StaticTextSprite
		: Renderable, IDisposable, IDeviceStateObject
	{
		#region Variables.
		private RenderImage _textImage = null;								// Target to send the text into.
		private TextSprite _textSprite = null;								// Sprite that will draw the text.
		private Sprite _displaySprite = null;								// Sprite that we will display.
		private bool _initialized = false;									// Initialize flag.
		private bool _disposed = false;										// Flag to indicate that the object is disposed.
		private Vector2D _maxImageSize = Vector2D.Zero;						// Maximum image size.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the number of spaces in a tab.
		/// </summary>
		public int TabSpaces
		{
			get
			{
				return _textSprite.TabSpaces;
			}
			set
			{
				if (value != _textSprite.TabSpaces)
				{
					_textSprite.TabSpaces = value;
					_initialized = false;
				}
			}
		}

		/// <summary>
		/// Property to return the number of lines for the text.
		/// </summary>
		public int LineCount
		{
			get
			{
				return _textSprite.LineCount;
			}
		}

        /// <summary>
        /// Property to show a shadow under the text.
        /// </summary>
        public bool Shadowed
        {
            get
            {
                return _textSprite.Shadowed;
            }
            set
            {
                if (_textSprite.Shadowed != value)
                {
                    _textSprite.Shadowed = value;
                    _initialized = false;
                }
            }
        }

        /// <summary>
        /// Property to set or return the color of the shadow.
        /// </summary>
        public Drawing.Color ShadowColor
        {
            get
            {
                return _textSprite.ShadowColor;
            }
            set
            {
                if (_textSprite.ShadowColor != value)
                {
                    _textSprite.ShadowColor = value;
                    _initialized = false;
                }
            }
        }

        /// <summary>
        /// Property to set or return the direction of the shadow.
        /// </summary>
        public FontShadowDirection ShadowDirection
        {
            get
            {
                return _textSprite.ShadowDirection;
            }
            set
            {
                if (_textSprite.ShadowDirection != value)
                {
                    _textSprite.ShadowDirection = value;
                    _initialized = false;
                }
            }
        }

        /// <summary>
        /// Property to set or return the offset between the shadow and the text.
        /// </summary>
        public Vector2D ShadowOffset
        {
            get
            {
                return _textSprite.ShadowOffset;
            }
            set
            {
                if (_textSprite.ShadowOffset != value)
                {
                    _textSprite.ShadowOffset = value;
                    _initialized = false;
                }
            }
        }

		/// <summary>
		/// Property to set or return whether this object will translate the CR/LF characters into a CR.
		/// </summary>
		public bool AutoAdjustCRLF
		{
			get
			{
				return _textSprite.AutoAdjustCRLF;
			}
			set
			{
				if (_textSprite.AutoAdjustCRLF != value)
				{
					_textSprite.AutoAdjustCRLF = value;
					_initialized = false;
				}
			}
		}

		/// <summary>
		/// Property to set or return a uniform scale across the X and Y axis.
		/// </summary>
		public override float UniformScale
		{
			get
			{
				return Scale.X;
			}
			set
			{
				if (value == 0.0f)
					return;

				// Set the uniform scale.
				Scale = new Vector2D(value, value);
				if (_displaySprite.Scale != Scale)
					_displaySprite.Scale = Scale;
			}
		}

		/// <summary>
		/// Property to return the primitive style for the object.
		/// </summary>
		public override PrimitiveStyle PrimitiveStyle
		{
			get
			{
				return PrimitiveStyle.TriangleList;
			}
		}

		/// <summary>
		/// Property to return whether to use an index buffer or not.
		/// </summary>
		public override bool UseIndices
		{
			get
			{
				return true;
			}
		}
	
		/// <summary>
		/// Property to set or return the image that this object is bound with.
		/// </summary>
		/// <value></value>
		public override Image Image
		{
			get
			{
				return _textSprite.Image;
			}
			set
			{
				// Do nothing.  This is read-only.
				throw new InvalidOperationException("The font image property is read-only.");				
			}
		}


		/// <summary>
		/// Property to set or return the sprite color.
		/// </summary>
		public override Drawing.Color Color
		{
			get
			{
				return _displaySprite.Color;
			}
			set
			{
				if (_displaySprite.Color != value)
					_displaySprite.Color = value;
			}
		}

		/// <summary>
		/// Property to set or return the opacity.
		/// </summary>
		public override int Opacity
		{
			get
			{
				return _displaySprite.Opacity;
			}
			set
			{
				if (_displaySprite.Opacity != value)
					_displaySprite.Opacity = value;
			}
		}

		/// <summary>
		/// Property to set or return whether word wrapping is enabled or disabled.
		/// </summary>
		public bool WordWrap
		{
			get
			{
				return _textSprite.WordWrap;
			}
			set
			{
				if (_textSprite.WordWrap != value)
				{
					_textSprite.WordWrap = value;
					_initialized = false;
				}
			}
		}

		/// <summary>
		/// Property to return the size of the text.
		/// </summary>
		public override Vector2D Size
		{
			get
			{
				return _textSprite.Size;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the horizontal alignment of the text.
		/// </summary>
		public Alignment Alignment
		{
			get
			{
				return _textSprite.Alignment;
			}
			set
			{
				if (_textSprite.Alignment != value)
				{
					_textSprite.Alignment = value;
					_initialized = false;
				}
			}
		}

		/// <summary>
		/// Property to set or return the maximum image buffer size.
		/// </summary>
		public Vector2D MaximumImageBufferSize
		{
			get
			{
				return _maxImageSize;
			}
			set
			{
				if (value.X < 1)
					value.X = Gorgon.CurrentDriver.MaximumTextureWidth;
				if (value.Y < 1)
					value.Y = Gorgon.CurrentDriver.MaximumTextureHeight;
				_maxImageSize = value;
				_initialized = false;
			}
		}

		/// <summary>
		/// Property to set or return the text for the buffer.
		/// </summary>
		/// <value></value>
		public string Text
		{
			get
			{
				return _textSprite.Text;
			}
			set
			{
				if (_textSprite.Text != value)
				{
					_textSprite.Text = value;
					_initialized = false;
				}
			}
		}

		/// <summary>
		/// Property to set or return the font.
		/// </summary>
		/// <value></value>
		public Font Font
		{
			get
			{
				return _textSprite.Font;
			}
			set
			{
				if (_textSprite.Font != value)
				{
					_textSprite.Font = value;
					_initialized = false;
				}
			}
		}

		/// <summary>
		/// Property to return the width of the text.
		/// </summary>
		public override float Width
		{
			get
			{
				return AABB.Width;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the height of the text.
		/// </summary>
		public override float Height
		{
			get
			{
				return AABB.Height;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the bounding rectangle for this object.
		/// </summary>
		/// <value></value>
		public Viewport Bounds
		{
			get
			{
				return _textSprite.Bounds;
			}
			set
			{
				if (_textSprite.Bounds != value)
				{
					_textSprite.Bounds = value;
					_initialized = false;
				}
			}
		}

		/// <summary>
		/// Property to return the length of the text.
		/// </summary>
		public int TextLength
		{
			get
			{
				return _textSprite.TextLength;
			}
		}

		/// <summary>
		/// Property to set or return the axis of the sprite.
		/// </summary>
		public override Vector2D Axis
		{
			get
			{
				return _displaySprite.Axis;
			}
			set
			{
				if (_displaySprite.Axis != value)
					_displaySprite.Axis = value;
			}
		}

		/// <summary>
		/// Property to set or return the position of the object.
		/// </summary>
		/// <value></value>
		public override Vector2D Position
		{
			get
			{
				return _displaySprite.Position;
			}
			set
			{
				if (_displaySprite.Position != value)
					_displaySprite.Position = value;
			}
		}

		/// <summary>
		/// Property to set or return the rotation angle in degrees.
		/// </summary>
		/// <value></value>
		public override float Rotation
		{
			get
			{
				return _displaySprite.Rotation;
			}
			set
			{
				if (_displaySprite.Rotation != value)
					_displaySprite.Rotation = value;
			}
		}

		/// <summary>
		/// Property to set or return the scale of the sprite.
		/// </summary>
		/// <value></value>
		public override Vector2D Scale
		{
			get
			{
				return _displaySprite.Scale;
			}
			set
			{
				if (_displaySprite.Scale != value)
					_displaySprite.Scale = value;
			}
		}

		/// <summary>
		/// Property to set or return the function used for alpha masking.
		/// </summary>
		/// <value></value>
		public override CompareFunctions AlphaMaskFunction
		{
			get
			{
				return _displaySprite.AlphaMaskFunction;
			}
			set
			{
				if (_displaySprite.AlphaMaskFunction != value)
					_displaySprite.AlphaMaskFunction = value;
			}
		}

		/// <summary>
		/// Property to set or return the alpha value used for alpha masking.
		/// </summary>
		/// <value></value>
		public override int AlphaMaskValue
		{
			get
			{
				return _displaySprite.AlphaMaskValue;
			}
			set
			{
				if (_displaySprite.AlphaMaskValue != value)
					_displaySprite.AlphaMaskValue = value;
			}
		}

		/// <summary>
		/// Property to set the blending mode.
		/// </summary>
		/// <value></value>
		public override BlendingModes BlendingMode
		{
			get
			{
				return _displaySprite.BlendingMode;
			}
			set
			{
				if (_displaySprite == null)
					return;

				if (_displaySprite.BlendingMode != value)
					_displaySprite.BlendingMode = value;
			}
		}

		/// <summary>
		/// Property to set or return the destination blending operation.
		/// </summary>
		/// <value></value>
		public override AlphaBlendOperation DestinationBlend
		{
			get
			{
				return _displaySprite.DestinationBlend;
			}
			set
			{
				if (_displaySprite.DestinationBlend != value)
					_displaySprite.DestinationBlend = value;
			}
		}

        /// <summary>
        /// Property to set or return the destination alpha blending operation.
        /// </summary>
        /// <value></value>
        public override AlphaBlendOperation DestinationBlendAlpha
        {
            get
            {
                return _displaySprite.DestinationBlendAlpha;
            }
            set
            {
                if (_displaySprite.DestinationBlendAlpha != value)
                    _displaySprite.DestinationBlendAlpha = value;
            }
        }

		/// <summary>
		/// Property to set or return the horizontal wrapping mode to use.
		/// </summary>
		/// <value></value>
		public override ImageAddressing HorizontalWrapMode
		{
			get
			{
				return _displaySprite.HorizontalWrapMode;
			}
			set
			{
				if (_displaySprite.HorizontalWrapMode != value)
					_displaySprite.HorizontalWrapMode = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the alpha mask function from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritAlphaMaskFunction
		{
			get
			{
				return _displaySprite.InheritAlphaMaskFunction;
			}
			set
			{
				_displaySprite.InheritAlphaMaskFunction = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the alpha mask value from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritAlphaMaskValue
		{
			get
			{
				return _displaySprite.InheritAlphaMaskValue;
			}
			set
			{
				_displaySprite.InheritAlphaMaskValue = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit blending from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritBlending
		{
			get
			{
				return _displaySprite.InheritBlending;
			}
			set
			{
				_displaySprite.InheritBlending = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the horizontal wrapping from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritHorizontalWrapping
		{
			get
			{
				return _displaySprite.InheritHorizontalWrapping;
			}
			set
			{
				_displaySprite.InheritHorizontalWrapping = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit smoothing from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritSmoothing
		{
			get
			{
				return _displaySprite.InheritSmoothing;
			}
			set
			{
				_displaySprite.InheritSmoothing = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil compare from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritStencilCompare
		{
			get
			{
				return _displaySprite.InheritStencilCompare;
			}
			set
			{
				_displaySprite.InheritStencilCompare = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil enabled flag from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritStencilEnabled
		{
			get
			{
				return _displaySprite.InheritStencilEnabled;
			}
			set
			{
				_displaySprite.InheritStencilEnabled = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil failed operation from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritStencilFailOperation
		{
			get
			{
				return _displaySprite.InheritStencilFailOperation;
			}
			set
			{
				_displaySprite.InheritStencilFailOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil mask from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritStencilMask
		{
			get
			{
				return _displaySprite.InheritStencilMask;
			}
			set
			{
				_displaySprite.InheritStencilMask = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil pass operation from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritStencilPassOperation
		{
			get
			{
				return _displaySprite.InheritStencilPassOperation;
			}
			set
			{
				_displaySprite.InheritStencilPassOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil reference from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritStencilReference
		{
			get
			{
				return _displaySprite.InheritStencilReference;
			}
			set
			{
				_displaySprite.InheritStencilReference = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil z-failed operation from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritStencilZFailOperation
		{
			get
			{
				return _displaySprite.InheritStencilZFailOperation;
			}
			set
			{
				_displaySprite.InheritStencilZFailOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the vertical wrapping from the layer.
		/// </summary>
		/// <value></value>
		public override bool InheritVerticalWrapping
		{
			get
			{
				return _displaySprite.InheritVerticalWrapping;
			}
			set
			{
				_displaySprite.InheritVerticalWrapping = value;
			}
		}

		/// <summary>
		/// Property to set or return the type of smoothing for the sprites.
		/// </summary>
		/// <value></value>
		public override Smoothing Smoothing
		{
			get
			{
				return _displaySprite.Smoothing;
			}
			set
			{
				if (_displaySprite.Smoothing != value)
					_displaySprite.Smoothing = value;
			}
		}

		/// <summary>
		/// Property to set or return the source blending operation.
		/// </summary>
		/// <value></value>
		public override AlphaBlendOperation SourceBlend
		{
			get
			{
				return _displaySprite.SourceBlend;
			}
			set
			{
				if (_displaySprite.SourceBlend != value)
					_displaySprite.SourceBlend = value;
			}
		}

        /// <summary>
        /// Property to set or return the source alpha blending operation.
        /// </summary>
        /// <value></value>
        public override AlphaBlendOperation SourceBlendAlpha
        {
            get
            {
                return _displaySprite.SourceBlendAlpha;
            }
            set
            {
                if (_displaySprite.SourceBlendAlpha != value)
                    _displaySprite.SourceBlendAlpha = value;
            }
        }

		/// <summary>
		/// Property to set or return the stencil comparison function.
		/// </summary>
		/// <value></value>
		public override CompareFunctions StencilCompare
		{
			get
			{
				return _displaySprite.StencilCompare;
			}
			set
			{
				if (_displaySprite.StencilCompare != value)
					_displaySprite.StencilCompare = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to enable the use of the stencil buffer or not.
		/// </summary>
		/// <value></value>
		public override bool StencilEnabled
		{
			get
			{
				return _displaySprite.StencilEnabled;
			}
			set
			{
				if (_displaySprite.StencilEnabled != value)
					_displaySprite.StencilEnabled = value;
			}
		}

		/// <summary>
		/// Property to set or return the operation for the failing stencil values.
		/// </summary>
		/// <value></value>
		public override StencilOperations StencilFailOperation
		{
			get
			{
				return _displaySprite.StencilFailOperation;
			}
			set
			{
				if (_displaySprite.StencilFailOperation != value)
					_displaySprite.StencilFailOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the mask value for the stencil buffer.
		/// </summary>
		/// <value></value>
		public override int StencilMask
		{
			get
			{
				return _displaySprite.StencilMask;
			}
			set
			{
				if (_displaySprite.StencilMask != value)
					_displaySprite.StencilMask = value;
			}
		}

		/// <summary>
		/// Property to set or return the operation for passing stencil values.
		/// </summary>
		/// <value></value>
		public override StencilOperations StencilPassOperation
		{
			get
			{
				return _displaySprite.StencilPassOperation;
			}
			set
			{
				if (_displaySprite.StencilPassOperation != value)
					_displaySprite.StencilPassOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the reference value for the stencil buffer.
		/// </summary>
		/// <value></value>
		public override int StencilReference
		{
			get
			{
				return _displaySprite.StencilReference;
			}
			set
			{
				if (_displaySprite.StencilReference != value)
					_displaySprite.StencilReference = value;
			}
		}

		/// <summary>
		/// Property to set or return the stencil operation for the failing depth values.
		/// </summary>
		/// <value></value>
		public override StencilOperations StencilZFailOperation
		{
			get
			{
				return _displaySprite.StencilZFailOperation;
			}
			set
			{
				if (_displaySprite.StencilZFailOperation != value)
					_displaySprite.StencilZFailOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the vertical wrapping mode to use.
		/// </summary>
		/// <value></value>
		public override ImageAddressing VerticalWrapMode
		{
			get
			{
				return _displaySprite.VerticalWrapMode;
			}
			set
			{
				if (_displaySprite.VerticalWrapMode != value)
					_displaySprite.VerticalWrapMode = value;
			}
		}

		/// <summary>
		/// Property to set or return the wrapping mode to use.
		/// </summary>
		/// <value></value>
		public override ImageAddressing WrapMode
		{
			get
			{
				return _displaySprite.WrapMode;
			}
			set
			{
				if (_displaySprite.WrapMode != value)
					_displaySprite.WrapMode = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the dimensions for an object.
		/// </summary>
		protected override void UpdateDimensions()
		{
			// Do nothing.
		}

		/// <summary>
		/// Function to return the number of vertices for this object.
		/// </summary>
		/// <returns>
		/// An array of vertices used for this renderable.
		/// </returns>
		protected internal override BatchVertex[] GetVertices()
		{
			return null;
		}

		/// <summary>
		/// Function to retrieve a line of text.
		/// </summary>
		/// <param name="line">Line index to for the text to retrieve.</param>
		/// <returns>The string at the line index.</returns>
		public string GetLine(int line)
		{
			return _textSprite.GetLine(line);
		}

		/// <summary>
		/// Function to append text to the already existing text.
		/// </summary>
		/// <param name="text">Text to append.</param>
		public void AppendText(string text)
		{
			_textSprite.AppendText(text);
			_initialized = false;
		}

		/// <summary>
		/// Function to insert text into the already existing text.
		/// </summary>
		/// <param name="position">Position to insert at.</param>
		/// <param name="text">Text to insert.</param>
		public void InsertText(int position, string text)
		{
			_textSprite.InsertText(position, text);
			_initialized = false;
		}

		/// <summary>
		/// Function to remove text from the already existing text.
		/// </summary>
		/// <param name="position">Position to start removing.</param>
		/// <param name="length">Number of characters to remove.</param>
		public void RemoveText(int position, int length)
		{
			_textSprite.RemoveText(position, length);
			_initialized = false;
		}

		/// <summary>
		/// Function to return the optimal dimensions of the specified text.
		/// </summary>
		/// <param name="text">Text to examine.</param>
		/// <param name="desiredWidth">Ideal width of the rectangle, only applies of wrap is set to TRUE.</param>
		/// <param name="wrap">TRUE to wrap text at the desired rectangle width, FALSE to let it continue.</param>
		/// <returns>Rectangle that will contain the text.</returns>
		public Drawing.RectangleF MeasureText(StringBuilder text, float desiredWidth, bool wrap)
		{
			return _textSprite.MeasureText(text, desiredWidth, wrap);
		}

		/// <summary>
		/// Function to update the AABB for the text.
		/// </summary>
		public override void UpdateAABB()
		{
			_displaySprite.UpdateAABB();
			SetAABB(_displaySprite.AABB);			
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public override Renderable Clone()
		{
			StaticTextSprite clone = null;			// Clone.

			clone = new StaticTextSprite(Name + ".Clone", Text, Font, Position, Color);
			clone._displaySprite = _displaySprite.Clone() as Sprite;
			clone._textSprite = _textSprite.Clone() as TextSprite;
			clone.MaximumImageBufferSize = _maxImageSize;

			return clone;
		}

		/// <summary>
		/// Function to get the color of a character vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to change.</param>
		/// <returns>Color of the sprite vertex.</returns>
		public Drawing.Color GetCharacterVertexColor(VertexLocations vertexPosition)
		{
			return _textSprite.GetCharacterVertexColor(vertexPosition);
		}

		/// <summary>
		/// Function to set the color of a character vertex.
		/// </summary>
		/// <param name="vertexPosition">Location of the vertex to change.</param>
		/// <param name="newColor">New color to set the vertex to.</param>
		public void SetCharacterVertexColor(VertexLocations vertexPosition, Drawing.Color newColor)
		{
			_textSprite.SetCharacterVertexColor(vertexPosition, newColor);
			_initialized = false;
		}

		/// <summary>
		/// Function to force an update on the object.
		/// </summary>
		public override void Refresh()
		{
			base.Refresh();
			_initialized = false;
			Initialize();
		}

		/// <summary>
		/// Function to initialize the static text sprite.
		/// </summary>
		public void Initialize()
		{
			Drawing.RectangleF imageSize = Drawing.RectangleF.Empty;		// Image size.
			RenderTarget lastTarget = null;									// Last render target.

			if (Text.Length < 1)
				return;

			try
			{
				_initialized = false;

				lastTarget = Gorgon.CurrentRenderTarget;

				if (_textImage != null)
					_textImage.Dispose();
				_textImage = null;

				// Create a target to send our data into.
				if (!_textSprite.HasBounds)
				{
					imageSize.Width = _textSprite.Width;
					imageSize.Height = _textSprite.Height;
				}
				else
				{
					imageSize.Width = _textSprite.Bounds.Width;
					imageSize.Height = _textSprite.Bounds.Height;
				}

				// Clip the size of the render target.
				if (imageSize.Width < 1.0f)
					imageSize.Width = 1.0f;
				if (imageSize.Height < 1.0f)
					imageSize.Height = 1.0f;

				// Don't go beyond the limits of the card.
				if (imageSize.Width > MaximumImageBufferSize.X)
					imageSize.Width = MaximumImageBufferSize.X;
				if (imageSize.Height > MaximumImageBufferSize.Y)
					imageSize.Height = MaximumImageBufferSize.Y;

				imageSize.Height = 64;
				_textImage = new RenderImage("@" + Name + ".TextSprite.RenderImage", (int)imageSize.Width, (int)imageSize.Height, ImageBufferFormats.BufferRGB888A8);

				Gorgon.CurrentRenderTarget = _textImage;

				_textImage.Clear(Drawing.Color.FromArgb(0, 0, 0, 0));

				// Reset any transformation.
				_textSprite.UpdateAABB();
				_textSprite.AlphaMaskFunction = CompareFunctions.Always;
				_textSprite.AlphaMaskValue = 0;
				_textSprite.BlendingMode = BlendingModes.Modulated;
				_textSprite.WrapMode = ImageAddressing.Clamp;				
				_textSprite.InheritStencilEnabled = false;
				_textSprite.Opacity = 255;
				_textSprite.UniformScale = 1.0f;
				// We need to draw this twice, if not, anti-aliased sprites get faded.
				_textSprite.Draw(true);

				// Text sprite.
				_displaySprite.Width = _textImage.Width;
				_displaySprite.Height = _textImage.Height;
				_displaySprite.Image = _textImage.Image;

				Gorgon.CurrentRenderTarget = lastTarget;

				UpdateAABB();

				_initialized = true;
			}
			finally
			{
				Gorgon.CurrentRenderTarget = lastTarget;
			}
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		/// <param name="flush">TRUE to flush the buffers after drawing, FALSE to only flush on state change.</param>
		public override void Draw(bool flush)
		{			
			if (!_initialized)
				Initialize();

			// Apply animations.
			if (Animations.Count > 0)
				((Renderable)this).ApplyAnimations();

			_displaySprite.Draw(flush);			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="StaticTextSprite"/> class.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="font">Font to use.</param>
		/// <param name="position">Initial position of the text.</param>
		/// <param name="textColor">Color of the</param>
		public StaticTextSprite(string name, string text, Font font, Vector2D position, Drawing.Color textColor)
			: base(name)
		{
			DeviceStateList.Add(this);
			_initialized = false;
			MaximumImageBufferSize = Vector2D.Zero;			
			_textSprite = new TextSprite(name + ".TextSprite", text, font, Drawing.Color.White);
			_displaySprite = new Sprite(name + ".DisplaySprite");

			SetAABB(Drawing.RectangleF.Empty);
			BlendingMode = BlendingModes.Modulated;
			Position = position;
			Color = textColor;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="font">Font to use.</param>
		/// <param name="position">Initial position of the text.</param>
		public StaticTextSprite(string name, string text, Font font, Vector2D position)
			: this(name, text, font, position, Drawing.Color.White)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="font">Font to use.</param>
		public StaticTextSprite(string name, string text, Font font)
			: this(name, text, font, Vector2D.Zero, Drawing.Color.White)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="font">Font to use.</param>
		/// <param name="color">Color of the text.</param>
		public StaticTextSprite(string name, string text, Font font, Drawing.Color color)
			: this(name, text, font, Vector2D.Zero, color)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="font">Font to use.</param>
		/// <param name="positionX">Horizontal position of the text.</param>
		/// <param name="positionY">Vertical position of the text.</param>
		/// <param name="textColor">Color of the</param>
		public StaticTextSprite(string name, string text, Font font, float positionX, float positionY, Drawing.Color textColor)
			: this(name, text, font, new Vector2D(positionX, positionY), textColor)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of the text sprite.</param>
		/// <param name="text">Text for the sprite.</param>
		/// <param name="font">Font to use.</param>
		/// <param name="positionX">Horizontal position of the text.</param>
		/// <param name="positionY">Vertical position of the text.</param>		
		public StaticTextSprite(string name, string text, Font font, float positionX, float positionY)
			: this(name, text, font, new Vector2D(positionX, positionY), Drawing.Color.White)
		{
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)			
			{
				if (disposing)
				{
					if (_textImage != null)
						_textImage.Dispose();

					DeviceStateList.Remove(this);
				}
				_textImage = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region IDeviceStateObject Members
		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public void DeviceLost()
		{
			DeviceReset();
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public void DeviceReset()
		{
			_initialized = false;
		}

		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		public void ForceRelease()
		{
			DeviceReset();
		}
		#endregion
	}
}
