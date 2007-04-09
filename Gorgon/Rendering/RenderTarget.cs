#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Tuesday, August 02, 2005 11:53:31 AM
// 
#endregion

using System;
using Drawing = System.Drawing;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Mathematics;
using D3D = Microsoft.DirectX.Direct3D;
using GorgonLibrary.Graphics.Shaders;
using GorgonLibrary.Internal;
using GorgonLibrary.Timing;

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
		: NamedObject, IDisposable, IDeviceStateObject, IRenderable
	{
		#region Variables.
		private TimingData _timingData;						// Timing data.
		private bool _inheritClearTargets;					// Flag to indicate that we will inherit the clear targets from Gorgon.
		private Viewport _lastClipper;						// Last clipper.
		private RenderTarget _lastTarget;					// Last active render target.
		private Blending _blending;							// Blending mode for drawing.
		private Smoothing _smoothing;						// Smoothing mode for drawing.
		private CompareFunctions _alphaFunction;			// Alpha comparison function.
		private int _alphaMaskValue;						// Mask value for comparison.
		private PrimitiveStyle _currentPrimitiveStyle;		// Current primitive style.
		private bool _useIndices;							// Flag to indicate whether to use an index buffer or not.
		private Image _drawingPattern;						// Image pattern to use for drawing.
		private Viewport _clipping;							// Clipping window.
		private bool _setOnce = false;						// Flag to set the states only once.
		private bool _statesSet = false;					// Flag to determine if the states have been set.
		private ImageAddressing _wrapHMode;					// Horizontal image wrapping mode.
		private ImageAddressing _wrapVMode;					// Horizontal image wrapping mode.
		private AlphaBlendOperation _sourceBlend;			// Source blending operation.
		private AlphaBlendOperation _destBlend;				// Destination blending operation.
		private StencilOperations _stencilPassOperation;	// Stencil pass operation.
		private StencilOperations _stencilFailOperation;	// Stencil fail operation.
		private StencilOperations _stencilZFailOperation;	// Stencil Z fail operation.
		private CompareFunctions _stencilCompare;			// Stencil compare operation.
		private int _stencilReference;						// Stencil reference value.
		private int _stencilMask;							// Stencil mask value.
		private bool _useStencil;							// Flag to indicate whether to use the stencil or not.
		private Matrix _orthoMatrix;						// Orthogonal matrix.

		/// <summary>Projection matrix changed.</summary>
		protected bool _projectionChanged;
		/// <summary>Default window.</summary>
		protected Viewport _defaultView;
		/// <summary>Width of render target.</summary>
		protected int _width;
		/// <summary>Height of render target.</summary>
		protected int _height;
		/// <summary>Priority for rendering.</summary>
		protected int _priority;
		/// <summary>Default background color of the render target.</summary>
		protected Drawing.Color _backColor;
		/// <summary>Event arguments for frame events.</summary>
		protected FrameEventArgs _frameEventArguments;
		/// <summary>Flag to indicate that we want to use a depth buffer.</summary>
		protected bool _useDepthBuffer;
		/// <summary>Flag to indicate that we want to use a stencil buffer.</summary>
		protected bool _useStencilBuffer;
		/// <summary>Color buffer from the render target.</summary>
		protected D3D.Surface _colorBuffer;
		/// <summary>Depth buffer to use.</summary>
		protected D3D.Surface _depthBuffer;
		/// <summary>Flags to indicate what to clear per frame.</summary>
		protected ClearTargets _clearTargets;
		/// <summary>Gorgon logo image.</summary>
		protected static Image _logoImage = null;
		/// <summary>Logo sprite.</summary>
		protected static Sprite _logoSprite = null;
		/// <summary>Logo statistics.</summary>
		protected static TextSprite _logoStats = null;
		#endregion

		#region Events.
		/// <summary>Event fired when rendering begins for this target.</summary>
		public event FrameEventHandler OnFrameBegin;
		/// <summary>Event fired when rendering ends for this target.</summary>
		public event FrameEventHandler OnFrameEnd;
		#endregion

		#region Properties.
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
			}
		}

		/// <summary>
		/// Property to return a z buffer for a swap chain.
		/// </summary>
		internal D3D.Surface DepthBuffer
		{
			get
			{
				return _depthBuffer;
			}
		}

		/// <summary>
		/// Property to return the surface for this render target.
		/// </summary>
		internal D3D.Surface SurfaceBuffer
		{
			get
			{
				return _colorBuffer;
			}
		}

		/// <summary>
		/// Property to set or return whether the projection matrix needs updating.
		/// </summary>
		internal bool ProjectionNeedsUpdate
		{
			get
			{
				return _projectionChanged;
			}
			set
			{
				_projectionChanged = value;
			}
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
		/// Property to set or return flags that indicate what should be cleared per frame.
		/// </summary>
		public ClearTargets ClearEachFrame
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
				if (_projectionChanged)
				{
					_orthoMatrix = Gorgon.Renderer.CreateOrthoMatrix(0, 0, _width, _height);
					_projectionChanged = false;
				}
				return _orthoMatrix;
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
				throw new NotImplementedException(NotImplementedTypes.Property, "Width", null);
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
				throw new NotImplementedException(NotImplementedTypes.Property, "Width", null);
			}
		}

		/// <summary>
		/// Property to set or return the rendering priority of the render target.
		/// </summary>
		public int Priority
		{
			get
			{
				return _priority;
			}
			set
			{
				_priority = value;
				Gorgon.RenderTargets.Sort();
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
		/// Property to return the timing data for a frame.
		/// </summary>
		public TimingData TimingData
		{
			get
			{
				return _timingData;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw frame statistics.
		/// </summary>
		private void DrawStats()
		{
			int backColor = 0;		// Background color.

			if (_logoStats == null)
				return;

			backColor = (int)((0xFFFFFF ^ (uint)_backColor.ToArgb()) & 0xFFFFFF);
			_logoStats.Color = Drawing.Color.FromArgb((int)(0xFF000000 | (uint)backColor));
			_logoStats.Text = "Frame draw time: " + Convert.ToSingle(_timingData.FrameDrawTime).ToString("0.0") + "ms\n" +
								"Current FPS: " + _timingData.CurrentFPS.ToString("0.0") + "\n" +
								"Average FPS: " + _timingData.AverageFPS.ToString("0.0") + "\n" +
								"Highest FPS: " + _timingData.HighestFPS.ToString("0.0") + "\n" +
								"Lowest FPS: " + _timingData.LowestFPS.ToString("0.0") + "\n";
			_logoStats.Draw();
		}

		/// <summary>
		/// Function to draw the logo.
		/// </summary>
		private void DrawLogo()
		{
			if ((_logoSprite == null) || (_logoImage == null) || (this != Gorgon.Screen))
				return;

			_logoSprite.SetPosition(Width - _logoSprite.Width, Height - _logoSprite.Height);
			_logoSprite.AlphaMaskFunction = CompareFunctions.Always;
			_logoSprite.BlendingMode = Blending.Normal;
			_logoSprite.Opacity = 225;
			_logoSprite.Draw();
		}


		/// <summary>
		/// Function to perform the actual drawing on the target.
		/// </summary>
		/// <param name="vertices">Vertices to draw.</param>
		private void Draw(VertexTypes.PositionDiffuse2DTexture1[] vertices)
		{
			StateManager manager = null;	// State manager.
			int start = 0;					// Vertex starting point.
			int count = 0;					// Vertex count.

			manager = Gorgon.StateManager;
			start = manager.RenderData.VertexOffset;
			count = manager.RenderData.VertexCount;

			// If we're at the end of the buffer, wrap around.
			if (((manager.RenderData.VerticesWritten + 2 >= count) || (Gorgon.Renderer.GetImage(0) != _drawingPattern) || (manager.StateChanged(this))) && (manager.RenderData.VerticesWritten != 0))
			{
				Gorgon.Renderer.Render();
				start = 0;
			}

			// Set the state for this sprite.
			if (((_setOnce) && (!_statesSet)) || (!_setOnce))
			{
				manager.SetStates(this);
				_statesSet = true;

				// Set the currently active image.
                Gorgon.Renderer.SetImage(0, _drawingPattern);
			}            

			// Write the data to the buffer.			
			manager.RenderData.VertexCache.WriteData(0, manager.RenderData.VertexOffset + manager.RenderData.VerticesWritten, vertices.Length, vertices);
		}

		/// <summary>
		/// Function to return a viable stencil/depth buffer.
		/// </summary>
		/// <param name="usestencil">TRUE to use a stencil, FALSE to exclude.</param>
		/// <param name="usedepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		protected abstract void UpdateDepthStencilFormat(bool usestencil, bool usedepth);

		/// <summary>
		/// Function to check for the availability of a particular depth/stencil buffer.
		/// </summary>
		/// <param name="driverindex">Index of the driver we're using.</param>
		/// <param name="backBuffer">Back buffer format.</param>
		/// <param name="depthBuffer">Depth buffer format.</param>
		/// <returns>TRUE if supported, FALSE if not.</returns>
		protected bool CheckBackBuffer(int driverindex, D3D.Format backBuffer, D3D.DepthFormat depthBuffer)
		{
			if (D3D.Manager.CheckDeviceFormat(driverindex, RenderWindow.DeviceType, backBuffer, D3D.Usage.DepthStencil, D3D.ResourceType.Surface, depthBuffer))
				return D3D.Manager.CheckDepthStencilMatch(driverindex, RenderWindow.DeviceType, backBuffer, backBuffer, depthBuffer);

			return false;
		}

		/// <summary>
		/// Function to set this render target and its default window as currently active.
		/// </summary>
		protected internal void SetActive()
		{
			_lastTarget = Gorgon.Renderer.CurrentRenderTarget;
			_lastClipper = Gorgon.Renderer.CurrentClippingView;
			Gorgon.Renderer.SetRenderTarget(this);
			Gorgon.Renderer.CurrentClippingView = ClippingViewport;
		}

		/// <summary>
		/// Function to reset the target/viewport to the previous view and target.
		/// </summary>
		protected internal void RestoreActive()
		{
			Gorgon.Renderer.SetRenderTarget(_lastTarget);
			Gorgon.Renderer.CurrentClippingView = _lastClipper;
		}

		/// <summary>
		/// Function to lock a layer so that we can begin drawing.
		/// </summary>
		public void BeginDrawing()
		{
			// Flush rendering before changing.
			Gorgon.Renderer.Render();

			// Set this target as the active target.
			SetActive();
		}

		/// <summary>
		/// Function to end the lock on the layer.
		/// </summary>
		public void EndDrawing()
		{
			// Dump data to the target.
			Gorgon.Renderer.Render();

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
			VertexTypes.PositionDiffuse2DTexture1[] pointVertex = new VertexTypes.PositionDiffuse2DTexture1[1];

			if ((penWidth <= 1.0f) && (penHeight <= 1.0f))
			{
				// Build end points.
				pointVertex[0].Position.X = x;
				pointVertex[0].Position.Y = y;
				pointVertex[0].Position.Z = -0.5f;
                pointVertex[0].Color = color.ToArgb();

				if (_drawingPattern == null)
				{
					pointVertex[0].TextureCoordinates.X = 0.0f;
					pointVertex[0].TextureCoordinates.Y = 0.0f;
				}
				else
				{
					pointVertex[0].TextureCoordinates.X = (x - 0.5f) / (float)_drawingPattern.ActualWidth;
					pointVertex[0].TextureCoordinates.Y = (y - 0.5f) / (float)_drawingPattern.ActualHeight;
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
			VertexTypes.PositionDiffuse2DTexture1[] rectVertex = new VertexTypes.PositionDiffuse2DTexture1[5];

			// Build end points.
			rectVertex[0].Position.X = x;
			rectVertex[0].Position.Y = y;
			rectVertex[0].Position.Z = -0.5f;

			rectVertex[1].Position.X = x + width - 1;
			rectVertex[1].Position.Y = y;
			rectVertex[1].Position.Z = -0.5f;

			rectVertex[2].Position.X = x + width - 1;
			rectVertex[2].Position.Y = y + height - 1;
			rectVertex[2].Position.Z = -0.5f;

			rectVertex[3].Position.X = x;
			rectVertex[3].Position.Y = y + height - 1;
			rectVertex[3].Position.Z = -0.5f;

			rectVertex[4].Position.X = x;
			rectVertex[4].Position.Y = y;
			rectVertex[4].Position.Z = -0.5f;

			rectVertex[0].Color = color.ToArgb();
			rectVertex[1].Color = color.ToArgb();
			rectVertex[2].Color = color.ToArgb();
			rectVertex[3].Color = color.ToArgb();
			rectVertex[4].Color = color.ToArgb();

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
			}
			else
			{
				float tu1, tu2, tv1, tv2;		// texture coordinates.
				tu1 = (x - 0.5f) / (float)_drawingPattern.ActualWidth;
				tu2 = (x + width - 0.5f) / (float)_drawingPattern.ActualWidth;
				tv1 = (y - 0.5f) / (float)_drawingPattern.ActualHeight;
				tv2 = (y + height - 0.5f) / (float)_drawingPattern.ActualHeight;
				rectVertex[0].TextureCoordinates.X = tu1;
				rectVertex[0].TextureCoordinates.Y = tv1;
				rectVertex[1].TextureCoordinates.X = tu2;
				rectVertex[1].TextureCoordinates.Y = tv1;
				rectVertex[2].TextureCoordinates.X = tu2;
				rectVertex[2].TextureCoordinates.Y = tv2;
				rectVertex[3].TextureCoordinates.X = tu1;
				rectVertex[3].TextureCoordinates.Y = tv2;
				rectVertex[4].TextureCoordinates.X = tu1;
				rectVertex[4].TextureCoordinates.Y = tv1;
			}
			_useIndices = false;

			// Set the current primitive style.
			_currentPrimitiveStyle = PrimitiveStyle.LineStrip;

			// Send to the renderer.
			Draw(rectVertex);
		}

		/// <summary>
		/// Function to draw an unfilled rectangle.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="width">Width of the rectangle.</param>
		/// <param name="height">Height of the rectangle.</param>
		/// <param name="color">Color for the rectangle.</param>
		public void FilledRectangle(float x, float y, float width, float height, Drawing.Color color)
		{
			// Vertex to represent the line.
			VertexTypes.PositionDiffuse2DTexture1[] lineVertex = new VertexTypes.PositionDiffuse2DTexture1[4];

			// Build end points.
			lineVertex[0].Position.X = x;
			lineVertex[0].Position.Y = y;
			lineVertex[0].Position.Z = -0.5f;
			lineVertex[1].Position.X = x + width;
			lineVertex[1].Position.Y = y;
			lineVertex[1].Position.Z = -0.5f;
			lineVertex[2].Position.X = x + width;
			lineVertex[2].Position.Y = y + height;
			lineVertex[2].Position.Z = -0.5f;
			lineVertex[3].Position.X = x;
			lineVertex[3].Position.Y = y + height;
			lineVertex[3].Position.Z = -0.5f;

			lineVertex[0].Color = color.ToArgb();
			lineVertex[1].Color = color.ToArgb();
			lineVertex[2].Color = color.ToArgb();
			lineVertex[3].Color = color.ToArgb();

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
				tu1 = (float)(x - 0.5f) / (float)_drawingPattern.ActualWidth;
				tu2 = (float)(x + width - 0.5f) / (float)_drawingPattern.ActualWidth;
				tv1 = (float)(y - 0.5f) / (float)_drawingPattern.ActualHeight;
				tv2 = (float)(y + height - 0.5f) / (float)_drawingPattern.ActualHeight;
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
		public void HLine(float x, float y, float width, Drawing.Color color, float penWidth, float penHeight)
		{
			if ((penHeight <= 1.0f) && (penWidth <= 1.0f))
			{
				// Vertex to represent the line.
				VertexTypes.PositionDiffuse2DTexture1[] lineVertex = new VertexTypes.PositionDiffuse2DTexture1[2];

				// Build end points.
				lineVertex[0].Position.X = x;
				lineVertex[0].Position.Y = y;
				lineVertex[0].Position.Z = -0.5f;
				lineVertex[1].Position.X = x + width;
				lineVertex[1].Position.Y = y;
				lineVertex[1].Position.Z = -0.5f;
				lineVertex[0].Color = color.ToArgb();
				lineVertex[1].Color = color.ToArgb();

				if (_drawingPattern == null)
				{
					lineVertex[0].TextureCoordinates.X = 0.0f;
					lineVertex[0].TextureCoordinates.Y = 0.0f;
					lineVertex[1].TextureCoordinates.X = 0.0f;
					lineVertex[1].TextureCoordinates.Y = 0.0f;
				}
				else
				{
					lineVertex[0].TextureCoordinates.X = (x - 0.5f) / (float)_drawingPattern.ActualWidth;
					lineVertex[0].TextureCoordinates.Y = (y - 0.5f) / (float)_drawingPattern.ActualHeight;
					lineVertex[1].TextureCoordinates.X = (x + width - 0.5f) / (float)_drawingPattern.ActualWidth;
					lineVertex[1].TextureCoordinates.Y = (y - 0.5f) / (float)_drawingPattern.ActualHeight;
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
		public void HLine(float x, float y, float width, Drawing.Color color, Vector2D penSize)
		{
			HLine(x, y, width, color, penSize.X, penSize.Y);
		}

		/// <summary>
		/// Function to draw a horizontal line.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="width">Width of the line.</param>
		/// <param name="color">Color for the line.</param>
		public void HLine(float x, float y, float width, Drawing.Color color)
		{
			HLine(x, y, width, color, 1.0f, 1.0f);
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
		public void VLine(float x, float y, float height, Drawing.Color color, float penWidth, float penHeight)
		{
			if ((penHeight <= 1.0f) && (penWidth <= 1.0f))
			{
				// Vertex to represent the line.
				VertexTypes.PositionDiffuse2DTexture1[] lineVertex = new VertexTypes.PositionDiffuse2DTexture1[2];

				// Build end points.
				lineVertex[0].Position.X = x;
				lineVertex[0].Position.Y = y;
				lineVertex[0].Position.Z = -0.5f;
				lineVertex[1].Position.X = x;
				lineVertex[1].Position.Y = y + height;
				lineVertex[1].Position.Z = -0.5f;
				lineVertex[0].Color = color.ToArgb();
				lineVertex[1].Color = color.ToArgb();

				if (_drawingPattern == null)
				{
					lineVertex[0].TextureCoordinates.X = 0.0f;
					lineVertex[0].TextureCoordinates.Y = 0.0f;
					lineVertex[1].TextureCoordinates.X = 0.0f;
					lineVertex[1].TextureCoordinates.Y = 0.0f;
				}
				else
				{
					lineVertex[0].TextureCoordinates.X = (x - 0.5f) / (float)_drawingPattern.ActualWidth;
					lineVertex[0].TextureCoordinates.Y = (y - 0.5f) / (float)_drawingPattern.ActualHeight;
					lineVertex[1].TextureCoordinates.X = (x - 0.5f) / (float)_drawingPattern.ActualWidth;
					lineVertex[1].TextureCoordinates.Y = (y + height - 0.5f) / (float)_drawingPattern.ActualHeight;
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
		public void VLine(float x, float y, float height, Drawing.Color color, Vector2D penSize)
		{
			VLine(x, y, height, color, penSize.X, penSize.Y);
		}

		/// <summary>
		/// Function to draw a horizontal line.
		/// </summary>
		/// <param name="x">Starting horizontal position.</param>
		/// <param name="y">Starting vertical position.</param>
		/// <param name="height">Height of the line.</param>
		/// <param name="color">Color for the line.</param>
		public void VLine(float x, float y, float height, Drawing.Color color)
		{
			VLine(x, y, height, color, 1.0f, 1.0f);
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
		public void Circle(float x, float y, float radius, Drawing.Color colorValue, float penWidth, float penHeight)
		{
			float dx;			// X delta.
			float dy;			// Y delta.
			float d;			// Delta.

			dx = 0;
			dy = radius;
			d = 1 - radius;

			// Plot first 8 symmetry points.
			SetPoint(dx + x, dy + y, colorValue, penWidth, penHeight);
			SetPoint(dy + x, dx + y, colorValue, penWidth, penHeight);
			SetPoint(dy + x, -dx + y, colorValue, penWidth, penHeight);
			SetPoint(dx + x, -dy + y, colorValue, penWidth, penHeight);
			SetPoint(-dx + x, -dy + y, colorValue, penWidth, penHeight);
			SetPoint(-dy + x, -dx + y, colorValue, penWidth, penHeight);
			SetPoint(-dy + x, dx + y, colorValue, penWidth, penHeight);
			SetPoint(-dx + x, dy + y, colorValue, penWidth, penHeight);

			while (dy > dx)
			{
				if (d < 0)
				{
					d += (dx * 2.0f) + 3;
					dx++;
				}
				else
				{
					d += ((dx - dy) * 2.0f) + 5;
					dx++;
					dy--;
				}

				SetPoint(dx + x, dy + y, colorValue, penWidth, penHeight);
				SetPoint(dy + x, dx + y, colorValue, penWidth, penHeight);
				SetPoint(dy + x, -dx + y, colorValue, penWidth, penHeight);
				SetPoint(dx + x, -dy + y, colorValue, penWidth, penHeight);
				SetPoint(-dx + x, -dy + y, colorValue, penWidth, penHeight);
				SetPoint(-dy + x, -dx + y, colorValue, penWidth, penHeight);
				SetPoint(-dy + x, dx + y, colorValue, penWidth, penHeight);
				SetPoint(-dx + x, dy + y, colorValue, penWidth, penHeight);
			}
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
		/// Function to draw an empty circle.
		/// </summary>
		/// <param name="x">Starting horizontal position of the circle.</param>
		/// <param name="y">Starting vertical position of the circle.</param>
		/// <param name="radius">Radius of the circle.</param>
		/// <param name="colorValue">Color of the circle.</param>
		public void FilledCircle(float x, float y, float radius, Drawing.Color colorValue)
		{
			float dx;			// X delta.
			float dy;			// Y delta.
			float d;			// Delta.

			dx = 0;
			dy = radius;
			d = 1 - radius;

			// Plot first 8 symmetry points.
			HLine(x, dy + y, dx, colorValue);
			HLine(x, dx + y, dy, colorValue);
			HLine(x, -dx + y, dy, colorValue);
			HLine(x, -dy + y, dx, colorValue);
			HLine(x - dx, -dy + y, dx, colorValue);
			HLine(x - dy, -dx + y, dy, colorValue);
			HLine(x - dy, dx + y, dy, colorValue);
			HLine(x - dx, dy + y, dx, colorValue);

			while (dy > dx)
			{
				if (d < 0)
				{
					d += (dx * 2.0f) + 3;
					dx++;
				}
				else
				{
					d += ((dx - dy) * 2.0f) + 5;
					dx++;
					dy--;
				}

				HLine(x, dy + y, dx, colorValue);
				HLine(x, dx + y, dy, colorValue);
				HLine(x, -dx + y, dy, colorValue);
				HLine(x, -dy + y, dx, colorValue);
				HLine(x - dx, -dy + y, dx, colorValue);
				HLine(x - dy, -dx + y, dy, colorValue);
				HLine(x - dy, dx + y, dy, colorValue);
				HLine(x - dx, dy + y, dx, colorValue);
			}
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
		public void Ellipse(float x, float y, float xradius, float yradius, Drawing.Color color, float penWidth, float penHeight)
		{
			float t1 = xradius * xradius;		// Define some terms to gain some speed.
			float t2 = t1 * 4.0f;
			float t3 = t2 * 2.0f;
			float t4 = yradius * yradius;
			float t5 = t4 * 4.0f;
			float t6 = t5 * 2.0f;
			float t7 = xradius * t5;
			float t8 = t7 * 2.0f;
			float t9 = 0;
			float d1 = t2 - t7 + (t4 / 2.0f);	// Error terms.
			float d2 = (t1 / 2.0f) - t8 + t5;

			float xpos = xradius;
			float ypos = 0;

			while (d2 < 0)
			{
				// Draw the 4 symmetrical points.
				SetPoint(x + xpos, y + ypos, color, penWidth, penHeight);
				SetPoint(x + xpos, y - ypos, color, penWidth, penHeight);
				SetPoint(x - xpos, y + ypos, color, penWidth, penHeight);
				SetPoint(x - xpos, y - ypos, color, penWidth, penHeight);

				ypos++;	// Move up.

				t9 += t3;

				// Straight up.
				if (d1 < 0)
				{
					d1 += t9 + t2;
					d2 += t9;
				}
				else
				{
					// Go up and left.
					xpos--;
					t8 -= t6;
					d1 += t9 + t2 - t8;
					d2 += t9 + t5 - t8;
				}
			}

			while (xpos >= 0)
			{
				// Draw the 4 symmetrical points.
				SetPoint(x + xpos, y + ypos, color, penWidth, penHeight);
				SetPoint(x + xpos, y - ypos, color, penWidth, penHeight);
				SetPoint(x - xpos, y + ypos, color, penWidth, penHeight);
				SetPoint(x - xpos, y - ypos, color, penWidth, penHeight);

				xpos--;		// Move left.
				t8 -= t6;

				// go up and left.
				if (d2 < 0)
				{
					ypos++;
					t9 += t3;
					d2 += t9 + t5 - t8;
				}
				else
					d2 += t5 - t8;	// Move left.
			}
		}

		/// <summary>
		/// Function to draw a filled ellipse.
		/// </summary>
		/// <param name="x">Horizontal position of the ellipse.</param>
		/// <param name="y">Vertical position of the ellipse.</param>
		/// <param name="xradius">Horizontal radius.</param>
		/// <param name="yradius">Vertical radius.</param>
		/// <param name="colorValue">Color of the ellipse.</param>
		public void FilledEllipse(float x, float y, float xradius, float yradius, Drawing.Color colorValue)
		{
			float t1 = xradius * xradius;		// Define some terms to gain some speed.
			float t2 = t1 * 4.0f;
			float t3 = t2 * 2.0f;
			float t4 = yradius * yradius;
			float t5 = t4 * 4.0f;
			float t6 = t5 * 2.0f;
			float t7 = xradius * t5;
			float t8 = t7 * 2.0f;
			float t9 = 0;
			float d1 = t2 - t7 + (t4 / 2.0f);	// Error terms.
			float d2 = (t1 / 2.0f) - t8 + t5;

			float xpos = xradius;
			float ypos = 0;

			while (d2 < 0)
			{
				HLine(x, y + ypos, xpos, colorValue);
				HLine(x, y - ypos, xpos, colorValue);
				HLine(x - xpos, y + ypos, xpos, colorValue);
				HLine(x - xpos, y - ypos, xpos, colorValue);

				ypos++;	// Move up.

				t9 += t3;

				// Straight up.
				if (d1 < 0)
				{
					d1 += t9 + t2;
					d2 += t9;
				}
				else
				{
					// Go up and left.
					xpos--;
					t8 -= t6;
					d1 += t9 + t2 - t8;
					d2 += t9 + t5 - t8;
				}
			}

			while (xpos >= 0)
			{
				HLine(x, y + ypos, xpos, colorValue);
				HLine(x, y - ypos, xpos, colorValue);
				HLine(x - xpos, y + ypos, xpos, colorValue);
				HLine(x - xpos, y - ypos, xpos, colorValue);

				xpos--;		// Move left.
				t8 -= t6;

				// go up and left.
				if (d2 < 0)
				{
					ypos++;
					t9 += t3;
					d2 += t9 + t5 - t8;
				}
				else
					d2 += t5 - t8;	// Move left.
			}
		}

		/// <summary>
		/// Function to refresh this render window and its child windows.
		/// </summary>
		public void Refresh()
		{
			_timingData.Reset();
			// Reset the default window.
			if ((_defaultView.Width != _width) || (_defaultView.Height != _height) || (_defaultView.Left != 0) || (_defaultView.Top !=0))
				_defaultView.SetWindowDimensions(0, 0, _width, _height);
			_defaultView.Refresh();
		}

		/// <summary>
		/// Function to clear the screen, depth buffer and stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear the backbuffer with.</param>
		/// <param name="depthValue">Value to overwrite the depth buffer with.</param>
		/// <param name="stencilValue">Value to overwrite the stencil buffer with.</param>
		public void Clear(Drawing.Color color, float depthValue, int stencilValue)
		{
			Gorgon.Renderer.Clear(color, depthValue, stencilValue, _clearTargets | ClearTargets.BackBuffer);
		}

		/// <summary>
		/// Function to clear the screen, depth buffer and stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear the backbuffer with.</param>
		/// <param name="depthValue">Value to overwrite the depth buffer with.</param>
		public void Clear(Drawing.Color color, float depthValue)
		{
			Gorgon.Renderer.Clear(color, depthValue, 0, _clearTargets | ClearTargets.BackBuffer | ClearTargets.StencilBuffer);
		}

		/// <summary>
		/// Function to clear the screen, depth buffer and stencil buffer.
		/// </summary>
		/// <param name="color">Color to clear the backbuffer with.</param>
		public void Clear(Drawing.Color color)
		{
			Gorgon.Renderer.Clear(color, 1.0f, 0, ClearTargets.BackBuffer | ClearTargets.DepthBuffer | ClearTargets.StencilBuffer);
		}

		/// <summary>
		/// Function to clear the screen, depth buffer and stencil buffer.
		/// </summary>
		public void Clear()
		{
			Clear(Drawing.Color.Black);
		}

		/// <summary>
		/// Function to render the scene for this target.
		/// </summary>
		/// <returns>TRUE if updating is to continue, FALSE if not.</returns>
		public virtual void Update()
		{
			// Set this render target as the active target.
			Gorgon.Renderer.SetRenderTarget(this);

			_frameEventArguments.SetTargets(_defaultView, this);

			// Reset the clipper for the target.
			Gorgon.Renderer.SetActiveView(ProjectionMatrix, _defaultView);
			Gorgon.Renderer.CurrentClippingView = _defaultView;

			// Force the screen to clear.
			if (Gorgon.Screen == this)
				Gorgon.Renderer.Clear(BackgroundColor, ClearEachFrame | ClearTargets.BackBuffer);
			else
			{
				if ((ClearEachFrame & ClearTargets.None) == 0)
					Gorgon.Renderer.Clear(BackgroundColor, ClearEachFrame);
			}

			if (OnFrameBegin != null)
				OnFrameBegin(this, _frameEventArguments);

			// Draw the logo.
			if (Gorgon.LogoVisible)
				DrawLogo();
			if (Gorgon.FrameStatsVisible)
				DrawStats();
			Gorgon.Renderer.Render();

			if (OnFrameEnd != null)
				OnFrameEnd(this, _frameEventArguments);

			_timingData.Refresh();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="name">Name of this render target.</param>
		/// <param name="width">Width of the render target.</param>
		/// <param name="height">Height of the render target.</param>
		/// <param name="priority">Priority of the render target.</param>
		protected RenderTarget(string name,int width,int height,int priority) : base(name)
		{
			_width = width;
			_height = height;
			_priority = priority;
			_frameEventArguments = new FrameEventArgs();
			_backColor = Drawing.Color.White;			
			_timingData = new TimingData(Gorgon.Timer);
			_clearTargets = Gorgon.ClearEachFrame;
			_inheritClearTargets = true;
			_lastClipper = null;
			_lastTarget = null;
			_smoothing = Smoothing.None;
			_blending = Blending.None;
			_alphaFunction = CompareFunctions.Always;
			_alphaMaskValue = 0;
			_useIndices = false;
			_drawingPattern = null;
			_clipping = null;
			_wrapHMode = ImageAddressing.Clamp;
			_wrapVMode = ImageAddressing.Clamp;
			_projectionChanged = true;
			_orthoMatrix = Matrix.Identity;
            BlendingMode = Blending.None;

			// Create a default window.
			_defaultView = new Viewport(0, 0, 1, 1);

			Gorgon.DeviceStateList.Add(this);
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
				Gorgon.DeviceStateList.Remove(this);
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
		public abstract void DeviceLost();

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public abstract void DeviceReset();

		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		public abstract void ForceRelease();
		#endregion

		#region ILayerObject Members
		/// <summary>
		/// Property to set or return the clipping rectangle for this object.
		/// </summary>
		public Viewport ClippingViewport
		{
			get
			{
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
		bool IRenderable.InheritSmoothing
		{
			get
			{
                throw new NotImplementedException(NotImplementedTypes.Property, "InheritSmoothing [get]", null);				
			}
			set
			{
                throw new NotImplementedException(NotImplementedTypes.Property, "InheritSmoothing [set]", null);				
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit blending from the layer.
		/// </summary>
		bool IRenderable.InheritBlending
		{
			get
			{
                throw new NotImplementedException(NotImplementedTypes.Property, "InheritBlending [get]", null);
			}
			set
			{
                throw new NotImplementedException(NotImplementedTypes.Property, "InheritBlending [set]", null);
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the alpha mask function from the layer.
		/// </summary>
		bool IRenderable.InheritAlphaMaskFunction
		{
			get
			{
                throw new NotImplementedException(NotImplementedTypes.Property, "InheritAlphaMaskFunction [get]", null);
			}
			set
			{
                throw new NotImplementedException(NotImplementedTypes.Property, "InheritAlphaMaskFunction [set]", null);
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the alpha mask value from the layer.
		/// </summary>
		bool IRenderable.InheritAlphaMaskValue
		{
			get
			{
                throw new NotImplementedException(NotImplementedTypes.Property, "InheritAlphaMaskValue [get]", null);
			}
			set
			{
                throw new NotImplementedException(NotImplementedTypes.Property, "InheritAlphaMaskValue [set]", null);
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
		/// <value></value>
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
		public Blending BlendingMode
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
		/// Property to return the AABB for the object.
		/// </summary>
		System.Drawing.RectangleF IRenderable.AABB
		{
			get 
            {
                throw new NotImplementedException(NotImplementedTypes.Property, "AABB [get]", null);
            }
		}

		/// <summary>
		/// Property to return the primitive style for the object.
		/// </summary>
		PrimitiveStyle IRenderable.PrimitiveStyle
		{
			get
			{
				return _currentPrimitiveStyle;
			}
		}

		/// <summary>
		/// Property to return whether to use an index buffer or not.
		/// </summary>
		bool IRenderable.UseIndices
		{
			get
			{
				return _useIndices;
			}		
		}

		/// <summary>
		/// Property to set or return whether we inherit the horizontal wrapping from the layer.
		/// </summary>
		/// <value></value>
		bool IRenderable.InheritHorizontalWrapping
		{
			get
			{
				return false;
			}
			set
			{				
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the vertical wrapping from the layer.
		/// </summary>
		/// <value></value>
		bool IRenderable.InheritVerticalWrapping
		{
			get
			{
				return false;
			}
			set
			{
				
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil enabled flag from the layer.
		/// </summary>
		/// <value></value>
		bool IRenderable.InheritStencilEnabled
		{
			get
			{
				return false;
			}
			set
			{
				
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil reference from the layer.
		/// </summary>
		/// <value></value>
		bool IRenderable.InheritStencilReference
		{
			get
			{
				return false;
			}
			set
			{
				
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil mask from the layer.
		/// </summary>
		/// <value></value>
		bool IRenderable.InheritStencilMask
		{
			get
			{
				return false;
			}
			set
			{
				
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil pass operation from the layer.
		/// </summary>
		/// <value></value>
		bool IRenderable.InheritStencilPassOperation
		{
			get
			{
				return false;
			}
			set
			{
				
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil failed operation from the layer.
		/// </summary>
		/// <value></value>
		bool IRenderable.InheritStencilFailOperation
		{
			get
			{
				return false;
			}
			set
			{
				
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil z-failed operation from the layer.
		/// </summary>
		/// <value></value>
		bool IRenderable.InheritStencilZFailOperation
		{
			get
			{
				return false;
			}
			set
			{
				
			}
		}

		/// <summary>
		/// Property to set or return whether we inherit the stencil compare from the layer.
		/// </summary>
		/// <value></value>
		bool IRenderable.InheritStencilCompare
		{
			get
			{
				return false;
			}
			set
			{
				
			}
		}

		/// <summary>
		/// Property to set or return a shader effect for this object.
		/// </summary>
		Shader IRenderable.Shader
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

        /// <summary>
        /// Property to return the children of this object.
        /// </summary>
        RenderableChildren IRenderable.Children
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Property to return the parent of this object.
        /// </summary>
        IRenderable IRenderable.Parent
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Function to set the parent for this object.
        /// </summary>
        /// <param name="parent">Parent of this object.</param>
        void IRenderable.SetParent(IRenderable parent)
        {            
        }

        /// <summary>
        /// Function to draw the sprite.
        /// </summary>
        /// <param name="flush">TRUE to flush the buffers after drawing, FALSE to only flush on state change.</param>
        void IRenderable.Draw(bool flush)
        {
        }

        /// <summary>
        /// Function to update children.
        /// </summary>
        void IRenderable.UpdateChildren()
        {
        }
		
		/// <summary>
		/// Property to set or return the color.
		/// </summary>		
		Drawing.Color IRenderable.Color
		{
			get
			{
				return Drawing.Color.White;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the opacity.
		/// </summary>
		byte IRenderable.Opacity
		{
			get
			{
				return 255;
			}
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return the image that this object is bound with.
		/// </summary>
		Image IRenderable.Image
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
        #endregion
	}
}
