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
// Created: Monday, February 26, 2007 1:01:53 AM
// 
#endregion


using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using SharpUtilities.Mathematics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Fonts;
using GorgonLibrary.Input;

namespace GorgonLibrary.GorgonGUI
{
	/// <summary>
	/// Object representing a button control.
	/// </summary>
	public class GUIButton
		: GUIControl
	{
		#region Enumerations.
		/// <summary>
		/// Enumeration containing rendering states for the button.
		/// </summary>
		private enum ButtonDrawStates
		{
			/// <summary>
			/// Button is to be drawn normally.
			/// </summary>
			ButtonNormal = 0,
			/// <summary>
			/// Button is pressed.
			/// </summary>
			ButtonPressed = 1,
			/// <summary>
			/// Button has the mouse over it.
			/// </summary>
			ButtonHilighted = 2		
		}
		#endregion

		#region Variables.
		private TextSprite _captionSprite = null;	// Text sprite.
		private SpriteManager _hiLight = null;		// Hilighted pieces.
		private SpriteManager _pressed = null;		// Pressed pieces.		
		private Drawing.Color _hiLightColor;		// Highlight color.
		private ButtonDrawStates _buttonState;		// Button rendering state.
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when the button is clicked.
		/// </summary>
		public event EventHandler Clicked = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the highlight color of the button.
		/// </summary>
		public Drawing.Color HighlightColor
		{
			get
			{
				return _hiLightColor;
			}
			set
			{
				_hiLightColor = value;
				if (_hiLightColor.A == 0)
					_hiLightColor = Drawing.Color.FromArgb(255, _hiLightColor);
			}
		}

		/// <summary>
		/// Property to set or return the alignment of the text.
		/// </summary>
		public Alignment Alignment
		{
			get
			{
				return _captionSprite.Alignment;
			}
			set
			{
				_captionSprite.Alignment = value;
			}
		}

		/// <summary>
		/// Property to set or return the caption for the control.
		/// </summary>
		public string Caption
		{
			get
			{
				return _captionSprite.Text;
			}
			set
			{
				_captionSprite.Text = value;
			}
		}
		#endregion

		#region Methods.		
		/// <summary>
		/// Function to retrieve the skin pieces.
		/// </summary>
		protected override void GetSkinPieces()
		{
			// Get the button pieces.
			foreach (Sprite sprite in _owner.Skin)
			{
				// Add normal pieces.
				if (sprite.Name.IndexOf("GUIButton.") == 0)					
				{	
					// Standard state.
					if ((sprite.Name.IndexOf(".HiLight") == -1) && (sprite.Name.IndexOf(".Pressed") == -1))
						_pieces.Add(sprite);
					// Hilighted state.
					if (sprite.Name.IndexOf(".HiLight") > -1)
						_hiLight.Add(sprite);
					// Pressed state.
					if (sprite.Name.IndexOf(".Pressed") > -1)
						_pressed.Add(sprite);

					// Set the clipper for each piece.
					sprite.ClippingViewport = ClippingView;
				}
			}

			// If we have no hi-lighted pieces, copy from the standard pieces.
			if (_hiLight.Count == 0)
			{
				// Add normal pieces.
				foreach (Sprite sprite in _pieces)
					_hiLight.Add(sprite);
			}
			else
				_hiLight["GUIButton.Client.HiLight"].Smoothing = Smoothing.Smooth;

			// If we have no pressed pieces, copy from the standard pieces.
			if (_pressed.Count == 0)
			{
				// Add normal pieces.
				foreach (Sprite sprite in _pieces)
					_pressed.Add(sprite);
			}
			else
				_pressed["GUIButton.Client.Pressed"].Smoothing = Smoothing.Smooth;

			// Set the client areas to smooth.
			_pieces["GUIButton.Client"].Smoothing = Smoothing.Smooth;			
		}

		/// <summary>
		/// Function to update the clipping for a control.
		/// </summary>
		protected override void UpdateClipping()
		{
			Vector2D clientPos = Vector2D.Zero;
			
			if (Parent == null)
				return;

			base.UpdateClipping();

			_captionSprite.ClippingViewport.Width = (int)_clientSize.X;
			_captionSprite.ClippingViewport.Height = (int)_clientSize.Y;

			// Convert to screen space.
			clientPos = Parent.ConvertToScreen(_clientPosition);

			_captionSprite.ClippingViewport.Left = (int)clientPos.X;
			_captionSprite.ClippingViewport.Top = (int)clientPos.Y;

			if (_captionSprite.ClippingViewport.Left < ClippingView.Left)
			{
				_captionSprite.ClippingViewport.Width -= (ClippingView.Left - _captionSprite.ClippingViewport.Left);
				_captionSprite.ClippingViewport.Left = ClippingView.Left;				
			}
			if (_captionSprite.ClippingViewport.Top < ClippingView.Top)
			{
				_captionSprite.ClippingViewport.Height -= (ClippingView.Top - _captionSprite.ClippingViewport.Top);
				_captionSprite.ClippingViewport.Top = ClippingView.Top;
			}

			if (_captionSprite.ClippingViewport.Width + _captionSprite.ClippingViewport.Left > ClippingView.Left + ClippingView.Width)
				_captionSprite.ClippingViewport.Width -= (_captionSprite.ClippingViewport.Width + _captionSprite.ClippingViewport.Left) - (ClippingView.Left + ClippingView.Width);
			if (_captionSprite.ClippingViewport.Height + _captionSprite.ClippingViewport.Top > ClippingView.Top + ClippingView.Height)
				_captionSprite.ClippingViewport.Height -= (_captionSprite.ClippingViewport.Height + _captionSprite.ClippingViewport.Top) - (ClippingView.Top + ClippingView.Height);
		}

		/// <summary>
		/// Function to build the control.
		/// </summary>
		protected override void BuildControl()
		{			
			Vector2D screenPos = Vector2D.Zero;		// Screen position.

			base.BuildControl();

			// Calculate client position.
			_clientPosition = new Vector2D(_pieces["GUIButton.Left"].Width + _position.X, _pieces["GUIButton.Top"].Height + _position.Y);
			_clientSize = new Vector2D(_size.X - _pieces["GUIButton.Right"].Width - _pieces["GUIButton.Left"].Width, _size.Y - _pieces["GUIButton.Bottom"].Height - _pieces["GUIButton.Top"].Height);

			if (Parent == null)
				return;

			// Convert to screen space.
			screenPos = Parent.ConvertToScreen(_clientPosition);

			// Place pieces.
			_pieces["GUIButton.TopLeft"].Position = new Vector2D(screenPos.X - _pieces["GUIButton.TopLeft"].Width, screenPos.Y - _pieces["GUIButton.TopLeft"].Height);
			_pieces["GUIButton.Top"].Position = new Vector2D(screenPos.X, screenPos.Y - _pieces["GUIButton.TopLeft"].Height);
			_pieces["GUIButton.Top"].SetScale((_clientSize.X / _pieces["GUIButton.Top"].Width) , 1.0f);			
			_pieces["GUIButton.TopRight"].Position = new Vector2D(screenPos.X + _clientSize.X, screenPos.Y - _pieces["GUIButton.TopLeft"].Height);			

			_pieces["GUIButton.Left"].Position = new Vector2D(screenPos.X - _pieces["GUIButton.Left"].Width, screenPos.Y);
			_pieces["GUIButton.Left"].SetScale(1.0f, _clientSize.Y / _pieces["GUIButton.Left"].Height);			
			_pieces["GUIButton.Right"].Position = new Vector2D(screenPos.X + _clientSize.X, screenPos.Y);
			_pieces["GUIButton.Right"].SetScale(1.0f, _clientSize.Y / _pieces["GUIButton.Left"].Height);			

			_pieces["GUIButton.Client"].Position = screenPos;
			_pieces["GUIButton.Client"].SetScale(_clientSize.X / _pieces["GUIButton.Client"].Width, _clientSize.Y / _pieces["GUIButton.Client"].Height);			

			_pieces["GUIButton.BottomLeft"].Position = new Vector2D(screenPos.X - _pieces["GUIButton.BottomLeft"].Width, screenPos.Y + _clientSize.Y);			
			_pieces["GUIButton.Bottom"].Position = new Vector2D(screenPos.X, screenPos.Y + _clientSize.Y);
			_pieces["GUIButton.Bottom"].SetScale((_clientSize.X / _pieces["GUIButton.Bottom"].Width), 1.0f);			
			_pieces["GUIButton.BottomRight"].Position = new Vector2D(screenPos.X + _clientSize.X, screenPos.Y + _clientSize.Y);

			// Copy settings to other pieces.
			for (int i = 0; i < _pieces.Count; i++)
			{
				_pressed[i].Position = _hiLight[i].Position = _pieces[i].Position;
				_pressed[i].Scale = _hiLight[i].Scale = _pieces[i].Scale;
				_pressed[i].Color = _pieces[i].Color = BackColor;
				_hiLight[i].Color = _hiLightColor;				
			}

			// Update the clipper.
			UpdateClipping();

			// Update the caption.
			_captionSprite.Font = Font;
			_captionSprite.Color = ForeColor;
			_captionSprite.Position = screenPos;
			

			if (MouseInView)
			{
				if (_buttonState == ButtonDrawStates.ButtonNormal)
					_buttonState = ButtonDrawStates.ButtonHilighted;
			}
			else
				_buttonState = ButtonDrawStates.ButtonNormal;
		}

		/// <summary>
		/// Function called when a mouse button up event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal override void OnMouseUp(object sender, MouseInputEventArgs e)
		{
			if (Clicked != null)
				Clicked(this, EventArgs.Empty);

			_buttonState = ButtonDrawStates.ButtonNormal;

			base.OnMouseUp(sender, e);
		}

		/// <summary>
		/// Function called when a mouse button down event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal override void OnMouseDown(object sender, MouseInputEventArgs e)
		{
			_buttonState = ButtonDrawStates.ButtonPressed;
			base.OnMouseDown(sender, e);
		}

		/// <summary>
		/// Function called when a mouse move event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal override void OnMouseMove(object sender, MouseInputEventArgs e)
		{
			if (e.Buttons != MouseButton.None)
				_buttonState = ButtonDrawStates.ButtonPressed;
			base.OnMouseMove(sender, e);
		}

		/// <summary>
		/// Function to draw the control.
		/// </summary>
		internal override void DrawControl()
		{
			if (Visible)
			{
				BuildControl();

				// Draw the button pieces.
				switch (_buttonState)
				{
					case ButtonDrawStates.ButtonPressed:
						for (int i = 0; i < _pressed.Count; i++)
							_pressed[i].Draw();
						break;
					case ButtonDrawStates.ButtonHilighted:
						for (int i = 0; i < _pressed.Count; i++)
							_hiLight[i].Draw();
						break;
					default:
						for (int i = 0; i < _pressed.Count; i++)
							_pieces[i].Draw();
						break;
				}

				_captionSprite.Draw();
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
		public GUIButton(GUI gui, string name, string text)
			: base(gui, name, Vector2D.Zero, Vector2D.Zero)
		{			
			_captionSprite = new TextSprite("@GUIButton." + name + ".Caption", text, gui.Skin.Font, ForeColor);
			_captionSprite.Alignment = Alignment.Center;			
			_captionSprite.ClippingViewport = new Viewport(0, 0, 1, 1);
			_hiLightColor = Drawing.Color.White;
			_hiLight = new SpriteManager();
			_pressed = new SpriteManager();
			BackColor = Drawing.Color.White;
		}
		#endregion
	}
}
