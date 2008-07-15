﻿#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Sunday, July 06, 2008 11:27:56 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary.Graphics;
using GorgonLibrary.InputDevices;

namespace GorgonLibrary.GUI
{
	/// <summary>
	/// Button object for the GUI system.
	/// </summary>
	public class GUIButton
		: GUIObject
	{
		#region Variables.
		private TextSprite _textLabel = null;				// Text sprite to draw on the button.
		private string _text = string.Empty;				// Text to display on the button.
		private Viewport _textClipper = null;				// Clipping view of the text label.
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when the button is clicked.
		/// </summary>
		public event EventHandler Clicked;
		#endregion

		#region Properties.
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
		/// Function called when the button is clicked.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event arguments.</param>
		protected virtual void OnClick(object sender, EventArgs e)
		{
			if (Clicked != null)
				Clicked(sender, e);
		}

		/// <summary>
		/// Function called when a mouse event has taken place in this window.
		/// </summary>
		/// <param name="eventType">Type of event that should be fired.</param>
		/// <param name="e">Event parameters.</param>
		protected internal override void MouseEvent(MouseEventType eventType, GorgonLibrary.InputDevices.MouseInputEventArgs e)
		{
			DefaultMouseEvent(eventType, e);

			if (eventType == MouseEventType.MouseButtonUp)
				OnClick(this, EventArgs.Empty);
		}

		/// <summary>
		/// Function called when a keyboard event has taken place in this window.
		/// </summary>
		/// <param name="isDown">TRUE if the button was pressed, FALSE if released.</param>
		/// <param name="e">Event parameters.</param>
		protected internal override void KeyboardEvent(bool isDown, GorgonLibrary.InputDevices.KeyboardInputEventArgs e)
		{
			if ((isDown) && (e.Key == KeyboardKeys.Space))
				OnClick(this, EventArgs.Empty);
			else
				DefaultKeyboardEvent(isDown, e);
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
			if (!IsCursorOnTop)
			{
				Skin.Elements["Controls.Button.Left"].Draw(new Drawing.Rectangle(position.X, position.Y, Skin.Elements["Controls.Button.Left"].Dimensions.Width, WindowDimensions.Height));
				Skin.Elements["Controls.Button.Right"].Draw(new Drawing.Rectangle(position.X + WindowDimensions.Width - Skin.Elements["Controls.Button.Right"].Dimensions.Width, position.Y, Skin.Elements["Controls.Button.Right"].Dimensions.Width, WindowDimensions.Height));
				Skin.Elements["Controls.Button.Body"].Draw(new Drawing.Rectangle(position.X + Skin.Elements["Controls.Button.Left"].Dimensions.Width, position.Y, WindowDimensions.Width - Skin.Elements["Controls.Button.Right"].Dimensions.Width - Skin.Elements["Controls.Button.Left"].Dimensions.Width, WindowDimensions.Height));
				clipperDimensions = new Drawing.Rectangle(Skin.Elements["Controls.Button.Left"].Dimensions.Width, 0, WindowDimensions.Width - Skin.Elements["Controls.Button.Right"].Dimensions.Width - Skin.Elements["Controls.Button.Left"].Dimensions.Width, WindowDimensions.Height);
				_textLabel.Position = new Vector2D(position.X + Skin.Elements["Controls.Button.Left"].Dimensions.Width, position.Y);
			}
			else
			{
				Skin.Elements["Controls.Button.Hover.Left"].Draw(new Drawing.Rectangle(position.X, position.Y, Skin.Elements["Controls.Button.Hover.Left"].Dimensions.Width, WindowDimensions.Height));
				Skin.Elements["Controls.Button.Hover.Right"].Draw(new Drawing.Rectangle(position.X + WindowDimensions.Width - Skin.Elements["Controls.Button.Hover.Right"].Dimensions.Width, position.Y, Skin.Elements["Controls.Button.Hover.Right"].Dimensions.Width, WindowDimensions.Height));
				Skin.Elements["Controls.Button.Hover.Body"].Draw(new Drawing.Rectangle(position.X + Skin.Elements["Controls.Button.Hover.Left"].Dimensions.Width, position.Y, WindowDimensions.Width - Skin.Elements["Controls.Button.Hover.Right"].Dimensions.Width - Skin.Elements["Controls.Button.Hover.Left"].Dimensions.Width, WindowDimensions.Height));
				clipperDimensions = new Drawing.Rectangle(Skin.Elements["Controls.Button.Hover.Left"].Dimensions.Width, 0, WindowDimensions.Width - Skin.Elements["Controls.Button.Hover.Right"].Dimensions.Width - Skin.Elements["Controls.Button.Hover.Left"].Dimensions.Width, WindowDimensions.Height);
				_textLabel.Position = new Vector2D(position.X + Skin.Elements["Controls.Button.Hover.Left"].Dimensions.Width, position.Y);
			}

			DrawFocusRectangle();

			if ((container != null) && (container.ClipChildren))
			{
				ResetClippingRegion();
				SetClippingRegion(GetClippingArea(this, clipperDimensions));
			}
			_textClipper.SetWindowDimensions(Gorgon.CurrentClippingViewport.Left, Gorgon.CurrentClippingViewport.Top, clipperDimensions.Width, clipperDimensions.Height);
			_textLabel.Draw();
			if ((container != null) && (container.ClipChildren))
				ResetClippingRegion();			
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GUIButton"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public GUIButton(string name)
			: base(name)
		{
			CanFocus = true;
			Visible = true;
			Font = null;
			ForeColor = Drawing.Color.White;
			TextAlignment = Alignment.Center;
			_textClipper = new Viewport(0, 0, 1, 1);
			_textLabel = new TextSprite(name + "ButtonFont." + Guid.NewGuid().ToString(), name, Font);			
		}
		#endregion
	}
}
