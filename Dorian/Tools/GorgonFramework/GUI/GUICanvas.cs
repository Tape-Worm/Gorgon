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
// Created: Sunday, July 13, 2008 1:29:21 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Control update event args.
	/// </summary>
	public class UpdateEventArgs
		: EventArgs
	{
		#region Variables.
		private GUICanvas _owner;			// Owner of the event.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the last frame draw time delta in milliseconds.
		/// </summary>
		public float FrameTimeDelta
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the drawing area in screen coordinates.
		/// </summary>
		public Viewport DrawArea
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the scale factor between the control view and the current render target.
		/// </summary>
		public Vector2D ViewScale
		{
			get
			{
				return new Vector2D((float)DrawArea.Width / (float)Gorgon.CurrentRenderTarget.Width, (float)DrawArea.Height / (float)Gorgon.CurrentRenderTarget.Height);
			}
		}

		/// <summary>
		/// Property to return the aspect ratio of the view.
		/// </summary>
		public float AspectRatio
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to convert a screen point to the canvas local coordinate system.
		/// </summary>
		/// <param name="coordinate">Screen point to convert.</param>
		/// <returns>The coordinate translated to the local coordinate system.</returns>
		public Vector2D ConvertToLocal(Vector2D coordinate)
		{
			return _owner.ScreenToPoint((Drawing.Point)coordinate);
		}

		/// <summary>
		/// Function to convert a local point to screen coordinates.
		/// </summary>
		/// <param name="coordinate">Local point to convert.</param>
		/// <returns>Screen coordinates of the local point.</returns>
		public Vector2D ConvertToScreen(Vector2D coordinate)
		{
			return _owner.PointToScreen((Drawing.Point)coordinate);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="UpdateEventArgs"/> class.
		/// </summary>
		/// <param name="owner">Owner of the event arguments.</param>
		/// <param name="frameTime">Last grame draw time in milliseconds</param>
		/// <param name="area">Drawing area in screen coordinates.</param>
		public UpdateEventArgs(GUICanvas owner, float frameTime, Drawing.Rectangle area)
		{
			_owner = owner;
			DrawArea = new Viewport(area.Left, area.Top, area.Width, area.Height);
			FrameTimeDelta = frameTime;
			if (DrawArea.Height > DrawArea.Width)
				AspectRatio = (float)DrawArea.Width / (float)DrawArea.Height;
			else
				AspectRatio = (float)DrawArea.Height / (float)DrawArea.Width;
		}
		#endregion
	}

	/// <summary>
	/// Delegate used for the draw event.
	/// </summary>
	/// <param name="sender">Sender of this event.</param>
	/// <param name="e">Event parameters.</param>
	public delegate void UpdateEventHandler(object sender, UpdateEventArgs e);

	/// <summary>
	/// A canvas control.
	/// </summary>
	/// <remarks>This control can be used to draw user defined graphics into a GUI control.</remarks>
	public class GUICanvas
		: GUIObject
	{
		#region Variables.
		private PanelBorderStyle _borderStyle;				// Border style.
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when the canvas is being redrawn.
		/// </summary>
		public event EventHandler CanvasDraw;
		/// <summary>
		/// Event fired when the canvas is being updated.
		/// </summary>
		public event UpdateEventHandler CanvasUpdate;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the border style for the label.
		/// </summary>
		public PanelBorderStyle BorderStyle
		{
			get
			{
				return _borderStyle;
			}
			set
			{
				_borderStyle = value;
				SetClientArea(WindowDimensions);
			}
		}

		/// <summary>
		/// Property to set or return the background color of the control.
		/// </summary>
		public Drawing.Color BackColor
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when a mouse event has taken place in this window.
		/// </summary>
		/// <param name="eventType">Type of event that should be fired.</param>
		/// <param name="e">Event parameters.</param>
		protected internal override void MouseEvent(MouseEventType eventType, GorgonLibrary.InputDevices.MouseInputEventArgs e)
		{
			DefaultMouseEvent(eventType, e);
		}

		/// <summary>
		/// Function called when a keyboard event has taken place in this window.
		/// </summary>
		/// <param name="isDown">TRUE if the button was pressed, FALSE if released.</param>
		/// <param name="e">Event parameters.</param>
		protected internal override void KeyboardEvent(bool isDown, GorgonLibrary.InputDevices.KeyboardInputEventArgs e)
		{
			DefaultKeyboardEvent(isDown, e);
		}

		/// <summary>
		/// Function to set the client area for the object.
		/// </summary>
		/// <param name="windowArea">Full area of the window.</param>
		protected override void SetClientArea(System.Drawing.Rectangle windowArea)
		{
			if (BorderStyle == PanelBorderStyle.Single)
			{
				windowArea.Width -= 2;
				windowArea.Height -= 2;
			}

			base.SetClientArea(windowArea);
		}

		/// <summary>
		/// Function to update the object.
		/// </summary>
		/// <param name="frameTime">Frame delta time.</param>
		/// <remarks>The frame delta is typically used with animations to help achieve a smooth appearance regardless of processor speed.</remarks>
		protected internal override void Update(float frameTime)
		{
			UpdateEventArgs e = null;
			Drawing.Rectangle clipArea = Drawing.Rectangle.Empty;

			clipArea = GetClippingArea(this, new Drawing.Rectangle(0, 0, ClientArea.Width, ClientArea.Height));
			e = new UpdateEventArgs(this, frameTime, clipArea);
			OnCanvasUpdate(this, e);
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		protected internal override void Draw()
		{
			Drawing.Point position = Owner.PointToScreen(WindowDimensions.Location);		// Screen position of the button.
			Drawing.Rectangle clipperDimensions = Drawing.Rectangle.Empty;					// Clipper dimensions.
			IGUIContainer container = Owner as IGUIContainer;								// GUI container.
			Drawing.Rectangle clipArea = Drawing.Rectangle.Empty;							// Clipping area.

			if ((container != null) && (container.ClipChildren))
				SetClippingRegion(GetClippingArea());

			Gorgon.CurrentRenderTarget.BeginDrawing();
			if (BorderStyle == PanelBorderStyle.Single)
				Gorgon.CurrentRenderTarget.Rectangle(position.X, position.Y, WindowDimensions.Width, WindowDimensions.Height, Drawing.Color.Black);
			Gorgon.CurrentRenderTarget.EndDrawing();

			DrawFocusRectangle();

			ResetClippingRegion();

			clipArea = GetClippingArea(this, new Drawing.Rectangle(0, 0, ClientArea.Width, ClientArea.Height));
			if ((container != null) && (container.ClipChildren))
			{
			    ResetClippingRegion();
				SetClippingRegion(clipArea);
			}

			if ((clipArea.Width < 1) || (clipArea.Height < 1))
				SetClippingRegion(new Drawing.Rectangle(0, 0, 1, 1));
				
			Gorgon.CurrentRenderTarget.Clear(BackColor);			
			OnCanvasDraw(this, EventArgs.Empty);
	
			if ((container != null) && (container.ClipChildren))
			    ResetClippingRegion();
		}

		/// <summary>
		/// Function called when the canvas is about to be updated.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		protected void OnCanvasUpdate(object sender, UpdateEventArgs e)
		{
			if (CanvasUpdate != null)
				CanvasUpdate(sender, e);
		}

		/// <summary>
		/// Function called when the canvas is drawing.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		protected void OnCanvasDraw(object sender, EventArgs e)
		{
			if (CanvasDraw != null)
				CanvasDraw(sender, e);
		}

		/// <summary>
		/// Function to convert a client point to screen coordinates.
		/// </summary>
		/// <param name="clientPoint">Client point to convert.</param>
		/// <returns>The screen coordinates of the point.</returns>
		public override System.Drawing.Point PointToScreen(System.Drawing.Point clientPoint)
		{
			Drawing.Point newPoint;		// New point.

			newPoint = base.PointToScreen(clientPoint);
			if (BorderStyle == PanelBorderStyle.Single)
			{
				newPoint.X += 1;
				newPoint.Y += 1;
			}

			return newPoint;
		}

		/// <summary>
		/// Function to convert a screen point to client coordinates.
		/// </summary>
		/// <param name="screenPoint">Screen point to convert.</param>
		/// <returns>The client coordinates of the point.</returns>
		public override System.Drawing.Point ScreenToPoint(System.Drawing.Point screenPoint)
		{
			if (BorderStyle == PanelBorderStyle.Single)
			{
				screenPoint.X -= 1;
				screenPoint.Y -= 1;
			}
			return base.ScreenToPoint(screenPoint);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GUICanvas"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public GUICanvas(string name)
			: base(name)
		{
			CanFocus = true;
			BackColor = Drawing.Color.Transparent;
			Visible = true;
			Font = null;
		}
		#endregion
	}
}
