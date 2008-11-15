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
// Created: Friday, July 29, 2005 10:29:47 PM
// 
#endregion

using System;
using System.Drawing;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Class to represent a virtual window within a rendering window.
	/// </summary>
	public class Viewport
	{
		#region Variables.
		private Rectangle _windowRect;		        // Requested window.
		private Rectangle  _viewPort;		        // Final viewport.
		private Matrix _orthoMatrix;				// Orthogonal projection matrix.
		private bool _projectionChanged;			// Projection matrix changed?
		private bool _updated;						// Flag to indicate that the view is updated.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether to use scissor testing for window clipping.
		/// </summary>
		public bool UseScissorTestClipping
		{
			get
			{
				if (Gorgon.CurrentDriver != default(Driver))
					return Gorgon.CurrentDriver.SupportScissorTesting;
				else
					return false;
			}
		}

		/// <summary>
		/// Property to return the view port rectangle.
		/// </summary>
		public Rectangle View
		{
			get
			{
				if ((_viewPort == Rectangle.Empty) || (_updated))
				{
					Refresh(Gorgon.CurrentRenderTarget);
					_projectionChanged = true;
				}

				return _viewPort;
			}
		}

		/// <summary>
		/// Property to return whether or not the view has been updated or not.
		/// </summary>
		public bool Updated
		{
			get
			{
				return _updated;
			}
			set
			{
				_updated = value;
			}
		}

		/// <summary>
		/// Property to set or return the horizontal window position.
		/// </summary>
		public int Left
		{
			get
			{
				return _windowRect.X;				
			}
			set
			{

				_windowRect.X = value;
				Refresh(Gorgon.CurrentRenderTarget);
			}
		}

		/// <summary>
		/// Property to set or return the vertical window position.
		/// </summary>
		public int Top
		{
			get
			{
				return _windowRect.Y;
			}
			set
			{

				_windowRect.Y = value;
				Refresh(Gorgon.CurrentRenderTarget);
			}
		}

		/// <summary>
		/// Property to set or return the width of the window.
		/// </summary>
		public int Width
		{
			get
			{
				return _windowRect.Width;
			}
			set
			{

				_windowRect.Width = value;
				_projectionChanged = true;
				Refresh(Gorgon.CurrentRenderTarget);
			}
		}

		/// <summary>
		/// Property to set or return the height of the window.
		/// </summary>
		public int Height
		{
			get
			{
				return _windowRect.Height;
			}
			set
			{
				_windowRect.Height = value;
				_projectionChanged = true;
				Refresh(Gorgon.CurrentRenderTarget);
			}
		}

		/// <summary>
		/// Property to return the orthogonal projection matrix.
		/// </summary>
		public Matrix ProjectionMatrix
		{
			get
			{
				if ((_viewPort == Rectangle.Empty) || (_updated))
				{
					this.Refresh(Gorgon.CurrentRenderTarget);
					_projectionChanged = true;
				}

				if (_projectionChanged)
				{
					_orthoMatrix = Gorgon.Renderer.CreateOrthoProjectionMatrix(_viewPort.Left, _viewPort.Top, _viewPort.Width, _viewPort.Height);					
					_projectionChanged = false;
				}
				
				return _orthoMatrix;
			}
		}

		/// <summary>
		/// Property to set or return the window dimensions.
		/// </summary>
		public Rectangle Dimensions
		{
			get
			{
				return _windowRect;
			}
			set
			{
				if ((value.Width != _windowRect.Width) || (value.Height != _windowRect.Height))
					_projectionChanged = true;
				_windowRect = value;
				Refresh(Gorgon.CurrentRenderTarget);
			}
		}

		/// <summary>
		/// Property to return the window dimensions clipped to the current render target.
		/// </summary>
		public Rectangle ClippedDimensions
		{
			get
			{
				Rectangle targetDimensions = Rectangle.Empty;			// Render target dimensions.
				Rectangle dimensions = Rectangle.Empty;					// Dimensions.

				if (Gorgon.CurrentRenderTarget == null)
					return Rectangle.Empty;

				dimensions = _windowRect;
				targetDimensions = new Rectangle(0, 0, Gorgon.CurrentRenderTarget.Width, Gorgon.CurrentRenderTarget.Height);

				// Clip to the target if necessary.
				if (!targetDimensions.Contains(dimensions))
				{
					if (dimensions.X < 0)
					{
						dimensions.Width -= -dimensions.X;
						_projectionChanged = true;
						dimensions.X = 0;						
					}
					if (dimensions.X >= targetDimensions.Width)
						dimensions.X = targetDimensions.Width - 1;
					if (dimensions.Y >= targetDimensions.Height)
						dimensions.Y = targetDimensions.Height - 1;
					if (dimensions.Y < 0)
					{
						dimensions.Height -= -dimensions.Y;
						_projectionChanged = true;
						dimensions.Y = 0;
					}
					if (dimensions.Right > targetDimensions.Width)
					{
						dimensions.Width = dimensions.Width - (dimensions.Right - targetDimensions.Width);
						_projectionChanged = true;
					}
					if (dimensions.Bottom > targetDimensions.Height)
					{
						dimensions.Height = dimensions.Height - (dimensions.Bottom - targetDimensions.Height);
						_projectionChanged = true;
					}
				}				

				return dimensions;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to set the window dimensions.
		/// </summary>
		/// <param name="left">Relative horizontal position of the window.</param>
		/// <param name="top">Relative vertical position of the window.</param>
		/// <param name="width">Relative width of the window.</param>
		/// <param name="height">Relative height of the window.</param>
		public void SetWindowDimensions(int left,int top,int width,int height)
		{
			_windowRect.X = left;
			_windowRect.Y = top;
			if ((width != _windowRect.Width) || (height != _windowRect.Height))
				_projectionChanged = true;
			_windowRect.Width = width;
			_windowRect.Height = height;
			Refresh(Gorgon.CurrentRenderTarget);
		}
		
		/// <summary>
		/// Function to set the window dimensions.
		/// </summary>
		/// <param name="dimensions">Relative dimensions of the window.</param>
		public void SetWindowDimensions(Rectangle dimensions)
		{
			SetWindowDimensions(dimensions.X, dimensions.Y, dimensions.Width, dimensions.Height);
		}

		/// <summary>
		/// Function to refresh the window.
		/// </summary>
		/// <param name="renderTarget">Render target to use.</param>
		public void Refresh(RenderTarget renderTarget)
		{
			Rectangle target;		// Render target dimensions.

			if (!Gorgon.IsInitialized)
				return;

			// Do nothing without an active render target.
			if (renderTarget == null) 
				return;

			_updated = true;

			// Ensure the view fits within the render target.
			target = new Rectangle(0, 0, renderTarget.Width, renderTarget.Height);

			// Constrain to the render target size.
			_viewPort = ClippedDimensions;

			// Don't allow a viewport without an area.
			if (_viewPort.Width == 0)
				_viewPort.Width = 1;				
			if (_viewPort.Height == 0)
				_viewPort.Height = 1;

			_projectionChanged = true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="left">Relative horizontal position of the window.</param>
		/// <param name="top">Relative vertical position of the window.</param>
		/// <param name="width">Relative width of the window.</param>
		/// <param name="height">Relative height of the window.</param>
		public Viewport(int left,int top,int width,int height)
		{
			_viewPort = Rectangle.Empty;
			_windowRect = new Rectangle(0, 0, 0, 0);
			_orthoMatrix = Matrix.Identity;
			_projectionChanged = true;
			SetWindowDimensions(left, top, width, height);
		}
		#endregion
	}
}
