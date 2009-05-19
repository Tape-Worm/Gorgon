#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Friday, May 18, 2007 2:42:31 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Object representing a zooming window.
	/// </summary>
	public class Zoomer
	{
		#region Variables.
		private formMain _owner = null;							// Owning form.
		private Sprite _zoomerSprite = null;					// Zoomer sprite.
		private TextSprite _zoomerCaption = null;				// Zoomer caption.
		private float _zoomerScale = 1.5f;						// Zoomer scale.
		private bool _zoomerFollows = true;						// Flag to indicate that the zoomer follows the cursor.
		private float _zoomerWindowSize = 192.0f;				// Zoomer window size.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the zoomer window size.
		/// </summary>
		public float ZoomerWindowSize
		{
			get
			{
				return _zoomerWindowSize;
			}
			set
			{
				_zoomerWindowSize = value;

				if (_zoomerWindowSize < 128.0f)
					_zoomerWindowSize = 128.0f;
				if (_zoomerWindowSize > 512.0f)
					_zoomerWindowSize = 512.0f;

				Settings.Root = "Zoomer";
				Settings.SetSetting("ZoomerWindowSize", _zoomerWindowSize.ToString("0.0", System.Globalization.CultureInfo.CurrentUICulture));
				Settings.Root = null;
				_zoomerSprite.Width = _zoomerWindowSize;
				_zoomerSprite.Height = _zoomerWindowSize;
			}
		}

		/// <summary>
		/// Property to set or return the zoom amount.
		/// </summary>
		public float ZoomAmount
		{
			get
			{
				return _zoomerScale;
			}
			set
			{
				_zoomerScale = value;

				if (_zoomerScale < 0.1f)
					_zoomerScale = 0.1f;
				if (_zoomerScale > 16.0f)
					_zoomerScale = 16.0f;

				_zoomerCaption.Text = "Zoomer - " + _zoomerScale.ToString("0.0", System.Globalization.CultureInfo.CurrentUICulture) + "x";

				Settings.Root = "Zoomer";
				Settings.SetSetting("ZoomerScale", _zoomerScale.ToString("0.0", System.Globalization.CultureInfo.CurrentUICulture));
				Settings.Root = null;
			}
		}

		/// <summary>
		/// Property to set or return whether the zoomer will follow the cursor or snap to the sides.
		/// </summary>
		public bool IsZoomerFollowingCursor
		{
			get
			{
				return _zoomerFollows;
			}
			set
			{
				_zoomerFollows = value;

				Settings.Root = "Zoomer";
				Settings.SetSetting("ZoomerFollowsCursor", _zoomerFollows.ToString());
				Settings.Root = null;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve stored settings.
		/// </summary>
		public void GetSettings()
		{
			try
			{
				Settings.Root = "Zoomer";				
				_zoomerScale = Convert.ToSingle(Settings.GetSetting("ZoomerScale", "1" + System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator + "5"), System.Globalization.CultureInfo.CurrentUICulture);
				_zoomerFollows = string.Compare(Settings.GetSetting("ZoomerFollowsCursor", "true"), "true", true) == 0;
				_zoomerWindowSize = Convert.ToSingle(Settings.GetSetting("ZoomerWindowSize", "192" + System.Globalization.CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator + "0"), System.Globalization.CultureInfo.CurrentUICulture);

				if (_zoomerWindowSize < 128.0f)
					_zoomerWindowSize = 128.0f;
				if (_zoomerWindowSize > 512.0f)
					_zoomerWindowSize = 512.0f;

				if (_zoomerScale < 0.1f)
					_zoomerScale = 0.1f;
				if (_zoomerScale > 16.0f)
					_zoomerScale = 16.0f;

				_zoomerCaption.Text = "Zoomer - " + _zoomerScale.ToString("0.0", System.Globalization.CultureInfo.CurrentUICulture) + "x";
				Settings.Root = null;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(_owner, "Unable to read the settings file.", ex);
			}
			finally
			{
				Settings.Root = null;
				_owner.ValidateForm();
			}
		}

		/// <summary>
		/// Function to draw the zoomer.
		/// </summary>
		/// <param name="cursorPosition">Cursor position.</param>
		public void Draw(Vector2D cursorPosition)
		{
			try
			{
				Vector2D newPosition = Vector2D.Unit;							// New position.
				Drawing.RectangleF zoomWindow = Drawing.RectangleF.Empty;		// Zoom window dimensions.
				Vector2D bufferSize = Vector2D.Zero;							// Buffer size.
				Vector2D offset = Vector2D.Zero;								// Offset vector.
				Vector2D scale = Vector2D.Zero;									// Scale of zoomer.

				// Grab back buffer bounds.
				bufferSize = _owner.DisplayDimensions;
				newPosition = _zoomerSprite.Position;

				// Perform scaling.
				scale.X = ZoomerWindowSize / _zoomerScale;
				scale.Y = ZoomerWindowSize / _zoomerScale;

				// Update the zoomer sprite.
				_zoomerSprite.ImageRegion = new Drawing.RectangleF(cursorPosition.X - (scale.X / 2.0f), cursorPosition.Y - (scale.Y / 2.0f), scale.X, scale.Y);
				_zoomerSprite.UniformScale = ZoomerWindowSize / _zoomerSprite.Width;

				if (!_zoomerFollows)
				{
					// Snap to corners.
					zoomWindow = new Drawing.RectangleF(0, 0, _zoomerSprite.ScaledWidth + 2, _zoomerSprite.ScaledHeight + 2 + _zoomerCaption.Height);
					if (zoomWindow.Contains(cursorPosition))
					{
						newPosition.X = bufferSize.X - _zoomerSprite.ScaledWidth - 1;
						newPosition.Y = bufferSize.Y - _zoomerSprite.ScaledHeight - 1;
					}
					else
					{
						newPosition.X = 1;
						newPosition.Y = _zoomerCaption.Height + 1;
					}
				}
				else
				{
					// Default offset.
					offset.X = 16.0f;
					offset.Y = 32.0f;

					// Move the zoomer to outside of the fixed clip range.
					if (_owner.Clipper.IsFixedDimensions)
					{
						offset.X += _owner.Clipper.FixedSize.X;
						offset.Y += _owner.Clipper.FixedSize.Y;
					}

					// Update the position.
					newPosition = Vector2D.Add(cursorPosition, offset);

					// Adjust for boundaries.
					if (newPosition.X >= bufferSize.X - _zoomerSprite.ScaledWidth - 1)
						newPosition.X = cursorPosition.X - 16.0f - _zoomerSprite.ScaledWidth;
					if (newPosition.Y >= bufferSize.Y - _zoomerCaption.Height - _zoomerSprite.ScaledHeight - 1)
						newPosition.Y = cursorPosition.Y - 20.0f - _zoomerSprite.ScaledHeight;

					// Don't go offscreen.
					if (newPosition.X >= bufferSize.X - _zoomerSprite.ScaledWidth - 16.0f)
						newPosition.X = bufferSize.X - _zoomerSprite.ScaledWidth - 16.0f;
					if (newPosition.X <= 1.0f)
						newPosition.X = 1.0f;
					if (newPosition.Y >= bufferSize.Y - _zoomerSprite.ScaledHeight - 16.0f)
						newPosition.Y = bufferSize.Y - _zoomerSprite.ScaledHeight - 20.0f;
					if (newPosition.Y <= 1.0f)
						newPosition.Y = 1.0f;
				}

				_zoomerSprite.Position = newPosition;
				_zoomerSprite.Draw();

				// Draw border.
				Gorgon.Screen.BlendingMode = BlendingModes.Inverted;
				Gorgon.Screen.Rectangle(newPosition.X - 1, newPosition.Y - 1 - _zoomerCaption.Height, _zoomerSprite.ScaledWidth + 2, _zoomerSprite.ScaledHeight + 2 + _zoomerCaption.Height, Drawing.Color.Blue);
				Gorgon.Screen.BlendingMode = BlendingModes.Modulated;
				Gorgon.Screen.FilledRectangle(newPosition.X, newPosition.Y - _zoomerCaption.Height, _zoomerSprite.ScaledWidth, _zoomerCaption.Height, Drawing.Color.Black);
				_zoomerCaption.SetPosition(newPosition.X + 3, newPosition.Y - _zoomerCaption.Height + 1);
				_zoomerCaption.Draw();
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Form that owns this object.</param>
		public Zoomer(formMain owner)
		{
			_owner = owner;

			// Create the zoomer sprite.
			_zoomerSprite = new Sprite("ZoomerSprite", _owner.BackBuffer);
			_zoomerSprite.Position = new Vector2D(1, 1);
			_zoomerSprite.AlphaMaskFunction = CompareFunctions.Always;
			_zoomerSprite.BlendingMode = BlendingModes.None;
			_zoomerSprite.WrapMode = ImageAddressing.Border;
			_zoomerSprite.Smoothing = Smoothing.None;
			_zoomerSprite.Width = ZoomerWindowSize;
			_zoomerSprite.Height = ZoomerWindowSize;

			// Create zoomer window caption.
			_zoomerCaption = new TextSprite("ZoomerCaption", "Zoomer - " + _zoomerScale.ToString("0.0") + "x", formMain.MainFont);
		}
		#endregion
	}
}
