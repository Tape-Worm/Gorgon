#region LGPL.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Sunday, January 21, 2007 7:24:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Drawing = System.Drawing;
using SharpUtilities.Mathematics;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Fonts;

namespace GorgonLibrary.GorgonGUI
{
	/// <summary>
	/// Object representing a GUI window.
	/// </summary>
	public class GUIWindow
		: GUIContainer
	{
		#region Variables.
		private Sprite _clientArea = null;			// Client area sprite.
		private Sprite _leftCorner = null;			// Left caption corner.
		private Sprite _rightCorner = null;			// Right caption corner.
		private Sprite _leftBorder = null;			// Left border.
		private Sprite _rightBorder = null;			// Right border.
		private Sprite _leftBottomBorder = null;	// Left bottom border.
		private Sprite _rightBottomBorder = null;	// Right bottom border.
		private Sprite _bottomBorder = null;		// Bottom border.
		private Sprite _captionBar = null;			// Caption bar.
		private Sprite _leftTopBorder = null;		// Top left corner border (if no caption).
		private Sprite _rightTopBorder = null;		// Top right corner border (if no caption).
		private Sprite _topBorder = null;			// Top border (if no caption).
		private TextSprite _captionSprite = null;	// Sprite caption.
		private string _caption = string.Empty;		// Caption string.
		private bool _isDragging = false;			// Window is being dragged.
		private Vector2D _dragStart;				// Drag start point.
		private bool _canDrag = true;				// Flag to indicate whether we can drag the window or not.
		private byte _backImageAlpha = 255;			// Background image alpha.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the mouse is over the caption or not.
		/// </summary>
		private bool MouseOverCaption
		{
			get
			{
				// If we hover over the caption, hilight it.
				if ((CanDrag) && (_owner.MousePosition.X >= _position.X) && (_owner.MousePosition.X <= _position.X + _size.X) && (_owner.MousePosition.Y >= _position.Y) && (_owner.MousePosition.Y <= _position.Y + _captionBar.Height))
					return true;
				
				return false;
			}
		}

		/// <summary>
		/// Property to return whether the window is dragging or not.
		/// </summary>
		public bool IsDragging
		{
			get
			{
				return _isDragging;
			}
		}

		/// <summary>
		/// Property to set or return whether or not the window can be dragged by the mouse.
		/// </summary>
		public bool CanDrag
		{
			get
			{
				return ((_canDrag) && (_caption != string.Empty) && (Focused));
			}
			set
			{
				_canDrag = value;
			}
		}

		/// <summary>
		/// Property to set or return the window caption.
		/// </summary>
		public string Caption
		{
			get
			{
				return _caption;
			}
			set
			{
				_caption = value;
				UpdateCaption();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the caption.
		/// </summary>
		private void UpdateCaption()
		{
			// Update caption text.
			if (_captionSprite != null)
			{
				_captionSprite.Text = _caption;
				_captionSprite.Alignment = Alignment.Center;
				if (_captionBar != null)
				{
					_captionSprite.Position = _captionBar.Position;
					_captionSprite.ClippingViewport = new Viewport((int)_captionBar.Position.X, (int)_captionBar.Position.Y, (int)_captionBar.ScaledDimensions.X, (int)_captionBar.Height);
				}
				_captionSprite.Font = Font;
			}
		}

		/// <summary>
		/// Function called when a mouse move event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal override void OnMouseMove(object sender, GorgonLibrary.Input.MouseInputEventArgs e)
		{
			// If we have a have caption let us drag the window.
			if (_isDragging)
			{
				_position.X = _owner.MousePosition.X + _dragStart.X;
				_position.Y = _owner.MousePosition.Y + _dragStart.Y;				
			}

			// Are we inside the window with the cursor?
			if ((_owner.MousePosition.X >= _position.X) && (_owner.MousePosition.X <= _position.X + _size.X) && (_owner.MousePosition.Y >= _position.Y) && (_owner.MousePosition.Y <= _position.Y + _size.Y))
				MouseInView = true;
			else
				MouseInView = false;

			base.OnMouseMove(sender, e);
		}

		/// <summary>
		/// Function called when a mouse button up event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal override void OnMouseUp(object sender, GorgonLibrary.Input.MouseInputEventArgs e)
		{
			if (_isDragging)
				_isDragging = false;

			base.OnMouseUp(sender, e);
		}

		/// <summary>
		/// Function called when a mouse button down event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal override void OnMouseDown(object sender, GorgonLibrary.Input.MouseInputEventArgs e)
		{
			if ((MouseOverCaption) && (!_isDragging))
			{
				_dragStart.X = (_position.X - _owner.MousePosition.X);
				_dragStart.Y = (_position.Y - _owner.MousePosition.Y);
				_isDragging = true;
			}

			// Give focus to the window we've clicked on.
			if ((MouseInView) && (!Focused))
				_owner.GiveFocus(this);

			base.OnMouseDown(sender, e);
		}

		/// <summary>
		/// Function to retrieve the skin pieces.
		/// </summary>
		protected override void GetSkinPieces()
		{
			_clientArea = _owner.Skin["GUIWindow.Client"];
			_rightCorner = _owner.Skin["GUIWindow.RightCorner"];
			_leftCorner = _owner.Skin["GUIWindow.LeftCorner"];
			_captionBar = _owner.Skin["GUIWindow.CaptionBar"];
			_leftBorder = _owner.Skin["GUIWindow.LeftBorder"];
			_rightBorder = _owner.Skin["GUIWindow.RightBorder"];
			_leftBottomBorder = _owner.Skin["GUIWindow.BorderLeftCorner"];
			_rightBottomBorder = _owner.Skin["GUIWindow.BorderRightCorner"];
			_bottomBorder = _owner.Skin["GUIWindow.BorderBottom"];
			_leftTopBorder = _owner.Skin["GUIWindow.TopLeftCorner"];
			_rightTopBorder = _owner.Skin["GUIWindow.TopRightCorner"];
			_topBorder = _owner.Skin["GUIWindow.TopBorderNoCaption"];
		}

		/// <summary>
		/// Function to build the control.
		/// </summary>
		protected override void BuildControl()
		{					
			int newWidth = 0;				// New width.
			float totalWidth = 0;			// Total width.
			Sprite topBorder = null;		// Top border.
			Sprite leftTopBorder = null;	// Top left border.
			Sprite rightTopBorder = null;	// Top right border.

			base.BuildControl();

			// Determine how to draw top borders by caption.
			if (_caption == string.Empty)
			{
				topBorder = _topBorder;
				leftTopBorder = _leftTopBorder;
				rightTopBorder = _rightTopBorder;
			}
			else
			{
				topBorder = _captionBar;
				leftTopBorder = _leftCorner;
				rightTopBorder = _rightCorner;
			}

			totalWidth = _size.X;
			_clientSize.X = _size.X - _leftBorder.Width - _rightBorder.Width;
			_clientSize.Y = _size.Y - _bottomBorder.Height - topBorder.ScaledDimensions.Y;

			// Update the caption.
			leftTopBorder.Position = _position;

			topBorder.SetPosition(leftTopBorder.Width + _position.X, _position.Y);
			newWidth = (int)(totalWidth - (leftTopBorder.Width + rightTopBorder.Width));
			topBorder.SetScale(newWidth / topBorder.Width, 1.0f);
			rightTopBorder.SetPosition((int)(topBorder.ScaledDimensions.X + topBorder.Position.X), _position.Y);
			
			// Set up client area.
			_clientPosition = new Vector2D(_leftBorder.Width + _position.X, (int)(topBorder.ScaledDimensions.Y + _position.Y));
			_clientArea.SetPosition((int)_clientPosition.X, (int)_clientPosition.Y);
			_clientArea.SetScale(_clientSize.X / _clientArea.Width, _clientSize.Y / _clientArea.Height);
			_clientArea.Smoothing = Smoothing.Smooth;

			// Update left & right border.
			_leftBorder.SetPosition(_position.X, _clientPosition.Y);
			_leftBorder.SetScale(1.0f, (_clientSize.Y / _leftBorder.Height));
			_rightBorder.SetPosition((int)(_position.X + _clientSize.X + _leftBorder.Width), _clientPosition.Y);
			_rightBorder.SetScale(1.0f, (_clientSize.Y / _rightBorder.Height));

			// Update bottom border.			
			newWidth = (int)(totalWidth - (_leftBottomBorder.Width + _rightBottomBorder.Width));
			_bottomBorder.SetScale(newWidth / _bottomBorder.Width, 1.0f);
			_leftBottomBorder.SetPosition(_position.X, (int)(_clientSize.Y + _clientPosition.Y));
			_bottomBorder.SetPosition(_leftBottomBorder.Width + _position.X, (int)(_clientSize.Y + _clientPosition.Y));
			_rightBottomBorder.SetPosition((int)(_bottomBorder.Position.X + _bottomBorder.ScaledDimensions.X), (int)(_clientSize.Y + _clientPosition.Y));

			// Set colors.
			if (ForeColor != Drawing.Color.Transparent)
				_captionSprite.Color = ForeColor;
			else
				_captionSprite.Color = Drawing.Color.White;

			if (BackColor != Drawing.Color.Transparent)
				_clientArea.Color = BackColor;
			else
				_clientArea.Color = Drawing.Color.White;

			if (BorderColor != Drawing.Color.Transparent)
			{
				_leftCorner.Color = BorderColor;
				_rightCorner.Color = BorderColor;
				_leftBorder.Color = BorderColor;
				_rightBorder.Color = BorderColor;
				_leftBottomBorder.Color = BorderColor;
				_rightBottomBorder.Color = BorderColor;
				_bottomBorder.Color = BorderColor;
				_captionBar.Color = BorderColor;
				_leftTopBorder.Color = BorderColor;
				_rightTopBorder.Color = BorderColor;
				_topBorder.Color = BorderColor;
			}
			else
			{
				_leftCorner.Color = Drawing.Color.White;
				_rightCorner.Color = Drawing.Color.White;
				_leftBorder.Color = Drawing.Color.White;
				_rightBorder.Color = Drawing.Color.White;
				_leftBottomBorder.Color = Drawing.Color.White;
				_rightBottomBorder.Color = Drawing.Color.White;
				_bottomBorder.Color = Drawing.Color.White;
				_captionBar.Color = Drawing.Color.White;
				_leftTopBorder.Color = Drawing.Color.White;
				_rightTopBorder.Color = Drawing.Color.White;
				_topBorder.Color = Drawing.Color.White;
			}				

			// Draw focused/unfocused.
			if (Focused)
			{
				if (BackgroundImage != null)
					BackgroundImage.Opacity = _backImageAlpha;
				if (BackColor.A == 0)
					BackColor = Drawing.Color.FromArgb(255, BackColor);
				if (BorderColor.A == 0)
					BorderColor = Drawing.Color.FromArgb(255, BorderColor);
				_clientArea.Opacity = BackColor.A;
				_leftCorner.Opacity = BorderColor.A;
				_rightCorner.Opacity = BorderColor.A;
				_leftBorder.Opacity = BorderColor.A;
				_rightBorder.Opacity = BorderColor.A;
				_leftBottomBorder.Opacity = BorderColor.A;
				_rightBottomBorder.Opacity = BorderColor.A;
				_bottomBorder.Opacity = BorderColor.A;
				_captionBar.Opacity = BorderColor.A;
				_leftTopBorder.Opacity = BorderColor.A;
				_rightTopBorder.Opacity = BorderColor.A;
				_topBorder.Opacity = BorderColor.A;
				_captionSprite.Opacity = BorderColor.A;
			}
			else
			{
				if (BackgroundImage != null)
					BackgroundImage.Color = Drawing.Color.FromArgb(96, BackgroundImage.Color);

				_clientArea.Opacity = 96;
				_leftCorner.Opacity = 96;
				_rightCorner.Opacity = 96;
				_leftBorder.Opacity = 96;
				_rightBorder.Opacity = 96;
				_leftBottomBorder.Opacity = 96;
				_rightBottomBorder.Opacity = 96;
				_bottomBorder.Opacity = 96;
				_captionBar.Opacity = 96;
				_leftTopBorder.Opacity = 96;
				_rightTopBorder.Opacity = 96;
				_topBorder.Opacity = 96;
				_captionSprite.Opacity = 96;
			}

			if (_caption != string.Empty)
				UpdateCaption();

			// Update clipping.
			if (ClippingView.Left != (int)_clientPosition.X)
				ClippingView.Left = (int)_clientPosition.X;
			if (ClippingView.Top != (int)_clientPosition.Y)
				ClippingView.Top = (int)_clientPosition.Y;
			if (ClippingView.Width != (int)_clientSize.X)
				ClippingView.Width = (int)_clientSize.X;
			if (ClippingView.Height != (int)_clientSize.Y)
				ClippingView.Height = (int)_clientSize.Y;
		}

		/// <summary>
		/// Function called when the window has gained focus.
		/// </summary>
		protected internal override void GainingFocus()
		{
			base.GainingFocus();

			if ((MouseOverCaption) && (!_isDragging))
			{
				_dragStart.X = (_position.X - _owner.MousePosition.X);
				_dragStart.Y = (_position.Y - _owner.MousePosition.Y);
				_isDragging = true;
			}
		}

		/// <summary>
		/// Function called when the window has lost its focus.
		/// </summary>
		protected internal override void LosingFocus()
		{
			base.LosingFocus();

			if (BackgroundImage != null)
				_backImageAlpha = BackgroundImage.Color.A;
		}

		/// <summary>
		/// Function to draw the control.
		/// </summary>
		internal override void DrawControl()
		{
			if (_visible)
			{
				BuildControl();

				// If we don't have a caption, then only draw a small border.
				if (_caption == string.Empty)
				{
					_leftTopBorder.Draw();
					_topBorder.Draw();
					_rightTopBorder.Draw();
				}
				else
				{
					_leftCorner.Draw();
					_captionBar.Draw();
					_rightCorner.Draw();
				}
				_leftBorder.Draw();
				_rightBorder.Draw();
				_leftBottomBorder.Draw();
				_bottomBorder.Draw();
				_rightBottomBorder.Draw();
				_captionSprite.Draw();
				_clientArea.Draw();

				// If we have a background image, show it.
				if (BackgroundImage != null)
				{
					Viewport backViewport = BackgroundImage.ClippingViewport;		// Store the previous viewport.

					// Clip to our label.
					BackgroundImage.ClippingViewport = ClippingView;

					BackgroundImage.Position = _clientPosition;

					if (ScaleBackgroundImage)
					{
						Vector2D newSize = Vector2D.Zero;		// New size.

						// Calculate new size.
						newSize.X = _clientSize.X / BackgroundImage.Width;
						newSize.Y = _clientSize.Y / BackgroundImage.Height;

						BackgroundImage.Scale = newSize;
					}
					BackgroundImage.Draw();

					// Restore the background viewport.
					BackgroundImage.ClippingViewport = backViewport;
					BackgroundImage.Draw();
				}

				// Draw any child controls.
				base.DrawControl();
			}
		}

		/// <summary>
		/// Function to convert a client position into screen space.
		/// </summary>
		/// <param name="clientPosition">Client position.</param>
		/// <returns>The position in screen space.</returns>
		public override Vector2D ConvertToScreen(Vector2D clientPosition)
		{
			BuildControl();
			return new Vector2D(_clientPosition.X + clientPosition.X, _clientPosition.Y + clientPosition.Y);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">GUI that owns this window.</param>
		/// <param name="name">Name of the window.</param>
		/// <param name="windowCaption">Caption for the window.</param>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical position.</param>
		/// <param name="width">Width of the window.</param>
		/// <param name="height">Height of the window.</param>
		internal GUIWindow(GUI owner, string name, string windowCaption, float x, float y, float width, float height)
			: base(owner, name, new Vector2D(x, y), new Vector2D(width, height))
		{
			_captionSprite = new TextSprite("@GUIWindow." + name + ".Caption", windowCaption, owner.Skin.DefaultFontColor);
			
			Font = null;
			Caption = windowCaption;
			BuildControl();
		}
		#endregion
	}
}
