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
// Created: Sunday, July 13, 2008 12:31:42 PM
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
	/// Checkbox control.
	/// </summary>
	public class GUICheckBox
		: GUIObject
	{
		#region Variables.
		private TextSprite _textLabel = null;				// Text sprite to draw on the button.
		private string _text = string.Empty;				// Text to display on the button.
		private Viewport _textClipper = null;				// Clipping view of the text label.
		private bool _checked = false;						// Check state.
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when the checkbox is clicked.
		/// </summary>
		public event EventHandler Clicked;
		/// <summary>
		/// Event fired when the value of the checkbox is changed.
		/// </summary>
		public event EventHandler ValueChanged;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the checkbox is in a checked state or not.
		/// </summary>
		public bool Checked
		{
			get
			{
				return _checked;
			}
			set
			{
				_checked = value;
				OnCheckChanged(this, EventArgs.Empty);
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
		/// Function called when the checked value is changed.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		protected virtual void OnCheckChanged(object sender, EventArgs e)
		{
			if (ValueChanged != null)
				ValueChanged(sender, e);
		}


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

			switch (eventType)
			{
				case MouseEventType.MouseButtonUp:
					OnClick(this, EventArgs.Empty);
					break;
				case MouseEventType.MouseButtonDown:
					Checked = !Checked;
					break;
			}
		}

		/// <summary>
		/// Function called when a keyboard event has taken place in this window.
		/// </summary>
		/// <param name="isDown">TRUE if the button was pressed, FALSE if released.</param>
		/// <param name="e">Event parameters.</param>
		protected internal override void KeyboardEvent(bool isDown, GorgonLibrary.InputDevices.KeyboardInputEventArgs e)
		{
			if ((isDown) && (e.Key == GorgonLibrary.InputDevices.KeyboardKeys.Space))
				Checked = !Checked;
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
			int boxY = 0;																	// Box Y position.
			int checkDiff = 0;																// Check height difference.

			if ((container != null) && (container.ClipChildren))
				SetClippingRegion(GetClippingArea());
			boxY = position.Y + (WindowDimensions.Height / 2);

			if (BackColor.A > 0)
			{
				Gorgon.CurrentRenderTarget.BeginDrawing();
				Gorgon.CurrentRenderTarget.FilledRectangle(position.X, position.Y, WindowDimensions.Width, WindowDimensions.Height, BackColor);
				Gorgon.CurrentRenderTarget.EndDrawing();
			}

			if (!IsCursorOnTop)
			{
				checkDiff = Skin.Elements["Controls.CheckBox.Check"].Dimensions.Height - Skin.Elements["Controls.CheckBox.Box"].Dimensions.Height;
				boxY -= Skin.Elements["Controls.CheckBox.Box"].Dimensions.Height / 2;
				Skin.Elements["Controls.CheckBox.Box"].Draw(new Drawing.Rectangle(position.X, boxY, Skin.Elements["Controls.CheckBox.Box"].Dimensions.Width, Skin.Elements["Controls.CheckBox.Box"].Dimensions.Height));
				clipperDimensions = new Drawing.Rectangle(Skin.Elements["Controls.CheckBox.Box"].Dimensions.Width, 0, ClientArea.Width - Skin.Elements["Controls.CheckBox.Box"].Dimensions.Width, ClientArea.Height);
				_textLabel.Position = new Vector2D(position.X + Skin.Elements["Controls.CheckBox.Box"].Dimensions.Width, position.Y);
			}
			else
			{
				checkDiff = Skin.Elements["Controls.CheckBox.Check"].Dimensions.Height - Skin.Elements["Controls.CheckBox.Box.Hover"].Dimensions.Height;
				boxY -= Skin.Elements["Controls.CheckBox.Box.Hover"].Dimensions.Height / 2;
				Skin.Elements["Controls.CheckBox.Box.Hover"].Draw(new Drawing.Rectangle(position.X, boxY, Skin.Elements["Controls.CheckBox.Box.Hover"].Dimensions.Width, Skin.Elements["Controls.CheckBox.Box.Hover"].Dimensions.Height));
				clipperDimensions = new Drawing.Rectangle(Skin.Elements["Controls.CheckBox.Box.Hover"].Dimensions.Width, 0, ClientArea.Width - Skin.Elements["Controls.CheckBox.Box.Hover"].Dimensions.Width, ClientArea.Height);
				_textLabel.Position = new Vector2D(position.X + Skin.Elements["Controls.CheckBox.Box.Hover"].Dimensions.Width, position.Y);
			}

			DrawFocusRectangle();

			if (Checked)
				Skin.Elements["Controls.CheckBox.Check"].Draw(new Drawing.Rectangle(position.X, boxY - checkDiff, Skin.Elements["Controls.CheckBox.Check"].Dimensions.Width, Skin.Elements["Controls.CheckBox.Check"].Dimensions.Height));

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
		/// Initializes a new instance of the <see cref="GUICheckBox"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public GUICheckBox(string name)
			: base(name)
		{
			CanFocus = true;
			BackColor = Drawing.Color.Transparent;
			Visible = true;
			Font = null;
			ForeColor = Drawing.Color.Black;
			TextAlignment = Alignment.UpperLeft;
			_textClipper = new Viewport(0, 0, 1, 1);
			_textLabel = new TextSprite(name + "CheckBox." + Guid.NewGuid().ToString(), name, Font);			
		}
		#endregion
	}
}
