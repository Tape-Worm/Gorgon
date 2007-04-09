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
// Created: Sunday, February 25, 2007 2:39:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using SharpUtilities.Mathematics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Fonts;

namespace GorgonLibrary.GorgonGUI
{
	/// <summary>
	/// Object representing a label control.
	/// </summary>
	public class GUILabel
		: GUIControl
	{
		#region Variables.
		private TextSprite _textSprite = null;		// Text sprite.
		private bool _autoSize = false;				// Flag to indicate whether we should automatically size the label to its contents.
		private bool _border = false;				// Flag to indicate whether we should have a border or not.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the border color.
		/// </summary>
		public override Drawing.Color BorderColor
		{
			get
			{
				return base.BorderColor;
			}
			set
			{
				base.BorderColor = value;
				if ((value == Drawing.Color.Transparent) || (value.A == 0))
					_border = false;
			}
		}

		/// <summary>
		/// Property to set or return the alignment of the text.
		/// </summary>
		public Alignment Alignment
		{
			get
			{
				return _textSprite.Alignment;
			}
			set
			{
				_textSprite.Alignment = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we should have a border or not.
		/// </summary>
		public bool Border
		{
			get
			{
				return _border;
			}
			set
			{
				_border = value;
				if ((value) && ((BorderColor == Drawing.Color.Transparent) || (BorderColor.A == 0)))
					BorderColor = Drawing.Color.Black;
			}
		}

		/// <summary>
		/// Property to set or return whether we should automatically size the label to its contents.
		/// </summary>
		public bool AutoSize
		{
			get
			{
				return _autoSize;
			}
			set
			{
				_autoSize = value;
			}
		}

		/// <summary>
		/// Property to set or return the text for the control.
		/// </summary>
		public string Text
		{
			get
			{
				return _textSprite.Text;
			}
			set
			{
				_textSprite.Text = value;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the skin pieces.
		/// </summary>
		protected override void GetSkinPieces()
		{
			// Not needed for labels.
		}

		/// <summary>
		/// Function to build the control.
		/// </summary>
		protected override void BuildControl()
		{
			Vector2D screenPos = Vector2D.Zero;		// Screen position.

			if (_textSprite == null)
				return;

			// Set the client size.
			_clientPosition = _position;

			if (!_autoSize)
			{
				_clientSize.X = _size.X;
				_clientSize.Y = _size.Y;
			}
			else
			{
				// Automatically size the label.
				Size = new Vector2D(_textSprite.Width, _textSprite.Height);
				_clientSize.X = Size.X;
				_clientSize.Y = Size.Y;
			}

			// Adjust for border.
			if (_border)
			{
				_clientPosition.X += 1.0f;
				_clientPosition.Y += 1.0f;
			}

			UpdateClipping();

			_textSprite.ClippingViewport = ClippingView;
			_textSprite.Font = Font;
			_textSprite.Color = ForeColor;

			// Set the position of the label.
			screenPos = Parent.ConvertToScreen(_clientPosition);
			_textSprite.Position = screenPos;
		}

		/// <summary>
		/// Function to draw the control.
		/// </summary>
		internal override void DrawControl()
		{
			Vector2D screenPos = Vector2D.Zero;		// Screen position.

			// Draw only if we have text to draw.
			if ((_textSprite.Text != string.Empty) && (_visible))
			{
				if (Parent != null)
					screenPos = Parent.ConvertToScreen(_position);
				else
					screenPos = _position;

				BuildControl();

				// Draw the background color & border.
				if (((BackColor != Drawing.Color.Transparent) && (BackColor.A != 0)) || (_border))
				{
					Viewport backViewport = Gorgon.CurrentRenderTarget.ClippingViewport;	// Store previous viewport.

					Gorgon.CurrentRenderTarget.BeginDrawing();					
					Gorgon.CurrentRenderTarget.ClippingViewport = _textSprite.ClippingViewport;
					if ((BackColor != Drawing.Color.Transparent) && (BackColor.A != 0))
						Gorgon.CurrentRenderTarget.FilledRectangle(screenPos.X, screenPos.Y, _size.X + 1.0f, _size.Y + 1.0f, BackColor);

					if (_border)
						Gorgon.CurrentRenderTarget.Rectangle(screenPos.X, screenPos.Y, _size.X + 1.0f, _size.Y + 1.0f, BorderColor);

					Gorgon.CurrentRenderTarget.EndDrawing();
					Gorgon.CurrentRenderTarget.ClippingViewport = backViewport;
				}

				// Draw a background sprite if we have one.
				if (BackgroundImage != null)
				{
					Viewport backViewport = BackgroundImage.ClippingViewport;		// Store the previous viewport.

					// Clip to our label.
					BackgroundImage.ClippingViewport = _textSprite.ClippingViewport;

					BackgroundImage.Position = screenPos;

					if (ScaleBackgroundImage)
					{
						Vector2D newSize = Vector2D.Zero;		// New size.

						// Calculate new size.
						newSize.X = _size.X / BackgroundImage.Width;
						newSize.Y = _size.Y / BackgroundImage.Height;

						BackgroundImage.Scale = newSize;
					}
					BackgroundImage.Draw();

					// Restore the background viewport.
					BackgroundImage.ClippingViewport = backViewport;
				}

				_textSprite.Draw();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="gui">GUI that owns this object.</param>
		/// <param name="name">Name of the object.</param>
		/// <param name="text">Text to show.</param>
		public GUILabel(GUI gui, string name, string text)
			: base(gui, name, Vector2D.Zero, Vector2D.Zero)
		{			
			_textSprite = new TextSprite("@GUILabel." + name + ".Caption", text, gui.Skin.Font, ForeColor);
			_clientSize = _textSprite.Size;
			_size = _textSprite.Size;
			_position = _textSprite.Position;
		}
		#endregion
	}
}
