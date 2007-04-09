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
// Created: Sunday, January 21, 2007 7:26:28 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Drawing = System.Drawing;
using SharpUtilities;
using SharpUtilities.Mathematics;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Fonts;
using GorgonLibrary.Input;

namespace GorgonLibrary.GorgonGUI
{
	/// <summary>
	/// Abstract object representing a GUI control.
	/// </summary>
	public abstract class GUIControl
		: NamedObject, IDisposable
	{
		#region Variables.
		private GUIControl _parent = null;			// Parent control.
		private Drawing.Color _borderColor;			// Color of the border.
		private Drawing.Color _backColor;			// Background color.
		private Sprite _background = null;			// Background image.
		private bool _scaleBackground = false;		// Flag to indicate that we should scale the background image.
		private Font _textFont = null;				// Text font.
		private Drawing.Color _foreColor;			// Foreground color.
		private Viewport _clipper = null;			// Clipping view port.		
		private bool _mouseInView = false;			// Flag to indicate that the mouse is over the window.
		private bool _isFocused = false;			// Flag to indicate that a window is focused or not.

		/// <summary>Sprite pieces.</summary>
		protected SpriteManager _pieces = null;
		/// <summary>GUI that owns this control.</summary>
		protected GUI _owner = null;
		/// <summary>Position of the control.</summary>
		protected Vector2D _position = Vector2D.Zero;
		/// <summary>Width and height of the control.</summary>
		protected Vector2D _size = Vector2D.Zero;
		/// <summary>Gorgon owner control.</summary>
		protected Control _gorgonOwner = null;
		/// <summary>Flag to indicate that the control is visible or not.</summary>
		protected bool _visible = true;
		/// <summary>Size of the client area.</summary>
		protected Vector2D _clientSize = Vector2D.Zero;
		/// <summary>Position of the client area in relation to the window.</summary>
		protected Vector2D _clientPosition = Vector2D.Zero;		
		#endregion

		#region Events.
		/// <summary>
		/// Event for mouse wheel changes.
		/// </summary>
		public event MouseInputEvent MouseWheel = null;
		/// <summary>
		/// Event for mouse double clicks.
		/// </summary>
		public event MouseInputEvent MouseDoubleClick = null;
		/// <summary>
		/// Event for mouse movement.
		/// </summary>
		public event MouseInputEvent MouseMove = null;
		/// <summary>
		/// Event for mouse button down.
		/// </summary>
		public event MouseInputEvent MouseDown = null;
		/// <summary>
		/// Event for mouse button up.
		/// </summary>
		public event MouseInputEvent MouseUp = null;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the clipping interface.
		/// </summary>
		internal Viewport ClippingView
		{
			get
			{
				return _clipper;
			}
		}

		/// <summary>
		/// Property to set or return whether the mouse is over the control.
		/// </summary>
		internal bool MouseInView
		{
			get
			{
				return _mouseInView;
			}
			set
			{
				_mouseInView = value;
			}
		}

		/// <summary>
		/// Property to return the control boundaries.
		/// </summary>
		public virtual Drawing.Rectangle Bounds
		{
			get
			{
				return new Drawing.Rectangle((int)_position.X, (int)_position.Y, (int)_size.X, (int)_size.Y);
			}
		}

		/// <summary>
		/// Property to return whether a window is focused or not.
		/// </summary>
		public bool Focused
		{
			get
			{
				return _isFocused;
			}
		}

		/// <summary>
		/// Property to set or return the border color.
		/// </summary>
		public virtual Drawing.Color BorderColor
		{
			get
			{
				return _borderColor;
			}
			set
			{
				_borderColor = value;
			}
		}

		/// <summary>
		/// Property to set or return the background color.
		/// </summary>
		public virtual Drawing.Color BackColor
		{
			get
			{
				return _backColor;
			}
			set
			{
				_backColor = value;
			}
		}

		/// <summary>
		/// Property to set or return the foreground color.
		/// </summary>
		public virtual Drawing.Color ForeColor
		{
			get
			{
				return _foreColor;
			}
			set
			{
				_foreColor = value;
			}
		}

		/// <summary>
		/// Property to set or return the background image.
		/// </summary>
		public virtual Sprite BackgroundImage
		{
			get
			{
				return _background;
			}
			set
			{
				_background = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the background image should scale to the size of the control.
		/// </summary>
		public virtual bool ScaleBackgroundImage
		{
			get
			{
				return _scaleBackground;
			}
			set
			{
				_scaleBackground = value;
			}
		}

		/// <summary>
		/// Property to set or return the font for the control.
		/// </summary>
		public virtual Font Font
		{
			get
			{
				if (_textFont == null)
					return _owner.Skin.Font;

				return _textFont;
			}
			set
			{
				_textFont = value;
			}
		}

		/// <summary>
		/// Property to set or return the parent control.
		/// </summary>
		public virtual GUIControl Parent
		{
			get
			{
				return _parent;
			}
			set
			{
				_parent = value;
			}
		}

		/// <summary>
		/// Property to set or return whether the control is visible.
		/// </summary>
		public bool Visible
		{
			get
			{
				return _visible;
			}
			set
			{
				_visible = value;
			}
		}

		/// <summary>
		/// Property to set or return the position of the control.
		/// </summary>
		public virtual Vector2D Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;				
			}
		}

		/// <summary>
		/// Property to set or return the width and height of the control.
		/// </summary>
		public virtual Vector2D Size
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
				if (_size.X == 0)
					_size.X = 1.0f;
				if (_size.Y == 0)
					_size.Y = 1.0f;
			}
		}

		/// <summary>
		/// Property to return the client size.
		/// </summary>
		public virtual Vector2D ClientSize
		{
			get
			{
				return new Vector2D(_clientSize.X, _clientSize.Y);
			}
		}

		/// <summary>
		/// Property to set or return the width of the client control.
		/// </summary>
		public virtual float Width
		{
			get
			{
				return _size.X;
			}
			set
			{
				Size = new Vector2D(value, _size.Y);				
			}
		}

		/// <summary>
		/// Property to set or return the height of the client control.
		/// </summary>
		public virtual float Height
		{
			get
			{
				return _size.Y;
			}
			set
			{
				Size = new Vector2D(_size.X, value);
			}
		}
		#endregion

		#region Methods.		
		/// <summary>
		/// Function called when a mouse move event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal virtual void OnMouseMove(object sender, MouseInputEventArgs e)
		{
			if (MouseMove != null)
				MouseMove(this, e);
		}

		/// <summary>
		/// Function called when a mouse button up event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal virtual void OnMouseUp(object sender, MouseInputEventArgs e)
		{
			if (MouseUp != null)
				MouseUp(this, e);
		}

		/// <summary>
		/// Function called when a mouse button down event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal virtual void OnMouseDown(object sender, MouseInputEventArgs e)
		{
			if (MouseDown != null)
				MouseDown(this, e);
		}

		/// <summary>
		/// Function called when a mouse double click event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal virtual void OnMouseDoubleClick(object sender, MouseInputEventArgs e)
		{
			if (MouseDoubleClick != null)
				MouseDoubleClick(this, e);
		}

		/// <summary>
		/// Function called when a mouse wheel event is fired.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="GorgonLibrary.Input.MouseInputEventArgs"/> instance containing the event data.</param>
		protected internal virtual void OnMouseWheel(object sender, MouseInputEventArgs e)
		{
			if (MouseWheel != null)
				MouseWheel(this, e);
		}

		/// <summary>
		/// Function to update the clipping for a control.
		/// </summary>
		protected virtual void UpdateClipping()
		{
			Vector2D screenPos = Vector2D.Zero;				// Screen position.
			Vector2D parentPos = Vector2D.Zero;				// Parent position.
			Vector2D parentClientScreen = Vector2D.Zero;	// Clipping bounds of the parent in screen space.
			Vector2D childClientScreen = Vector2D.Zero;		// Clipping bounds of the child in screen space.

			// No parent, then leave.
			if (_parent == null)
				return;

			// Convert the position to screen position.
			screenPos = _parent.ConvertToScreen(_position);
			parentPos = _parent.ConvertToScreen(Vector2D.Zero);

			// Clip to parent.
			if (screenPos.X < parentPos.X)
				screenPos.X = parentPos.X;
			if (screenPos.Y < parentPos.Y)
				screenPos.Y = parentPos.Y;

			parentClientScreen = _parent.ConvertToScreen(_parent.ClientSize);
			childClientScreen = screenPos + _size;

			// Clip to parent client.
			if (parentClientScreen.X < childClientScreen.X)
				parentClientScreen.X = _size.X - (childClientScreen.X - parentClientScreen.X);
			else
				parentClientScreen.X = _size.X;

			if (parentClientScreen.Y < childClientScreen.Y)
				parentClientScreen.Y = _size.Y - (childClientScreen.Y - parentClientScreen.Y);
			else
				parentClientScreen.Y = _size.Y;

			// If there's a change in the clipping rectangle, then update.
			if (_clipper.Left != (int)screenPos.X)
				_clipper.Left = (int)screenPos.X;
			if (_clipper.Top != (int)screenPos.Y)
				_clipper.Top = (int)screenPos.Y;
			if (_clipper.Width != (int)parentClientScreen.X)
				_clipper.Width = (int)parentClientScreen.X;
			if (_clipper.Height != (int)parentClientScreen.Y)
				_clipper.Height = (int)parentClientScreen.Y;
		}

		/// <summary>
		/// Function to build the control.
		/// </summary>
		protected virtual void BuildControl()
		{
			// If we don't have the skin for the control, get it.
			if (_pieces.Count == 0)
				GetSkinPieces();
		}

		/// <summary>
		/// Function to retrieve the skin pieces.
		/// </summary>
		protected abstract void GetSkinPieces();

		/// <summary>
		/// Function called when the window has gained focus.
		/// </summary>
		protected internal virtual void GainingFocus()
		{
			_isFocused = true;
		}

		/// <summary>
		/// Function called when the window has lost its focus.
		/// </summary>
		protected internal virtual void LosingFocus()
		{
			_isFocused = false;
		}

		/// <summary>
		/// Function to draw the control.
		/// </summary>
		internal abstract void DrawControl();

		/// <summary>
		/// Function to convert a client position into screen space.
		/// </summary>
		/// <param name="clientPosition">Client position.</param>
		/// <returns>The position in screen space.</returns>
		public virtual Vector2D ConvertToScreen(Vector2D clientPosition)
		{
			if (_parent != null)
				return _parent.ConvertToScreen(clientPosition);
			else
				return new Vector2D(clientPosition.X + _position.X, _clientPosition.Y + _position.Y);
		}

		/// <summary>
		/// Function to convert a screen position into a client space.
		/// </summary>
		/// <param name="screenPosition">Screen position to convert.</param>
		/// <returns>The position in client space.</returns>
		public virtual Vector2D ConvertToClient(Vector2D screenPosition)
		{
			Vector2D screenSpace = ConvertToScreen(Vector2D.Zero);		// Convert the client position to screen space.

			return new Vector2D(screenPosition.X - screenSpace.X, screenPosition.Y - screenSpace.Y);
		}

		/// <summary>
		/// Function to set the position of the control.
		/// </summary>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical position.</param>
		public virtual void SetPosition(float x, float y)
		{
			Position = new Vector2D(x, y);
		}

		/// <summary>
		/// Function to set the position of the control.
		/// </summary>
		/// <param name="x">Horizontal position.</param>
		/// <param name="y">Vertical position.</param>
		/// <param name="width">Width of the control.</param>
		/// <param name="height">Height of the control.</param>		
		public virtual void SetDimensions(float x, float y, float width, float height)
		{
			Position = new Vector2D(x, y);
			Size = new Vector2D(width, height);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">GUI that owns this control.</param>
		/// <param name="name">Name of the control.</param>
		/// <param name="position">Position on the screen for the control.</param>
		/// <param name="size">Width and height of the control.</param>
		protected GUIControl(GUI owner, string name, Vector2D position, Vector2D size)
			: base(name)
		{
			if (Gorgon.Screen == null)
				throw new NotInitializedException("Gorgon was not initialized.  Please call Gorgon.Initialize() and Gorgon.SetMode().", null);

			if (owner == null)
				throw new ArgumentNullException("owner");

			// TODO: Make a real exception.
			if (owner.Skin == null)
				throw new Exception("NOT A VALID SKIN!");
			
			// Get the owner.
			_gorgonOwner = Gorgon.Screen.Owner;
			_owner = owner;

			// Create sprite manager.
			_pieces = new SpriteManager();

			// Set default colors.
			_borderColor = Drawing.Color.Transparent;
			_backColor = Drawing.Color.Transparent;
			_foreColor = _owner.Skin.DefaultFontColor;

			// Set the clipper.
			_clipper = new Viewport(0, 0, (int)size.X, (int)size.Y);

			_position = position;
			Size = size;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
