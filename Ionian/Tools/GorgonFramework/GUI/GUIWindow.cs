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
// Created: Tuesday, May 27, 2008 6:52:46 PM
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
	/// Object representing a GUI window.
	/// </summary>
	public class GUIWindow
		: GUIObject, IGUIContainer
	{
		#region Variables.
		private GUIObjectCollection _guiObjects = null;					// List of objects contained in the window.		
		private TextSprite _captionTextLabel = null;					// Caption text.
		private bool _dragging = false;									// Flag to indicate that we're dragging.
		private Drawing.Rectangle _captionRectangle;					// Caption rectangle.
		private Vector2D _dragDelta = Vector2D.Zero;					// Drag delta.
		private Viewport _clipView = null;								// Clipping view port.
		private bool _hasCaption = true;								// Flag to indicate whether we have a caption on this window or not.
		private GUIObject _focused = null;								// Focused control.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to determine if the mouse is over the caption.
		/// </summary>
		private bool IsMouseOverCaption
		{
			get
			{
				if (_captionRectangle == Drawing.Rectangle.Empty)
					return false;

				if ((Desktop != null) && (_captionRectangle.Contains((Drawing.Point)Desktop.MousePosition)))
					return true;
				else
					return false;
			}
		}

		/// <summary>
		/// Property to return the focused control for this window.
		/// </summary>
		public GUIObject FocusedControl
		{
			get
			{
				return _focused;
			}
			internal set
			{
				_focused = value;
			}
		}

		/// <summary>
		/// Property to set or return the background color of the panel.
		/// </summary>
		public Drawing.Color BackgroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the window is able to move or not.
		/// </summary>
		public bool CanMove
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the window has a caption.
		/// </summary>
		public bool HasCaption
		{
			get
			{
				return _hasCaption;
			}
			set
			{
				_hasCaption = value;
				SkinUpdated();
			}
		}
		
		/// <summary>
		/// Property to return the default border size.
		/// </summary>
		public int DefaultBorderSize
		{
			get
			{
				if (Skin == null)
					return 0;
				else
					return Skin.Elements["Window.Border.Vertical.Left"].Dimensions.Width;
			}
		}

		/// <summary>
		/// Property to return the default caption height.
		/// </summary>
		public int DefaultCaptionHeight
		{
			get
			{
				if (Skin == null)
					return Font.LineHeight + 4;
				else
				{
					if (HasCaption)
					{
						if ((Font.LineHeight - Skin.Elements["Window.Caption"].Dimensions.Height) > 2)
							return (Skin.Elements["Window.Caption"].Dimensions.Height) + (Font.LineHeight - Skin.Elements["Window.Caption"].Dimensions.Height);
						else
							return Skin.Elements["Window.Caption"].Dimensions.Height;
					}
					else
						return Skin.Elements["Window.Border.Top.Horizontal"].Dimensions.Height;
				}
			}
		}

		/// <summary>
		/// Property to return the default border height.
		/// </summary>
		public int DefaultBorderHeight
		{
			get
			{
				if (Skin == null)
					return 0;
				else
					return Skin.Elements["Window.Border.Bottom.Horizontal"].Dimensions.Height;
			}
		}

		/// <summary>
		/// Property to set or return the owner object for this object.
		/// </summary>
		public override GUIObject Owner
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException("GUI windows cannot be owned.");
			}
		}

		/// <summary>
		/// Property to set or return the caption for the window.
		/// </summary>
		public string Text
		{
			get
			{
				return _captionTextLabel.Text;
			}
			set
			{
				if (value == null)
					value = string.Empty;
				_captionTextLabel.Text = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve all controls in the focused window.
		/// </summary>
		/// <param name="controls">Control list to populate.</param>
		/// <param name="container">Container that owns the control.</param>
		private void GetAllControls(List<GUIObject> controls, IGUIContainer container)
		{
			for (int i = 0; i < container.GUIObjects.Count; i++)
			{
				IGUIContainer ownerContainer = container.GUIObjects[i] as IGUIContainer;
				if (ownerContainer != null)
					GetAllControls(controls, ownerContainer);

				controls.Add(container.GUIObjects[i]);
			}
		}

		/// <summary>
		/// Function to draw the non-client area.
		/// </summary>
		private void DrawNonClientArea()
		{
			Vector2D nonClientPosition = Position;
			Drawing.Rectangle captionScreen = Drawing.Rectangle.Empty;
			Viewport lastView = null;

			if (Skin == null)
				return;
			
			if (HasCaption)
			{
				_captionRectangle = new Drawing.Rectangle(Position.X + Skin.Elements["Window.Caption.LeftCorner"].Dimensions.Width, Position.Y,
									WindowDimensions.Width - Skin.Elements["Window.Caption.LeftCorner"].Dimensions.Width - Skin.Elements["Window.Caption.RightCorner"].Dimensions.Width,
									DefaultCaptionHeight);
				captionScreen = _captionRectangle;

				Skin.Elements["Window.Caption.LeftCorner"].Draw(new Drawing.Rectangle(captionScreen.Location.X - Skin.Elements["Window.Caption.LeftCorner"].Dimensions.Width, captionScreen.Location.Y, Skin.Elements["Window.Caption.LeftCorner"].Dimensions.Width, captionScreen.Size.Height));
				Skin.Elements["Window.Caption.RightCorner"].Draw(new Drawing.Rectangle(captionScreen.Width + captionScreen.X, captionScreen.Location.Y, Skin.Elements["Window.Caption.RightCorner"].Dimensions.Width, captionScreen.Size.Height));
				Skin.Elements["Window.Caption"].Draw(captionScreen);
			}
			else
			{
				Skin.Elements["Window.Border.Top.LeftCorner"].Draw(new Drawing.Rectangle(Position.X, Position.Y, Skin.Elements["Window.Border.Top.LeftCorner"].Dimensions.Width, Skin.Elements["Window.Border.Top.LeftCorner"].Dimensions.Height));
				Skin.Elements["Window.Border.Top.Horizontal"].Draw(new Drawing.Rectangle(Position.X + Skin.Elements["Window.Border.Top.LeftCorner"].Dimensions.Width, Position.Y, WindowDimensions.Width - Skin.Elements["Window.Border.Top.RightCorner"].Dimensions.Width - Skin.Elements["Window.Border.Top.RightCorner"].Dimensions.Width, Skin.Elements["Window.Border.Top.Horizontal"].Dimensions.Height));
				Skin.Elements["Window.Border.Top.RightCorner"].Draw(new Drawing.Rectangle(WindowDimensions.Right - Skin.Elements["Window.Border.Top.RightCorner"].Dimensions.Width, Position.Y, Skin.Elements["Window.Border.Top.RightCorner"].Dimensions.Width, Skin.Elements["Window.Border.Top.RightCorner"].Dimensions.Height));
			}

			Skin.Elements["Window.Border.Vertical.Left"].Draw(new Drawing.Rectangle(Position.X, DefaultCaptionHeight + Position.Y, DefaultBorderSize, WindowDimensions.Height - DefaultCaptionHeight - DefaultBorderHeight));
			Skin.Elements["Window.Border.Vertical.Right"].Draw(new Drawing.Rectangle(Position.X + WindowDimensions.Width - Skin.Elements["Window.Border.Vertical.Right"].Dimensions.Width, DefaultCaptionHeight + Position.Y, DefaultBorderSize, WindowDimensions.Height - DefaultCaptionHeight - DefaultBorderHeight));

			Skin.Elements["Window.Border.Bottom.LeftCorner"].Draw(new Drawing.Rectangle(Position.X, Position.Y + WindowDimensions.Height - Skin.Elements["Window.Border.Bottom.LeftCorner"].Dimensions.Height, Skin.Elements["Window.Border.Bottom.LeftCorner"].Dimensions.Width, Skin.Elements["Window.Border.Bottom.LeftCorner"].Dimensions.Height));
			Skin.Elements["Window.Border.Bottom.Horizontal"].Draw(new Drawing.Rectangle(Position.X + Skin.Elements["Window.Border.Bottom.LeftCorner"].Dimensions.Width, Position.Y + WindowDimensions.Height - Skin.Elements["Window.Border.Bottom.Horizontal"].Dimensions.Height, WindowDimensions.Width - Skin.Elements["Window.Border.Bottom.RightCorner"].Dimensions.Width - Skin.Elements["Window.Border.Bottom.LeftCorner"].Dimensions.Width, Skin.Elements["Window.Border.Bottom.Horizontal"].Dimensions.Height));
			Skin.Elements["Window.Border.Bottom.RightCorner"].Draw(new Drawing.Rectangle(WindowDimensions.Right - Skin.Elements["Window.Border.Bottom.RightCorner"].Dimensions.Width, Position.Y + WindowDimensions.Height - Skin.Elements["Window.Border.Bottom.RightCorner"].Dimensions.Height, Skin.Elements["Window.Border.Bottom.RightCorner"].Dimensions.Width, Skin.Elements["Window.Border.Bottom.RightCorner"].Dimensions.Height));

			if (HasCaption)
			{
				_clipView.Left = captionScreen.Left;
				_clipView.Top = captionScreen.Top;
				_clipView.Width = captionScreen.Width;
				_clipView.Height = captionScreen.Height;
				lastView = Gorgon.CurrentClippingViewport;
				Gorgon.CurrentClippingViewport = _clipView;
				if (Desktop.Focused == this)
					_captionTextLabel.Color = Drawing.Color.White;
				else
					_captionTextLabel.Color = Drawing.Color.Gray;
				_captionTextLabel.Position = new Vector2D(captionScreen.Left, captionScreen.Top);
				_captionTextLabel.Draw();
				Gorgon.CurrentClippingViewport = lastView;
			}
		}

		/// <summary>
		/// Function called when a keyboard event has taken place in this window.
		/// </summary>
		/// <param name="isDown">TRUE if the button was pressed, FALSE if released.</param>
		/// <param name="e">Event parameters.</param>
		protected internal override void KeyboardEvent(bool isDown, GorgonLibrary.InputDevices.KeyboardInputEventArgs e)
		{
			if ((isDown) && (e.Key == GorgonLibrary.InputDevices.KeyboardKeys.Tab))
				SelectNextControl();

			if ((_focused != null) && (_focused.CanFocus))
				_focused.KeyboardEvent(isDown, e);
			else
				DefaultKeyboardEvent(isDown, e);
		}

		/// <summary>
		/// Function to draw the object.
		/// </summary>
		protected internal override void Draw()
		{
			Drawing.Rectangle screenPoints;		// Screen coordinates.

			if (Visible)
			{
				screenPoints = RectToScreen(ClientArea);

				Gorgon.CurrentRenderTarget.BeginDrawing();

				DrawNonClientArea();

				Gorgon.CurrentRenderTarget.FilledRectangle(screenPoints.X, screenPoints.Y, screenPoints.Width, screenPoints.Height, BackgroundColor);
				Gorgon.CurrentRenderTarget.EndDrawing();

				// Draw each child.
				var children = from guiObject in GUIObjects
							   where guiObject.Visible
							   orderby guiObject.ZOrder
							   select guiObject;

				foreach (GUIObject guiObject in children)
					guiObject.Draw();
			}
		}

		/// <summary>
		/// Function to set the client area for the object.
		/// </summary>
		/// <param name="windowArea">Full area of the window.</param>
		protected override void SetClientArea(System.Drawing.Rectangle windowArea)
		{
			if (Skin != null)
			{
				windowArea.Width -= DefaultBorderSize * 2;
				windowArea.Height -= DefaultCaptionHeight + DefaultBorderHeight;
			}

			base.SetClientArea(windowArea);
		}

		/// <summary>
		/// Function called when a mouse event has taken place in this window.
		/// </summary>
		/// <param name="eventType">Type of event that should be fired.</param>
		/// <param name="e">Event parameters.</param>
		protected internal override void MouseEvent(MouseEventType eventType, GorgonLibrary.InputDevices.MouseInputEventArgs e)
		{
			if ((!ClientArea.Contains(ScreenToPoint((Drawing.Point)e.Position))) && (HasCaption) && (CanMove))
			{
				Drawing.Point screenPos = (Drawing.Point)e.Position;
				switch (eventType)
				{
					case MouseEventType.MouseButtonDown:
						if ((IsMouseOverCaption) && (!_dragging) && (e.Buttons == GorgonLibrary.InputDevices.MouseButtons.Button1))
						{
							_dragDelta = Vector2D.Subtract(new Vector2D(Position.X, Position.Y), screenPos);
							_dragging = true;
						}
						break;
					case MouseEventType.MouseButtonUp:
						if (e.Buttons == GorgonLibrary.InputDevices.MouseButtons.Button1)
							_dragging = false;
						break;
				}
			}
			else
			{
				var children = from guiObject in GUIObjects
							   where guiObject.Visible && guiObject.Enabled
							   orderby guiObject.ZOrder descending
							   select guiObject;

				// Forward any events on to our child objects and terminate event handling if the child has received the event.
				foreach (GUIObject child in children)
				{
					if (child.WindowDimensions.Contains(ScreenToPoint((Drawing.Point)e.Position)))
					{
						if ((FocusedControl != child) && (eventType == MouseEventType.MouseButtonDown))
							child.Focus();
						child.MouseEvent(eventType, e);
						return;
					}
				}

				DefaultMouseEvent(eventType, e);
			}
		}

		/// <summary>
		/// Function to be called when the skin has been updated.
		/// </summary>
		internal void SkinUpdated()
		{
			SetClientArea(WindowDimensions);
		}

		/// <summary>
		/// Function to update the object.
		/// </summary>
		/// <param name="frameTime">Frame delta time.</param>
		/// <remarks>The frame delta is typically used with animations to help achieve a smooth appearance regardless of processor speed.</remarks>
		protected internal override void Update(float frameTime)
		{
			// Draw each child.
			var children = from guiObject in GUIObjects
						   where guiObject.Enabled
						   orderby guiObject.ZOrder
						   select guiObject;
						
			foreach (GUIObject guiObject in children)
				guiObject.Update(frameTime);

			Drawing.Point mousePosition = (Drawing.Point)(Desktop.MousePosition + _dragDelta);			// Mouse position.

			if (Owner != null)
				mousePosition = Owner.ScreenToPoint(mousePosition);

			if (_dragging)			
				Position = mousePosition;
		}

		/// <summary>
		/// Function to select the next control that can receive focus.
		/// </summary>
		public void SelectNextControl()
		{
			List<GUIObject> controls = new List<GUIObject>();

			GetAllControls(controls, this);

			var guiControls = from control in controls
							  where control.Enabled && control.Visible && control.CanFocus
							  orderby control.ZOrder
							  select control;

			if (_focused == null)
				_focused = guiControls.FirstOrDefault();
			else
			{
				for (int i = 0; i < guiControls.Count(); i++)
				{
					if (guiControls.ElementAt(i) == _focused)
					{
						if (i < guiControls.Count() - 1)
							_focused = guiControls.ElementAt(i + 1);
						else
							_focused = guiControls.ElementAt(0);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Function to convert a client point to screen coordinates.
		/// </summary>
		/// <param name="clientPoint">Client point to convert.</param>
		/// <returns>The screen coordinates of the point.</returns>
		public override System.Drawing.Point PointToScreen(System.Drawing.Point clientPoint)
		{
			Drawing.Point result = base.PointToScreen(clientPoint);

			result.X += DefaultBorderSize;
			result.Y += DefaultCaptionHeight;
			return result;
		}

		/// <summary>
		/// Function to convert a screen point to client coordinates.
		/// </summary>
		/// <param name="screenPoint">Screen point to convert.</param>
		/// <returns>The client coordinates of the point.</returns>
		public override System.Drawing.Point ScreenToPoint(System.Drawing.Point screenPoint)
		{
			Drawing.Point result = base.ScreenToPoint(screenPoint);

			result.X -= DefaultBorderSize;
			result.Y -= DefaultCaptionHeight;
			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GUIWindow"/> class.
		/// </summary>
		/// <param name="name">The name of the window.</param>
		/// <param name="x">The horizontal position of the window.</param>
		/// <param name="y">The vertical position of the window.</param>
		/// <param name="width">The width of the window.</param>
		/// <param name="height">The height of the window.</param>
		public GUIWindow(string name, int x, int y, int width, int height)
			: base(name)
		{
			CanMove = true;
			CanFocus = true;
			_guiObjects = new GUIObjectCollection();
			BackgroundColor = Drawing.Color.FromKnownColor(System.Drawing.KnownColor.Control);
			WindowDimensions = new System.Drawing.Rectangle(x, y, width, height);			
			Font = null;
			Visible = true;
			_captionTextLabel = new TextSprite("WindowCaptionLabel", Name, Font, Drawing.Color.White);
			_clipView = new Viewport(0, 0, 1, 1);
		}
		#endregion

		#region IGUIContainer Members
		/// <summary>
		/// Property to set or return whether this control will clip its children.
		/// </summary>
		/// <value></value>
		bool IGUIContainer.ClipChildren
		{
			get
			{
				return true;
			}
			set
			{
				
			}
		}

		/// <summary>
		/// Property to return the list of objects contained within this container.
		/// </summary>
		/// <value></value>
		public GUIObjectCollection GUIObjects
		{
			get 
			{
				return _guiObjects;
			}
		}
		#endregion
	}
}
