#region LGPL.
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
// Created: Tuesday, May 27, 2008 6:52:46 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Drawing = System.Drawing;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Extras.GUI
{
	/// <summary>
	/// Object representing a GUI window.
	/// </summary>
	public class GUIWindow
		: GUIPanel
	{
		#region Variables.
		private Font _windowFont = null;								// Font for the window.
		private bool _disposed = false;									// Flag to indicate that the object is disposed.
		private TextSprite _captionTextLabel = null;					// Caption text.
		private bool _dragging = false;									// Flag to indicate that we're dragging.
		private Drawing.Rectangle _captionRectangle;					// Caption rectangle.
		private Vector2D _dragDelta = Vector2D.Zero;					// Drag delta.
		private Viewport _clipView = null;								// Clipping view port.
		private static int _defaultCaptionHeight = 20;					// Default caption height.
		private static int _defaultBorderWidth = 4;						// Default border width.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the current GUI skin.
		/// </summary>
		private GUISkin Skin
		{
			get
			{
				if (Desktop != null)
					return Desktop.Skin;
				else
					return null;
			}
		}

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
		/// Property to return the default border size.
		/// </summary>
		public int DefaultBorderSize
		{
			get
			{
				if (Skin == null)
					return 0;
				else
					return Skin.Elements["Window.Border.Vertical"].Dimensions.Width;
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
					return _windowFont.LineHeight + 4;
				else
				{
					if ((_windowFont.LineHeight - Skin.Elements["Window.Caption"].Dimensions.Height) > 2)
						return (Skin.Elements["Window.Caption"].Dimensions.Height) + (_windowFont.LineHeight - Skin.Elements["Window.Caption"].Dimensions.Height) + 4;
					else
						return Skin.Elements["Window.Caption"].Dimensions.Height + 4;
				}
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
				throw new GUIInvalidTypeException("GUI windows cannot be owned.");
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

		/// <summary>
		/// Property to set or return the font for the window.
		/// </summary>
		public Font Font
		{
			get
			{
				return _windowFont;
			}
			set
			{
				if (_windowFont != null)
					_windowFont.Dispose();

				if (value == null)
					_windowFont = new Font(Name + ".WindowFont." + Guid.NewGuid().ToString(), "Arial", 9.0f, true, true, false, false);
				else
				{
					_windowFont = new Font(Name + ".WindowFont." + Guid.NewGuid().ToString(), value.FamilyName, value.FontSize, value.AntiAlias, value.Bold, value.Underline, value.Italic);
					_windowFont.BaseColor = value.BaseColor;
					_windowFont.CharacterList = value.CharacterList;
					_windowFont.GlyphHeightPadding = value.GlyphHeightPadding;
					_windowFont.GlyphLeftOffset = value.GlyphLeftOffset;
					_windowFont.GlyphTopOffset = value.GlyphTopOffset;
					_windowFont.GlyphWidthPadding = value.GlyphWidthPadding;
					_windowFont.GradientAngle = value.GradientAngle;
					_windowFont.GradientColor = value.GradientColor;
					_windowFont.Italic = value.Italic;
					_windowFont.MaxFontImageHeight = value.MaxFontImageHeight;
					_windowFont.MaxFontImageWidth = value.MaxFontImageWidth;
					_windowFont.OutlineColor = value.OutlineColor;
					_windowFont.OutlineWidth = value.OutlineWidth;
					_windowFont.Strikeout = value.Strikeout;
					_windowFont.UserBrush = value.UserBrush;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
					_windowFont.Dispose();
				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to draw the non-client area.
		/// </summary>
		protected override void DrawNonClientArea()
		{
			Vector2D nonClientPosition = Position;
			Drawing.Rectangle captionScreen;
			Viewport lastView = null;

			if (Skin == null)
				return;

			_captionRectangle = new System.Drawing.Rectangle(Position.X + Skin.Elements["Window.Caption.LeftCorner"].Dimensions.Width, Position.Y,
						WindowDimensions.Width - Skin.Elements["Window.Caption.RightCorner"].Dimensions.Width - (DefaultBorderSize * 2), DefaultCaptionHeight);

			//_captionRectangle = new System.Drawing.Rectangle(Position.X + DefaultBorderSize, Position.Y + DefaultBorderSize, WindowDimensions.Width - DefaultBorderSize * 2, DefaultCaptionHeight);
			captionScreen = _captionRectangle;

			if (Owner != null)
			{
				nonClientPosition = Owner.PointToScreen(Position);
				captionScreen = Owner.RectToScreen(_captionRectangle);
			}

			Skin.Elements["Window.Caption.LeftCorner"].Draw(new Drawing.Rectangle(captionScreen.Location.X - Skin.Elements["Window.Caption.LeftCorner"].Dimensions.Width, captionScreen.Location.Y, Skin.Elements["Window.Caption.LeftCorner"].Dimensions.Width, captionScreen.Size.Height));
			Skin.Elements["Window.Caption.RightCorner"].Draw(new Drawing.Rectangle(captionScreen.Width + captionScreen.X, captionScreen.Location.Y, Skin.Elements["Window.Caption.RightCorner"].Dimensions.Width, captionScreen.Size.Height));
			Skin.Elements["Window.Caption"].Draw(captionScreen);

			Skin.Elements["Window.Border.Vertical"].Draw(new Drawing.Rectangle(Position.X, DefaultCaptionHeight + Position.Y, DefaultBorderSize, WindowDimensions.Height - DefaultCaptionHeight));
			Skin.Elements["Window.Border.Vertical"].Draw(new Drawing.Rectangle(Position.X + WindowDimensions.Width - Skin.Elements["Window.Border.Vertical"].Dimensions.Width + 2, DefaultCaptionHeight + Position.Y, DefaultBorderSize, WindowDimensions.Height - DefaultCaptionHeight));

			//Gorgon.CurrentRenderTarget.FilledRectangle(WindowDimensions.X, WindowDimensions.Y, WindowDimensions.Width, WindowDimensions.Height, Drawing.Color.FromArgb(128, Drawing.Color.FromKnownColor(System.Drawing.KnownColor.WindowFrame)));
			//Gorgon.CurrentRenderTarget.FilledRectangle(captionScreen.X, captionScreen.Y, captionScreen.Width, captionScreen.Height, Drawing.Color.FromArgb(128, Drawing.Color.FromKnownColor(System.Drawing.KnownColor.ActiveCaption)));

			_clipView.Left = captionScreen.Left;
			_clipView.Top = captionScreen.Top;
			_clipView.Width = captionScreen.Width;
			_clipView.Height = captionScreen.Height;
			lastView = Gorgon.CurrentClippingViewport;
			Gorgon.CurrentClippingViewport = _clipView;
			_captionTextLabel.Position = new Vector2D(captionScreen.Left, captionScreen.Top);
			_captionTextLabel.Draw();
			Gorgon.CurrentClippingViewport = lastView;
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
				windowArea.Height -= DefaultCaptionHeight;
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
			if (!ClientArea.Contains(ScreenToPoint((Drawing.Point)e.Position)))
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
				base.MouseEvent(eventType, e);
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
		internal override void Update(float frameTime)
		{
			base.Update(frameTime);

			Drawing.Point mousePosition = (Drawing.Point)(Desktop.MousePosition + _dragDelta);			// Mouse position.

			if (Owner != null)
				mousePosition = Owner.ScreenToPoint(mousePosition);

			if (_dragging)			
				Position = mousePosition;
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
			WindowDimensions = new System.Drawing.Rectangle(x, y, width, height);			
			Font = null;			
			_captionTextLabel = new TextSprite("WindowCaptionLabel", Name, Font, Drawing.Color.FromKnownColor(System.Drawing.KnownColor.ActiveCaptionText));
			_clipView = new Viewport(0, 0, 1, 1);
		}
		#endregion
	}
}
