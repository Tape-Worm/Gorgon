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
// Created: Sunday, July 13, 2008 1:10:56 PM
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
	/// Label control.
	/// </summary>
	public class GUILabel
		: GUIObject
	{
		#region Variables.
		private TextSprite _textLabel = null;				// Text sprite to draw on the button.
		private string _text = string.Empty;				// Text to display on the button.
		private Viewport _textClipper = null;				// Clipping view of the text label.
		private PanelBorderStyle _borderStyle;				// Border style.
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

		/// <summary>
		/// Property to set or return the foreground color of the button.
		/// </summary>
		public Drawing.Color ForeColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the text for the button.
		/// </summary>
		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if (value == null)
					value = string.Empty;
				_text = value;
			}
		}

		/// <summary>
		/// Property to set or return the text alignment.
		/// </summary>
		public Alignment TextAlignment
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
			Drawing.Rectangle screenDims = Owner.RectToScreen(WindowDimensions);

			_textLabel.Bounds = _textClipper;
			_textLabel.Text = Text;
			_textLabel.Color = ForeColor;
			_textLabel.Alignment = TextAlignment;
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		protected internal override void Draw()
		{
			Drawing.Point position = Owner.PointToScreen(WindowDimensions.Location);		// Screen position of the button.
			Drawing.Rectangle clipperDimensions = Drawing.Rectangle.Empty;					// Clipper dimensions.
			IGUIContainer container = Owner as IGUIContainer;								// GUI container.

			if ((container != null) && (container.ClipChildren))
				SetClippingRegion(GetClippingArea());

			Gorgon.CurrentRenderTarget.BeginDrawing();
			if (BackColor.A > 0)
				Gorgon.CurrentRenderTarget.FilledRectangle(position.X, position.Y, WindowDimensions.Width, WindowDimensions.Height, BackColor);
			if (BorderStyle == PanelBorderStyle.Single)
				Gorgon.CurrentRenderTarget.Rectangle(position.X, position.Y, WindowDimensions.Width, WindowDimensions.Height, Drawing.Color.Black);
			Gorgon.CurrentRenderTarget.EndDrawing();

			_textLabel.Position = new Vector2D(position.X, position.Y);

			if ((container != null) && (container.ClipChildren))
			{
				ResetClippingRegion();
				SetClippingRegion(GetClippingArea(this, new Drawing.Rectangle(0, 0, ClientArea.Width, ClientArea.Height)));
			}

			_textClipper.SetWindowDimensions(Gorgon.CurrentClippingViewport.Left, Gorgon.CurrentClippingViewport.Top, ClientArea.Width, ClientArea.Height);
			_textLabel.Draw();

			if ((container != null) && (container.ClipChildren))
				ResetClippingRegion();
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
		/// Initializes a new instance of the <see cref="GUILabel"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public GUILabel(string name)
			: base(name)
		{
			BackColor = Drawing.Color.Transparent;
			Visible = true;
			Font = null;
			ForeColor = Drawing.Color.Black;
			TextAlignment = Alignment.UpperLeft;
			_textClipper = new Viewport(0, 0, 1, 1);
			_textLabel = new TextSprite(name + "Label." + Guid.NewGuid().ToString(), name, Font);			
		}
		#endregion
	}
}
