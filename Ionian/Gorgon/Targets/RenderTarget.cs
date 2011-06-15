#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Tuesday, August 02, 2005 11:53:31 AM
// 
#endregion

using System;
using System.Windows.Forms;
using Drawing = System.Drawing;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object for render targets.
	/// </summary>
	/// <remarks>
	/// The rendering display can be redirected into a texture rather than the back
	/// buffer, you can also redirect the display to another window via swap chains.  
	/// This is accomplished through the use of render targets.  
	/// </remarks>
	public abstract class RenderTarget 
		: NamedObject, IDisposable, IDeviceStateObject, IRenderableStates
	{
		#region Variables.
		private bool _inheritClearTargets;					// Flag to indicate that we will inherit the clear targets from Gorgon.
		private RenderTarget _lastTarget;					// Last active render target.
		private BlendingModes _blending;					// Blending mode for drawing.
		private Smoothing _smoothing;						// Smoothing mode for drawing.
		private CompareFunctions _alphaFunction;			// Alpha comparison function.
		private int _alphaMaskValue;						// Mask value for comparison.
		private PrimitiveStyle _currentPrimitiveStyle;		// Current primitive style.
		private bool _useIndices;							// Flag to indicate whether to use an index buffer or not.
		private Image _drawingPattern;						// Image pattern to use for drawing.
		private Vector2D _patternOffset = Vector2D.Zero;	// Drawing pattern offset.
		private bool _setOnce;								// Flag to set the states only once.
		private bool _statesSet;							// Flag to determine if the states have been set.
		private ImageAddressing _wrapHMode;					// Horizontal image wrapping mode.
		private ImageAddressing _wrapVMode;					// Horizontal image wrapping mode.
		private AlphaBlendOperation _sourceBlend;			// Source blending operation.
		private AlphaBlendOperation _destBlend;				// Destination blending operation.
		private StencilOperations _stencilPassOperation;	// Stencil pass operation.
		private StencilOperations _stencilFailOperation;	// Stencil fail operation.
		private StencilOperations _stencilZFailOperation;	// Stencil Z fail operation.
		private CompareFunctions _stencilCompare;			// Stencil compare operation.
		private float _primitiveDepth = 0.0f;				// Depth of the primitive.
		private int _stencilReference;						// Stencil reference value.
		private int _stencilMask;							// Stencil mask value.
		private bool _useStencil;							// Flag to indicate whether to use the stencil or not.
		private int _width;									// Width of render target.
		private int _height;								// Height of render target.
		private bool _useDepthBuffer;						// Flag to indicate that we want to use a depth buffer.
		private bool _useStencilBuffer;						// Flag to indicate that we want to use a stencil buffer.
		private Viewport _defaultView;						// Default window.		
		private Drawing.Color _backColor;					// Default background color of the render target.
		private ClearTargets _clearTargets;					// Flags to indicate what to clear per frame.		
		private D3D9.Surface _colorBuffer;					// Color buffer from the render target.
		private D3D9.Surface _depthBuffer;					// Depth buffer to use.
		private D3D9.Surface _convertBuffer;				// Conversion buffer.
		private float _depthBias;							// Depth bias.
		private bool _depthWriteEnabled;					// Depth writing enabled flag.
		private CompareFunctions _depthCompare;				// Depth test comparison function.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return a z buffer for a swap chain.
		/// </summary>
		internal D3D9.Surface DepthBuffer
		{
			get
			{
				return _depthBuffer;
			}
		}

		/// <summary>
		/// Property to return the surface for this render target.
		/// </summary>
		internal D3D9.Surface SurfaceBuffer
		{
			get
			{
				return _colorBuffer;
			}
		}

		/// <summary>
		/// Property to return whether this render target is valid for post pixel shader blending.
		/// </summary>
		/// <remarks>
		/// If the driver supports post pixel shader blending of render targets (<see cref="GorgonLibrary.Driver.SupportMRTPostPixelShaderBlending">Driver.SupportMRTPostPixelShaderBlending</see> = True) 
		/// then this property needs to be queried to find out if the particular render target can support post pixel shader blending.
		/// </remarks>
		public abstract bool IsValidForMRTPostPixelShaderBlending
		{
			get;
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
		/// Property to set or return the image used as the pattern for drawing.
		/// </summary>
		public Image DrawingPattern
		{
			get
			{
				return _drawingPattern;
			}
			set
			{
				_drawingPattern = value;
			}
		}

		/// <summary>
		/// Property to set or return the offset of the drawing pattern.
		/// </summary>
		public Vector2D DrawingPatternOffset
		{
			get
			{
				return _patternOffset;
			}
			set
			{
				_patternOffset = value;
			}
		}

		/// <summary>
		/// Property to set or return flags that indicate what should be cleared per frame.
		/// </summary>
		public virtual ClearTargets ClearEachFrame
		{
			get
			{
				if (_inheritClearTargets)
					return Gorgon.ClearEachFrame;

				return _clearTargets;
			}
			set
			{
				_inheritClearTargets = false;
				_clearTargets = value;
			}
		}

		/// <summary>
		/// Property to set or return the active window.
		/// </summary>
		public Viewport DefaultView
		{
			get
			{
				return _defaultView;
			}
		}

		/// <summary>
		/// Property to return the projection matrix.
		/// </summary>
		public Matrix ProjectionMatrix
		{
			get
			{
				return _defaultView.ProjectionMatrix;
			}
		}

		/// <summary>
		/// Property to set or return whether this render target will inherit the per frame clearing settings from Gorgon.
		/// </summary>
		public bool InheritClearFlags
		{
			get
			{
				return _inheritClearTargets;
			}
			set
			{
				_inheritClearTargets = value;
				if (value)
					_clearTargets = Gorgon.ClearEachFrame;
			}
		}

		/// <summary>
		/// Property to set or return whether we want to use a depth buffer or not.
		/// </summary>
		public virtual bool UseDepthBuffer
		{
			get
			{
				return _useDepthBuffer;
			}
			set
			{
				if (!value)
					_useStencilBuffer = false;
				_useDepthBuffer = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we want to use a stencil buffer or not.
		/// </summary>
		public virtual bool UseStencilBuffer
		{
			get
			{
				return _useStencilBuffer;
			}
			set
			{
				if (!_useDepthBuffer)
					value = false;
				_useStencilBuffer = value;
			}
		}

		/// <summary>
		/// Property to set or return the width of the render target.
		/// </summary>
		public virtual int Width
		{
			get
			{
				return _width;
			}
			set
			{
				_width = value;
			}
		}

		/// <summary>
		/// Property to set or return the height of the render target.
		/// </summary>
		public virtual int Height
		{
			get
			{
				return _height;
			}
			set
			{
				_height = value;
			}
		}

		/// <summary>
		/// Property to get or set the default background color for the render target.
		/// </summary>
		public Drawing.Color BackgroundColor
		{
			get
			{
				return _backColor;
			}
			set
			{
				_backColor = value;
			}
		}

		/// <summary>
		/// Property to set or return the depth value for primitives being drawn.
		/// </summary>
		public float PrimitiveDepth
		{
			get
			{
				return _primitiveDepth;
			}
			set
			{
				if (value < 0.0f)
					value = 0.0f;
				if (value > 1.0f)
					value = 1.0f;
				_primitiveDepth = value;
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
		/// Function to perform the actual drawing on the target.
		/// </summary>
		/// <param name="vertices">Vertices to draw.</param>
		private void Draw(VertexTypeList.PositionDiffuse2DTexture1[] vertices)
		{
			SpriteStateCache manager = null;	// State manager.
			int count = 0;						// Vertex count.
			bool stateChanged = false;			// Flag to indicate a state change.

			manager = Gorgon.GlobalStateSettings;
			count = Geometry.VertexCount;

			stateChanged = manager.StateChanged(this, _drawingPattern);

			// If we're at the end of the buffer, wrap around.
			if (((Geometry.VerticesWritten + vertices.Length >= count) || (stateChanged)) && (Geometry.VerticesWritten != 0))
				Gorgon.Renderer.Render();

			// Set the state for this sprite.
			if ((stateChanged) && (((_setOnce) && (!_statesSet)) || (!_setOnce)))
			{
				manager.SetStates(this);
				_statesSet = true;

				// Set the currently active image.
                Gorgon.Renderer.SetImage(0, _drawingPattern);
			}            

			// Write the data to the buffer.			
			Geometry.VertexCache.WriteData(0, Geometry.VertexOffset + Geometry.VerticesWritten, vertices.Length, vertices);
		}

		/// <summary>
		/// Function to perform the conversion.
		/// </summary>
		/// <param name="image">Image to receive the render target data.</param>
		/// <param name="targetFormat">Format of the render target.</param>
		protected void ConvertToImageData(Image image, ImageBufferFormats targetFormat)
		{
			D3D9.Surface imageSurface = null;			// Image surface.
			Drawing.Rectangle sourceRect;				// Source rectangle.

			try
			{
				if (image == null)
					throw new ArgumentNullException("image");
				if ((image.ImageType != ImageType.Dynamic) && (image.ImageType != ImageType.RenderTarget))
					throw new GorgonException(GorgonErrors.CannotUpdate, "The destination image is not a dynamic image or a render target.");
				if (image.Format != targetFormat)
					throw new GorgonException(GorgonErrors.CannotUpdate, "The destination image format does not match that of the render target.");

				// Flush the render buffer.
				if (Gorgon.IsRunning)
					Gorgon.Renderer.Render();

				// Create a conversion buffer if necessary.
				if ((_convertBuffer == null) || (_convertBuffer.Description.Width != Width) || (_convertBuffer.Description.Height != Height) || (Converter.Convert(targetFormat) != _convertBuffer.Description.Format))
				{
					if (_convertBuffer != null)
						_convertBuffer.Dispose();
					_convertBuffer = D3D9.Surface.CreateOffscreenPlain(Gorgon.Screen.Device, Width, Height, Converter.Convert(targetFormat), D3D9.Pool.SystemMemory);
				}

				Gorgon.Screen.Device.GetRenderTargetData(SurfaceBuffer, _convertBuffer);
				sourceRect = new Drawing.Rectangle(0, 0, Width, Height);

				if (sourceRect.Width > image.Width)
					sourceRect.Width = image.Width;
				if (sourceRect.Height > image.Height)
					sourceRect.Height = image.Height;

				// Get the surface data.
				imageSurface = image.D3DTexture.GetSurfaceLevel(0);

				// Send the render target data to the surface.
				Gorgon.Screen.Device.UpdateSurface(_convertBuffer, sourceRect, imageSurface, Drawing.Point.Empty);
			}
			finally
			{
				if (imageSurface != null)
					imageSurface.Dispose();
				imageSurface = null;
			}
		}

		/// <summary>
		/// Function to set the color buffer.
		/// </summary>
		/// <param name="surface">The buffer to use.</param>
		protected void SetColorBuffer(D3D9.Surface surface)
		{
			if (_colorBuffer != null)
				_colorBuffer.Dispose();
			_colorBuffer = surface;
		}

		/// <summary>
		/// Function to set the depth buffer.
		/// </summary>
		/// <param name="depthBuffer">The buffer to use.</param>
		protected void SetDepthBuffer(D3D9.Surface depthBuffer)
		{
			if (_depthBuffer != null)
				_depthBuffer.Dispose();
			_depthBuffer = depthBuffer;
		}

		/// <summary>
		/// Function to set the blending modes.
		/// </summary>
		/// <param name="value">Blending value.</param>
		protected virtual void SetBlendMode(BlendingModes value)
		{
			if (value == BlendingModes.PreMultiplied)
				return;

			switch (value)
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
		/// Function to return a viable stencil/depth buffer.
		/// </summary>
		/// <param name="usestencil">TRUE to use a stencil, FALSE to exclude.</param>
		/// <param name="usedepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		protected abstract void UpdateDepthStencilFormat(bool usestencil, bool usedepth);

		/// <summary>
		/// Function to set this render target and its default window as currently active.
		/// </summary>
		protected internal void SetActive()
		{
			_lastTarget = Gorgon.CurrentRenderTarget;
			Gorgon.CurrentRenderTarget = this;
		}

		/// <summary>
		/// Function to reset the target/viewport to the previous view and target.
		/// </summary>
		protected internal void RestoreActive()
		{
			Gorgon.CurrentRenderTarget = _lastTarget;
			_lastTarget = null;
		}

		/// <summary>
		/// Function to lock a layer so that we can begin drawing.
		/// </summary>
		public void BeginDrawing()
		{
			// This doesn't mean anything on the same render target.
			if (Gorgon.IsRenderTargetActive(this))
				return;

			if (_lastTarget == null)
			{
				// Flush rendering before changing.
				Gorgon.Renderer.Render();

				// Set this target as the active target.
				SetActive();
			}
		}

		/// <summary>
		/// Function to end the lock on the layer.
		/// </summary>
		public void EndDrawing()
		{
			// Dump data to the target.
			Gorgon.Renderer.Render();

			if (_lastTarget == null)
				return;

			// Restore the previous target.
			RestoreActive();
		}

		/// <summary>
		/// Function to draw a single point.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="color">Color for the line.</param>
		/// <param name="penWidth">Width of the point.</param>
		/// <param name="penHeight">Height of the point.</param>
		public void SetPoint(float x, float y, Drawing.Color color, float penWidth, float penHeight)
		{
			// Vertex to represent the line.
			VertexTypeList.PositionDiffuse2DTexture1[] pointVertex = new VertexTypeList.PositionDiffuse2DTexture1[1];

			if ((penWidth <= 1.0f) && (penHeight <= 1.0f))
			{
				// Build end points.
				pointVertex[0].Position.X = (int)x;
				pointVertex[0].Position.Y = (int)y;
				pointVertex[0].Position.Z = -PrimitiveDepth;
                pointVertex[0].ColorValue = color.ToArgb();

				if (_drawingPattern == null)
				{
					pointVertex[0].TextureCoordinates.X = 0.0f;
					pointVertex[0].TextureCoordinates.Y = 0.0f;
				}
				else
				{
					pointVertex[0].TextureCoordinates.X = (x + _patternOffset.X - 0.5f) / (float)_drawingPattern.ActualWidth;
					pointVertex[0].TextureCoordinates.Y = (y + _patternOffset.Y - 0.5f) / (float)_drawingPattern.ActualHeight;
				}

				// Set the current primitive style.
				_currentPrimitiveStyle = PrimitiveStyle.PointList;
				_useIndices = false;

				// Send to the renderer.
				Draw(pointVertex);
			}
			else
			{
				float centerX, centerY;		// Center position for the point.

				centerX = penWidth / 2.0f;
				centerY = penHeight / 2.0f;

				// Draw a rectangle.
				FilledRectangle(x - centerX, y - centerY, penWidth, penHeight, color);
			}
		}

		/// <summary>
		/// Function to draw a single point.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="color">Color for the line.</param>
		/// <param name="penSize">Size of the point.</param>
		public void SetPoint(float x, float y, Drawing.Color color, Vector2D penSize)
		{
			SetPoint(x, y, color, penSize.X, penSize.Y);
		}

		/// <summary>
		/// Function to draw a single point.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="color">Color for the line.</param>
		public void SetPoint(float x, float y, Drawing.Color color)
		{
			SetPoint(x, y, color, 1.0f, 1.0f);
		}
		
		/// <summary>
		/// Function to draw a line.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="width">Width of the line.</param>
		/// <param name="height">Height of the line.</param>
		/// <param name="color">Color for the line.</param>
		/// <param name="penWidth">Width of the point.</param>
		/// <param name="penHeight">Height of the point.</param>
		/// <remarks>Taken from: SDL_gfx Written by Andreas Schiffler</remarks>		
		public void Line(float x, float y, float width, float height, Drawing.Color color, float penWidth, float penHeight)
		{
			int i;				// Loop.
			int numPixels;		// Number of pixels.
			float dx;			// Delta X.
			float dy;			// Delta Y.
			float x1,x2;		// X start/endpoint position.
			float y1,y2;		// Y start/endpoint position.
			float xinc1;		// X increment bias.
			float yinc1;		// Y increment bias.
			float xinc2;		// X increment bias.
			float yinc2;		// Y increment bias.
			float d;			// Delta position.
			float dinc1;		// Delta increment bias.
			float dinc2;		// Delta increment bias.			

			// Calculate deltas.
			if (width < 0)
				width += 1.0f;
			if (width > 0)
				width -= 1.0f;
			if (height < 0)
				height += 1.0f;
			if (height > 0)
				height -= 1.0f;

			x1 = x;
			y1 = y;
			x2 = x + width;
			y2 = y + height;

			dx = MathUtility.Abs(x2 - x1);
			dy = MathUtility.Abs(y2 - y1);

			if (dx >= dy)
			{
				numPixels = (int)dx + 1;
				d = (2 * dy) - dx;
				dinc1 = dy * 2;
				dinc2 = (dy - dx) * 2;
				xinc1 = 1;
				xinc2 = 1;
				yinc1 = 0;
				yinc2 = 1;
			}
			else
			{
				numPixels = (int)dy + 1;
				d = (2 * dx) - dy;
				dinc1 = dx * 2;
				dinc2 = (dx - dy) * 2;
				xinc1 = 0;
				xinc2 = 1;
				yinc1 = 1;
				yinc2 = 1;
			}

			if (x > x2)
			{
				xinc1 = -xinc1;
				xinc2 = -xinc2;
			}
			if (y > y2)
			{
				yinc1 = -yinc1;
				yinc2 = -yinc2;
			}

			x1 = x;
			y1 = y;

			_setOnce = true;
			_statesSet = false;
			// Loop through and draw line.
			for (i = 1; i <= numPixels; i++)
			{
				SetPoint(x1, y1, color, penWidth, penHeight);

				if (d < 0)
				{
					d += dinc1;
					x1 += xinc1;
					y1 += yinc1;
				}
				else
				{
					d += dinc2;
					x1 += xinc2;
					y1 += yinc2;
				}
			}
			_setOnce = false;
		}

		/// <summary>
		/// Function to draw a line.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="width">Width of the line.</param>
		/// <param name="height">Height of the line.</param>
		/// <param name="color">Color for the line.</param>
		/// <param name="penSize">Size of the pen.</param>		
		public void Line(float x, float y, float width, float height, Drawing.Color color, Vector2D penSize)
		{
			Line(x, y, width, height, color, penSize.X, penSize.Y);
		}

		/// <summary>
		/// Function to draw a line.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="width">Width of the line.</param>
		/// <param name="height">Height of the line.</param>
		/// <param name="color">Color for the line.</param>
		public void Line(float x, float y, float width, float height, Drawing.Color color)
		{
			Line(x, y, width, height, color, 1.0f, 1.0f);
		}

		/// <summary>
		/// Function to draw an unfilled rectangle.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="width">Width of the rectangle.</param>
		/// <param name="height">Height of the rectangle.</param>
		/// <param name="color">Color for the rectangle.</param>
		public void Rectangle(float x, float y, float width, float height, Drawing.Color color)
		{
			// Vertex to represent the line.
			VertexTypeList.PositionDiffuse2DTexture1[] rectVertex = new VertexTypeList.PositionDiffuse2DTexture1[8];

			// Build end points.
			rectVertex[0].Position.X = x;
			rectVertex[0].Position.Y = y;
			rectVertex[0].Position.Z = -PrimitiveDepth;

			rectVertex[1].Position.X = x + width - 1;
			rectVertex[1].Position.Y = y;
			rectVertex[1].Position.Z = -PrimitiveDepth;

			rectVertex[2].Position.X = x + width - 1;
			rectVertex[2].Position.Y = y;
			rectVertex[2].Position.Z = -PrimitiveDepth;

			rectVertex[3].Position.X = x + width - 1;
			rectVertex[3].Position.Y = y + height - 1;
			rectVertex[3].Position.Z = -PrimitiveDepth;

			rectVertex[4].Position.X = x + width - 1;
			rectVertex[4].Position.Y = y + height - 1;
			rectVertex[4].Position.Z = -PrimitiveDepth;

			rectVertex[5].Position.X = x;
			rectVertex[5].Position.Y = y + height - 1;
			rectVertex[5].Position.Z = -PrimitiveDepth;

			rectVertex[6].Position.X = x;
			rectVertex[6].Position.Y = y + height - 1;
			rectVertex[6].Position.Z = -PrimitiveDepth;

			rectVertex[7].Position.X = x;
			rectVertex[7].Position.Y = y;
			rectVertex[7].Position.Z = -PrimitiveDepth;

			rectVertex[0].ColorValue = color.ToArgb();
			rectVertex[1].ColorValue = color.ToArgb();
			rectVertex[2].ColorValue = color.ToArgb();
			rectVertex[3].ColorValue = color.ToArgb();
			rectVertex[4].ColorValue = color.ToArgb();
			rectVertex[5].ColorValue = color.ToArgb();
			rectVertex[6].ColorValue = color.ToArgb();
			rectVertex[7].ColorValue = color.ToArgb();

			if (_drawingPattern == null)
			{
				rectVertex[0].TextureCoordinates.X = 0.0f;
				rectVertex[0].TextureCoordinates.Y = 0.0f;
				rectVertex[1].TextureCoordinates.X = 0.0f;
				rectVertex[1].TextureCoordinates.Y = 0.0f;
				rectVertex[2].TextureCoordinates.X = 0.0f;
				rectVertex[2].TextureCoordinates.Y = 0.0f;
				rectVertex[3].TextureCoordinates.X = 0.0f;
				rectVertex[3].TextureCoordinates.Y = 0.0f;
				rectVertex[4].TextureCoordinates.X = 0.0f;
				rectVertex[4].TextureCoordinates.Y = 0.0f;
				rectVertex[5].TextureCoordinates.X = 0.0f;
				rectVertex[5].TextureCoordinates.Y = 0.0f;
				rectVertex[6].TextureCoordinates.X = 0.0f;
				rectVertex[6].TextureCoordinates.Y = 0.0f;
				rectVertex[7].TextureCoordinates.X = 0.0f;
				rectVertex[7].TextureCoordinates.Y = 0.0f;
			}
			else
			{
				float tu1, tu2, tv1, tv2;		// texture coordinates.
				tu1 = (x + _patternOffset.X - 0.5f) / (float)_drawingPattern.ActualWidth;
				tu2 = (x + _patternOffset.X + width - 0.5f) / (float)_drawingPattern.ActualWidth;
				tv1 = (y + _patternOffset.Y - 0.5f) / (float)_drawingPattern.ActualHeight;
				tv2 = (y + _patternOffset.Y + height - 0.5f) / (float)_drawingPattern.ActualHeight;
				rectVertex[0].TextureCoordinates.X = tu1;
				rectVertex[0].TextureCoordinates.Y = tv1;

				rectVertex[1].TextureCoordinates.X = tu2;
				rectVertex[1].TextureCoordinates.Y = tv1;

				rectVertex[2].TextureCoordinates.X = tu2;
				rectVertex[2].TextureCoordinates.Y = tv1;

				rectVertex[3].TextureCoordinates.X = tu2;
				rectVertex[3].TextureCoordinates.Y = tv2;

				rectVertex[4].TextureCoordinates.X = tu2;
				rectVertex[4].TextureCoordinates.Y = tv2;

				rectVertex[5].TextureCoordinates.X = tu1;
				rectVertex[5].TextureCoordinates.Y = tv2;

				rectVertex[6].TextureCoordinates.X = tu1;
				rectVertex[6].TextureCoordinates.Y = tv2;

				rectVertex[7].TextureCoordinates.X = tu1;
				rectVertex[7].TextureCoordinates.Y = tv1;
			}
			_useIndices = false;

			// Set the current primitive style.
			_currentPrimitiveStyle = PrimitiveStyle.LineList;

			// Send to the renderer.
			Draw(rectVertex);
		}

		/// <summary>
		/// Function to draw a filled rectangle.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="width">Width of the rectangle.</param>
		/// <param name="height">Height of the rectangle.</param>
		/// <param name="color">Color for the rectangle.</param>
		public void FilledRectangle(float x, float y, float width, float height, Drawing.Color color)
		{
			// Vertex to represent the line.
			VertexTypeList.PositionDiffuse2DTexture1[] lineVertex = new VertexTypeList.PositionDiffuse2DTexture1[4];

			// Build end points.
			lineVertex[0].Position.X = (int)x;
			lineVertex[0].Position.Y = (int)y;
			lineVertex[0].Position.Z = -PrimitiveDepth;
			lineVertex[1].Position.X = (int)(x + width);
			lineVertex[1].Position.Y = (int)y;
			lineVertex[1].Position.Z = -PrimitiveDepth;
			lineVertex[2].Position.X = (int)(x + width);
			lineVertex[2].Position.Y = (int)(y + height);
			lineVertex[2].Position.Z = -PrimitiveDepth;
			lineVertex[3].Position.X = (int)x;
			lineVertex[3].Position.Y = (int)(y + height);
			lineVertex[3].Position.Z = -PrimitiveDepth;

			lineVertex[0].ColorValue = color.ToArgb();
			lineVertex[1].ColorValue = color.ToArgb();
			lineVertex[2].ColorValue = color.ToArgb();
			lineVertex[3].ColorValue = color.ToArgb();

			if (_drawingPattern == null)
			{
				lineVertex[0].TextureCoordinates.X = 0.0f;
				lineVertex[0].TextureCoordinates.Y = 0.0f;
				lineVertex[1].TextureCoordinates.X = 0.0f;
				lineVertex[1].TextureCoordinates.Y = 0.0f;
				lineVertex[2].TextureCoordinates.X = 0.0f;
				lineVertex[2].TextureCoordinates.Y = 0.0f;
				lineVertex[3].TextureCoordinates.X = 0.0f;
				lineVertex[3].TextureCoordinates.Y = 0.0f;
			}
			else
			{
				float tu1, tu2, tv1, tv2;		// texture coordinates.
				tu1 = (x + _patternOffset.X - 0.5f) / (float)_drawingPattern.ActualWidth;
				tu2 = (x + _patternOffset.X + width - 0.5f) / (float)_drawingPattern.ActualWidth;
				tv1 = (y + _patternOffset.Y - 0.5f) / (float)_drawingPattern.ActualHeight;
				tv2 = (y + _patternOffset.Y + height - 0.5f) / (float)_drawingPattern.ActualHeight;
				lineVertex[0].TextureCoordinates.X = tu1;
				lineVertex[0].TextureCoordinates.Y = tv1;
				lineVertex[1].TextureCoordinates.X = tu2;
				lineVertex[1].TextureCoordinates.Y = tv1;
				lineVertex[2].TextureCoordinates.X = tu2;
				lineVertex[2].TextureCoordinates.Y = tv2;
				lineVertex[3].TextureCoordinates.X = tu1;
				lineVertex[3].TextureCoordinates.Y = tv2;
			}
			_useIndices = false;

			// Set the current primitive style.
			_currentPrimitiveStyle = PrimitiveStyle.TriangleList;
			_useIndices = true;

			// Send to the renderer.
			Draw(lineVertex);
		}

		/// <summary>
		/// Function to draw a horizontal line.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="width">Width of the line.</param>
		/// <param name="color">Color for the line.</param>
		/// <param name="penWidth">Width of the pen.</param>
		/// <param name="penHeight">Height of the pen.</param>
		public void HorizontalLine(float x, float y, float width, Drawing.Color color, float penWidth, float penHeight)
		{
			if (width < 1.0f)
				return;

			if ((penHeight <= 1.0f) && (penWidth <= 1.0f))
			{
				// Vertex to represent the line.
				VertexTypeList.PositionDiffuse2DTexture1[] lineVertex = new VertexTypeList.PositionDiffuse2DTexture1[2];

				// Build end points.
				lineVertex[0].Position.X = (int)x;
				lineVertex[0].Position.Y = (int)y;
				lineVertex[0].Position.Z = -PrimitiveDepth;
				lineVertex[1].Position.X = (int)(x + width);
				lineVertex[1].Position.Y = (int)y;
				lineVertex[1].Position.Z = -PrimitiveDepth;
				lineVertex[0].ColorValue = color.ToArgb();
				lineVertex[1].ColorValue = color.ToArgb();

				if (_drawingPattern == null)
				{
					lineVertex[0].TextureCoordinates.X = 0.0f;
					lineVertex[0].TextureCoordinates.Y = 0.0f;
					lineVertex[1].TextureCoordinates.X = 0.0f;
					lineVertex[1].TextureCoordinates.Y = 0.0f;
				}
				else
				{
					lineVertex[0].TextureCoordinates.X = (x + _patternOffset.X - 0.5f) / (float)_drawingPattern.ActualWidth;
					lineVertex[0].TextureCoordinates.Y = (y + _patternOffset.Y - 0.5f) / (float)_drawingPattern.ActualHeight;
					lineVertex[1].TextureCoordinates.X = (x + _patternOffset.X + width - 0.5f) / (float)_drawingPattern.ActualWidth;
					lineVertex[1].TextureCoordinates.Y = (y + _patternOffset.Y - 0.5f) / (float)_drawingPattern.ActualHeight;
				}
				_useIndices = false;

				// Set the current primitive style.
				_currentPrimitiveStyle = PrimitiveStyle.LineList;

				// Send to the renderer.
				Draw(lineVertex);
			}
			else
				Line(x, y, width, 1.0f, color,penWidth,penHeight);
		}

		/// <summary>
		/// Function to draw a horizontal line.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="width">Width of the line.</param>
		/// <param name="color">Color for the line.</param>
		/// <param name="penSize">Size of the pen.</param>
		public void HorizontalLine(float x, float y, float width, Drawing.Color color, Vector2D penSize)
		{
			HorizontalLine(x, y, width, color, penSize.X, penSize.Y);
		}

		/// <summary>
		/// Function to draw a horizontal line.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="width">Width of the line.</param>
		/// <param name="color">Color for the line.</param>
		public void HorizontalLine(float x, float y, float width, Drawing.Color color)
		{
			HorizontalLine(x, y, width, color, 1.0f, 1.0f);
		}

		/// <summary>
		/// Function to draw a horizontal line.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="height">Height of the line.</param>
		/// <param name="color">Color for the line.</param>
		/// <param name="penWidth">Width of the pen.</param>
		/// <param name="penHeight">Height of the pen.</param>
		public void VerticalLine(float x, float y, float height, Drawing.Color color, float penWidth, float penHeight)
		{
			if (height < 1.0f)
				return;

			if ((penHeight <= 1.0f) && (penWidth <= 1.0f))
			{
				// Vertex to represent the line.
				VertexTypeList.PositionDiffuse2DTexture1[] lineVertex = new VertexTypeList.PositionDiffuse2DTexture1[2];

				// Build end points.
				lineVertex[0].Position.X = (int)x;
				lineVertex[0].Position.Y = (int)y;
				lineVertex[0].Position.Z = -PrimitiveDepth;
				lineVertex[1].Position.X = (int)x;
				lineVertex[1].Position.Y = (int)(y + height);
				lineVertex[1].Position.Z = -PrimitiveDepth;
				lineVertex[0].ColorValue = color.ToArgb();
				lineVertex[1].ColorValue = color.ToArgb();

				if (_drawingPattern == null)
				{
					lineVertex[0].TextureCoordinates.X = 0.0f;
					lineVertex[0].TextureCoordinates.Y = 0.0f;
					lineVertex[1].TextureCoordinates.X = 0.0f;
					lineVertex[1].TextureCoordinates.Y = 0.0f;
				}
				else
				{
					lineVertex[0].TextureCoordinates.X = (x + _patternOffset.X - 0.5f) / (float)_drawingPattern.ActualWidth;
					lineVertex[0].TextureCoordinates.Y = (y + _patternOffset.Y - 0.5f) / (float)_drawingPattern.ActualHeight;
					lineVertex[1].TextureCoordinates.X = (x + _patternOffset.X - 0.5f) / (float)_drawingPattern.ActualWidth;
					lineVertex[1].TextureCoordinates.Y = (y + _patternOffset.Y + height - 0.5f) / (float)_drawingPattern.ActualHeight;
				}
				_useIndices = false;

				// Set the current primitive style.
				_currentPrimitiveStyle = PrimitiveStyle.LineList;

				// Send to the renderer.
				Draw(lineVertex);
			}
			else
				Line(x, y, 1.0f, height, color,penWidth,penHeight);
		}

		/// <summary>
		/// Function to draw a horizontal line.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="height">Height of the line.</param>
		/// <param name="color">Color for the line.</param>
		/// <param name="penSize">Size of the pen.</param>
		public void VerticalLine(float x, float y, float height, Drawing.Color color, Vector2D penSize)
		{
			VerticalLine(x, y, height, color, penSize.X, penSize.Y);
		}

		/// <summary>
		/// Function to draw a horizontal line.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="height">Height of the line.</param>
		/// <param name="color">Color for the line.</param>
		public void VerticalLine(float x, float y, float height, Drawing.Color color)
		{
			VerticalLine(x, y, height, color, 1.0f, 1.0f);
		}

		/// <summary>
		/// Function to draw an unfilled circle.
		/// </summary>
		/// <param name="x">Starting horizontal position of the circle.</param>
		/// <param name="y">Starting vertical position of the circle.</param>
		/// <param name="radius">Radius of the circle.</param>
		/// <param name="colorValue">Color of the circle.</param>
		/// <param name="penWidth">Width of the pen.</param>
		/// <param name="penHeight">Height of the pen.</param>
		/// <remarks>Taken from: Wikipedia entry on circle drawing</remarks>
		public void Circle(float x, float y, float radius, Drawing.Color colorValue, float penWidth, float penHeight)
		{
			// Radius check
			if (radius < 0)
				return;

			// Draw a point for empty radius
			if (radius == 0)
			{
				SetPoint(x, y, colorValue);
				return;
			}

			float error = -radius;
			float nx = radius;
			float ny = 0;

			// The following while loop may altered to 'while (x > y)' for a
			// performance benefit, as long as a call to 'plot4points' follows
			// the body of the loop. This allows for the elimination of the
			// '(x != y') test in 'plot8points', providing a further benefit.
			//
			// For the sake of clarity, this is not shown here.
			while (nx >= ny)
			{
				Circle_Plot8Points_Internal(x, y, nx, ny, colorValue, penWidth, penHeight);

				error += ny;
				++ny;
				error += ny;

				// The following test may be implemented in assembly language in
				// most machines by testing the carry flag after adding 'y' to
				// the value of 'error' in the previous step, since 'error'
				// nominally has a negative value.
				if (error >= 0)
				{
					--nx;
					error -= nx;
					error -= nx;
				}
			}
		}

		void Circle_Plot8Points_Internal(float cx, float cy, float x, float y, Drawing.Color color, float penWidth, float penHeight)
		{
			Circle_Plot4Points_Internal(cx, cy, x, y, color, penWidth, penHeight);

			if (x != y)
				Circle_Plot4Points_Internal(cx, cy, y, x, color, penWidth, penHeight);
		}

		void Circle_Plot4Points_Internal(float cx, float cy, float x, float y, Drawing.Color color, float penWidth, float penHeight)
		{
			SetPoint(cx + x, cy + y, color, penWidth, penHeight);

			if (x != 0)
				SetPoint(cx - x, cy + y, color, penWidth, penHeight);

			if (y != 0)
				SetPoint(cx + x, cy - y, color, penWidth, penHeight);

			SetPoint(cx - x, cy - y, color, penWidth, penHeight);
		}

		/// <summary>
		/// Function to draw an unfilled circle.
		/// </summary>
		/// <param name="x">Starting horizontal position of the circle.</param>
		/// <param name="y">Starting vertical position of the circle.</param>
		/// <param name="radius">Radius of the circle.</param>
		/// <param name="colorValue">Color of the circle.</param>
		/// <param name="penSize">Size of the pen.</param>
		public void Circle(float x, float y, float radius, Drawing.Color colorValue, Vector2D penSize)
		{
			Circle(x, y, radius, colorValue, penSize.X, penSize.Y);
		}

		/// <summary>
		/// Function to draw an unfilled circle.
		/// </summary>
		/// <param name="x">Starting horizontal position of the circle.</param>
		/// <param name="y">Starting vertical position of the circle.</param>
		/// <param name="radius">Radius of the circle.</param>
		/// <param name="colorValue">Color of the circle.</param>
		public void Circle(float x, float y, float radius, Drawing.Color colorValue)
		{
			Circle(x, y, radius, colorValue, 1.0f, 1.0f);
		}

		/// <summary>
		/// Function to draw a filled ellipse.
		/// </summary>
		/// <param name="x">Horizontal position of the ellipse.</param>
		/// <param name="y">Vertical position of the ellipse.</param>
		/// <param name="xradius">Horizontal radius.</param>
		/// <param name="yradius">Vertical radius.</param>
		/// <param name="colorValue">Color of the ellipse.</param>
		/// <remarks>Taken from: SDL_gfx Written by Andreas Schiffler</remarks>
		public void FilledEllipse(float x, float y, float xradius, float yradius, Drawing.Color colorValue)
		{
			int ix, iy;
			int h, i, j, k;
			int oh, oi, oj, ok;
			int xmh, xph;
			int xmi, xpi;
			int xmj, xpj;
			int xmk, xpk;

			// Check for no radius.
			if ((xradius < 0) || (yradius < 0))
				return;

			// Check for 0 horizontal radius.
			if (xradius == 0)
			{
				VerticalLine(x, y - yradius, yradius + yradius, colorValue);
				return;
			}
			// Check for 0 vertical radius.
			if (yradius == 0)
			{
				HorizontalLine(x - xradius, y, xradius + xradius, colorValue);
				return;
			}
			
			// Initialize.
			oh = oi = oj = ok = 0xFFFF;

			/*
			 * Draw 
			 */
			if (xradius > yradius)
			{
				ix = 0;
				iy = (int)xradius * 64;

				do
				{
					h = ((ix + 32) >> 6);
					i = ((iy + 32) >> 6);
					j = ((h * (int)yradius) / (int)xradius);
					k = ((i * (int)yradius) / (int)xradius);

					if ((ok != k) && (oj != k))
					{
						xph = (int)x + h;
						xmh = (int)x - h;
						if (k > 0)
						{
							HorizontalLine(xmh, y + k, xph - xmh, colorValue);
							HorizontalLine(xmh, y - k, xph - xmh, colorValue);
						}
						else
							HorizontalLine(xmh, y, xph - xmh, colorValue);
						ok = k;
					}
					if ((oj != j) && (ok != j) && (k != j))
					{
						xmi = (int)x - i;
						xpi = (int)x + i;
						if (j > 0)
						{
							HorizontalLine(xmi, y + j, xpi - xmi, colorValue);
							HorizontalLine(xmi, y - j, xpi - xmi, colorValue);
						}
						else
							HorizontalLine(xmi, y, xpi - xmi, colorValue);
						oj = j;
					}

					ix = ix + iy / (int)xradius;
					iy = iy - ix / (int)xradius;

				} while (i > h);
			}
			else
			{
				ix = 0;
				iy = (int)yradius * 64;

				do
				{
					h = (ix + 32) >> 6;
					i = (iy + 32) >> 6;
					j = (h * (int)xradius) / (int)yradius;
					k = (i * (int)xradius) / (int)yradius;

					if ((oi != i) && (oh != i))
					{
						xmj = (int)x - j;
						xpj = (int)x + j;
						if (i > 0)
						{
							HorizontalLine(xmj, y + i, xpj - xmj, colorValue);
							HorizontalLine(xmj, y - i, xpj - xmj, colorValue);
						}
						else
							HorizontalLine(xmj, y, xpj - xmj, colorValue);
						oi = i;
					}
					if ((oh != h) && (oi != h) && (i != h))
					{
						xmk = (int)x - k;
						xpk = (int)x + k;
						if (h > 0)
						{
							HorizontalLine(xmk, y + h, xmk - xpk, colorValue);
							HorizontalLine(xmk, y - h, xmk - xpk, colorValue);
						}
						else
							HorizontalLine(xmk, y, xmk - xpk, colorValue);
						oh = h;
					}

					ix = ix + iy / (int)yradius;
					iy = iy - ix / (int)yradius;

				} while (i > h);
			}
		}


		/// <summary>
		/// Function to draw a filled circle.
		/// </summary>
		/// <param name="x">Starting horizontal position of the circle.</param>
		/// <param name="y">Starting vertical position of the circle.</param>
		/// <param name="radius">Radius of the circle.</param>
		/// <param name="colorValue">Color of the circle.</param>
		/// <remarks>Taken from: SDL_gfx Written by Andreas Schiffler</remarks>
		public void FilledCircle(float x, float y, float radius, Drawing.Color colorValue)
		{
			float cx = 0;
			float cy = radius;
			float ocx = (float)0xffff;
			float ocy = (float)0xffff;
			float df = 1 - radius;
			float d_e = 3;
			float d_se = -2 * radius + 5.0f;
			float xpcx, xmcx, xpcy, xmcy;
			float ypcy, ymcy, ypcx, ymcx;

			// Verify radius.
			if (radius < 0)
				return;

			// Set a single pixel for no radius.
			if (radius == 0)
			{
				SetPoint(x, y, colorValue);
				return;
			}

			do
			{
				xpcx = x + cx;
				xmcx = x - cx;
				xpcy = x + cy;
				xmcy = x - cy;
				if (ocy != cy)
				{
					if (cy > 0)
					{
						ypcy = y + cy;
						ymcy = y - cy;
						HorizontalLine(xmcx, ypcy, xpcx - xmcx, colorValue);
						HorizontalLine(xmcx, ymcy, xpcx - xmcx, colorValue);
					}
					else
						HorizontalLine(xmcx, y, xpcx - xmcx, colorValue);
					ocy = cy;
				}
				if (ocx != cx)
				{
					if (cx != cy)
					{
						if (cx > 0)
						{
							ypcx = y + cx;
							ymcx = y - cx;
							HorizontalLine(xmcy, ymcx, xpcy - xmcy, colorValue);
							HorizontalLine(xmcy, ypcx, xpcy - xmcy, colorValue);
						}
						else
							HorizontalLine(xmcy, y, xpcy - xmcy, colorValue);
					}
					ocx = cx;
				}
				/*
				 * Update 
				 */
				if (df < 0)
				{
					df += d_e;
					d_e += 2;
					d_se += 2;
				}
				else
				{
					df += d_se;
					d_e += 2;
					d_se += 4;
					cy--;
				}
				cx++;
			} while (cx <= cy);
		}

		/// <summary>
		/// Function to draw an empty ellipse.
		/// </summary>
		/// <param name="x">Horizontal position of the ellipse.</param>
		/// <param name="y">Vertical position of the ellipse.</param>
		/// <param name="xradius">Horizontal radius.</param>
		/// <param name="yradius">Vertical radius.</param>
		/// <param name="color">Color of the ellipse.</param>
		public void Ellipse(float x, float y, float xradius, float yradius, Drawing.Color color)
		{
			Ellipse(x, y, xradius, yradius, color, 1.0f, 1.0f);
		}

		/// <summary>
		/// Function to draw an empty ellipse.
		/// </summary>
		/// <param name="x">Horizontal position of the ellipse.</param>
		/// <param name="y">Vertical position of the ellipse.</param>
		/// <param name="xradius">Horizontal radius.</param>
		/// <param name="yradius">Vertical radius.</param>
		/// <param name="color">Color of the ellipse.</param>
		/// <param name="penSize">Size of the pen.</param>
		public void Ellipse(float x, float y, float xradius, float yradius, Drawing.Color color, Vector2D penSize)
		{
			Ellipse(x, y, xradius, yradius, color, penSize.X, penSize.Y);
		}

		/// <summary>
		/// Function to draw an empty ellipse.
		/// </summary>
		/// <param name="x">Horizontal position of the ellipse.</param>
		/// <param name="y">Vertical position of the ellipse.</param>
		/// <param name="xradius">Horizontal radius.</param>
		/// <param name="yradius">Vertical radius.</param>
		/// <param name="color">Color of the ellipse.</param>
		/// <param name="penWidth">Width of the pen.</param>
		/// <param name="penHeight">Height of the pen.</param>
		/// <remarks>Taken from implementation of fast ellipse drawing paper for BASIC.</remarks>
		public void Ellipse(float x, float y, float xradius, float yradius, Drawing.Color color, float penWidth, float penHeight)
		{
			float nx                = xradius;
			float ny                = 0;
			float XChange          = yradius * yradius * (1 - 2 * xradius);
			float YChange          = xradius * xradius;
			float EllipseError     = 0;
			float TwoASquare       = 2 * xradius * xradius;
			float TwoBSquare       = 2 * yradius * yradius;
			float StoppingX        = TwoBSquare * xradius;
			float StoppingY        = 0;
			Vector2D penVector = new Vector2D(penWidth, penHeight);

			// Check radius
			if (xradius < 0 || yradius < 0)
				return;

			// Line drawing
			if (xradius == 0)
			{
				VerticalLine(x, y - yradius, yradius + yradius, color, penWidth, penHeight);
				return;
			}

			// Line drawing
			if (yradius == 0)
			{
				HorizontalLine(x - xradius, y, xradius + xradius, color, penWidth, penHeight);
				return;
			}

			while (StoppingX >= StoppingY)
			{
				SetPoint(x - nx, y + ny, color, penVector);
				SetPoint(x + nx, y + ny, color, penVector);
				SetPoint(x - nx, y - ny, color, penVector);
				SetPoint(x + nx, y - ny, color, penVector);

				ny++;
				StoppingY += TwoASquare;
				EllipseError += YChange;
				YChange += TwoASquare;

				if ((2 * EllipseError + XChange) > 0)
				{
					nx--;
					StoppingX -= TwoBSquare;
					EllipseError += XChange;
					XChange += TwoBSquare;
				}
			}

			nx = 0;
			ny = yradius;
			XChange = yradius * yradius;
			YChange = xradius * xradius * (1 - 2 * yradius);
			EllipseError = 0;
			StoppingX = 0;
			StoppingY = TwoASquare * yradius;

			while (StoppingX <= StoppingY)
			{
				SetPoint(x - nx, y + ny, color, penVector);
				SetPoint(x + nx, y + ny, color, penVector);
				SetPoint(x - nx, y - ny, color, penVector);
				SetPoint(x + nx, y - ny, color, penVector);

				nx++;
				StoppingX += TwoBSquare;
				EllipseError += XChange;
				XChange += TwoBSquare;

				if ((2 * EllipseError + YChange) > 0)
				{
					ny--;
					StoppingY -= TwoASquare;
					EllipseError += YChange;
					YChange += TwoASquare;
				}
			}
		}

		/// <summary>
		/// Function to refresh this render window and its child windows.
		/// </summary>
		public void Refresh()
		{
			// Reset the default window.
			if ((_defaultView.Width != _width) || (_defaultView.Height != _height) || (_defaultView.Left != 0) || (_defaultView.Top !=0))
				_defaultView.SetWindowDimensions(0, 0, _width, _height);
			_defaultView.Refresh(this);
		}

		/// <summary>
		/// Function to clear the screen, depth buffer and stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear the backbuffer with.</param>
		/// <param name="depthValue">Value to overwrite the depth buffer with.</param>
		/// <param name="stencilValue">Value to overwrite the stencil buffer with.</param>
		public void Clear(Drawing.Color color, float depthValue, int stencilValue)
		{
			BeginDrawing();
			Gorgon.Renderer.Clear(color, depthValue, stencilValue, ClearEachFrame);
			EndDrawing();
		}

		/// <summary>
		/// Function to clear the screen, depth buffer and stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear the backbuffer with.</param>
		/// <param name="depthValue">Value to overwrite the depth buffer with.</param>
		public void Clear(Drawing.Color color, float depthValue)
		{
			BeginDrawing();
			Gorgon.Renderer.Clear(color, depthValue, 0, ClearEachFrame);
			EndDrawing();
		}

		/// <summary>
		/// Function to clear the screen, depth buffer and stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear the backbuffer with.</param>
		public void Clear(Drawing.Color color)
		{
			BeginDrawing();
			Gorgon.Renderer.Clear(color, 1.0f, 0, ClearEachFrame);
			EndDrawing();
		}

		/// <summary>
		/// Function to clear the screen, depth buffer and stencil buffer.
		/// </summary>
		public void Clear()
		{
			Clear(BackgroundColor);
		}

		/// <summary>
		/// Function to render the scene for this target.
		/// </summary>
		public abstract void Update();

		/// <summary>
		/// Function to copy the render target into a render target image.
		/// </summary>
		/// <param name="destination">The render image that will receive the data.</param>
		public abstract void CopyToImage(RenderImage destination);

		/// <summary>
		/// Function to copy the render target into an image.
		/// </summary>
		/// <param name="image">Image that will receive the data.</param>
		public abstract void CopyToImage(Image image);
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of this render target.</param>
		/// <param name="width">Width of the render target.</param>
		/// <param name="height">Height of the render target.</param>
		protected RenderTarget(string name,int width,int height) : base(name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			if (RenderTargetCache.Targets.Contains(name))
				throw new ArgumentException("'" + name + "' already exists.");

			_width = width;
			_height = height;
			_backColor = Drawing.Color.White;			
			_clearTargets = Gorgon.ClearEachFrame;
			_inheritClearTargets = true;
			_lastTarget = null;
			_smoothing = Smoothing.None;
			_blending = BlendingModes.None;
			_alphaFunction = CompareFunctions.Always;			
			_drawingPattern = null;
			_wrapHMode = ImageAddressing.Clamp;
			_wrapVMode = ImageAddressing.Clamp;
			_sourceBlend = AlphaBlendOperation.One;
			_destBlend = AlphaBlendOperation.Zero;
			_depthBias = 0.0f;
			_depthCompare = CompareFunctions.LessThanOrEqual;
			_depthWriteEnabled = true;

			// Create a default window.
			_defaultView = new Viewport(0, 0, 1, 1);

			DeviceStateList.Add(this);
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~RenderTarget()
		{
			Dispose(false);
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
			{
				if (_convertBuffer != null)
					_convertBuffer.Dispose();
				_convertBuffer = null;
				DeviceStateList.Remove(this);

				// Unregister from the factory.
				if (RenderTargetCache.Targets.Contains(Name))
					RenderTargetCache.Targets.Remove(Name);
			}
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

		#region IDeviceStateObject Members
		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public virtual void DeviceLost()
		{
			if (_convertBuffer != null)
				_convertBuffer.Dispose();
			_convertBuffer = null;
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public abstract void DeviceReset();

		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		public virtual void ForceRelease()
		{
			if (_convertBuffer != null)
				_convertBuffer.Dispose();
			_convertBuffer = null;
		}
		#endregion

		#region ICommonRenderable Members
		/// <summary>
		/// Property to set or return whether to enable the depth buffer (if applicable) writing or not.
		/// </summary>
		public virtual bool DepthWriteEnabled
		{
			get
			{
				return _depthWriteEnabled;
			}
			set
			{
				_depthWriteEnabled = value;
			}
		}

		/// <summary>
		/// Property to set or return (if applicable) the depth buffer bias.
		/// </summary>
		public virtual float DepthBufferBias
		{
			get
			{
				return _depthBias;
			}
			set
			{
				_depthBias = value;
			}
		}

		/// <summary>
		/// Property to set or return the depth buffer (if applicable) testing comparison function.
		/// </summary>
		public virtual CompareFunctions DepthTestFunction
		{
			get
			{
				return _depthCompare;
			}
			set
			{
				_depthCompare = value;
			}
		}

		/// <summary>
		/// Property to set or return the wrapping mode to use.
		/// </summary>
		public ImageAddressing WrapMode
		{
			get
			{
				return _wrapHMode;
			}
			set
			{
				_wrapVMode = _wrapHMode = value;
			}
		}

		/// <summary>
		/// Property to set or return the horizontal wrapping mode to use.
		/// </summary>
		public ImageAddressing HorizontalWrapMode
		{
			get
			{
				return _wrapHMode;
			}
			set
			{
				_wrapHMode = value;
			}
		}

		/// <summary>
		/// Property to set or return the vertical wrapping mode to use.
		/// </summary>
		public ImageAddressing VerticalWrapMode
		{
			get
			{
				return _wrapVMode;
			}
			set
			{
				_wrapVMode = value;
			}
		}

		/// <summary>
		/// Property to set or return the type of smoothing for the sprites.
		/// </summary>
		public Smoothing Smoothing
		{
			get
			{
				return _smoothing;
			}
			set
			{
				_smoothing = value;
			}
		}

		/// <summary>
		/// Property to set or return the function used for alpha masking.
		/// </summary>
		public CompareFunctions AlphaMaskFunction
		{
			get
			{
				return _alphaFunction;
			}
			set
			{
				_alphaFunction = value;
			}
		}

		/// <summary>
		/// Property to set or return the alpha value used for alpha masking.
		/// </summary>
		public int AlphaMaskValue
		{
			get
			{
				return _alphaMaskValue;
			}
			set
			{
				_alphaMaskValue = value;
			}
		}

		/// <summary>
		/// Property to set or return the blending mode.
		/// </summary>
		public BlendingModes BlendingMode
		{
			get
			{
				return _blending;
			}
			set
			{
				_blending = value;
				SetBlendMode(value);
			}
		}

		/// <summary>
		/// Property to set or return the source blending operation.
		/// </summary>
		public AlphaBlendOperation SourceBlend
		{
			get
			{
				return _sourceBlend;
			}
			set
			{
				_sourceBlend = value;				
			}
		}

		/// <summary>
		/// Property to set or return the destination blending operation.
		/// </summary>
		public AlphaBlendOperation DestinationBlend
		{
			get
			{
				return _destBlend;
			}
			set
			{
				_destBlend = value;
			}
		}

		/// <summary>
		/// Property to set or return whether to enable the use of the stencil buffer or not.
		/// </summary>
		public bool StencilEnabled
		{
			get
			{
				return _useStencil;
			}
			set
			{
				_useStencil = value;
			}
		}

		/// <summary>
		/// Property to set or return the reference value for the stencil buffer.
		/// </summary>
		public int StencilReference
		{
			get
			{
				return _stencilReference;
			}
			set
			{
				_stencilReference = value;
			}
		}

		/// <summary>
		/// Property to set or return the mask value for the stencil buffer.
		/// </summary>
		public int StencilMask
		{
			get
			{
				return _stencilMask;
			}
			set
			{
				_stencilMask = value;
			}
		}

		/// <summary>
		/// Property to set or return the operation for passing stencil values.
		/// </summary>
		public StencilOperations StencilPassOperation
		{
			get
			{
				return _stencilPassOperation;
			}
			set
			{
				_stencilPassOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the operation for the failing stencil values.
		/// </summary>
		public StencilOperations StencilFailOperation
		{
			get
			{
				return _stencilFailOperation;
			}
			set
			{
				_stencilFailOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the stencil operation for the failing depth values.
		/// </summary>
		public StencilOperations StencilZFailOperation
		{
			get
			{
				return _stencilZFailOperation;
			}
			set
			{
				_stencilZFailOperation = value;
			}
		}

		/// <summary>
		/// Property to set or return the stencil comparison function.
		/// </summary>
		public CompareFunctions StencilCompare
		{
			get
			{
				return _stencilCompare;
			}
			set
			{
				_stencilCompare = value;
			}
		}

		/// <summary>
		/// Property to return the primitive style for the object.
		/// </summary>
		public PrimitiveStyle PrimitiveStyle
		{
			get
			{
				return _currentPrimitiveStyle;
			}
		}

		/// <summary>
		/// Property to return whether to use an index buffer or not.
		/// </summary>
		public bool UseIndices
		{
			get
			{
				return _useIndices;
			}		
		}
		#endregion
	}
}
